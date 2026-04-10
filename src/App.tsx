import React, { useState, useEffect, useRef } from 'react';
import { 
  Cpu, 
  Code, 
  Box, 
  Zap, 
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
  AlertTriangle,
  ExternalLink,
  BookOpen,
  GitBranch,
  Type,
  FileCode,
  Trash2,
  Database
} from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { GoogleGenAI } from "@google/genai";
import Markdown from 'react-markdown';

// --- Types ---
interface Message {
  role: 'user' | 'assistant';
  content: string;
  timestamp: number;
  files?: any[];
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

// --- App Component ---
export default function App() {
  const [kb, setKb] = useState<KBData | null>(null);
  const [activeTab, setActiveTab] = useState<'chat' | 'dashboard' | 'project_info'>('chat');
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [projectScan, setProjectScan] = useState<ProjectScan | null>(null);
  const [unityStatus, setUnityStatus] = useState<UnityStatus | null>(null);
  const [blenderStatus, setBlenderStatus] = useState<BlenderStatus | null>(null);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [blenderPresets, setBlenderPresets] = useState<BlenderPreset[]>([]);
  const [isRepairing, setIsRepairing] = useState(false);
  const [isClearingChat, setIsClearingChat] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [showGithubGuide, setShowGithubGuide] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [localPathInput, setLocalPathInput] = useState('');
  const [isGeneratingBlueprint, setIsGeneratingBlueprint] = useState(false);
  const [isUpdatingKB, setIsUpdatingKB] = useState(false);
  const [showCapabilities, setShowCapabilities] = useState(false);
  const [capabilities, setCapabilities] = useState<any>(null);
  const [notification, setNotification] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);
  
