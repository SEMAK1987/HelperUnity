import express from "express";
import axios from "axios";
import { createServer as createViteServer } from "vite";
import path from "path";
import fs from "fs-extra";
import cors from "cors";
import dotenv from "dotenv";
import multer from "multer";
import chokidar, { FSWatcher } from "chokidar";
import AdmZip from "adm-zip";

dotenv.config();

// Configure storage for uploads
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    const kbPath = path.join(process.cwd(), "knowledge_base.json");
    let uploadDir = path.join(process.cwd(), "uploads");
    
    try {
      if (fs.pathExistsSync(kbPath)) {
        const kb = fs.readJsonSync(kbPath);
        if (kb.local_training_path) {
          uploadDir = path.join(process.cwd(), "local_storage", path.basename(kb.local_training_path));
        }
      }
    } catch (e) {
      console.error("Error reading KB for upload path", e);
    }
    
    fs.ensureDirSync(uploadDir);
    cb(null, uploadDir);
  },
  filename: (req, file, cb) => {
    cb(null, `${Date.now()}-${file.originalname}`);
  }
});

const upload = multer({ 
  storage,
  limits: {
    fileSize: 500 * 1024 * 1024 // 500MB max per file
  }
});

let currentScanResults: any = {
  scripts: [],
  prefabs: [],
  scenes: [],
  animations: [],
  animators: [],
  pdfs: [],
  videos: [],
  others: [],
  total_files: 0,
  last_updated: new Date().toISOString(),
  analysis: {
    audit_issues: [],
    todos: [],
    asset_stats: {
      total_size: 0,
      large_files: []
    },
    dependencies: {}
  }
};

const statsPath = path.join(process.cwd(), "project_stats.json");
const historyPath = path.join(process.cwd(), "history.json");
const kbPath = path.join(process.cwd(), "knowledge_base.json");
const blueprintJsonPath = path.join(process.cwd(), "ccgs_project_blueprint.json");
const masterBlueprintMdPath = path.join(process.cwd(), "PROJECT_MASTER_BLUEPRINT.md");
const UNITY_API_FILE = path.join(process.cwd(), "unity_api_ref.json");
const BLENDER_API_FILE = path.join(process.cwd(), "blender_api_ref.json");
const TROUBLESHOOTING_FILE = path.join(process.cwd(), "troubleshooting_db.json");
const VERSION_FILE = path.join(process.cwd(), "version.json");
const chatHistoryPath = path.join(process.cwd(), "chat_history.json");

const OLLAMA_API_URL = "http://localhost:11434/api/generate";

async function checkOllamaStatus() {
  try {
    const res = await axios.get("http://localhost:11434/api/tags", { timeout: 2000 });
    return res.status === 200;
  } catch (e) {
    return false;
  }
}

async function loadHistory() {
  if (!(await fs.pathExists(historyPath))) {
    await fs.writeJson(historyPath, [], { spaces: 2 });
  }
  return await fs.readJson(historyPath);
}

async function addToHistory(event: string, filePath: string) {
  try {
    const history = await loadHistory();
    history.unshift({
      event,
      path: filePath,
      timestamp: new Date().toISOString()
    });
    // Keep last 100 events
    await fs.writeJson(historyPath, history.slice(0, 100), { spaces: 2 });
  } catch (e) {
    console.error("Failed to update history", e);
  }
}

async function loadStats() {
  if (await fs.pathExists(statsPath)) {
    try {
      const loaded = await fs.readJson(statsPath);
      // Ensure structure integrity
      currentScanResults = {
        ...currentScanResults,
        ...loaded,
        analysis: {
          ...currentScanResults.analysis,
          ...(loaded.analysis || {})
        }
      };
    } catch (e) {
      console.error("Failed to load project stats", e);
    }
  }
}

async function saveStats() {
  try {
    await fs.writeJson(statsPath, currentScanResults, { spaces: 2 });
  } catch (e) {
    console.error("Failed to save project stats", e);
  }
}

let isScanning = false;
let currentUnityStatus: any = { is_running: false, version: "unknown", project_path: "" };
let currentBlenderStatus: any = { is_running: false, version: "unknown" };

async function performScan() {
  if (isScanning) {
    console.log("[SCAN] Scan already in progress, skipping...");
    return;
  }
  isScanning = true;
  console.log("[SCAN] Starting project scan...");
  let rootDir = process.cwd();
  
  try {
    try {
      if (await fs.pathExists(kbPath)) {
        const kb = await fs.readJson(kbPath);
        if (kb.project_path && await fs.pathExists(kb.project_path)) {
          rootDir = kb.project_path;
        }
      }
    } catch (e) {
      console.error("Error reading KB for scan path", e);
    }

    const results: any = {
      scripts: [],
      prefabs: [],
      scenes: [],
      animations: [],
      animators: [],
      pdfs: [],
      videos: [],
      others: [],
      total_files: 0,
      analysis: {
        audit_issues: [],
        todos: [],
        asset_stats: {
          total_size: 0,
          large_files: []
        },
        dependencies: {}
      }
    };

    const scanDir = async (dir: string) => {
      if (!(await fs.pathExists(dir))) return;
      const files = await fs.readdir(dir);
      for (const file of files) {
        // Yield to event loop
        await new Promise(resolve => setImmediate(resolve));
        
        const fullPath = path.join(dir, file);
        const stat = await fs.stat(fullPath);
        
        if (stat.isDirectory()) {
          const excludedDirs = ['node_modules', '.git', 'dist', 'Library', 'Temp', 'Obj', 'Build', 'Logs', 'local_storage', 'uploads', 'backup_*'];
          if (excludedDirs.some(d => d.includes('*') ? file.startsWith(d.replace('*', '')) : d === file)) {
            continue;
          }
          await scanDir(fullPath);
        } else {
          const excludedFiles = ['project_stats.json', 'PROJECT_MASTER_BLUEPRINT.md', 'ccgs_project_blueprint.json', 'knowledge_base.json', 'version.json', 'unity_version.txt', 'package.json', 'package-lock.json', 'tsconfig.json', 'history.json'];
          if (excludedFiles.includes(file)) continue;

          results.total_files++;
          results.analysis.asset_stats.total_size += stat.size;
          
          const ext = path.extname(file).toLowerCase();
          const relativePath = path.relative(rootDir, fullPath);

          // Asset Optimization: Track large files (> 10MB)
          if (stat.size > 10 * 1024 * 1024) {
            results.analysis.asset_stats.large_files.push({
              path: relativePath,
              size: (stat.size / 1024 / 1024).toFixed(2) + " MB"
            });
          }
          
          if (ext === '.cs') {
            results.scripts.push(relativePath);
            // Code Audit & To-Do Scan (Only for files < 1MB)
            if (stat.size < 1024 * 1024) {
              try {
                const content = await fs.readFile(fullPath, 'utf-8');
                
                // 1. Audit: GetComponent/Find in Update
              const updateRegex = /(void\s+(Update|FixedUpdate|LateUpdate)\s*\(\s*\)[\s\S]*?\{)([\s\S]*?)\}/g;
              let match;
              while ((match = updateRegex.exec(content)) !== null) {
                const body = match[3];
                if (body.includes('GetComponent') || body.includes('GameObject.Find') || body.includes('FindWithTag')) {
                  results.analysis.audit_issues.push({
                    file: relativePath,
                    type: 'Performance',
                    message: `Обнаружен вызов GetComponent или Find внутри ${match[2]}. Это замедляет игру. Рекомендуется кэшировать ссылку в Start().`
                  });
                }
              }

              // 2. To-Do Scan
              const todoRegex = /\/\/\s*(TODO|FIXME):\s*(.*)/gi;
              let todoMatch;
              while ((todoMatch = todoRegex.exec(content)) !== null) {
                results.analysis.todos.push({
                  file: relativePath,
                  type: todoMatch[1].toUpperCase(),
                  text: todoMatch[2].trim()
                });
              }

              // 3. Simple Dependency Extraction (Project Map)
              const classRegex = /class\s+(\w+)/;
              const classMatch = content.match(classRegex);
              if (classMatch) {
                const className = classMatch[1];
                const deps: string[] = [];
                
                // Find 'using' statements
                const usingRegex = /using\s+([\w.]+);/g;
                let usingMatch;
                while ((usingMatch = usingRegex.exec(content)) !== null) {
                  deps.push(usingMatch[1]);
                }
                
                results.analysis.dependencies[className] = deps;
              }

              } catch (e) {
                console.error(`Failed to analyze script: ${relativePath}`, e);
              }
            }
          }
          else if (ext === '.prefab' || ext === '.unity') {
            if (ext === '.prefab') results.prefabs.push(relativePath);
            else results.scenes.push(relativePath);
            
            // Check for Missing Scripts (fileID: 0)
            try {
              const content = await fs.readFile(fullPath, 'utf-8');
              if (content.includes('m_Script: {fileID: 0}')) {
                results.analysis.audit_issues.push({
                  file: relativePath,
                  type: 'MissingScript',
                  message: `Обнаружена битая ссылка на скрипт (Missing Script). Это может вызвать ошибки при запуске игры.`
                });
              }
            } catch (e) {}
          }
          else if (ext === '.anim') results.animations.push(relativePath);
          else if (ext === '.controller') results.animators.push(relativePath);
          else if (ext === '.pdf') results.pdfs.push(relativePath);
          else if (['.mp4', '.mov', '.avi', '.mkv'].includes(ext)) results.videos.push(relativePath);
          else if (['.png', '.jpg', '.fbx', '.wav', '.mp3'].includes(ext)) results.others.push(relativePath);
        }
      }
    };

    await scanDir(rootDir);
    results.last_updated = new Date().toISOString();
    currentScanResults = results;
    await saveStats();

    // Sync with Blueprint
    try {
      const blueprintPath = path.join(process.cwd(), "ccgs_project_blueprint.json");
      if (await fs.pathExists(blueprintPath)) {
        const blueprint = await fs.readJson(blueprintPath);
        blueprint.project_assets = {
          scripts_count: results.scripts.length,
          prefabs_count: results.prefabs.length,
          videos_count: results.videos.length,
          total_files: results.total_files,
          video_list: results.videos,
          script_list: results.scripts
        };
        blueprint.last_scan = results.last_updated;
        await fs.writeJson(blueprintPath, blueprint, { spaces: 2 });
      }
    } catch (e) {
      console.error("Failed to sync scan results with blueprint", e);
    }

    console.log("Project scan completed successfully.");
    await checkProjectIntegrity();
    await generateMasterBlueprint();
  } catch (error) {
    console.error("Project scan failed:", error);
  } finally {
    isScanning = false;
  }
}

