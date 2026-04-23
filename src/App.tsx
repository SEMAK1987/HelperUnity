import React, { useState, useEffect, useRef } from 'react';
import { 
  Cpu, 
  Code, 
  Box, 
  Zap, 
  BrainCircuit,
  Send, 
  Copy, 
  Check, 
  Terminal, 
  Settings,
  Sparkles,
  Gamepad2,
  Cuboid as Cube,
  Folder,
  Info,
  Github,
  Layers,
  Paperclip,
  FileText,
  Image as ImageIcon,
  Video,
  Music,
  X,
  RefreshCw,
  Wifi,
  WifiOff,
  ChevronRight,
  ChevronLeft,
  AlertTriangle,
  ExternalLink,
  BookOpen,
  GitBranch,
  Type,
  FileCode,
  Trash2,
  Database,
  Code2,
  HelpCircle,
  Download,
  Save,
  Map as MapIcon,
  Users,
  Shield,
  ArrowRight,
  Target,
  Layout,
  Swords,
  Clock
} from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { GoogleGenerativeAI } from "@google/generative-ai";
import Markdown from 'react-markdown';

// --- Types ---
interface Message {
  role: 'user' | 'assistant';
  content: string;
  timestamp: number;
  files?: any[];
  audioVariants?: { name: string; url: string }[];
}

interface KBData {
  version: string;
  name: string;
  description: string;
  project_path: string;
  local_training_path?: string;
  system_instruction: string;
  unity_ai_assistant?: {
    description: string;
    combined_knowledge: string;
  };
  blender_manuals?: string[];
}

interface ProjectScan {
  scripts: string[];
  prefabs: string[];
  scenes: string[];
  animations: string[];
  animators: string[];
  pdfs: string[];
  videos: string[];
  others: string[];
  total_files: number;
  last_updated?: string;
  analysis: {
    audit_issues: { file: string; type: string; message: string }[];
    todos: { file: string; type: string; text: string }[];
    asset_stats: {
      total_size: number;
      large_files: { path: string; size: string }[];
    };
    dependencies: Record<string, string[]>;
  };
}

interface HistoryItem {
  event: string;
  path: string;
  timestamp: string;
}

interface BlenderPreset {
  id: string;
  name: string;
  desc: string;
  code: string;
}

interface UnityStatus {
  is_running: boolean;
  version: string;
  project_path: string;
}

interface BlenderStatus {
  is_running: boolean;
  version: string;
}

interface GimpStatus {
  is_running: boolean;
  version: string;
}

interface RedotStatus {
  is_running: boolean;
  version: string;
}

interface PhotoshopStatus {
  is_running: boolean;
  version: string;
  path: string;
}

const VKImageCard = ({ res, type, showNotification, onZoom }: { res: any, type: string, showNotification: any, onZoom: (url: string) => void }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [retryKey, setRetryKey] = useState(0);

  // We append retryKey to the URL to bypass cache
  const imageUrl = `${res.url}&retry=${retryKey}`;

  return (
    <motion.div 
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      onClick={() => !loading && !error && onZoom(imageUrl)}
      className={`relative group overflow-hidden rounded-3xl border transition-all duration-500 shadow-2xl bg-white/5 cursor-zoom-in ${
        error ? 'border-red-500/50' : 'border-white/10 hover:border-blue-500/50'
      }`}
    >
      <div className={`w-full relative ${type === 'live' ? 'aspect-[9/16]' : 'aspect-[15.9/4]'}`}>
        {loading && (
          <div className="absolute inset-0 flex flex-col items-center justify-center p-6 space-y-4 bg-black/40 z-10 backdrop-blur-sm">
            <RefreshCw className="w-8 h-8 text-blue-500 animate-spin" />
            <div className="text-center">
              <p className="text-[10px] text-white font-black tracking-widest animate-pulse uppercase">Манифестация...</p>
              <p className="text-[8px] text-slate-500 mt-1 uppercase tracking-tighter">Нейросеть рисует ассет</p>
            </div>
          </div>
        )}
        
        {error ? (
          <div className="absolute inset-0 flex flex-col items-center justify-center p-6 space-y-4 bg-red-950/20 backdrop-blur-sm z-20">
            <AlertTriangle className="w-10 h-10 text-red-500" />
            <div className="text-center">
              <p className="text-[10px] text-white font-bold uppercase mb-1">Ошибка синтеза</p>
              <p className="text-[8px] text-red-400/60 uppercase max-w-[120px] mx-auto leading-relaxed">Сервер перегружен. Попробуйте еще раз.</p>
            </div>
            <button 
              onClick={(e) => { e.stopPropagation(); setError(false); setLoading(true); setRetryKey(prev => prev + 1); }}
              className="px-5 py-2 bg-white/10 hover:bg-white/20 border border-white/10 rounded-2xl text-[9px] font-black uppercase transition-all flex items-center gap-2 group pointer-events-auto"
            >
              <RefreshCw className="w-3 h-3 group-hover:rotate-180 transition-transform duration-500" />
              Повторить
            </button>
          </div>
        ) : (
          <img 
            src={imageUrl} 
            alt={`VK Cover ${res.id}`} 
            className={`w-full h-full object-cover transition-transform duration-[2000ms] group-hover:scale-110 ${loading ? 'opacity-0 scale-110' : 'opacity-100 scale-100'}`}
            onLoad={() => setLoading(false)}
            onError={() => { setError(true); setLoading(false); }}
            referrerPolicy="no-referrer"
          />
        )}

        {/* Overlay - only show if loaded */}
        {!loading && !error && (
          <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/20 to-transparent opacity-0 group-hover:opacity-100 transition-all duration-500 flex flex-col justify-end p-6 translate-y-4 group-hover:translate-y-0">
             <div className="flex items-center justify-between mb-2">
                <span className="px-3 py-1 bg-blue-600 rounded-lg text-[10px] font-black uppercase text-white shadow-lg border border-blue-400/30">
                  Вариант #{res.id}
                </span>
                <div className="flex gap-2">
                  <a 
                    href={imageUrl} 
                    download={res.filename}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="p-3 bg-white text-black rounded-2xl hover:bg-blue-600 hover:text-white transition-all transform hover:scale-110 active:scale-95 shadow-xl border border-white/10"
                    onClick={() => showNotification(`Подготовка файла #${res.id}...`, "info")}
                  >
                    <Download className="w-5 h-5" />
                  </a>
                </div>
             </div>
             <p className="text-[9px] text-slate-400 font-medium italic line-clamp-1 opacity-60">{res.prompt_note}</p>
          </div>
        )}
      </div>
    </motion.div>
  );
};