  const chatEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [updateInfo, setUpdateInfo] = useState<any>(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const [updateProgress, setUpdateProgress] = useState(0);
  const [ollamaRunning, setOllamaRunning] = useState(false);

  const [showOllamaGuide, setShowOllamaGuide] = useState(false);
  const [showMigrationModal, setShowMigrationModal] = useState(false);
  const [migrationGuide, setMigrationGuide] = useState('');
  const [unityPackages, setUnityPackages] = useState<any[]>([]);
  const [isMigrating, setIsMigrating] = useState(false);

  const fetchPackagesInfo = async () => {
    try {
      const res = await fetch('/api/unity/packages-info');
      const data = await res.json();
      setUnityPackages(data);
    } catch (e) {}
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
      "Обновление версии до 13.2.0...",
      "Регенерация PROJECT_MASTER_BLUEPRINT.md..."
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
  const ai = new GoogleGenAI({ apiKey: process.env.GEMINI_API_KEY || "" });

  useEffect(() => {
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
      .then(data => setKb(data))
      .catch(err => {
        console.error("Failed to fetch KB, using fallback", err);
        setKb({
          name: "Unity AI Assistant",
          version: "13.3.0",
          description: "Гибридный ИИ-помощник",
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
      
      fetch('/api/ai/ollama-status')
        .then(res => res.json())
        .then(data => setOllamaRunning(data.isRunning));

      fetch('/api/project/history')
        .then(res => res.json())
        .then(data => setHistory(data));
      
      fetch('/api/project/scan')
        .then(res => res.json())
        .then(data => data.success && setProjectScan(data.scan));
    }, 5000);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
      clearInterval(statusInterval);
    };
  }, []);

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
    const updatedKb = { ...kb, local_training_path: localPathInput };
    try {
      const response = await fetch('/api/kb/update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedKb)
      });
      if (response.ok) {
        setKb(updatedKb);
        setShowSettings(false);
      }
    } catch (error) {
      console.error("Failed to save settings", error);
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
    if (!text.trim() || isTyping || !kb) return;

    const userMsg: Message = {
      role: 'user',
      content: text,
      timestamp: Date.now()
    };

    setMessages(prev => [...prev, userMsg]);
    setInput('');
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

      const response = await ai.models.generateContent({
        model: "gemini-3-flash-preview",
        contents: text,
        config: {
          systemInstruction: kb.system_instruction,
        },
      });

      const aiMsg: Message = {
        role: 'assistant',
        content: response.text || "Извините, я не смог сгенерировать ответ.",
        timestamp: Date.now()
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

  const handleUpdateKB = async () => {
    setIsUpdatingKB(true);
    try {
      const res = await fetch('/api/kb/update-api-refs', { method: 'POST' });
      const data = await res.json();
      if (data.success) {
        showNotification(data.message, "success");
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

  const handleRepair = async () => {
    if (isRepairing) return;
    setIsRepairing(true);
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 15000); // 15s timeout

      const res = await fetch('/api/system/repair', { 
        method: 'POST',
        signal: controller.signal
      });
      clearTimeout(timeoutId);
      
      const data = await res.json();
      if (data.success) {
        showNotification(data.message, "success");
        setTimeout(() => window.location.reload(), 1500);
      } else {
        showNotification("Ошибка: " + (data.error || "Неизвестная ошибка"), "error");
      }
    } catch (error: any) {
      if (error.name === 'AbortError') {
        showNotification("Процесс занимает много времени, он продолжится в фоне. Перезагрузите страницу через минуту.", "info");
      } else {
        showNotification("Ошибка при восстановлении системы.", "error");
      }
    } finally {
      setIsRepairing(false);
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setUploadProgress(0);

    const formData = new FormData();
    Array.from(files).forEach(f => formData.append('files', f));

    try {
      // Fake progress for visual feedback
      let progress = 0;
      const interval = setInterval(() => {
        progress += Math.random() * 15;
        if (progress > 95) progress = 95;
        setUploadProgress(Math.floor(progress));
      }, 300);

      const res = await fetch('/api/upload', {
        method: 'POST',
        body: formData
      });
      
      clearInterval(interval);
      setUploadProgress(100);
      
      const data = await res.json();
      if (data.success) {
        const fileMsg: Message = { 
          role: 'user', 
          content: `Загружены файлы: ${data.files.map((f: any) => f.name).join(', ')}`,
          timestamp: Date.now(),
          files: data.files
        };
        setMessages(prev => [...prev, fileMsg]);
      }
    } catch (error) {
      console.error("Upload error:", error);
    } finally {
      setTimeout(() => {
        setIsUploading(false);
        setUploadProgress(0);
      }, 500);
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
                onClick={() => {
                  fetch('/api/project/scan')
                    .then(res => res.json())
                    .then(data => data.success && setProjectScan(data.scan));
                }}
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
                <span className="text-[9px] font-mono text-slate-500">{unityStatus?.version || '---'}</span>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Cube className="w-3.5 h-3.5 text-purple-400" />
                  <span className="text-[10px] text-slate-300">Blender</span>
                </div>
                <span className="text-[9px] font-mono text-slate-500">{blenderStatus?.version || '---'}</span>
              </div>
            </div>
          </div>

          {/* System Repair Button */}
          <button 
            onClick={handleRepair}
            disabled={isRepairing}
            className={`w-full p-4 rounded-2xl border transition-all group text-left ${
              isRepairing 
              ? 'bg-red-600/20 border-red-500/50 cursor-wait' 
              : 'bg-red-600/10 border-red-500/20 hover:bg-red-600/20 hover:border-red-500/40'
            }`}
          >
            <div className="flex items-center gap-3 mb-1">
              <div className={`p-2 bg-black/40 rounded-lg group-hover:text-red-400 transition-colors ${isRepairing ? 'animate-spin' : ''}`}>
                <RefreshCw className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">
                {isRepairing ? 'Очистка...' : 'Самоочистка'}
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">Исправить ошибки и восстановить целостность системы.</p>
          </button>

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
                <Layers className="w-3.5 h-3.5" /> Дашборд
              </button>
              <button 
                onClick={() => setActiveTab('project_info')}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === 'project_info' ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' : 'text-slate-500 hover:text-slate-300'
                }`}
              >
                <Info className="w-3.5 h-3.5" /> О проекте
              </button>
            </nav>
          </div>

          <div className="flex items-center gap-4">
            <button 
              onClick={() => {
                fetchPackagesInfo();
                setShowMigrationModal(true);
              }}
              className="px-4 py-2 rounded-xl bg-orange-600/20 border border-orange-500/30 text-orange-400 hover:text-white hover:bg-orange-600 transition-all group flex items-center gap-2 shadow-lg shadow-orange-600/10"
              title="Миграция Unity"
            >
              <GitBranch className="w-4 h-4 group-hover:scale-110 transition-transform" />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">Миграция</span>
            </button>
            <button 
              onClick={handleLaunchOllama}
              className={`px-4 py-2 rounded-xl border transition-all group flex items-center gap-2 shadow-lg ${
                ollamaRunning 
                ? 'bg-cyan-600/20 border-cyan-500/50 text-cyan-400 shadow-cyan-600/20' 
                : 'bg-slate-800/20 border-white/5 text-slate-500 shadow-none'
              }`}
              title={ollamaRunning ? "Ollama активна (Offline AI готов)" : "Нажмите, чтобы проверить статус Ollama"}
            >
              <Cpu className={`w-4 h-4 group-hover:scale-110 transition-transform ${ollamaRunning ? 'animate-pulse' : ''}`} />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">
                {ollamaRunning ? 'Ollama: OK' : 'Ollama: Off'}
              </span>
            </button>
            <button 
              onClick={fetchCapabilities}
              className="px-4 py-2 rounded-xl bg-purple-600/20 border border-purple-500/30 text-purple-400 hover:text-white hover:bg-purple-600 transition-all group flex items-center gap-2 shadow-lg shadow-purple-600/10"
              title="О возможностях ИИ"
            >
              <Zap className="w-4 h-4 group-hover:scale-110 transition-transform" />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">ИИ Моде</span>
            </button>
            <button 
              onClick={checkUpdates}
              className="px-4 py-2 rounded-xl bg-green-600/20 border border-green-500/30 text-green-400 hover:text-white hover:bg-green-600 transition-all group flex items-center gap-2 shadow-lg shadow-green-600/10"
              title="Проверить обновления"
            >
              <RefreshCw className="w-4 h-4 group-hover:rotate-180 transition-transform duration-500" />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">Обновить</span>
            </button>
            <button 
              onClick={() => {
                setLocalPathInput(kb?.local_training_path || '');
                setShowSettings(true);
              }}
              className="px-4 py-2 rounded-xl bg-blue-600/20 border border-blue-500/30 text-blue-400 hover:text-white hover:bg-blue-600 transition-all group flex items-center gap-2 shadow-lg shadow-blue-600/10"
              title="Настройка локального хранилища"
            >
              <Folder className="w-4 h-4 group-hover:scale-110 transition-transform" />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">Хранилище</span>
            </button>
            <div className="flex items-center gap-2 px-3 py-1.5 bg-white/5 rounded-full border border-white/5">
              <Sparkles className="w-3 h-3 text-blue-400" />
              <span className="text-[10px] font-bold text-slate-400 uppercase">Gemini 3.0 Flash</span>
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
              
              <h2 className="text-2xl font-bold text-white mb-4 uppercase tracking-tight">Unity AI Assistant v{kb?.version || '13.3'}</h2>
              <p className="text-slate-400 text-sm leading-relaxed mb-10 max-w-lg px-4">
                Я полностью осведомлен о вашем проекте по пути <br/>
                <code className="text-blue-400 break-all bg-white/5 px-2 py-1 rounded mt-2 inline-block">
                  {kb?.project_path || 'Загрузка...'}
                </code>. 
                <br/><br/>
                Задавайте любые вопросы по Unity или Blender на русском языке. Экспертные знания C# и Python теперь интегрированы напрямую в чат.
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
                  <span className="text-[10px] font-bold text-white uppercase">Загрузка файлов...</span>
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
        <div className="p-6 bg-gradient-to-t from-[#0a0a0c] via-[#0a0a0c] to-transparent">
          <div className="max-w-4xl mx-auto relative">
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
                  disabled={!input.trim() || isTyping}
                  className={`absolute right-4 top-4 p-3 rounded-xl transition-all ${
                    input.trim() && !isTyping 
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
          ) : activeTab === 'dashboard' ? (
            <div className="flex-1 overflow-y-auto p-8 scrollbar-thin scrollbar-thumb-white/5 space-y-8">
              <div className="max-w-6xl mx-auto space-y-8">
                
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
                      <h2 className="text-xl font-bold text-white tracking-tighter">{capabilities.name}</h2>
                      <p className="text-xs text-slate-400">{capabilities.description}</p>
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
                          <Zap className="w-4 h-4 text-yellow-400" /> База знаний из {capabilities.video_knowledge_base.total_videos} видео
                        </h3>
                        <span className="text-[10px] text-slate-500 font-mono uppercase">Обновлено: {capabilities.video_knowledge_base.update_date}</span>
                      </div>
                      
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
                    </div>
                  )}

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