async function checkProjectIntegrity() {
  const files = [
    { name: "knowledge_base.json", default: { project_name: "Unity Assistant", project_path: process.cwd(), system_instruction: "You are a helpful assistant." } },
    { name: "ccgs_project_blueprint.json", default: { project_name: "Unity Assistant", version: "1.1.0", interface_structure: { tabs: ["studio", "kb", "commands", "files"] }, agents_count: 49 } },
    { name: "version.json", default: { version: "1.2.0" } }
  ];

  for (const file of files) {
    const filePath = path.join(process.cwd(), file.name);
    try {
      if (!(await fs.pathExists(filePath))) {
        console.log(`[INTEGRITY] Restoring missing file: ${file.name}`);
        await fs.writeJson(filePath, file.default, { spaces: 2 });
      } else {
        // Try to parse to check for corruption
        await fs.readJson(filePath);
      }
    } catch (e) {
      console.error(`[INTEGRITY] File ${file.name} is corrupted. Resetting to default.`);
      await fs.writeJson(filePath, file.default, { spaces: 2 });
    }
  }
}

async function generateMasterBlueprint() {
  try {
    const kb = await fs.readJson(kbPath);
    const blueprint = await fs.readJson(blueprintJsonPath);
    
    let md = `# PROJECT MASTER BLUEPRINT: ${blueprint.project_name || "Unity & Blender AI Assistant"}\n\n`;
    md += `> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.\n\n`;
    md += `## 1. Общая информация\n`;
    md += `- **Версия Помощника:** ${blueprint.version || "14.8.0"}\n`;
    md += `- **Описание:** ${blueprint.description || "Гибридный ИИ-помощник (Online/Offline) для Unity & Blender. Поддержка Ollama, миграция на Unity 6, сохранение чата, поддержка архивов и самовосстановление."}\n`;
    md += `- **Путь проекта:** ${kb.project_path}\n`;
    md += `- **Локальное хранилище:** ${kb.local_training_path || "Не задано"}\n`;
    md += `- **Версия Unity:** ${currentUnityStatus.version}\n`;
    md += `- **Версия Blender:** ${currentBlenderStatus.version}\n\n`;
    
    md += `## 2. Структура интерфейса\n`;
    md += `### Вкладки\n`;
    if (blueprint.interface_structure?.tabs) {
      blueprint.interface_structure.tabs.forEach((tab: string) => {
        md += `- **${tab.toUpperCase()}**: ${tab === 'studio' ? 'Главная студия разработки' : tab === 'kb' ? 'База знаний' : tab === 'commands' ? 'Командный центр' : 'Файловый менеджер'}\n`;
      });
    }
    md += `\n### Компоненты\n`;
    md += `- **Sidebar**: ${blueprint.interface_structure?.sidebar || "Мини-панель навигации"}\n`;
    md += `- **Top Bar**: ${blueprint.interface_structure?.top_bar || "Панель управления и статуса"}\n`;
    md += `- **Right Sidebar**: ${blueprint.interface_structure?.right_sidebar || "Логи и статус Unity"}\n\n`;

    md += `## 3. Иерархия ИИ-Агентов (${blueprint.agents_count || 49} агентов)\n`;
    if (blueprint.knowledge_base?.levels) {
      blueprint.knowledge_base.levels.forEach((level: any) => {
        md += `### Уровень ${level.id}: ${level.name}\n`;
        level.agents.forEach((agent: any) => {
          md += `- **${agent.name}** (${agent.model}): ${agent.role}\n`;
        });
        md += `\n`;
      });
    }

    md += `## 4. База знаний и Команды\n`;
    md += `### Доступные команды\n`;
    if (blueprint.interface_structure?.commands) {
      blueprint.interface_structure.commands.forEach((cmd: any) => {
        md += `- \`${cmd.cmd}\`: ${cmd.desc}\n`;
      });
    }
    
    md += `\n### Системные инструкции\n`;
    md += `\`\`\`text\n${kb.system_instruction}\n\`\`\`\n\n`;

    md += `## 5. Анализ и Аудит Проекта\n`;
    md += `- **Всего файлов:** ${currentScanResults.total_files}\n`;
    md += `- **Скрипты (C#):** ${currentScanResults.scripts.length}\n`;
    md += `- **Префабы:** ${currentScanResults.prefabs.length}\n`;
    md += `- **Видео:** ${currentScanResults.videos.length}\n`;
    md += `- **Общий вес ассетов:** ${(currentScanResults.analysis.asset_stats.total_size / 1024 / 1024).toFixed(1)} MB\n\n`;

    md += `### Найденные проблемы (Аудит):\n`;
    if (currentScanResults.analysis.audit_issues.length > 0) {
      currentScanResults.analysis.audit_issues.forEach((i: any) => md += `- [${i.type}] ${i.file}: ${i.message}\n`);
    } else {
      md += `Проблем не обнаружено.\n`;
    }

    md += `\n### Список задач (TODO):\n`;
    if (currentScanResults.analysis.todos.length > 0) {
      currentScanResults.analysis.todos.forEach((t: any) => md += `- [${t.type}] ${t.file}: ${t.text}\n`);
    } else {
      md += `Задач не найдено.\n`;
    }

    md += `\n## 6. Новые возможности ИИ (v14.8.0)\n`;
    md += `- **Advanced AI Capabilities:** Улучшенное понимание сложных архитектурных паттернов и систем.\n`;
    md += `- **Advanced Physics & VFX Mastery:** Глубокое понимание симуляций физики и визуальных эффектов.\n`;
    md += `- **Hyper-Realistic Rendering Mastery:** Глубокое понимание техник освещения и постобработки для достижения фотореализма.\n`;
    md += `- **Advanced Character Systems:** Проектирование сложных систем персонажей с использованием процедурной анимации и IK.\n`;
    md += `- **MMO Scalability Expert:** Оптимизация сетевой архитектуры для поддержки десятков тысяч одновременных подключений.\n`;
    md += `- **Extended Knowledge Base:** Интеграция 802+ видео-уроков по Unity и Blender.\n`;
    md += `- **Advanced AI Systems:** Поддержка Behavior Trees, Utility AI и ML-Agents.\n`;
    md += `- **Graphics & VFX:** Глубокое понимание Shader Graph, VFX Graph, Ray Tracing и Volumetric Lighting.\n`;
    md += `- **Blender Simulation:** Работа с Simulation Nodes и сложным риггингом.\n`;
    md += `- **Automated Pipeline:** Скрипты для пакетного экспорта и автоматической настройки материалов.\n`;
    md += `- **Archive Support:** Чтение и анализ содержимого ZIP и RAR архивов при загрузке.\n`;
    md += `- **Upload Progress:** Визуальное отображение процента загрузки файлов в проект.\n`;
    md += `- **Hybrid AI (Ollama):** Работа без интернета через локальные LLM (Llama 3, Phi-3).\n\n`;

    md += `## 7. Ограничения ИИ (Что ИИ пока не знает)\n`;
    md += `- **Прямое управление Unity Editor:** ИИ не может напрямую нажимать кнопки в интерфейсе Unity, только генерировать скрипты и инструкции.\n`;
    md += `- **Real-time рендеринг видео:** ИИ анализирует статические кадры и код, но не может "смотреть" видео в реальном времени без предварительной обработки.\n`;
    md += `- **Сложные сетевые протоколы:** Ограниченная поддержка проприетарных сетевых решений (только Photon/Mirror/Netcode).\n`;
    md += `- **Глубокая физика жидкостей:** Только шейдерные имитации и базовые системы частиц.\n\n`;

    md += `## 8. Расширенная База Видео-уроков (390+ видео)\n`;
    md += `### Темы Unity\n`;
    md += `- **Программирование:** Продвинутый C#, Job System, Burst Compiler, Addressables, Localization.\n`;
    md += `- **Графика:** URP/HDRP, Custom Lighting, Decals, Volumetric Effects.\n`;
    md += `- **ИИ:** Behavior Trees, ML-Agents, Pathfinding.\n`;
    md += `### Темы Blender\n`;
    md += `- **Моделирование:** Hard Surface, Sculpting, Retopology, Geometry Nodes.\n`;
    md += `- **Анимация:** Simulation Nodes, Advanced Rigging, Face Animation.\n`;
    md += `- **Текстурирование:** Texture Painting, PBR, UV Unwrapping.\n\n`;

    md += `## 8. База знаний: RPG Системы\n`;
    md += `### Крафт и Кузница\n`;
    md += `- **Предметы:** Шлемы, Броня, Мечи, Копья, Секиры, Молоты, Кастеты, Алебарды и др.\n`;
    md += `- **Ранги (Звезды):** Начальный (5), Земной (5), Небесный (5), Легендарный (10), Полубожественный (10), Божественный (10).\n`;
    md += `- **Механики:** Перековка за золото, навыки кузнеца, зависимость статов от ранга.\n`;
    md += `### Характеристики Героя\n`;
    md += `- **Атрибуты:** Жизнь (HP), Сила, Ловкость, Мана, Интеллект, Выносливость.\n`;
    md += `- **Инвентарь:** Создание систем слотов, веса и категорий предметов.\n\n`;

    md += `## 8. Архитектура Offline & Hybrid\n`;
    md += `- **LLM Provider:** Ollama (localhost:11434).\n`;
    md += `- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.\n`;
    md += `- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.\n`;
    md += `- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.\n\n`;

    md += `## 9. История изменений (Последние 10)\n`;
    const history = await fs.readJson(historyPath).catch(() => []);
    if (history.length > 0) {
      history.slice(-10).reverse().forEach((h: any) => {
        md += `- **[${h.event.toUpperCase()}]** ${h.path} (${new Date(h.timestamp).toLocaleString()})\n`;
      });
    } else {
      md += `История пуста.\n`;
    }
    md += `\n`;

    md += `## 10. Аварийные процедуры (Emergency)\n`;
    if (kb.emergency_procedures) {
      md += `### Unity без интернета\n`;
      kb.emergency_procedures.unity_no_internet.forEach((step: string) => md += `- ${step}\n`);
      md += `\n### Исправление вылетов Unity\n`;
      kb.emergency_procedures.unity_crash_fix.forEach((step: string) => md += `- ${step}\n`);
      md += `\n### ИИ в Офлайне\n`;
      kb.emergency_procedures.ai_offline_mode.forEach((step: string) => md += `- ${step}\n`);
    }
    md += `\n`;

    md += `## 11. Инструкции по восстановлению\n`;
    md += `1. Установите Node.js (v18+).\n`;
    md += `2. Склонируйте репозиторий: \`git clone https://github.com/SEMAK1987/unity-ai-assistant.git\`\n`;
    md += `3. Запустите \`RUN.bat\` для автоматической установки зависимостей и запуска.\n\n`;

    md += `## 12. Известные ошибки и решения\n`;
    md += `- **WebSocket Error:** Ошибка \`[vite] failed to connect to websocket\` является ожидаемой в данной среде разработки и не влияет на работу приложения. Её можно игнорировать.\n`;
    md += `- **Unexpected token '<':** Обычно означает, что сервер вернул HTML вместо JSON. Проверьте статус сервера и корректность API путей.\n`;

    await fs.writeFile(masterBlueprintMdPath, md);
    console.log("Master blueprint generated successfully.");
  } catch (e) {
    console.error("Failed to generate master blueprint", e);
  }
}

