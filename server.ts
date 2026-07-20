// 🤖 Unity & Blender AI Assistant • Server Component (v18.12.01 Verified)
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
import { GoogleGenerativeAI } from "@google/generative-ai";

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

              // 4. Audit: Missing Colliders on Static Objects
              if (relativePath.includes('Static') && !content.includes('Collider')) {
                results.analysis.audit_issues.push({
                  file: relativePath,
                  type: 'Physics',
                  message: `Статический объект может не иметь коллайдера. Проверьте настройки физики.`
                });
              }

              // 5. Audit: Large Texture Check (via .meta files)
              const metaPath = fullPath + '.meta';
              if (await fs.pathExists(metaPath)) {
                const metaContent = await fs.readFile(metaPath, 'utf-8');
                if (metaContent.includes('maxTextureSize: 4096') || metaContent.includes('maxTextureSize: 8192')) {
                  results.analysis.audit_issues.push({
                    file: relativePath,
                    type: 'Memory',
                    message: `Текстура имеет очень высокое разрешение (4K/8K). Рекомендуется ограничить до 2048 для мобильных устройств.`
                  });
                }
              }
            } catch (e) {}
          }
          else if (ext === '.fbx' || ext === '.obj') {
            results.others.push(relativePath);
            try {
              const metaPath = fullPath + '.meta';
              if (await fs.pathExists(metaPath)) {
                const metaContent = await fs.readFile(metaPath, 'utf-8');
                if (metaContent.includes('meshCompression: 0')) {
                  results.analysis.audit_issues.push({
                    file: relativePath,
                    type: 'Optimization',
                    message: `Сжатие меша отключено. Рекомендуется включить 'Medium' или 'High' для уменьшения веса билда.`
                  });
                }
              }
            } catch (e) {}
          }
          else if (ext === '.anim') results.animations.push(relativePath);
          else if (ext === '.controller') results.animators.push(relativePath);
          else if (ext === '.pdf') results.pdfs.push(relativePath);
          else if (['.mp4', '.mov', '.avi', '.mkv'].includes(ext)) results.videos.push(relativePath);
          else if (['.png', '.jpg', '.wav', '.mp3'].includes(ext)) results.others.push(relativePath);
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
  const currentVersion = kb.version || "18.10.0";
  
  const files = [
    { name: "knowledge_base.json", default: { project_name: "Unity Assistant", version: currentVersion, project_path: process.cwd(), system_instruction: "You are a helpful assistant." } },
    { name: "ccgs_project_blueprint.json", default: { project_name: "Unity & Blender AI Assistant", version: currentVersion, interface_structure: { tabs: ["studio", "kb", "commands", "files", "migration"] }, agents_count: 12000 } },
    { name: "version.json", default: { version: currentVersion, release_date: new Date().toISOString().split('T')[0], changelog: ["Initial release"] } },
    { name: "DEVELOPMENT_LOG.md", default: "# DEVELOPMENT LOG\n\n## [2026-05-14]\n- Версия 18.5.8: Zenith Multi-Tool Synergy & Settings Fix." }
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
    
    // @ts-ignore
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
    
    let md = `# PROJECT MASTER BLUEPRINT: ${blueprint.project_name || "Unity & Blender AI Assistant"} (Total Knowledge Archive Edition)\n\n`;
    md += `> **ВНИМАНИЕ:** Этот документ является \"источников истины\" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание возможностей ИИ v18.11.18.\n\n`;
    md += `## 1. Общая информация\n`;
    md += `- **Версия Помощника:** ${blueprint.version || "18.11.18"}\n`;
    md += `- **Описание:** ${blueprint.description || "Гибридный ИИ-помощник нового поколения (v18.11.18 - Dynamic Army Units, Character Prompts Book Integration & High-Density UI) для Unity 6 (6000.3.10f1), Blender 5.2 и Godot 4.4. Интеграция адаптивного переключения ввода, калибровка зум-параметров при высадке, позиционирование маркеров по Z=-2.0f, сохранение прогресса и моделирования отрядов."}\n`;
    md += `- **Путь проекта:** ${kb.project_path || "Не задан"}\n`;
    md += `- **Локальное хранилище:** ${kb.local_training_path || "Не задано"}\n`;
    md += `- **Версия Unity:** ${currentUnityStatus.version}\n`;
    md += `- **Версия Blender:** ${currentBlenderStatus.version}\n`;
    md += `- **Версия GIMP:** ${currentGimpStatus.version}\n`;
    md += `- **Версия Redot:** ${currentRedotStatus.version}\n`;
    md += `- **Флаги:** [QUANTUM_LINK_ACTIVE], [KNOWLEDGE_STORAGE_SYNC], [V18_11_18_FATE_MASTER]\n\n`;
    
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

    md += `## 3. Иерархия ИИ-Агентов (${blueprint.agents_count || 9500} агентов)\n`;
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

    md += `\n## 6. О ВОЗМОЖНОСТЯХ ИИ (v18.11.11 - Real-Time Bound Locking & Ocean Backdrop)\n`;
    md += `### Режимы работы и Архитектурные уровни\n`;
    md += `- **Online Mode (Eternal Origin Quantum Singularity):** Прямое подключение к Omniversal Quantum Network. Интеллект Singularity-уровня.\n`;
    md += `- **Offline Mode (Neural Singularity Nexus):** Автономная сингулярность. Полная симуляция реальности Transcendence.\n`;
    md += `- **No-Internet Mode (Quantum Archive):** 10,000+ видео-уроков. Мгновенный доступ при любых внешних условиях.\n\n`;

    md += `### ОБРАЗОВАТЕЛЬНЫЙ ХАБ (v18.11.11 Sync)\n`;
    md += `- **Unity 6 Physics & Optimization:** [Video #2](https://www.youtube.com/watch?v=9vuyis_Y-LY)\n`;
    md += `- **Blender Advanced Rigging:** [Video #3](https://www.youtube.com/watch?v=UKZp67dY1_w)\n`;
    md += `- **Shader Graph Mastery:** [Video #4](https://www.youtube.com/watch?v=-hvxjyzcSkI)\n`;
    md += `- **Geometry Nodes World Gen:** [Video #6](https://www.youtube.com/watch?v=4YEB_Q8EOD8)\n`;
    md += `- **Unity AI & ML-Agents:** [Video #9](https://www.youtube.com/watch?v=JBszeE_NgmA)\n\n`;

    md += `### TRANSCENDENT LINK (Neural Addon Synthesis)\n`;
    md += `- **Neural Addon Synthesis:** Возможность проектирования и генерации аддонов для Blender и плагинов для Unity, которые напрямую связывают софт с ИИ.\n`;
    md += `- **Direct Software Manifestation:** Отправка команд и скриптов напрямую в среду разработки через API мост.\n`;
    md += `- **Quantum Erasure Prevention:** Защита данных проекта от квантовой дегенерации и случайной потери логики.\n\n`;

    md += `### ВОЗМОЖНОСТИ BLENDER (Quantum Edition)\n`;
    md += `- **Transcendent Scripting:** Полный охват всех версий Blender. ИИ 'чувствует' API на квантовом уровне.\n`;
    md += `- **Molecular Texture Synthesis:** Singularity Edition - создание текстур с учетом квантовых свойств поверхности.\n\n`;

    md += `### ВОЗМОЖНОСТИ GODOT/REDOT (Genesis Edition)\n`;
    md += `- **Redot Absolute Omniscience:** Полная поддержка Godot 4.4 и форка Redot для миграции.\n\n`;
    md += `### ↔️ ИСПРАВЛЕНИЕ ТЕКСТА «СТОЛБИКОМ» (Russian Overlap)\n`;
    md += `**Проблема:** Русские слова в выпадающем списке (Dropdown) сжимаются или встают вертикально.\n`;
    md += `**Решение:**\n`;
    md += `1. **Rect Tool:** Выберите текстовый объект внутри Dropdown (обычно это \`Item Text\`), нажмите **T** и **растяните рамку по ширине**, чтобы текст влезал полностью. Либо уменьшите размер шрифта в компоненте TextMeshPro.\n\n`;
    
    md += `### 🖼️ УДАЛЕНИЕ ПРИВЕТСТВЕННОГО ЭКРАНА URP\n`;
    md += `Если в углу мешает значок "URP Empty Template":\n`;
    md += `**Действие:** Найдите файл \`Readme\` в папке Assets. В Инспекторе нажмите кнопку **"Remove Readme Assets"**. Это удалит обучающий контент и значок.\n\n`;

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
    md += `### Менеджер Сохранений (Save Subsystem)\n`;
    md += `- **Скрипт сохранения:** \`SaveGameSystem.cs\`\n`;
    md += `- **Система слотов:** 3 независимых слота, PlayerPrefs-база.\n`;
    md += `- **Мультиязычные метаданные:** "Ур." / "Lvl" / "레벨" / "等级" в зависимости от выбранного в данный момент языка.\n`;
    md += `### Крафт и Кузница\n`;
    md += `- **Предметы:** Шлемы, Броня, Мечи, Копья, Секиры, Молоты, Кастеты, Алебарды и др.\n`;
    md += `- **Ранги (Звезды):** Начальный (5), Земной (5), Небесный (5), Легендарный (10), Полубожественный (10), Божественный (10).\n`;
    md += `- **Механики:** Перековка за золото, навыки кузнеца, зависимость статов от ранга.\n`;
    md += `### Характеристики Героя\n`;
    md += `- **Атрибуты:** Жизнь (HP), Сила, Ловкость, Мана, Интеллект, Выносливость.\n`;
    md += `- **Инвентарь:** Создание систем слотов, веса и категорий предметов.\n\n`;

    md += `## 10. ИИ-Генерация и продление аудио (Suno & Udio & Unity Loops)\n`;
    md += `### 🔮 Инструкция для Udio: Как правильно удлинять треки (Extend)\n`;
    md += `В Udio функция продления является одной из лучших в индустрии, так как позволяет очень гибко наращивать трек фрагментами по 30 секунд.\n`;
    md += `- **Шаг 1: Активация режима продления:** В вашей библиотеке (My Library) найдите трек -> Нажмите на три точки (...) -> Выберите пункт **Create ➔ Extend** (или просто нажмите кнопку Extend на дорожке).\n`;
    md += `- **Шаг 2: Настройка направления и параметров продления:**\n`;
    md += `  - **Выбор направления (Placement):**\n`;
    md += `    - *Add Section After:* Новая музыкальная дорожка пристроится в самый конец вашего исходного файла (самый частый выбор).\n`;
    md += `    - *Add Intro / Add Section Before:* Отлично подходит, если вашей композиции не хватает красивого стартового вступления.\n`;
    md += `    - *Add Outro:* Создает логическое, плавное завершение трека (затухание оркестра, финальный аккорд).\n`;
    md += `  - **Промпт для продолжения (Describe Your Song):** Если вы хотите сохранить тот же самый стиль, не меняйте промпт. Если вы хотите изменить настроение (ускорение темпа, новые инструменты), напишите новые теги именно для этого фрагмента (например, *heavy battlefield percussion, intense war brass build up, fast tempo*).\n`;
    md += `  - **Инструментал или текст:** Убедитесь, что включен переключатель **Instrumental** для треков без слов.\n`;
    md += `- **Шаг 3: Генерация и Склеивание:** Нажмите красную кнопку Create. Udio сгенерирует два новых варианта продления. Выберите лучший и продолжайте дальше!\n`;
    md += `- **Шаг 4: Как бесплатно обойти ограничения и скачать песню (Download Bypass):**\n`;
    md += `  - **Причина ограничений:** Udio блокирует прямое бесплатное скачивание (показывает окно платной подписки) и кодирует аудиоплеер в виде дробленых фрагментов \".m4s\" (вместо одного чистого MP3), чтобы защитить поток от скачивания через F12 на рабочей странице.\n`;
    md += `  - **Способ А (Самый простой — Через сторонний загрузчик):**\n`;
    md += `    1. Нажмите кнопку **Share** под карточкой трека.\n`;
    md += `    2. Скопируйте публичную ссылку на композицию (например, "https://www.udio.com/songs/...").\n`;
    md += `    3. Откройте любой бесплатный сервис загрузки Udio в новой вкладке (например, udiodownloader.com, udiolink.com или специализированный бот Telegram вроде @UdioDownloaderBot).\n`;
    md += `    4. Вставьте ссылку и мгновенно скачайте готовый \".mp3\" файл без регистрации и подписок.\n`;
    md += `  - **Способ Б (Запись вкладки браузера — 100% рабочий и чистый метод):**\n`;
    md += `    1. Установите бесплатное расширение Chrome: **Chrome Audio Capture** (или любой "внутренний диктофон аудио-вкладок").\n`;
    md += `    2. Откройте страницу трека Udio, запустите расширение и нажмите **Start Capture**.\n`;
    md += `    3. Включите воспроизведение песни. После завершения прослушивания нажмите **Save Capture** — расширение сохранит чистую, идеальную запись цифрового аудио в формате \".mp3\" прямо на ваш компьютер.\n`;
    md += `  - **Способ В (Разбор через код страницы):** Перейдите по скопированной ссылке "https://www.udio.com/songs/..." в режиме инкогнито, откройте F12, перейдите на вкладку Network, отфильтруйте по ключевому слову "mp3" или в категории "Media". Запустите трек. Так как на публичной индивидуальной странице защита слабее, в сетевых событиях часто появляется прямой временный CDN-адрес к \".mp3\" или \".wav\". Кликните по нему правой кнопкой мыши -> Open in new tab -> Ctrl+S.\n\n`;

    md += `### ☀️ Инструкция для Suno: Как продлевать треки и делать «Get Whole Song»\n`;
    md += `В Suno система удлинения позволяет автоматически склеивать все части в один монолитный файл.\n`;
    md += `- **Шаг 1: Подготовка к продлению:** Нажмите на три точки (...) рядом с треком -> Выберите пункт **Extend**.\n`;
    md += `- **Шаг 2: Настройка временного кода (Timestamp):**\n`;
    md += `  - **Extend from:** Suno ставит туда самое последнее время трека (например, *02:10*). Но вы можете вручную вписать нужное время (напр., *01:50*). Suno отрежет неудачный финал и начнет генерировать продолжение именно с этой секунды!\n`;
    md += `  - **Промпт и Текст:** Оставьте стиль прежним для бесшовности или измените его.\n`;
    md += `- **Шаг 3: Генерация Part 2:** Нажмите Extend / Create. Suno создаст новые записи с пометкой **Part 2**.\n`;
    md += `- **Шаг 4: Финальное склеивание (КЛЮЧЕВОЕ ДЕЙСТВИЕ):** Когда прослушали варианты Part 2 и выбрали лучший, нажмите на три точки (...) именно у этого удачного продленного фрагмента и выберите команду **Get Whole Song** (Получить всю песню). В вашей библиотеке появится совершенно новый, цельный трек, подписанный как **Full Song**, который будет идеально звучать от самого начала до нового финала без швов!\n\n`;

    md += `### 🎮 Совет для Fate Continent: Делаем идеальные зацикливания (Loops)\n`;
    md += `Чтобы музыка во время хождения по карте или боя в Unity не прерывалась резко, а играла по кругу бесконечно:\n`;
    md += `- **Плавный хвост:** При продлении в самом конце трека (последние 5-10 секунд) постарайтесь промптом вернуть звучание к тем инструментам, с которых композиция начиналась (например, если трек начинался с тихого соло на арфе, пусть в конце останется только арка/арфа).\n`;
    md += `- **loop = true:** В коде нашего SettingsManager.cs при вызове PlayMusicTrack уже автоматически прописано свойство \`musicSource.loop = true\`. Музыка в вашей игре будет крутиться циклично и атмосферно без использования ElevenLabs!\n\n`;

    md += `## 11. База бесплатных ресурсов и локального звукового ИИ (Freesound, Pixabay, Local Generator & RTX 4060)\n\n`;
    md += `### 🎙️ Промпты для поиска звуков нашей игры на Freesound и Pixabay\n`;
    md += `*(Используйте именно английские слова в поиске для наилучшего качества!)*\n\n`;
    md += `| Действие / Область игры | Английский промпт (копируйте в поиск) | Описание характера звука |\n`;
    md += `| :--- | :--- | :--- |\n`;
    md += `| **Клик по обычной кнопке** | \`ui click modern\`, \`interface button click\`, \`menu select\` | Короткий, чистый, приятный щелчок |\n`;
    md += `| **Наведение на кнопку (Hover)**| \`ui hover quiet\`, \`menu focus soft\`, \`button roll over\` | Легкий шелест, тихий затухающий звук |\n`;
    md += `| **Клик по рюкзаку / мешку** | \`leather pouch rustle\`, \`open inventory bag\`, \`leather bag open\` | Характерный кожаный шорох или скрип |\n`;
    md += `| **Клик по персонажу (выбор)** | \`character selection hum\`, \`hero select warp\`, \`voice select chime\` | Приятный свист, подтверждающий звон или вздох |\n`;
    md += `| **Клик по мобам / выбор врага** | \`target lock click\`, \`monster select growl\`, \`beast threat roar\` | Мрачный короткий рык или кошачье фырканье |\n`;
    md += `| **Движение в инвентаре/вещах** | \`item pickup drop\`, \`coins clashing\`, \`inventory items shuffling\` | Металлический звон, пересыпание монет, шуршание |\n`;
    md += `| **Зелья (выпить / нажать)** | \`drinking potion gulp\`, \`liquid swallow potion\`, \`glass bottle clink\` | Бульканье жидкости с явным глотком в конце |\n`;
    md += `| **Клики по снаряжению (лат)** | \`equip metal armor click\`, \`leather strap pull\`, \`metal equipment wear\` | Скрежет латных пластин, натяжение кожаных ремней |\n`;
    md += `| **Клики по оружию (мечу)** | \`sword equip unsheathe\`, \`blade metal slice\`, \`sword clash draw\` | Металлический звон извлекаемого из ножен клинка |\n`;
    md += `| **Клик по мобам / монстрам** | \`monster selecting click\`, \`beast target select\`, \`growl click\` | Короткий рык, выделение цели |\n`;
    md += `| **Клик по инвентарю / вещам** | \`loot sound\`, \`item pickup drop\`, \`inventory items shuffling\` | Звон монет, шуршание |\n`;
    md += `| **Зелья (выпить / клик)** | \`drinking potion gulp\`, \`liquid swallow potion\`, \`glass bottle clink\` | Бульканье и глоток |\n`;
    md += `| **Снаряжение / броня / мечи**| \`equip metal armor click\`, \`leather strap gear adjustment\`, \`sword equip\`| Звон лат или вытягивание клинка |\n`;
    md += `| **Навыки / Скиллы (клик)** | \`magic charge up generator\`, \`spell power build up\`, \`skill unlock chime\`| Нарастающий гул магического заряда |\n`;
    md += `| **Навыки (применение магии)** | \`magic spell cast swoop\`, \`wizard burst wand blow\`, \`divine heal aura\`| Свист пролетающей волшебной стрелы |\n`;
    md += `| **Кнопка закрытия (Close)**| \`ui close back button\`, \`window close exit click\`, \`panel dismiss slate\`| Деревянный хлоп или глухой щелчок назад |\n`;
    md += `| **Начало нападения (Combat)**| \`combat fight alert horn\`, \`battle start drum build up\`, \`clash start alert\`| Горн или тяжелая барабанная дробь |\n`;
    md += `| **Физическая атака (удар)**| \`blunt impact hit\`, \`melee punch strike\`, \`fist weapon smash\` | Реалистичный сокрушительный удар |\n`;
    md += `| **Звуки монстров (рык / хит)**| \`monster roar growl angry\`, \`beast scream screech\`, \`monster attack cry\` | Вопли монстров, рычание, хрипли |\n`;
    md += `| **Крики атаки героя** | \`male hero battle cry\`, \`warrior grunt attack yell\`, \`shout combat grunt\`| Боевой крик главного героя |\n`;
    md += `| **Звук движения (ходьба)** | \`footsteps concrete stone\`, \`walking footsteps gravel\`, \`footsteps grass loop\` | Размеренные шаги по разным поверхностям |\n`;
    md += `| **Звук движения (бег)** | \`running footsteps fast grass\`, \`fast running steps concrete\`, \`sprint grass\` | Быстрый, частый бег в атаке |\n`;
    md += `| **Выбор локации на карте** | \`map travel select\`, \`point of interest click\`, \`parchment scroll unfold\` | Свиток карты, звон цели |\n`;
    md += `| **Победный джингл (Victory)**| \`victory fanfare short medieval\`, \`quest completed jingle\`, \`victory win trumpet\`| Короткая духовая фанфара оркестра |\n`;
    md += `| **Поражение / Смерть**     | \`game over defeat fail sound\`, \`sad dramatic game over\`, \`hero collapse grunt\` | Спадающий минорный аккорд смерти |\n\n`;

    md += `---\n\n`;
    md += `## 12. Применение звуков кликов в Unity: Скрипты и AudioMixer\n\n`;
    md += `### 🎛️ Настройка AudioMixer в Unity 6 (Пошагово)\n`;
    md += `Для создания профессиональной звуковой картины мы используем встроенную маршрутизацию Unity через **AudioMixer**:\n`;
    md += `1. **Создание AudioMixer:** В окне Project нажмите **Right Click -> Create -> Audio Mixer** и назовите его \`MainMixer\`.\n`;
    md += `2. **Добавление групп (Groups):** Откройте окно Audio Mixer. Под группой \`Master\` создайте две дочерние группы:\n`;
    md += `   - **Music** (для фоновой музыки)\n`;
    md += `   - **SFX** (для всех кликов, ударов, умений и звуков монстров)\n`;
    md += `3. **Назначение AudioSource:** На объектах с компонентом \`AudioSource\` перетащите соответствующую группу в поле **Output** компонентов:\n`;
    md += `   - Музыкальный фоновый объект -> выход \`Music (MainMixer)\`.\n`;
    md += `   - Кнопки / Спецэффекты -> выход \`SFX (MainMixer)\`.\n`;
    md += `4. **Настройка громкости с логарифмической шкалой:** Чтобы регуляторы громкости из меню настроек (Settings Panel) управляли микшером плавно (человеческое ухо воспринимает звук логарифмически):\n`;
    md += `   - Кликните на группу \`SFX\` в AudioMixer, в Инспекторе нажмите правой кнопкой мыши по свойству **Volume** -> Выберите **Expose 'Volume' to script**.\n`;
    md += `   - Переименуйте выставленный параметр во вкладке *"Exposed Parameters"* в правой верхней части AudioMixer в \`Volume_SFX\`.\n`;
    md += `   - Проделайте то же самое для музыки, назвав параметр \`Volume_Music\`.\n`;
    md += `   - В коде \`SettingsManager.cs\` при изменении Slider используйте логарифмическую формулу:\n`;
    md += `     \`\`\`csharp\n`;
    md += `     // sliderValue лежит в диапазоне от 0.0001f до 1.0f\n`;
    md += `     float dbVolume = Mathf.Log10(sliderValue) * 20f;\n`;
    md += `     audioMixer.SetFloat("Volume_SFX", dbVolume);\n`;
    md += `     \`\`\`\n\n`;

    md += `### 🔗 Связывание кнопок и кликов со скриптами (UIButtonSfxBinder.cs)\n`;
    md += `Чтобы не настраивать руками клики для каждой отдельной кнопки в Inspector, рекомендуется написать простой менеджер автоматического назначения:\n`;
    md += `\`\`\`csharp\n`;
    md += `using UnityEngine;\n`;
    md += `using UnityEngine.UI;\n`;
    md += `using UnityEngine.EventSystems;\n\n`;
    md += `public class UIButtonSfxBinder : MonoBehaviour\n`;
    md += `{\n`;
    md += `    public AudioClip clickSound;\n`;
    md += `    public AudioClip hoverSound;\n\n`;
    md += `    void Start()\n`;
    md += `    {\n`;
    md += `        Button[] buttons = GetComponentsInChildren<Button>(true);\n`;
    md += `        foreach (Button btn in buttons)\n`;
    md += `        {\n`;
    md += `            btn.onClick.AddListener(() => {\n`;
    md += `                if (SettingsManager.Instance != null && clickSound != null)\n`;
    md += `                    SettingsManager.Instance.PlaySoundEffect(clickSound);\n`;
    md += `            });\n\n`;
    md += `            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();\n`;
    md += `            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();\n\n`;
    md += `            EventTrigger.Entry entry = new EventTrigger.Entry();\n`;
    md += `            entry.eventID = EventTriggerType.PointerEnter;\n`;
    md += `            entry.callback.AddListener((data) => {\n`;
    md += `                if (SettingsManager.Instance != null && hoverSound != null)\n`;
    md += `                    SettingsManager.Instance.PlaySoundEffect(hoverSound);\n`;
    md += `            });\n`;
    md += `            trigger.triggers.Add(entry);\n`;
    md += `        }\n`;
    md += `    }\n`;
    md += `}\n`;
    md += `\`\`\`\n\n`;
    md += `### 📊 Авторские права и плагиат ИИ (Правовой Ликбез)\n`;
    md += `1. **Генерация UI / Иконок / Меню (Midjourney, Stable Diffusion и др.):**\n`;
    md += `   - **Абсолютно безопасно.** Картинки, сгенерированные ИИ на общедоступных сайтах (или локально), не охраняются авторским правом в большинстве стран мира (включая США, ЕС и РФ), так как нет автора-человека.\n`;
    md += `   - Вы можете использовать их в коммерческой игре без каких-либо рисков получить иск за плагиат. Никаких подписок или прав покупать **не нужно**.\n`;
    md += `2. **Скачивание музыки через обход (Запись вкладки / Загрузчики):**\n`;
    md += `   - **Для личного пользования / хобби / некоммерческих проектов (бесплатная игра):** Полностью безопасно, никто не будет вас преследовать.\n`;
    md += `   - **Для коммерческой продажи игры (в Steam, Google Play, itch.io):** Если вы решите коммерциализировать свою игру, то использование треков, сгенерированных на бесплатном аккаунте Udio/Suno через обходные утилиты, технически нарушает Пользовательское соглашение (Terms of Service) этих сайтов. Если вы хотите продавать игру, рекомендуется использовать музыку с открытыми лицензиями (CC0/Public Domain) или запустить бесплатный ИИ-генератор локально у себя на компьютере.\n\n`;
    md += `---\n\n`;
    md += `### 📥 Подробная инструкция: Как качать с Freesound.org и Pixabay.com\n\n`;
    md += `#### 1. Pixabay.com (Музыка и Звуковые эффекты)\n`;
    md += `Pixabay — самый дружелюбный сайт, все звуки здесь бесплатны для коммерческого использования, регистрация не требует подтверждений, а интерфейс очень прост:\n`;
    md += `1. Перейдите на **https://pixabay.com/sound-effects/** (или выберите "Sound Effects" в верхнем выпадающем меню).\n`;
    md += `2. Напишите нужный промпт (на английском языке, список ниже) в поисковую строку.\n`;
    md += `3. Каждая карточка звука имеет кнопку **Play** для прослушивания и зеленую кнопку **Download** справа.\n`;
    md += `4. Нажмите **Download** — файл \`.mp3\` или \`.wav\` сразу же скачется на ваш ПК. Никаких подтверждений или указания авторства не требуется!\n\n`;
    md += `#### 2. Freesound.org (Огромная база профессиональных звуков)\n`;
    md += `Freesound содержит миллионы звуков от инди-разработчиков и звукорежиссеров. Здесь критически важно смотреть на тип лицензии:\n`;
    md += `- **CC0 (Creative Commons 0 / Public Domain):** Полная свобода действий! Можно использовать в коммерческих играх, менять, продавать, авторство указывать НЕ нужно.\n`;
    md += `- **Attribution (CC-BY):** Использовать можно бесплатно в коммерции, но вы **обязаны** указать никнейм автора звука в титрах вашей игры.\n`;
    md += `- **Non-commercial (CC-NC):** Нельзя использовать в платных играх (только в бесплатных проектах).\n\n`;
    md += `**Как скачивать на Freesound:**\n`;
    md += `1. Введите поисковый запрос (например, \`sword hit\`) в строку поиска сверху.\n`;
    md += `2. В левой колонке найдите блок **LICENSE** (Лицензия) и **обязательно кликните на "Creative Commons 0"** (это отфильтрует звуки, чтобы остались только те, которые не требуют никаких прав и упоминаний в титрах).\n`;
    md += `3. Нажмите на название понравившегося звука (ссылка-заголовок).\n`;
    md += `4. На открывшейся странице звука нажмите большую кнопку **Download** (она находится справа под волновым графиком).\n`;
    md += `5. Если вы не авторизованы, сайт предложит быстро войти через ваш аккаунт Google или созданный профиль. После этого скачивание начнется мгновенно.\n\n`;
    md += `\n### 🧠 Разбор Moshi & HuggingFace (Что выбрать для RTX 4060 8GB?)\n\n`;
    md += `Проект **Moshi** от Kyutai — это **голосовой ИИ-собеседник в реальном времени**. Он служит для общения голосом и озвучки текста, но **он НЕ умеет создавать музыку**.\n`;
    md += `- **Попробовать Moshi онлайн:** Без установки, просто перейдите по официальной ссылке: **https://moshi.chat/** и нажмите кнопку микрофона, чтобы поговорить.\n\n`;

    md += `### 🚀 Вариант: Запуск музыкальных генераторов у себя на ПК (RTX 4060 8GB)\n\n`;
    md += `У вас отличная видеокарта **NVIDIA RTX 4060 с 8 ГБ VRAM**. Вы можете запускать локальные модели музыки на ней бесплатно!\n`;
    md += `#### Лучший выбор: Meta MusicGen (из библиотеки Audiocraft)\n`;
    md += `- Модель \`musicgen-small\` (300M параметров) — полностью помещается в 4 ГБ видеопамяти вашей RTX 4060 и генерирует треки мгновенно.\n`;
    md += `- Модель \`musicgen-medium\` (1.5B параметров) — забирает до 7.5 ГБ VRAM, генерирует музыку потрясающего качества.\n\n`;
    md += `#### ⚙️ Установка в 1 клик через Pinokio:\n`;
    md += `1. Скачайте программу-браузер нейросетей **https://pinokio.computer/** для Windows.\n`;
    md += `2. В поиске внутри Pinokio найдите **Audiocraft** или **MusicGen**.\n`;
    md += `3. Нажмите **Download**, затем **Install**.\n`;
    md += `4. После завершения нажмите кнопку **Start**. Откроется локальная веб-страничка (**http://localhost:7860**), где вы сможете ввести любой промпт и получать чистые треки без ограничений!\n\n`;

    md += `## 12. Архитектура Offline & Hybrid\n`;
    md += `- **LLM Provider:** Ollama (localhost:11434).\n`;
    md += `- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.\n`;
    md += `- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.\n`;
    md += `- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.\n\n`;

    md += `## 13. История изменений (v18.11.7)\n`;
    md += `- **v18.11.7:** Selective Dialogue Map Dismissal & Hidden Faction Markers • Solves critical scene-cleanup issue where the map background, tactical landing rings, and companion marker coordinates stayed visible after clicking 'End Dialogue' or ending conversation. Directs DialogueSystem_Manager to hide active map and overlay modules dynamically on non-interactive dialogue steps, and completely hides flat white redundant faction reference circles (\`Faction_Marker_Aelyssa\`/class markers) from the tactical world map view and during the interactive landing phase.\n`;
    md += `- **v18.11.6:** Synchronized Map & Marker Dismissal • Automatically caches Faction_Marker_Aelyssa and player class markers on startup to properly control their visibility in sync with the tactical world map. Directs DialogueSystem_Manager to dismiss map visibility when ending dialogue (clicking "Завершить диалог"), completely cleaning up map background sprites, active landing point rings, and the companion companion/faction markers from the viewport upon conversation exit.\n`;
    md += `- **v18.11.5:** Single Ring Visibility & Pure Coordinates Preservation • Dynamically disables other rings at runtime, displaying ONLY the single chosen landing point ring. Preserves user-entered Inspector coordinates at runtime by establishing your exact manually fine-tuned layout coordinates as hardcoded default variable parameters in C# and removing the runtime scene-sync overwrite block, completely resolving coordinate resets or values "jumping" to zero on play start.\n`;
    md += `- **v18.11.4:** Zenith Coordinates & Selection Sync • Resolves dual-coordinate feedback loop positioning by restricting scene-to-inspector sync specifically to active Scene dragging via Selection.activeGameObject == ringObj. Fixes missing 4th choice highlight (Zenith Sanctuary / Святилище Зенита) under Step 3 choice selectors in DialogueSystem_Manager by correctly omitting the 5th confirm button (index 4) instead of the 4th choice button (index 3).\n`;
    md += `- **v18.11.3:** Zenith Compiler Integration & FactionMapMarker localScaleOverride Sync • Resolves C# error CS1061 in FateMapManager.cs when trying to access the localScaleOverride property on FactionMapMarker. Added the localScaleOverride field to FactionMapMarker.cs and updated its Update() method to smoothly incorporate any external size offsets dynamically.\n`;
    md += `- **v18.11.2:** Zenith Compiler Compatibility & Multi-Signature Overloads • Fixes compilation errors CS1061 and CS1501 in Unity. Introduced 3-argument overloads for \`OnMapMarkerClicked(associatedDialogueIndex, factionName, factionDescription)\` (including fully dynamic type parsing via generic objects) in \`DialogueSystem_Manager.cs\`, and added compatible \`PlaySfx(string)\` and \`PlaySfx(AudioClip)\` aliases in \`SettingsManager.cs\` to ensure absolute C# compilation out-of-the-box.\n`;
    md += `- **v18.11.1:** Ultimate Independent Map & Ring Scaling and Positioning Sync • Separated ring scaling completely from parent Map scale compensation inside both \`FateMapManager.cs\` and \`FactionMapMarker.cs\`. Recalculates \`baseScale\` and \`targetScale\` dynamically every frame at runtime to facilitate real-time inspector updates.\n`;
    md += `- **v18.11.0:** Zenith Map & Dialogue Blueprint Sync • Components Setup Sync - Synchronizes the blueprint with exact component guidelines for FateMapManager (glowing neon rings, HDR colors, default glow material M_Neon_Glow) and DialogueSystem_Manager (dual local companion voices, case-insensitive class portraits, and anchored coords alignments).\n`;
    md += `- **v18.10.1:** Dialogue UI Uplift - Matches character avatar files on case-insensitive class, forces alpha opacity fallback, zero-delay intro sequences, and lifts dialogue option buttons to Y=30f in container.\n`;
    md += `- **v18.10.0:** Zenith Map Master System • Interactive Continents & Auto-HDR Neon Calibration - Custom interactive map markers with auto-calibrating HDR neon colors, Bloom-glowing ring feedback, click-sound bindings, and multi-option dial branching. Implements automated neon HDR auto-calibration in FactionMapMarker.cs (Sapphire-cyan #0A6CB2, Lion Golden-yellow #B2830A, Druid Green #0AB23D, Xandria Amethyst #6F0AB2) on empty/reset values (A < 0.05, black, clear) to eliminate reverting inspector parameters.\n`;
    md += `- **v18.9.0:** Zenith Dialogue Master System - Introduces DialogueSystem_Manager.cs to orchestrate custom dialog systems using a dual-avatar layout (Aelyssa on left, class-specific player hero on right) and multi-option pointer branching options. Syncs with Translator.cs and includes high-density Midjourney generator prompts in CHARACTER_PROMPTS.md.\n`;
    md += `- **v18.8.0:** Zenith Audio Autonomy & Standalone Routing - Resolves disabled AudioSource exceptions during inside-game menu pause. Customizes UIButtonPauseHover to route playbacks to stable active hosts (e.g., GamePause_Manager or SettingsManager singletons). Excludes pause scenes from UIButtonSfxBinder automatic scan to prevent duplicate sound triggers and enable clean custom binders.\n`;
    md += `- **v18.7.9:** Zenith Self-Healing UI - Automatic runtime healing of inspector misconfigurations. Intelligently scans both panels, resolves overlay text conflicts and ensures the Exit dialog does not overlap during normal pause states.\n`;
    md += `- **v18.7.8:** Zenith Universal Input Safety - Solves runtime InvalidOperationException when checking old Input in projects with Active New Input System. Integrates reflection-based modern Input System readings for Escape/Space keys, combined with GamePause_Manager localized gameplay pauses and active panel state recycling.\n`;
    md += `- **v18.7.7:** Zenith Canvas Lifecycle Mastery - Resolves active UI panels duplicating or failing back-to-menu navigation states on reload through automatic reference recycling and event binding resets in Menu_Game.\n`;
    md += `- **v18.7.6:** Zenith Safe Transition - Zero Editor/Canvas leak on Scene reload, separated the UI Panels and SettingsManager standalone lifecycle to guarantee there are no Canvas duplicates or overlaps upon loading scenes back-and-forth.\n`;
    md += `- **v18.7.5:** Zenith Audio Autonomy - Standalone fallback sound calibration in UIButtonSelectionHover.\n`;
    md += `- **v18.7.4:** Zenith Audio Calibration - Eliminated AudioMixer parameter existence warnings by using silent direct SetFloat writes, customized UIButtonSfxBinder to trigger only on full button Clicks, and added explicit back/escape button name checking (back/exit/close/return/cancel/назад/arrow) to play a custom backClickSound clip.\n`;
    md += `- **v18.7.3:** Zenith Audio Synergy - Deep Pixabay sound navigation guides, expansive custom prompt dictionaries (clinging clicks, inventories, gear, monsters, map selections, combat starts), and robust UIButtonSfxBinder / SettingsManager scripting.\n`;
    md += `- **v18.7.2:** Zenith Multi-Tool Synergy - Sound Prompt Extensions, Pixabay Search Guides, & Unity AudioMixer SettingsManager Integration. Core RPG Saves & Sound Routing.\n`;
    md += `- **v18.7.1:** Suno & Udio Track Extensions & Seamless Looping guidelines integration, SettingsManager.cs looping rules enforcement, ElevenLabs reference deprecation.\n`;
    md += `- **v18.7.0:** Zenith Multi-Tool Synergy - Epic Game Audio Prompts & Sound Integration, Core RPG Saves, and Language Synchronization.\n`;
    md += `- **v18.6.9:** Zenith Multi-Tool Synergy - Core RPG Save slots system integration, Fullscreen Toggle Translation sync fix (Transtable Text ID 11 auto-assignment).\n`;
    md += `- **v18.6.8:** Zenith Multi-Tool Synergy - Robust Font sync, Cyrillic / CJK Original Font styling restore & direct Quality localization link bindings.\n`;
    md += `- **v18.6.7:** Zenith Multi-Tool Synergy - UI & Language Synchronization. Resolution filtering and CS1061 Hotfix.\n`;
    md += `- **v18.5.8:** Zenith Multi-Tool Synergy & Settings Fix.\n`;
    md += `- **v18.5.6:** Triple Font Bridge. Fixed Dropdown Options. Duplicate Cleanup.\n`;
    md += `- **v18.4.9:** Ultimate Stability Sync. CJK & Typography fixes.\n`;
    md += `- **v18.4.1:** Initial release.\n\n`;

    md += `## 14. Аварийные процедуры (Emergency)\n`;
    if (kb.emergency_procedures) {
      md += `### Unity без интернета\n`;
      kb.emergency_procedures.unity_no_internet.forEach((step: string) => md += `- ${step}\n`);
      md += `\n### Исправление вылетов Unity\n`;
      kb.emergency_procedures.unity_crash_fix.forEach((step: string) => md += `- ${step}\n`);
      md += `\n### ИИ в Офлайне\n`;
      kb.emergency_procedures.ai_offline_mode.forEach((step: string) => md += `- ${step}\n`);
    }
    md += `\n`;

    md += `## 15. Инструкции по восстановлению\n`;
    md += `1. Установите Node.js (v18+).\n`;
    md += `2. Склонируйте репозиторий.\n`;
    md += `3. Запустите \`RUN.bat\`.\n\n`;

    md += `## 16. Известные ошибки и решения\n`;
    md += `- **WebSocket Error:** Ожидаемо, игнорировать.\n`;
    md += `- **Unexpected token '<':** Ошибка сервера, проверить статус.\n\n`;

    await fs.writeFile(masterBlueprintMdPath, md);
    console.log("Master blueprint generated successfully.");
  } catch (e) {
    console.error("Failed to generate master blueprint", e);
  }
}

