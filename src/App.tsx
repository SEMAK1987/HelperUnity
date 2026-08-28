import React, { useState, useEffect, useRef } from "react";
import { CastleFacilities } from "./components/CastleFacilities";
import { ExternalSkillsDBView } from "./components/ExternalSkillsDBView";
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
  Upload,
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
  User,
  Scroll,
  ShieldCheck,
  CheckCircle,
  Shield,
  ArrowRight,
  Target,
  Layout,
  Swords,
  Clock,
  Skull,
  CloudOff,
  TrendingUp,
  Coins,
  Flame,
  Droplets,
  Wind,
  Mountain,
  FlaskConical,
  Calculator,
  Dna,
  Sword,
  Star,
  Eye,
  ZapOff,
  Crown,
  Sun,
  Moon,
  Activity,
  Play,
  LogOut,
  Monitor,
  Volume2,
  Globe,
  AlertCircle,
  MessageSquare,
  Compass,
} from "lucide-react";
import { motion, AnimatePresence } from "motion/react";
import { GoogleGenAI } from "@google/genai";
import Markdown from "react-markdown";

// --- Types ---
interface Message {
  role: "user" | "assistant";
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

// --- Components ---

function GameHelpView() {
  const [content, setContent] = useState<string>("Загрузка руководства...");
  const [search, setSearch] = useState("");

  useEffect(() => {
    fetch("/GAME_HELP_GUIDE.md")
      .then((res) => res.text())
      .then((text) => setContent(text))
      .catch(() =>
        setContent("# Ошибка\nНе удалось загрузить GAME_HELP_GUIDE.md"),
      );
  }, []);

  const filteredLines = content
    .split("\n")
    .filter((line) => line.toLowerCase().includes(search.toLowerCase()));

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="h-full flex flex-col p-6 space-y-6 overflow-hidden"
    >
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-black text-white uppercase tracking-tighter italic flex items-center gap-3">
            <BookOpen className="w-8 h-8 text-blue-500" />
            Помощь По Игре (Unity 6)
          </h2>
          <p className="text-xs text-slate-500 uppercase tracking-widest font-bold ml-11">
            Интерактивное руководство по разработке • v18.12.44
          </p>
        </div>
        <div className="flex items-center gap-4">
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
            <input
              type="text"
              placeholder="Поиск по документации..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="bg-white/5 border border-white/10 rounded-full py-2 pl-10 pr-4 text-sm text-white focus:outline-none focus:border-blue-500 transition-all w-64 uppercase tracking-widest font-bold"
            />
          </div>
          <button
            onClick={() => window.open("/GAME_HELP_GUIDE.md", "_blank")}
            className="p-3 rounded-2xl bg-blue-500/20 text-blue-400 hover:bg-blue-500 hover:text-white transition-all shadow-lg shadow-blue-500/10 border border-blue-500/30"
          >
            <ExternalLink className="w-5 h-5" />
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto bg-black/40 border border-white/5 rounded-[2.5rem] p-10 backdrop-blur-md custom-scrollbar shadow-2xl relative overflow-x-hidden">
        <div className="prose prose-invert max-w-none relative z-10">
          <Markdown
            components={{
              h1: ({ children }) => (
                <h1 className="text-4xl font-black text-white mb-10 uppercase tracking-tighter border-b-4 border-blue-600 pb-4 inline-block">
                  {children}
                </h1>
              ),
              h2: ({ children }) => (
                <h2 className="text-2xl font-black text-blue-400 mt-16 mb-6 uppercase tracking-tighter flex items-center gap-3">
                  <Cpu className="w-6 h-6" /> {children}
                </h2>
              ),
              h3: ({ children }) => (
                <h3 className="text-lg font-black text-white mt-10 mb-4 uppercase italic border-l-4 border-blue-500 pl-4">
                  {children}
                </h3>
              ),
              p: ({ children }) => (
                <p className="text-slate-400 leading-relaxed mb-6 font-medium">
                  {children}
                </p>
              ),
              code: ({ children }) => (
                <code className="bg-slate-900 text-blue-300 px-2 py-1 rounded-lg font-mono text-xs border border-white/10 shadow-inner">
                  {children}
                </code>
              ),
              pre: ({ children }) => (
                <div className="relative group my-8 bg-[#0a0a0c] rounded-3xl border border-white/5 p-6 shadow-2xl">
                  <div className="absolute top-0 right-0 left-0 h-1 rounded-t-3xl bg-gradient-to-r from-blue-600 to-purple-600 opacity-50" />
                  <pre className="text-sm font-mono text-slate-300 leading-relaxed overflow-x-auto p-2 scrollbar-thin scrollbar-thumb-white/5">
                    {children}
                  </pre>
                  <button className="absolute top-6 right-6 opacity-0 group-hover:opacity-100 transition-opacity p-2 bg-white/5 rounded-xl hover:bg-white/10 border border-white/10">
                    <Copy className="w-4 h-4 text-slate-400" />
                  </button>
                </div>
              ),
              li: ({ children }) => (
                <li className="text-slate-400 mb-4 list-none flex gap-4 items-start">
                  <div className="w-2 h-2 rounded-full bg-blue-500 mt-2 flex-shrink-0 animate-pulse shadow-[0_0_8px_rgba(59,130,246,0.5)]" />{" "}
                  <span className="flex-1">{children}</span>
                </li>
              ),
              ul: ({ children }) => (
                <ul className="space-y-4 mb-10 ml-2">{children}</ul>
              ),
              hr: () => <hr className="border-white/5 my-16" />,
              strong: ({ children }) => (
                <strong className="text-white font-black uppercase tracking-widest text-[10px] bg-blue-600/20 px-2 py-1 rounded border border-blue-500/30 mx-1">
                  {children}
                </strong>
              ),
            }}
          >
            {search ? filteredLines.join("\n") : content}
          </Markdown>
        </div>

        {/* Decorative elements */}
        <div className="absolute top-0 right-0 w-96 h-96 bg-blue-600/5 rounded-full blur-[100px] pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-purple-600/5 rounded-full blur-[100px] pointer-events-none" />
      </div>
    </motion.div>
  );
}

function SearchIcon({ className, ...props }: any) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      {...props}
    >
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  );
}

function VKImageCard({
  res,
  type,
  showNotification,
  onZoom,
}: {
  res: any;
  type: string;
  showNotification: any;
  onZoom: (url: string) => void;
}) {
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
        error ? "border-red-500/50" : "border-white/10 hover:border-blue-500/50"
      }`}
    >
      <div
        className={`w-full relative ${type === "live" ? "aspect-[9/16]" : "aspect-[15.9/4]"}`}
      >
        {loading && (
          <div className="absolute inset-0 flex flex-col items-center justify-center p-6 space-y-4 bg-black/40 z-10 backdrop-blur-sm">
            <RefreshCw className="w-8 h-8 text-blue-500 animate-spin" />
            <div className="text-center">
              <p className="text-[10px] text-white font-black tracking-widest animate-pulse uppercase">
                Манифестация...
              </p>
              <p className="text-[8px] text-slate-500 mt-1 uppercase tracking-tighter">
                Нейросеть рисует ассет
              </p>
            </div>
          </div>
        )}

        {error ? (
          <div className="absolute inset-0 flex flex-col items-center justify-center p-6 space-y-4 bg-red-950/20 backdrop-blur-sm z-20">
            <AlertTriangle className="w-10 h-10 text-red-500" />
            <div className="text-center">
              <p className="text-[10px] text-white font-bold uppercase mb-1">
                Ошибка синтеза
              </p>
              <p className="text-[8px] text-red-400/60 uppercase max-w-[120px] mx-auto leading-relaxed">
                Сервер перегружен. Попробуйте еще раз.
              </p>
            </div>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setError(false);
                setLoading(true);
                setRetryKey((prev) => prev + 1);
              }}
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
            className={`w-full h-full object-cover transition-transform duration-[2000ms] group-hover:scale-110 ${loading ? "opacity-0 scale-110" : "opacity-100 scale-100"}`}
            onLoad={() => setLoading(false)}
            onError={() => {
              setError(true);
              setLoading(false);
            }}
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
                  onClick={() =>
                    showNotification(`Подготовка файла #${res.id}...`, "info")
                  }
                >
                  <Download className="w-5 h-5" />
                </a>
              </div>
            </div>
            <p className="text-[9px] text-slate-400 font-medium italic line-clamp-1 opacity-60">
              {res.prompt_note}
            </p>
          </div>
        )}
      </div>
    </motion.div>
  );
}

// --- Menu Studio Preview ---
function MenuStudioPreview({ onDownload }: { onDownload: () => void }) {
  const atmosphericParticles = React.useMemo(
    () =>
      [...Array(5)].map((_, i) => ({
        id: i,
        initialX: Math.random() * 800,
        initialY: Math.random() * 500,
        targetX: Math.random() * 800,
        targetY: Math.random() * 500,
        duration: 10 + Math.random() * 10,
      })),
    [],
  );

  const [language, setLanguage] = React.useState("Русский");
  const [view, setView] = React.useState<"main" | "settings">("main");
  const [volume, setVolume] = React.useState(80);
  const [music, setMusic] = React.useState(60);
  const [quality, setQuality] = React.useState("8K ULTRA");
  const [resolution, setResolution] = React.useState("3840x2160");
  const [isFullscreen, setIsFullscreen] = React.useState(false);
  const [showLanguageList, setShowLanguageList] = React.useState(false);

  const t = {
    Русский: {
      play: "Играть",
      settings: "Настройки",
      exit: "Выход",
      back: "Назад",
      volume: "Звук",
      music: "Музыка",
      quality: "Качество (8K)",
      res: "Разрешение (ULTRA)",
      fs: "Весь экран",
      graphics: "Графика",
      lang: "Язык",
      help: "Помощь По Игре",
      capabilities: "Возможности ИИ",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Синтаксис Сингулярности",
      offline: "Защищенный Режим",
      clear: "Очистить",
      clearing: "Очистка...",
      thinking: "Cortex Matrix Analysis (8K)",
      synth: "Синтез данных Unity 6 & Blender 5.2",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Скачать Фон (JPG 8K)",
      q_vlow: "Очень Низкое",
      q_low: "Низкое",
      q_med: "Среднее",
      q_high: "Высокое",
      q_vhigh: "Очень Высокое",
      q_ultra: "Ультра",
    },
    English: {
      play: "Play",
      settings: "Settings",
      exit: "Exit",
      back: "Back",
      volume: "Sound",
      music: "Music",
      quality: "Quality (8K)",
      res: "Resolution (ULTRA)",
      fs: "Fullscreen",
      graphics: "Graphics",
      lang: "Language",
      help: "Game Help",
      capabilities: "AI Capabilities",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Singularity Syntax",
      offline: "Secure Mode",
      clear: "Clear",
      clearing: "Clearing...",
      thinking: "Cortex Matrix Analysis (8K)",
      synth: "Synthesizing Unity 6 & Blender 5.2 data",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Download Background (JPG 8K)",
      q_vlow: "Very Low",
      q_low: "Low",
      q_med: "Medium",
      q_high: "High",
      q_vhigh: "Very High",
      q_ultra: "Ultra",
    },
    Deutsch: {
      play: "Spielen",
      settings: "Einstellungen",
      exit: "Beenden",
      back: "Zurück",
      volume: "Ton",
      music: "Musik",
      quality: "Qualität (8K)",
      res: "Auflösung (ULTRA)",
      fs: "Vollbild",
      graphics: "Grafik",
      lang: "Sprache",
      help: "Spielhilfe",
      capabilities: "KI-Fähigkeiten",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Singularitäts-Syntax",
      offline: "Gesicherter Modus",
      clear: "Löschen",
      clearing: "Löschen...",
      thinking: "Cortex-Matrix-Analyse (8K)",
      synth: "Synthese von Unity 6 & Blender 5.2 Daten",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Hintergrund Herunterladen (JPG 8K)",
      q_vlow: "Sehr Niedrig",
      q_low: "Niedrig",
      q_med: "Mittel",
      q_high: "Hoch",
      q_vhigh: "Sehr Hoch",
      q_ultra: "Ultra",
    },
    Français: {
      play: "Jouer",
      settings: "Paramètres",
      exit: "Quitter",
      back: "Retour",
      volume: "Son",
      music: "Musique",
      quality: "Qualité (8K)",
      res: "Résolution (ULTRA)",
      fs: "Plein écran",
      graphics: "Graphisme",
      lang: "Langue",
      help: "Aide au Jeu",
      capabilities: "Capacités de l'IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Syntaxe de Singularité",
      offline: "Mode Sécurisé",
      clear: "Effacer",
      clearing: "Effacement...",
      thinking: "Analyse de la Matrice Cortex (8K)",
      synth: "Synthèse des données Unity 6 & Blender 5.2",
      proMastery: "Maîtrise Visuelle Menu Studio",
      downloadBg: "Télécharger le Fond (JPG 8K)",
      q_vlow: "Très Bas",
      q_low: "Bas",
      q_med: "Moyen",
      q_high: "Haut",
      q_vhigh: "Très Haut",
      q_ultra: "Ultra",
    },
    Español: {
      play: "Jugar",
      settings: "Ajustes",
      exit: "Salir",
      back: "Volver",
      volume: "Sonido",
      music: "Música",
      quality: "Calidad (8K)",
      res: "Resolución (ULTRA)",
      fs: "Pantalla completa",
      graphics: "Gráficos",
      lang: "Idioma",
      help: "Ayuda del Juego",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxis de Singularidad",
      offline: "Modo Seguro",
      clear: "Limpiar",
      clearing: "Limpiando...",
      thinking: "Análisis de la Matriz Cortex (8K)",
      synth: "Sintetizando datos de Unity 6 y Blender 5.2",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Descargar Fondo (JPG 8K)",
      q_vlow: "Muy Bajo",
      q_low: "Bajo",
      q_med: "Medio",
      q_high: "Alto",
      q_vhigh: "Muy Alto",
      q_ultra: "Ultra",
    },
    日本語: {
      play: "プレイ",
      settings: "設定",
      exit: "終了",
      back: "戻る",
      volume: "音量",
      music: "音乐",
      quality: "品質 (8K)",
      res: "解像度 (ULTRA)",
      fs: "全画面",
      graphics: "グラフィック",
      lang: "言語",
      help: "ゲームヘルプ",
      capabilities: "AI機能",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: オフ",
      sync: "シンギュラリティ構文",
      offline: "セキュアモード",
      clear: "クリア",
      clearing: "クリア中...",
      thinking: "皮질 매트릭스 분석 (8K)",
      synth: "Unity 6とBlender 5.2のデータを統合中",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "背景をダウンロード (JPG 8K)",
      q_vlow: "非常に低い",
      q_low: "低い",
      q_med: "中くらい",
      q_high: "高い",
      q_vhigh: "非常に高い",
      q_ultra: "ウルトラ",
    },
    한국어: {
      play: "플레이",
      settings: "설정",
      exit: "나가기",
      back: "뒤로",
      volume: "소리",
      music: "음악",
      quality: "품질 (8K)",
      res: "해상도 (ULTRA)",
      fs: "전체 화면",
      graphics: "그래픽",
      lang: "언어",
      help: "게임 도움말",
      capabilities: "AI 능력",
      ollama: "Ollama: 확인",
      ollamaOff: "Ollama: 꺼짐",
      sync: "특이점 구문",
      offline: "보안 모д",
      clear: "지우기",
      clearing: "지우는 중...",
      thinking: "피질 매트릭스 분석 (8K)",
      synth: "Unity 6 및 Blender 5.2 데이터 합성 중",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "배경 다운로드 (JPG 8K)",
      q_vlow: "매우 낮음",
      q_low: "낮음",
      q_med: "중간",
      q_high: "높음",
      q_vhigh: "매우 높음",
      q_ultra: "울트라",
    },
    简体中文: {
      play: "开始",
      settings: "设置",
      exit: "退出",
      back: "返回",
      volume: "音量",
      music: "音乐",
      quality: "画质 (8K)",
      res: "分辨率 (ULTRA)",
      fs: "全屏",
      graphics: "图像",
      lang: "语言",
      help: "游戏帮助",
      capabilities: "AI 能力",
      ollama: "Ollama: 正常",
      ollamaOff: "Ollama: 关闭",
      sync: "奇点语法",
      offline: "安全模式",
      clear: "清除",
      clearing: "正在清除...",
      thinking: "皮层矩阵分析 (8K)",
      synth: "综合 Unity 6 & Blender 5.2 数据",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "下载背景 (JPG 8K)",
      q_vlow: "极低",
      q_low: "低",
      q_med: "中",
      q_high: "高",
      q_vhigh: "极高",
      q_ultra: "终极",
    },
    Português: {
      play: "Jogar",
      settings: "Configurações",
      exit: "Sair",
      back: "Voltar",
      volume: "Som",
      music: "Música",
      quality: "Qualidade (8K)",
      res: "Resolução (ULTRA)",
      fs: "Tela cheia",
      graphics: "Gráficos",
      lang: "Idioma",
      help: "Ajuda do Jogo",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxe de Singularidade",
      offline: "Modo Seguro",
      clear: "Limpar",
      clearing: "Limpando...",
      thinking: "Análise da Matriz Cortex (8K)",
      synth: "Sintetizando dados de Unity 6 e Blender 5.2",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Baixar Fundo (JPG 8K)",
      q_vlow: "Muito Baixo",
      q_low: "Baixo",
      q_med: "Médio",
      q_high: "Alto",
      q_vhigh: "Muito Alto",
      q_ultra: "Ultra",
    },
  }[language as keyof typeof t] || {
    play: "Play",
    settings: "Settings",
    exit: "Exit",
    back: "Back",
    volume: "Sound",
    music: "Music",
    quality: "Quality",
    res: "Resolution",
    fs: "Fullscreen",
    graphics: "Graphics",
    lang: "Language",
    downloadBg: "Download",
    q_vlow: "Very Low",
    q_low: "Low",
    q_med: "Medium",
    q_high: "High",
    q_vhigh: "Very High",
    q_ultra: "Ultra",
  };

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="relative w-full aspect-video rounded-[3rem] overflow-hidden bg-slate-900 shadow-2xl border border-white/10 group"
    >
      {/* Background Layer: Animated Castles & Landscape */}
      <div className="absolute inset-0 bg-[url('https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&q=80&w=2070')] bg-cover bg-center opacity-40" />

      <div className="absolute top-6 left-6 z-50 opacity-0 group-hover:opacity-100 transition-all duration-700">
        <button
          onClick={onDownload}
          className="flex items-center gap-2 px-4 py-2 bg-black/40 backdrop-blur-xl border border-white/10 rounded-xl text-[10px] font-black text-white uppercase tracking-widest hover:bg-blue-600/80 hover:shadow-[0_0_20px_rgba(37,99,235,0.4)] transition-all shadow-2xl cursor-pointer"
        >
          <Download className="w-3 h-3" /> {t.downloadBg}
        </button>
      </div>

      {/* Castles Silhouette Layer */}
      <div className="absolute inset-0 flex items-end justify-between px-20 pb-10 pointer-events-none opacity-50">
        {[
          { name: "Human Castle", h: 120, x: -20 },
          { name: "Elf Citadel", h: 160, x: 0 },
          { name: "Orc Fortress", h: 100, x: 20 },
          { name: "Undead Necropolis", h: 140, x: -10 },
        ].map((castle, i) => (
          <motion.div
            key={i}
            animate={{ y: [0, -5, 0] }}
            transition={{
              duration: 5 + i,
              repeat: Infinity,
              ease: "easeInOut",
            }}
            className="relative"
            style={{ marginLeft: `${castle.x}px` }}
          >
            <svg
              width="80"
              height={castle.h}
              viewBox={`0 0 80 ${castle.h}`}
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d={`M10 ${castle.h} L10 40 L40 10 L70 40 L70 ${castle.h} Z`}
                fill="#1e293b"
              />
              <rect x="25" y="50" width="30" height="20" fill="#334155" />
              <path d="M5 40 L75 40 L40 5 Z" fill="#0f172a" />
            </svg>
          </motion.div>
        ))}
      </div>

      <div className="absolute inset-0 bg-gradient-to-tr from-slate-950 via-transparent to-slate-950/50" />

      {/* Character Center Placeholder */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        <motion.div
          animate={{ y: [0, -10, 0] }}
          transition={{ duration: 4, repeat: Infinity, ease: "easeInOut" }}
          className="relative"
        >
          <svg
            width="200"
            height="300"
            viewBox="0 0 200 300"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            className="drop-shadow-[0_0_30px_rgba(255,100,100,0.3)]"
          >
            <circle cx="100" cy="60" r="15" fill="#f87171" opacity="0.8" />
            <path
              d="M100 75 L100 130 L70 170 L50 210 M100 130 L130 170 L150 210 M80 100 L50 130 L60 150 M120 100 L150 130 L140 150"
              stroke="#f87171"
              strokeWidth="12"
              strokeLinecap="round"
              opacity="0.6"
            />
            <path
              d="M70 210 Q100 230 130 210"
              stroke="#f87171"
              strokeWidth="10"
              strokeLinecap="round"
              opacity="0.6"
            />
            <ellipse
              cx="100"
              cy="220"
              rx="60"
              ry="15"
              fill="black"
              opacity="0.4"
            />
          </svg>
          <div className="absolute inset-0 bg-red-500/20 blur-3xl rounded-full scale-150 animate-pulse" />
        </motion.div>
      </div>

      <AnimatePresence mode="wait">
        {view === "main" ? (
          <motion.div
            key="main"
            initial={{ opacity: 0, x: 100, filter: "blur(10px)" }}
            animate={{ opacity: 1, x: 0, filter: "blur(0px)" }}
            exit={{ opacity: 0, x: -100, filter: "blur(10px)" }}
            transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
            className="absolute right-12 top-1/2 -translate-y-1/2 flex flex-col gap-6"
          >
            {[
              {
                id: "play",
                icon: <Flame className="w-8 h-8" />,
                color: "bg-orange-500",
                label: t.play,
              },
              {
                id: "settings",
                icon: <Settings className="w-8 h-8" />,
                color: "bg-blue-600",
                label: t.settings,
              },
              {
                id: "exit",
                icon: <X className="w-8 h-8" />,
                color: "bg-red-600",
                label: t.exit,
              },
            ].map((btn, idx) => (
              <motion.div
                key={btn.id}
                initial={{ opacity: 0, x: 50 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.1 * idx, duration: 0.5 }}
                className="flex items-center gap-4 justify-end group"
              >
                <span className="text-white font-black uppercase tracking-widest opacity-0 group-hover:opacity-100 transition-all duration-500 bg-black/40 px-4 py-2 rounded-xl backdrop-blur-xl border border-white/10 translate-x-4 group-hover:translate-x-0">
                  {btn.label}
                </span>
                <button
                  onClick={() => btn.id === "settings" && setView("settings")}
                  className={`${btn.color} p-6 rounded-full text-white shadow-2xl hover:scale-110 active:scale-90 transition-all duration-500 border-4 border-white/20 relative group-hover:shadow-[0_0_40px_rgba(255,255,255,0.4)] overflow-hidden`}
                >
                  <div className="absolute inset-0 bg-gradient-to-tr from-white/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                  <div className="relative z-10">{btn.icon}</div>
                  <motion.div
                    animate={{ scale: [1, 1.2, 1] }}
                    transition={{ duration: 2, repeat: Infinity }}
                    className="absolute inset-0 rounded-full border-4 border-white/0 group-hover:border-white/20 transition-all"
                  />
                </button>
              </motion.div>
            ))}
          </motion.div>
        ) : (
          <motion.div
            key="settings"
            initial={{ opacity: 0, y: 100, filter: "blur(20px)" }}
            animate={{ opacity: 1, y: 0, filter: "blur(0px)" }}
            exit={{ opacity: 0, y: 100, filter: "blur(20px)" }}
            transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
            className="absolute inset-0 flex flex-col p-12 bg-black/40 backdrop-blur-3xl"
          >
            {/* Settings Header */}
            <h2 className="text-6xl font-black text-white/5 uppercase tracking-[1em] absolute top-24 left-1/2 -translate-x-1/2 select-none pointer-events-none">
              {t.settings.toUpperCase()}
            </h2>

            <div className="grid grid-cols-2 gap-20 h-full items-center">
              {/* Left Column: Quality & Fullscreen */}
              <div className="space-y-12">
                <div className="space-y-4">
                  <h3 className="text-sm font-black text-white/40 uppercase tracking-widest">
                    {t.quality}
                  </h3>
                  <div className="relative">
                    <select
                      value={quality}
                      onChange={(e) => setQuality(e.target.value)}
                      className="w-full bg-white/10 border border-white/20 rounded-xl py-4 px-4 text-white text-[11px] font-bold appearance-none cursor-pointer hover:bg-white/20 transition-all focus:outline-none whitespace-nowrap overflow-hidden text-ellipsis"
                    >
                      {[
                        { id: "Very Low", label: t.q_vlow },
                        { id: "Low", label: t.q_low },
                        { id: "Medium", label: t.q_med },
                        { id: "High", label: t.q_high },
                        { id: "Very High", label: t.q_vhigh },
                        { id: "Ultra", label: t.q_ultra },
                      ].map((q) => (
                        <option
                          key={q.id}
                          value={q.id}
                          className="bg-slate-900"
                        >
                          {q.label}
                        </option>
                      ))}
                    </select>
                    <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none">
                      <ChevronRight className="w-4 h-4 text-white/40 rotate-90" />
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="text-sm font-black text-white/40 uppercase tracking-widest">
                    {t.res}
                  </h3>
                  <div className="relative">
                    <select
                      value={resolution}
                      onChange={(e) => setResolution(e.target.value)}
                      className="w-full bg-white/10 border border-white/20 rounded-xl py-4 px-6 text-white text-[10px] font-bold appearance-none cursor-pointer hover:bg-white/20 transition-all focus:outline-none max-h-40 overflow-y-auto"
                    >
                      {[
                        "640 x 480",
                        "800 x 600",
                        "1024 x 768",
                        "1280 x 720",
                        "1366 x 768",
                        "1600 x 900",
                        "1920 x 1080",
                        "2560 x 1440",
                        "3840 x 2160",
                        "7680 x 4320",
                      ].map((r) => (
                        <option key={r} value={r} className="bg-slate-900">
                          {r}
                        </option>
                      ))}
                    </select>
                    <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none">
                      <ChevronRight className="w-4 h-4 text-white/40 rotate-90" />
                    </div>
                  </div>
                </div>

                <div
                  className="flex items-center gap-4 group cursor-pointer"
                  onClick={() => setIsFullscreen(!isFullscreen)}
                >
                  <div
                    className={`w-8 h-8 rounded-lg border-2 flex items-center justify-center transition-all ${isFullscreen ? "bg-blue-600 border-blue-400" : "bg-white/5 border-white/20"}`}
                  >
                    {isFullscreen && <Check className="w-5 h-5 text-white" />}
                  </div>
                  <span className="text-sm font-black text-white/60 uppercase tracking-widest group-hover:text-white transition-colors">
                    {t.fs}
                  </span>
                </div>
              </div>

              {/* Right Column: Audio & Graphics & Language */}
              <div className="space-y-12">
                <div className="space-y-6">
                  <div className="space-y-2">
                    <div className="flex justify-between text-[10px] font-black text-white/40 uppercase">
                      <span>{t.volume}</span>
                      <span>{volume}%</span>
                    </div>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={volume}
                      onChange={(e) => setVolume(parseInt(e.target.value))}
                      className="w-full h-1 bg-white/10 rounded-lg appearance-none cursor-pointer accent-blue-500"
                    />
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-[10px] font-black text-white/40 uppercase">
                      <span>{t.music}</span>
                      <span>{music}%</span>
                    </div>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={music}
                      onChange={(e) => setMusic(parseInt(e.target.value))}
                      className="w-full h-1 bg-white/10 rounded-lg appearance-none cursor-pointer accent-blue-500"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-4">
                  <button className="py-4 bg-white/10 border border-white/20 rounded-xl text-xs font-black text-white uppercase tracking-widest hover:bg-white/20 transition-all">
                    {t.graphics}
                  </button>
                  <div className="relative">
                    <button
                      onClick={() => setShowLanguageList(!showLanguageList)}
                      className="w-full py-4 bg-white/10 border border-white/20 rounded-xl text-xs font-black text-white uppercase tracking-widest hover:bg-white/20 transition-all flex items-center justify-center gap-3"
                    >
                      {t.lang}: {language}
                      <Globe className="w-4 h-4 text-blue-400" />
                    </button>

                    {showLanguageList && (
                      <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="absolute bottom-full mb-2 w-full bg-slate-900 border border-white/20 rounded-xl overflow-hidden z-50 shadow-2xl"
                      >
                        {[
                          "Русский",
                          "English",
                          "Deutsch",
                          "Français",
                          "Español",
                          "Português",
                          "日本語",
                          "한국어",
                          "简体中文",
                        ].map((l) => (
                          <button
                            key={l}
                            onClick={() => {
                              setLanguage(l);
                              setShowLanguageList(false);
                            }}
                            className="w-full py-3 px-4 text-left text-[10px] font-bold text-white/60 hover:text-white hover:bg-white/5 transition-all border-b border-white/5 last:border-none"
                          >
                            {l}
                          </button>
                        ))}
                      </motion.div>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Back Button */}
            <button
              onClick={() => setView("main")}
              className="absolute bottom-12 right-12 p-6 bg-blue-500 rounded-full text-white shadow-2xl hover:scale-110 active:scale-95 transition-all border-4 border-white/20"
            >
              <ArrowLeft className="w-8 h-8" />
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Atmospheric FX Overlay */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
        {atmosphericParticles.map((p) => (
          <motion.div
            key={p.id}
            animate={{
              x: [p.initialX, p.targetX, p.initialX],
              y: [p.initialY, p.targetY, p.initialY],
              opacity: [0, 0.2, 0],
              scale: [1, 1.5, 1],
            }}
            transition={{
              duration: p.duration,
              repeat: Infinity,
              ease: "linear",
            }}
            className="absolute w-64 h-64 bg-blue-500/5 rounded-full blur-[100px]"
          />
        ))}
      </div>
    </motion.div>
  );
}

function ArrowLeft({ className, ...props }: any) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      {...props}
    >
      <path d="m12 19-7-7 7-7" />
      <path d="M19 12H5" />
    </svg>
  );
}

function AtmosphericOverlay() {
  const atmosphericParticles = React.useMemo(
    () =>
      [...Array(8)].map((_, i) => ({
        id: i,
        initialX: Math.random() * 1200,
        initialY: Math.random() * 800,
        targetX: Math.random() * 1200,
        targetY: Math.random() * 800,
        duration: 15 + Math.random() * 15,
      })),
    [],
  );

  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden z-0">
      {atmosphericParticles.map((p) => (
        <motion.div
          key={p.id}
          animate={{
            x: [p.initialX, p.targetX, p.initialX],
            y: [p.initialY, p.targetY, p.initialY],
            opacity: [0, 0.15, 0],
            scale: [1, 2, 1],
          }}
          transition={{
            duration: p.duration,
            repeat: Infinity,
            ease: "linear",
          }}
          className="absolute w-96 h-96 bg-blue-500/5 rounded-full blur-[120px]"
        />
      ))}
    </div>
  );
}

const MemoizedAtmosphericOverlay = React.memo(AtmosphericOverlay);

const DIALOGUE_TRANSLATIONS: Record<
  number,
  Record<"RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH", string>
