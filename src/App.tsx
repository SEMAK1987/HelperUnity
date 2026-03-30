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
  ExternalLink
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
}

interface UnityStatus {
  is_running: boolean;
  version: string;
  project_path: string;
}

// --- App Component ---
export default function App() {
  const [kb, setKb] = useState<KBData | null>(null);
  const [activeTab, setActiveTab] = useState<'chat' | 'project_info'>('chat');
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [projectScan, setProjectScan] = useState<ProjectScan | null>(null);
  const [unityStatus, setUnityStatus] = useState<UnityStatus | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [showGithubGuide, setShowGithubGuide] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [localPathInput, setLocalPathInput] = useState('');
  const [isGeneratingBlueprint, setIsGeneratingBlueprint] = useState(false);
  
  const chatEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Initialize Gemini
  const ai = new GoogleGenAI({ apiKey: process.env.GEMINI_API_KEY || "" });

  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    fetch('/api/kb')
      .then(res => res.json())
      .then(data => setKb(data));

    fetch('/api/project/scan')
      .then(res => res.json())
      .then(data => data.success && setProjectScan(data.scan));

    const statusInterval = setInterval(() => {
      fetch('/api/unity/status')
        .then(res => res.json())
        .then(status => setUnityStatus(status));
      
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
  }, [messages]);

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
        alert("Master Blueprint (PROJECT_MASTER_BLUEPRINT.md) успешно обновлен!");
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
      setMessages(prev => [...prev, {
        role: 'assistant',
        content: "Ошибка: Не удалось подключиться к ИИ. Пожалуйста, проверьте ваш API ключ.",
        timestamp: Date.now()
      }]);
    } finally {
      setIsTyping(false);
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
      // Fake progress
      const interval = setInterval(() => {
        setUploadProgress(prev => Math.min(prev + 10, 90));
      }, 200);

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
              <p className="text-[10px] text-slate-500 uppercase font-mono">Full Version 1.1</p>
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
              <span className="text-[10px] font-bold text-slate-500 uppercase">Unity</span>
              <div className="flex items-center gap-1.5">
                <div className={`w-1.5 h-1.5 rounded-full ${unityStatus?.is_running ? 'bg-blue-500 animate-pulse' : 'bg-slate-700'}`} />
                <span className={`text-[9px] font-bold uppercase ${unityStatus?.is_running ? 'text-blue-400' : 'text-slate-600'}`}>
                  {unityStatus?.is_running ? 'Активен' : 'Не запущен'}
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

        {/* Content Area */}
        <div className="flex-1 overflow-hidden flex flex-col">
          {activeTab === 'chat' ? (
            <>
              {/* Messages */}
        <div className="flex-1 overflow-y-auto p-6 space-y-8 scrollbar-thin scrollbar-thumb-white/5">
          {messages.length === 0 && (
            <div className="h-full flex flex-col items-center justify-center text-center max-w-2xl mx-auto">
              <motion.div 
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                className="w-24 h-24 bg-blue-600/10 rounded-[3rem] flex items-center justify-center mb-10 border border-blue-500/20 shadow-2xl shadow-blue-600/10"
              >
                <Cpu className="w-12 h-12 text-blue-500" />
              </motion.div>
              
              <h2 className="text-2xl font-bold text-white mb-4 uppercase tracking-tight">Unity AI Assistant v12.0</h2>
              <p className="text-slate-400 text-sm leading-relaxed mb-10 max-w-lg">
                Я полностью осведомлен о вашем проекте по пути <code className="text-blue-400">C:\Users\user\Desktop\HelperUnity-main\HelperUnity-main</code>. 
                Задавайте любые вопросы по Unity или Blender на русском языке.
              </p>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 w-full">
                <div className="p-6 rounded-3xl bg-white/5 border border-white/5 text-left hover:bg-white/10 transition-all cursor-pointer group">
                  <Gamepad2 className="w-6 h-6 text-blue-400 mb-4 group-hover:scale-110 transition-transform" />
                  <div className="text-xs font-bold text-white uppercase mb-2">Unity C# Expert</div>
                  <div className="text-[11px] text-slate-500 leading-relaxed">Оптимизированный код, SOLID, лучшие практики движка и навыки мобов.</div>
                </div>
                <div className="p-6 rounded-3xl bg-white/5 border border-white/5 text-left hover:bg-white/10 transition-all cursor-pointer group">
                  <Cube className="w-6 h-6 text-purple-400 mb-4 group-hover:scale-110 transition-transform" />
                  <div className="text-xs font-bold text-white uppercase mb-2">Blender Python Expert</div>
                  <div className="text-[11px] text-slate-500 leading-relaxed">Автоматизация API bpy, процедурные инструменты и экспорт в Unity.</div>
                </div>
              </div>
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
          ) : (
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
          )}
        </div>
      </main>

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
