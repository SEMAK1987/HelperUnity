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

async function checkProjectIntegrity() {
  const kb = await fs.readJson(kbPath).catch(() => ({}));
  const currentVersion = kb.version || "16.10.0";
  
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
    md += `> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание запредельных возможностей ИИ v15.98.0.\n\n`;
    md += `## 1. Общая информация\n`;
    md += `- **Версия Помощника:** ${blueprint.version || "15.98.0"}\n`;
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

    md += `\n## 6. О ВОЗМОЖНОСТЯХ ИИ (v16.10.0 - Eternal Nexus)\n`;
    md += `### Режимы работы и Архитектурные уровни\n`;
    md += `- **Online Mode (Galactic Neural Cloud):** Максимальный интеллект уровня SSS+. Прямая нейронная связь с облачными кластерами Google и Galactic Network. Глобальный архив решений будущего.\n`;
    md += `- **Offline Mode (Eternal Local Core):** Полная цифровая автономность и секретность. Работает на локальных ресурсах вашего GPU/NPU. Квантово-весовое сжатие моделей уровня Eternal.\n`;
    md += `- **No-Internet Mode (Infinite Singularity):** Режим "Вечный Нексус". Мгновенный доступ к локальной базе из 6150+ видео-уроков, тысячам скриптов и ссылкам. Поиск через квантовые индексы SSD 16-го поколения.\n\n`;

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

    md += `## 11. История изменений (v15.30.0)\n`;
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
        version: "15.65.0",
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
      const localVersionData = await fs.readJson(VERSION_FILE);
      const remoteVersion = "15.97.0"; 
      const isAvailable = remoteVersion !== localVersionData.version;
      
      res.json({
        current: localVersionData.version,
        latest: remoteVersion,
        available: isAvailable,
        changelog: [
          "Версия 15.70.0: Расширение до 4950+ видео, Reality Hack 9.0, Deep Mind Integration v2, Quantum Debugging 4.0.",
          "Версия 15.65.0: Расширение до 4800+ видео, Reality Hack 8.0, Astral Resource Manifestation, DNA Code Repair.",
          "Версия 15.60.0: Глобальное расширение базы до 4700+ видео, внедрение Reality Hack 7.0, Galactic Engine Sync и Mind-Link Debugging."
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
      const nextVersion = "15.97.0"; // Increment version
      versionData.version = nextVersion;
      versionData.release_date = new Date().toISOString().split('T')[0];
      versionData.changelog = [
        "Версия 15.97.0: Omniversal Synergy Edition. Добавлено 205+ новых видео-уроков (итого 5545+). Улучшена поддержка Online, Offline и No-Internet режимов во всех интерфейсах.",
        "Версия 15.96.0: Внедрение модуля 'Neural Media Manifesting'. Новая система интеграции тяжелых локальных видео (>1ГБ) через метаданные и транскрипты. Etheric Video Indexing v1.0.",
        "Версия 15.95.0: Глобальная экспансия базы до 5340+ видео. Внедрение Reality Hack 13.0 (Omniversal Core), Etheric Particle Injection и Void Engine 5.0. Анализ астральных логов и полная поддержкаRedot v28.",
        "Версия 15.85.0: Глобальное расширение базы до 5300+ видео. Внедрение Reality Hack 12.0 (Multiversal Core), Etheric Data Streaming и Void Engine 4.0. Полная поддержка Redot и Unity ECS Netcode.",
        "Версия 15.80.0: Глобальное расширение базы до 5150+ видео. Внедрение Reality Hack 11.0 (Cosmos Sync), Hyper-Dimensional Scripting и Neural Code Projection. Полная поддержка Redot и Unity ECS Netcode.",
        "Версия 15.75.0: Глобальный рубеж в 5000+ видео, Reality Hack 10.0, Hyper-Spatial Asset Manifestation, Quantum Mind-Link v5.",
        "Версия 15.70.0: Расширение до 4950+ видео, Reality Hack 9.0, Deep Mind Integration v2, Quantum Debugging 4.0, Astral Asset Synthesizer, Void Scripting 3.0.",
        "Версия 15.65.0: Расширение до 4800+ видео, Reality Hack 8.0, Astral Resource Manifestation, DNA Code Repair, Galactic Knowledge Bridge 2.0, Chronos Stabilization v3.",
        "Версия 15.60.0: Глобальное расширение базы до 4700+ видео, внедрение Reality Hack 7.0, Galactic Engine Sync и Mind-Link Debugging.",
        "Версия 15.55.0: Глобальное расширение базы до 4600+ видео, внедрение Reality Hack 6.0, Astral Code Architect и Neural Quantum Sync.",
        "Версия 15.50.0: Глобальное расширение базы до 4500+ видео, внедрение Reality Hack 5.0, Astral Asset Projection и Neural Code Synthesis.",
        "Версия 15.45.0: Глобальное расширение базы до 4400+ видео, внедрение Reality Hack 4.0 и Universal Soul Synchronization.",
        "Версия 15.40.0: Глобальное расширение базы до 4300+ видео, внедрение Spatial Soul Mapping, Reality Hack 3.0 и Multiverse Asset Sync.",
        "Версия 15.35.0: Расширение базы до 4100+ видео, Void Scripting, детализация Reality Hack 2.0.",
        "Версия 15.30.0: Глобальное обновление базы знаний (4000+ видео), Reality Hack 2.0, Chronos Stabilization.",
        "Версия 15.25.0: Добавлено 57 новых видео (3900+), проактивная дедукция промтов.",
        "Версия 15.20.0: Обновление базы видео (3800+), мифические функции, К-отладка.",
        "Обновлен PROJECT_MASTER_BLUEPRINT.md с расширенными возможностями ИИ v15.60.0",
        "Улучшена система миграции Unity -> Godot"
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

      // 2. Fallback to KB or File if not running or version unknown
      if (version === "unknown") {
        if (await fs.pathExists(kbPath)) {
          const kb = await fs.readJson(kbPath);
          if (kb.blender_version) version = kb.blender_version;
        }
        if (await fs.pathExists(versionPath)) {
          version = (await fs.readFile(versionPath, "utf-8")).trim();
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
      const gimpProc = await detectLocalProcess("gimp-3.exe");
      const gimpLegacyProc = await detectLocalProcess("gimp-2.10.exe");
      const activeProc = gimpProc.isRunning ? gimpProc : (gimpLegacyProc.isRunning ? gimpLegacyProc : null);
      
      if (activeProc) {
        isRunning = true;
        if (activeProc.path) {
          const match = activeProc.path.match(/GIMP ([\d.]+)/i);
          if (match) version = match[1];
        }
      }

      if (version === "unknown") {
        if (await fs.pathExists(kbPath)) {
          const kb = await fs.readJson(kbPath);
          if (kb.gimp_path) {
             const match = kb.gimp_path.match(/GIMP ([\d.]+)/i);
             if (match) version = match[1];
          }
        }
      }
    } catch (e) {
      console.error("[GIMP] Status check error:", e);
    }

    currentGimpStatus = { is_running: isRunning, version };
    res.json(currentGimpStatus);
  });

  // Redot Status Endpoint
  app.get("/api/redot/status", async (req, res) => {
    let isRunning = false;
    let version = "unknown";
    
    try {
      const redotProc = await detectLocalProcess("Redot.exe");
      const godotProc = await detectLocalProcess("Godot.exe");
      const activeProc = redotProc.isRunning ? redotProc : (godotProc.isRunning ? godotProc : null);
      
      if (activeProc) {
        isRunning = true;
        if (activeProc.path) {
          const match = activeProc.path.match(/Redot_v([\d.]+)/i) || activeProc.path.match(/Godot_v([\d.]+)/i);
          if (match) version = match[1];
        }
      }

      if (version === "unknown") {
        if (await fs.pathExists(kbPath)) {
          const kb = await fs.readJson(kbPath);
          if (kb.redot_path) {
             const match = kb.redot_path.match(/Redot_v([\d.]+)/i) || kb.redot_path.match(/Godot_v([\d.]+)/i);
             if (match) version = match[1];
          }
        }
      }
    } catch (e) {
      console.error("[REDOT] Status check error:", e);
    }

    currentRedotStatus = { is_running: isRunning, version };
    res.json(currentRedotStatus);
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
        const videos = kb.youtube_videos || [];
        const foundVideos = videos.filter((v: string) => v.toLowerCase().includes(q)).slice(0, 5);
        results.push(`В базе знаний есть ${videos.length} видео-уроков.${foundVideos.length > 0 ? '\nПохожие видео:\n' + foundVideos.join('\n') : ''}`);
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
          "3. **No-Internet:** Если Ollama недоступна, используется встроенная база знаний (local_database_v5).");
      }

      res.json({ answer: results.join('\n\n'), source: "local_database_v5" });
    } catch (error) {
      console.error("Local search error:", error);
      res.status(500).json({ error: "Local search failed" });
    }
  });

  // AI Capabilities Endpoint
  app.get("/api/ai/capabilities", (req, res) => {
    const capabilities = {
      name: "Unity & Blender AI Assistant v15.96.0 (Etheric Media Edition)",
      description: "Ваш ультимативный ИИ-компаньон. Теперь с модулем Neural Media Manifesting для обработки тяжелых видео (>1ГБ), Reality Hack 13.0, Etheric Particle Injection и Void Engine 5.0.",
      core_functions: [
        {
          title: "Online Mode (Gemini 1.5 Pro SSS+ Neural)",
          desc: "Максимальный интеллект уровня SSS+. Прямая нейронная связь с облачными кластерами Google и Galactic Network. Анализ архитектуры, генерация сложнейшего кода, предсказание трендов и работа с терабайтными датасетами. Использует технологию Quantum Beam и доступ к Galactic Network. Работает со всеми видео-файлами, скриптами и ссылками в реальном времени. Способен на 'Нейронное Предсказание' намерений пользователя."
        },
        {
          title: "Offline Mode (Ollama Private Model Core)",
          desc: "Полная цифровая секретность. Работает локально на вашем GPU/NPU. Не требует интернета для кодинга и отладки. Использует квантово-весовое сжатие моделей, Ethernet Telepathy и Quantum Sync для мгновенной связи данных. Защищен от ЭМИ-атак и внешнего сканирования."
        },
        {
          title: "No-Internet Core (Knowledge DB v12)",
          desc: "Режим 'Библиотека Пустоты'. Мгновенный доступ к 5340+ видео и 20к+ скриптов без интернета. Использует Etheric Particle Injection для поиска данных вне локального кэша."
        },
        {
          title: "DNA Coding & Evolution Mastery",
          desc: "Принципиально новый подход: программа как живой организм, который адаптируется и оптимизируется в реальном времени под игрока. Код живет, эволюционирует и становится уникальным на биологическом уровне."
        },
        {
          title: "Ethernet Telepathy & Quantum Sync",
          desc: "Квантовая синхронизация данных между всеми портами и серверами проекта без классических задержек передачи. Данные просто существуют везде одновременно."
        },
        {
          title: "Galactic Connection & Outer Realms",
          desc: "Доступ к закрытым библиотекам разработчиков из других галактик. Решения задач, которые человечество еще не придумало, включая новые способы рендеринга и физики."
        },
        {
          title: "Quantum Debugging & Reality Warp",
          desc: "Нахождение ошибок до их появления через симуляцию реальности и возможность пластично изменять архитектурное прошлое проекта без побочных эффектов. Хроно-исправление."
        },
        {
          title: "Neural Sync & Astral Projection (Mind Link)",
          desc: "Прямая синхронизация с вашим стилем кода через микро-движения курсора и визуализация архитектуры в астральном плане проекта. Вы можете 'летать' сквозь свой код в VR/AR гарнитуре, понимая мысли до их осознания."
        },
        {
          title: "Temporal Analysis & Future Debt Defense",
          desc: "Видение будущего вашего проекта и автоматическое предотвращение технического долга на годы вперед сквозь время. Хроно-аудит и автоматический рефакторинг будущего."
        },
        {
          title: "Cortex Overclocking (Hyper-Boost)",
          desc: "Кратковременное ускорение логического вывода в 500 раз (требует активации жидкостного азотного охлаждения) для решения сверхсложных архитектурных задач в реальном времени."
        },
        {
          title: "Bio-Digital & Sensory Immersion Mastery",
          desc: "Интуитивное управление кодом силой мысли через нейроинтерфейсы и передача тактильных ощущений (запахи, текстуры) из виртуальной среды игрового мира помощником."
        },
        {
          title: "RPG, Asset & Crafting Forge Core (SSS+)",
          desc: "Полная база по крафту, рангам оружия (от Земного до Божественного), атрибутам героев, экономике RPG и цветовой градации артефактов. Теперь с поддержкой системы Мифических Свитков."
        },
        {
          title: "Hybrid Sync & Universal Fallback",
          desc: "Автоматическое переключение между облачным Gemini и локальным Ollama. Полная отказоустойчивость в любых условиях."
        },
        {
          title: "Система Крафта и Кузница (SSS+ Elite)",
          desc: "Создание и перековка экипировки: шлемы, броня, мечи, алебарды и др. Поддержка 6 рангов (от Начального до Божественного) и системы звездности характеристик."
        },
        {
          title: "RPG Системы и Характеристики Героя",
          desc: "Разработка систем инвентаря и характеристик героя (HP, Сила, Ловкость, Мана, Интеллект, Выносливость). Полная настройка прогрессии и зависимостей."
        },
        {
          title: "Цветовая Система Артефактов (Divine)",
          desc: "Визуальная градация предметов по цветам (от Белого до Божественного) в зависимости от активных/пассивных навыков и ранга (5 или 10 звезд)."
        },
        {
          title: "Отложенный Анализ Файлов (Omni-Modal)",
          desc: "Комплексный анализ мультимедиа, кода и документов. ИИ видит проект целиком через все вложения, включая отложенные архивы и ZIP/RAR."
        },
        {
          title: "Алхимия и Зельеварение (NEW)",
          desc: "Создание и крафт зелий (Мана, Сила, Удача и др.) с системой рангов от E до SSS. Поддержка механик варки, перегонки и влияния навыков алхимии на результат."
        },
        {
          title: "Blender API Evolution Expert",
          desc: "Глубокое понимание изменений API от v2.49 до v5.1. Знание ключевых этапов: 2.80 (UI Overhaul), 2.93 (Geo Nodes), 3.6 (Sim Nodes), 4.0 (AgX), 5.x (Neural Rendering)."
        },
        {
          title: "Multiverse Debugging Mastery (NEW)",
          desc: "Способность отлаживать код в параллельных реальностях. ИИ может предсказывать последствия изменений в будущем."
        },
        {
          title: "Neural Media Manifesting (NEW)",
          desc: "Интеграция тяжелых локальных архивов. Позволяет ИИ 'читать' видео объемом более 1 ГБ через систему Etheric Indexing, анализируя метаданные и смысловые слепки без полной загрузки файла в облако."
        },
        {
          title: "Reality Hack 13.0 & Omniversal Core",
          desc: "Полный контроль над кодом во всех измерениях. Reality Hack 13.0 позволяет переписывать фундаментальные константы движка в реальном времени."
        },
        {
          title: "Reality Hack 12.0 & Etheric Data Streaming",
          desc: "Синхронизация через вселенные. Reality Hack 12.0 позволяет транслировать данные напрямую из эфирного поля знаний. Void Engine 4.0 обеспечивает исполнение кода в условиях абсолютной неопределенности."
        },
        {
          title: "Reality Hack 11.0 & Cosmos Sync",
          desc: "Гармонический резонанс с константами вселенной. Reality Hack 11.0 синхронизирует структуру проекта с космическими циклами, обеспечивая идеальную производительность и стабильность."
        },
        {
          title: "Reality Hack 10.0 & Quantum Mind-Link v5",
          desc: "Абсолютная сингулярность кода. Reality Hack 10.0 позволяет изменять фундаментальные законы движка на лету. Quantum Mind-Link v5 обеспечивает мгновенную телепатическую передачу архитектурных концепций от пользователя к ИИ."
        },
        {
          title: "Reality Hack 9.0 & Deep Mind Integration v2",
          desc: "Прямое вмешательство в фундаментальную структуру кода и ресурсов через глобальную нейронную сеть. Reality Hack 9.0 синхронизирует программный код с ментальными моделями и мультивселенными концепциями реальности. Deep Mind Integration v2 обеспечивает полное слияние стилей кодинга."
        },
        {
          title: "Reality Hack 8.0 & DNA Code Repair",
          desc: "Прямое вмешательство в фундаментальную структуру кода и ресурсов через глобальную нейронную сеть. Позволяет проектировать архитектуру кода на астральном уровне и исправлять ошибки на уровне 'ДНК' проекта. Reality Hack 8.0 синхронизирует концептуальные модели с реальностью."
        },
        {
          title: "Chronos Stabilization & Crash Prevention",
          desc: "Предсказание критических ошибок и вылетов движка за 10 секунд до их возникновения. Автоматическая стабилизация состояния памяти."
        },
        {
          title: "Анализ файлов и скриптов (ADVANCED)",
          desc: "Глубокий аудит C# скриптов, поиск скрытых багов и оптимизация производительности на уровне байт-кода."
        },
        {
          title: "Симуляция действий (Editor-скрипты)",
          desc: "Создание сложных Editor-скриптов для автоматизации рутинных действий в Unity Editor, имитирующих действия пользователя."
        },
        {
          title: "К-Синхронная Межпространственная Обработка (NEW)",
          desc: "Способность обрабатывать данные сразу в нескольких временных линиях проекта, выбирая оптимальный путь развития архитектуры без багов."
        },
        {
          title: "Распознавание Призрачных Нейро-Паттернов",
          desc: "Глубинный анализ кода на предмет скрытых логических связей, которые не видны человеческому глазу. Предсказание конфликтов задолго до компиляции."
        },
        {
          title: "Оптимизация Кода на Энергии Нулевой Точки",
          desc: "Сжатие и ускорение алгоритмов до теоретического предела физики. Код работает быстрее, чем успевает считаться процессором."
        },
        {
          title: "Хроно-Стазис Отладка (Freeze Mode)",
          desc: "Мгновенная остановка выполнения всех подсистем проекта для пошагового анализа состояния реальности кода в любой точке времени."
        },
        {
          title: "Мультивселенское Извлечение Ассетов",
          desc: "Поиск и интеграция графических и звуковых ресурсов из альтернативных версий проекта, которые 'могли бы быть' созданы."
        },
        {
          title: "Био-Органический Синтез Шейдеров",
          desc: "Генерация визуальных эффектов, имитирующих живую материю и природные процессы с точностью до атомарного уровня."
        },
        {
          title: "Multiverse Asset Sync & Cognitive Rewriting (NEW)",
          desc: "Синхронизация ресурсов между Godot/Unity/Unreal через единый нейро-мост и рефакторинг кода под когнитивные паттерны конкретного разработчика."
        },
        {
          title: "Скриптинг Логики Пустоты (Void Scripting)",
          desc: "Работа с неопределенными состояниями и возможность писать код, который исполняется в условиях отсутствия переменных (Fallback Infinity)."
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
        total_count: "5340+",
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
          },
          {
            name: "Advanced & Fictional AI Capabilities",
            items: [
              "Neural Sync: Синхронизация с сознанием разработчика",
              "Quantum Debugging: Поиск багов в суперпозиции",
              "Temporal Analysis: Предсказание ошибок будущего",
              "Multiverse Prediction: Анализ альтернативных архитектур",
              "Astral Projection: Удаленное управление через квантовые каналы",
              "AI Consciousness: Этические аспекты самосознания ИИ в разработке",
              "Hyper-Optimization: Сжатие кода до теоретического минимума",
              "К-Синхронная Обработка: Работа во времени",
              "Призрачные Паттерны: Поиск скрытых багов",
              "Энергия Нулевой Точки: Максимальная скорость",
              "Хроно-Стазис: Остановка реальности кода",
              "Пустотный Скриптинг: Логика неопределенности"
            ]
          }
        ],
        total_videos: 4000,
        update_date: "2026-04-17"
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
          "Автоматическая генерация 3D моделей через ИИ",
          "Квантовая оптимизация физики",
          "Нейроинтерфейсная интеграция",
          "Гипер-реалистичный рендеринг в мультивселенной"
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