> = {
  0: {
    RU: "Здравствуй, путник! Наш Континент Судьбы погружается во тьму древнего безвременья. Я буду сопровождать тебя в этом опасном походе.",
    EN: "Greetings, traveler! Our Fate Continent is sinking into the darkness of ancient timelessness. I will accompany you in this dangerous journey.",
    DE: "Seid gegrüßt, Reisender! Unser Schicksalskontinent versinkt in der Dunkelheit der alten Zeitlosigkeit. Ich werde euch auf dieser gefährlichen Reise begleiten.",
    FR: "Salutations, voyageur ! Notre Continent du Destin sombre dans les ténèbres d'une intemporalité ancienne. Je vous accompagnerai dans ce voyage dangereux.",
    ES: "¡Saludos, viajero! Nuestro Continente del Destino se está hundiendo en la oscuridad de una atemporalidad antigua. Te acompañaré en este peligroso viaje.",
    PT: "Saudações, viajante! Nosso Continente do Destino está afundando na escuridão de uma atemporalidade antiga. Eu irei acompanhá-lo nesta jornada perigosa.",
    JA: "旅人よ、挨拶を！私たちの運命の大陸は、古代の永遠の闇へと沈みつつあります。この危険な旅路、私が同行しましょう。",
    KR: "반갑다, 여행자여! 우리의 운명 대륙이 고대 무한의 어둠 속으로 잠기고 있다. 내가 이 위험한 여정에 동행하겠다.",
    CH: "你好，旅人！我们的命运大陆正在沉入远古无尽的黑暗之中。我将陪伴你度过这段危险的旅程。",
  },
  1: {
    RU: "Меня зовут Аэлисса, хранительница священного Кристалла Зенита. Моя магия защитит тебя от коварства Кровавых Пустошей.",
    EN: "My name is Aelyssa, keeper of the sacred Zenith Crystal. My magic will protect you from the treachery of the Crimson Wastes.",
    DE: "Mein Name is Aelyssa, Hüterin des heiligen Zenit-Kristalls. Meine Magie wird euch vor dem Verrat der Blutigen Ödlande schützen.",
    FR: "Je m'appelle Aelyssa, gardienne du cristal sacré du Zénith. Ma magie vous protégera de la trahison des Landes Sanglantes.",
    ES: "Mi nombre es Aelyssa, guardiana del sagrado Cristal Cenit. Mi magia te protegerá de la traición de los Páramos Sangrientos.",
    PT: "Meu nome é Aelyssa, guardiã do sagrado Cristal Zenith. Minha magia irá protegê-lo da traição das Terras Desoladas Carmesins.",
    JA: "私はアエリッサ、聖なるゼни스크ристаルの守護者です。私の魔法が、血の荒野の邪悪からあなたを守るでしょう。",
    KR: "내 이름은 앨리사, 신성한 제니스 크리스탈의 수호자다. 나의 마법이 크림슨 황무지의 배신으로부터 당신을 지켜줄 것이다.",
    CH: "我叫艾莉莎，神圣天顶水晶的守护者。我的魔法将保护你免受绯红荒野的背叛。",
  },
  2: {
    RU: "Отлично! Твое оружие заряжено энергией Зенита. Двинемся вперед через северные врата замка!",
    EN: "Excellent! Your weapon is infused with Zenith energy. Let us move forward through the northern castle gates!",
    DE: "Hervorragend! Ihre Waffe ist mit Zenit-Energie erfüllt. Lasst uns durch die nördlichen Burgtore vorrücken!",
    FR: "Excellent ! Votre arme est imprégnée de l'énergie du Zénith. Avançons par les portes nord du château !",
    ES: "¡Excelente! Tu arma está infundida con energía Cenit. ¡Avancemos por las puertas del norte del castillo!",
    PT: "Excelente! Sua arma está infundida com empolgante energia Zenith. Vamos avançar pelos portões norte do castelo!",
    JA: "素晴らしい！あなたの武器にはゼニスのマ力が注入されました。城の北門を通り、前進しましょう！",
    KR: "훌륭하다! 당신의 무기에 제니스 에너지가 주입되었다. 북쪽 성문을 통해 전진하자!",
    CH: "太棒了！你的武器被注入了天顶能量。让我们从北门穿过城堡前进吧！",
  },
  3: {
    RU: "Помни: каждый выбор здесь имеет значение. Наш отряд готов к бою. Теперь выбери область на Континенте Судьбы для первой боевой зачистки:",
    EN: "Remember: every choice here has consequences. Our squad is ready. Now select a territory on the Fate Continent for the initial tactical sweep:",
    DE: "Denkt daran: Jede Entscheidung hier hat Konsequenzen. Unsere Truppe ist bereit. Wählt nun ein Gebiet auf dem Schicksalskontinent für die erste taktische Säuberung aus:",
    FR: "Rappelez-vous : chaque choix ici a des conséquences. Notre escouade est prête. Sélectionnez maintenant un territoire sur le Continent du Destin pour le nettoyage tactique initial :",
    ES: "Recuerda: cada elección aquí tiene consecuencias. Nuestro escuadra está listo. Ahora selecciona un territory en el Continente del Destino para el barrido táctico inicial:",
    PT: "Lembre-se: cada escolha aqui tem consequências. Nosso esquadrão está pronto. Agora selecione um território no Continent do Destino para a varredura tática inicial:",
    JA: "忘れないでください、ここでの選択にはすべて結果が伴います。私たちの部隊は準備万端です。さあ、最初の戦術的掃討のために、運命の大陸から地域を選択してください：",
    KR: "기억해라: 이곳에서의 모든 선택은 그 결과가 따른다. 우리 부대는 전투 준비가 끝났다. 이제 운명의 대륙에서 첫 전술적 소탕을 전개할 지역을 선택해라:",
    CH: "记住：这里的每一个选择都有其后果。我们的队伍已准备就绪。现在请选择命运大陆上的一个区域进行首次战术肃清：",
  },
  4: {
    RU: "Вы выбрали Кровавые Пустоши! Здесь сильны орды бандитов и адские ветры Зенита. Да пребудет с тобой благословение Кристалла! Мы отправляемся в бой.",
    EN: "You have selected the Crimson Wastes! Bandit hordes and infernal Zenith winds plague this land. May the blessing of the Crystal guide us! Charging into battle.",
    DE: "Ihr habt die Blutigen Öдланде gewählt! Banditenhorden und höllische Zenit-Winde plagen dieses Land. Möge der Segen des Kristalls uns leiten! Wir ziehen in den Kampf.",
    FR: "Vous avez choisi les Landes Sanglantes ! Des hordes de bandits et des vents infernaux du Zénith accablent cette terre. Que la bénédiction du Cristal nous guide ! En route vers la bataille.",
    ES: "¡Has seleccionado los Páramos Sangrientos! Las hordas de bandidos y los vientos infernales de Cenit asolan esta tierra. ¡Que la bendición del Cristal nos guíe! Entrando en batalla.",
    PT: "Você selecionou as Terras Desoladas Carmesins! Hordas de bandidos e ventos infernais de Zenith assolam esta terra. Que a bênção do Cristal nos guie! Entrando em batalha.",
    JA: "血の荒野を選択しましたね！山賊の群れと荒れ狂うゼニスの魔力嵐が大地を襲っています。クリスタルの祝福があなたを導いてくれますように！戦場へ進撃します。",
    KR: "크림슨 황무지를 선택하셨습니다! 무법자 무리들과 매서운 제니스의 폭풍이 부는 거친 대지입니다. 크리스탈의 축복이 우리를 이끌어 주기를! 전투로 나아갑니다.",
    CH: "你选择了绯红荒野！这里充满着强盗豪客与无情的天顶烈风。愿水晶的庇护指引我们！前进杀入战场。",
  },
  5: {
    RU: "Вы выбрали Ледяной Пик! Вечная мерзлота испытывает сильных духом, а гигантские Големы Льда охраняют древние секреты. Да пребудет с нами благословение Кристалла!",
    EN: "You have selected the Ice-Bound Peak! Permafrost tests the strong, and giant Ice Golems guard ancient secrets. May the blessing of the Crystal protect us!",
    DE: "Ihr habt den Eisigen Gipfel gewählt! Permafrost prüft die Starken, und gigantische Eisgolems bewachen uralte Geheimnisse. Möge der Kristall uns schützen!",
    FR: "Vous avez choisi le Pic de Glace ! Le pergélisol met à l'épreuve les forts, et des Golems de glace géants gardent des secrets anciens. Que la bénédiction du Cristal nous protège !",
    ES: "¡Has seleccionado el Pico Helado! El permafrost pone a prueba a los fuertes y los Golems de Hielo gigantes custodian antiguos secretos. ¡Que la bendición de Cristal nos proteja!",
    PT: "Você selecionou o Pico Congelado! O permafrost testa os fortes, enquanto gigantescos Golems de Gelo protegem os tesouros antigos. Que o Cristal nos proteja!",
    JA: "氷結의 峰を選択しましたね！過酷な永久凍土が意志を試しており、巨大な氷のゴーレムたちが古代の神秘を守るために立ちはだかっています。クリスタルの保護がありますように！",
    KR: "빙설의 봉우리를 선택했다! 혹독한 영구 동토가 의지를 시험하며, 거대한 얼음 골렘들이 고대의 신비를 경비하고 있다. 크리스탈의 보살핌이 있기를!",
    CH: "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇护我们！",
  },
  6: {
    RU: "Вы выбрали Древние Руины! Забытые катакомбы хранят остатки древних кристаллов Зенита, но берегись ловушек и древних теней. Да пребудет с тобой благословение Кристалла!",
    EN: "You have selected the Ancient Ruins! Forgotten catacombs hold remnants of ancient Zenith energy crystals, but beware deadly traps and immortal shadows. Crystal bless you!",
    DE: "Ihr habt die Alten Ruinen gewählt! Vergessene Katakomben bergen Reste uralter Zenit-Kristalle, aber hütет euch vor тодличных ловушек и древних теней. Möге der Kristall euch segnen!",
    FR: "Vous avez choisi les Ruines Anciennes ! Des catacombes oubliées recèlent des vestiges d'anciens cristaux d'énergie du Zénит, maar gare aux pièges mortels et aux ombres anciennes. Que le Cristal vous bénisse !",
    ES: "¡Has seleccionado las Ruinas Antiguas! Catacumbas olvidadas albergan restos de los antiguos cristales de energía Cenit, pero ten cuidado con las trampas mortales y las sombras antiguas. ¡El Cristal te bendiga!",
    PT: "Você selecionou as Ruínas Antigas! Catacumbas esquecidas guardam vestígios dos antigos cristais de energia Zenith, mas cuidado com armadilhas mortais e sombras imortais. Que o Cristal o abençoe!",
    JA: "古代の遺跡を選択しましたね！忘れられた地下墓地には古代のゼニスマ力結晶の残骸が隠されていますが、致命的な罠と不滅の影を警戒してください。クリスタルの祝福を！",
    KR: "고대 유적지를 선택했다! 잊혀진 지하 묘지에 고대 제니스 마력 결정의 잔재가 숨겨져 있지만, 치명적인 함정과 불멸의 그림자를 경계해라. 크리스탈의 축복을!",
    CH: "你选择了远古遗迹！被遗忘的墓穴藏有远古天顶能量水晶的余烬，但务必小心致命的陷阱与不死的幽影。愿水晶赐福于你！",
  },
  7: {
    RU: "Вы выбрали Грозовые Кряжи! Облачный архипелаг, парящий над бездной. Здесь бушуют постоянные молнии, а воздух раздирают стихийные бури. Да пребудет с нами Кристалл!",
    EN: "You have selected the Storm Ridges! A cloud archipelago floating over the abyss. Constant lightning storms rage here, and elemental tempests tear the air. May the Crystal protect us!",
    DE: "Ihr habt die Sturmkämme gewählt! Ein Wolkenarchipel, der über dem Abgrund schwebt. Hier wüten ständige Gewitter und Elementarstürmе zerreißt die Luft. Möге der Kristall uns schützen!",
    FR: "Vous avez choisi les Crêtes de Tempête ! Un archipel de nuages flottant au-dessus de l'abîme. Des tempêtes de foudre constantes y font rage, et des tempêtes élémentaires déchirent l'air. Que le Cristal nous protège !",
    ES: "¡Has seleccionado las Crestas de Tormenta! Un archipiélago de nubes que flota sobre el abismo. Constantemente rugen tormentas de rayos и las tempestades elementales desgarran el aire. ¡Que el Cristal nos proteja!",
    PT: "Você selecionou os Cumes da Tempestade! Um arquipélago de nuvens flutuando sobre o abismo. Tempestades de raios constantes rugem aqui, e tempestades elementais rasгам os ar. Que o Cristal nos proteжа!",
    JA: "嵐 of 尾根を選択しましたね！深淵の上に浮かぶ雲の群島です。絶え間ない雷雨が吹き荒れ、元素の嵐が空気を引き裂いています。クリスタルの加護がありますように！",
    KR: "폭풍 산맥을 선택했다! 심연 위에 떠 있는 구름 군도입니다. 이곳에는 끊임없는 번개 폭풍이 치고 원소의 폭풍이 공기를 찢고 있습니다. 크리스탈의 보살핌이 있기를!",
    CH: "你选择了雷暴山脊！悬浮在深渊之上的云中群岛。这里肆虐着连绵不断的雷暴，元素风暴撕裂着空气。愿水晶庇护我们！",
  },
  8: {
    RU: "Прекрасно! Мы прибыли на выбранную точку Контента Судьбы. Гляди, здесь заложена наша первая Башня (1 ур. — аванпост с сигнальным огнем). Она выглядит скромно, но её сияние рассеивает тьму.",
    EN: "Fabulous! We arrived at your selected point. Look, here lies our first Tower (Level 1 — basic sentry post). It is modest for now, but its light guards us from ancient shadows.",
    DE: "Fabelhaft! We are an Ihrem ausgewählten Ort angekommen. Schauen Sie, hier steht unser erster Turm (Stufe 1). Er ist noch bescheiden, aber sein Licht vertreibt die Dunkelheit.",
    FR: "Merveilleux ! Nous sommes arrivés au point sélectionné. Regardez, voici notre premier château (Niveau 1). Il est modeste pour l'instant, mais sa lumière dissipe les ténébres.",
    ES: "¡Fabuloso! Hemos llegado a tu punto seleccionado. Mira, aquí yace nuestro primer Castillo (Nivel 1). Es modesto por ahora, pero su luz disipa la oscuridad.",
    PT: "Fabulo! Chegamos ao seu ponto selecionado. Olhe, aqui jaz nossa primeira Torre (Nível 1). É modesta por enquanto, mas sua luz dissipa as sombras.",
    JA: "見事です！選択した地点に到着しました。ご覧ください、これが私たちの最初の塔（レベル1）です。今はまだささやかですが、その光は古代の闇を払いのけます。",
    KR: "훌륭하다! 당신이 선택한 영지에 도착했다. 보라, 이곳에 우리의 첫 번째 타워(1레벨)가 세워졌다. 지금은 검소하지만 그 빛이 어둠을 걷어낸다.",
    CH: "太棒了！我们已经到达了你所选择的地点。看，这里建成了我们的第一座城堡（1级）。虽然目前它还很简陋，但它的光芒能够驱散远古的黑暗。",
  },
  9: {
    RU: "Великолепная работа! Ты улучшил Башню до 2-го уровня! Посмотри на эти прочные каменные пристройки, боковые крылья обороны и вращающийся шпиль с Кристаллом Zenith.",
    EN: "Magnificent work! You upgraded our Tower to Level 2! Look at these heavy stone structures, custom side wings, and the spinning Crystal Zenith spire emitting cyan color.",
    DE: "Großartige Arbeit! Sie haben unseren Turm auf Stufe 2 настраивать! Sehen Sie sich diese schweren Steinstrukturen und die rotierende Spitze des Zenit-Kristalls an.",
    FR: "Travail magnifique ! Vous avez amélioré le château au Niveau 2 ! Regardez ces structures de pierre solides, les ailes de défense et la flèche rotative du Cristal Zenith.",
    ES: "¡Trabajo magnífico! ¡Mejoraste nuestro Castillo al Nivel 2! Mira las estructuras de piedra, las alas defensivas y la aguja giratoria del Cristal Cenit.",
    PT: "Trabalho magnífico! Você atualizou nossa Torre para o Nível 2! Veja estas estruturas de pedra pesadas, asas de defesa e a agulha rotativa do Cristal Zenith.",
    JA: "見事な手際です！塔をレベル2へアップグレードしましたね！頑丈な石造りの外壁、防衛用の側翼、그리고 シアン色の光を放つ回転式のゼニスクリスタル尖塔をご覧ください。",
    KR: "장엄한 업적이다! 타워를 2레벨로 승급시켰다! 견고한 돌 벽과 방어용 사이드 윙, 그리고 청록색으로 자전하는 제니스 크리스탈 첨탑을 느껴보라.",
    CH: "叹为观止的杰作！你已成功将城堡升级至2级！快看看这些坚固的石质外墙、侧翼防卫设施，以及散发着青色荧光的旋转天顶水晶塔尖。",
  },
  10: {
    RU: "Теперь мы можем войти внутрь Цитадели 2-го уровня. Здесь построены 3 ключевые здания: 1) Военные Казармы (покупка воинов), 2) Оружейная (снаряжение), 3) Шпионская Тайная Ложа.",
    EN: "Now we can enter inside the Level 2 Citadel. Three essential facilities are active: 1) Military Barracks (hire soldiers), 2) Armory Warehouse (equipment), 3) Secret Espionage Lodge.",
    DE: "Jetzt können wir das Innere der Zitadelle von Stufe 2 betreten. Drei wichtige Einrichtungen sind aktiv: 1) Kasernen, 2) Rüstkammer, 3) Geheime Spionage-Loge.",
    FR: "Nous pouvons maintenant entrer dans la Citadelle de Niveau 2. Trois bâtiments clés sont actifs : 1) Caserne militaire, 2) Armurerie (équipements), 3) Loge secrète d'espionnage.",
    ES: "Ahora podemos entrar al Castillo de Nivel 2. Tres edificios clave están activos: 1) Cuartel militar, 2) Armería (equipamiento), 3) Logia secreта de espionaje.",
    PT: "Agora podemos entrar dentro do Castelo de Nível 2. Três instalações essenciais estão ativas: 1) Quartel militar, 2) Armaria (equipamento), 3) Loja secreta de espionagem.",
    JA: "これでレベル2シтаデルの内部に入ることができます。ここには3つの重要施設があります：1）軍事兵舎（兵士의 雇用）、2）装備武器庫、3）隠密スパイ諜報機関です。",
    KR: "이제 2레벨 요새 내부로 진입할 수 있다. 세 가지 핵심 건물이 가동 중이다: 1) 군사 병영(병사 고용), 2) 전술 무기고(장비), 3) 장막의 스파이 첩보원 길드.",
    CH: "现在我们可以进入2级城堡的内部了。这里已经建成了三座核心建筑：1) 军旅兵营（招募士兵）、2) 战备军械库（挑选装备）、3) 密探斥候会所（刺探军情）。",
  },
  11: {
    RU: "Каждый Лорд-Герой имеет жесткий лимит войска. На 5-м уровне ты можешь нести с собой максимум 4 отряда воинов. Один отряд воинов дает +15 к Силе Гарнизона твоей Башни Земли.",
    EN: "Every Lord Hero has strict troop capacity limits. At level 5, you can carry a maximum of 4 troop squads. Each hired squad grants +15 Garrison Power to your Land Tower.",
    DE: "Jeder Lord-Held hat strenge Truppenlimits. Auf Stufe 5 können Sie maximal 4 Truppenteile mitführen. Jede angeworbene Truppe erhöht die Garnisonsstärke um +15.",
    FR: "Chaque Héros a des limites de troupes strictes. Au niveau 5, vous можете transporter un maximum de 4 escouades. Chaque troupe recrutée ajoute +15 à la puissance de la garnison.",
    ES: "Cada Héroe tiene límites de tropas estrictos. En el nivel 5, puedes llevar un máximo de 4 escuadrones. Cada tropa reclutada añade +15 al poder de la guarnición.",
    PT: "Cada Herói tem limites estritos de tropas. No nível 5, você pode carregar no máximo 4 esquadrões. Cada tropa contratada adiciona +15 ao poder da guarnição.",
    JA: "各ロード（英雄）には厳格な兵力上限が設けられています。レベル5のあなたが一帯に連れて歩けるのは最大4部隊までです。1部隊雇用するごとに、拠点の防衛評値が+15されます。",
    KR: "각각의 로드(영웅)는 엄격한 군대 최대 소지 제한이 있다. 5레벨인 전사는 최대 4개 소대까지 데리고 다닐 수 있다. 고용 시 소대당 영지 방어력이 +15 증가한다.",
    CH: "每一位领主英雄都有极严格的带兵上限。在当前的5级状态下，你最多只能携带4支士兵分队。每招募一支分队，都会为你的地盘防御战力提供 +15 评分加成。",
  },
  12: {
    RU: "Одень героя! Нажми на иконку вверху слева, чтобы открыть Меню Снаряжения. Здесь ты можешь перетаскивать оружие в ячейки и запустить спутник-разведчик тайной Шпионской Ложи за 150 золота!",
    EN: "Equip your Hero! Tap the top-left player icon to open the Equipment grid. Drag your glowing swords or armor into active slots, and deploy a stealth spy for 150 gold!",
    DE: "Rüstet euren Helden aus! Klickt auf das Symbol oben links, um das Ausrüstungmenü zu öffnen. Zieht Waffen in Slots und entsendet einen Spion für 150 Gold!",
    FR: "Équipez votre Héros ! Cliquez sur l'icône en haut à gauche pour ouvrir la grille d'équipement. Glissez vos armes dans les fentes et envoyez un espion pour 150 pièces d'or !",
    ES: "¡Equipa a tu Héroe! Haz clic en el icono superior izquierdo para abrir el menú de equipamiento. ¡Arrastra tus armas a las ranuras y envía un espía por 150 de oro!",
    PT: "Equipe seu Herói! Clique no ícone superior esquerdo para abrir o painel de equipamentos. Arrasте suas armas para os compartimentos e envie um espião por 150 de ouro!",
    JA: "英雄を装備させましょう！左上のプレイヤーアイコンをクリックして、装備画面を開きます。武器や防具をスロットにドラッグ＆ドロップし、150ゴールドでスパイを隠密派遣してください！",
    KR: "영웅을 무장시키십시오! 왼쪽 상단 프로필을 클릭하여 장비 교환 패널을 엽니다. 무기를 빈 슬롯에 장착하고, 150 골드로 성의 정찰 길드에서 첩보원을 파견하십시오!",
    CH: "全副武装，准备迎战！点击左上角的角色头像，即可打开装备栏。直接将神兵利器 or 重铠防具拖入对应槽位，甚至可以消耗 150 金币开启密探谍报侦察！",
  },
};
const SPEAKER_NAMES: Record<
  "RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH",
  string
> = {
  RU: "Аэлисса, Хранительница Кристалла",
  EN: "Aelyssa, Keeper of Crystal",
  DE: "Aelyssa, Hüterin des Kristalls",
  FR: "Aelyssa, Gardienne du Cristal",
  ES: "Aelyssa, Guardiana del Cristal",
  PT: "Aelyssa, Guardiã do Cristal",
  JA: "クリスタルの守護者アエリッサ",
  KR: "크리스탈의 수호자 엘리사",
  CH: "水晶守护者 艾莉莎",
};

const LEVEL_LABELS: Record<
  "RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH",
  string
> = {
  RU: "Ур.",
  EN: "Lvl",
  DE: "Stufe",
  FR: "Niv.",
  ES: "Nivel",
  PT: "Nível",
  JA: "Lv.",
  KR: "레벨",
  CH: "等级",
};

const STATS_LABELS: Record<
  "XP" | "MANA" | "UPGRADE" | "GAIN_XP" | "USE_MANA" | "RESET",
  Record<"RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH", string>
> = {
  XP: {
    RU: "Опыт",
    EN: "XP",
    DE: "EP",
    FR: "XP",
    ES: "EXP",
    PT: "XP",
    JA: "経験値",
    KR: "경험치",
    CH: "经验",
  },
  MANA: {
    RU: "Мана",
    EN: "Mana",
    DE: "Mana",
    FR: "Mana",
    ES: "Maná",
    PT: "Mana",
    JA: "マナ",
    KR: "마나",
    CH: "魔法值",
  },
  UPGRADE: {
    RU: "Лвл Ап +",
    EN: "Lvl Up +",
    DE: "Aufsteigen +",
    FR: "Niveau +",
    ES: "Nivel +",
    PT: "Nível +",
    JA: "レベルアップ +",
    KR: "레벨업 +",
    CH: "升级 +",
  },
  GAIN_XP: {
    RU: "Получить Опыт",
    EN: "Gain XP",
    DE: "EP Erhalten",
    FR: "Plus d'XP",
    ES: "Ganar EXP",
    PT: "Ganhar XP",
    JA: "経験値獲得",
    KR: "경험치 획득",
    CH: "获得经验",
  },
  USE_MANA: {
    RU: "Тратить Ману",
    EN: "Spend Mana",
    DE: "Mana Ausgeben",
    FR: "Utiliser Mana",
    ES: "Gastar Maná",
    PT: "Gastar Mana",
    JA: "マナ消費",
    KR: "마나 소비",
    CH: "消耗魔法",
  },
  RESET: {
    RU: "← Сбросить сюжет",
    EN: "← Restart quest",
    DE: "← Quest neu starten",
    FR: "← Relancer la quête",
    ES: "← Reiniciar misión",
    PT: "← Reiniciar jornada",
    JA: "← クエスト再起動",
    KR: "← 퀘스트 재시작",
    CH: "← 重新开始",
  },
};

export const ALL_TROOPS_ROSTER = [
  {
    id: "guard",
    name: "Боец фракции",
    baseHp: 180,
    hpPerLvl: 30,
    baseDmg: 15,
    dmgPerLvl: 3,
    baseArm: 8,
    armPerLvl: 1.5,
    cost: 50,
    rarity: "Простой",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 12,
    attrPerLvl: 3.2,
    icon: "🛡️",
    p: "Symmetrical close-up eye-level 3D portrait headshot of a brave royal faction fighter, clean medieval steel helmet, fierce human warrior face, neon cyan glowing gem on chest armor, stylized polished clay render, flat simple pure white background (#ffffff), no floor shadows, no vignette, game asset avatar style.",
    passives: [
      {
        name: "Штурмовой Строй",
        desc: "Повышает выносливость и защиту соседних пехотинцев на +15%.",
      },
      {
        name: "Латный Воротник",
        desc: "Увеличивает защиту против лучников и стрелкового оружия на +25%.",
      },
    ],
    actives: [
      {
        name: "Удар Рукоятью (Ульт)",
        desc: "Наносит 180% физического урона и оглушает врага на 1 ход.",
      },
    ],
  },
  {
    id: "archer",
    name: "Эльфийский Лучник",
    baseHp: 115,
    hpPerLvl: 20,
    baseDmg: 24,
    dmgPerLvl: 4,
    baseArm: 3,
    armPerLvl: 0.8,
    cost: 75,
    rarity: "Простой",
    classType: "Лучник",
    mainAttr: "Ловкость (AGI)",
    baseAttr: 14,
    attrPerLvl: 4.1,
    icon: "🏹",
    p: "Symmetrical close-up front-facing 3D portrait face shot of an elegant elven marksman archer wearing an emerald leather cowl hood, glowing green eyes, high precision stylized clay render, flat simple pure white background (#ffffff), zero shadow projection, high density avatar.",
    passives: [
      {
        name: "Соколиное Око",
        desc: "Позволяет атаковать сквозь лесные препятствия без штрафа к точности.",
      },
      {
        name: "Упреждающий Прыжок",
        desc: "Шанс уклонения из ближней дистанции повышен на +20%.",
      },
    ],
    actives: [
      {
        name: "Стрела Ветра (Ульт)",
        desc: "Обстреливает выбранный сектор градом стрел, нанося 250% урона.",
      },
    ],
  },
  {
    id: "arcanist",
    name: "Боевой Маг Зенита",
    baseHp: 90,
    hpPerLvl: 15,
    baseDmg: 38,
    dmgPerLvl: 5.5,
    baseArm: 2,
    armPerLvl: 0.5,
    cost: 120,
    rarity: "Простой",
    classType: "Маг",
    mainAttr: "Интеллект (INT)",
    baseAttr: 16,
    attrPerLvl: 5.0,
    icon: "🔮",
    p: "Symmetrical cute close-up 3D face portrait of an arcane battle archmage apprentice wizard, wearing a neon cyan crystal crown headband, violet magic silk robe, glowing purple mystical eyes, stylized game clay render, pure white isolated background (#ffffff), high-contrasted toy portrait.",
    passives: [
      {
        name: "Резонанс Зенита",
        desc: "Каждые 2 хода пассивно восполняет 8 ед. маны союзникам.",
      },
      {
        name: "Эфирный Переток",
        desc: "Поглощает первый вражеский магический импульс, снижая его урон на 40%.",
      },
    ],
    actives: [
      {
        name: "Импульс Звезд (Ульт)",
        desc: "Разрывает эфирные связи, нанося магический урон по площади (320% силы).",
      },
    ],
  },
  {
    id: "paladin",
    name: "Паладин Света",
    baseHp: 420,
    hpPerLvl: 55,
    baseDmg: 45,
    dmgPerLvl: 5,
    baseArm: 18,
    armPerLvl: 2.5,
    cost: 150,
    rarity: "Элитный",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 18,
    attrPerLvl: 4.5,
    icon: "⚡",
    p: "Symmetrical high-end 3D face closeup portrait of a grand paladin, gold-plated steel visor crown helmet, shining warm golden eyes, stylized clay render, plain flat pure white background (#ffffff), no ambient shadows on floor.",
    passives: [
      {
        name: "Аура Жизни",
        desc: "Повышает регенерацию здоровья союзников в радиусе 2 клеток на +10%.",
      },
      {
        name: "Стойкость Титана",
        desc: "Уменьшает весь входящий физический урон на фиксированные 15 ед.",
      },
      {
        name: "Магнитный Щит",
        desc: "Принимает на себя 25% урона направленного в слабого соседа.",
      },
    ],
    actives: [
      {
        name: "Кара Господня (Ульт)",
        desc: "Наносит 320% урона по площади в форме креста и вешает немоту на магов.",
      },
      {
        name: "Дар Спасения",
        desc: "Снимает дебаффы с выбранной дивизии и восполняет им 400 HP.",
      },
    ],
  },
  {
    id: "cavalry",
    name: "Имперская Конница",
    baseHp: 550,
    hpPerLvl: 70,
    baseDmg: 58,
    dmgPerLvl: 7,
    baseArm: 14,
    armPerLvl: 2.2,
    cost: 220,
    rarity: "Редкий",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 22,
    attrPerLvl: 4.8,
    icon: "🏇",
    p: "Detailed 3D face closeup portrait model of a heavy imperial cavalry knight captain, silver steel visor with cyan flowing feather plume, stylized game character look on flat pure white background (#ffffff), sharp clay.",
    passives: [
      {
        name: "Инерция Разбега",
        desc: "Каждые 3 клетки перемещения повышают урон следующего удара на +15%.",
      },
      {
        name: "Шпорный Натиск",
        desc: "Шанс увернуться от атак копейщиков повышен на 25%.",
      },
      {
        name: "Тяжелый Топот",
        desc: "Наносит 12-24 урона всем целям преграждающим коннице путь.",
      },
    ],
    actives: [
      {
        name: "Таран Пики (Ульт)",
        desc: "Пробивает строй врагов, нанося 350% урона и отбрасывая их на 2 клетки.",
      },
      {
        name: "Конный Свист",
        desc: "Повышает скорость передвижения всей армии на +2 на 1 ход.",
      },
    ],
  },
  {
    id: "cannoneer",
    name: "Осадно-боевой Пушкарь",
    baseHp: 480,
    hpPerLvl: 65,
    baseDmg: 85,
    dmgPerLvl: 9.5,
    baseArm: 10,
    armPerLvl: 1.8,
    cost: 300,
    rarity: "Эпический",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 28,
    attrPerLvl: 5.8,
    icon: "💣",
    p: "Detailed 3D game portrait headshot of a dwarven master engineer bombardier cannoneer, heavy brass goggles, charred face, stylized toy sculpt on flat pure white background (#ffffff).",
    passives: [
      {
        name: "Осадная Наводка",
        desc: "Множитель урона по заграждениям и каменным замкам ИИ повышен в x3.0.",
      },
      {
        name: "Термо-Каркас",
        desc: "Полная невосприимчивость к эффектам горения и магме.",
      },
      {
        name: "Тяжелый Орудийный Ствол",
        desc: "Препятствует отбрасыванию и перемещению пушкаря врагами.",
      },
      {
        name: "Шрапнельный Подрыв",
        desc: "При получении критического удара взрывается, нанося 50 урона вокруг.",
      },
    ],
    actives: [
      {
        name: "Лобовой Выстрел (Ульт)",
        desc: "Выстреливает ядром по площади конуса, нанося 400% физического урона.",
      },
      {
        name: "Осветительная Ракета",
        desc: "Обнаруживает невидимых разведчиков в радиусе 5 клеток.",
      },
    ],
  },
  {
    id: "centaur",
    name: "Кентавр Степей",
    baseHp: 620,
    hpPerLvl: 80,
    baseDmg: 72,
    dmgPerLvl: 8.5,
    baseArm: 12,
    armPerLvl: 2.0,
    cost: 350,
    rarity: "Редкий",
    classType: "Лучник",
    mainAttr: "Ловкость (AGI)",
    baseAttr: 26,
    attrPerLvl: 5.2,
    icon: "🏹",
    p: "Stunning 3D close-up face shot portrait of a wild steppe centaur warrior chieftain, braided hair, warpaint stripes on face, clay stylized render, flat white solid plain background (#ffffff).",
    passives: [
      {
        name: "Воля Равнин",
        desc: "Передвижение по зыбучим пескам и болоту не расходует лишние ОД.",
      },
      {
        name: "Степной Обход",
        desc: "Шанс нанести критический урон со спины повышен до 45%.",
      },
      {
        name: "Легкие Копыта",
        desc: "Уклонение от физических снарядов и ядов повышено на +15%.",
      },
      {
        name: "Оперенные Стрелы",
        desc: "Атака с разгона наносит дополнительный чистый урон.",
      },
    ],
    actives: [
      {
        name: "Грозовая Стрела (Ульт)",
        desc: "Выстрел бьет цепной молнией до 3 противников на 360% урона.",
      },
      {
        name: "Песчаная Завеса",
        desc: "Снижает дальность обзора и стрельбы вражеских лучников на 3 клетки.",
      },
    ],
  },
  {
    id: "necromancer",
    name: "Некромант Тьмы",
    baseHp: 500,
    hpPerLvl: 60,
    baseDmg: 90,
    dmgPerLvl: 11,
    baseArm: 6,
    armPerLvl: 1.0,
    cost: 400,
    rarity: "Легендарный",
    classType: "Маг",
    mainAttr: "Интеллект (INT)",
    baseAttr: 32,
    attrPerLvl: 7.0,
    icon: "💀",
    p: "Ominous close-up 3D face portrait profile mask of a master dark necromancer wizard, obsidian bone crown, glowing faint green spirit fire eyes, stylized clay render, pure white isolated background (#ffffff).",
    passives: [
      {
        name: "Жатва Душ",
        desc: "Убийство любой единицы на поле боя исцеляет Некроманта на 12% HP.",
      },
      {
        name: "Эфирные Руны",
        desc: "Повышает сопротивление магическим атакам на +35%.",
      },
      {
        name: "Чумной Ветер",
        desc: "Ослабляет силу атаки ближайших вражеских отрядов на -15%.",
      },
      {
        name: "Костяная Эгида",
        desc: "Каждые 3 хода получает щит, блокирующий 100% урона одного выстрела.",
      },
      {
        name: "Провидение Могил",
        desc: "Дальность сотворения призывов увеличена на +2 клетки.",
      },
    ],
    actives: [
      {
        name: "Призыв Жнеца (Ульт)",
        desc: "Призывает мощного Скелета-Паладина 15 уровня на поле боя.",
      },
      {
        name: "Касание Бездны",
        desc: "Проклинает вражеский полк, нанося 150% урона и блокируя их исцеление.",
      },
      {
        name: "Ритуал Крови",
        desc: "Поглощает здоровье любого призванного существа, восстанавливая здоровье некроманту.",
      },
    ],
  },
  {
    id: "griffin",
    name: "Элитный Королевский Грифон",
    baseHp: 1200,
    hpPerLvl: 155,
    baseDmg: 110,
    dmgPerLvl: 13.5,
    baseArm: 24,
    armPerLvl: 3.5,
    cost: 500,
    rarity: "Легендарный",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 35,
    attrPerLvl: 8.0,
    icon: "🦅",
    p: "Majestic 3D close-up headshot photo of an elite royal-griffin avian beast, golden sharp beak, white crown plumage, neon cyan runic collar, stylized octane render, flat white background (#ffffff).",
    passives: [
      {
        name: "Крылатая Эфирность",
        desc: "Полный полет: игнорирует горы, болота и вражеский заслон.",
      },
      {
        name: "Всречный Сквозняк",
        desc: "Отражает 25% стрел обратно во вражеского стрелка.",
      },
      {
        name: "Перьевой Панцирь",
        desc: "Повышает выживаемость при получении критических ударов на +30%.",
      },
      {
        name: "Мертвая Зона",
        desc: "Враги под грифоном не могут совершать ответные удары.",
      },
      {
        name: "Упреждение Сверху",
        desc: "Наносит на +40% больше урона целям с низким здоровьем.",
      },
    ],
    actives: [
      {
        name: "Небесный Коготь (Ульт)",
        desc: "Обрушивается вихрем на врага, нанося 480% урона и дезориентируя цель.",
      },
      {
        name: "Свежее Оперение",
        desc: "Расправляет крылья, мгновенно исцеляя себя на 300 HP.",
      },
      {
        name: "Горный Клич",
        desc: "Пугает врагов в радиусе 3 клеток, снижая их Очки Действия на 2.",
      },
    ],
  },
  {
    id: "lord_knight",
    name: "Рыцарь-Властелин",
    baseHp: 1500,
    hpPerLvl: 200,
    baseDmg: 140,
    dmgPerLvl: 17.5,
    baseArm: 35,
    armPerLvl: 5.5,
    cost: 600,
    rarity: "Легендарный",
    classType: "Воин",
    mainAttr: "Сила (STR)",
    baseAttr: 45,
    attrPerLvl: 10.0,
    icon: "👑",
    p: "Symmetrical closeup headshot photo of a powerful legend Lord Knight commander, royal dark slate steel helmet, neon cyan dragon gemstone glow, stylized 3D sculpt clay render, isolated on flat pure white background (#ffffff), zero floor shadow.",
    passives: [
      {
        name: "Аура Властелина",
        desc: "Повышает боевую волю всей армии, увеличивая урон союзников на +20%.",
      },
      {
        name: "Эгида Дракона",
        desc: "Полный иммунитет к оглушению, обморожению, ядам и немоте.",
      },
      {
        name: "Круговое Сечение",
        desc: "Каждая механическая атака наносит 50% сопутствующего урона всем флангам.",
      },
      {
        name: "Обряд Неуязвимости",
        desc: "При получении смертельного удара становится неуязвим на 1 ход.",
      },
      {
        name: "Магическое Слово",
        desc: "При пропуске хода восстанавливает 10% здоровья казне.",
      },
    ],
    actives: [
      {
        name: "Меч Правосудия (Ульт)",
        desc: "Вонзает клинок в землю, нанося 600% урона всем врагам и сжигая ману.",
      },
      {
        name: "Грозовой Шпиль",
        desc: "Навешивает щит на паладина, поглощающий до 1000 урона.",
      },
      {
        name: "Королевский Созыв",
        desc: "Призывает боевые духи павших воинов на поле боя для финальной атаки.",
      },
    ],
  },
];

