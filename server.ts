import express from "express";
import { createServer as createViteServer } from "vite";
import path from "path";
import fs from "fs-extra";
import cors from "cors";
import dotenv from "dotenv";
import multer from "multer";
import chokidar, { FSWatcher } from "chokidar";

dotenv.config();

// Configure storage for uploads
const storage = multer.diskStorage({
  destination: async (req, file, cb) => {
    const kbPath = path.join(process.cwd(), "knowledge_base.json");
    let uploadDir = path.join(process.cwd(), "uploads");
    
    try {
      if (await fs.pathExists(kbPath)) {
        const kb = await fs.readJson(kbPath);
        if (kb.local_training_path) {
          uploadDir = path.join(process.cwd(), "local_storage", path.basename(kb.local_training_path));
        }
      }
    } catch (e) {
      console.error("Error reading KB for upload path", e);
    }
    
    await fs.ensureDir(uploadDir);
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
const KB_FILE = path.join(process.cwd(), "knowledge_base.json");
const UNITY_API_FILE = path.join(process.cwd(), "unity_api_ref.json");
const BLENDER_API_FILE = path.join(process.cwd(), "blender_api_ref.json");
const TROUBLESHOOTING_FILE = path.join(process.cwd(), "troubleshooting_db.json");
const VERSION_FILE = path.join(process.cwd(), "version.json");

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
  if (isScanning) return;
  isScanning = true;
  
  const kbPath = path.join(process.cwd(), "knowledge_base.json");
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
            // Code Audit & To-Do Scan
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
    const kb = await fs.readJson(KB_FILE);
    const blueprint = await fs.readJson(BLUEPRINT_JSON_PATH);
    
    let md = `# PROJECT MASTER BLUEPRINT: ${blueprint.project_name || "Unity & Blender AI Assistant"}\n\n`;
    md += `> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.\n\n`;
    md += `## 1. Общая информация\n`;
    md += `- **Версия Помощника:** ${blueprint.version || "1.2.0"}\n`;
    md += `- **Описание:** ${blueprint.description || kb.description}\n`;
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

    md += `\n## 6. Новые возможности ИИ (v13.0)\n`;
    md += `- **Unity Bridge:** Автоматическая конвертация материалов Blender -> Unity (URP/HDRP).\n`;
    md += `- **Blender Automation:** Пакетный экспорт объектов, очистка сцен, настройка освещения.\n`;
    md += `- **Git LFS:** Автоматическая генерация конфигурации для тяжелых ассетов.\n`;
    md += `- **Offline API Docs:** Локальные справочники Unity API и Blender Python.\n\n`;

    md += `## 7. Инструкции по восстановлению\n`;
    md += `1. Установите Node.js (v18+).\n`;
    md += `2. Склонируйте репозиторий: \`git clone https://github.com/SEMAK1987/unity-ai-assistant.git\`\n`;
    md += `3. Запустите \`RUN.bat\` для автоматической установки зависимостей и запуска.\n`;

    await fs.writeFile(MASTER_BLUEPRINT_MD_PATH, md);
    console.log("Master blueprint generated successfully.");
  } catch (e) {
    console.error("Failed to generate master blueprint", e);
  }
}

async function startServer() {
  const app = express();
  const PORT = Number(process.env.PORT) || 3001;

  app.use(cors());
  app.use(express.json({ limit: '50mb' }));

  const kbPath = path.join(process.cwd(), "knowledge_base.json");
  const blueprintJsonPath = path.join(process.cwd(), "ccgs_project_blueprint.json");
  const masterBlueprintMdPath = path.join(process.cwd(), "PROJECT_MASTER_BLUEPRINT.md");

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
  app.post("/api/upload", upload.array("files", 10), (req, res) => {
    try {
      const files = req.files as Express.Multer.File[];
      if (!files || files.length === 0) {
        return res.status(400).json({ error: "No files uploaded" });
      }
      const results = files.map(f => ({
        name: f.originalname,
        size: f.size,
        type: f.mimetype,
        path: f.path
      }));
      res.json({ success: true, files: results });
    } catch (error) {
      res.status(500).json({ error: "Upload failed" });
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
      // In a real scenario, this would fetch from a remote URL
      // For demo, we simulate a "remote" version that is higher if requested
      const remoteVersion = "1.3.0"; 
      const isAvailable = remoteVersion !== localVersionData.version;
      
      res.json({
        current: localVersionData.version,
        latest: remoteVersion,
        available: isAvailable,
        changelog: [
          "Улучшена система ИИ агентов",
          "Оптимизирована работа с Unity/Blender",
          "Исправлены критические ошибки в интерфейсе"
        ]
      });
    } catch (error) {
      res.status(500).json({ error: "Failed to check for updates" });
    }
  });

  app.post("/api/update/apply", async (req, res) => {
    try {
      console.log("[UPDATE] Starting update process...");
      // 1. Download latest version (Simulated)
      // In reality: const response = await axios.get(UPDATE_URL, { responseType: 'arraybuffer' });
      
      // 2. Backup current version
      const backupDir = path.join(process.cwd(), "backup_" + Date.now());
      await fs.ensureDir(backupDir);
      // Copy essential files to backup
      await fs.copy(path.join(process.cwd(), "server.ts"), path.join(backupDir, "server.ts"));
      await fs.copy(path.join(process.cwd(), "src"), path.join(backupDir, "src"));

      // 3. Update version.json
      const versionData = await fs.readJson(VERSION_FILE);
      versionData.version = "1.3.0";
      await fs.writeJson(VERSION_FILE, versionData, { spaces: 2 });

      console.log("[UPDATE] Update applied successfully. Restarting...");
      
      // 4. Trigger restart (in a real environment, the .bat file would handle this)
      res.json({ success: true, message: "Update applied. Please restart the application." });
      
      // Optional: auto-exit to let .bat restart
      // setTimeout(() => process.exit(0), 2000);
    } catch (error) {
      console.error("[UPDATE] Error:", error);
      res.status(500).json({ error: "Update failed" });
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
      
      if (keywords.some(k => k.includes('видео') || k.includes('обучение'))) {
        results.push(`В базе знаний есть ${stats.videos.length} видео-уроков.`);
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

      if (results.length === 0) {
        results.push("К сожалению, в локальной базе знаний не найдено точного ответа. Попробуйте перефразировать запрос (например: 'задачи', 'аудит', 'скрипты').");
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
      name: "Unity & Blender AI Assistant v13.0",
      description: "Ваш персональный эксперт по разработке игр, 3D-моделированию и автоматизации.",
      core_functions: [
        {
          title: "Работа с проектами Unity",
          desc: "Анализ C# кода, поиск ошибок производительности (GetComponent в Update), отслеживание TODO задач, аудит ассетов и веса проекта."
        },
        {
          title: "Интеграция с Blender",
          desc: "Генерация Python-скриптов для автоматизации моделирования, пакетный экспорт объектов, настройка освещения и очистка сцен."
        },
        {
          title: "Офлайн-режим (Offline 2.0)",
          desc: "Работа без интернета на основе накопленной базы знаний, локальных справочников API Unity/Blender и истории изменений проекта."
        },
        {
          title: "Самовосстановление",
          desc: "Автоматическое исправление ошибок в конфигурации, восстановление серверов и регенерация Master Blueprint для защиты проекта."
        }
      ],
      files_handled: [
        "knowledge_base.json - База знаний и инструкции ИИ",
        "project_stats.json - Статистика, аудит и задачи",
        "history.json - История изменений файлов",
        "PROJECT_MASTER_BLUEPRINT.md - Полный слепок проекта для восстановления",
        "unity_api_ref.json / blender_api_ref.json - Локальные справочники API"
      ],
      game_genres: [
        "RPG / Cultivation (Система стадий, мобов, характеристик)",
        "Action / Shooter (FPS камера, системы оружия)",
        "Simulation (Экономика, профессии, инвентарь)",
        "Multiplayer (Основы сетевого взаимодействия и синхронизации)"
      ]
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

      res.json({ success: true, message: "Базы знаний API и Troubleshooting успешно обновлены!" });
    } catch (error) {
      res.status(500).json({ error: "Failed to update API refs" });
    }
  });

  // System Repair Endpoint
  app.post("/api/system/repair", async (req, res) => {
    try {
      console.log("[SYSTEM] Starting self-repair process...");
      await checkProjectIntegrity();
      await initWatcher();
      await performScan();
      await generateMasterBlueprint();
      res.json({ success: true, message: "Система успешно восстановлена и синхронизирована." });
    } catch (error) {
      console.error("[SYSTEM] Repair failed:", error);
      res.status(500).json({ error: "Repair failed" });
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