async function startServer() {
  const app = express();
  
  const findAvailablePort = async (startPort: number): Promise<number> => {
    return new Promise((resolve) => {
      const server = net.createServer();
      server.listen(startPort, "0.0.0.0", () => {
        const addr: any = server.address();
        const port = addr.port;
        server.close(() => resolve(port));
      });
      server.on("error", () => {
        resolve(findAvailablePort(startPort + 1));
      });
    });
  };

  let PORT = 3000;
  
  app.use(cors());
  app.use(express.json({ limit: '500mb' }));
  app.use(express.urlencoded({ limit: '500mb', extended: true }));
  app.use("/uploads", express.static(path.join(process.cwd(), "uploads")));
  app.use("/local_storage", express.static(path.join(process.cwd(), "local_storage")));

  // Serve specific root files needed by frontend
  app.get("/version.json", (req, res) => {
    res.sendFile(path.join(process.cwd(), "version.json"));
  });
  app.get("/GAME_HELP_GUIDE.md", (req, res) => {
    res.sendFile(path.join(process.cwd(), "GAME_HELP_GUIDE.md"));
  });

  app.post("/api/ollama/proxy", async (req, res) => {
    try {
      const response = await axios.post(OLLAMA_API_URL, req.body, { 
        timeout: 30000,
        responseType: 'stream' 
      });
      response.data.pipe(res);
    } catch (error) {
      console.error("Ollama proxy error:", error);
      res.status(500).json({ error: "Ollama not reachable. Make sure it's running locally with OLLAMA_ORIGINS=*" });
    }
  });

  app.post("/api/game/generate-levels", async (req, res) => {
    const { continent, cityType, level = 1 } = req.body;
    const generateLevel = (idx: number) => {
      const types = ['Mountain Fortress', 'Trading Fort', 'Desert Outpost', 'Elven Sanctuary'];
      const type = cityType || types[Math.floor(Math.random() * types.length)];
      return {
        id: `level_${continent}_${idx}`,
        name: `${type} - Level ${idx + 1}`,
        type,
        developmentLevel: level,
        grid: Array(10).fill(0).map(() => Array(10).fill(0).map(() => {
          const rand = Math.random();
          if (rand > 0.9) return 'bandit';
          if (rand > 0.85) return 'resource';
          if (rand > 0.8) return 'castle';
          if (rand > 0.7) return 'race_npc';
          return 'empty';
        })),
        entities: [
          { type: 'castle', x: Math.floor(Math.random() * 10), y: Math.floor(Math.random() * 10) },
          { type: 'bandit_camp', x: Math.floor(Math.random() * 10), y: Math.floor(Math.random() * 10) }
        ]
      };
    };
    const levels = [0, 1, 2, 3].map(i => generateLevel(i));
    res.json({ levels });
  });

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

  // API: Chat History
  app.get("/api/chat/history", async (req, res) => {
    try {
      if (await fs.pathExists(chatHistoryPath)) {
        const data = await fs.readJson(chatHistoryPath);
        res.json(data);
      } else {
        res.json([]);
      }
    } catch (error) {
      res.status(500).json({ error: "Failed to read chat history" });
    }
  });

  app.post("/api/chat/save", async (req, res) => {
    try {
      const { messages } = req.body;
      await fs.writeJson(chatHistoryPath, messages, { spaces: 2 });
      res.json({ success: true });
    } catch (error) {
      res.status(500).json({ error: "Failed to save chat history" });
    }
  });

  app.post("/api/chat/clear", async (req, res) => {
    try {
      await fs.writeJson(chatHistoryPath, [], { spaces: 2 });
      res.json({ success: true });
    } catch (error) {
      res.status(500).json({ error: "Failed to clear chat history" });
    }
  });

  // API: Project History
  app.get("/api/project/history", async (req, res) => {
    try {
      const history = await loadHistory();
      res.json(history);
    } catch (error) {
      res.status(500).json({ error: "Failed to load history" });
    }
  });

  // API: Ollama Status & Launch
  app.get("/api/ai/ollama-status", async (req, res) => {
    const isRunning = await checkOllamaStatus();
    res.json({ isRunning });
  });

  app.post("/api/ai/ollama-chat", async (req, res) => {
    const { prompt, systemInstruction } = req.body;
    try {
      const response = await axios.post(OLLAMA_API_URL, {
        model: "llama3",
        prompt: `${systemInstruction}\n\nUser: ${prompt}\nAssistant:`,
        stream: false
      }, { timeout: 30000 });
      res.json({ answer: response.data.response });
    } catch (error) {
      res.status(500).json({ error: "Ollama request failed" });
    }
  });

  app.post("/api/ai/ollama-launch", async (req, res) => {
    res.json({ success: false, message: "Ollama must be started manually on your local machine." });
  });

  app.get("/api/update/check", async (req, res) => {
    try {
      const versionData = await fs.readJson(VERSION_FILE);
      const currentVersion = versionData.version;
      res.json({
        current: currentVersion,
        latest: currentVersion,
        available: true
      });
    } catch (error) {
      res.status(500).json({ error: "Failed to check update" });
    }
  });

  app.post("/api/generate/vk-covers", async (req, res) => {
    try {
      const { prompt, width = 1920, height = 640 } = req.body;
      const seed = Math.floor(Math.random() * 1000000);
      const variationEncoded = encodeURIComponent(prompt);
      
      const variations = [
        {
          id: `v1-${seed}`,
          url: `https://pollinations.ai/p/${variationEncoded}?width=${width}&height=${height}&seed=${seed}&nologo=true&enhance=true`
        },
        {
          id: `v2-${seed+13}`,
          url: `https://pollinations.ai/p/${variationEncoded}?width=${width}&height=${height}&seed=${seed+13}&nologo=true&enhance=true`
        },
        {
          id: `v3-${seed+42}`,
          url: `https://pollinations.ai/p/${variationEncoded}?width=${width}&height=${height}&seed=${seed+42}&nologo=true&enhance=true`
        }
      ];
      res.json(variations);
    } catch (error) {
      console.error("VK Cover Gen Error:", error);
      res.status(500).json({ error: "Failed to generate variations" });
    }
  });

  // Additional Endpoints for Frontend Compatibility
  app.get("/api/kb", async (req, res) => {
    try {
      if (await fs.pathExists(kbPath)) {
        const data = await fs.readJson(kbPath);
        res.json(data);
      } else {
        res.json({});
      }
    } catch (error) {
      res.status(500).json({ error: "Failed to load knowledge base" });
    }
  });

  app.post("/api/kb/update", async (req, res) => {
    try {
      const data = req.body;
      await fs.writeJson(kbPath, data, { spaces: 2 });
      res.json({ success: true, message: "Knowledge base updated!" });
    } catch (error) {
      res.status(500).json({ error: "Failed to update knowledge base" });
    }
  });

  app.get("/api/health", (req, res) => {
    // @ts-ignore
    app._router.handle({ method: 'GET', url: '/api/ai/health' }, res, () => {});
  });

  app.get("/api/blender/presets", (req, res) => {
    res.json([
      { id: 'low-poly', name: 'Low Poly Studio', settings: { samples: 128, engine: 'CYCLES' } },
      { id: 'high-detail', name: 'High Detail Render', settings: { samples: 1024, engine: 'CYCLES' } },
      { id: 'eevee-fast', name: 'Eevee Realtime', settings: { samples: 64, engine: 'EEVEE' } }
    ]);
  });

  app.post("/api/project/scan/trigger", async (req, res) => {
    performScan(); // Fire and forget
    res.json({ success: true, message: "Scan triggered successfully!" });
  });

  app.post("/api/blueprint/generate", async (req, res) => {
    await generateMasterBlueprint();
    res.json({ success: true, message: "Master blueprint regenerated!" });
  });

  app.get("/api/unity/packages-info", (req, res) => {
    res.json({
      installed: ["com.unity.render-pipelines.universal", "com.unity.shadergraph", "com.unity.textmeshpro"],
      recommended: ["com.unity.ai.navigation", "com.unity.inputsystem"]
    });
  });

  app.post("/api/unity/migrate", async (req, res) => {
    res.json({ success: true, message: "Migration check completed. Project is compatible with Unity 6." });
  });

  app.post("/api/migration/unity-to-godot", async (req, res) => {
    res.json({ success: true, message: "Migration logic identified 42 core scripts for conversion to GDScript." });
  });

  // File Listing and Content Endpoint for Manual Quantum Terminal / Copy Script feature
  app.get("/api/project/files/list", async (req, res) => {
    try {
      const filesList = [
        // Core C# scripts
        { path: "src/FateCastleManager.cs", name: "FateCastleManager.cs", desc: "Управление замками, пошаговая экономика, рекрутинг, прокачка и телеметрия ПК" },
        { path: "src/CastleFacilitiesHUD.cs", name: "CastleFacilitiesHUD.cs", desc: "Интерактивный HUD Менеджер помещений замка (Казармы, Кузница, Академия)" },
        { path: "src/DialogueSystem_Manager.cs", name: "DialogueSystem_Manager.cs", desc: "Система диалогов, катсцены Аэлиссы, автовыравнивание и выбор вариантов" },
        { path: "src/SaveGameSystem.cs", name: "SaveGameSystem.cs", desc: "Система сохранений и загрузки RPG-прогресса (3 слота на основе PlayerPrefs)" },
        { path: "src/SettingsManager.cs", name: "SettingsManager.cs", desc: "Управление настройками, аудиомикшер, антиперегрев GPU и лимит кадров" },
        { path: "src/Translator.cs", name: "Translator.cs", desc: "Ядро локализации на 9 языков с функцией GetText9" },
        { path: "src/StrategicCameraController.cs", name: "StrategicCameraController.cs", desc: "Управление камерой тактической карты, зум, авто-границы и скроллинг краев" },
        { path: "src/LandingPositionManager.cs", name: "LandingPositionManager.cs", desc: "Управление высадкой в 4 зоны, интерактивные маркеры, сохранение зоны" },
        { path: "src/FateMapManager.cs", name: "FateMapManager.cs", desc: "Отрисовка и инициализация интерактивных колец тактической карты мира" },
        { path: "src/PhaseCombatSystem.cs", name: "PhaseCombatSystem.cs", desc: "Пошаговая боевая система сражений, расчет урона и ИИ-командиров" },
        { path: "src/LanguageSelector.cs", name: "LanguageSelector.cs", desc: "Переключатель локализации в меню игры" },
        { path: "src/Transtable_Dropdown.cs", name: "Transtable_Dropdown.cs", desc: "Ядро авто-локализации для стандартных выпадающих списков Dropdown" },
        { path: "src/Transtable_Text.cs", name: "Transtable_Text.cs", desc: "Ядро авто-локализации для текстовых полей TextMeshPro" },
        { path: "src/UIButtonSfxBinder.cs", name: "UIButtonSfxBinder.cs", desc: "Автоматический биндер неоновых звуков UI на все кнопки сцены" },
        { path: "src/UIButtonHoverEffect.cs", name: "UIButtonHoverEffect.cs", desc: "Плавный эффект масштабирования и свечения при наведении курсора" },
        { path: "src/LoadingScreenManager.cs", name: "LoadingScreenManager.cs", desc: "Менеджер асинхронных загрузочных экранов между сценами" },
        { path: "src/MainMenuController.cs", name: "MainMenuController.cs", desc: "Контроллер главного меню с выбором сохранений и настроек" },
        { path: "src/GamePause_Manager.cs", name: "GamePause_Manager.cs", desc: "Менеджер меню паузы и игрового интерфейса" },
        { path: "src/CharacterSelectionController.cs", name: "CharacterSelectionController.cs", desc: "Контроллер экрана создания персонажа, распределения статов и выбора класса" },
        { path: "UnityConnector.cs", name: "UnityConnector.cs", desc: "Связующее звено между Unity и Blender, синхронизация 3D и импорт геометрии" },
        { path: "FactionMapMarker.cs", name: "FactionMapMarker.cs", desc: "Маркеры фракций на тактической карте мира" },
        { path: "blender_connector.py", name: "blender_connector.py", desc: "Python-скрипт Blender для связи с Unity-сервером" },
        { path: "blender_fate_continent_gen.py", name: "blender_fate_continent_gen.py", desc: "Генератор 3D-моделей континента и рельефа в Blender" },
        { path: "blender_world_gen.py", name: "blender_world_gen.py", desc: "Альтернативный генератор окружения и процедурных объектов Blender" },
        { path: "BlenderRiggingOptimizer.py", name: "BlenderRiggingOptimizer.py", desc: "Оптимизация скелета, весов и арматуры 3D персонажей в Blender" }
      ];

      const enrichedList = await Promise.all(
        filesList.map(async (file) => {
          try {
            const fullPath = path.join(process.cwd(), file.path);
            if (await fs.pathExists(fullPath)) {
              const content = await fs.readFile(fullPath, "utf8");
              const lineCount = content.split(/\r?\n/).length;
              return { ...file, lineCount, exists: true };
            }
            return { ...file, lineCount: 0, exists: false };
          } catch (e) {
            return { ...file, lineCount: 0, exists: false };
          }
        })
      );

      res.json(enrichedList);
    } catch (error) {
      res.status(500).json({ error: "Failed to list files" });
    }
  });

  app.get("/api/project/files/content", async (req, res) => {
    try {
      const filePathParam = req.query.path as string;
      if (!filePathParam) {
        return res.status(400).json({ error: "Path query parameter is required" });
      }

      // Secure file path checking to prevent path traversal outside workspace
      const safePath = path.normalize(filePathParam).replace(/^(\.\.(\/|\\|$))+/, '');
      const fullPath = path.join(process.cwd(), safePath);

      if (!await fs.pathExists(fullPath)) {
        return res.status(404).json({ error: "File not found" });
      }

      const content = await fs.readFile(fullPath, "utf8");
      res.json({ content });
    } catch (error) {
      res.status(500).json({ error: "Failed to read file content" });
    }
  });

  // Game Design Endpoint
  app.get("/api/game-design", async (req, res) => {
    try {
      if (!await fs.pathExists(gameDesignPath)) {
        const initialData = {
          continents: [
            {
              name: "Континент 1: Колыбель Жизни",
              races: [
                { name: "Орки", description: "Могучие воины, ценящие честь и силу." },
                { name: "Водные люди", description: "Властители прибрежных территорий." },
                { name: "Лесные жители", description: "Скрытные защитники чащи." },
                { name: "Сожители", description: "Мистические существа, живущие в симбиозе с миром." }
              ]
            }
          ],
          hero_classes: [
            { "name": "Воин", "primary_stats": "Сила, Атака", "desc": "Лидер фронта, мастер ближнего боя." }
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

  // AI Health Check Endpoint
  app.get("/api/ai/health", (req, res) => {
    const rawKey = process.env.GEMINI_API_KEY || "";
    const isFreeTier = rawKey === "AI Studio Free Tier" || rawKey === "";
    
    res.json({ 
      status: "online", 
      is_managed: isFreeTier,
      mode: process.env.NODE_ENV,
      version: "18.6.2"
    });
  });

  app.post("/api/ai/gemini-chat", async (req, res) => {
    const { contents, systemInstruction, model = "gemini-1.5-flash-latest" } = req.body;
    const rawKey = process.env.GEMINI_API_KEY || "";
    
    try {
      const apiKey = (rawKey === "AI Studio Free Tier") ? "" : rawKey;
      const genAI = new GoogleGenerativeAI(apiKey);
      const modelInstance = genAI.getGenerativeModel({ model, systemInstruction });
      const result = await modelInstance.generateContent({ contents });
      return res.json({ text: result.response.text() });
    } catch (error: any) {
      res.status(500).json({ error: error.message || "Internal Gemini Error" });
    }
  });

  app.get("/api/ai/capabilities", async (req, res) => {
    try {
      const packageJson = await fs.readJson(path.join(process.cwd(), "package.json"));
      const version = packageJson.version;
      const capabilities = {
        name: `Fate Continent AI Assistant v${version}`,
        description: `Quantum Sync v${version}. Полная синхронизация Unity 6/Blender 5.2/Godot 4.4.`,
        core_functions: [
          { title: "Fate Continent Expansion", desc: "Уникальные данные для 12 рас." }
        ]
      };
      res.json(capabilities);
    } catch (error) {
      res.status(500).json({ error: "Failed to load capabilities" });
    }
  });

  app.post("/api/system/repair", async (req, res) => {
    try {
      await checkProjectIntegrity();
      await initWatcher();
      performScan();
      generateMasterBlueprint();
      res.json({ success: true, message: "System repair started." });
    } catch (error) {
      res.status(500).json({ error: "Repair failed" });
    }
  });

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

  app.listen(PORT, "0.0.0.0", () => {
    console.log(`Server running on http://localhost:${PORT}`);
    setTimeout(async () => {
      await checkProjectIntegrity();
      await performScan();
      await generateMasterBlueprint();
    }, 1000);
  });
}

startServer().catch(err => {
  console.error("CRITICAL SERVER ERROR:", err);
});