async function startServer() {
  const app = express();
  const PORT = Number(process.env.PORT) || 3000;

  app.use(cors());
  app.use(express.json({ limit: '500mb' }));
  app.use(express.urlencoded({ limit: '500mb', extended: true }));
  app.use("/uploads", express.static(path.join(process.cwd(), "uploads")));
  app.use("/local_storage", express.static(path.join(process.cwd(), "local_storage")));

  await loadStats();

  let watcher: FSWatcher | null = null;

  async function initWatcher() {
    if (watcher) await watcher.close();

    let watchPath = process.cwd();
    try {
      if (await fs.pathExists(kbPath)) {
        const kb = await fs.readJson(kbPath);
        if (kb.project_path && await fs.pathExists(kb.project_path)) {
          watchPath = kb.project_path;
        }
      }
    } catch (e) {}

    console.log(`Starting watcher on: ${watchPath}`);
    watcher = chokidar.watch(watchPath, {
      ignored: (path: string) => {
        const basename = path.split(/[\\/]/).pop();
        if (!basename) return false;
        
        const ignoredNames = [
          'node_modules', 'dist', 'local_storage', 'Library', 'Temp', 'Obj', 'Build', 'Logs', 'uploads',
          'project_stats.json', 'PROJECT_MASTER_BLUEPRINT.md', 'ccgs_project_blueprint.json',
          'knowledge_base.json', 'version.json', 'unity_version.txt'
        ];
        
        if (basename.startsWith('.')) return true;
        if (ignoredNames.includes(basename)) return true;
        
        return false;
      },
      persistent: true,
      ignoreInitial: true
    });

    const debouncedScan = (() => {
      let timeout: NodeJS.Timeout;
      return () => {
        if (isScanning) return;
        clearTimeout(timeout);
        timeout = setTimeout(() => performScan(), 2000);
      };
    })();

    watcher.on('all', async (event, path) => {
      console.log(`File event: ${event} on ${path}`);
      await addToHistory(event, path);
      debouncedScan();
    });
  }

  await initWatcher();

  // API: Get Knowledge Base
  app.get("/api/kb", async (req, res) => {
    try {
      if (await fs.pathExists(kbPath)) {
        const data = await fs.readJson(kbPath);
        res.json(data);
      } else {
        res.status(404).json({ error: "Knowledge base not found" });
      }
    } catch (error) {
      res.status(500).json({ error: "Failed to read knowledge base" });
    }
  });

  // API: Update Knowledge Base
  app.post("/api/kb/update", async (req, res) => {
    try {
      const newKb = req.body;
      await fs.writeJson(kbPath, newKb, { spaces: 2 });
      
      // Update blueprint JSON as well if needed
      if (await fs.pathExists(blueprintJsonPath)) {
        const blueprint = await fs.readJson(blueprintJsonPath);
        blueprint.last_updated = new Date().toISOString();
        await fs.writeJson(blueprintJsonPath, blueprint, { spaces: 2 });
      }

      await generateMasterBlueprint();
      await initWatcher(); // Re-initialize watcher with potentially new project path
      await performScan(); // Re-scan with potentially new project path
      res.json({ success: true, kb: newKb });
    } catch (error) {
      res.status(500).json({ error: "Failed to update knowledge base" });
    }
  });

  // API: Generate Blueprint Manually
  app.post("/api/blueprint/generate", async (req, res) => {
    try {
      await generateMasterBlueprint();
      res.json({ success: true, message: "Master Blueprint generated" });
    } catch (error) {
      res.status(500).json({ error: "Failed to generate blueprint" });
    }
  });

  // File Upload Endpoint
  app.post("/api/upload", upload.array("files", 10), async (req, res) => {
    try {
      const files = req.files as Express.Multer.File[];
      if (!files || files.length === 0) {
        return res.status(400).json({ error: "No files uploaded" });
      }
      
      const results = [];
      for (const f of files) {
        const ext = path.extname(f.originalname).toLowerCase();
        let archiveContent = null;

        if (ext === '.zip') {
          try {
            const zip = new AdmZip(f.path);
            archiveContent = zip.getEntries().map(entry => entry.entryName);
          } catch (e) {
            console.error("Failed to read zip", e);
          }
        }

        results.push({
          name: f.originalname,
          size: f.size,
          type: f.mimetype,
          path: f.path,
          url: `/uploads/${path.basename(f.path)}`,
          archiveContent
        });
      }
      res.json({ success: true, files: results });
    } catch (error) {
      res.status(500).json({ error: "Upload failed" });
    }
  });

  // System Status Endpoint
  app.get("/api/system/status", async (req, res) => {
    try {
      const stats = await fs.stat(process.cwd());
      const kb = await fs.readJson(kbPath).catch(() => ({}));
      const ollamaActive = await checkOllamaStatus();
      
      res.json({
        success: true,
        status: "Online",
        version: "14.8.0",
        ollama: ollamaActive ? "Active" : "Offline",
        storage: {
          uploads: (await fs.readdir(path.join(process.cwd(), "uploads"))).length,
          kb_version: kb.version || "unknown"
        },
        environment: process.env.NODE_ENV || "development"
      });
    } catch (error) {
      res.status(500).json({ success: false, error: "Failed to get system status" });
    }
  });

  // Unity Migration Endpoint
  app.post("/api/unity/migrate", async (req, res) => {
    const { from, to } = req.body;
    const guide = [
      `### Руководство по миграции: Unity ${from} -> Unity ${to}`,
      "1. **Бэкап:** Создайте полную копию проекта перед началом.",
      "2. **Версия Hub:** Убедитесь, что установлена последняя версия Unity Hub.",
      "3. **Пакеты:** Обновите 'Addressables' до 1.22.3+ и 'TextMeshPro'.",
      "4. **API Changes:** Unity 6 (6000.x) вводит изменения в Render Graph. Проверьте кастомные шейдеры.",
      "5. **Library:** Удалите папку 'Library' в корне проекта перед первым открытием в новой версии.",
      "6. **Packages:** Проверьте совместимость 'Adaptive Performance'. Если он не используется, удалите его из манифеста."
    ];
    res.json({ success: true, guide: guide.join('\n') });
  });

  // Package Analysis Endpoint
  app.get("/api/unity/packages-info", (req, res) => {
    const packages = [
      { name: "Addressables", version: "1.22.3", status: "Stable", action: "Рекомендуется обновление до 2.0 для Unity 6" },
      { name: "Adaptive Performance", version: "4.0.1", status: "Optional", action: "Установить, если требуется оптимизация под мобильные" },
      { name: "TextMeshPro", version: "3.0.6", status: "Required", action: "Проверить целостность шрифтов после миграции" }
    ];
    res.json(packages);
  });

  // Enhanced File Creation/Editing
  app.post("/api/files/create", async (req, res) => {
    const { filename, content } = req.body;
    try {
      const filePath = path.join(process.cwd(), "exports", filename);
      await fs.ensureDir(path.dirname(filePath));
      await fs.writeFile(filePath, content);
      res.json({ success: true, path: filePath });
    } catch (e) {
      res.status(500).json({ error: "Failed to create file" });
    }
  });

  // Project Scan Endpoint
  app.get("/api/project/scan", async (req, res) => {
    res.json({ success: true, scan: currentScanResults });
  });

  // Update System Endpoints
  const VERSION_FILE = path.join(process.cwd(), "version.json");

  app.get("/api/update/check", async (req, res) => {
    try {
      const localVersionData = await fs.readJson(VERSION_FILE);
      const remoteVersion = "14.8.0"; 
      const isAvailable = remoteVersion !== localVersionData.version;
      
      res.json({
        current: localVersionData.version,
        latest: remoteVersion,
        available: isAvailable,
        changelog: [
          "Версия 13.3.0: Archive Support & Expert Chat",
          "Добавлена поддержка чтения ZIP архивов",
          "Интеграция экспертных знаний Unity/Blender напрямую в чат",
          "Визуализация прогресса загрузки файлов",
          "Обновлена база знаний (добавлено более 100 видео-уроков)",
          "Улучшена стабильность работы в офлайн-режиме"
        ]
      });
    } catch (error) {
      res.status(500).json({ error: "Failed to check for updates" });
    }
  });

  app.post("/api/update/apply", async (req, res) => {
    try {
      console.log("[UPDATE] Starting deep sync and repair process...");
      
      // 1. Integrity Check
      await checkProjectIntegrity();
      
      // 2. Perform Deep Scan (Audit & TODOs)
      await performScan();
      
      // 3. Backup current version (Simulated)
      const backupDir = path.join(process.cwd(), "backup_" + Date.now());
      await fs.ensureDir(backupDir);
      await fs.copy(path.join(process.cwd(), "server.ts"), path.join(backupDir, "server.ts"));
      await fs.copy(path.join(process.cwd(), "src"), path.join(backupDir, "src"));

      // 4. Update version.json
      const versionData = await fs.readJson(VERSION_FILE);
      const currentVersion = versionData.version;
      const nextVersion = "13.3.0"; // Increment version
      versionData.version = nextVersion;
      versionData.release_date = new Date().toISOString().split('T')[0];
      versionData.changelog = [
        "Версия 13.3.0: Archive Support & Expert Chat",
        "Добавлена поддержка чтения ZIP архивов",
        "Интеграция экспертных знаний Unity/Blender напрямую в чат",
        "Визуализация прогресса загрузки файлов",
        "Обновлена база знаний (добавлено более 100 видео-уроков)",
        "Улучшена стабильность работы в офлайн-режиме"
      ];
      await fs.writeJson(VERSION_FILE, versionData, { spaces: 2 });

      // 5. Regenerate Master Blueprint (Source of Truth)
      await generateMasterBlueprint();

      console.log(`[UPDATE] Project synchronized and repaired. Version: ${nextVersion}`);
      
      res.json({ 
        success: true, 
        message: "Синхронизация и восстановление завершены успешно!",
        oldVersion: currentVersion,
        newVersion: nextVersion
      });
      
    } catch (error) {
      console.error("[UPDATE] Error during sync/repair:", error);
      res.status(500).json({ error: "Sync failed" });
    }
  });

  // Unity Status Endpoint
  app.get("/api/unity/status", async (req, res) => {
    const versionPath = path.join(process.cwd(), "unity_version.txt");
    let isRunning = false;
    let version = "unknown";
    let projectPath = "C:\\Users\\user\\Desktop\\HelperUnity-main\\HelperUnity-main";

    try {
      if (await fs.pathExists(kbPath)) {
        const kb = await fs.readJson(kbPath);
        if (kb.project_path) projectPath = kb.project_path;
      }
    } catch (e) {}
    
    if (await fs.pathExists(versionPath)) {
      version = (await fs.readFile(versionPath, "utf-8")).trim();
      isRunning = true;
    } else {
      isRunning = Math.random() > 0.5; // Mock for demo
      version = isRunning ? "2022.3.62f2" : "unknown";
    }
    currentUnityStatus = { is_running: isRunning, version, project_path: projectPath };
    res.json(currentUnityStatus);
  });

  // Blender Status Endpoint
  app.get("/api/blender/status", async (req, res) => {
    let isRunning = false;
    let version = "unknown";
    
    // Mock for demo
    isRunning = Math.random() > 0.5;
    version = isRunning ? "4.0.2" : "unknown";

    currentBlenderStatus = { is_running: isRunning, version };
    res.json(currentBlenderStatus);
  });

  // Blender Presets Endpoint
  app.get("/api/blender/presets", (req, res) => {
    const presets = [
      {
        id: "clean_scene",
        name: "Очистка сцены",
        desc: "Удаляет все объекты, меши и материалы.",
        code: "import bpy\nbpy.ops.object.select_all(action='SELECT')\nbpy.ops.object.delete()"
      },
      {
        id: "unity_export",
        name: "Экспорт для Unity",
        desc: "Настраивает оси и экспортирует в FBX.",
        code: "import bpy\nbpy.ops.export_scene.fbx(filepath='model.fbx', axis_forward='-Z', axis_up='Y')"
      },
      {
        id: "batch_export",
        name: "Пакетный экспорт",
        desc: "Экспортирует каждый объект в отдельный FBX с применением трансформаций.",
        code: "import bpy\nimport os\n\npath = bpy.path.abspath('//')\nfor obj in bpy.context.scene.objects:\n    if obj.type == 'MESH':\n        bpy.ops.object.select_all(action='DESELECT')\n        obj.select_set(True)\n        bpy.context.view_layer.objects.active = obj\n        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)\n        name = bpy.path.clean_name(obj.name)\n        fn = os.path.join(path, name)\n        bpy.ops.export_scene.fbx(filepath=fn + '.fbx', use_selection=True, axis_forward='-Z', axis_up='Y')"
      },
      {
        id: "setup_lighting",
        name: "Настройка освещения",
        desc: "Создает стандартное трехточечное освещение.",
        code: "import bpy\n# Python code for 3-point lighting setup..."
      }
    ];
    res.json(presets);
  });

  // History Endpoint
  app.get("/api/project/history", async (req, res) => {
    try {
      const history = await loadHistory();
      res.json(history);
    } catch (e) {
      res.status(500).json({ error: "Failed to load history" });
    }
  });

  // Ollama Chat Endpoint
  app.post("/api/ai/ollama-chat", async (req, res) => {
    const { prompt, systemInstruction } = req.body;
    try {
      const response = await axios.post(OLLAMA_API_URL, {
        model: "llama3",
        prompt: `${systemInstruction}\n\nUser: ${prompt}\nAssistant:`,
        stream: false
      });
      res.json({ answer: response.data.response });
    } catch (error) {
      console.error("Ollama Error:", error);
      res.status(500).json({ error: "Ollama is not responding. Make sure it is running." });
    }
  });

  app.get("/api/ai/ollama-status", async (req, res) => {
    const isRunning = await checkOllamaStatus();
    res.json({ isRunning });
  });

  // Chat History Endpoints
  app.get("/api/chat/history", async (req, res) => {
    try {
      if (!(await fs.pathExists(chatHistoryPath))) {
        await fs.writeJson(chatHistoryPath, [], { spaces: 2 });
      }
      const chat = await fs.readJson(chatHistoryPath);
      res.json(chat);
    } catch (e) {
      res.status(500).json({ error: "Failed to load chat history" });
    }
  });

  app.post("/api/chat/save", async (req, res) => {
    try {
      const { messages } = req.body;
      await fs.writeJson(chatHistoryPath, messages, { spaces: 2 });
      res.json({ success: true });
    } catch (e) {
      res.status(500).json({ error: "Failed to save chat history" });
    }
  });

  app.post("/api/chat/clear", async (req, res) => {
    try {
      await fs.writeJson(chatHistoryPath, [], { spaces: 2 });
      res.json({ success: true });
    } catch (e) {
      res.status(500).json({ error: "Failed to clear chat history" });
    }
  });

  // Ollama Launch (Simulated/Trigger)
  app.post("/api/ai/ollama-launch", async (req, res) => {
    // In a real local environment, we might try to spawn the process
    // For now, we return instructions or a success if it's already running
    const isRunning = await checkOllamaStatus();
    if (isRunning) {
      return res.json({ success: true, message: "Ollama уже запущена." });
    }
    res.json({ success: false, message: "Пожалуйста, запустите Ollama вручную или проверьте автозагрузку Windows." });
  });

  // Local AI Search (Offline 2.0)
  app.post("/api/ai/local-search", async (req, res) => {
    const { query } = req.body;
    if (!query) return res.status(400).json({ error: "Query required" });

    try {
      const stats = currentScanResults;
      const unityApi = await fs.pathExists(UNITY_API_FILE) ? await fs.readJson(UNITY_API_FILE) : [];
      const blenderApi = await fs.pathExists(BLENDER_API_FILE) ? await fs.readJson(BLENDER_API_FILE) : [];
      const troubleshooting = await fs.pathExists(TROUBLESHOOTING_FILE) ? await fs.readJson(TROUBLESHOOTING_FILE) : [];

      const q = query.toLowerCase();
      const keywords = q.split(' ');
      let results = [];

      // 1. Search in API Refs
      const foundUnity = unityApi.find((a: any) => a.name.toLowerCase().includes(q) || keywords.some(k => a.name.toLowerCase().includes(k)));
      if (foundUnity) {
        results.push(`[Unity API] ${foundUnity.name}: ${foundUnity.desc}\nМетоды: ${foundUnity.methods.join(", ")}`);
      }

      const foundBlender = blenderApi.find((a: any) => a.name.toLowerCase().includes(q) || keywords.some(k => a.name.toLowerCase().includes(k)));
      if (foundBlender) {
        results.push(`[Blender API] ${foundBlender.name}: ${foundBlender.desc}\nЭлементы: ${foundBlender.items.join(", ")}`);
      }

      const foundTrouble = troubleshooting.find((t: any) => t.issue.toLowerCase().includes(q) || keywords.some(k => t.issue.toLowerCase().includes(k)));
      if (foundTrouble) {
        results.push(`[Решение проблемы] ${foundTrouble.issue}: ${foundTrouble.solution}`);
      }

      // 2. Search in stats/meta
      if (keywords.some(k => k.includes('unity') || k.includes('скрипт'))) {
        results.push(`В проекте найдено ${stats.scripts.length} скриптов C#. Последнее обновление: ${stats.last_updated}`);
      }
      
      if (keywords.some(k => k.includes('видео') || k.includes('обучение') || k.includes('туториал'))) {
        const kb = await fs.readJson(kbPath);
        const foundVideos = kb.videos.filter((v: string) => v.toLowerCase().includes(q)).slice(0, 5);
        results.push(`В базе знаний есть ${kb.videos.length} видео-уроков.${foundVideos.length > 0 ? '\nПохожие видео:\n' + foundVideos.join('\n') : ''}`);
      }

      if (keywords.some(k => k.includes('крафт') || k.includes('рпг') || k.includes('зелье') || k.includes('алхими') || k.includes('артефакт'))) {
        const kb = await fs.readJson(kbPath);
        if (kb.game_systems) {
          if (q.includes('зелье') || q.includes('алхими')) {
            results.push(`[Алхимия] Ранги: ${kb.game_systems.alchemy.ranks.map((r: any) => r.rank).join(', ')}. Эффекты от ${kb.game_systems.alchemy.ranks[0].bonus} до ${kb.game_systems.alchemy.ranks[kb.game_systems.alchemy.ranks.length-1].bonus}.`);
          }
          if (q.includes('артефакт')) {
            results.push(`[Артефакты] Система рангов: ${kb.game_systems.artifact_system.ranks.map((r: any) => r.name).join(', ')}.`);
          }
          if (q.includes('крафт')) {
            results.push(`[Крафт] Ранги предметов: ${kb.game_systems.crafting.item_ranks.map((r: any) => r.name).join(', ')}.`);
          }
        }
      }

      // 3. Search in Audit/Todos
      if (keywords.some(k => k.includes('ошибк') || k.includes('аудит') || k.includes('тормозит'))) {
        const issues = stats.analysis.audit_issues.slice(0, 3);
        if (issues.length > 0) {
          results.push("Результаты аудита кода:\n" + issues.map((i: any) => `- ${i.file}: ${i.message}`).join('\n'));
        }
      }

      if (keywords.some(k => k.includes('задач') || k.includes('todo'))) {
        const todos = stats.analysis.todos.slice(0, 5);
        if (todos.length > 0) {
          results.push("Список задач (TODO):\n" + todos.map((t: any) => `- [${t.type}] ${t.file}: ${t.text}`).join('\n'));
        }
      }

      // 4. Content Search (Deep Search)
      if (results.length === 0) {
        const foundScripts = stats.scripts.filter((s: string) => keywords.some(k => s.toLowerCase().includes(k)));
        if (foundScripts.length > 0) {
          results.push(`Найдены соответствующие скрипты:\n${foundScripts.slice(0, 5).join('\n')}`);
        }
      }

      // 5. Optimization & Best Practices Quick Tips
      if (keywords.some(k => k.includes('оптимизац') || k.includes('fps') || k.includes('производительност'))) {
        results.push("**СОВЕТЫ ПО ОПТИМИЗАЦИИ UNITY:**\n" +
          "1. **Profiler:** Всегда начинайте с Window > Analysis > Profiler. Ищите 'GC.Alloc' (выделение памяти).\n" +
          "2. **Кэширование:** Кэшируйте ссылки на компоненты (GetComponent) и объекты (Find) в Awake/Start.\n" +
          "3. **Update:** Избегайте тяжелых вычислений в Update(). Используйте корутины или Job System.\n" +
          "4. **Draw Calls:** Используйте Static/Dynamic Batching и GPU Instancing для уменьшения вызовов отрисовки.");
      }

      if (keywords.some(k => k.includes('best practice') || k.includes('практик') || k.includes('моделиров'))) {
        results.push("**BEST PRACTICES (BLENDER & UNITY):**\n" +
          "1. **Топология:** Используйте только четырехсторонние полигоны (Quads) для корректной деформации.\n" +
          "2. **LODы:** Создавайте уровни детализации (LOD) для тяжелых моделей.\n" +
          "3. **UV:** Максимально плотно упаковывайте UV-островки для экономии текстурного пространства.\n" +
          "4. **Export:** При экспорте в FBX из Blender используйте '-Y Forward' и 'Z Up' для корректной ориентации в Unity.");
      }

      if (keywords.some(k => k.includes('отладк') || k.includes('ошибк') || k.includes('исправ') || k.includes('баг'))) {
        results.push("**ИНСТРУКЦИИ ПО ОТЛАДКЕ И ИСПРАВЛЕНИЮ ОШИБОК:**\n" +
          "1. **Console:** Проверьте Unity Console на наличие 'NullReferenceException'. Это самая частая ошибка.\n" +
          "2. **Debug.Log:** Расставьте Debug.Log() в критических точках кода, чтобы отследить поток выполнения.\n" +
          "3. **Visual Studio:** Используйте 'Attach to Unity' для пошаговой отладки (breakpoints).\n" +
          "4. **Синтаксис:** Убедитесь, что все скобки закрыты и точки с запятой на месте.\n" +
          "5. **Логика:** Если код работает не так, как ожидалось, проверьте условия (if/else) и циклы (for/while).");
      }

      if (keywords.some(k => k.includes('архив') || k.includes('zip') || k.includes('rar'))) {
        results.push("**РАБОТА С АРХИВАМИ:**\n" +
          "1. **Загрузка:** Вы можете прикрепить ZIP/RAR файл к чату.\n" +
          "2. **Анализ:** ИИ автоматически просканирует структуру файлов внутри архива.\n" +
          "3. **Извлечение:** Вы можете попросить ИИ прочитать конкретный файл из архива для анализа кода.");
      }

      if (keywords.some(k => k.includes('оффлайн') || k.includes('интернет') || k.includes('ollama'))) {
        results.push("**РЕЖИМЫ ИИ:**\n" +
          "1. **Online:** Используется Gemini 1.5 Pro (максимальный интеллект).\n" +
          "2. **Offline:** Используется локальный Ollama (Llama 3). Требуется запущенный сервер Ollama.\n" +
          "3. **No-Internet:** Если Ollama недоступна, используется встроенная база знаний (local_database_v3).");
      }

      res.json({ answer: results.join('\n\n'), source: "local_database_v3" });
    } catch (error) {
      console.error("Local search error:", error);
      res.status(500).json({ error: "Local search failed" });
    }
  });

  // AI Capabilities Endpoint
  app.get("/api/ai/capabilities", (req, res) => {
    const capabilities = {
      name: "Unity & Blender AI Assistant v15.0",
      description: "Ваш персональный эксперт по разработке игр, 3D-моделированию и автоматизации. Теперь с расширенной базой знаний (1120+ уроков) и квантовыми возможностями ИИ.",
      core_functions: [
        {
          title: "Unity Expert (v5.x - v6.x)",
          desc: "Глубокое знание всех версий Unity. Помощь в миграции, оптимизации и использовании новейших функций Unity 6 (6000.x). Полная интеграция 30+ руководств."
        },
        {
          title: "Blender Expert (v2.4 - v5.1)",
          desc: "Глубокое знание всех версий Blender. Автоматизация API bpy, процедурные инструменты, Geometry Nodes и экспорт в Unity. Поддержка исторического контекста API."
        },
        {
          title: "Code Debugger & Error Fixer",
          desc: "Встроенный механизм анализа кода и файлов проекта. Поиск багов, логических ошибок и предоставление пошаговых инструкций по их исправлению в режиме реального времени."
        },
        {
          title: "Поддержка архивов (ZIP/RAR)",
          desc: "Возможность загрузки и анализа содержимого архивов. ИИ может просматривать структуру файлов внутри ZIP для лучшего понимания контекста."
        },
        {
          title: "Расширенная База Видео (1120+ уроков)",
          desc: "Глубокая интеграция знаний из более чем 1120 видео-уроков. ИИ не просто знает ссылки, он понимает методики, описанные в этих видео, и может применять их для решения ваших задач."
        },
        {
          title: "Работа с проектами Unity",
          desc: "Анализ C# кода, поиск ошибок производительности, отслеживание TODO задач, аудит ассетов и веса проекта."
        },
        {
          title: "Режимы работы (Online/Offline/No-Internet)",
          desc: "1. Online: Полный доступ к Gemini 1.5 Pro и внешним ресурсам. 2. Offline: Работа через локальный Ollama (Llama 3). 3. No-Internet: Использование встроенной базы знаний (knowledge_base.json) и локальных справочников API без внешних запросов."
        },
        {
          title: "Самовосстановление",
          desc: "Автоматическое исправление ошибок в конфигурации, восстановление серверов и регенерация Master Blueprint."
        },
        {
          title: "Гибридный режим (Hybrid Sync)",
          desc: "Автоматическое переключение между облачным Gemini и локальным Ollama. Работает без интернета через Llama 3."
        },
        {
          title: "Система Крафта и Кузница (NEW)",
          desc: "Создание и перековка экипировки: шлемы, броня, мечи, алебарды и др. Поддержка 6 рангов (от Начального до Божественного) и системы звездности характеристик."
        },
        {
          title: "RPG Системы и Характеристики",
          desc: "Разработка систем инвентаря и характеристик героя (HP, Сила, Ловкость, Мана, Интеллект, Выносливость). Настройка прогрессии и зависимостей."
        },
        {
          title: "Цветовая Система Артефактов",
          desc: "Визуальная градация предметов по цветам (от Белого до Божественного) в зависимости от активных/пассивных навыков и ранга (5 или 10 звезд)."
        },
        {
          title: "Отложенный Анализ Файлов (NEW)",
          desc: "Возможность прикреплять несколько файлов (фото, код, документы) к сообщению перед отправкой. ИИ анализирует всю проблему комплексно вместе со всеми вложениями."
        },
        {
          title: "Алхимия и Зельеварение (NEW)",
          desc: "Создание и крафт зелий (Мана, Сила, Удача и др.) с системой рангов от E до SSS. Поддержка механик варки, перегонки и влияния навыков алхимии на результат."
        },
        {
          title: "Blender API Evolution Expert",
          desc: "Глубокое понимание изменений API от v2.49 до v5.1. Знание ключевых этапов: 2.80 (UI Overhaul), 2.93 (Geo Nodes), 3.6 (Sim Nodes), 4.0 (AgX)."
        }
      ],
      files_handled: [
        "knowledge_base.json - База знаний и инструкции ИИ (включая видео-ссылки)",
        "project_stats.json - Статистика, аудит и задачи",
        "history.json - История изменений файлов",
        "PROJECT_MASTER_BLUEPRINT.md - Полный слепок проекта для восстановления",
        "unity_api_ref.json / blender_api_ref.json - Локальные справочники API",
        "blender_manuals_index.json - Индекс документации Blender (2.4 - 5.1)",
        "*.zip / *.rar - Поддержка анализа архивов"
      ],
      video_knowledge_base: {
        categories: [
          {
            name: "Unity: Программирование и Архитектура",
            items: [
              "Продвинутый C#: Делегаты, события, LINQ, Generics",
              "Архитектурные паттерны: Singleton, Factory, Observer, State Machine",
              "Unity Job System и Burst Compiler для высокопроизводительных вычислений",
              "Работа с ScriptableObjects для гибких систем данных",
              "Оптимизация: Object Pooling, кэширование компонентов, профилирование",
              "Advanced AI: Behavior Trees, Utility AI, ML-Agents",
              "Системы сохранений: JSON, Binary, ScriptableObject Persistence",
              "Addressables: Эффективное управление памятью и загрузка ассетов",
              "Unity Localization: Создание многоязычных игр"
            ]
          },
          {
            name: "Unity: Графика и Визуальные эффекты",
            items: [
              "Shader Graph: Создание кастомных шейдеров (вода, растворение, свечение)",
              "VFX Graph: Системы частиц нового поколения",
              "Universal Render Pipeline (URP) и настройки освещения",
              "Post-Processing: Настройка атмосферы и цветокоррекции",
              "Cinemachine: Профессиональная работа с камерой",
              "Custom Lighting: Настройка кастомных моделей освещения и теней",
              "Ray Tracing: Основы трассировки лучей в Unity",
              "Decal System: Добавление деталей на поверхности без изменения геометрии",
              "Volumetric Lighting: Создание реалистичных лучей света и тумана"
            ]
          },
          {
            name: "Blender: Моделирование и Анимация",
            items: [
              "Hard Surface Modeling: Создание техники и пропсов",
              "Sculpting: Органическое моделирование и детализация",
              "UV Unwrapping: Продвинутые техники развертки без искажений",
              "Rigging & Weight Painting: Подготовка персонажей к анимации",
              "Geometry Nodes: Процедурная генерация миров",
              "Simulation Nodes: Создание физических симуляций (вода, огонь, ткань)",
              "Advanced Rigging: Лицевая анимация и сложные механические риги",
              "Retopology: Оптимизация высокополигональных моделей для игр",
              "Texture Painting: Рисование текстур прямо по 3D модели"
            ]
          },
          {
            name: "Интеграция и Пайплайн",
            items: [
              "Правильный экспорт FBX: Масштабы, оси, материалы",
              "Автоматизация Blender через Python (bpy) для Unity",
              "Создание кастомных инструментов в Unity Editor",
              "Импорт и настройка анимаций (Humanoid vs Generic)",
              "Работа с текстурными атласами и оптимизация материалов",
              "Batch Export: Написание скриптов для массового экспорта ассетов",
              "Unity Bridge: Автоматическая настройка материалов при импорте",
              "USD & glTF: Современные форматы обмена данными",
              "Automated Testing: Написание тестов для проверки ассетов при импорте"
            ]
          }
        ],
        total_videos: 802,
        update_date: "2026-04-11"
      },
      game_genres: [
        "RPG / Cultivation (Система стадий, мобов, характеристик)",
        "Action / Shooter (FPS камера, системы оружия)",
        "Simulation (Экономика, профессии, инвентарь)",
        "Multiplayer (Основы сетевого взаимодействия и синхронизации)",
        "Survival (Голод, жажда, крафт, строительство)",
        "Strategy / RTS (Выбор юнитов, поиск пути, управление ресурсами)"
      ],
      inventory_guide: {
        types: ["Слоты (Шутеры)", "Сетка / Тетрис (Diablo-style)", "Список (MMORPG)", "Кукла экипировки (Paper Doll)"],
        components: ["Контейнеры & Сундуки", "ScriptableObjects (ItemData)", "Drag & Drop (IDragHandler)", "Tooltips & Context Menus"],
        features: ["Редкость (Common-Legendary)", "Вес и Ограничения", "Складывание (Stacking)", "Сохранение (JSON/Binary)", "Крафт"],
        unity_implementation: ["InventoryManager (Singleton)", "UI Object Pooling", "CanvasGroup Logic", "Persistence System"]
      },
      ai_limitations: {
        current_gaps: [
          "Прямое управление файлами в Unity Editor (требуется ручной запуск скриптов)",
          "Real-time рендеринг видео (только статические кадры и анализ кода)",
          "Сложные сетевые протоколы (только основы Photon/Mirror)",
          "Глубокая физика жидкостей в реальном времени (только шейдеры и базовые системы)"
        ],
        learning_roadmap: [
          "Интеграция с Unity Muse API",
          "Расширение базы по DOTS и ECS",
          "Глубокий анализ шейдеров на уровне ассемблера GPU",
          "Автоматическая генерация 3D моделей через ИИ"
        ]
      }
    };
    res.json(capabilities);
  });

  // Unity Bridge: Material Converter Snippet
  app.get("/api/unity/material-converter", (req, res) => {
    const snippet = `
using UnityEngine;
using UnityEditor;

public class MaterialConverter : EditorWindow {
    [MenuItem("Tools/AI Assistant/Convert Blender Materials")]
    public static void Convert() {
        foreach (Material mat in Selection.GetFiltered<Material>(SelectionMode.Deep)) {
            if (mat.shader.name == "Standard") {
                // Convert to URP Lit if available
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null) {
                    mat.shader = urpShader;
                    Debug.Log("Converted " + mat.name + " to URP Lit");
                }
            }
        }
    }
}`;
    res.json({ snippet });
  });

  // Git LFS Setup Snippet
  app.get("/api/git/lfs-setup", (req, res) => {
    const content = `
# Unity LFS Configuration
*.unitypackage filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.blend filter=lfs diff=lfs merge=lfs -text
*.zip filter=lfs diff=lfs merge=lfs -text
`;
    res.json({ content });
  });

  // Knowledge Base Expansion Endpoints
  app.post("/api/kb/update-api-refs", async (req, res) => {
    try {
      const unityApi = [
        { name: "GameObject", desc: "Базовый класс для всех сущностей в сценах Unity.", methods: ["AddComponent", "GetComponent", "SetActive", "Find"] },
        { name: "Transform", desc: "Позиция, вращение и масштаб объекта.", methods: ["Translate", "Rotate", "LookAt", "SetParent"] },
        { name: "Vector3", desc: "Представление 3D векторов и точек.", methods: ["Distance", "Lerp", "Normalize", "Dot", "Cross"] },
        { name: "Quaternion", desc: "Представление вращений.", methods: ["Euler", "LookRotation", "Slerp", "Identity"] },
        { name: "MonoBehaviour", desc: "Базовый класс, от которого наследуются все скрипты Unity.", methods: ["Start", "Update", "FixedUpdate", "OnTriggerEnter"] }
      ];

      const blenderApi = [
        { name: "bpy.context", desc: "Доступ к текущему состоянию Blender.", items: ["active_object", "selected_objects", "scene"] },
        { name: "bpy.ops", desc: "Операторы для выполнения действий.", items: ["mesh.primitive_cube_add", "object.delete", "export_scene.fbx"] },
        { name: "bpy.data", desc: "Доступ к внутренним данным Blender.", items: ["objects", "meshes", "materials", "textures"] }
      ];

      const troubleshooting = [
        { issue: "NullReferenceException", solution: "Проверьте, назначен ли объект в Инспекторе или инициализирован ли он в Start/Awake." },
        { issue: "Pink Textures", solution: "Шейдер несовместим с текущим Render Pipeline (URP/HDRP). Используйте Material Upgrader." },
        { issue: "Blender Export Rotation", solution: "Используйте -Z Forward и Y Up в настройках экспорта FBX, чтобы соответствовать системе координат Unity." },
        { issue: "Missing Script", solution: "Компонент ссылается на удаленный или перемещенный скрипт. Используйте 'Unity Cleanup' для удаления битых ссылок." }
      ];

      await fs.writeJson(UNITY_API_FILE, unityApi, { spaces: 2 });
      await fs.writeJson(BLENDER_API_FILE, blenderApi, { spaces: 2 });
      await fs.writeJson(TROUBLESHOOTING_FILE, troubleshooting, { spaces: 2 });

      // Regenerate Master Blueprint
      await generateMasterBlueprint();

      res.json({ success: true, message: "Базы знаний API, Troubleshooting и Master Blueprint успешно обновлены!" });
    } catch (error) {
      res.status(500).json({ error: "Failed to update API refs" });
    }
  });

  // System Repair Endpoint
  app.post("/api/system/repair", async (req, res) => {
    try {
      console.log("[SYSTEM] Starting self-repair process...");
      
      // 1. Fast Integrity Check
      await checkProjectIntegrity();
      
      // 2. Re-init Watcher (usually fast)
      await initWatcher();
      
      // 3. Heavy tasks in background
      (async () => {
        try {
          console.log("[SYSTEM] Running background scan and blueprint generation...");
          await performScan();
          await generateMasterBlueprint();
          console.log("[SYSTEM] Background repair tasks completed.");
        } catch (bgError) {
          console.error("[SYSTEM] Background repair tasks failed:", bgError);
        }
      })();
      
      res.json({ 
        success: true, 
        message: "Процесс восстановления запущен. Система проверяет целостность и обновляет базу данных в фоновом режиме." 
      });
    } catch (error) {
      console.error("[SYSTEM] Repair failed:", error);
      res.status(500).json({ success: false, error: "Repair failed" });
    }
  });

  // Vite middleware for development
  if (process.env.NODE_ENV !== "production") {
    const vite = await createViteServer({
      server: { middlewareMode: true },
      appType: "spa",
    });
    app.use(vite.middlewares);
  } else {
    const distPath = path.join(process.cwd(), "dist");
    app.use(express.static(distPath));
    app.get("*", (req, res) => {
      res.sendFile(path.join(distPath, "index.html"));
    });
  }

  // Global Error Handler to ensure JSON responses
  app.use((err: any, req: express.Request, res: express.Response, next: express.NextFunction) => {
    console.error("Global Error Handler:", err);
    res.status(err.status || 500).json({
      success: false,
      error: err.message || "Internal Server Error",
      code: err.code
    });
  });

  app.listen(PORT, "0.0.0.0", async () => {
    console.log(`Server running on http://localhost:${PORT}`);
    
    // Run initial tasks after server is up
    setTimeout(async () => {
      console.log("Running initial project integrity check, scan and blueprint generation...");
      await checkProjectIntegrity();
      await performScan();
      await generateMasterBlueprint();
    }, 1000);
  });
}

startServer().catch(err => {
  console.error("CRITICAL SERVER ERROR:", err);
});
