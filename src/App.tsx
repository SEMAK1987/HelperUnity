import React, { useState, useEffect, useRef } from 'react';
import { 
  Download, 
  ArrowRight, 
  Settings, 
  Terminal, 
  AlertTriangle, 
  CheckCircle2, 
  Database, 
  Cpu, 
  Code, 
  ShieldCheck,
  Save,
  RefreshCw,
  Layers,
  Users,
  Command as CmdIcon,
  Zap,
  HardDrive,
  Wifi,
  WifiOff,
  Search,
  ChevronRight,
  ChevronDown,
  Play,
  Square,
  Paperclip,
  FileText,
  Image as ImageIcon,
  Video,
  Music,
  X
} from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';

// Types
interface Agent {
  id: string;
  name: string;
  role: string;
  model: string;
}

interface Level {
  id: number;
  name: string;
  agents: Agent[];
}

interface Command {
  cmd: string;
  desc: string;
}

interface UnityStatus {
  is_running: boolean;
  version: string;
  project_path: string;
}

interface KBData {
  version: string;
  levels: Level[];
  commands: Command[];
  unity_status: UnityStatus;
}

interface Config {
  apiKey: string;
  version: string;
  appUrl: string;
}

export default function App() {
  const [kb, setKb] = useState<KBData | null>(null);
  const [config, setConfig] = useState<Config | null>(null);
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [logs, setLogs] = useState<string[]>([]);
  const [activeTab, setActiveTab] = useState<'studio' | 'kb' | 'commands' | 'files' | 'recovery'>('studio');
  const [showHierarchy, setShowHierarchy] = useState(false);
  const [chatInput, setChatInput] = useState('');
  const [messages, setMessages] = useState<{role: string, content: string, agent?: string, files?: any[]}[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isDragging, setIsDragging] = useState(false);
  const [uploadedFiles, setUploadedFiles] = useState<any[]>([]);
  const [recoveryBlueprint, setRecoveryBlueprint] = useState<string>('');
  const [isRecovering, setIsRecovering] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const addLog = (msg: string, type: 'info' | 'error' | 'process' | 'success' = 'info') => {
    const time = new Date().toLocaleTimeString();
    const prefix = type === 'error' ? 'ОШИБКА' : type === 'process' ? 'ПРОЦЕСС' : type === 'success' ? 'УСПЕХ' : 'ИНФО';
    setLogs(prev => [`[${time}] ${prefix}: ${msg}`, ...prev.slice(0, 49)]);
  };

  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    fetch('/api/config').then(res => res.json()).then(setConfig);
    fetch('/api/kb').then(res => res.json()).then(data => {
      setKb(data);
      if (data.levels?.[0]?.agents?.[0]) setSelectedAgent(data.levels[0].agents[0]);
    });

    addLog("Ядро CCGS активно. Ожидание подключения Unity...");
    
    const interval = setInterval(() => {
      fetch('/api/unity/status')
        .then(res => res.json())
        .then(status => {
          setKb(prev => prev ? { ...prev, unity_status: status } : null);
        });
    }, 5000);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
      clearInterval(interval);
    };
  }, []);

  const handleUpdate = async () => {
    setIsUpdating(true);
    addLog("Запрос на обновление системы...", 'process');
    try {
      const res = await fetch('/api/kb');
      const data = await res.json();
      setKb(data);
      addLog("Система успешно синхронизирована", 'success');
    } catch (error) {
      addLog("Ошибка при обновлении данных", 'error');
    } finally {
      setIsUpdating(false);
    }
  };

  const handleRecovery = async () => {
    if (!recoveryBlueprint.trim()) return;
    setIsRecovering(true);
    addLog("Запуск процесса восстановления из Blueprint...", 'process');
    try {
      const blueprint = JSON.parse(recoveryBlueprint);
      const res = await fetch('/api/recovery', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ blueprint })
      });
      const data = await res.json();
      if (data.success) {
        addLog("Проект успешно восстановлен!", 'success');
        window.location.reload();
      } else {
        addLog("Ошибка восстановления: " + data.error, 'error');
      }
    } catch (error) {
      addLog("Некорректный формат Blueprint JSON", 'error');
    } finally {
      setIsRecovering(false);
    }
  };

  const validateFiles = (files: FileList): { valid: boolean, error?: string } => {
    const fileList = Array.from(files);
    
    const videos = fileList.filter(f => f.type.startsWith('video/'));
    const images = fileList.filter(f => f.type.startsWith('image/'));
    const pngImages = images.filter(f => f.type === 'image/png' || f.name.toLowerCase().endsWith('.png'));
    const audios = fileList.filter(f => f.type.startsWith('audio/'));
    const docs = fileList.filter(f => !f.type.startsWith('video/') && !f.type.startsWith('image/') && !f.type.startsWith('audio/'));

    // Video: 1 video up to 100MB
    if (videos.length > 1) return { valid: false, error: "Можно загрузить только 1 видео за раз" };
    if (videos.length === 1 && videos[0].size > 100 * 1024 * 1024) return { valid: false, error: "Видео не должно превышать 100МБ" };

    // Photos: 10 photos (PNG) up to 200MB total
    if (images.length > 10) return { valid: false, error: "Можно загрузить до 10 изображений за раз" };
    if (images.length > 0 && pngImages.length !== images.length) {
      return { valid: false, error: "Все скриншоты должны быть в формате PNG" };
    }
    const imagesSize = images.reduce((acc, f) => acc + f.size, 0);
    if (imagesSize > 200 * 1024 * 1024) return { valid: false, error: "Общий размер изображений не должен превышать 200МБ" };

    // Docs: 5 docs up to 500MB total
    if (docs.length > 5) return { valid: false, error: "Можно загрузить до 5 документов за раз" };
    const docsSize = docs.reduce((acc, f) => acc + f.size, 0);
    if (docsSize > 500 * 1024 * 1024) return { valid: false, error: "Общий размер документов не должен превышать 500МБ" };

    // Audio: 5 audio up to 50MB total
    if (audios.length > 5) return { valid: false, error: "Можно загрузить до 5 аудиофайлов за раз" };
    const audiosSize = audios.reduce((acc, f) => acc + f.size, 0);
    if (audiosSize > 50 * 1024 * 1024) return { valid: false, error: "Общий размер аудиофайлов не должен превышать 50МБ" };

    return { valid: true };
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    const validation = validateFiles(files);
    if (!validation.valid) {
      addLog(validation.error || "Ошибка валидации файлов", 'error');
      return;
    }

    setIsUploading(true);
    setUploadProgress(0);
    addLog(`Загрузка ${files.length} файлов...`, 'process');

    const formData = new FormData();
    Array.from(files).forEach((f: File) => formData.append('files', f));

    try {
      // Simulate progress for better UX as fetch doesn't support it natively without XHR
      const progressInterval = setInterval(() => {
        setUploadProgress(prev => Math.min(prev + 10, 90));
      }, 200);

      const res = await fetch('/api/upload', {
        method: 'POST',
        body: formData
      });
      
      clearInterval(progressInterval);
      setUploadProgress(100);
      
      const data = await res.json();
      
      if (data.success) {
        addLog(`Успешно загружено ${data.files.length} файлов`, 'success');
        setUploadedFiles(prev => [...prev, ...data.files]);
        const fileMsg = { 
          role: 'user', 
          content: `Загружены файлы: ${data.files.map((f: any) => f.name).join(', ')}`,
          files: data.files
        };
        setMessages(prev => [...prev, fileMsg]);
      } else {
        addLog("Ошибка при загрузке файлов", 'error');
      }
    } catch (error) {
      addLog("Сетевая ошибка при загрузке", 'error');
    } finally {
      setTimeout(() => {
        setIsUploading(false);
        setUploadProgress(0);
      }, 500);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const files = e.dataTransfer.files;
    if (files && files.length > 0) {
      const mockEvent = { target: { files } } as unknown as React.ChangeEvent<HTMLInputElement>;
      handleFileUpload(mockEvent);
    }
  };

  const findBestAgent = (input: string): Agent | null => {
    if (!kb) return null;
    const lowerInput = input.toLowerCase();
    for (const level of kb.levels) {
      for (const agent of level.agents) {
        const keywords = agent.role.toLowerCase().split(' ');
        if (keywords.some(k => k.length > 3 && lowerInput.includes(k))) {
          return agent;
        }
      }
    }
    return kb.levels[0].agents[0];
  };

  const handleSendMessage = () => {
    if (!chatInput.trim()) return;
    
    const bestAgent = findBestAgent(chatInput);
    const userMsg = { role: 'user', content: chatInput };
    setMessages(prev => [...prev, userMsg]);
    setChatInput('');

    if (bestAgent) {
      setSelectedAgent(bestAgent);
      addLog(`Запрос передан агенту: ${bestAgent.name}`, 'info');
      
      setTimeout(() => {
        const aiMsg = { 
          role: 'assistant', 
          content: `Я проанализировал ваш запрос. Как ${bestAgent.name}, я готов помочь с этим. Начинаю выполнение...`,
          agent: bestAgent.name
        };
        setMessages(prev => [...prev, aiMsg]);
        addLog(`${bestAgent.name} приступил к работе.`, 'success');
      }, 1000);
    }
  };

  if (!kb || !config) {
    return (
      <div className="min-h-screen bg-[#070708] flex items-center justify-center">
        <RefreshCw className="w-6 h-6 text-blue-500 animate-spin" />
      </div>
    );
  }

  return (
    <div 
      className="h-screen bg-[#070708] text-slate-400 font-sans text-[11px] flex overflow-hidden relative"
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      
      {/* Drag Overlay */}
      <AnimatePresence>
        {isDragging && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="absolute inset-0 z-[200] bg-blue-600/20 backdrop-blur-md border-4 border-dashed border-blue-500 m-4 rounded-3xl flex flex-col items-center justify-center pointer-events-none"
          >
            <div className="bg-blue-600 p-6 rounded-full shadow-2xl shadow-blue-600/50 mb-4">
              <Download className="w-12 h-12 text-white animate-bounce" />
            </div>
            <h2 className="text-2xl font-bold text-white uppercase tracking-widest">Перетащите файлы сюда</h2>
            <p className="text-blue-300 mt-2">Видео, скриншоты (PNG), документы и аудио</p>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Upload Progress Overlay */}
      <AnimatePresence>
        {isUploading && (
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 20 }}
            className="absolute bottom-20 right-8 z-[150] bg-[#121214] border border-white/10 p-4 rounded-xl shadow-2xl w-64"
          >
            <div className="flex items-center justify-between mb-2">
              <span className="text-[10px] font-bold text-white uppercase">Загрузка файлов...</span>
              <span className="text-[10px] text-blue-400 font-mono">{uploadProgress}%</span>
            </div>
            <div className="h-1.5 bg-white/5 rounded-full overflow-hidden">
              <motion.div 
                className="h-full bg-blue-600"
                initial={{ width: 0 }}
                animate={{ width: `${uploadProgress}%` }}
              />
            </div>
          </motion.div>
        )}
      </AnimatePresence>
      
      {/* Mini Sidebar */}
      <aside className="w-12 border-r border-white/5 bg-black/40 flex flex-col items-center py-4 gap-4">
        <div className="w-8 h-8 bg-blue-600 rounded flex items-center justify-center shadow-lg shadow-blue-600/20 mb-2">
          <Cpu className="w-4 h-4 text-white" />
        </div>
        
        <div className="flex flex-col gap-2">
          <button onClick={() => setActiveTab('studio')} className={`p-2 rounded-md transition-all ${activeTab === 'studio' ? 'bg-white/10 text-white' : 'hover:text-white'}`}>
            <Layers className="w-4 h-4" />
          </button>
          <button onClick={() => setActiveTab('kb')} className={`p-2 rounded-md transition-all ${activeTab === 'kb' ? 'bg-white/10 text-white' : 'hover:text-white'}`}>
            <Database className="w-4 h-4" />
          </button>
          <button onClick={() => setActiveTab('commands')} className={`p-2 rounded-md transition-all ${activeTab === 'commands' ? 'bg-white/10 text-white' : 'hover:text-white'}`}>
            <CmdIcon className="w-4 h-4" />
          </button>
          <button onClick={() => setActiveTab('files')} className={`p-2 rounded-md transition-all ${activeTab === 'files' ? 'bg-white/10 text-white' : 'hover:text-white'}`}>
            <Paperclip className="w-4 h-4" />
          </button>
          <button onClick={() => setActiveTab('recovery')} className={`p-2 rounded-md transition-all ${activeTab === 'recovery' ? 'bg-white/10 text-white' : 'hover:text-white'}`}>
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>

        <div className="mt-auto flex flex-col gap-3 items-center pb-4">
          <div className={`w-2 h-2 rounded-full ${isOnline ? 'bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.5)]' : 'bg-red-500'}`} title={isOnline ? 'Online' : 'Offline'} />
          <div className={`w-2 h-2 rounded-full ${kb.unity_status.is_running ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.5)]' : 'bg-slate-700'}`} title="Unity Status" />
          <button onClick={() => setShowHierarchy(true)} className="p-2 hover:text-white transition-colors">
            <Users className="w-4 h-4" />
          </button>
          <button onClick={handleUpdate} className={`p-2 hover:text-white transition-colors ${isUpdating ? 'animate-spin text-blue-400' : ''}`}>
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 flex flex-col overflow-hidden">
        
        {/* Top Bar */}
        <header className="h-10 border-b border-white/5 bg-black/20 flex items-center justify-between px-4">
          <div className="flex items-center gap-4">
            <h2 className="font-bold text-white uppercase tracking-tighter text-[10px]">Claude Code Game Studios</h2>
            <div className="h-3 w-px bg-white/10" />
            <div className="flex items-center gap-2 text-[9px] text-slate-500 uppercase">
              <ShieldCheck className="w-3 h-3 text-green-500" />
              API: <span className="text-slate-300">{config.apiKey.slice(0, 6)}...</span>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button 
              onClick={() => setShowHierarchy(true)}
              className="px-3 py-1 bg-white/5 hover:bg-white/10 border border-white/10 rounded text-[9px] font-bold text-white uppercase transition-all"
            >
              Иерархия Агентов (48)
            </button>
            <button 
              onClick={handleUpdate}
              className="px-3 py-1 bg-blue-600 hover:bg-blue-500 text-white rounded text-[9px] font-bold uppercase shadow-lg shadow-blue-600/20 transition-all"
            >
              ОБНОВИТЬ
            </button>
          </div>
        </header>

        {/* Content View */}
        <div className="flex-1 flex overflow-hidden">
          
          {/* Main Area */}
          <div className="flex-1 flex flex-col bg-black/10">
            
            {activeTab === 'studio' && (
              <div className="flex-1 flex flex-col overflow-hidden">
                {/* Messages Area */}
                <div className="flex-1 overflow-y-auto p-4 space-y-4 scrollbar-thin scrollbar-thumb-white/5">
                  {messages.length === 0 && (
                    <div className="h-full flex flex-col items-center justify-center text-slate-600 opacity-50">
                      <Zap className="w-8 h-8 mb-2" />
                      <p>Система готова. Введите запрос для начала работы агентов.</p>
                    </div>
                  )}
                  {messages.map((msg, i) => (
                    <div key={i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                      <div className={`max-w-[80%] p-3 rounded-lg border ${msg.role === 'user' ? 'bg-blue-600/10 border-blue-500/20 text-slate-200' : 'bg-white/5 border-white/10 text-slate-300'}`}>
                        {msg.agent && <div className="text-[9px] font-bold text-blue-400 uppercase mb-1">{msg.agent}</div>}
                        <p className="leading-relaxed">{msg.content}</p>
                        {msg.files && (
                          <div className="mt-2 flex flex-wrap gap-2">
                            {msg.files.map((f, fi) => (
                              <div key={fi} className="flex items-center gap-1 bg-black/20 px-2 py-1 rounded border border-white/5 text-[8px]">
                                {f.type.includes('image') ? <ImageIcon className="w-2 h-2" /> : f.type.includes('video') ? <Video className="w-2 h-2" /> : f.type.includes('audio') ? <Music className="w-2 h-2" /> : <FileText className="w-2 h-2" />}
                                {f.name}
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>

                {/* Input Area */}
                <div className="p-4 border-t border-white/5 bg-black/40">
                  <div className="max-w-3xl mx-auto flex gap-2">
                    <button 
                      onClick={() => fileInputRef.current?.click()}
                      className="p-2 bg-white/5 hover:bg-white/10 border border-white/10 rounded-md text-slate-400 hover:text-white transition-all"
                    >
                      <Paperclip className="w-4 h-4" />
                    </button>
                    <input 
                      type="file" 
                      ref={fileInputRef}
                      onChange={handleFileUpload}
                      multiple
                      className="hidden"
                    />
                    <input 
                      type="text" 
                      value={chatInput}
                      onChange={(e) => setChatInput(e.target.value)}
                      onKeyDown={(e) => e.key === 'Enter' && handleSendMessage()}
                      placeholder="Опишите задачу для студии..."
                      className="flex-1 bg-white/5 border border-white/10 rounded-md px-3 py-2 text-white focus:outline-none focus:border-blue-500/50 transition-all"
                    />
                    <button 
                      onClick={handleSendMessage}
                      className="bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded-md transition-all flex items-center gap-2"
                    >
                      <Play className="w-3 h-3" />
                    </button>
                  </div>
                </div>
              </div>
            )}

            {activeTab === 'kb' && (
              <div className="flex-1 p-6 overflow-auto scrollbar-thin scrollbar-thumb-white/5">
                <div className="flex items-center justify-between mb-6">
                  <div className="flex flex-col">
                    <h2 className="text-xl font-bold text-white">База Знаний CCGS</h2>
                    <p className="text-[10px] text-slate-500 mt-1 uppercase tracking-wider">Глобальный проектный чертеж (Blueprint) активен</p>
                  </div>
                  <div className="flex gap-2">
                    <a 
                      href="/ccgs_project_blueprint.json" 
                      download 
                      className="bg-white/5 hover:bg-white/10 border border-white/10 text-white px-4 py-2 rounded-lg text-xs font-bold flex items-center gap-2 transition-all"
                    >
                      <Download className="w-4 h-4" /> Скачать Blueprint
                    </a>
                    <button className="bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded-lg text-xs font-bold flex items-center gap-2">
                      <Save className="w-4 h-4" /> Сохранить
                    </button>
                  </div>
                </div>
                <div className="bg-black/40 border border-white/10 rounded-2xl p-6 font-mono text-xs text-blue-400">
                  <pre>{JSON.stringify(kb, null, 2)}</pre>
                </div>
              </div>
            )}

            {activeTab === 'commands' && (
              <div className="flex-1 p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 overflow-auto scrollbar-thin scrollbar-thumb-white/5">
                {kb.commands.map(cmd => (
                  <div key={cmd.cmd} className="bg-white/5 border border-white/10 rounded-xl p-4 hover:border-blue-500/30 transition-colors group cursor-pointer">
                    <div className="flex items-center gap-3 mb-2">
                      <div className="w-8 h-8 rounded bg-black/40 flex items-center justify-center border border-white/5 group-hover:text-blue-400">
                        <CmdIcon className="w-4 h-4" />
                      </div>
                      <code className="text-blue-400 font-bold">{cmd.cmd}</code>
                    </div>
                    <p className="text-xs text-slate-500">{cmd.desc}</p>
                  </div>
                ))}
              </div>
            )}

            {activeTab === 'files' && (
              <div className="flex-1 p-6 overflow-auto scrollbar-thin scrollbar-thumb-white/5">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-xl font-bold text-white">Менеджер Файлов</h2>
                  <div className="flex gap-2">
                    <button 
                      onClick={() => fileInputRef.current?.click()}
                      className="bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded-lg text-xs font-bold flex items-center gap-2 transition-all"
                    >
                      <Paperclip className="w-4 h-4" /> Загрузить файлы
                    </button>
                  </div>
                </div>
                
                <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
                  {uploadedFiles.map((f, i) => (
                    <div key={i} className="bg-white/5 border border-white/10 rounded-xl p-3 flex flex-col items-center group hover:border-blue-500/30 transition-all">
                      <div className="w-12 h-12 bg-black/40 rounded-lg flex items-center justify-center mb-2 group-hover:text-blue-400">
                        {f.type.includes('image') ? <ImageIcon className="w-6 h-6" /> : f.type.includes('video') ? <Video className="w-6 h-6" /> : f.type.includes('audio') ? <Music className="w-6 h-6" /> : <FileText className="w-6 h-6" />}
                      </div>
                      <span className="text-[9px] text-slate-300 text-center truncate w-full mb-1">{f.name}</span>
                      <span className="text-[8px] text-slate-600 uppercase">{(f.size / 1024 / 1024).toFixed(1)} MB</span>
                    </div>
                  ))}
                  
                  <div className="aspect-square bg-white/5 border border-dashed border-white/10 rounded-xl flex flex-col items-center justify-center text-slate-600 hover:border-blue-500/50 hover:text-blue-400 transition-all cursor-pointer" onClick={() => fileInputRef.current?.click()}>
                    <Paperclip className="w-8 h-8 mb-2" />
                    <span className="text-[10px]">Добавить</span>
                  </div>
                </div>

                {uploadedFiles.length === 0 && (
                  <div className="h-64 flex flex-col items-center justify-center text-slate-600 opacity-50">
                    <HardDrive className="w-12 h-12 mb-2" />
                    <p>Нет загруженных файлов. Используйте кнопку выше для добавления.</p>
                  </div>
                )}
              </div>
            )}

            {activeTab === 'recovery' && (
              <div className="flex-1 p-6 overflow-auto scrollbar-thin scrollbar-thumb-white/5">
                <div className="flex items-center justify-between mb-6">
                  <div className="flex flex-col">
                    <h2 className="text-xl font-bold text-white">Восстановление Проекта</h2>
                    <p className="text-[10px] text-slate-500 mt-1 uppercase tracking-wider">Используйте файл Blueprint для восстановления всех данных</p>
                  </div>
                </div>
                
                <div className="bg-white/5 border border-white/10 rounded-2xl p-6">
                  <p className="text-slate-400 mb-4">Вставьте содержимое файла <code className="text-blue-400">ccgs_project_blueprint.json</code> ниже, чтобы полностью восстановить структуру проекта, агентов и настройки.</p>
                  <textarea 
                    value={recoveryBlueprint}
                    onChange={(e) => setRecoveryBlueprint(e.target.value)}
                    placeholder='{"project_name": "...", "knowledge_base": {...}, ...}'
                    className="w-full h-64 bg-black/40 border border-white/10 rounded-xl p-4 font-mono text-[10px] text-blue-400 focus:outline-none focus:border-blue-500/50 transition-all mb-4"
                  />
                  <button 
                    onClick={handleRecovery}
                    disabled={isRecovering || !recoveryBlueprint.trim()}
                    className={`w-full py-3 rounded-xl font-bold uppercase tracking-widest transition-all ${isRecovering || !recoveryBlueprint.trim() ? 'bg-slate-800 text-slate-600 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-500 text-white shadow-lg shadow-blue-600/20'}`}
                  >
                    {isRecovering ? 'Восстановление...' : 'Запустить Восстановление'}
                  </button>
                </div>

                <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="bg-white/5 border border-white/10 rounded-xl p-4">
                    <h3 className="text-white font-bold mb-2 flex items-center gap-2">
                      <ShieldCheck className="w-4 h-4 text-green-500" /> Что восстановится?
                    </h3>
                    <ul className="space-y-2 text-[10px] text-slate-500 list-disc list-inside">
                      <li>Все 48 специализированных AI-агентов</li>
                      <li>Иерархия и роли отделов</li>
                      <li>Название и описание проекта</li>
                      <li>Связи с Unity и настройки окружения</li>
                    </ul>
                  </div>
                  <div className="bg-white/5 border border-white/10 rounded-xl p-4">
                    <h3 className="text-white font-bold mb-2 flex items-center gap-2">
                      <AlertTriangle className="w-4 h-4 text-yellow-500" /> Важное примечание
                    </h3>
                    <p className="text-[10px] text-slate-500 leading-relaxed">
                      Восстановление перезапишет текущую базу знаний. Убедитесь, что у вас есть актуальная копия Blueprint перед началом процесса.
                    </p>
                  </div>
                </div>
              </div>
            )}

          </div>

          {/* Right Sidebar: Logs & Status */}
          <aside className="w-64 border-l border-white/5 bg-black/20 flex flex-col overflow-hidden">
            {/* Unity Status */}
            <div className="p-3 border-b border-white/5">
              <div className="flex items-center justify-between mb-2">
                <span className="text-[9px] font-bold text-slate-500 uppercase">Unity Engine</span>
                <span className={`text-[8px] px-1.5 py-0.5 rounded ${kb.unity_status.is_running ? 'bg-green-500/20 text-green-400' : 'bg-slate-800 text-slate-500'}`}>
                  {kb.unity_status.is_running ? 'CONNECTED' : 'DISCONNECTED'}
                </span>
              </div>
              {kb.unity_status.is_running && (
                <div className="space-y-1">
                  <div className="flex justify-between text-[9px]">
                    <span className="text-slate-600">Версия:</span>
                    <span className="text-slate-300">{kb.unity_status.version}</span>
                  </div>
                  <div className="text-[8px] text-blue-400 font-mono truncate">{kb.unity_status.project_path}</div>
                </div>
              )}
            </div>

            {/* Logs */}
            <div className="flex-1 flex flex-col overflow-hidden">
              <div className="px-3 py-2 border-b border-white/5 bg-white/5 flex items-center gap-2">
                <Terminal className="w-3 h-3 text-slate-500" />
                <span className="text-[9px] font-bold text-slate-500 uppercase">Системный лог</span>
              </div>
              <div className="flex-1 overflow-y-auto p-3 font-mono text-[9px] space-y-1 scrollbar-thin scrollbar-thumb-white/5">
                {logs.map((log, i) => (
                  <div key={i} className={`${log.includes('ОШИБКА') ? 'text-red-400' : log.includes('ПРОЦЕСС') ? 'text-blue-400' : log.includes('УСПЕХ') ? 'text-green-400' : 'text-slate-600'}`}>
                    {log}
                  </div>
                ))}
              </div>
            </div>
          </aside>
        </div>
      </main>

      {/* Hierarchy Modal */}
      <AnimatePresence>
        {showHierarchy && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-8"
          >
            <motion.div 
              initial={{ scale: 0.95, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.95, y: 20 }}
              className="bg-[#0c0c0e] border border-white/10 rounded-2xl w-full max-w-4xl max-h-[80vh] flex flex-col overflow-hidden shadow-2xl"
            >
              <div className="p-4 border-b border-white/5 flex items-center justify-between bg-white/5">
                <div className="flex items-center gap-3">
                  <Users className="w-5 h-5 text-blue-500" />
                  <h3 className="text-lg font-bold text-white">Иерархия Агентов CCGS (48)</h3>
                </div>
                <button onClick={() => setShowHierarchy(false)} className="p-2 hover:bg-white/10 rounded-lg transition-colors">
                  <X className="w-4 h-4" />
                </button>
              </div>
              
              <div className="flex-1 overflow-y-auto p-6 grid grid-cols-1 md:grid-cols-3 gap-6 scrollbar-thin scrollbar-thumb-white/10">
                {kb.levels.map(level => (
                  <div key={level.id} className="space-y-3">
                    <div className="flex items-center gap-2 pb-2 border-b border-white/5">
                      <span className="text-[10px] font-mono text-blue-500">L{level.id}</span>
                      <h4 className="text-xs font-bold text-white uppercase">{level.name}</h4>
                    </div>
                    <div className="space-y-2">
                      {level.agents.map(agent => (
                        <div 
                          key={agent.id}
                          className="p-2 rounded-lg bg-white/5 border border-white/5 hover:border-blue-500/30 transition-all cursor-default group"
                        >
                          <div className="flex justify-between items-start mb-1">
                            <span className="text-[10px] font-bold text-slate-200 group-hover:text-blue-400">{agent.name}</span>
                            <span className="text-[8px] text-slate-600 font-mono">{agent.model}</span>
                          </div>
                          <p className="text-[9px] text-slate-500 leading-tight">{agent.role}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

    </div>
  );
}
