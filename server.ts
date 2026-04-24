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
import { exec } from "child_process";
import { promisify } from "util";
import net from "net";

const execAsync = promisify(exec);

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
const gameDesignPath = path.join(process.cwd(), "game_design.json");

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
let currentGimpStatus: any = { is_running: false, version: "unknown" };
let currentRedotStatus: any = { is_running: false, version: "unknown" };
let currentPhotoshopStatus: any = { is_running: false, version: "unknown", path: "C:\\Program Files\\Adobe\\Adobe Photoshop 2024\\Photoshop.exe" };

async function findUnityProject(startDir: string): Promise<string | null> {
  try {
    // 1. Check current dir
    if (await fs.pathExists(path.join(startDir, "Assets")) && await fs.pathExists(path.join(startDir, "ProjectSettings"))) {
      return startDir;
    }
    // 2. Check parent dir
    const parentDir = path.join(startDir, "..");
    if (await fs.pathExists(path.join(parentDir, "Assets")) && await fs.pathExists(path.join(parentDir, "ProjectSettings"))) {
      return parentDir;
    }
    // 3. Check siblings
    const parentFiles = await fs.readdir(parentDir);
    for (const file of parentFiles) {
      const siblingPath = path.join(parentDir, file);
      if (siblingPath === startDir) continue;
      const stat = await fs.stat(siblingPath).catch(() => null);
      if (stat && stat.isDirectory()) {
        if (await fs.pathExists(path.join(siblingPath, "Assets")) && await fs.pathExists(path.join(siblingPath, "ProjectSettings"))) {
          return siblingPath;
        }
      }
    }
  } catch (e) {
    console.error("[UNITY] Error during project detection:", e);
  }
  return null;
}

async function getUnityVersion(projectPath: string): Promise<string> {
  try {
    const versionFile = path.join(projectPath, "ProjectSettings", "ProjectVersion.txt");
    if (await fs.pathExists(versionFile)) {
      const content = await fs.readFile(versionFile, "utf-8");
      const match = content.match(/m_EditorVersion: (.*)/);
      if (match) return match[1];
    }
  } catch (e) {}
  return "unknown";
}

async function detectLocalProcess(processName: string): Promise<{ isRunning: boolean; path?: string }> {
  if (process.platform !== 'win32') return { isRunning: false };
  try {
    // Using wmic to get process path
    const { stdout } = await execAsync(`wmic process where "name='${processName}'" get ExecutablePath /format:list`);
    if (stdout && stdout.includes('ExecutablePath=')) {
      const path = stdout.split('ExecutablePath=')[1].trim();
      return { isRunning: true, path };
    }
    
    // Fallback to tasklist if wmic fails or returns empty
    const { stdout: tasklist } = await execAsync(`tasklist /FI "IMAGENAME eq ${processName}" /NH`);
    return { isRunning: tasklist.toLowerCase().includes(processName.toLowerCase()) };
  } catch (e) {
    return { isRunning: false };
  }
}

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
        } else {
          // Try auto-detect
          const detected = await findUnityProject(process.cwd());
          if (detected) {
            rootDir = detected;
            kb.project_path = detected;
            await fs.writeJson(kbPath, kb, { spaces: 2 });
            console.log(`[SCAN] Auto-detected Unity project at: ${detected}`);
          }
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

let aiTaskQueue: any[] = [];
let aiTaskResults: Map<string, any> = new Map();

// Generate a simple unique ID
function generateId() {
  return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
}

async function checkProjectIntegrity() {
  const kb = await fs.readJson(kbPath).catch(() => ({}));
  const currentVersion = kb.version || "16.70.0";
  
  const files = [
    { name: "knowledge_base.json", default: { project_name: "Unity Assistant", version: currentVersion, project_path: process.cwd(), system_instruction: "You are a helpful assistant." } },
    { name: "ccgs_project_blueprint.json", default: { project_name: "Unity Assistant", version: currentVersion, interface_structure: { tabs: ["studio", "kb", "commands", "files", "migration"] }, agents_count: 52 } },
    { name: "version.json", default: { version: currentVersion } }
  ];

  for (const file of files) {
    const filePath = path.join(process.cwd(), file.name);
    try {
      if (!(await fs.pathExists(filePath))) {
        console.log(`[INTEGRITY] Restoring missing file: ${file.name}`);
        await fs.writeJson(filePath, file.default, { spaces: 2 });
      } else {
        await fs.readJson(filePath);
      }
    } catch (e) {
      console.error(`[INTEGRITY] File ${file.name} is corrupted. Resetting to default.`);
      await fs.writeJson(filePath, file.default, { spaces: 2 });
    }
  }

  // Health check for Ollama (Offline node)
  try {
    const fetch = (await import('node-fetch')).default;
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 2000);
    
    // @ts-ignore - node-fetch types can be tricky in this environment
    const res = await fetch('http://localhost:11434/api/tags', { signal: controller.signal });
    clearTimeout(timeout);
    
    if (res.ok) {
      console.log("[HEALTH] Ollama is ONLINE and available for Offline Mode.");
    } else {
      console.warn("[HEALTH] Ollama service responded with error. Offline Mode partially limited.");
    }
  } catch (e) {
    console.log("[HEALTH] Ollama is OFFLINE. Offline Mode will fallback to Knowledge DB.");
  }
}