// --- App Component ---
export default function App() {
  const [kb, setKb] = useState<KBData | null>(null);
  const [activeTab, setActiveTab] = useState<'chat' | 'dashboard' | 'project_info' | 'migration' | 'game_design'>('chat');
  const [appVersion, setAppVersion] = useState('17.12.0');
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const [projectScan, setProjectScan] = useState<ProjectScan | null>(null);
  const [unityStatus, setUnityStatus] = useState<UnityStatus | null>(null);
  const [blenderStatus, setBlenderStatus] = useState<BlenderStatus | null>(null);
  const [gimpStatus, setGimpStatus] = useState<GimpStatus | null>(null);
  const [redotStatus, setRedotStatus] = useState<RedotStatus | null>(null);
  const [photoshopStatus, setPhotoshopStatus] = useState<PhotoshopStatus | null>(null);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [blenderPresets, setBlenderPresets] = useState<BlenderPreset[]>([]);
  const [isClearingChat, setIsClearingChat] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadTimeRemaining, setUploadTimeRemaining] = useState<string | null>(null);
  const [showGithubGuide, setShowGithubGuide] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [showQuantumLink, setShowQuantumLink] = useState(false);
  const [guideTab, setGuideTab] = useState<'blender' | 'unity' | 'manual'>('blender');
  const [manualPrompt, setManualPrompt] = useState('');
  const [manualResultCode, setManualResultCode] = useState('');
  const [isManualGenerating, setIsManualGenerating] = useState(false);
  const [manualTarget, setManualTarget] = useState<'blender' | 'unity'>('blender');
  const [localPathInput, setLocalPathInput] = useState('');
  const [projectPathInput, setProjectPathInput] = useState('');
  const [gimpPathInput, setGimpPathInput] = useState('');
  const [redotPathInput, setRedotPathInput] = useState('');
  const [blenderVersionInput, setBlenderVersionInput] = useState('');
  const [isGeneratingBlueprint, setIsGeneratingBlueprint] = useState(false);
  const [isUpdatingKB, setIsUpdatingKB] = useState(false);
  const [showCapabilities, setShowCapabilities] = useState(false);
  const [capabilities, setCapabilities] = useState<any>(null);
  const [notification, setNotification] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);
  const [attachedFiles, setAttachedFiles] = useState<any[]>([]);
  const [migrationData, setMigrationData] = useState<any>(null);
  const [isFetchingMigration, setIsFetchingMigration] = useState(false);
  const [showVKGenerator, setShowVKGenerator] = useState(false);
  const [vkPrompt, setVkPrompt] = useState('');
  const [vkType, setVkType] = useState<'static' | 'live'>('static');
  const [vkResults, setVkResults] = useState<any[]>([]);
  const [isGeneratingVK, setIsGeneratingVK] = useState(false);
  
  const chatEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [updateInfo, setUpdateInfo] = useState<any>(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const [updateProgress, setUpdateProgress] = useState(0);
  const [ollamaRunning, setOllamaRunning] = useState(false);
  const [suggestedQuestions, setSuggestedQuestions] = useState<string[]>([]);

  const fileToBase64 = async (url: string): Promise<{ mimeType: string; data: string }> => {
    try {
      const response = await fetch(url);
      const blob = await response.blob();
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          const base64data = reader.result as string;
          const data = base64data.split(',')[1];
          resolve({ mimeType: blob.type, data });
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
      });
    } catch (e) {
      console.error("Error in fileToBase64:", e);
      throw e;
    }
  };

  const [showOllamaGuide, setShowOllamaGuide] = useState(false);
  const [showMigrationModal, setShowMigrationModal] = useState(false);
  const [migrationGuide, setMigrationGuide] = useState('');
  const [unityPackages, setUnityPackages] = useState<any[]>([]);
  const [isMigrating, setIsMigrating] = useState(false);
  const [gameDesign, setGameDesign] = useState<any>(null);
  const [isSavingGameDesign, setIsSavingGameDesign] = useState(false);
  const [designSubTab, setDesignSubTab] = useState<'World' | 'Castle System' | 'Heroes & Units' | 'Visuals & Nav' | 'Abilities' | 'Balancing & Rarity' | 'Economy'>('World');

  const fetchPackagesInfo = async () => {
    try {
      const res = await fetch('/api/unity/packages-info');
      if (!res.ok) throw new Error("Failed to fetch packages");
      const data = await res.json();
      setUnityPackages(data);
    } catch (e) {
      console.error("Error fetching packages info:", e);
      showNotification("Не удалось загрузить информацию о пакетах.", "error");
    }
  };

  const handleMigrate = async () => {
    setIsMigrating(true);
    try {
      const res = await fetch('/api/unity/migrate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ from: '2022.3.62f2', to: '6000.3.10f1' })
      });
      const data = await res.json();
      setMigrationGuide(data.guide);
    } catch (e) {
      showNotification("Ошибка при генерации руководства.", "error");
    } finally {
      setIsMigrating(false);
    }
  };

  const showNotification = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
    setNotification({ message, type });
    setTimeout(() => setNotification(null), 4000);
  };

  const checkUpdates = async () => {
    try {
      const response = await fetch('/api/update/check');
      const data = await response.json();
      setUpdateInfo(data);
      if (data.available) {
        setShowUpdateModal(true);
      } else {
        showNotification("У вас уже установлена последняя версия!", "info");
      }
    } catch (error) {
      console.error("Update check error:", error);
    }
  };

  const applyUpdate = async () => {
    setIsUpdating(true);
    setUpdateProgress(0);
    
    // Detailed sync steps for UI
    const syncSteps = [
      "Проверка целостности файлов...",
      "Глубокое сканирование проекта (Аудит)...",
      "Синхронизация с локальным хранилищем...",
      "Исправление найденных ошибок...",
      "Обновление версии до 16.99.0...",
      "Инициализация Omniversal Quantum Link...",
      "Установка Нейронного Моста (Blender & Unity)...",
      "Регенерация PROJECT_MASTER_BLUEPRINT.md (Quantum Link)..."
    ];

    let step = 0;
    const interval = setInterval(() => {
      if (step < syncSteps.length) {
        setUpdateProgress(Math.floor(((step + 1) / syncSteps.length) * 90));
        step++;
      }
    }, 800);

    try {
      const response = await fetch('/api/update/apply', { method: 'POST' });
      const data = await response.json();
      
      if (response.ok && data.success) {
        setUpdateProgress(100);
        clearInterval(interval);
        setTimeout(() => {
          showNotification(data.message, "success");
          setShowUpdateModal(false);
          // Refresh KB to show new version
          fetch('/api/kb')
            .then(res => res.json())
            .then(data => setKb(data));
        }, 1000);
      } else {
        throw new Error(data.error || "Sync failed");
      }
    } catch (error) {
      console.error("Update apply error:", error);
      clearInterval(interval);
      showNotification("Ошибка при синхронизации. Попробуйте еще раз.", "error");
    } finally {
      setIsUpdating(false);
    }
  };

  // Initialize Gemini
  const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY || "");

  useEffect(() => {
    if (input.length > 0 && input.length < 15) {
      const suggestions = [
        "Как создать модульное здание в Blender?",
        "Unity DOTS: основы оптимизации",
        "Создай скрипт для поведения врагов (Unity)",
        "Геометрические ноды: процедурный город",
        "Как перенести проект из Unity в Redot?",
        "Как работает Neural Memory?",
        "Как настроить Quantum Link?"
      ].filter(s => s.toLowerCase().includes(input.toLowerCase()) || input.length < 3).slice(0, 4);
      setSuggestedQuestions(suggestions);
    } else {
      setSuggestedQuestions([]);
    }
  }, [input]);

  useEffect(() => {
    fetchGameDesign();
    // Load chat history
    fetch('/api/chat/history')
      .then(res => res.json())
      .then(data => {
        if (data && data.length > 0) setMessages(data);
      });

    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    fetch('/api/kb')
      .then(res => res.json())
      .then(data => {
        setKb(data);
        setLocalPathInput(data.local_training_path || '');
        setProjectPathInput(data.project_path || '');
        setGimpPathInput(data.gimp_path || '');
        setRedotPathInput(data.redot_path || '');
        setBlenderVersionInput(data.blender_version || '');
      })
      .catch(err => {
        console.error("Failed to fetch KB, using fallback", err);
        setKb({
          name: "Unity AI Assistant",
          version: "16.99.0",
          description: "Гибридный ИИ-помощник с Quantum Link",
          project_path: "Unknown",
          system_instruction: "Ты — экспертный ИИ-ассистент."
        });
      });

    fetch('/api/project/scan')
      .then(res => res.json())
      .then(data => data.success && setProjectScan(data.scan));

    fetch('/api/blender/presets')
      .then(res => res.json())
      .then(data => setBlenderPresets(data));

    const statusInterval = setInterval(() => {
      fetch('/api/unity/status')
        .then(res => res.json())
        .then(status => setUnityStatus(status));
      
      fetch('/api/blender/status')
        .then(res => res.json())
        .then(status => setBlenderStatus(status));

      fetch('/api/gimp/status')
        .then(res => res.json())
        .then(status => setGimpStatus(status));

      fetch('/api/redot/status')
        .then(res => res.json())
        .then(status => setRedotStatus(status));

      fetch('/api/photoshop/status')
        .then(res => res.json())
        .then(status => setPhotoshopStatus(status));
      
      fetch('/api/ai/ollama-status')
        .then(res => res.json())
        .then(data => setOllamaRunning(data.isRunning));

      fetch('/api/project/history')
        .then(res => res.json())
        .then(data => setHistory(data));
      
      fetch('/api/project/scan')
        .then(res => res.json())
        .then(data => data.success && setProjectScan(data.scan));

      // Fetch Version
      fetch('/api/ai/capabilities')
        .then(res => res.json())
        .then(data => {
          const v = data.name.match(/v([\d.]+)/)?.[1] || '17.6.0';
          setAppVersion(v);
        });
    }, 5000);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
      clearInterval(statusInterval);
    };
  }, []);

  // --- AI Task Processor (for Blender/Unity Addons) ---
  useEffect(() => {
    const processTasks = async () => {
      if (!isOnline || !genAI) return;
      
      try {
        const res = await fetch('/api/ai/tasks');
        const tasks = await res.json();
        
        if (tasks && tasks.length > 0) {
          for (const task of tasks) {
            console.log(`[AI TASK] Processing ${task.id}: ${task.prompt}`);
            
            try {
              let systemInstruction = kb?.system_instruction || "You are a helpful assistant.";
              
              if (task.target === 'blender') {
                systemInstruction += "\nIMPORTANT: GENERATE ONLY PURE PYTHON CODE FOR BLENDER 4.x. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Focus on bpy modules.";
              } else if (task.target === 'unity') {
                systemInstruction += "\nIMPORTANT: GENERATE ONLY PURE C# CODE FOR UNITY 6. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Use standard Unity namespaces.";
              }

              const model = genAI.getGenerativeModel({ 
                model: "gemini-1.5-flash",
                systemInstruction: systemInstruction
              });

              // Add context if available
              let fullPrompt = "";
              
              // Neural Memory: Include recent chat history as context for the link
              const recentHistory = messages.slice(-5).map(m => `${m.role.toUpperCase()}: ${m.content}`).join("\n");
              if (recentHistory) {
                fullPrompt += `### NEURAL MEMORY (RECENT CHAT CONTEXT) ###\n${recentHistory}\n\n`;
              }

              fullPrompt += `### TASK FOR ${task.target.toUpperCase()} ###\n${task.prompt}`;
              
              if (task.context) {
                fullPrompt += `\n\n### SOFTWARE CONTEXT ###\n${JSON.stringify(task.context)}`;
              }

              const result = await model.generateContent(fullPrompt);
              const response = await result.response;
              let code = response.text();
              
              // Clean code from potential markdown blocks
              code = code.replace(/```python\n?/g, '').replace(/```\n?/g, '').replace(/```csharp\n?/g, '').replace(/```cs\n?/g, '');
              
              await fetch('/api/ai/complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ taskId: task.id, code: code.trim() })
              });
              
              console.log(`[AI TASK] Completed ${task.id}`);
            } catch (err: any) {
              console.error(`[AI TASK] Failed ${task.id}:`, err);
              await fetch('/api/ai/complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ taskId: task.id, error: err.message })
              });
            }
          }
        }
      } catch (e) {
        // Silent error for task polling
      }
    };

    const interval = setInterval(processTasks, 3000);
    return () => clearInterval(interval);
  }, [kb, genAI, isOnline]);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    // Save chat history when messages change
    if (messages.length > 0) {
      fetch('/api/chat/save', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ messages })
      });
    }
  }, [messages]);

  const handleClearChat = async () => {
    if (isClearingChat) return;
    setIsClearingChat(true);
    try {
      const res = await fetch('/api/chat/clear', { method: 'POST' });
      if (res.ok) {
        setMessages([]);
        showNotification("Чат очищен.", "info");
      }
    } catch (e) {
      showNotification("Ошибка при очистке чата.", "error");
    } finally {
      setIsClearingChat(false);
    }
  };

  const handleManualGenerateCode = async () => {
    if ((!manualPrompt.trim() && attachedFiles.length === 0) || !genAI) return;
    setIsManualGenerating(true);
    setManualResultCode('');

    try {
      let systemInstruction = kb?.system_instruction || "You are a helpful assistant.";
      
      if (manualTarget === 'blender') {
        systemInstruction += "\nIMPORTANT: GENERATE ONLY PURE PYTHON CODE FOR BLENDER 4.x. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Focus on bpy modules.";
      } else if (manualTarget === 'unity') {
        systemInstruction += "\nIMPORTANT: GENERATE ONLY PURE C# CODE FOR UNITY 6. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Use standard Unity namespaces.";
      }

      const model = genAI.getGenerativeModel({ 
        model: "gemini-1.5-flash",
        systemInstruction: systemInstruction
      });

      const parts: any[] = [];
      const recentHistory = messages.slice(-5).map(m => `${m.role.toUpperCase()}: ${m.content}`).join("\n");
      if (recentHistory) {
        parts.push({ text: `### NEURAL MEMORY (RECENT CHAT CONTEXT) ###\n${recentHistory}\n\n` });
      }

      if (attachedFiles.length > 0) {
        for (const file of attachedFiles) {
          if (file.type && file.type.startsWith('image/')) {
            const data = await fileToBase64(file.url);
            parts.push({ inlineData: data });
          }
        }
      }

      parts.push({ text: `### MANUAL CODE REQUEST FOR ${manualTarget.toUpperCase()} ###\n${manualPrompt}` });

      const result = await model.generateContent({ contents: [{ role: 'user', parts }] });
      const response = await result.response;
      let code = response.text();
      code = code.replace(/```python\n?/g, '').replace(/```\n?/g, '').replace(/```csharp\n?/g, '').replace(/```cs\n?/g, '');
      setManualResultCode(code.trim());
      setAttachedFiles([]);
      showNotification("Код сгенерирован (Multi-Modal)!", "success");
    } catch (err: any) {
      console.error(err);
      showNotification("Ошибка генерации: " + err.message, "error");
    } finally {
      setIsManualGenerating(false);
    }
  };

  const handleLaunchOllama = async () => {
    try {
      const res = await fetch('/api/ai/ollama-launch', { method: 'POST' });
      const data = await res.json();
      if (data.success) {
        showNotification(data.message, "success");
      } else {
        showNotification(data.message, "info");
      }
    } catch (e) {
      showNotification("Не удалось связаться с сервисом Ollama.", "error");
    }
  };

  const handleSaveSettings = async () => {
    if (!kb) return;
    const updatedKb = { 
      ...kb, 
      local_training_path: localPathInput,
      project_path: projectPathInput,
      gimp_path: gimpPathInput,
      redot_path: redotPathInput,
      blender_version: blenderVersionInput
    };
    try {
      const response = await fetch('/api/kb/update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedKb)
      });
      if (response.ok) {
        setKb(updatedKb);
        setShowSettings(false);
        showNotification("Настройки сохранены. Запускаю сканирование...", "success");
        handleRefreshScan();
      }
    } catch (error) {
      console.error("Failed to save settings", error);
    }
  };

  const handleRefreshScan = async () => {
    try {
      const res = await fetch('/api/project/scan/trigger', { method: 'POST' });
      const data = await res.json();
      if (data.success) {
        setProjectScan(data.scan);
        showNotification("Статистика проекта обновлена!", "success");
      }
    } catch (e) {
      showNotification("Ошибка при сканировании проекта.", "error");
    }
  };

  const handleGenerateBlueprint = async () => {
    setIsGeneratingBlueprint(true);
    try {
      const response = await fetch('/api/blueprint/generate', {
        method: 'POST'
      });
      if (response.ok) {
        showNotification("Master Blueprint (PROJECT_MASTER_BLUEPRINT.md) успешно обновлен!", "success");
      }
    } catch (error) {
      console.error("Failed to generate blueprint", error);
    } finally {
      setIsGeneratingBlueprint(false);
    }
  };

  const handleSend = async (text: string = input) => {
    if ((!text.trim() && attachedFiles.length === 0) || isTyping || !kb) return;

    const userMsg: Message = {
      role: 'user',
      content: text,
      timestamp: Date.now(),
      files: attachedFiles.length > 0 ? [...attachedFiles] : undefined
    };

    const newMessages = [...messages, userMsg];
    setMessages(newMessages);
    setInput('');
    setAttachedFiles([]);
    setIsTyping(true);

    try {
      // Offline Fallback Check
      if (!isOnline) {
        if (ollamaRunning) {
          const ollamaRes = await fetch('/api/ai/ollama-chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
              prompt: text, 
              systemInstruction: kb.system_instruction 
            })
          });
          const ollamaData = await ollamaRes.json();
          if (ollamaRes.ok) {
            setMessages(prev => [...prev, {
              role: 'assistant',
              content: `[OLLAMA - OFFLINE]\n\n${ollamaData.answer}`,
              timestamp: Date.now()
            }]);
            return;
          }
        }

        // Fallback to local database search if Ollama fails or is not running
        const localRes = await fetch('/api/ai/local-search', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ query: text })
        });
        const localData = await localRes.json();
        setMessages(prev => [...prev, {
          role: 'assistant',
          content: `[LOCAL DB - OFFLINE]\n\n${localData.answer}`,
          timestamp: Date.now()
        }]);
        return;
      }

      // Prepare contents for Gemini (History + Images)
      const contents = [];
      // Limit history to last 10 messages to avoid token issues
      const historyToProcess = newMessages.slice(-10);
      
      for (const msg of historyToProcess) {
        const parts: any[] = [{ text: msg.content }];
        
        if (msg.files) {
          for (const file of msg.files) {
            if (file.type && file.type.startsWith('image/')) {
              try {
                // Cache base64 in the file object to avoid re-converting
                if (!file.base64) {
                  const base64 = await fileToBase64(file.url);
                  file.base64 = base64;
                }
                parts.push({
                  inlineData: file.base64
                });
              } catch (e) {
                console.error("Error converting image to base64", e);
              }
            }
          }
        }
        
        contents.push({
          role: msg.role === 'assistant' ? 'model' : 'user',
          parts: parts
        });
      }

      const model = genAI.getGenerativeModel({ 
        model: "gemini-1.5-flash",
        systemInstruction: kb.system_instruction 
      });

      const result = await model.generateContent({
        contents: contents
      });
      const response = await result.response;
      const textResponse = response.text();

      // Check for audio requests to generate variants
      const audioKeywords = ['музыка', 'песня', 'звук', 'мелодия', 'mp3', 'music', 'song', 'audio'];
      const isAudioRequest = audioKeywords.some(k => text.toLowerCase().includes(k));

      const aiMsg: Message = {
        role: 'assistant',
        content: textResponse || "Извините, я не смог сгенерировать ответ.",
        timestamp: Date.now(),
        audioVariants: isAudioRequest ? [
          { name: "Экспериментальный вариант 1 (Quantum Sonic)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3" },
          { name: "Экспериментальный вариант 2 (Neural Melodic)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3" },
          { name: "Экспериментальный вариант 3 (Void Resonance)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3" },
          { name: "Экспериментальный вариант 4 (Reality Warp)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3" },
          { name: "Экспериментальный вариант 5 (Eternal Harmony)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3" },
          { name: "Экспериментальный вариант 6 (Subatomic Beats)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3" },
          { name: "Экспериментальный вариант 7 (Quantum Distortion)", url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-7.mp3" },
        ] : undefined
      };

      setMessages(prev => [...prev, aiMsg]);
    } catch (error) {
      console.error("Gemini Error:", error);
      // Fallback to local search on error
      try {
        const localRes = await fetch('/api/ai/local-search', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ query: text })
        });
        const localData = await localRes.json();
        setMessages(prev => [...prev, {
          role: 'assistant',
          content: `[ОШИБКА СЕТИ - ЛОКАЛЬНЫЙ ПОИСК]\n\n${localData.answer}`,
          timestamp: Date.now()
        }]);
      } catch (e) {
        setMessages(prev => [...prev, {
          role: 'assistant',
          content: "Ошибка: Не удалось подключиться к ИИ и локальный поиск недоступен.",
          timestamp: Date.now()
        }]);
      }
    } finally {
      setIsTyping(false);
    }
  };

  const handleGenerateVKCovers = async () => {
    if (!vkPrompt.trim()) return;
    setIsGeneratingVK(true);
    setVkResults([]);
    try {
      const res = await fetch('/api/generate/vk-covers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          prompt: vkPrompt,
          type: vkType
        })
      });
      const data = await res.json();
      if (data.success) {
        setVkResults(data.variations);
        showNotification("Сгенерировано 10 вариантов обложек!", "success");
      }
    } catch (e) {
      showNotification("Ошибка генерации обложек.", "error");
    } finally {
      setIsGeneratingVK(false);
    }
  };

  const handleUpdateKB = async () => {
    setIsUpdatingKB(true);
    try {
      const res = await fetch('/api/kb/update-api-refs', { method: 'POST' });
      const data = await res.json();
      if (data.success) {
        showNotification(data.message, "success");
        // Refresh capabilities to show new data
        fetchCapabilities();
      }
    } catch (error) {
      showNotification("Ошибка при обновлении баз знаний.", "error");
    } finally {
      setIsUpdatingKB(false);
    }
  };

  const fetchCapabilities = async () => {
    try {
      const res = await fetch('/api/ai/capabilities');
      const data = await res.json();
      setCapabilities(data);
      setShowCapabilities(true);
    } catch (error) {
      showNotification("Не удалось загрузить информацию о возможностях.", "error");
    }
  };

  const fetchGameDesign = async () => {
    try {
      const res = await fetch('/api/game-design');
      const data = await res.json();
      setGameDesign(data);
    } catch (e) {
      console.error("Error fetching game design:", e);
    }
  };

  const handleSaveGameDesign = async () => {
    if (!gameDesign) return;
    setIsSavingGameDesign(true);
    try {
      const res = await fetch('/api/game-design/update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(gameDesign)
      });
      const data = await res.json();
      if (data.success) {
        showNotification(data.message, "success");
      }
    } catch (e) {
      showNotification("Ошибка при сохранении дизайна игры.", "error");
    } finally {
      setIsSavingGameDesign(false);
    }
  };

  const handleGetMaterialConverter = async () => {
    try {
      const res = await fetch('/api/unity/material-converter');
      const data = await res.json();
      // Copy to clipboard
      await navigator.clipboard.writeText(data.snippet);
      showNotification("C# скрипт конвертера скопирован в буфер обмена!", "success");
    } catch (error) {
      showNotification("Ошибка при получении скрипта.", "error");
    }
  };

  const handleGetGitLFS = async () => {
    try {
      const res = await fetch('/api/git/lfs-setup');
      const data = await res.json();
      await navigator.clipboard.writeText(data.content);
      showNotification(".gitattributes для LFS скопирован!", "success");
    } catch (error) {
      showNotification("Ошибка при получении конфигурации.", "error");
    }
  };

  const fetchMigrationData = async () => {
    setIsFetchingMigration(true);
    try {
      const res = await fetch('/api/migration/unity-to-godot', { method: 'POST' });
      const data = await res.json();
      if (data.success) {
        setMigrationData(data);
        setActiveTab('migration');
      }
    } catch (e) {
      showNotification("Ошибка при загрузке данных миграции.", "error");
    } finally {
      setIsFetchingMigration(false);
    }
  };
  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setUploadProgress(0);
    setUploadTimeRemaining(null);

    const formData = new FormData();
    const totalSize = Array.from(files).reduce((acc, f) => acc + f.size, 0);
    Array.from(files).forEach(f => formData.append('files', f));

    try {
      const startTime = Date.now();
      
      const xhr = new XMLHttpRequest();
      
      const uploadPromise = new Promise((resolve, reject) => {
        xhr.upload.addEventListener('progress', (event) => {
          if (event.lengthComputable) {
            const percentComplete = (event.loaded / event.total) * 100;
            setUploadProgress(Math.floor(percentComplete));
            
            // Calculate time remaining
            const elapsedTime = (Date.now() - startTime) / 1000; // in seconds
            const uploadSpeed = event.loaded / elapsedTime; // bytes per second
            const remainingBytes = event.total - event.loaded;
            const remainingTimeSeconds = remainingBytes / uploadSpeed;
            
            if (remainingTimeSeconds > 0 && isFinite(remainingTimeSeconds)) {
              const minutes = Math.floor(remainingTimeSeconds / 60);
              const seconds = Math.floor(remainingTimeSeconds % 60);
              const speedMB = (uploadSpeed / (1024 * 1024)).toFixed(2);
              setUploadTimeRemaining(
                `${minutes > 0 ? `${minutes} мин ` : ""}${seconds} сек (${speedMB} MB/s)`
              );
            }
          }
        });

        xhr.addEventListener('load', () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            try {
              const response = JSON.parse(xhr.responseText);
              resolve(response);
            } catch (e) {
              reject(new Error("Сервер вернул некорректный ответ (не JSON)"));
            }
          } else {
            reject(new Error(`Ошибка загрузки: статус ${xhr.status}`));
          }
        });

        xhr.addEventListener('error', () => reject(new Error('Upload failed')));
        xhr.addEventListener('abort', () => reject(new Error('Upload aborted')));

        xhr.open('POST', '/api/upload');
        xhr.send(formData);
      });

      const data: any = await uploadPromise;
      
      setUploadProgress(100);
      setUploadTimeRemaining(null);
      
      if (data.success) {
        setAttachedFiles(prev => [...prev, ...data.files]);
        showNotification("Файлы прикреплены к сообщению", "success");
      }
    } catch (error) {
      console.error("Upload error:", error);
      showNotification("Ошибка при загрузке. Возможно, файл слишком большой.", "error");
    } finally {
      setTimeout(() => {
        setIsUploading(false);
        setUploadProgress(0);
        setUploadTimeRemaining(null);
      }, 1000);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const copyToClipboard = (text: string, id: string) => {
    navigator.clipboard.writeText(text);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  if (!kb) {
    return (
      <div className="h-screen bg-[#0a0a0c] flex items-center justify-center">
        <motion.div 
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
        >
          <Zap className="w-8 h-8 text-blue-500" />
        </motion.div>
      </div>
    );
  }

  return (
    <div className="h-screen bg-[#0a0a0c] text-slate-300 font-sans flex overflow-hidden">
      
      {/* Sidebar for Stats and Status */}
      <aside className="w-64 border-r border-white/5 bg-black/40 flex flex-col z-50 overflow-y-auto scrollbar-none">
        <div className="p-6 border-b border-white/5">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center shadow-lg shadow-blue-600/20">
              <Cpu className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="text-sm font-bold text-white uppercase tracking-tighter">AI Assistant</h1>
              <p className="text-[10px] text-slate-500 uppercase font-mono">v{kb?.version || '12.0'}</p>
            </div>
          </div>

          <div className="space-y-2">
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">Сеть</span>
              <div className="flex items-center gap-1.5">
                {isOnline ? <Wifi className="w-3 h-3 text-green-500" /> : <WifiOff className="w-3 h-3 text-red-500" />}
                <span className={`text-[9px] font-bold uppercase ${isOnline ? 'text-green-500' : 'text-red-500'}`}>
                  {isOnline ? 'Онлайн' : 'Офлайн'}
                </span>
              </div>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">Зрение (Vision)</span>
              <div className="flex items-center gap-1.5">
                <ImageIcon className="w-3 h-3 text-blue-400" />
                <span className="text-[9px] font-bold uppercase text-blue-400">Активно</span>
              </div>
            </div>
            <div className="px-2 py-1 bg-white/5 rounded-lg border border-white/5 flex items-center gap-2">
              <Info className="w-3 h-3 text-slate-600" />
              <span className="text-[8px] text-slate-600 uppercase leading-tight">HMR WebSocket может быть отключен (это нормально)</span>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">AI Агент</span>
              <div className="flex items-center gap-1.5">
                <div className={`w-1.5 h-1.5 rounded-full ${isTyping ? 'bg-yellow-500 animate-pulse' : 'bg-green-500'}`} />
                <span className={`text-[9px] font-bold uppercase ${isTyping ? 'text-yellow-400' : 'text-green-500'}`}>
                  {isTyping ? 'Думает...' : 'Готов'}
                </span>
              </div>
            </div>
          </div>
        </div>

        <div className="p-6 space-y-6">
          {/* Project Stats */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest flex items-center gap-2">
                <Layers className="w-3 h-3" /> Статистика проекта
                <span className="flex h-1.5 w-1.5 relative">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-green-500"></span>
                </span>
              </h3>
              <button 
                onClick={handleRefreshScan}
                className="p-1 hover:bg-white/5 rounded-md transition-colors text-slate-500 hover:text-white"
                title="Обновить статистику"
              >
                <RefreshCw className="w-3 h-3" />
              </button>
            </div>
            {projectScan ? (
              <div className="space-y-3">
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Code className="w-3 h-3 text-blue-400" />
                    <span className="text-[10px] text-slate-400">Скрипты (C#)</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">{projectScan.scripts.length}</span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Box className="w-3 h-3 text-purple-400" />
                    <span className="text-[10px] text-slate-400">Префабы</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">{projectScan.prefabs.length}</span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Gamepad2 className="w-3 h-3 text-green-400" />
                    <span className="text-[10px] text-slate-400">Сцены</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">{projectScan.scenes.length}</span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Zap className="w-3 h-3 text-yellow-400" />
                    <span className="text-[10px] text-slate-400">Анимации</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">{projectScan.animations.length}</span>
                </div>
                <div className="pt-2 border-t border-white/5 flex items-center justify-between">
                  <span className="text-[10px] font-bold text-white uppercase">Всего файлов</span>
                  <span className="text-[10px] font-mono text-blue-400">{projectScan.total_files}</span>
                </div>
                <div className="mt-2 text-[8px] text-slate-600 uppercase tracking-tighter text-right italic">
                  Обновлено: {projectScan.last_updated ? new Date(projectScan.last_updated).toLocaleTimeString() : '---'}
                </div>
              </div>
            ) : (
              <div className="flex items-center gap-2 text-[10px] text-slate-600 italic">
                <RefreshCw className="w-3 h-3 animate-spin" /> Сканирование...
              </div>
            )}
          </div>

          {/* Software Status */}
          <div className="space-y-4">
            <h3 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest flex items-center gap-2">
              <Settings className="w-3 h-3" /> Статус ПО
            </h3>
            <div className="space-y-2">
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Gamepad2 className="w-3.5 h-3.5 text-blue-400" />
                  <span className="text-[10px] text-slate-300">Unity</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`w-1.5 h-1.5 rounded-full ${unityStatus?.is_running ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-slate-700'}`} />
                  <span className="text-[9px] font-mono text-slate-500">{unityStatus?.version || '---'}</span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Cube className="w-3.5 h-3.5 text-purple-400" />
                  <span className="text-[10px] text-slate-300">Blender</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`w-1.5 h-1.5 rounded-full ${blenderStatus?.is_running ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-slate-700'}`} />
                  <span className="text-[9px] font-mono text-slate-500">{blenderStatus?.version || '---'}</span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ImageIcon className="w-3.5 h-3.5 text-orange-400" />
                  <span className="text-[10px] text-slate-300">GIMP</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`w-1.5 h-1.5 rounded-full ${gimpStatus?.is_running ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-slate-700'}`} />
                  <span className="text-[9px] font-mono text-slate-500">{gimpStatus?.version || '---'}</span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Zap className="w-3.5 h-3.5 text-cyan-400" />
                  <span className="text-[10px] text-slate-300">Redot</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`w-1.5 h-1.5 rounded-full ${redotStatus?.is_running ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-slate-700'}`} />
                  <span className="text-[9px] font-mono text-slate-500">{redotStatus?.version || '---'}</span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ImageIcon className="w-3.5 h-3.5 text-blue-500" />
                  <span className="text-[10px] text-slate-300">Photoshop</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`w-1.5 h-1.5 rounded-full ${photoshopStatus?.is_running ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-slate-700'}`} />
                  <span className="text-[9px] font-mono text-slate-500">{photoshopStatus?.version || '---'}</span>
                </div>
              </div>
            </div>
          </div>

          {/* About AI Button */}
          <button 
            onClick={fetchCapabilities}
            className="w-full p-4 rounded-2xl bg-blue-600/10 border border-blue-500/20 hover:bg-blue-600/20 hover:border-blue-500/40 transition-all group text-left"
          >
            <div className="flex items-center gap-3 mb-2">
              <div className="p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors">
                <Info className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">О возможностях ИИ</span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">Всё о том, что умеет наш ИИ и как он работает с проектом.</p>
          </button>

          {/* GitHub Guide Button */}
          <button 
            onClick={() => setShowGithubGuide(true)}
            className="w-full p-4 rounded-2xl bg-white/5 border border-white/5 hover:border-blue-500/30 hover:bg-blue-600/5 transition-all group text-left"
          >
            <div className="flex items-center gap-3 mb-2">
              <div className="p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors">
                <Github className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">GitHub Guide</span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">Инструкция по переносу проекта через консоль.</p>
          </button>

          {/* Dashboard Button */}
          <button 
            onClick={() => setActiveTab('dashboard')}
            className={`w-full p-4 rounded-2xl border transition-all group text-left ${
              activeTab === 'dashboard' 
              ? 'bg-blue-600/20 border-blue-500/40 shadow-lg shadow-blue-600/10' 
              : 'bg-white/5 border-white/5 hover:border-blue-500/30 hover:bg-blue-600/5'
            }`}
          >
            <div className="flex items-center gap-3 mb-2">
              <div className={`p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors ${activeTab === 'dashboard' ? 'text-blue-400' : ''}`}>
                <Layers className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">Дашборд</span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">Панель мониторинга и статистики проекта.</p>
          </button>

          {/* Project Info Button */}
          <button 
            onClick={() => setActiveTab('project_info')}
            className={`w-full p-4 rounded-2xl border transition-all group text-left ${
              activeTab === 'project_info' 
              ? 'bg-blue-600/20 border-blue-500/40 shadow-lg shadow-blue-600/10' 
              : 'bg-white/5 border-white/5 hover:border-blue-500/30 hover:bg-blue-600/5'
            }`}
          >
            <div className="flex items-center gap-3 mb-2">
              <div className={`p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors ${activeTab === 'project_info' ? 'text-blue-400' : ''}`}>
                <Info className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">О проекте</span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">Информация о текущем проекте и его истории.</p>
          </button>
        </div>

        <div className="mt-auto p-6 border-t border-white/5">
          <div className="flex items-center gap-2 text-[10px] text-slate-500 uppercase font-mono">
            <Folder className="w-3 h-3" />
            <span className="truncate">...assistant-full</span>
          </div>
        </div>
      </aside>

      {/* Main Chat Area */}
      <main className="flex-1 flex flex-col relative overflow-hidden">
        
        {/* Header */}
        <header className="h-16 border-b border-white/5 bg-black/20 flex items-center justify-between px-6 backdrop-blur-md z-40">
          <div className="flex items-center gap-8">
            <div className="flex flex-col">
              <h2 className="text-xs font-bold text-white uppercase tracking-wider">
                Интеллектуальный помощник
              </h2>
              <span className="text-[9px] text-slate-500 uppercase tracking-widest mt-0.5">Unity & Blender Expert</span>
            </div>

            <nav className="flex items-center gap-1 bg-white/5 p-1 rounded-xl border border-white/5">
              <button 
                onClick={() => setActiveTab('chat')}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === 'chat' ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' : 'text-slate-500 hover:text-slate-300'
                }`}
              >
                <Send className="w-3.5 h-3.5" /> Чат
              </button>
              <button 
                onClick={() => setActiveTab('dashboard')}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === 'dashboard' ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' : 'text-slate-500 hover:text-slate-300'
                }`}
              >
                <Layers className="w-3.5 h-3.5" /> Хранилище
              </button>
              <button 
                onClick={() => setShowQuantumLink(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  showQuantumLink ? 'bg-orange-600 text-white shadow-lg shadow-orange-600/20' : 'text-orange-400 hover:bg-orange-600/10'
                }`}
              >
                <Zap className="w-3.5 h-3.5" /> Quantum Link
              </button>
              <button 
                onClick={fetchMigrationData}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === 'migration' ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' : 'text-slate-500 hover:text-slate-300'
                }`}
              >
                <GitBranch className="w-3.5 h-3.5" /> Миграция
              </button>
              <button 
                onClick={() => setShowVKGenerator(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-blue-400 hover:bg-blue-600/10 border border-blue-500/20`}
              >
                <ImageIcon className="w-3.5 h-3.5" /> Обложки ВК
              </button>
              <button 
                onClick={() => setActiveTab('game_design')}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === 'game_design' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-purple-400 hover:bg-purple-600/10 border border-purple-500/20'
                }`}
              >
                <Gamepad2 className="w-3.5 h-3.5" /> Студия Игры
              </button>
              <button 
                onClick={fetchCapabilities}
                className="px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-blue-400 hover:bg-blue-600/10"
              >
                <Info className="w-3.5 h-3.5" /> Возможности ИИ
              </button>
            </nav>
          </div>

          <div className="flex items-center gap-4">
            <button 
              onClick={handleLaunchOllama}
              className={`px-4 py-2 rounded-xl border transition-all group flex items-center gap-2 shadow-lg ${
                ollamaRunning 
                ? 'bg-cyan-600/20 border-cyan-500/50 text-cyan-400 shadow-cyan-600/20' 
                : 'bg-slate-800/20 border-white/5 text-slate-500 shadow-none'
              }`}
              title={ollamaRunning ? "Ollama активна" : "Ollama: Off"}
            >
              <Cpu className={`w-4 h-4 group-hover:scale-110 transition-transform ${ollamaRunning ? 'animate-pulse' : ''}`} />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">
                {ollamaRunning ? 'Ollama: OK' : 'Ollama: Off'}
              </span>
            </button>
            <div className="flex items-center gap-2 px-3 py-1.5 bg-white/5 rounded-full border border-white/5">
              <Sparkles className="w-3 h-3 text-blue-400" />
              <span className="text-[10px] font-bold text-slate-400 uppercase">Gemini 1.5 Pro</span>
            </div>
          </div>
        </header>

        {/* Update Modal */}
        <AnimatePresence>
          {showUpdateModal && (
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-[200] bg-black/90 backdrop-blur-md flex items-center justify-center p-6"
            >
              <motion.div 
                initial={{ scale: 0.9, y: 20 }}
                animate={{ scale: 1, y: 0 }}
                exit={{ scale: 0.9, y: 20 }}
                className="bg-[#0f0f11] border border-white/10 rounded-3xl w-full max-w-lg overflow-hidden flex flex-col shadow-2xl"
              >
                <div className="p-8 border-b border-white/5 flex items-center justify-between bg-gradient-to-r from-blue-600/10 to-purple-600/10">
                  <div className="flex items-center gap-4">
                    <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                      <Zap className="w-6 h-6" />
                    </div>
                    <div>
                      <h2 className="text-xl font-bold text-white uppercase tracking-tight">Доступно обновление</h2>
                      <p className="text-xs text-slate-400">Новая версия: <span className="text-blue-400 font-bold">{updateInfo?.latest}</span></p>
                    </div>
                  </div>
                </div>
                
                <div className="p-8 space-y-6">
                  <div className="space-y-3">
                    <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Что нового:</h4>
                    <ul className="space-y-2">
                      {updateInfo?.changelog?.map((item: string, i: number) => (
                        <li key={i} className="flex items-start gap-3 text-sm text-slate-300">
                          <div className="w-1.5 h-1.5 rounded-full bg-blue-500 mt-1.5 flex-shrink-0" />
                          {item}
                        </li>
                      ))}
                    </ul>
                  </div>

                  {isUpdating ? (
                    <div className="space-y-4 py-4">
                      <div className="flex items-center justify-between text-[10px] font-bold uppercase tracking-widest text-blue-400">
                        <span>Загрузка и установка...</span>
                        <span>{updateProgress}%</span>
                      </div>
                      <div className="h-2 bg-white/5 rounded-full overflow-hidden">
                        <motion.div 
                          className="h-full bg-blue-600 shadow-[0_0_15px_rgba(59,130,246,0.5)]"
                          initial={{ width: 0 }}
                          animate={{ width: `${updateProgress}%` }}
                        />
                      </div>
                    </div>
                  ) : (
                    <div className="flex gap-4 pt-4">
                      <button 
                        onClick={() => setShowUpdateModal(false)}
                        className="flex-1 px-6 py-4 bg-white/5 hover:bg-white/10 text-white rounded-2xl text-xs font-bold uppercase tracking-widest transition-all border border-white/10"
                      >
                        Позже
                      </button>
                      <button 
                        onClick={applyUpdate}
                        className="flex-1 px-6 py-4 bg-blue-600 hover:bg-blue-500 text-white rounded-2xl text-xs font-bold uppercase tracking-widest transition-all shadow-lg shadow-blue-600/20"
                      >
                        Обновить сейчас
                      </button>
                    </div>
                  )}
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Content Area */}
        <div className="flex-1 overflow-hidden flex flex-col">
          {activeTab === 'chat' ? (
            <>
              {/* Chat Header */}
              <div className="px-6 py-4 border-b border-white/5 flex items-center justify-between bg-black/20">
                <div className="flex items-center gap-3">
                  <div className={`w-2 h-2 rounded-full ${isOnline ? 'bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.5)]' : 'bg-red-500'}`} />
                  <span className="text-[10px] font-bold text-white uppercase tracking-widest">
                    {isOnline ? 'Online Mode' : 'Offline Mode'}
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <button 
                    onClick={handleClearChat}
                    disabled={isClearingChat}
                    className="p-2 hover:bg-white/5 rounded-lg text-slate-500 hover:text-red-400 transition-all flex items-center gap-2 disabled:opacity-50"
                    title="Очистить историю чата"
                  >
                    <Trash2 className={`w-4 h-4 ${isClearingChat ? 'animate-spin' : ''}`} />
                    <span className="text-[9px] font-bold uppercase hidden sm:inline">
                      {isClearingChat ? 'Очистка...' : 'Очистить'}
                    </span>
                  </button>
                  <button 
                    onClick={() => setShowSettings(true)}
                    className="p-2 hover:bg-white/5 rounded-lg text-slate-500 hover:text-white transition-all"
                  >
                    <Settings className="w-4 h-4" />
                  </button>
                </div>
              </div>
              {/* Messages */}
        <div className="flex-1 overflow-y-auto p-6 space-y-8 scrollbar-thin scrollbar-thumb-white/5">
          {messages.length === 0 && (
            <div className="flex-1 flex flex-col items-center justify-center text-center max-w-2xl mx-auto py-10">
              <motion.div 
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                className="w-24 h-24 bg-blue-600/10 rounded-[3rem] flex items-center justify-center mb-10 border border-blue-500/20 shadow-2xl shadow-blue-600/10"
              >
                <Cpu className="w-12 h-12 text-blue-500" />
              </motion.div>
              
              <h2 className="text-2xl font-bold text-white mb-4 uppercase tracking-tight">Unity AI Assistant v17.12.0</h2>
              <p className="text-slate-400 text-sm leading-relaxed mb-10 max-w-lg px-4">
                Я полностью осведомлен о вашем проекте по пути <br/>
                <code className="text-blue-400 break-all bg-white/5 px-2 py-1 rounded mt-2 inline-block">
                  {kb?.project_path || 'Загрузка...'}
                </code>. 
                <br/><br/>
                Задавайте любые вопросы по Unity, Blender или Photoshop на русском языке. Модули продвинутого ИИ для RTS и Turn-Based стратегий, генерации обложек ВК и проект 'Континент судьбы' (v17.12.0) активированы.
              </p>

              {/* Cards removed as per user request */}
            </div>
          )}

          {messages.map((msg, i) => (
            <motion.div 
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              key={i} 
              className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
            >
              <div className={`max-w-[90%] group relative ${msg.role === 'user' ? 'bg-blue-600 text-white rounded-2xl rounded-tr-none px-5 py-3 shadow-lg shadow-blue-600/10' : 'w-full'}`}>
                {msg.role === 'assistant' && (
                  <div className="flex gap-5">
                    <div className="w-10 h-10 rounded-xl bg-white/5 border border-white/10 flex items-center justify-center flex-shrink-0 mt-1">
                      <Cpu className="w-5 h-5 text-blue-400" />
                    </div>
                    <div className="flex-1 space-y-4">
                      <div className="flex items-center justify-between">
                        <span className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">
                          AI Assistant
                        </span>
                        <button 
                          onClick={() => copyToClipboard(msg.content, `msg-${i}`)}
                          className="p-1.5 hover:bg-white/5 rounded-md text-slate-500 hover:text-white transition-all opacity-0 group-hover:opacity-100"
                        >
                          {copiedId === `msg-${i}` ? <Check className="w-3 h-3 text-green-500" /> : <Copy className="w-3 h-3" />}
                        </button>
                      </div>
                      <div className="markdown-body prose prose-invert prose-sm max-w-none text-slate-300 leading-relaxed">
                        <Markdown>{msg.content}</Markdown>
                      </div>

                      {msg.audioVariants && (
                        <div className="mt-6 space-y-4 pt-6 border-t border-white/5">
                          <h4 className="text-[10px] font-bold text-white uppercase tracking-widest flex items-center gap-2">
                            <Music className="w-3 h-3 text-blue-400" /> Сгенерированные аудио-варианты (v16.99.0):
                          </h4>
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                            {msg.audioVariants.map((variant, vi) => (
                              <div key={vi} className="p-4 rounded-2xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all space-y-3">
                                <div className="flex items-center justify-between">
                                  <span className="text-[10px] font-bold text-slate-400 uppercase truncate pr-2">{variant.name}</span>
                                  <a 
                                    href={variant.url} 
                                    download={`${variant.name}.mp3`}
                                    className="p-1.5 bg-blue-600/20 hover:bg-blue-600/40 rounded-lg text-blue-400 transition-all flex-shrink-0"
                                    title="Скачать MP3"
                                  >
                                    <Download className="w-3 h-3" />
                                  </a>
                                </div>
                                <audio controls className="w-full h-8 accent-blue-500">
                                  <source src={variant.url} type="audio/mpeg" />
                                  Ваш браузер не поддерживает аудио.
                                </audio>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                )}
                {msg.role === 'user' && (
                  <div className="space-y-3">
                    <div className="text-sm font-medium leading-relaxed">{msg.content}</div>
                    {msg.files && (
                      <div className="flex flex-wrap gap-2">
                        {msg.files.map((f, fi) => (
                          <div key={fi} className="flex items-center gap-2 bg-black/20 px-3 py-1.5 rounded-lg border border-white/10 text-[9px] font-bold uppercase">
                            {f.type.includes('image') ? <ImageIcon className="w-3 h-3 text-blue-400" /> : 
                             f.type.includes('video') ? <Video className="w-3 h-3 text-purple-400" /> : 
                             f.type.includes('audio') ? <Music className="w-3 h-3 text-green-400" /> : 
                             <FileText className="w-3 h-3 text-slate-400" />}
                            {f.name} ({(f.size / 1024 / 1024).toFixed(1)}MB)
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </motion.div>
          ))}
          
          {isTyping && (
            <div className="flex gap-5">
              <div className="w-10 h-10 rounded-xl bg-white/5 border border-white/10 flex items-center justify-center">
                <motion.div
                  animate={{ scale: [1, 1.2, 1] }}
                  transition={{ duration: 1, repeat: Infinity }}
                >
                  <Zap className="w-5 h-5 text-blue-500" />
                </motion.div>
              </div>
              <div className="flex gap-1.5 items-center py-4">
                <motion.div animate={{ opacity: [0.3, 1, 0.3] }} transition={{ duration: 1, repeat: Infinity, delay: 0 }} className="w-1.5 h-1.5 bg-slate-500 rounded-full" />
                <motion.div animate={{ opacity: [0.3, 1, 0.3] }} transition={{ duration: 1, repeat: Infinity, delay: 0.2 }} className="w-1.5 h-1.5 bg-slate-500 rounded-full" />
                <motion.div animate={{ opacity: [0.3, 1, 0.3] }} transition={{ duration: 1, repeat: Infinity, delay: 0.4 }} className="w-1.5 h-1.5 bg-slate-500 rounded-full" />
              </div>
            </div>
          )}
          <div ref={chatEndRef} />
        </div>

        {/* Upload Progress Overlay */}
        {/* Notifications */}
        <AnimatePresence>
          {notification && (
            <motion.div 
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 50 }}
              className={`fixed top-8 right-8 z-[200] p-4 rounded-2xl shadow-2xl border flex items-center gap-3 min-w-[300px] ${
                notification.type === 'success' ? 'bg-green-600/10 border-green-500/20 text-green-400' :
                notification.type === 'error' ? 'bg-red-600/10 border-red-500/20 text-red-400' :
                'bg-blue-600/10 border-blue-500/20 text-blue-400'
              }`}
            >
              {notification.type === 'success' ? <Check className="w-5 h-5" /> : 
               notification.type === 'error' ? <AlertTriangle className="w-5 h-5" /> : 
               <Info className="w-5 h-5" />}
              <span className="text-xs font-bold uppercase tracking-tight">{notification.message}</span>
            </motion.div>
          )}
        </AnimatePresence>

        <AnimatePresence>
          {isUploading && (
            <motion.div 
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 20 }}
              className="absolute bottom-24 right-8 z-[150] bg-[#121214] border border-white/10 p-4 rounded-2xl shadow-2xl w-72"
            >
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <RefreshCw className="w-3 h-3 text-blue-400 animate-spin" />
                  <div className="flex flex-col">
                    <span className="text-[10px] font-bold text-white uppercase">Загрузка файлов...</span>
                    {uploadTimeRemaining && (
                      <span className="text-[8px] text-slate-500 uppercase">Осталось: {uploadTimeRemaining}</span>
                    )}
                  </div>
                </div>
                <span className="text-[10px] text-blue-400 font-mono font-bold">{uploadProgress}%</span>
              </div>
              <div className="h-1.5 bg-white/5 rounded-full overflow-hidden">
                <motion.div 
                  className="h-full bg-blue-600 shadow-[0_0_8px_rgba(59,130,246,0.5)]"
                  initial={{ width: 0 }}
                  animate={{ width: `${uploadProgress}%` }}
                />
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Input */}
        <div className="p-6 bg-gradient-to-t from-[#0a0a0c] via-[#0a0a0c] to-transparent relative">
          {suggestedQuestions.length > 0 && (
            <div className="absolute bottom-full left-0 right-0 p-4 flex gap-2 overflow-x-auto bg-black/60 backdrop-blur-md border-t border-white/5 z-50">
              {suggestedQuestions.map((q, i) => (
                <button
                  key={i}
                  onClick={() => handleSend(q)}
                  className="whitespace-nowrap px-4 py-2 rounded-full bg-blue-600/20 border border-blue-500/30 text-[10px] font-bold text-blue-400 hover:bg-blue-600 hover:text-white transition-all uppercase tracking-wider"
                >
                  {q}
                </button>
              ))}
            </div>
          )}
          <div className="max-w-4xl mx-auto relative">
            {/* Attached Files Queue */}
            <AnimatePresence>
              {attachedFiles.length > 0 && (
                <motion.div 
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: 10 }}
                  className="mb-4 flex flex-wrap gap-2"
                >
                  {attachedFiles.map((file, idx) => (
                    <div key={idx} className="group relative flex items-center gap-2 bg-blue-600/10 border border-blue-500/30 px-3 py-2 rounded-xl text-[10px] font-bold text-blue-400 uppercase">
                      {file.type.includes('image') ? <ImageIcon className="w-3.5 h-3.5" /> : <FileText className="w-3.5 h-3.5" />}
                      <span className="max-w-[120px] truncate">{file.name}</span>
                      <button 
                        onClick={() => setAttachedFiles(prev => prev.filter((_, i) => i !== idx))}
                        className="ml-1 p-1 hover:bg-red-500/20 hover:text-red-400 rounded-md transition-all"
                      >
                        <X className="w-3 h-3" />
                      </button>
                    </div>
                  ))}
                </motion.div>
              )}
            </AnimatePresence>

            <div className="absolute -top-12 left-0 right-0 flex justify-center pointer-events-none">
              <div className="px-4 py-1.5 bg-white/5 backdrop-blur-sm border border-white/5 rounded-full text-[9px] text-slate-500 uppercase tracking-widest flex items-center gap-2">
                <Terminal className="w-3 h-3" /> Нажмите Enter, чтобы отправить сообщение
              </div>
            </div>
            <div className="relative group flex gap-3">
              <button 
                onClick={() => fileInputRef.current?.click()}
                className="p-4 bg-white/5 border border-white/10 rounded-2xl text-slate-400 hover:text-white hover:border-white/20 transition-all"
              >
                <Paperclip className="w-5 h-5" />
              </button>
              <input 
                type="file" 
                ref={fileInputRef}
                onChange={handleFileUpload}
                multiple
                className="hidden"
              />
              <div className="relative flex-1">
                <textarea
                  value={input}
                  onChange={(e) => setInput(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && !e.shiftKey) {
                      e.preventDefault();
                      handleSend();
                    }
                  }}
                  placeholder="Задайте вопрос по Unity или Blender..."
                  className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-5 pr-16 text-sm text-white placeholder:text-slate-600 focus:outline-none focus:border-blue-500/50 focus:bg-white/[0.07] transition-all resize-none h-18 scrollbar-none"
                />
                <button 
                  onClick={() => handleSend()}
                  disabled={(!input.trim() && attachedFiles.length === 0) || isTyping}
                  className={`absolute right-4 top-4 p-3 rounded-xl transition-all ${
                    (input.trim() || attachedFiles.length > 0) && !isTyping 
                    ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20 hover:scale-105 active:scale-95' 
                    : 'bg-white/5 text-slate-600'
                  }`}
                >
                  <Send className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
          <p className="text-center text-[9px] text-slate-600 mt-5 uppercase tracking-widest">
            AI может ошибаться. Проверяйте код перед использованием в проекте.
          </p>
        </div>
            </>
          ) : activeTab === 'migration' ? (
            <div className="flex-1 overflow-y-auto p-8 scrollbar-thin scrollbar-thumb-white/5">
              <div className="max-w-4xl mx-auto space-y-8">
                <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-orange-600/10 to-red-600/10 border border-orange-500/20">
                  <div className="flex items-center gap-6 mb-6">
                    <div className="w-16 h-16 bg-orange-600 rounded-3xl flex items-center justify-center shadow-2xl shadow-orange-600/40">
                      <GitBranch className="w-8 h-8 text-white" />
                    </div>
                    <div>
                      <h2 className="text-2xl font-bold text-white uppercase tracking-tighter">Помощник миграции (Unity → Godot/Redot)</h2>
                      <p className="text-sm text-slate-400">Инструменты и справочники для переноса ваших проектов на открытые движки.</p>
                    </div>
                  </div>
                  <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-slate-300 leading-relaxed">
                    {migrationData?.message}
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                    <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                      <Code className="w-4 h-4 text-blue-400" /> Карта соответствий API
                    </h3>
                    <div className="space-y-2">
                      {Object.entries(migrationData?.mapping || {}).map(([unity, godot]: [any, any]) => (
                        <div key={unity} className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/5 group hover:bg-white/10 transition-all">
                          <span className="text-[10px] font-mono text-blue-400">{unity}</span>
                          <ChevronRight className="w-3 h-3 text-slate-600" />
                          <span className="text-[10px] font-mono text-orange-400">{godot}</span>
                        </div>
                      ))}
                    </div>
                  </section>

                  <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                    <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                      <Zap className="w-4 h-4 text-yellow-400" /> Советы по конвертации
                    </h3>
                    <div className="space-y-4">
                      {migrationData?.tips.map((tip: string, i: number) => (
                        <div key={i} className="p-4 rounded-2xl bg-white/5 border border-white/5 text-xs text-slate-300 leading-relaxed">
                          {tip}
                        </div>
                      ))}
                    </div>
                  </section>
                </div>

                <div className="p-8 rounded-[2.5rem] bg-blue-600/5 border border-blue-500/10">
                  <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-4">Автоматизированный перенос (Experimental)</h3>
                  <p className="text-xs text-slate-400 mb-6">
                    Мы работаем над скриптом, который сможет автоматически конвертировать структуру сцены (.unity → .tscn) и базовые C# скрипты. 
                    На данный момент рекомендуется использовать ручной перенос с помощью карты соответствий выше.
                  </p>
                  <button 
                    onClick={() => showNotification("Функция автоматического переноса находится в разработке.", "info")}
                    className="px-6 py-3 bg-blue-600/20 border border-blue-500/30 rounded-xl text-[10px] font-bold text-blue-400 uppercase tracking-widest hover:bg-blue-600 hover:text-white transition-all"
                  >
                    Запустить анализ проекта
                  </button>
                </div>
              </div>
            </div>
          ) : activeTab === 'game_design' ? (
            <div className="flex-1 overflow-y-auto p-8 space-y-8 bg-black/20 scrollbar-thin scrollbar-thumb-white/5">
              <div className="max-w-7xl mx-auto space-y-8">
                {/* Game Studio Header */}
                <div className="flex flex-col md:flex-row items-center justify-between gap-6">
                  <div className="flex items-center gap-5">
                    <div className="p-5 bg-gradient-to-br from-purple-600/30 to-blue-600/30 rounded-[2rem] border border-purple-500/30 shadow-2xl shadow-purple-600/20">
                      <Gamepad2 className="w-12 h-12 text-purple-400" />
                    </div>
                    <div>
                      <div className="flex items-center gap-3">
                        <h2 className="text-4xl font-black text-white uppercase tracking-tighter italic">
                          {gameDesign?.game_title || 'Континент судьбы'}
                        </h2>
                        <span className="px-3 py-1 rounded-full bg-purple-600/20 border border-purple-500/30 text-[10px] font-bold text-purple-400 uppercase tracking-widest">v{gameDesign?.version || '1.2.0'}</span>
                      </div>
                      <p className="text-xs text-slate-500 uppercase tracking-[0.3em] font-black mt-2 flex items-center gap-2">
                        <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                        Центральный штаб разработки • {gameDesign?.style || 'High Fantasy'}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <button 
                      onClick={() => showNotification("Генерация GDD документа...", "info")}
                      className="px-6 py-4 rounded-2xl bg-white/5 border border-white/10 text-white hover:bg-white/10 transition-all flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest"
                    >
                      <Download className="w-4 h-4" /> Экспорт GDD
                    </button>
                    <button 
                      onClick={handleSaveGameDesign}
                      disabled={isSavingGameDesign}
                      className={`px-10 py-4 rounded-2xl flex items-center gap-3 transition-all font-black uppercase text-[11px] tracking-widest ${
                        isSavingGameDesign ? 'bg-slate-800 text-slate-500' : 'bg-purple-600 hover:bg-purple-500 text-white shadow-2xl shadow-purple-900/40 active:scale-95'
                      }`}
                    >
                      {isSavingGameDesign ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                      {isSavingGameDesign ? 'Синхронизация...' : 'Сохранить Изменения'}
                    </button>
                  </div>
                </div>

                {/* Sub-tabs for Game Design */}
                <div className="flex items-center gap-2 p-1.5 bg-black/40 rounded-2xl border border-white/5 w-fit">
                  {['World', 'Castle System', 'Heroes & Units', 'Visuals & Nav', 'Abilities', 'Synergies', 'Balancing & Rarity', 'Economy'].map((tab) => (
                    <button
                      key={tab}
                      onClick={() => setDesignSubTab(tab as any)}
                      className={`px-8 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${
                        designSubTab === tab ? 'bg-purple-600 text-white shadow-lg' : 'text-slate-500 hover:text-white'
                      }`}
                    >
                      {tab}
                    </button>
                  ))}
                </div>

                {designSubTab === 'World' ? (
                  <motion.div 
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="grid grid-cols-1 lg:grid-cols-2 gap-8"
                  >
                    <div className="space-y-8">
                       <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">1. География Континентов</h3>
                       <div className="grid grid-cols-1 gap-6">
                         {gameDesign?.continents?.map((cont: any, i: number) => (
                           <div key={i} className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 hover:border-purple-500/40 transition-all group relative overflow-hidden">
                             <div className="absolute top-0 right-0 p-8 opacity-5 group-hover:scale-110 transition-transform">
                               <MapIcon className="w-24 h-24 text-white" />
                             </div>
                             <div className="relative z-10 space-y-6">
                               <div className="flex items-center justify-between">
                                 <div className="flex items-center gap-4">
                                   <div className="w-12 h-12 rounded-2xl bg-purple-600 text-white flex items-center justify-center font-black italic text-xl">0{i+1}</div>
                                   <input 
                                     value={cont.name}
                                     onChange={(e) => {
                                        const newConts = [...gameDesign.continents];
                                        newConts[i].name = e.target.value;
                                        setGameDesign({...gameDesign, continents: newConts});
                                     }}
                                     className="bg-transparent border-none text-2xl font-black text-white focus:outline-none uppercase tracking-tighter w-full"
                                   />
                                 </div>
                                 {cont.visuals && (
                                   <div className="flex items-center gap-2">
                                     <span className="px-2 py-1 rounded-lg bg-white/5 border border-white/10 text-[8px] font-black uppercase text-slate-400">
                                       {cont.visuals.main_color}
                                     </span>
                                     <span className="px-2 py-1 rounded-lg bg-purple-600/20 border border-purple-500/30 text-[8px] font-black uppercase text-purple-400">
                                       {cont.visuals.hero_icon}
                                     </span>
                                   </div>
                                 )}
                               </div>

                               <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                 {cont.environment && (
                                   <div className="space-y-3">
                                     <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest px-1">Окружение</h5>
                                     <div className="space-y-1">
                                       {cont.environment.map((item: string, idx: number) => (
                                         <div key={idx} className="flex items-start gap-2 text-[9px] text-slate-400 leading-tight">
                                           <div className="w-1 h-1 rounded-full bg-purple-500 mt-1 flex-shrink-0" />
                                           {item}
                                         </div>
                                       ))}
                                     </div>
                                   </div>
                                 )}

                                 <div className="space-y-3">
                                   <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest px-1">Фракции</h5>
                                   {cont.factions ? (
                                     <div className="grid grid-cols-1 gap-3">
                                       {cont.factions.map((f: any, fi: number) => (
                                         <div key={fi} className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                           <div className="flex items-center justify-between">
                                             <input 
                                               value={f.name}
                                               onChange={(e) => {
                                                  const newConts = [...gameDesign.continents];
                                                  newConts[i].factions[fi].name = e.target.value;
                                                  setGameDesign({...gameDesign, continents: newConts});
                                               }}
                                               className="bg-transparent border-none text-[10px] font-black text-purple-400 focus:outline-none uppercase tracking-widest"
                                             />
                                             <Users className="w-3 h-3 text-slate-700" />
                                           </div>
                                           <textarea 
                                             value={f.locations}
                                             onChange={(e) => {
                                                const newConts = [...gameDesign.continents];
                                                newConts[i].factions[fi].locations = e.target.value;
                                                setGameDesign({...gameDesign, continents: newConts});
                                             }}
                                             className="w-full bg-transparent border-none text-[9px] text-slate-500 focus:outline-none resize-none h-10 leading-relaxed"
                                           />
                                         </div>
                                       ))}
                                     </div>
                                   ) : cont.structure ? (
                                      <div className="p-4 bg-blue-600/5 border border-blue-500/20 rounded-2xl space-y-3">
                                        {Object.entries(cont.structure).map(([key, val]: any, si: number) => (
                                          <div key={si} className="flex items-center justify-between text-[9px]">
                                            <span className="text-slate-500 uppercase">{key}:</span>
                                            <span className="text-slate-300 font-bold">{val}</span>
                                          </div>
                                        ))}
                                      </div>
                                   ) : null}
                                 </div>
                               </div>
                ) : designSubTab === 'Visuals & Nav' ? (
                  <motion.div 
                    initial={{ opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="grid grid-cols-1 lg:grid-cols-2 gap-8"
                  >
                    <div className="space-y-8">
                      <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8 relative overflow-hidden">
                        <div className="absolute -top-12 -right-12 w-64 h-64 bg-green-600/5 rounded-full blur-[80px]" />
                        <h4 className="text-[10px] font-black text-green-400 uppercase tracking-[0.4em] mb-4">Выделение клеток</h4>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                          {Object.entries(gameDesign?.visual_system?.cell_highlight || {}).map(([key, val]: any) => (
                            <div key={key} className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-2">
                              <div className="text-[9px] text-slate-500 uppercase font-black">{key}</div>
                              <div className="text-sm text-white font-medium">{val}</div>
                            </div>
                          ))}
                        </div>
                        <div className="p-6 bg-white/5 rounded-3xl space-y-4">
                          <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest">Система подсказок (Зум)</h5>
                          <div className="space-y-3">
                            {Object.entries(gameDesign?.visual_system?.scaling_hints || {}).map(([key, val]: any) => (
                              <div key={key} className="flex items-center justify-between text-[11px]">
                                <span className="text-slate-500 uppercase">{key}:</span>
                                <span className="text-blue-400 font-bold italic">{val}</span>
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-8">
                       <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                         <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-[0.4em]">Механика Камеры</h4>
                         <div className="space-y-6">
                           <div className="grid grid-cols-1 gap-3">
                             <div className="text-[10px] text-slate-500 uppercase font-black">Уровни масштаба</div>
                             {Object.entries(gameDesign?.camera_mechanics?.zoom_levels || {}).map(([key, val]: any) => (
                               <div key={key} className="p-4 bg-white/5 rounded-2xl border border-white/5 flex items-center justify-between">
                                  <span className="text-[10px] text-slate-400 uppercase">{key}</span>
                                  <span className="text-[11px] text-white font-medium italic text-right">{val}</span>
                               </div>
                             ))}
                           </div>
                           <div className="p-6 bg-purple-600/5 rounded-3xl border border-purple-500/20 space-y-4">
                              <div className="flex items-center gap-3 text-purple-400 font-bold text-[10px] uppercase tracking-widest">
                                <RefreshCw className="w-4 h-4" /> Вращение
                              </div>
                              <div className="text-[11px] text-slate-300">
                                {gameDesign?.camera_mechanics?.rotation?.free}. Фиксированные углы: {gameDesign?.camera_mechanics?.rotation?.fixed?.join('°, ')}°. {gameDesign?.camera_mechanics?.rotation?.auto}.
                              </div>
                           </div>
                           <div className="grid grid-cols-2 gap-4">
                             <div className="p-6 bg-white/5 rounded-3xl border border-white/5 space-y-2">
                               <div className="text-[9px] text-slate-500 uppercase font-black">Интерфейс</div>
                               <div className="text-xs text-white">{gameDesign?.camera_mechanics?.ui?.join(', ')}</div>
                             </div>
                             <div className="p-6 bg-white/5 rounded-3xl border border-white/5 space-y-2">
                               <div className="text-[9px] text-slate-500 uppercase font-black">Анимация</div>
                               <div className="text-xs text-white">{gameDesign?.camera_mechanics?.animations?.join(', ')}</div>
                             </div>
                           </div>
                         </div>
                       </div>
                    </div>
                  </motion.div>
                             </div>
                           </div>
                         ))}
                       </div>
                    </div>

                    <div className="space-y-8">
                       <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">2. Глобальный Лор</h3>
                       <div className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 space-y-6">
                         <div className="flex items-center gap-4">
                           <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                             <Sparkles className="w-5 h-5" />
                           </div>
                           <h4 className="text-sm font-black text-white uppercase tracking-widest">Манифест Концепции</h4>
                         </div>
                         <textarea 
                           value={gameDesign?.core_concept || ''}
                           onChange={(e) => setGameDesign({...gameDesign, core_concept: e.target.value})}
                           className="w-full bg-black/40 border border-white/5 rounded-3xl p-6 text-sm text-slate-300 focus:outline-none focus:border-purple-500/40 min-h-[250px] transition-all leading-relaxed resize-none"
                           placeholder="Напишите историю мира Континент Судьбы..."
                         />
                         <div className="p-6 rounded-2xl bg-purple-600/5 border border-purple-500/20">
                            <p className="text-[10px] text-purple-300/60 leading-relaxed italic">
                              "Мир, где культивация силы — единственный путь к вершине. Четыре континента, десятки рас и тысячи лет войны за Эфирные Источники."
                            </p>
                         </div>
                       </div>

                       <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6">
                         <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Атрибуты Визуального Стиля</h4>
                         <div className="flex flex-wrap gap-2">
                           {['Китайское фэнтези', 'Xianxia', 'Руническая магия', 'Парящие горы', 'Эфирный свет', 'Древние секты'].map((tag, i) => (
                             <span key={i} className="px-3 py-1.5 rounded-xl bg-white/5 border border-white/10 text-[9px] text-slate-400 uppercase tracking-widest font-bold">
                               {tag}
                             </span>
                           ))}
                         </div>
                       </div>
                    </div>
                  </motion.div>
                ) : designSubTab === 'Castle System' ? (
                  <motion.div 
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="space-y-12"
                  >
                    <div className="grid grid-cols-1 gap-12">
                      {gameDesign?.continents?.map((cont: any, i: number) => (
                        <div key={i} className="space-y-6">
                           <div className="flex items-center gap-4 px-4">
                             <div className="w-8 h-8 rounded-lg bg-purple-600 text-white flex items-center justify-center font-black italic text-xs leading-none">0{i+1}</div>
                             <h4 className="text-lg font-black text-white uppercase tracking-tighter italic">{cont.name}: Путь Развития Замка</h4>
                           </div>
                           <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
                             {cont.castles?.map((lvl: any) => (
                               <div key={lvl.level} className="p-6 rounded-[2rem] bg-white/5 border border-white/5 hover:border-purple-500/40 transition-all group relative overflow-hidden">
                                 <div className="text-[9px] font-black text-slate-700 uppercase mb-3 tracking-widest flex items-center justify-between">
                                   <span>Level {lvl.level}</span>
                                   {lvl.special && <Zap className="w-3 h-3 text-yellow-500" />}
                                 </div>
                                 <h5 className="text-xs font-black text-white uppercase mb-4 tracking-tighter leading-tight min-h-[2rem]">{lvl.name}</h5>
                                 <div className="space-y-3">
                                   <div className="p-3 rounded-xl bg-black/40 border border-white/5 space-y-1.5">
                                     <div className="text-[8px] text-slate-600 uppercase font-black">Внешний вид</div>
                                     <div className="text-[10px] text-slate-400 leading-tight h-10 overflow-y-auto scrollbar-none">{lvl.appearance}</div>
                                   </div>
                                   <div className="space-y-2">
                                     {lvl.units && (
                                       <div className="flex items-center justify-between text-[9px]">
                                         <span className="text-slate-500 uppercase">Войска:</span>
                                         <span className="text-blue-400 font-bold text-right">{lvl.units}</span>
                                       </div>
                                     )}
                                     {lvl.income && (
                                       <div className="flex items-center justify-between text-[9px]">
                                         <span className="text-slate-500 uppercase">Доход:</span>
                                         <span className="text-yellow-500 font-bold">{lvl.income}</span>
                                       </div>
                                     )}
                                   </div>
                                   {lvl.bonuses && (
                                     <div className="pt-2 border-t border-white/5 italic text-[9px] text-purple-400 line-clamp-2 leading-relaxed h-8">
                                       {lvl.bonuses}
                                     </div>
                                   )}
                                   {lvl.special && (
                                     <div className="px-2 py-1 bg-yellow-500/10 border border-yellow-500/30 rounded-lg text-[8px] font-bold text-yellow-500 uppercase text-center mt-2 truncate">
                                       {lvl.special}
                                     </div>
                                   )}
                                 </div>
                               </div>
                             ))}
                           </div>
                        </div>
                      ))}
                    </div>
                  </motion.div>
                ) : designSubTab === 'Heroes & Units' ? (
                   <motion.div 
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="grid grid-cols-1 lg:grid-cols-2 gap-8"
                  >
                    <div className="space-y-8">
                       <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Основные Классы</h3>
                       <div className="grid grid-cols-1 gap-4">
                         {gameDesign?.hero_classes?.main_heroes?.map((h: any, i: number) => (
                            <div key={i} className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 hover:border-purple-500/40 transition-all group">
                              <div className="flex items-center justify-between mb-8">
                                <div className="flex items-center gap-4">
                                  <div className="p-4 bg-purple-600 rounded-2xl text-white">
                                    {h.class === 'Воин' ? <Shield className="w-6 h-6" /> : 
                                     h.class === 'Лучник' ? <Target className="w-6 h-6" /> : <Zap className="w-6 h-6" />}
                                  </div>
                                  <div>
                                    <h4 className="text-xl font-black text-white uppercase tracking-tighter italic">{h.class}</h4>
                                    <span className="text-[9px] text-purple-400 font-black uppercase tracking-widest">{h.bonus}</span>
                                  </div>
                                </div>
                              </div>
                              <div className="grid grid-cols-3 gap-4 mb-6">
                                <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                   <div className="text-[8px] text-slate-500 uppercase font-black mb-1">HP</div>
                                   <div className="text-lg font-black text-red-500">{h.hp}</div>
                                </div>
                                <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                   <div className="text-[8px] text-slate-500 uppercase font-black mb-1">ATK</div>
                                   <div className="text-lg font-black text-orange-500">{h.atk}</div>
                                </div>
                                <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                   <div className="text-[8px] text-slate-500 uppercase font-black mb-1">DEF</div>
                                   <div className="text-lg font-black text-blue-500">{h.def}</div>
                                </div>
                              </div>
                              <div className="flex items-center gap-6 text-[10px] text-slate-500 uppercase font-black tracking-widest px-2">
                                 <span className="flex items-center gap-2"><ArrowRight className="w-3 h-3"/> SPEED: {h.speed}</span>
                                 <span className="flex items-center gap-2"><ArrowRight className="w-3 h-3"/> RANGE: {h.range}</span>
                              </div>
                            </div>
                         ))}
                       </div>
                    </div>

                    <div className="space-y-8">
                       <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Под-герои (Отряд)</h3>
                       <div className="grid grid-cols-1 gap-4">
                         {gameDesign?.hero_classes?.sub_heroes?.map((h: any, i: number) => (
                            <div key={i} className="p-6 rounded-3xl bg-black/40 border border-white/5 flex items-center justify-between">
                               <div className="flex items-center gap-4">
                                  <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center text-slate-500">
                                    {h.class === 'Воин' ? <Shield className="w-4 h-4" /> : 
                                     h.class === 'Лучник' ? <Target className="w-4 h-4" /> : <Zap className="w-4 h-4" />}
                                  </div>
                                  <div>
                                    <h5 className="text-[11px] font-black text-white uppercase tracking-widest">{h.class} (Sub)</h5>
                                    <p className="text-[9px] text-slate-600 font-mono italic">{h.skill}</p>
                                  </div>
                               </div>
                               <div className="text-right">
                                  <div className="text-[10px] font-black text-red-500">{h.hp} HP</div>
                                  <div className="text-[10px] font-black text-orange-500">{h.atk} ATK</div>
                               </div>
                            </div>
                         ))}
                       </div>

                       <div className="p-8 rounded-[2.5rem] bg-gradient-to-r from-blue-600/10 to-transparent border border-blue-500/20 space-y-6">
                         <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-widest">Система Грузоподъёмности</h4>
                         <div className="space-y-4">
                           <div className="grid grid-cols-1 gap-2">
                             {[
                               { label: "Простой Герой (L1)", value: "600 КГ", sub: "+500кг каждые 100 ур." },
                               { label: "Главный Герой (L1)", value: "1200 КГ", sub: "+1000кг каждые 100 ур." }
                             ].map((b, i) => (
                               <div key={i} className="flex hover:bg-white/5 p-2 rounded-xl transition-colors items-center justify-between">
                                 <div>
                                   <div className="text-[10px] text-white font-black uppercase">{b.label}</div>
                                   <div className="text-[8px] text-slate-500 uppercase">{b.sub}</div>
                                 </div>
                                 <div className="text-[11px] font-black text-blue-400">{b.value}</div>
                               </div>
                             ))}
                           </div>
                         </div>
                       </div>
                    </div>
                  </motion.div>
                ) : designSubTab === 'Abilities' ? (
                  <motion.div 
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="space-y-12"
                  >
                    <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
                       <div className="xl:col-span-1 space-y-6">
                         <div className="flex items-center justify-between px-4">
                           <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">Простые Герои (L1-5000)</h3>
                           <div className="px-2 py-1 bg-white/5 rounded-lg text-[9px] text-slate-400 font-bold">1-10 LVL</div>
                         </div>
                         <div className="space-y-4">
                            {Object.entries(gameDesign?.ability_system?.simple || {}).map(([cls, skills]: any) => (
                              <div key={cls} className="p-6 rounded-[2rem] bg-black/40 border border-white/10 space-y-4">
                                <h4 className="text-sm font-black text-white uppercase italic">{cls}</h4>
                                <div className="space-y-2">
                                  {skills.map((s: any, i: number) => (
                                    <div key={i} className="p-3 bg-white/5 rounded-xl border border-white/5 group hover:border-purple-500/30 transition-all">
                                      <div className="flex items-center justify-between mb-1">
                                        <span className="text-[10px] font-black text-white uppercase truncate pr-2">{s.name}</span>
                                        <span className={`text-[8px] font-black uppercase ${s.type === 'active' ? 'text-orange-400' : 'text-blue-400'}`}>{s.type}</span>
                                      </div>
                                      <div className="flex justify-between text-[9px] text-slate-500 italic">
                                        <span>L1: {s.lvl1}</span>
                                        <span>L10: {s.lvl10}</span>
                                      </div>
                                      {s.cd && <div className="mt-1 text-[8px] text-yellow-500/60 font-mono">Откат: {s.cd}х</div>}
                                    </div>
                                  ))}
                                </div>
                              </div>
                            ))}
                         </div>
                       </div>

                       <div className="xl:col-span-1 space-y-6">
                         <div className="flex items-center justify-between px-4">
                           <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">Главные Герои (L1-5000)</h3>
                           <div className="px-2 py-1 bg-purple-600/20 rounded-lg text-[9px] text-purple-400 font-bold">1-20 LVL</div>
                         </div>
                         <div className="space-y-4">
                            {Object.entries(gameDesign?.ability_system?.main || {}).map(([cls, skills]: any) => (
                              <div key={cls} className="p-6 rounded-[2rem] bg-purple-600/5 border border-purple-500/20 space-y-4">
                                <h4 className="text-sm font-black text-white uppercase italic">{cls}</h4>
                                <div className="space-y-2">
                                  {skills.map((s: any, i: number) => (
                                    <div key={i} className={`p-3 rounded-xl border transition-all ${s.type === 'heroic' ? 'bg-purple-600/20 border-purple-500/40' : 'bg-black/60 border-white/5'}`}>
                                      <div className="flex items-center justify-between mb-1">
                                        <span className="text-[10px] font-black text-white uppercase">{s.name}</span>
                                        <span className={`text-[8px] font-black uppercase ${s.type === 'heroic' ? 'text-yellow-400 animate-pulse' : 'text-purple-400'}`}>{s.type}</span>
                                      </div>
                                      <p className="text-[9px] text-slate-400 leading-snug">{s.lvl20 || s.lvl10}</p>
                                      {s.cd && <div className="mt-1 text-[8px] text-yellow-500/60 font-mono">Откат: {s.cd}х</div>}
                                    </div>
                                  ))}
                                </div>
                              </div>
                            ))}
                         </div>
                       </div>

                       <div className="xl:col-span-1 space-y-8">
                         <div className="space-y-6">
                           <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Система Откатов</h3>
                           <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6">
                             <div className="grid grid-cols-1 gap-4">
                               {gameDesign?.cooldown_system?.modifiers && Object.entries(gameDesign.cooldown_system.modifiers).map(([key, val]: any) => (
                                 <div key={key} className="space-y-2">
                                   <div className="text-[9px] text-slate-600 uppercase font-black px-1">{key === 'gear' ? 'Экипировка' : key === 'skills' ? 'Навыки' : key === 'locations' ? 'Локации' : 'Эффекты'}</div>
                                   <div className="p-3 bg-white/5 rounded-2xl border border-white/5 text-[10px] text-slate-300 italic">
                                     {val}
                                   </div>
                                 </div>
                               ))}
                             </div>
                             <div className="p-4 bg-yellow-500/5 border border-yellow-500/20 rounded-2xl">
                               <div className="flex items-center gap-2 mb-2">
                                 <Clock className="w-3 h-3 text-yellow-500" />
                                 <span className="text-[9px] font-black text-yellow-500 uppercase">Ограничения</span>
                               </div>
                               <ul className="text-[10px] text-slate-400 space-y-1 list-disc pl-4 italic">
                                 <li>Мин. откат обычных: 1 ход</li>
                                 <li>Мин. откат героич: 3 хода</li>
                                 <li>Округление всех значений вверх</li>
                               </ul>
                             </div>
                           </div>
                         </div>

                         <div className="space-y-6">
                           <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Механика Прокачки</h3>
                           <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-purple-600/10 to-transparent border border-purple-500/20 space-y-6">
                             <div className="space-y-4">
                               <div className="flex items-center justify-between text-[10px]">
                                 <span className="text-slate-400">Простые герои</span>
                                 <span className="text-white font-bold">+1 XP / использование</span>
                               </div>
                               <div className="flex items-center justify-between text-[10px]">
                                 <span className="text-slate-400">Главные герои</span>
                                 <span className="text-purple-400 font-bold">+2 XP / использование</span>
                               </div>
                               <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                                 <h5 className="text-[9px] font-black text-white uppercase mb-2">Визуальная эволюция</h5>
                                 <div className="space-y-2 text-[9px] text-slate-500 italic">
                                   <p>1-5 ур: Легкое мерцание иконки</p>
                                   <p>6-15 ур: Яркий свет + анимация</p>
                                   <p>16-20 ур: Эфирная аура вокруг</p>
                                 </div>
                               </div>
                             </div>
                           </div>
                         </div>
                       </div>
                    </div>
                  </motion.div>
                ) : designSubTab === 'Synergies' ? (
                  <motion.div 
                    initial={{ opacity: 0, scale: 0.98 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="space-y-12"
                  >
                    <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
                       {Object.entries(gameDesign?.skill_synergies?.classes || {}).map(([cls, data]: any) => (
                         <div key={cls} className="space-y-6">
                           <div className="flex items-center gap-4 px-4">
                             <div className={`p-3 rounded-2xl ${
                               cls === 'Warrior' ? 'bg-orange-600/20 text-orange-400' : 
                               cls === 'Mage' ? 'bg-purple-600/20 text-purple-400' : 'bg-green-600/20 text-green-400'
                             }`}>
                               {cls === 'Warrior' ? <Shield className="w-5 h-5" /> : 
                                cls === 'Mage' ? <Zap className="w-5 h-5" /> : <Target className="w-5 h-5" />}
                             </div>
                             <div>
                               <h3 className="text-lg font-black text-white uppercase italic">{cls === 'Warrior' ? 'Воин' : cls === 'Mage' ? 'Маг' : 'Стрелок'}</h3>
                               <span className="text-[9px] text-slate-500 uppercase font-black tracking-widest">Система Синергий</span>
                             </div>
                           </div>

                           <div className="space-y-4">
                             <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest px-4">Комбинации</h4>
                             <div className="space-y-3">
                               {data.synergies.map((syn: any, si: number) => (
                                 <div key={si} className="p-6 rounded-[2rem] bg-black/40 border border-white/10 space-y-4 group hover:border-white/20 transition-all">
                                   <div className="flex items-center gap-2 flex-wrap">
                                      {syn.skills.map((s: string, ski: number) => (
                                        <React.Fragment key={ski}>
                                          <span className="px-3 py-1.5 rounded-xl bg-white/5 border border-white/5 text-[10px] font-black text-white uppercase tracking-tighter italic">
                                            {s}
                                          </span>
                                          {ski < syn.skills.length - 1 && <ArrowRight className="w-3 h-3 text-slate-700" />}
                                        </React.Fragment>
                                      ))}
                                   </div>
                                   <div className="space-y-2">
                                      <div className="text-[9px] text-slate-500 uppercase font-black italic">{syn.condition}</div>
                                      <div className="p-3 bg-purple-600/5 rounded-xl border border-purple-500/20 text-[11px] text-purple-300 leading-relaxed">
                                        {syn.effect}
                                      </div>
                                      <div className="text-[10px] text-orange-400/60 font-mono flex items-center gap-2">
                                        <Sparkles className="w-3 h-3" /> {syn.visual}
                                      </div>
                                   </div>
                                 </div>
                               ))}
                             </div>

                             <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-white/5 to-transparent border border-white/5 space-y-4">
                               <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Цепочки комбинаций</h4>
                               <div className="space-y-2">
                                 {data.combos.map((c: string, ci: number) => (
                                   <div key={ci} className="flex items-center gap-3 text-[10px] text-slate-300">
                                      <div className="w-1.5 h-1.5 rounded-full bg-purple-500 shrink-0" />
                                      {c}
                                   </div>
                                 ))}
                               </div>
                             </div>

                             <div className="p-8 rounded-[2.5rem] bg-purple-600/5 border border-purple-500/10 space-y-4">
                               <h4 className="text-[10px] font-black text-purple-400 uppercase tracking-widest">Особые эффекты</h4>
                               <div className="space-y-3">
                                 {data.special_effects.map((e: string, ei: number) => (
                                   <div key={ei} className="p-3 bg-black/40 rounded-2xl border border-white/5 text-[10px] text-slate-400 leading-relaxed italic">
                                      {e}
                                   </div>
                                 ))}
                               </div>
                             </div>
                           </div>
                         </div>
                       ))}
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                       <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                         <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest px-4">Общие механики взаимодействия</h4>
                         <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {[
                              { title: 'Эффект «Комбо»', desc: gameDesign?.skill_synergies?.general?.combo_effect, icon: <Zap className="w-4 h-4 text-yellow-500" /> },
                              { title: 'Специализация', desc: gameDesign?.skill_synergies?.general?.specialization, icon: <Sparkles className="w-4 h-4 text-purple-500" /> },
                              { title: 'Адаптация', desc: gameDesign?.skill_synergies?.general?.adaptation, icon: <RefreshCw className="w-4 h-4 text-blue-500" /> },
                              { title: 'Мастерство', desc: gameDesign?.skill_synergies?.general?.mastery, icon: <Star className="w-4 h-4 text-orange-500" /> }
                            ].map((m, i) => (
                              <div key={i} className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                                <div className="flex items-center gap-3">
                                  {m.icon}
                                  <div className="text-[11px] font-black text-white uppercase tracking-widest">{m.title}</div>
                                </div>
                                <p className="text-[10px] text-slate-500 leading-relaxed italic">{m.desc}</p>
                              </div>
                            ))}
                         </div>
                       </div>

                       <div className="p-10 rounded-[3rem] bg-gradient-to-br from-blue-600/10 to-purple-600/10 border border-blue-500/20 space-y-8">
                         <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-widest">Правила Активации</h4>
                         <div className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                               <div className="space-y-2">
                                 <div className="text-[9px] text-slate-500 uppercase font-black">Окно активации</div>
                                 <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-white font-medium italic">
                                   {gameDesign?.skill_synergies?.rules?.activation_window}
                                 </div>
                               </div>
                               <div className="space-y-2">
                                 <div className="text-[9px] text-slate-500 uppercase font-black">Перезарядка синергии</div>
                                 <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-white font-medium italic">
                                   {gameDesign?.skill_synergies?.rules?.cooldown}
                                 </div>
                               </div>
                            </div>
                            <div className="space-y-4 pt-4 border-t border-white/10">
                               <div className="text-[9px] text-slate-500 uppercase font-black">Влияние местности</div>
                               <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                                  {Object.entries(gameDesign?.skill_synergies?.rules?.terrain_mod || {}).map(([key, val]: any) => (
                                    <div key={key} className="p-3 bg-white/5 rounded-xl border border-white/5 flex flex-col gap-1">
                                      <div className="text-[8px] text-slate-600 uppercase font-black">{key === 'mountains' ? 'Горы' : key === 'forest' ? 'Лес' : 'Равнины'}</div>
                                      <div className="text-[10px] text-slate-300 font-bold">{val}</div>
                                    </div>
                                  ))}
                               </div>
                            </div>
                            <div className="p-6 bg-purple-600/10 rounded-[2.5rem] border border-purple-500/20 text-[11px] text-purple-300 italic leading-relaxed">
                              "Синергии требуют непосредственного участия игрока и понимания таймингов. При активации иконка умения мерцает золотым, а звуковой сигнал подтверждает успех комбинации."
                            </div>
                         </div>
                       </div>
                    </div>
                  </motion.div>
                ) : designSubTab === 'Economy' ? (
                  <motion.div 
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="space-y-8"
                  >
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                      <div className="lg:col-span-2 space-y-8">
                        <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Экономика Найма</h3>
                        <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 overflow-hidden">
                          <table className="w-full text-left text-[10px]">
                            <thead>
                              <tr className="text-slate-600 border-b border-white/5 uppercase tracking-widest">
                                <th className="pb-4 pt-2 font-black">Тип войск</th>
                                <th className="pb-4 pt-2 font-black">База (K)</th>
                                <th className="pb-4 pt-2 font-black">Сосед (+25%)</th>
                                <th className="pb-4 pt-2 font-black">Дальний (+50%)</th>
                                <th className="pb-4 pt-2 font-black">Крайний (+100%)</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-white/5">
                              {[
                                { name: 'Легкая броня', base: '3000-5000' },
                                { name: 'Средняя броня', base: '6000-10000' },
                                { name: 'Тяжелая броня', base: '11000-15000' },
                                { name: 'Дальние (Ср)', base: '16000-20000' },
                                { name: 'Легендарные', base: '50000-100000' }
                              ].map((row, i) => {
                                const baseMin = parseInt(row.base.split('-')[0]);
                                const baseMax = parseInt(row.base.split('-')[1]);
                                return (
                                  <tr key={i} className="group hover:bg-white/5 transition-colors">
                                    <td className="py-4 font-black text-white">{row.name}</td>
                                    <td className="py-4 text-slate-400 italic">{row.base}</td>
                                    <td className="py-4 text-blue-400 font-bold">
                                      {Math.round(baseMin*1.25)}-{Math.round(baseMax*1.25)}
                                    </td>
                                    <td className="py-4 text-purple-400 font-bold">
                                      {Math.round(baseMin*1.5)}-{Math.round(baseMax*1.5)}
                                    </td>
                                    <td className="py-4 text-red-500 font-bold">
                                      {Math.round(baseMin*2)}-{Math.round(baseMax*2)}
                                    </td>
                                  </tr>
                                );
                              })}
                            </tbody>
                          </table>
                        </div>

                        <div className="p-8 rounded-[2.5rem] bg-gradient-to-r from-purple-600/10 to-transparent border border-purple-500/20">
                          <h4 className="text-[10px] font-black text-purple-400 uppercase tracking-widest mb-6">Скидки по Классам</h4>
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                             <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                               <div className="text-[9px] text-slate-500 uppercase">Воин</div>
                               <div className="text-xs text-white">-10% Тяжелая/Ближние</div>
                             </div>
                             <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                               <div className="text-[9px] text-slate-500 uppercase">Стрелок</div>
                               <div className="text-xs text-white">-10% Дальние</div>
                             </div>
                             <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                               <div className="text-[9px] text-slate-500 uppercase">Маг</div>
                               <div className="text-xs text-white">-10% Легенды/Маги</div>
                             </div>
                          </div>
                        </div>

                        <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                           <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Бонусы Замков (L5)</h4>
                           <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                             <div className="space-y-4">
                               <div className="text-[9px] text-blue-400 font-black uppercase tracking-widest">Типы Бонусов</div>
                               <div className="space-y-2">
                                 {[
                                   { type: "Экономический", bonus: "+10% Золота" },
                                   { type: "Военный", bonus: "+5% Груз" },
                                   { type: "Магический", bonus: "-5% Цена Легенд" },
                                   { type: "Торговый", bonus: "-5% Цена Континента" }
                                 ].map((b, i) => (
                                   <div key={i} className="flex justify-between items-center text-[10px] p-2 bg-white/5 rounded-xl border border-white/5">
                                      <span className="text-slate-500 italic">{b.type}</span>
                                      <span className="text-white font-bold">{b.bonus}</span>
                                   </div>
                                 ))}
                               </div>
                             </div>
                             <div className="space-y-4">
                               <div className="text-[9px] text-purple-400 font-black uppercase tracking-widest">Множитель за кол-во</div>
                               <div className="space-y-2">
                                 {[
                                   { count: "3 Замка", mult: "x1.5" },
                                   { count: "5 Замка", mult: "x2.0" },
                                   { count: "7 Замков", mult: "x3.0" },
                                   { count: "10 Замков", mult: "x4.0" }
                                 ].map((m, i) => (
                                   <div key={i} className="flex justify-between items-center text-[10px] p-2 bg-white/5 rounded-xl border border-white/5">
                                      <span className="text-slate-500 font-black uppercase">{m.count}</span>
                                      <span className="text-purple-400 font-bold">{m.mult}</span>
                                   </div>
                                 ))}
                               </div>
                             </div>
                           </div>
                           <div className="p-6 bg-blue-600/5 rounded-3xl border border-blue-500/20 text-[10px] leading-relaxed text-slate-500 italic">
                             "Важно: На уровне сложности 'Невероятный' множитель бонусов замков снижен до x0.6, а макс. кол-во замков одного типа ограничено семью до 1000 уровня."
                           </div>
                        </div>
                      </div>

                      <div className="space-y-8">
                        <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">Уровни Сложности</h3>
                        <div className="grid grid-cols-1 gap-4">
                          {[
                            { name: 'Новичок', color: 'text-green-400', desc: '-20% Цена, +20% Грузовик, +50% Золото' },
                            { name: 'Средний', color: 'text-blue-400', desc: 'Базовые параметры' },
                            { name: 'Сложный', color: 'text-orange-400', desc: '+25% Цена, -15% Грузовик, -20% Золото' },
                            { name: 'Невероятный', color: 'text-red-500', desc: '+50% Цена, -30% Грузовик, -40% Золото, Легенды с 1000 уровня' }
                          ].map((diff, i) => (
                             <div key={i} className="p-6 rounded-3xl bg-white/5 border border-white/5 hover:border-blue-500/30 transition-all group">
                               <div className={`text-[11px] font-black uppercase tracking-widest mb-2 ${diff.color}`}>{diff.name}</div>
                               <p className="text-[10px] text-slate-500 leading-relaxed italic">{diff.desc}</p>
                             </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </motion.div>
                ) : (
                  <motion.div 
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="space-y-12"
                  >
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                       {gameDesign?.unit_balance?.tiers?.map((tier: any, i: number) => (
                          <div key={i} className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 space-y-6">
                             <div className="text-[10px] font-black text-slate-600 uppercase tracking-[0.3em]">Tier 0{i+1}</div>
                             <h4 className="text-lg font-black text-white uppercase tracking-tighter">{tier.type}</h4>
                             <div className="space-y-3 border-t border-white/5 pt-4">
                                <div className="flex items-center justify-between text-[10px]">
                                  <span className="text-slate-500">HP:</span>
                                  <span className="text-red-400 font-bold">{tier.hp[0]}-{tier.hp[1]}</span>
                                </div>
                                <div className="flex items-center justify-between text-[10px]">
                                  <span className="text-slate-500">ATK:</span>
                                  <span className="text-orange-400 font-bold">{tier.atk[0]}-{tier.atk[1]}</span>
                                </div>
                                <div className="flex items-center justify-between text-[10px]">
                                  <span className="text-slate-500">DEF:</span>
                                  <span className="text-blue-400 font-bold">{tier.def[0]}-{tier.def[1]}</span>
                                </div>
                             </div>
                          </div>
                       ))}
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                       <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                          <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Система Редкости</h4>
                          <div className="grid grid-cols-1 gap-3">
                             {gameDesign?.rarity_tiers?.map((tier: any, i: number) => (
                               <div key={i} className="p-4 bg-white/5 rounded-2xl border border-white/5 flex items-center justify-between group hover:border-white/20 transition-all">
                                  <div className="flex items-center gap-4">
                                     <div className={`w-3 h-3 rounded-full ${
                                       i === 0 ? 'bg-white' : i === 1 ? 'bg-green-500' : i === 2 ? 'bg-blue-500' :
                                       i === 3 ? 'bg-purple-600' : i === 4 ? 'bg-pink-500' : i === 5 ? 'bg-red-500' :
                                       i === 6 ? 'bg-yellow-500' : 'bg-cyan-400 shadow-[0_0_15px_rgba(34,211,238,0.5)]'
                                     }`} />
                                     <span className="text-[11px] font-black text-white uppercase tracking-widest">{tier.name}</span>
                                  </div>
                                  <div className="text-[10px] text-slate-500 font-mono">{tier.bonus}</div>
                               </div>
                             ))}
                          </div>
                       </div>

                       <div className="space-y-8">
                          <div className="p-10 rounded-[3rem] bg-gradient-to-br from-purple-600/10 to-blue-600/10 border border-purple-500/20 space-y-6">
                            <h4 className="text-[10px] font-black text-white uppercase tracking-widest">Формула Масштабирования</h4>
                            <div className="space-y-4">
                               {gameDesign?.unit_balance?.scaling_formula && Object.entries(gameDesign.unit_balance.scaling_formula).map(([key, val]: any) => (
                                 <div key={key} className="flex items-center justify-between p-4 bg-black/40 rounded-2xl border border-white/5">
                                    <span className="text-[10px] text-slate-500">LVL {key.replace('_', '-')}:</span>
                                    <span className="text-[10px] font-bold text-purple-400">{val}</span>
                                 </div>
                               ))}
                            </div>
                          </div>

                          <div className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 space-y-6">
                             <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Процесс Эволюции</h4>
                             <div className="flex items-center justify-between">
                               {gameDesign?.unit_balance?.evolution?.map((evo: string, i: number) => (
                                 <div key={i} className="flex flex-col items-center gap-2 group cursor-help">
                                    <div className="w-10 h-10 rounded-full bg-white/5 border border-white/10 flex items-center justify-center text-[10px] font-black text-slate-700 group-hover:bg-purple-600 group-hover:text-white transition-all">0{i+1}</div>
                                    <span className="text-[8px] font-black text-slate-600 uppercase tracking-widest opacity-0 group-hover:opacity-100 transition-opacity">{evo}</span>
                                 </div>
                               ))}
                             </div>
                             <p className="text-[9px] text-slate-600 text-center italic">"Каждая стадия эволюции открывает новые пассивные способности и увеличивает базовые характеристики на 15%."</p>
                          </div>
                       </div>
                    </div>
                  </motion.div>
                )}
              </div>
            </div>

          ) : activeTab === 'dashboard' ? (
            <div className="flex-1 overflow-y-auto p-8 scrollbar-thin scrollbar-thumb-white/5 space-y-8">
              <div className="max-w-6xl mx-auto space-y-8">
                
                {/* Quantum Link Integration Block */}
                <motion.div 
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="p-8 rounded-[2.5rem] bg-gradient-to-br from-blue-600/10 via-purple-600/10 to-cyan-600/10 border border-blue-500/20 relative overflow-hidden group"
                >
                  <div className="absolute top-0 right-0 p-8 opacity-10 group-hover:scale-110 transition-transform duration-700">
                    <Zap className="w-32 h-32 text-blue-400" />
                  </div>
                  
                  <div className="relative z-10">
                    <div className="flex items-center gap-4 mb-6">
                      <div className="w-12 h-12 bg-blue-600 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-600/20">
                        <ExternalLink className="w-6 h-6 text-white" />
                      </div>
                      <div>
                        <h2 className="text-xl font-bold text-white uppercase tracking-tighter">Quantum Link Integration (v16.99.0)</h2>
                        <p className="text-xs text-slate-400">Прямое управление Blender и Unity через нейронный мост.</p>
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                      {/* Blender Card */}
                      <div className="p-6 rounded-3xl bg-black/40 border border-white/5 hover:border-orange-500/30 transition-all">
                        <div className="flex items-center gap-3 mb-4 text-orange-400 uppercase font-bold text-xs tracking-widest">
                          <Cube className="w-4 h-4" /> Blender Addon
                        </div>
                        <p className="text-[10px] text-slate-400 mb-6 leading-relaxed">
                          Создает панель управления в Blender. Позволяет ИИ напрямую генерировать меши, материалы и логику сцены.
                        </p>
                        <button 
                          onClick={() => window.open('/blender_connector.py', '_blank')}
                          className="w-full py-3 rounded-xl bg-orange-600/20 border border-orange-500/30 text-[10px] font-bold text-orange-400 uppercase tracking-widest hover:bg-orange-600 hover:text-white transition-all"
                        >
                          Открыть blender_connector.py
                        </button>
                      </div>

                      {/* Unity Card */}
                      <div className="p-6 rounded-3xl bg-black/40 border border-white/5 hover:border-cyan-500/30 transition-all">
                        <div className="flex items-center gap-3 mb-4 text-cyan-400 uppercase font-bold text-xs tracking-widest">
                          <Gamepad2 className="w-4 h-4" /> Unity Connector
                        </div>
                        <p className="text-[10px] text-slate-400 mb-6 leading-relaxed">
                          Окно редактора для Unity. Генерируйте C# скрипты и управляйте игровыми объектами с помощью ИИ.
                        </p>
                        <button 
                          onClick={() => window.open('/UnityConnector.cs', '_blank')}
                          className="w-full py-3 rounded-xl bg-cyan-600/20 border border-cyan-500/30 text-[10px] font-bold text-cyan-400 uppercase tracking-widest hover:bg-cyan-600 hover:text-white transition-all"
                        >
                          Открыть UnityConnector.cs
                        </button>
                      </div>
                    </div>

                    <div className="p-6 rounded-2xl bg-white/5 border border-white/5">
                      <h4 className="text-[10px] font-bold text-white uppercase tracking-widest mb-4">Краткая инструкция по установке:</h4>
                      <div className="space-y-3">
                        <div className="flex items-start gap-4">
                          <div className="w-5 h-5 rounded-full bg-blue-600/20 border border-blue-500/30 flex items-center justify-center text-[10px] font-bold text-blue-400 shrink-0">1</div>
                          <p className="text-[10px] text-slate-400">Нажмите кнопки выше, чтобы открыть код. Скопируйте его и сохраните в файл с указанным именем в корне вашего проекта.</p>
                        </div>
                        <div className="flex items-start gap-4">
                          <div className="w-5 h-5 rounded-full bg-blue-600/20 border border-blue-500/30 flex items-center justify-center text-[10px] font-bold text-blue-400 shrink-0">2</div>
                          <p className="text-[10px] text-slate-400"><span className="text-orange-400 font-bold uppercase">Blender:</span> Зайдите в Edit → Preferences → Add-ons → Install, выберите файл и активируйте галочку.</p>
                        </div>
                        <div className="flex items-start gap-4">
                          <div className="w-5 h-5 rounded-full bg-blue-600/20 border border-blue-500/30 flex items-center justify-center text-[10px] font-bold text-blue-400 shrink-0">3</div>
                          <p className="text-[10px] text-slate-400"><span className="text-cyan-400 font-bold uppercase">Unity:</span> Создайте в папке Assets папку "Editor" и поместите файл туда. Окно появится в меню "AI Assistant".</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </motion.div>

                {/* Top Stats Grid */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="p-6 rounded-3xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all">
                    <div className="flex items-center gap-4 mb-4">
                      <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                        <AlertTriangle className="w-5 h-5" />
                      </div>
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest">Аудит кода</h3>
                    </div>
                    <div className="text-2xl font-bold text-white mb-1">{projectScan?.analysis.audit_issues.length || 0}</div>
                    <p className="text-[10px] text-slate-500 uppercase">Проблем производительности</p>
                  </div>
                  
                  <div className="p-6 rounded-3xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all">
                    <div className="flex items-center gap-4 mb-4">
                      <div className="p-3 bg-purple-600/20 rounded-2xl text-purple-400">
                        <Check className="w-5 h-5" />
                      </div>
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest">Задачи (TODO)</h3>
                    </div>
                    <div className="text-2xl font-bold text-white mb-1">{projectScan?.analysis.todos.length || 0}</div>
                    <p className="text-[10px] text-slate-500 uppercase">Активных задач в коде</p>
                  </div>

                  <div className="p-6 rounded-3xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all">
                    <div className="flex items-center gap-4 mb-4">
                      <div className="p-3 bg-green-600/20 rounded-2xl text-green-400">
                        <Box className="w-5 h-5" />
                      </div>
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest">Оптимизация</h3>
                    </div>
                    <div className="text-2xl font-bold text-white mb-1">
                      {(projectScan?.analysis.asset_stats.total_size ? (projectScan.analysis.asset_stats.total_size / 1024 / 1024).toFixed(1) : 0)} MB
                    </div>
                    <p className="text-[10px] text-slate-500 uppercase">Общий вес ассетов</p>
                  </div>
                </div>

                {/* Knowledge Base Section */}
                <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-blue-600/10 to-purple-600/10 border border-white/10">
                  <div className="flex flex-col md:flex-row items-center justify-between gap-6">
                    <div className="flex items-center gap-6">
                      <div className="w-16 h-16 bg-blue-600 rounded-3xl flex items-center justify-center shadow-2xl shadow-blue-600/40">
                        <Layers className="w-8 h-8 text-white" />
                      </div>
                      <div>
                        <h3 className="text-lg font-bold text-white uppercase tracking-tighter">Расширение Базы Знаний</h3>
                        <p className="text-xs text-slate-400 max-w-md">
                          Обновите локальные справочники Unity API, Blender Python и Troubleshooting для более точной помощи в офлайн-режиме.
                        </p>
                      </div>
                    </div>
                    <button 
                      onClick={handleUpdateKB}
                      disabled={isUpdatingKB}
                      className={`px-8 py-4 rounded-2xl font-bold uppercase tracking-widest text-xs transition-all flex items-center gap-3 ${
                        isUpdatingKB 
                        ? 'bg-white/5 text-slate-500' 
                        : 'bg-white text-black hover:bg-blue-500 hover:text-white shadow-xl shadow-white/10'
                      }`}
                    >
                      {isUpdatingKB ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Sparkles className="w-4 h-4" />}
                      {isUpdatingKB ? 'Обновление...' : 'Обновить Базы'}
                    </button>
                  </div>
                </div>

                {/* New Features Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5 space-y-6">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-orange-600/20 rounded-2xl text-orange-400">
                        <RefreshCw className="w-5 h-5" />
                      </div>
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest">Unity Bridge</h3>
                    </div>
                    <p className="text-xs text-slate-400">
                      Автоматическая настройка материалов при импорте из Blender. Конвертация Standard материалов в URP/HDRP.
                    </p>
                    <button 
                      onClick={handleGetMaterialConverter}
                      className="w-full py-3 bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl text-[10px] font-bold text-white uppercase tracking-widest transition-all"
                    >
                      Получить C# Скрипт
                    </button>
                  </div>

                  <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5 space-y-6">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                        <GitBranch className="w-5 h-5" />
                      </div>
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest">Git LFS Setup</h3>
                    </div>
                    <p className="text-xs text-slate-400">
                      Генерация правильного .gitattributes для Unity проекта. Защита от раздувания репозитория тяжелыми ассетами.
                    </p>
                    <button 
                      onClick={handleGetGitLFS}
                      className="w-full py-3 bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl text-[10px] font-bold text-white uppercase tracking-widest transition-all"
                    >
                      Копировать .gitattributes
                    </button>
                  </div>

                  <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5 space-y-6">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-green-600/20 rounded-2xl text-green-400">
                        <Type className="w-5 h-5" />
                      </div>
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest">Naming Standard</h3>
                    </div>
                    <p className="text-xs text-slate-400">
                      Генератор имен по стандарту (T_Texture, M_Material, P_Prefab). Помогает поддерживать порядок в проекте.
                    </p>
                    <button 
                      onClick={() => showNotification("Стандарты именования: T_ (Texture), M_ (Material), P_ (Prefab), S_ (Script)", "info")}
                      className="w-full py-3 bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl text-[10px] font-bold text-white uppercase tracking-widest transition-all"
                    >
                      Показать стандарт
                    </button>
                  </div>

                  <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5 space-y-6">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-pink-600/20 rounded-2xl text-pink-400">
                        <Sparkles className="w-5 h-5" />
                      </div>
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest">Shader Assistant</h3>
                    </div>
                    <p className="text-xs text-slate-400">
                      Библиотека готовых решений для Shader Graph и HLSL. Офлайн-подсказки по созданию эффектов.
                    </p>
                    <button 
                      onClick={() => showNotification("Функция в разработке. Используйте чат для запроса шейдеров.", "info")}
                      className="w-full py-3 bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl text-[10px] font-bold text-white uppercase tracking-widest transition-all"
                    >
                      Открыть библиотеку
                    </button>
                  </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                  {/* Audit & Todos */}
                  <div className="space-y-8">
                    <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                        <Terminal className="w-4 h-4 text-blue-400" /> Результаты аудита
                      </h3>
                      <div className="space-y-4">
                        {projectScan?.analysis.audit_issues.map((issue, i) => (
                          <div key={i} className="p-4 rounded-2xl bg-red-600/5 border border-red-500/20 space-y-2">
                            <div className="flex items-center justify-between">
                              <span className="text-[10px] font-bold text-red-400 uppercase">{issue.type}</span>
                              <span className="text-[9px] text-slate-500 font-mono">{issue.file}</span>
                            </div>
                            <p className="text-xs text-slate-300 leading-relaxed">{issue.message}</p>
                          </div>
                        ))}
                        {(!projectScan?.analysis.audit_issues.length) && (
                          <p className="text-xs text-slate-600 italic">Проблем не обнаружено. Ваш код оптимизирован!</p>
                        )}
                      </div>
                    </section>

                    <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                        <Check className="w-4 h-4 text-purple-400" /> Список задач (AI To-Do)
                      </h3>
                      <div className="space-y-3">
                        {projectScan?.analysis.todos.map((todo, i) => (
                          <div key={i} className="flex items-start gap-4 p-4 rounded-2xl bg-white/5 border border-white/5 group hover:bg-white/10 transition-all">
                            <div className={`mt-1 w-2 h-2 rounded-full flex-shrink-0 ${todo.type === 'FIXME' ? 'bg-red-500' : 'bg-yellow-500'}`} />
                            <div className="flex-1">
                              <div className="flex items-center justify-between mb-1">
                                <span className="text-[10px] font-bold text-slate-400 uppercase">{todo.type}</span>
                                <span className="text-[9px] text-slate-600 font-mono">{todo.file}</span>
                              </div>
                              <p className="text-xs text-slate-200">{todo.text}</p>
                            </div>
                          </div>
                        ))}
                        {(!projectScan?.analysis.todos.length) && (
                          <p className="text-xs text-slate-600 italic">Задач TODO не найдено.</p>
                        )}
                      </div>
                    </section>
                  </div>

                  {/* Assets & History */}
                  <div className="space-y-8">
                    <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                        <Cube className="w-4 h-4 text-green-400" /> Тяжелые ассеты
                      </h3>
                      <div className="space-y-3">
                        {projectScan?.analysis.asset_stats.large_files.map((asset, i) => (
                          <div key={i} className="flex items-center justify-between p-4 rounded-2xl bg-white/5 border border-white/5">
                            <div className="flex items-center gap-3 overflow-hidden">
                              <ImageIcon className="w-4 h-4 text-slate-500 flex-shrink-0" />
                              <span className="text-xs text-slate-300 truncate">{asset.path}</span>
                            </div>
                            <span className="text-[10px] font-mono text-red-400 font-bold ml-4">{asset.size}</span>
                          </div>
                        ))}
                        {(!projectScan?.analysis.asset_stats.large_files.length) && (
                          <p className="text-xs text-slate-600 italic">Все ассеты в пределах нормы (до 10MB).</p>
                        )}
                      </div>
                    </section>

                    <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                        <RefreshCw className="w-4 h-4 text-blue-400" /> История изменений (Mini-Git)
                      </h3>
                      <div className="space-y-4 max-h-[400px] overflow-y-auto scrollbar-none pr-2">
                        {history.map((item, i) => (
                          <div key={i} className="relative pl-6 border-l border-white/10 pb-4 last:pb-0">
                            <div className={`absolute left-[-4px] top-0 w-2 h-2 rounded-full ${
                              item.event === 'add' ? 'bg-green-500' : 
                              item.event === 'change' ? 'bg-blue-500' : 
                              'bg-red-500'
                            }`} />
                            <div className="flex items-center justify-between mb-1">
                              <span className="text-[10px] font-bold text-white uppercase">{item.event}</span>
                              <span className="text-[9px] text-slate-600">{new Date(item.timestamp).toLocaleTimeString()}</span>
                            </div>
                            <p className="text-[11px] text-slate-400 truncate">{item.path}</p>
                          </div>
                        ))}
                        {history.length === 0 && (
                          <p className="text-xs text-slate-600 italic">История пуста. Начните работу с файлами!</p>
                        )}
                      </div>
                    </section>

                    {/* Blender Presets */}
                    <section className="p-8 rounded-[2.5rem] bg-gradient-to-br from-purple-600/10 to-blue-600/10 border border-white/10">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                        <Cube className="w-4 h-4 text-purple-400" /> Пресеты Blender
                      </h3>
                      <div className="grid grid-cols-1 gap-4">
                        {blenderPresets.map((preset) => (
                          <div key={preset.id} className="p-4 rounded-2xl bg-black/40 border border-white/5 hover:border-purple-500/30 transition-all group">
                            <div className="flex items-center justify-between mb-2">
                              <h4 className="text-xs font-bold text-white uppercase">{preset.name}</h4>
                              <button 
                                onClick={() => copyToClipboard(preset.code, preset.id)}
                                className="p-1.5 hover:bg-white/5 rounded-md text-slate-500 hover:text-white transition-all"
                              >
                                {copiedId === preset.id ? <Check className="w-3 h-3 text-green-500" /> : <Copy className="w-3 h-3" />}
                              </button>
                            </div>
                            <p className="text-[10px] text-slate-500 mb-3">{preset.desc}</p>
                            <div className="bg-black/60 rounded-lg p-3 font-mono text-[9px] text-purple-400 overflow-x-auto">
                              {preset.code.split('\n')[0]}...
                            </div>
                          </div>
                        ))}
                      </div>
                    </section>
                  </div>
                </div>
              </div>
            </div>
          ) : activeTab === 'project_info' ? (
            <div className="flex-1 overflow-y-auto p-8 scrollbar-thin scrollbar-thumb-white/5">
              <div className="max-w-4xl mx-auto">
                <div className="bg-white/5 border border-white/5 rounded-[2.5rem] p-10 shadow-2xl relative overflow-hidden group">
                  <div className="absolute top-0 right-0 p-12 opacity-5 group-hover:opacity-10 transition-opacity pointer-events-none">
                    <Cpu className="w-64 h-64 text-white" />
                  </div>
                  <div className="relative z-10">
                    <div className="markdown-body prose prose-invert prose-sm max-w-none text-slate-300 leading-relaxed">
                      <Markdown>{kb?.unity_ai_assistant?.combined_knowledge}</Markdown>
                    </div>
                  </div>
                </div>

                <div className="mt-12 p-8 rounded-[2rem] bg-gradient-to-br from-blue-600/10 to-purple-600/10 border border-white/10 relative overflow-hidden">
                  <div className="flex items-center justify-between gap-6">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-blue-600/20 rounded-2xl">
                        <FileText className="w-6 h-6 text-blue-400" />
                      </div>
                      <div>
                        <h3 className="text-lg font-bold text-white uppercase tracking-tight">Master Recovery Blueprint</h3>
                        <p className="text-xs text-slate-400">Файл PROJECT_MASTER_BLUEPRINT.md обновлен и готов к использованию.</p>
                      </div>
                    </div>
                    <button 
                      onClick={() => setShowSettings(true)}
                      className="px-6 py-3 bg-white/5 hover:bg-white/10 text-white rounded-2xl text-[10px] font-bold uppercase tracking-widest transition-all border border-white/10"
                    >
                      Настройки
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ) : null}
        </div>
      </main>

      {/* Ollama Guide Modal */}
      <AnimatePresence>
        {showOllamaGuide && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-6"
          >
            <motion.div 
              initial={{ scale: 0.9, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.9, y: 20 }}
              className="bg-[#121214] border border-white/10 rounded-3xl w-full max-w-2xl max-h-[80vh] overflow-hidden flex flex-col shadow-2xl"
            >
              <div className="p-6 border-b border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-cyan-600/20 rounded-lg text-cyan-400">
                    <Cpu className="w-5 h-5" />
                  </div>
                  <h2 className="text-lg font-bold text-white uppercase tracking-tight">Ollama (Offline AI) Guide</h2>
                </div>
                <button onClick={() => setShowOllamaGuide(false)} className="p-2 hover:bg-white/5 rounded-lg transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-8 space-y-6 scrollbar-thin scrollbar-thumb-white/5">
                <div className="space-y-4">
                  <h4 className="text-sm font-bold text-white uppercase">1. Установка</h4>
                  <p className="text-xs text-slate-400">Скачайте Ollama с официального сайта <a href="https://ollama.com" target="_blank" className="text-cyan-400 underline">ollama.com</a> и установите её.</p>
                  
                  <h4 className="text-sm font-bold text-white uppercase">2. Загрузка модели</h4>
                  <p className="text-xs text-slate-400">Откройте терминал (CMD) и введите команду для загрузки Llama 3:</p>
                  <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-cyan-400 border border-white/5">
                    ollama run llama3
                  </div>

                  <h4 className="text-sm font-bold text-white uppercase">3. Интеграция с проектом</h4>
                  <p className="text-xs text-slate-400">Наш проект автоматически обнаружит Ollama на порту 11434. Если интернет пропадет, чат переключится на локальную модель.</p>
                  
                  <div className="p-4 bg-yellow-600/10 border border-yellow-500/20 rounded-xl">
                    <p className="text-[10px] text-yellow-500 leading-relaxed">
                      <strong>Важно:</strong> Для работы Ollama требуется минимум 8ГБ оперативной памяти (рекомендуется 16ГБ+ и видеокарта NVIDIA).
                    </p>
                  </div>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Migration Modal */}
      <AnimatePresence>
        {showMigrationModal && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-6"
          >
            <motion.div 
              initial={{ scale: 0.9, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.9, y: 20 }}
              className="bg-[#121214] border border-white/10 rounded-3xl w-full max-w-3xl max-h-[90vh] overflow-hidden flex flex-col shadow-2xl"
            >
              <div className="p-6 border-b border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-orange-600/20 rounded-lg text-orange-400">
                    <GitBranch className="w-5 h-5" />
                  </div>
                  <h2 className="text-lg font-bold text-white uppercase tracking-tight">Unity Migration Center</h2>
                </div>
                <button onClick={() => setShowMigrationModal(false)} className="p-2 hover:bg-white/5 rounded-lg transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-8 space-y-8 scrollbar-thin scrollbar-thumb-white/5">
                <div className="grid grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Текущие пакеты:</h4>
                    <div className="space-y-2">
                      {unityPackages.map((pkg, i) => (
                        <div key={i} className="p-3 rounded-xl bg-white/5 border border-white/5">
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-xs font-bold text-white">{pkg.name}</span>
                            <span className="text-[9px] text-slate-500">{pkg.version}</span>
                          </div>
                          <p className="text-[9px] text-slate-400 italic">{pkg.action}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                  <div className="space-y-4">
                    <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Действие:</h4>
                    <button 
                      onClick={handleMigrate}
                      disabled={isMigrating}
                      className="w-full py-4 bg-orange-600 hover:bg-orange-500 text-white rounded-2xl text-xs font-bold uppercase tracking-widest transition-all shadow-lg shadow-orange-600/20"
                    >
                      {isMigrating ? 'Генерация...' : 'Создать план миграции на Unity 6'}
                    </button>
                    {migrationGuide && (
                      <div className="p-4 rounded-xl bg-black/40 border border-white/5 font-sans text-xs text-slate-300 leading-relaxed">
                        <Markdown>{migrationGuide}</Markdown>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* GitHub Guide Modal */}
      <AnimatePresence>
        {showGithubGuide && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-6"
          >
            <motion.div 
              initial={{ scale: 0.9, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.9, y: 20 }}
              className="bg-[#121214] border border-white/10 rounded-3xl w-full max-w-2xl max-h-[80vh] overflow-hidden flex flex-col shadow-2xl"
            >
              <div className="p-6 border-b border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-white/5 rounded-lg text-blue-400">
                    <Github className="w-5 h-5" />
                  </div>
                  <h2 className="text-lg font-bold text-white uppercase tracking-tight">GitHub Console Guide</h2>
                </div>
                <button 
                  onClick={() => setShowGithubGuide(false)}
                  className="p-2 hover:bg-white/5 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
              
              <div className="flex-1 overflow-y-auto p-8 space-y-8 scrollbar-thin scrollbar-thumb-white/5">
                <div className="bg-blue-600/10 border border-blue-500/20 rounded-2xl p-4 flex gap-4">
                  <Info className="w-5 h-5 text-blue-400 flex-shrink-0" />
                  <p className="text-[11px] text-blue-300 leading-relaxed">
                    Следуйте этим шагам, чтобы перенести проект <code className="text-white">C:\Users\user\Desktop\HelperUnity-main\HelperUnity-main</code> на GitHub через консоль.
                  </p>
                </div>

                <div className="space-y-6">
                  <div className="relative pl-8 border-l border-white/10">
                    <div className="absolute left-[-5px] top-0 w-2.5 h-2.5 rounded-full bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.5)]" />
                    <h4 className="text-[11px] font-bold text-white uppercase mb-1">Шаг 1: Инициализация</h4>
                    <p className="text-[10px] text-slate-500 mb-2">Откройте терминал в папке проекта и выполните:</p>
                    <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-blue-400 border border-white/5">
                      git init
                    </div>
                  </div>

                  <div className="relative pl-8 border-l border-white/10">
                    <div className="absolute left-[-5px] top-0 w-2.5 h-2.5 rounded-full bg-blue-500" />
                    <h4 className="text-[11px] font-bold text-white uppercase mb-1">Шаг 2: Добавление файлов</h4>
                    <p className="text-[10px] text-slate-500 mb-2">Добавьте все файлы проекта в индекс:</p>
                    <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-blue-400 border border-white/5">
                      git add .
                    </div>
                  </div>

                  <div className="relative pl-8 border-l border-white/10">
                    <div className="absolute left-[-5px] top-0 w-2.5 h-2.5 rounded-full bg-blue-500" />
                    <h4 className="text-[11px] font-bold text-white uppercase mb-1">Шаг 3: Первый коммит</h4>
                    <p className="text-[10px] text-slate-500 mb-2">Зафиксируйте изменения:</p>
                    <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-blue-400 border border-white/5">
                      git commit -m "Initial commit: Unity & Blender Assistant"
                    </div>
                  </div>

                  <div className="relative pl-8 border-l border-white/10">
                    <div className="absolute left-[-5px] top-0 w-2.5 h-2.5 rounded-full bg-blue-500" />
                    <h4 className="text-[11px] font-bold text-white uppercase mb-1">Шаг 4: Привязка репозитория</h4>
                    <p className="text-[10px] text-slate-500 mb-2">Создайте репозиторий на GitHub и вставьте его URL:</p>
                    <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-blue-400 border border-white/5">
                      git remote add origin https://github.com/ВАШ_ЛОГИН/ВАШ_РЕПОЗИТОРИЙ.git<br/>
                      git branch -M main
                    </div>
                  </div>

                  <div className="relative pl-8 border-l border-white/10">
                    <div className="absolute left-[-5px] top-0 w-2.5 h-2.5 rounded-full bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.5)]" />
                    <h4 className="text-[11px] font-bold text-white uppercase mb-1">Шаг 5: Публикация</h4>
                    <p className="text-[10px] text-slate-500 mb-2">Отправьте файлы на сервер:</p>
                    <div className="bg-black/40 rounded-xl p-4 font-mono text-[11px] text-blue-400 border border-white/5">
                      git push -u origin main
                    </div>
                  </div>
                </div>

                <div className="bg-yellow-600/10 border border-yellow-500/20 rounded-2xl p-4 flex gap-4">
                  <AlertTriangle className="w-5 h-5 text-yellow-400 flex-shrink-0" />
                  <div className="space-y-1">
                    <h5 className="text-[11px] font-bold text-yellow-400 uppercase">Важное примечание</h5>
                    <p className="text-[10px] text-yellow-300/70 leading-relaxed">
                      Убедитесь, что у вас установлен Git. Если нет, скачайте его с официального сайта <a href="https://git-scm.com" target="_blank" className="text-white underline">git-scm.com</a>.
                    </p>
                  </div>
                </div>
              </div>

              <div className="p-6 border-t border-white/5 bg-black/20 flex justify-end">
                <button 
                  onClick={() => setShowGithubGuide(false)}
                  className="px-6 py-2 bg-blue-600 hover:bg-blue-500 text-white rounded-xl text-xs font-bold uppercase transition-all"
                >
                  Понятно
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

        {/* Capabilities Modal */}
        <AnimatePresence>
          {showCapabilities && capabilities && (
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-[300] flex items-center justify-center p-4 bg-black/80 backdrop-blur-xl"
            >
              <motion.div 
                initial={{ scale: 0.9, y: 20 }}
                animate={{ scale: 1, y: 0 }}
                exit={{ scale: 0.9, y: 20 }}
                className="bg-[#0a0a0c] border border-white/10 rounded-[3rem] w-full max-w-3xl max-h-[85vh] overflow-hidden shadow-2xl flex flex-col"
              >
                <div className="p-8 border-b border-white/5 flex items-center justify-between bg-gradient-to-r from-blue-600/10 to-transparent">
                  <div className="flex items-center gap-4">
                    <div className="p-4 bg-blue-600 rounded-3xl shadow-xl shadow-blue-600/20">
                      <Zap className="w-6 h-6 text-white" />
                    </div>
                    <div>
                      <h2 className="text-xl font-bold text-white tracking-tighter">Unity & Blender AI Assistant v{appVersion}</h2>
                      <p className="text-xs text-slate-400">Расширенная база знаний: 10000+ видео</p>
                    </div>
                  </div>
                  <button 
                    onClick={() => setShowCapabilities(false)}
                    className="p-3 hover:bg-white/5 rounded-2xl text-slate-500 hover:text-white transition-all"
                  >
                    <X className="w-6 h-6" />
                  </button>
                </div>

                <div className="flex-1 overflow-y-auto p-8 space-y-10 scrollbar-thin scrollbar-thumb-white/5">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {capabilities.core_functions.map((func: any, i: number) => (
                      <div key={i} className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                        <h4 className="text-xs font-bold text-blue-400 uppercase tracking-widest">{func.title}</h4>
                        <p className="text-xs text-slate-400 leading-relaxed">{func.desc}</p>
                      </div>
                    ))}
                  </div>

                  <div className="space-y-6">
                    <h3 className="text-xs font-bold text-white uppercase tracking-widest flex items-center gap-3">
                      <FileCode className="w-4 h-4 text-purple-400" /> Обрабатываемые файлы
                    </h3>
                    <div className="grid grid-cols-1 gap-3">
                      {capabilities.files_handled.map((file: string, i: number) => (
                        <div key={i} className="px-5 py-3 rounded-2xl bg-white/5 border border-white/5 text-[11px] text-slate-300 font-mono">
                          {file}
                        </div>
                      ))}
                    </div>
                  </div>

                  {capabilities.video_knowledge_base && (
                    <div className="space-y-8">
                      <div className="flex items-center justify-between">
                        <h3 className="text-xs font-bold text-white uppercase tracking-widest flex items-center gap-3">
                          <Zap className="w-4 h-4 text-yellow-400" /> База знаний: 9200+ видео & Global Synergy
                        </h3>
                        <span className="text-[10px] text-slate-500 font-mono uppercase">Обновлено: {capabilities.video_knowledge_base.update_date}</span>
                      </div>
                      
                      {capabilities.video_knowledge_base && capabilities.video_knowledge_base.categories && (
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                          {capabilities.video_knowledge_base.categories.map((cat: any, i: number) => (
                            <div key={i} className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-4 group hover:bg-white/10 transition-all">
                              <h4 className="text-[11px] font-bold text-blue-400 uppercase tracking-wider border-b border-white/5 pb-2">{cat.name}</h4>
                              <ul className="space-y-2">
                                {cat.items.map((item: string, j: number) => (
                                  <li key={j} className="text-[11px] text-slate-400 flex items-start gap-2">
                                    <span className="text-blue-500 mt-1">•</span>
                                    <span>{item}</span>
                                  </li>
                                ))}
                              </ul>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  )}

                  {capabilities.ai_limitations?.current_gaps && (
                    <div className="space-y-6">
                      <h3 className="text-xs font-bold text-red-400 uppercase tracking-widest flex items-center gap-3">
                        <AlertTriangle className="w-4 h-4" /> Чего ИИ пока НЕ знает
                      </h3>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {capabilities.ai_limitations.current_gaps.map((limit: string, i: number) => (
                          <div key={i} className="px-5 py-3 rounded-2xl bg-red-500/5 border border-red-500/10 text-[11px] text-slate-400 italic">
                            {limit}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {capabilities.game_genres && (
                    <div className="space-y-6">
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest flex items-center gap-3">
                        <Gamepad2 className="w-4 h-4 text-green-400" /> Поддерживаемые жанры
                      </h3>
                      <div className="flex flex-wrap gap-3">
                        {capabilities.game_genres.map((genre: string, i: number) => (
                          <div key={i} className="px-4 py-2 rounded-full bg-green-600/10 border border-green-500/20 text-[10px] font-bold text-green-400 uppercase tracking-widest">
                            {genre}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                  {capabilities.inventory_guide && (
                    <div className="space-y-6">
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest flex items-center gap-3">
                        <Box className="w-4 h-4 text-orange-400" /> Системы инвентаря
                      </h3>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="p-5 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                          <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Типы</h4>
                          <div className="flex flex-wrap gap-2">
                            {capabilities.inventory_guide.types.map((t: string, i: number) => (
                              <span key={i} className="px-3 py-1 rounded-lg bg-black/40 text-[10px] text-slate-300">{t}</span>
                            ))}
                          </div>
                        </div>
                        <div className="p-5 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                          <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Компоненты</h4>
                          <div className="flex flex-wrap gap-2">
                            {capabilities.inventory_guide.components.map((c: string, i: number) => (
                              <span key={i} className="px-3 py-1 rounded-lg bg-black/40 text-[10px] text-slate-300">{c}</span>
                            ))}
                          </div>
                        </div>
                        <div className="p-5 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                          <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Особенности</h4>
                          <div className="flex flex-wrap gap-2">
                            {capabilities.inventory_guide.features.map((f: string, i: number) => (
                              <span key={i} className="px-3 py-1 rounded-lg bg-black/40 text-[10px] text-slate-300">{f}</span>
                            ))}
                          </div>
                        </div>
                        <div className="p-5 rounded-3xl bg-white/5 border border-white/5 space-y-3">
                          <h4 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Реализация Unity</h4>
                          <div className="flex flex-wrap gap-2">
                            {capabilities.inventory_guide.unity_implementation.map((u: string, i: number) => (
                              <span key={i} className="px-3 py-1 rounded-lg bg-black/40 text-[10px] text-slate-300">{u}</span>
                            ))}
                          </div>
                        </div>
                      </div>
                    </div>
                  )}

                  {kb?.blender_manuals && (
                    <div className="space-y-6">
                      <h3 className="text-xs font-bold text-white uppercase tracking-widest flex items-center gap-3">
                        <BookOpen className="w-4 h-4 text-blue-400" /> Документация Blender (v2.4 - v5.1)
                      </h3>
                      <div className="p-6 rounded-3xl bg-blue-600/5 border border-blue-500/20">
                        <p className="text-[11px] text-slate-400 leading-relaxed mb-4">
                          ИИ интегрировал знания из всех официальных руководств Blender. Это позволяет давать точные ответы как по классическим методам (Internal Render, Layers), так и по самым современным (Eevee Next, Simulation Nodes).
                        </p>
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                          {kb.blender_manuals.slice(0, 8).map((url: string, i: number) => {
                            const version = url.split('/').filter(Boolean).pop();
                            return (
                              <div key={i} className="px-3 py-2 rounded-xl bg-black/40 border border-white/5 text-[10px] text-center text-blue-300 font-mono">
                                v{version}
                              </div>
                            );
                          })}
                          <div className="px-3 py-2 rounded-xl bg-black/40 border border-white/5 text-[10px] text-center text-slate-500 font-mono italic">
                            + еще {kb.blender_manuals.length - 8}
                          </div>
                        </div>
                      </div>
                    </div>
                  )}
                </div>

                <div className="p-8 bg-white/5 border-t border-white/5 flex justify-center">
                  <button 
                    onClick={() => setShowCapabilities(false)}
                    className="px-10 py-4 bg-white text-black rounded-2xl font-bold uppercase tracking-widest text-xs hover:bg-blue-600 hover:text-white transition-all shadow-xl shadow-white/5"
                  >
                    Понятно
                  </button>
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>
      
      {/* Quantum Link Modal */}
      <AnimatePresence>
        {showQuantumLink && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[110] bg-black/90 backdrop-blur-xl flex items-center justify-center p-6"
          >
            <motion.div 
              initial={{ scale: 0.9, y: 50 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.9, y: 50 }}
              className="bg-[#0a0a0c] border border-white/10 rounded-[3.5rem] w-full max-w-4xl max-h-[90vh] overflow-hidden flex flex-col shadow-[0_0_100px_rgba(59,130,246,0.1)] relative"
            >
              <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-orange-500 via-blue-500 to-cyan-500" />
              
              <div className="p-10 border-b border-white/5 flex items-center justify-between bg-gradient-to-r from-blue-600/10 to-transparent">
                <div className="flex items-center gap-6">
                  <div className="p-5 bg-blue-600 rounded-[2rem] shadow-2xl shadow-blue-600/30">
                    <Zap className="w-8 h-8 text-white animate-pulse" />
                  </div>
                  <div>
                    <h2 className="text-2xl font-bold text-white uppercase tracking-tighter">Quantum Link Fusion (v{appVersion})</h2>
                    <p className="text-xs text-slate-400 font-mono uppercase tracking-[0.2em]">Neural Integration Bridge</p>
                  </div>
                </div>
                <button 
                   onClick={() => setShowQuantumLink(false)}
                   className="p-4 hover:bg-white/5 rounded-3xl text-slate-500 hover:text-white transition-all border border-white/5"
                >
                  <X className="w-8 h-8" />
                </button>
              </div>

              {/* Navigation Tabs */}
              <div className="px-10 py-6 bg-black/20 border-b border-white/5 flex items-center gap-4">
                <button 
                  onClick={() => setGuideTab('blender')}
                  className={`flex-1 flex items-center justify-center gap-3 py-4 rounded-2xl text-xs font-bold uppercase tracking-widest transition-all border ${
                    guideTab === 'blender' 
                    ? 'bg-orange-600/20 border-orange-500/50 text-orange-400 shadow-lg shadow-orange-600/10' 
                    : 'bg-white/5 border-white/5 text-slate-500 hover:text-white'
                  }`}
                >
                  <Cube className="w-4 h-4" /> Blender Guide
                </button>
                <button 
                  onClick={() => setGuideTab('unity')}
                  className={`flex-1 flex items-center justify-center gap-3 py-4 rounded-2xl text-xs font-bold uppercase tracking-widest transition-all border ${
                    guideTab === 'unity' 
                    ? 'bg-cyan-600/20 border-cyan-500/50 text-cyan-400 shadow-lg shadow-cyan-600/10' 
                    : 'bg-white/5 border-white/5 text-slate-500 hover:text-white'
                  }`}
                >
                  <Gamepad2 className="w-4 h-4" /> Unity Guide
                </button>
                <button 
                  onClick={() => setGuideTab('manual')}
                  className={`flex-1 flex items-center justify-center gap-3 py-4 rounded-2xl text-xs font-bold uppercase tracking-widest transition-all border ${
                    guideTab === 'manual' 
                    ? 'bg-purple-600/20 border-purple-500/50 text-purple-400 shadow-lg shadow-purple-600/10' 
                    : 'bg-white/5 border-white/5 text-slate-500 hover:text-white'
                  }`}
                >
                  <Code2 className="w-4 h-4" /> Quantum Terminal
                </button>
              </div>

              <div className="flex-1 overflow-y-auto p-10 space-y-12 scrollbar-thin scrollbar-thumb-white/5">
                
                {/* Multi-Modal & Status Aware Banner */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 p-6 rounded-[2.5rem] bg-gradient-to-r from-blue-600/10 to-purple-600/10 border border-white/10 shadow-2xl relative overflow-hidden group">
                  <div className="absolute top-0 right-0 p-8 opacity-5 group-hover:opacity-10 transition-opacity">
                    <BrainCircuit className="w-32 h-32 text-white" />
                  </div>
                  <div className="space-y-4 relative z-10">
                    <h3 className="text-sm font-bold text-white uppercase tracking-[0.2em] flex items-center gap-3">
                      <ImageIcon className="w-5 h-5 text-blue-400" /> Multi-Modal Neural Scan
                    </h3>
                    <p className="text-[11px] text-slate-400 leading-relaxed">
                      ИИ в режиме реального времени анализирует ваши скриншоты, GIF и изображения. Отправьте визуал в чат, и Quantum Link автоматически подготовит соответствующий скрипт или решение.
                    </p>
                    <div className="flex gap-2">
                       <span className="px-3 py-1 rounded-full bg-blue-600/20 text-blue-400 text-[9px] font-bold uppercase tracking-widest border border-blue-500/20">OCR Active</span>
                       <span className="px-3 py-1 rounded-full bg-purple-600/20 text-purple-400 text-[9px] font-bold uppercase tracking-widest border border-purple-500/20">Vision Logic 3.0</span>
                    </div>
                  </div>
                  <div className="space-y-4 relative z-10">
                    <h3 className="text-sm font-bold text-white uppercase tracking-[0.2em] flex items-center gap-3">
                      <RefreshCw className="w-5 h-5 text-green-400" /> Software Status Awareness
                    </h3>
                    <div className="grid grid-cols-2 gap-3">
                      <div className="p-3 rounded-2xl bg-black/40 border border-white/5 flex items-center gap-3">
                        <div className={`w-2 h-2 rounded-full ${unityStatus?.is_running ? 'bg-green-500 shadow-[0_0_10px_rgba(34,197,94,0.5)]' : 'bg-red-500'}`} />
                        <div className="flex flex-col">
                          <span className="text-[10px] font-bold text-white uppercase">Unity</span>
                          <span className="text-[8px] text-slate-500">{unityStatus?.version || 'Unknown'}</span>
                        </div>
                      </div>
                      <div className="p-3 rounded-2xl bg-black/40 border border-white/5 flex items-center gap-3">
                        <div className={`w-2 h-2 rounded-full ${blenderStatus?.is_running ? 'bg-green-500 shadow-[0_0_10px_rgba(34,197,94,0.5)]' : 'bg-red-500'}`} />
                        <div className="flex flex-col">
                          <span className="text-[10px] font-bold text-white uppercase">Blender</span>
                          <span className="text-[8px] text-slate-500">{blenderStatus?.version || 'Unknown'}</span>
                        </div>
                      </div>
                      <div className="p-3 rounded-2xl bg-black/40 border border-white/5 flex items-center gap-3">
                        <div className="w-2 h-2 rounded-full bg-slate-500" />
                        <div className="flex flex-col">
                          <span className="text-[10px] font-bold text-white uppercase">GIMP</span>
                          <span className="text-[8px] text-slate-500">v2.10.x</span>
                        </div>
                      </div>
                      <div className="p-3 rounded-2xl bg-black/40 border border-white/5 flex items-center gap-3">
                        <div className="w-2 h-2 rounded-full bg-slate-500" />
                        <div className="flex flex-col">
                          <span className="text-[10px] font-bold text-white uppercase">Redot</span>
                          <span className="text-[8px] text-slate-500">v4.x</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                
                {guideTab === 'blender' ? (
                  <motion.div 
                    key="blender"
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="space-y-12"
                  >
                    {/* Header Info */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-center">
                      <div className="space-y-6">
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-orange-600/10 border border-orange-500/20 text-[10px] font-bold text-orange-400 uppercase tracking-widest">
                          <Check className="w-3 h-3" /> Авто-определение Blender активно
                        </div>
                        <h3 className="text-3xl font-bold text-white tracking-tighter">Связь с Blender</h3>
                        <p className="text-slate-400 text-sm leading-relaxed">
                          Нейронный аддон позволяет ИИ манипулировать мешами, назначать шейдеры и строить сложные сцены прямо во время вашего диалога. Квантовый мост автоматически переводит ваши мысли в Python-скрипты.
                        </p>
                        <button 
                          onClick={() => window.open('/blender_connector.py', '_blank')}
                          className="px-8 py-4 bg-orange-600 text-white rounded-2xl text-xs font-bold uppercase tracking-widest hover:bg-orange-500 transition-all shadow-xl shadow-orange-600/20 flex items-center gap-3"
                        >
                          <Cube className="w-5 h-5" /> Скачать blender_connector.py
                        </button>
                      </div>
                      <div className="relative group">
                        <div className="absolute inset-0 bg-blue-500/10 blur-[100px] rounded-full group-hover:bg-blue-500/20 transition-all" />
                        <div className="p-8 rounded-[3rem] bg-black/40 border border-white/5 relative z-10 flex flex-col items-center justify-center text-center gap-4">
                          <div className="w-20 h-20 bg-orange-600/20 rounded-3xl flex items-center justify-center text-orange-400">
                            <Cpu className="w-10 h-10 animate-pulse" />
                          </div>
                          <span className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Quantum Engine Processing</span>
                        </div>
                      </div>
                    </div>

                    {/* Steps Implementation */}
                    <div className="space-y-8">
                      <h4 className="text-xs font-bold text-slate-500 uppercase tracking-widest">Пошаговый процесс установки:</h4>
                      <div className="grid grid-cols-1 gap-6">
                        {[
                          { title: "Загрузка", text: "Скачайте .py файл по кнопке выше и сохраните его.", icon: <Folder className="w-5 h-5" /> },
                          { title: "Инсталляция", text: "В Blender: Edit -> Preferences -> Add-ons -> Install. Выберите ваш файл.", icon: <Settings className="w-5 h-5" /> },
                          { title: "Активация", text: "Поставьте галочку напротив 'AI Assistant Link'.", icon: <Check className="w-5 h-5" /> },
                          { title: "Рабочая панель", text: "Нажмите 'N' во Viewport. Вкладка 'AI Assistant' появится в правой боковой панели приложения.", icon: <Layers className="w-5 h-5" /> },
                          { title: "Творчество", text: "Введите запрос (например: 'Создай город') и нажмите 'Manifest Code'. ИИ сгенерирует меши и материалы. Также вы можете использовать вкладку Quantum Terminal для ручного получения кода.", icon: <Sparkles className="w-5 h-5" /> },
                        ].map((step, idx) => (
                          <motion.div 
                            key={idx}
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: idx * 0.1 }}
                            className="flex items-start gap-6 group"
                          >
                            <div className="w-12 h-12 flex-shrink-0 bg-white/5 border border-white/10 rounded-2xl flex items-center justify-center text-orange-400 group-hover:bg-orange-600/20 group-hover:border-orange-500 transition-all">
                              {step.icon}
                            </div>
                            <div>
                                <h5 className="text-sm font-bold text-white uppercase tracking-tight mb-1">Шаг {idx+1}: {step.title}</h5>
                                <p className="text-xs text-slate-500 leading-relaxed">{step.text}</p>
                            </div>
                          </motion.div>
                        ))}
                      </div>
                    </div>
                  </motion.div>
                ) : guideTab === 'unity' ? (
                  <motion.div 
                    key="unity"
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="space-y-12"
                  >
                     {/* Header Info */}
                     <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-center">
                      <div className="space-y-6">
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-cyan-600/10 border border-cyan-500/20 text-[10px] font-bold text-cyan-400 uppercase tracking-widest">
                          <Check className="w-3 h-3" /> Unity 2021+ Engine Connected
                        </div>
                        <h3 className="text-3xl font-bold text-white tracking-tighter">Связь с Unity</h3>
                        <p className="text-slate-400 text-sm leading-relaxed">
                          Quantum Connector создает мост между нейросетью и Unity Editor. ИИ пишет C# скрипты, настраивает компоненты и управляет иерархией вашей игры в реальном времени.
                        </p>
                        <button 
                          onClick={() => window.open('/UnityConnector.cs', '_blank')}
                          className="px-8 py-4 bg-cyan-600 text-white rounded-2xl text-xs font-bold uppercase tracking-widest hover:bg-cyan-500 transition-all shadow-xl shadow-cyan-600/20 flex items-center gap-3"
                        >
                          <Gamepad2 className="w-5 h-5" /> Скачать UnityConnector.cs
                        </button>
                      </div>
                      <div className="relative group">
                        <div className="absolute inset-0 bg-cyan-500/10 blur-[100px] rounded-full group-hover:bg-cyan-500/20 transition-all" />
                        <div className="p-8 rounded-[3rem] bg-black/40 border border-white/5 relative z-10 flex flex-col items-center justify-center text-center gap-4">
                          <div className="w-20 h-20 bg-cyan-600/20 rounded-3xl flex items-center justify-center text-cyan-400">
                            <Database className="w-10 h-10 animate-bounce" />
                          </div>
                          <span className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Neural C# Compiler Ready</span>
                        </div>
                      </div>
                    </div>

                    {/* Steps Implementation */}
                    <div className="space-y-8">
                      <h4 className="text-xs font-bold text-slate-500 uppercase tracking-widest">Пошаговая инструкция для Unity:</h4>
                      <div className="grid grid-cols-1 gap-6">
                        {[
                          { title: "Подготовка папок", text: "В вашем проекте Unity (окно Project), зайдите в Assets и создайте там папку с именем 'Editor'. Это критически важно.", icon: <Folder className="w-5 h-5 text-cyan-400" /> },
                          { title: "Загрузка скрипта", text: "Скачайте UnityConnector.cs и переместите его прямо в созданную папку Assets/Editor.", icon: <Send className="w-5 h-5 text-cyan-400" /> },
                          { title: "Компиляция", text: "Подождите несколько секунд, пока Unity скомпилирует скрипт. В верхнем меню появится пункт 'AI Assistant'.", icon: <RefreshCw className="w-5 h-5 text-cyan-400" /> },
                          { title: "Запуск Quantum Window", text: "Перейдите в AI Assistant -> Quantum Singularity Window. В открывшемся окне вы увидите поле ввода и кнопку 'Manifest Code'.", icon: <Zap className="w-5 h-5 text-cyan-400" /> },
                          { title: "Написание запросов", text: "Пишите запрос прямо в окне Unity. Например: 'Создай игрока с CharacterController'. ИИ обработает это и выведет C# код. Нужен чистый код без аддона? Перейдите во вкладку Quantum Terminal.", icon: <Code className="w-5 h-5 text-cyan-400" /> },
                        ].map((step, idx) => (
                          <motion.div 
                            key={idx}
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: idx * 0.1 }}
                            className="flex items-start gap-6 group"
                          >
                            <div className="w-12 h-12 flex-shrink-0 bg-white/5 border border-white/10 rounded-2xl flex items-center justify-center text-cyan-400 group-hover:bg-cyan-600/20 group-hover:border-cyan-500 transition-all">
                              {step.icon}
                            </div>
                            <div>
                                <h5 className="text-sm font-bold text-white uppercase tracking-tight mb-1">Шаг {idx+1}: {step.title}</h5>
                                <p className="text-xs text-slate-500 leading-relaxed">{step.text}</p>
                            </div>
                          </motion.div>
                        ))}
                      </div>
                    </div>
                  </motion.div>
                ) : (
                  <motion.div 
                    key="manual"
                    initial={{ opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="space-y-8"
                  >
                    <div className="p-8 rounded-[2.5rem] bg-purple-600/5 border border-purple-500/20 space-y-6">
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-xl font-bold text-white uppercase tracking-tight">Quantum Terminal (Manual Mode)</h3>
                          <p className="text-xs text-purple-400 uppercase tracking-widest font-mono">Neural Code Manifistation Tool</p>
                        </div>
                        <div className="flex bg-black/40 p-1 rounded-xl border border-white/5">
                          <button 
                            onClick={() => setManualTarget('blender')}
                            className={`px-4 py-2 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all ${manualTarget === 'blender' ? 'bg-orange-600 text-white' : 'text-slate-500 hover:text-white'}`}
                          >Blender</button>
                          <button 
                            onClick={() => setManualTarget('unity')}
                            className={`px-4 py-2 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all ${manualTarget === 'unity' ? 'bg-cyan-600 text-white' : 'text-slate-500 hover:text-white'}`}
                          >Unity</button>
                        </div>
                      </div>

                      <div className="space-y-4">
                        <textarea 
                          value={manualPrompt}
                          onChange={(e) => setManualPrompt(e.target.value)}
                          placeholder="Введите запрос или прикрепите скриншот для генерации кода..."
                          className="w-full h-32 bg-black/40 border border-white/10 rounded-2xl p-6 text-sm text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500/50 resize-none transition-all"
                        />
                        
                        {/* Mind Link: AI Suggestions */}
                        <div className="space-y-3">
                          <h4 className="text-[10px] font-bold text-purple-400 uppercase tracking-widest flex items-center gap-2">
                             <Sparkles className="w-3 h-3" /> Mind Link: ИИ предлагает варианты:
                          </h4>
                          <div className="flex flex-wrap gap-2">
                             {[
                               "Создать скрипт по скриншоту ошибки",
                               "Оптимизировать текущую сцену",
                               "Добавить процедурную анимацию",
                               "Генерация UI по наброску",
                               "Исправить баги в последнем изменении"
                             ].map((suggestion, si) => (
                               <button 
                                 key={si}
                                 onClick={() => setManualPrompt(suggestion)}
                                 className="px-3 py-1.5 rounded-lg bg-white/5 border border-white/10 text-[9px] text-slate-400 hover:text-white hover:bg-white/10 transition-all uppercase tracking-widest"
                               >
                                 {suggestion}
                               </button>
                             ))}
                          </div>
                        </div>

                        {attachedFiles.length > 0 && (
                          <div className="flex flex-wrap gap-3">
                            {attachedFiles.map((file, i) => (
                              <div key={i} className="relative group w-20 h-20">
                                <img src={file.url} className="w-full h-full object-cover rounded-xl border border-white/10" alt="attach" />
                                <button 
                                  onClick={() => setAttachedFiles(prev => prev.filter((_, idx) => idx !== i))}
                                  className="absolute -top-2 -right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-all"
                                >
                                  <X className="w-3 h-3" />
                                </button>
                              </div>
                            ))}
                          </div>
                        )}

                        <div className="flex gap-4">
                          <button 
                            onClick={() => fileInputRef.current?.click()}
                            className="px-6 py-4 bg-white/5 hover:bg-white/10 border border-white/5 rounded-2xl text-slate-400 hover:text-white transition-all flex items-center justify-center gap-2"
                          >
                            <Paperclip className="w-4 h-4" /> Прикрепить
                          </button>
                          <button 
                            onClick={handleManualGenerateCode}
                            disabled={isManualGenerating || (!manualPrompt.trim() && attachedFiles.length === 0)}
                            className="flex-1 py-4 bg-purple-600 hover:bg-purple-500 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-2xl font-bold uppercase tracking-widest text-xs transition-all shadow-xl shadow-purple-600/20 flex items-center justify-center gap-3"
                          >
                            {isManualGenerating ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Zap className="w-4 h-4" />}
                            {isManualGenerating ? 'Нейронный синтез...' : 'Сгенерировать код'}
                          </button>
                        </div>
                      </div>

                      {manualResultCode && (
                        <motion.div 
                          initial={{ opacity: 0, y: 10 }}
                          animate={{ opacity: 1, y: 0 }}
                          className="space-y-4"
                        >
                          <div className="flex items-center justify-between">
                            <span className="text-[10px] font-bold text-purple-400 uppercase tracking-widest font-mono">Результат: {manualTarget.toUpperCase()} {manualTarget === 'blender' ? 'PYTHON' : 'C#'}</span>
                            <button 
                              onClick={() => {
                                navigator.clipboard.writeText(manualResultCode);
                                showNotification("Код скопирован в буфер обмена!", "success");
                              }}
                              className="flex items-center gap-2 px-3 py-1 bg-white/5 hover:bg-white/10 border border-white/10 rounded-lg text-[10px] text-white transition-all"
                            >
                              <Copy className="w-3 h-3" /> Copy Code
                            </button>
                          </div>
                          <pre className="p-6 bg-black text-xs text-purple-300 font-mono overflow-x-auto rounded-2xl border border-white/5 max-h-72 scrollbar-thin scrollbar-thumb-white/10">
                            {manualResultCode}
                          </pre>
                        </motion.div>
                      )}
                    </div>
                    
                    <div className="p-6 rounded-2xl bg-white/5 border border-white/5 flex gap-4">
                      <HelpCircle className="w-5 h-5 text-slate-500 shrink-0" />
                      <p className="text-[10px] text-slate-500 leading-relaxed italic">
                        Режим Manual Quantum Terminal генерирует код без отправки в аддоны. Используйте это, чтобы вручную копировать и настраивать скрипты прямо в вашей среде разработки. ИИ всё еще видит контекст нашего чата!
                      </p>
                    </div>
                  </motion.div>
                )}

                {/* Common Neural Memory Feature */}
                <div className="p-8 rounded-3xl bg-gradient-to-r from-purple-600/20 to-blue-600/20 border border-purple-500/30 flex gap-6 relative overflow-hidden group">
                  <div className="absolute top-0 right-0 p-4 opacity-10">
                    <Sparkles className="w-20 h-20 text-purple-400 group-hover:rotate-12 transition-transform" />
                  </div>
                  <div className="w-14 h-14 bg-purple-600 rounded-2xl flex items-center justify-center flex-shrink-0 shadow-lg shadow-purple-600/20">
                    <BrainCircuit className="w-8 h-8 text-white" />
                  </div>
                  <div className="relative z-10">
                    <h5 className="text-sm font-bold text-white uppercase tracking-tight mb-2 flex items-center gap-2">
                       Neural Memory Integration <span className="text-[8px] bg-purple-500 px-2 py-0.5 rounded-full text-white">Advanced</span>
                    </h5>
                    <p className="text-xs text-purple-200/70 leading-relaxed mb-3">
                      Ваш чат теперь напрямую связан с Quantum Link! ИИ запоминает последние 5 сообщений из браузера и использует их при генерации кода в Blender или Unity. 
                    </p>
                    <div className="flex items-center gap-4 text-[10px] text-purple-400/80 font-mono">
                       <span className="flex items-center gap-1"><Check className="w-3 h-3"/> ЧАТ</span>
                       <ChevronRight className="w-3 h-3 text-white/20"/>
                       <span className="flex items-center gap-1"><Check className="w-3 h-3"/> МОЗГ</span>
                       <ChevronRight className="w-3 h-3 text-white/20"/>
                       <span className="flex items-center gap-1"><Check className="w-3 h-3"/> ГЕЙМДЕВ</span>
                    </div>
                  </div>
                </div>

                {/* Common Warning */}
                <div className="p-8 rounded-3xl bg-blue-600/10 border border-blue-500/20 flex gap-6">
                  <div className="w-12 h-12 bg-blue-600 rounded-2xl flex items-center justify-center flex-shrink-0 animate-pulse">
                    <Info className="w-6 h-6 text-white" />
                  </div>
                  <div>
                    <h5 className="text-sm font-bold text-white uppercase tracking-tight mb-1">Золотое правило Квантовой Связи</h5>
                    <p className="text-xs text-blue-300/70 leading-relaxed">
                      Для того чтобы Quantum Link работал, это окно браузера должно оставаться открытым. Оно работает как «Ценральный Процессор» и передает данные между вашим ИИ и игровым движком.
                    </p>
                  </div>
                </div>

              </div>

              <div className="p-10 bg-black/40 flex justify-end gap-4 border-t border-white/5">
                <div className="flex-1 flex items-center gap-4 px-6 text-[10px] text-slate-600 uppercase tracking-widest font-mono">
                  <span className="flex h-2 w-2 relative">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2 w-2 bg-green-500"></span>
                  </span>
                  Neural Bridge v{appVersion} Active
                </div>
                <button 
                  onClick={() => setShowQuantumLink(false)}
                  className="px-10 py-4 bg-white text-black hover:bg-blue-600 hover:text-white rounded-2xl text-xs font-bold uppercase tracking-widest transition-all shadow-xl shadow-white/10"
                >
                  Закрыть и начать работу
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* VK Cover Generator Modal */}
      <AnimatePresence>
        {showVKGenerator && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[110] flex items-center justify-center bg-black/90 backdrop-blur-xl p-4"
          >
            <motion.div 
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="bg-[#0f0f12] border border-white/10 rounded-[2.5rem] w-full max-w-6xl max-h-[90vh] overflow-hidden flex flex-col shadow-2xl"
            >
              {/* Header */}
              <div className="p-8 border-b border-white/5 flex items-center justify-between bg-gradient-to-r from-blue-600/10 to-purple-600/10">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400 shadow-xl shadow-blue-500/10">
                    <ImageIcon className="w-8 h-8" />
                  </div>
                  <div>
                    <h3 className="text-2xl font-black text-white uppercase tracking-tighter italic">Генератор Обложек VK v17.12.3</h3>
                    <p className="text-xs text-slate-500 uppercase tracking-[0.2em] font-bold">Континент Судьбы • Умный Синтез</p>
                  </div>
                </div>
                <button 
                  onClick={() => setShowVKGenerator(false)}
                  className="p-3 hover:bg-white/10 rounded-2xl text-slate-400 transition-all hover:rotate-90"
                >
                  <X className="w-8 h-8" />
                </button>
              </div>

              <div className="flex-1 overflow-hidden flex flex-col lg:flex-row">
                {/* Left Controls */}
                <div className="w-full lg:w-[380px] p-8 border-b lg:border-b-0 lg:border-r border-white/5 space-y-10 overflow-y-auto scrollbar-thin scrollbar-thumb-white/5 bg-black/20">
                  <div className="space-y-6">
                    <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.3em]">1. Тип обложки</h4>
                    <div className="grid grid-cols-2 gap-3">
                      <button 
                        onClick={() => setVkType('static')}
                        className={`p-5 rounded-2xl border transition-all text-left group ${
                          vkType === 'static' ? 'bg-blue-600 border-blue-500 shadow-lg shadow-blue-900/40' : 'bg-white/5 border-white/10 hover:border-white/20'
                        }`}
                      >
                        <div className="text-[10px] font-black uppercase tracking-wider mb-1">Статичная</div>
                        <div className="text-[8px] opacity-60 font-mono tracking-widest">1590x400 (ПК)</div>
                      </button>
                      <button 
                        onClick={() => setVkType('live')}
                        className={`p-5 rounded-2xl border transition-all text-left group ${
                          vkType === 'live' ? 'bg-purple-600 border-purple-500 shadow-lg shadow-purple-900/40' : 'bg-white/5 border-white/10 hover:border-white/20'
                        }`}
                      >
                        <div className="text-[10px] font-black uppercase tracking-wider mb-1">Живая</div>
                        <div className="text-[8px] opacity-60 font-mono tracking-widest">1080x1920 (Моб)</div>
                      </button>
                    </div>
                  </div>

                  <div className="space-y-6">
                    <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.3em]">2. Пресеты «Континент судьбы»</h4>
                    <div className="grid grid-cols-1 gap-2">
                      {[
                        { name: "Заглавный экран", prompt: "Парящие острова в небе, величественная секта культиваторов, золотой замок Континент Судьбы, эфирная атмосфера, фэнтези живопись, яркие магические частицы" },
                        { name: "Выбор героя", prompt: "Три мастера культивации: Воин (огненная ци), Лучник (призрачная энергия), Маг (лазурная аура), рунические круги под ногами, стиль китайского фэнтези xianxia" },
                        { name: "Битва рас", prompt: "Эпическое столкновение Орков и Эльфов в мире культивации, магические мечи в воздухе, всплески энергии, стилизованный 2D концепт-арт" },
                        { name: "Магия и руны", prompt: "Крупный план формирования золотого ядра или древней руны, светящиеся иероглифы, магические нити силы, высокое качество текстур кисти" },
                        { name: "Финал: Императоры", prompt: "Тронный зал Императора Небес, золотые и черные тона, герой против божественной сущности, невероятная мощь, концепт-арт топ-уровня" }
                      ].map((preset, i) => (
                        <button
                          key={i}
                          onClick={() => setVkPrompt(preset.prompt)}
                          className="w-full p-4 bg-white/5 hover:bg-white/10 border border-white/10 rounded-2xl text-[10px] font-bold text-slate-400 hover:text-white text-left transition-all flex items-center justify-between group"
                        >
                          <span className="uppercase tracking-widest">{preset.name}</span>
                          <ChevronRight className="w-3 h-3 translate-x-[-4px] opacity-0 group-hover:opacity-100 group-hover:translate-x-0 transition-all text-blue-400" />
                        </button>
                      ))}
                    </div>
                  </div>

                  <div className="space-y-6">
                    <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.3em]">3. Промпт Манифестации</h4>
                    <div className="space-y-6">
                      <textarea 
                        value={vkPrompt}
                        onChange={(e) => setVkPrompt(e.target.value)}
                        placeholder="Опишите видение обложки..."
                        className="w-full bg-black/40 border border-white/10 rounded-3xl p-5 text-sm text-white focus:outline-none focus:border-blue-500/50 min-h-[140px] transition-all resize-none shadow-inner"
                      />
                      <button 
                        onClick={handleGenerateVKCovers}
                        disabled={isGeneratingVK || !vkPrompt.trim()}
                        className={`w-full py-5 rounded-3xl flex items-center justify-center gap-4 transition-all font-black uppercase text-[11px] tracking-[0.2em] shadow-2xl ${
                          isGeneratingVK || !vkPrompt.trim() 
                          ? 'bg-slate-800/50 text-slate-600 border border-white/5' 
                          : 'bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:scale-[1.02] active:scale-[0.98]'
                        }`}
                      >
                        {isGeneratingVK ? (
                          <>
                            <RefreshCw className="w-5 h-5 animate-spin" />
                            Синтез подождите...
                          </>
                        ) : (
                          <>
                            <Sparkles className="w-5 h-5" />
                            Генерировать 6 фото
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                </div>

                {/* Right Results Grid */}
                <div className="flex-1 p-10 bg-[#0a0a0c] overflow-y-auto scrollbar-thin scrollbar-thumb-white/5">
                  {vkResults.length === 0 && !isGeneratingVK && (
                    <div className="h-full flex flex-col items-center justify-center text-center space-y-6 opacity-20 group">
                      <div className="p-10 bg-white/5 rounded-full border border-white/10 group-hover:scale-110 transition-transform duration-1000">
                        <ImageIcon className="w-20 h-20" />
                      </div>
                      <p className="text-sm uppercase font-black tracking-[0.4em] italic">Manifestation Hub Empty</p>
                    </div>
                  )}

                  <div className={`grid gap-6 ${vkType === 'live' ? 'grid-cols-2 lg:grid-cols-3' : 'grid-cols-1'}`}>
                    {isGeneratingVK && Array.from({ length: 3 }).map((_, i) => (
                      <div key={i} className={`animate-pulse bg-white/5 border border-white/10 rounded-3xl flex items-center justify-center shadow-2xl ${vkType === 'live' ? 'aspect-[9/16]' : 'aspect-[15.9/4]'}`}>
                        <div className="flex flex-col items-center gap-4">
                          <RefreshCw className="w-10 h-10 text-slate-800 animate-spin" />
                          <div className="h-2 w-24 bg-white/10 rounded-full" />
                        </div>
                      </div>
                    ))}

                    {vkResults.map((res: any) => (
                      <VKImageCard 
                        key={res.id} 
                        res={res} 
                        type={vkType} 
                        showNotification={showNotification} 
                        onZoom={(url) => setSelectedImage(url)}
                      />
                    ))}
                  </div>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Image Zoom Lightbox */}
      <AnimatePresence>
        {selectedImage && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setSelectedImage(null)}
            className="fixed inset-0 z-[200] bg-black/95 backdrop-blur-xl flex flex-col items-center justify-center p-4 cursor-zoom-out"
          >
            <div className="absolute top-8 left-8 right-8 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center shadow-lg">
                  <Sparkles className="w-5 h-5 text-white" />
                </div>
                <div>
                   <h4 className="text-white font-bold uppercase tracking-tight">Просмотр ассета</h4>
                   <p className="text-[10px] text-slate-500 uppercase tracking-widest font-black">Континент Судьбы v17.12.3</p>
                </div>
              </div>
              <button 
                onClick={(e) => { e.stopPropagation(); setSelectedImage(null); }}
                className="p-4 bg-white/5 hover:bg-red-500 hover:text-white border border-white/10 rounded-2xl text-slate-400 transition-all flex items-center gap-3 uppercase font-black text-[10px] tracking-widest"
              >
                <X className="w-5 h-5" />
                Вернуться к выбору
              </button>
            </div>

            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className={`max-w-[90vw] max-h-[75vh] rounded-[2rem] overflow-hidden border border-white/10 shadow-2xl relative ${vkType === 'live' ? 'aspect-[9/16] h-full' : 'aspect-[15.9/4] w-full'}`}
              onClick={(e) => e.stopPropagation()}
            >
              <img 
                src={selectedImage} 
                alt="Selected asset" 
                className="w-full h-full object-contain bg-black/50"
                referrerPolicy="no-referrer"
              />
            </motion.div>

            <div className="mt-8 flex gap-4">
               <a 
                href={selectedImage}
                download="vk_cover.jpg"
                target="_blank"
                rel="noopener noreferrer"
                onClick={(e) => e.stopPropagation()}
                className="px-8 py-4 bg-blue-600 hover:bg-blue-500 text-white rounded-2xl font-black uppercase text-xs tracking-widest transition-all flex items-center gap-3 shadow-xl shadow-blue-600/20"
               >
                 <Download className="w-5 h-5" />
                 Скачать оригинал
               </a>
               <button 
                onClick={(e) => { e.stopPropagation(); setSelectedImage(null); }}
                className="px-8 py-4 bg-white/5 hover:bg-white/10 border border-white/10 text-white rounded-2xl font-black uppercase text-xs tracking-widest transition-all"
               >
                 Закрыть
               </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Settings Modal */}
      <AnimatePresence>
        {showSettings && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-6"
          >
            <motion.div 
              initial={{ scale: 0.9, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.9, opacity: 0, y: 20 }}
              className="bg-slate-900 border border-white/10 rounded-[2.5rem] w-full max-w-lg overflow-hidden shadow-2xl"
            >
              <div className="p-8 border-b border-white/5 flex items-center justify-between bg-white/5">
                <div className="flex items-center gap-4">
                  <button 
                    onClick={() => setShowSettings(false)}
                    className="p-2 hover:bg-white/5 rounded-lg text-slate-500 hover:text-white transition-all sm:hidden"
                  >
                    <ChevronLeft className="w-5 h-5" />
                  </button>
                  <div className="p-3 bg-blue-600/20 rounded-2xl">
                    <Settings className="w-6 h-6 text-blue-400" />
                  </div>
                  <div>
                    <h2 className="text-xl font-bold text-white">Локальное хранение</h2>
                    <p className="text-xs text-slate-500 uppercase tracking-widest mt-1">Настройка пути для обучения ИИ</p>
                  </div>
                </div>
                <button 
                  onClick={() => setShowSettings(false)}
                  className="p-2 hover:bg-white/5 rounded-xl transition-colors text-slate-500 hover:text-white"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
              
              <div className="p-8 space-y-6">
                <div className="space-y-3">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
                    <Gamepad2 className="w-4 h-4 text-green-400" /> Путь к проекту Unity
                  </label>
                  <input 
                    type="text"
                    value={projectPathInput}
                    onChange={(e) => setProjectPathInput(e.target.value)}
                    placeholder="Например: C:\MyUnityProject"
                    className="w-full bg-black/40 border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-slate-700 focus:outline-none focus:border-green-500/50 transition-all font-mono text-sm"
                  />
                </div>

                <div className="space-y-3">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
                    <ImageIcon className="w-4 h-4 text-orange-400" /> Путь к GIMP (.exe)
                  </label>
                  <input 
                    type="text"
                    value={gimpPathInput}
                    onChange={(e) => setGimpPathInput(e.target.value)}
                    placeholder="C:\...\gimp-3.exe"
                    className="w-full bg-black/40 border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-slate-700 focus:outline-none focus:border-orange-500/50 transition-all font-mono text-sm"
                  />
                </div>

                <div className="space-y-3">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
                    <Zap className="w-4 h-4 text-cyan-400" /> Путь к Redot/Godot (.exe)
                  </label>
                  <input 
                    type="text"
                    value={redotPathInput}
                    onChange={(e) => setRedotPathInput(e.target.value)}
                    placeholder="D:\...\Redot.exe"
                    className="w-full bg-black/40 border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-slate-700 focus:outline-none focus:border-cyan-500/50 transition-all font-mono text-sm"
                  />
                </div>

                <div className="space-y-3">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
                    <Box className="w-4 h-4 text-purple-400" /> Версия Blender
                  </label>
                  <input 
                    type="text"
                    value={blenderVersionInput}
                    onChange={(e) => setBlenderVersionInput(e.target.value)}
                    placeholder="Например: 4.1.0"
                    className="w-full bg-black/40 border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-slate-700 focus:outline-none focus:border-purple-500/50 transition-all font-mono text-sm"
                  />
                </div>

                <div className="space-y-3">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
                    <Folder className="w-4 h-4 text-blue-400" /> Путь сохранения файлов
                  </label>
                  <input 
                    type="text"
                    value={localPathInput}
                    onChange={(e) => setLocalPathInput(e.target.value)}
                    placeholder="Например: C:\AI_Training_Data"
                    className="w-full bg-black/40 border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-slate-700 focus:outline-none focus:border-blue-500/50 transition-all font-mono text-sm"
                  />
                  <p className="text-[10px] text-slate-500 leading-relaxed italic">
                    Все файлы, которые вы присылаете в чат, будут сохраняться по этому пути для локального использования и обучения ИИ.
                  </p>
                </div>

                <div className="p-4 rounded-2xl bg-blue-600/5 border border-blue-500/10 flex gap-4">
                  <Info className="w-5 h-5 text-blue-400 shrink-0" />
                  <p className="text-[11px] text-slate-400 leading-relaxed">
                    Если интернет отсутствует, ИИ будет использовать файлы из этой директории как приоритетный источник знаний.
                  </p>
                </div>

                <div className="pt-4 border-t border-white/5">
                  <button 
                    onClick={handleGenerateBlueprint}
                    disabled={isGeneratingBlueprint}
                    className="w-full flex items-center justify-center gap-3 px-6 py-4 rounded-2xl bg-slate-800 hover:bg-slate-700 text-white font-bold text-xs uppercase tracking-widest transition-all border border-white/5 disabled:opacity-50"
                  >
                    <RefreshCw className={`w-4 h-4 ${isGeneratingBlueprint ? 'animate-spin' : ''}`} />
                    {isGeneratingBlueprint ? 'Генерация...' : 'Обновить Master Blueprint'}
                  </button>
                  <p className="text-[9px] text-slate-600 mt-2 text-center uppercase tracking-tighter">
                    Создает файл PROJECT_MASTER_BLUEPRINT.md со всеми данными проекта
                  </p>
                </div>
              </div>

              <div className="p-8 bg-black/20 flex gap-4">
                <button 
                  onClick={() => setShowSettings(false)}
                  className="flex-1 px-6 py-4 rounded-2xl bg-white/5 text-white font-bold text-sm hover:bg-white/10 transition-all border border-white/5"
                >
                  Отмена
                </button>
                <button 
                  onClick={handleSaveSettings}
                  className="flex-1 px-6 py-4 rounded-2xl bg-blue-600 text-white font-bold text-sm hover:bg-blue-500 transition-all shadow-lg shadow-blue-600/20"
                >
                  Сохранить
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Global CSS for Markdown */}
      <style>{`
        .markdown-body pre {
          background: #121214 !important;
          padding: 1.25rem !important;
          border-radius: 1rem !important;
          border: 1px solid rgba(255, 255, 255, 0.05) !important;
          overflow-x: auto !important;
          margin: 1.25rem 0 !important;
        }
        .markdown-body code {
          font-family: 'JetBrains Mono', monospace !important;
          font-size: 0.85rem !important;
          color: #60a5fa !important;
        }
        .markdown-body p {
          margin-bottom: 1.25rem !important;
        }
        .markdown-body ul {
          list-style-type: disc !important;
          padding-left: 1.5rem !important;
          margin-bottom: 1.25rem !important;
        }
        .markdown-body h1, .markdown-body h2, .markdown-body h3 {
          color: white !important;
          font-weight: 700 !important;
          margin-top: 2rem !important;
          margin-bottom: 1rem !important;
        }
        .markdown-body a {
          color: #3b82f6 !important;
          text-decoration: underline !important;
        }
      `}</style>
    </div>
  );
}