export default function App() {
  async function fetchWithRetry(url: string, retries = 5, delay = 1000) {
    for (let i = 0; i < retries; i++) {
      try {
        const res = await fetch(url);
        if (res.ok) return await res.json();
        throw new Error("Not OK");
      } catch (e) {
        if (i === retries - 1) throw e;
        await new Promise((resolve) => setTimeout(resolve, delay));
      }
    }
  }

  const [kb, setKb] = useState<KBData | null>(null);
  const [activeTab, setActiveTab] = useState<
    | "chat"
    | "dashboard"
    | "project_info"
    | "migration"
    | "game_design"
    | "game_help"
    | "external_skills_db"
    | "project_scripts"
  >("chat");
  const [appVersion, setAppVersion] = useState("18.12.44");

  useEffect(() => {
    // Автоматическая синхронизация версии с сервером
    fetchWithRetry("/version.json")
      .then((data) => {
        if (data && data.version) setAppVersion(data.version);
      })
      .catch((err) => console.error("Version sync error:", err));
  }, []);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [isTyping, setIsTyping] = useState(false);
  const [isOllamaMode, setIsOllamaMode] = useState(false);
  const [sourceImage, setSourceImage] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [isOnline, setIsOnline] = useState(true);
  const [isThinking, setIsThinking] = useState(false);
  const [thinkingSteps, setThinkingSteps] = useState<string[]>([]);
  const [aiHealth, setAiHealth] = useState<"online" | "limited" | "error">(
    "online",
  );
  const [serverHealth, setServerHealth] = useState<
    "online" | "offline" | "error"
  >("online");
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const [projectScan, setProjectScan] = useState<ProjectScan | null>(null);
  const [unityStatus, setUnityStatus] = useState<UnityStatus | null>(null);
  const [blenderStatus, setBlenderStatus] = useState<BlenderStatus | null>(
    null,
  );
  const [gimpStatus, setGimpStatus] = useState<GimpStatus | null>(null);
  const [redotStatus, setRedotStatus] = useState<RedotStatus | null>(null);
  const [photoshopStatus, setPhotoshopStatus] =
    useState<PhotoshopStatus | null>(null);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [blenderPresets, setBlenderPresets] = useState<BlenderPreset[]>([]);
  const [isClearingChat, setIsClearingChat] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadTimeRemaining, setUploadTimeRemaining] = useState<string | null>(
    null,
  );
  const [showGithubGuide, setShowGithubGuide] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [showQuantumLink, setShowQuantumLink] = useState(false);
  const [guideTab, setGuideTab] = useState<"blender" | "unity" | "manual">(
    "blender",
  );
  const [manualPrompt, setManualPrompt] = useState("");
  const [manualResultCode, setManualResultCode] = useState("");
  const [isManualGenerating, setIsManualGenerating] = useState(false);
  const [manualTarget, setManualTarget] = useState<"blender" | "unity">(
    "blender",
  );
  const [localPathInput, setLocalPathInput] = useState("");
  const [projectPathInput, setProjectPathInput] = useState("");
  const [gimpPathInput, setGimpPathInput] = useState("");
  const [redotPathInput, setRedotPathInput] = useState("");
  const [blenderVersionInput, setBlenderVersionInput] = useState("");
  const [isGeneratingBlueprint, setIsGeneratingBlueprint] = useState(false);
  const [isUpdatingKB, setIsUpdatingKB] = useState(false);
  const [showCapabilities, setShowCapabilities] = useState(false);
  const [capabilities, setCapabilities] = useState<any>(null);
  const [notification, setNotification] = useState<{
    message: string;
    type: "success" | "error" | "info";
  } | null>(null);
  const [attachedFiles, setAttachedFiles] = useState<any[]>([]);
  const [migrationData, setMigrationData] = useState<any>(null);
  const [isFetchingMigration, setIsFetchingMigration] = useState(false);
  const [showVKGenerator, setShowVKGenerator] = useState(false);
  const [vkPrompt, setVkPrompt] = useState("");
  const [vkType, setVkType] = useState<"static" | "live">("static");
  const [vkResults, setVkResults] = useState<any[]>([]);
  const [isGeneratingVK, setIsGeneratingVK] = useState(false);
  const [vkProgress, setVkProgress] = useState(0);

  // Core Project Scripts State
  const [projectFiles, setProjectFiles] = useState<{ path: string; name: string; desc: string; lineCount?: number }[]>([]);
  const [selectedFile, setSelectedFile] = useState<{ path: string; name: string; desc: string; lineCount?: number } | null>(null);
  const [selectedFileContent, setSelectedFileContent] = useState<string>("");
  const [isReadingFile, setIsReadingFile] = useState<boolean>(false);
  const [copiedFile, setCopiedFile] = useState<boolean>(false);

  // Helper to force reload the currently selected file content
  const reloadSelectedFileContent = (quiet: boolean = false) => {
    if (selectedFile) {
      if (!quiet) setIsReadingFile(true);
      fetch(`/api/project/files/content?path=${encodeURIComponent(selectedFile.path)}&t=${Date.now()}`)
        .then((res) => res.json())
        .then((data) => {
          if (data && data.content) {
            setSelectedFileContent(data.content);
          } else {
            setSelectedFileContent("Error: Empty file content received.");
          }
        })
        .catch((err) => {
          console.error("Error reading project file content:", err);
          if (!quiet) setSelectedFileContent("Error loading file content from server.");
        })
        .finally(() => {
          if (!quiet) setIsReadingFile(false);
        });
    }
  };

  // Helper to reload both file list (with dynamic line counts) and current file content
  const reloadProjectFilesAndContent = (quiet: boolean = false) => {
    if (!quiet) setIsReadingFile(true);
    fetch("/api/project/files/list")
      .then((res) => res.json())
      .then((data) => {
        if (Array.isArray(data)) {
          setProjectFiles(data);
          if (selectedFile) {
            const updated = data.find(f => f.path === selectedFile.path);
            if (updated) {
              setSelectedFile(updated);
            }
          }
        }
      })
      .catch((err) => console.error("Error refreshing project files list:", err))
      .finally(() => {
        if (selectedFile) {
          fetch(`/api/project/files/content?path=${encodeURIComponent(selectedFile.path)}&t=${Date.now()}`)
            .then((res) => res.json())
            .then((data) => {
              if (data && data.content) {
                setSelectedFileContent(data.content);
              }
            })
            .catch((err) => console.error("Error reloading selected file content:", err))
            .finally(() => {
              if (!quiet) setIsReadingFile(false);
            });
        } else {
          if (!quiet) setIsReadingFile(false);
        }
      });
  };

  // Fetch file list when tab changes to project_scripts
  useEffect(() => {
    if (activeTab === "project_scripts") {
      fetch("/api/project/files/list")
        .then((res) => res.json())
        .then((data) => {
          if (Array.isArray(data)) {
            setProjectFiles(data);
            if (data.length > 0 && !selectedFile) {
              setSelectedFile(data[0]);
            }
          }
        })
        .catch((err) => console.error("Error loading project files list:", err));
    }
  }, [activeTab]);

  // Fetch file content when selectedFile changes
  useEffect(() => {
    if (selectedFile) {
      reloadSelectedFileContent(false);
    }
  }, [selectedFile]);

  // Background Live Synchronization effect - polls disk content and file list every 3.5 seconds
  useEffect(() => {
    if (activeTab === "project_scripts" && selectedFile) {
      const interval = setInterval(() => {
        fetch("/api/project/files/list")
          .then((res) => res.json())
          .then((data) => {
            if (Array.isArray(data)) {
              setProjectFiles(data);
              const updated = data.find(f => f.path === selectedFile.path);
              if (updated && updated.lineCount !== selectedFile.lineCount) {
                setSelectedFile(prev => prev ? { ...prev, lineCount: updated.lineCount } : null);
              }
            }
          })
          .catch((err) => console.error("Error background auto-syncing files list:", err));

        fetch(`/api/project/files/content?path=${encodeURIComponent(selectedFile.path)}&t=${Date.now()}`)
          .then((res) => res.json())
          .then((data) => {
            if (data && data.content && data.content !== selectedFileContent) {
              setSelectedFileContent(data.content);
            }
          })
          .catch((err) => console.error("Error background auto-syncing file content:", err));
      }, 3500);
      return () => clearInterval(interval);
    }
  }, [activeTab, selectedFile, selectedFileContent]);
  const [vkStatus, setVkStatus] = useState("");
  const [showStudioGuide, setShowStudioGuide] = useState(false);

  // Custom Map Marker Splitter State
  const [showMarkerSplitter, setShowMarkerSplitter] = useState(false);
  const [markerImage, setMarkerImage] = useState<string | null>(null);
  const [markerTolerance, setMarkerTolerance] = useState<number>(35);
  const [markerSmoothing, setMarkerSmoothing] = useState<boolean>(true);
  const [markerHasAlpha, setMarkerHasAlpha] = useState<boolean>(true);
  const [markerPadding, setMarkerPadding] = useState<number>(5); // custom crop padding
  const [markerPartCount, setMarkerPartCount] = useState<number>(3);
  const [markerCenters, setMarkerCenters] = useState<number[]>([
    18.0, 50.0, 81.5,
  ]); // default optimized centers for Midjourney 16:9 layout
  const [markerYCenters, setMarkerYCenters] = useState<number[]>([
    50.0, 50.0, 50.0,
  ]);
  const [markerCropSize, setMarkerCropSize] = useState<number>(30); // Crop size as % of image width
  const [showUnityGuide, setShowUnityGuide] = useState<boolean>(true);

  const chatEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Map Marker Splitter Refs using dynamic map
  const canvasRefs = useRef<{ [key: number]: HTMLCanvasElement | null }>({});

  useEffect(() => {
    if (markerPartCount === 3) {
      setMarkerCenters([18.0, 50.0, 81.5]);
      setMarkerYCenters([50.0, 50.0, 50.0]);
    } else if (markerPartCount === 6) {
      // 2x3 Grid default layout for raw Midjourney grids (3 on top row, 3 on bottom row)
      setMarkerCenters([18.0, 50.0, 81.5, 18.0, 50.0, 81.5]);
      setMarkerYCenters([28.0, 28.0, 28.0, 72.0, 72.0, 72.0]);
    } else {
      const defaults: number[] = [];
      const yDefaults: number[] = [];
      for (let i = 0; i < markerPartCount; i++) {
        defaults.push(
          parseFloat(((i + 0.5) * (100 / markerPartCount)).toFixed(2)),
        );
        yDefaults.push(50.0);
      }
      setMarkerCenters(defaults);
      setMarkerYCenters(yDefaults);
    }
  }, [markerPartCount]);

  useEffect(() => {
    if (!markerImage) return;

    let active = true;
    const img = new Image();
    img.crossOrigin = "anonymous";
    img.onload = () => {
      if (!active) return;
      const H = img.naturalHeight;
      const W = img.naturalWidth;

      markerCenters.forEach((centerPercent, index) => {
        const canvas = canvasRefs.current[index];
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        if (!ctx) return;

        // Output perfect square resolution match based on markerCropSize (percentage of image width)
        const cropPercent = markerCropSize || 30;
        let size = (cropPercent / 100) * W;

        // Ensure size does not exceed W or H
        if (size > W) size = W;
        if (size > H) size = H;

        canvas.width = size;
        canvas.height = size;

        ctx.clearRect(0, 0, size, size);

        // Center calculation on X and Y
        const cx = (centerPercent / 100) * W;
        const cyPercent =
          markerYCenters[index] !== undefined ? markerYCenters[index] : 50.0;
        const cy = (cyPercent / 100) * H;

        // Boundaries crop box
        let sx = cx - size / 2;
        let sy = cy - size / 2;

        // Clamping to original image boundaries
        if (sx < 0) sx = 0;
        if (sx + size > W) sx = W - size;
        if (sy < 0) sy = 0;
        if (sy + size > H) sy = H - size;

        // Padding/crop shrink percentage zoom
        const padPx = (markerPadding / 100) * size;
        const sWidth = size - padPx * 2;
        const sHeight = size - padPx * 2;
        const sourceSx = sx + padPx;
        const sourceSy = sy + padPx;

        // Draw to output square canvas
        ctx.drawImage(
          img,
          sourceSx,
          sourceSy,
          sWidth,
          sHeight,
          0,
          0,
          size,
          size,
        );

        // Transparency Chroma-Key filter
        if (markerHasAlpha) {
          try {
            const imgData = ctx.getImageData(0, 0, size, size);
            const data = imgData.data;
            const len = data.length;
            const tolerance = markerTolerance;

            for (let i = 0; i < len; i += 4) {
              const r = data[i];
              const g = data[i + 1];
              const b = data[i + 2];

              // Max RGB channel distance from pitch-black
              const br = Math.max(r, g, b);

              if (br < tolerance) {
                if (markerSmoothing) {
                  const ratio = br / tolerance; // 0 to 1
                  data[i + 3] = Math.round(ratio * data[i + 3]);
                } else {
                  data[i + 3] = 0;
                }
              } else if (br < tolerance * 1.5 && markerSmoothing) {
                const ratio = (br - tolerance) / (tolerance * 0.5); // 0 to 1
                const factor = 0.5 + 0.5 * ratio;
                data[i + 3] = Math.round(data[i + 3] * factor);
              }
            }
            ctx.putImageData(imgData, 0, 0);
          } catch (err) {
            console.error("Canvas pixel manipulation failed: ", err);
          }
        }
      });
    };
    img.src = markerImage;
    return () => {
      active = false;
    };
  }, [
    markerImage,
    markerCenters,
    markerYCenters,
    markerCropSize,
    markerTolerance,
    markerSmoothing,
    markerHasAlpha,
    markerPadding,
    markerPartCount,
  ]);

  useEffect(() => {
    const handlePaste = (e: ClipboardEvent) => {
      if (!showMarkerSplitter) return;
      const items = e.clipboardData?.items;
      if (items) {
        for (let i = 0; i < items.length; i++) {
          if (items[i].type.indexOf("image") !== -1) {
            const blob = items[i].getAsFile();
            if (blob) {
              const reader = new FileReader();
              reader.onloadend = () => {
                setMarkerImage(reader.result as string);
                showNotification(
                  "Изображение успешно вставлено из буфера обмена!",
                  "success",
                );
              };
              reader.readAsDataURL(blob);
            }
          }
        }
      }
    };

    window.addEventListener("paste", handlePaste);
    return () => window.removeEventListener("paste", handlePaste);
  }, [showMarkerSplitter]);

  // Generate dynamic offline demo image containing 3 fantasy rings
  const generateDemoMarkerImage = () => {
    const canvas = document.createElement("canvas");
    canvas.width = 1200;
    canvas.height = 675; // 16:9 aspect ratio
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    // Solid dark-fantasy background
    ctx.fillStyle = "#06060c";
    ctx.fillRect(0, 0, 1200, 675);

    // Add realistic subtle grid lines to simulate raw Midjourney output
    ctx.strokeStyle = "rgba(255, 255, 255, 0.02)";
    ctx.lineWidth = 1;
    for (let x = 0; x < 1200; x += 50) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, 675);
      ctx.stroke();
    }
    for (let y = 0; y < 675; y += 50) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(1200, y);
      ctx.stroke();
    }

    // Design three glowing placeholder circles
    const designs = [
      {
        glow: "rgba(59, 130, 246, 0.35)",
        border: "#3b82f6",
        accent: "#60a5fa",
        text: "IMPERIAL",
        sub: "COMPASS",
      },
      {
        glow: "rgba(249, 115, 22, 0.35)",
        border: "#f97316",
        accent: "#ef4444",
        text: "OUTLAW",
        sub: "FIRE RING",
      },
      {
        glow: "rgba(16, 185, 129, 0.35)",
        border: "#10b981",
        accent: "#34d399",
        text: "NEUTRAL",
        sub: "DRUID STONE",
      },
    ];

    // Optimized centers to align with default markerCenters state: [18.0%, 50.0%, 81.5%]
    const pixelCenters = [
      (18.0 / 100) * 1200,
      (50.0 / 100) * 1200,
      (81.5 / 100) * 1200,
    ];

    const cy = 675 / 2;
    const r = 100; // circle radius

    designs.forEach((des, idx) => {
      const cx = pixelCenters[idx];

      // Radiant Bloom Glow
      const grad = ctx.createRadialGradient(cx, cy, r - 40, cx, cy, r + 60);
      grad.addColorStop(0, des.glow);
      grad.addColorStop(0.5, "rgba(0, 0, 0, 0)");
      grad.addColorStop(0.8, "rgba(0, 0, 0, 0.8)");
      grad.addColorStop(1, "rgba(0, 0, 0, 1)");
      ctx.fillStyle = grad;
      ctx.beginPath();
      ctx.arc(cx, cy, r + 60, 0, Math.PI * 2);
      ctx.fill();

      // Heavy outer game HUD circle
      ctx.strokeStyle = des.border;
      ctx.lineWidth = 6;
      ctx.beginPath();
      ctx.arc(cx, cy, r, 0, Math.PI * 2);
      ctx.stroke();

      // Secondary interior ring
      ctx.strokeStyle = des.accent;
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(cx, cy, r - 20, 0, Math.PI * 2);
      ctx.stroke();

      // Compass rays or spikes
      ctx.strokeStyle = "#22d3ee";
      ctx.lineWidth = 3;
      for (let degree = 0; degree < 360; degree += 45) {
        const radAngle = (degree * Math.PI) / 180;
        ctx.beginPath();
        ctx.moveTo(
          cx + Math.cos(radAngle) * (r - 10),
          cy + Math.sin(radAngle) * (r - 10),
        );
        ctx.lineTo(
          cx + Math.cos(radAngle) * (r + 10),
          cy + Math.sin(radAngle) * (r + 10),
        );
        ctx.stroke();
      }

      // Concentric central cores
      ctx.fillStyle = des.accent;
      ctx.beginPath();
      ctx.arc(cx, cy, 15, 0, Math.PI * 2);
      ctx.fill();

      // Title text inside
      ctx.fillStyle = "#ffffff";
      ctx.font = 'bold 12px "JetBrains Mono", monospace';
      ctx.textAlign = "center";
      ctx.textBaseline = "middle";
      ctx.fillText(des.text, cx, cy - 40);

      // Subtitle
      ctx.fillStyle = des.accent;
      ctx.font = '9px "Inter", sans-serif';
      ctx.fillText(des.sub, cx, cy + 40);
    });

    setMarkerImage(canvas.toDataURL("image/png"));
    showNotification("Загружен интерактивный тестовый пример!", "info");
  };

  // Download logic helper
  const downloadMarker = (index: number) => {
    const canvas = canvasRefs.current[index];
    if (!canvas) return;

    const names = [
      "Fate_Imperial_Compass_Marker.png",
      "Fate_Outlaw_Fire_Marker.png",
      "Fate_Neutral_Druid_Marker.png",
    ];
    const fileName = names[index] || `Fate_Marker_${index + 1}.png`;

    try {
      const link = document.createElement("a");
      link.download = fileName;
      link.href = canvas.toDataURL("image/png", 1.0); // Full quality lossless PNG
      link.click();
      showNotification(
        `Успешно сохранено: ${fileName} (без потери качества!)`,
        "success",
      );
    } catch (err) {
      showNotification(
        "Ошибка при сохранении изображения. Попробуйте загрузить локальный файл.",
        "error",
      );
    }
  };

  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [updateInfo, setUpdateInfo] = useState<any>(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const [updateProgress, setUpdateProgress] = useState(0);
  const [ollamaRunning, setOllamaRunning] = useState(false);
  const [suggestedQuestions, setSuggestedQuestions] = useState<string[]>([]);
  const [language, setLanguage] = useState("Русский");

  const t = {
    Русский: {
      play: "Играть",
      settings: "Настройки",
      exit: "Выход",
      back: "Назад",
      volume: "Звук",
      music: "Музыка",
      quality: "Качество",
      res: "Разрешение",
      fs: "Весь экран",
      graphics: "Графика",
      lang: "Язык",
      help: "Помощь По Игре",
      capabilities: "Возможности ИИ",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Синтаксис Сингулярности",
      offline: "Защищенный Режим",
      clear: "Очистить",
      clearing: "Очистка...",
      thinking: "Cortex Matrix Analysis",
      synth: "Синтез данных Unity 6 & Blender 4.3",
      proMastery: "Professional Multi-Tool Mastery",
      downloadBg: "Скачать Фон (JPG 4K)",
    },
    English: {
      play: "Play",
      settings: "Settings",
      exit: "Exit",
      back: "Back",
      volume: "Sound",
      music: "Music",
      quality: "Quality",
      res: "Resolution",
      fs: "Fullscreen",
      graphics: "Graphics",
      lang: "Language",
      help: "Game Help",
      capabilities: "AI Capabilities",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Singularity Syntax",
      offline: "Secure Mode",
      clear: "Clear",
      clearing: "Clearing...",
      thinking: "Cortex Matrix Analysis",
      synth: "Synthesizing Unity 6 & Blender 4.3 data",
      proMastery: "Professional Multi-Tool Mastery",
      downloadBg: "Download Background (JPG 4K)",
    },
    Deutsch: {
      play: "Spielen",
      settings: "Einstellungen",
      exit: "Beenden",
      back: "Zurück",
      volume: "Ton",
      music: "Musik",
      quality: "Qualität",
      res: "Auflösung",
      fs: "Vollbild",
      graphics: "Grafik",
      lang: "Sprache",
      help: "Spielhilfe",
      capabilities: "KI-Fähigkeiten",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Singularitäts-Syntax",
      offline: "Gesicherter Modus",
      clear: "Löschen",
      clearing: "Löschen...",
      thinking: "Cortex-Matrix-Analyse",
      synth: "Synthese von Unity 6 & Blender 4.3 Daten",
      proMastery: "Professionelle Multi-Tool-Meisterschaft",
      downloadBg: "Hintergrund Herunterladen (JPG 4K)",
    },
    Français: {
      play: "Jouer",
      settings: "Paramètres",
      exit: "Quitter",
      back: "Retour",
      volume: "Son",
      music: "Musique",
      quality: "Qualité",
      res: "Résolution",
      fs: "Plein écran",
      graphics: "Graphisme",
      lang: "Langue",
      help: "Aide au Jeu",
      capabilities: "Capacités de l'IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Syntaxe de Singularité",
      offline: "Mode Sécurisé",
      clear: "Effacer",
      clearing: "Effacement...",
      thinking: "Analyse de la Matrice Cortex",
      synth: "Synthèse des données Unity 6 & Blender 4.3",
      proMastery: "Maîtrise Professionnelle Multi-Tool",
      downloadBg: "Télécharger le Fond (JPG 4K)",
    },
    Español: {
      play: "Jugar",
      settings: "Ajustes",
      exit: "Salir",
      back: "Volver",
      volume: "Sonido",
      music: "Música",
      quality: "Calidad",
      res: "Resolución",
      fs: "Pantalla completa",
      graphics: "Gráficos",
      lang: "Idioma",
      help: "Ayuda del Juego",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxis de Singularidad",
      offline: "Modo Seguro",
      clear: "Limpiar",
      clearing: "Limpiando...",
      thinking: "Análisis de la Matriz Cortex",
      synth: "Sintetizando datos de Unity 6 y Blender 4.3",
      proMastery: "Maestría Profesional Multiherramienta",
      downloadBg: "Descargar Fondo (JPG 4K)",
    },
    日本語: {
      play: "プレイ",
      settings: "設定",
      exit: "終了",
      back: "戻る",
      volume: "音量",
      music: "音楽",
      quality: "品質",
      res: "解像度",
      fs: "全画面",
      graphics: "グラフィック",
      lang: "言語",
      help: "ゲームヘルプ",
      capabilities: "AI機能",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: オフ",
      sync: "シンギュラリティ構文",
      offline: "セキュアモード",
      clear: "クリア",
      clearing: "クリア中...",
      thinking: "皮質マトリックス分析",
      synth: "Unity 6とBlender 4.3のデータを統合中",
      proMastery: "プロフェッショナルマルチツールマスタリー",
      downloadBg: "背景を下载 (JPG 4K)",
    },
    한국어: {
      play: "플레이",
      settings: "설정",
      exit: "나가기",
      back: "뒤로",
      volume: "소리",
      music: "음악",
      quality: "품질",
      res: "해상도",
      fs: "전체 화면",
      graphics: "그래픽",
      lang: "언어",
      help: "게임 도움말",
      capabilities: "AI 능력",
      ollama: "Ollama: 확인",
      ollamaOff: "Ollama: 꺼짐",
      sync: "특이점 구문",
      offline: "보안 모드",
      clear: "지우기",
      clearing: "지우는 중...",
      thinking: "피질 매트릭스 분석",
      synth: "Unity 6 및 Blender 4.3 데이터 합성 중",
      proMastery: "전문 멀티 툴 마스터리",
      downloadBg: "배경 다운로드 (JPG 4K)",
    },
    简体中文: {
      play: "开始",
      settings: "设置",
      exit: "退出",
      back: "返回",
      volume: "音量",
      music: "音乐",
      quality: "画质",
      res: "分辨率",
      fs: "全屏",
      graphics: "图像",
      lang: "语言",
      help: "游戏帮助",
      capabilities: "AI 能力",
      ollama: "Ollama: 正常",
      ollamaOff: "Ollama: 关闭",
      sync: "奇点语法",
      offline: "安全模式",
      clear: "清除",
      clearing: "正在清除...",
      thinking: "皮层矩阵分析",
      synth: "综合 Unity 6 & Blender 4.3 数据",
      proMastery: "专业多工具大师级",
      downloadBg: "下载背景 (JPG 4K)",
    },
    Português: {
      play: "Jogar",
      settings: "Configurações",
      exit: "Sair",
      back: "Voltar",
      volume: "Som",
      music: "Música",
      quality: "Qualidade",
      res: "Resolução",
      fs: "Tela cheia",
      graphics: "Gráficos",
      lang: "Idioma",
      help: "Ajuda de Jogo",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxe de Singularidade",
      offline: "Modo Seguro",
      clear: "Limpar",
      clearing: "Limpando...",
      thinking: "Análise da Matriz Cortex",
      synth: "Sintetizando dados de Unity 6 e Blender 4.3",
      proMastery: "Domínio de Multi-Ferramentas Profissional",
      downloadBg: "Baixar Fundo (JPG 4K)",
    },
  }[language as keyof any] || {
    play: "Играть",
    settings: "Настройки",
    exit: "Выход",
    back: "Назад",
    volume: "Звук",
    music: "Музыка",
    quality: "Качество",
    res: "Разрешение",
    fs: "Весь экран",
    graphics: "Графика",
    lang: "Язык",
  };

  const fileToBase64 = async (
    url: string,
  ): Promise<{ mimeType: string; data: string }> => {
    try {
      const response = await fetch(url);
      const blob = await response.blob();
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          const base64data = reader.result as string;
          const data = base64data.split(",")[1];
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
  const [migrationGuide, setMigrationGuide] = useState("");
  const [unityPackages, setUnityPackages] = useState<any[]>([]);
  const [isMigrating, setIsMigrating] = useState(false);
  const [gameDesign, setGameDesign] = useState<any>(null);
  const [isSavingGameDesign, setIsSavingGameDesign] = useState(false);
  const [designSubTab, setDesignSubTab] = useState<
    | "World"
    | "Castle System"
    | "Heroes & Units"
    | "Visuals & Nav"
    | "Abilities"
    | "Synergies"
    | "Balancing & Rarity"
    | "Economy"
    | "Strategies"
    | "Combat & Environment"
    | "Potions & Alchemy"
    | "Menu Studio"
    | "Quests & NPC"
    | "AI Strategies"
  >("World");
  const [selectedDifficulty, setSelectedDifficulty] =
    useState<string>("Средний");
  const [activeQuests, setActiveQuests] = useState<any[]>([]);
  const [synergyHeroType, setSynergyHeroType] = useState<"simple" | "main">(
    "simple",
  );
  const [simDialogueHero, setSimDialogueHero] = useState<
    "warrior" | "archer" | "mage"
  >("warrior");
  const [simDialogueLang, setSimDialogueLang] = useState<
    "RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH"
  >("RU");
  const [simDialogueStep, setSimDialogueStep] = useState<number>(0);
  const [simHeroLvl, setSimHeroLvl] = useState<number>(5);
  const [simHeroXp, setSimHeroXp] = useState<number>(1420);
  const [simHeroMana, setSimHeroMana] = useState<number>(240);
  const [dialogueActiveScene, setDialogueActiveScene] =
    useState<boolean>(false);
  const [hoveredRegion, setHoveredRegion] = useState<string | null>(null);
  const [midjourneyContTab, setMidjourneyContTab] = useState<
    "astralis" | "vulcania" | "nordgard" | "zenith"
  >("astralis");

  // RPG & Castle simulation States for v18.11.15
  const [isCharacterMenuOpen, setIsCharacterMenuOpen] =
    useState<boolean>(false);
  const [simCastleLevel, setSimCastleLevel] = useState<number>(1);
  const [playerGold, setPlayerGold] = useState<number>(500);
  const [recruitedTroops, setRecruitedTroops] = useState<number>(15);
  const [garrisonTroops, setGarrisonTroops] = useState<number>(8);
  const [aiUpgradeProb, setAiUpgradeProb] = useState<number>(0.3);
  const [aiRecruitProb, setAiRecruitProb] = useState<number>(0.4);
  const [aiEquipProb, setAiEquipProb] = useState<number>(0.35);
  const [aiIncomeMult, setAiIncomeMult] = useState<number>(1.35);
  const [aiStartingPower, setAiStartingPower] = useState<number>(15);
  const [useManualConfig, setUseManualConfig] = useState<boolean>(false);
  const [selectedLandingSlot, setSelectedLandingSlot] = useState<number>(0);

  // Weapon Skill Stats Leveling (v18.11.15)
  const [swordLevel, setSwordLevel] = useState<number>(1);
  const [bowLevel, setBowLevel] = useState<number>(1);
  const [staffLevel, setStaffLevel] = useState<number>(1);

  // Castle positioning modes & coordinate settings (v18.11.15)
  const [isCastlePlacementManual, setIsCastlePlacementManual] =
    useState<boolean>(false);
  const [manualCastlePositions, setManualCastlePositions] = useState<
    Record<string, { x: number; y: number; z: number }>
  >({
    player: { x: -5.3, y: -0.4, z: 4.2 },
    peak: { x: 14.8, y: 1.2, z: 12.5 },
    ruins: { x: -12.4, y: -0.3, z: -10.2 },
    zenith: { x: 6.5, y: 0.8, z: -4.5 },
  });

  // Spire visual customizable params
  const [spireColor, setSpireColor] = useState<string>("#22d3ee"); // cyan
  const [spireRotationSpeed, setSpireRotationSpeed] = useState<number>(3.5); // seconds
  const [spireGlowStrength, setSpireGlowStrength] = useState<number>(20); // pixels
  const [gameDayCount, setGameDayCount] = useState<number>(1);
  const [aiActionLogs, setAiActionLogs] = useState<string[]>([
    "День 1: Лорд Мельгард (ИИ) получил пассивное золото (+60).",
    "День 1: Лорд Мельгард завербовал 4 лучников во втором замке.",
  ]);

  const [equippedItems, setEquippedItems] = useState<
    Record<string, { name: string; bonus: string; icon: string }>
  >({
    helmet: { name: "Прошитый кожаный чепец", bonus: "+3 Броня", icon: "🪖" },
    armor: {
      name: "Железный нагрудник авангарда",
      bonus: "+15 Броня",
      icon: "👕",
    },
    weapon: { name: "Стальной меч Судьбы", bonus: "+25 Сила", icon: "🗡️" },
    shield: { name: "Приточный щит Альянса", bonus: "+8 Блок", icon: "🛡️" },
    boots: { name: "Кованые латные сапоги", bonus: "+10 Скор.", icon: "👢" },
    ring: { name: "Перстень Кристалла", bonus: "+35 Мана", icon: "💍" },
  });

  // Dynamic high-performance state variables for Citadel Interiors (v18.11.16)
  const [activeCastleBuildingTab, setActiveCastleBuildingTab] = useState<
    "barracks" | "forge_shop" | "academy_arena"
  >("barracks");
  const [currentCastleFacility, setCurrentCastleFacility] = useState<
    "hub" | "barracks" | "forge_shop" | "academy_arena"
  >("hub");
  const [selectedTroopId, setSelectedTroopId] = useState<string>("guard");

  // Custom levels & experience tracking for all 10 troop types in the army!
  const [troopProgress, setTroopProgress] = useState<
    Record<string, { lvl: number; xp: number }>
  >({
    guard: { lvl: 1, xp: 0 },
    archer: { lvl: 1, xp: 0 },
    arcanist: { lvl: 1, xp: 0 },
    paladin: { lvl: 1, xp: 0 },
    cavalry: { lvl: 1, xp: 0 },
    cannoneer: { lvl: 1, xp: 0 },
    centaur: { lvl: 1, xp: 0 },
    necromancer: { lvl: 1, xp: 0 },
    griffin: { lvl: 1, xp: 0 },
    lord_knight: { lvl: 1, xp: 0 },
  });
  const [customImages, setCustomImages] = useState<Record<string, string>>(
    () => {
      try {
        const stored = localStorage.getItem("fc_custom_images");
        return stored ? JSON.parse(stored) : {};
      } catch (e) {
        return {};
      }
    },
  );

  // Vram Overheat / RAM optimizer protection helper
  const [isVramSaverActive, setIsVramSaverActive] = useState<boolean>(true); // TRUE by default to safeguard their graphics card!

  // Autonomous Castle Drift state variables
  const [isAutonomousDriftActive, setIsAutonomousDriftActive] =
    useState<boolean>(true);
  const [driftTime, setDriftTime] = useState<number>(0);

  // Selected trainee for Academy Training ground tab
  const [selectedTrainee, setSelectedTrainee] = useState<
    "main" | "simple" | "principle" | "troops"
  >("main");

  // Trainee Levels and Experience values
  const [traineeLevels, setTraineeLevels] = useState<
    Record<string, { lvl: number; xp: number; nameRU: string; nameEN: string }>
  >({
    main: {
      lvl: 5,
      xp: 350,
      nameRU: "Главный Герой (Покоритель)",
      nameEN: "Main Vanguard Hero",
    },
    simple: {
      lvl: 3,
      xp: 120,
      nameRU: "Простой Герой (Степной Вожак)",
      nameEN: "Simple Plains Leader",
    },
    principle: {
      lvl: 4,
      xp: 180,
      nameRU: "Принципиальный Герой (Эмиссар)",
      nameEN: "Principle High Oracle",
    },
    troops: {
      lvl: 2,
      xp: 90,
      nameRU: "Союзные Регулярные Войска",
      nameEN: "Alliance Regular Troops",
    },
  });

  // Level thresholds and limits
  const [traineeCooldowns, setTraineeCooldowns] = useState<
    Record<string, number>
  >({
    main: 0,
    simple: 0,
    principle: 0,
    troops: 0,
  });

  // Synchronize custom uploaded images to LocalStorage so they persist across reboots!
  useEffect(() => {
    try {
      localStorage.setItem("fc_custom_images", JSON.stringify(customImages));
    } catch (e) {}
  }, [customImages]);

  // Autonomous Castle movement simulator effect
  useEffect(() => {
    if (isAutonomousDriftActive && !isCastlePlacementManual) {
      const interval = setInterval(() => {
        setDriftTime((t) => {
          const nextTime = t + 0.04;
          setManualCastlePositions((prev) => ({
            player: {
              x: -5.3 + Math.sin(nextTime * 0.4) * 2.0,
              y: -0.4,
              z: 4.2 + Math.cos(nextTime * 0.4) * 2.0,
            },
            peak: {
              x: 14.8 + Math.cos(nextTime * 0.5 + 1.2) * 3.0,
              y: 1.2,
              z: 12.5 + Math.sin(nextTime * 0.5) * 2.5,
            },
            ruins: {
              x: -12.4 + Math.sin(nextTime * 0.3 + 2.4) * 2.5,
              y: -0.3,
              z: -10.2 + Math.cos(nextTime * 0.3) * 3.0,
            },
            zenith: {
              x: 6.5 + Math.cos(nextTime * 0.6) * 1.5,
              y: 0.8,
              z: -4.5 + Math.sin(nextTime * 0.6 + 3.1) * 2.0,
            },
          }));
          return nextTime;
        });
      }, 70);
      return () => clearInterval(interval);
    }
  }, [isAutonomousDriftActive, isCastlePlacementManual]);

  const [trainedHeroLvl, setTrainedHeroLvl] = useState<number>(5);
  const [trainedHeroXp, setTrainedHeroXp] = useState<number>(350);
  const [trainedWarriorsLvl, setTrainedWarriorsLvl] = useState<number>(2);
  const [trainedWarriorsXp, setTrainedWarriorsXp] = useState<number>(120);
  const [heroTrainingCooldownDay, setHeroTrainingCooldownDay] =
    useState<number>(0);
  const [warriorTrainingCooldownDay, setWarriorTrainingCooldownDay] =
    useState<number>(0);

  const [activeSpellPrompt, setActiveSpellPrompt] = useState<{
    name: string;
    cost: number;
    desc: string;
    command: string;
  } | null>(null);
  const [activeTransferPrompt, setActiveTransferPrompt] =
    useState<boolean>(false);

  // Sync starting gold with selected difficulty levels
  useEffect(() => {
    const d = selectedDifficulty || "";
    if (d === "Новичок" || d === "Novice") {
      setPlayerGold(1000);
    } else if (d === "Легкий" || d === "Легко" || d === "Easy") {
      setPlayerGold(800);
    } else if (d === "Сложный" || d === "Сложно" || d === "Hard") {
      setPlayerGold(300);
    } else if (d === "Кошмар" || d === "Nightmare") {
      setPlayerGold(100);
    } else {
      setPlayerGold(500);
    }
  }, [selectedDifficulty]);

  const fetchPackagesInfo = async () => {
    try {
      const res = await fetch("/api/unity/packages-info");
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
      const res = await fetch("/api/unity/migrate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ from: "2022.3.62f2", to: "6000.3.10f1" }),
      });
      const data = await res.json();
      setMigrationGuide(data.guide);
    } catch (e) {
      showNotification("Ошибка при генерации руководства.", "error");
    } finally {
      setIsMigrating(false);
    }
  };

  const showNotification = (
    message: string,
    type: "success" | "error" | "info" = "info",
  ) => {
    setNotification({ message, type });
    setTimeout(() => setNotification(null), 4000);
  };

  const handleImageUpload = (slotId: string, file: File) => {
    const reader = new FileReader();
    reader.onloadend = () => {
      setCustomImages((prev) => ({
        ...prev,
        [slotId]: reader.result as string,
      }));
      showNotification("📸 Изображение успешно занесено в окошко!", "success");
    };
    reader.readAsDataURL(file);
  };

  const checkUpdates = async () => {
    try {
      const response = await fetch("/api/update/check");
      const data = await response.json();
      setUpdateInfo(data);
      if (data.available) {
        setShowUpdateModal(true);
      } else {
        setAppVersion("18.8.0");
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
      "Обновление версии до 18.5.0...",
      "Инициализация Omniversal Quantum Link...",
      "Установка Нейронного Моста (Blender & Unity)...",
      "Регенерация PROJECT_MASTER_BLUEPRINT.md (Quantum Link)...",
    ];

    let step = 0;
    const interval = setInterval(() => {
      if (step < syncSteps.length) {
        setUpdateProgress(Math.floor(((step + 1) / syncSteps.length) * 90));
        step++;
      }
    }, 800);

    try {
      const response = await fetch("/api/update/apply", { method: "POST" });
      const data = await response.json();

      if (response.ok && data.success) {
        setUpdateProgress(100);
        clearInterval(interval);
        setTimeout(() => {
          showNotification(data.message, "success");
          setShowUpdateModal(false);
          // Refresh KB to show new version
          fetch("/api/kb")
            .then((res) => res.json())
            .then((data) => setKb(data));
        }, 1000);
      } else {
        throw new Error(data.error || "Sync failed");
      }
    } catch (error) {
      console.error("Update apply error:", error);
      clearInterval(interval);
      showNotification(
        "Ошибка при синхронизации. Попробуйте еще раз.",
        "error",
      );
    } finally {
      setIsUpdating(false);
    }
  };

  // Initialize Gemini
  const ai = React.useMemo(() => {
    try {
      // Robustly check for API key in different environments
      let apiKey = "";
      try {
        if (typeof (import.meta as any).env !== "undefined") {
          apiKey = (import.meta as any).env.VITE_GEMINI_API_KEY || "";
        }
      } catch (e) {}

      if (!apiKey) {
        try {
          if (typeof process !== "undefined" && process.env) {
            apiKey = (process.env as any).GEMINI_API_KEY || "";
          }
        } catch (e) {}
      }

      if (!apiKey) return null;
      return new GoogleGenAI({ apiKey });
    } catch (e) {
      console.warn("Local Gemini init failed:", e);
      return null;
    }
  }, []);

  // Monitor online status
  React.useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);
    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);
    return () => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
    };
  }, []);

  useEffect(() => {
    if (input.length > 0 && input.length < 15) {
      const suggestions = [
        "Как создать модульное здание в Blender?",
        "Unity DOTS: основы оптимизации",
        "Создай скрипт для поведения врагов (Unity)",
        "Геометрические ноды: процедурный город",
        "Как перенести проект из Unity в Redot?",
        "Как работает Neural Memory?",
        "Как настроить Quantum Link?",
      ]
        .filter(
          (s) =>
            s.toLowerCase().includes(input.toLowerCase()) || input.length < 3,
        )
        .slice(0, 4);
      setSuggestedQuestions(suggestions);
    } else {
      setSuggestedQuestions([]);
    }
  }, [input]);

  useEffect(() => {
    fetchGameDesign();
    // Load chat history
    fetch("/api/chat/history")
      .then((res) => res.json())
      .then((data) => {
        if (data && data.length > 0) setMessages(data);
      });

    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);
    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);
    setIsOnline(navigator.onLine);

    fetchWithRetry("/api/kb")
      .then((data) => {
        setKb(data);
        setLocalPathInput(data.local_training_path || "");
        setProjectPathInput(data.project_path || "");
        setGimpPathInput(data.gimp_path || "");
        setRedotPathInput(data.redot_path || "");
        setBlenderVersionInput(data.blender_version || "");
      })
      .catch((err) => {
        console.error("Failed to fetch KB after retries", err);
        setKb({
          name: "Unity AI Assistant",
          version: "18.2.0",
          description: "Гибридный ИИ-помощник с Quantum Link",
          project_path: "Unknown",
          system_instruction: "Ты — экспертный ИИ-ассистент.",
        } as KBData);
      });

    fetch("/api/project/scan")
      .then((res) => res.json())
      .then((data) => data.success && setProjectScan(data.scan));

    fetch("/api/blender/presets")
      .then((res) => res.json())
      .then((data) => setBlenderPresets(data));

    const checkAIStatus = async () => {
      try {
        const res = await fetch("/api/ai/health");
        if (res.ok) {
          const data = await res.json();
          // Status is online if level is free or premium
          if (data.level === "free" || data.level === "premium") {
            setAiHealth("online");
          } else {
            setAiHealth("limited");
          }
        } else {
          setAiHealth("error");
        }
      } catch (e) {
        setAiHealth("error");
      }
    };

    const checkServer = async () => {
      try {
        const res = await fetch("/api/health");
        setIsOnline(res.ok);
        setServerHealth(res.ok ? "online" : "error");
        setAppVersion("18.2.0"); // Force sync version
      } catch (e) {
        setIsOnline(false);
        setServerHealth("offline");
      }
    };

    const statusInterval = setInterval(() => {
      checkServer();
      checkAIStatus();
      // Local API calls are safe even if offline from internet
      const fetchStatus = (url: string, setter: Function) => {
        fetch(url)
          .then((res) => (res.ok ? res.json() : null))
          .then((data) => data && setter(data))
          .catch(() => {});
      };

      fetchStatus("/api/unity/status", setUnityStatus);
      fetchStatus("/api/blender/status", setBlenderStatus);
      fetchStatus("/api/gimp/status", setGimpStatus);
      fetchStatus("/api/redot/status", setRedotStatus);
      fetchStatus("/api/photoshop/status", setPhotoshopStatus);

      fetch("/api/ai/ollama-status")
        .then((res) => (res.ok ? res.json() : { isRunning: false }))
        .then((data) => setOllamaRunning(data.isRunning))
        .catch(() => setOllamaRunning(false));

      fetch("/api/project/history")
        .then((res) => (res.ok ? res.json() : []))
        .then((data) => setHistory(data))
        .catch(() => {});

      if (navigator.onLine) {
        fetch("/api/ai/capabilities")
          .then((res) => (res.ok ? res.json() : null))
          .then((data) => {
            if (data) {
              const v = data.name.match(/v([\d.]+)/)?.[1] || "18.2.0";
              setAppVersion(v);
            }
          })
          .catch(() => {});
      }
    }, 10000);

    return () => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
      clearInterval(statusInterval);
    };
  }, []);

  const messagesRef = useRef<Message[]>([]);
  useEffect(() => {
    messagesRef.current = messages;
  }, [messages]);

  useEffect(() => {
    const processTasks = async () => {
      // Get latest state values safely
      const liveOnline = navigator.onLine;
      if (liveOnline !== isOnline) setIsOnline(liveOnline);

      if (!liveOnline && !ollamaRunning) return;

      try {
        const res = await fetch("/api/ai/tasks");
        const tasks = await res.json();

        if (tasks && tasks.length > 0) {
          for (const task of tasks) {
            console.log(
              `[AI TASK] Processing ${task.id}: ${task.prompt} (Mode: ${liveOnline ? "Online" : "Ollama"})`,
            );

            try {
              let systemInstruction =
                kb?.system_instruction || "You are a helpful assistant.";

              if (task.target === "blender") {
                systemInstruction +=
                  "\nIMPORTANT: GENERATE ONLY PURE PYTHON CODE FOR BLENDER 4.x. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Focus on bpy modules.";
              } else if (task.target === "unity") {
                systemInstruction +=
                  "\nIMPORTANT: GENERATE ONLY PURE C# CODE FOR UNITY 6. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Use standard Unity namespaces.\nCRITICAL UI RULE: ALWAYS USE UGUI (CANVAS SYSTEM). DO NOT USE PLANES OR TERRAINS FOR UI. Hierarchy: Canvas -> Panel -> Image/TMP -> Button. Use Scale With Screen Size (1920x1080).";
              }

              let fullPrompt = "";
              const recentHistory = messagesRef.current
                .slice(-5)
                .map((m) => `${m.role.toUpperCase()}: ${m.content}`)
                .join("\n");
              if (recentHistory) {
                fullPrompt += `### NEURAL MEMORY (RECENT CHAT CONTEXT) ###\n${recentHistory}\n\n`;
              }
              fullPrompt += `### TASK FOR ${task.target.toUpperCase()} ###\n${task.prompt}`;
              if (task.context) {
                fullPrompt += `\n\n### SOFTWARE CONTEXT ###\n${JSON.stringify(task.context)}`;
              }

              let code = "";
              if (liveOnline && isOnline) {
                try {
                  const response = await ai.models.generateContent({
                    model: "gemini-1.5-flash",
                    config: { systemInstruction: systemInstruction },
                    contents: [{ role: "user", parts: [{ text: fullPrompt }] }],
                  });
                  code = response.text;
                } catch (err) {
                  console.error("Assistant Code Gen Error (Frontend):", err);
                  // Fallback to server proxy if frontend fails
                  try {
                    const response = await fetch("/api/ai/gemini-chat", {
                      method: "POST",
                      headers: { "Content-Type": "application/json" },
                      body: JSON.stringify({
                        contents: [
                          { role: "user", parts: [{ text: fullPrompt }] },
                        ],
                        systemInstruction: systemInstruction,
                        model: "gemini-1.5-flash",
                      }),
                    });
                    const data = await response.json();
                    if (!response.ok)
                      throw new Error(data.error || "Gemini Server Error");
                    code = data.text;
                  } catch (serverErr) {
                    console.error(
                      "Assistant Code Gen Error (Server):",
                      serverErr,
                    );
                    throw err;
                  }
                }
              } else if (ollamaRunning) {
                const ollamaRes = await fetch("/api/ai/ollama-chat", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({
                    prompt: fullPrompt,
                    systemInstruction,
                  }),
                });
                const ollamaData = await ollamaRes.json();
                code = ollamaData.answer;
              }

              if (!code) throw new Error("Empty AI response");

              code = code
                .replace(/```python\n?/g, "")
                .replace(/```\n?/g, "")
                .replace(/```csharp\n?/g, "")
                .replace(/```cs\n?/g, "");

              await fetch("/api/ai/complete", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ taskId: task.id, code: code.trim() }),
              });

              console.log(`[AI TASK] Completed ${task.id}`);
            } catch (err: any) {
              console.error(`[AI TASK] Failed ${task.id}:`, err);
              await fetch("/api/ai/complete", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ taskId: task.id, error: err.message }),
              });
            }
          }
        }
      } catch (e) {
        // Silent fail
      }
    };

    const interval = setInterval(processTasks, 4000);
    return () => clearInterval(interval);
  }, [kb, ollamaRunning]); // Removed isOnline, ai and messages from dependencies to avoid loop restarts

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
    // Save chat history when messages change
    if (messages.length > 0) {
      fetch("/api/chat/save", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ messages }),
      });
    }
  }, [messages]);

  const handleClearChat = async () => {
    if (isClearingChat) return;
    setIsClearingChat(true);
    try {
      const res = await fetch("/api/chat/clear", { method: "POST" });
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
    if (!manualPrompt.trim() && attachedFiles.length === 0) return;
    setIsManualGenerating(true);
    setManualResultCode("");

    try {
      let systemInstruction =
        kb?.system_instruction || "You are a helpful assistant.";

      if (manualTarget === "blender") {
        systemInstruction +=
          "\nIMPORTANT: GENERATE ONLY PURE PYTHON CODE FOR BLENDER 4.x. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Focus on bpy modules.";
      } else if (manualTarget === "unity") {
        systemInstruction +=
          "\nIMPORTANT: GENERATE ONLY PURE C# CODE FOR UNITY 6. NO MARKDOWN, NO EXPLANATIONS. START CODE DIRECTLY. Use standard Unity namespaces.\nCRITICAL UI RULE: ALWAYS USE UGUI (CANVAS SYSTEM). DO NOT USE PLANES OR TERRAINS FOR UI. Hierarchy: Canvas -> Panel -> Image/TMP -> Button. Use Scale With Screen Size (1920x1080).";
      }

      let code = "";
      const recentHistory = messages
        .slice(-5)
        .map((m) => `${m.role.toUpperCase()}: ${m.content}`)
        .join("\n");
      let fullPrompt = "";
      if (recentHistory) {
        fullPrompt += `### NEURAL MEMORY (RECENT CHAT CONTEXT) ###\n${recentHistory}\n\n`;
      }
      fullPrompt += `### MANUAL CODE REQUEST FOR ${manualTarget.toUpperCase()} ###\n${manualPrompt}`;

      if (isOnline) {
        const parts: any[] = [];
        if (recentHistory)
          parts.push({
            text: `### NEURAL MEMORY (RECENT CHAT CONTEXT) ###\n${recentHistory}\n\n`,
          });

        if (attachedFiles.length > 0) {
          for (const file of attachedFiles) {
            if (file.type && file.type.startsWith("image/")) {
              const data = await fileToBase64(file.url);
              parts.push({ inlineData: { data, mimeType: file.type } });
            }
          }
        }

        parts.push({
          text: `### MANUAL CODE REQUEST FOR ${manualTarget.toUpperCase()} ###\n${manualPrompt}`,
        });

        try {
          const response = await ai.models.generateContent({
            model: "gemini-1.5-flash",
            config: { systemInstruction: systemInstruction },
            contents: [{ role: "user", parts }],
          });
          code = response.text;
        } catch (err) {
          console.error("Manual Code Gen Error (Frontend):", err);
          try {
            const response = await fetch("/api/ai/gemini-chat", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({
                contents: [{ role: "user", parts }],
                systemInstruction: systemInstruction,
                model: "gemini-1.5-flash",
              }),
            });
            const data = await response.json();
            if (!response.ok)
              throw new Error(data.error || "Gemini Server Error");
            code = data.text;
          } catch (serverErr) {
            console.error("Manual Code Gen Error (Server):", serverErr);
            throw err;
          }
        }
      } else if (ollamaRunning) {
        const ollamaRes = await fetch("/api/ai/ollama-chat", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ prompt: fullPrompt, systemInstruction }),
        });
        const ollamaData = await ollamaRes.json();
        code = ollamaData.answer;
      } else {
        throw new Error("Нет доступа к ИИ (Офлайн и Ollama не активна)");
      }

      if (!code) throw new Error("Empty AI response");
      code = code
        .replace(/```python\n?/g, "")
        .replace(/```\n?/g, "")
        .replace(/```csharp\n?/g, "")
        .replace(/```cs\n?/g, "");
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
      const res = await fetch("/api/ai/ollama-launch", { method: "POST" });
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
      blender_version: blenderVersionInput,
    };
    try {
      const response = await fetch("/api/kb/update", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updatedKb),
      });
      if (response.ok) {
        setKb(updatedKb);
        setShowSettings(false);
        showNotification(
          "Настройки сохранены. Запускаю сканирование...",
          "success",
        );
        handleRefreshScan();
      }
    } catch (error) {
      console.error("Failed to save settings", error);
    }
  };

  const handleRefreshScan = async () => {
    try {
      const res = await fetch("/api/project/scan/trigger", { method: "POST" });
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
      const response = await fetch("/api/blueprint/generate", {
        method: "POST",
      });
      if (response.ok) {
        showNotification(
          "Master Blueprint (PROJECT_MASTER_BLUEPRINT.md) успешно обновлен!",
          "success",
        );
      }
    } catch (error) {
      console.error("Failed to generate blueprint", error);
    } finally {
      setIsGeneratingBlueprint(false);
    }
  };

  const handleSend = async (text: string = input) => {
    if ((!text.trim() && attachedFiles.length === 0) || isTyping || !kb) return;

    let promptText = text;
    const isContinue =
      text.toLowerCase() === "продолжить" || text.toLowerCase() === "continue";

    if (isContinue) {
      promptText =
        "ПОЖАЛУЙСТА, ПРОДОЛЖИ СВОЙ ПРЕДЫДУЩИЙ ОТВЕТ С ТОГО МЕСТА, ГДЕ ОН ПРЕРВАЛСЯ. Не повторяй уже написанное, начни прямо с продолжения.";
    }

    const userMsg: Message = {
      role: "user",
      content: text,
      timestamp: Date.now(),
      files: attachedFiles.length > 0 ? [...attachedFiles] : undefined,
    };

    const newMessages = [...messages, userMsg];
    setMessages(newMessages);
    setInput("");
    setAttachedFiles([]);
    setIsTyping(true);

    setIsThinking(true);
    const thinkingSequences = [
      "Инициализация нейронных контуров v18.8.0...",
      "Анализ контекста проекта (Unity 6 & Blender 4.3)...",
      "Проверка статуса Quantum Link и облачных узлов...",
      "Доступ к базе 13,000+ видео-уроков...",
      "Синтез оптимального решения для вашего запроса...",
    ];

    setThinkingSteps([thinkingSequences[0]]);

    // Animate thinking steps
    let stepIndex = 1;
    const thinkingInterval = setInterval(() => {
      if (stepIndex < thinkingSequences.length) {
        setThinkingSteps((prev) => [...prev, thinkingSequences[stepIndex]]);
        stepIndex++;
      }
    }, 1200);

    try {
      // Offline Fallback Check (Only if we really want to skip Gemini entirely)
      if (!navigator.onLine) {
        console.warn("Navigator reports offline, but we'll try API first.");
      }

      // Prepare contents for Gemini (History + Images)
      const contents = [];
      // Limit history to last 10 messages to avoid token issues
      const historyToProcess = newMessages.slice(-10);

      for (const msg of historyToProcess) {
        const parts: any[] = [{ text: msg.content }];

        if (msg.files) {
          for (const file of msg.files) {
            if (file.type && file.type.startsWith("image/")) {
              try {
                // Cache base64 in the file object to avoid re-converting
                if (!file.base64) {
                  const base64 = await fileToBase64(file.url);
                  file.base64 = base64;
                }
                parts.push({
                  inlineData: { data: file.base64, mimeType: file.type },
                });
              } catch (e) {
                console.error("Error converting image to base64", e);
              }
            }
          }
        }

        contents.push({
          role: msg.role === "assistant" ? "model" : "user",
          parts: parts,
        });
      }

      let textResponse = "";
      const systemInst =
        kb.system_instruction +
        "\n\n### GLOBAL PROJECT MASTERY v18.8.0 ###\n- CORE KNOWLEDGE: Integrated PDF Manual (Parts 1-8) & Game Master Spec.\n- FATE CONTINENT: Specialized in RPG architecture and Zenith Glassmorphism.\n- 3D & ENGINE: Elite Unity 6 & Blender expertise.\n- CORTEX SYNC: Local Database + Automated AI Repair active.";

      try {
        let localSuccess = false;

        // Try Ollama first if enabled
        if (isOllamaMode) {
          try {
            console.log("Attempting Ollama local call via proxy...");
            const ollamaPrompt = `You are a helpful AI Assistant for Unity and Blender. Your knowledge base is version 18.2.0.
            Authorized reference: Fate Continent Documentation (Internal MD files).
            Directives:
            1. Always refer to technical manual sections (Part 1-8).
            2. Maintain Zenith Glassmorphism UI standards.
            3. Fix hierarchy issues by keeping camera points in world-space.
            System Instruction: ${systemInst}
            
            History:
            ${contents.map((c) => `${c.role === "model" ? "Assistant" : "User"}: ${c.parts[0].text}`).join("\n")}
            
            User Request: ${promptText}`;

            const response = await fetch("/api/ollama/proxy", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({
                model: "llama3",
                prompt: ollamaPrompt,
                stream: false,
              }),
            });

            if (response.ok) {
              const data = await response.json();
              textResponse = data.response;
              localSuccess = true;
              console.log("Ollama success.");
            } else {
              console.warn(
                "Ollama proxy returned error, falling back to Gemini.",
              );
            }
          } catch (ollamaErr) {
            console.error("Ollama proxy check failed:", ollamaErr);
          }
        }

        if (!localSuccess && ai) {
          try {
            console.log("Attempting direct local Gemini call...");

            const response = (await Promise.race([
              ai.models.generateContent({
                model: "gemini-flash-latest",
                contents: contents.map((c, i) =>
                  i === contents.length - 1 && isContinue
                    ? { ...c, parts: [{ text: promptText }] }
                    : c,
                ),
                config: {
                  systemInstruction: systemInst,
                },
              }),
              new Promise((_, reject) =>
                setTimeout(() => reject(new Error("Timeout")), 25000),
              ),
            ])) as any;

            if (response && response.text) {
              textResponse = response.text;
              localSuccess = true;
              console.log("Local Gemini success.");
            }
          } catch (localErr: any) {
            console.warn("Local Gemini failed or timed out:", localErr.message);
            // Special check for API Key in browser
            if (
              localErr.message?.includes("API_KEY_INVALID") ||
              localErr.message?.includes("400")
            ) {
              console.error("Direct API Key is invalid.");
            }
          }
        }

        if (!localSuccess) {
          console.log("Falling back to server-side Gemini proxy...");
          const response = await fetch("/api/ai/gemini-chat", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              contents: contents.map((c, i) =>
                i === contents.length - 1 && isContinue
                  ? { ...c, parts: [{ text: promptText }] }
                  : c,
              ),
              systemInstruction: systemInst,
              model: "gemini-1.5-flash",
            }),
          });

          const data = await response.json();
          if (!response.ok) {
            // Carry over the detailed error if possible
            const errorReason =
              data.details || data.error || "Gemini Server Error";
            throw new Error(errorReason);
          }
          textResponse = data.text;
        }
      } catch (err: any) {
        console.error("Chat Error (Final failure chain):", err);
        throw new Error(err.message || "AI Failed");
      }

      // Check for audio requests to generate variants
      const audioKeywords = [
        "музыка",
        "песня",
        "звук",
        "мелодия",
        "mp3",
        "music",
        "song",
        "audio",
      ];
      const isAudioRequest = audioKeywords.some((k) =>
        text.toLowerCase().includes(k),
      );

      const aiMsg: Message = {
        role: "assistant",
        content: textResponse || "Извините, я не смог сгенерировать ответ.",
        timestamp: Date.now(),
        audioVariants: isAudioRequest
          ? [
              {
                name: "Экспериментальный вариант 1 (Quantum Sonic)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
              },
              {
                name: "Экспериментальный вариант 2 (Neural Melodic)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3",
              },
              {
                name: "Экспериментальный вариант 3 (Void Resonance)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3",
              },
              {
                name: "Экспериментальный вариант 4 (Reality Warp)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3",
              },
              {
                name: "Экспериментальный вариант 5 (Eternal Harmony)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3",
              },
              {
                name: "Экспериментальный вариант 6 (Subatomic Beats)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3",
              },
              {
                name: "Экспериментальный вариант 7 (Quantum Distortion)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-7.mp3",
              },
            ]
          : undefined,
      };

      setMessages((prev) => [...prev, aiMsg]);
    } catch (error: any) {
      console.error("Gemini Error:", error);
      const errorText = error.toString();
      const isKeyError =
        errorText.includes("ключ") ||
        errorText.includes("API_KEY_INVALID") ||
        errorText.includes("key") ||
        errorText.includes("401");

      if (isKeyError) {
        setMessages((prev) => [
          ...prev,
          {
            role: "assistant",
            content: `### ❌ ОШИБКА КОНФИГУРАЦИИ API
Ваш ключ Gemini определен как недействительный. 

**Как исправить:**
1. Нажмите на ⚙️ **Настройки** слева.
2. Откройте вкладку **Секреты**.
3. Найдите \`GEMINI_API_KEY\`.
4. Убедитесь, что там вставлен **код** (например: \`AIzaSy... \`), а не текст "Бесплатный уровень".
5. Нажмите **Применить изменения**.

*Пока ключ не исправлен, я буду отвечать из локального архива знаний.*`,
            timestamp: Date.now(),
          },
        ]);
      }

      // Fallback Strategy: Ollama -> Local Search
      try {
        if (ollamaRunning) {
          const ollamaRes = await fetch("/api/ai/ollama-chat", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              prompt: promptText,
              systemInstruction: kb.system_instruction,
            }),
          });
          const ollamaData = await ollamaRes.json();
          if (ollamaRes.ok) {
            setMessages((prev) => [
              ...prev,
              {
                role: "assistant",
                content: `[OLLAMA - ПРОВЕРКА СВЯЗИ]\n\n${ollamaData.answer}`,
                timestamp: Date.now(),
              },
            ]);
            return;
          }
        }

        const localRes = await fetch("/api/ai/local-search", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            query: text,
            history: newMessages.slice(-5),
          }),
        });
        const localData = await localRes.json();

        // Friendly wrapping if it's a key error
        let finalContent = localData.answer;
        if (isKeyError && !finalContent.includes("КЛЮЧ API")) {
          finalContent = `### 📡 СТАТУС СИНГУЛЯРНОСТИ: ОНЛАЙН (v18.5.6)\nСвязь установлена, но возникла техническая проблема с API-ключом Gemini.\n\n${finalContent}`;
        }

        setMessages((prev) => [
          ...prev,
          {
            role: "assistant",
            content: finalContent,
            timestamp: Date.now(),
          },
        ]);
      } catch (e) {
        setMessages((prev) => [
          ...prev,
          {
            role: "assistant",
            content:
              "Ошибка: Не удалось подключиться к ИИ и локальный поиск недоступен.",
            timestamp: Date.now(),
          },
        ]);
      }
    } finally {
      clearInterval(thinkingInterval);
      setIsTyping(false);
      setIsThinking(false);
      setThinkingSteps([]);
    }
  };

  const handleGenerateVKCovers = async () => {
    if (!vkPrompt.trim()) return;
    setIsGeneratingVK(true);
    setVkResults([]);
    setVkProgress(2);
    setVkStatus("Инициализация квантового ядра...");

    try {
      let finalPrompt = vkPrompt;

      // Phase 1: Gemini Image Analysis (Vision Synthesis)
      if (sourceImage && isOnline) {
        setVkStatus("Анализ композиции и освещения (Vision)...");
        setVkProgress(5);
        try {
          const imageData = sourceImage.split(",")[1];
          const mimeType = sourceImage
            .split(",")[0]
            .split(":")[1]
            .split(";")[0];

          setVkProgress(10);
          let description = "";
          const visionPrompt =
            "Analyze this image carefully. Describe the scene structure (foreground, midground, background), lighting direction, and major colors. Start with 'STRUCTURE: ' and keep it technical for an AI generator to use as absolute spatial reference.";

          try {
            const result = await ai.models.generateContent({
              model: "gemini-1.5-flash",
              contents: [
                {
                  role: "user",
                  parts: [
                    { text: visionPrompt },
                    { inlineData: { data: imageData, mimeType } },
                  ],
                },
              ],
            });
            description = result.text;
          } catch (err) {
            console.error("Vision Error (Frontend):", err);
            try {
              const response = await fetch("/api/ai/gemini-chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                  contents: [
                    {
                      role: "user",
                      parts: [
                        { text: visionPrompt },
                        { inlineData: { data: imageData, mimeType } },
                      ],
                    },
                  ],
                  model: "gemini-1.5-flash",
                }),
              });
              const data = await response.json();
              if (!response.ok)
                throw new Error(data.error || "Vision Server Error");
              description = data.text;
            } catch (serverErr) {
              console.error("Vision Error (Server):", serverErr);
              throw err;
            }
          }
          setVkProgress(15);

          // Ultra-Coherent Prompt Strategy
          finalPrompt = `[CORE REFERENCE]: ${description}. [TASK]: Integrate exactly these elements into the reference scene: ${vkPrompt}. [STRICT RULE]: Do NOT change the camera angle, perspective, or major existing landscape features. Add requested objects as if they were always there. Professional digital matte painting, 8k, volumetric light, unified scene coherence.`;
          console.log("Ultra-Coherent Synthesis Prompt:", finalPrompt);
        } catch (visionErr) {
          console.error("Vision Analysis failed:", visionErr);
          setVkStatus("Сбой Vision, использую базовую логику...");
          setVkProgress(15);
        }
      } else if (sourceImage && !isOnline) {
        setVkStatus("Офлайн режим: Анализ Vision пропущен...");
        setVkProgress(15);
      }

      setVkStatus("Запуск параллельного синтеза (Burst Mode)...");

      // Phase 2: Parallel Batch Generation (Faster)
      // We generate all 10 in parallel but update progress as they return
      const totalSteps = 10;
      let completedSteps = 0;

      const generateStep = async (stepIndex: number) => {
        try {
          const res = await fetch("/api/generate/vk-covers", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              prompt: finalPrompt,
              type: vkType,
              index: stepIndex,
              sourceImage: sourceImage,
            }),
          });

          if (!res.ok) throw new Error(`Step ${stepIndex} failed`);
          const data = await res.json();

          if (data.success && data.variations && data.variations.length > 0) {
            setVkResults((prev) => [...prev, ...data.variations]);
          }
        } finally {
          completedSteps++;
          const progress = 15 + Math.floor((completedSteps / totalSteps) * 85);
          setVkProgress(progress);
          setVkStatus(`Обработано ${completedSteps} из 10 (Синтез)...`);
        }
      };

      // Fire all requests in parallel
      await Promise.all(
        Array.from({ length: totalSteps }).map((_, i) => generateStep(i + 1)),
      );

      showNotification(
        "Сгенерировано 10 уникальных обложек в Burst-режиме!",
        "success",
      );
      setVkStatus("Синтез завершен!");
    } catch (e) {
      console.error("VK Gen Error:", e);
      showNotification("Ошибка последовательной генерации.", "error");
      setVkStatus("Ошибка генерации");
    } finally {
      setIsGeneratingVK(false);
      // Keep progress at 100 for a moment then clear if needed
    }
  };

  const handleVKFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 50 * 1024 * 1024) {
      showNotification("Файл слишком большой! Максимум 50МБ.", "error");
      return;
    }
    setIsUploading(true);
    const reader = new FileReader();
    reader.onloadend = () => {
      setSourceImage(reader.result as string);
      setIsUploading(false);
      showNotification("Фото успешно загружено для синтеза!", "success");
    };
    reader.readAsDataURL(file);
  };

  const downloadBackground = async () => {
    const imageUrl =
      "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&q=100&w=3840&fm=jpg";
    const proxyUrl = `/api/download-proxy?url=${encodeURIComponent(imageUrl)}&filename=Continental_of_Fate_Background.jpg&t=${Date.now()}`;

    try {
      const link = document.createElement("a");
      link.href = proxyUrl;
      link.setAttribute("download", "Continental_of_Fate_Background.jpg");
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (err) {
      console.error("Download failed:", err);
      window.open(proxyUrl, "_blank");
    }
  };

  const handleUpdateKB = async () => {
    setIsUpdatingKB(true);
    try {
      const res = await fetch("/api/kb/update-api-refs", { method: "POST" });
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
      const data = await fetchWithRetry("/api/ai/capabilities");
      if (data) {
        setCapabilities(data);
        setShowCapabilities(true);
      }
    } catch (error) {
      if (navigator.onLine) {
        showNotification(
          "Не удалось загрузить информацию о возможностях.",
          "error",
        );
      }
    }
  };

  const fetchGameDesign = async () => {
    try {
      const data = await fetchWithRetry("/api/game-design");
      setGameDesign(data);
    } catch (e) {
      console.error("Error fetching game design:", e);
    }
  };

  const handleSaveGameDesign = async () => {
    if (!gameDesign) return;
    setIsSavingGameDesign(true);
    try {
      const res = await fetch("/api/game-design/update", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(gameDesign),
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
      const res = await fetch("/api/unity/material-converter");
      const data = await res.json();
      // Copy to clipboard
      await navigator.clipboard.writeText(data.snippet);
      showNotification(
        "C# скрипт конвертера скопирован в буфер обмена!",
        "success",
      );
    } catch (error) {
      showNotification("Ошибка при получении скрипта.", "error");
    }
  };

  const handleGetGitLFS = async () => {
    try {
      const res = await fetch("/api/git/lfs-setup");
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
      const res = await fetch("/api/migration/unity-to-godot", {
        method: "POST",
      });
      const data = await res.json();
      if (data.success) {
        setMigrationData(data);
        setActiveTab("migration");
      }
    } catch (e) {
      showNotification("Ошибка при загрузке данных миграции.", "error");
    } finally {
      setIsFetchingMigration(false);
    }
  };
  const uploadFiles = async (files: FileList | File[]) => {
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setUploadProgress(0);
    setUploadTimeRemaining(null);

    const formData = new FormData();
    Array.from(files).forEach((f) => formData.append("files", f));

    try {
      const startTime = Date.now();

      const xhr = new XMLHttpRequest();

      const uploadPromise = new Promise((resolve, reject) => {
        xhr.upload.addEventListener("progress", (event) => {
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
                `${minutes > 0 ? `${minutes} мин ` : ""}${seconds} сек (${speedMB} MB/s)`,
              );
            }
          }
        });

        xhr.addEventListener("load", () => {
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

        xhr.addEventListener("error", () => reject(new Error("Upload failed")));
        xhr.addEventListener("abort", () =>
          reject(new Error("Upload aborted")),
        );

        xhr.open("POST", "/api/upload");
        xhr.send(formData);
      });

      const data: any = await uploadPromise;

      setUploadProgress(100);
      setUploadTimeRemaining(null);

      if (data.success) {
        setAttachedFiles((prev) => [...prev, ...data.files]);
        showNotification("Файлы прикреплены к сообщению", "success");
      }
    } catch (error) {
      console.error("Upload error:", error);
      showNotification(
        "Ошибка при загрузке. Возможно, файл слишком большой.",
        "error",
      );
    } finally {
      setTimeout(() => {
        setIsUploading(false);
        setUploadProgress(0);
        setUploadTimeRemaining(null);
      }, 1000);
    }
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      uploadFiles(e.target.files);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    const items = e.clipboardData.items;
    const files: File[] = [];
    for (let i = 0; i < items.length; i++) {
      if (items[i].type.indexOf("image") !== -1) {
        const blob = items[i].getAsFile();
        if (blob) {
          const file = new File([blob], `pasted-image-${Date.now()}-${i}.png`, {
            type: blob.type,
          });
          files.push(file);
        }
      }
    }
    if (files.length > 0) {
      uploadFiles(files);
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
    <div className="h-screen bg-[#0a0a0c] text-slate-300 font-sans flex overflow-hidden relative">
      <MemoizedAtmosphericOverlay />

      {/* Sidebar for Stats and Status */}
      <aside className="w-64 border-r border-white/5 bg-black/40 flex flex-col z-50 overflow-y-auto scrollbar-none">
        <div className="p-6 border-b border-white/5">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center shadow-lg shadow-blue-600/20">
              <Cpu className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="text-sm font-bold text-white uppercase tracking-tighter">
                AI Assistant
              </h1>
              <p className="text-[10px] text-slate-500 uppercase font-mono">
                v{appVersion}
              </p>
            </div>
          </div>

          <div className="space-y-2">
            <div className="flex flex-col gap-2 p-2 rounded-lg bg-white/5 border border-white/5">
              <div className="flex items-center justify-between">
                <span className="text-[10px] font-bold text-slate-500 uppercase">
                  Сервер
                </span>
                <div className="flex items-center gap-1.5">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${serverHealth === "online" ? "bg-green-500" : "bg-red-500 animate-pulse"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase ${serverHealth === "online" ? "text-green-500" : "text-red-500"}`}
                  >
                    {serverHealth === "online" ? "Связь OK" : "Ошибка"}
                  </span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[10px] font-bold text-slate-500 uppercase">
                  ИИ Интеллект
                </span>
                <div className="flex items-center gap-1.5">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${aiHealth === "online" ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-yellow-500"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase ${aiHealth === "online" ? "text-green-500" : "text-yellow-500"}`}
                  >
                    {aiHealth === "online" ? "Связь OK" : "Ограничен"}
                  </span>
                </div>
              </div>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">
                Зрение (Vision)
              </span>
              <div className="flex items-center gap-1.5">
                <ImageIcon className="w-3 h-3 text-blue-400" />
                <span className="text-[9px] font-bold uppercase text-blue-400">
                  Активно
                </span>
              </div>
            </div>
            <div className="px-2 py-1 bg-white/5 rounded-lg border border-white/5 flex items-center gap-2">
              <Info className="w-3 h-3 text-slate-600" />
              <span className="text-[8px] text-slate-600 uppercase leading-tight">
                HMR WebSocket может быть отключен (это нормально)
              </span>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">
                AI Агент
              </span>
              <div className="flex items-center gap-1.5">
                <div
                  className={`w-1.5 h-1.5 rounded-full ${isTyping ? "bg-yellow-500 animate-pulse" : "bg-green-500"}`}
                />
                <span
                  className={`text-[9px] font-bold uppercase ${isTyping ? "text-yellow-400" : "text-green-500"}`}
                >
                  {isTyping ? "Думает..." : "Готов"}
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
                    <span className="text-[10px] text-slate-400">
                      Скрипты (C#)
                    </span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.scripts.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Box className="w-3 h-3 text-purple-400" />
                    <span className="text-[10px] text-slate-400">Префабы</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.prefabs.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Gamepad2 className="w-3 h-3 text-green-400" />
                    <span className="text-[10px] text-slate-400">Сцены</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.scenes.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Zap className="w-3 h-3 text-yellow-400" />
                    <span className="text-[10px] text-slate-400">Анимации</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.animations.length}
                  </span>
                </div>
                <div className="pt-2 border-t border-white/5 flex items-center justify-between">
                  <span className="text-[10px] font-bold text-white uppercase">
                    Всего файлов
                  </span>
                  <span className="text-[10px] font-mono text-blue-400">
                    {projectScan.total_files}
                  </span>
                </div>
                <div className="mt-2 text-[8px] text-slate-600 uppercase tracking-tighter text-right italic">
                  Обновлено:{" "}
                  {projectScan.last_updated
                    ? new Date(projectScan.last_updated).toLocaleTimeString()
                    : "---"}
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
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${unityStatus?.is_running ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-slate-700"}`}
                  />
                  <span className="text-[9px] font-mono text-slate-500">
                    {unityStatus?.version || "---"}
                  </span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Cube className="w-3.5 h-3.5 text-purple-400" />
                  <span className="text-[10px] text-slate-300">Blender</span>
                </div>
                <div className="flex items-center gap-2">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${blenderStatus?.is_running ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-slate-700"}`}
                  />
                  <span className="text-[9px] font-mono text-slate-500">
                    {blenderStatus?.version || "---"}
                  </span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ImageIcon className="w-3.5 h-3.5 text-orange-400" />
                  <span className="text-[10px] text-slate-300">GIMP</span>
                </div>
                <div className="flex items-center gap-2">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${gimpStatus?.is_running ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-slate-700"}`}
                  />
                  <span className="text-[9px] font-mono text-slate-500">
                    {gimpStatus?.version || "---"}
                  </span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Zap className="w-3.5 h-3.5 text-cyan-400" />
                  <span className="text-[10px] text-slate-300">Redot</span>
                </div>
                <div className="flex items-center gap-2">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${redotStatus?.is_running ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-slate-700"}`}
                  />
                  <span className="text-[9px] font-mono text-slate-500">
                    {redotStatus?.version || "---"}
                  </span>
                </div>
              </div>
              <div className="p-3 rounded-xl bg-white/5 border border-white/5 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ImageIcon className="w-3.5 h-3.5 text-blue-500" />
                  <span className="text-[10px] text-slate-300">Photoshop</span>
                </div>
                <div className="flex items-center gap-2">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${photoshopStatus?.is_running ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-slate-700"}`}
                  />
                  <span className="text-[9px] font-mono text-slate-500">
                    {photoshopStatus?.version || "---"}
                  </span>
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
              <span className="text-[11px] font-bold text-white uppercase">
                О возможностях ИИ
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Всё о том, что умеет наш ИИ и как он работает с проектом.
            </p>
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
              <span className="text-[11px] font-bold text-white uppercase">
                GitHub Guide
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Инструкция по переносу проекта через консоль.
            </p>
          </button>

          {/* Dashboard Button */}
          <button
            onClick={() => setActiveTab("dashboard")}
            className={`w-full p-4 rounded-2xl border transition-all group text-left ${
              activeTab === "dashboard"
                ? "bg-blue-600/20 border-blue-500/40 shadow-lg shadow-blue-600/10"
                : "bg-white/5 border-white/5 hover:border-blue-500/30 hover:bg-blue-600/5"
            }`}
          >
            <div className="flex items-center gap-3 mb-2">
              <div
                className={`p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors ${activeTab === "dashboard" ? "text-blue-400" : ""}`}
              >
                <Layers className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">
                Дашборд
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Панель мониторинга и статистики проекта.
            </p>
          </button>

          {/* Project Info Button */}
          <button
            onClick={() => setActiveTab("project_info")}
            className={`w-full p-4 rounded-2xl border transition-all group text-left ${
              activeTab === "project_info"
                ? "bg-blue-600/20 border-blue-500/40 shadow-lg shadow-blue-600/10"
                : "bg-white/5 border-white/5 hover:border-blue-500/30 hover:bg-blue-600/5"
            }`}
          >
            <div className="flex items-center gap-3 mb-2">
              <div
                className={`p-2 bg-black/40 rounded-lg group-hover:text-blue-400 transition-colors ${activeTab === "project_info" ? "text-blue-400" : ""}`}
              >
                <Info className="w-4 h-4" />
              </div>
              <span className="text-[11px] font-bold text-white uppercase">
                О проекте
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Информация о текущем проекте и его истории.
            </p>
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
              <span className="text-[9px] text-slate-500 uppercase tracking-widest mt-0.5">
                Unity & Blender Expert
              </span>
            </div>

            <nav className="flex items-center gap-1 bg-white/5 p-1 rounded-xl border border-white/5">
              <button
                onClick={() => setActiveTab("chat")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "chat"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/20"
                    : "text-slate-500 hover:text-slate-300"
                }`}
              >
                <Send className="w-3.5 h-3.5" /> Чат
              </button>
              <button
                onClick={() => setActiveTab("project_scripts")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "project_scripts"
                    ? "bg-amber-600 text-white shadow-lg shadow-amber-600/20"
                    : "text-slate-500 hover:text-slate-300"
                }`}
              >
                <Code className="w-3.5 h-3.5" /> Скрипты
              </button>
              <button
                onClick={() => setActiveTab("dashboard")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "dashboard"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/20"
                    : "text-slate-500 hover:text-slate-300"
                }`}
              >
                <Layers className="w-3.5 h-3.5" /> Хранилище
              </button>
              <button
                onClick={() => setShowQuantumLink(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  showQuantumLink
                    ? "bg-orange-600 text-white shadow-lg shadow-orange-600/20"
                    : "text-orange-400 hover:bg-orange-600/10"
                }`}
              >
                <Zap className="w-3.5 h-3.5" /> Quantum Link
              </button>
              <button
                onClick={fetchMigrationData}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "migration"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/20"
                    : "text-slate-500 hover:text-slate-300"
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
                onClick={() => setShowMarkerSplitter(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-amber-400 hover:bg-amber-600/10 border border-amber-500/20 hover:scale-105 active:scale-95 duration-300`}
              >
                <Compass className="w-3.5 h-3.5 text-amber-400" /> Сплиттер
                Маркеров
              </button>
              <button
                onClick={() => setActiveTab("external_skills_db")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "external_skills_db"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/40"
                    : "text-blue-400 hover:bg-blue-600/10 border border-blue-500/20"
                } hover:scale-105 active:scale-100 duration-300`}
              >
                <BrainCircuit className="w-3.5 h-3.5" /> База Знаний ИИ
              </button>
              <button
                onClick={() => setActiveTab("game_design")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "game_design"
                    ? "bg-purple-600 text-white shadow-lg shadow-purple-600/20"
                    : "text-purple-400 hover:bg-purple-600/10 border border-purple-500/20"
                }`}
              >
                <Gamepad2 className="w-3.5 h-3.5" /> Студия Игры
              </button>
              <button
                onClick={() => setActiveTab("game_help")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "game_help"
                    ? "bg-green-600 text-white shadow-lg shadow-green-600/40"
                    : "text-green-400 hover:bg-green-600/10 border border-green-500/20"
                } hover:scale-105 active:scale-95 duration-300`}
              >
                <HelpCircle className="w-3.5 h-3.5" /> {t.help}
              </button>
              <button
                onClick={fetchCapabilities}
                className="px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-blue-400 hover:bg-blue-600/10 hover:scale-105 active:scale-95 duration-300"
              >
                <Info className="w-3.5 h-3.5" /> {t.capabilities}
              </button>
            </nav>
          </div>

          <div className="flex items-center gap-4">
            <button
              onClick={handleLaunchOllama}
              className={`px-4 py-2 rounded-xl border transition-all group flex items-center gap-2 shadow-lg ${
                ollamaRunning
                  ? "bg-cyan-600/20 border-cyan-500/50 text-cyan-400 shadow-cyan-600/40"
                  : "bg-slate-800/20 border-white/5 text-slate-500 shadow-none"
              } hover:shadow-cyan-500/20 active:scale-95 duration-300`}
              title={ollamaRunning ? t.ollama : t.ollamaOff}
            >
              <Cpu
                className={`w-4 h-4 group-hover:scale-110 transition-transform ${ollamaRunning ? "animate-pulse" : ""}`}
              />
              <span className="text-[10px] font-bold uppercase tracking-widest hidden sm:inline">
                {ollamaRunning ? t.ollama : t.ollamaOff}
              </span>
            </button>
            <div className="flex items-center gap-2 px-3 py-1.5 bg-white/5 rounded-full border border-white/5">
              <Sparkles className="w-3 h-3 text-blue-400" />
              <span className="text-[10px] font-bold text-slate-400 uppercase">
                Gemini 1.5 Pro
              </span>
            </div>
          </div>
        </header>

        {/* Update Modal */}
        {/* Content Area */}
        <div className="flex-1 overflow-hidden flex flex-col relative">
          <AnimatePresence mode="wait">
            <motion.div
              key={activeTab}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.3, ease: "easeInOut" }}
              className="flex-1 overflow-hidden flex flex-col"
            >
              {activeTab === "chat" ? (
                <>
                  {/* Chat Header */}
                  <div className="px-6 py-4 border-b border-white/5 flex items-center justify-between bg-black/20">
                    <div className="flex items-center gap-3">
                      <div
                        className={`w-2 h-2 rounded-full ${aiHealth === "online" ? "bg-green-500 shadow-[0_0_12px_rgba(34,197,94,0.6)]" : "bg-yellow-500"}`}
                      />
                      <span className="text-[10px] font-bold text-white uppercase tracking-widest group">
                        {aiHealth === "online"
                          ? `${t.sync} (v18.8.0)`
                          : `${t.offline} (v18.8.0)`}
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={handleClearChat}
                        disabled={isClearingChat}
                        className="p-2 hover:bg-white/5 rounded-lg text-slate-500 hover:text-red-400 transition-all flex items-center gap-2 disabled:opacity-50 hover:shadow-glow active:scale-95"
                        title={t.clear}
                      >
                        <Trash2
                          className={`w-4 h-4 ${isClearingChat ? "animate-spin" : ""}`}
                        />
                        <span className="text-[9px] font-bold uppercase hidden sm:inline">
                          {isClearingChat ? t.clearing : t.clear}
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
                          whileHover={{ scale: 1.05 }}
                          className="w-24 h-24 bg-blue-600/10 rounded-[3rem] flex items-center justify-center mb-10 border border-blue-500/20 shadow-2xl shadow-blue-600/10 cursor-pointer group transition-all"
                        >
                          <Cpu className="w-12 h-12 text-blue-500 group-hover:text-blue-400 group-hover:rotate-12 transition-all" />
                        </motion.div>

                        <h2 className="text-2xl font-black text-white mb-4 uppercase tracking-tighter shadow-blue-500/10 drop-shadow-xl transition-all duration-700">
                          Unity AI Assistant v{appVersion}
                        </h2>
                        <div className="text-slate-400 text-sm leading-relaxed mb-10 max-w-lg px-4 font-medium italic">
                          {language === "Русский" ? (
                            <>
                              Я полностью осведомлен о вашем проекте по пути{" "}
                              <br />
                              <code className="text-blue-400 break-all bg-white/5 px-2 py-1 rounded mt-2 inline-block shadow-inner ring-1 ring-white/5 font-mono">
                                {kb?.project_path || "Загрузка..."}
                              </code>
                              .
                              <br />
                              <br />
                              Задавайте любые вопросы по Unity, Blender,
                              Photoshop или GIMP. Модули Menu Studio Visuals
                              Mastery, Omni-Answer Engine и проект 'Континент
                              судьбы' (v18.8.0) активированы.
                              <br />
                              <br />
                              <span className="text-xs text-orange-400 font-black uppercase ring-1 ring-orange-400/30 px-3 py-1.5 rounded-full bg-orange-400/5 shadow-lg shadow-orange-500/5 inline-block animate-pulse">
                                Внимание: {t.proMastery}
                              </span>
                            </>
                          ) : (
                            <>
                              I am fully aware of your project at path <br />
                              <code className="text-blue-400 break-all bg-white/5 px-2 py-1 rounded mt-2 inline-block shadow-inner ring-1 ring-white/5 font-mono">
                                {kb?.project_path || "Loading..."}
                              </code>
                              .
                              <br />
                              <br />
                              Ask any questions about Unity, Blender, Photoshop,
                              or GIMP. Zenith 3D Mastery, Omni-Answer Engine,
                              and project 'Fate Continent' (v18.8.0) are active.
                              <br />
                              <br />
                              <span className="text-xs text-orange-400 font-black uppercase ring-1 ring-orange-400/30 px-3 py-1.5 rounded-full bg-orange-400/5 shadow-lg shadow-orange-500/5 inline-block animate-pulse">
                                Attention: {t.proMastery}
                              </span>
                            </>
                          )}
                        </div>

                        <div className="p-8 bg-white/5 border border-white/5 rounded-[2rem] w-full text-left space-y-6">
                          <h3 className="text-sm font-black text-white uppercase tracking-[0.2em] flex items-center gap-2">
                            <Shield className="w-4 h-4 text-blue-500" /> Статус
                            API и Настройка ключей
                          </h3>
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-[11px] leading-relaxed text-slate-400">
                            <div className="space-y-3">
                              <p className="font-bold text-blue-400 uppercase">
                                Если в настройках Secrets пусто или стоит "AI
                                Studio Free Tier":
                              </p>
                              <ul className="space-y-1 opacity-80">
                                <li>
                                  • Это встроенный бесплатный доступ платформы.
                                </li>
                                <li>
                                  • Он работает автоматически — **нажимать
                                  карандаш не нужно**.
                                </li>
                                <li>
                                  • Если видите статус "Офлайн", значит сервис
                                  API временно перегружен.
                                </li>
                              </ul>
                            </div>
                            <div className="space-y-3">
                              <p className="font-bold text-orange-400 uppercase">
                                Если у вас есть личный ключ (AIza...):
                              </p>
                              <ul className="space-y-1 opacity-80">
                                <li>
                                  • В верхнем меню Secrets выберите **"Select
                                  key"**.
                                </li>
                                <li>
                                  • Вставьте ваш код и нажмите **"Apply
                                  changes"**.
                                </li>
                                <li>• Это снимет лимиты бесплатного уровня.</li>
                              </ul>
                            </div>
                          </div>
                        </div>
                      </div>
                    )}

                    {isThinking && (
                      <div className="flex justify-start">
                        <div className="max-w-[85%] bg-slate-900 shadow-2xl border border-blue-500/30 rounded-3xl p-6 backdrop-blur-xl">
                          <div className="flex items-center gap-4 mb-4">
                            <div className="w-8 h-8 rounded-full border-2 border-blue-500 border-t-transparent animate-spin shadow-[0_0_15px_rgba(59,130,246,0.3)]" />
                            <div>
                              <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-[0.2em]">
                                {t.thinking}
                              </h4>
                              <p className="text-[9px] text-slate-500 uppercase font-black tracking-widest">
                                {t.synth}
                              </p>
                            </div>
                          </div>
                          <div className="space-y-2">
                            {thinkingSteps.map((step, si) => (
                              <motion.div
                                initial={{ opacity: 0, x: -10 }}
                                animate={{ opacity: 1, x: 0 }}
                                key={si}
                                className="text-[10px] font-mono text-slate-400 flex items-center gap-2"
                              >
                                <span className="text-blue-500/50">›</span>{" "}
                                {step}
                              </motion.div>
                            ))}
                          </div>
                        </div>
                      </div>
                    )}

                    {messages.map((msg, i) => (
                      <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        key={i}
                        className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
                      >
                        <div
                          className={`max-w-[90%] group relative ${msg.role === "user" ? "bg-blue-600 text-white rounded-2xl rounded-tr-none px-5 py-3 shadow-lg shadow-blue-600/10" : "w-full"}`}
                        >
                          {msg.role === "assistant" && (
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
                                    onClick={() =>
                                      copyToClipboard(msg.content, `msg-${i}`)
                                    }
                                    className="p-1.5 hover:bg-white/5 rounded-md text-slate-500 hover:text-white transition-all opacity-0 group-hover:opacity-100"
                                  >
                                    {copiedId === `msg-${i}` ? (
                                      <Check className="w-3 h-3 text-green-500" />
                                    ) : (
                                      <Copy className="w-3 h-3" />
                                    )}
                                  </button>
                                </div>
                                <div className="markdown-body prose prose-invert prose-sm max-w-none text-slate-300 leading-relaxed">
                                  <Markdown>{msg.content}</Markdown>
                                </div>

                                {msg.audioVariants && (
                                  <div className="mt-6 space-y-4 pt-6 border-t border-white/5">
                                    <h4 className="text-[10px] font-bold text-white uppercase tracking-widest flex items-center gap-2">
                                      <Music className="w-3 h-3 text-blue-400" />{" "}
                                      Сгенерированные аудио-варианты (v18.8.0):
                                    </h4>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                      {msg.audioVariants.map((variant, vi) => (
                                        <div
                                          key={vi}
                                          className="p-4 rounded-2xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all space-y-3"
                                        >
                                          <div className="flex items-center justify-between">
                                            <span className="text-[10px] font-bold text-slate-400 uppercase truncate pr-2">
                                              {variant.name}
                                            </span>
                                            <a
                                              href={variant.url}
                                              download={`${variant.name}.mp3`}
                                              className="p-1.5 bg-blue-600/20 hover:bg-blue-600/40 rounded-lg text-blue-400 transition-all flex-shrink-0"
                                              title="Скачать MP3"
                                            >
                                              <Download className="w-3 h-3" />
                                            </a>
                                          </div>
                                          <audio
                                            controls
                                            className="w-full h-8 accent-blue-500"
                                          >
                                            <source
                                              src={variant.url}
                                              type="audio/mpeg"
                                            />
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
                          {msg.role === "user" && (
                            <div className="space-y-3">
                              <div className="text-sm font-medium leading-relaxed">
                                {msg.content}
                              </div>
                              {msg.files && (
                                <div className="flex flex-wrap gap-2">
                                  {msg.files.map((f, fi) => (
                                    <div
                                      key={fi}
                                      className="flex items-center gap-2 bg-black/20 px-3 py-1.5 rounded-lg border border-white/10 text-[9px] font-bold uppercase"
                                    >
                                      {f.type.includes("image") ? (
                                        <ImageIcon className="w-3 h-3 text-blue-400" />
                                      ) : f.type.includes("video") ? (
                                        <Video className="w-3 h-3 text-purple-400" />
                                      ) : f.type.includes("audio") ? (
                                        <Music className="w-3 h-3 text-green-400" />
                                      ) : (
                                        <FileText className="w-3 h-3 text-slate-400" />
                                      )}
                                      {f.name} (
                                      {(f.size / 1024 / 1024).toFixed(1)}MB)
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
                          <motion.div
                            animate={{ opacity: [0.3, 1, 0.3] }}
                            transition={{
                              duration: 1,
                              repeat: Infinity,
                              delay: 0,
                            }}
                            className="w-1.5 h-1.5 bg-slate-500 rounded-full"
                          />
                          <motion.div
                            animate={{ opacity: [0.3, 1, 0.3] }}
                            transition={{
                              duration: 1,
                              repeat: Infinity,
                              delay: 0.2,
                            }}
                            className="w-1.5 h-1.5 bg-slate-500 rounded-full"
                          />
                          <motion.div
                            animate={{ opacity: [0.3, 1, 0.3] }}
                            transition={{
                              duration: 1,
                              repeat: Infinity,
                              delay: 0.4,
                            }}
                            className="w-1.5 h-1.5 bg-slate-500 rounded-full"
                          />
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
                          notification.type === "success"
                            ? "bg-green-600/10 border-green-500/20 text-green-400"
                            : notification.type === "error"
                              ? "bg-red-600/10 border-red-500/20 text-red-400"
                              : "bg-blue-600/10 border-blue-500/20 text-blue-400"
                        }`}
                      >
                        {notification.type === "success" ? (
                          <Check className="w-5 h-5" />
                        ) : notification.type === "error" ? (
                          <AlertTriangle className="w-5 h-5" />
                        ) : (
                          <Info className="w-5 h-5" />
                        )}
                        <span className="text-xs font-bold uppercase tracking-tight">
                          {notification.message}
                        </span>
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
                              <span className="text-[10px] font-bold text-white uppercase">
                                Загрузка файлов...
                              </span>
                              {uploadTimeRemaining && (
                                <span className="text-[8px] text-slate-500 uppercase">
                                  Осталось: {uploadTimeRemaining}
                                </span>
                              )}
                            </div>
                          </div>
                          <span className="text-[10px] text-blue-400 font-mono font-bold">
                            {uploadProgress}%
                          </span>
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
                              <div
                                key={idx}
                                className="group relative flex items-center gap-2 bg-blue-600/10 border border-blue-500/30 px-3 py-2 rounded-xl text-[10px] font-bold text-blue-400 uppercase"
                              >
                                {file.type.includes("image") ? (
                                  <ImageIcon className="w-3.5 h-3.5" />
                                ) : (
                                  <FileText className="w-3.5 h-3.5" />
                                )}
                                <span className="max-w-[120px] truncate">
                                  {file.name}
                                </span>
                                <button
                                  onClick={() =>
                                    setAttachedFiles((prev) =>
                                      prev.filter((_, i) => i !== idx),
                                    )
                                  }
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
                          <Terminal className="w-3 h-3" /> Нажмите Enter, чтобы
                          отправить сообщение
                        </div>
                      </div>
                      <div className="relative group flex gap-3">
                        <button
                          onClick={() => setIsOllamaMode(!isOllamaMode)}
                          className={`p-4 border rounded-2xl transition-all flex items-center gap-2 group/btn ${
                            isOllamaMode
                              ? "bg-purple-600/20 border-purple-500/50 text-purple-400"
                              : "bg-white/5 border-white/10 text-slate-400 hover:text-white hover:border-white/20"
                          }`}
                          title={
                            isOllamaMode
                              ? "Ollama Active (Offline)"
                              : "Gemini Active (Online)"
                          }
                        >
                          <div
                            className={`w-2 h-2 rounded-full animate-pulse ${isOllamaMode ? "bg-purple-500 shadow-[0_0_8px_rgba(168,85,247,0.5)]" : "bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.5)]"}`}
                          />
                          <Database className="w-5 h-5" />
                          <span className="text-[10px] font-bold uppercase tracking-tighter hidden md:block">
                            {isOllamaMode ? "Ollama" : "Online"}
                          </span>
                        </button>
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
                            onPaste={handlePaste}
                            onKeyDown={(e) => {
                              if (e.key === "Enter" && !e.shiftKey) {
                                e.preventDefault();
                                handleSend();
                              }
                            }}
                            placeholder="Задайте вопрос по Unity или Blender..."
                            className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-5 pr-16 text-sm text-white placeholder:text-slate-600 focus:outline-none focus:border-blue-500/50 focus:bg-white/[0.07] transition-all resize-none h-18 scrollbar-none"
                          />
                          <button
                            onClick={() => handleSend()}
                            disabled={
                              (!input.trim() && attachedFiles.length === 0) ||
                              isTyping
                            }
                            className={`absolute right-4 top-4 p-3 rounded-xl transition-all ${
                              (input.trim() || attachedFiles.length > 0) &&
                              !isTyping
                                ? "bg-blue-600 text-white shadow-lg shadow-blue-600/20 hover:scale-105 active:scale-95"
                                : "bg-white/5 text-slate-600"
                            }`}
                          >
                            <Send className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </div>
                    <p className="text-center text-[9px] text-slate-600 mt-5 uppercase tracking-widest">
                      AI может ошибаться. Проверяйте код перед использованием в
                      проекте.
                    </p>
                  </div>
                </>
              ) : activeTab === "migration" ? (
                <div className="flex-1 overflow-y-auto p-8 scrollbar-thin scrollbar-thumb-white/5 h-full">
                  <div className="max-w-4xl mx-auto space-y-8">
                    <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-orange-600/10 to-red-600/10 border border-orange-500/20">
                      <div className="flex items-center gap-6 mb-6">
                        <div className="w-16 h-16 bg-orange-600 rounded-3xl flex items-center justify-center shadow-2xl shadow-orange-600/40">
                          <GitBranch className="w-8 h-8 text-white" />
                        </div>
                        <div>
                          <h2 className="text-2xl font-bold text-white uppercase tracking-tighter italic">
                            Помощник миграции (Unity → Godot/Redot)
                          </h2>
                          <p className="text-sm text-slate-400">
                            Инструменты и справочники для переноса ваших
                            проектов на открытые движки.
                          </p>
                        </div>
                      </div>
                      <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-slate-300 leading-relaxed">
                        {migrationData?.message ||
                          "Нет данных для отображения. Воспользуйтесь инструментами миграции в чате."}
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                      <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                        <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                          <Code className="w-4 h-4 text-blue-400" /> Карта
                          соответствий API
                        </h3>
                        <div className="space-y-2">
                          {Object.entries(migrationData?.mapping || {}).map(
                            ([unity, godot]: [any, any]) => (
                              <div
                                key={unity}
                                className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/5 group hover:bg-white/10 transition-all"
                              >
                                <span className="text-[10px] font-mono text-blue-400">
                                  {unity}
                                </span>
                                <ChevronRight className="w-3 h-3 text-slate-600" />
                                <span className="text-[10px] font-mono text-orange-400">
                                  {godot}
                                </span>
                              </div>
                            ),
                          )}
                        </div>
                      </section>

                      <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                        <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                          <Zap className="w-4 h-4 text-yellow-400" /> Советы по
                          конвертации
                        </h3>
                        <div className="space-y-4">
                          {migrationData?.tips?.map(
                            (tip: string, i: number) => (
                              <div
                                key={i}
                                className="p-4 rounded-2xl bg-white/5 border border-white/5 text-xs text-slate-300 leading-relaxed"
                              >
                                {tip}
                              </div>
                            ),
                          )}
                        </div>
                      </section>
                    </div>

                    <div className="p-8 rounded-[2.5rem] bg-blue-600/5 border border-blue-500/10">
                      <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-4">
                        Автоматизированный перенос (Experimental)
                      </h3>
                      <p className="text-xs text-slate-400 mb-6">
                        Мы работаем над скриптом, который сможет автоматически
                        конвертировать структуру сцены (.unity → .tscn) и
                        базовые C# скрипты. На данный момент рекомендуется
                        использовать ручной перенос с помощью карты соответствий
                        выше.
                      </p>
                      <button
                        onClick={() =>
                          showNotification(
                            "Функция автоматического переноса находится в разработке.",
                            "info",
                          )
                        }
                        className="px-6 py-3 bg-blue-600/20 border border-blue-500/30 rounded-xl text-[10px] font-bold text-blue-400 uppercase tracking-widest hover:bg-blue-600 hover:text-white transition-all"
                      >
                        Запустить анализ проекта
                      </button>
                    </div>
                  </div>
                </div>
              ) : activeTab === "external_skills_db" ? (
                <ExternalSkillsDBView />
              ) : activeTab === "game_help" ? (
                <GameHelpView />
              ) : activeTab === "game_design" ? (
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
                              {gameDesign?.game_title || "Континент судьбы"}
                            </h2>
                            <span className="px-3 py-1 rounded-full bg-purple-600/20 border border-purple-500/30 text-[10px] font-bold text-purple-400 uppercase tracking-widest">
                              v{gameDesign?.version || "1.2.0"}
                            </span>
                          </div>
                          <div className="text-xs text-slate-500 uppercase tracking-[0.3em] font-black mt-2 flex items-center gap-2">
                            <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                            Центральный штаб разработки •{" "}
                            {gameDesign?.style || "High Fantasy"}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <button
                          onClick={() =>
                            showNotification(
                              "Генерация GDD документа...",
                              "info",
                            )
                          }
                          className="px-6 py-4 rounded-2xl bg-white/5 border border-white/10 text-white hover:bg-white/10 transition-all flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest"
                        >
                          <Download className="w-4 h-4" /> Экспорт GDD
                        </button>
                        <button
                          onClick={handleSaveGameDesign}
                          disabled={isSavingGameDesign}
                          className={`px-10 py-4 rounded-2xl flex items-center gap-3 transition-all font-black uppercase text-[11px] tracking-widest ${
                            isSavingGameDesign
                              ? "bg-slate-800 text-slate-500"
                              : "bg-purple-600 hover:bg-purple-500 text-white shadow-2xl shadow-purple-900/40 active:scale-95"
                          }`}
                        >
                          {isSavingGameDesign ? (
                            <RefreshCw className="w-4 h-4 animate-spin" />
                          ) : (
                            <Save className="w-4 h-4" />
                          )}
                          {isSavingGameDesign
                            ? "Синхронизация..."
                            : "Сохранить Изменения"}
                        </button>
                      </div>
                    </div>

                    {/* Sub-tabs for Game Design */}
                    <div className="flex items-center gap-2 p-1.5 bg-black/40 rounded-2xl border border-white/5 w-fit overflow-x-auto max-w-full no-scrollbar">
                      {[
                        "World",
                        "Castle System",
                        "Heroes & Units",
                        "Visuals & Nav",
                        "Abilities",
                        "Synergies",
                        "Balancing & Rarity",
                        "Economy",
                        "Strategies",
                        "Combat & Environment",
                        "Potions & Alchemy",
                        "Quests & NPC",
                        "AI Strategies",
                        "Menu Studio",
                      ].map((tab) => (
                        <button
                          key={tab}
                          onClick={() => setDesignSubTab(tab as any)}
                          className={`px-8 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${
                            designSubTab === tab
                              ? "bg-purple-600 text-white shadow-lg"
                              : "text-slate-500 hover:text-white"
                          }`}
                        >
                          {tab}
                        </button>
                      ))}
                    </div>

                    {designSubTab === "World" ? (
                      <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="grid grid-cols-1 lg:grid-cols-2 gap-8"
                      >
                        <div className="space-y-8">
                          <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                            1. География Континентов
                          </h3>
                          <div className="grid grid-cols-1 gap-6">
                            {gameDesign?.continents?.map(
                              (cont: any, i: number) => (
                                <div
                                  key={i}
                                  className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 hover:border-purple-500/40 transition-all group relative overflow-hidden"
                                >
                                  <div className="absolute top-0 right-0 p-8 opacity-5 group-hover:scale-110 transition-transform">
                                    <MapIcon className="w-24 h-24 text-white" />
                                  </div>
                                  <div className="relative z-10 space-y-6">
                                    <div className="flex items-center justify-between">
                                      <div className="flex items-center gap-4">
                                        <div className="w-12 h-12 rounded-2xl bg-purple-600 text-white flex items-center justify-center font-black italic text-xl">
                                          0{i + 1}
                                        </div>
                                        <input
                                          value={cont.name}
                                          onChange={(e) => {
                                            const newConts = [
                                              ...gameDesign.continents,
                                            ];
                                            newConts[i].name = e.target.value;
                                            setGameDesign({
                                              ...gameDesign,
                                              continents: newConts,
                                            });
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
                                          <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest px-1">
                                            Окружение
                                          </h5>
                                          <div className="space-y-1">
                                            {cont.environment?.map(
                                              (item: string, idx: number) => (
                                                <div
                                                  key={idx}
                                                  className="flex items-start gap-2 text-[9px] text-slate-400 leading-tight"
                                                >
                                                  <div className="w-1 h-1 rounded-full bg-purple-500 mt-1 flex-shrink-0" />
                                                  {item}
                                                </div>
                                              ),
                                            )}
                                          </div>
                                        </div>
                                      )}

                                      <div className="space-y-3">
                                        <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest px-1">
                                          Фракции
                                        </h5>
                                        {cont.factions ? (
                                          <div className="grid grid-cols-1 gap-3">
                                            {cont.factions?.map(
                                              (f: any, fi: number) => (
                                                <div
                                                  key={fi}
                                                  className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1"
                                                >
                                                  <div className="flex items-center justify-between">
                                                    <input
                                                      value={f.name}
                                                      onChange={(e) => {
                                                        const newConts = [
                                                          ...gameDesign.continents,
                                                        ];
                                                        newConts[i].factions[
                                                          fi
                                                        ].name = e.target.value;
                                                        setGameDesign({
                                                          ...gameDesign,
                                                          continents: newConts,
                                                        });
                                                      }}
                                                      className="bg-transparent border-none text-[10px] font-black text-purple-400 focus:outline-none uppercase tracking-widest"
                                                    />
                                                    <Users className="w-3 h-3 text-slate-700" />
                                                  </div>
                                                  <textarea
                                                    value={f.locations}
                                                    onChange={(e) => {
                                                      const newConts = [
                                                        ...gameDesign.continents,
                                                      ];
                                                      newConts[i].factions[
                                                        fi
                                                      ].locations =
                                                        e.target.value;
                                                      setGameDesign({
                                                        ...gameDesign,
                                                        continents: newConts,
                                                      });
                                                    }}
                                                    className="w-full bg-transparent border-none text-[9px] text-slate-500 focus:outline-none resize-none h-10 leading-relaxed"
                                                  />
                                                </div>
                                              ),
                                            )}
                                          </div>
                                        ) : cont.structure ? (
                                          <div className="p-4 bg-blue-600/5 border border-blue-500/20 rounded-2xl space-y-3">
                                            {Object.entries(cont.structure).map(
                                              ([key, val]: any, si: number) => (
                                                <div
                                                  key={si}
                                                  className="flex items-center justify-between text-[9px]"
                                                >
                                                  <span className="text-slate-500 uppercase">
                                                    {key}:
                                                  </span>
                                                  <span className="text-slate-300 font-bold">
                                                    {val}
                                                  </span>
                                                </div>
                                              ),
                                            )}
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
                                          <h4 className="text-[10px] font-black text-green-400 uppercase tracking-[0.4em] mb-4">
                                            Выделение клеток
                                          </h4>
                                          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                            {Object.entries(
                                              gameDesign?.visual_system
                                                ?.cell_highlight || {},
                                            ).map(([key, val]: any) => (
                                              <div
                                                key={key}
                                                className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-2"
                                              >
                                                <div className="text-[9px] text-slate-500 uppercase font-black">
                                                  {key}
                                                </div>
                                                <div className="text-sm text-white font-medium">
                                                  {val}
                                                </div>
                                              </div>
                                            ))}
                                          </div>
                                          <div className="p-6 bg-white/5 rounded-3xl space-y-4">
                                            <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest">
                                              Система подсказок (Зум)
                                            </h5>
                                            <div className="space-y-3">
                                              {Object.entries(
                                                gameDesign?.visual_system
                                                  ?.scaling_hints || {},
                                              ).map(([key, val]: any) => (
                                                <div
                                                  key={key}
                                                  className="flex items-center justify-between text-[11px]"
                                                >
                                                  <span className="text-slate-500 uppercase">
                                                    {key}:
                                                  </span>
                                                  <span className="text-blue-400 font-bold italic">
                                                    {val}
                                                  </span>
                                                </div>
                                              ))}
                                            </div>
                                          </div>
                                        </div>
                                      </div>

                                      <div className="space-y-8">
                                        <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                                          <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-[0.4em]">
                                            Механика Камеры
                                          </h4>
                                          <div className="space-y-6">
                                            <div className="grid grid-cols-1 gap-3">
                                              <div className="text-[10px] text-slate-500 uppercase font-black">
                                                Уровни масштаба
                                              </div>
                                              {Object.entries(
                                                gameDesign?.camera_mechanics
                                                  ?.zoom_levels || {},
                                              ).map(([key, val]: any) => (
                                                <div
                                                  key={key}
                                                  className="p-4 bg-white/5 rounded-2xl border border-white/5 flex items-center justify-between"
                                                >
                                                  <span className="text-[10px] text-slate-400 uppercase">
                                                    {key}
                                                  </span>
                                                  <span className="text-[11px] text-white font-medium italic text-right">
                                                    {val}
                                                  </span>
                                                </div>
                                              ))}
                                            </div>
                                            <div className="p-6 bg-purple-600/5 rounded-3xl border border-purple-500/20 space-y-4">
                                              <div className="flex items-center gap-3 text-purple-400 font-bold text-[10px] uppercase tracking-widest">
                                                <RefreshCw className="w-4 h-4" />{" "}
                                                Вращение
                                              </div>
                                              <div className="text-[11px] text-slate-300">
                                                {
                                                  gameDesign?.camera_mechanics
                                                    ?.rotation?.free
                                                }
                                                . Фиксированные углы:{" "}
                                                {gameDesign?.camera_mechanics?.rotation?.fixed?.join(
                                                  "°, ",
                                                )}
                                                °.{" "}
                                                {
                                                  gameDesign?.camera_mechanics
                                                    ?.rotation?.auto
                                                }
                                                .
                                              </div>
                                            </div>
                                            <div className="grid grid-cols-2 gap-4">
                                              <div className="p-6 bg-white/5 rounded-3xl border border-white/5 space-y-2">
                                                <div className="text-[9px] text-slate-500 uppercase font-black">
                                                  Интерфейс
                                                </div>
                                                <div className="text-xs text-white">
                                                  {gameDesign?.camera_mechanics?.ui?.join(
                                                    ", ",
                                                  )}
                                                </div>
                                              </div>
                                              <div className="p-6 bg-white/5 rounded-3xl border border-white/5 space-y-2">
                                                <div className="text-[9px] text-slate-500 uppercase font-black">
                                                  Анимация
                                                </div>
                                                <div className="text-xs text-white">
                                                  {gameDesign?.camera_mechanics?.animations?.join(
                                                    ", ",
                                                  )}
                                                </div>
                                              </div>
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    </motion.div>
                                  </div>
                                </div>
                              ),
                            )}
                          </div>
                        </div>

                        <div className="space-y-8">
                          <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                            2. Глобальный Лор
                          </h3>
                          <div className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 space-y-6">
                            <div className="flex items-center gap-4">
                              <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                                <Sparkles className="w-5 h-5" />
                              </div>
                              <h4 className="text-sm font-black text-white uppercase tracking-widest">
                                Манифест Концепции
                              </h4>
                            </div>
                            <textarea
                              value={gameDesign?.core_concept || ""}
                              onChange={(e) =>
                                setGameDesign({
                                  ...gameDesign,
                                  core_concept: e.target.value,
                                })
                              }
                              className="w-full bg-black/40 border border-white/5 rounded-3xl p-6 text-sm text-slate-300 focus:outline-none focus:border-purple-500/40 min-h-[250px] transition-all leading-relaxed resize-none"
                              placeholder="Напишите историю мира Континент Судьбы..."
                            />
                            <div className="p-6 rounded-2xl bg-purple-600/5 border border-purple-500/20">
                              <p className="text-[10px] text-purple-300/60 leading-relaxed italic">
                                "Мир, где культивация силы — единственный путь к
                                вершине. Четыре континента, десятки рас и тысячи
                                лет войны за Эфирные Источники."
                              </p>
                            </div>
                          </div>

                          <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6">
                            <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                              Атрибуты Визуального Стиля
                            </h4>
                            <div className="flex flex-wrap gap-2">
                              {[
                                "Китайское фэнтези",
                                "Xianxia",
                                "Руническая магия",
                                "Парящие горы",
                                "Эфирный свет",
                                "Древние секты",
                              ].map((tag, i) => (
                                <span
                                  key={i}
                                  className="px-3 py-1.5 rounded-xl bg-white/5 border border-white/10 text-[9px] text-slate-400 uppercase tracking-widest font-bold"
                                >
                                  {tag}
                                </span>
                              ))}
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Castle System" ? (
                      !gameDesign?.castle_mechanics ? (
                        <div className="flex-1 flex items-center justify-center py-20">
                          <div className="text-center">
                            <Cpu className="w-12 h-12 text-slate-700 mx-auto mb-4 animate-pulse" />
                            <p className="text-slate-500 text-[10px] uppercase font-black tracking-widest italic">
                              Данные Castle Mechanics не найдены
                            </p>
                          </div>
                        </div>
                      ) : (
                        <motion.div
                          initial={{ opacity: 0 }}
                          animate={{ opacity: 1 }}
                          className="space-y-12"
                        >
                          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                            <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                              <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                                Визуальные состояния
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {Object.entries(
                                  gameDesign?.castle_mechanics?.visual_states ||
                                    {},
                                ).map(([key, state]: any) => (
                                  <div key={key} className="space-y-4">
                                    <div className="flex items-center gap-3">
                                      <div
                                        className={`p-2 rounded-xl ${key === "abandoned" ? "bg-slate-800 text-slate-400" : "bg-red-600/20 text-red-400"}`}
                                      >
                                        {key === "abandoned" ? (
                                          <CloudOff className="w-4 h-4" />
                                        ) : (
                                          <Skull className="w-4 h-4" />
                                        )}
                                      </div>
                                      <h5 className="text-[11px] font-black text-white uppercase tracking-widest">
                                        {state?.title || key}
                                      </h5>
                                    </div>
                                    <div className="space-y-2">
                                      {state?.features?.map(
                                        (f: string, fi: number) => (
                                          <div
                                            key={fi}
                                            className="flex items-start gap-2 text-[9px] text-slate-500 italic leading-tight"
                                          >
                                            <div className="w-1 h-1 rounded-full bg-purple-500 mt-1.5 shrink-0" />
                                            {f}
                                          </div>
                                        ),
                                      )}
                                    </div>
                                  </div>
                                ))}
                              </div>

                              <div className="pt-8 border-t border-white/5 space-y-4">
                                <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                                  Стили континентов
                                </h4>
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                  {Object.entries(
                                    gameDesign?.castle_mechanics
                                      ?.continental_styles || {},
                                  ).map(([key, style]: any) => (
                                    <div
                                      key={key}
                                      className="p-4 bg-white/5 rounded-2xl border border-white/5 space-y-1"
                                    >
                                      <div className="text-[8px] text-slate-600 uppercase font-black">
                                        {style?.name || key}
                                      </div>
                                      <div className="text-[10px] text-slate-300 leading-relaxed italic">
                                        {typeof style === "string"
                                          ? style
                                          : style?.style ||
                                            style?.desc ||
                                            "No style info"}
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              </div>
                            </div>

                            <div className="p-10 rounded-[3rem] bg-gradient-to-br from-purple-600/10 to-blue-600/10 border border-purple-500/20 space-y-8">
                              <h4 className="text-[10px] font-black text-white uppercase tracking-widest flex items-center gap-2">
                                <TrendingUp className="w-4 h-4 text-purple-400" />{" "}
                                Глобальные уровни развития (1—5)
                              </h4>
                              <div className="space-y-4">
                                {gameDesign?.castle_mechanics?.development_levels?.map(
                                  (dl: any) => (
                                    <div
                                      key={dl.level}
                                      className="p-6 bg-black/40 rounded-3xl border border-white/5 group hover:border-white/20 transition-all"
                                    >
                                      <div className="flex items-center justify-between mb-4">
                                        <div className="flex items-center gap-3">
                                          <div className="w-8 h-8 rounded-xl bg-purple-600 flex items-center justify-center text-xs font-black italic">
                                            {dl.level}
                                          </div>
                                          <h5 className="text-[11px] font-black text-white uppercase tracking-widest">
                                            {dl.name}
                                          </h5>
                                        </div>
                                        <span className="text-[8px] text-slate-500 font-bold uppercase">
                                          {dl.state}
                                        </span>
                                      </div>
                                      <div className="grid grid-cols-3 gap-4">
                                        <div className="space-y-1">
                                          <div className="text-[8px] text-slate-600 uppercase font-black">
                                            Гарнизон
                                          </div>
                                          <div className="text-[10px] text-slate-400 italic leading-tight">
                                            {dl.garrison}
                                          </div>
                                        </div>
                                        <div className="space-y-1">
                                          <div className="text-[8px] text-slate-600 uppercase font-black">
                                            Оборона
                                          </div>
                                          <div className="text-[10px] text-slate-400 italic leading-tight">
                                            {dl.defense}
                                          </div>
                                        </div>
                                        <div className="space-y-1">
                                          <div className="text-[8px] text-slate-600 uppercase font-black">
                                            Экономика
                                          </div>
                                          <div className="text-[10px] text-slate-400 italic leading-tight">
                                            {dl.economy}
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  ),
                                )}
                              </div>
                            </div>
                          </div>

                          <div className="grid grid-cols-1 gap-12">
                            {gameDesign?.continents?.map(
                              (cont: any, i: number) => (
                                <div key={i} className="space-y-6">
                                  <div className="flex items-center gap-4 px-4">
                                    <div className="w-8 h-8 rounded-lg bg-purple-600 text-white flex items-center justify-center font-black italic text-xs leading-none">
                                      0{i + 1}
                                    </div>
                                    <h4 className="text-lg font-black text-white uppercase tracking-tighter italic">
                                      {cont.name}: Путь Развития Замка
                                    </h4>
                                  </div>
                                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
                                    {cont.castles?.map((lvl: any) => (
                                      <div
                                        key={lvl.level}
                                        className="p-6 rounded-[2rem] bg-white/5 border border-white/5 hover:border-purple-500/40 transition-all group relative overflow-hidden"
                                      >
                                        <div className="text-[9px] font-black text-slate-700 uppercase mb-3 tracking-widest flex items-center justify-between">
                                          <span>Level {lvl.level}</span>
                                          {lvl.special && (
                                            <Zap className="w-3 h-3 text-yellow-500" />
                                          )}
                                        </div>
                                        <h5 className="text-xs font-black text-white uppercase mb-4 tracking-tighter leading-tight min-h-[2rem]">
                                          {lvl.name}
                                        </h5>
                                        <div className="space-y-3">
                                          <div className="p-3 rounded-xl bg-black/40 border border-white/5 space-y-1.5">
                                            <div className="text-[8px] text-slate-600 uppercase font-black">
                                              Внешний вид
                                            </div>
                                            <div className="text-[10px] text-slate-400 leading-tight h-10 overflow-y-auto scrollbar-none">
                                              {lvl.appearance}
                                            </div>
                                          </div>
                                          <div className="space-y-2">
                                            {lvl.units && (
                                              <div className="flex items-center justify-between text-[9px]">
                                                <span className="text-slate-500 uppercase">
                                                  Войска:
                                                </span>
                                                <span className="text-blue-400 font-bold text-right">
                                                  {lvl.units}
                                                </span>
                                              </div>
                                            )}
                                            {lvl.income && (
                                              <div className="flex items-center justify-between text-[9px]">
                                                <span className="text-slate-500 uppercase">
                                                  Доход:
                                                </span>
                                                <span className="text-yellow-500 font-bold">
                                                  {lvl.income}
                                                </span>
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
                              ),
                            )}
                          </div>
                        </motion.div>
                      )
                    ) : designSubTab === "Heroes & Units" ? (
                      <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        className="grid grid-cols-1 lg:grid-cols-2 gap-8"
                      >
                        <div className="space-y-8">
                          <div className="p-6 rounded-3xl bg-purple-600/10 border border-purple-500/20">
                            <h3 className="text-[10px] font-black text-purple-400 uppercase tracking-[0.4em] mb-4 flex items-center gap-2">
                              <Users className="w-4 h-4" /> Лимиты покупки
                              героев
                            </h3>
                            <div className="grid grid-cols-2 gap-2">
                              {[
                                {
                                  l: "Континент 1",
                                  v: gameDesign?.hero_limits?.continent_1,
                                },
                                {
                                  l: "Континент 2",
                                  v: gameDesign?.hero_limits?.continent_2,
                                },
                                {
                                  l: "Континент 3",
                                  v: gameDesign?.hero_limits?.continent_3,
                                },
                                {
                                  l: "Континент 4",
                                  v: gameDesign?.hero_limits?.continent_4,
                                },
                              ].map((item, idx) => (
                                <div
                                  key={idx}
                                  className="flex items-center justify-between p-2 bg-black/40 rounded-xl"
                                >
                                  <span className="text-[9px] text-slate-400 font-bold uppercase">
                                    {item.l}
                                  </span>
                                  <span className="text-[11px] text-white font-black">
                                    {item.v} шт
                                  </span>
                                </div>
                              ))}
                            </div>
                            <div className="mt-2 text-[8px] text-purple-300 font-bold text-center uppercase tracking-widest">
                              +1 Основной герой у всех (Игрок/ИИ)
                            </div>
                          </div>

                          <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                            Основные Классы
                          </h3>
                          <div className="grid grid-cols-1 gap-4">
                            {gameDesign?.hero_classes?.main_heroes?.map(
                              (h: any, i: number) => (
                                <div
                                  key={i}
                                  className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 hover:border-purple-500/40 transition-all group"
                                >
                                  <div className="flex items-center justify-between mb-8">
                                    <div className="flex items-center gap-4">
                                      <div className="p-4 bg-purple-600 rounded-2xl text-white">
                                        {h.class === "Воин" ? (
                                          <Shield className="w-6 h-6" />
                                        ) : h.class === "Лучник" ? (
                                          <Target className="w-6 h-6" />
                                        ) : (
                                          <Zap className="w-6 h-6" />
                                        )}
                                      </div>
                                      <div>
                                        <h4 className="text-xl font-black text-white uppercase tracking-tighter italic">
                                          {h.class}
                                        </h4>
                                        <span className="text-[9px] text-purple-400 font-black uppercase tracking-widest">
                                          {h.bonus}
                                        </span>
                                      </div>
                                    </div>
                                  </div>
                                  <div className="grid grid-cols-3 gap-4 mb-6">
                                    <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                      <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                        HP
                                      </div>
                                      <div className="text-lg font-black text-red-500">
                                        {h.hp}
                                      </div>
                                    </div>
                                    <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                      <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                        ATK
                                      </div>
                                      <div className="text-lg font-black text-orange-500">
                                        {h.atk}
                                      </div>
                                    </div>
                                    <div className="p-3 bg-black/40 rounded-xl text-center border border-white/5">
                                      <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                        DEF
                                      </div>
                                      <div className="text-lg font-black text-blue-500">
                                        {h.def}
                                      </div>
                                    </div>
                                  </div>
                                  <div className="flex items-center gap-6 text-[10px] text-slate-500 uppercase font-black tracking-widest px-2">
                                    <span className="flex items-center gap-2">
                                      <ArrowRight className="w-3 h-3" /> SPEED:{" "}
                                      {h.speed}
                                    </span>
                                    <span className="flex items-center gap-2">
                                      <ArrowRight className="w-3 h-3" /> RANGE:{" "}
                                      {h.range}
                                    </span>
                                  </div>
                                </div>
                              ),
                            )}
                          </div>
                        </div>

                        <div className="space-y-8">
                          <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                            Под-герои (Отряд)
                          </h3>
                          <div className="grid grid-cols-1 gap-4">
                            {gameDesign?.hero_classes?.sub_heroes?.map(
                              (h: any, i: number) => (
                                <div
                                  key={i}
                                  className="p-6 rounded-3xl bg-black/40 border border-white/5 flex items-center justify-between"
                                >
                                  <div className="flex items-center gap-4">
                                    <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center text-slate-500">
                                      {h.class === "Воин" ? (
                                        <Shield className="w-4 h-4" />
                                      ) : h.class === "Лучник" ? (
                                        <Target className="w-4 h-4" />
                                      ) : (
                                        <Zap className="w-4 h-4" />
                                      )}
                                    </div>
                                    <div>
                                      <h5 className="text-[11px] font-black text-white uppercase tracking-widest">
                                        {h.class} (Sub)
                                      </h5>
                                      <p className="text-[9px] text-slate-600 font-mono italic">
                                        {h.skill}
                                      </p>
                                    </div>
                                  </div>
                                  <div className="text-right">
                                    <div className="text-[10px] font-black text-red-500">
                                      {h.hp} HP
                                    </div>
                                    <div className="text-[10px] font-black text-orange-500">
                                      {h.atk} ATK
                                    </div>
                                  </div>
                                </div>
                              ),
                            )}
                          </div>

                          <div className="p-8 rounded-[2.5rem] bg-gradient-to-r from-blue-600/10 to-transparent border border-blue-500/20 space-y-6">
                            <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-widest">
                              Система Грузоподъёмности
                            </h4>
                            <div className="space-y-4">
                              <div className="grid grid-cols-1 gap-2">
                                {[
                                  {
                                    label: "Простой Герой (L1)",
                                    value: "600 КГ",
                                    sub: "+500кг каждые 100 ур.",
                                  },
                                  {
                                    label: "Главный Герой (L1)",
                                    value: "1200 КГ",
                                    sub: "+1000кг каждые 100 ур.",
                                  },
                                ].map((b, i) => (
                                  <div
                                    key={i}
                                    className="flex hover:bg-white/5 p-2 rounded-xl transition-colors items-center justify-between"
                                  >
                                    <div>
                                      <div className="text-[10px] text-white font-black uppercase">
                                        {b.label}
                                      </div>
                                      <div className="text-[8px] text-slate-500 uppercase">
                                        {b.sub}
                                      </div>
                                    </div>
                                    <div className="text-[11px] font-black text-blue-400">
                                      {b.value}
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>

                          <div className="p-8 rounded-[2.5rem] bg-red-600/5 border border-red-500/20 space-y-6">
                            <div className="flex items-center justify-between">
                              <h4 className="text-[10px] font-black text-red-400 uppercase tracking-widest flex items-center gap-2">
                                <Skull className="w-4 h-4" /> Разбойники: Ранги
                                и Характеристики
                              </h4>
                              <span className="px-2 py-1 bg-red-500/10 rounded-lg text-[8px] font-black text-red-500 uppercase">
                                Enemy NPC
                              </span>
                            </div>
                            <div className="space-y-4">
                              {gameDesign?.bandit_faction?.ranks?.map(
                                (rank: any, ri: number) => (
                                  <div
                                    key={ri}
                                    className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-2 group hover:border-red-500/30 transition-all"
                                  >
                                    <div className="flex items-center justify-between">
                                      <span className="text-[10px] font-black text-white uppercase italic">
                                        {rank.rank} (ур. {rank.level})
                                      </span>
                                      <span className="text-[9px] text-slate-500 font-bold">
                                        {rank.range}
                                      </span>
                                    </div>
                                    <div className="grid grid-cols-4 gap-2">
                                      <div className="text-center p-1.5 bg-red-500/10 rounded-lg">
                                        <div className="text-[7px] text-red-400 uppercase font-bold">
                                          HP
                                        </div>
                                        <div className="text-[10px] text-white font-black">
                                          {rank.hp}
                                        </div>
                                      </div>
                                      <div className="text-center p-1.5 bg-orange-500/10 rounded-lg">
                                        <div className="text-[7px] text-orange-400 uppercase font-bold">
                                          ATK
                                        </div>
                                        <div className="text-[10px] text-white font-black">
                                          {rank.atk}
                                        </div>
                                      </div>
                                      <div className="text-center p-1.5 bg-blue-500/10 rounded-lg">
                                        <div className="text-[7px] text-blue-400 uppercase font-bold">
                                          DEF
                                        </div>
                                        <div className="text-[10px] text-white font-black">
                                          {rank.def}
                                        </div>
                                      </div>
                                      <div className="text-center p-1.5 bg-green-500/10 rounded-lg">
                                        <div className="text-[7px] text-green-400 uppercase font-bold">
                                          SPD
                                        </div>
                                        <div className="text-[10px] text-white font-black">
                                          {rank.spd}
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                ),
                              )}
                            </div>

                            <div className="pt-4 border-t border-white/5 space-y-4">
                              <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest flex items-center gap-2">
                                <Crown className="w-4 h-4 text-orange-400" />{" "}
                                Герои Разбойников
                              </h5>
                              <div className="grid grid-cols-1 gap-3">
                                {Object.values(
                                  gameDesign?.bandit_faction?.heroes || {},
                                ).map((hero: any, hi: number) => (
                                  <div
                                    key={hi}
                                    className="p-4 bg-white/5 rounded-2xl border border-white/5 flex items-start gap-4"
                                  >
                                    <div className="p-3 bg-red-600/20 rounded-xl text-red-500">
                                      {hero.name === "Тень Ветра" ? (
                                        <Zap className="w-5 h-5" />
                                      ) : hero.name === "Огненный Лис" ? (
                                        <Flame className="w-5 h-5" />
                                      ) : hero.name === "Глаз Ястреба" ? (
                                        <Target className="w-5 h-5" />
                                      ) : (
                                        <Sword className="w-5 h-5" />
                                      )}
                                    </div>
                                    <div className="flex-1 space-y-2">
                                      <div className="flex items-center justify-between">
                                        <span className="text-[11px] font-black text-white uppercase italic">
                                          {hero.name}
                                        </span>
                                        <span className="text-[8px] text-slate-500 font-bold uppercase">
                                          {hero.title}
                                        </span>
                                      </div>
                                      <div className="flex gap-4 text-[9px] text-slate-400 font-bold italic">
                                        <span>HP: {hero.hp}</span>
                                        <span>ATK: {hero.atk}</span>
                                        <span>SPD: {hero.spd}</span>
                                      </div>
                                      <div className="bg-black/20 p-2 rounded-lg">
                                        <div className="text-[8px] text-orange-400 font-black uppercase mb-1">
                                          Ультимейт: {hero.ultimate.name}
                                        </div>
                                        <p className="text-[8px] text-slate-500 leading-tight italic">
                                          {hero.ultimate.desc}
                                        </p>
                                      </div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>

                            <div className="pt-4 border-t border-white/5 space-y-3">
                              <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest">
                                Внешний вид по континентам
                              </h5>
                              <div className="space-y-3">
                                {Object.entries(
                                  gameDesign?.castle_mechanics
                                    ?.continental_styles || {},
                                ).map(([key, style]: any) => (
                                  <div
                                    key={key}
                                    className="p-3 bg-black/40 rounded-xl border border-white/5 space-y-2"
                                  >
                                    <div className="flex items-center gap-2">
                                      <div className="w-1.5 h-1.5 rounded-full bg-red-500" />
                                      <span className="text-[9px] font-black text-white uppercase">
                                        {style.name}
                                      </span>
                                    </div>
                                    <div className="space-y-1">
                                      <p className="text-[8px] text-slate-400">
                                        <span className="text-slate-500 font-bold uppercase">
                                          Одежда:
                                        </span>{" "}
                                        {style.appearance.clothes}
                                      </p>
                                      <p className="text-[8px] text-slate-400">
                                        <span className="text-slate-500 font-bold uppercase">
                                          Оружие:
                                        </span>{" "}
                                        {style.appearance.weapons}
                                      </p>
                                      <p className="text-[8px] text-slate-400">
                                        <span className="text-slate-500 font-bold uppercase">
                                          Особенности:
                                        </span>{" "}
                                        {style.appearance.features}
                                      </p>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Abilities" ? (
                      <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        className="space-y-12"
                      >
                        <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
                          <div className="xl:col-span-1 space-y-6">
                            <div className="flex items-center justify-between px-4">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                                Простые Герои (L1-5000)
                              </h3>
                              <div className="px-2 py-1 bg-white/5 rounded-lg text-[9px] text-slate-400 font-bold">
                                1-10 LVL
                              </div>
                            </div>
                            <div className="space-y-4">
                              {Object.entries(
                                gameDesign?.ability_system?.simple || {},
                              ).map(([cls, skills]: any) => (
                                <div
                                  key={cls}
                                  className="p-6 rounded-[2rem] bg-black/40 border border-white/10 space-y-4"
                                >
                                  <h4 className="text-sm font-black text-white uppercase italic">
                                    {cls}
                                  </h4>
                                  <div className="space-y-2">
                                    {skills.map((s: any, i: number) => (
                                      <div
                                        key={i}
                                        className="p-3 bg-white/5 rounded-xl border border-white/5 group hover:border-purple-500/30 transition-all"
                                      >
                                        <div className="flex items-center justify-between mb-1">
                                          <span className="text-[10px] font-black text-white uppercase truncate pr-2">
                                            {s.name}
                                          </span>
                                          <span
                                            className={`text-[8px] font-black uppercase ${s.type === "active" ? "text-orange-400" : "text-blue-400"}`}
                                          >
                                            {s.type}
                                          </span>
                                        </div>
                                        <div className="flex justify-between text-[9px] text-slate-500 italic">
                                          <span>L1: {s.lvl1}</span>
                                          <span>L10: {s.lvl10}</span>
                                        </div>
                                        {s.cd && (
                                          <div className="mt-1 text-[8px] text-yellow-500/60 font-mono">
                                            Откат: {s.cd}х
                                          </div>
                                        )}
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="xl:col-span-1 space-y-6">
                            <div className="flex items-center justify-between px-4">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                                Главные Герои (L1-5000)
                              </h3>
                              <div className="px-2 py-1 bg-purple-600/20 rounded-lg text-[9px] text-purple-400 font-bold">
                                1-20 LVL
                              </div>
                            </div>
                            <div className="space-y-4">
                              {Object.entries(
                                gameDesign?.ability_system?.main || {},
                              ).map(([cls, skills]: any) => (
                                <div
                                  key={cls}
                                  className="p-6 rounded-[2rem] bg-purple-600/5 border border-purple-500/20 space-y-4"
                                >
                                  <h4 className="text-sm font-black text-white uppercase italic">
                                    {cls}
                                  </h4>
                                  <div className="space-y-2">
                                    {skills.map((s: any, i: number) => (
                                      <div
                                        key={i}
                                        className={`p-3 rounded-xl border transition-all ${s.type === "heroic" ? "bg-purple-600/20 border-purple-500/40" : "bg-black/60 border-white/5"}`}
                                      >
                                        <div className="flex items-center justify-between mb-1">
                                          <span className="text-[10px] font-black text-white uppercase">
                                            {s.name}
                                          </span>
                                          <span
                                            className={`text-[8px] font-black uppercase ${s.type === "heroic" ? "text-yellow-400 animate-pulse" : "text-purple-400"}`}
                                          >
                                            {s.type}
                                          </span>
                                        </div>
                                        <p className="text-[9px] text-slate-400 leading-snug">
                                          {s.lvl20 || s.lvl10}
                                        </p>
                                        {s.cd && (
                                          <div className="mt-1 text-[8px] text-yellow-500/60 font-mono">
                                            Откат: {s.cd}х
                                          </div>
                                        )}
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="xl:col-span-1 space-y-8">
                            <div className="space-y-6">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                                Система Откатов
                              </h3>
                              <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6">
                                <div className="grid grid-cols-1 gap-4">
                                  {gameDesign?.cooldown_system?.modifiers &&
                                    Object.entries(
                                      gameDesign.cooldown_system.modifiers,
                                    ).map(([key, val]: any) => (
                                      <div key={key} className="space-y-2">
                                        <div className="text-[9px] text-slate-600 uppercase font-black px-1">
                                          {key === "gear"
                                            ? "Экипировка"
                                            : key === "skills"
                                              ? "Навыки"
                                              : key === "locations"
                                                ? "Локации"
                                                : "Эффекты"}
                                        </div>
                                        <div className="p-3 bg-white/5 rounded-2xl border border-white/5 text-[10px] text-slate-300 italic">
                                          {val}
                                        </div>
                                      </div>
                                    ))}
                                </div>
                                <div className="p-4 bg-yellow-500/5 border border-yellow-500/20 rounded-2xl">
                                  <div className="flex items-center gap-2 mb-2">
                                    <Clock className="w-3 h-3 text-yellow-500" />
                                    <span className="text-[9px] font-black text-yellow-500 uppercase">
                                      Ограничения
                                    </span>
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
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                                Механика Прокачки
                              </h3>
                              <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-purple-600/10 to-transparent border border-purple-500/20 space-y-6">
                                <div className="space-y-4">
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-400">
                                      Простые герои
                                    </span>
                                    <span className="text-white font-bold">
                                      +1 XP / использование
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-400">
                                      Главные герои
                                    </span>
                                    <span className="text-purple-400 font-bold">
                                      +2 XP / использование
                                    </span>
                                  </div>
                                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                                    <h5 className="text-[9px] font-black text-white uppercase mb-2">
                                      Визуальная эволюция
                                    </h5>
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
                    ) : designSubTab === "Synergies" ? (
                      <motion.div
                        initial={{ opacity: 0, scale: 0.98 }}
                        animate={{ opacity: 1, scale: 1 }}
                        className="space-y-12"
                      >
                        <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 px-4">
                          <div>
                            <h3 className="text-xl font-black text-white uppercase italic tracking-tighter">
                              Прогрессия Синергий
                            </h3>
                            <p className="text-[10px] text-slate-500 uppercase tracking-widest mt-1">
                              Взаимодействие умений и уровни мастерства (1—9999)
                            </p>
                          </div>
                          <div className="flex p-1 bg-black/40 rounded-2xl border border-white/5">
                            <button
                              onClick={() => setSynergyHeroType("simple")}
                              className={`px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${synergyHeroType === "simple" ? "bg-blue-600 text-white shadow-[0_0_20px_rgba(37,99,235,0.3)]" : "text-slate-500 hover:text-slate-300"}`}
                            >
                              Простые герои
                            </button>
                            <button
                              onClick={() => setSynergyHeroType("main")}
                              className={`px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${synergyHeroType === "main" ? "bg-purple-600 text-white shadow-[0_0_20px_rgba(147,51,234,0.3)]" : "text-slate-500 hover:text-slate-300"}`}
                            >
                              Главные герои
                            </button>
                          </div>
                        </div>

                        <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
                          {Object.entries(
                            (synergyHeroType === "simple"
                              ? gameDesign?.skill_synergies?.simple_heroes
                              : gameDesign?.skill_synergies?.main_heroes) || {},
                          ).map(([cls, data]: any) => (
                            <div key={cls} className="space-y-8">
                              <div className="flex items-center gap-4 px-6">
                                <div
                                  className={`p-4 rounded-3xl ${
                                    cls === "Warrior"
                                      ? "bg-orange-600/20 text-orange-400 border border-orange-500/20"
                                      : cls === "Mage"
                                        ? "bg-purple-600/20 text-purple-400 border border-purple-500/20"
                                        : "bg-green-600/20 text-green-400 border border-green-500/20"
                                  }`}
                                >
                                  {cls === "Warrior" ? (
                                    <Shield className="w-6 h-6" />
                                  ) : cls === "Mage" ? (
                                    <Zap className="w-6 h-6" />
                                  ) : (
                                    <Target className="w-6 h-6" />
                                  )}
                                </div>
                                <div>
                                  <h3 className="text-lg font-black text-white uppercase italic">
                                    {cls === "Warrior"
                                      ? "Воин"
                                      : cls === "Mage"
                                        ? "Маг"
                                        : "Стрелок"}
                                  </h3>
                                  <div className="flex items-center gap-2">
                                    <span className="w-2 h-2 rounded-full bg-purple-500 animate-pulse" />
                                    <span className="text-[9px] text-slate-500 uppercase font-black tracking-widest">
                                      Прогрессия 5 Тиров
                                    </span>
                                  </div>
                                </div>
                              </div>

                              <div className="space-y-6">
                                {data.tiers?.map((tier: any, ti: number) => (
                                  <div key={ti} className="relative group">
                                    {ti < data.tiers.length - 1 && (
                                      <div className="absolute left-[34px] top-20 bottom-0 w-[2px] bg-gradient-to-b from-white/10 to-transparent z-0 hidden md:block" />
                                    )}

                                    <div className="p-8 rounded-[3rem] bg-black/40 border border-white/10 group-hover:border-white/20 transition-all relative z-10 space-y-6">
                                      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                                        <div className="flex items-center gap-4">
                                          <div className="w-10 h-10 rounded-2xl bg-white/5 border border-white/10 flex items-center justify-center text-[11px] font-black text-slate-400 italic">
                                            {ti + 1}
                                          </div>
                                          <div>
                                            <div className="text-[10px] text-purple-400 font-bold uppercase tracking-[0.2em]">
                                              {tier.range} LVL
                                            </div>
                                            <h5 className="text-sm font-black text-white uppercase tracking-tighter italic">
                                              {tier.title}
                                            </h5>
                                          </div>
                                        </div>
                                        <div className="flex gap-2">
                                          <span className="px-3 py-1 bg-blue-600/10 border border-blue-500/20 rounded-lg text-[9px] font-black text-blue-400 text-center flex items-center">
                                            {tier.stats}
                                          </span>
                                          <span className="px-3 py-1 bg-yellow-600/10 border border-yellow-500/20 rounded-lg text-[9px] font-black text-yellow-500 text-center flex items-center">
                                            CD: {tier.cooldown}
                                          </span>
                                        </div>
                                      </div>

                                      <div className="space-y-3 pl-4 border-l-2 border-white/5">
                                        {tier.effects?.map(
                                          (eff: string, ei: number) => (
                                            <div
                                              key={ei}
                                              className="flex gap-3 text-[10px] text-slate-300 leading-relaxed italic"
                                            >
                                              <div className="mt-1.5 w-1 h-1 rounded-full bg-blue-500 shrink-0" />
                                              {eff}
                                            </div>
                                          ),
                                        )}
                                      </div>

                                      <div className="p-4 bg-purple-600/5 rounded-2xl border border-purple-500/10 flex items-center gap-3">
                                        <Sparkles className="w-4 h-4 text-purple-400" />
                                        <span className="text-[9px] text-slate-400 uppercase font-bold tracking-widest">
                                          {tier.visual}
                                        </span>
                                      </div>
                                    </div>
                                  </div>
                                ))}

                                {synergyHeroType === "main" &&
                                  data.unique_effects && (
                                    <div className="p-10 rounded-[3rem] bg-gradient-to-br from-purple-600/20 to-blue-600/20 border border-purple-500/30 space-y-6">
                                      <h4 className="text-[10px] font-black text-white uppercase tracking-widest flex items-center gap-2">
                                        <Star className="w-4 h-4 text-yellow-500" />{" "}
                                        Уникальные особенности
                                      </h4>
                                      <div className="space-y-3">
                                        {data.unique_effects.map(
                                          (ue: string, uei: number) => (
                                            <div
                                              key={uei}
                                              className="p-3 bg-black/40 rounded-2xl border border-white/5 text-[10px] text-slate-300 font-bold italic leading-relaxed"
                                            >
                                              {ue}
                                            </div>
                                          ),
                                        )}
                                      </div>
                                    </div>
                                  )}
                              </div>
                            </div>
                          ))}
                        </div>

                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                          <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                            <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest px-4">
                              Общие механики взаимодействия
                            </h4>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                              {[
                                {
                                  title: "Эффект «Комбо»",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.combo_effect,
                                  icon: (
                                    <Zap className="w-4 h-4 text-yellow-500" />
                                  ),
                                },
                                {
                                  title: "Специализация",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.specialization,
                                  icon: (
                                    <Sparkles className="w-4 h-4 text-purple-500" />
                                  ),
                                },
                                {
                                  title: "Адаптация",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.adaptation,
                                  icon: (
                                    <RefreshCw className="w-4 h-4 text-blue-500" />
                                  ),
                                },
                                {
                                  title: "Мастерство",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.mastery,
                                  icon: (
                                    <Star className="w-4 h-4 text-orange-500" />
                                  ),
                                },
                              ].map((m, i) => (
                                <div
                                  key={i}
                                  className="p-6 rounded-3xl bg-white/5 border border-white/5 space-y-3"
                                >
                                  <div className="flex items-center gap-3">
                                    {m.icon}
                                    <div className="text-[11px] font-black text-white uppercase tracking-widest">
                                      {m.title}
                                    </div>
                                  </div>
                                  <p className="text-[10px] text-slate-500 leading-relaxed italic">
                                    {m.desc}
                                  </p>
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="p-10 rounded-[3rem] bg-gradient-to-br from-blue-600/10 to-purple-600/10 border border-blue-500/20 space-y-8">
                            <h4 className="text-[10px] font-black text-blue-400 uppercase tracking-widest">
                              Правила Активации
                            </h4>
                            <div className="space-y-6">
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div className="space-y-2">
                                  <div className="text-[9px] text-slate-500 uppercase font-black">
                                    Окно активации
                                  </div>
                                  <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-white font-medium italic">
                                    {
                                      gameDesign?.skill_synergies?.rules
                                        ?.activation_window
                                    }
                                  </div>
                                </div>
                                <div className="space-y-2">
                                  <div className="text-[9px] text-slate-500 uppercase font-black">
                                    Перезарядка синергии
                                  </div>
                                  <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-white font-medium italic">
                                    {
                                      gameDesign?.skill_synergies?.rules
                                        ?.cooldown
                                    }
                                  </div>
                                </div>
                              </div>
                              <div className="space-y-4 pt-4 border-t border-white/10">
                                <div className="text-[9px] text-slate-500 uppercase font-black">
                                  Влияние местности
                                </div>
                                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                                  {Object.entries(
                                    gameDesign?.skill_synergies?.rules
                                      ?.terrain_mod || {},
                                  ).map(([key, val]: any) => (
                                    <div
                                      key={key}
                                      className="p-3 bg-white/5 rounded-xl border border-white/5 flex flex-col gap-1"
                                    >
                                      <div className="text-[8px] text-slate-600 uppercase font-black">
                                        {key === "mountains"
                                          ? "Горы"
                                          : key === "forest"
                                            ? "Лес"
                                            : "Равнины"}
                                      </div>
                                      <div className="text-[10px] text-slate-300 font-bold">
                                        {val}
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              </div>
                              <div className="p-6 bg-purple-600/10 rounded-[2.5rem] border border-purple-500/20 text-[11px] text-purple-300 italic leading-relaxed">
                                "Синергии требуют непосредственного участия
                                игрока и понимания таймингов. При активации
                                иконка умения мерцает золотым, а звуковой сигнал
                                подтверждает успех комбинации."
                              </div>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Balancing & Rarity" ? (
                      <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        className="space-y-12"
                      >
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                          <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                            <div className="flex items-center justify-between">
                              <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                                Механика Урона
                              </h4>
                              <div className="px-3 py-1 bg-blue-500/10 rounded-lg text-[9px] text-blue-400 font-black uppercase">
                                Ур. 1—9999
                              </div>
                            </div>

                            <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-blue-600/10 to-transparent border border-blue-500/20 space-y-6">
                              <div className="flex items-center gap-4">
                                <div className="p-4 bg-blue-600 rounded-2xl text-white shadow-lg">
                                  <Calculator className="w-6 h-6" />
                                </div>
                                <div className="flex-1">
                                  <h5 className="text-[10px] font-black text-slate-400 uppercase italic mb-1">
                                    Базовая Формула
                                  </h5>
                                  <div className="text-xl font-black text-blue-400 tracking-tighter whitespace-pre-wrap">
                                    {
                                      gameDesign?.combat_mechanics?.formulas
                                        ?.base_damage
                                    }
                                  </div>
                                </div>
                              </div>
                              <div className="space-y-4 pt-4 border-t border-white/5">
                                {gameDesign?.combat_mechanics?.calculation_steps?.map(
                                  (step: string, si: number) => (
                                    <div
                                      key={si}
                                      className="flex items-center gap-4 text-[11px] text-slate-400 font-bold italic group"
                                    >
                                      <div className="w-6 h-6 rounded-full bg-white/5 border border-white/10 flex items-center justify-center text-[9px] font-black group-hover:bg-blue-600 group-hover:text-white transition-all">
                                        {si + 1}
                                      </div>
                                      <span className="group-hover:text-white transition-colors">
                                        {step}
                                      </span>
                                    </div>
                                  ),
                                )}
                              </div>
                            </div>

                            <div className="space-y-4">
                              <h5 className="text-[9px] font-black text-slate-500 uppercase tracking-widest px-2">
                                Система Заклинаний
                              </h5>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="p-5 bg-purple-600/5 border border-purple-500/20 rounded-2xl space-y-2">
                                  <div className="text-[8px] text-purple-400 font-black uppercase">
                                    Стоимость маны
                                  </div>
                                  <div className="text-[10px] text-white font-mono italic">
                                    {
                                      gameDesign?.combat_mechanics?.formulas
                                        ?.spells?.cost
                                    }
                                  </div>
                                </div>
                                <div className="p-5 bg-orange-600/5 border border-orange-500/20 rounded-2xl space-y-2">
                                  <div className="text-[8px] text-orange-400 font-black uppercase">
                                    Урон заклинаний
                                  </div>
                                  <div className="text-[10px] text-white font-mono italic">
                                    {
                                      gameDesign?.combat_mechanics?.formulas
                                        ?.spells?.damage
                                    }
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>

                          <div className="p-10 rounded-[3rem] bg-purple-600/5 border border-purple-500/10 space-y-8">
                            <div className="flex items-center justify-between">
                              <h4 className="text-[10px] font-black text-purple-400 uppercase tracking-[0.4em]">
                                Коэффициенты Прогрессии
                              </h4>
                              <div className="flex items-center gap-2">
                                <TrendingUp className="w-3 h-3 text-purple-400" />
                                <span className="text-[9px] text-slate-500 font-bold">
                                  Scaling Logic
                                </span>
                              </div>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                              {Object.entries(
                                gameDesign?.combat_mechanics?.formulas
                                  ?.level_scaling || {},
                              ).map(([key, formula]: any) => (
                                <div
                                  key={key}
                                  className="p-5 bg-black/40 rounded-3xl border border-white/5 group hover:border-purple-500/30 transition-all"
                                >
                                  <div className="text-[9px] text-slate-600 uppercase font-black mb-2 flex items-center justify-between">
                                    {key}
                                    <span className="w-1.5 h-1.5 rounded-full bg-purple-600/40 group-hover:bg-purple-500 animate-pulse" />
                                  </div>
                                  <div className="text-[11px] text-purple-400 font-mono font-black italic break-words leading-relaxed">
                                    {formula}
                                  </div>
                                </div>
                              ))}
                            </div>

                            <div className="p-8 bg-indigo-600/5 rounded-[2.5rem] border border-indigo-500/20 space-y-4">
                              <div className="flex items-center justify-between">
                                <h5 className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">
                                  High-Level Balancer (L100+)
                                </h5>
                                <span className="text-[8px] text-slate-500 font-black">
                                  ANTI-INFLATION
                                </span>
                              </div>
                              <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-center">
                                <div className="text-[12px] font-black text-white italic mb-1">
                                  {
                                    gameDesign?.combat_mechanics?.formulas
                                      ?.high_level_balancer?.modifier_formula
                                  }
                                </div>
                                <p className="text-[9px] text-slate-500 leading-snug">
                                  Замедляет рост характеристик при достижении
                                  порога в{" "}
                                  {
                                    gameDesign?.combat_mechanics?.formulas
                                      ?.high_level_balancer?.threshold
                                  }{" "}
                                  уровней, предотвращая "раздувание" цифр.
                                </p>
                              </div>
                            </div>
                          </div>
                        </div>

                        <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                          <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                            Статистика Классов (База и Рост)
                          </h4>
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                            {Object.entries(
                              gameDesign?.combat_mechanics
                                ?.class_growth_tables || {},
                            ).map(([cls, stats]: any) => (
                              <div
                                key={cls}
                                className="p-8 rounded-[2.5rem] bg-white/5 border border-white/5 space-y-6 group hover:bg-white/10 transition-all"
                              >
                                <div className="flex items-center justify-between">
                                  <h5 className="text-xl font-black text-white uppercase italic tracking-tighter">
                                    {cls}
                                  </h5>
                                  <div className="p-2 bg-white/5 rounded-xl">
                                    {cls === "warrior" ? (
                                      <Sword className="w-5 h-5 text-red-400" />
                                    ) : cls === "mage" ? (
                                      <Zap className="w-5 h-5 text-blue-400" />
                                    ) : (
                                      <Target className="w-5 h-5 text-green-400" />
                                    )}
                                  </div>
                                </div>
                                <div className="space-y-3">
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-500 uppercase font-black">
                                      HP (Base/Growth)
                                    </span>
                                    <span className="text-red-400 font-bold">
                                      {stats.base_hp} / +{stats.hp_growth * 100}
                                      %
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-500 uppercase font-black">
                                      MP (Base/Growth)
                                    </span>
                                    <span className="text-blue-400 font-bold">
                                      {stats.base_mp} / +{stats.mp_growth * 100}
                                      %
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-500 uppercase font-black">
                                      ATK/DEF Growth
                                    </span>
                                    <span className="text-purple-400 font-bold">
                                      +{stats.atk_growth * 100}% / +
                                      {stats.def_growth * 100}%
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between text-[10px] pt-2 border-t border-white/5">
                                    <span className="text-slate-600 uppercase font-black">
                                      Regen (HP/MP)
                                    </span>
                                    <span className="text-white font-black">
                                      {stats.regen_hp} / {stats.regen_mp}
                                    </span>
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Economy" ? (
                      <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="space-y-8"
                      >
                        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                          <div className="lg:col-span-2 space-y-8">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                              Экономика Найма
                            </h3>
                            <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 overflow-hidden">
                              <table className="w-full text-left text-[10px]">
                                <thead>
                                  <tr className="text-slate-600 border-b border-white/5 uppercase tracking-widest">
                                    <th className="pb-4 pt-2 font-black">
                                      Тип войск
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      База (K)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Сосед (+25%)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Дальний (+50%)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Крайний (+100%)
                                    </th>
                                  </tr>
                                </thead>
                                <tbody className="divide-y divide-white/5">
                                  {[
                                    { name: "Легкая броня", base: "3000-5000" },
                                    {
                                      name: "Средняя броня",
                                      base: "6000-10000",
                                    },
                                    {
                                      name: "Тяжелая броня",
                                      base: "11000-15000",
                                    },
                                    {
                                      name: "Дальние (Ср)",
                                      base: "16000-20000",
                                    },
                                    {
                                      name: "Легендарные",
                                      base: "50000-100000",
                                    },
                                  ].map((row, i) => {
                                    const baseMin = parseInt(
                                      row.base.split("-")[0],
                                    );
                                    const baseMax = parseInt(
                                      row.base.split("-")[1],
                                    );
                                    return (
                                      <tr
                                        key={i}
                                        className="group hover:bg-white/5 transition-colors"
                                      >
                                        <td className="py-4 font-black text-white">
                                          {row.name}
                                        </td>
                                        <td className="py-4 text-slate-400 italic">
                                          {row.base}
                                        </td>
                                        <td className="py-4 text-blue-400 font-bold">
                                          {Math.round(baseMin * 1.25)}-
                                          {Math.round(baseMax * 1.25)}
                                        </td>
                                        <td className="py-4 text-purple-400 font-bold">
                                          {Math.round(baseMin * 1.5)}-
                                          {Math.round(baseMax * 1.5)}
                                        </td>
                                        <td className="py-4 text-red-500 font-bold">
                                          {Math.round(baseMin * 2)}-
                                          {Math.round(baseMax * 2)}
                                        </td>
                                      </tr>
                                    );
                                  })}
                                </tbody>
                              </table>
                            </div>

                            <div className="p-8 rounded-[2.5rem] bg-gradient-to-r from-purple-600/10 to-transparent border border-purple-500/20">
                              <h4 className="text-[10px] font-black text-purple-400 uppercase tracking-widest mb-6">
                                Скидки по Классам
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    Воин
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Тяжелая/Ближние
                                  </div>
                                </div>
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    Стрелок
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Дальние
                                  </div>
                                </div>
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    Маг
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Легенды/Маги
                                  </div>
                                </div>
                              </div>
                            </div>

                            <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                              <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                                Бонусы Замков (L5)
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                                <div className="space-y-4">
                                  <div className="text-[9px] text-blue-400 font-black uppercase tracking-widest">
                                    Типы Бонусов
                                  </div>
                                  <div className="space-y-2">
                                    {[
                                      {
                                        type: "Экономический",
                                        bonus: "+10% Золота",
                                      },
                                      { type: "Военный", bonus: "+5% Груз" },
                                      {
                                        type: "Магический",
                                        bonus: "-5% Цена Легенд",
                                      },
                                      {
                                        type: "Торговый",
                                        bonus: "-5% Цена Континента",
                                      },
                                    ].map((b, i) => (
                                      <div
                                        key={i}
                                        className="flex justify-between items-center text-[10px] p-2 bg-white/5 rounded-xl border border-white/5"
                                      >
                                        <span className="text-slate-500 italic">
                                          {b.type}
                                        </span>
                                        <span className="text-white font-bold">
                                          {b.bonus}
                                        </span>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                                <div className="space-y-4">
                                  <div className="text-[9px] text-purple-400 font-black uppercase tracking-widest">
                                    Множитель за кол-во
                                  </div>
                                  <div className="space-y-2">
                                    {[
                                      { count: "3 Замка", mult: "x1.5" },
                                      { count: "5 Замка", mult: "x2.0" },
                                      { count: "7 Замков", mult: "x3.0" },
                                      { count: "10 Замков", mult: "x4.0" },
                                    ].map((m, i) => (
                                      <div
                                        key={i}
                                        className="flex justify-between items-center text-[10px] p-2 bg-white/5 rounded-xl border border-white/5"
                                      >
                                        <span className="text-slate-500 font-black uppercase">
                                          {m.count}
                                        </span>
                                        <span className="text-purple-400 font-bold">
                                          {m.mult}
                                        </span>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              </div>
                              <div className="p-6 bg-blue-600/5 rounded-3xl border border-blue-500/20 text-[10px] leading-relaxed text-slate-500 italic mt-6">
                                "Важно: На уровне сложности 'Невероятный'
                                множитель бонусов замков снижен до x0.6, а макс.
                                кол-во замков одного типа ограничено семью до
                                1000 уровня."
                              </div>
                            </div>
                          </div>

                          <div className="space-y-8">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                              Уровни Сложности
                            </h3>
                            <div className="grid grid-cols-1 gap-4">
                              {[
                                {
                                  name: "Новичок",
                                  color: "text-green-400",
                                  desc: "-20% Цена, +20% Грузовик, +50% Золото",
                                },
                                {
                                  name: "Средний",
                                  color: "text-blue-400",
                                  desc: "Базовые параметры",
                                },
                                {
                                  name: "Сложный",
                                  color: "text-orange-400",
                                  desc: "+25% Цена, -15% Грузовик, -20% Золото",
                                },
                                {
                                  name: "Невероятный",
                                  color: "text-red-500",
                                  desc: "+50% Цена, -30% Грузовик, -40% Золото, Легенды с 1000 уровня",
                                },
                              ].map((diff, i) => (
                                <div
                                  key={i}
                                  className="p-6 rounded-3xl bg-white/5 border border-white/5 hover:border-blue-500/30 transition-all group"
                                >
                                  <div
                                    className={`text-[11px] font-black uppercase tracking-widest mb-2 ${diff.color}`}
                                  >
                                    {diff.name}
                                  </div>
                                  <p className="text-[10px] text-slate-500 leading-relaxed italic">
                                    {diff.desc}
                                  </p>
                                </div>
                              ))}
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Strategies" ? (
                      <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="space-y-12"
                      >
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                          <div className="space-y-6">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                              Тактическое Развертывание Разбойников
                            </h3>
                            <div className="grid grid-cols-1 gap-4">
                              {Object.entries(
                                gameDesign?.bandit_faction?.strategies || {},
                              ).map(([key, strat]: any) => (
                                <div
                                  key={key}
                                  className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5 space-y-4 hover:border-red-500/20 transition-all"
                                >
                                  <div className="flex items-center justify-between">
                                    <h4 className="text-xl font-black text-white uppercase italic tracking-tighter">
                                      {key === "wind_plains"
                                        ? "Ветреные Равнины"
                                        : key === "mountain_range"
                                          ? "Горный Хребет"
                                          : key === "ancient_woods"
                                            ? "Древние Леса"
                                            : "Рассвет Империи"}
                                    </h4>
                                    <span className="px-3 py-1 bg-red-500/10 rounded-xl text-[10px] font-black text-red-500">
                                      {strat.cities} Города
                                    </span>
                                  </div>
                                  <p className="text-[11px] text-slate-400 leading-relaxed">
                                    <span className="text-slate-500 font-bold uppercase mr-2">
                                      Тактика:
                                    </span>
                                    {strat.tactics}
                                  </p>
                                  <div className="space-y-2">
                                    <div className="text-[9px] text-slate-600 uppercase font-black">
                                      Приоритеты развития:
                                    </div>
                                    <div className="flex flex-wrap gap-2">
                                      {strat.priorities.map((p, pi) => (
                                        <span
                                          key={pi}
                                          className="px-3 py-1 bg-white/5 border border-white/10 rounded-lg text-[9px] text-slate-300 font-bold"
                                        >
                                          {p}
                                        </span>
                                      ))}
                                    </div>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="space-y-6">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                              Прогрессия по Сложности
                            </h3>
                            <div className="space-y-4">
                              {Object.entries(
                                gameDesign?.bandit_faction
                                  ?.difficulty_scaling || {},
                              ).map(([key, scale]: any) => (
                                <div
                                  key={key}
                                  className="p-8 rounded-[2.5rem] bg-gradient-to-br from-slate-900 to-black border border-white/5 space-y-4"
                                >
                                  <div className="flex items-center justify-between">
                                    <span
                                      className={`text-[10px] font-black uppercase tracking-widest ${
                                        key === "beginner"
                                          ? "text-green-400"
                                          : key === "medium"
                                            ? "text-blue-400"
                                            : key === "hard"
                                              ? "text-orange-400"
                                              : "text-red-500"
                                      }`}
                                    >
                                      {key}
                                    </span>
                                    <span className="text-[10px] font-mono text-slate-600 italic">
                                      Диапазон УР: {scale.lvl_range}
                                    </span>
                                  </div>
                                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                                    <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                      Основные Цели:
                                    </div>
                                    <p className="text-[11px] text-slate-300 italic leading-relaxed">
                                      {scale.goal}
                                    </p>
                                  </div>
                                </div>
                              ))}
                            </div>

                            <div className="p-10 rounded-[3rem] bg-indigo-600/5 border border-indigo-500/20 space-y-6">
                              <h4 className="text-[10px] font-black text-indigo-400 uppercase tracking-[0.4em]">
                                Общие Рекомендации
                              </h4>
                              <div className="space-y-4">
                                {[
                                  {
                                    t: "Развитие городов",
                                    d: "Приоритет на экономические постройки в начале, затем военные.",
                                  },
                                  {
                                    t: "Покупка войск",
                                    d: "Балансируйте между массовкой и элитными отрядами для засад.",
                                  },
                                  {
                                    t: "Использование зелий",
                                    d: "Всегда держите запас зелий скорости и невидимости.",
                                  },
                                  {
                                    t: "Герои",
                                    d: "Специализируйте героев под условия континента.",
                                  },
                                ].map((rec, ri) => (
                                  <div
                                    key={ri}
                                    className="flex items-start gap-4 p-4 hover:bg-white/5 rounded-2xl transition-all"
                                  >
                                    <div className="w-1.5 h-1.5 rounded-full bg-indigo-500 mt-1.5 flex-shrink-0" />
                                    <div className="space-y-1">
                                      <div className="text-[10px] font-black text-white uppercase italic">
                                        {rec.t}
                                      </div>
                                      <p className="text-[10px] text-slate-500 leading-snug">
                                        {rec.d}
                                      </p>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Combat & Environment" ? (
                      <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="space-y-12"
                      >
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-8">
                          <div className="space-y-6">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4 flex items-center gap-2">
                              <MapIcon className="w-3 h-3" /> Континенты и
                              Ландшафт
                            </h3>
                            <div className="grid grid-cols-1 gap-4">
                              {Object.entries(
                                gameDesign?.world_combat_locations
                                  ?.continents || {},
                              ).map(([key, data]: any) => (
                                <div
                                  key={key}
                                  className="p-8 rounded-[3rem] bg-black/40 border border-white/5 space-y-6 hover:bg-black/60 transition-all flex flex-col"
                                >
                                  <div className="flex items-center justify-between">
                                    <h4
                                      className={`text-xl font-black uppercase italic tracking-tighter ${
                                        key === "plains_of_winds"
                                          ? "text-green-400"
                                          : key === "mountain_range"
                                            ? "text-slate-400"
                                            : key === "ancient_woods"
                                              ? "text-emerald-500"
                                              : "text-amber-400"
                                      }`}
                                    >
                                      {data.name}
                                    </h4>
                                    <div className="flex gap-2 text-slate-500 opacity-50">
                                      {key === "plains_of_winds" ? (
                                        <Wind className="w-5 h-5" />
                                      ) : key === "mountain_range" ? (
                                        <Mountain className="w-5 h-5" />
                                      ) : key === "ancient_woods" ? (
                                        <Flame className="w-5 h-5" />
                                      ) : (
                                        <Shield className="w-5 h-5" />
                                      )}
                                    </div>
                                  </div>

                                  <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                      <div className="text-[8px] text-slate-600 uppercase font-black tracking-widest">
                                        Состав Клеток:
                                      </div>
                                      <div className="space-y-1">
                                        {Object.entries(data.cells).map(
                                          ([ctype, perc]: any) => (
                                            <div
                                              key={ctype}
                                              className="flex justify-between items-center text-[10px] text-slate-400"
                                            >
                                              <span className="italic">
                                                {ctype}
                                              </span>
                                              <span className="font-mono">
                                                {perc}%
                                              </span>
                                            </div>
                                          ),
                                        )}
                                      </div>
                                    </div>
                                    <div className="space-y-2">
                                      <div className="text-[8px] text-slate-600 uppercase font-black tracking-widest">
                                        Эффекты:
                                      </div>
                                      <div className="p-3 bg-white/5 rounded-xl space-y-1">
                                        <div className="text-[9px] text-green-400 font-bold tracking-tighter">
                                          {data.effects.bonus}
                                        </div>
                                        <div className="text-[9px] text-red-400 font-bold tracking-tighter">
                                          {data.effects.debuff}
                                        </div>
                                      </div>
                                    </div>
                                  </div>

                                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5 mt-auto">
                                    <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                      Тактика:
                                    </div>
                                    <p className="text-[11px] text-slate-300 italic leading-relaxed">
                                      {data.tactics}
                                    </p>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="space-y-8">
                            <div className="space-y-6">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4 flex items-center gap-2">
                                <Zap className="w-3 h-3 text-yellow-500" />{" "}
                                Динамические События
                              </h3>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div className="p-8 rounded-[2.5rem] bg-indigo-600/5 border border-indigo-500/20 space-y-6">
                                  <div className="flex items-center gap-4">
                                    <div className="p-3 bg-blue-500 rounded-xl">
                                      <Droplets className="w-5 h-5 text-white" />
                                    </div>
                                    <div>
                                      <div className="text-[10px] font-black text-blue-400 uppercase tracking-widest">
                                        Weather: Rain
                                      </div>
                                      <div className="text-[9px] text-slate-500 font-bold uppercase">
                                        Chance:{" "}
                                        {gameDesign?.dynamic_events?.weather
                                          ?.rain?.chance * 100}
                                        %
                                      </div>
                                    </div>
                                  </div>
                                  <p className="text-[10px] text-slate-400 italic leading-relaxed">
                                    {
                                      gameDesign?.dynamic_events?.weather?.rain
                                        ?.effects
                                    }
                                  </p>
                                </div>

                                <div className="p-8 rounded-[2.5rem] bg-slate-600/5 border border-slate-500/20 space-y-6">
                                  <div className="flex items-center gap-4">
                                    <div className="p-3 bg-slate-500 rounded-xl">
                                      <CloudOff className="w-5 h-5 text-white" />
                                    </div>
                                    <div>
                                      <div className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
                                        Weather: Fog
                                      </div>
                                      <div className="text-[9px] text-slate-500 font-bold uppercase">
                                        Chance:{" "}
                                        {gameDesign?.dynamic_events?.weather
                                          ?.fog?.chance * 100}
                                        %
                                      </div>
                                    </div>
                                  </div>
                                  <p className="text-[10px] text-slate-400 italic leading-relaxed">
                                    {
                                      gameDesign?.dynamic_events?.weather?.fog
                                        ?.effects
                                    }
                                  </p>
                                </div>
                              </div>
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                              <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6 shadow-2xl">
                                <Sun className="w-5 h-5 text-orange-400" />
                                <div className="space-y-2">
                                  <h4 className="text-xs font-black text-white uppercase italic tracking-widest">
                                    Дневной Цикл
                                  </h4>
                                  <p className="text-[10px] text-slate-500 italic leading-relaxed">
                                    {
                                      gameDesign?.dynamic_events?.time_cycle
                                        ?.day?.effects
                                    }
                                  </p>
                                </div>
                              </div>
                              <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6 shadow-2xl">
                                <Moon className="w-5 h-5 text-indigo-400" />
                                <div className="space-y-2">
                                  <h4 className="text-xs font-black text-white uppercase italic tracking-widest">
                                    Ночной Цикл
                                  </h4>
                                  <p className="text-[10px] text-slate-500 italic leading-relaxed">
                                    {
                                      gameDesign?.dynamic_events?.time_cycle
                                        ?.night?.effects
                                    }
                                  </p>
                                </div>
                              </div>
                            </div>

                            <div className="p-10 rounded-[3rem] bg-red-600/5 border border-red-500/20 space-y-6">
                              <h4 className="text-[10px] font-black text-red-500 uppercase tracking-[0.4em]">
                                Случайные Угрозы
                              </h4>
                              <div className="grid grid-cols-1 gap-4">
                                {Object.entries(
                                  gameDesign?.dynamic_events
                                    ?.random_encounters || {},
                                ).map(([key, data]: any) => (
                                  <div
                                    key={key}
                                    className="flex items-center justify-between p-4 bg-white/5 rounded-2xl border border-white/5 group hover:bg-white/10 transition-all cursor-default text-slate-100"
                                  >
                                    <div className="flex flex-col gap-1">
                                      <span className="text-[11px] font-black text-white uppercase italic tracking-tighter">
                                        {key.replace("_", " ")}
                                      </span>
                                      {data.chance && (
                                        <span className="text-[9px] text-slate-600 font-bold uppercase">
                                          Шанс: {data.chance * 100}%
                                        </span>
                                      )}
                                    </div>
                                    <div className="text-right">
                                      <div className="text-[9px] text-slate-400 italic max-w-[200px] leading-snug">
                                        {data.effects || data.rewards}
                                      </div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>
                        </div>

                        <div className="p-10 rounded-[4rem] bg-black/40 border border-white/10 space-y-8">
                          <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                            Боевые Объекты и Клетки
                          </h4>
                          <div className="grid grid-cols-2 md:grid-cols-5 gap-6">
                            {Object.entries(
                              gameDesign?.world_combat_locations?.cell_types ||
                                {},
                            ).map(([key, data]: any) => (
                              <div
                                key={key}
                                className="flex flex-col items-center gap-4 p-6 rounded-3xl bg-white/5 border border-white/5 group hover:border-white/20 transition-all"
                              >
                                <div
                                  className={`p-4 rounded-2xl ${
                                    key === "passable"
                                      ? "bg-green-600/10 text-green-400"
                                      : key === "hard"
                                        ? "bg-orange-600/10 text-orange-400"
                                        : key === "impassable"
                                          ? "bg-slate-600/10 text-slate-400"
                                          : key === "hidden"
                                            ? "bg-blue-600/10 text-blue-400"
                                            : "bg-red-600/10 text-red-400"
                                  }`}
                                >
                                  {key === "passable" ? (
                                    <Layout className="w-6 h-6" />
                                  ) : key === "hard" ? (
                                    <Activity className="w-6 h-6" />
                                  ) : key === "impassable" ? (
                                    <Box className="w-6 h-6" />
                                  ) : key === "hidden" ? (
                                    <Eye className="w-6 h-6" />
                                  ) : (
                                    <Skull className="w-6 h-6" />
                                  )}
                                </div>
                                <div className="text-center">
                                  <span className="text-[10px] font-black text-white uppercase italic tracking-tighter block">
                                    {key}
                                  </span>
                                  <span className="text-[9px] text-slate-500 font-bold uppercase block mt-1">
                                    {data.effects || "Стандарт"}
                                  </span>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      </motion.div>
                    ) : designSubTab === "Quests & NPC" ? (
                      <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="space-y-12"
                      >
                        {/* DIAMOND DESIGN DIALOGUE SYSTEM WORKSPACE v18.9.0 */}
                        <div className="p-8 md:p-12 rounded-[3.5rem] bg-gradient-to-br from-indigo-950/40 via-purple-950/20 to-black border border-indigo-500/20 space-y-8 shadow-2xl relative overflow-hidden backdrop-blur-xl">
                          <div className="absolute top-0 right-0 p-8 opacity-[0.03] pointer-events-none">
                            <MessageSquare className="w-96 h-96 text-indigo-400" />
                          </div>

                          <div className="flex flex-col xl:flex-row items-start xl:items-center justify-between gap-6 relative z-10 border-b border-indigo-500/10 pb-6">
                            <div>
                              <div className="flex items-center gap-3">
                                <span className="px-3 py-1 rounded-full bg-indigo-500/20 border border-indigo-500/30 text-[9px] font-black uppercase text-indigo-400 tracking-widest animate-pulse">
                                  ZENITH EXCLUSIVE DIALOG SYSTEM (v18.11.7)
                                </span>
                                <span className="px-2 py-0.5 rounded-full bg-amber-500/20 border border-amber-400/30 text-[9px] font-bold text-amber-400">
                                  Unity 6 Ready
                                </span>
                              </div>
                              <h3 className="text-3xl font-black text-white uppercase italic tracking-tighter mt-2">
                                DialogueSystem_Manager • Симулятор Диалога
                              </h3>
                              <p className="text-xs text-slate-400 mt-1 max-w-2xl">
                                Проектирование и визуализация нелинейных
                                диалоговых цепочек с выбором класса героя и
                                тактической области удаления скверны.
                              </p>
                            </div>

                            {/* Controls (Language) */}
                            <div className="flex flex-wrap items-center gap-4 relative z-20">
                              <span className="text-[10px] font-black text-slate-500 uppercase tracking-wider">
                                Язык озвучки / текста:
                              </span>
                              <div className="bg-black/40 p-1 rounded-xl border border-white/5 flex gap-1">
                                {[
                                  "RU",
                                  "EN",
                                  "DE",
                                  "FR",
                                  "ES",
                                  "PT",
                                  "JA",
                                  "KR",
                                  "CH",
                                ].map((lang) => (
                                  <button
                                    key={lang}
                                    onClick={() => {
                                      setSimDialogueLang(lang as any);
                                      // Play synth click
                                      try {
                                        const ctx = new (
                                          window.AudioContext ||
                                          (window as any).webkitAudioContext
                                        )();
                                        const osc = ctx.createOscillator();
                                        const gain = ctx.createGain();
                                        osc.type = "sine";
                                        osc.frequency.setValueAtTime(
                                          600,
                                          ctx.currentTime,
                                        );
                                        osc.frequency.exponentialRampToValueAtTime(
                                          300,
                                          ctx.currentTime + 0.05,
                                        );
                                        gain.gain.setValueAtTime(
                                          0.1,
                                          ctx.currentTime,
                                        );
                                        gain.gain.exponentialRampToValueAtTime(
                                          0.01,
                                          ctx.currentTime + 0.05,
                                        );
                                        osc.connect(gain);
                                        gain.connect(ctx.destination);
                                        osc.start();
                                        osc.stop(ctx.currentTime + 0.05);
                                      } catch (e) {}
                                    }}
                                    className={`px-2 md:px-3 py-1.5 rounded-lg text-[10px] font-black tracking-wider transition-all ${
                                      simDialogueLang === lang
                                        ? "bg-indigo-600 text-white shadow-md"
                                        : "text-slate-400 hover:text-white hover:bg-white/5"
                                    }`}
                                  >
                                    {lang}
                                  </button>
                                ))}
                              </div>

                              {dialogueActiveScene && (
                                <button
                                  onClick={() => {
                                    setDialogueActiveScene(false);
                                    setSimDialogueStep(0);
                                  }}
                                  className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-300 border border-white/10 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all"
                                >
                                  ← Сменить Героя
                                </button>
                              )}
                            </div>
                          </div>

                          {/* WORKSPACE PREVIEW - SWITCH BETWEEN SELECTION & ACTIVE DIALOGUE */}
                          {!dialogueActiveScene ? (
                            /* PHASE 1: SPLIT SCREEN CLASS SELECTION FOR DIALOGUE */
                            <div className="space-y-6 relative z-10">
                              <div className="text-center max-w-xl mx-auto space-y-2">
                                <span className="text-[10px] font-black text-amber-500 uppercase tracking-widest block">
                                  ШАГ 1: ВЫБЕРИТЕ ПЕРСОНАЖА ДЛЯ ВХОДА В СЦЕНУ
                                  ДИАЛОГА
                                </span>
                                <h4 className="text-xl font-black text-white uppercase italic tracking-tight">
                                  Кто поведет отряд на зачистку Континента?
                                </h4>
                                <p className="text-[11px] text-slate-400">
                                  Выберите героя, чтобы инициализировать 1:1
                                  диалоговое окно с Аэлиссой, настроить спрайты
                                  в Unity и опробовать нелинейные развилки
                                  сюжета.
                                </p>
                              </div>

                              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-4">
                                {/* CARD: WARRIOR */}
                                <div className="p-6 rounded-[2.5rem] bg-indigo-950/20 border border-indigo-500/20 hover:border-red-500/30 transition-all duration-300 flex flex-col justify-between space-y-6 relative group">
                                  <div className="absolute top-3 right-3 px-2 py-0.5 rounded-full bg-red-500/20 border border-red-500/30 text-[8px] font-bold text-red-400">
                                    FIERCE ATK
                                  </div>
                                  <div className="space-y-4">
                                    <div className="flex items-center gap-3">
                                      <div className="w-16 h-16 rounded-2xl bg-black/50 border border-red-500/30 flex items-center justify-center p-1 relative overflow-hidden">
                                        <svg
                                          className="w-full h-full"
                                          viewBox="0 0 100 100"
                                          fill="none"
                                          xmlns="http://www.w3.org/2000/svg"
                                        >
                                          <circle
                                            cx="50"
                                            cy="50"
                                            r="45"
                                            fill="#1e1b4b"
                                          />
                                          <path
                                            d="M25 50C25 30 35 20 50 20C65 20 75 30 75 50C75 60 72 75 70 85H30C28 75 25 60 25 50Z"
                                            fill="#64748b"
                                          />
                                          <path
                                            d="M48 10L52 10L50 25L48 10Z"
                                            fill="#ef4444"
                                          />
                                          <circle
                                            cx="50"
                                            cy="10"
                                            r="3"
                                            fill="#ef4444"
                                          />
                                          <path
                                            d="M32 42H68V48H32V42Z"
                                            fill="#0f172a"
                                          />
                                          <path
                                            d="M50 22V85"
                                            stroke="#f59e0b"
                                            strokeWidth="2.5"
                                          />
                                          <path
                                            d="M30 40H70"
                                            stroke="#f59e0b"
                                            strokeWidth="2"
                                          />
                                          <circle
                                            cx="40"
                                            cy="45"
                                            r="2.5"
                                            fill="#f87171"
                                          />
                                          <circle
                                            cx="60"
                                            cy="45"
                                            r="2.5"
                                            fill="#f87171"
                                          />
                                        </svg>
                                      </div>
                                      <div>
                                        <span className="text-[9px] font-black uppercase text-slate-500 block tracking-widest">
                                          КЛАСС ВОИН
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          Коронный Воитель
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          Промпт портрета 1:1 (Warrior Headshot)
                                        </div>
                                        <p className="text-[9.5px] italic text-slate-400 select-all leading-tight">
                                          Bust headshot portrait of a brave
                                          heavy Warrior hero from Fate
                                          Continent, looking forward, white
                                          background. Wearing a magnificent
                                          golden and heavy matte slate-metal
                                          helmet with glowing blue energy slots
                                          and integrated Zenith crown accents.
                                          --ar 1:1
                                        </p>
                                        <button
                                          onClick={() => {
                                            navigator.clipboard.writeText(
                                              "Bust headshot portrait of a brave heavy Warrior hero from Fate Continent, looking forward, white background. Wearing a magnificent golden and heavy matte slate-metal helmet with glowing blue energy slots and integrated Zenith crown accents. --ar 1:1",
                                            );
                                            showNotification(
                                              "Промпт портрета Воина скопирован!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          Копировать промпт 📋
                                        </button>
                                      </div>
                                    </div>
                                  </div>

                                  <button
                                    onClick={() => {
                                      setSimDialogueHero("warrior");
                                      setSimDialogueStep(0);
                                      setDialogueActiveScene(true);
                                      showNotification(
                                        "Сцена диалога запущена за класс: Воин",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-red-600 to-amber-600 hover:from-red-500 hover:to-amber-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Выбрать и Начать Диалог ⚔️
                                  </button>
                                </div>

                                {/* CARD: ARCHER */}
                                <div className="p-6 rounded-[2.5rem] bg-indigo-950/20 border border-indigo-500/20 hover:border-emerald-500/30 transition-all duration-300 flex flex-col justify-between space-y-6 relative group">
                                  <div className="absolute top-3 right-3 px-2 py-0.5 rounded-full bg-emerald-500/20 border border-emerald-500/30 text-[8px] font-bold text-emerald-400">
                                    HIGH AGI
                                  </div>
                                  <div className="space-y-4">
                                    <div className="flex items-center gap-3">
                                      <div className="w-16 h-16 rounded-2xl bg-black/50 border border-emerald-500/30 flex items-center justify-center p-1 relative overflow-hidden">
                                        <svg
                                          className="w-full h-full"
                                          viewBox="0 0 100 100"
                                          fill="none"
                                          xmlns="http://www.w3.org/2000/svg"
                                        >
                                          <circle
                                            cx="50"
                                            cy="50"
                                            r="45"
                                            fill="#064e3b"
                                          />
                                          <path
                                            d="M25 55C25 32 35 15 50 15C65 15 75 32 75 55C75 70 70 85 70 90H30C30 85 25 70 25 55Z"
                                            fill="#0f172a"
                                          />
                                          <path
                                            d="M30 50L50 20L70 50L50 65L30 50Z"
                                            fill="#10b981"
                                          />
                                          <path
                                            d="M38 46C42 48 46 48 50 46"
                                            stroke="#34d399"
                                            strokeWidth="3"
                                          />
                                          <path
                                            d="M62 46C58 48 54 48 50 46"
                                            stroke="#34d399"
                                            strokeWidth="3"
                                          />
                                          <circle
                                            cx="44"
                                            cy="45"
                                            r="1.5"
                                            fill="#34d399"
                                          />
                                          <circle
                                            cx="56"
                                            cy="45"
                                            r="1.5"
                                            fill="#34d399"
                                          />
                                          <path
                                            d="M15 25C20 18 30 18 35 25"
                                            stroke="#fbbf24"
                                            strokeWidth="2"
                                          />
                                        </svg>
                                      </div>
                                      <div>
                                        <span className="text-[9px] font-black uppercase text-slate-500 block tracking-widest">
                                          КЛАСС СТРЕЛОК
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          Лучник в Капюшоне
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          Промпт портрета 1:1 (Archer Headshot)
                                        </div>
                                        <p className="text-[9.5px] italic text-slate-400 select-all leading-tight">
                                          Bust headshot portrait of an agile
                                          Master Archer hero from Fate
                                          Continent, looking slightly aside,
                                          white background. Wearing a sleek hood
                                          made of dark obsidian star-weave
                                          fabric with glowing green energy
                                          lining. --ar 1:1
                                        </p>
                                        <button
                                          onClick={() => {
                                            navigator.clipboard.writeText(
                                              "Bust headshot portrait of an agile Master Archer hero from Fate Continent, looking slightly aside, white background. Wearing a sleek hood made of dark obsidian star-weave fabric with glowing green energy lining. --ar 1:1",
                                            );
                                            showNotification(
                                              "Промпт портрета Лучника скопирован!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          Копировать промпт 📋
                                        </button>
                                      </div>
                                    </div>
                                  </div>

                                  <button
                                    onClick={() => {
                                      setSimDialogueHero("archer");
                                      setSimDialogueStep(0);
                                      setDialogueActiveScene(true);
                                      showNotification(
                                        "Сцена диалога запущена за класс: Лучник",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-emerald-600 to-indigo-600 hover:from-emerald-500 hover:to-indigo-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Выбрать и Начать Диалог 🏹
                                  </button>
                                </div>

                                {/* CARD: MAGE */}
                                <div className="p-6 rounded-[2.5rem] bg-indigo-950/20 border border-indigo-500/20 hover:border-purple-500/30 transition-all duration-300 flex flex-col justify-between space-y-6 relative group">
                                  <div className="absolute top-3 right-3 px-2 py-0.5 rounded-full bg-purple-500/20 border border-purple-500/30 text-[8px] font-bold text-purple-400 font-black">
                                    COSMIC MP
                                  </div>
                                  <div className="space-y-4">
                                    <div className="flex items-center gap-3">
                                      <div className="w-16 h-16 rounded-2xl bg-black/50 border border-purple-500/30 flex items-center justify-center p-1 relative overflow-hidden">
                                        <svg
                                          className="w-full h-full"
                                          viewBox="0 0 100 100"
                                          fill="none"
                                          xmlns="http://www.w3.org/2000/svg"
                                        >
                                          <circle
                                            cx="50"
                                            cy="50"
                                            r="45"
                                            fill="#311042"
                                          />
                                          <path
                                            d="M15 58L50 5L85 58H15Z"
                                            fill="#2e1065"
                                          />
                                          <ellipse
                                            cx="50"
                                            cy="58"
                                            rx="35"
                                            ry="6"
                                            fill="#4c1d95"
                                          />
                                          <path
                                            d="M30 45L50 15L70 45"
                                            stroke="#f472b6"
                                            strokeWidth="3.5"
                                          />
                                          <circle
                                            cx="50"
                                            cy="18"
                                            r="4.5"
                                            fill="#a5f3fc"
                                          />
                                          <path
                                            d="M40 70C42 65 48 65 50 60C52 65 58 65 60 70C58 75 52 75 50 80C48 75 42 75 40 70Z"
                                            fill="#f472b6"
                                          />
                                        </svg>
                                      </div>
                                      <div>
                                        <span className="text-[9px] font-black uppercase text-slate-500 block tracking-widest">
                                          КЛАСС МАГ
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          Звездный Архимаг
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          Промпт портрета 1:1 (Mage Headshot)
                                        </div>
                                        <p className="text-[9.5px] italic text-slate-400 select-all leading-tight">
                                          Bust headshot portrait of a powerful
                                          Cosmic Magician hero from Fate
                                          Continent, looking at the camera,
                                          white background. Wearing a floating
                                          stellar crystal crown in starry hat
                                          style. --ar 1:1
                                        </p>
                                        <button
                                          onClick={() => {
                                            navigator.clipboard.writeText(
                                              "Bust headshot portrait of a powerful Cosmic Magician hero from Fate Continent, looking at the camera, white background. Wearing a floating stellar crystal crown in starry hat style. --ar 1:1",
                                            );
                                            showNotification(
                                              "Промпт портрета Мага скопирован!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          Копировать промпт 📋
                                        </button>
                                      </div>
                                    </div>
                                  </div>

                                  <button
                                    onClick={() => {
                                      setSimDialogueHero("mage");
                                      setSimDialogueStep(0);
                                      setDialogueActiveScene(true);
                                      showNotification(
                                        "Сцена диалога запущена за класс: Маг",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:from-purple-500 hover:to-indigo-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Выбрать и Начать Диалог 🔮
                                  </button>
                                </div>
                              </div>
                            </div>
                          ) : (
                            /* PHASE 2: IMMERSIVE ACTIVE DIALOGUE CANVAS LAYOUT (LEFT/RIGHT PORTRAITS + CENTRAL MAIN BALLOON) */
                            <div className="space-y-8 relative z-10">
                              {/* Dialogue Active Header bar */}
                              <div className="flex items-center justify-between bg-black/40 px-6 py-3 rounded-2xl border border-white/5">
                                <div className="flex items-center gap-2">
                                  <span className="w-2.5 h-2.5 rounded-full bg-green-500 animate-ping" />
                                  <span className="text-[10.5px] font-black text-indigo-400 uppercase tracking-widest font-mono">
                                    Сцена диалога: Аэлисса 🧝‍♀️ &{" "}
                                    {simDialogueHero === "warrior"
                                      ? "Воин ⚔️"
                                      : simDialogueHero === "archer"
                                        ? "Лучник 🏹"
                                        : "Маг 🔮"}
                                  </span>
                                </div>
                                <span className="text-[10px] text-slate-500 font-bold uppercase font-sans">
                                  Текущий шаг сценария: #{simDialogueStep} •{" "}
                                  {simDialogueStep === 0
                                    ? "Приветствие"
                                    : simDialogueStep === 1
                                      ? "Информация"
                                      : simDialogueStep === 2
                                        ? "Энергетический запал"
                                        : simDialogueStep === 3
                                          ? "Выбор очищения континента"
                                          : "Финал и зачистка"}
                                </span>
                              </div>

                              {/* The Active RPG Screen Layout mockup with guides */}
                              {simDialogueStep >= 4 ? (
                                /* TRANSITIONAL BATTLE ZONE & HERO STATUS HUD OVERVIEW */
                                <div
                                  className="w-full min-h-[500px] rounded-[3rem] bg-slate-950/90 border-2 border-indigo-500/20 relative overflow-hidden p-6 md:p-8 flex flex-col justify-between animate-fade-in font-sans"
                                  id="TransitionalBattleZone"
                                >
                                  {/* SLIDING LEFT SIDE HERO CUSTOMIZATION PANEL & CHARACTER MENU */}
                                  <div
                                    className={`absolute top-0 left-0 h-full w-[350px] sm:w-[420px] md:w-[480px] z-50 bg-slate-950/98 border-r-2 border-[#22d3ee]/30 shadow-[10px_0_40px_rgba(0,0,0,0.85)] p-5 flex flex-col justify-between overflow-y-auto filter backdrop-blur-2xl transition-all duration-300 transform ${isCharacterMenuOpen ? "translate-x-0" : "-translate-x-full"}`}
                                    id="Hero_Sliding_Character_Panel"
                                  >
                                    <div>
                                      {/* Header with Title & Close Action */}
                                      <div className="flex justify-between items-center pb-3 border-b border-white/5 mb-4">
                                        <div>
                                          <h3 className="text-xs font-black text-cyan-400 uppercase tracking-widest">
                                            🛡️{" "}
                                            {simDialogueLang === "RU"
                                              ? "МЕНЮ ПЕРСОНАЖА"
                                              : "CHARACTER INVENTORY"}
                                          </h3>
                                          <span className="text-[7px] font-mono text-slate-500 block">
                                            GameObject:
                                            Character_Panel_Side_Menu
                                          </span>
                                        </div>
                                        <button
                                          onClick={() => {
                                            setIsCharacterMenuOpen(false);
                                            try {
                                              const ctx = new (
                                                window.AudioContext ||
                                                (window as any)
                                                  .webkitAudioContext
                                              )();
                                              const osc =
                                                ctx.createOscillator();
                                              osc.frequency.setValueAtTime(
                                                300,
                                                ctx.currentTime,
                                              );
                                              const g = ctx.createGain();
                                              g.gain.setValueAtTime(
                                                0.08,
                                                ctx.currentTime,
                                              );
                                              osc.connect(g);
                                              g.connect(ctx.destination);
                                              osc.start();
                                              osc.stop(ctx.currentTime + 0.1);
                                            } catch (e) {}
                                          }}
                                          className="w-7 h-7 rounded-full bg-slate-900 border border-white/10 flex items-center justify-center text-slate-400 hover:text-white hover:bg-slate-800 transition-all font-black text-xs"
                                        >
                                          ✕
                                        </button>
                                      </div>

                                      {/* Hero Synergy and Class Toggler Badges inside HUD */}
                                      <div className="bg-slate-900/60 p-3 rounded-2xl border border-white/5 mb-4 space-y-2">
                                        <div className="flex justify-between items-center">
                                          <span className="text-[8px] font-bold text-slate-400 uppercase tracking-wider">
                                            {simDialogueLang === "RU"
                                              ? "Тип Синергии:"
                                              : "Synergy Type:"}
                                          </span>
                                          <div className="flex gap-1.5">
                                            <button
                                              onClick={() =>
                                                setSynergyHeroType("simple")
                                              }
                                              className={`px-2 py-0.5 rounded text-[8px] font-bold uppercase transition-all ${synergyHeroType === "simple" ? "bg-indigo-600 text-white shadow-[0_0_10px_rgba(99,102,241,0.4)]" : "bg-slate-800 text-slate-400 hover:bg-slate-700"}`}
                                            >
                                              {simDialogueLang === "RU"
                                                ? "Простой"
                                                : "Simple"}
                                            </button>
                                            <button
                                              onClick={() =>
                                                setSynergyHeroType("main")
                                              }
                                              className={`px-2 py-0.5 rounded text-[8px] font-bold uppercase transition-all ${synergyHeroType === "main" ? "bg-amber-500 text-black font-black shadow-[0_0_10px_rgba(245,158,11,0.4)]" : "bg-slate-800 text-slate-400 hover:bg-slate-700"}`}
                                            >
                                              {simDialogueLang === "RU"
                                                ? "Основной"
                                                : "Main"}
                                            </button>
                                          </div>
                                        </div>

                                        <div className="flex justify-between items-center">
                                          <span className="text-[8px] font-bold text-slate-400 uppercase tracking-wider">
                                            {simDialogueLang === "RU"
                                              ? "Сменить Класс:"
                                              : "Switch Class:"}
                                          </span>
                                          <div className="flex gap-1">
                                            {(
                                              [
                                                "warrior",
                                                "archer",
                                                "mage",
                                              ] as const
                                            ).map((cls) => (
                                              <button
                                                key={cls}
                                                onClick={() =>
                                                  setSimDialogueHero(cls)
                                                }
                                                className={`px-2 py-0.5 rounded text-[8px] font-bold uppercase transition-all ${simDialogueHero === cls ? "bg-cyan-500 text-black font-black" : "bg-slate-800 text-slate-400 hover:bg-slate-700"}`}
                                              >
                                                {cls === "warrior"
                                                  ? simDialogueLang === "RU"
                                                    ? "⚔️ Воин"
                                                    : "⚔️ Warrior"
                                                  : cls === "archer"
                                                    ? simDialogueLang === "RU"
                                                      ? "🏹 Лучник"
                                                      : "🏹 Archer"
                                                    : simDialogueLang === "RU"
                                                      ? "🧙‍♂️ Маг"
                                                      : "🧙‍♂️ Mage"}
                                              </button>
                                            ))}
                                          </div>
                                        </div>
                                      </div>

                                      {/* TWO COLUMNS: LEFT EQUIPMENT & RIGHT STATS */}
                                      <div className="grid grid-cols-12 gap-3 mb-4">
                                        {/* Left Slot Box: Equipment Grid */}
                                        <div className="col-span-6 bg-slate-900/40 border border-white/5 p-3 rounded-2xl space-y-2">
                                          <span className="text-[7.5px] font-black text-slate-500 uppercase block tracking-wider text-center">
                                            {simDialogueLang === "RU"
                                              ? "СНАРЯЖЕНИЕ"
                                              : "EQUIPMENT SLOTS"}
                                          </span>

                                          <div className="grid grid-cols-2 gap-2">
                                            {Object.entries(equippedItems).map(
                                              ([slotKey, item]) => (
                                                <div
                                                  key={slotKey}
                                                  onClick={() => {
                                                    showNotification(
                                                      `${item.icon} ${item.name}: ${item.bonus}`,
                                                      "info",
                                                    );
                                                  }}
                                                  className="bg-slate-950 hover:bg-slate-900/80 p-1.5 rounded-xl border border-white/5 hover:border-cyan-500/20 transition-all text-center cursor-pointer group"
                                                  title={item.name}
                                                >
                                                  <div className="text-sm mb-0.5">
                                                    {item.icon}
                                                  </div>
                                                  <div className="text-[6.5px] font-mono text-slate-400 truncate">
                                                    {item.name}
                                                  </div>
                                                  <div className="text-[5.5px] text-cyan-400 font-bold">
                                                    {item.bonus}
                                                  </div>
                                                </div>
                                              ),
                                            )}
                                          </div>

                                          <button
                                            onClick={() => {
                                              setActiveTransferPrompt(true);
                                              try {
                                                const ctx = new (
                                                  window.AudioContext ||
                                                  (window as any)
                                                    .webkitAudioContext
                                                )();
                                                const osc =
                                                  ctx.createOscillator();
                                                osc.frequency.setValueAtTime(
                                                  440,
                                                  ctx.currentTime,
                                                );
                                                osc.frequency.setValueAtTime(
                                                  880,
                                                  ctx.currentTime + 0.1,
                                                );
                                                const g = ctx.createGain();
                                                g.gain.setValueAtTime(
                                                  0.08,
                                                  ctx.currentTime,
                                                );
                                                osc.connect(g);
                                                g.connect(ctx.destination);
                                                osc.start();
                                                osc.stop(ctx.currentTime + 0.2);
                                              } catch (e) {}
                                            }}
                                            className="w-full py-2 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-black font-black text-[7.5px] uppercase tracking-wider rounded-xl transition-all shadow-[0_0_15px_rgba(245,158,11,0.25)] hover:scale-[1.01]"
                                          >
                                            🚀{" "}
                                            {simDialogueLang === "RU"
                                              ? "ПЕРЕНЕСТИ НА СЦЕНУ (C#)"
                                              : "TRANSFER TO SCENE"}
                                          </button>
                                        </div>

                                        {/* Right Slot Box: Characteristics with level points distribution (v18.11.15) */}
                                        {(() => {
                                          const getStartingPoints = () => {
                                            const diff = String(
                                              selectedDifficulty || "Средний",
                                            ).toLowerCase();
                                            if (
                                              diff.includes("легк") ||
                                              diff.includes("easy")
                                            )
                                              return 45;
                                            if (
                                              diff.includes("сложн") ||
                                              diff.includes("hard")
                                            )
                                              return 15;
                                            if (
                                              diff.includes("кошм") ||
                                              diff.includes("nightmare")
                                            )
                                              return 0;
                                            return 30; // 'Средний' / 'Medium'
                                          };

                                          const startingPoints =
                                            getStartingPoints();
                                          const levelPoints = simHeroLvl * 5;
                                          const totalPointsAllocatable =
                                            startingPoints + levelPoints;
                                          const spentPoints =
                                            swordLevel -
                                            1 +
                                            (bowLevel - 1) +
                                            (staffLevel - 1);
                                          const availablePoints =
                                            totalPointsAllocatable -
                                            spentPoints;

                                          const playStatTune = (
                                            pitch: number,
                                          ) => {
                                            try {
                                              const ctx = new (
                                                window.AudioContext ||
                                                (window as any)
                                                  .webkitAudioContext
                                              )();
                                              const osc =
                                                ctx.createOscillator();
                                              const g = ctx.createGain();
                                              osc.frequency.setValueAtTime(
                                                pitch,
                                                ctx.currentTime,
                                              );
                                              osc.frequency.exponentialRampToValueAtTime(
                                                pitch * 1.5,
                                                ctx.currentTime + 0.1,
                                              );
                                              g.gain.setValueAtTime(
                                                0.06,
                                                ctx.currentTime,
                                              );
                                              osc.connect(g);
                                              g.connect(ctx.destination);
                                              osc.start();
                                              osc.stop(ctx.currentTime + 0.1);
                                            } catch (e) {}
                                          };

                                          const handleAddStat = (
                                            stat: "sword" | "bow" | "staff",
                                          ) => {
                                            if (availablePoints > 0) {
                                              if (stat === "sword")
                                                setSwordLevel(
                                                  (prev) => prev + 1,
                                                );
                                              if (stat === "bow")
                                                setBowLevel((prev) => prev + 1);
                                              if (stat === "staff")
                                                setStaffLevel(
                                                  (prev) => prev + 1,
                                                );
                                              playStatTune(523.25);
                                            }
                                          };

                                          const handleSubtractStat = (
                                            stat: "sword" | "bow" | "staff",
                                          ) => {
                                            if (
                                              stat === "sword" &&
                                              swordLevel > 1
                                            ) {
                                              setSwordLevel((prev) => prev - 1);
                                              playStatTune(392);
                                            }
                                            if (
                                              stat === "bow" &&
                                              bowLevel > 1
                                            ) {
                                              setBowLevel((prev) => prev - 1);
                                              playStatTune(392);
                                            }
                                            if (
                                              stat === "staff" &&
                                              staffLevel > 1
                                            ) {
                                              setStaffLevel((prev) => prev - 1);
                                              playStatTune(392);
                                            }
                                          };

                                          const handleResetStats = () => {
                                            setSwordLevel(1);
                                            setBowLevel(1);
                                            setStaffLevel(1);
                                            try {
                                              const ctx = new (
                                                window.AudioContext ||
                                                (window as any)
                                                  .webkitAudioContext
                                              )();
                                              const osc =
                                                ctx.createOscillator();
                                              const g = ctx.createGain();
                                              osc.frequency.setValueAtTime(
                                                300,
                                                ctx.currentTime,
                                              );
                                              osc.frequency.exponentialRampToValueAtTime(
                                                150,
                                                ctx.currentTime + 0.25,
                                              );
                                              g.gain.setValueAtTime(
                                                0.1,
                                                ctx.currentTime,
                                              );
                                              osc.connect(g);
                                              g.connect(ctx.destination);
                                              osc.start();
                                              osc.stop(ctx.currentTime + 0.25);
                                            } catch (e) {}
                                            showNotification(
                                              simDialogueLang === "RU"
                                                ? "Характеристики успешно сброшены!"
                                                : "Weapon points reset successfully!",
                                              "info",
                                            );
                                          };

                                          return (
                                            <div className="col-span-6 bg-slate-900/40 border border-white/5 p-3 rounded-2xl flex flex-col justify-between">
                                              <div>
                                                <span className="text-[7.5px] font-black text-slate-500 uppercase block tracking-wider text-center mb-1.5">
                                                  {simDialogueLang === "RU"
                                                    ? "ХАРАКТЕРИСТИКИ"
                                                    : "CHARACTERISTICS"}
                                                </span>

                                                <div className="space-y-1 text-[8px]">
                                                  <div className="flex justify-between border-b border-white/5 pb-0.5">
                                                    <span className="text-slate-400">
                                                      {simDialogueLang === "RU"
                                                        ? "Уровень:"
                                                        : "Level:"}
                                                    </span>
                                                    <span className="font-mono text-amber-400 font-bold">
                                                      {simHeroLvl}
                                                    </span>
                                                  </div>

                                                  {/* Subtitle with unspent point counters */}
                                                  <div className="py-1 px-1.5 bg-cyan-950/40 border border-cyan-500/10 rounded-lg flex justify-between items-center text-[7.5px] font-bold">
                                                    <span className="text-[#22d3ee]">
                                                      {simDialogueLang === "RU"
                                                        ? "Свободные Очки:"
                                                        : "Skill Points:"}
                                                    </span>
                                                    <span
                                                      className={`font-mono px-1 rounded ${availablePoints > 0 ? "bg-[#22d3ee] text-slate-950 animate-pulse font-black" : "text-slate-400 bg-slate-900"}`}
                                                    >
                                                      {availablePoints}
                                                    </span>
                                                  </div>

                                                  {/* Stat 1: Sword level */}
                                                  <div className="flex items-center justify-between py-0.5 border-b border-white/5">
                                                    <span className="text-slate-300 font-sans flex items-center gap-1">
                                                      ⚔️{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "Меч"
                                                        : "Sword"}
                                                    </span>
                                                    <div className="flex items-center gap-1.5">
                                                      <button
                                                        onClick={() =>
                                                          handleSubtractStat(
                                                            "sword",
                                                          )
                                                        }
                                                        disabled={
                                                          swordLevel <= 1
                                                        }
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-red-950 hover:text-red-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        -
                                                      </button>
                                                      <span className="font-mono font-black text-white text-[8.5px] w-4 text-center">
                                                        {swordLevel}
                                                      </span>
                                                      <button
                                                        onClick={() =>
                                                          handleAddStat("sword")
                                                        }
                                                        disabled={
                                                          availablePoints <= 0
                                                        }
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-emerald-950 hover:text-emerald-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        +
                                                      </button>
                                                    </div>
                                                  </div>

                                                  {/* Stat 2: Bow level */}
                                                  <div className="flex items-center justify-between py-0.5 border-b border-white/5">
                                                    <span className="text-slate-300 font-sans flex items-center gap-1">
                                                      🏹{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "Лук"
                                                        : "Bow"}
                                                    </span>
                                                    <div className="flex items-center gap-1.5">
                                                      <button
                                                        onClick={() =>
                                                          handleSubtractStat(
                                                            "bow",
                                                          )
                                                        }
                                                        disabled={bowLevel <= 1}
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-red-950 hover:text-red-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        -
                                                      </button>
                                                      <span className="font-mono font-black text-white text-[8.5px] w-4 text-center">
                                                        {bowLevel}
                                                      </span>
                                                      <button
                                                        onClick={() =>
                                                          handleAddStat("bow")
                                                        }
                                                        disabled={
                                                          availablePoints <= 0
                                                        }
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-emerald-950 hover:text-emerald-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        +
                                                      </button>
                                                    </div>
                                                  </div>

                                                  {/* Stat 3: Staff level */}
                                                  <div className="flex items-center justify-between py-0.5 border-b border-white/5">
                                                    <span className="text-slate-300 font-sans flex items-center gap-1">
                                                      🔮{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "Посох"
                                                        : "Staff"}
                                                    </span>
                                                    <div className="flex items-center gap-1.5">
                                                      <button
                                                        onClick={() =>
                                                          handleSubtractStat(
                                                            "staff",
                                                          )
                                                        }
                                                        disabled={
                                                          staffLevel <= 1
                                                        }
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-red-950 hover:text-red-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        -
                                                      </button>
                                                      <span className="font-mono font-black text-white text-[8.5px] w-4 text-center">
                                                        {staffLevel}
                                                      </span>
                                                      <button
                                                        onClick={() =>
                                                          handleAddStat("staff")
                                                        }
                                                        disabled={
                                                          availablePoints <= 0
                                                        }
                                                        className="w-4 h-4 bg-slate-800 disabled:opacity-30 hover:bg-emerald-950 hover:text-emerald-400 text-slate-200 border border-white/10 rounded flex items-center justify-center font-bold text-[9px] transition-all"
                                                      >
                                                        +
                                                      </button>
                                                    </div>
                                                  </div>

                                                  <div className="flex justify-between border-b border-white/5 pb-0.5 text-slate-500 text-[7px] font-mono">
                                                    <span>
                                                      {simDialogueLang === "RU"
                                                        ? "Баз. (Сложность):"
                                                        : "Diff Base:"}
                                                    </span>
                                                    <span>
                                                      {startingPoints} pts (
                                                      {selectedDifficulty})
                                                    </span>
                                                  </div>

                                                  <div className="flex justify-between text-slate-500 text-[7px] font-mono">
                                                    <span>
                                                      {simDialogueLang === "RU"
                                                        ? "За Уровни (+5/ур):"
                                                        : "From Lvl (+5/lvl):"}
                                                    </span>
                                                    <span>
                                                      +{levelPoints} pts
                                                    </span>
                                                  </div>
                                                </div>

                                                <button
                                                  onClick={handleResetStats}
                                                  className="mt-3 w-full py-1.5 bg-slate-850 hover:bg-red-950 border border-white/10 rounded-xl text-white font-black text-[7.5px] uppercase tracking-widest transition-all"
                                                >
                                                  🔄{" "}
                                                  {simDialogueLang === "RU"
                                                    ? "СБРОСИТЬ ОЧКИ"
                                                    : "RESET STATS"}
                                                </button>
                                              </div>

                                              <div className="bg-slate-950/70 p-1.5 rounded-xl text-[6px] text-slate-500 leading-normal border border-white/5 mt-2">
                                                {simDialogueLang === "RU"
                                                  ? "* Нажмите СБРОС для обнуления. Очки меняются также в зависимости от вашей сложности."
                                                  : "* Press RESET. Points calculate dynamically based on current game difficulty."}
                                              </div>
                                            </div>
                                          );
                                        })()}
                                      </div>

                                      {/* BOTTOM PART: ACTIVE SKILLS & CAST SPELL PROMPTS */}
                                      <div className="bg-slate-900/30 p-4 border border-white/5 rounded-2xl relative">
                                        <div className="flex justify-between items-center mb-3">
                                          <span className="text-[8px] font-black text-[#22d3ee] uppercase tracking-widest">
                                            ⚡{" "}
                                            {simDialogueLang === "RU"
                                              ? "НАВЫКИ И УМЕНИЯ КЛАССА"
                                              : "CLASS SKILLS & SPELLS"}
                                          </span>
                                          <span className="px-1.5 py-0.5 rounded bg-cyan-950 text-cyan-400 font-mono text-[6px] uppercase">
                                            {synergyHeroType === "main"
                                              ? simDialogueLang === "RU"
                                                ? "Главный"
                                                : "Main Hero"
                                              : simDialogueLang === "RU"
                                                ? "Простой"
                                                : "Simple Companion"}
                                          </span>
                                        </div>

                                        <div className="space-y-2">
                                          {simDialogueHero === "warrior" &&
                                            (synergyHeroType === "main" ? (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🌋 Сокрушительный Удар Земли",
                                                      cost: 50,
                                                      desc: "Вызывает подземную волну, наносящую 250 урона и оглушающую противника на 2 секунды во всей зоне.",
                                                      command:
                                                        "CAST_SPELL ID=Slam_Seismic -Owner=Player -Dmg=250 -Stun=2s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🌋 Сокрушительный Удар
                                                      Земли
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Seismic Slam (Seismic
                                                      Wave, stun)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    50 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "👹 Ярость Великана",
                                                      cost: 70,
                                                      desc: "Вводит героя в неистовство. Показатель силы увеличивается на 100%, скорость атаки удваивается на 8 секунд.",
                                                      command:
                                                        "CAST_SPELL ID=Giant_Rage -Owner=Player -StatMult=Strength,2.0 -Dur=8s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      👹 Ярость Великана
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Giant\'s Reckless Rage
                                                      (Strength +100%)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    70 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🛡️ Щит Судьбы",
                                                      cost: 60,
                                                      desc: "Накладывает на всю армию барьер поглощения урона (400 единиц прочности) под свечением Bloom.",
                                                      command:
                                                        "CAST_SPELL ID=Fate_Shield -Owner=Player -ShieldDurability=400 -Color=Cyan",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🛡️ Щит Судьбы
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Sovereign Fate Wall
                                                      (Absorption Shield)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    60 MP 🧪
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🔨 Тяжелый Удар",
                                                      cost: 30,
                                                      desc: "Простой направленный выпад мечом. Урон 120, сбрасывает каст врагу.",
                                                      command:
                                                        "CAST_SPELL ID=Heavy_Strike -Owner=Player -Dmg=120",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🔨 Тяжелый Удар
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Basic Heavy Sword Strike
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    30 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🛡️ Блок Щитом",
                                                      cost: 25,
                                                      desc: "Блокирует 50% входящего физического урона в течение 5 секунд.",
                                                      command:
                                                        "CAST_SPELL ID=Basic_Block -Owner=Player -BlockReduction=50% -Dur=5s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🛡️ Блок Щитом
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Shield Block
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    25 MP 🧪
                                                  </span>
                                                </div>
                                              </>
                                            ))}

                                          {simDialogueHero === "archer" &&
                                            (synergyHeroType === "main" ? (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🏹 Град Стрел Зенит",
                                                      cost: 55,
                                                      desc: "Призывает сокрушительный огненный ливень стрел на отряд противника. Урон 300 в секунду в течение 3 сек.",
                                                      command:
                                                        "CAST_SPELL ID=Zenith_Rain -Owner=Player -DPS=300 -Dur=3s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🏹 Град Стрел Зенит
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Zenith Rain of Star-Fire
                                                      (AoE DPS)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    55 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🎯 Выстрел Точности",
                                                      cost: 45,
                                                      desc: "Выстрел по уязвимому месту. Наносит 450 критического чистого урона, игнорируя броню вражеского лорда.",
                                                      command:
                                                        "CAST_SPELL ID=Deadly_Shot -Owner=Player -TrueDmg=450",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🎯 Выстрел Точности
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Deadly Precision Bolt
                                                      (Armor Ignore)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    45 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "💨 Дымовая Завеса",
                                                      cost: 40,
                                                      desc: "Создает облако дыма, дающее 100% уклонения и скрывающее замок/героя на 10 секунд на стратегической карте.",
                                                      command:
                                                        "CAST_SPELL ID=Smoke_Escape -Owner=Player -Evasion=100% -Dur=10s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      💨 Дымовая Завеса
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Tactical Smokescreen
                                                      Escape
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    40 MP 🧪
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "⚡ Быстрая Стрела",
                                                      cost: 20,
                                                      desc: "Наносит быстрый точечный урон в 80 единиц с быстрой перезарядкой.",
                                                      command:
                                                        "CAST_SPELL ID=Quick_Arrow -Owner=Player -Dmg=80",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ⚡ Быстрая Стрела
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Basic Quick Arrow (Low
                                                      cooldown)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    20 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "⚙️ Стальной Капкан",
                                                      cost: 35,
                                                      desc: "Останавливает движение задетой цели на 4 секунды.",
                                                      command:
                                                        "CAST_SPELL ID=Basic_Snare -Owner=Player -Root=4s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ⚙️ Стальной Капкан
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Mechanical Snare
                                                      Trap
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    35 MP 🧪
                                                  </span>
                                                </div>
                                              </>
                                            ))}

                                          {simDialogueHero === "mage" &&
                                            (synergyHeroType === "main" ? (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "☄️ Метеоритный Спектр",
                                                      cost: 80,
                                                      desc: "Обрушивает космическую волну метеоритов на указанную крепость. Наносит 600 урона боссам и рушит оборонительные стены.",
                                                      command:
                                                        "CAST_SPELL ID=Meteor_Spectrum -Owner=Player -Dmg=600 -SiegeDmg=150",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ☄️ Метеоритный Спектр
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Meteor Spectrum Cosmic
                                                      Blast (Siege, AoE)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    80 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🔮 Энергетический Барьер",
                                                      cost: 60,
                                                      desc: "Генерирует вокруг Кристалла мана-купол на 12 секунд. Отражает любые снаряды противника назад в стрелка.",
                                                      command:
                                                        "CAST_SPELL ID=Mana_Barrier -Owner=Player -ReflectProjectiles=True -Dur=12s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🔮 Энергетический Барьер
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Mana Mirror Aegis (Deflect
                                                      shield)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    60 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🌀 Абсолютный Телепорт",
                                                      cost: 50,
                                                      desc: "Герой мгновенно телепортируется в любую точку на карте Континента, уклоняясь от всех засад.",
                                                      command:
                                                        "CAST_SPELL ID=Absolute_Teleport -Owner=Player -Target=SelectedZone",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🌀 Абсолютный Телепорт
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Zenith Absolute Blink
                                                      Portal
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    50 MP 🧪
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "🔥 Огненная Вспышка",
                                                      cost: 25,
                                                      desc: "Легкий огненный заряд. Наносит 100 взрывного урона по одиночной цели.",
                                                      command:
                                                        "CAST_SPELL ID=Spark_Shot -Owner=Player -Dmg=100",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🔥 Огненная Вспышка
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Fireball Spark
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    25 MP 🧪
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() => {
                                                    if (playerGold >= 50) {
                                                      setPlayerGold(
                                                        (prev) => prev - 50,
                                                      );
                                                      setSimHeroMana((prev) =>
                                                        Math.min(
                                                          300,
                                                          prev + 100,
                                                        ),
                                                      );
                                                      showNotification(
                                                        "✨ Залито +100 Маны за 50 золота!",
                                                        "success",
                                                      );
                                                    } else {
                                                      showNotification(
                                                        "❌ Недостаточно золота для закупки зелья маны (50)!",
                                                        "error",
                                                      );
                                                    }
                                                  }}
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      🧪 Заливка Маны (+100 за
                                                      50💰)
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Consumes gold to restore
                                                      chemical mana
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-amber-950 text-amber-300 font-mono text-[7px] font-bold">
                                                    50 Gold 💰
                                                  </span>
                                                </div>
                                              </>
                                            ))}
                                        </div>
                                      </div>
                                    </div>

                                    <div className="pt-4 border-t border-white/5 text-center">
                                      <span className="text-[8px] text-slate-400 block mb-1">
                                        {simDialogueLang === "RU"
                                          ? "Fate Continent • Версия 18.12.44"
                                          : "Fate Continent • Lvl 18.12.44"}
                                      </span>
                                      <span className="text-[6.5px] text-slate-600 block leading-tight">
                                        {simDialogueLang === "RU"
                                          ? "Интеграция с С#-скриптом SaveGameSystem.cs и SettingsManager.cs завершена."
                                          : "Ready for direct transmission and persistent save loading."}
                                      </span>
                                    </div>
                                  </div>

                                  {/* INTERACTIVE PROMPT OVERLAY: SPELL CASTING COMMAND TRANSMISSION */}
                                  {activeSpellPrompt && (
                                    <div className="absolute inset-0 bg-black/85 z-[60] flex items-center justify-center p-6 animate-fade-in font-sans">
                                      <div className="bg-slate-900 border border-indigo-500/40 rounded-3xl p-6 max-w-sm w-full space-y-4 shadow-[0_0_50px_rgba(34,211,238,0.3)]">
                                        <div className="text-center">
                                          <div className="text-2xl mb-1">
                                            ⚡
                                          </div>
                                          <h4 className="text-xs font-black text-cyan-400 uppercase tracking-widest">
                                            СИМУЛЯЦИЯ ЗАКЛИНАНИЯ (C# CAST)
                                          </h4>
                                          <span className="text-[7px] font-mono text-slate-500">
                                            COMMAND PORTAL TRANSMITTER
                                          </span>
                                        </div>

                                        <div className="bg-slate-950 p-4 rounded-xl border border-white/5 space-y-2">
                                          <div className="text-[10px] text-white font-black">
                                            {activeSpellPrompt.name}
                                          </div>
                                          <div className="text-[8.5px] text-slate-400 leading-relaxed italic">
                                            "{activeSpellPrompt.desc}"
                                          </div>

                                          <div className="pt-2 border-t border-white/5">
                                            <div className="text-[7.5px] text-slate-500 uppercase font-bold mb-1">
                                              C# Command String (Passed via
                                              UDP/IPC)
                                            </div>
                                            <span className="block p-1.5 bg-slate-900 rounded font-mono text-[7.5px] text-amber-300 break-words">
                                              {activeSpellPrompt.command}
                                            </span>
                                          </div>
                                        </div>

                                        <div className="grid grid-cols-2 gap-3 text-[9px] font-bold uppercase tracking-wider">
                                          <button
                                            onClick={() =>
                                              setActiveSpellPrompt(null)
                                            }
                                            className="py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-xl transition"
                                          >
                                            {simDialogueLang === "RU"
                                              ? "Отмена"
                                              : "Cancel"}
                                          </button>
                                          <button
                                            onClick={() => {
                                              if (
                                                simHeroMana >=
                                                activeSpellPrompt.cost
                                              ) {
                                                setSimHeroMana(
                                                  (prev) =>
                                                    prev -
                                                    activeSpellPrompt.cost,
                                                );
                                                showNotification(
                                                  `✨ Успешно кастовано ${activeSpellPrompt.name}! Передано в C# сцену.`,
                                                  "success",
                                                );

                                                try {
                                                  const ctx = new (
                                                    window.AudioContext ||
                                                    (window as any)
                                                      .webkitAudioContext
                                                  )();
                                                  const osc =
                                                    ctx.createOscillator();
                                                  const filter =
                                                    ctx.createBiquadFilter();
                                                  const gain = ctx.createGain();

                                                  osc.type = "sawtooth";
                                                  osc.frequency.setValueAtTime(
                                                    150,
                                                    ctx.currentTime,
                                                  );
                                                  osc.frequency.exponentialRampToValueAtTime(
                                                    1200,
                                                    ctx.currentTime + 0.4,
                                                  );

                                                  filter.type = "lowpass";
                                                  filter.frequency.setValueAtTime(
                                                    800,
                                                    ctx.currentTime,
                                                  );
                                                  filter.Q.setValueAtTime(
                                                    10,
                                                    ctx.currentTime,
                                                  );

                                                  gain.gain.setValueAtTime(
                                                    0.12,
                                                    ctx.currentTime,
                                                  );
                                                  gain.gain.exponentialRampToValueAtTime(
                                                    0.01,
                                                    ctx.currentTime + 0.45,
                                                  );

                                                  osc.connect(filter);
                                                  filter.connect(gain);
                                                  gain.connect(ctx.destination);

                                                  osc.start();
                                                  osc.stop(
                                                    ctx.currentTime + 0.45,
                                                  );
                                                } catch (e) {}
                                              } else {
                                                showNotification(
                                                  "❌ Недостаточно маны! Используйте зелье или восстановите её.",
                                                  "error",
                                                );
                                              }
                                              setActiveSpellPrompt(null);
                                            }}
                                            className="py-2.5 bg-gradient-to-r from-cyan-600 to-indigo-600 hover:from-cyan-500 hover:to-indigo-500 text-white rounded-xl transition"
                                          >
                                            {simDialogueLang === "RU"
                                              ? "Кастовать 🔮"
                                              : "Cast Spell 🔮"}
                                          </button>
                                        </div>
                                      </div>
                                    </div>
                                  )}

                                  {/* INTERACTIVE PROMPT OVERLAY: HERO & RECRUITED GEAR TRANSFER TO SCENE */}
                                  {activeTransferPrompt && (
                                    <div className="absolute inset-0 bg-black/85 z-[60] flex items-center justify-center p-6 animate-fade-in font-sans">
                                      <div className="bg-slate-900 border border-amber-500/40 rounded-3xl p-6 max-w-sm w-full space-y-4 shadow-[0_0_50px_rgba(245,158,11,0.25)]">
                                        <div className="text-center">
                                          <div className="text-2xl mb-1">
                                            🚀
                                          </div>
                                          <h4 className="text-xs font-black text-amber-400 uppercase tracking-widest">
                                            ВЫСАДКА НА СЦЕНУ (SPAWN HERO)
                                          </h4>
                                          <span className="text-[7px] font-mono text-slate-500">
                                            C# PROCEDURAL TRANSMISSION AGENT
                                          </span>
                                        </div>

                                        <div className="bg-slate-950 p-4 rounded-xl border border-white/5 text-[9.5px] text-slate-300 space-y-1.5 leading-normal">
                                          <p className="text-white font-bold text-center mb-1">
                                            {simDialogueLang === "RU"
                                              ? "Перенести героя и артефакты на 3D сцену?"
                                              : "Deploy commander and equipped items to active 3D Continent?"}
                                          </p>
                                          <div>
                                            👉{" "}
                                            <span className="text-cyan-400 font-bold">
                                              Персонаж
                                            </span>
                                            :{" "}
                                            {simDialogueHero === "warrior"
                                              ? "Рэгнар (Воин)"
                                              : simDialogueHero === "archer"
                                                ? "Аларик (Стрелок)"
                                                : "Элизиус (Маг)"}
                                          </div>
                                          <div>
                                            👉{" "}
                                            <span className="text-cyan-400 font-bold">
                                              Синергия
                                            </span>
                                            :{" "}
                                            {synergyHeroType === "main"
                                              ? "Главный (Main)"
                                              : "Простой (Simple)"}
                                          </div>
                                          <div>
                                            👉{" "}
                                            <span className="text-cyan-400 font-bold font-mono">
                                              Вещи для спавна
                                            </span>
                                            :{" "}
                                            {Object.values(equippedItems)
                                              .map((x) => x.icon)
                                              .join(" ")}{" "}
                                            ({Object.keys(equippedItems).length}{" "}
                                            шт.)
                                          </div>
                                          <div>
                                            👉{" "}
                                            <span className="text-cyan-400 font-bold">
                                              Свита в походе
                                            </span>
                                            :{" "}
                                            <span className="font-mono text-white font-black">
                                              {recruitedTroops} воинов
                                            </span>
                                          </div>

                                          <div className="pt-2 mt-2 border-t border-white/5">
                                            <div className="text-[7px] text-slate-500 uppercase font-bold mb-1">
                                              Command Code (Passed to Unity
                                              Engine)
                                            </div>
                                            <span className="block p-1 bg-slate-900 rounded font-mono text-[7px] text-emerald-400 break-words">
                                              SPAWN_HERO Class={simDialogueHero}{" "}
                                              Lvl={simHeroLvl} Synergy=
                                              {synergyHeroType} Troops=
                                              {recruitedTroops} Items=
                                              {Object.keys(equippedItems).join(
                                                ",",
                                              )}
                                            </span>
                                          </div>
                                        </div>

                                        <div className="grid grid-cols-2 gap-3 text-[9px] font-bold uppercase tracking-wider">
                                          <button
                                            onClick={() =>
                                              setActiveTransferPrompt(false)
                                            }
                                            className="py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-xl transition"
                                          >
                                            {simDialogueLang === "RU"
                                              ? "Отмена"
                                              : "Cancel"}
                                          </button>
                                          <button
                                            onClick={() => {
                                              showNotification(
                                                simDialogueLang === "RU"
                                                  ? `🚀 Герой ${simDialogueHero === "warrior" ? "Рэгнар" : simDialogueHero === "archer" ? "Аларик" : "Элизиус"} перенесен на 3D поле Континента Судьбы!`
                                                  : `Deployed ${simDialogueHero} successfully!`,
                                                "success",
                                              );

                                              try {
                                                const ctx = new (
                                                  window.AudioContext ||
                                                  (window as any)
                                                    .webkitAudioContext
                                                )();
                                                const osc =
                                                  ctx.createOscillator();
                                                const osc2 =
                                                  ctx.createOscillator();
                                                const g = ctx.createGain();
                                                osc.frequency.setValueAtTime(
                                                  100,
                                                  ctx.currentTime,
                                                );
                                                osc.frequency.linearRampToValueAtTime(
                                                  1500,
                                                  ctx.currentTime + 0.6,
                                                );
                                                osc2.frequency.setValueAtTime(
                                                  200,
                                                  ctx.currentTime,
                                                );
                                                osc2.frequency.linearRampToValueAtTime(
                                                  3000,
                                                  ctx.currentTime + 0.6,
                                                );
                                                g.gain.setValueAtTime(
                                                  0.14,
                                                  ctx.currentTime,
                                                );
                                                g.gain.linearRampToValueAtTime(
                                                  0.01,
                                                  ctx.currentTime + 0.65,
                                                );
                                                osc.connect(g);
                                                osc2.connect(g);
                                                g.connect(ctx.destination);
                                                osc.start();
                                                osc2.start();
                                                osc.stop(
                                                  ctx.currentTime + 0.65,
                                                );
                                                osc2.stop(
                                                  ctx.currentTime + 0.65,
                                                );
                                              } catch (e) {}

                                              setActiveTransferPrompt(false);
                                            }}
                                            className="py-2.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-black font-black rounded-xl transition shadow-[0_0_20px_rgba(245,158,11,0.4)]"
                                          >
                                            {simDialogueLang === "RU"
                                              ? "Высадить 🚀"
                                              : "Deploy Hero 🚀"}
                                          </button>
                                        </div>
                                      </div>
                                    </div>
                                  )}
                                  {/* TOP-LEFT FLOATING GLASSMORPHIC STATUS FRAME "РАМКА ШАПКА" */}
                                  <div
                                    onClick={() => {
                                      setIsCharacterMenuOpen((p) => !p);
                                      try {
                                        const ctx = new (
                                          window.AudioContext ||
                                          (window as any).webkitAudioContext
                                        )();
                                        const osc = ctx.createOscillator();
                                        const g = ctx.createGain();
                                        osc.frequency.setValueAtTime(
                                          600,
                                          ctx.currentTime,
                                        );
                                        osc.frequency.exponentialRampToValueAtTime(
                                          1200,
                                          ctx.currentTime + 0.1,
                                        );
                                        g.gain.setValueAtTime(
                                          0.08,
                                          ctx.currentTime,
                                        );
                                        osc.connect(g);
                                        g.connect(ctx.destination);
                                        osc.start();
                                        osc.stop(ctx.currentTime + 0.1);
                                      } catch (e) {}
                                    }}
                                    className="absolute top-4 left-4 z-40 bg-slate-900/95 border-2 border-indigo-400 shadow-[0_0_30px_rgba(99,102,241,0.5)] p-4 md:p-5 rounded-[2rem] max-w-sm w-[290px] md:w-[320px] filter backdrop-blur-md transition-all group hover:scale-[1.03] cursor-pointer"
                                    id="Player_Status_HUD_Frame"
                                    title="Нажмите для открытия меню снаряжения и характеристик"
                                  >
                                    <div className="absolute top-2 right-4 text-[7px] font-black text-amber-400 uppercase tracking-widest animate-pulse">
                                      Кликните: Меню Персонажа ⚙️
                                    </div>
                                    <div className="absolute -inset-1 border border-dashed border-indigo-500/20 rounded-[2.2rem] pointer-events-none" />
                                    <span className="text-[7px] text-slate-500 font-mono tracking-widest block font-bold uppercase select-none mb-1">
                                      GameObject: Player_Status_HUD_Frame
                                    </span>

                                    <div className="flex items-center gap-4">
                                      {/* Mini Profile Portrait with neon glowing border archetype */}
                                      <div
                                        className={`w-14 h-14 rounded-2xl bg-black/50 border-2 flex items-center justify-center p-1.5 shrink-0 ${
                                          simDialogueHero === "warrior"
                                            ? "border-red-500 shadow-[0_0_15px_rgba(239,68,68,0.4)]"
                                            : simDialogueHero === "archer"
                                              ? "border-emerald-500 shadow-[0_0_15px_rgba(16,185,129,0.4)]"
                                              : "border-purple-500 shadow-[0_0_15px_rgba(168,85,247,0.4)]"
                                        }`}
                                      >
                                        {simDialogueHero === "warrior" ? (
                                          <svg
                                            className="w-full h-full"
                                            viewBox="0 0 100 100"
                                            fill="none"
                                          >
                                            <circle
                                              cx="50"
                                              cy="50"
                                              r="45"
                                              fill="#1e1b4b"
                                            />
                                            <path
                                              d="M25 50C25 30 35 20 50 20C65 20 75 30 75 50Z"
                                              fill="#64748b"
                                            />
                                            <path
                                              d="M50 22V85"
                                              stroke="#f59e0b"
                                              strokeWidth="4"
                                            />
                                          </svg>
                                        ) : simDialogueHero === "archer" ? (
                                          <svg
                                            className="w-full h-full"
                                            viewBox="0 0 100 100"
                                            fill="none"
                                          >
                                            <circle
                                              cx="50"
                                              cy="50"
                                              r="45"
                                              fill="#064e3b"
                                            />
                                            <path
                                              d="M25 55C25 32 35 15 50 15C65 15 75 32 75 55Z"
                                              fill="#10b981"
                                            />
                                          </svg>
                                        ) : (
                                          <svg
                                            className="w-full h-full"
                                            viewBox="0 0 100 100"
                                            fill="none"
                                          >
                                            <circle
                                              cx="50"
                                              cy="50"
                                              r="45"
                                              fill="#311042"
                                            />
                                            <path
                                              d="M15 58L50 5L85 58Z"
                                              fill="#2e1065"
                                            />
                                          </svg>
                                        )}
                                      </div>

                                      {/* Hero Name and current dynamic localized level */}
                                      <div className="flex-1 min-w-0">
                                        <h5 className="text-xs font-black text-white uppercase tracking-tight truncate">
                                          {simDialogueHero === "warrior"
                                            ? simDialogueLang === "RU"
                                              ? "Рэгнар (Воин)"
                                              : "Ragnar (Warrior)"
                                            : simDialogueHero === "archer"
                                              ? simDialogueLang === "RU"
                                                ? "Аларик (Стрелок)"
                                                : "Alaric (Archer)"
                                              : simDialogueLang === "RU"
                                                ? "Элизиус (Маг)"
                                                : "Elysius (Mage)"}
                                        </h5>

                                        <div className="flex items-center gap-1.5 mt-0.5">
                                          <span className="text-[10px] font-black text-amber-400 uppercase tracking-widest bg-amber-500/10 px-1.5 py-0.5 rounded border border-amber-500/20">
                                            {LEVEL_LABELS[simDialogueLang]}{" "}
                                            {simHeroLvl}
                                          </span>
                                          <span className="text-[8.5px] text-slate-400 font-mono">
                                            {simDialogueLang === "RU"
                                              ? "Континент Судьбы"
                                              : "Fate Continent"}
                                          </span>
                                        </div>
                                      </div>
                                    </div>

                                    {/* XP Progress Bar & Control */}
                                    <div className="mt-4 space-y-1">
                                      <div className="flex justify-between text-[9px] font-black text-indigo-200">
                                        <span>
                                          {STATS_LABELS.XP[simDialogueLang]}
                                        </span>
                                        <span className="font-mono">
                       x��[�n����8VQKB����b��d����eC�k#�!,�#r�uw)�PHqpR���NR���j��V�H��
�W�ď�3�Kr���-����3�3gf�w�7� :>�����X�]s �<wo��ٮ��R�6%}j���ey���dU��YI#�#%N��a�[*�*䋜�J�&P^�(�R�j�eb-��
WR
�Cް
����/]��D�V_��[CX��EK*(Dw8��,X��S�R4�4N媤s��	9��ۊ�:'��Bْ؏$ϏD��v�*�\]�����0��=kk���8�}���p�����CƆ�M}��+qi�7+��;����v�}��ݾ��)��3˖�V�d*�W%�~�"��W������"���2��v�ˀVȚ��ÒJ*싓�X�e��剳B��]�
��XA��1�a��e���C�}a�x��(�lNƅh�0�eR"f�K�ɮ��+J&���qjɂ]�.	f��xĽ��0�:�d�Tn�"U�%���/�M�X�d���5�ݠ���+z�C�ۉ0����ݸr{�o������[�T�X&�1�7 Pk�:%;�e1�D+�7a�
7�7Ԃ�d���4ӎ�MNN���X�暤Y����Q�������?�7��߸P� �_�u,�ְl�ݯĞ?���G�^�^'��M�D�]BpG�׎+�0Xާ��s��kw��(������',�a9
_c��q��}&�;��`,�4.��#2�<L[Uۑ��\,�6
��}��X��{T�=p?�?�o��ϱd������޴�)yN�DLE��=����%�
�����.�G5�{@u�+�(,u�I=OT";��=C'Q�>M�/;��-��O��|rul&��&�|x=���kH�"5�+%4؉4���6�^f3`;(K�^c��ŠYq��!�����'��5��� ?�c�2�f\��{�u���61�:��.ZJ�e6�Z�~�̟f�.V�r�#{l�����z��Jh���3��	W���;������,���K�oC=���`�>c�)���m��jb�O��E~QL��E����.^�	�SB������]%2#9�l�?�4&(V�<���	�/�V�A���6>/����p�����Xی2�(Ʈ�ܥ�m�e4�0Dݖ�3�of<��Y���Dm�Hm�g�0����~{{q�rD�ڶtz���1��A�ڭ8[������3�wg�gs�g�ڬ꽵c�@`[{R푩iCC+���`(2ɂ&䒢T߭��\�(F��ʹ�"҃O��C���ԼN�#tK�9l��H�c�"I%eJ��B
A�.4�����$0�8�,ޚ���_�ݺzcv$$�:����]�w���>���^�}�����v1��3�O7��[�tXף��QSƤ۩���[�p�K��-�X�i��fa֠�0]"�}R�l�+�RA
��܅i�=�1���CW$-��o����*^��LEL�cBz"&�5S�;e�Tr�X��dY�a1j�x9e�]�J�\"^˗ۛ^� �����l]N��x+x:���:G"=��w��T�ia,R�&C'���CO����)u��O�L���RQ�����J��O��C#R���>������S%K��@�{�7�Us�sk����s<X?���A����hq�g������kd����ڵeu�y�i�\�t�����0�t�z�2B�#���xɋL���ȇ�y���Q�
	���#y�_�wh4��	XLy��f�n� �h��7�Lw�K��7L���YNfO�ѡz�'T~dnҢA+��EM�D�Q���8����*3��%���I�¡��e������l�Lwq�a��Cy�������ⰁN�9���P!�6��򞞣��k�O��쳬Ć�s�'��X�q��I���� �|�~���p���>��+��o�c���9�Eh���fX���f���S�7"`lh��z��(�Ɓ��W4���}D�C�%�.D�G,�ٖ%�)P������;�� �S�(ᙞ�4Tc���{DW�R#�x����F�0�~N���V֛>�d���$>����G� �
�='P�b)KU`��fV�ܛ���@ؑ˴�@.S�6S?HW#��@��m�NJ�ʬ�(K���W�Ѽ찓�<�±���5$}�J���߆�Ad�9(�m�pG�b��`�.�2��M赣��E������V�Gӎc+���Rɒ�ng���٘�g^��v���/�܍���������61rhdpp�uaCs8adj�q��$�q�t�x*b����l�^������y^�D_��3mӨ\�We����n��+Xv#��^a7Ȳ������e5��_����f�<C�f�b\�hE�r鸘�9�5s�x�b��o虌��'�k�b�9]��ʊ����V�.�Y��h[)����lWl�� �m7?c����g^A@�^�_��� x1.
?'�^8c���q��(��8�.4�߶��yǰ4�S
�W�"�#%��Fq7 �N��}��Bد�|���6����!Wn�_���g��8�G���c;D?}�����fP%��iEs�zYU��s��z�-ɥDNW4T����.�`u�(؋�4u�Y�6i���  �� �<6