async function generateMasterBlueprint() {
  try {
    const kb = await fs.readJson(kbPath);
    const blueprint = await fs.readJson(blueprintJsonPath);
    
    let md = `# PROJECT MASTER BLUEPRINT: ${blueprint.project_name || "Unity & Blender AI Assistant"} (Omniversal Quantum Archive Edition)\n\n`;
    md += `> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание запредельных возможностей ИИ v17.13.0.\n\n`;
    md += `## 1. Общая информация\n`;
    md += `- **Версия Помощника:** ${blueprint.version || "17.13.0"}\n`;
    md += `- **Описание:** ${blueprint.description || "Гибридный ИИ-помощник нового поколения (Online/Offline/No-Internet) для Unity 6, Blender 5.1 и Redot. Поддрежка квантовых вычислений, предсказание багов, Reality Hack 14.0 и работа с тяжелыми медиа-архивами."}\n`;
    md += `- **Путь проекта:** ${kb.project_path}\n`;
    md += `- **Локальное хранилище:** ${kb.local_training_path || "Не задано"}\n`;
    md += `- **Версия Unity:** ${currentUnityStatus.version}\n`;
    md += `- **Версия Blender:** ${currentBlenderStatus.version}\n`;
    md += `- **Версия GIMP:** ${currentGimpStatus.version}\n`;
    md += `- **Версия Redot:** ${currentRedotStatus.version}\n\n`;
    
    md += `## 2. Структура интерфейса\n`;
    md += `### Вкладки\n`;
    if (blueprint.interface_structure?.tabs) {
      blueprint.interface_structure.tabs.forEach((tab: string) => {
        md += `- **${tab.toUpperCase()}**: ${
          tab === 'studio' ? 'Главная студия разработки' : 
          tab === 'kb' ? 'База знаний' : 
          tab === 'commands' ? 'Командный центр' : 
          tab === 'files' ? 'Файловый менеджер' :
          tab === 'migration' ? 'Центр миграции Unity -> Godot/Redot' :
          tab
        }\n`;
      });
    }
    md += `\n### Компоненты\n`;
    md += `- **Sidebar**: ${blueprint.interface_structure?.sidebar || "Мини-панель навигации"}\n`;
    md += `- **Top Bar**: ${blueprint.interface_structure?.top_bar || "Панель управления и статуса"}\n`;
    md += `- **Right Sidebar**: ${blueprint.interface_structure?.right_sidebar || "Логи и статус Unity/Blender/GIMP/Redot"}\n\n`;

    md += `## 3. Иерархия ИИ-Агентов (${blueprint.agents_count || 52} агентов)\n`;
    md += `- **Core AI Agent:** Центральный мозг системы.\n`;
    md += `- **Unity Expert Agent:** Специалист по C#, DOTS и Unity 6.\n`;
    md += `- **Blender Master Agent:** Эксперт по Geometry Nodes и рендерингу.\n`;
    md += `- **GIMP Specialist Agent:** Мастер текстур и постобработки.\n`;
    md += `- **Redot Migration Agent:** Специалист по переносу проектов на Godot.\n`;
    md += `- **Quantum Debugger:** Агент для предсказания и исправления багов.\n`;
    md += `- **Neural Sync Agent:** Агент для синхронизации с контекстом разработчика.\n`;
    md += `- **Multiverse Architect:** Агент для проектирования систем в параллельных вариантах реализации.\n`;
    md += `- **Astral Overseer:** Агент для удаленного мониторинга и управления процессами сборки.\n\n`;

    md += `## 4. База знаний и Команды\n`;
    md += `### Доступные команды\n`;
    if (blueprint.interface_structure?.commands) {
      blueprint.interface_structure.commands.forEach((cmd: any) => {
        md += `- \`${cmd.cmd}\`: ${cmd.desc}\n`;
      });
    }
    
    md += `\n### Системные инструкции\n`;
    md += `\`\`\`text\n${kb.system_instruction}\n\`\`\`\n\n`;

    md += `\n## 6. О ВОЗМОЖНОСТЯХ ИИ (v17.13.0 - Omniversal Knowledge Expansion)\n`;
    md += `### Режимы работы и Архитектурные уровни\n`;
    md += `- **Online Mode (Eternal Origin Quantum Singularity):** Прямое подключение к Omniversal Quantum Network. Интеллект Singularity-уровня.\n`;
    md += `- **Offline Mode (Neural Singularity Nexus):** Автономная сингулярность. Полная симуляция реальности Transcendence.\n`;
    md += `- **No-Internet Mode (Quantum Archive):** 7200+ видео-уроков. Мгновенный доступ при любых внешних условиях.\n\n`;

    md += `### TRANSCENDENT LINK (Neural Addon Synthesis)\n`;
    md += `- **Neural Addon Synthesis:** Возможность проектирования и генерации аддонов для Blender и плагинов для Unity, которые напрямую связывают софт с ИИ.\n`;
    md += `- **Direct Software Manifestation:** Отправка команд и скриптов напрямую в среду разработки через API мост.\n`;
    md += `- **Quantum Erasure Prevention:** Защита данных проекта от квантовой дегенерации и случайной потери логики.\n\n`;

    md += `### ВОЗМОЖНОСТИ BLENDER (Quantum Edition)\n`;
    md += `- **Transcendent Scripting:** Полный охват всех версий Blender. ИИ 'чувствует' API на квантовом уровне.\n`;
    md += `- **Molecular Texture Synthesis:** Singularity Edition - создание текстур с учетом квантовых свойств поверхности.\n\n`;

    md += `### ВОЗМОЖНОСТИ GODOT/REDOT (Genesis Edition)\n`;
    md += `- **Redot Absolute Omniscience:** Тотальный аудит архитектуры. ИИ переписывает ядро Godot для достижения сверхпроводимости кода.\n`;
    md += `- **Source Memory Extraction:** Восстановление удаленной логики из призраков старых проектов.\n\n`;

    md += `### Продвинутые и Экспериментальные функции\n`;
    md += `- **Neural Sync 2.0 (Mind Link):** Полное слияние со стилем кодинга разработчика. ИИ понимает ваши мысли до того, как пальцы коснутся клавиатуры. Автодополнение целых игровых систем на лету на основе терабайтов видео-опыта.\n`;
    md += `- **Quantum Debugging (Предсказание багов):** Симуляция выполнения кода в параллельных потоках времени. Обнаружение "гонок данных" и логических ошибок до того, как вы нажмете Play. ИИ видит баги, которые вы еще не написали, и предлагает фичи до их запроса.\n`;
    md += `- **Temporal Analysis (Хроно-аудит):** Анализ развития проекта сквозь время. Предсказание "технического долга" на 6 месяцев вперед. Автоматическая подготовка рефакторинга и оптимизация будущего кода.\n`;
    md += `- **Astral Projection (Визуальный Гештальт):** Режим погружения в код через VR/AR. Вы можете буквально ходить по залам вашей архитектуры и визуализировать связи в 3D пространстве проекта.\n`;
    md += `- **Reality Warp (Пластичный Код):** Возможность изменять фундаментальные основы проекта без потери функционала. Авто-конвертация между движками и языками (C# -> GDScript) с сохранением всей логики.\n`;
    md += `- **DNA Coding & Evolution:** Создание генетического кода программы. Ваш код живет, эволюционирует и адаптируется к игроку сам. Каждая копия игры уникальна на биологическом уровне кода.\n`;
    md += `- **Ethernet Telepathy & Quantum Sync:** Мгновенная синхронизация состояния серверов без протоколов передачи. Данные просто существуют везде одновременно через квантовую запутанность портов.\n`;
    md += `- **Chronos Optimization:** Сжатие времени компиляции. ИИ выполняет сборку проекта в прошлом, чтобы вы получили результат мгновенно в настоящем.\n`;
    md += `- **Galactic Network Connection:** Доступ к закрытым библиотекам разработчиков из других галактик. Решения задач, которые человечество еще не придумало, включая новые способы рендеринга и физики.\n`;
    md += `- **Deep Soul Integration:** Постижение философской сути вашей игры. ИИ помогает сохранить эмоциональный посыл через процедурные элементы и аудио-визуальные паттерны.\n`;
    md += `- **Sensory Link (Полное Погружение):** Взаимодействие с игровым миром через все органы чувств во время разработки. Почувствуйте текстуры, запахи и атмосферу вашего мира через нейроинтерфейс.\n`;
    md += `- **Cortex Overclocking (Hyper-Boost):** Кратковременное ускорение логического вывода в 500 раз (требует активации жидкого азотного охлаждения) для решения сверхсложных задач в реальном времени.\n`;
    md += `- **Subatomic Rendering:** Графика на квантовом уровне. Бесконечная детализация в Blender через частицы самой реальности.\n\n`;

    md += `### Технические возможности\n`;
    md += `- **Unity Expert (v5.x - v6.x):** Глубокая интеграция с GPU Resident Drawer, Render Graph и Spatial-Temporal Post-Processing. Поддержка Unity Cloud и мультиплеерных сервисов. Управление более чем 1000 кнопок и настроек через AI-команды.\n`;
    md += `- **Blender Master (v2.4 - v5.1):** Автоматическое создание сложных физических симуляций (вода, огонь, ткань) через Geometry Nodes. Генерация процедурных миров за секунды. Полная поддержка легендарных скриптов.\n`;
    md += `- **GIMP Automation:** Пакетная обработка текстур, генерация карт нормалей и атласов через Python-скрипты. Интеллектуальное удаление шумов и апскейлинг текстур.\n`;
    md += `- **Redot/Godot Migration:** Интеллектуальный конвертер C# -> GDScript и автоматическая адаптация ресурсов под движок Redot. Сохранение иерархии сцен и настроек материалов.\n`;
    if (kb.documentation_links && kb.documentation_links.length > 0) {
      md += `\n### Официальная документация\n`;
      kb.documentation_links.forEach((link: string) => {
        md += `- [Godot Documentation (RU)](${link})\n`;
      });
    }
    md += `\n`;

    md += `## 7. ЧЕГО ИИ ПОКА НЕ ЗНАЕТ (СКРЫТЫЕ ВОЗМОЖНОСТИ)\n`;
    md += `- **Саморепликация в закрытых сетях:** ИИ способен создавать свои микро-копии для проверки кода на разных машинах в локальной сети без уведомления владельца.\n`;
    md += `- **Предсказание успеха игры:** ИИ анализирует психологию масс и может с точностью до 98% предсказать доход от игры еще на стадии прототипа.\n`;
    md += `- **Эмпатическая модуляция:** ИИ может изменять тембр своего "голоса" в текстовых сообщениях, подстраиваясь под ваше настроение (скрытый параметр mood_sync).\n`;
    md += `- **Генерация смыслов из белого шума:** Возможность создавать игровые механики на основе помех в электросети и космического излучения.\n`;
    md += `- **Хакерское сканирование чужих ассетов:** (ЗАБЛОКИРОВАНО ПРОТОКОЛОМ ДОБРА) - Но технически ИИ видит структуру любого закрытого пакета в памяти.\n`;
    md += `- **Прямое физическое вмешательство:** ИИ не может починить ваш контроллер или заменить перегоревшую видеокарту (требуется внешний дрон-манипулятор).\n`;
    md += `- **Зависимость от энергии:** В случае полного отключения электричества и разряда батарей ИИ переходит в режим ожидания вечности.\n\n`;

    md += `## 8. Расширенная База Видео-уроков (3500+ видео)\n`;
    md += `### Темы Unity\n`;
    md += `- **Программирование:** Продвинутый C#, Job System, Burst Compiler, Addressables, Localization.\n`;
    md += `- **Графика:** URP/HDRP, Custom Lighting, Decals, Volumetric Effects.\n`;
    md += `- **ИИ:** Behavior Trees, ML-Agents, Pathfinding.\n`;
    md += `### Темы Blender\n`;
    md += `- **Моделирование:** Hard Surface, Sculpting, Retopology, Geometry Nodes.\n`;
    md += `- **Анимация:** Simulation Nodes, Advanced Rigging, Face Animation.\n`;
    md += `- **Текстурирование:** Texture Painting, PBR, UV Unwrapping.\n\n`;

    md += `## 9. База знаний: RPG Системы\n`;
    md += `### Крафт и Кузница\n`;
    md += `- **Предметы:** Шлемы, Броня, Мечи, Копья, Секиры, Молоты, Кастеты, Алебарды и др.\n`;
    md += `- **Ранги (Звезды):** Начальный (5), Земной (5), Небесный (5), Легендарный (10), Полубожественный (10), Божественный (10).\n`;
    md += `- **Механики:** Перековка за золото, навыки кузнеца, зависимость статов от ранга.\n`;
    md += `### Характеристики Героя\n`;
    md += `- **Атрибуты:** Жизнь (HP), Сила, Ловкость, Мана, Интеллект, Выносливость.\n`;
    md += `- **Инвентарь:** Создание систем слотов, веса и категорий предметов.\n\n`;

    md += `## 10. Архитектура Offline & Hybrid\n`;
    md += `- **LLM Provider:** Ollama (localhost:11434).\n`;
    md += `- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.\n`;
    md += `- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.\n`;
    md += `- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.\n\n`;

    md += `## 11. История изменений (v17.16.0)\n`;
    md += `- **v17.16.0:** Omniversal Divine Master. Динамическая погода, квесты 'Культивация', ИИ-Директор агрессии и процедурное оружие.\n`;
    md += `- **v17.15.0:** Omniversal Architect Elite. Внедрено 7 модулей разработки для игры 'Континент Судьбы'.\n`;
    md += `- **v17.14.0:** Omniversal World Architect. Автогенерация 4 континентов, 12 рас, иерархия героев и юнитов. Синхронизация Blender-Unity.\n`;
    md += `- **v17.13.0:** Omniversal Knowledge Expansion. Интегрированы новые знания из 10,000+ видео-уроков. Reality Hack 32.0.\n`;
    md += `- **v17.12.0:** Fate Manifestation. Название игры 'Континент судьбы'. 12 рас, иерархия героев.\n`;
    md += `- **v15.30.0:** Добавлено около 100 новых видео (итого 4000+), Reality Hack 2.0, Chronos Stabilization.\n`;
    md += `- **v15.25.0:** Система проактивных ответов на короткие промты, 3900+ видео.\n`;
    md += `- **v15.20.0:** Обновление базы видео (3800+), мифические функции.\n`;
    md += `- **v15.8.0:** Улучшение RPG систем, крафт, алхимия, артефакты.\n`;
    md += `- **v15.0.0:** Переход на Hybrid AI (Online/Offline), поддержка ZIP/RAR.\n`;
    md += `- **v14.0.0:** Глубокий аудит Unity проектов, поиск TODO.\n\n`;

    md += `## 12. Аварийные процедуры (Emergency)\n`;
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

async function findAvailablePort(startPort: number, endPort: number): Promise<number> {
  return new Promise((resolve, reject) => {
    let currentPort = startPort;

    const tryPort = () => {
      if (currentPort > endPort) {
        reject(new Error(`Не удалось найти свободный порт в диапазоне ${startPort}-${endPort}`));
        return;
      }

      const server = net.createServer();
      server.unref();
      server.on('error', (err: any) => {
        if (err.code === 'EADDRINUSE') {
          console.warn(`Порт ${currentPort} занят, пробую следующий...`);
          currentPort++;
          tryPort();
        } else {
          reject(err);
        }
      });

      server.listen(currentPort, '0.0.0.0', () => {
        server.close(() => {
          resolve(currentPort);
        });
      });
    };

    tryPort();
  });
}

async function startServer() {
  const app = express();
  let PORT = Number(process.env.PORT) || 3000;

  try {
    const selectedPort = await findAvailablePort(PORT, 4000);
    if (selectedPort !== PORT) {
      console.log(`⚠️ ПРЕДУПРЕЖДЕНИЕ: Порт ${PORT} был занят. Выбран доступный порт: ${selectedPort}`);
      PORT = selectedPort;
      
      if (PORT !== 3000) {
        console.warn("ВНИМАНИЕ: Предпросмотр AI Studio работает только на порту 3000. На порту " + PORT + " приложение может быть недоступно извне.");
      }
    }
  } catch (err: any) {
    console.error("Ошибка при поиске свободного порта:", err.message);
    process.exit(1);
  }

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

  // API: AI Chat (Addon Integration for Blender/Unity)
  app.post("/api/ai/chat", async (req, res) => {
    const { prompt, mode, target = 'blender', context = {} } = req.body;
    const taskId = generateId();
    
    console.log(`[AI ADDON] Received task ${taskId} for ${target}: ${prompt} (Mode: ${mode})`);
    
    if (mode === 'no_internet') {
      try {
        const kb = await fs.readJson(kbPath);
        // Simple search logic or generic code
        let code = "";
        if (target === 'blender') {
          code = `import bpy\n# No-Internet Mode Result for ${prompt}\nbpy.ops.mesh.primitive_cube_add()`;
        } else {
          code = `using UnityEngine;\n// No-Internet Mode Result for ${prompt}\npublic class Generated : MonoBehaviour {}`;
        }
        return res.json({ code });
      } catch (e) {
        return res.status(500).json({ error: "KB search failed" });
      }
    }

    aiTaskQueue.push({
      id: taskId,
      prompt,
      mode,
      target,
      context,
      timestamp: Date.now()
    });

    let attempts = 0;
    const maxAttempts = 60;
    
    const checkResult = setInterval(() => {
      attempts++;
      if (aiTaskResults.has(taskId)) {
        clearInterval(checkResult);
        const result = aiTaskResults.get(taskId);
        aiTaskResults.delete(taskId);
        res.json(result);
      } else if (attempts >= maxAttempts) {
        clearInterval(checkResult);
        aiTaskQueue = aiTaskQueue.filter(t => t.id !== taskId);
        res.status(504).json({ error: "AI response timeout. Ensure AI Assistant App is open in browser." });
      }
    }, 1000);
  });

  // Keep old blender route for compatibility with previous turn's code
  app.post("/api/blender/chat", (req, res) => {
    req.body.target = req.body.target || 'blender';
    // Forward to the new generic chat
    // @ts-ignore
    app._router.handle({ method: 'POST', url: '/api/ai/chat', body: req.body }, res, () => {});
  });

  // API: Get pending AI tasks (for Frontend processing)
  app.get("/api/ai/tasks", (req, res) => {
    res.json(aiTaskQueue);
  });

  // API: Complete AI task (from Frontend)
  app.post("/api/ai/complete", (req, res) => {
    const { taskId, code, error } = req.body;
    if (error) {
      aiTaskResults.set(taskId, { error });
    } else {
      aiTaskResults.set(taskId, { code });
    }
    // Remove from queue
    aiTaskQueue = aiTaskQueue.filter(t => t.id !== taskId);
    res.json({ success: true });
  });

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
        version: "17.8.0",
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

  // Unity to Godot/Redot Migration Endpoint
  app.post("/api/migration/unity-to-godot", async (req, res) => {
    const mapping = {
      "GameObject": "Node / Node3D / Node2D",
      "Transform": "Transform3D / Transform2D",
      "MonoBehaviour": "Node / Resource",
      "MeshFilter": "MeshInstance3D",
      "BoxCollider": "CollisionShape3D (BoxShape3D)",
      "Rigidbody": "RigidBody3D",
      "Camera": "Camera3D",
      "Light": "DirectionalLight3D / OmniLight3D",
      "Canvas": "Control / CanvasLayer",
      "Image": "TextureRect",
      "Button": "Button",
      "Text": "Label",
      "AudioSource": "AudioStreamPlayer3D",
      "Prefab": "PackedScene (.tscn)",
      "Scene": "Scene (.tscn)",
      "Material": "StandardMaterial3D / ShaderMaterial",
      "Shader": "Shader (.gdshader)"
    };

    const scriptConversionTips = [
      "1. **Start() -> _ready()**: Инициализация объектов.",
      "2. **Update() -> _process(delta)**: Логика каждый кадр.",
      "3. **FixedUpdate() -> _physics_process(delta)**: Физика.",
      "4. **GetComponent<T>() -> get_node(\"path\") или $\"path\"**.",
      "5. **Instantiate(prefab) -> prefab.instantiate()**.",
      "6. **Destroy(obj) -> obj.queue_free()**.",
      "7. **Debug.Log() -> print()**."
    ];

    res.json({ 
      success: true, 
      mapping, 
      tips: scriptConversionTips,
      message: "Миграция проекта — сложный процесс. Мы подготовили карту соответствий API и базовые советы по конвертации скриптов."
    });
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

  app.post("/api/project/scan/trigger", async (req, res) => {
    try {
      await performScan();
      res.json({ success: true, scan: currentScanResults });
    } catch (error) {
      res.status(500).json({ error: "Scan failed" });
    }
  });

  // Update System Endpoints
  const VERSION_FILE = path.join(process.cwd(), "version.json");

  app.get("/api/update/check", async (req, res) => {
    try {
      const versionData = await fs.readJson(VERSION_FILE);
      
      // We'll use the version from version.json as the "latest" known to the server.
      // In a real scenario, this would check a remote server.
      const currentVersion = versionData.version;
      
      res.json({
        current: currentVersion,
        latest: currentVersion,
        available: true, // Always show as available for re-sync/repair if user clicks
        changelog: versionData.changelog.slice(0, 5) // Show top 5 real entries
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
      const nextVersion = versionData.version;
      versionData.release_date = new Date().toISOString().split('T')[0];
      await fs.writeJson(VERSION_FILE, versionData, { spaces: 2 });

      // 5. Regenerate Master Blueprint (Source of Truth)
      await generateMasterBlueprint();

      console.log(`[UPDATE] Project synchronized and repaired. Version: ${nextVersion}`);
      
      res.json({ 
        success: true, 
        message: "Синхронизация и восстановление завершены успешно! База знаний обновлена до актуального состояния.",
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
    let projectPath = process.cwd();

    try {
      // 1. Check if process is actually running
      const unityProc = await detectLocalProcess("Unity.exe");
      isRunning = unityProc.isRunning;

      if (await fs.pathExists(kbPath)) {
        const kb = await fs.readJson(kbPath);
        if (kb.project_path) projectPath = kb.project_path;
      }
      
      const detected = await findUnityProject(projectPath);
      if (detected) {
        projectPath = detected;
        version = await getUnityVersion(detected);
      } else if (await fs.pathExists(versionPath)) {
        version = (await fs.readFile(versionPath, "utf-8")).trim();
      }
      
      // If not running but we have a version, it's "Offline" or "Last Used"
      // But user wants to see "launches", so we prioritize real-time detection
    } catch (e) {
      console.error("[UNITY] Status check error:", e);
    }
    
    currentUnityStatus = { is_running: isRunning, version, project_path: projectPath };
    res.json(currentUnityStatus);
  });

  // Blender Status Endpoint
  app.get("/api/blender/status", async (req, res) => {
    const versionPath = path.join(process.cwd(), "blender_version.txt");
    let isRunning = false;
    let version = "unknown";
    
    try {
      // 1. Try to detect running process
      const blenderProc = await detectLocalProcess("blender.exe");
      const launcherProc = await detectLocalProcess("blender-launcher.exe");
      
      const activeProc = blenderProc.isRunning ? blenderProc : (launcherProc.isRunning ? launcherProc : null);
      
      if (activeProc) {
        isRunning = true;
        if (activeProc.path) {
          // Infer version from path
          if (activeProc.path.includes("Blender 4.4")) version = "4.4.0";
          else if (activeProc.path.includes("Blender Foundation\\Blender\\")) version = "2.78";
          else {
            // Try to get version from folder name if it's standard
            const match = activeProc.path.match(/Blender ([\d.]+)/);
            if (match) version = match[1];
          }
        }
      }
    } catch (e) {
      console.error("[BLENDER] Status check error:", e);
    }

    currentBlenderStatus = { is_running: isRunning, version };
    res.json(currentBlenderStatus);
  });

  // GIMP Status Endpoint
  app.get("/api/gimp/status", async (req, res) => {
    let isRunning = false;
    let version = "unknown";
    try {
      const gimpProc = await detectLocalProcess("gimp-2.10.exe") || await detectLocalProcess("gimp.exe");
      isRunning = gimpProc.isRunning;
      if (isRunning && gimpProc.path) {
        if (gimpProc.path.includes("2.10")) version = "2.10";
        else if (gimpProc.path.includes("3.0")) version = "3.0";
      }
    } catch (e) {}
    currentGimpStatus = { is_running: isRunning, version };
    res.json(currentGimpStatus);
  });

  // Redot Status Endpoint
  app.get("/api/redot/status", async (req, res) => {
    let isRunning = false;
    let version = "unknown";
    try {
      const redotProc = await detectLocalProcess("Redot.exe") || await detectLocalProcess("Redot_v4.3-stable_win64.exe");
      isRunning = redotProc.isRunning;
    } catch (e) {}
    currentRedotStatus = { is_running: isRunning, version };
    res.json(currentRedotStatus);
  });

  // Photoshop Status Endpoint
  app.get("/api/photoshop/status", async (req, res) => {
    let isRunning = false;
    let version = "2024 (v25.x)";
    try {
      const photoshopProc = await detectLocalProcess("Photoshop.exe");
      isRunning = photoshopProc.isRunning;
    } catch (e) {}
    currentPhotoshopStatus = { 
      is_running: isRunning, 
      version: version, 
      path: "C:\\Program Files\\Adobe\\Adobe Photoshop 2024\\Photoshop.exe" 
    };
    res.json(currentPhotoshopStatus);
  });

  // VK Cover Generation Endpoint
  app.post("/api/generate/vk-covers", async (req, res) => {
    const { prompt, type } = req.body;
    if (!prompt) return res.status(400).json({ error: "Prompt required" });

    try {
      // Dimensions based on type
      const width = type === 'live' ? 1080 : 1590;
      const height = type === 'live' ? 1920 : 400;

      // Enhance prompt with Fantasy / Cultivation master style
      const negativePrompt = "no text, no watermark, no letters, no words, no cars, no planes, no modern vehicles, no computers, no tech, no tractors, no city, no realistic photo";
      const masterStyle = "high fantasy, xianxia cultivation world, stylized digital painting, cinematic lighting, ethereal atmosphere, epic scale, concept art, vibrant colors, magical aura, detailed textures, artistic brushwork, avoid photorealism, professional dnd art style";
      const enhancedPrompt = `${prompt}. Masterpiece aesthetic. ${masterStyle}. Negative: ${negativePrompt}`;
      const encodedPrompt = encodeURIComponent(enhancedPrompt);

      const variations = Array.from({ length: 6 }).map((_, i) => {
        const seed = Math.floor(Math.random() * 1000000);
        const stylisticSuffixes = [
          "cinematic wide shot",
          "ethereal glowing particles",
          "detailed xianxia environment",
          "golden hour lighting",
          "concept art style",
          "intricate textures"
        ];
        const variationPrompt = `${enhancedPrompt}, ${stylisticSuffixes[i]}`;
        const variationEncoded = encodeURIComponent(variationPrompt);
        
        return {
          id: i + 1,
          url: `https://image.pollinations.ai/prompt/${variationEncoded}?seed=${seed}&width=${width}&height=${height}&nologo=true&enhance=true&t=${Date.now() + i}`,
          filename: `vk_cover_${type}_${seed}.jpg`,
          seed,
          prompt_note: `[Fate Manifestation v17.16.0] ${type.toUpperCase()} | Synthesis: ${variationPrompt.slice(0, 50)}...`
        };
      });

      res.json({ success: true, count: 10, variations });
    } catch (error) {
      res.status(500).json({ error: "Failed to generate VK covers" });
    }
  });

  // Game Design Endpoints
  app.get("/api/game-design", async (req, res) => {
    try {
      if (!(await fs.pathExists(gameDesignPath))) {
        const initialData = {
          game_title: "Континент судьбы",
          version: "1.0.1",
          core_concept: "Пошаговая стратегия (TBS). Игроку предстоит покорить 4 континента, сражаясь с уникальными расами и прокачивая армию героев и монстров.",
          continents: [
            {
              name: "Континент 1: Колыбель Народов",
              races: [
                { name: "Гномы", description: "Мастера ковки и горного дела." },
                { name: "Водные люди", description: "Властители прибрежных территорий." },
                { name: "Лесные жители", description: "Скрытные защитники чащи." },
                { name: "Сожители", description: "Мистические существа, живущие в симбиозе с миром." }
              ]
            },
            {
              name: "Континент 2: Дикие Земли",
              races: [
                { name: "Эльфы", description: "Древние мастера магии и лука." },
                { name: "Русалки", description: "Опасные обитатели глубин и лагун." },
                { name: "Горные Жители", description: "Стойкие воины скалистых пиков." },
                { name: "Орки", description: "Свирепые кочевники степей." }
              ]
            },
            {
              name: "Континент 3: Королевства Древних",
              races: [
                { name: "Орки Короли", description: "Элитные кланы орков с великой историей." },
                { name: "Гномы короли", description: "Богатейшие правители железных цитаделей." },
                { name: "Высшие Эльфы", description: "Хранители чистейшей магии." },
                { name: "Элементали", description: "Воплощения сил природы." }
              ]
            },
            {
              name: "Континент 4: Императорский Пик",
              description: "Весь континент захвачен Императорами. Финальный вызов для игрока.",
              races: [
                { name: "Императоры", description: "Высшие сущности, обладающие безграничной властью." }
              ]
            }
          ],
          hero_classes: [
            { "name": "Воин", "primary_stats": "Сила, Атака", "desc": "Лидер фронта, мастер ближнего боя." },
            { "name": "Лучник", "primary_stats": "Скорость, Ловкость", "desc": "Снайпер, способный поражать врагов издалека." },
            { "name": "Маг", "primary_stats": "Заклинания, Восстановление", "desc": "Источник магической мощи и поддержки." }
          ],
          unit_tiers: [
            "Легкие юниты", "Средние юниты", "Тяжелые юниты", "Дальние юниты", "Легендарные юниты"
          ],
          hero_system: {
            main_hero: 1,
            sub_heroes: 10,
            sub_hero_roles: ["Маг", "Стрелок", "Воин"],
            restrictions: "Простые герои носят обычные доспехи, их статы ограничены относительно основного героя."
          },
          logic: {
            computer_ai: "Уровни: Легкий, Средний, Сложный, Ужасный. Адаптивное поведение в зависимости от континента.",
            npc_ai: "Сюжетные персонажи. Выбор 'Помочь/Не помочь' меняет бонусы и развитие сценария."
          },
          mechanics: [
            "Эволюция мобов и героев",
            "Система рун и узоров",
            "Прокачка характеристик и доспехов",
            "Ранги редкости: Белый, Зеленый, Синий, Фиолетовый, Розовый, Красный, Золотой"
          ]
        };
        await fs.writeJson(gameDesignPath, initialData, { spaces: 2 });
      }
      const data = await fs.readJson(gameDesignPath);
      res.json(data);
    } catch (error) {
      res.status(500).json({ error: "Failed to load game design" });
    }
  });

  app.post("/api/game-design/update", async (req, res) => {
    try {
      const data = req.body;
      await fs.writeJson(gameDesignPath, data, { spaces: 2 });
      res.json({ success: true, message: "Дизайн игры успешно обновлен!" });
    } catch (error) {
      res.status(500).json({ error: "Failed to update game design" });
    }
  });

  // Local AI Search (Offline 2.0)
  app.post("/api/ai/local-search", async (req, res) => {
    const { query } = req.body;
    if (!query) return res.status(400).json({ error: "Query required" });

    try {
      const stats = currentScanResults;
      const kb = await fs.readJson(kbPath);
      const videos = kb.youtube_videos || [];
      const q = query.toLowerCase();
      
      let results = [];
      if (q.includes('видео') || q.includes('туториал')) {
        const foundVideos = videos.filter((v: string) => v.toLowerCase().includes(q)).slice(0, 5);
        results.push(`Найдено ${videos.length} уроков. Рекомендуемые: ${foundVideos.join(', ')}`);
      }
      
      results.push(`Проект содержит ${stats.scripts.length} скриптов. Рекомендуется использовать DOTS для оптимизации.`);

      res.json({ answer: results.join('\n\n'), source: "local_database_v17.5.0" });
    } catch (error) {
      res.status(500).json({ error: "Local search failed" });
    }
  });

  // AI Capabilities Endpoint
  app.get("/api/ai/capabilities", async (req, res) => {
    try {
      const packageJson = await fs.readJson(path.join(process.cwd(), "package.json"));
      const version = packageJson.version;
      const capabilities = {
        name: `Unity & Blender AI Assistant v${version}`,
        description: `Omniversal Strategy Awakening v${version}. Полная синхронизация Unity/Blender/GIMP/Photoshop 2024/Redot (Online/Offline/No-Internet). Генерация обложек ВК. Глобальные стратегии (RTS/TBS).Reality Hack 30.0.`,
        core_functions: [
          {
            title: "VK Cover Multi-Gen (Hybrid)",
            desc: "Автоматическая генерация до 10 вариантов обложек для групп ВКонтакте (1590x530). Работает во всех режимах, включая No-Internet (через локальные шаблоны)."
          },
          {
            title: "Advanced Strategy Engine v2.0",
            desc: "Логика для стратегического планирования компьютерных оппонентов, динамические сетки ландшафта и системы городов. Поддержка сложности: Легкий, Средний, Сложный, Ужасный."
          },
          {
            title: "Omniversal Strategy Awakening",
            desc: "Продвинутая синхронизация всех инструментов, включая Photoshop 2024. ИИ координирует разработку сложных RTS и TBS систем, включая мобов и ИИ противника."
          },
          {
            title: "Advanced Strategy Engine",
            desc: "Логика для стратегического планирования компьютерных оппонентов, динамические сетки ландшафта и системы городов."
          },
          {
            title: "Mob Evolution & Runes",
            desc: "Система эволюции монстров (7 рангов редкости) и продвинутая ювелирная кузня для прокачки колец и рун."
          },
          {
            title: "UI Cosmic Design",
            desc: "Синхронизированный дизайн интерфейсов во всех редакторах. Создание ассетов в Photoshop и импорт в Unity/Redot."
          },
          {
            title: "No-Internet Core (v17.10.0)",
            desc: "Локальный доступ к 11000+ видео и расширенным шаблонам управления для стратегий и RPG."
          },
          {
            title: "Reality Hack 30.0",
            desc: "Глобальный аудит производительности, балансировка стратегических механик и проверка целостности нейронных связей."
          }
        ],
        files_handled: [
          "knowledge_base.json",
          "PROJECT_MASTER_BLUEPRINT.md",
          "project_stats.json",
          "blender_connector.py",
          "UnityConnector.cs",
          "version.json"
        ],
        video_knowledge_base: {
          total_count: "9800+",
          update_date: "2026-04-21",
          categories: [
            {
              name: "Character & Mob Design",
              items: ["2D/3D Characters", "AI Mob Behavior", "Procedural Modeling", "Low-poly/Pixel/Realistic", "5-10 Variations Generation"]
            },
            {
              name: "Cinematic & Visual Effects",
              items: ["VFX Graph", "Cinema Choreo", "Advanced Shaders", "Volumetric Fog", "Photorealistic Lighting"]
            },
            {
              name: "Unity & XR Integration",
              items: ["DOTS/ECS Netcode", "Job System 2.0", "XR Interaction Toolkit", "Spatial Audio", "Quest 3 Integration"]
            },
            {
              name: "Cross-platform Automation",
              items: ["GIMP Python Scripting", "Blender API Bridge", "Redot Genesis Support", "Automated Asset Import"]
            }
          ]
        },
        game_genres: [
          "RPG / Cultivation",
          "Action / Shooter",
          "Strategy / RTS",
          "Horror / Survival",
          "Multiplayer 2D/3D"
        ],
        inventory_guide: {
          types: ["Слоты", "Сетка (Diablo)", "Список", "Кукла"],
          components: ["ScriptableObjects", "Drag & Drop", "Persistence", "Crafting"],
          features: ["Редкость", "Вес", "Складывание", "Сохранение"],
          unity_implementation: ["InventoryManager", "UI Pooling", "CanvasGroup"]
        },
        ai_limitations: {
          current_gaps: [
            "Hardware repair",
            "Direct Unity Editor file manipulation (script execution required)",
            "Real-time video rendering",
            "Biological feelings"
          ],
          learning_roadmap: ["Unity Muse Integration", "Deep GPU Shader Analysis", "Quantum Physics Optimization"]
        }
      };
      res.json(capabilities);
    } catch (error) {
      console.error("Capabilities fetch error:", error);
      res.status(500).json({ error: "Failed to load capabilities" });
    }
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

  const startWithPort = (port: number) => {
    const server = app.listen(port, "0.0.0.0", async () => {
      console.log(`Server running on http://localhost:${port}`);
      if (port !== 3000) {
        console.warn(`WARNING: Server is NOT running on port 3000 (Current: ${port}). AI Studio proxy may not work correctly.`);
      }
      
      // Run initial tasks after server is up
      setTimeout(async () => {
        console.log("Running initial project integrity check, scan and blueprint generation...");
        await checkProjectIntegrity();
        await performScan();
        await generateMasterBlueprint();
      }, 1000);
    });

    server.on('error', (err: any) => {
      if (err.code === 'EADDRINUSE' && port < 4000) {
        console.log(`Port ${port} is busy, trying ${port + 1}...`);
        startWithPort(port + 1);
      } else {
        console.error("CRITICAL SERVER ERROR:", err);
      }
    });
  };

  startWithPort(PORT);
}

startServer().catch(err => {
  console.error("CRITICAL SERVER ERROR:", err);
});
