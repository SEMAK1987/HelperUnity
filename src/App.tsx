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
  const [content, setContent] = useState<string>("Ğ—Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞ° Ñ€ÑƒĞºĞ¾Ğ²Ğ¾Ğ´ÑÑ‚Ğ²Ğ°...");
  const [search, setSearch] = useState("");

  useEffect(() => {
    fetch("/GAME_HELP_GUIDE.md")
      .then((res) => res.text())
      .then((text) => setContent(text))
      .catch(() =>
        setContent("# ĞÑˆĞ¸Ğ±ĞºĞ°\nĞĞµ ÑƒĞ´Ğ°Ğ»Ğ¾ÑÑŒ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·Ğ¸Ñ‚ÑŒ GAME_HELP_GUIDE.md"),
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
            ĞŸĞ¾Ğ¼Ğ¾Ñ‰ÑŒ ĞŸĞ¾ Ğ˜Ğ³Ñ€Ğµ (Unity 6)
          </h2>
          <p className="text-xs text-slate-500 uppercase tracking-widest font-bold ml-11">
            Ğ˜Ğ½Ñ‚ĞµÑ€Ğ°ĞºÑ‚Ğ¸Ğ²Ğ½Ğ¾Ğµ Ñ€ÑƒĞºĞ¾Ğ²Ğ¾Ğ´ÑÑ‚Ğ²Ğ¾ Ğ¿Ğ¾ Ñ€Ğ°Ğ·Ñ€Ğ°Ğ±Ğ¾Ñ‚ĞºĞµ â€¢ v18.12.44
          </p>
        </div>
        <div className="flex items-center gap-4">
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
            <input
              type="text"
              placeholder="ĞŸĞ¾Ğ¸ÑĞº Ğ¿Ğ¾ Ğ´Ğ¾ĞºÑƒĞ¼ĞµĞ½Ñ‚Ğ°Ñ†Ğ¸Ğ¸..."
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
                ĞœĞ°Ğ½Ğ¸Ñ„ĞµÑÑ‚Ğ°Ñ†Ğ¸Ñ...
              </p>
              <p className="text-[8px] text-slate-500 mt-1 uppercase tracking-tighter">
                ĞĞµĞ¹Ñ€Ğ¾ÑĞµÑ‚ÑŒ Ñ€Ğ¸ÑÑƒĞµÑ‚ Ğ°ÑÑĞµÑ‚
              </p>
            </div>
          </div>
        )}

        {error ? (
          <div className="absolute inset-0 flex flex-col items-center justify-center p-6 space-y-4 bg-red-950/20 backdrop-blur-sm z-20">
            <AlertTriangle className="w-10 h-10 text-red-500" />
            <div className="text-center">
              <p className="text-[10px] text-white font-bold uppercase mb-1">
                ĞÑˆĞ¸Ğ±ĞºĞ° ÑĞ¸Ğ½Ñ‚ĞµĞ·Ğ°
              </p>
              <p className="text-[8px] text-red-400/60 uppercase max-w-[120px] mx-auto leading-relaxed">
                Ğ¡ĞµÑ€Ğ²ĞµÑ€ Ğ¿ĞµÑ€ĞµĞ³Ñ€ÑƒĞ¶ĞµĞ½. ĞŸĞ¾Ğ¿Ñ€Ğ¾Ğ±ÑƒĞ¹Ñ‚Ğµ ĞµÑ‰Ğµ Ñ€Ğ°Ğ·.
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
              ĞŸĞ¾Ğ²Ñ‚Ğ¾Ñ€Ğ¸Ñ‚ÑŒ
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
                Ğ’Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ #{res.id}
              </span>
              <div className="flex gap-2">
                <a
                  href={imageUrl}
                  download={res.filename}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="p-3 bg-white text-black rounded-2xl hover:bg-blue-600 hover:text-white transition-all transform hover:scale-110 active:scale-95 shadow-xl border border-white/10"
                  onClick={() =>
                    showNotification(`ĞŸĞ¾Ğ´Ğ³Ğ¾Ñ‚Ğ¾Ğ²ĞºĞ° Ñ„Ğ°Ğ¹Ğ»Ğ° #${res.id}...`, "info")
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

  const [language, setLanguage] = React.useState("Ğ ÑƒÑÑĞºĞ¸Ğ¹");
  const [view, setView] = React.useState<"main" | "settings">("main");
  const [volume, setVolume] = React.useState(80);
  const [music, setMusic] = React.useState(60);
  const [quality, setQuality] = React.useState("8K ULTRA");
  const [resolution, setResolution] = React.useState("3840x2160");
  const [isFullscreen, setIsFullscreen] = React.useState(false);
  const [showLanguageList, setShowLanguageList] = React.useState(false);

  const t = {
    Ğ ÑƒÑÑĞºĞ¸Ğ¹: {
      play: "Ğ˜Ğ³Ñ€Ğ°Ñ‚ÑŒ",
      settings: "ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸",
      exit: "Ğ’Ñ‹Ñ…Ğ¾Ğ´",
      back: "ĞĞ°Ğ·Ğ°Ğ´",
      volume: "Ğ—Ğ²ÑƒĞº",
      music: "ĞœÑƒĞ·Ñ‹ĞºĞ°",
      quality: "ĞšĞ°Ñ‡ĞµÑÑ‚Ğ²Ğ¾ (8K)",
      res: "Ğ Ğ°Ğ·Ñ€ĞµÑˆĞµĞ½Ğ¸Ğµ (ULTRA)",
      fs: "Ğ’ĞµÑÑŒ ÑĞºÑ€Ğ°Ğ½",
      graphics: "Ğ“Ñ€Ğ°Ñ„Ğ¸ĞºĞ°",
      lang: "Ğ¯Ğ·Ñ‹Ğº",
      help: "ĞŸĞ¾Ğ¼Ğ¾Ñ‰ÑŒ ĞŸĞ¾ Ğ˜Ğ³Ñ€Ğµ",
      capabilities: "Ğ’Ğ¾Ğ·Ğ¼Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸ Ğ˜Ğ˜",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Ğ¡Ğ¸Ğ½Ñ‚Ğ°ĞºÑĞ¸Ñ Ğ¡Ğ¸Ğ½Ğ³ÑƒĞ»ÑÑ€Ğ½Ğ¾ÑÑ‚Ğ¸",
      offline: "Ğ—Ğ°Ñ‰Ğ¸Ñ‰ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ ĞµĞ¶Ğ¸Ğ¼",
      clear: "ĞÑ‡Ğ¸ÑÑ‚Ğ¸Ñ‚ÑŒ",
      clearing: "ĞÑ‡Ğ¸ÑÑ‚ĞºĞ°...",
      thinking: "Cortex Matrix Analysis (8K)",
      synth: "Ğ¡Ğ¸Ğ½Ñ‚ĞµĞ· Ğ´Ğ°Ğ½Ğ½Ñ‹Ñ… Unity 6 & Blender 5.2",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Ğ¡ĞºĞ°Ñ‡Ğ°Ñ‚ÑŒ Ğ¤Ğ¾Ğ½ (JPG 8K)",
      q_vlow: "ĞÑ‡ĞµĞ½ÑŒ ĞĞ¸Ğ·ĞºĞ¾Ğµ",
      q_low: "ĞĞ¸Ğ·ĞºĞ¾Ğµ",
      q_med: "Ğ¡Ñ€ĞµĞ´Ğ½ĞµĞµ",
      q_high: "Ğ’Ñ‹ÑĞ¾ĞºĞ¾Ğµ",
      q_vhigh: "ĞÑ‡ĞµĞ½ÑŒ Ğ’Ñ‹ÑĞ¾ĞºĞ¾Ğµ",
      q_ultra: "Ğ£Ğ»ÑŒÑ‚Ñ€Ğ°",
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
      back: "ZurÃ¼ck",
      volume: "Ton",
      music: "Musik",
      quality: "QualitÃ¤t (8K)",
      res: "AuflÃ¶sung (ULTRA)",
      fs: "Vollbild",
      graphics: "Grafik",
      lang: "Sprache",
      help: "Spielhilfe",
      capabilities: "KI-FÃ¤higkeiten",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "SingularitÃ¤ts-Syntax",
      offline: "Gesicherter Modus",
      clear: "LÃ¶schen",
      clearing: "LÃ¶schen...",
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
    FranÃ§ais: {
      play: "Jouer",
      settings: "ParamÃ¨tres",
      exit: "Quitter",
      back: "Retour",
      volume: "Son",
      music: "Musique",
      quality: "QualitÃ© (8K)",
      res: "RÃ©solution (ULTRA)",
      fs: "Plein Ã©cran",
      graphics: "Graphisme",
      lang: "Langue",
      help: "Aide au Jeu",
      capabilities: "CapacitÃ©s de l'IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Syntaxe de SingularitÃ©",
      offline: "Mode SÃ©curisÃ©",
      clear: "Effacer",
      clearing: "Effacement...",
      thinking: "Analyse de la Matrice Cortex (8K)",
      synth: "SynthÃ¨se des donnÃ©es Unity 6 & Blender 5.2",
      proMastery: "MaÃ®trise Visuelle Menu Studio",
      downloadBg: "TÃ©lÃ©charger le Fond (JPG 8K)",
      q_vlow: "TrÃ¨s Bas",
      q_low: "Bas",
      q_med: "Moyen",
      q_high: "Haut",
      q_vhigh: "TrÃ¨s Haut",
      q_ultra: "Ultra",
    },
    EspaÃ±ol: {
      play: "Jugar",
      settings: "Ajustes",
      exit: "Salir",
      back: "Volver",
      volume: "Sonido",
      music: "MÃºsica",
      quality: "Calidad (8K)",
      res: "ResoluciÃ³n (ULTRA)",
      fs: "Pantalla completa",
      graphics: "GrÃ¡ficos",
      lang: "Idioma",
      help: "Ayuda del Juego",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxis de Singularidad",
      offline: "Modo Seguro",
      clear: "Limpiar",
      clearing: "Limpiando...",
      thinking: "AnÃ¡lisis de la Matriz Cortex (8K)",
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
    æ—¥æœ¬èª: {
      play: "ãƒ—ãƒ¬ã‚¤",
      settings: "è¨­å®š",
      exit: "çµ‚äº†",
      back: "æˆ»ã‚‹",
      volume: "éŸ³é‡",
      music: "éŸ³ä¹",
      quality: "å“è³ª (8K)",
      res: "è§£åƒåº¦ (ULTRA)",
      fs: "å…¨ç”»é¢",
      graphics: "ã‚°ãƒ©ãƒ•ã‚£ãƒƒã‚¯",
      lang: "è¨€èª",
      help: "ã‚²ãƒ¼ãƒ ãƒ˜ãƒ«ãƒ—",
      capabilities: "AIæ©Ÿèƒ½",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: ã‚ªãƒ•",
      sync: "ã‚·ãƒ³ã‚®ãƒ¥ãƒ©ãƒªãƒ†ã‚£æ§‹æ–‡",
      offline: "ã‚»ã‚­ãƒ¥ã‚¢ãƒ¢ãƒ¼ãƒ‰",
      clear: "ã‚¯ãƒªã‚¢",
      clearing: "ã‚¯ãƒªã‚¢ä¸­...",
      thinking: "çš®ì§ˆ ë§¤íŠ¸ë¦­ìŠ¤ ë¶„ì„ (8K)",
      synth: "Unity 6ã¨Blender 5.2ã®ãƒ‡ãƒ¼ã‚¿ã‚’çµ±åˆä¸­",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "èƒŒæ™¯ã‚’ãƒ€ã‚¦ãƒ³ãƒ­ãƒ¼ãƒ‰ (JPG 8K)",
      q_vlow: "éå¸¸ã«ä½ã„",
      q_low: "ä½ã„",
      q_med: "ä¸­ãã‚‰ã„",
      q_high: "é«˜ã„",
      q_vhigh: "éå¸¸ã«é«˜ã„",
      q_ultra: "ã‚¦ãƒ«ãƒˆãƒ©",
    },
    í•œêµ­ì–´: {
      play: "í”Œë ˆì´",
      settings: "ì„¤ì •",
      exit: "ë‚˜ê°€ê¸°",
      back: "ë’¤ë¡œ",
      volume: "ì†Œë¦¬",
      music: "ìŒì•…",
      quality: "í’ˆì§ˆ (8K)",
      res: "í•´ìƒë„ (ULTRA)",
      fs: "ì „ì²´ í™”ë©´",
      graphics: "ê·¸ë˜í”½",
      lang: "ì–¸ì–´",
      help: "ê²Œì„ ë„ì›€ë§",
      capabilities: "AI ëŠ¥ë ¥",
      ollama: "Ollama: í™•ì¸",
      ollamaOff: "Ollama: êº¼ì§",
      sync: "íŠ¹ì´ì  êµ¬ë¬¸",
      offline: "ë³´ì•ˆ ëª¨Ğ´",
      clear: "ì§€ìš°ê¸°",
      clearing: "ì§€ìš°ëŠ” ì¤‘...",
      thinking: "í”¼ì§ˆ ë§¤íŠ¸ë¦­ìŠ¤ ë¶„ì„ (8K)",
      synth: "Unity 6 ë° Blender 5.2 ë°ì´í„° í•©ì„± ì¤‘",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "ë°°ê²½ ë‹¤ìš´ë¡œë“œ (JPG 8K)",
      q_vlow: "ë§¤ìš° ë‚®ìŒ",
      q_low: "ë‚®ìŒ",
      q_med: "ì¤‘ê°„",
      q_high: "ë†’ìŒ",
      q_vhigh: "ë§¤ìš° ë†’ìŒ",
      q_ultra: "ìš¸íŠ¸ë¼",
    },
    ç®€ä½“ä¸­æ–‡: {
      play: "å¼€å§‹",
      settings: "è®¾ç½®",
      exit: "é€€å‡º",
      back: "è¿”å›",
      volume: "éŸ³é‡",
      music: "éŸ³ä¹",
      quality: "ç”»è´¨ (8K)",
      res: "åˆ†è¾¨ç‡ (ULTRA)",
      fs: "å…¨å±",
      graphics: "å›¾åƒ",
      lang: "è¯­è¨€",
      help: "æ¸¸æˆå¸®åŠ©",
      capabilities: "AI èƒ½åŠ›",
      ollama: "Ollama: æ­£å¸¸",
      ollamaOff: "Ollama: å…³é—­",
      sync: "å¥‡ç‚¹è¯­æ³•",
      offline: "å®‰å…¨æ¨¡å¼",
      clear: "æ¸…é™¤",
      clearing: "æ­£åœ¨æ¸…é™¤...",
      thinking: "çš®å±‚çŸ©é˜µåˆ†æ (8K)",
      synth: "ç»¼åˆ Unity 6 & Blender 5.2 æ•°æ®",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "ä¸‹è½½èƒŒæ™¯ (JPG 8K)",
      q_vlow: "æä½",
      q_low: "ä½",
      q_med: "ä¸­",
      q_high: "é«˜",
      q_vhigh: "æé«˜",
      q_ultra: "ç»ˆæ",
    },
    PortuguÃªs: {
      play: "Jogar",
      settings: "ConfiguraÃ§Ãµes",
      exit: "Sair",
      back: "Voltar",
      volume: "Som",
      music: "MÃºsica",
      quality: "Qualidade (8K)",
      res: "ResoluÃ§Ã£o (ULTRA)",
      fs: "Tela cheia",
      graphics: "GrÃ¡ficos",
      lang: "Idioma",
      help: "Ajuda do Jogo",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxe de Singularidade",
      offline: "Modo Seguro",
      clear: "Limpar",
      clearing: "Limpando...",
      thinking: "AnÃ¡lise da Matriz Cortex (8K)",
      synth: "Sintetizando dados de Unity 6 e Blender 5.2",
      proMastery: "Menu Studio Visuals Mastery",
      downloadBg: "Baixar Fundo (JPG 8K)",
      q_vlow: "Muito Baixo",
      q_low: "Baixo",
      q_med: "MÃ©dio",
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
                          "Ğ ÑƒÑÑĞºĞ¸Ğ¹",
                          "English",
                          "Deutsch",
                          "FranÃ§ais",
                          "EspaÃ±ol",
                          "PortuguÃªs",
                          "æ—¥æœ¬èª",
                          "í•œêµ­ì–´",
                          "ç®€ä½“ä¸­æ–‡",
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
    RU: "Ğ—Ğ´Ñ€Ğ°Ğ²ÑÑ‚Ğ²ÑƒĞ¹, Ğ¿ÑƒÑ‚Ğ½Ğ¸Ğº! ĞĞ°Ñˆ ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹ Ğ¿Ğ¾Ğ³Ñ€ÑƒĞ¶Ğ°ĞµÑ‚ÑÑ Ğ²Ğ¾ Ñ‚ÑŒĞ¼Ñƒ Ğ´Ñ€ĞµĞ²Ğ½ĞµĞ³Ğ¾ Ğ±ĞµĞ·Ğ²Ñ€ĞµĞ¼ĞµĞ½ÑŒÑ. Ğ¯ Ğ±ÑƒĞ´Ñƒ ÑĞ¾Ğ¿Ñ€Ğ¾Ğ²Ğ¾Ğ¶Ğ´Ğ°Ñ‚ÑŒ Ñ‚ĞµĞ±Ñ Ğ² ÑÑ‚Ğ¾Ğ¼ Ğ¾Ğ¿Ğ°ÑĞ½Ğ¾Ğ¼ Ğ¿Ğ¾Ñ…Ğ¾Ğ´Ğµ.",
    EN: "Greetings, traveler! Our Fate Continent is sinking into the darkness of ancient timelessness. I will accompany you in this dangerous journey.",
    DE: "Seid gegrÃ¼ÃŸt, Reisender! Unser Schicksalskontinent versinkt in der Dunkelheit der alten Zeitlosigkeit. Ich werde euch auf dieser gefÃ¤hrlichen Reise begleiten.",
    FR: "Salutations, voyageur ! Notre Continent du Destin sombre dans les tÃ©nÃ¨bres d'une intemporalitÃ© ancienne. Je vous accompagnerai dans ce voyage dangereux.",
    ES: "Â¡Saludos, viajero! Nuestro Continente del Destino se estÃ¡ hundiendo en la oscuridad de una atemporalidad antigua. Te acompaÃ±arÃ© en este peligroso viaje.",
    PT: "SaudaÃ§Ãµes, viajante! Nosso Continente do Destino estÃ¡ afundando na escuridÃ£o de uma atemporalidade antiga. Eu irei acompanhÃ¡-lo nesta jornada perigosa.",
    JA: "æ—…äººã‚ˆã€æŒ¨æ‹¶ã‚’ï¼ç§ãŸã¡ã®é‹å‘½ã®å¤§é™¸ã¯ã€å¤ä»£ã®æ°¸é ã®é—‡ã¸ã¨æ²ˆã¿ã¤ã¤ã‚ã‚Šã¾ã™ã€‚ã“ã®å±é™ºãªæ—…è·¯ã€ç§ãŒåŒè¡Œã—ã¾ã—ã‚‡ã†ã€‚",
    KR: "ë°˜ê°‘ë‹¤, ì—¬í–‰ìì—¬! ìš°ë¦¬ì˜ ìš´ëª… ëŒ€ë¥™ì´ ê³ ëŒ€ ë¬´í•œì˜ ì–´ë‘  ì†ìœ¼ë¡œ ì ê¸°ê³  ìˆë‹¤. ë‚´ê°€ ì´ ìœ„í—˜í•œ ì—¬ì •ì— ë™í–‰í•˜ê² ë‹¤.",
    CH: "ä½ å¥½ï¼Œæ—…äººï¼æˆ‘ä»¬çš„å‘½è¿å¤§é™†æ­£åœ¨æ²‰å…¥è¿œå¤æ— å°½çš„é»‘æš—ä¹‹ä¸­ã€‚æˆ‘å°†é™ªä¼´ä½ åº¦è¿‡è¿™æ®µå±é™©çš„æ—…ç¨‹ã€‚",
  },
  1: {
    RU: "ĞœĞµĞ½Ñ Ğ·Ğ¾Ğ²ÑƒÑ‚ ĞÑĞ»Ğ¸ÑÑĞ°, Ñ…Ñ€Ğ°Ğ½Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ğ¸Ñ†Ğ° ÑĞ²ÑÑ‰ĞµĞ½Ğ½Ğ¾Ğ³Ğ¾ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ° Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°. ĞœĞ¾Ñ Ğ¼Ğ°Ğ³Ğ¸Ñ Ğ·Ğ°Ñ‰Ğ¸Ñ‚Ğ¸Ñ‚ Ñ‚ĞµĞ±Ñ Ğ¾Ñ‚ ĞºĞ¾Ğ²Ğ°Ñ€ÑÑ‚Ğ²Ğ° ĞšÑ€Ğ¾Ğ²Ğ°Ğ²Ñ‹Ñ… ĞŸÑƒÑÑ‚Ğ¾ÑˆĞµĞ¹.",
    EN: "My name is Aelyssa, keeper of the sacred Zenith Crystal. My magic will protect you from the treachery of the Crimson Wastes.",
    DE: "Mein Name is Aelyssa, HÃ¼terin des heiligen Zenit-Kristalls. Meine Magie wird euch vor dem Verrat der Blutigen Ã–dlande schÃ¼tzen.",
    FR: "Je m'appelle Aelyssa, gardienne du cristal sacrÃ© du ZÃ©nith. Ma magie vous protÃ©gera de la trahison des Landes Sanglantes.",
    ES: "Mi nombre es Aelyssa, guardiana del sagrado Cristal Cenit. Mi magia te protegerÃ¡ de la traiciÃ³n de los PÃ¡ramos Sangrientos.",
    PT: "Meu nome Ã© Aelyssa, guardiÃ£ do sagrado Cristal Zenith. Minha magia irÃ¡ protegÃª-lo da traiÃ§Ã£o das Terras Desoladas Carmesins.",
    JA: "ç§ã¯ã‚¢ã‚¨ãƒªãƒƒã‚µã€è–ãªã‚‹ã‚¼Ğ½Ğ¸ìŠ¤í¬Ñ€Ğ¸ÑÑ‚Ğ°ãƒ«ã®å®ˆè­·è€…ã§ã™ã€‚ç§ã®é­”æ³•ãŒã€è¡€ã®è’é‡ã®é‚ªæ‚ªã‹ã‚‰ã‚ãªãŸã‚’å®ˆã‚‹ã§ã—ã‚‡ã†ã€‚",
    KR: "ë‚´ ì´ë¦„ì€ ì•¨ë¦¬ì‚¬, ì‹ ì„±í•œ ì œë‹ˆìŠ¤ í¬ë¦¬ìŠ¤íƒˆì˜ ìˆ˜í˜¸ìë‹¤. ë‚˜ì˜ ë§ˆë²•ì´ í¬ë¦¼ìŠ¨ í™©ë¬´ì§€ì˜ ë°°ì‹ ìœ¼ë¡œë¶€í„° ë‹¹ì‹ ì„ ì§€ì¼œì¤„ ê²ƒì´ë‹¤.",
    CH: "æˆ‘å«è‰¾è‰èï¼Œç¥åœ£å¤©é¡¶æ°´æ™¶çš„å®ˆæŠ¤è€…ã€‚æˆ‘çš„é­”æ³•å°†ä¿æŠ¤ä½ å…å—ç»¯çº¢è’é‡çš„èƒŒå›ã€‚",
  },
  2: {
    RU: "ĞÑ‚Ğ»Ğ¸Ñ‡Ğ½Ğ¾! Ğ¢Ğ²Ğ¾Ğµ Ğ¾Ñ€ÑƒĞ¶Ğ¸Ğµ Ğ·Ğ°Ñ€ÑĞ¶ĞµĞ½Ğ¾ ÑĞ½ĞµÑ€Ğ³Ğ¸ĞµĞ¹ Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°. Ğ”Ğ²Ğ¸Ğ½ĞµĞ¼ÑÑ Ğ²Ğ¿ĞµÑ€ĞµĞ´ Ñ‡ĞµÑ€ĞµĞ· ÑĞµĞ²ĞµÑ€Ğ½Ñ‹Ğµ Ğ²Ñ€Ğ°Ñ‚Ğ° Ğ·Ğ°Ğ¼ĞºĞ°!",
    EN: "Excellent! Your weapon is infused with Zenith energy. Let us move forward through the northern castle gates!",
    DE: "Hervorragend! Ihre Waffe ist mit Zenit-Energie erfÃ¼llt. Lasst uns durch die nÃ¶rdlichen Burgtore vorrÃ¼cken!",
    FR: "Excellent ! Votre arme est imprÃ©gnÃ©e de l'Ã©nergie du ZÃ©nith. AvanÃ§ons par les portes nord du chÃ¢teau !",
    ES: "Â¡Excelente! Tu arma estÃ¡ infundida con energÃ­a Cenit. Â¡Avancemos por las puertas del norte del castillo!",
    PT: "Excelente! Sua arma estÃ¡ infundida com empolgante energia Zenith. Vamos avanÃ§ar pelos portÃµes norte do castelo!",
    JA: "ç´ æ™´ã‚‰ã—ã„ï¼ã‚ãªãŸã®æ­¦å™¨ã«ã¯ã‚¼ãƒ‹ã‚¹ã®ãƒåŠ›ãŒæ³¨å…¥ã•ã‚Œã¾ã—ãŸã€‚åŸã®åŒ—é–€ã‚’é€šã‚Šã€å‰é€²ã—ã¾ã—ã‚‡ã†ï¼",
    KR: "í›Œë¥­í•˜ë‹¤! ë‹¹ì‹ ì˜ ë¬´ê¸°ì— ì œë‹ˆìŠ¤ ì—ë„ˆì§€ê°€ ì£¼ì…ë˜ì—ˆë‹¤. ë¶ìª½ ì„±ë¬¸ì„ í†µí•´ ì „ì§„í•˜ì!",
    CH: "å¤ªæ£’äº†ï¼ä½ çš„æ­¦å™¨è¢«æ³¨å…¥äº†å¤©é¡¶èƒ½é‡ã€‚è®©æˆ‘ä»¬ä»åŒ—é—¨ç©¿è¿‡åŸå ¡å‰è¿›å§ï¼",
  },
  3: {
    RU: "ĞŸĞ¾Ğ¼Ğ½Ğ¸: ĞºĞ°Ğ¶Ğ´Ñ‹Ğ¹ Ğ²Ñ‹Ğ±Ğ¾Ñ€ Ğ·Ğ´ĞµÑÑŒ Ğ¸Ğ¼ĞµĞµÑ‚ Ğ·Ğ½Ğ°Ñ‡ĞµĞ½Ğ¸Ğµ. ĞĞ°Ñˆ Ğ¾Ñ‚Ñ€ÑĞ´ Ğ³Ğ¾Ñ‚Ğ¾Ğ² Ğº Ğ±Ğ¾Ñ. Ğ¢ĞµĞ¿ĞµÑ€ÑŒ Ğ²Ñ‹Ğ±ĞµÑ€Ğ¸ Ğ¾Ğ±Ğ»Ğ°ÑÑ‚ÑŒ Ğ½Ğ° ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğµ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹ Ğ´Ğ»Ñ Ğ¿ĞµÑ€Ğ²Ğ¾Ğ¹ Ğ±Ğ¾ĞµĞ²Ğ¾Ğ¹ Ğ·Ğ°Ñ‡Ğ¸ÑÑ‚ĞºĞ¸:",
    EN: "Remember: every choice here has consequences. Our squad is ready. Now select a territory on the Fate Continent for the initial tactical sweep:",
    DE: "Denkt daran: Jede Entscheidung hier hat Konsequenzen. Unsere Truppe ist bereit. WÃ¤hlt nun ein Gebiet auf dem Schicksalskontinent fÃ¼r die erste taktische SÃ¤uberung aus:",
    FR: "Rappelez-vous : chaque choix ici a des consÃ©quences. Notre escouade est prÃªte. SÃ©lectionnez maintenant un territoire sur le Continent du Destin pour le nettoyage tactique initial :",
    ES: "Recuerda: cada elecciÃ³n aquÃ­ tiene consecuencias. Nuestro escuadra estÃ¡ listo. Ahora selecciona un territory en el Continente del Destino para el barrido tÃ¡ctico inicial:",
    PT: "Lembre-se: cada escolha aqui tem consequÃªncias. Nosso esquadrÃ£o estÃ¡ pronto. Agora selecione um territÃ³rio no Continent do Destino para a varredura tÃ¡tica inicial:",
    JA: "å¿˜ã‚Œãªã„ã§ãã ã•ã„ã€ã“ã“ã§ã®é¸æŠã«ã¯ã™ã¹ã¦çµæœãŒä¼´ã„ã¾ã™ã€‚ç§ãŸã¡ã®éƒ¨éšŠã¯æº–å‚™ä¸‡ç«¯ã§ã™ã€‚ã•ã‚ã€æœ€åˆã®æˆ¦è¡“çš„æƒè¨ã®ãŸã‚ã«ã€é‹å‘½ã®å¤§é™¸ã‹ã‚‰åœ°åŸŸã‚’é¸æŠã—ã¦ãã ã•ã„ï¼š",
    KR: "ê¸°ì–µí•´ë¼: ì´ê³³ì—ì„œì˜ ëª¨ë“  ì„ íƒì€ ê·¸ ê²°ê³¼ê°€ ë”°ë¥¸ë‹¤. ìš°ë¦¬ ë¶€ëŒ€ëŠ” ì „íˆ¬ ì¤€ë¹„ê°€ ëë‚¬ë‹¤. ì´ì œ ìš´ëª…ì˜ ëŒ€ë¥™ì—ì„œ ì²« ì „ìˆ ì  ì†Œíƒ•ì„ ì „ê°œí•  ì§€ì—­ì„ ì„ íƒí•´ë¼:",
    CH: "è®°ä½ï¼šè¿™é‡Œçš„æ¯ä¸€ä¸ªé€‰æ‹©éƒ½æœ‰å…¶åæœã€‚æˆ‘ä»¬çš„é˜Ÿä¼å·²å‡†å¤‡å°±ç»ªã€‚ç°åœ¨è¯·é€‰æ‹©å‘½è¿å¤§é™†ä¸Šçš„ä¸€ä¸ªåŒºåŸŸè¿›è¡Œé¦–æ¬¡æˆ˜æœ¯è‚ƒæ¸…ï¼š",
  },
  4: {
    RU: "Ğ’Ñ‹ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ»Ğ¸ ĞšÑ€Ğ¾Ğ²Ğ°Ğ²Ñ‹Ğµ ĞŸÑƒÑÑ‚Ğ¾ÑˆĞ¸! Ğ—Ğ´ĞµÑÑŒ ÑĞ¸Ğ»ÑŒĞ½Ñ‹ Ğ¾Ñ€Ğ´Ñ‹ Ğ±Ğ°Ğ½Ğ´Ğ¸Ñ‚Ğ¾Ğ² Ğ¸ Ğ°Ğ´ÑĞºĞ¸Ğµ Ğ²ĞµÑ‚Ñ€Ñ‹ Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°. Ğ”Ğ° Ğ¿Ñ€ĞµĞ±ÑƒĞ´ĞµÑ‚ Ñ Ñ‚Ğ¾Ğ±Ğ¾Ğ¹ Ğ±Ğ»Ğ°Ğ³Ğ¾ÑĞ»Ğ¾Ğ²ĞµĞ½Ğ¸Ğµ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ°! ĞœÑ‹ Ğ¾Ñ‚Ğ¿Ñ€Ğ°Ğ²Ğ»ÑĞµĞ¼ÑÑ Ğ² Ğ±Ğ¾Ğ¹.",
    EN: "You have selected the Crimson Wastes! Bandit hordes and infernal Zenith winds plague this land. May the blessing of the Crystal guide us! Charging into battle.",
    DE: "Ihr habt die Blutigen Ã–Ğ´Ğ»Ğ°Ğ½Ğ´Ğµ gewÃ¤hlt! Banditenhorden und hÃ¶llische Zenit-Winde plagen dieses Land. MÃ¶ge der Segen des Kristalls uns leiten! Wir ziehen in den Kampf.",
    FR: "Vous avez choisi les Landes Sanglantes ! Des hordes de bandits et des vents infernaux du ZÃ©nith accablent cette terre. Que la bÃ©nÃ©diction du Cristal nous guide ! En route vers la bataille.",
    ES: "Â¡Has seleccionado los PÃ¡ramos Sangrientos! Las hordas de bandidos y los vientos infernales de Cenit asolan esta tierra. Â¡Que la bendiciÃ³n del Cristal nos guÃ­e! Entrando en batalla.",
    PT: "VocÃª selecionou as Terras Desoladas Carmesins! Hordas de bandidos e ventos infernais de Zenith assolam esta terra. Que a bÃªnÃ§Ã£o do Cristal nos guie! Entrando em batalha.",
    JA: "è¡€ã®è’é‡ã‚’é¸æŠã—ã¾ã—ãŸã­ï¼å±±è³Šã®ç¾¤ã‚Œã¨è’ã‚Œç‹‚ã†ã‚¼ãƒ‹ã‚¹ã®é­”åŠ›åµãŒå¤§åœ°ã‚’è¥²ã£ã¦ã„ã¾ã™ã€‚ã‚¯ãƒªã‚¹ã‚¿ãƒ«ã®ç¥ç¦ãŒã‚ãªãŸã‚’å°ã„ã¦ãã‚Œã¾ã™ã‚ˆã†ã«ï¼æˆ¦å ´ã¸é€²æ’ƒã—ã¾ã™ã€‚",
    KR: "í¬ë¦¼ìŠ¨ í™©ë¬´ì§€ë¥¼ ì„ íƒí•˜ì…¨ìŠµë‹ˆë‹¤! ë¬´ë²•ì ë¬´ë¦¬ë“¤ê³¼ ë§¤ì„œìš´ ì œë‹ˆìŠ¤ì˜ í­í’ì´ ë¶€ëŠ” ê±°ì¹œ ëŒ€ì§€ì…ë‹ˆë‹¤. í¬ë¦¬ìŠ¤íƒˆì˜ ì¶•ë³µì´ ìš°ë¦¬ë¥¼ ì´ëŒì–´ ì£¼ê¸°ë¥¼! ì „íˆ¬ë¡œ ë‚˜ì•„ê°‘ë‹ˆë‹¤.",
    CH: "ä½ é€‰æ‹©äº†ç»¯çº¢è’é‡ï¼è¿™é‡Œå……æ»¡ç€å¼ºç›—è±ªå®¢ä¸æ— æƒ…çš„å¤©é¡¶çƒˆé£ã€‚æ„¿æ°´æ™¶çš„åº‡æŠ¤æŒ‡å¼•æˆ‘ä»¬ï¼å‰è¿›æ€å…¥æˆ˜åœºã€‚",
  },
  5: {
    RU: "Ğ’Ñ‹ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ»Ğ¸ Ğ›ĞµĞ´ÑĞ½Ğ¾Ğ¹ ĞŸĞ¸Ğº! Ğ’ĞµÑ‡Ğ½Ğ°Ñ Ğ¼ĞµÑ€Ğ·Ğ»Ğ¾Ñ‚Ğ° Ğ¸ÑĞ¿Ñ‹Ñ‚Ñ‹Ğ²Ğ°ĞµÑ‚ ÑĞ¸Ğ»ÑŒĞ½Ñ‹Ñ… Ğ´ÑƒÑ…Ğ¾Ğ¼, Ğ° Ğ³Ğ¸Ğ³Ğ°Ğ½Ñ‚ÑĞºĞ¸Ğµ Ğ“Ğ¾Ğ»ĞµĞ¼Ñ‹ Ğ›ÑŒĞ´Ğ° Ğ¾Ñ…Ñ€Ğ°Ğ½ÑÑÑ‚ Ğ´Ñ€ĞµĞ²Ğ½Ğ¸Ğµ ÑĞµĞºÑ€ĞµÑ‚Ñ‹. Ğ”Ğ° Ğ¿Ñ€ĞµĞ±ÑƒĞ´ĞµÑ‚ Ñ Ğ½Ğ°Ğ¼Ğ¸ Ğ±Ğ»Ğ°Ğ³Ğ¾ÑĞ»Ğ¾Ğ²ĞµĞ½Ğ¸Ğµ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ°!",
    EN: "You have selected the Ice-Bound Peak! Permafrost tests the strong, and giant Ice Golems guard ancient secrets. May the blessing of the Crystal protect us!",
    DE: "Ihr habt den Eisigen Gipfel gewÃ¤hlt! Permafrost prÃ¼ft die Starken, und gigantische Eisgolems bewachen uralte Geheimnisse. MÃ¶ge der Kristall uns schÃ¼tzen!",
    FR: "Vous avez choisi le Pic de Glace ! Le pergÃ©lisol met Ã  l'Ã©preuve les forts, et des Golems de glace gÃ©ants gardent des secrets anciens. Que la bÃ©nÃ©diction du Cristal nous protÃ¨ge !",
    ES: "Â¡Has seleccionado el Pico Helado! El permafrost pone a prueba a los fuertes y los Golems de Hielo gigantes custodian antiguos secretos. Â¡Que la bendiciÃ³n de Cristal nos proteja!",
    PT: "VocÃª selecionou o Pico Congelado! O permafrost testa os fortes, enquanto gigantescos Golems de Gelo protegem os tesouros antigos. Que o Cristal nos proteja!",
    JA: "æ°·çµì˜ å³°ã‚’é¸æŠã—ã¾ã—ãŸã­ï¼éé…·ãªæ°¸ä¹…å‡åœŸãŒæ„å¿—ã‚’è©¦ã—ã¦ãŠã‚Šã€å·¨å¤§ãªæ°·ã®ã‚´ãƒ¼ãƒ¬ãƒ ãŸã¡ãŒå¤ä»£ã®ç¥ç§˜ã‚’å®ˆã‚‹ãŸã‚ã«ç«‹ã¡ã¯ã ã‹ã£ã¦ã„ã¾ã™ã€‚ã‚¯ãƒªã‚¹ã‚¿ãƒ«ã®ä¿è­·ãŒã‚ã‚Šã¾ã™ã‚ˆã†ã«ï¼",
    KR: "ë¹™ì„¤ì˜ ë´‰ìš°ë¦¬ë¥¼ ì„ íƒí–ˆë‹¤! í˜¹ë…í•œ ì˜êµ¬ ë™í† ê°€ ì˜ì§€ë¥¼ ì‹œí—˜í•˜ë©°, ê±°ëŒ€í•œ ì–¼ìŒ ê³¨ë ˜ë“¤ì´ ê³ ëŒ€ì˜ ì‹ ë¹„ë¥¼ ê²½ë¹„í•˜ê³  ìˆë‹¤. í¬ë¦¬ìŠ¤íƒˆì˜ ë³´ì‚´í•Œì´ ìˆê¸°ë¥¼!",
    CH: "ä½ é€‰æ‹©äº†å†°å°ä¹‹å·…ï¼æ°¸æ’çš„æå¯’å°†è€ƒéªŒä½ çš„æ„å¿—ï¼Œè€Œå¯’å†°å·¨é­”æ­£å®ˆæŠ¤ç€å¤è€å¥‡è¿¹ã€‚æ„¿æ°´æ™¶åº‡æŠ¤æˆ‘ä»¬ï¼",
  },
  6: {
    RU: "Ğ’Ñ‹ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ»Ğ¸ Ğ”Ñ€ĞµĞ²Ğ½Ğ¸Ğµ Ğ ÑƒĞ¸Ğ½Ñ‹! Ğ—Ğ°Ğ±Ñ‹Ñ‚Ñ‹Ğµ ĞºĞ°Ñ‚Ğ°ĞºĞ¾Ğ¼Ğ±Ñ‹ Ñ…Ñ€Ğ°Ğ½ÑÑ‚ Ğ¾ÑÑ‚Ğ°Ñ‚ĞºĞ¸ Ğ´Ñ€ĞµĞ²Ğ½Ğ¸Ñ… ĞºÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ¾Ğ² Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°, Ğ½Ğ¾ Ğ±ĞµÑ€ĞµĞ³Ğ¸ÑÑŒ Ğ»Ğ¾Ğ²ÑƒÑˆĞµĞº Ğ¸ Ğ´Ñ€ĞµĞ²Ğ½Ğ¸Ñ… Ñ‚ĞµĞ½ĞµĞ¹. Ğ”Ğ° Ğ¿Ñ€ĞµĞ±ÑƒĞ´ĞµÑ‚ Ñ Ñ‚Ğ¾Ğ±Ğ¾Ğ¹ Ğ±Ğ»Ğ°Ğ³Ğ¾ÑĞ»Ğ¾Ğ²ĞµĞ½Ğ¸Ğµ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ°!",
    EN: "You have selected the Ancient Ruins! Forgotten catacombs hold remnants of ancient Zenith energy crystals, but beware deadly traps and immortal shadows. Crystal bless you!",
    DE: "Ihr habt die Alten Ruinen gewÃ¤hlt! Vergessene Katakomben bergen Reste uralter Zenit-Kristalle, aber hÃ¼tĞµÑ‚ euch vor Ñ‚Ğ¾Ğ´Ğ»Ğ¸Ñ‡Ğ½Ñ‹Ñ… Ğ»Ğ¾Ğ²ÑƒÑˆĞµĞº Ğ¸ Ğ´Ñ€ĞµĞ²Ğ½Ğ¸Ñ… Ñ‚ĞµĞ½ĞµĞ¹. MÃ¶Ğ³Ğµ der Kristall euch segnen!",
    FR: "Vous avez choisi les Ruines Anciennes ! Des catacombes oubliÃ©es recÃ¨lent des vestiges d'anciens cristaux d'Ã©nergie du ZÃ©nĞ¸Ñ‚, maar gare aux piÃ¨ges mortels et aux ombres anciennes. Que le Cristal vous bÃ©nisse !",
    ES: "Â¡Has seleccionado las Ruinas Antiguas! Catacumbas olvidadas albergan restos de los antiguos cristales de energÃ­a Cenit, pero ten cuidado con las trampas mortales y las sombras antiguas. Â¡El Cristal te bendiga!",
    PT: "VocÃª selecionou as RuÃ­nas Antigas! Catacumbas esquecidas guardam vestÃ­gios dos antigos cristais de energia Zenith, mas cuidado com armadilhas mortais e sombras imortais. Que o Cristal o abenÃ§oe!",
    JA: "å¤ä»£ã®éºè·¡ã‚’é¸æŠã—ã¾ã—ãŸã­ï¼å¿˜ã‚Œã‚‰ã‚ŒãŸåœ°ä¸‹å¢“åœ°ã«ã¯å¤ä»£ã®ã‚¼ãƒ‹ã‚¹ãƒåŠ›çµæ™¶ã®æ®‹éª¸ãŒéš ã•ã‚Œã¦ã„ã¾ã™ãŒã€è‡´å‘½çš„ãªç½ ã¨ä¸æ»…ã®å½±ã‚’è­¦æˆ’ã—ã¦ãã ã•ã„ã€‚ã‚¯ãƒªã‚¹ã‚¿ãƒ«ã®ç¥ç¦ã‚’ï¼",
    KR: "ê³ ëŒ€ ìœ ì ì§€ë¥¼ ì„ íƒí–ˆë‹¤! ìŠí˜€ì§„ ì§€í•˜ ë¬˜ì§€ì— ê³ ëŒ€ ì œë‹ˆìŠ¤ ë§ˆë ¥ ê²°ì •ì˜ ì”ì¬ê°€ ìˆ¨ê²¨ì ¸ ìˆì§€ë§Œ, ì¹˜ëª…ì ì¸ í•¨ì •ê³¼ ë¶ˆë©¸ì˜ ê·¸ë¦¼ìë¥¼ ê²½ê³„í•´ë¼. í¬ë¦¬ìŠ¤íƒˆì˜ ì¶•ë³µì„!",
    CH: "ä½ é€‰æ‹©äº†è¿œå¤é—è¿¹ï¼è¢«é—å¿˜çš„å¢“ç©´è—æœ‰è¿œå¤å¤©é¡¶èƒ½é‡æ°´æ™¶çš„ä½™çƒ¬ï¼Œä½†åŠ¡å¿…å°å¿ƒè‡´å‘½çš„é™·é˜±ä¸ä¸æ­»çš„å¹½å½±ã€‚æ„¿æ°´æ™¶èµç¦äºä½ ï¼",
  },
  7: {
    RU: "Ğ’Ñ‹ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ»Ğ¸ Ğ“Ñ€Ğ¾Ğ·Ğ¾Ğ²Ñ‹Ğµ ĞšÑ€ÑĞ¶Ğ¸! ĞĞ±Ğ»Ğ°Ñ‡Ğ½Ñ‹Ğ¹ Ğ°Ñ€Ñ…Ğ¸Ğ¿ĞµĞ»Ğ°Ğ³, Ğ¿Ğ°Ñ€ÑÑ‰Ğ¸Ğ¹ Ğ½Ğ°Ğ´ Ğ±ĞµĞ·Ğ´Ğ½Ğ¾Ğ¹. Ğ—Ğ´ĞµÑÑŒ Ğ±ÑƒÑˆÑƒÑÑ‚ Ğ¿Ğ¾ÑÑ‚Ğ¾ÑĞ½Ğ½Ñ‹Ğµ Ğ¼Ğ¾Ğ»Ğ½Ğ¸Ğ¸, Ğ° Ğ²Ğ¾Ğ·Ğ´ÑƒÑ… Ñ€Ğ°Ğ·Ğ´Ğ¸Ñ€Ğ°ÑÑ‚ ÑÑ‚Ğ¸Ñ…Ğ¸Ğ¹Ğ½Ñ‹Ğµ Ğ±ÑƒÑ€Ğ¸. Ğ”Ğ° Ğ¿Ñ€ĞµĞ±ÑƒĞ´ĞµÑ‚ Ñ Ğ½Ğ°Ğ¼Ğ¸ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»!",
    EN: "You have selected the Storm Ridges! A cloud archipelago floating over the abyss. Constant lightning storms rage here, and elemental tempests tear the air. May the Crystal protect us!",
    DE: "Ihr habt die SturmkÃ¤mme gewÃ¤hlt! Ein Wolkenarchipel, der Ã¼ber dem Abgrund schwebt. Hier wÃ¼ten stÃ¤ndige Gewitter und ElementarstÃ¼rmĞµ zerreiÃŸt die Luft. MÃ¶Ğ³Ğµ der Kristall uns schÃ¼tzen!",
    FR: "Vous avez choisi les CrÃªtes de TempÃªte ! Un archipel de nuages flottant au-dessus de l'abÃ®me. Des tempÃªtes de foudre constantes y font rage, et des tempÃªtes Ã©lÃ©mentaires dÃ©chirent l'air. Que le Cristal nous protÃ¨ge !",
    ES: "Â¡Has seleccionado las Crestas de Tormenta! Un archipiÃ©lago de nubes que flota sobre el abismo. Constantemente rugen tormentas de rayos Ğ¸ las tempestades elementales desgarran el aire. Â¡Que el Cristal nos proteja!",
    PT: "VocÃª selecionou os Cumes da Tempestade! Um arquipÃ©lago de nuvens flutuando sobre o abismo. Tempestades de raios constantes rugem aqui, e tempestades elementais rasĞ³Ğ°Ğ¼ os ar. Que o Cristal nos proteĞ¶Ğ°!",
    JA: "åµ of å°¾æ ¹ã‚’é¸æŠã—ã¾ã—ãŸã­ï¼æ·±æ·µã®ä¸Šã«æµ®ã‹ã¶é›²ã®ç¾¤å³¶ã§ã™ã€‚çµ¶ãˆé–“ãªã„é›·é›¨ãŒå¹ãè’ã‚Œã€å…ƒç´ ã®åµãŒç©ºæ°—ã‚’å¼•ãè£‚ã„ã¦ã„ã¾ã™ã€‚ã‚¯ãƒªã‚¹ã‚¿ãƒ«ã®åŠ è­·ãŒã‚ã‚Šã¾ã™ã‚ˆã†ã«ï¼",
    KR: "í­í’ ì‚°ë§¥ì„ ì„ íƒí–ˆë‹¤! ì‹¬ì—° ìœ„ì— ë–  ìˆëŠ” êµ¬ë¦„ êµ°ë„ì…ë‹ˆë‹¤. ì´ê³³ì—ëŠ” ëŠì„ì—†ëŠ” ë²ˆê°œ í­í’ì´ ì¹˜ê³  ì›ì†Œì˜ í­í’ì´ ê³µê¸°ë¥¼ ì°¢ê³  ìˆìŠµë‹ˆë‹¤. í¬ë¦¬ìŠ¤íƒˆì˜ ë³´ì‚´í•Œì´ ìˆê¸°ë¥¼!",
    CH: "ä½ é€‰æ‹©äº†é›·æš´å±±è„Šï¼æ‚¬æµ®åœ¨æ·±æ¸Šä¹‹ä¸Šçš„äº‘ä¸­ç¾¤å²›ã€‚è¿™é‡Œè‚†è™ç€è¿ç»µä¸æ–­çš„é›·æš´ï¼Œå…ƒç´ é£æš´æ’•è£‚ç€ç©ºæ°”ã€‚æ„¿æ°´æ™¶åº‡æŠ¤æˆ‘ä»¬ï¼",
  },
  8: {
    RU: "ĞŸÑ€ĞµĞºÑ€Ğ°ÑĞ½Ğ¾! ĞœÑ‹ Ğ¿Ñ€Ğ¸Ğ±Ñ‹Ğ»Ğ¸ Ğ½Ğ° Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ½Ğ½ÑƒÑ Ñ‚Ğ¾Ñ‡ĞºÑƒ ĞšĞ¾Ğ½Ñ‚ĞµĞ½Ñ‚Ğ° Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹. Ğ“Ğ»ÑĞ´Ğ¸, Ğ·Ğ´ĞµÑÑŒ Ğ·Ğ°Ğ»Ğ¾Ğ¶ĞµĞ½Ğ° Ğ½Ğ°ÑˆĞ° Ğ¿ĞµÑ€Ğ²Ğ°Ñ Ğ‘Ğ°ÑˆĞ½Ñ (1 ÑƒÑ€. â€” Ğ°Ğ²Ğ°Ğ½Ğ¿Ğ¾ÑÑ‚ Ñ ÑĞ¸Ğ³Ğ½Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¼ Ğ¾Ğ³Ğ½ĞµĞ¼). ĞĞ½Ğ° Ğ²Ñ‹Ğ³Ğ»ÑĞ´Ğ¸Ñ‚ ÑĞºÑ€Ğ¾Ğ¼Ğ½Ğ¾, Ğ½Ğ¾ ĞµÑ‘ ÑĞ¸ÑĞ½Ğ¸Ğµ Ñ€Ğ°ÑÑĞµĞ¸Ğ²Ğ°ĞµÑ‚ Ñ‚ÑŒĞ¼Ñƒ.",
    EN: "Fabulous! We arrived at your selected point. Look, here lies our first Tower (Level 1 â€” basic sentry post). It is modest for now, but its light guards us from ancient shadows.",
    DE: "Fabelhaft! We are an Ihrem ausgewÃ¤hlten Ort angekommen. Schauen Sie, hier steht unser erster Turm (Stufe 1). Er ist noch bescheiden, aber sein Licht vertreibt die Dunkelheit.",
    FR: "Merveilleux ! Nous sommes arrivÃ©s au point sÃ©lectionnÃ©. Regardez, voici notre premier chÃ¢teau (Niveau 1). Il est modeste pour l'instant, mais sa lumiÃ¨re dissipe les tÃ©nÃ©bres.",
    ES: "Â¡Fabuloso! Hemos llegado a tu punto seleccionado. Mira, aquÃ­ yace nuestro primer Castillo (Nivel 1). Es modesto por ahora, pero su luz disipa la oscuridad.",
    PT: "Fabulo! Chegamos ao seu ponto selecionado. Olhe, aqui jaz nossa primeira Torre (NÃ­vel 1). Ã‰ modesta por enquanto, mas sua luz dissipa as sombras.",
    JA: "è¦‹äº‹ã§ã™ï¼é¸æŠã—ãŸåœ°ç‚¹ã«åˆ°ç€ã—ã¾ã—ãŸã€‚ã”è¦§ãã ã•ã„ã€ã“ã‚ŒãŒç§ãŸã¡ã®æœ€åˆã®å¡”ï¼ˆãƒ¬ãƒ™ãƒ«1ï¼‰ã§ã™ã€‚ä»Šã¯ã¾ã ã•ã•ã‚„ã‹ã§ã™ãŒã€ãã®å…‰ã¯å¤ä»£ã®é—‡ã‚’æ‰•ã„ã®ã‘ã¾ã™ã€‚",
    KR: "í›Œë¥­í•˜ë‹¤! ë‹¹ì‹ ì´ ì„ íƒí•œ ì˜ì§€ì— ë„ì°©í–ˆë‹¤. ë³´ë¼, ì´ê³³ì— ìš°ë¦¬ì˜ ì²« ë²ˆì§¸ íƒ€ì›Œ(1ë ˆë²¨)ê°€ ì„¸ì›Œì¡Œë‹¤. ì§€ê¸ˆì€ ê²€ì†Œí•˜ì§€ë§Œ ê·¸ ë¹›ì´ ì–´ë‘ ì„ ê±·ì–´ë‚¸ë‹¤.",
    CH: "å¤ªæ£’äº†ï¼æˆ‘ä»¬å·²ç»åˆ°è¾¾äº†ä½ æ‰€é€‰æ‹©çš„åœ°ç‚¹ã€‚çœ‹ï¼Œè¿™é‡Œå»ºæˆäº†æˆ‘ä»¬çš„ç¬¬ä¸€åº§åŸå ¡ï¼ˆ1çº§ï¼‰ã€‚è™½ç„¶ç›®å‰å®ƒè¿˜å¾ˆç®€é™‹ï¼Œä½†å®ƒçš„å…‰èŠ’èƒ½å¤Ÿé©±æ•£è¿œå¤çš„é»‘æš—ã€‚",
  },
  9: {
    RU: "Ğ’ĞµĞ»Ğ¸ĞºĞ¾Ğ»ĞµĞ¿Ğ½Ğ°Ñ Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°! Ğ¢Ñ‹ ÑƒĞ»ÑƒÑ‡ÑˆĞ¸Ğ» Ğ‘Ğ°ÑˆĞ½Ñ Ğ´Ğ¾ 2-Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ²Ğ½Ñ! ĞŸĞ¾ÑĞ¼Ğ¾Ñ‚Ñ€Ğ¸ Ğ½Ğ° ÑÑ‚Ğ¸ Ğ¿Ñ€Ğ¾Ñ‡Ğ½Ñ‹Ğµ ĞºĞ°Ğ¼ĞµĞ½Ğ½Ñ‹Ğµ Ğ¿Ñ€Ğ¸ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸, Ğ±Ğ¾ĞºĞ¾Ğ²Ñ‹Ğµ ĞºÑ€Ñ‹Ğ»ÑŒÑ Ğ¾Ğ±Ğ¾Ñ€Ğ¾Ğ½Ñ‹ Ğ¸ Ğ²Ñ€Ğ°Ñ‰Ğ°ÑÑ‰Ğ¸Ğ¹ÑÑ ÑˆĞ¿Ğ¸Ğ»ÑŒ Ñ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ¾Ğ¼ Zenith.",
    EN: "Magnificent work! You upgraded our Tower to Level 2! Look at these heavy stone structures, custom side wings, and the spinning Crystal Zenith spire emitting cyan color.",
    DE: "GroÃŸartige Arbeit! Sie haben unseren Turm auf Stufe 2 Ğ½Ğ°ÑÑ‚Ñ€Ğ°Ğ¸Ğ²Ğ°Ñ‚ÑŒ! Sehen Sie sich diese schweren Steinstrukturen und die rotierende Spitze des Zenit-Kristalls an.",
    FR: "Travail magnifique ! Vous avez amÃ©liorÃ© le chÃ¢teau au Niveau 2 ! Regardez ces structures de pierre solides, les ailes de dÃ©fense et la flÃ¨che rotative du Cristal Zenith.",
    ES: "Â¡Trabajo magnÃ­fico! Â¡Mejoraste nuestro Castillo al Nivel 2! Mira las estructuras de piedra, las alas defensivas y la aguja giratoria del Cristal Cenit.",
    PT: "Trabalho magnÃ­fico! VocÃª atualizou nossa Torre para o NÃ­vel 2! Veja estas estruturas de pedra pesadas, asas de defesa e a agulha rotativa do Cristal Zenith.",
    JA: "è¦‹äº‹ãªæ‰‹éš›ã§ã™ï¼å¡”ã‚’ãƒ¬ãƒ™ãƒ«2ã¸ã‚¢ãƒƒãƒ—ã‚°ãƒ¬ãƒ¼ãƒ‰ã—ã¾ã—ãŸã­ï¼é ‘ä¸ˆãªçŸ³é€ ã‚Šã®å¤–å£ã€é˜²è¡›ç”¨ã®å´ç¿¼ã€ê·¸ë¦¬ê³  ã‚·ã‚¢ãƒ³è‰²ã®å…‰ã‚’æ”¾ã¤å›è»¢å¼ã®ã‚¼ãƒ‹ã‚¹ã‚¯ãƒªã‚¹ã‚¿ãƒ«å°–å¡”ã‚’ã”è¦§ãã ã•ã„ã€‚",
    KR: "ì¥ì—„í•œ ì—…ì ì´ë‹¤! íƒ€ì›Œë¥¼ 2ë ˆë²¨ë¡œ ìŠ¹ê¸‰ì‹œì¼°ë‹¤! ê²¬ê³ í•œ ëŒ ë²½ê³¼ ë°©ì–´ìš© ì‚¬ì´ë“œ ìœ™, ê·¸ë¦¬ê³  ì²­ë¡ìƒ‰ìœ¼ë¡œ ìì „í•˜ëŠ” ì œë‹ˆìŠ¤ í¬ë¦¬ìŠ¤íƒˆ ì²¨íƒ‘ì„ ëŠê»´ë³´ë¼.",
    CH: "å¹ä¸ºè§‚æ­¢çš„æ°ä½œï¼ä½ å·²æˆåŠŸå°†åŸå ¡å‡çº§è‡³2çº§ï¼å¿«çœ‹çœ‹è¿™äº›åšå›ºçš„çŸ³è´¨å¤–å¢™ã€ä¾§ç¿¼é˜²å«è®¾æ–½ï¼Œä»¥åŠæ•£å‘ç€é’è‰²è§å…‰çš„æ—‹è½¬å¤©é¡¶æ°´æ™¶å¡”å°–ã€‚",
  },
  10: {
    RU: "Ğ¢ĞµĞ¿ĞµÑ€ÑŒ Ğ¼Ñ‹ Ğ¼Ğ¾Ğ¶ĞµĞ¼ Ğ²Ğ¾Ğ¹Ñ‚Ğ¸ Ğ²Ğ½ÑƒÑ‚Ñ€ÑŒ Ğ¦Ğ¸Ñ‚Ğ°Ğ´ĞµĞ»Ğ¸ 2-Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ²Ğ½Ñ. Ğ—Ğ´ĞµÑÑŒ Ğ¿Ğ¾ÑÑ‚Ñ€Ğ¾ĞµĞ½Ñ‹ 3 ĞºĞ»ÑÑ‡ĞµĞ²Ñ‹Ğµ Ğ·Ğ´Ğ°Ğ½Ğ¸Ñ: 1) Ğ’Ğ¾ĞµĞ½Ğ½Ñ‹Ğµ ĞšĞ°Ğ·Ğ°Ñ€Ğ¼Ñ‹ (Ğ¿Ğ¾ĞºÑƒĞ¿ĞºĞ° Ğ²Ğ¾Ğ¸Ğ½Ğ¾Ğ²), 2) ĞÑ€ÑƒĞ¶ĞµĞ¹Ğ½Ğ°Ñ (ÑĞ½Ğ°Ñ€ÑĞ¶ĞµĞ½Ğ¸Ğµ), 3) Ğ¨Ğ¿Ğ¸Ğ¾Ğ½ÑĞºĞ°Ñ Ğ¢Ğ°Ğ¹Ğ½Ğ°Ñ Ğ›Ğ¾Ğ¶Ğ°.",
    EN: "Now we can enter inside the Level 2 Citadel. Three essential facilities are active: 1) Military Barracks (hire soldiers), 2) Armory Warehouse (equipment), 3) Secret Espionage Lodge.",
    DE: "Jetzt kÃ¶nnen wir das Innere der Zitadelle von Stufe 2 betreten. Drei wichtige Einrichtungen sind aktiv: 1) Kasernen, 2) RÃ¼stkammer, 3) Geheime Spionage-Loge.",
    FR: "Nous pouvons maintenant entrer dans la Citadelle de Niveau 2. Trois bÃ¢timents clÃ©s sont actifs : 1) Caserne militaire, 2) Armurerie (Ã©quipements), 3) Loge secrÃ¨te d'espionnage.",
    ES: "Ahora podemos entrar al Castillo de Nivel 2. Tres edificios clave estÃ¡n activos: 1) Cuartel militar, 2) ArmerÃ­a (equipamiento), 3) Logia secreÑ‚Ğ° de espionaje.",
    PT: "Agora podemos entrar dentro do Castelo de NÃ­vel 2. TrÃªs instalaÃ§Ãµes essenciais estÃ£o ativas: 1) Quartel militar, 2) Armaria (equipamento), 3) Loja secreta de espionagem.",
    JA: "ã“ã‚Œã§ãƒ¬ãƒ™ãƒ«2ã‚·Ñ‚Ğ°ãƒ‡ãƒ«ã®å†…éƒ¨ã«å…¥ã‚‹ã“ã¨ãŒã§ãã¾ã™ã€‚ã“ã“ã«ã¯3ã¤ã®é‡è¦æ–½è¨­ãŒã‚ã‚Šã¾ã™ï¼š1ï¼‰è»äº‹å…µèˆï¼ˆå…µå£«ì˜ é›‡ç”¨ï¼‰ã€2ï¼‰è£…å‚™æ­¦å™¨åº«ã€3ï¼‰éš å¯†ã‚¹ãƒ‘ã‚¤è«œå ±æ©Ÿé–¢ã§ã™ã€‚",
    KR: "ì´ì œ 2ë ˆë²¨ ìš”ìƒˆ ë‚´ë¶€ë¡œ ì§„ì…í•  ìˆ˜ ìˆë‹¤. ì„¸ ê°€ì§€ í•µì‹¬ ê±´ë¬¼ì´ ê°€ë™ ì¤‘ì´ë‹¤: 1) êµ°ì‚¬ ë³‘ì˜(ë³‘ì‚¬ ê³ ìš©), 2) ì „ìˆ  ë¬´ê¸°ê³ (ì¥ë¹„), 3) ì¥ë§‰ì˜ ìŠ¤íŒŒì´ ì²©ë³´ì› ê¸¸ë“œ.",
    CH: "ç°åœ¨æˆ‘ä»¬å¯ä»¥è¿›å…¥2çº§åŸå ¡çš„å†…éƒ¨äº†ã€‚è¿™é‡Œå·²ç»å»ºæˆäº†ä¸‰åº§æ ¸å¿ƒå»ºç­‘ï¼š1) å†›æ—…å…µè¥ï¼ˆæ‹›å‹Ÿå£«å…µï¼‰ã€2) æˆ˜å¤‡å†›æ¢°åº“ï¼ˆæŒ‘é€‰è£…å¤‡ï¼‰ã€3) å¯†æ¢æ–¥å€™ä¼šæ‰€ï¼ˆåˆºæ¢å†›æƒ…ï¼‰ã€‚",
  },
  11: {
    RU: "ĞšĞ°Ğ¶Ğ´Ñ‹Ğ¹ Ğ›Ğ¾Ñ€Ğ´-Ğ“ĞµÑ€Ğ¾Ğ¹ Ğ¸Ğ¼ĞµĞµÑ‚ Ğ¶ĞµÑÑ‚ĞºĞ¸Ğ¹ Ğ»Ğ¸Ğ¼Ğ¸Ñ‚ Ğ²Ğ¾Ğ¹ÑĞºĞ°. ĞĞ° 5-Ğ¼ ÑƒÑ€Ğ¾Ğ²Ğ½Ğµ Ñ‚Ñ‹ Ğ¼Ğ¾Ğ¶ĞµÑˆÑŒ Ğ½ĞµÑÑ‚Ğ¸ Ñ ÑĞ¾Ğ±Ğ¾Ğ¹ Ğ¼Ğ°ĞºÑĞ¸Ğ¼ÑƒĞ¼ 4 Ğ¾Ñ‚Ñ€ÑĞ´Ğ° Ğ²Ğ¾Ğ¸Ğ½Ğ¾Ğ². ĞĞ´Ğ¸Ğ½ Ğ¾Ñ‚Ñ€ÑĞ´ Ğ²Ğ¾Ğ¸Ğ½Ğ¾Ğ² Ğ´Ğ°ĞµÑ‚ +15 Ğº Ğ¡Ğ¸Ğ»Ğµ Ğ“Ğ°Ñ€Ğ½Ğ¸Ğ·Ğ¾Ğ½Ğ° Ñ‚Ğ²Ğ¾ĞµĞ¹ Ğ‘Ğ°ÑˆĞ½Ğ¸ Ğ—ĞµĞ¼Ğ»Ğ¸.",
    EN: "Every Lord Hero has strict troop capacity limits. At level 5, you can carry a maximum of 4 troop squads. Each hired squad grants +15 Garrison Power to your Land Tower.",
    DE: "Jeder Lord-Held hat strenge Truppenlimits. Auf Stufe 5 kÃ¶nnen Sie maximal 4 Truppenteile mitfÃ¼hren. Jede angeworbene Truppe erhÃ¶ht die GarnisonsstÃ¤rke um +15.",
    FR: "Chaque HÃ©ros a des limites de troupes strictes. Au niveau 5, vous Ğ¼Ğ¾Ğ¶ĞµÑ‚Ğµ transporter un maximum de 4 escouades. Chaque troupe recrutÃ©e ajoute +15 Ã  la puissance de la garnison.",
    ES: "Cada HÃ©roe tiene lÃ­mites de tropas estrictos. En el nivel 5, puedes llevar un mÃ¡ximo de 4 escuadrones. Cada tropa reclutada aÃ±ade +15 al poder de la guarniciÃ³n.",
    PT: "Cada HerÃ³i tem limites estritos de tropas. No nÃ­vel 5, vocÃª pode carregar no mÃ¡ximo 4 esquadrÃµes. Cada tropa contratada adiciona +15 ao poder da guarniÃ§Ã£o.",
    JA: "å„ãƒ­ãƒ¼ãƒ‰ï¼ˆè‹±é›„ï¼‰ã«ã¯å³æ ¼ãªå…µåŠ›ä¸Šé™ãŒè¨­ã‘ã‚‰ã‚Œã¦ã„ã¾ã™ã€‚ãƒ¬ãƒ™ãƒ«5ã®ã‚ãªãŸãŒä¸€å¸¯ã«é€£ã‚Œã¦æ­©ã‘ã‚‹ã®ã¯æœ€å¤§4éƒ¨éšŠã¾ã§ã§ã™ã€‚1éƒ¨éšŠé›‡ç”¨ã™ã‚‹ã”ã¨ã«ã€æ‹ ç‚¹ã®é˜²è¡›è©•å€¤ãŒ+15ã•ã‚Œã¾ã™ã€‚",
    KR: "ê°ê°ì˜ ë¡œë“œ(ì˜ì›…)ëŠ” ì—„ê²©í•œ êµ°ëŒ€ ìµœëŒ€ ì†Œì§€ ì œí•œì´ ìˆë‹¤. 5ë ˆë²¨ì¸ ì „ì‚¬ëŠ” ìµœëŒ€ 4ê°œ ì†ŒëŒ€ê¹Œì§€ ë°ë¦¬ê³  ë‹¤ë‹ ìˆ˜ ìˆë‹¤. ê³ ìš© ì‹œ ì†ŒëŒ€ë‹¹ ì˜ì§€ ë°©ì–´ë ¥ì´ +15 ì¦ê°€í•œë‹¤.",
    CH: "æ¯ä¸€ä½é¢†ä¸»è‹±é›„éƒ½æœ‰æä¸¥æ ¼çš„å¸¦å…µä¸Šé™ã€‚åœ¨å½“å‰çš„5çº§çŠ¶æ€ä¸‹ï¼Œä½ æœ€å¤šåªèƒ½æºå¸¦4æ”¯å£«å…µåˆ†é˜Ÿã€‚æ¯æ‹›å‹Ÿä¸€æ”¯åˆ†é˜Ÿï¼Œéƒ½ä¼šä¸ºä½ çš„åœ°ç›˜é˜²å¾¡æˆ˜åŠ›æä¾› +15 è¯„åˆ†åŠ æˆã€‚",
  },
  12: {
    RU: "ĞĞ´ĞµĞ½ÑŒ Ğ³ĞµÑ€Ğ¾Ñ! ĞĞ°Ğ¶Ğ¼Ğ¸ Ğ½Ğ° Ğ¸ĞºĞ¾Ğ½ĞºÑƒ Ğ²Ğ²ĞµÑ€Ñ…Ñƒ ÑĞ»ĞµĞ²Ğ°, Ñ‡Ñ‚Ğ¾Ğ±Ñ‹ Ğ¾Ñ‚ĞºÑ€Ñ‹Ñ‚ÑŒ ĞœĞµĞ½Ñ Ğ¡Ğ½Ğ°Ñ€ÑĞ¶ĞµĞ½Ğ¸Ñ. Ğ—Ğ´ĞµÑÑŒ Ñ‚Ñ‹ Ğ¼Ğ¾Ğ¶ĞµÑˆÑŒ Ğ¿ĞµÑ€ĞµÑ‚Ğ°ÑĞºĞ¸Ğ²Ğ°Ñ‚ÑŒ Ğ¾Ñ€ÑƒĞ¶Ğ¸Ğµ Ğ² ÑÑ‡ĞµĞ¹ĞºĞ¸ Ğ¸ Ğ·Ğ°Ğ¿ÑƒÑÑ‚Ğ¸Ñ‚ÑŒ ÑĞ¿ÑƒÑ‚Ğ½Ğ¸Ğº-Ñ€Ğ°Ğ·Ğ²ĞµĞ´Ñ‡Ğ¸Ğº Ñ‚Ğ°Ğ¹Ğ½Ğ¾Ğ¹ Ğ¨Ğ¿Ğ¸Ğ¾Ğ½ÑĞºĞ¾Ğ¹ Ğ›Ğ¾Ğ¶Ğ¸ Ğ·Ğ° 150 Ğ·Ğ¾Ğ»Ğ¾Ñ‚Ğ°!",
    EN: "Equip your Hero! Tap the top-left player icon to open the Equipment grid. Drag your glowing swords or armor into active slots, and deploy a stealth spy for 150 gold!",
    DE: "RÃ¼stet euren Helden aus! Klickt auf das Symbol oben links, um das AusrÃ¼stungmenÃ¼ zu Ã¶ffnen. Zieht Waffen in Slots und entsendet einen Spion fÃ¼r 150 Gold!",
    FR: "Ã‰quipez votre HÃ©ros ! Cliquez sur l'icÃ´ne en haut Ã  gauche pour ouvrir la grille d'Ã©quipement. Glissez vos armes dans les fentes et envoyez un espion pour 150 piÃ¨ces d'or !",
    ES: "Â¡Equipa a tu HÃ©roe! Haz clic en el icono superior izquierdo para abrir el menÃº de equipamiento. Â¡Arrastra tus armas a las ranuras y envÃ­a un espÃ­a por 150 de oro!",
    PT: "Equipe seu HerÃ³i! Clique no Ã­cone superior esquerdo para abrir o painel de equipamentos. ArrasÑ‚Ğµ suas armas para os compartimentos e envie um espiÃ£o por 150 de ouro!",
    JA: "è‹±é›„ã‚’è£…å‚™ã•ã›ã¾ã—ã‚‡ã†ï¼å·¦ä¸Šã®ãƒ—ãƒ¬ã‚¤ãƒ¤ãƒ¼ã‚¢ã‚¤ã‚³ãƒ³ã‚’ã‚¯ãƒªãƒƒã‚¯ã—ã¦ã€è£…å‚™ç”»é¢ã‚’é–‹ãã¾ã™ã€‚æ­¦å™¨ã‚„é˜²å…·ã‚’ã‚¹ãƒ­ãƒƒãƒˆã«ãƒ‰ãƒ©ãƒƒã‚°ï¼†ãƒ‰ãƒ­ãƒƒãƒ—ã—ã€150ã‚´ãƒ¼ãƒ«ãƒ‰ã§ã‚¹ãƒ‘ã‚¤ã‚’éš å¯†æ´¾é£ã—ã¦ãã ã•ã„ï¼",
    KR: "ì˜ì›…ì„ ë¬´ì¥ì‹œí‚¤ì‹­ì‹œì˜¤! ì™¼ìª½ ìƒë‹¨ í”„ë¡œí•„ì„ í´ë¦­í•˜ì—¬ ì¥ë¹„ êµí™˜ íŒ¨ë„ì„ ì—½ë‹ˆë‹¤. ë¬´ê¸°ë¥¼ ë¹ˆ ìŠ¬ë¡¯ì— ì¥ì°©í•˜ê³ , 150 ê³¨ë“œë¡œ ì„±ì˜ ì •ì°° ê¸¸ë“œì—ì„œ ì²©ë³´ì›ì„ íŒŒê²¬í•˜ì‹­ì‹œì˜¤!",
    CH: "å…¨å‰¯æ­¦è£…ï¼Œå‡†å¤‡è¿æˆ˜ï¼ç‚¹å‡»å·¦ä¸Šè§’çš„è§’è‰²å¤´åƒï¼Œå³å¯æ‰“å¼€è£…å¤‡æ ã€‚ç›´æ¥å°†ç¥å…µåˆ©å™¨ or é‡é“ é˜²å…·æ‹–å…¥å¯¹åº”æ§½ä½ï¼Œç”šè‡³å¯ä»¥æ¶ˆè€— 150 é‡‘å¸å¼€å¯å¯†æ¢è°æŠ¥ä¾¦å¯Ÿï¼",
  },
};
const SPEAKER_NAMES: Record<
  "RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH",
  string
> = {
  RU: "ĞÑĞ»Ğ¸ÑÑĞ°, Ğ¥Ñ€Ğ°Ğ½Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ğ¸Ñ†Ğ° ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ°",
  EN: "Aelyssa, Keeper of Crystal",
  DE: "Aelyssa, HÃ¼terin des Kristalls",
  FR: "Aelyssa, Gardienne du Cristal",
  ES: "Aelyssa, Guardiana del Cristal",
  PT: "Aelyssa, GuardiÃ£ do Cristal",
  JA: "ã‚¯ãƒªã‚¹ã‚¿ãƒ«ã®å®ˆè­·è€…ã‚¢ã‚¨ãƒªãƒƒã‚µ",
  KR: "í¬ë¦¬ìŠ¤íƒˆì˜ ìˆ˜í˜¸ì ì—˜ë¦¬ì‚¬",
  CH: "æ°´æ™¶å®ˆæŠ¤è€… è‰¾è‰è",
};

const LEVEL_LABELS: Record<
  "RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH",
  string
> = {
  RU: "Ğ£Ñ€.",
  EN: "Lvl",
  DE: "Stufe",
  FR: "Niv.",
  ES: "Nivel",
  PT: "NÃ­vel",
  JA: "Lv.",
  KR: "ë ˆë²¨",
  CH: "ç­‰çº§",
};

const STATS_LABELS: Record<
  "XP" | "MANA" | "UPGRADE" | "GAIN_XP" | "USE_MANA" | "RESET",
  Record<"RU" | "EN" | "DE" | "FR" | "ES" | "PT" | "JA" | "KR" | "CH", string>
> = {
  XP: {
    RU: "ĞĞ¿Ñ‹Ñ‚",
    EN: "XP",
    DE: "EP",
    FR: "XP",
    ES: "EXP",
    PT: "XP",
    JA: "çµŒé¨“å€¤",
    KR: "ê²½í—˜ì¹˜",
    CH: "ç»éªŒ",
  },
  MANA: {
    RU: "ĞœĞ°Ğ½Ğ°",
    EN: "Mana",
    DE: "Mana",
    FR: "Mana",
    ES: "ManÃ¡",
    PT: "Mana",
    JA: "ãƒãƒŠ",
    KR: "ë§ˆë‚˜",
    CH: "é­”æ³•å€¼",
  },
  UPGRADE: {
    RU: "Ğ›Ğ²Ğ» ĞĞ¿ +",
    EN: "Lvl Up +",
    DE: "Aufsteigen +",
    FR: "Niveau +",
    ES: "Nivel +",
    PT: "NÃ­vel +",
    JA: "ãƒ¬ãƒ™ãƒ«ã‚¢ãƒƒãƒ— +",
    KR: "ë ˆë²¨ì—… +",
    CH: "å‡çº§ +",
  },
  GAIN_XP: {
    RU: "ĞŸĞ¾Ğ»ÑƒÑ‡Ğ¸Ñ‚ÑŒ ĞĞ¿Ñ‹Ñ‚",
    EN: "Gain XP",
    DE: "EP Erhalten",
    FR: "Plus d'XP",
    ES: "Ganar EXP",
    PT: "Ganhar XP",
    JA: "çµŒé¨“å€¤ç²å¾—",
    KR: "ê²½í—˜ì¹˜ íšë“",
    CH: "è·å¾—ç»éªŒ",
  },
  USE_MANA: {
    RU: "Ğ¢Ñ€Ğ°Ñ‚Ğ¸Ñ‚ÑŒ ĞœĞ°Ğ½Ñƒ",
    EN: "Spend Mana",
    DE: "Mana Ausgeben",
    FR: "Utiliser Mana",
    ES: "Gastar ManÃ¡",
    PT: "Gastar Mana",
    JA: "ãƒãƒŠæ¶ˆè²»",
    KR: "ë§ˆë‚˜ ì†Œë¹„",
    CH: "æ¶ˆè€—é­”æ³•",
  },
  RESET: {
    RU: "â† Ğ¡Ğ±Ñ€Ğ¾ÑĞ¸Ñ‚ÑŒ ÑÑĞ¶ĞµÑ‚",
    EN: "â† Restart quest",
    DE: "â† Quest neu starten",
    FR: "â† Relancer la quÃªte",
    ES: "â† Reiniciar misiÃ³n",
    PT: "â† Reiniciar jornada",
    JA: "â† ã‚¯ã‚¨ã‚¹ãƒˆå†èµ·å‹•",
    KR: "â† í€˜ìŠ¤íŠ¸ ì¬ì‹œì‘",
    CH: "â† é‡æ–°å¼€å§‹",
  },
};

export const ALL_TROOPS_ROSTER = [
  {
    id: "guard",
    name: "Ğ‘Ğ¾ĞµÑ† Ñ„Ñ€Ğ°ĞºÑ†Ğ¸Ğ¸",
    baseHp: 180,
    hpPerLvl: 30,
    baseDmg: 15,
    dmgPerLvl: 3,
    baseArm: 8,
    armPerLvl: 1.5,
    cost: 50,
    rarity: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 12,
    attrPerLvl: 3.2,
    icon: "ğŸ›¡ï¸",
    p: "Symmetrical close-up eye-level 3D portrait headshot of a brave royal faction fighter, clean medieval steel helmet, fierce human warrior face, neon cyan glowing gem on chest armor, stylized polished clay render, flat simple pure white background (#ffffff), no floor shadows, no vignette, game asset avatar style.",
    passives: [
      {
        name: "Ğ¨Ñ‚ÑƒÑ€Ğ¼Ğ¾Ğ²Ğ¾Ğ¹ Ğ¡Ñ‚Ñ€Ğ¾Ğ¹",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ Ğ²Ñ‹Ğ½Ğ¾ÑĞ»Ğ¸Ğ²Ğ¾ÑÑ‚ÑŒ Ğ¸ Ğ·Ğ°Ñ‰Ğ¸Ñ‚Ñƒ ÑĞ¾ÑĞµĞ´Ğ½Ğ¸Ñ… Ğ¿ĞµÑ…Ğ¾Ñ‚Ğ¸Ğ½Ñ†ĞµĞ² Ğ½Ğ° +15%.",
      },
      {
        name: "Ğ›Ğ°Ñ‚Ğ½Ñ‹Ğ¹ Ğ’Ğ¾Ñ€Ğ¾Ñ‚Ğ½Ğ¸Ğº",
        desc: "Ğ£Ğ²ĞµĞ»Ğ¸Ñ‡Ğ¸Ğ²Ğ°ĞµÑ‚ Ğ·Ğ°Ñ‰Ğ¸Ñ‚Ñƒ Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ² Ğ»ÑƒÑ‡Ğ½Ğ¸ĞºĞ¾Ğ² Ğ¸ ÑÑ‚Ñ€ĞµĞ»ĞºĞ¾Ğ²Ğ¾Ğ³Ğ¾ Ğ¾Ñ€ÑƒĞ¶Ğ¸Ñ Ğ½Ğ° +25%.",
      },
    ],
    actives: [
      {
        name: "Ğ£Ğ´Ğ°Ñ€ Ğ ÑƒĞºĞ¾ÑÑ‚ÑŒÑ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 180% Ñ„Ğ¸Ğ·Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¸ Ğ¾Ğ³Ğ»ÑƒÑˆĞ°ĞµÑ‚ Ğ²Ñ€Ğ°Ğ³Ğ° Ğ½Ğ° 1 Ñ…Ğ¾Ğ´.",
      },
    ],
  },
  {
    id: "archer",
    name: "Ğ­Ğ»ÑŒÑ„Ğ¸Ğ¹ÑĞºĞ¸Ğ¹ Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº",
    baseHp: 115,
    hpPerLvl: 20,
    baseDmg: 24,
    dmgPerLvl: 4,
    baseArm: 3,
    armPerLvl: 0.8,
    cost: 75,
    rarity: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹",
    classType: "Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº",
    mainAttr: "Ğ›Ğ¾Ğ²ĞºĞ¾ÑÑ‚ÑŒ (AGI)",
    baseAttr: 14,
    attrPerLvl: 4.1,
    icon: "ğŸ¹",
    p: "Symmetrical close-up front-facing 3D portrait face shot of an elegant elven marksman archer wearing an emerald leather cowl hood, glowing green eyes, high precision stylized clay render, flat simple pure white background (#ffffff), zero shadow projection, high density avatar.",
    passives: [
      {
        name: "Ğ¡Ğ¾ĞºĞ¾Ğ»Ğ¸Ğ½Ğ¾Ğµ ĞĞºĞ¾",
        desc: "ĞŸĞ¾Ğ·Ğ²Ğ¾Ğ»ÑĞµÑ‚ Ğ°Ñ‚Ğ°ĞºĞ¾Ğ²Ğ°Ñ‚ÑŒ ÑĞºĞ²Ğ¾Ğ·ÑŒ Ğ»ĞµÑĞ½Ñ‹Ğµ Ğ¿Ñ€ĞµĞ¿ÑÑ‚ÑÑ‚Ğ²Ğ¸Ñ Ğ±ĞµĞ· ÑˆÑ‚Ñ€Ğ°Ñ„Ğ° Ğº Ñ‚Ğ¾Ñ‡Ğ½Ğ¾ÑÑ‚Ğ¸.",
      },
      {
        name: "Ğ£Ğ¿Ñ€ĞµĞ¶Ğ´Ğ°ÑÑ‰Ğ¸Ğ¹ ĞŸÑ€Ñ‹Ğ¶Ğ¾Ğº",
        desc: "Ğ¨Ğ°Ğ½Ñ ÑƒĞºĞ»Ğ¾Ğ½ĞµĞ½Ğ¸Ñ Ğ¸Ğ· Ğ±Ğ»Ğ¸Ğ¶Ğ½ĞµĞ¹ Ğ´Ğ¸ÑÑ‚Ğ°Ğ½Ñ†Ğ¸Ğ¸ Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞµĞ½ Ğ½Ğ° +20%.",
      },
    ],
    actives: [
      {
        name: "Ğ¡Ñ‚Ñ€ĞµĞ»Ğ° Ğ’ĞµÑ‚Ñ€Ğ° (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞĞ±ÑÑ‚Ñ€ĞµĞ»Ğ¸Ğ²Ğ°ĞµÑ‚ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ½Ğ½Ñ‹Ğ¹ ÑĞµĞºÑ‚Ğ¾Ñ€ Ğ³Ñ€Ğ°Ğ´Ğ¾Ğ¼ ÑÑ‚Ñ€ĞµĞ», Ğ½Ğ°Ğ½Ğ¾ÑÑ 250% ÑƒÑ€Ğ¾Ğ½Ğ°.",
      },
    ],
  },
  {
    id: "arcanist",
    name: "Ğ‘Ğ¾ĞµĞ²Ğ¾Ğ¹ ĞœĞ°Ğ³ Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°",
    baseHp: 90,
    hpPerLvl: 15,
    baseDmg: 38,
    dmgPerLvl: 5.5,
    baseArm: 2,
    armPerLvl: 0.5,
    cost: 120,
    rarity: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹",
    classType: "ĞœĞ°Ğ³",
    mainAttr: "Ğ˜Ğ½Ñ‚ĞµĞ»Ğ»ĞµĞºÑ‚ (INT)",
    baseAttr: 16,
    attrPerLvl: 5.0,
    icon: "ğŸ”®",
    p: "Symmetrical cute close-up 3D face portrait of an arcane battle archmage apprentice wizard, wearing a neon cyan crystal crown headband, violet magic silk robe, glowing purple mystical eyes, stylized game clay render, pure white isolated background (#ffffff), high-contrasted toy portrait.",
    passives: [
      {
        name: "Ğ ĞµĞ·Ğ¾Ğ½Ğ°Ğ½Ñ Ğ—ĞµĞ½Ğ¸Ñ‚Ğ°",
        desc: "ĞšĞ°Ğ¶Ğ´Ñ‹Ğµ 2 Ñ…Ğ¾Ğ´Ğ° Ğ¿Ğ°ÑÑĞ¸Ğ²Ğ½Ğ¾ Ğ²Ğ¾ÑĞ¿Ğ¾Ğ»Ğ½ÑĞµÑ‚ 8 ĞµĞ´. Ğ¼Ğ°Ğ½Ñ‹ ÑĞ¾ÑĞ·Ğ½Ğ¸ĞºĞ°Ğ¼.",
      },
      {
        name: "Ğ­Ñ„Ğ¸Ñ€Ğ½Ñ‹Ğ¹ ĞŸĞµÑ€ĞµÑ‚Ğ¾Ğº",
        desc: "ĞŸĞ¾Ğ³Ğ»Ğ¾Ñ‰Ğ°ĞµÑ‚ Ğ¿ĞµÑ€Ğ²Ñ‹Ğ¹ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¸Ğ¹ Ğ¼Ğ°Ğ³Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ Ğ¸Ğ¼Ğ¿ÑƒĞ»ÑŒÑ, ÑĞ½Ğ¸Ğ¶Ğ°Ñ ĞµĞ³Ğ¾ ÑƒÑ€Ğ¾Ğ½ Ğ½Ğ° 40%.",
      },
    ],
    actives: [
      {
        name: "Ğ˜Ğ¼Ğ¿ÑƒĞ»ÑŒÑ Ğ—Ğ²ĞµĞ·Ğ´ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "Ğ Ğ°Ğ·Ñ€Ñ‹Ğ²Ğ°ĞµÑ‚ ÑÑ„Ğ¸Ñ€Ğ½Ñ‹Ğµ ÑĞ²ÑĞ·Ğ¸, Ğ½Ğ°Ğ½Ğ¾ÑÑ Ğ¼Ğ°Ğ³Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ ÑƒÑ€Ğ¾Ğ½ Ğ¿Ğ¾ Ğ¿Ğ»Ğ¾Ñ‰Ğ°Ğ´Ğ¸ (320% ÑĞ¸Ğ»Ñ‹).",
      },
    ],
  },
  {
    id: "paladin",
    name: "ĞŸĞ°Ğ»Ğ°Ğ´Ğ¸Ğ½ Ğ¡Ğ²ĞµÑ‚Ğ°",
    baseHp: 420,
    hpPerLvl: 55,
    baseDmg: 45,
    dmgPerLvl: 5,
    baseArm: 18,
    armPerLvl: 2.5,
    cost: 150,
    rarity: "Ğ­Ğ»Ğ¸Ñ‚Ğ½Ñ‹Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 18,
    attrPerLvl: 4.5,
    icon: "âš¡",
    p: "Symmetrical high-end 3D face closeup portrait of a grand paladin, gold-plated steel visor crown helmet, shining warm golden eyes, stylized clay render, plain flat pure white background (#ffffff), no ambient shadows on floor.",
    passives: [
      {
        name: "ĞÑƒÑ€Ğ° Ğ–Ğ¸Ğ·Ğ½Ğ¸",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ Ñ€ĞµĞ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ñ Ğ·Ğ´Ğ¾Ñ€Ğ¾Ğ²ÑŒÑ ÑĞ¾ÑĞ·Ğ½Ğ¸ĞºĞ¾Ğ² Ğ² Ñ€Ğ°Ğ´Ğ¸ÑƒÑĞµ 2 ĞºĞ»ĞµÑ‚Ğ¾Ğº Ğ½Ğ° +10%.",
      },
      {
        name: "Ğ¡Ñ‚Ğ¾Ğ¹ĞºĞ¾ÑÑ‚ÑŒ Ğ¢Ğ¸Ñ‚Ğ°Ğ½Ğ°",
        desc: "Ğ£Ğ¼ĞµĞ½ÑŒÑˆĞ°ĞµÑ‚ Ğ²ĞµÑÑŒ Ğ²Ñ…Ğ¾Ğ´ÑÑ‰Ğ¸Ğ¹ Ñ„Ğ¸Ğ·Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ ÑƒÑ€Ğ¾Ğ½ Ğ½Ğ° Ñ„Ğ¸ĞºÑĞ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ½Ñ‹Ğµ 15 ĞµĞ´.",
      },
      {
        name: "ĞœĞ°Ğ³Ğ½Ğ¸Ñ‚Ğ½Ñ‹Ğ¹ Ğ©Ğ¸Ñ‚",
        desc: "ĞŸÑ€Ğ¸Ğ½Ğ¸Ğ¼Ğ°ĞµÑ‚ Ğ½Ğ° ÑĞµĞ±Ñ 25% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ½Ğ°Ğ¿Ñ€Ğ°Ğ²Ğ»ĞµĞ½Ğ½Ğ¾Ğ³Ğ¾ Ğ² ÑĞ»Ğ°Ğ±Ğ¾Ğ³Ğ¾ ÑĞ¾ÑĞµĞ´Ğ°.",
      },
    ],
    actives: [
      {
        name: "ĞšĞ°Ñ€Ğ° Ğ“Ğ¾ÑĞ¿Ğ¾Ğ´Ğ½Ñ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 320% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¿Ğ¾ Ğ¿Ğ»Ğ¾Ñ‰Ğ°Ğ´Ğ¸ Ğ² Ñ„Ğ¾Ñ€Ğ¼Ğµ ĞºÑ€ĞµÑÑ‚Ğ° Ğ¸ Ğ²ĞµÑˆĞ°ĞµÑ‚ Ğ½ĞµĞ¼Ğ¾Ñ‚Ñƒ Ğ½Ğ° Ğ¼Ğ°Ğ³Ğ¾Ğ².",
      },
      {
        name: "Ğ”Ğ°Ñ€ Ğ¡Ğ¿Ğ°ÑĞµĞ½Ğ¸Ñ",
        desc: "Ğ¡Ğ½Ğ¸Ğ¼Ğ°ĞµÑ‚ Ğ´ĞµĞ±Ğ°Ñ„Ñ„Ñ‹ Ñ Ğ²Ñ‹Ğ±Ñ€Ğ°Ğ½Ğ½Ğ¾Ğ¹ Ğ´Ğ¸Ğ²Ğ¸Ğ·Ğ¸Ğ¸ Ğ¸ Ğ²Ğ¾ÑĞ¿Ğ¾Ğ»Ğ½ÑĞµÑ‚ Ğ¸Ğ¼ 400 HP.",
      },
    ],
  },
  {
    id: "cavalry",
    name: "Ğ˜Ğ¼Ğ¿ĞµÑ€ÑĞºĞ°Ñ ĞšĞ¾Ğ½Ğ½Ğ¸Ñ†Ğ°",
    baseHp: 550,
    hpPerLvl: 70,
    baseDmg: 58,
    dmgPerLvl: 7,
    baseArm: 14,
    armPerLvl: 2.2,
    cost: 220,
    rarity: "Ğ ĞµĞ´ĞºĞ¸Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 22,
    attrPerLvl: 4.8,
    icon: "ğŸ‡",
    p: "Detailed 3D face closeup portrait model of a heavy imperial cavalry knight captain, silver steel visor with cyan flowing feather plume, stylized game character look on flat pure white background (#ffffff), sharp clay.",
    passives: [
      {
        name: "Ğ˜Ğ½ĞµÑ€Ñ†Ğ¸Ñ Ğ Ğ°Ğ·Ğ±ĞµĞ³Ğ°",
        desc: "ĞšĞ°Ğ¶Ğ´Ñ‹Ğµ 3 ĞºĞ»ĞµÑ‚ĞºĞ¸ Ğ¿ĞµÑ€ĞµĞ¼ĞµÑ‰ĞµĞ½Ğ¸Ñ Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞ°ÑÑ‚ ÑƒÑ€Ğ¾Ğ½ ÑĞ»ĞµĞ´ÑƒÑÑ‰ĞµĞ³Ğ¾ ÑƒĞ´Ğ°Ñ€Ğ° Ğ½Ğ° +15%.",
      },
      {
        name: "Ğ¨Ğ¿Ğ¾Ñ€Ğ½Ñ‹Ğ¹ ĞĞ°Ñ‚Ğ¸ÑĞº",
        desc: "Ğ¨Ğ°Ğ½Ñ ÑƒĞ²ĞµÑ€Ğ½ÑƒÑ‚ÑŒÑÑ Ğ¾Ñ‚ Ğ°Ñ‚Ğ°Ğº ĞºĞ¾Ğ¿ĞµĞ¹Ñ‰Ğ¸ĞºĞ¾Ğ² Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞµĞ½ Ğ½Ğ° 25%.",
      },
      {
        name: "Ğ¢ÑĞ¶ĞµĞ»Ñ‹Ğ¹ Ğ¢Ğ¾Ğ¿Ğ¾Ñ‚",
        desc: "ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 12-24 ÑƒÑ€Ğ¾Ğ½Ğ° Ğ²ÑĞµĞ¼ Ñ†ĞµĞ»ÑĞ¼ Ğ¿Ñ€ĞµĞ³Ñ€Ğ°Ğ¶Ğ´Ğ°ÑÑ‰Ğ¸Ğ¼ ĞºĞ¾Ğ½Ğ½Ğ¸Ñ†Ğµ Ğ¿ÑƒÑ‚ÑŒ.",
      },
    ],
    actives: [
      {
        name: "Ğ¢Ğ°Ñ€Ğ°Ğ½ ĞŸĞ¸ĞºĞ¸ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞŸÑ€Ğ¾Ğ±Ğ¸Ğ²Ğ°ĞµÑ‚ ÑÑ‚Ñ€Ğ¾Ğ¹ Ğ²Ñ€Ğ°Ğ³Ğ¾Ğ², Ğ½Ğ°Ğ½Ğ¾ÑÑ 350% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¸ Ğ¾Ñ‚Ğ±Ñ€Ğ°ÑÑ‹Ğ²Ğ°Ñ Ğ¸Ñ… Ğ½Ğ° 2 ĞºĞ»ĞµÑ‚ĞºĞ¸.",
      },
      {
        name: "ĞšĞ¾Ğ½Ğ½Ñ‹Ğ¹ Ğ¡Ğ²Ğ¸ÑÑ‚",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ ÑĞºĞ¾Ñ€Ğ¾ÑÑ‚ÑŒ Ğ¿ĞµÑ€ĞµĞ´Ğ²Ğ¸Ğ¶ĞµĞ½Ğ¸Ñ Ğ²ÑĞµĞ¹ Ğ°Ñ€Ğ¼Ğ¸Ğ¸ Ğ½Ğ° +2 Ğ½Ğ° 1 Ñ…Ğ¾Ğ´.",
      },
    ],
  },
  {
    id: "cannoneer",
    name: "ĞÑĞ°Ğ´Ğ½Ğ¾-Ğ±Ğ¾ĞµĞ²Ğ¾Ğ¹ ĞŸÑƒÑˆĞºĞ°Ñ€ÑŒ",
    baseHp: 480,
    hpPerLvl: 65,
    baseDmg: 85,
    dmgPerLvl: 9.5,
    baseArm: 10,
    armPerLvl: 1.8,
    cost: 300,
    rarity: "Ğ­Ğ¿Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 28,
    attrPerLvl: 5.8,
    icon: "ğŸ’£",
    p: "Detailed 3D game portrait headshot of a dwarven master engineer bombardier cannoneer, heavy brass goggles, charred face, stylized toy sculpt on flat pure white background (#ffffff).",
    passives: [
      {
        name: "ĞÑĞ°Ğ´Ğ½Ğ°Ñ ĞĞ°Ğ²Ğ¾Ğ´ĞºĞ°",
        desc: "ĞœĞ½Ğ¾Ğ¶Ğ¸Ñ‚ĞµĞ»ÑŒ ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¿Ğ¾ Ğ·Ğ°Ğ³Ñ€Ğ°Ğ¶Ğ´ĞµĞ½Ğ¸ÑĞ¼ Ğ¸ ĞºĞ°Ğ¼ĞµĞ½Ğ½Ñ‹Ğ¼ Ğ·Ğ°Ğ¼ĞºĞ°Ğ¼ Ğ˜Ğ˜ Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞµĞ½ Ğ² x3.0.",
      },
      {
        name: "Ğ¢ĞµÑ€Ğ¼Ğ¾-ĞšĞ°Ñ€ĞºĞ°Ñ",
        desc: "ĞŸĞ¾Ğ»Ğ½Ğ°Ñ Ğ½ĞµĞ²Ğ¾ÑĞ¿Ñ€Ğ¸Ğ¸Ğ¼Ñ‡Ğ¸Ğ²Ğ¾ÑÑ‚ÑŒ Ğº ÑÑ„Ñ„ĞµĞºÑ‚Ğ°Ğ¼ Ğ³Ğ¾Ñ€ĞµĞ½Ğ¸Ñ Ğ¸ Ğ¼Ğ°Ğ³Ğ¼Ğµ.",
      },
      {
        name: "Ğ¢ÑĞ¶ĞµĞ»Ñ‹Ğ¹ ĞÑ€ÑƒĞ´Ğ¸Ğ¹Ğ½Ñ‹Ğ¹ Ğ¡Ñ‚Ğ²Ğ¾Ğ»",
        desc: "ĞŸÑ€ĞµĞ¿ÑÑ‚ÑÑ‚Ğ²ÑƒĞµÑ‚ Ğ¾Ñ‚Ğ±Ñ€Ğ°ÑÑ‹Ğ²Ğ°Ğ½Ğ¸Ñ Ğ¸ Ğ¿ĞµÑ€ĞµĞ¼ĞµÑ‰ĞµĞ½Ğ¸Ñ Ğ¿ÑƒÑˆĞºĞ°Ñ€Ñ Ğ²Ñ€Ğ°Ğ³Ğ°Ğ¼Ğ¸.",
      },
      {
        name: "Ğ¨Ñ€Ğ°Ğ¿Ğ½ĞµĞ»ÑŒĞ½Ñ‹Ğ¹ ĞŸĞ¾Ğ´Ñ€Ñ‹Ğ²",
        desc: "ĞŸÑ€Ğ¸ Ğ¿Ğ¾Ğ»ÑƒÑ‡ĞµĞ½Ğ¸Ğ¸ ĞºÑ€Ğ¸Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ ÑƒĞ´Ğ°Ñ€Ğ° Ğ²Ğ·Ñ€Ñ‹Ğ²Ğ°ĞµÑ‚ÑÑ, Ğ½Ğ°Ğ½Ğ¾ÑÑ 50 ÑƒÑ€Ğ¾Ğ½Ğ° Ğ²Ğ¾ĞºÑ€ÑƒĞ³.",
      },
    ],
    actives: [
      {
        name: "Ğ›Ğ¾Ğ±Ğ¾Ğ²Ğ¾Ğ¹ Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ» (Ğ£Ğ»ÑŒÑ‚)",
        desc: "Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ»Ğ¸Ğ²Ğ°ĞµÑ‚ ÑĞ´Ñ€Ğ¾Ğ¼ Ğ¿Ğ¾ Ğ¿Ğ»Ğ¾Ñ‰Ğ°Ğ´Ğ¸ ĞºĞ¾Ğ½ÑƒÑĞ°, Ğ½Ğ°Ğ½Ğ¾ÑÑ 400% Ñ„Ğ¸Ğ·Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ°.",
      },
      {
        name: "ĞÑĞ²ĞµÑ‚Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ğ°Ñ Ğ Ğ°ĞºĞµÑ‚Ğ°",
        desc: "ĞĞ±Ğ½Ğ°Ñ€ÑƒĞ¶Ğ¸Ğ²Ğ°ĞµÑ‚ Ğ½ĞµĞ²Ğ¸Ğ´Ğ¸Ğ¼Ñ‹Ñ… Ñ€Ğ°Ğ·Ğ²ĞµĞ´Ñ‡Ğ¸ĞºĞ¾Ğ² Ğ² Ñ€Ğ°Ğ´Ğ¸ÑƒÑĞµ 5 ĞºĞ»ĞµÑ‚Ğ¾Ğº.",
      },
    ],
  },
  {
    id: "centaur",
    name: "ĞšĞµĞ½Ñ‚Ğ°Ğ²Ñ€ Ğ¡Ñ‚ĞµĞ¿ĞµĞ¹",
    baseHp: 620,
    hpPerLvl: 80,
    baseDmg: 72,
    dmgPerLvl: 8.5,
    baseArm: 12,
    armPerLvl: 2.0,
    cost: 350,
    rarity: "Ğ ĞµĞ´ĞºĞ¸Ğ¹",
    classType: "Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº",
    mainAttr: "Ğ›Ğ¾Ğ²ĞºĞ¾ÑÑ‚ÑŒ (AGI)",
    baseAttr: 26,
    attrPerLvl: 5.2,
    icon: "ğŸ¹",
    p: "Stunning 3D close-up face shot portrait of a wild steppe centaur warrior chieftain, braided hair, warpaint stripes on face, clay stylized render, flat white solid plain background (#ffffff).",
    passives: [
      {
        name: "Ğ’Ğ¾Ğ»Ñ Ğ Ğ°Ğ²Ğ½Ğ¸Ğ½",
        desc: "ĞŸĞµÑ€ĞµĞ´Ğ²Ğ¸Ğ¶ĞµĞ½Ğ¸Ğµ Ğ¿Ğ¾ Ğ·Ñ‹Ğ±ÑƒÑ‡Ğ¸Ğ¼ Ğ¿ĞµÑĞºĞ°Ğ¼ Ğ¸ Ğ±Ğ¾Ğ»Ğ¾Ñ‚Ñƒ Ğ½Ğµ Ñ€Ğ°ÑÑ…Ğ¾Ğ´ÑƒĞµÑ‚ Ğ»Ğ¸ÑˆĞ½Ğ¸Ğµ ĞĞ”.",
      },
      {
        name: "Ğ¡Ñ‚ĞµĞ¿Ğ½Ğ¾Ğ¹ ĞĞ±Ñ…Ğ¾Ğ´",
        desc: "Ğ¨Ğ°Ğ½Ñ Ğ½Ğ°Ğ½ĞµÑÑ‚Ğ¸ ĞºÑ€Ğ¸Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ ÑƒÑ€Ğ¾Ğ½ ÑĞ¾ ÑĞ¿Ğ¸Ğ½Ñ‹ Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞµĞ½ Ğ´Ğ¾ 45%.",
      },
      {
        name: "Ğ›ĞµĞ³ĞºĞ¸Ğµ ĞšĞ¾Ğ¿Ñ‹Ñ‚Ğ°",
        desc: "Ğ£ĞºĞ»Ğ¾Ğ½ĞµĞ½Ğ¸Ğµ Ğ¾Ñ‚ Ñ„Ğ¸Ğ·Ğ¸Ñ‡ĞµÑĞºĞ¸Ñ… ÑĞ½Ğ°Ñ€ÑĞ´Ğ¾Ğ² Ğ¸ ÑĞ´Ğ¾Ğ² Ğ¿Ğ¾Ğ²Ñ‹ÑˆĞµĞ½Ğ¾ Ğ½Ğ° +15%.",
      },
      {
        name: "ĞĞ¿ĞµÑ€ĞµĞ½Ğ½Ñ‹Ğµ Ğ¡Ñ‚Ñ€ĞµĞ»Ñ‹",
        desc: "ĞÑ‚Ğ°ĞºĞ° Ñ Ñ€Ğ°Ğ·Ğ³Ğ¾Ğ½Ğ° Ğ½Ğ°Ğ½Ğ¾ÑĞ¸Ñ‚ Ğ´Ğ¾Ğ¿Ğ¾Ğ»Ğ½Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğ¹ Ñ‡Ğ¸ÑÑ‚Ñ‹Ğ¹ ÑƒÑ€Ğ¾Ğ½.",
      },
    ],
    actives: [
      {
        name: "Ğ“Ñ€Ğ¾Ğ·Ğ¾Ğ²Ğ°Ñ Ğ¡Ñ‚Ñ€ĞµĞ»Ğ° (Ğ£Ğ»ÑŒÑ‚)",
        desc: "Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ» Ğ±ÑŒĞµÑ‚ Ñ†ĞµĞ¿Ğ½Ğ¾Ğ¹ Ğ¼Ğ¾Ğ»Ğ½Ğ¸ĞµĞ¹ Ğ´Ğ¾ 3 Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ²Ğ½Ğ¸ĞºĞ¾Ğ² Ğ½Ğ° 360% ÑƒÑ€Ğ¾Ğ½Ğ°.",
      },
      {
        name: "ĞŸĞµÑÑ‡Ğ°Ğ½Ğ°Ñ Ğ—Ğ°Ğ²ĞµÑĞ°",
        desc: "Ğ¡Ğ½Ğ¸Ğ¶Ğ°ĞµÑ‚ Ğ´Ğ°Ğ»ÑŒĞ½Ğ¾ÑÑ‚ÑŒ Ğ¾Ğ±Ğ·Ğ¾Ñ€Ğ° Ğ¸ ÑÑ‚Ñ€ĞµĞ»ÑŒĞ±Ñ‹ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¸Ñ… Ğ»ÑƒÑ‡Ğ½Ğ¸ĞºĞ¾Ğ² Ğ½Ğ° 3 ĞºĞ»ĞµÑ‚ĞºĞ¸.",
      },
    ],
  },
  {
    id: "necromancer",
    name: "ĞĞµĞºÑ€Ğ¾Ğ¼Ğ°Ğ½Ñ‚ Ğ¢ÑŒĞ¼Ñ‹",
    baseHp: 500,
    hpPerLvl: 60,
    baseDmg: 90,
    dmgPerLvl: 11,
    baseArm: 6,
    armPerLvl: 1.0,
    cost: 400,
    rarity: "Ğ›ĞµĞ³ĞµĞ½Ğ´Ğ°Ñ€Ğ½Ñ‹Ğ¹",
    classType: "ĞœĞ°Ğ³",
    mainAttr: "Ğ˜Ğ½Ñ‚ĞµĞ»Ğ»ĞµĞºÑ‚ (INT)",
    baseAttr: 32,
    attrPerLvl: 7.0,
    icon: "ğŸ’€",
    p: "Ominous close-up 3D face portrait profile mask of a master dark necromancer wizard, obsidian bone crown, glowing faint green spirit fire eyes, stylized clay render, pure white isolated background (#ffffff).",
    passives: [
      {
        name: "Ğ–Ğ°Ñ‚Ğ²Ğ° Ğ”ÑƒÑˆ",
        desc: "Ğ£Ğ±Ğ¸Ğ¹ÑÑ‚Ğ²Ğ¾ Ğ»ÑĞ±Ğ¾Ğ¹ ĞµĞ´Ğ¸Ğ½Ğ¸Ñ†Ñ‹ Ğ½Ğ° Ğ¿Ğ¾Ğ»Ğµ Ğ±Ğ¾Ñ Ğ¸ÑÑ†ĞµĞ»ÑĞµÑ‚ ĞĞµĞºÑ€Ğ¾Ğ¼Ğ°Ğ½Ñ‚Ğ° Ğ½Ğ° 12% HP.",
      },
      {
        name: "Ğ­Ñ„Ğ¸Ñ€Ğ½Ñ‹Ğµ Ğ ÑƒĞ½Ñ‹",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ ÑĞ¾Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ²Ğ»ĞµĞ½Ğ¸Ğµ Ğ¼Ğ°Ğ³Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¼ Ğ°Ñ‚Ğ°ĞºĞ°Ğ¼ Ğ½Ğ° +35%.",
      },
      {
        name: "Ğ§ÑƒĞ¼Ğ½Ğ¾Ğ¹ Ğ’ĞµÑ‚ĞµÑ€",
        desc: "ĞÑĞ»Ğ°Ğ±Ğ»ÑĞµÑ‚ ÑĞ¸Ğ»Ñƒ Ğ°Ñ‚Ğ°ĞºĞ¸ Ğ±Ğ»Ğ¸Ğ¶Ğ°Ğ¹ÑˆĞ¸Ñ… Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¸Ñ… Ğ¾Ñ‚Ñ€ÑĞ´Ğ¾Ğ² Ğ½Ğ° -15%.",
      },
      {
        name: "ĞšĞ¾ÑÑ‚ÑĞ½Ğ°Ñ Ğ­Ğ³Ğ¸Ğ´Ğ°",
        desc: "ĞšĞ°Ğ¶Ğ´Ñ‹Ğµ 3 Ñ…Ğ¾Ğ´Ğ° Ğ¿Ğ¾Ğ»ÑƒÑ‡Ğ°ĞµÑ‚ Ñ‰Ğ¸Ñ‚, Ğ±Ğ»Ğ¾ĞºĞ¸Ñ€ÑƒÑÑ‰Ğ¸Ğ¹ 100% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¾Ğ´Ğ½Ğ¾Ğ³Ğ¾ Ğ²Ñ‹ÑÑ‚Ñ€ĞµĞ»Ğ°.",
      },
      {
        name: "ĞŸÑ€Ğ¾Ğ²Ğ¸Ğ´ĞµĞ½Ğ¸Ğµ ĞœĞ¾Ğ³Ğ¸Ğ»",
        desc: "Ğ”Ğ°Ğ»ÑŒĞ½Ğ¾ÑÑ‚ÑŒ ÑĞ¾Ñ‚Ğ²Ğ¾Ñ€ĞµĞ½Ğ¸Ñ Ğ¿Ñ€Ğ¸Ğ·Ñ‹Ğ²Ğ¾Ğ² ÑƒĞ²ĞµĞ»Ğ¸Ñ‡ĞµĞ½Ğ° Ğ½Ğ° +2 ĞºĞ»ĞµÑ‚ĞºĞ¸.",
      },
    ],
    actives: [
      {
        name: "ĞŸÑ€Ğ¸Ğ·Ñ‹Ğ² Ğ–Ğ½ĞµÑ†Ğ° (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞŸÑ€Ğ¸Ğ·Ñ‹Ğ²Ğ°ĞµÑ‚ Ğ¼Ğ¾Ñ‰Ğ½Ğ¾Ğ³Ğ¾ Ğ¡ĞºĞµĞ»ĞµÑ‚Ğ°-ĞŸĞ°Ğ»Ğ°Ğ´Ğ¸Ğ½Ğ° 15 ÑƒÑ€Ğ¾Ğ²Ğ½Ñ Ğ½Ğ° Ğ¿Ğ¾Ğ»Ğµ Ğ±Ğ¾Ñ.",
      },
      {
        name: "ĞšĞ°ÑĞ°Ğ½Ğ¸Ğµ Ğ‘ĞµĞ·Ğ´Ğ½Ñ‹",
        desc: "ĞŸÑ€Ğ¾ĞºĞ»Ğ¸Ğ½Ğ°ĞµÑ‚ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¸Ğ¹ Ğ¿Ğ¾Ğ»Ğº, Ğ½Ğ°Ğ½Ğ¾ÑÑ 150% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¸ Ğ±Ğ»Ğ¾ĞºĞ¸Ñ€ÑƒÑ Ğ¸Ñ… Ğ¸ÑÑ†ĞµĞ»ĞµĞ½Ğ¸Ğµ.",
      },
      {
        name: "Ğ Ğ¸Ñ‚ÑƒĞ°Ğ» ĞšÑ€Ğ¾Ğ²Ğ¸",
        desc: "ĞŸĞ¾Ğ³Ğ»Ğ¾Ñ‰Ğ°ĞµÑ‚ Ğ·Ğ´Ğ¾Ñ€Ğ¾Ğ²ÑŒĞµ Ğ»ÑĞ±Ğ¾Ğ³Ğ¾ Ğ¿Ñ€Ğ¸Ğ·Ğ²Ğ°Ğ½Ğ½Ğ¾Ğ³Ğ¾ ÑÑƒÑ‰ĞµÑÑ‚Ğ²Ğ°, Ğ²Ğ¾ÑÑÑ‚Ğ°Ğ½Ğ°Ğ²Ğ»Ğ¸Ğ²Ğ°Ñ Ğ·Ğ´Ğ¾Ñ€Ğ¾Ğ²ÑŒĞµ Ğ½ĞµĞºÑ€Ğ¾Ğ¼Ğ°Ğ½Ñ‚Ñƒ.",
      },
    ],
  },
  {
    id: "griffin",
    name: "Ğ­Ğ»Ğ¸Ñ‚Ğ½Ñ‹Ğ¹ ĞšĞ¾Ñ€Ğ¾Ğ»ĞµĞ²ÑĞºĞ¸Ğ¹ Ğ“Ñ€Ğ¸Ñ„Ğ¾Ğ½",
    baseHp: 1200,
    hpPerLvl: 155,
    baseDmg: 110,
    dmgPerLvl: 13.5,
    baseArm: 24,
    armPerLvl: 3.5,
    cost: 500,
    rarity: "Ğ›ĞµĞ³ĞµĞ½Ğ´Ğ°Ñ€Ğ½Ñ‹Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 35,
    attrPerLvl: 8.0,
    icon: "ğŸ¦…",
    p: "Majestic 3D close-up headshot photo of an elite royal-griffin avian beast, golden sharp beak, white crown plumage, neon cyan runic collar, stylized octane render, flat white background (#ffffff).",
    passives: [
      {
        name: "ĞšÑ€Ñ‹Ğ»Ğ°Ñ‚Ğ°Ñ Ğ­Ñ„Ğ¸Ñ€Ğ½Ğ¾ÑÑ‚ÑŒ",
        desc: "ĞŸĞ¾Ğ»Ğ½Ñ‹Ğ¹ Ğ¿Ğ¾Ğ»ĞµÑ‚: Ğ¸Ğ³Ğ½Ğ¾Ñ€Ğ¸Ñ€ÑƒĞµÑ‚ Ğ³Ğ¾Ñ€Ñ‹, Ğ±Ğ¾Ğ»Ğ¾Ñ‚Ğ° Ğ¸ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¸Ğ¹ Ğ·Ğ°ÑĞ»Ğ¾Ğ½.",
      },
      {
        name: "Ğ’ÑÑ€ĞµÑ‡Ğ½Ñ‹Ğ¹ Ğ¡ĞºĞ²Ğ¾Ğ·Ğ½ÑĞº",
        desc: "ĞÑ‚Ñ€Ğ°Ğ¶Ğ°ĞµÑ‚ 25% ÑÑ‚Ñ€ĞµĞ» Ğ¾Ğ±Ñ€Ğ°Ñ‚Ğ½Ğ¾ Ğ²Ğ¾ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¾Ğ³Ğ¾ ÑÑ‚Ñ€ĞµĞ»ĞºĞ°.",
      },
      {
        name: "ĞŸĞµÑ€ÑŒĞµĞ²Ğ¾Ğ¹ ĞŸĞ°Ğ½Ñ†Ğ¸Ñ€ÑŒ",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ Ğ²Ñ‹Ğ¶Ğ¸Ğ²Ğ°ĞµĞ¼Ğ¾ÑÑ‚ÑŒ Ğ¿Ñ€Ğ¸ Ğ¿Ğ¾Ğ»ÑƒÑ‡ĞµĞ½Ğ¸Ğ¸ ĞºÑ€Ğ¸Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¸Ñ… ÑƒĞ´Ğ°Ñ€Ğ¾Ğ² Ğ½Ğ° +30%.",
      },
      {
        name: "ĞœĞµÑ€Ñ‚Ğ²Ğ°Ñ Ğ—Ğ¾Ğ½Ğ°",
        desc: "Ğ’Ñ€Ğ°Ğ³Ğ¸ Ğ¿Ğ¾Ğ´ Ğ³Ñ€Ğ¸Ñ„Ğ¾Ğ½Ğ¾Ğ¼ Ğ½Ğµ Ğ¼Ğ¾Ğ³ÑƒÑ‚ ÑĞ¾Ğ²ĞµÑ€ÑˆĞ°Ñ‚ÑŒ Ğ¾Ñ‚Ğ²ĞµÑ‚Ğ½Ñ‹Ğµ ÑƒĞ´Ğ°Ñ€Ñ‹.",
      },
      {
        name: "Ğ£Ğ¿Ñ€ĞµĞ¶Ğ´ĞµĞ½Ğ¸Ğµ Ğ¡Ğ²ĞµÑ€Ñ…Ñƒ",
        desc: "ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ Ğ½Ğ° +40% Ğ±Ğ¾Ğ»ÑŒÑˆĞµ ÑƒÑ€Ğ¾Ğ½Ğ° Ñ†ĞµĞ»ÑĞ¼ Ñ Ğ½Ğ¸Ğ·ĞºĞ¸Ğ¼ Ğ·Ğ´Ğ¾Ñ€Ğ¾Ğ²ÑŒĞµĞ¼.",
      },
    ],
    actives: [
      {
        name: "ĞĞµĞ±ĞµÑĞ½Ñ‹Ğ¹ ĞšĞ¾Ğ³Ğ¾Ñ‚ÑŒ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "ĞĞ±Ñ€ÑƒÑˆĞ¸Ğ²Ğ°ĞµÑ‚ÑÑ Ğ²Ğ¸Ñ…Ñ€ĞµĞ¼ Ğ½Ğ° Ğ²Ñ€Ğ°Ğ³Ğ°, Ğ½Ğ°Ğ½Ğ¾ÑÑ 480% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¸ Ğ´ĞµĞ·Ğ¾Ñ€Ğ¸ĞµĞ½Ñ‚Ğ¸Ñ€ÑƒÑ Ñ†ĞµĞ»ÑŒ.",
      },
      {
        name: "Ğ¡Ğ²ĞµĞ¶ĞµĞµ ĞĞ¿ĞµÑ€ĞµĞ½Ğ¸Ğµ",
        desc: "Ğ Ğ°ÑĞ¿Ñ€Ğ°Ğ²Ğ»ÑĞµÑ‚ ĞºÑ€Ñ‹Ğ»ÑŒÑ, Ğ¼Ğ³Ğ½Ğ¾Ğ²ĞµĞ½Ğ½Ğ¾ Ğ¸ÑÑ†ĞµĞ»ÑÑ ÑĞµĞ±Ñ Ğ½Ğ° 300 HP.",
      },
      {
        name: "Ğ“Ğ¾Ñ€Ğ½Ñ‹Ğ¹ ĞšĞ»Ğ¸Ñ‡",
        desc: "ĞŸÑƒĞ³Ğ°ĞµÑ‚ Ğ²Ñ€Ğ°Ğ³Ğ¾Ğ² Ğ² Ñ€Ğ°Ğ´Ğ¸ÑƒÑĞµ 3 ĞºĞ»ĞµÑ‚Ğ¾Ğº, ÑĞ½Ğ¸Ğ¶Ğ°Ñ Ğ¸Ñ… ĞÑ‡ĞºĞ¸ Ğ”ĞµĞ¹ÑÑ‚Ğ²Ğ¸Ñ Ğ½Ğ° 2.",
      },
    ],
  },
  {
    id: "lord_knight",
    name: "Ğ Ñ‹Ñ†Ğ°Ñ€ÑŒ-Ğ’Ğ»Ğ°ÑÑ‚ĞµĞ»Ğ¸Ğ½",
    baseHp: 1500,
    hpPerLvl: 200,
    baseDmg: 140,
    dmgPerLvl: 17.5,
    baseArm: 35,
    armPerLvl: 5.5,
    cost: 600,
    rarity: "Ğ›ĞµĞ³ĞµĞ½Ğ´Ğ°Ñ€Ğ½Ñ‹Ğ¹",
    classType: "Ğ’Ğ¾Ğ¸Ğ½",
    mainAttr: "Ğ¡Ğ¸Ğ»Ğ° (STR)",
    baseAttr: 45,
    attrPerLvl: 10.0,
    icon: "ğŸ‘‘",
    p: "Symmetrical closeup headshot photo of a powerful legend Lord Knight commander, royal dark slate steel helmet, neon cyan dragon gemstone glow, stylized 3D sculpt clay render, isolated on flat pure white background (#ffffff), zero floor shadow.",
    passives: [
      {
        name: "ĞÑƒÑ€Ğ° Ğ’Ğ»Ğ°ÑÑ‚ĞµĞ»Ğ¸Ğ½Ğ°",
        desc: "ĞŸĞ¾Ğ²Ñ‹ÑˆĞ°ĞµÑ‚ Ğ±Ğ¾ĞµĞ²ÑƒÑ Ğ²Ğ¾Ğ»Ñ Ğ²ÑĞµĞ¹ Ğ°Ñ€Ğ¼Ğ¸Ğ¸, ÑƒĞ²ĞµĞ»Ğ¸Ñ‡Ğ¸Ğ²Ğ°Ñ ÑƒÑ€Ğ¾Ğ½ ÑĞ¾ÑĞ·Ğ½Ğ¸ĞºĞ¾Ğ² Ğ½Ğ° +20%.",
      },
      {
        name: "Ğ­Ğ³Ğ¸Ğ´Ğ° Ğ”Ñ€Ğ°ĞºĞ¾Ğ½Ğ°",
        desc: "ĞŸĞ¾Ğ»Ğ½Ñ‹Ğ¹ Ğ¸Ğ¼Ğ¼ÑƒĞ½Ğ¸Ñ‚ĞµÑ‚ Ğº Ğ¾Ğ³Ğ»ÑƒÑˆĞµĞ½Ğ¸Ñ, Ğ¾Ğ±Ğ¼Ğ¾Ñ€Ğ¾Ğ¶ĞµĞ½Ğ¸Ñ, ÑĞ´Ğ°Ğ¼ Ğ¸ Ğ½ĞµĞ¼Ğ¾Ñ‚Ğµ.",
      },
      {
        name: "ĞšÑ€ÑƒĞ³Ğ¾Ğ²Ğ¾Ğµ Ğ¡ĞµÑ‡ĞµĞ½Ğ¸Ğµ",
        desc: "ĞšĞ°Ğ¶Ğ´Ğ°Ñ Ğ¼ĞµÑ…Ğ°Ğ½Ğ¸Ñ‡ĞµÑĞºĞ°Ñ Ğ°Ñ‚Ğ°ĞºĞ° Ğ½Ğ°Ğ½Ğ¾ÑĞ¸Ñ‚ 50% ÑĞ¾Ğ¿ÑƒÑ‚ÑÑ‚Ğ²ÑƒÑÑ‰ĞµĞ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ° Ğ²ÑĞµĞ¼ Ñ„Ğ»Ğ°Ğ½Ğ³Ğ°Ğ¼.",
      },
      {
        name: "ĞĞ±Ñ€ÑĞ´ ĞĞµÑƒÑĞ·Ğ²Ğ¸Ğ¼Ğ¾ÑÑ‚Ğ¸",
        desc: "ĞŸÑ€Ğ¸ Ğ¿Ğ¾Ğ»ÑƒÑ‡ĞµĞ½Ğ¸Ğ¸ ÑĞ¼ĞµÑ€Ñ‚ĞµĞ»ÑŒĞ½Ğ¾Ğ³Ğ¾ ÑƒĞ´Ğ°Ñ€Ğ° ÑÑ‚Ğ°Ğ½Ğ¾Ğ²Ğ¸Ñ‚ÑÑ Ğ½ĞµÑƒÑĞ·Ğ²Ğ¸Ğ¼ Ğ½Ğ° 1 Ñ…Ğ¾Ğ´.",
      },
      {
        name: "ĞœĞ°Ğ³Ğ¸Ñ‡ĞµÑĞºĞ¾Ğµ Ğ¡Ğ»Ğ¾Ğ²Ğ¾",
        desc: "ĞŸÑ€Ğ¸ Ğ¿Ñ€Ğ¾Ğ¿ÑƒÑĞºĞµ Ñ…Ğ¾Ğ´Ğ° Ğ²Ğ¾ÑÑÑ‚Ğ°Ğ½Ğ°Ğ²Ğ»Ğ¸Ğ²Ğ°ĞµÑ‚ 10% Ğ·Ğ´Ğ¾Ñ€Ğ¾Ğ²ÑŒÑ ĞºĞ°Ğ·Ğ½Ğµ.",
      },
    ],
    actives: [
      {
        name: "ĞœĞµÑ‡ ĞŸÑ€Ğ°Ğ²Ğ¾ÑÑƒĞ´Ğ¸Ñ (Ğ£Ğ»ÑŒÑ‚)",
        desc: "Ğ’Ğ¾Ğ½Ğ·Ğ°ĞµÑ‚ ĞºĞ»Ğ¸Ğ½Ğ¾Ğº Ğ² Ğ·ĞµĞ¼Ğ»Ñ, Ğ½Ğ°Ğ½Ğ¾ÑÑ 600% ÑƒÑ€Ğ¾Ğ½Ğ° Ğ²ÑĞµĞ¼ Ğ²Ñ€Ğ°Ğ³Ğ°Ğ¼ Ğ¸ ÑĞ¶Ğ¸Ğ³Ğ°Ñ Ğ¼Ğ°Ğ½Ñƒ.",
      },
      {
        name: "Ğ“Ñ€Ğ¾Ğ·Ğ¾Ğ²Ğ¾Ğ¹ Ğ¨Ğ¿Ğ¸Ğ»ÑŒ",
        desc: "ĞĞ°Ğ²ĞµÑˆĞ¸Ğ²Ğ°ĞµÑ‚ Ñ‰Ğ¸Ñ‚ Ğ½Ğ° Ğ¿Ğ°Ğ»Ğ°Ğ´Ğ¸Ğ½Ğ°, Ğ¿Ğ¾Ğ³Ğ»Ğ¾Ñ‰Ğ°ÑÑ‰Ğ¸Ğ¹ Ğ´Ğ¾ 1000 ÑƒÑ€Ğ¾Ğ½Ğ°.",
      },
      {
        name: "ĞšĞ¾Ñ€Ğ¾Ğ»ĞµĞ²ÑĞºĞ¸Ğ¹ Ğ¡Ğ¾Ğ·Ñ‹Ğ²",
        desc: "ĞŸÑ€Ğ¸Ğ·Ñ‹Ğ²Ğ°ĞµÑ‚ Ğ±Ğ¾ĞµĞ²Ñ‹Ğµ Ğ´ÑƒÑ…Ğ¸ Ğ¿Ğ°Ğ²ÑˆĞ¸Ñ… Ğ²Ğ¾Ğ¸Ğ½Ğ¾Ğ² Ğ½Ğ° Ğ¿Ğ¾Ğ»Ğµ Ğ±Ğ¾Ñ Ğ´Ğ»Ñ Ñ„Ğ¸Ğ½Ğ°Ğ»ÑŒĞ½Ğ¾Ğ¹ Ğ°Ñ‚Ğ°ĞºĞ¸.",
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
    // ĞĞ²Ñ‚Ğ¾Ğ¼Ğ°Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ°Ñ ÑĞ¸Ğ½Ñ…Ñ€Ğ¾Ğ½Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ Ğ²ĞµÑ€ÑĞ¸Ğ¸ Ñ ÑĞµÑ€Ğ²ĞµÑ€Ğ¾Ğ¼
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
                  "Ğ˜Ğ·Ğ¾Ğ±Ñ€Ğ°Ğ¶ĞµĞ½Ğ¸Ğµ ÑƒÑĞ¿ĞµÑˆĞ½Ğ¾ Ğ²ÑÑ‚Ğ°Ğ²Ğ»ĞµĞ½Ğ¾ Ğ¸Ğ· Ğ±ÑƒÑ„ĞµÑ€Ğ° Ğ¾Ğ±Ğ¼ĞµĞ½Ğ°!",
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
    showNotification("Ğ—Ğ°Ğ³Ñ€ÑƒĞ¶ĞµĞ½ Ğ¸Ğ½Ñ‚ĞµÑ€Ğ°ĞºÑ‚Ğ¸Ğ²Ğ½Ñ‹Ğ¹ Ñ‚ĞµÑÑ‚Ğ¾Ğ²Ñ‹Ğ¹ Ğ¿Ñ€Ğ¸Ğ¼ĞµÑ€!", "info");
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
        `Ğ£ÑĞ¿ĞµÑˆĞ½Ğ¾ ÑĞ¾Ñ…Ñ€Ğ°Ğ½ĞµĞ½Ğ¾: ${fileName} (Ğ±ĞµĞ· Ğ¿Ğ¾Ñ‚ĞµÑ€Ğ¸ ĞºĞ°Ñ‡ĞµÑÑ‚Ğ²Ğ°!)`,
        "success",
      );
    } catch (err) {
      showNotification(
        "ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ ÑĞ¾Ñ…Ñ€Ğ°Ğ½ĞµĞ½Ğ¸Ğ¸ Ğ¸Ğ·Ğ¾Ğ±Ñ€Ğ°Ğ¶ĞµĞ½Ğ¸Ñ. ĞŸĞ¾Ğ¿Ñ€Ğ¾Ğ±ÑƒĞ¹Ñ‚Ğµ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·Ğ¸Ñ‚ÑŒ Ğ»Ğ¾ĞºĞ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ñ„Ğ°Ğ¹Ğ».",
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
  const [language, setLanguage] = useState("Ğ ÑƒÑÑĞºĞ¸Ğ¹");

  const t = {
    Ğ ÑƒÑÑĞºĞ¸Ğ¹: {
      play: "Ğ˜Ğ³Ñ€Ğ°Ñ‚ÑŒ",
      settings: "ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸",
      exit: "Ğ’Ñ‹Ñ…Ğ¾Ğ´",
      back: "ĞĞ°Ğ·Ğ°Ğ´",
      volume: "Ğ—Ğ²ÑƒĞº",
      music: "ĞœÑƒĞ·Ñ‹ĞºĞ°",
      quality: "ĞšĞ°Ñ‡ĞµÑÑ‚Ğ²Ğ¾",
      res: "Ğ Ğ°Ğ·Ñ€ĞµÑˆĞµĞ½Ğ¸Ğµ",
      fs: "Ğ’ĞµÑÑŒ ÑĞºÑ€Ğ°Ğ½",
      graphics: "Ğ“Ñ€Ğ°Ñ„Ğ¸ĞºĞ°",
      lang: "Ğ¯Ğ·Ñ‹Ğº",
      help: "ĞŸĞ¾Ğ¼Ğ¾Ñ‰ÑŒ ĞŸĞ¾ Ğ˜Ğ³Ñ€Ğµ",
      capabilities: "Ğ’Ğ¾Ğ·Ğ¼Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸ Ğ˜Ğ˜",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Ğ¡Ğ¸Ğ½Ñ‚Ğ°ĞºÑĞ¸Ñ Ğ¡Ğ¸Ğ½Ğ³ÑƒĞ»ÑÑ€Ğ½Ğ¾ÑÑ‚Ğ¸",
      offline: "Ğ—Ğ°Ñ‰Ğ¸Ñ‰ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ ĞµĞ¶Ğ¸Ğ¼",
      clear: "ĞÑ‡Ğ¸ÑÑ‚Ğ¸Ñ‚ÑŒ",
      clearing: "ĞÑ‡Ğ¸ÑÑ‚ĞºĞ°...",
      thinking: "Cortex Matrix Analysis",
      synth: "Ğ¡Ğ¸Ğ½Ñ‚ĞµĞ· Ğ´Ğ°Ğ½Ğ½Ñ‹Ñ… Unity 6 & Blender 4.3",
      proMastery: "Professional Multi-Tool Mastery",
      downloadBg: "Ğ¡ĞºĞ°Ñ‡Ğ°Ñ‚ÑŒ Ğ¤Ğ¾Ğ½ (JPG 4K)",
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
      back: "ZurÃ¼ck",
      volume: "Ton",
      music: "Musik",
      quality: "QualitÃ¤t",
      res: "AuflÃ¶sung",
      fs: "Vollbild",
      graphics: "Grafik",
      lang: "Sprache",
      help: "Spielhilfe",
      capabilities: "KI-FÃ¤higkeiten",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "SingularitÃ¤ts-Syntax",
      offline: "Gesicherter Modus",
      clear: "LÃ¶schen",
      clearing: "LÃ¶schen...",
      thinking: "Cortex-Matrix-Analyse",
      synth: "Synthese von Unity 6 & Blender 4.3 Daten",
      proMastery: "Professionelle Multi-Tool-Meisterschaft",
      downloadBg: "Hintergrund Herunterladen (JPG 4K)",
    },
    FranÃ§ais: {
      play: "Jouer",
      settings: "ParamÃ¨tres",
      exit: "Quitter",
      back: "Retour",
      volume: "Son",
      music: "Musique",
      quality: "QualitÃ©",
      res: "RÃ©solution",
      fs: "Plein Ã©cran",
      graphics: "Graphisme",
      lang: "Langue",
      help: "Aide au Jeu",
      capabilities: "CapacitÃ©s de l'IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Syntaxe de SingularitÃ©",
      offline: "Mode SÃ©curisÃ©",
      clear: "Effacer",
      clearing: "Effacement...",
      thinking: "Analyse de la Matrice Cortex",
      synth: "SynthÃ¨se des donnÃ©es Unity 6 & Blender 4.3",
      proMastery: "MaÃ®trise Professionnelle Multi-Tool",
      downloadBg: "TÃ©lÃ©charger le Fond (JPG 4K)",
    },
    EspaÃ±ol: {
      play: "Jugar",
      settings: "Ajustes",
      exit: "Salir",
      back: "Volver",
      volume: "Sonido",
      music: "MÃºsica",
      quality: "Calidad",
      res: "ResoluciÃ³n",
      fs: "Pantalla completa",
      graphics: "GrÃ¡ficos",
      lang: "Idioma",
      help: "Ayuda del Juego",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxis de Singularidad",
      offline: "Modo Seguro",
      clear: "Limpiar",
      clearing: "Limpiando...",
      thinking: "AnÃ¡lisis de la Matriz Cortex",
      synth: "Sintetizando datos de Unity 6 y Blender 4.3",
      proMastery: "MaestrÃ­a Profesional Multiherramienta",
      downloadBg: "Descargar Fondo (JPG 4K)",
    },
    æ—¥æœ¬èª: {
      play: "ãƒ—ãƒ¬ã‚¤",
      settings: "è¨­å®š",
      exit: "çµ‚äº†",
      back: "æˆ»ã‚‹",
      volume: "éŸ³é‡",
      music: "éŸ³æ¥½",
      quality: "å“è³ª",
      res: "è§£åƒåº¦",
      fs: "å…¨ç”»é¢",
      graphics: "ã‚°ãƒ©ãƒ•ã‚£ãƒƒã‚¯",
      lang: "è¨€èª",
      help: "ã‚²ãƒ¼ãƒ ãƒ˜ãƒ«ãƒ—",
      capabilities: "AIæ©Ÿèƒ½",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: ã‚ªãƒ•",
      sync: "ã‚·ãƒ³ã‚®ãƒ¥ãƒ©ãƒªãƒ†ã‚£æ§‹æ–‡",
      offline: "ã‚»ã‚­ãƒ¥ã‚¢ãƒ¢ãƒ¼ãƒ‰",
      clear: "ã‚¯ãƒªã‚¢",
      clearing: "ã‚¯ãƒªã‚¢ä¸­...",
      thinking: "çš®è³ªãƒãƒˆãƒªãƒƒã‚¯ã‚¹åˆ†æ",
      synth: "Unity 6ã¨Blender 4.3ã®ãƒ‡ãƒ¼ã‚¿ã‚’çµ±åˆä¸­",
      proMastery: "ãƒ—ãƒ­ãƒ•ã‚§ãƒƒã‚·ãƒ§ãƒŠãƒ«ãƒãƒ«ãƒãƒ„ãƒ¼ãƒ«ãƒã‚¹ã‚¿ãƒªãƒ¼",
      downloadBg: "èƒŒæ™¯ã‚’ä¸‹è½½ (JPG 4K)",
    },
    í•œêµ­ì–´: {
      play: "í”Œë ˆì´",
      settings: "ì„¤ì •",
      exit: "ë‚˜ê°€ê¸°",
      back: "ë’¤ë¡œ",
      volume: "ì†Œë¦¬",
      music: "ìŒì•…",
      quality: "í’ˆì§ˆ",
      res: "í•´ìƒë„",
      fs: "ì „ì²´ í™”ë©´",
      graphics: "ê·¸ë˜í”½",
      lang: "ì–¸ì–´",
      help: "ê²Œì„ ë„ì›€ë§",
      capabilities: "AI ëŠ¥ë ¥",
      ollama: "Ollama: í™•ì¸",
      ollamaOff: "Ollama: êº¼ì§",
      sync: "íŠ¹ì´ì  êµ¬ë¬¸",
      offline: "ë³´ì•ˆ ëª¨ë“œ",
      clear: "ì§€ìš°ê¸°",
      clearing: "ì§€ìš°ëŠ” ì¤‘...",
      thinking: "í”¼ì§ˆ ë§¤íŠ¸ë¦­ìŠ¤ ë¶„ì„",
      synth: "Unity 6 ë° Blender 4.3 ë°ì´í„° í•©ì„± ì¤‘",
      proMastery: "ì „ë¬¸ ë©€í‹° íˆ´ ë§ˆìŠ¤í„°ë¦¬",
      downloadBg: "ë°°ê²½ ë‹¤ìš´ë¡œë“œ (JPG 4K)",
    },
    ç®€ä½“ä¸­æ–‡: {
      play: "å¼€å§‹",
      settings: "è®¾ç½®",
      exit: "é€€å‡º",
      back: "è¿”å›",
      volume: "éŸ³é‡",
      music: "éŸ³ä¹",
      quality: "ç”»è´¨",
      res: "åˆ†è¾¨ç‡",
      fs: "å…¨å±",
      graphics: "å›¾åƒ",
      lang: "è¯­è¨€",
      help: "æ¸¸æˆå¸®åŠ©",
      capabilities: "AI èƒ½åŠ›",
      ollama: "Ollama: æ­£å¸¸",
      ollamaOff: "Ollama: å…³é—­",
      sync: "å¥‡ç‚¹è¯­æ³•",
      offline: "å®‰å…¨æ¨¡å¼",
      clear: "æ¸…é™¤",
      clearing: "æ­£åœ¨æ¸…é™¤...",
      thinking: "çš®å±‚çŸ©é˜µåˆ†æ",
      synth: "ç»¼åˆ Unity 6 & Blender 4.3 æ•°æ®",
      proMastery: "ä¸“ä¸šå¤šå·¥å…·å¤§å¸ˆçº§",
      downloadBg: "ä¸‹è½½èƒŒæ™¯ (JPG 4K)",
    },
    PortuguÃªs: {
      play: "Jogar",
      settings: "ConfiguraÃ§Ãµes",
      exit: "Sair",
      back: "Voltar",
      volume: "Som",
      music: "MÃºsica",
      quality: "Qualidade",
      res: "ResoluÃ§Ã£o",
      fs: "Tela cheia",
      graphics: "GrÃ¡ficos",
      lang: "Idioma",
      help: "Ajuda de Jogo",
      capabilities: "Capacidades de IA",
      ollama: "Ollama: OK",
      ollamaOff: "Ollama: Off",
      sync: "Sintaxe de Singularidade",
      offline: "Modo Seguro",
      clear: "Limpar",
      clearing: "Limpando...",
      thinking: "AnÃ¡lise da Matriz Cortex",
      synth: "Sintetizando dados de Unity 6 e Blender 4.3",
      proMastery: "DomÃ­nio de Multi-Ferramentas Profissional",
      downloadBg: "Baixar Fundo (JPG 4K)",
    },
  }[language as keyof any] || {
    play: "Ğ˜Ğ³Ñ€Ğ°Ñ‚ÑŒ",
    settings: "ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸",
    exit: "Ğ’Ñ‹Ñ…Ğ¾Ğ´",
    back: "ĞĞ°Ğ·Ğ°Ğ´",
    volume: "Ğ—Ğ²ÑƒĞº",
    music: "ĞœÑƒĞ·Ñ‹ĞºĞ°",
    quality: "ĞšĞ°Ñ‡ĞµÑÑ‚Ğ²Ğ¾",
    res: "Ğ Ğ°Ğ·Ñ€ĞµÑˆĞµĞ½Ğ¸Ğµ",
    fs: "Ğ’ĞµÑÑŒ ÑĞºÑ€Ğ°Ğ½",
    graphics: "Ğ“Ñ€Ğ°Ñ„Ğ¸ĞºĞ°",
    lang: "Ğ¯Ğ·Ñ‹Ğº",
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
    useState<string>("Ğ¡Ñ€ĞµĞ´Ğ½Ğ¸Ğ¹");
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
    "Ğ”ĞµĞ½ÑŒ 1: Ğ›Ğ¾Ñ€Ğ´ ĞœĞµĞ»ÑŒĞ³Ğ°Ñ€Ğ´ (Ğ˜Ğ˜) Ğ¿Ğ¾Ğ»ÑƒÑ‡Ğ¸Ğ» Ğ¿Ğ°ÑÑĞ¸Ğ²Ğ½Ğ¾Ğµ Ğ·Ğ¾Ğ»Ğ¾Ñ‚Ğ¾ (+60).",
    "Ğ”ĞµĞ½ÑŒ 1: Ğ›Ğ¾Ñ€Ğ´ ĞœĞµĞ»ÑŒĞ³Ğ°Ñ€Ğ´ Ğ·Ğ°Ğ²ĞµÑ€Ğ±Ğ¾Ğ²Ğ°Ğ» 4 Ğ»ÑƒÑ‡Ğ½Ğ¸ĞºĞ¾Ğ² Ğ²Ğ¾ Ğ²Ñ‚Ğ¾Ñ€Ğ¾Ğ¼ Ğ·Ğ°Ğ¼ĞºĞµ.",
  ]);

  const [equippedItems, setEquippedItems] = useState<
    Record<string, { name: string; bonus: string; icon: string }>
  >({
    helmet: { name: "ĞŸÑ€Ğ¾ÑˆĞ¸Ñ‚Ñ‹Ğ¹ ĞºĞ¾Ğ¶Ğ°Ğ½Ñ‹Ğ¹ Ñ‡ĞµĞ¿ĞµÑ†", bonus: "+3 Ğ‘Ñ€Ğ¾Ğ½Ñ", icon: "ğŸª–" },
    armor: {
      name: "Ğ–ĞµĞ»ĞµĞ·Ğ½Ñ‹Ğ¹ Ğ½Ğ°Ğ³Ñ€ÑƒĞ´Ğ½Ğ¸Ğº Ğ°Ğ²Ğ°Ğ½Ğ³Ğ°Ñ€Ğ´Ğ°",
      bonus: "+15 Ğ‘Ñ€Ğ¾Ğ½Ñ",
      icon: "ğŸ‘•",
    },
    weapon: { name: "Ğ¡Ñ‚Ğ°Ğ»ÑŒĞ½Ğ¾Ğ¹ Ğ¼ĞµÑ‡ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹", bonus: "+25 Ğ¡Ğ¸Ğ»Ğ°", icon: "ğŸ—¡ï¸" },
    shield: { name: "ĞŸÑ€Ğ¸Ñ‚Ğ¾Ñ‡Ğ½Ñ‹Ğ¹ Ñ‰Ğ¸Ñ‚ ĞĞ»ÑŒÑĞ½ÑĞ°", bonus: "+8 Ğ‘Ğ»Ğ¾Ğº", icon: "ğŸ›¡ï¸" },
    boots: { name: "ĞšĞ¾Ğ²Ğ°Ğ½Ñ‹Ğµ Ğ»Ğ°Ñ‚Ğ½Ñ‹Ğµ ÑĞ°Ğ¿Ğ¾Ğ³Ğ¸", bonus: "+10 Ğ¡ĞºĞ¾Ñ€.", icon: "ğŸ‘¢" },
    ring: { name: "ĞŸĞµÑ€ÑÑ‚ĞµĞ½ÑŒ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ°", bonus: "+35 ĞœĞ°Ğ½Ğ°", icon: "ğŸ’" },
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
      nameRU: "Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğ¹ Ğ“ĞµÑ€Ğ¾Ğ¹ (ĞŸĞ¾ĞºĞ¾Ñ€Ğ¸Ñ‚ĞµĞ»ÑŒ)",
      nameEN: "Main Vanguard Hero",
    },
    simple: {
      lvl: 3,
      xp: 120,
      nameRU: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹ Ğ“ĞµÑ€Ğ¾Ğ¹ (Ğ¡Ñ‚ĞµĞ¿Ğ½Ğ¾Ğ¹ Ğ’Ğ¾Ğ¶Ğ°Ğº)",
      nameEN: "Simple Plains Leader",
    },
    principle: {
      lvl: 4,
      xp: 180,
      nameRU: "ĞŸÑ€Ğ¸Ğ½Ñ†Ğ¸Ğ¿Ğ¸Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ“ĞµÑ€Ğ¾Ğ¹ (Ğ­Ğ¼Ğ¸ÑÑĞ°Ñ€)",
      nameEN: "Principle High Oracle",
    },
    troops: {
      lvl: 2,
      xp: 90,
      nameRU: "Ğ¡Ğ¾ÑĞ·Ğ½Ñ‹Ğµ Ğ ĞµĞ³ÑƒĞ»ÑÑ€Ğ½Ñ‹Ğµ Ğ’Ğ¾Ğ¹ÑĞºĞ°",
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
    if (d === "ĞĞ¾Ğ²Ğ¸Ñ‡Ğ¾Ğº" || d === "Novice") {
      setPlayerGold(1000);
    } else if (d === "Ğ›ĞµĞ³ĞºĞ¸Ğ¹" || d === "Ğ›ĞµĞ³ĞºĞ¾" || d === "Easy") {
      setPlayerGold(800);
    } else if (d === "Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ñ‹Ğ¹" || d === "Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ğ¾" || d === "Hard") {
      setPlayerGold(300);
    } else if (d === "ĞšĞ¾ÑˆĞ¼Ğ°Ñ€" || d === "Nightmare") {
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
      showNotification("ĞĞµ ÑƒĞ´Ğ°Ğ»Ğ¾ÑÑŒ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·Ğ¸Ñ‚ÑŒ Ğ¸Ğ½Ñ„Ğ¾Ñ€Ğ¼Ğ°Ñ†Ğ¸Ñ Ğ¾ Ğ¿Ğ°ĞºĞµÑ‚Ğ°Ñ….", "error");
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
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ğ¸ Ñ€ÑƒĞºĞ¾Ğ²Ğ¾Ğ´ÑÑ‚Ğ²Ğ°.", "error");
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
      showNotification("ğŸ“¸ Ğ˜Ğ·Ğ¾Ğ±Ñ€Ğ°Ğ¶ĞµĞ½Ğ¸Ğµ ÑƒÑĞ¿ĞµÑˆĞ½Ğ¾ Ğ·Ğ°Ğ½ĞµÑĞµĞ½Ğ¾ Ğ² Ğ¾ĞºĞ¾ÑˆĞºĞ¾!", "success");
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
        showNotification("Ğ£ Ğ²Ğ°Ñ ÑƒĞ¶Ğµ ÑƒÑÑ‚Ğ°Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ° Ğ¿Ğ¾ÑĞ»ĞµĞ´Ğ½ÑÑ Ğ²ĞµÑ€ÑĞ¸Ñ!", "info");
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
      "ĞŸÑ€Ğ¾Ğ²ĞµÑ€ĞºĞ° Ñ†ĞµĞ»Ğ¾ÑÑ‚Ğ½Ğ¾ÑÑ‚Ğ¸ Ñ„Ğ°Ğ¹Ğ»Ğ¾Ğ²...",
      "Ğ“Ğ»ÑƒĞ±Ğ¾ĞºĞ¾Ğµ ÑĞºĞ°Ğ½Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ° (ĞÑƒĞ´Ğ¸Ñ‚)...",
      "Ğ¡Ğ¸Ğ½Ñ…Ñ€Ğ¾Ğ½Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ Ñ Ğ»Ğ¾ĞºĞ°Ğ»ÑŒĞ½Ñ‹Ğ¼ Ñ…Ñ€Ğ°Ğ½Ğ¸Ğ»Ğ¸Ñ‰ĞµĞ¼...",
      "Ğ˜ÑĞ¿Ñ€Ğ°Ğ²Ğ»ĞµĞ½Ğ¸Ğµ Ğ½Ğ°Ğ¹Ğ´ĞµĞ½Ğ½Ñ‹Ñ… Ğ¾ÑˆĞ¸Ğ±Ğ¾Ğº...",
      "ĞĞ±Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ¸Ğµ Ğ²ĞµÑ€ÑĞ¸Ğ¸ Ğ´Ğ¾ 18.5.0...",
      "Ğ˜Ğ½Ğ¸Ñ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ Omniversal Quantum Link...",
      "Ğ£ÑÑ‚Ğ°Ğ½Ğ¾Ğ²ĞºĞ° ĞĞµĞ¹Ñ€Ğ¾Ğ½Ğ½Ğ¾Ğ³Ğ¾ ĞœĞ¾ÑÑ‚Ğ° (Blender & Unity)...",
      "Ğ ĞµĞ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ñ PROJECT_MASTER_BLUEPRINT.md (Quantum Link)...",
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
        "ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ ÑĞ¸Ğ½Ñ…Ñ€Ğ¾Ğ½Ğ¸Ğ·Ğ°Ñ†Ğ¸Ğ¸. ĞŸĞ¾Ğ¿Ñ€Ğ¾Ğ±ÑƒĞ¹Ñ‚Ğµ ĞµÑ‰Ğµ Ñ€Ğ°Ğ·.",
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
        "ĞšĞ°Ğº ÑĞ¾Ğ·Ğ´Ğ°Ñ‚ÑŒ Ğ¼Ğ¾Ğ´ÑƒĞ»ÑŒĞ½Ğ¾Ğµ Ğ·Ğ´Ğ°Ğ½Ğ¸Ğµ Ğ² Blender?",
        "Unity DOTS: Ğ¾ÑĞ½Ğ¾Ğ²Ñ‹ Ğ¾Ğ¿Ñ‚Ğ¸Ğ¼Ğ¸Ğ·Ğ°Ñ†Ğ¸Ğ¸",
        "Ğ¡Ğ¾Ğ·Ğ´Ğ°Ğ¹ ÑĞºÑ€Ğ¸Ğ¿Ñ‚ Ğ´Ğ»Ñ Ğ¿Ğ¾Ğ²ĞµĞ´ĞµĞ½Ğ¸Ñ Ğ²Ñ€Ğ°Ğ³Ğ¾Ğ² (Unity)",
        "Ğ“ĞµĞ¾Ğ¼ĞµÑ‚Ñ€Ğ¸Ñ‡ĞµÑĞºĞ¸Ğµ Ğ½Ğ¾Ğ´Ñ‹: Ğ¿Ñ€Ğ¾Ñ†ĞµĞ´ÑƒÑ€Ğ½Ñ‹Ğ¹ Ğ³Ğ¾Ñ€Ğ¾Ğ´",
        "ĞšĞ°Ğº Ğ¿ĞµÑ€ĞµĞ½ĞµÑÑ‚Ğ¸ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚ Ğ¸Ğ· Unity Ğ² Redot?",
        "ĞšĞ°Ğº Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°ĞµÑ‚ Neural Memory?",
        "ĞšĞ°Ğº Ğ½Ğ°ÑÑ‚Ñ€Ğ¾Ğ¸Ñ‚ÑŒ Quantum Link?",
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
          description: "Ğ“Ğ¸Ğ±Ñ€Ğ¸Ğ´Ğ½Ñ‹Ğ¹ Ğ˜Ğ˜-Ğ¿Ğ¾Ğ¼Ğ¾Ñ‰Ğ½Ğ¸Ğº Ñ Quantum Link",
          project_path: "Unknown",
          system_instruction: "Ğ¢Ñ‹ â€” ÑĞºÑĞ¿ĞµÑ€Ñ‚Ğ½Ñ‹Ğ¹ Ğ˜Ğ˜-Ğ°ÑÑĞ¸ÑÑ‚ĞµĞ½Ñ‚.",
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
        showNotification("Ğ§Ğ°Ñ‚ Ğ¾Ñ‡Ğ¸Ñ‰ĞµĞ½.", "info");
      }
    } catch (e) {
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ¾Ñ‡Ğ¸ÑÑ‚ĞºĞµ Ñ‡Ğ°Ñ‚Ğ°.", "error");
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
        throw new Error("ĞĞµÑ‚ Ğ´Ğ¾ÑÑ‚ÑƒĞ¿Ğ° Ğº Ğ˜Ğ˜ (ĞÑ„Ğ»Ğ°Ğ¹Ğ½ Ğ¸ Ollama Ğ½Ğµ Ğ°ĞºÑ‚Ğ¸Ğ²Ğ½Ğ°)");
      }

      if (!code) throw new Error("Empty AI response");
      code = code
        .replace(/```python\n?/g, "")
        .replace(/```\n?/g, "")
        .replace(/```csharp\n?/g, "")
        .replace(/```cs\n?/g, "");
      setManualResultCode(code.trim());
      setAttachedFiles([]);
      showNotification("ĞšĞ¾Ğ´ ÑĞ³ĞµĞ½ĞµÑ€Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½ (Multi-Modal)!", "success");
    } catch (err: any) {
      console.error(err);
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ğ¸: " + err.message, "error");
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
      showNotification("ĞĞµ ÑƒĞ´Ğ°Ğ»Ğ¾ÑÑŒ ÑĞ²ÑĞ·Ğ°Ñ‚ÑŒÑÑ Ñ ÑĞµÑ€Ğ²Ğ¸ÑĞ¾Ğ¼ Ollama.", "error");
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
          "ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸ ÑĞ¾Ñ…Ñ€Ğ°Ğ½ĞµĞ½Ñ‹. Ğ—Ğ°Ğ¿ÑƒÑĞºĞ°Ñ ÑĞºĞ°Ğ½Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ...",
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
        showNotification("Ğ¡Ñ‚Ğ°Ñ‚Ğ¸ÑÑ‚Ğ¸ĞºĞ° Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ° Ğ¾Ğ±Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ°!", "success");
      }
    } catch (e) {
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ ÑĞºĞ°Ğ½Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğ¸ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ°.", "error");
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
          "Master Blueprint (PROJECT_MASTER_BLUEPRINT.md) ÑƒÑĞ¿ĞµÑˆĞ½Ğ¾ Ğ¾Ğ±Ğ½Ğ¾Ğ²Ğ»ĞµĞ½!",
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
      text.toLowerCase() === "Ğ¿Ñ€Ğ¾Ğ´Ğ¾Ğ»Ğ¶Ğ¸Ñ‚ÑŒ" || text.toLowerCase() === "continue";

    if (isContinue) {
      promptText =
        "ĞŸĞĞ–ĞĞ›Ğ£Ğ™Ğ¡Ğ¢Ğ, ĞŸĞ ĞĞ”ĞĞ›Ğ–Ğ˜ Ğ¡Ğ’ĞĞ™ ĞŸĞ Ğ•Ğ”Ğ«Ğ”Ğ£Ğ©Ğ˜Ğ™ ĞĞ¢Ğ’Ğ•Ğ¢ Ğ¡ Ğ¢ĞĞ“Ğ ĞœĞ•Ğ¡Ğ¢Ğ, Ğ“Ğ”Ğ• ĞĞ ĞŸĞ Ğ•Ğ Ğ’ĞĞ›Ğ¡Ğ¯. ĞĞµ Ğ¿Ğ¾Ğ²Ñ‚Ğ¾Ñ€ÑĞ¹ ÑƒĞ¶Ğµ Ğ½Ğ°Ğ¿Ğ¸ÑĞ°Ğ½Ğ½Ğ¾Ğµ, Ğ½Ğ°Ñ‡Ğ½Ğ¸ Ğ¿Ñ€ÑĞ¼Ğ¾ Ñ Ğ¿Ñ€Ğ¾Ğ´Ğ¾Ğ»Ğ¶ĞµĞ½Ğ¸Ñ.";
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
      "Ğ˜Ğ½Ğ¸Ñ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ Ğ½ĞµĞ¹Ñ€Ğ¾Ğ½Ğ½Ñ‹Ñ… ĞºĞ¾Ğ½Ñ‚ÑƒÑ€Ğ¾Ğ² v18.8.0...",
      "ĞĞ½Ğ°Ğ»Ğ¸Ğ· ĞºĞ¾Ğ½Ñ‚ĞµĞºÑÑ‚Ğ° Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ° (Unity 6 & Blender 4.3)...",
      "ĞŸÑ€Ğ¾Ğ²ĞµÑ€ĞºĞ° ÑÑ‚Ğ°Ñ‚ÑƒÑĞ° Quantum Link Ğ¸ Ğ¾Ğ±Ğ»Ğ°Ñ‡Ğ½Ñ‹Ñ… ÑƒĞ·Ğ»Ğ¾Ğ²...",
      "Ğ”Ğ¾ÑÑ‚ÑƒĞ¿ Ğº Ğ±Ğ°Ğ·Ğµ 13,000+ Ğ²Ğ¸Ğ´ĞµĞ¾-ÑƒÑ€Ğ¾ĞºĞ¾Ğ²...",
      "Ğ¡Ğ¸Ğ½Ñ‚ĞµĞ· Ğ¾Ğ¿Ñ‚Ğ¸Ğ¼Ğ°Ğ»ÑŒĞ½Ğ¾Ğ³Ğ¾ Ñ€ĞµÑˆĞµĞ½Ğ¸Ñ Ğ´Ğ»Ñ Ğ²Ğ°ÑˆĞµĞ³Ğ¾ Ğ·Ğ°Ğ¿Ñ€Ğ¾ÑĞ°...",
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
        "Ğ¼ÑƒĞ·Ñ‹ĞºĞ°",
        "Ğ¿ĞµÑĞ½Ñ",
        "Ğ·Ğ²ÑƒĞº",
        "Ğ¼ĞµĞ»Ğ¾Ğ´Ğ¸Ñ",
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
        content: textResponse || "Ğ˜Ğ·Ğ²Ğ¸Ğ½Ğ¸Ñ‚Ğµ, Ñ Ğ½Ğµ ÑĞ¼Ğ¾Ğ³ ÑĞ³ĞµĞ½ĞµÑ€Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ğ¾Ñ‚Ğ²ĞµÑ‚.",
        timestamp: Date.now(),
        audioVariants: isAudioRequest
          ? [
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 1 (Quantum Sonic)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 2 (Neural Melodic)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 3 (Void Resonance)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 4 (Reality Warp)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 5 (Eternal Harmony)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 6 (Subatomic Beats)",
                url: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3",
              },
              {
                name: "Ğ­ĞºÑĞ¿ĞµÑ€Ğ¸Ğ¼ĞµĞ½Ñ‚Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚ 7 (Quantum Distortion)",
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
        errorText.includes("ĞºĞ»ÑÑ‡") ||
        errorText.includes("API_KEY_INVALID") ||
        errorText.includes("key") ||
        errorText.includes("401");

      if (isKeyError) {
        setMessages((prev) => [
          ...prev,
          {
            role: "assistant",
            content: `### âŒ ĞĞ¨Ğ˜Ğ‘ĞšĞ ĞšĞĞĞ¤Ğ˜Ğ“Ğ£Ğ ĞĞ¦Ğ˜Ğ˜ API
Ğ’Ğ°Ñˆ ĞºĞ»ÑÑ‡ Gemini Ğ¾Ğ¿Ñ€ĞµĞ´ĞµĞ»ĞµĞ½ ĞºĞ°Ğº Ğ½ĞµĞ´ĞµĞ¹ÑÑ‚Ğ²Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğ¹. 

**ĞšĞ°Ğº Ğ¸ÑĞ¿Ñ€Ğ°Ğ²Ğ¸Ñ‚ÑŒ:**
1. ĞĞ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ Ğ½Ğ° âš™ï¸ **ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸** ÑĞ»ĞµĞ²Ğ°.
2. ĞÑ‚ĞºÑ€Ğ¾Ğ¹Ñ‚Ğµ Ğ²ĞºĞ»Ğ°Ğ´ĞºÑƒ **Ğ¡ĞµĞºÑ€ĞµÑ‚Ñ‹**.
3. ĞĞ°Ğ¹Ğ´Ğ¸Ñ‚Ğµ \`GEMINI_API_KEY\`.
4. Ğ£Ğ±ĞµĞ´Ğ¸Ñ‚ĞµÑÑŒ, Ñ‡Ñ‚Ğ¾ Ñ‚Ğ°Ğ¼ Ğ²ÑÑ‚Ğ°Ğ²Ğ»ĞµĞ½ **ĞºĞ¾Ğ´** (Ğ½Ğ°Ğ¿Ñ€Ğ¸Ğ¼ĞµÑ€: \`AIzaSy... \`), Ğ° Ğ½Ğµ Ñ‚ĞµĞºÑÑ‚ "Ğ‘ĞµÑĞ¿Ğ»Ğ°Ñ‚Ğ½Ñ‹Ğ¹ ÑƒÑ€Ğ¾Ğ²ĞµĞ½ÑŒ".
5. ĞĞ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ **ĞŸÑ€Ğ¸Ğ¼ĞµĞ½Ğ¸Ñ‚ÑŒ Ğ¸Ğ·Ğ¼ĞµĞ½ĞµĞ½Ğ¸Ñ**.

*ĞŸĞ¾ĞºĞ° ĞºĞ»ÑÑ‡ Ğ½Ğµ Ğ¸ÑĞ¿Ñ€Ğ°Ğ²Ğ»ĞµĞ½, Ñ Ğ±ÑƒĞ´Ñƒ Ğ¾Ñ‚Ğ²ĞµÑ‡Ğ°Ñ‚ÑŒ Ğ¸Ğ· Ğ»Ğ¾ĞºĞ°Ğ»ÑŒĞ½Ğ¾Ğ³Ğ¾ Ğ°Ñ€Ñ…Ğ¸Ğ²Ğ° Ğ·Ğ½Ğ°Ğ½Ğ¸Ğ¹.*`,
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
                content: `[OLLAMA - ĞŸĞ ĞĞ’Ğ•Ğ ĞšĞ Ğ¡Ğ’Ğ¯Ğ—Ğ˜]\n\n${ollamaData.answer}`,
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
        if (isKeyError && !finalContent.includes("ĞšĞ›Ğ®Ğ§ API")) {
          finalContent = `### ğŸ“¡ Ğ¡Ğ¢ĞĞ¢Ğ£Ğ¡ Ğ¡Ğ˜ĞĞ“Ğ£Ğ›Ğ¯Ğ ĞĞĞ¡Ğ¢Ğ˜: ĞĞĞ›ĞĞ™Ğ (v18.5.6)\nĞ¡Ğ²ÑĞ·ÑŒ ÑƒÑÑ‚Ğ°Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ°, Ğ½Ğ¾ Ğ²Ğ¾Ğ·Ğ½Ğ¸ĞºĞ»Ğ° Ñ‚ĞµÑ…Ğ½Ğ¸Ñ‡ĞµÑĞºĞ°Ñ Ğ¿Ñ€Ğ¾Ğ±Ğ»ĞµĞ¼Ğ° Ñ API-ĞºĞ»ÑÑ‡Ğ¾Ğ¼ Gemini.\n\n${finalContent}`;
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
              "ĞÑˆĞ¸Ğ±ĞºĞ°: ĞĞµ ÑƒĞ´Ğ°Ğ»Ğ¾ÑÑŒ Ğ¿Ğ¾Ğ´ĞºĞ»ÑÑ‡Ğ¸Ñ‚ÑŒÑÑ Ğº Ğ˜Ğ˜ Ğ¸ Ğ»Ğ¾ĞºĞ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ¿Ğ¾Ğ¸ÑĞº Ğ½ĞµĞ´Ğ¾ÑÑ‚ÑƒĞ¿ĞµĞ½.",
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
    setVkStatus("Ğ˜Ğ½Ğ¸Ñ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ ĞºĞ²Ğ°Ğ½Ñ‚Ğ¾Ğ²Ğ¾Ğ³Ğ¾ ÑĞ´Ñ€Ğ°...");

    try {
      let finalPrompt = vkPrompt;

      // Phase 1: Gemini Image Analysis (Vision Synthesis)
      if (sourceImage && isOnline) {
        setVkStatus("ĞĞ½Ğ°Ğ»Ğ¸Ğ· ĞºĞ¾Ğ¼Ğ¿Ğ¾Ğ·Ğ¸Ñ†Ğ¸Ğ¸ Ğ¸ Ğ¾ÑĞ²ĞµÑ‰ĞµĞ½Ğ¸Ñ (Vision)...");
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
          setVkStatus("Ğ¡Ğ±Ğ¾Ğ¹ Vision, Ğ¸ÑĞ¿Ğ¾Ğ»ÑŒĞ·ÑƒÑ Ğ±Ğ°Ğ·Ğ¾Ğ²ÑƒÑ Ğ»Ğ¾Ğ³Ğ¸ĞºÑƒ...");
          setVkProgress(15);
        }
      } else if (sourceImage && !isOnline) {
        setVkStatus("ĞÑ„Ğ»Ğ°Ğ¹Ğ½ Ñ€ĞµĞ¶Ğ¸Ğ¼: ĞĞ½Ğ°Ğ»Ğ¸Ğ· Vision Ğ¿Ñ€Ğ¾Ğ¿ÑƒÑ‰ĞµĞ½...");
        setVkProgress(15);
      }

      setVkStatus("Ğ—Ğ°Ğ¿ÑƒÑĞº Ğ¿Ğ°Ñ€Ğ°Ğ»Ğ»ĞµĞ»ÑŒĞ½Ğ¾Ğ³Ğ¾ ÑĞ¸Ğ½Ñ‚ĞµĞ·Ğ° (Burst Mode)...");

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
          setVkStatus(`ĞĞ±Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°Ğ½Ğ¾ ${completedSteps} Ğ¸Ğ· 10 (Ğ¡Ğ¸Ğ½Ñ‚ĞµĞ·)...`);
        }
      };

      // Fire all requests in parallel
      await Promise.all(
        Array.from({ length: totalSteps }).map((_, i) => generateStep(i + 1)),
      );

      showNotification(
        "Ğ¡Ğ³ĞµĞ½ĞµÑ€Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¾ 10 ÑƒĞ½Ğ¸ĞºĞ°Ğ»ÑŒĞ½Ñ‹Ñ… Ğ¾Ğ±Ğ»Ğ¾Ğ¶ĞµĞº Ğ² Burst-Ñ€ĞµĞ¶Ğ¸Ğ¼Ğµ!",
        "success",
      );
      setVkStatus("Ğ¡Ğ¸Ğ½Ñ‚ĞµĞ· Ğ·Ğ°Ğ²ĞµÑ€ÑˆĞµĞ½!");
    } catch (e) {
      console.error("VK Gen Error:", e);
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ğ¾ÑĞ»ĞµĞ´Ğ¾Ğ²Ğ°Ñ‚ĞµĞ»ÑŒĞ½Ğ¾Ğ¹ Ğ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ğ¸.", "error");
      setVkStatus("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ³ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ğ¸");
    } finally {
      setIsGeneratingVK(false);
      // Keep progress at 100 for a moment then clear if needed
    }
  };

  const handleVKFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 50 * 1024 * 1024) {
      showNotification("Ğ¤Ğ°Ğ¹Ğ» ÑĞ»Ğ¸ÑˆĞºĞ¾Ğ¼ Ğ±Ğ¾Ğ»ÑŒÑˆĞ¾Ğ¹! ĞœĞ°ĞºÑĞ¸Ğ¼ÑƒĞ¼ 50ĞœĞ‘.", "error");
      return;
    }
    setIsUploading(true);
    const reader = new FileReader();
    reader.onloadend = () => {
      setSourceImage(reader.result as string);
      setIsUploading(false);
      showNotification("Ğ¤Ğ¾Ñ‚Ğ¾ ÑƒÑĞ¿ĞµÑˆĞ½Ğ¾ Ğ·Ğ°Ğ³Ñ€ÑƒĞ¶ĞµĞ½Ğ¾ Ğ´Ğ»Ñ ÑĞ¸Ğ½Ñ‚ĞµĞ·Ğ°!", "success");
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
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ¾Ğ±Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ¸Ğ¸ Ğ±Ğ°Ğ· Ğ·Ğ½Ğ°Ğ½Ğ¸Ğ¹.", "error");
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
          "ĞĞµ ÑƒĞ´Ğ°Ğ»Ğ¾ÑÑŒ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·Ğ¸Ñ‚ÑŒ Ğ¸Ğ½Ñ„Ğ¾Ñ€Ğ¼Ğ°Ñ†Ğ¸Ñ Ğ¾ Ğ²Ğ¾Ğ·Ğ¼Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚ÑÑ….",
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
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ ÑĞ¾Ñ…Ñ€Ğ°Ğ½ĞµĞ½Ğ¸Ğ¸ Ğ´Ğ¸Ğ·Ğ°Ğ¹Ğ½Ğ° Ğ¸Ğ³Ñ€Ñ‹.", "error");
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
        "C# ÑĞºÑ€Ğ¸Ğ¿Ñ‚ ĞºĞ¾Ğ½Ğ²ĞµÑ€Ñ‚ĞµÑ€Ğ° ÑĞºĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½ Ğ² Ğ±ÑƒÑ„ĞµÑ€ Ğ¾Ğ±Ğ¼ĞµĞ½Ğ°!",
        "success",
      );
    } catch (error) {
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ¿Ğ¾Ğ»ÑƒÑ‡ĞµĞ½Ğ¸Ğ¸ ÑĞºÑ€Ğ¸Ğ¿Ñ‚Ğ°.", "error");
    }
  };

  const handleGetGitLFS = async () => {
    try {
      const res = await fetch("/api/git/lfs-setup");
      const data = await res.json();
      await navigator.clipboard.writeText(data.content);
      showNotification(".gitattributes Ğ´Ğ»Ñ LFS ÑĞºĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½!", "success");
    } catch (error) {
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ¿Ğ¾Ğ»ÑƒÑ‡ĞµĞ½Ğ¸Ğ¸ ĞºĞ¾Ğ½Ñ„Ğ¸Ğ³ÑƒÑ€Ğ°Ñ†Ğ¸Ğ¸.", "error");
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
      showNotification("ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞµ Ğ´Ğ°Ğ½Ğ½Ñ‹Ñ… Ğ¼Ğ¸Ğ³Ñ€Ğ°Ñ†Ğ¸Ğ¸.", "error");
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
                `${minutes > 0 ? `${minutes} Ğ¼Ğ¸Ğ½ ` : ""}${seconds} ÑĞµĞº (${speedMB} MB/s)`,
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
              reject(new Error("Ğ¡ĞµÑ€Ğ²ĞµÑ€ Ğ²ĞµÑ€Ğ½ÑƒĞ» Ğ½ĞµĞºĞ¾Ñ€Ñ€ĞµĞºÑ‚Ğ½Ñ‹Ğ¹ Ğ¾Ñ‚Ğ²ĞµÑ‚ (Ğ½Ğµ JSON)"));
            }
          } else {
            reject(new Error(`ĞÑˆĞ¸Ğ±ĞºĞ° Ğ·Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞ¸: ÑÑ‚Ğ°Ñ‚ÑƒÑ ${xhr.status}`));
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
        showNotification("Ğ¤Ğ°Ğ¹Ğ»Ñ‹ Ğ¿Ñ€Ğ¸ĞºÑ€ĞµĞ¿Ğ»ĞµĞ½Ñ‹ Ğº ÑĞ¾Ğ¾Ğ±Ñ‰ĞµĞ½Ğ¸Ñ", "success");
      }
    } catch (error) {
      console.error("Upload error:", error);
      showNotification(
        "ĞÑˆĞ¸Ğ±ĞºĞ° Ğ¿Ñ€Ğ¸ Ğ·Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞµ. Ğ’Ğ¾Ğ·Ğ¼Ğ¾Ğ¶Ğ½Ğ¾, Ñ„Ğ°Ğ¹Ğ» ÑĞ»Ğ¸ÑˆĞºĞ¾Ğ¼ Ğ±Ğ¾Ğ»ÑŒÑˆĞ¾Ğ¹.",
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
                  Ğ¡ĞµÑ€Ğ²ĞµÑ€
                </span>
                <div className="flex items-center gap-1.5">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${serverHealth === "online" ? "bg-green-500" : "bg-red-500 animate-pulse"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase ${serverHealth === "online" ? "text-green-500" : "text-red-500"}`}
                  >
                    {serverHealth === "online" ? "Ğ¡Ğ²ÑĞ·ÑŒ OK" : "ĞÑˆĞ¸Ğ±ĞºĞ°"}
                  </span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[10px] font-bold text-slate-500 uppercase">
                  Ğ˜Ğ˜ Ğ˜Ğ½Ñ‚ĞµĞ»Ğ»ĞµĞºÑ‚
                </span>
                <div className="flex items-center gap-1.5">
                  <div
                    className={`w-1.5 h-1.5 rounded-full ${aiHealth === "online" ? "bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]" : "bg-yellow-500"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase ${aiHealth === "online" ? "text-green-500" : "text-yellow-500"}`}
                  >
                    {aiHealth === "online" ? "Ğ¡Ğ²ÑĞ·ÑŒ OK" : "ĞĞ³Ñ€Ğ°Ğ½Ğ¸Ñ‡ĞµĞ½"}
                  </span>
                </div>
              </div>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">
                Ğ—Ñ€ĞµĞ½Ğ¸Ğµ (Vision)
              </span>
              <div className="flex items-center gap-1.5">
                <ImageIcon className="w-3 h-3 text-blue-400" />
                <span className="text-[9px] font-bold uppercase text-blue-400">
                  ĞĞºÑ‚Ğ¸Ğ²Ğ½Ğ¾
                </span>
              </div>
            </div>
            <div className="px-2 py-1 bg-white/5 rounded-lg border border-white/5 flex items-center gap-2">
              <Info className="w-3 h-3 text-slate-600" />
              <span className="text-[8px] text-slate-600 uppercase leading-tight">
                HMR WebSocket Ğ¼Ğ¾Ğ¶ĞµÑ‚ Ğ±Ñ‹Ñ‚ÑŒ Ğ¾Ñ‚ĞºĞ»ÑÑ‡ĞµĞ½ (ÑÑ‚Ğ¾ Ğ½Ğ¾Ñ€Ğ¼Ğ°Ğ»ÑŒĞ½Ğ¾)
              </span>
            </div>
            <div className="flex items-center justify-between p-2 rounded-lg bg-white/5 border border-white/5">
              <span className="text-[10px] font-bold text-slate-500 uppercase">
                AI ĞĞ³ĞµĞ½Ñ‚
              </span>
              <div className="flex items-center gap-1.5">
                <div
                  className={`w-1.5 h-1.5 rounded-full ${isTyping ? "bg-yellow-500 animate-pulse" : "bg-green-500"}`}
                />
                <span
                  className={`text-[9px] font-bold uppercase ${isTyping ? "text-yellow-400" : "text-green-500"}`}
                >
                  {isTyping ? "Ğ”ÑƒĞ¼Ğ°ĞµÑ‚..." : "Ğ“Ğ¾Ñ‚Ğ¾Ğ²"}
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
                <Layers className="w-3 h-3" /> Ğ¡Ñ‚Ğ°Ñ‚Ğ¸ÑÑ‚Ğ¸ĞºĞ° Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ°
                <span className="flex h-1.5 w-1.5 relative">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-green-500"></span>
                </span>
              </h3>
              <button
                onClick={handleRefreshScan}
                className="p-1 hover:bg-white/5 rounded-md transition-colors text-slate-500 hover:text-white"
                title="ĞĞ±Ğ½Ğ¾Ğ²Ğ¸Ñ‚ÑŒ ÑÑ‚Ğ°Ñ‚Ğ¸ÑÑ‚Ğ¸ĞºÑƒ"
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
                      Ğ¡ĞºÑ€Ğ¸Ğ¿Ñ‚Ñ‹ (C#)
                    </span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.scripts.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Box className="w-3 h-3 text-purple-400" />
                    <span className="text-[10px] text-slate-400">ĞŸÑ€ĞµÑ„Ğ°Ğ±Ñ‹</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.prefabs.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Gamepad2 className="w-3 h-3 text-green-400" />
                    <span className="text-[10px] text-slate-400">Ğ¡Ñ†ĞµĞ½Ñ‹</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.scenes.length}
                  </span>
                </div>
                <div className="flex items-center justify-between group">
                  <div className="flex items-center gap-2">
                    <Zap className="w-3 h-3 text-yellow-400" />
                    <span className="text-[10px] text-slate-400">ĞĞ½Ğ¸Ğ¼Ğ°Ñ†Ğ¸Ğ¸</span>
                  </div>
                  <span className="text-[10px] font-mono text-white">
                    {projectScan.animations.length}
                  </span>
                </div>
                <div className="pt-2 border-t border-white/5 flex items-center justify-between">
                  <span className="text-[10px] font-bold text-white uppercase">
                    Ğ’ÑĞµĞ³Ğ¾ Ñ„Ğ°Ğ¹Ğ»Ğ¾Ğ²
                  </span>
                  <span className="text-[10px] font-mono text-blue-400">
                    {projectScan.total_files}
                  </span>
                </div>
                <div className="mt-2 text-[8px] text-slate-600 uppercase tracking-tighter text-right italic">
                  ĞĞ±Ğ½Ğ¾Ğ²Ğ»ĞµĞ½Ğ¾:{" "}
                  {projectScan.last_updated
                    ? new Date(projectScan.last_updated).toLocaleTimeString()
                    : "---"}
                </div>
              </div>
            ) : (
              <div className="flex items-center gap-2 text-[10px] text-slate-600 italic">
                <RefreshCw className="w-3 h-3 animate-spin" /> Ğ¡ĞºĞ°Ğ½Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ...
              </div>
            )}
          </div>

          {/* Software Status */}
          <div className="space-y-4">
            <h3 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest flex items-center gap-2">
              <Settings className="w-3 h-3" /> Ğ¡Ñ‚Ğ°Ñ‚ÑƒÑ ĞŸĞ
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
                Ğ Ğ²Ğ¾Ğ·Ğ¼Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚ÑÑ… Ğ˜Ğ˜
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Ğ’ÑÑ‘ Ğ¾ Ñ‚Ğ¾Ğ¼, Ñ‡Ñ‚Ğ¾ ÑƒĞ¼ĞµĞµÑ‚ Ğ½Ğ°Ñˆ Ğ˜Ğ˜ Ğ¸ ĞºĞ°Ğº Ğ¾Ğ½ Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°ĞµÑ‚ Ñ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ¾Ğ¼.
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
              Ğ˜Ğ½ÑÑ‚Ñ€ÑƒĞºÑ†Ğ¸Ñ Ğ¿Ğ¾ Ğ¿ĞµÑ€ĞµĞ½Ğ¾ÑÑƒ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ° Ñ‡ĞµÑ€ĞµĞ· ĞºĞ¾Ğ½ÑĞ¾Ğ»ÑŒ.
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
                Ğ”Ğ°ÑˆĞ±Ğ¾Ñ€Ğ´
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              ĞŸĞ°Ğ½ĞµĞ»ÑŒ Ğ¼Ğ¾Ğ½Ğ¸Ñ‚Ğ¾Ñ€Ğ¸Ğ½Ğ³Ğ° Ğ¸ ÑÑ‚Ğ°Ñ‚Ğ¸ÑÑ‚Ğ¸ĞºĞ¸ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ°.
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
                Ğ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğµ
              </span>
            </div>
            <p className="text-[9px] text-slate-500 leading-relaxed">
              Ğ˜Ğ½Ñ„Ğ¾Ñ€Ğ¼Ğ°Ñ†Ğ¸Ñ Ğ¾ Ñ‚ĞµĞºÑƒÑ‰ĞµĞ¼ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğµ Ğ¸ ĞµĞ³Ğ¾ Ğ¸ÑÑ‚Ğ¾Ñ€Ğ¸Ğ¸.
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
                Ğ˜Ğ½Ñ‚ĞµĞ»Ğ»ĞµĞºÑ‚ÑƒĞ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ¿Ğ¾Ğ¼Ğ¾Ñ‰Ğ½Ğ¸Ğº
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
                <Send className="w-3.5 h-3.5" /> Ğ§Ğ°Ñ‚
              </button>
              <button
                onClick={() => setActiveTab("project_scripts")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "project_scripts"
                    ? "bg-amber-600 text-white shadow-lg shadow-amber-600/20"
                    : "text-slate-500 hover:text-slate-300"
                }`}
              >
                <Code className="w-3.5 h-3.5" /> Ğ¡ĞºÑ€Ğ¸Ğ¿Ñ‚Ñ‹
              </button>
              <button
                onClick={() => setActiveTab("dashboard")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "dashboard"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/20"
                    : "text-slate-500 hover:text-slate-300"
                }`}
              >
                <Layers className="w-3.5 h-3.5" /> Ğ¥Ñ€Ğ°Ğ½Ğ¸Ğ»Ğ¸Ñ‰Ğµ
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
                <GitBranch className="w-3.5 h-3.5" /> ĞœĞ¸Ğ³Ñ€Ğ°Ñ†Ğ¸Ñ
              </button>
              <button
                onClick={() => setShowVKGenerator(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-blue-400 hover:bg-blue-600/10 border border-blue-500/20`}
              >
                <ImageIcon className="w-3.5 h-3.5" /> ĞĞ±Ğ»Ğ¾Ğ¶ĞºĞ¸ Ğ’Ğš
              </button>
              <button
                onClick={() => setShowMarkerSplitter(true)}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 text-amber-400 hover:bg-amber-600/10 border border-amber-500/20 hover:scale-105 active:scale-95 duration-300`}
              >
                <Compass className="w-3.5 h-3.5 text-amber-400" /> Ğ¡Ğ¿Ğ»Ğ¸Ñ‚Ñ‚ĞµÑ€
                ĞœĞ°Ñ€ĞºĞµÑ€Ğ¾Ğ²
              </button>
              <button
                onClick={() => setActiveTab("external_skills_db")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "external_skills_db"
                    ? "bg-blue-600 text-white shadow-lg shadow-blue-600/40"
                    : "text-blue-400 hover:bg-blue-600/10 border border-blue-500/20"
                } hover:scale-105 active:scale-100 duration-300`}
              >
                <BrainCircuit className="w-3.5 h-3.5" /> Ğ‘Ğ°Ğ·Ğ° Ğ—Ğ½Ğ°Ğ½Ğ¸Ğ¹ Ğ˜Ğ˜
              </button>
              <button
                onClick={() => setActiveTab("game_design")}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-bold uppercase transition-all flex items-center gap-2 ${
                  activeTab === "game_design"
                    ? "bg-purple-600 text-white shadow-lg shadow-purple-600/20"
                    : "text-purple-400 hover:bg-purple-600/10 border border-purple-500/20"
                }`}
              >
                <Gamepad2 className="w-3.5 h-3.5" /> Ğ¡Ñ‚ÑƒĞ´Ğ¸Ñ Ğ˜Ğ³Ñ€Ñ‹
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
                          {language === "Ğ ÑƒÑÑĞºĞ¸Ğ¹" ? (
                            <>
                              Ğ¯ Ğ¿Ğ¾Ğ»Ğ½Ğ¾ÑÑ‚ÑŒÑ Ğ¾ÑĞ²ĞµĞ´Ğ¾Ğ¼Ğ»ĞµĞ½ Ğ¾ Ğ²Ğ°ÑˆĞµĞ¼ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğµ Ğ¿Ğ¾ Ğ¿ÑƒÑ‚Ğ¸{" "}
                              <br />
                              <code className="text-blue-400 break-all bg-white/5 px-2 py-1 rounded mt-2 inline-block shadow-inner ring-1 ring-white/5 font-mono">
                                {kb?.project_path || "Ğ—Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞ°..."}
                              </code>
                              .
                              <br />
                              <br />
                              Ğ—Ğ°Ğ´Ğ°Ğ²Ğ°Ğ¹Ñ‚Ğµ Ğ»ÑĞ±Ñ‹Ğµ Ğ²Ğ¾Ğ¿Ñ€Ğ¾ÑÑ‹ Ğ¿Ğ¾ Unity, Blender,
                              Photoshop Ğ¸Ğ»Ğ¸ GIMP. ĞœĞ¾Ğ´ÑƒĞ»Ğ¸ Menu Studio Visuals
                              Mastery, Omni-Answer Engine Ğ¸ Ğ¿Ñ€Ğ¾ĞµĞºÑ‚ 'ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚
                              ÑÑƒĞ´ÑŒĞ±Ñ‹' (v18.8.0) Ğ°ĞºÑ‚Ğ¸Ğ²Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ñ‹.
                              <br />
                              <br />
                              <span className="text-xs text-orange-400 font-black uppercase ring-1 ring-orange-400/30 px-3 py-1.5 rounded-full bg-orange-400/5 shadow-lg shadow-orange-500/5 inline-block animate-pulse">
                                Ğ’Ğ½Ğ¸Ğ¼Ğ°Ğ½Ğ¸Ğµ: {t.proMastery}
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
                            <Shield className="w-4 h-4 text-blue-500" /> Ğ¡Ñ‚Ğ°Ñ‚ÑƒÑ
                            API Ğ¸ ĞĞ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ° ĞºĞ»ÑÑ‡ĞµĞ¹
                          </h3>
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-[11px] leading-relaxed text-slate-400">
                            <div className="space-y-3">
                              <p className="font-bold text-blue-400 uppercase">
                                Ğ•ÑĞ»Ğ¸ Ğ² Ğ½Ğ°ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ°Ñ… Secrets Ğ¿ÑƒÑÑ‚Ğ¾ Ğ¸Ğ»Ğ¸ ÑÑ‚Ğ¾Ğ¸Ñ‚ "AI
                                Studio Free Tier":
                              </p>
                              <ul className="space-y-1 opacity-80">
                                <li>
                                  â€¢ Ğ­Ñ‚Ğ¾ Ğ²ÑÑ‚Ñ€Ğ¾ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ±ĞµÑĞ¿Ğ»Ğ°Ñ‚Ğ½Ñ‹Ğ¹ Ğ´Ğ¾ÑÑ‚ÑƒĞ¿ Ğ¿Ğ»Ğ°Ñ‚Ñ„Ğ¾Ñ€Ğ¼Ñ‹.
                                </li>
                                <li>
                                  â€¢ ĞĞ½ Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°ĞµÑ‚ Ğ°Ğ²Ñ‚Ğ¾Ğ¼Ğ°Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¸ â€” **Ğ½Ğ°Ğ¶Ğ¸Ğ¼Ğ°Ñ‚ÑŒ
                                  ĞºĞ°Ñ€Ğ°Ğ½Ğ´Ğ°Ñˆ Ğ½Ğµ Ğ½ÑƒĞ¶Ğ½Ğ¾**.
                                </li>
                                <li>
                                  â€¢ Ğ•ÑĞ»Ğ¸ Ğ²Ğ¸Ğ´Ğ¸Ñ‚Ğµ ÑÑ‚Ğ°Ñ‚ÑƒÑ "ĞÑ„Ğ»Ğ°Ğ¹Ğ½", Ğ·Ğ½Ğ°Ñ‡Ğ¸Ñ‚ ÑĞµÑ€Ğ²Ğ¸Ñ
                                  API Ğ²Ñ€ĞµĞ¼ĞµĞ½Ğ½Ğ¾ Ğ¿ĞµÑ€ĞµĞ³Ñ€ÑƒĞ¶ĞµĞ½.
                                </li>
                              </ul>
                            </div>
                            <div className="space-y-3">
                              <p className="font-bold text-orange-400 uppercase">
                                Ğ•ÑĞ»Ğ¸ Ñƒ Ğ²Ğ°Ñ ĞµÑÑ‚ÑŒ Ğ»Ğ¸Ñ‡Ğ½Ñ‹Ğ¹ ĞºĞ»ÑÑ‡ (AIza...):
                              </p>
                              <ul className="space-y-1 opacity-80">
                                <li>
                                  â€¢ Ğ’ Ğ²ĞµÑ€Ñ…Ğ½ĞµĞ¼ Ğ¼ĞµĞ½Ñ Secrets Ğ²Ñ‹Ğ±ĞµÑ€Ğ¸Ñ‚Ğµ **"Select
                                  key"**.
                                </li>
                                <li>
                                  â€¢ Ğ’ÑÑ‚Ğ°Ğ²ÑŒÑ‚Ğµ Ğ²Ğ°Ñˆ ĞºĞ¾Ğ´ Ğ¸ Ğ½Ğ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ **"Apply
                                  changes"**.
                                </li>
                                <li>â€¢ Ğ­Ñ‚Ğ¾ ÑĞ½Ğ¸Ğ¼ĞµÑ‚ Ğ»Ğ¸Ğ¼Ğ¸Ñ‚Ñ‹ Ğ±ĞµÑĞ¿Ğ»Ğ°Ñ‚Ğ½Ğ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ²Ğ½Ñ.</li>
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
                                <span className="text-blue-500/50">â€º</span>{" "}
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
                                      Ğ¡Ğ³ĞµĞ½ĞµÑ€Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ½Ñ‹Ğµ Ğ°ÑƒĞ´Ğ¸Ğ¾-Ğ²Ğ°Ñ€Ğ¸Ğ°Ğ½Ñ‚Ñ‹ (v18.8.0):
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
                                              title="Ğ¡ĞºĞ°Ñ‡Ğ°Ñ‚ÑŒ MP3"
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
                                            Ğ’Ğ°Ñˆ Ğ±Ñ€Ğ°ÑƒĞ·ĞµÑ€ Ğ½Ğµ Ğ¿Ğ¾Ğ´Ğ´ĞµÑ€Ğ¶Ğ¸Ğ²Ğ°ĞµÑ‚ Ğ°ÑƒĞ´Ğ¸Ğ¾.
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
                                Ğ—Ğ°Ğ³Ñ€ÑƒĞ·ĞºĞ° Ñ„Ğ°Ğ¹Ğ»Ğ¾Ğ²...
                              </span>
                              {uploadTimeRemaining && (
                                <span className="text-[8px] text-slate-500 uppercase">
                                  ĞÑÑ‚Ğ°Ğ»Ğ¾ÑÑŒ: {uploadTimeRemaining}
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
                          <Terminal className="w-3 h-3" /> ĞĞ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ Enter, Ñ‡Ñ‚Ğ¾Ğ±Ñ‹
                          Ğ¾Ñ‚Ğ¿Ñ€Ğ°Ğ²Ğ¸Ñ‚ÑŒ ÑĞ¾Ğ¾Ğ±Ñ‰ĞµĞ½Ğ¸Ğµ
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
                            placeholder="Ğ—Ğ°Ğ´Ğ°Ğ¹Ñ‚Ğµ Ğ²Ğ¾Ğ¿Ñ€Ğ¾Ñ Ğ¿Ğ¾ Unity Ğ¸Ğ»Ğ¸ Blender..."
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
                      AI Ğ¼Ğ¾Ğ¶ĞµÑ‚ Ğ¾ÑˆĞ¸Ğ±Ğ°Ñ‚ÑŒÑÑ. ĞŸÑ€Ğ¾Ğ²ĞµÑ€ÑĞ¹Ñ‚Ğµ ĞºĞ¾Ğ´ Ğ¿ĞµÑ€ĞµĞ´ Ğ¸ÑĞ¿Ğ¾Ğ»ÑŒĞ·Ğ¾Ğ²Ğ°Ğ½Ğ¸ĞµĞ¼ Ğ²
                      Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğµ.
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
                            ĞŸĞ¾Ğ¼Ğ¾Ñ‰Ğ½Ğ¸Ğº Ğ¼Ğ¸Ğ³Ñ€Ğ°Ñ†Ğ¸Ğ¸ (Unity â†’ Godot/Redot)
                          </h2>
                          <p className="text-sm text-slate-400">
                            Ğ˜Ğ½ÑÑ‚Ñ€ÑƒĞ¼ĞµĞ½Ñ‚Ñ‹ Ğ¸ ÑĞ¿Ñ€Ğ°Ğ²Ğ¾Ñ‡Ğ½Ğ¸ĞºĞ¸ Ğ´Ğ»Ñ Ğ¿ĞµÑ€ĞµĞ½Ğ¾ÑĞ° Ğ²Ğ°ÑˆĞ¸Ñ…
                            Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ¾Ğ² Ğ½Ğ° Ğ¾Ñ‚ĞºÑ€Ñ‹Ñ‚Ñ‹Ğµ Ğ´Ğ²Ğ¸Ğ¶ĞºĞ¸.
                          </p>
                        </div>
                      </div>
                      <div className="p-4 bg-black/40 rounded-2xl border border-white/5 text-xs text-slate-300 leading-relaxed">
                        {migrationData?.message ||
                          "ĞĞµÑ‚ Ğ´Ğ°Ğ½Ğ½Ñ‹Ñ… Ğ´Ğ»Ñ Ğ¾Ñ‚Ğ¾Ğ±Ñ€Ğ°Ğ¶ĞµĞ½Ğ¸Ñ. Ğ’Ğ¾ÑĞ¿Ğ¾Ğ»ÑŒĞ·ÑƒĞ¹Ñ‚ĞµÑÑŒ Ğ¸Ğ½ÑÑ‚Ñ€ÑƒĞ¼ĞµĞ½Ñ‚Ğ°Ğ¼Ğ¸ Ğ¼Ğ¸Ğ³Ñ€Ğ°Ñ†Ğ¸Ğ¸ Ğ² Ñ‡Ğ°Ñ‚Ğµ."}
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                      <section className="p-8 rounded-[2.5rem] bg-black/40 border border-white/5">
                        <h3 className="text-sm font-bold text-white uppercase tracking-widest mb-6 flex items-center gap-3">
                          <Code className="w-4 h-4 text-blue-400" /> ĞšĞ°Ñ€Ñ‚Ğ°
                          ÑĞ¾Ğ¾Ñ‚Ğ²ĞµÑ‚ÑÑ‚Ğ²Ğ¸Ğ¹ API
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
                          <Zap className="w-4 h-4 text-yellow-400" /> Ğ¡Ğ¾Ğ²ĞµÑ‚Ñ‹ Ğ¿Ğ¾
                          ĞºĞ¾Ğ½Ğ²ĞµÑ€Ñ‚Ğ°Ñ†Ğ¸Ğ¸
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
                        ĞĞ²Ñ‚Ğ¾Ğ¼Ğ°Ñ‚Ğ¸Ğ·Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ½Ñ‹Ğ¹ Ğ¿ĞµÑ€ĞµĞ½Ğ¾Ñ (Experimental)
                      </h3>
                      <p className="text-xs text-slate-400 mb-6">
                        ĞœÑ‹ Ñ€Ğ°Ğ±Ğ¾Ñ‚Ğ°ĞµĞ¼ Ğ½Ğ°Ğ´ ÑĞºÑ€Ğ¸Ğ¿Ñ‚Ğ¾Ğ¼, ĞºĞ¾Ñ‚Ğ¾Ñ€Ñ‹Ğ¹ ÑĞ¼Ğ¾Ğ¶ĞµÑ‚ Ğ°Ğ²Ñ‚Ğ¾Ğ¼Ğ°Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¸
                        ĞºĞ¾Ğ½Ğ²ĞµÑ€Ñ‚Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ ÑÑ‚Ñ€ÑƒĞºÑ‚ÑƒÑ€Ñƒ ÑÑ†ĞµĞ½Ñ‹ (.unity â†’ .tscn) Ğ¸
                        Ğ±Ğ°Ğ·Ğ¾Ğ²Ñ‹Ğµ C# ÑĞºÑ€Ğ¸Ğ¿Ñ‚Ñ‹. ĞĞ° Ğ´Ğ°Ğ½Ğ½Ñ‹Ğ¹ Ğ¼Ğ¾Ğ¼ĞµĞ½Ñ‚ Ñ€ĞµĞºĞ¾Ğ¼ĞµĞ½Ğ´ÑƒĞµÑ‚ÑÑ
                        Ğ¸ÑĞ¿Ğ¾Ğ»ÑŒĞ·Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ñ€ÑƒÑ‡Ğ½Ğ¾Ğ¹ Ğ¿ĞµÑ€ĞµĞ½Ğ¾Ñ Ñ Ğ¿Ğ¾Ğ¼Ğ¾Ñ‰ÑŒÑ ĞºĞ°Ñ€Ñ‚Ñ‹ ÑĞ¾Ğ¾Ñ‚Ğ²ĞµÑ‚ÑÑ‚Ğ²Ğ¸Ğ¹
                        Ğ²Ñ‹ÑˆĞµ.
                      </p>
                      <button
                        onClick={() =>
                          showNotification(
                            "Ğ¤ÑƒĞ½ĞºÑ†Ğ¸Ñ Ğ°Ğ²Ñ‚Ğ¾Ğ¼Ğ°Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ Ğ¿ĞµÑ€ĞµĞ½Ğ¾ÑĞ° Ğ½Ğ°Ñ…Ğ¾Ğ´Ğ¸Ñ‚ÑÑ Ğ² Ñ€Ğ°Ğ·Ñ€Ğ°Ğ±Ğ¾Ñ‚ĞºĞµ.",
                            "info",
                          )
                        }
                        className="px-6 py-3 bg-blue-600/20 border border-blue-500/30 rounded-xl text-[10px] font-bold text-blue-400 uppercase tracking-widest hover:bg-blue-600 hover:text-white transition-all"
                      >
                        Ğ—Ğ°Ğ¿ÑƒÑÑ‚Ğ¸Ñ‚ÑŒ Ğ°Ğ½Ğ°Ğ»Ğ¸Ğ· Ğ¿Ñ€Ğ¾ĞµĞºÑ‚Ğ°
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
                              {gameDesign?.game_title || "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ ÑÑƒĞ´ÑŒĞ±Ñ‹"}
                            </h2>
                            <span className="px-3 py-1 rounded-full bg-purple-600/20 border border-purple-500/30 text-[10px] font-bold text-purple-400 uppercase tracking-widest">
                              v{gameDesign?.version || "1.2.0"}
                            </span>
                          </div>
                          <div className="text-xs text-slate-500 uppercase tracking-[0.3em] font-black mt-2 flex items-center gap-2">
                            <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                            Ğ¦ĞµĞ½Ñ‚Ñ€Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ ÑˆÑ‚Ğ°Ğ± Ñ€Ğ°Ğ·Ñ€Ğ°Ğ±Ğ¾Ñ‚ĞºĞ¸ â€¢{" "}
                            {gameDesign?.style || "High Fantasy"}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <button
                          onClick={() =>
                            showNotification(
                              "Ğ“ĞµĞ½ĞµÑ€Ğ°Ñ†Ğ¸Ñ GDD Ğ´Ğ¾ĞºÑƒĞ¼ĞµĞ½Ñ‚Ğ°...",
                              "info",
                            )
                          }
                          className="px-6 py-4 rounded-2xl bg-white/5 border border-white/10 text-white hover:bg-white/10 transition-all flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest"
                        >
                          <Download className="w-4 h-4" /> Ğ­ĞºÑĞ¿Ğ¾Ñ€Ñ‚ GDD
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
                            ? "Ğ¡Ğ¸Ğ½Ñ…Ñ€Ğ¾Ğ½Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ..."
                            : "Ğ¡Ğ¾Ñ…Ñ€Ğ°Ğ½Ğ¸Ñ‚ÑŒ Ğ˜Ğ·Ğ¼ĞµĞ½ĞµĞ½Ğ¸Ñ"}
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
                            1. Ğ“ĞµĞ¾Ğ³Ñ€Ğ°Ñ„Ğ¸Ñ ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ¾Ğ²
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
                                            ĞĞºÑ€ÑƒĞ¶ĞµĞ½Ğ¸Ğµ
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
                                          Ğ¤Ñ€Ğ°ĞºÑ†Ğ¸Ğ¸
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
                                            Ğ’Ñ‹Ğ´ĞµĞ»ĞµĞ½Ğ¸Ğµ ĞºĞ»ĞµÑ‚Ğ¾Ğº
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
                                              Ğ¡Ğ¸ÑÑ‚ĞµĞ¼Ğ° Ğ¿Ğ¾Ğ´ÑĞºĞ°Ğ·Ğ¾Ğº (Ğ—ÑƒĞ¼)
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
                                            ĞœĞµÑ…Ğ°Ğ½Ğ¸ĞºĞ° ĞšĞ°Ğ¼ĞµÑ€Ñ‹
                                          </h4>
                                          <div className="space-y-6">
                                            <div className="grid grid-cols-1 gap-3">
                                              <div className="text-[10px] text-slate-500 uppercase font-black">
                                                Ğ£Ñ€Ğ¾Ğ²Ğ½Ğ¸ Ğ¼Ğ°ÑÑˆÑ‚Ğ°Ğ±Ğ°
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
                                                Ğ’Ñ€Ğ°Ñ‰ĞµĞ½Ğ¸Ğµ
                                              </div>
                                              <div className="text-[11px] text-slate-300">
                                                {
                                                  gameDesign?.camera_mechanics
                                                    ?.rotation?.free
                                                }
                                                . Ğ¤Ğ¸ĞºÑĞ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ½Ñ‹Ğµ ÑƒĞ³Ğ»Ñ‹:{" "}
                                                {gameDesign?.camera_mechanics?.rotation?.fixed?.join(
                                                  "Â°, ",
                                                )}
                                                Â°.{" "}
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
                                                  Ğ˜Ğ½Ñ‚ĞµÑ€Ñ„ĞµĞ¹Ñ
                                                </div>
                                                <div className="text-xs text-white">
                                                  {gameDesign?.camera_mechanics?.ui?.join(
                                                    ", ",
                                                  )}
                                                </div>
                                              </div>
                                              <div className="p-6 bg-white/5 rounded-3xl border border-white/5 space-y-2">
                                                <div className="text-[9px] text-slate-500 uppercase font-black">
                                                  ĞĞ½Ğ¸Ğ¼Ğ°Ñ†Ğ¸Ñ
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
                            2. Ğ“Ğ»Ğ¾Ğ±Ğ°Ğ»ÑŒĞ½Ñ‹Ğ¹ Ğ›Ğ¾Ñ€
                          </h3>
                          <div className="p-8 rounded-[2.5rem] bg-white/5 border border-white/10 space-y-6">
                            <div className="flex items-center gap-4">
                              <div className="p-3 bg-blue-600/20 rounded-2xl text-blue-400">
                                <Sparkles className="w-5 h-5" />
                              </div>
                              <h4 className="text-sm font-black text-white uppercase tracking-widest">
                                ĞœĞ°Ğ½Ğ¸Ñ„ĞµÑÑ‚ ĞšĞ¾Ğ½Ñ†ĞµĞ¿Ñ†Ğ¸Ğ¸
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
                              placeholder="ĞĞ°Ğ¿Ğ¸ÑˆĞ¸Ñ‚Ğµ Ğ¸ÑÑ‚Ğ¾Ñ€Ğ¸Ñ Ğ¼Ğ¸Ñ€Ğ° ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹..."
                            />
                            <div className="p-6 rounded-2xl bg-purple-600/5 border border-purple-500/20">
                              <p className="text-[10px] text-purple-300/60 leading-relaxed italic">
                                "ĞœĞ¸Ñ€, Ğ³Ğ´Ğµ ĞºÑƒĞ»ÑŒÑ‚Ğ¸Ğ²Ğ°Ñ†Ğ¸Ñ ÑĞ¸Ğ»Ñ‹ â€” ĞµĞ´Ğ¸Ğ½ÑÑ‚Ğ²ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ¿ÑƒÑ‚ÑŒ Ğº
                                Ğ²ĞµÑ€ÑˆĞ¸Ğ½Ğµ. Ğ§ĞµÑ‚Ñ‹Ñ€Ğµ ĞºĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°, Ğ´ĞµÑÑÑ‚ĞºĞ¸ Ñ€Ğ°Ñ Ğ¸ Ñ‚Ñ‹ÑÑÑ‡Ğ¸
                                Ğ»ĞµÑ‚ Ğ²Ğ¾Ğ¹Ğ½Ñ‹ Ğ·Ğ° Ğ­Ñ„Ğ¸Ñ€Ğ½Ñ‹Ğµ Ğ˜ÑÑ‚Ğ¾Ñ‡Ğ½Ğ¸ĞºĞ¸."
                              </p>
                            </div>
                          </div>

                          <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6">
                            <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                              ĞÑ‚Ñ€Ğ¸Ğ±ÑƒÑ‚Ñ‹ Ğ’Ğ¸Ğ·ÑƒĞ°Ğ»ÑŒĞ½Ğ¾Ğ³Ğ¾ Ğ¡Ñ‚Ğ¸Ğ»Ñ
                            </h4>
                            <div className="flex flex-wrap gap-2">
                              {[
                                "ĞšĞ¸Ñ‚Ğ°Ğ¹ÑĞºĞ¾Ğµ Ñ„ÑĞ½Ñ‚ĞµĞ·Ğ¸",
                                "Xianxia",
                                "Ğ ÑƒĞ½Ğ¸Ñ‡ĞµÑĞºĞ°Ñ Ğ¼Ğ°Ğ³Ğ¸Ñ",
                                "ĞŸĞ°Ñ€ÑÑ‰Ğ¸Ğµ Ğ³Ğ¾Ñ€Ñ‹",
                                "Ğ­Ñ„Ğ¸Ñ€Ğ½Ñ‹Ğ¹ ÑĞ²ĞµÑ‚",
                                "Ğ”Ñ€ĞµĞ²Ğ½Ğ¸Ğµ ÑĞµĞºÑ‚Ñ‹",
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
                              Ğ”Ğ°Ğ½Ğ½Ñ‹Ğµ Castle Mechanics Ğ½Ğµ Ğ½Ğ°Ğ¹Ğ´ĞµĞ½Ñ‹
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
                                Ğ’Ğ¸Ğ·ÑƒĞ°Ğ»ÑŒĞ½Ñ‹Ğµ ÑĞ¾ÑÑ‚Ğ¾ÑĞ½Ğ¸Ñ
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
                                  Ğ¡Ñ‚Ğ¸Ğ»Ğ¸ ĞºĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ¾Ğ²
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
                                Ğ“Ğ»Ğ¾Ğ±Ğ°Ğ»ÑŒĞ½Ñ‹Ğµ ÑƒÑ€Ğ¾Ğ²Ğ½Ğ¸ Ñ€Ğ°Ğ·Ğ²Ğ¸Ñ‚Ğ¸Ñ (1â€”5)
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
                                            Ğ“Ğ°Ñ€Ğ½Ğ¸Ğ·Ğ¾Ğ½
                                          </div>
                                          <div className="text-[10px] text-slate-400 italic leading-tight">
                                            {dl.garrison}
                                          </div>
                                        </div>
                                        <div className="space-y-1">
                                          <div className="text-[8px] text-slate-600 uppercase font-black">
                                            ĞĞ±Ğ¾Ñ€Ğ¾Ğ½Ğ°
                                          </div>
                                          <div className="text-[10px] text-slate-400 italic leading-tight">
                                            {dl.defense}
                                          </div>
                                        </div>
                                        <div className="space-y-1">
                                          <div className="text-[8px] text-slate-600 uppercase font-black">
                                            Ğ­ĞºĞ¾Ğ½Ğ¾Ğ¼Ğ¸ĞºĞ°
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
                                      {cont.name}: ĞŸÑƒÑ‚ÑŒ Ğ Ğ°Ğ·Ğ²Ğ¸Ñ‚Ğ¸Ñ Ğ—Ğ°Ğ¼ĞºĞ°
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
                                              Ğ’Ğ½ĞµÑˆĞ½Ğ¸Ğ¹ Ğ²Ğ¸Ğ´
                                            </div>
                                            <div className="text-[10px] text-slate-400 leading-tight h-10 overflow-y-auto scrollbar-none">
                                              {lvl.appearance}
                                            </div>
                                          </div>
                                          <div className="space-y-2">
                                            {lvl.units && (
                                              <div className="flex items-center justify-between text-[9px]">
                                                <span className="text-slate-500 uppercase">
                                                  Ğ’Ğ¾Ğ¹ÑĞºĞ°:
                                                </span>
                                                <span className="text-blue-400 font-bold text-right">
                                                  {lvl.units}
                                                </span>
                                              </div>
                                            )}
                                            {lvl.income && (
                                              <div className="flex items-center justify-between text-[9px]">
                                                <span className="text-slate-500 uppercase">
                                                  Ğ”Ğ¾Ñ…Ğ¾Ğ´:
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
                              <Users className="w-4 h-4" /> Ğ›Ğ¸Ğ¼Ğ¸Ñ‚Ñ‹ Ğ¿Ğ¾ĞºÑƒĞ¿ĞºĞ¸
                              Ğ³ĞµÑ€Ğ¾ĞµĞ²
                            </h3>
                            <div className="grid grid-cols-2 gap-2">
                              {[
                                {
                                  l: "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ 1",
                                  v: gameDesign?.hero_limits?.continent_1,
                                },
                                {
                                  l: "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ 2",
                                  v: gameDesign?.hero_limits?.continent_2,
                                },
                                {
                                  l: "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ 3",
                                  v: gameDesign?.hero_limits?.continent_3,
                                },
                                {
                                  l: "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ 4",
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
                                    {item.v} ÑˆÑ‚
                                  </span>
                                </div>
                              ))}
                            </div>
                            <div className="mt-2 text-[8px] text-purple-300 font-bold text-center uppercase tracking-widest">
                              +1 ĞÑĞ½Ğ¾Ğ²Ğ½Ğ¾Ğ¹ Ğ³ĞµÑ€Ğ¾Ğ¹ Ñƒ Ğ²ÑĞµÑ… (Ğ˜Ğ³Ñ€Ğ¾Ğº/Ğ˜Ğ˜)
                            </div>
                          </div>

                          <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                            ĞÑĞ½Ğ¾Ğ²Ğ½Ñ‹Ğµ ĞšĞ»Ğ°ÑÑÑ‹
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
                                        {h.class === "Ğ’Ğ¾Ğ¸Ğ½" ? (
                                          <Shield className="w-6 h-6" />
                                        ) : h.class === "Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº" ? (
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
                            ĞŸĞ¾Ğ´-Ğ³ĞµÑ€Ğ¾Ğ¸ (ĞÑ‚Ñ€ÑĞ´)
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
                                      {h.class === "Ğ’Ğ¾Ğ¸Ğ½" ? (
                                        <Shield className="w-4 h-4" />
                                      ) : h.class === "Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº" ? (
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
                              Ğ¡Ğ¸ÑÑ‚ĞµĞ¼Ğ° Ğ“Ñ€ÑƒĞ·Ğ¾Ğ¿Ğ¾Ğ´ÑŠÑ‘Ğ¼Ğ½Ğ¾ÑÑ‚Ğ¸
                            </h4>
                            <div className="space-y-4">
                              <div className="grid grid-cols-1 gap-2">
                                {[
                                  {
                                    label: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹ Ğ“ĞµÑ€Ğ¾Ğ¹ (L1)",
                                    value: "600 ĞšĞ“",
                                    sub: "+500ĞºĞ³ ĞºĞ°Ğ¶Ğ´Ñ‹Ğµ 100 ÑƒÑ€.",
                                  },
                                  {
                                    label: "Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğ¹ Ğ“ĞµÑ€Ğ¾Ğ¹ (L1)",
                                    value: "1200 ĞšĞ“",
                                    sub: "+1000ĞºĞ³ ĞºĞ°Ğ¶Ğ´Ñ‹Ğµ 100 ÑƒÑ€.",
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
                                <Skull className="w-4 h-4" /> Ğ Ğ°Ğ·Ğ±Ğ¾Ğ¹Ğ½Ğ¸ĞºĞ¸: Ğ Ğ°Ğ½Ğ³Ğ¸
                                Ğ¸ Ğ¥Ğ°Ñ€Ğ°ĞºÑ‚ĞµÑ€Ğ¸ÑÑ‚Ğ¸ĞºĞ¸
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
                                        {rank.rank} (ÑƒÑ€. {rank.level})
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
                                Ğ“ĞµÑ€Ğ¾Ğ¸ Ğ Ğ°Ğ·Ğ±Ğ¾Ğ¹Ğ½Ğ¸ĞºĞ¾Ğ²
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
                                      {hero.name === "Ğ¢ĞµĞ½ÑŒ Ğ’ĞµÑ‚Ñ€Ğ°" ? (
                                        <Zap className="w-5 h-5" />
                                      ) : hero.name === "ĞĞ³Ğ½ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ›Ğ¸Ñ" ? (
                                        <Flame className="w-5 h-5" />
                                      ) : hero.name === "Ğ“Ğ»Ğ°Ğ· Ğ¯ÑÑ‚Ñ€ĞµĞ±Ğ°" ? (
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
                                          Ğ£Ğ»ÑŒÑ‚Ğ¸Ğ¼ĞµĞ¹Ñ‚: {hero.ultimate.name}
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
                                Ğ’Ğ½ĞµÑˆĞ½Ğ¸Ğ¹ Ğ²Ğ¸Ğ´ Ğ¿Ğ¾ ĞºĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°Ğ¼
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
                                          ĞĞ´ĞµĞ¶Ğ´Ğ°:
                                        </span>{" "}
                                        {style.appearance.clothes}
                                      </p>
                                      <p className="text-[8px] text-slate-400">
                                        <span className="text-slate-500 font-bold uppercase">
                                          ĞÑ€ÑƒĞ¶Ğ¸Ğµ:
                                        </span>{" "}
                                        {style.appearance.weapons}
                                      </p>
                                      <p className="text-[8px] text-slate-400">
                                        <span className="text-slate-500 font-bold uppercase">
                                          ĞÑĞ¾Ğ±ĞµĞ½Ğ½Ğ¾ÑÑ‚Ğ¸:
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
                                ĞŸÑ€Ğ¾ÑÑ‚Ñ‹Ğµ Ğ“ĞµÑ€Ğ¾Ğ¸ (L1-5000)
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
                                            ĞÑ‚ĞºĞ°Ñ‚: {s.cd}Ñ…
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
                                Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğµ Ğ“ĞµÑ€Ğ¾Ğ¸ (L1-5000)
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
                                            ĞÑ‚ĞºĞ°Ñ‚: {s.cd}Ñ…
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
                                Ğ¡Ğ¸ÑÑ‚ĞµĞ¼Ğ° ĞÑ‚ĞºĞ°Ñ‚Ğ¾Ğ²
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
                                            ? "Ğ­ĞºĞ¸Ğ¿Ğ¸Ñ€Ğ¾Ğ²ĞºĞ°"
                                            : key === "skills"
                                              ? "ĞĞ°Ğ²Ñ‹ĞºĞ¸"
                                              : key === "locations"
                                                ? "Ğ›Ğ¾ĞºĞ°Ñ†Ğ¸Ğ¸"
                                                : "Ğ­Ñ„Ñ„ĞµĞºÑ‚Ñ‹"}
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
                                      ĞĞ³Ñ€Ğ°Ğ½Ğ¸Ñ‡ĞµĞ½Ğ¸Ñ
                                    </span>
                                  </div>
                                  <ul className="text-[10px] text-slate-400 space-y-1 list-disc pl-4 italic">
                                    <li>ĞœĞ¸Ğ½. Ğ¾Ñ‚ĞºĞ°Ñ‚ Ğ¾Ğ±Ñ‹Ñ‡Ğ½Ñ‹Ñ…: 1 Ñ…Ğ¾Ğ´</li>
                                    <li>ĞœĞ¸Ğ½. Ğ¾Ñ‚ĞºĞ°Ñ‚ Ğ³ĞµÑ€Ğ¾Ğ¸Ñ‡: 3 Ñ…Ğ¾Ğ´Ğ°</li>
                                    <li>ĞĞºÑ€ÑƒĞ³Ğ»ĞµĞ½Ğ¸Ğµ Ğ²ÑĞµÑ… Ğ·Ğ½Ğ°Ñ‡ĞµĞ½Ğ¸Ğ¹ Ğ²Ğ²ĞµÑ€Ñ…</li>
                                  </ul>
                                </div>
                              </div>
                            </div>

                            <div className="space-y-6">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                                ĞœĞµÑ…Ğ°Ğ½Ğ¸ĞºĞ° ĞŸÑ€Ğ¾ĞºĞ°Ñ‡ĞºĞ¸
                              </h3>
                              <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-purple-600/10 to-transparent border border-purple-500/20 space-y-6">
                                <div className="space-y-4">
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-400">
                                      ĞŸÑ€Ğ¾ÑÑ‚Ñ‹Ğµ Ğ³ĞµÑ€Ğ¾Ğ¸
                                    </span>
                                    <span className="text-white font-bold">
                                      +1 XP / Ğ¸ÑĞ¿Ğ¾Ğ»ÑŒĞ·Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between text-[10px]">
                                    <span className="text-slate-400">
                                      Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğµ Ğ³ĞµÑ€Ğ¾Ğ¸
                                    </span>
                                    <span className="text-purple-400 font-bold">
                                      +2 XP / Ğ¸ÑĞ¿Ğ¾Ğ»ÑŒĞ·Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ
                                    </span>
                                  </div>
                                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                                    <h5 className="text-[9px] font-black text-white uppercase mb-2">
                                      Ğ’Ğ¸Ğ·ÑƒĞ°Ğ»ÑŒĞ½Ğ°Ñ ÑĞ²Ğ¾Ğ»ÑÑ†Ğ¸Ñ
                                    </h5>
                                    <div className="space-y-2 text-[9px] text-slate-500 italic">
                                      <p>1-5 ÑƒÑ€: Ğ›ĞµĞ³ĞºĞ¾Ğµ Ğ¼ĞµÑ€Ñ†Ğ°Ğ½Ğ¸Ğµ Ğ¸ĞºĞ¾Ğ½ĞºĞ¸</p>
                                      <p>6-15 ÑƒÑ€: Ğ¯Ñ€ĞºĞ¸Ğ¹ ÑĞ²ĞµÑ‚ + Ğ°Ğ½Ğ¸Ğ¼Ğ°Ñ†Ğ¸Ñ</p>
                                      <p>16-20 ÑƒÑ€: Ğ­Ñ„Ğ¸Ñ€Ğ½Ğ°Ñ Ğ°ÑƒÑ€Ğ° Ğ²Ğ¾ĞºÑ€ÑƒĞ³</p>
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
                              ĞŸÑ€Ğ¾Ğ³Ñ€ĞµÑÑĞ¸Ñ Ğ¡Ğ¸Ğ½ĞµÑ€Ğ³Ğ¸Ğ¹
                            </h3>
                            <p className="text-[10px] text-slate-500 uppercase tracking-widest mt-1">
                              Ğ’Ğ·Ğ°Ğ¸Ğ¼Ğ¾Ğ´ĞµĞ¹ÑÑ‚Ğ²Ğ¸Ğµ ÑƒĞ¼ĞµĞ½Ğ¸Ğ¹ Ğ¸ ÑƒÑ€Ğ¾Ğ²Ğ½Ğ¸ Ğ¼Ğ°ÑÑ‚ĞµÑ€ÑÑ‚Ğ²Ğ° (1â€”9999)
                            </p>
                          </div>
                          <div className="flex p-1 bg-black/40 rounded-2xl border border-white/5">
                            <button
                              onClick={() => setSynergyHeroType("simple")}
                              className={`px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${synergyHeroType === "simple" ? "bg-blue-600 text-white shadow-[0_0_20px_rgba(37,99,235,0.3)]" : "text-slate-500 hover:text-slate-300"}`}
                            >
                              ĞŸÑ€Ğ¾ÑÑ‚Ñ‹Ğµ Ğ³ĞµÑ€Ğ¾Ğ¸
                            </button>
                            <button
                              onClick={() => setSynergyHeroType("main")}
                              className={`px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${synergyHeroType === "main" ? "bg-purple-600 text-white shadow-[0_0_20px_rgba(147,51,234,0.3)]" : "text-slate-500 hover:text-slate-300"}`}
                            >
                              Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğµ Ğ³ĞµÑ€Ğ¾Ğ¸
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
                                      ? "Ğ’Ğ¾Ğ¸Ğ½"
                                      : cls === "Mage"
                                        ? "ĞœĞ°Ğ³"
                                        : "Ğ¡Ñ‚Ñ€ĞµĞ»Ğ¾Ğº"}
                                  </h3>
                                  <div className="flex items-center gap-2">
                                    <span className="w-2 h-2 rounded-full bg-purple-500 animate-pulse" />
                                    <span className="text-[9px] text-slate-500 uppercase font-black tracking-widest">
                                      ĞŸÑ€Ğ¾Ğ³Ñ€ĞµÑÑĞ¸Ñ 5 Ğ¢Ğ¸Ñ€Ğ¾Ğ²
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
                                        Ğ£Ğ½Ğ¸ĞºĞ°Ğ»ÑŒĞ½Ñ‹Ğµ Ğ¾ÑĞ¾Ğ±ĞµĞ½Ğ½Ğ¾ÑÑ‚Ğ¸
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
                              ĞĞ±Ñ‰Ğ¸Ğµ Ğ¼ĞµÑ…Ğ°Ğ½Ğ¸ĞºĞ¸ Ğ²Ğ·Ğ°Ğ¸Ğ¼Ğ¾Ğ´ĞµĞ¹ÑÑ‚Ğ²Ğ¸Ñ
                            </h4>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                              {[
                                {
                                  title: "Ğ­Ñ„Ñ„ĞµĞºÑ‚ Â«ĞšĞ¾Ğ¼Ğ±Ğ¾Â»",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.combo_effect,
                                  icon: (
                                    <Zap className="w-4 h-4 text-yellow-500" />
                                  ),
                                },
                                {
                                  title: "Ğ¡Ğ¿ĞµÑ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.specialization,
                                  icon: (
                                    <Sparkles className="w-4 h-4 text-purple-500" />
                                  ),
                                },
                                {
                                  title: "ĞĞ´Ğ°Ğ¿Ñ‚Ğ°Ñ†Ğ¸Ñ",
                                  desc: gameDesign?.skill_synergies?.general
                                    ?.adaptation,
                                  icon: (
                                    <RefreshCw className="w-4 h-4 text-blue-500" />
                                  ),
                                },
                                {
                                  title: "ĞœĞ°ÑÑ‚ĞµÑ€ÑÑ‚Ğ²Ğ¾",
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
                              ĞŸÑ€Ğ°Ğ²Ğ¸Ğ»Ğ° ĞĞºÑ‚Ğ¸Ğ²Ğ°Ñ†Ğ¸Ğ¸
                            </h4>
                            <div className="space-y-6">
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div className="space-y-2">
                                  <div className="text-[9px] text-slate-500 uppercase font-black">
                                    ĞĞºĞ½Ğ¾ Ğ°ĞºÑ‚Ğ¸Ğ²Ğ°Ñ†Ğ¸Ğ¸
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
                                    ĞŸĞµÑ€ĞµĞ·Ğ°Ñ€ÑĞ´ĞºĞ° ÑĞ¸Ğ½ĞµÑ€Ğ³Ğ¸Ğ¸
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
                                  Ğ’Ğ»Ğ¸ÑĞ½Ğ¸Ğµ Ğ¼ĞµÑÑ‚Ğ½Ğ¾ÑÑ‚Ğ¸
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
                                          ? "Ğ“Ğ¾Ñ€Ñ‹"
                                          : key === "forest"
                                            ? "Ğ›ĞµÑ"
                                            : "Ğ Ğ°Ğ²Ğ½Ğ¸Ğ½Ñ‹"}
                                      </div>
                                      <div className="text-[10px] text-slate-300 font-bold">
                                        {val}
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              </div>
                              <div className="p-6 bg-purple-600/10 rounded-[2.5rem] border border-purple-500/20 text-[11px] text-purple-300 italic leading-relaxed">
                                "Ğ¡Ğ¸Ğ½ĞµÑ€Ğ³Ğ¸Ğ¸ Ñ‚Ñ€ĞµĞ±ÑƒÑÑ‚ Ğ½ĞµĞ¿Ğ¾ÑÑ€ĞµĞ´ÑÑ‚Ğ²ĞµĞ½Ğ½Ğ¾Ğ³Ğ¾ ÑƒÑ‡Ğ°ÑÑ‚Ğ¸Ñ
                                Ğ¸Ğ³Ñ€Ğ¾ĞºĞ° Ğ¸ Ğ¿Ğ¾Ğ½Ğ¸Ğ¼Ğ°Ğ½Ğ¸Ñ Ñ‚Ğ°Ğ¹Ğ¼Ğ¸Ğ½Ğ³Ğ¾Ğ². ĞŸÑ€Ğ¸ Ğ°ĞºÑ‚Ğ¸Ğ²Ğ°Ñ†Ğ¸Ğ¸
                                Ğ¸ĞºĞ¾Ğ½ĞºĞ° ÑƒĞ¼ĞµĞ½Ğ¸Ñ Ğ¼ĞµÑ€Ñ†Ğ°ĞµÑ‚ Ğ·Ğ¾Ğ»Ğ¾Ñ‚Ñ‹Ğ¼, Ğ° Ğ·Ğ²ÑƒĞºĞ¾Ğ²Ğ¾Ğ¹ ÑĞ¸Ğ³Ğ½Ğ°Ğ»
                                Ğ¿Ğ¾Ğ´Ñ‚Ğ²ĞµÑ€Ğ¶Ğ´Ğ°ĞµÑ‚ ÑƒÑĞ¿ĞµÑ… ĞºĞ¾Ğ¼Ğ±Ğ¸Ğ½Ğ°Ñ†Ğ¸Ğ¸."
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
                                ĞœĞµÑ…Ğ°Ğ½Ğ¸ĞºĞ° Ğ£Ñ€Ğ¾Ğ½Ğ°
                              </h4>
                              <div className="px-3 py-1 bg-blue-500/10 rounded-lg text-[9px] text-blue-400 font-black uppercase">
                                Ğ£Ñ€. 1â€”9999
                              </div>
                            </div>

                            <div className="p-8 rounded-[2.5rem] bg-gradient-to-br from-blue-600/10 to-transparent border border-blue-500/20 space-y-6">
                              <div className="flex items-center gap-4">
                                <div className="p-4 bg-blue-600 rounded-2xl text-white shadow-lg">
                                  <Calculator className="w-6 h-6" />
                                </div>
                                <div className="flex-1">
                                  <h5 className="text-[10px] font-black text-slate-400 uppercase italic mb-1">
                                    Ğ‘Ğ°Ğ·Ğ¾Ğ²Ğ°Ñ Ğ¤Ğ¾Ñ€Ğ¼ÑƒĞ»Ğ°
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
                                Ğ¡Ğ¸ÑÑ‚ĞµĞ¼Ğ° Ğ—Ğ°ĞºĞ»Ğ¸Ğ½Ğ°Ğ½Ğ¸Ğ¹
                              </h5>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="p-5 bg-purple-600/5 border border-purple-500/20 rounded-2xl space-y-2">
                                  <div className="text-[8px] text-purple-400 font-black uppercase">
                                    Ğ¡Ñ‚Ğ¾Ğ¸Ğ¼Ğ¾ÑÑ‚ÑŒ Ğ¼Ğ°Ğ½Ñ‹
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
                                    Ğ£Ñ€Ğ¾Ğ½ Ğ·Ğ°ĞºĞ»Ğ¸Ğ½Ğ°Ğ½Ğ¸Ğ¹
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
                                ĞšĞ¾ÑÑ„Ñ„Ğ¸Ñ†Ğ¸ĞµĞ½Ñ‚Ñ‹ ĞŸÑ€Ğ¾Ğ³Ñ€ĞµÑÑĞ¸Ğ¸
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
                                  Ğ—Ğ°Ğ¼ĞµĞ´Ğ»ÑĞµÑ‚ Ñ€Ğ¾ÑÑ‚ Ñ…Ğ°Ñ€Ğ°ĞºÑ‚ĞµÑ€Ğ¸ÑÑ‚Ğ¸Ğº Ğ¿Ñ€Ğ¸ Ğ´Ğ¾ÑÑ‚Ğ¸Ğ¶ĞµĞ½Ğ¸Ğ¸
                                  Ğ¿Ğ¾Ñ€Ğ¾Ğ³Ğ° Ğ²{" "}
                                  {
                                    gameDesign?.combat_mechanics?.formulas
                                      ?.high_level_balancer?.threshold
                                  }{" "}
                                  ÑƒÑ€Ğ¾Ğ²Ğ½ĞµĞ¹, Ğ¿Ñ€ĞµĞ´Ğ¾Ñ‚Ğ²Ñ€Ğ°Ñ‰Ğ°Ñ "Ñ€Ğ°Ğ·Ğ´ÑƒĞ²Ğ°Ğ½Ğ¸Ğµ" Ñ†Ğ¸Ñ„Ñ€.
                                </p>
                              </div>
                            </div>
                          </div>
                        </div>

                        <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                          <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">
                            Ğ¡Ñ‚Ğ°Ñ‚Ğ¸ÑÑ‚Ğ¸ĞºĞ° ĞšĞ»Ğ°ÑÑĞ¾Ğ² (Ğ‘Ğ°Ğ·Ğ° Ğ¸ Ğ Ğ¾ÑÑ‚)
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
                              Ğ­ĞºĞ¾Ğ½Ğ¾Ğ¼Ğ¸ĞºĞ° ĞĞ°Ğ¹Ğ¼Ğ°
                            </h3>
                            <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 overflow-hidden">
                              <table className="w-full text-left text-[10px]">
                                <thead>
                                  <tr className="text-slate-600 border-b border-white/5 uppercase tracking-widest">
                                    <th className="pb-4 pt-2 font-black">
                                      Ğ¢Ğ¸Ğ¿ Ğ²Ğ¾Ğ¹ÑĞº
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Ğ‘Ğ°Ğ·Ğ° (K)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Ğ¡Ğ¾ÑĞµĞ´ (+25%)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      Ğ”Ğ°Ğ»ÑŒĞ½Ğ¸Ğ¹ (+50%)
                                    </th>
                                    <th className="pb-4 pt-2 font-black">
                                      ĞšÑ€Ğ°Ğ¹Ğ½Ğ¸Ğ¹ (+100%)
                                    </th>
                                  </tr>
                                </thead>
                                <tbody className="divide-y divide-white/5">
                                  {[
                                    { name: "Ğ›ĞµĞ³ĞºĞ°Ñ Ğ±Ñ€Ğ¾Ğ½Ñ", base: "3000-5000" },
                                    {
                                      name: "Ğ¡Ñ€ĞµĞ´Ğ½ÑÑ Ğ±Ñ€Ğ¾Ğ½Ñ",
                                      base: "6000-10000",
                                    },
                                    {
                                      name: "Ğ¢ÑĞ¶ĞµĞ»Ğ°Ñ Ğ±Ñ€Ğ¾Ğ½Ñ",
                                      base: "11000-15000",
                                    },
                                    {
                                      name: "Ğ”Ğ°Ğ»ÑŒĞ½Ğ¸Ğµ (Ğ¡Ñ€)",
                                      base: "16000-20000",
                                    },
                                    {
                                      name: "Ğ›ĞµĞ³ĞµĞ½Ğ´Ğ°Ñ€Ğ½Ñ‹Ğµ",
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
                                Ğ¡ĞºĞ¸Ğ´ĞºĞ¸ Ğ¿Ğ¾ ĞšĞ»Ğ°ÑÑĞ°Ğ¼
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    Ğ’Ğ¾Ğ¸Ğ½
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Ğ¢ÑĞ¶ĞµĞ»Ğ°Ñ/Ğ‘Ğ»Ğ¸Ğ¶Ğ½Ğ¸Ğµ
                                  </div>
                                </div>
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    Ğ¡Ñ‚Ñ€ĞµĞ»Ğ¾Ğº
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Ğ”Ğ°Ğ»ÑŒĞ½Ğ¸Ğµ
                                  </div>
                                </div>
                                <div className="p-4 bg-black/40 rounded-2xl border border-white/5 space-y-1">
                                  <div className="text-[9px] text-slate-500 uppercase">
                                    ĞœĞ°Ğ³
                                  </div>
                                  <div className="text-xs text-white">
                                    -10% Ğ›ĞµĞ³ĞµĞ½Ğ´Ñ‹/ĞœĞ°Ğ³Ğ¸
                                  </div>
                                </div>
                              </div>
                            </div>

                            <div className="p-10 rounded-[3rem] bg-black/40 border border-white/10 space-y-8">
                              <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                                Ğ‘Ğ¾Ğ½ÑƒÑÑ‹ Ğ—Ğ°Ğ¼ĞºĞ¾Ğ² (L5)
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                                <div className="space-y-4">
                                  <div className="text-[9px] text-blue-400 font-black uppercase tracking-widest">
                                    Ğ¢Ğ¸Ğ¿Ñ‹ Ğ‘Ğ¾Ğ½ÑƒÑĞ¾Ğ²
                                  </div>
                                  <div className="space-y-2">
                                    {[
                                      {
                                        type: "Ğ­ĞºĞ¾Ğ½Ğ¾Ğ¼Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹",
                                        bonus: "+10% Ğ—Ğ¾Ğ»Ğ¾Ñ‚Ğ°",
                                      },
                                      { type: "Ğ’Ğ¾ĞµĞ½Ğ½Ñ‹Ğ¹", bonus: "+5% Ğ“Ñ€ÑƒĞ·" },
                                      {
                                        type: "ĞœĞ°Ğ³Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹",
                                        bonus: "-5% Ğ¦ĞµĞ½Ğ° Ğ›ĞµĞ³ĞµĞ½Ğ´",
                                      },
                                      {
                                        type: "Ğ¢Ğ¾Ñ€Ğ³Ğ¾Ğ²Ñ‹Ğ¹",
                                        bonus: "-5% Ğ¦ĞµĞ½Ğ° ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°",
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
                                    ĞœĞ½Ğ¾Ğ¶Ğ¸Ñ‚ĞµĞ»ÑŒ Ğ·Ğ° ĞºĞ¾Ğ»-Ğ²Ğ¾
                                  </div>
                                  <div className="space-y-2">
                                    {[
                                      { count: "3 Ğ—Ğ°Ğ¼ĞºĞ°", mult: "x1.5" },
                                      { count: "5 Ğ—Ğ°Ğ¼ĞºĞ°", mult: "x2.0" },
                                      { count: "7 Ğ—Ğ°Ğ¼ĞºĞ¾Ğ²", mult: "x3.0" },
                                      { count: "10 Ğ—Ğ°Ğ¼ĞºĞ¾Ğ²", mult: "x4.0" },
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
                                "Ğ’Ğ°Ğ¶Ğ½Ğ¾: ĞĞ° ÑƒÑ€Ğ¾Ğ²Ğ½Ğµ ÑĞ»Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸ 'ĞĞµĞ²ĞµÑ€Ğ¾ÑÑ‚Ğ½Ñ‹Ğ¹'
                                Ğ¼Ğ½Ğ¾Ğ¶Ğ¸Ñ‚ĞµĞ»ÑŒ Ğ±Ğ¾Ğ½ÑƒÑĞ¾Ğ² Ğ·Ğ°Ğ¼ĞºĞ¾Ğ² ÑĞ½Ğ¸Ğ¶ĞµĞ½ Ğ´Ğ¾ x0.6, Ğ° Ğ¼Ğ°ĞºÑ.
                                ĞºĞ¾Ğ»-Ğ²Ğ¾ Ğ·Ğ°Ğ¼ĞºĞ¾Ğ² Ğ¾Ğ´Ğ½Ğ¾Ğ³Ğ¾ Ñ‚Ğ¸Ğ¿Ğ° Ğ¾Ğ³Ñ€Ğ°Ğ½Ğ¸Ñ‡ĞµĞ½Ğ¾ ÑĞµĞ¼ÑŒÑ Ğ´Ğ¾
                                1000 ÑƒÑ€Ğ¾Ğ²Ğ½Ñ."
                              </div>
                            </div>
                          </div>

                          <div className="space-y-8">
                            <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4">
                              Ğ£Ñ€Ğ¾Ğ²Ğ½Ğ¸ Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸
                            </h3>
                            <div className="grid grid-cols-1 gap-4">
                              {[
                                {
                                  name: "ĞĞ¾Ğ²Ğ¸Ñ‡Ğ¾Ğº",
                                  color: "text-green-400",
                                  desc: "-20% Ğ¦ĞµĞ½Ğ°, +20% Ğ“Ñ€ÑƒĞ·Ğ¾Ğ²Ğ¸Ğº, +50% Ğ—Ğ¾Ğ»Ğ¾Ñ‚Ğ¾",
                                },
                                {
                                  name: "Ğ¡Ñ€ĞµĞ´Ğ½Ğ¸Ğ¹",
                                  color: "text-blue-400",
                                  desc: "Ğ‘Ğ°Ğ·Ğ¾Ğ²Ñ‹Ğµ Ğ¿Ğ°Ñ€Ğ°Ğ¼ĞµÑ‚Ñ€Ñ‹",
                                },
                                {
                                  name: "Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ñ‹Ğ¹",
                                  color: "text-orange-400",
                                  desc: "+25% Ğ¦ĞµĞ½Ğ°, -15% Ğ“Ñ€ÑƒĞ·Ğ¾Ğ²Ğ¸Ğº, -20% Ğ—Ğ¾Ğ»Ğ¾Ñ‚Ğ¾",
                                },
                                {
                                  name: "ĞĞµĞ²ĞµÑ€Ğ¾ÑÑ‚Ğ½Ñ‹Ğ¹",
                                  color: "text-red-500",
                                  desc: "+50% Ğ¦ĞµĞ½Ğ°, -30% Ğ“Ñ€ÑƒĞ·Ğ¾Ğ²Ğ¸Ğº, -40% Ğ—Ğ¾Ğ»Ğ¾Ñ‚Ğ¾, Ğ›ĞµĞ³ĞµĞ½Ğ´Ñ‹ Ñ 1000 ÑƒÑ€Ğ¾Ğ²Ğ½Ñ",
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
                              Ğ¢Ğ°ĞºÑ‚Ğ¸Ñ‡ĞµÑĞºĞ¾Ğµ Ğ Ğ°Ğ·Ğ²ĞµÑ€Ñ‚Ñ‹Ğ²Ğ°Ğ½Ğ¸Ğµ Ğ Ğ°Ğ·Ğ±Ğ¾Ğ¹Ğ½Ğ¸ĞºĞ¾Ğ²
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
                                        ? "Ğ’ĞµÑ‚Ñ€ĞµĞ½Ñ‹Ğµ Ğ Ğ°Ğ²Ğ½Ğ¸Ğ½Ñ‹"
                                        : key === "mountain_range"
                                          ? "Ğ“Ğ¾Ñ€Ğ½Ñ‹Ğ¹ Ğ¥Ñ€ĞµĞ±ĞµÑ‚"
                                          : key === "ancient_woods"
                                            ? "Ğ”Ñ€ĞµĞ²Ğ½Ğ¸Ğµ Ğ›ĞµÑĞ°"
                                            : "Ğ Ğ°ÑÑĞ²ĞµÑ‚ Ğ˜Ğ¼Ğ¿ĞµÑ€Ğ¸Ğ¸"}
                                    </h4>
                                    <span className="px-3 py-1 bg-red-500/10 rounded-xl text-[10px] font-black text-red-500">
                                      {strat.cities} Ğ“Ğ¾Ñ€Ğ¾Ğ´Ğ°
                                    </span>
                                  </div>
                                  <p className="text-[11px] text-slate-400 leading-relaxed">
                                    <span className="text-slate-500 font-bold uppercase mr-2">
                                      Ğ¢Ğ°ĞºÑ‚Ğ¸ĞºĞ°:
                                    </span>
                                    {strat.tactics}
                                  </p>
                                  <div className="space-y-2">
                                    <div className="text-[9px] text-slate-600 uppercase font-black">
                                      ĞŸÑ€Ğ¸Ğ¾Ñ€Ğ¸Ñ‚ĞµÑ‚Ñ‹ Ñ€Ğ°Ğ·Ğ²Ğ¸Ñ‚Ğ¸Ñ:
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
                              ĞŸÑ€Ğ¾Ğ³Ñ€ĞµÑÑĞ¸Ñ Ğ¿Ğ¾ Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸
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
                                      Ğ”Ğ¸Ğ°Ğ¿Ğ°Ğ·Ğ¾Ğ½ Ğ£Ğ : {scale.lvl_range}
                                    </span>
                                  </div>
                                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                                    <div className="text-[8px] text-slate-500 uppercase font-black mb-1">
                                      ĞÑĞ½Ğ¾Ğ²Ğ½Ñ‹Ğµ Ğ¦ĞµĞ»Ğ¸:
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
                                ĞĞ±Ñ‰Ğ¸Ğµ Ğ ĞµĞºĞ¾Ğ¼ĞµĞ½Ğ´Ğ°Ñ†Ğ¸Ğ¸
                              </h4>
                              <div className="space-y-4">
                                {[
                                  {
                                    t: "Ğ Ğ°Ğ·Ğ²Ğ¸Ñ‚Ğ¸Ğµ Ğ³Ğ¾Ñ€Ğ¾Ğ´Ğ¾Ğ²",
                                    d: "ĞŸÑ€Ğ¸Ğ¾Ñ€Ğ¸Ñ‚ĞµÑ‚ Ğ½Ğ° ÑĞºĞ¾Ğ½Ğ¾Ğ¼Ğ¸Ñ‡ĞµÑĞºĞ¸Ğµ Ğ¿Ğ¾ÑÑ‚Ñ€Ğ¾Ğ¹ĞºĞ¸ Ğ² Ğ½Ğ°Ñ‡Ğ°Ğ»Ğµ, Ğ·Ğ°Ñ‚ĞµĞ¼ Ğ²Ğ¾ĞµĞ½Ğ½Ñ‹Ğµ.",
                                  },
                                  {
                                    t: "ĞŸĞ¾ĞºÑƒĞ¿ĞºĞ° Ğ²Ğ¾Ğ¹ÑĞº",
                                    d: "Ğ‘Ğ°Ğ»Ğ°Ğ½ÑĞ¸Ñ€ÑƒĞ¹Ñ‚Ğµ Ğ¼ĞµĞ¶Ğ´Ñƒ Ğ¼Ğ°ÑÑĞ¾Ğ²ĞºĞ¾Ğ¹ Ğ¸ ÑĞ»Ğ¸Ñ‚Ğ½Ñ‹Ğ¼Ğ¸ Ğ¾Ñ‚Ñ€ÑĞ´Ğ°Ğ¼Ğ¸ Ğ´Ğ»Ñ Ğ·Ğ°ÑĞ°Ğ´.",
                                  },
                                  {
                                    t: "Ğ˜ÑĞ¿Ğ¾Ğ»ÑŒĞ·Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ Ğ·ĞµĞ»Ğ¸Ğ¹",
                                    d: "Ğ’ÑĞµĞ³Ğ´Ğ° Ğ´ĞµÑ€Ğ¶Ğ¸Ñ‚Ğµ Ğ·Ğ°Ğ¿Ğ°Ñ Ğ·ĞµĞ»Ğ¸Ğ¹ ÑĞºĞ¾Ñ€Ğ¾ÑÑ‚Ğ¸ Ğ¸ Ğ½ĞµĞ²Ğ¸Ğ´Ğ¸Ğ¼Ğ¾ÑÑ‚Ğ¸.",
                                  },
                                  {
                                    t: "Ğ“ĞµÑ€Ğ¾Ğ¸",
                                    d: "Ğ¡Ğ¿ĞµÑ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ¸Ñ€ÑƒĞ¹Ñ‚Ğµ Ğ³ĞµÑ€Ğ¾ĞµĞ² Ğ¿Ğ¾Ğ´ ÑƒÑĞ»Ğ¾Ğ²Ğ¸Ñ ĞºĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°.",
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
                              <MapIcon className="w-3 h-3" /> ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ñ‹ Ğ¸
                              Ğ›Ğ°Ğ½Ğ´ÑˆĞ°Ñ„Ñ‚
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
                                        Ğ¡Ğ¾ÑÑ‚Ğ°Ğ² ĞšĞ»ĞµÑ‚Ğ¾Ğº:
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
                                        Ğ­Ñ„Ñ„ĞµĞºÑ‚Ñ‹:
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
                                      Ğ¢Ğ°ĞºÑ‚Ğ¸ĞºĞ°:
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
                                Ğ”Ğ¸Ğ½Ğ°Ğ¼Ğ¸Ñ‡ĞµÑĞºĞ¸Ğµ Ğ¡Ğ¾Ğ±Ñ‹Ñ‚Ğ¸Ñ
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
                                    Ğ”Ğ½ĞµĞ²Ğ½Ğ¾Ğ¹ Ğ¦Ğ¸ĞºĞ»
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
                                    ĞĞ¾Ñ‡Ğ½Ğ¾Ğ¹ Ğ¦Ğ¸ĞºĞ»
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
                                Ğ¡Ğ»ÑƒÑ‡Ğ°Ğ¹Ğ½Ñ‹Ğµ Ğ£Ğ³Ñ€Ğ¾Ğ·Ñ‹
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
                                          Ğ¨Ğ°Ğ½Ñ: {data.chance * 100}%
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
                            Ğ‘Ğ¾ĞµĞ²Ñ‹Ğµ ĞĞ±ÑŠĞµĞºÑ‚Ñ‹ Ğ¸ ĞšĞ»ĞµÑ‚ĞºĞ¸
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
                                    {data.effects || "Ğ¡Ñ‚Ğ°Ğ½Ğ´Ğ°Ñ€Ñ‚"}
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
                                DialogueSystem_Manager â€¢ Ğ¡Ğ¸Ğ¼ÑƒĞ»ÑÑ‚Ğ¾Ñ€ Ğ”Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ°
                              </h3>
                              <p className="text-xs text-slate-400 mt-1 max-w-2xl">
                                ĞŸÑ€Ğ¾ĞµĞºÑ‚Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½Ğ¸Ğµ Ğ¸ Ğ²Ğ¸Ğ·ÑƒĞ°Ğ»Ğ¸Ğ·Ğ°Ñ†Ğ¸Ñ Ğ½ĞµĞ»Ğ¸Ğ½ĞµĞ¹Ğ½Ñ‹Ñ…
                                Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ¾Ğ²Ñ‹Ñ… Ñ†ĞµĞ¿Ğ¾Ñ‡ĞµĞº Ñ Ğ²Ñ‹Ğ±Ğ¾Ñ€Ğ¾Ğ¼ ĞºĞ»Ğ°ÑÑĞ° Ğ³ĞµÑ€Ğ¾Ñ Ğ¸
                                Ñ‚Ğ°ĞºÑ‚Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ¹ Ğ¾Ğ±Ğ»Ğ°ÑÑ‚Ğ¸ ÑƒĞ´Ğ°Ğ»ĞµĞ½Ğ¸Ñ ÑĞºĞ²ĞµÑ€Ğ½Ñ‹.
                              </p>
                            </div>

                            {/* Controls (Language) */}
                            <div className="flex flex-wrap items-center gap-4 relative z-20">
                              <span className="text-[10px] font-black text-slate-500 uppercase tracking-wider">
                                Ğ¯Ğ·Ñ‹Ğº Ğ¾Ğ·Ğ²ÑƒÑ‡ĞºĞ¸ / Ñ‚ĞµĞºÑÑ‚Ğ°:
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
                                  â† Ğ¡Ğ¼ĞµĞ½Ğ¸Ñ‚ÑŒ Ğ“ĞµÑ€Ğ¾Ñ
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
                                  Ğ¨ĞĞ“ 1: Ğ’Ğ«Ğ‘Ğ•Ğ Ğ˜Ğ¢Ğ• ĞŸĞ•Ğ Ğ¡ĞĞĞĞ–Ğ Ğ”Ğ›Ğ¯ Ğ’Ğ¥ĞĞ”Ğ Ğ’ Ğ¡Ğ¦Ğ•ĞĞ£
                                  Ğ”Ğ˜ĞĞ›ĞĞ“Ğ
                                </span>
                                <h4 className="text-xl font-black text-white uppercase italic tracking-tight">
                                  ĞšÑ‚Ğ¾ Ğ¿Ğ¾Ğ²ĞµĞ´ĞµÑ‚ Ğ¾Ñ‚Ñ€ÑĞ´ Ğ½Ğ° Ğ·Ğ°Ñ‡Ğ¸ÑÑ‚ĞºÑƒ ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°?
                                </h4>
                                <p className="text-[11px] text-slate-400">
                                  Ğ’Ñ‹Ğ±ĞµÑ€Ğ¸Ñ‚Ğµ Ğ³ĞµÑ€Ğ¾Ñ, Ñ‡Ñ‚Ğ¾Ğ±Ñ‹ Ğ¸Ğ½Ğ¸Ñ†Ğ¸Ğ°Ğ»Ğ¸Ğ·Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ 1:1
                                  Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ¾Ğ²Ğ¾Ğµ Ğ¾ĞºĞ½Ğ¾ Ñ ĞÑĞ»Ğ¸ÑÑĞ¾Ğ¹, Ğ½Ğ°ÑÑ‚Ñ€Ğ¾Ğ¸Ñ‚ÑŒ ÑĞ¿Ñ€Ğ°Ğ¹Ñ‚Ñ‹
                                  Ğ² Unity Ğ¸ Ğ¾Ğ¿Ñ€Ğ¾Ğ±Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ğ½ĞµĞ»Ğ¸Ğ½ĞµĞ¹Ğ½Ñ‹Ğµ Ñ€Ğ°Ğ·Ğ²Ğ¸Ğ»ĞºĞ¸
                                  ÑÑĞ¶ĞµÑ‚Ğ°.
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
                                          ĞšĞ›ĞĞ¡Ğ¡ Ğ’ĞĞ˜Ğ
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          ĞšĞ¾Ñ€Ğ¾Ğ½Ğ½Ñ‹Ğ¹ Ğ’Ğ¾Ğ¸Ñ‚ĞµĞ»ÑŒ
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° 1:1 (Warrior Headshot)
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
                                              "ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° Ğ’Ğ¾Ğ¸Ğ½Ğ° ÑĞºĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          ĞšĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ğ¿Ñ€Ğ¾Ğ¼Ğ¿Ñ‚ ğŸ“‹
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
                                        "Ğ¡Ñ†ĞµĞ½Ğ° Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ° Ğ·Ğ°Ğ¿ÑƒÑ‰ĞµĞ½Ğ° Ğ·Ğ° ĞºĞ»Ğ°ÑÑ: Ğ’Ğ¾Ğ¸Ğ½",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-red-600 to-amber-600 hover:from-red-500 hover:to-amber-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Ğ’Ñ‹Ğ±Ñ€Ğ°Ñ‚ÑŒ Ğ¸ ĞĞ°Ñ‡Ğ°Ñ‚ÑŒ Ğ”Ğ¸Ğ°Ğ»Ğ¾Ğ³ âš”ï¸
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
                                          ĞšĞ›ĞĞ¡Ğ¡ Ğ¡Ğ¢Ğ Ğ•Ğ›ĞĞš
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº Ğ² ĞšĞ°Ğ¿ÑÑˆĞ¾Ğ½Ğµ
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° 1:1 (Archer Headshot)
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
                                              "ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° Ğ›ÑƒÑ‡Ğ½Ğ¸ĞºĞ° ÑĞºĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          ĞšĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ğ¿Ñ€Ğ¾Ğ¼Ğ¿Ñ‚ ğŸ“‹
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
                                        "Ğ¡Ñ†ĞµĞ½Ğ° Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ° Ğ·Ğ°Ğ¿ÑƒÑ‰ĞµĞ½Ğ° Ğ·Ğ° ĞºĞ»Ğ°ÑÑ: Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-emerald-600 to-indigo-600 hover:from-emerald-500 hover:to-indigo-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Ğ’Ñ‹Ğ±Ñ€Ğ°Ñ‚ÑŒ Ğ¸ ĞĞ°Ñ‡Ğ°Ñ‚ÑŒ Ğ”Ğ¸Ğ°Ğ»Ğ¾Ğ³ ğŸ¹
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
                                          ĞšĞ›ĞĞ¡Ğ¡ ĞœĞĞ“
                                        </span>
                                        <h5 className="text-lg font-black text-white uppercase italic tracking-tight">
                                          Ğ—Ğ²ĞµĞ·Ğ´Ğ½Ñ‹Ğ¹ ĞÑ€Ñ…Ğ¸Ğ¼Ğ°Ğ³
                                        </h5>
                                      </div>
                                    </div>

                                    <div className="space-y-2 text-[11px] text-slate-300">
                                      <div className="font-mono bg-black/40 p-3 rounded-xl border border-white/5 space-y-1">
                                        <div className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">
                                          ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° 1:1 (Mage Headshot)
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
                                              "ĞŸÑ€Ğ¾Ğ¼Ğ¿Ñ‚ Ğ¿Ğ¾Ñ€Ñ‚Ñ€ĞµÑ‚Ğ° ĞœĞ°Ğ³Ğ° ÑĞºĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ğ½!",
                                              "success",
                                            );
                                          }}
                                          className="mt-2 w-full py-1 text-center bg-indigo-600 hover:bg-indigo-500 text-white rounded text-[9px] font-bold uppercase transition"
                                        >
                                          ĞšĞ¾Ğ¿Ğ¸Ñ€Ğ¾Ğ²Ğ°Ñ‚ÑŒ Ğ¿Ñ€Ğ¾Ğ¼Ğ¿Ñ‚ ğŸ“‹
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
                                        "Ğ¡Ñ†ĞµĞ½Ğ° Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ° Ğ·Ğ°Ğ¿ÑƒÑ‰ĞµĞ½Ğ° Ğ·Ğ° ĞºĞ»Ğ°ÑÑ: ĞœĞ°Ğ³",
                                        "info",
                                      );
                                    }}
                                    className="w-full py-3 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:from-purple-500 hover:to-indigo-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest transition-all shadow-lg"
                                  >
                                    Ğ’Ñ‹Ğ±Ñ€Ğ°Ñ‚ÑŒ Ğ¸ ĞĞ°Ñ‡Ğ°Ñ‚ÑŒ Ğ”Ğ¸Ğ°Ğ»Ğ¾Ğ³ ğŸ”®
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
                                    Ğ¡Ñ†ĞµĞ½Ğ° Ğ´Ğ¸Ğ°Ğ»Ğ¾Ğ³Ğ°: ĞÑĞ»Ğ¸ÑÑĞ° ğŸ§â€â™€ï¸ &{" "}
                                    {simDialogueHero === "warrior"
                                      ? "Ğ’Ğ¾Ğ¸Ğ½ âš”ï¸"
                                      : simDialogueHero === "archer"
                                        ? "Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº ğŸ¹"
                                        : "ĞœĞ°Ğ³ ğŸ”®"}
                                  </span>
                                </div>
                                <span className="text-[10px] text-slate-500 font-bold uppercase font-sans">
                                  Ğ¢ĞµĞºÑƒÑ‰Ğ¸Ğ¹ ÑˆĞ°Ğ³ ÑÑ†ĞµĞ½Ğ°Ñ€Ğ¸Ñ: #{simDialogueStep} â€¢{" "}
                                  {simDialogueStep === 0
                                    ? "ĞŸÑ€Ğ¸Ğ²ĞµÑ‚ÑÑ‚Ğ²Ğ¸Ğµ"
                                    : simDialogueStep === 1
                                      ? "Ğ˜Ğ½Ñ„Ğ¾Ñ€Ğ¼Ğ°Ñ†Ğ¸Ñ"
                                      : simDialogueStep === 2
                                        ? "Ğ­Ğ½ĞµÑ€Ğ³ĞµÑ‚Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ Ğ·Ğ°Ğ¿Ğ°Ğ»"
                                        : simDialogueStep === 3
                                          ? "Ğ’Ñ‹Ğ±Ğ¾Ñ€ Ğ¾Ñ‡Ğ¸Ñ‰ĞµĞ½Ğ¸Ñ ĞºĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°"
                                          : "Ğ¤Ğ¸Ğ½Ğ°Ğ» Ğ¸ Ğ·Ğ°Ñ‡Ğ¸ÑÑ‚ĞºĞ°"}
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
                                            ğŸ›¡ï¸{" "}
                                            {simDialogueLang === "RU"
                                              ? "ĞœĞ•ĞĞ® ĞŸĞ•Ğ Ğ¡ĞĞĞĞ–Ğ"
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
                                          âœ•
                                        </button>
                                      </div>

                                      {/* Hero Synergy and Class Toggler Badges inside HUD */}
                                      <div className="bg-slate-900/60 p-3 rounded-2xl border border-white/5 mb-4 space-y-2">
                                        <div className="flex justify-between items-center">
                                          <span className="text-[8px] font-bold text-slate-400 uppercase tracking-wider">
                                            {simDialogueLang === "RU"
                                              ? "Ğ¢Ğ¸Ğ¿ Ğ¡Ğ¸Ğ½ĞµÑ€Ğ³Ğ¸Ğ¸:"
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
                                                ? "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹"
                                                : "Simple"}
                                            </button>
                                            <button
                                              onClick={() =>
                                                setSynergyHeroType("main")
                                              }
                                              className={`px-2 py-0.5 rounded text-[8px] font-bold uppercase transition-all ${synergyHeroType === "main" ? "bg-amber-500 text-black font-black shadow-[0_0_10px_rgba(245,158,11,0.4)]" : "bg-slate-800 text-slate-400 hover:bg-slate-700"}`}
                                            >
                                              {simDialogueLang === "RU"
                                                ? "ĞÑĞ½Ğ¾Ğ²Ğ½Ğ¾Ğ¹"
                                                : "Main"}
                                            </button>
                                          </div>
                                        </div>

                                        <div className="flex justify-between items-center">
                                          <span className="text-[8px] font-bold text-slate-400 uppercase tracking-wider">
                                            {simDialogueLang === "RU"
                                              ? "Ğ¡Ğ¼ĞµĞ½Ğ¸Ñ‚ÑŒ ĞšĞ»Ğ°ÑÑ:"
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
                                                    ? "âš”ï¸ Ğ’Ğ¾Ğ¸Ğ½"
                                                    : "âš”ï¸ Warrior"
                                                  : cls === "archer"
                                                    ? simDialogueLang === "RU"
                                                      ? "ğŸ¹ Ğ›ÑƒÑ‡Ğ½Ğ¸Ğº"
                                                      : "ğŸ¹ Archer"
                                                    : simDialogueLang === "RU"
                                                      ? "ğŸ§™â€â™‚ï¸ ĞœĞ°Ğ³"
                                                      : "ğŸ§™â€â™‚ï¸ Mage"}
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
                                              ? "Ğ¡ĞĞĞ Ğ¯Ğ–Ğ•ĞĞ˜Ğ•"
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
                                            ğŸš€{" "}
                                            {simDialogueLang === "RU"
                                              ? "ĞŸĞ•Ğ Ğ•ĞĞ•Ğ¡Ğ¢Ğ˜ ĞĞ Ğ¡Ğ¦Ğ•ĞĞ£ (C#)"
                                              : "TRANSFER TO SCENE"}
                                          </button>
                                        </div>

                                        {/* Right Slot Box: Characteristics with level points distribution (v18.11.15) */}
                                        {(() => {
                                          const getStartingPoints = () => {
                                            const diff = String(
                                              selectedDifficulty || "Ğ¡Ñ€ĞµĞ´Ğ½Ğ¸Ğ¹",
                                            ).toLowerCase();
                                            if (
                                              diff.includes("Ğ»ĞµĞ³Ğº") ||
                                              diff.includes("easy")
                                            )
                                              return 45;
                                            if (
                                              diff.includes("ÑĞ»Ğ¾Ğ¶Ğ½") ||
                                              diff.includes("hard")
                                            )
                                              return 15;
                                            if (
                                              diff.includes("ĞºĞ¾ÑˆĞ¼") ||
                                              diff.includes("nightmare")
                                            )
                                              return 0;
                                            return 30; // 'Ğ¡Ñ€ĞµĞ´Ğ½Ğ¸Ğ¹' / 'Medium'
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
                                                ? "Ğ¥Ğ°Ñ€Ğ°ĞºÑ‚ĞµÑ€Ğ¸ÑÑ‚Ğ¸ĞºĞ¸ ÑƒÑĞ¿ĞµÑˆĞ½Ğ¾ ÑĞ±Ñ€Ğ¾ÑˆĞµĞ½Ñ‹!"
                                                : "Weapon points reset successfully!",
                                              "info",
                                            );
                                          };

                                          return (
                                            <div className="col-span-6 bg-slate-900/40 border border-white/5 p-3 rounded-2xl flex flex-col justify-between">
                                              <div>
                                                <span className="text-[7.5px] font-black text-slate-500 uppercase block tracking-wider text-center mb-1.5">
                                                  {simDialogueLang === "RU"
                                                    ? "Ğ¥ĞĞ ĞĞšĞ¢Ğ•Ğ Ğ˜Ğ¡Ğ¢Ğ˜ĞšĞ˜"
                                                    : "CHARACTERISTICS"}
                                                </span>

                                                <div className="space-y-1 text-[8px]">
                                                  <div className="flex justify-between border-b border-white/5 pb-0.5">
                                                    <span className="text-slate-400">
                                                      {simDialogueLang === "RU"
                                                        ? "Ğ£Ñ€Ğ¾Ğ²ĞµĞ½ÑŒ:"
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
                                                        ? "Ğ¡Ğ²Ğ¾Ğ±Ğ¾Ğ´Ğ½Ñ‹Ğµ ĞÑ‡ĞºĞ¸:"
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
                                                      âš”ï¸{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "ĞœĞµÑ‡"
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
                                                      ğŸ¹{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "Ğ›ÑƒĞº"
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
                                                      ğŸ”®{" "}
                                                      {simDialogueLang === "RU"
                                                        ? "ĞŸĞ¾ÑĞ¾Ñ…"
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
                                                        ? "Ğ‘Ğ°Ğ·. (Ğ¡Ğ»Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚ÑŒ):"
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
                                                        ? "Ğ—Ğ° Ğ£Ñ€Ğ¾Ğ²Ğ½Ğ¸ (+5/ÑƒÑ€):"
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
                                                  ğŸ”„{" "}
                                                  {simDialogueLang === "RU"
                                                    ? "Ğ¡Ğ‘Ğ ĞĞ¡Ğ˜Ğ¢Ğ¬ ĞĞ§ĞšĞ˜"
                                                    : "RESET STATS"}
                                                </button>
                                              </div>

                                              <div className="bg-slate-950/70 p-1.5 rounded-xl text-[6px] text-slate-500 leading-normal border border-white/5 mt-2">
                                                {simDialogueLang === "RU"
                                                  ? "* ĞĞ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ Ğ¡Ğ‘Ğ ĞĞ¡ Ğ´Ğ»Ñ Ğ¾Ğ±Ğ½ÑƒĞ»ĞµĞ½Ğ¸Ñ. ĞÑ‡ĞºĞ¸ Ğ¼ĞµĞ½ÑÑÑ‚ÑÑ Ñ‚Ğ°ĞºĞ¶Ğµ Ğ² Ğ·Ğ°Ğ²Ğ¸ÑĞ¸Ğ¼Ğ¾ÑÑ‚Ğ¸ Ğ¾Ñ‚ Ğ²Ğ°ÑˆĞµĞ¹ ÑĞ»Ğ¾Ğ¶Ğ½Ğ¾ÑÑ‚Ğ¸."
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
                                            âš¡{" "}
                                            {simDialogueLang === "RU"
                                              ? "ĞĞĞ’Ğ«ĞšĞ˜ Ğ˜ Ğ£ĞœĞ•ĞĞ˜Ğ¯ ĞšĞ›ĞĞ¡Ğ¡Ğ"
                                              : "CLASS SKILLS & SPELLS"}
                                          </span>
                                          <span className="px-1.5 py-0.5 rounded bg-cyan-950 text-cyan-400 font-mono text-[6px] uppercase">
                                            {synergyHeroType === "main"
                                              ? simDialogueLang === "RU"
                                                ? "Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğ¹"
                                                : "Main Hero"
                                              : simDialogueLang === "RU"
                                                ? "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹"
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
                                                      name: "ğŸŒ‹ Ğ¡Ğ¾ĞºÑ€ÑƒÑˆĞ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğ¹ Ğ£Ğ´Ğ°Ñ€ Ğ—ĞµĞ¼Ğ»Ğ¸",
                                                      cost: 50,
                                                      desc: "Ğ’Ñ‹Ğ·Ñ‹Ğ²Ğ°ĞµÑ‚ Ğ¿Ğ¾Ğ´Ğ·ĞµĞ¼Ğ½ÑƒÑ Ğ²Ğ¾Ğ»Ğ½Ñƒ, Ğ½Ğ°Ğ½Ğ¾ÑÑÑ‰ÑƒÑ 250 ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¸ Ğ¾Ğ³Ğ»ÑƒÑˆĞ°ÑÑ‰ÑƒÑ Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ²Ğ½Ğ¸ĞºĞ° Ğ½Ğ° 2 ÑĞµĞºÑƒĞ½Ğ´Ñ‹ Ğ²Ğ¾ Ğ²ÑĞµĞ¹ Ğ·Ğ¾Ğ½Ğµ.",
                                                      command:
                                                        "CAST_SPELL ID=Slam_Seismic -Owner=Player -Dmg=250 -Stun=2s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸŒ‹ Ğ¡Ğ¾ĞºÑ€ÑƒÑˆĞ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğ¹ Ğ£Ğ´Ğ°Ñ€
                                                      Ğ—ĞµĞ¼Ğ»Ğ¸
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Seismic Slam (Seismic
                                                      Wave, stun)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    50 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ‘¹ Ğ¯Ñ€Ğ¾ÑÑ‚ÑŒ Ğ’ĞµĞ»Ğ¸ĞºĞ°Ğ½Ğ°",
                                                      cost: 70,
                                                      desc: "Ğ’Ğ²Ğ¾Ğ´Ğ¸Ñ‚ Ğ³ĞµÑ€Ğ¾Ñ Ğ² Ğ½ĞµĞ¸ÑÑ‚Ğ¾Ğ²ÑÑ‚Ğ²Ğ¾. ĞŸĞ¾ĞºĞ°Ğ·Ğ°Ñ‚ĞµĞ»ÑŒ ÑĞ¸Ğ»Ñ‹ ÑƒĞ²ĞµĞ»Ğ¸Ñ‡Ğ¸Ğ²Ğ°ĞµÑ‚ÑÑ Ğ½Ğ° 100%, ÑĞºĞ¾Ñ€Ğ¾ÑÑ‚ÑŒ Ğ°Ñ‚Ğ°ĞºĞ¸ ÑƒĞ´Ğ²Ğ°Ğ¸Ğ²Ğ°ĞµÑ‚ÑÑ Ğ½Ğ° 8 ÑĞµĞºÑƒĞ½Ğ´.",
                                                      command:
                                                        "CAST_SPELL ID=Giant_Rage -Owner=Player -StatMult=Strength,2.0 -Dur=8s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ‘¹ Ğ¯Ñ€Ğ¾ÑÑ‚ÑŒ Ğ’ĞµĞ»Ğ¸ĞºĞ°Ğ½Ğ°
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Giant\'s Reckless Rage
                                                      (Strength +100%)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    70 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ›¡ï¸ Ğ©Ğ¸Ñ‚ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹",
                                                      cost: 60,
                                                      desc: "ĞĞ°ĞºĞ»Ğ°Ğ´Ñ‹Ğ²Ğ°ĞµÑ‚ Ğ½Ğ° Ğ²ÑÑ Ğ°Ñ€Ğ¼Ğ¸Ñ Ğ±Ğ°Ñ€ÑŒĞµÑ€ Ğ¿Ğ¾Ğ³Ğ»Ğ¾Ñ‰ĞµĞ½Ğ¸Ñ ÑƒÑ€Ğ¾Ğ½Ğ° (400 ĞµĞ´Ğ¸Ğ½Ğ¸Ñ† Ğ¿Ñ€Ğ¾Ñ‡Ğ½Ğ¾ÑÑ‚Ğ¸) Ğ¿Ğ¾Ğ´ ÑĞ²ĞµÑ‡ĞµĞ½Ğ¸ĞµĞ¼ Bloom.",
                                                      command:
                                                        "CAST_SPELL ID=Fate_Shield -Owner=Player -ShieldDurability=400 -Color=Cyan",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ›¡ï¸ Ğ©Ğ¸Ñ‚ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Sovereign Fate Wall
                                                      (Absorption Shield)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    60 MP ğŸ§ª
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ”¨ Ğ¢ÑĞ¶ĞµĞ»Ñ‹Ğ¹ Ğ£Ğ´Ğ°Ñ€",
                                                      cost: 30,
                                                      desc: "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹ Ğ½Ğ°Ğ¿Ñ€Ğ°Ğ²Ğ»ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ²Ñ‹Ğ¿Ğ°Ğ´ Ğ¼ĞµÑ‡Ğ¾Ğ¼. Ğ£Ñ€Ğ¾Ğ½ 120, ÑĞ±Ñ€Ğ°ÑÑ‹Ğ²Ğ°ĞµÑ‚ ĞºĞ°ÑÑ‚ Ğ²Ñ€Ğ°Ğ³Ñƒ.",
                                                      command:
                                                        "CAST_SPELL ID=Heavy_Strike -Owner=Player -Dmg=120",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ”¨ Ğ¢ÑĞ¶ĞµĞ»Ñ‹Ğ¹ Ğ£Ğ´Ğ°Ñ€
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Basic Heavy Sword Strike
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    30 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ›¡ï¸ Ğ‘Ğ»Ğ¾Ğº Ğ©Ğ¸Ñ‚Ğ¾Ğ¼",
                                                      cost: 25,
                                                      desc: "Ğ‘Ğ»Ğ¾ĞºĞ¸Ñ€ÑƒĞµÑ‚ 50% Ğ²Ñ…Ğ¾Ğ´ÑÑ‰ĞµĞ³Ğ¾ Ñ„Ğ¸Ğ·Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ° Ğ² Ñ‚ĞµÑ‡ĞµĞ½Ğ¸Ğµ 5 ÑĞµĞºÑƒĞ½Ğ´.",
                                                      command:
                                                        "CAST_SPELL ID=Basic_Block -Owner=Player -BlockReduction=50% -Dur=5s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ›¡ï¸ Ğ‘Ğ»Ğ¾Ğº Ğ©Ğ¸Ñ‚Ğ¾Ğ¼
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Shield Block
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    25 MP ğŸ§ª
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
                                                      name: "ğŸ¹ Ğ“Ñ€Ğ°Ğ´ Ğ¡Ñ‚Ñ€ĞµĞ» Ğ—ĞµĞ½Ğ¸Ñ‚",
                                                      cost: 55,
                                                      desc: "ĞŸÑ€Ğ¸Ğ·Ñ‹Ğ²Ğ°ĞµÑ‚ ÑĞ¾ĞºÑ€ÑƒÑˆĞ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğ¹ Ğ¾Ğ³Ğ½ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ»Ğ¸Ğ²ĞµĞ½ÑŒ ÑÑ‚Ñ€ĞµĞ» Ğ½Ğ° Ğ¾Ñ‚Ñ€ÑĞ´ Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ²Ğ½Ğ¸ĞºĞ°. Ğ£Ñ€Ğ¾Ğ½ 300 Ğ² ÑĞµĞºÑƒĞ½Ğ´Ñƒ Ğ² Ñ‚ĞµÑ‡ĞµĞ½Ğ¸Ğµ 3 ÑĞµĞº.",
                                                      command:
                                                        "CAST_SPELL ID=Zenith_Rain -Owner=Player -DPS=300 -Dur=3s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ¹ Ğ“Ñ€Ğ°Ğ´ Ğ¡Ñ‚Ñ€ĞµĞ» Ğ—ĞµĞ½Ğ¸Ñ‚
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Zenith Rain of Star-Fire
                                                      (AoE DPS)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    55 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ¯ Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ» Ğ¢Ğ¾Ñ‡Ğ½Ğ¾ÑÑ‚Ğ¸",
                                                      cost: 45,
                                                      desc: "Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ» Ğ¿Ğ¾ ÑƒÑĞ·Ğ²Ğ¸Ğ¼Ğ¾Ğ¼Ñƒ Ğ¼ĞµÑÑ‚Ñƒ. ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 450 ĞºÑ€Ğ¸Ñ‚Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ³Ğ¾ Ñ‡Ğ¸ÑÑ‚Ğ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ°, Ğ¸Ğ³Ğ½Ğ¾Ñ€Ğ¸Ñ€ÑƒÑ Ğ±Ñ€Ğ¾Ğ½Ñ Ğ²Ñ€Ğ°Ğ¶ĞµÑĞºĞ¾Ğ³Ğ¾ Ğ»Ğ¾Ñ€Ğ´Ğ°.",
                                                      command:
                                                        "CAST_SPELL ID=Deadly_Shot -Owner=Player -TrueDmg=450",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ¯ Ğ’Ñ‹ÑÑ‚Ñ€ĞµĞ» Ğ¢Ğ¾Ñ‡Ğ½Ğ¾ÑÑ‚Ğ¸
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Deadly Precision Bolt
                                                      (Armor Ignore)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    45 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ’¨ Ğ”Ñ‹Ğ¼Ğ¾Ğ²Ğ°Ñ Ğ—Ğ°Ğ²ĞµÑĞ°",
                                                      cost: 40,
                                                      desc: "Ğ¡Ğ¾Ğ·Ğ´Ğ°ĞµÑ‚ Ğ¾Ğ±Ğ»Ğ°ĞºĞ¾ Ğ´Ñ‹Ğ¼Ğ°, Ğ´Ğ°ÑÑ‰ĞµĞµ 100% ÑƒĞºĞ»Ğ¾Ğ½ĞµĞ½Ğ¸Ñ Ğ¸ ÑĞºÑ€Ñ‹Ğ²Ğ°ÑÑ‰ĞµĞµ Ğ·Ğ°Ğ¼Ğ¾Ğº/Ğ³ĞµÑ€Ğ¾Ñ Ğ½Ğ° 10 ÑĞµĞºÑƒĞ½Ğ´ Ğ½Ğ° ÑÑ‚Ñ€Ğ°Ñ‚ĞµĞ³Ğ¸Ñ‡ĞµÑĞºĞ¾Ğ¹ ĞºĞ°Ñ€Ñ‚Ğµ.",
                                                      command:
                                                        "CAST_SPELL ID=Smoke_Escape -Owner=Player -Evasion=100% -Dur=10s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ’¨ Ğ”Ñ‹Ğ¼Ğ¾Ğ²Ğ°Ñ Ğ—Ğ°Ğ²ĞµÑĞ°
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Tactical Smokescreen
                                                      Escape
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    40 MP ğŸ§ª
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "âš¡ Ğ‘Ñ‹ÑÑ‚Ñ€Ğ°Ñ Ğ¡Ñ‚Ñ€ĞµĞ»Ğ°",
                                                      cost: 20,
                                                      desc: "ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ Ğ±Ñ‹ÑÑ‚Ñ€Ñ‹Ğ¹ Ñ‚Ğ¾Ñ‡ĞµÑ‡Ğ½Ñ‹Ğ¹ ÑƒÑ€Ğ¾Ğ½ Ğ² 80 ĞµĞ´Ğ¸Ğ½Ğ¸Ñ† Ñ Ğ±Ñ‹ÑÑ‚Ñ€Ğ¾Ğ¹ Ğ¿ĞµÑ€ĞµĞ·Ğ°Ñ€ÑĞ´ĞºĞ¾Ğ¹.",
                                                      command:
                                                        "CAST_SPELL ID=Quick_Arrow -Owner=Player -Dmg=80",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      âš¡ Ğ‘Ñ‹ÑÑ‚Ñ€Ğ°Ñ Ğ¡Ñ‚Ñ€ĞµĞ»Ğ°
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Basic Quick Arrow (Low
                                                      cooldown)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    20 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "âš™ï¸ Ğ¡Ñ‚Ğ°Ğ»ÑŒĞ½Ğ¾Ğ¹ ĞšĞ°Ğ¿ĞºĞ°Ğ½",
                                                      cost: 35,
                                                      desc: "ĞÑÑ‚Ğ°Ğ½Ğ°Ğ²Ğ»Ğ¸Ğ²Ğ°ĞµÑ‚ Ğ´Ğ²Ğ¸Ğ¶ĞµĞ½Ğ¸Ğµ Ğ·Ğ°Ğ´ĞµÑ‚Ğ¾Ğ¹ Ñ†ĞµĞ»Ğ¸ Ğ½Ğ° 4 ÑĞµĞºÑƒĞ½Ğ´Ñ‹.",
                                                      command:
                                                        "CAST_SPELL ID=Basic_Snare -Owner=Player -Root=4s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      âš™ï¸ Ğ¡Ñ‚Ğ°Ğ»ÑŒĞ½Ğ¾Ğ¹ ĞšĞ°Ğ¿ĞºĞ°Ğ½
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Mechanical Snare
                                                      Trap
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    35 MP ğŸ§ª
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
                                                      name: "â˜„ï¸ ĞœĞµÑ‚ĞµĞ¾Ñ€Ğ¸Ñ‚Ğ½Ñ‹Ğ¹ Ğ¡Ğ¿ĞµĞºÑ‚Ñ€",
                                                      cost: 80,
                                                      desc: "ĞĞ±Ñ€ÑƒÑˆĞ¸Ğ²Ğ°ĞµÑ‚ ĞºĞ¾ÑĞ¼Ğ¸Ñ‡ĞµÑĞºÑƒÑ Ğ²Ğ¾Ğ»Ğ½Ñƒ Ğ¼ĞµÑ‚ĞµĞ¾Ñ€Ğ¸Ñ‚Ğ¾Ğ² Ğ½Ğ° ÑƒĞºĞ°Ğ·Ğ°Ğ½Ğ½ÑƒÑ ĞºÑ€ĞµĞ¿Ğ¾ÑÑ‚ÑŒ. ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 600 ÑƒÑ€Ğ¾Ğ½Ğ° Ğ±Ğ¾ÑÑĞ°Ğ¼ Ğ¸ Ñ€ÑƒÑˆĞ¸Ñ‚ Ğ¾Ğ±Ğ¾Ñ€Ğ¾Ğ½Ğ¸Ñ‚ĞµĞ»ÑŒĞ½Ñ‹Ğµ ÑÑ‚ĞµĞ½Ñ‹.",
                                                      command:
                                                        "CAST_SPELL ID=Meteor_Spectrum -Owner=Player -Dmg=600 -SiegeDmg=150",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      â˜„ï¸ ĞœĞµÑ‚ĞµĞ¾Ñ€Ğ¸Ñ‚Ğ½Ñ‹Ğ¹ Ğ¡Ğ¿ĞµĞºÑ‚Ñ€
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Meteor Spectrum Cosmic
                                                      Blast (Siege, AoE)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    80 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ”® Ğ­Ğ½ĞµÑ€Ğ³ĞµÑ‚Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ Ğ‘Ğ°Ñ€ÑŒĞµÑ€",
                                                      cost: 60,
                                                      desc: "Ğ“ĞµĞ½ĞµÑ€Ğ¸Ñ€ÑƒĞµÑ‚ Ğ²Ğ¾ĞºÑ€ÑƒĞ³ ĞšÑ€Ğ¸ÑÑ‚Ğ°Ğ»Ğ»Ğ° Ğ¼Ğ°Ğ½Ğ°-ĞºÑƒĞ¿Ğ¾Ğ» Ğ½Ğ° 12 ÑĞµĞºÑƒĞ½Ğ´. ĞÑ‚Ñ€Ğ°Ğ¶Ğ°ĞµÑ‚ Ğ»ÑĞ±Ñ‹Ğµ ÑĞ½Ğ°Ñ€ÑĞ´Ñ‹ Ğ¿Ñ€Ğ¾Ñ‚Ğ¸Ğ²Ğ½Ğ¸ĞºĞ° Ğ½Ğ°Ğ·Ğ°Ğ´ Ğ² ÑÑ‚Ñ€ĞµĞ»ĞºĞ°.",
                                                      command:
                                                        "CAST_SPELL ID=Mana_Barrier -Owner=Player -ReflectProjectiles=True -Dur=12s",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ”® Ğ­Ğ½ĞµÑ€Ğ³ĞµÑ‚Ğ¸Ñ‡ĞµÑĞºĞ¸Ğ¹ Ğ‘Ğ°Ñ€ÑŒĞµÑ€
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Mana Mirror Aegis (Deflect
                                                      shield)
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    60 MP ğŸ§ª
                                                  </span>
                                                </div>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸŒ€ ĞĞ±ÑĞ¾Ğ»ÑÑ‚Ğ½Ñ‹Ğ¹ Ğ¢ĞµĞ»ĞµĞ¿Ğ¾Ñ€Ñ‚",
                                                      cost: 50,
                                                      desc: "Ğ“ĞµÑ€Ğ¾Ğ¹ Ğ¼Ğ³Ğ½Ğ¾Ğ²ĞµĞ½Ğ½Ğ¾ Ñ‚ĞµĞ»ĞµĞ¿Ğ¾Ñ€Ñ‚Ğ¸Ñ€ÑƒĞµÑ‚ÑÑ Ğ² Ğ»ÑĞ±ÑƒÑ Ñ‚Ğ¾Ñ‡ĞºÑƒ Ğ½Ğ° ĞºĞ°Ñ€Ñ‚Ğµ ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ°, ÑƒĞºĞ»Ğ¾Ğ½ÑÑÑÑŒ Ğ¾Ñ‚ Ğ²ÑĞµÑ… Ğ·Ğ°ÑĞ°Ğ´.",
                                                      command:
                                                        "CAST_SPELL ID=Absolute_Teleport -Owner=Player -Target=SelectedZone",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸŒ€ ĞĞ±ÑĞ¾Ğ»ÑÑ‚Ğ½Ñ‹Ğ¹ Ğ¢ĞµĞ»ĞµĞ¿Ğ¾Ñ€Ñ‚
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Zenith Absolute Blink
                                                      Portal
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    50 MP ğŸ§ª
                                                  </span>
                                                </div>
                                              </>
                                            ) : (
                                              <>
                                                <div
                                                  onClick={() =>
                                                    setActiveSpellPrompt({
                                                      name: "ğŸ”¥ ĞĞ³Ğ½ĞµĞ½Ğ½Ğ°Ñ Ğ’ÑĞ¿Ñ‹ÑˆĞºĞ°",
                                                      cost: 25,
                                                      desc: "Ğ›ĞµĞ³ĞºĞ¸Ğ¹ Ğ¾Ğ³Ğ½ĞµĞ½Ğ½Ñ‹Ğ¹ Ğ·Ğ°Ñ€ÑĞ´. ĞĞ°Ğ½Ğ¾ÑĞ¸Ñ‚ 100 Ğ²Ğ·Ñ€Ñ‹Ğ²Ğ½Ğ¾Ğ³Ğ¾ ÑƒÑ€Ğ¾Ğ½Ğ° Ğ¿Ğ¾ Ğ¾Ğ´Ğ¸Ğ½Ğ¾Ñ‡Ğ½Ğ¾Ğ¹ Ñ†ĞµĞ»Ğ¸.",
                                                      command:
                                                        "CAST_SPELL ID=Spark_Shot -Owner=Player -Dmg=100",
                                                    })
                                                  }
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ”¥ ĞĞ³Ğ½ĞµĞ½Ğ½Ğ°Ñ Ğ’ÑĞ¿Ñ‹ÑˆĞºĞ°
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Simple Fireball Spark
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-purple-950 text-purple-300 font-mono text-[7px] font-bold">
                                                    25 MP ğŸ§ª
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
                                                        "âœ¨ Ğ—Ğ°Ğ»Ğ¸Ñ‚Ğ¾ +100 ĞœĞ°Ğ½Ñ‹ Ğ·Ğ° 50 Ğ·Ğ¾Ğ»Ğ¾Ñ‚Ğ°!",
                                                        "success",
                                                      );
                                                    } else {
                                                      showNotification(
                                                        "âŒ ĞĞµĞ´Ğ¾ÑÑ‚Ğ°Ñ‚Ğ¾Ñ‡Ğ½Ğ¾ Ğ·Ğ¾Ğ»Ğ¾Ñ‚Ğ° Ğ´Ğ»Ñ Ğ·Ğ°ĞºÑƒĞ¿ĞºĞ¸ Ğ·ĞµĞ»ÑŒÑ Ğ¼Ğ°Ğ½Ñ‹ (50)!",
                                                        "error",
                                                      );
                                                    }
                                                  }}
                                                  className="bg-slate-950 hover:bg-slate-900 border border-white/5 hover:border-cyan-400/20 p-2 rounded-xl transition cursor-pointer flex justify-between items-center group"
                                                >
                                                  <div>
                                                    <div className="text-[9px] font-bold text-slate-200 group-hover:text-white">
                                                      ğŸ§ª Ğ—Ğ°Ğ»Ğ¸Ğ²ĞºĞ° ĞœĞ°Ğ½Ñ‹ (+100 Ğ·Ğ°
                                                      50ğŸ’°)
                                                    </div>
                                                    <div className="text-[6.5px] text-slate-500">
                                                      Consumes gold to restore
                                                      chemical mana
                                                    </div>
                                                  </div>
                                                  <span className="px-1.5 py-0.5 rounded bg-amber-950 text-amber-300 font-mono text-[7px] font-bold">
                                                    50 Gold ğŸ’°
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
                                          ? "Fate Continent â€¢ Ğ’ĞµÑ€ÑĞ¸Ñ 18.12.44"
                                          : "Fate Continent â€¢ Lvl 18.12.44"}
                                      </span>
                                      <span className="text-[6.5px] text-slate-600 block leading-tight">
                                        {simDialogueLang === "RU"
                                          ? "Ğ˜Ğ½Ñ‚ĞµĞ³Ñ€Ğ°Ñ†Ğ¸Ñ Ñ Ğ¡#-ÑĞºÑ€Ğ¸Ğ¿Ñ‚Ğ¾Ğ¼ SaveGameSystem.cs Ğ¸ SettingsManager.cs Ğ·Ğ°Ğ²ĞµÑ€ÑˆĞµĞ½Ğ°."
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
                                            âš¡
                                          </div>
                                          <h4 className="text-xs font-black text-cyan-400 uppercase tracking-widest">
                                            Ğ¡Ğ˜ĞœĞ£Ğ›Ğ¯Ğ¦Ğ˜Ğ¯ Ğ—ĞĞšĞ›Ğ˜ĞĞĞĞ˜Ğ¯ (C# CAST)
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
                                              ? "ĞÑ‚Ğ¼ĞµĞ½Ğ°"
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
                                                  `âœ¨ Ğ£ÑĞ¿ĞµÑˆĞ½Ğ¾ ĞºĞ°ÑÑ‚Ğ¾Ğ²Ğ°Ğ½Ğ¾ ${activeSpellPrompt.name}! ĞŸĞµÑ€ĞµĞ´Ğ°Ğ½Ğ¾ Ğ² C# ÑÑ†ĞµĞ½Ñƒ.`,
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
                                                  "âŒ ĞĞµĞ´Ğ¾ÑÑ‚Ğ°Ñ‚Ğ¾Ñ‡Ğ½Ğ¾ Ğ¼Ğ°Ğ½Ñ‹! Ğ˜ÑĞ¿Ğ¾Ğ»ÑŒĞ·ÑƒĞ¹Ñ‚Ğµ Ğ·ĞµĞ»ÑŒĞµ Ğ¸Ğ»Ğ¸ Ğ²Ğ¾ÑÑÑ‚Ğ°Ğ½Ğ¾Ğ²Ğ¸Ñ‚Ğµ ĞµÑ‘.",
                                                  "error",
                                                );
                                              }
                                              setActiveSpellPrompt(null);
                                            }}
                                            className="py-2.5 bg-gradient-to-r from-cyan-600 to-indigo-600 hover:from-cyan-500 hover:to-indigo-500 text-white rounded-xl transition"
                                          >
                                            {simDialogueLang === "RU"
                                              ? "ĞšĞ°ÑÑ‚Ğ¾Ğ²Ğ°Ñ‚ÑŒ ğŸ”®"
                                              : "Cast Spell ğŸ”®"}
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
                                            ğŸš€
                                          </div>
                                          <h4 className="text-xs font-black text-amber-400 uppercase tracking-widest">
                                            Ğ’Ğ«Ğ¡ĞĞ”ĞšĞ ĞĞ Ğ¡Ğ¦Ğ•ĞĞ£ (SPAWN HERO)
                                          </h4>
                                          <span className="text-[7px] font-mono text-slate-500">
                                            C# PROCEDURAL TRANSMISSION AGENT
                                          </span>
                                        </div>

                                        <div className="bg-slate-950 p-4 rounded-xl border border-white/5 text-[9.5px] text-slate-300 space-y-1.5 leading-normal">
                                          <p className="text-white font-bold text-center mb-1">
                                            {simDialogueLang === "RU"
                                              ? "ĞŸĞµÑ€ĞµĞ½ĞµÑÑ‚Ğ¸ Ğ³ĞµÑ€Ğ¾Ñ Ğ¸ Ğ°Ñ€Ñ‚ĞµÑ„Ğ°ĞºÑ‚Ñ‹ Ğ½Ğ° 3D ÑÑ†ĞµĞ½Ñƒ?"
                                              : "Deploy commander and equipped items to active 3D Continent?"}
                                          </p>
                                          <div>
                                            ğŸ‘‰{" "}
                                            <span className="text-cyan-400 font-bold">
                                              ĞŸĞµÑ€ÑĞ¾Ğ½Ğ°Ğ¶
                                            </span>
                                            :{" "}
                                            {simDialogueHero === "warrior"
                                              ? "Ğ ÑĞ³Ğ½Ğ°Ñ€ (Ğ’Ğ¾Ğ¸Ğ½)"
                                              : simDialogueHero === "archer"
                                                ? "ĞĞ»Ğ°Ñ€Ğ¸Ğº (Ğ¡Ñ‚Ñ€ĞµĞ»Ğ¾Ğº)"
                                                : "Ğ­Ğ»Ğ¸Ğ·Ğ¸ÑƒÑ (ĞœĞ°Ğ³)"}
                                          </div>
                                          <div>
                                            ğŸ‘‰{" "}
                                            <span className="text-cyan-400 font-bold">
                                              Ğ¡Ğ¸Ğ½ĞµÑ€Ğ³Ğ¸Ñ
                                            </span>
                                            :{" "}
                                            {synergyHeroType === "main"
                                              ? "Ğ“Ğ»Ğ°Ğ²Ğ½Ñ‹Ğ¹ (Main)"
                                              : "ĞŸÑ€Ğ¾ÑÑ‚Ğ¾Ğ¹ (Simple)"}
                                          </div>
                                          <div>
                                            ğŸ‘‰{" "}
                                            <span className="text-cyan-400 font-bold font-mono">
                                              Ğ’ĞµÑ‰Ğ¸ Ğ´Ğ»Ñ ÑĞ¿Ğ°Ğ²Ğ½Ğ°
                                            </span>
                                            :{" "}
                                            {Object.values(equippedItems)
                                              .map((x) => x.icon)
                                              .join(" ")}{" "}
                                            ({Object.keys(equippedItems).length}{" "}
                                            ÑˆÑ‚.)
                                          </div>
                                          <div>
                                            ğŸ‘‰{" "}
                                            <span className="text-cyan-400 font-bold">
                                              Ğ¡Ğ²Ğ¸Ñ‚Ğ° Ğ² Ğ¿Ğ¾Ñ…Ğ¾Ğ´Ğµ
                                            </span>
                                            :{" "}
                                            <span className="font-mono text-white font-black">
                                              {recruitedTroops} Ğ²Ğ¾Ğ¸Ğ½Ğ¾Ğ²
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
                                              ? "ĞÑ‚Ğ¼ĞµĞ½Ğ°"
                                              : "Cancel"}
                                          </button>
                                          <button
                                            onClick={() => {
                                              showNotification(
                                                simDialogueLang === "RU"
                                                  ? `ğŸš€ Ğ“ĞµÑ€Ğ¾Ğ¹ ${simDialogueHero === "warrior" ? "Ğ ÑĞ³Ğ½Ğ°Ñ€" : simDialogueHero === "archer" ? "ĞĞ»Ğ°Ñ€Ğ¸Ğº" : "Ğ­Ğ»Ğ¸Ğ·Ğ¸ÑƒÑ"} Ğ¿ĞµÑ€ĞµĞ½ĞµÑĞµĞ½ Ğ½Ğ° 3D Ğ¿Ğ¾Ğ»Ğµ ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚Ğ° Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹!`
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
                                              ? "Ğ’Ñ‹ÑĞ°Ğ´Ğ¸Ñ‚ÑŒ ğŸš€"
                                              : "Deploy Hero ğŸš€"}
                                          </button>
                                        </div>
                                      </div>
                                    </div>
                                  )}
                                  {/* TOP-LEFT FLOATING GLASSMORPHIC STATUS FRAME "Ğ ĞĞœĞšĞ Ğ¨ĞĞŸĞšĞ" */}
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
                                    title="ĞĞ°Ğ¶Ğ¼Ğ¸Ñ‚Ğµ Ğ´Ğ»Ñ Ğ¾Ñ‚ĞºÑ€Ñ‹Ñ‚Ğ¸Ñ Ğ¼ĞµĞ½Ñ ÑĞ½Ğ°Ñ€ÑĞ¶ĞµĞ½Ğ¸Ñ Ğ¸ Ñ…Ğ°Ñ€Ğ°ĞºÑ‚ĞµÑ€Ğ¸ÑÑ‚Ğ¸Ğº"
                                  >
                                    <div className="absolute top-2 right-4 text-[7px] font-black text-amber-400 uppercase tracking-widest animate-pulse">
                                      ĞšĞ»Ğ¸ĞºĞ½Ğ¸Ñ‚Ğµ: ĞœĞµĞ½Ñ ĞŸĞµÑ€ÑĞ¾Ğ½Ğ°Ğ¶Ğ° âš™ï¸
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
                                              ? "Ğ ÑĞ³Ğ½Ğ°Ñ€ (Ğ’Ğ¾Ğ¸Ğ½)"
                                              : "Ragnar (Warrior)"
                                            : simDialogueHero === "archer"
                                              ? simDialogueLang === "RU"
                                                ? "ĞĞ»Ğ°Ñ€Ğ¸Ğº (Ğ¡Ñ‚Ñ€ĞµĞ»Ğ¾Ğº)"
                                                : "Alaric (Archer)"
                                              : simDialogueLang === "RU"
                                                ? "Ğ­Ğ»Ğ¸Ğ·Ğ¸ÑƒÑ (ĞœĞ°Ğ³)"
                                                : "Elysius (Mage)"}
                                        </h5>

                                        <div className="flex items-center gap-1.5 mt-0.5">
                                          <span className="text-[10px] font-black text-amber-400 uppercase tracking-widest bg-amber-500/10 px-1.5 py-0.5 rounded border border-amber-500/20">
                                            {LEVEL_LABELS[simDialogueLang]}{" "}
                                            {simHeroLvl}
                                          </span>
                                          <span className="text-[8.5px] text-slate-400 font-mono">
                                            {simDialogueLang === "RU"
                                              ? "ĞšĞ¾Ğ½Ñ‚Ğ¸Ğ½ĞµĞ½Ñ‚ Ğ¡ÑƒĞ´ÑŒĞ±Ñ‹"
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
                      xœì}ksÛF–è÷û+ÚšlŠJDŠO‰ÒZ“+KŠ­º²ä+ÉÎ«\6DB$Ö À@K«ü˜T2åLœÉxn2É$ÎãîNvwöâX±âgÕşğ/øŒÂ=§ AŠ’ÙĞÃ;±DB@£ûôéó>§ÛŞÖm­rBµÌ7«—Ø Kç’ÉÿÑá®íè ]UŒ_vyÿÑÁ¢v±ë›á^VĞÛU*êX_9fK¥¸­+É%™eÖŒ¢ZŒ/×t™UkY7Wâe­XT¶dZEÕò~ÅWÊš£æX5şNªºz¶¯Û1ˆQt}3k/XË(aü%K)jªáÄ3n±eË¬ÄkŠÏ&“.iFQ+™qXf—•"Lèä¹ä¹T²ºzÎ*-)±Lv J¤3ùd"×–9–bØš£™F\Wk–Â¿d’É>‰‘ÛÎš®­¯K<ÂØŠVtÊ£ìüKë'§œ¨hF,•L°X£<„êg¯0øKÿ¥:? ñ†K—º¾ypĞ»µ«{×_a'Ca§,³d©¶Í){™-TU£È&LÃ±L½2ØİŒÚ‘¿âÄ3¶ZA¯ÅS]ão{7ËººÊş¥f;ÚòZ|IuVTØ*ºêÄß}Á–a˜ñ%])\W9f¦‘$6ŒAÀ¶¾°8¾¸pnfüØÔÌBâäøìø;€=“š¢›¥š:£¥³İc=òFÛ!„@Å4L‰)7)(" ÒĞL„¶b¯IhµfUuÕ'¢UÍ¸°	Mg³©üË§ŸÊ‰
G©Ÿ	Õ
–	 TœšÍ
‚nÚLƒõ)ªìÄéI2-YZ‘áxz„­PRªğHk–Uñ‡‡éNÊwOm—jc]CÛ4&t­pal=ÖÏÆ~ÉdĞÆ±Ö¤îgIÛag•1C]a1©§MØJ‰ñZQ3‘›{`¿ùd'1ÑSl¦kı‰ué‚æû”ê°?ÖÿÏ ˜v€  H,Háœ]Ğt ‰¦Eë¯ÔÒÛqv°d?0¢Ä²¥şª¦…µ„­:g½¦;‹ZE•]¨\:“Hçd6>şšeYÅ—Ê=j¶êjÕ4àµÀğç•JuÑ1÷T2;”;wö*K&Ò{
R¢Xr¡“‰”ä(÷™aƒjÁ‰•¤ä?‰#.ª ±œ%Æ ¬Är(;ÒvÌj¬#rHuv‰§Pf1µŸ­_êŠø±à+R±XÕR/J3
ŸF) ÒNa'0‡L.)m™ÅD¿ó4:¹ag3sQÅt>“’\èªl®Ìš Åh²[zhU1ØØØë›?-#íùí5Ö÷ôÖ‡¿cî·îWî—îÇîŸÜ/Ü¿1÷ÿòŸîwxá¥çQ¯ç™©3S3ìô©#}²û±>»V(€:*ı¨ô¢XªS³ÓâIäº¸ÄTİVåñê!Ã“ÿúÿ‘{¿~­ş»é>t±WaÇ1¸ö¸~½~Õİ ¢ö=¯Tí"À˜÷ù&+4cÙÜ_”Ä+1.	e( :TWãéè¾kñüÔ3ÓYF½y´yõN_c]Õ=ëJ¾ÍºR«VU«  ¶ó¿sM£]à;%ÜWL¼7¶;˜éZAí^aòåŸÖûX_÷j5áŸ=÷æ)²çè Ğ£ºÔ÷UïB6ÔËf¥Ya“bÆÎ™s{”Û,a)`dô³Â›ÿÊÜOİ»õËîFıJı] zwàßmwƒÂ,Áø¹~@G½L­¢Yh t‡@”§
(¹,òJzx5="E}›ªN`Waµ{‚~ß¢áK9
‹@´º<cédR»‡Òòl~ïzÆòáçÏu¶”´Y€ˆ{á­AÙ¸…àù@ª™ :CASÁ3Œ©œdmö™	úS›0‚n†çWøxò—ï˜û.il0÷1ê`(t€rµşAıCR~¹‚*ş¾ßç
E!ïãr`Á¬TuÕQõ5Ğy ,µHIhªzO*‘i=©$©$W~®¥’ÔğPDr‰´	ÿEK’ùç}^d¹DÎ‰F,ÙoãœˆoÉmsâÒpãRÀê&k«]e0Ò¤[{H¹¥îé­¿şùÉåß?ùóÕlİ´Ùm3cI,ék¬ÅâwzaêÜ
'úI=}VâîÑh„L0İO@p¼ïn¡<éIšõkr½€Ø8/DD.:v½dM³hwww Ó}xÎŸeÇ³ãóã§NLO°ñÙI¶85??=?÷›;35fzêfZlb|aqfŠ-LŸ<=3¾8=7ÛeØÎz`¥µÊ²< ¾;&Ó)Ü1b<ê‘.˜:ƒ­X±ı}êÇBz_ƒ[¸Ï7".‡0@èØóH*ÅQü–Î&eâàÚÇ¶O±2şhokúSIŸ„¤;Øş³I1µİf¤ZéUµª¡LĞŞh40ØÖ!çaÄy¥æ8ú¤ÃÆˆ óW#-ìDĞøT²È·øtÓÉöI2à2ÎcÚÖ«“c&Ç3AÆaÁ;4£_ÑP-†vİt7ëW8Éº
ìrÃ(¿ån2÷.|¼7\®¿/\–ÒTíÌøìñÓãó“lrêÔÌÜ['§fÙÄÜÉS3S‹S“œI:†·œİ¶ÀÀÌÛc™·o®‰æ(ºVh.£•Ê°cÈkÃ©®MVra"2€pßË¿o1÷³ÆÒŞ®_Ç¥½U¿ÆM`i·d»õº°´ŠmìÅvT[9:Á)'=ÁHME_üd&æ~Ø~§~ƒ[ƒ~B¿ı–{fN‰:†‚R•òĞî©!Â£p¹ñ9×M@®M@.j|U¿Ö¯“Ã[D¿ãF#ºÙ|M3$‘Kôõt?xÌÜ¯aİ@ZÉ%¾ßá|?á´qí‡äC×o«†æ”Ù‚bœšb­IQÇrV‚6V·“F;¨Ç k«(«ñ$™•Õ¸RsL¦«JI¢¥êÊªZ”#‰“Óã3sÇOO[œŸ]Âß‚œfĞ¶$=ûZb[Ü¬1‚	„™ŸÄüiŒ¨îüUg=\ÑKi$ÇEÑ%¡t3&İP²… Ö¦Pû,¹)…K ^rR#~ácw˜%ú8½»Ãå¤†'AğØ-Ïp½Ğµpşş®ïV –²Y¿Ê ŸËî¸÷Ã#Ğ+ˆ_›,3É¸Œõ<€òÖuèáR<½É?Á«‘ÓÆh	.°½ÇÅ·{îz66ù¸îÁ{>„?lÀÏ«0 Ğ·™û=<„4¿íGèøA‚¹_¯ÈĞ4N}
¹ï]˜|¹ÏŸİÀN•ZcğòMñFœù#>¦ï1ºúİÀIò`À5|?¸CÄ†â¢èğ
N\ô4Ào„Élá |îxxò—Æ2ëê‡¼eÖ@%Z²M½² n®):L(SVAÂY³™V‚i«6¿¶¤8®.kª^<ÂÆAŞÏÄ‹ZE5lÍ4àVÌ/R-›™ËüîYu…gúihhfHJA÷Ò
Š®¯1¥¨T¦ğ²‚Y~yŠ‹šcZÖM¸ÖË{Á…ÁYÃûáA›¡¾ıí¢š0„ÙòMÕyÅRª<c&Û®b¶¨Ÿ+\-’QĞ$ƒ·B…oùîÎ iå{4™ö3u EçE‰Ö—ÍşÌ]qQø"…Cdï#„zq=İ¶çÀ¶oq=‡+Æ¤ØëBTC[âO;¾‚²ámaF| Bİ=w¿ƒÏ?°<J²÷¸p{;hv”àD1nR­‚Ø¦Qh›PlÎ0n'É7$£Ãv#áÙl÷mæÑêœÙ!ï]­¨–¢yÒ\uTE$%´Ü’m:?½ùCMÓh«xl.µ³Ÿ4hî¶Úı¤Á¤ütÎOÊO¤ò¹Tzd ™ÈôŸõÆjƒô¬ÆSÉ®§2îÓëOoİØàY?\CD$÷|„BU»jÑ‡ÌıW‹ÂÇ˜¿;ú¥íìüUÇjš^d¯k%‹æ
@9&¶Aß%9G0æ{Ü”06H&g	ÿyit¿®ê1Ú€à¢ıHxhh‹ÿ»¬”•Aü_ÂÚ/–
à:„É!èåPƒ^îÒfzê°‘ÚÙp%Oööˆ~µÄaÌO-L-†ª#b@©òìûK»Ûoƒ¯°Ù©7‘o-,NdÓ³‹Sóã‹ÓgÁsó‚6Ú,O¥ûÙ+ƒÑ´¤³wû²RTãšá™Cê §kÃ[L*klqJÅBRkèÈõPtîÙ/²)ø·ˆ–­ˆUCí¾ÊÔöI6«ÛÉí xşm´ù$ƒ×ĞTŠ£ü³e®tPğëO	£RK©ÎRA*×/[wh›-«e$üÍRŞm&ñU,³&ê*í"Ÿä3,¼´3\³ˆ´-œµsäC¾aw
†²Wr¶¤›@Şš”NòıŒãïÌÆ.7rÉ €g»c ­Ì6÷¦—ß¿^‚÷ÀùåÔ9ù	J•ıâHŞa¹âdš8„fkL‹Ç‘N3ªz¶Cdî¶T¢ûrJÉc¶§·nş?YIX´h”ÑP‚ûÌı0æ^âsøïOîWè\ÆBåÈô©ûwt-‹ß¸]ËoOÍN/`'Æ'§æÙÄÜìâüÜÌÌÔ¼4Hp°œ‹„@´9‘<H‘B•4
E·¢¸ßr¯à†çåû	ıcÌóÒ¡®·Å}‚Ü]ÅNÏŸjqö¡ñÃ7’ÜC'úµ 7èçªˆ4¡~±f¨lâ;Š©ÍF»¨Xš²¤«6+®JEx¢ºw‰¶çÄJÎC%D£S€4ÚE•?ñc@™Šì¸	
ñ´Q0+*/ Ø-!u+-dBI«-Œ3‹†—ï¸Uxƒ|H}Wäà}êêeôŸ’0{$UÅ®YêècîN^jç}xQ©,ìÖ"=IÃıé­7ØzUWÖTXÚvƒ-ÚÅ»Ë³&ÑBœ`ÉJ0ÃFb(F¨f(ĞO‘Y f ¡–VTÖd-Iªs< fÊ»ŠÚŠ‹¼Ê¤=GÒ^˜÷¤Ú˜·¥h6TĞTP`=Ì[˜±²èu9áuÀ	QuUY%ü,©ş=ÌåÅ²4Óz!¦k>^,6Ø²&Øpı]¸øî©aºzQÕI~w¯Cyç?Fá&šÁwsÊ˜2—Î©ÎVíTƒVG°Å´«#¿<Z¥†BŸæ2Ó8BXãiĞÑÌ"%ç‰¦ÅÊš¥Êîd±¨E­xZôFXXç–bÍ
ç£LÑ¼¾NYæ-¸Æ3„«¦9‘ŒÇë‹2]u ƒÃMúúdŸÆ"UMØÊjdwŸÇœ÷aıöR‹¹ùÈ¥Ñ†t‡™p› }àşÀãï°¨ºy/ ±Ÿa` ÷†½Ïë@Õ¯‰úx˜“àŞgpÿ÷<®š÷ƒÚğ}ïÀi|ôrâêÌ?ğøk®e5=É"ı{%øˆGşÄø{áí‰ó²Ğóªhx@ô‘DTešU›€PJ-ë¦iÉ“šÆó”|…É =şU–‘…a÷t‹;´Dp æÜãe;¸uC„Ş³€DúˆaôëCÃtşôêKë¾—xiÅÈ×»ÌSFx..|X
Èx±w›©(8’?öòw¢Ú€ Â=4`ø·½Ñ
©Ø«^EyACIÀ E
òÉšîô_
Ş¿ÑO€ˆ<÷×„j±…ïd£ë= Êãs"‘À—&lìU‘rÒ²ÑYyöK‰"f4²¨Ñ(âF£ˆ%Å†‹*~40Šô!Fç@VéÎ¢ee,“!s†m$(<{ÓÊØ¥¾Lç¸ÖŒt\+éHqÙ|`¸ŞCd …8GŠÒÏDéL-t0hàM:êåâ½;¯v–?\ğ··¡Ât]üx¨ro&aŞ9=Kôx %÷ ıl‡ì|€óî¿¢
Tî~Çã9y~¦ŸÿèğÒº°rem#_Ÿ8O
8ÏÃÅ°²‹Z<8J ùŒ[ò¶	©sÆDÛ¯ó&¤"DÛcDyµ°´¨¶KÔÖˆÚ*YªjìVŸ†Õ‹;Ûâê;Å&rİE‘rRKô|0,şT"™9ë%>ø×’‰‘üÙÖ ûäNAö2,çÚ|rãyWfTLtcŞâá6·Üoİ¯İoxXÍßì¼/İ›„
(SXæêôü¬Ô|d£iew”
›àñ¤hÙ1s•M‹õ„s„ÑÔ•ÈÂF·­j„…ÃßI[jå,«ÂnÄBˆÃÛÎ ìœ+ÒrîD…¦Ye	~T—šsX¢ÈßÚV°Â‹–ùÏö`Í ºÖê]İ×ÙêøÊâ×Ûâö¢)ZÅK~ÁK^–.Mˆ-JRğQı÷^Úd¡„
¬[ƒ®‚?º[îUŸšykaaœ-,ÎÍ¿Å°ŞÕÜ¼|öĞÊËßøÉrí¥äèdİ,É0­g•’IÛk«ãUôB™…è›	9éb32ë³g…i™yŒ‹çÁaˆÁÍ®):[p¬ZÁ©Y*{™ùÎe[•|ÆÑ±)¦—F_ÅQ²Cœ]Èœ•ó˜Áê96²ã–R-k¿ˆ6«˜Vµ»i‰:ÂLx K¡û=ÍV°””úi«–±ûCÅ>Âì`bq$'ñ\ë‰Ñ…f ?ï¢â¦ŸıQÑŒx9şN&'UM³Ó¨ƒÅ;¥ËğwË˜h“Ê›	
˜óc—0U-7ø©tlàéÙéÅ·Ø©ù¹‰©ÉÓóã3ìØééŒìŞ’½aaV9hP¢';ä “xV)ÆÖŒx"s÷GÜûm½S¤´öºuó60¶áçš¾'ü´Ÿğ£±®òòs¿­_N°”d*°h„`ÅÂ± èo­±¹šS5Kc3uR¿ÑÔC8|,ÊN‰"N0ıÛÜÊáIaŸø–@•=4éP Y@­BÕJ†_woÄ’"àô-­*—é ß.øÂ4Ò1¡Ù³…3Ç}~'CÖ;æ:†(âöNZÔ3.7>5Ô°ÊZ<ûÌÚÂÒÔõ¢\…oÖaäeA×%û¹Ú-è¼c}I–d) Ò©t×Ç&úmµ¢öX_Ùqª£ƒƒ+++‰•LÂ´Jƒ »ä ÌM®Ciò(±paÙµ%Dƒ% ˜:'–BCnŠX#³¢ıZ‘Î€íhA³
º*ïVğæ¤¡
Ï­Ñ³Æúº?û²Ù–5][·«š¥‚ÈeZòÙafU)hŒ:™H¦ÛDéVå^Ş(í¬éêØº|\	ìßQv~I¯Y±—8ëæ
HÏªQrÊ—ª«ıÛ¯¤M–É4°-1­8Ö'ˆáIÕ.KË	Ô­E» TUê±Êy‰K°„í‚z±â”	‹ Ó8™RÿfF¼ßùüÎ±™”øı6…ëqïûEJMd–ä;^_‰'€ˆ’$<äÀnhd$èZ ¼  Ô(“§€É‘D±™fÅÀ)õ¥³”‡Ë*sê«½%Ïd²)Z°Fæ I¿ÈçrC#ô.Şğ€°ox'ÍNÛgËl(‚…Á¯ae€×ân
˜ÉåÔpZÙ7œxC`C	Y-Vè°.HÎ¢Qe!º4$ä¡a‚`Á%"L¯§.S(¡hPJ¿³©3S3,Í¦Ş\Äª-S“lbzq|®œ›?ubÍÍ²3ã3§§ØÄ‰ñÙãS¤el7ürŞøòË¤€Ú£ò(É‡à[úĞNdaå`t¾­ á2Vÿ-áşA¤>ÈYîZ†F¤dØ ]S$v‰1–„ëØ|¶IîÀ'mYr!%&Ñ,”Î…›İĞÙ¨h´Œ%p5ÃYÙMTÒÉšÅ²¾¬|ˆß¤5ÑÛÑª©¯•$³‡›­jj†cãN²t~ —cüa©lU´H–
P]3(lÛj
6VÁÓ9’ÜÆßõnxÔ¼ŠO{òˆIÖÀ0„Ğ±‡ÌMç¹õk;;= î9D¦Õ=îÉzÜó-bîìPÕ•5è-õ3ãÅCiäÅÃiäÅùtS¼x˜NùBóâïîñâÖ†¿n™†ÃÌú‹ê2sQe–R©*–c³˜{'c?Õ±1uÙ?0ı¼§ò’× =–içC3m:Û„jd2Ævˆ?‘~â8Æ‰Â‚Gìâoı±]pP8N†),läÌÀæãxX'Y»±±”Ê—Ã­{¦·î?Ëu§ÓĞŞº?Ïë>Ü[÷çqİê—NõBæ16CÑ™Ã}æK™Å2ÍeV°°.#-†¬“ûúx&‹šx.9I²¡¡Pqt\ˆ:‚ä¥úRçMÇ;C•RXNqœi³#„¿ªj‘ŸQ@½•¶à²‘œØf5ÓÒJšáÇ‘’ Â¢Æ3jAL¯ÌñaŒ‰F¦,0ÌJ$šÎF¸QvÖÌ`^$XHÓXÀ•ºd34Ğ(ÓŒeO¦%æ‘ŒtrÆÃT9ú9Z•œ•M‘ SR+Dˆn¡Bû-CX§(¦ü½éïMÓ•¶ıZÎ¨I";lÕ¥iö$/Èƒá:ˆ/³*hÏÇiüs™^ñl'”5
İóPhªŠÊ¢ìó/à\À¾†Rœ‰¥s,› ®bøplM\à-\ Z'Cz?ÁÊEË¬ÆE¸{ùó@}_
‚‰²ŒäÀ `øÑÁ’t¼³ì#G1AR€à²áÂû_êÚ²üÄAÁ€XS¶–j°Á$¤÷ÙQ’S/*FIµÌš­¯-¨Î´a¨Ö‰Å“3D:w®ìTt@!‰'ÿçB¼Yî­¼ˆÊzSÒ0UcÉ¢ZêÿgÉÒ™Ù±¯Ì¡·îï•ŞM’ûH®š)‰ê¤J[¹±¨Rà5Y†±C6•\†UÅ¹ƒ<Óqp¨õ ¯‰³QéUXSŒ¶ó¼Ú²P[YÃœîõôÖÍ¿±IA°Ô°jiÈ¤9¬%»Â³**ô WGÜÏP•'v\ñ9¥ñ“õªüLyá¡|4O]íX’¤=OZ,õ°(ÔvæK3ç˜mìŸ³5åËTH”$àF_!ô:¡›²&wß7&ğJóêUF¡É>¯±>~ncl\×5Å(¨¤DVÛ>,u9jñ‚Í×–ÖXl®*ŠÖqhJ>—[&;öùàÆ”Aî§w‚e›D.Âp4¼—>•†?Ôi÷óD”=L8æF´ÖÃnh‚¾ê,4P!ÖØ8”bÑ‰¼	Ãõó+À‰Ëğ¯½´RƒA¶UU{iÇmÈh!¿L3şKÔ.ğ%OçúÏú.'¥LÈ>r¨V×£?‡iu-UÔã{öâzƒï-®×Ú×£à‡iq«5«ª«]®¯7şŞúz­}}SÉ¥‘|êP­o° æ³Ø›ÀáY`ï—×iƒÑ¹ƒÀWh™­ócç£Òc’”£z“&CÀSY•QAiÖT€nÑ–ß'¡§¶šòT|İ¦Í½t¨ecÍ¨Öäƒ"œµ*@ÎB¢¼şPÑ’©»¢#JmG­w/baù±NË*İ0™2¸ŒJd3>“i	íèªªbÙêëº©815á(VIu|²ÒÇõ`“ß¾òğëT«(Õ¬—O6Íz‰)@İqÔoV¨Y¶iÅ¹7	h¸R@×x³*«,fHr/*#:¦›f…á‘[¯±’n® Ëˆ×é1"Ÿ	0a}¶ ıZ=Ä¬¡­P5´6Îdš³J†`fd-‹z|!8laÚè1…ıc
{ö€,Ó	ı­šB9Ä0‘—Ù”¡VÖğhÚm>»˜Ê'R)¬ª nxO5Í´UËUí¶#Gk©Ö¸Kñú”,“ x÷ÄçxcC¾¾ztÕäE=ù¯ø‰¢ÜMqp‹»ånúåKïñãG_fî§î§ÌıO1Å3q)X½"wì@Jœ¥ŞDuéŠ¥„R¾í+^]å‡T¹@æ#<nÍ(j%“W‡æ¨à}oE/Cˆ/M¼hYÒˆĞØ×{bØ	h¾ êjÁ‘°İÙD'À;FZyG$á$¥¨Ä£²xŞjÙ$œ‘
¢Å´-6JcŸœTŒš¢SŒeE·÷ç¤¸ğgREz*• kùº_ÿË3óÃ§°<5%çdlÅ»Û¤xT§vßx°Æ¬˜5Û/v_m;Qü˜[İ§Ë&áIù“Éd9QĞÎä§¢¦ô´E'í@)·›Æh7 .©ïñlœHÕ,äàÂ¸mê¯w¢U£X»´]³ÿßÿsğœı#ğ¼TûüIbÜˆÔÒZö¬(şÔÏ`;V­G¯ôº~¹~¾ òn²N4›L¦=ŠRØ®~©†²¤«Å•~•ŞH+•%Ú*}ó¯O¥¿h^¿RÈ£úw‹ÅŞxkàmÒA>rÏ-/cè>nâË¬"FZAõB³=i€'~^PÖß¡E­‚&yÓğœÂ²Ñ­;¢«tÕÔŒúxøfÕA…Fh,N8ÕE€(•@myô ÒiÆ‹zWğ ¤Á|SíZmZ„Ò;X}¼¯ØyÛK§a=Ûú³İ¹!'m:7ZÏ%yÈö(I‚hH¾qoïk9K&æ~ÉùàVıJˆ×¾´"Ä“Òí÷i=åYøš“Ö“±7iÙ°ë‚4yDÃ“ìD•ƒ-±špÌ×µUª3˜1Òñç˜(OaV¢½µ€X{ñö^ â×ˆ}Üº$¿æ3Ìpk šb›6Uñ&?ÙÑĞ CDÒC&>tÒJv#‹Ê™,w)Çé…Ü¸k™şxˆØ#Ñ<G3ñiÆ:R	roŒ5X±jÇÜä¢r²L¨„[¬j©¹Á&F_)l‰D»¢×Å&Vh”`<ê8oÅÃ‰±ÕÑ`ĞYÈÎkP;8R€C³]
óø¥/§Ä—ówŸëoáN4ôŸ}-ïDd²‡9âçŸ9Ó«Ä’C‚E{a™óZ97[9?»­õ˜ó÷˜3o?wæL4Ò<ÿÌ¹§9GÍœİcÎÍÖcÎÏn¿î1çî1gŞö“9Ór‰ùoÂãœNx1Ù§TåÂaó:{å0Îãì×ãxQıÍŸ»›îúqõso¹[î½0NfÄ¡¹šS5m§çcŞÖvò(Ôzf†™ƒ¡ç]Şå±wYr8=¹§#{Íc6=¹Ù™†ë¡~½õüÊ;·vŒÚ1j9Ï…n|™rÏ«,ÿ|Ï«ÜÒ<Õ§Ç”›­Ç”wo=òN÷˜2ë1å7Yşù¦ÜÒ<C\)7[)ïŞz~äî1eöB{‘3¾y¾¦öas#7ïœ'ÙÃ‹œ¼|³~ÙİtoûÕ•¾ª_ƒë×C8”Ç‚†•”8Zõ<ÊÛÚ>DÁÕs){pèù”zNå]ë9•%‡ÓS•{ª²×|~ÓÓ•›ípéÊ|¢S–yw=¿òÎ­§-£¶ÜÔy¿Â|YsÏµ,ÿ|ÏµÜÒ|¨Çš›­ÇšŸÕzŞåî±fÑ~î¬¹ç`–¾§5·4ß*×cÍÍÖcÍÏj=óN÷X³hÏ‹›yOé¿$[\ø¯=Ïuã,6Ùºã;UO‰jáéª…§:”Âä|³ ZÔèßéˆ³%İ,\h=èÌ7Â³–Â¨.ÅS„#ÏÃÁ(·Ü~ 'qµ~¹~Õ¯Â×{ğsË½çnErà™À¡E,k_°ÙÇ âIgríî½J{Ço…òSGy¸m—!mçR Â†ˆ“8]-YJ‘|xíN[Ì	
qœhëTf®hŞÌNYæ{…¥’Iš›¶áN’<¶ŒıÓsàç¥*Q!(®<‘º°hRô5¦$IeòÔ¥ì¢ E$úèãÁP÷J¢s4"3QZ¥Hª”Uë$¡’toii”&R>ïÌi^-€zè¼€ÌÉ›Y9uñX9uÙÌ)€]Ëœé1'‰ÖcN’í ˜ÓÔ¯jZõdM|^=ÆÔÅc=ÆÔek0¦n,[j£Ç”$Z)I¶`JÇeZ¤hÓFÁ¬¨'kºÓ`éşK”éŸ­O%èÔ>C{6"zß\ïƒ%øÍqô(¾D{á)>%·wÁ*ÁØ›z©‚™–H˜£a!‚, Š:xüzMµl2¡}F¢h4\hŒ?âSæŠjÉc,m~4é‚ÆDB±Î@»Ï'0Á:¯lp+ÛàL£e4ªÏ¹Æ´áÄZI?‰„ËpyĞ=³ÌCN#$é¸ôÎ’|@–äcÌ¼V*;l˜M˜º=ŠHÁjŠÎ0¦Á4Ø’¹ÊŠjU5ŠH9áî©™v —F¦GÒfœ#àŒ\ØÆ|”•¢ 8¯…Å©S,?ÊOL±Ó§ÏON±“ã³“ã‹S’>Á°”œ<†¥äÙË/3¹Ó‡f™`­å,4€vÉäõ+ÒÍú™UUãÀ‰‹£Õø Zjw ’6!–‡¶©V«v°îE[<R“­·ÄÙ«,ÅÓ$vùôÖç_ÿcës?q?roÂ¿/ÜOİ?1øõsğ¥û±ûŸîwîŸÄuÂxÇ_»ßÀOèŠÀŒËCÈV·‡x¥Rm‚R ª«
îP dºº'ÂQnÓøÍı¨ş{÷¾»U¿R¿ân0÷qı²û>nÕ¯2÷¶»Q¿Âê×Üûõkõ÷êïãÕú”·ÜÅxDÖíú5æŞqQk'µËÚÔ'JP]cxßâ¬±´IıKÓtx>*Êë Òn pú 2[ü´{péÃ…p€°àïú
øo×¯Ãîá+bíøoÚ„YÿÈWºş>¬Í#øúÀİHP^ğmE6ıú0 Ë{®›ˆKŸÁîò ½õë©Áı²~PñGxÃO°P×ê2ëûÎ}ï{W®`œsò1ßyFE‚×c¾?ğ9 ^ğ Â{¸>ßs0=B0Êêè`Uºü’ào&KŒ2×Ô¦EV¨YVÎ)aÜ¦í ]¨àWù’L»q*Q©Ï6˜Rº‹ªL•5"M—PSIz-İr†²!r†Ü¯9¶lÁ&âøWÿ€Î?vàu“~„Ú8€¬¢g(¯mAÃœ}“!Î%‘Ô?†‚;gcŸg*ÿìyF¼}şqÙ‡†hÄ	4A²~¾‰Ÿyò¥uq :Ø/Ç8¢½Æüšg¿aˆvĞ"Ówé<ÍeK]ˆ§·>Ş`¡^z.wCÖG9¨¥•ŸÆÚ6–ØC#m)XcMs]+Ğ·ÀxÒ™ »°P˜yå¨– üm´ä¶›\ @™öèåªÆ¬_£¾æ.P~AIêGŠ ¼ş„8‡},G¹“Ñ:ƒù7(2t?ºTs“FMcĞÿÂØzŒgGRSmÕ9Õ˜İ½ÓLÓÄìU;”ìäùgâƒµ"5³` 1œU6Æ™Ã¤R®hFÑ\IŒ×Ššé™ÌØo~¢Ã˜è‘)6ÈòkÔìDÑ+êÒÍ	Ü_Œ¼Z>ÄM»ÀÆBLV,Q°T‰æì‚¦ƒldZáGUŠhLÇÍ3€NbÙRUSÂZvë4¢;‹ZEƒ¡C¹‘D:&½–ÏQè”8˜¹¶‡:#ùáÄğp„ĞŠ˜L«eb£R¢8!p’‰dşE@Øè†Zpb¥PÀõ{		´mk†‚.•ˆN¿°ÄÊvÌjX`´lœP¼.ŠÈ‘Aq‰§Pfè„]§Ö°ËæÊ¬éhËZãª}\ÍD£1(\×aÿıŸM]DézÄßg±WQÇjÕ…‚ºJÿ‘>:\ûìZ¡ Ú6¹âÂ\¢-ÅvÏpuM”¡ÚèP29˜ñóõ[®æ’A­5³Í×Ğf³-eìZo§ÚğíIùğÕµVâŠ®S¢ÖhZôÓ[ÿñg¶	,¢9X˜Gw
İJ~br…"¼·‘¹HÔ8m°vsWÿ!Ó	ã8¨}×	a.ZEñ˜Q/ªz,²¯ «?6ÒSV··²ºs/{¢¬Â¨Ò‡pX/ºKg^8Z]­šˆÜ¼R©.šÁ*•Ì%¢„×¨³±tÏ(³ckÏaT&•OäBØTJí‘&•~ŞÉÊ^!P2‘Œ{BZ*²a©ˆÎÆ·}Ï¦hQÙÔÒ/¦eî@ğığYæ>üsÿÀÃq0–´~­~Å}ìnâW÷Q0’Ã$y@Â£¶èÓ#Îsîû°ş»Á«^ë<¨ìrı·<<ô–™†™;ĞÑ}ŒHH<<¦ê¶J7lD·nO¾ø`—’Ç<xCqoóØÜÚâJøÆØh3Ã8]ŒhĞË}Œ&½Ëÿ¼	kú¾÷Tı]\ĞúõPK©Z–iíûB"Yo;Ún3‰Zo±x&wÌ¸Å–-³/¬)Zn\ò’,ğ›0í6ïÈ5®5ïkœûÈ£Z[â_€]ØsAsn5èŞ*Es%şNò\ò\:W]=g•–”ØĞ@*ŸHƒœ—LdúÏz²Š®ÂË@¸9+o	¦%VÜü.Ïí!°OÓ>ìD³¯‰µÇŠÊ–¨FáÃYŸ˜g0Œ²ÅùñÙ…éÅé¹Ùñ6?÷şœ:3¿"ÈcÙÓ<&µ¢ZŠ^|á™üyíQ*Ó“ÏnòL¦oİ¯xŞæ,ı­-‰i”¹ÿæ~Ê³‘nÂ÷Ïİ¿^Ãƒîß¡ÿ¯İâî'^zôJØ|ÏI~Ó-ïÔbäëœÁ"ı\}ÌùïŒ'Ö¼OŠĞå).›~Ê	ö…œßcî@ÛPrøÀO«zKF)ò¢ïyVĞ#»]0ïóì MjÅKŒ(BÚÜrŞ‚2'€Ä»€6»¼°Y1‰-œpı:–Õ¦¥Ö<âå¹o A4Ìã}ÁƒRì¾îJ¿Ÿáà¼³ûŞÀßVÍ)'0—Ï¯ÎuIéGè,¿‡âÛ]ìŠCXäÍÁònÖÿÀEr\bD)ÄC*„¶D¾–ÈÃ»Ï“òZPLƒüÈÅÕ‡Ş‘ÜÄÄ0±¤ßJİ‡yÔ¯D• ÄØîWÓ9!r¼:œk‹`¸KŒ©»~^ã9cˆy Ÿ<	üÉAMM ¤å„Qs¶R»älµ¶­ôxŸÏB~ò—wéGkwÎ²á‚~µŸx"âU~ŒÀ÷İ‡lâÌıKPA§<DúÍ(ã–d;[xEı]üº!L$ÌŞ÷C½ùòÿ×Û¯"ÀñÃ{3øé Öœ§	vı€s¦-O-C¤Y¬Èüø ëW{«Áê
2ÜU:íŸ.r›Ku›Èî„àû¡q!ğz.{C:åÙšˆôïFNsı:wb4`şO»Fã–û~‘ìSÄ°À:Ä%¥’$Ã\4Ò>aÍDÂ"HÊ\ÂæTFh<\Ã/\£†”†²FàD0DJš!}!Ø³D6¬’;dÀtÒ4E6Œ“ûaˆLú†ÈZ"s©ôˆ°DÊšå÷õÓ[Ÿız=¹?!Aå¡ÉÿÆâÂÁxµÕBw„.'ı&®£ó’Œ¹ßáyPv{òåMiÚB±@î—11•`©üK‹ºHÓ³³Sólbzq|rj†;==39={œ›ŸŸk\üå¼nO‹şfèdY<L&ÅN¥¼),µk]Õ%Ğº*Kñ½Nn{Z—ôÀb‘”xzëãqóúâî¬‡õ¼xPL‚?^¦jë-¤ èÀ“Ëß u}e—¹µÅ/šô-,€¾d&Ö”ï@•Û‚t…ÔñÅT>‘J%RÃ´h÷PåŸÕ¯+Mî ’N½õb	ZºZ[ïx™$Ûƒ\4Ñù;ı…ôÇ2M¬¨^²@[oùJ÷bkŸíWHİ€¼™•éŠRRmsóºÁÛ.:Õì3–RYP@/8ÚEul}Û%êh§·÷İé*­û–ƒ±õÖï¤.›	cÁ!ÄÉŸ
ôÖò•Öa› }¶]!î$E3Ôâ	Õ2g.ê¸•‚ßé{©µ×m—ÂõÍjËPß¬F0Rì³ı
©Û2<Ê{.>aÂ‚›+Æ¤4p‡?PÇ~b§÷ìü·0pC±,Í´ì ®…\–Ş;^bì¼i^Šhä­ø²ûñxÇåİùoÔ™¼±ËÛvı3é…*‡RU‹ÓZ&Öò•:ƒ©Ö>Û¯º$i\¯ŸfZ¥¦s¢ÌfÌ’Í“é*¨öeÕã1BÊg@Á™Y±Ö6ÿ[)cİ0t,îëÔ,CöøkÑÚuœŠÏ² 
”²L‹¥¢³Ó©QH0JõÇ;; ÿ´U¼kV›Gbï ‘Õ¡?~ÁÜå¡[ñÖ£ıÔ•[ü\|ƒ¼R-Mg.¡ûoÜ?¶ò5¼â/î·îçîßy€Èßñİ_º7Å+>u?İOı£f(«ñ2¬	_4k-ëæ
¬¸R´mú$ÅÊå+Ç—Ë.X¦®/)VÜ)kò
¼hëŠ6Îk[ãnITrĞtL7KLãv]z¨)Bˆü0cT ›5Ò[çòƒÍdö¸Ş)œ+{Z§q9Ö§¤¦‹Fõ¾`[‡u €ìĞÂF=°„Qqø}$İTÍÔEÆ±ê>Ğ<¦>Ëë´áù"~ıØ¸ÓÆ"8 ª½Ğ`8+×6sçQ–J³×¨Eé^>ä/øİîE»±(9GŒ/ú
ÛØxl×¾Óˆ§?lñ(…‡î½ú5vâô$½ëÛ^,ı»ĞšÂ¨(š½1\5ol¬şw²~_¿Î¸ÏÂsÀ“j§cƒN0Äãæ~ô+lS«BR±¯ŸÂcJš{ïrƒ/F¸`¼ã¦ûàJVßcœºªSÃ7ßGGwk>Â"â^ÅznbşI o¸6©¯Ùê°Û²„¨J°çGû äº¡ÑxY»ĞT‰ˆé{ˆ: ‘UíGBêÚVô`Êß…«³m¥•=¨µmµ•hë­„¬¸UÍ•èË›ì]IÆrùáD&&/=ÂäıP;o/¡”Ï“KI‰m23Wİ3$˜£¨¶e½=(g¾êèáÚQÔIˆ¨RB„µ" LèJÑÔ:ØªKˆDRò€X:³ÅzX]E[ÎZ<µé× ÇüşÓ¤[+ƒjÅŒb”x
nßüiºİô5»Áí›°Hïj”õÍ¢¼ŠcíÉE9C­vEÒntç÷%âV
ªŠÇ.+º}0…¢«ËÁ#qÚwP(Êj÷aÃx‹›[ßå¡ğwšŞm3ÀAÍwİÛh·ºˆCš0E´~-n(°ƒ'”JUÑJ•ª®:jñ›WÑ³Ë=¼&«(Õ0åD"¨¦ºÏ¾p©jÍĞíÎ§¼{zœªë†ûä“Ö7hõÿŒ›P{yJ²§·n\	ÅÃ&<Äg­À»< ¶¿§&íé#s‘ê[âæ®oíb¬]õÕDÒnx.YZ‘áL9°ã)¦—F›_³Ü&=Ôê­¢çëZ ›ïÊ\‘?3S¯/²‰¹™Ó'gGá÷ÉSã³Ós³,6>5óÖÂÂx?›]˜œb'ÇgfØëóã'»=øz·»Ûs*Z&Ò<yËŸ@Â¬U»4Áã¬&×¥¢ØIèÑ´Ø4ÔšóXlÖäñ"ºRµûùDºïÓ£cIƒb` Ô	UA
{ÊR—•%vL)–T¶ ›Dê;÷#÷–Ó¹é~ÊK¤|éşÑı¨¯ëĞ(‰í‰cÁŒ—Á‘œï	Ow8<p¨ ÆTgËˆ£D«X”gç€´$Ä¶ñ Îa##©dz MaFX®ÿlsÁ1=e­XT ÛjW¥Ï¹ÿ¸lZ•n	¿VëóWõœXËsÅ=wJW~US»ë«kBÕ9¶l¨- ç§óp%NVYŠ'2!ÇásKÿ¢œQÖ>K±çW»î/6£.;İ:ä¼}ÀZ7d"Â‹(F|äşlÊOİ¯±j‹aà\ã`vb×ß} 1x“ÅNNÍ.ÎÍ÷w9We;I]‰gÒ¬Œ?*ÅÑØxø»lèwÒ‰œ¥VÎ"=ğ¶<VòJîBZvkf‡İºÛfmK-Ö,®¯ñ`0‰BVü
Ğ T²es§’İcº8ÆÚ'ÉÇ-¥ZÊ»· –M©OÑRVPoé>&õ¨}±ÔõÚo'¾e/y7îƒğîcæêX_’%Y*Éÿuÿô²¦ëc}†itI°°­VtÃë+;Nutppee%±’I˜Vi0„tÛU÷4éhA³
º*!2 "9	@Àk²OXc}ÙœÌÚ¿H.§†ÓJ÷J ªª8e‰';™N²áäü„ıŸüÉ±şœæŸóüzß3œcø?ÿ
÷äÓş9Í¯ó~Ş&ÀCM«ùe	Èï1<R8¹‰ŸwšOæšæ³‡Ï#üú¿~EÌóÙ#<r)ş9Å¯ó~(ğXN-ç–G<r,“›€i–C€ä©4~Låc *™Ü‰T4×¥Â²š?4sÍó¹ğÉğÙ¤qºù<Ÿyš	„‡éæ_„éf8gpB,Ã×2‡4`bˆOt˜_æ÷ÀÏ¡ƒëŞÖ‡§òüz†Œê‡iÛÛñÚƒĞ sÛ‡¥¨>o^P	ûÚô­è”Çú@$;4€æ»bˆïŠ!(øì¡I²É$^0@©º®UmY1$+-†d%vˆ!ğŒÔ(é6«’Ïå–‡h‡z }f£ÉÎÙ”4d‡$eg©7øü‚·C§¡Ÿœ(rŠÅ 7çY6×”!rüó¿>”£²‹|*_X–Ú×-ìBbƒw¦£¨wHŞnŸQ–lS¯9*Ó[uâÉ€Æ3zp»f¼q&làÉ ¥`ê¦e³ª©¡16®^ÄäÛ8W÷»K;ÓÎÆú®­4İâ—=0ğ\Ú €Ìæ›§S—°w·dÉìR[]ÂšÚ–sÿèn¹wúÙÓ[ıâÉåß?ùóålu[QÎ8*˜"ep-¬NYµµ_«Ü÷/Œj6BZ¢ùhú°ñó‘DÌ·ÅÈ‡r—k²GŒîEü*šÑÒ!%p£%µª
õ9–¦%]í“ï"ª0oBøûÿ  ÿÿì}{WÙ¶ïÿ÷S,Ùûî‹»	&á!rôô@¤•±½€İwŸ=ì")H¶I*»’ˆlO
Š‚¢­4¨ˆØbkwûˆ^2Æùá+ìcx%ŒŞáÎ9WU<lªV)è¡z4B’Z©µÖ\sıæ\sş&í51`ï§º¡Ã±·¸°´T°óTÅĞà
î¶ ÂÛL‘Â9Õ™~Z9Ífê®;Çë§™ót˜:Œ™±Óï_anâILÅ…§#Á×>C­™¢nÌû×½›—©öæ–²/¯Kf ‚9¸e®›ñ‚%Z¡$ö¥jãñ™]ÄQÈ+Eç.PiPƒØŞnƒñzyŞ@“bèÃm /—UF/I°N¿‡—‰¼JĞã$Ò@¾ÌNxÔ™‹G}ÍğÒd =YÃaàØ¼ÃkóÈ•‹Æ…‹bJ3ÒuçŸxÃ­$!›?Ñ7Ã¶é0«Í›%tÎ[UÛPUÇœ±T_Îj*êW±CÕ5ÇŸ¨‚k*ª‘ŒÖª8%ƒUü!<2«‘TŠ’`nİ¬îøaö…
­kì rmÂ W…w[Y$ ª«EQOŠÊ~ğõÎü³ãRµÀÔL ¦®æÅÍDèÁ<X÷#'$ ][®¨¸Àép8‹Ê
ì…Îwğ4Â
r«JÖRDåqF¶R^º«ìİ»Ìï Ù‡¾1Ğ Y³âs³N….{Ã>XÒ’
º´ul‡°‡5Ë~†»Ëô&`e9 Z9ÌìOÌ¹ÛHôÀFn ØdlXƒªIó¬5,ùaèKxĞ°
£™‡¯¦úM+~C±Æ¥sU‹JÖÅ¡QÅX).ë«#Éíkìø0Ç=Z.ì$'<5l~¯`r®ıv}ªs‚ÌÊÒZ9‹g‹~5”„ÿÏÁŸ6ß+ƒä†¼|±Â	±Aõ=»¡-™‡>İ°0@N×Ük8Şb,dºœ­÷4EA+>Ö£»{Ô0nTàÁ~ÖJÈ=7Ä]­İßÛoÆÄŸACñ[Œ¢1oˆŞ6è¢1D7¡“6+ÉÇ+¾Vş[=H«TÔ€¬†Ø—ŞP^“\.¢Ù³L÷¨È¯ÿ¶€ÖõÀÿ:SRFë¯·d”…ULAqD¾ã@şŒŠ\õ”™Gó½û)}ïzJ“It„zp²‘ªÄœ”¥S â°)æ“e  †%cQ%®ÏGÅK®Ö;éá]Å%Ã6‡id>£ƒtP¶CÕ¨âÉ«}­¡dhIã„É‰ùöËğe€è¼áìkBì†« CÀ-©€/Â­>c†Ô¬tr®BÀEÜ,ñg›'tX°ìöFükŠYzÃ’ÏëÚ’éß–°•oãı:UŸƒ%>QÀş˜ı”ÈÌŸRã”Í#iÂêC£ÄF¬ı<C.ŸÉËİ¥2wKªË#k7S	GäbÂïkå°Ídm(ÓğcXßD>§¼ïxqî¹.Ì=zCÜş1*š8ƒùx÷é†"ê¢Ú6/©`Á«ìÂë£X¸ı|ZgÔm^qqœ×¿ğ¿qNô5×3×[Èp³~A­!ÙÍ”Õ’9	€Oá—°¹+¼Œ¥zTj|šsSñÂ¥ã…ßŞe×›p‡ÁV¸xÎşÙv“ÍuÒ¼ÏğšåXš…~¿ Æ*ÖC›ë»ÄK@èuÒW×åŒf$‹Gyİ<ªoô[T»
©·.ÑûHhGó,_PáşÇzŒhT\T#\£"œH-÷FÉ	FÅ)eÏDòåú£i¦ú5.¡ÉÇÃŒ×Wá…dµÊ« XÄ•ÊËibIœÔÊÊâùÖf–œ‘¶Î¢ÒÖ§ï›ÕjqÌpÑGaç*@+üÑ"KL„½|NâÓ8Å¸¼²*ñ±*50-ç†¥|ã¹1ŠqõÙÁßóğ´Ø…É^XaFõBºà…«(¤¨i°ôŞKLö…Ş¿ ù¢ÅÁ´:"üÏ7ôïU-x.Â‰ÍÒF8Íoømğ$WµÅ\Xà&’eúòt=]ª’
GË/¢‹ÚÊÓFW	¬3¾6Pïä¬
ª½Ä‹¢ŒòïÕJÚ¢ûq®§Ü\®æú“Slª-Í¡GÅñ&u„eaşuïçW7¼ â(/Ê‡IQÒv¡ò#‘í!1å
ÃùÃ_ÆH‘kÃc#©Ÿ¢İà¢FWIU‹±6	ã|ÄÚ±v²^
ğ¼‰­Öu7qî_àú¥åĞ®ÕQ#ŞÎUŠR/Jc¤Î.ÒÃŸµLv™Í›]6JL¶¶á|üón•5¿C¢Ó«)áø=EÊŒÇNiÕŒIaƒÈ¦KûÆÈùİ£ŞM“‰õ6¡ëô~š=4]T[/¾«&šõ…|*I‹Æ8…ã}ÿ‚ê*¶‰IûTÛNKÚO—rä$¯³±Öe7Ìß´ëá+œ¶©¹nÓs±Ñl”šnïë£÷MÈÍ4„!Ù†Ş¡FéŞÅëd¿Ğ†Şz©XÂÕ69ôÒT£A'ÓªœÓèâŸ1Êîç|´¦h½d­Ò¦7Ñs¼äÊ’×Ñ~C{Ü(m
Œ/dJ@Á‰ñÜ³ô“g!ãÕÛH‡Nç"5áÎ‰uG°g´4¡œsZùïK8NYİã’Mo]\³5NúMø¥š­í¤…	¸¦…=–.ÃÛ<²Ò*¾f }`PÜ:f¹ç:LWFüEBªZQ[¾Ñc…ùu4Ui­ùrƒ^¾µË«ª5:oÖØ4É«²ŒŞçĞ:¦ØÇ"*ûBBªÔr Ì¼!øö ê`j†ªÂ †æ© ˜¡LibRÀ…~cöúe¼†¯²jÖâõ‘ÓˆÎíZY«A/CØí¹Ñ•¬*‘û›Qr«	8dì']RÍ$?uì”,e»……$Šü‡@·|¥Ú
F¶¯me~©Ùëâ]y¼²ÏıSyi?:Ğ©ŸöH>æRT5Äó6SŒ(ÖÁr’‚¿âÃ6Ja<€À“/M³îûÚÅèıµò¿&Í#…X#†ø	İìtháQ|­î—TÖä“ü2ÈÀWšWéTƒŒ¤)Ôk-î¥énP¥Ó «jÍK¸ qæWüš¼ƒ© ƒPéQ”Ì$!Ù…‡á,$»0q_	Ğ}«ÖI¾Ã–\a¯Úı="¹‘ Æå“¥ 9xÕPØ$¼´`Ò8À43—: áöúCĞ±¯¤PXíb‡d?üåA·&F ¹Y£áÇ>È!ˆ©è„‚¢€ e–Á"ƒ÷Za‰H­4FÚša¨X$„ï€)f9–0ìVDµK¶D×-;.K§v±ª3aUöc",rX1’ëTˆºæ÷Ü44º„[ÙQ¯ßë
± V}Nõº<LQ¡:€ã|1p…Ñ,Kªà6¾Îp´‡—0[=œÚØÔE¼«¹YF7%H‡âo¡ÖpãA-™¦t]¸‹`Ø“ı ‹¶ªÆH8³Ï¨R„Oöc,¡DDb°İè;?RÂFY]ĞÔ=<@8¾‹U€şÿ>¤W=ú®)„zÃë¦ƒ]xEõ6Ãói}Ò–Ak†Üüœ]ï×¿ 5rVÒ¤ÁmM†»°ÚøKñ±°
oĞ^“Œ,G¯'»G6å<Mİşy9zm¥o¥†;RçlÆ£“ê^~ÕNœ•ä/x{òQWr´[ø.ÕÿdKİ&z’º; ¿ÁgX²§-ùğVêîKı0–¼>ÌR®¤§“÷á«‡†—cÑå—ğÚP<y!K[¶±•¾ÇËS+}Ë£Ãøºu^3À…ÖZòÜ@òòMcêxÁÇm0Ùİ•º<ÂVÚŸ`—/¬œï¢w Yº¦õi _Õ
Æ>?ºü˜><0’êŸæ’|Õ¶ÒeÉî	üš»«İ—+}ÃË£ç¡	l×J÷‰9€Cãâ³ÒõF£zw§'ùğ)Ì<ä®t_ £C“©şkY#%;ºRÚpÒáÿä»,¦aK½LFa˜GÚà6|?Ù~7yî‰ù®[p¨ûË£Qìúp{ên&~p9ÚÁçG_	ly<
K€:ğx:5ÜF®koÑVÁ…•ó}0·8ˆ+·@Æµ¥úŸâü§:†WÎß…±Lu?Mu‚¬˜†5ÖêZ–ÙÊ­Ÿ“OÆàÙ“§³¼óqêò8L7’¾NX4$İ—_N³åÑÔDöZÂU2ü­ô>]ùş
ŠSò—hª¯#5ÁWİ½vX#©¡NŞ&,¯ŞåÑŸ·İæ.‚”¸Â™¸•ê!Í1v‰ËÒ;Fve`"ÙK
95Ğ¾<ş†åÖÊ…a\nÉç±äó‰äNP¶ lyìzê\ElytÖ×JßÏúxò‡_s‘šøä¿At§·~„E_¿|<Rƒ´ ß-¼Úà5–|2@ôš6–8CËÑ>ÚÜ]AÓ¸ïãèÿ€¿¡²K>ê)Èº6;íƒ\fæBlÌ	_eéÚ iŞÑ?m(²w3Rë]¨¼R]£¤Å‡¡§\˜`(^­€ÚëëH¬ê¦ŞnáÚíñU_òå8®~¾ÍÓ“]IŞ{ˆ’@Ô	&o}EgV½N<œ)`‰èåù¾'·;6®æ»~İ¤JD;æ/u¤šÙ—ˆuá!ó|×õ·SøU‰ë3‹³×#–n]˜ú 1øx~ôR¢óá|ÿp":³4u}şvÿ·ƒÏ'¢–nıòvz÷M;< ¼¾P<ÚüÀó…‡C‰Á‰‘Ÿ—î¿šÍßz…{Ö5yd±­“÷ ^Yzz!záíì]xº/.>èLŒ\L|ß³Ğ>¾ôôÜÛØ•ÅqşŞ¸q›€ ùú³ıóCí‰ÎWK¿Ìœûmº?¿Öë§?%n=N¼]˜êMtv.ŸI<ë™Ş÷ÛtÏÒíŞùşWøÊÈ½ÄÅIœ½~¿Û6ßş
ÄtßÚ™ïï|;9	¸ğrñü <w¢k
z•èŠB‡Ÿ¿ÍÚ·±§Km—æ»~k{ûeáÒDâÙíDÏdâŞ=hdqöÎâı¥Ÿ~Xœy–¸Ø–èùaéÖô^L\~üÛôím†u`BµŞL^X˜z¾0ùãâ•ï—.^Å®ÏŞZºØ³pg`16´p·-1=¹p§?ÑóK"öE·ûÜüí±´0ÀŸK®¢ØwÌjËááİ…ŸzùºÇÑ½òÇ„¡÷:4şÉA›ìL\ˆ&¢ío'º¯;¡¯óÑØ|û÷¸’Â´Øv~é—¾’æ;za‘‘ğE¯ƒ ‚
I\ùÇ;: R¹»ØßC›¸xÕÆ›ûØÎµG‚£¸LöX.Î&zG–Úûg' Ç‰‘‹×fpT~¼±ğóhâ^7XìnâÎL¢óüb¨İùÁç0´og.,^ƒ%<?8™è|ùöÍİ…W8³×gà¸FaÎ'.]Å+<¨„Pr„Öìó—Ú°½#ÚóÑ*áÃ]ğÁ³ù¡~¾À¶€=œ¼øvæúÛXwâÙ¥ùöçïZ[Óm‰kúDê»a$¤0øşB„ÖUÔV©®=Ìhh8VKé-uÇjêuJŞtŞËÁcÿO Bxƒb©érª¥ì6§=+G£E•‚<oÀéú¡ë«áí7ÈPmª¨ƒp1‡uŠ88L°Õ[Qğ />L‡Á£â8JÁtÔZÎC$µ¯¸14B‹Nš»7EÍo<á_&†ÇlÍ>kjõYX£ÏšÚ|ÖÔä3Y‹O´Ÿuµ÷¬¯&W\l²Šœ¥Ä,+“ÒùUf·f@(ÁÚTA<mæÂà—…ÅëLWÜ*ÓúîódIµH¨Ìğğkk¥J¼N_³E™ç('Vo+§B¨€á’@ë”r®¥÷ZMì•Éä]‡©Êi_[å';£í³é±ŠÖc+XÅ½Nï`6İF©¿œlOc8`Ì|3\^fmŞ0&){rR•õÊé2Yôhøˆ:;¼³dıºFèïøe]"Œ½‘‘*ÊCZ©"Wïç‚Nk±°~|¤¼¶±dWÇòø“T÷Ódw×òD‰gÛğé*˜s p¿ÅüÀóÅ(˜ß÷Ìzqó¾ò(L‹ñùÜ`&sõ™¶qéÜ†Æe:¤=FÏâ“”Ü3wuÇ´|×µcZf_Ÿ²iYb‘%õÉ˜–ûvLËÓrÇ´ä×i™uí˜–˜–YufwLËß5-µÑúËçY©î’ñ,@Ñ°(,L=ò—âYêVGòA©0AëKŒˆâ gæ»æè;yÁôqu¹–¸[¼»•òxJ×®`rî7 »7ŠŞ2fI¬eôÌÖ„,h%¯c#Öû{ö`ÁVæòÀ~cğ^s¤æ£eÆ£¦£†£)³QÌh´Êd´Ú`ÜkÎ:ÆòÂ]·ÚPt8-	"dãeZ"­1-3Á2*û˜$JÄ²Ò²„Dì -`È ”kş”ù@>÷mŠ9ë[?«ˆ!×µ~VW2_Å‘¸±	´®ş³)¬²(ˆ‰J#J™À„B2(ˆ•„3C1âg™Òè©ˆ"e5±T”ısèæ–&Ìæñä8-‡“KWnİ¤,ºG©¡N3Ïg±Ï¸8{'ÑùÇ¹óØVsÏE¦G½ì–Iú§¦Œ,/ãfÇF†D‘A+ ß’Gï#U°İÛ*«jjXCEeCueE;\W}ˆ}q¬Ş­­úêä_Õ6T×VÕ60;í(+t8
vöç=Æ4mÑú—œl¨®©ª‡ıúkãvCİ±¯ç7@ÅÉgÊ™	$¤*-¦îs)>S÷aé ß€ê3q†GĞ …?ì+‘J¤R-È~åoŞr¬”ñË-·`C*7u&Xw‚sRëlxœšìgg$Æ±üø]®ˆ9%Ô\·Ñ2üªª…oªUÔ°×°,EX~-ò¹+@‹‰ş¥_Wï±ü·±§¿š|º¿ÔA[ÉWíÉWm˜^˜ºså§F®'š1ÓŞwÆoÍÜ%2…ƒñXfúfr&¬G{YcÄYÅ2'1"ïg.{IDoP 4Æ¿©¹Î4™çĞúnhG.@ŞÎlñcôóñYü¶Bó"r\–\rSÄÇ´õ˜µŸ(M¬6:nä!'ªŸË#û½!äË8$}J«ÎÙãS\§d·¹'!ÑZ¼øËÂÍ‹c±ù‹m˜¼ñ}Obâ%—³¥/$ïıwÛ¹¥ófF¯G—n¶'İÆ<8Ó˜º–f˜Şÿ4u¾]K_]é½ÄÁå’Xbö4¦Mg¥—G£©‰–|Õµm3ÓmR–]^7<9îG'k«N4ÔÁæT{¬®áˆ‰'hD *7~QSÑÀJŠ‹ş+ZËş“Õ«e{¡ó¿¢_™yŒHSSHd1Ş]½şˆY0w…Íu¨QmiøÁ(ğHÈÁÿ'æR¥&äqû0U±!Z¥Cˆ‘’ğ’"ãl¦ IÌR}Pj	hZàÃJ“,…#ªĞN|ˆµ3dØ¤Î_"±+ñwöhÔÛiZhRİÄŞ:¡Ñ.Æâc&5ÉìJ I‹ÄM‰„™hVğo  M>¯+úƒiøÓ ÕÄ1­@5ñ}@m„ıÎÁ§òŞbW‘KŸ~ß¶øôš¦f9Ê š{RcZñÌ1_‹öÃu1Á)¬ã1¤zTR¥¡Œc‘pPÓ+ÿ ¿X¿:œ™,‘¸t%qã1Ëçé×5Õ7ƒdC×ˆqf¬k9M=jcùÉŞÔpûG‚] ,ÑÀOå²Æ"+’‰“ÚC#æávx—.˜%‚v½ĞÀäZ€ºáæ" S¿ KÆÛäEæ6æ’üAb¯C]	»"WÄ©èİ#Íß¬ ¦ó—¦1¡|æFb0ªÁÓ×Ï&g‘„â§öÄı¡DÏ\²¯=„ÏXR¹ckyüüƒd`ÈÔ2ô0ÕÇY}nßLïB¸zmq1d©Éá•›Û«šA—„nVÔªn8YYqôøÉŠšãG*¶ á–:2×^\Xò_Ñª­@¸÷xMƒ,şğ	†õ¤ŸûjgiıqŠh*oñF':7¿èªx4‚EG…ó¨dŸŠ¤«şˆ/ìú¼²º?fø;¤ßµ;€&W d“:Sw,§šmŠÊˆòıŸ>f~+š›U9Â‚‘šo¥¯‘ËÒi$}VO…üràÓBÄ†ëY"bß·ã²½MÒU¼áºf©¦“•Ûú°@¶Z©9d‰»váŞËÄË¨¾ÚŞ@7Sƒ™£¶ıÚ“T×©‡”]wvÅ-º“úlSõâ‡­@“õãPşAŞ89‡áß/…”€¥Ğ6Ñ;òvêÁâ³hb¦Oó¹NŞKLŸ¹Y¸7›œ´Ëê,“±hòş]b5ä|£Éá‘Ô£–zÑ‘ê¸‹ğv{ ×UÖªŠú†-€¡ÎÂ²u–oíÏªXCKhUù­j-	**ŸU¿M+(Â—ÖÌ\¯ù¥R‡„îY«¤”DP	y?Œ«l®{Y€FïS%6î¼Çâg”=­œ™¥2=mT5+JÈ¦ÕåyC¥´FÍÖ10nä3.é´DuÁCa5âÂÎ„˜	 U¼»E‚)ß–°S00JÄi­™D«&ü°‚Y5“!œ†ªEÅûÊÜBPõÎıßbf>z½O	›ê¾…Ñ	Ü,|Œ?ÔâTœ3ş#ÕC#ÍuÀï*evƒ»ˆçzEÃd5 @Ç«ÊÌÆêyİ—
_Ğ#	Å.pA$•¼Ú]˜MD‰[¶gR8!5úxåüuh5Õ×³ÒÓÃ[¡ÿéÇ’³Ëµ‘gIŸq
IùÕEb|İè¿Â©˜%ßS:Ó<’
Ï¤,iç¨i˜ q“ˆT'´ğR—&d­¡çFYr)*÷!óJX2k’T@i+D‹`±)ŒrpÉº·“z¼¬¢f§¥ •!G˜H Ãì ˆ"bmBßçgÚDÍ×£‰g—¯Ÿ,>oGfÊŞç`¼%®¼ä¼¸‰KW–nvÏ_î›¿ùFŒ¿ Ñ%šrË“c—¨Ì 	r!Kıti9Ú®—0è†·‰¼o`9ÚGU;VúÆØÊ­¾äË1€é½ ú]©î'ÈÿúéîÇâh>RUwìdıñŠ¯j·ÊÏ\\œígvÜßŠHŠÏœ%ÿ›Öd¶7ùêºExyµc­j¥ùÅIßxÈ{Ú`ş¬Ë´ùã€ò6‡6`£¤íÚõºÅş¾×ä'hÓjz
X>ôuGdÉö°:¬ä$«”µÀ>ÜˆY€óŸsšQÎ“UÖ²\¯øxXf*\b©aÜ- ÷ŒªHF¹Û†o+XÎó%ÕJ>Ç¨Hz”ı˜g­âÔµµ+³…Æı¸dö=ÖY”™NæmP.€¤6ËŸZ¨Fñ†úæC5>	¨o¢û¢Pÿ'àVÎ¤ç¥u£k ÿõ÷ø+Ÿa€İaPc¡Ş?$ûÂxÿmìibäYâŞÕ¥şÇĞhâÍk û€³„À~jäzªï–&I>‰A«É+±•ómØ¿A}£z˜$ßÔ1Óç"yXHFu h?Ÿ%¸?%%Ü(˜äA–7Å#¡£T¡Ïu¡cÚ™¦bÛé:^/8»tüd<&€ï«ı~«{âµ`Í$C>Y¢È7€}WØ×š)ë‘%5œ®u¨W-dUg\²ÏG1™¸E å/Ü[j»ÍË],vN$£ˆø_?ÆW¦:–.^ç‚‰ŒôCíog^â»D;Ÿèº˜è|¸pnBÜëÕõ r,±öª³¡’QÕ]&¯Qñ¬åXˆŒ‚I¨ˆ&Ü…,y'Š¥ùÎaÕu,[ô¨-ã–ÿøÀı¡ªš†- ÷{Ë²Ã¤íõ·Ä{“¯½LağY¬ÂNK‘j†ëíñlšŒ|İ/ ¥)‘FŸ«+ÈK2{]­àf|$îz›ÃNŸâàŞèØ•+4tSpåÕH> è‹*¨Â-û;ªœÆÚĞ²ìfA9 ¿õ£ø÷	ŒaZËr'"À¿Ç}CÜ±ˆ[Ê4àyÿ©"=ó•¹¹mo‰¢ÿ5?…^“uEN1Fç%o¸} >‰¦JÁÍ ¿âúdÀõ^¿? «åR.Ì?d˜™OiÁM"	1¹©Işäb´Mäó)Ÿğ7Ñ}Qàÿ½¦ÄŞPğe=ÜÈÊzXû¿?°¿
k«vDRî§qÿaÉïÂıƒKçÏÇ'z@£o§g–~~"æáï½…ş•¾§XAÓÆ–£×x¹Òô_•;:×Eş}*ôã@i›?‚O¦•4—îcá‘ÿ¯øÜk‚BN—6ôÆd×#¦Ğ@~Ûd:J|ZwípÙB.}&¥ãeÒi0=\†²¢e<Şf@ªù®‘; „)7xàC@0tfé×…Ñ7íãZĞ øw@î8¦Ot¾â¾|x}áÖ–jœú912&á0±qİgŸê˜ëXF³Ò7¶<Õ€ü£‘Ôí(yç‡;è/_K^¿†núÏ®8ztğ¼ÃY¸ï¿¢õ/*+,İ<ÿY÷=ëqàHÔ1EˆtœR7¢” ¬GäĞrn€ÿJ+Ñ7× ÿÙ-©­ì0€&ÖØáã‹¿O†(­œ"ıy:Œœ$×†v¦Ù‹jüèKÀ®Sğ§~RNŞ°›D²±U¯âöº0—%È$`óIæ¾É/5<nşA:Êrõ{ÈEı\8+gŸ9¹e…GÉ<RŒxıFŒÛ]oÈ?Ä½‘h`‘'jT7SqR0ûaRÌH­B›ÈWŞ[ñk¶—ÏÇ`Èa:‚ Ò.ÄşøL!Ø
²
(Rñ¹•–ÀöDÿZ`Ñâµ¦†R“Fƒ	ô+Ô¤¨rH(±Ó)»\{BfCÏìD±Ä_Ã¢œæÇ¶:À{DĞ‰Gîà¯~ğƒ½)”×Yå—UÉçf¯ë”fù_Ğà³C-èlU…bÛgï/Ì¿İ™êgùX÷s0ºğèÊ|¿I“„ã«şkÉŸo&‡ Rbis–?–_DS¦“Ï§?’ ÷°‘y§yÚ	R¼ºæzy¡¹íIbx>Í	Hæ® ‹®í„Lâ'ß
Ñ]ßùaÒ=zÑğJ6L<kQwˆr€¼€à)AÌËä¾÷1·ñº-ŒïO¼y½4ÚşvúrâÊ¯‰¿¾=L<¿ —¨å|bbŒWœµ:ã³k`e Æ@“§îá/ˆğÇXjt85Ü¶2¶çèLr°<yğWÎ"‹:û»F·WäüÇêªêNV­ª«¨9ôáÑºÓ‰ÑòéĞš²-#)yökuq4Éİ—tÜv^1:g›A@Š1€Dìd—KöaŒ,¿ê^E‹õc®ñùu®­ÁØ÷²¼,édıDÕûù¹N®Ú§hor.ÇqMô¢ätOë|!]/¹0|2¨* HÀ”& ,ØCŒ¢tÉÛ&›Æ»{?0Ş5ó?ïŞÔÒà–G,®Cµ÷¯¶ûŒÂB¸‹eé;/%&f£¯­ƒ»/R—G’÷ ÓÙòËq:†kû8Aï Îøáö8	¾àyj8ûºöÓœ¯Òó<Pø>)Ã1Ø{_cNFhR”á$ÆˆÖÀ“ÆúéyTsöL¬ÄäLm=$n…½HsâòÉ’Šş &Én D5ëó6ÉƒB›[-…ÂóS÷&Ÿ'.¶-Ü¼—¸>³x~féb/úµ-< a\ê[øyò=Ÿt"ÆØ”ów“¿t&‡¦§)„åEGêNåŒvdÉ‰®•è¶Àõ•uU[€%HZ’>qbôÉ–„–k‘Ò9Çû±LEH"“Ğ™vÄÿ³ ªÇD`]…?èã©<FÍHßo]/î½Ã¢'‘!…ÜäÓ„ì«ŞŒ_ƒ<qÔĞ_Ÿ‚rNÖ×L!ÊáB …Ó5TşºeÉíÃôdZ=Ÿş5Q+BÿšùŸKä÷£¥ÅÜŞÉâ÷±šÀ¯Ş#a J¤e³‚ºïvbæò©=·‚·oùu,ùÓ4ñö_ÚGÆØw‡ãr£Ï¼Æ‘CA§&±ÑN™¡!Ñ^f9ÓOé”÷)isŒ³5iÑİÀ›{ÄëÆÈl¤Ï•xbV%/‘âËZ(vˆP¨D4¨l)ƒ—¦7wgI\Fàúh ñüÂÒ‹ogÀ¬_|ötaê<ˆ›Õ|'¹zófjº-uwlåräğçäËëzàÆDG2J´(9!Ö?oßÁª­ˆ¸¶— —WÇ¼EÅ˜N¹%×ÉÄ9E–hT³+ÓÙ•šE:¹*6JÈw\•ıŞˆŸ¹T°Ñ^ôÈ!¢¬…¬·¿É-²ï#‰½ŞÁ¿«/mc×y¦ÒlV ù'õsƒYÔõ£„‡§1*h–¾1
¢>Ç+úd¨şâ€qx‹±*!¥–)ƒŞ§€jG”U¯_˜p ³E
1ï3‰˜MP®ìP§˜6Q¼ı¥vÀ>Dè”úš`êï'˜º^‰pÒ”cRÈ›•DyPÉ¡L\éG<45;?6
m.İM L{Ll)#©¾T÷`êò¦Qli”üpá\zsÏIf×˜S®¢DèùV|ç'Ş”J'"²«QJ]ÇCŠqN*#&^Òç¨ÁIrÂ§ÓI˜ÚKotV‘˜jæ—ş†…]Œ1…	ùPÔˆ7À™YH
¸Â ñÌã¥°DTôa5"3Eõ6kŸÃ×´$úJCDüÏÏzßNßĞÎƒ-¸0¾]]x84ÿlc¯¼÷ÛtÏÒík‹ı½wÛ8ÉÏ|tlşÖ+D×‹ã¯ç'¯‰ò¦~¸Û©sORw®iÊø“äO9‚\˜ÊhOOèAÚVÎÀ;,5ÔEáípçøü
X¢©áA€û¸
RÃ}©®­ñRg…Uof/C?µUí(Şª8Ï6Åè‘Ñà(‹ŒkKÜÕôõGy¨F¿ÒQÕúĞß#Ò‚©ÂØı)*FPríÕq°PY]ĞŞ¸Ş›"H¯)?Áx—#Ü·Ï+8j”çuI.İËƒÆ>XĞ‹8ÿîĞÆÓ~rúLÍõ–s‚Üzß%¦½\:g5~Ä—äñÏ.Av^£4ñt~mƒÒÎËßhóQ"05HìÓÌê/ÀÑúH0ˆ„ŞT#•¶•Ğ)¯ÏJÇQkÙ®Ÿ™ûÊmì*w˜­ÊhùïÄŠ¹t+…ğãş9áï%ND‹©“ÃXşÛÊH“ Í_j³.R¤k”Jã¢š7cŒÈƒtTG,+,Dw¿áÔ…ÙgĞ£L‹¯Ãğ¡Î59B1MÁNQ
K:^zL‹°E†ñ÷Ï^©\°óQqœÌKã4F¼¾0g=Ñ"ƒH²Ò&ƒÊ•JĞÚ ‘©ÉÄàãÅ¶öù»oæ‡¯'.¿]Ì¨üæLbäÑüà“÷à_'ê’4ê¾Ğ“<7|2FaĞ®¤ú¯±Ôİ¶•ŞkTgtĞ8 ímU'7J¤®ª¡®ªb(ÆÎB{ü¶oY¥›A*H’ñîÒryƒÔ3œ|œ3ˆR}’
È‚%,`õzUr)ø™SëÁR"M¾–l·ã%_ç² =?]ËBtWœêF§¿áşò(÷‹“'şÕ¨IØFE(]´“OL`ñâEQ±)ìòPRâ„ÕHàÔ'â0[çÑ,ä5ò?ò>Æ"€sAŞ/q<t3ƒ[ ü¼Øû•ÇÂc!ş¯(­ËBàûvæêâó§€>,Íqî)fa]¾É8¼øhósi¾y`÷LŸ×<š_y”çj—Ş”ùã¹¸³a:kaœïì¦ã ƒRØÓ"µb@'ù¡ÓM
– V½§eµQíj%ÔEZ¿7móã7–î¼Y˜~º0Õ‡¾éÑ—‰ÑX:$>³Ô~Ãj´íCåè àTJüéI&:Õñ‚¨=¢}ø™Xl[Ü¯T×¯ª«ß„[†‰€:ÂuÚ·,(¤?M•£![-a€ççN¦ƒUÓñÑ­%àh@¿±[Ö³²T)Ğœ!6Û¸3À¤(/Ã¤\
ş›{‘Ë’ÆÇZ9°¡ªçœì‹â^jÊ›òaĞë[,”uªùÂE„8](g•×w&Ooc«É¯xß÷›û_†>¿g;.©°Õx”Óüöúd‚µãÃÌŞD´PM(Ú`Wÿ“"şFY5qhˆôÏ¿ºÙÛÄò~›ÖÉ:¹•ÑŸş$r{!1õ†¾ò†=ùy8ğ'óvjo·áµ·z`Ø‚8ÿÕ°Ñ‘XİUú$—l¼ØàxçÍ¬Ø<wí6~‹ñ#İÆDñ;CŸv)09$ØOË8—ì€Á§k¨¨l¨®¬¨9‰à¦¾Ù¯ŒO\~x7;ğï&&-\èåÂwàÀ‰41Î†îPeØµ†×ı~·÷´qÛ'…Bµ`åÈkòAñ‡Ìn
ÂJ±µÚŠY‹ÎÅ›”@Ø’x°èõKaÙÖ$¹e›7gÜƒá>FÑ #ãd³êuŸ…å`‘Ñ†ŒÏ+Qv—Ãò™°ík‡=xæF¿Ã¤{››£ÄÎûÛˆä(‘ ˜Ó.ôû‡UÉu
,![‹vZ~‹6bø½QQá%[£ş‹ÖR‰İ¾ÇagÁF›“Æ–ŸÍê7ı-–VS«şg³´9óÌˆë~˜®@vÏZlÂæ¡Ÿ*ÂzÙÍ§±±Y´b»==•AÌ×c{Lµ™û×½şÉßb½góX1å¢_gC^ÿ!¯äSš#r nZ®yu'Œ‹$¿>×’œ)s2ù¢<Œbœó¤±øíµå¹0Ç¹Øz°^q9‹ß%OÊyFĞ§…ª£üé)ÆÚGRø†ÎÎÌ‹Ï ‚¼ÊËËÏ`qvñ®XV½.³},gëÚ_êÌ6HÃ–º=–ü¥“øqÚW.³ÔpGªk˜-şœºVÎRS#©ÁäÃi–ìiÃ3¥sğÉÑ§©ó—òæ#•ş´fD¯ô¤ºŸb¸×ÀˆùgZ·›¦[Ã+¯òˆùÇ¡AÂ¼îÙk‰‘G‰‰—‰«÷æ»æŸ/Ì\Ÿşmúöü¹'‰öA¦¨láÜDââÔâ»‹—F»_,İÑëôL&îİyôhŠ7M¬¢áîqŒ~HVÂØø(.Ÿ×uŠIì`Í‰*ŠüÅJ°>¥ÙåÙ?”€ljåîßcNiìß
üßí¦tß*½{Ã¸Õ…læk.ÏúÓIš¸XÓÕ¡0lÈ.)½|vÏŸYMÕå¬èL1;\W}ˆå×ÉAUæ¼Z<´³Vn9ù½Ls8m.¢E”Cî ñCŒƒ>Ô¸º›ıy©1_5Ğehİ†=>ãËü¹ ˆOBä–µ]ì+±ï)¶§7“¢3>´•jÛİ:»ÓÎrÅ»v>¿7`óØ¾.*ã[±
0Ã‹¾›äó™xÜ—N7›\)§½rËAåÌ<;³3Ü&‹ìv³«.gO¦Ø/¡¾S_©ÏÌ³}m?i?é„—OªÍRş¾}»³ÀYì(°:Kv¯Sdn4¹(’›B)Ù¬Jnô@`ÒS³OiÑÈ2Âè~0'xíwCûfŸnÇ‡’|‡µgP“P—,°-}ë!*±ÿo¡&Z…›P[0?%¸ÃJPhßUššBr£Ğ0LÎV¢>=÷‡Ãá**oïXIŞ0Ì½°X¤5SÈ[¿¬b‡İÊA¶Ù÷:œ–ò¾’-åı{r•‹@KAÊâª'„+ÇyK"C†lØs ÏizÃË#{›=aÑV´q9ğ†Cò"!Y­GÄ±À‰¼Eº@…Ä&ê(s"Œ¨avúOlŒQRÈ ò\Xªr
@ÉJ‹JK›V4ö—%{¡Ğ"Õ[ËZõv!M-´ê5¡4ßÂ&=_< +‚K	Š-AıÏrŒ‘Á|csŠm­âMhZÌQ,ÖŒ®ÇÄÚÒ8Mòa)
y¥ÀA_Ddq¹É§½”Ş PlM‚ÅñÁĞ4ÂSmĞi’±n˜‚vršBMBË CéÄ{”56*ŠÏAÁöDˆ†è‹HÀU!ÔÜ;çódIppNccBA¼-&:{Ö‘AY<*«Í²ğtQ+µŠ[æÒ˜‘!±¯Óv½Q]2 Ø ÇëlÇS°ÿĞmdf[Ø¿‡»üÇĞÆg%×©fòk‘3TÄ¥pMˆ¶‹›w6e¶0(´3bXcÔˆêËÿCÚ‹²Ûls¦Åtg2èÊšŒ,›Qd:„ˆ."Ç?-3¿dŞÅì£GYõI­,äõy”ˆO(²…¬6²ØŠíÌQdgÿ—íµ³ø½İÀ¬ávú­L9ş†tÂ_ÿ!:E(n*.•p6“m5	x¤¸fæà(ßQ<³EòRGGtJâp2wk@ò{]ê&«<´„NªBğ
°4‹©ï³«bN@DÍÄ
ñ+Ÿ£OLEê£9su™ášÜËigŸ	5€*Ä“œ?Ãdˆ6Vl,ø(÷â£Òº}F«Èı™•–lı Ğ¾¥s8œâ-ò]Lo²d¯x‹ŞĞ-–SlîÖ†

ñÓ\Rà—Á­ÜkÏvˆ«*Éçk¥SuPL\W‘iEÜÚÙnXÁrw-LAn)†û!%J•pˆI!#aUòá‰[+Ë?í(+t8
¥Æ¢0s/Œ„mò$_>Š#†]‘ŠôÑÉODckÉ#Ua™D™h–<&[ñD˜ÜfÉ’C.+“¯„‰héÜ++f¨>,Ù¿‹õ
4ºñ ìÜ+½Àx˜ñ
4h<;÷â
\ÈşVä œuÆŠ˜MÔ½$û`g’İ5\sâS‰µB¤MšÎ“(¼+­ÛG¡E¥€Y¶à•ÛOVóÄ¦Y·…ĞCé9Ç–<•¶‹`roT°Éï˜ìÉÛQLÓjáSNêÔv“M­\‚Õ²yó¡5Mr„"<<	ïÛ?MÃ•Âº…¡ ÏÎÏ#Ö×¼İ_Û¿ùè^1ÅóåîoÍdèä^˜a—óUµéï´¥¿±
ÅÂŠ¯Ã„çœ¯«<²Î×Í÷õXñe˜äœóe©[çË°lƒğ—}g‰–ÛVª@³Å¶›2ØW"•H¥–+ƒ_n}¬Êà.¦ÙRí"bòıpJ¡V3Öyhı‡ÓocO~íNôL~@1r=ùÓŒF'½åÚ‚e¢‰÷™k#VèsTV˜øêf5—1Ï+\é|ÑÓœDË `¶·Æ,€/Ç˜ÖU™™»À_½FüI½Äl§Z¯“^4“^dW#÷¢,òtKœ¶2„1ı<ıC£ø¨ğù¼RÀ%SM–ŞòÉR3rPèı/¤°Ì*õ¬+‰3üx!1M¼]x4õvrdaêÖbûÍ…;÷}ŞNNòßÍ2|ä^œ*½÷VòÑKoOİéÕaÓ’˜	í—z Õß•º<®ñ}¿èî(p÷w‚®M¾qI¡·9à§ äDt£``Ğæÿ	1ùŒä
³#Uÿ©Ÿö“7ï¨än—™}÷ß°#Z–Ğ‘w0Uc†ÿä Ë?¬Êr _à-¾ µŠ:`y£  íMÉÏ3ã,ØRyÖŒ‰ˆà•×Ø(D2£MªÛf´‹
K„ÇÙQX"ä05™j{í7›g¥_§äÖg3g/"Ê¯ìo¦ôK,‹ŸgcÄ-k”=ĞOÚE³±…b‚µoÈ¬ıéOÂŞ!ÑšÌuæÀY~‚-ÚA¼@TZ-kŒGùˆãÅt['-2+:§…YÓ»¬SàmÒ=ŒrfSh-µZÕEòX3àVmÆxY1Ü«¢‹œ‚åéF)ÎˆÂÓ²RL‡e_Yû@P¡˜!›|«M<}/Áø\ÆvAq¼pOi€}„ V…°ÖaœFƒ5Š5€…êßBåoê·Pñ‹wËB¥o¥Êï˜UêŞ*eÏU}ZK‹w0[ÁZ1}VY*üúœÙ‹D~•CSç6)-ÒŠÁÎ2ç·MÇ¾²LÃeµ·z—Ù×Ï~›kà1w„—ó²Ùíìg³NÒÁ3HÄãŠ¨!Eµi¨ iq´—Ür“ñ…ó¾ûVü9•ÀQ%’«ğ;œÍ7ÉD·ú
Éá#Ù¬†âf^¿¡î~e¬yqO- ¥m uÚlÖÈÒiy»Ï&òŒ~:ã^‰TÚ[p¶cI„¿rô‹5ëP4ÀO¿Ä£høÅ=ğùL˜"­é¥õ¡Gúe·¨<Å–µUşŞúË/‡…­}Î¬Ayüzß=gLHf_Ÿ³RKÛ+gBù™6ŠúµQ Ö¨±ÕËñ?ÿÓ²10Q—q½Ë UñFbn„·U¨+ÈíÕ]ÒR«„½X¢5l¸`²ñ*Q¨b†:ŒQC”—€ZÅcŠì´ßüüKh)"©îtŒª€°ÄÅw,+$åëU_ÿÍw»,|áW^(ârÉ!3Õ¢Ö»,™J‹‚áøõ¾¤Âj¾ÕìK€Ówíõ9û6~?>N,¿ ¦»Øµ¦Î†ú¼¦Z¿¯tF_û3‰Å-G5_^$â>üó’—ÔÓ‚ƒàÅÇñYü7'ô§ğ[±Æ·‡2­¼ÜÆ.vt;¯J	€Ú×ÊÂ°—P­à°’&pÄ0Šû‘İ¬ŞÛÀ@¶ W•C…Ö-.YUu[--qG´	ó)Òú…Ç •0İ{È#8-ğı#»¹¾+yö©Î‰¬üØ=`G4ØgÌYbY	 Èù2îr/â…BPpyĞÑïu»}œ›½û\NKvâ¿óóÁl’\Ñ¯wÚœÍJ¯±=8¾–,ÿ£Ş€·vPé.)Hu¨¼P:ëxGlpmk}P\d¥>Ø+JÌ¨7÷•ÆkƒÕ1¬:HÛ†^yh¢ËäYnaD‡¸Tlµ^O“Ù ¸J(¼}Nîµps–¹vÁ~©?^ñU-ûZ3‰³½kÌ’[‘·ç9ğ½ƒë¿çÄ÷*É?”÷İ7V$å™ìõÂ°rgºf:÷6Ú7ëÑ$ªTÕ»zXf©&;râİçÛ†šÍ¿·êQ`Ûg7YGaÕZ h –øvicE–4hE°mãu¯u;vÏÊÀÙfÕ´5n|kËÒ@Üœ¸¬Or¸¬¢8×/Ã|ğ²20×ª![EÀ¾×’à\½ÙC˜Şª*µÈ+b†«ô­w	GÎâ%ÎÎ¯_îg¿=Êşx6³‘àiCÚ
+ùÕlô.ı•õ.}:ç]‰ğÚY†¯œEá°Âøİî’ûYöNj[#Š¹‚š½éşLoøQ÷7~íø&¯ßQÍÙPÈöNEûÙÏŞ©¶×~vGÆ_;2¾ÉkóJü÷„xµv#‰èì¿>iÙİ¿§Ù‚Ìµ-|ƒÿïÌ†˜ÏÕC/c•OnF½ê9¬Ô€[UB2q® õ½~X§• áØEhğÍgåSRE“¢úµzŸ–ó‹JJ
XI‰@ff–§R÷J™NÄ¨;âòª.Ÿ, ªèÙº5ºÔÂnúDÊ­aÁ!^œqÀÃ•"ƒlR¡‚hNÑg€ÄaG.V]gÄ§µUT´ÎOk«¨d}zrT|­ÍŠä£#³ô¡ „ÙŠ
lEÌ^ 4ÌZ5©¤©¨ÉõiŒ‹…Ã"ZäFdX#˜ÎˆjvÔBû¶he•Š™%9á@>ÉuJ¤1+ƒRDì‹Zó"¦&˜½Û¤Y´Oèt³Épˆınïélh.5†_$,³F%Vü6'óÉMaXÿÍ6²lûJì{ÊJXğŒÍYÿ´ÚŒªçÉn›¯nS±dÿÇÖâñ†å=%4µ¶¯÷Ï|Ãcü²+âÒË¼Õb»=+,¨É'Ÿap«?dsÉÔÀš¥ lhÖBFñ X›Êò›kØV »ç-Ô¾‡~êjŠø|Øsø2zD²‰ô}…„­Ì¯Ì§àI>SÈnª‘–ŞÄïÆŸÅŸa6ò—ç)>oà3¯)obiTü3N3øÇ(q©ÎÌuÏuòäŠ	xor—È£•³<ÊF:J)ĞÊÖœ¨Ò
ï(ÌM‰¬U‰d*í€ë’wå™6³÷˜ŸÖı{`í™¹U»ÑÄTT­úğ‘†rvè¯µG«+)Ô©şxU%;^Q[UÃ¾¨nh¨:Ä±#ğ¹ü,şY]SeÒ±ZÁøšË]ŠÏ†Ãfs8Ù_æÏ¾îñ‡ı-
{›Zmr¸E–ÌïØ<¶¯‹Êì HL.ü³»8éQ$}n:‰—tgN•?ne<=¯qDmL¯Õd‡q3+\«†Æ±jr´¤>bÚŸ¤_µßƒ¶Ò´Z+:CZM×Î¹:Û-…<²[ÿËË¤YAİ·Çi·NûBç°/t-Wï¼?wÖ7Ùa1{ı¸O#˜
ço´	•°İï)É!ŠÄ/<â;™ì½	ÙD6T–juòäP³çäÆÍ`ÚÚè\wüÑ©Ïˆ5ÚÇ‰ÚÊcµµU• YL+^Ô‚#iÍüí@½²”ø¥3¶”˜À„Û†M•}ÒX%iÈòÉ‰Á]˜îQ":qòÉ¹ósm°A“\LÃ¦İ…yŒ´ıOÍ]¿Àı>EÉ“c(-¸ÅOÂ?m˜ã[Àæ.R¾ä‹¹n6×>w‘šÅäàÌ›§DÈ7$dÓ” Œ˜Ä={^BZuzqs5°}„!ÓØ4x‘Ğ¼OB,í˜{9>×|é€ÿºy;Q¨‘–ÚzÉE›ßOÿöæ •ì¤P|³P|Õ:á°¤6ËaD"Zø.§aGìRtˆW4uÉ B•&bŒ­•[2´ëˆiş‘ÕV8#CS­LR]¨Õ]áˆ*°U" I'åYÀ”ğ!fUB;¿Pháª–XñªíMÇı-‹óÕøµÓ. ÖÿîõM§fº”¡fXG²4E‰n¾ø•€’µYí ûnÁŒ¥$Œ½ÆvÙÀ^ù^ÇHrÂíÔûıûZ#RÿF¸d=cñd &É/£Aæé6VwŸƒŞa.`”pÇ‡â?‚M5ÿşÚâ›6LÒì¬-_´îÚâU,Y[whKã›év^]Z—­\]×¡óCñÛ;ëjg]iëŠû°d]]'`H80e¾sû®,­Ó;+km;+ËÄ­»²x™KVVN%·m½²´Nï¬,+o¸y7˜Ù¾İŠÊ†ê/«X}Uy©ö9öeU]Õ!î,‡—Õ±CUÕ5õbŞ]+‚é‚¶’\Ç.÷æ®ë,^­³t¯j“ä–mŞ@–;Ëô“…Â­>ùÀY‘À^ô27S—ˆQ»\h	|«67Jù<›qû¯®§î®½¬È^ÀJ‹˜½°ÔyğëŞĞˆŞ“0İRŸu8ùÿï¼§IQÑOŒ÷8à;ŠÌiÏÜ“ç,…öÁ”j/~Wğ­`fgş:"Ô %ì0 ØÁ"áò0höÂâ’İy»E¨ÄøB±@Ä²›âĞ'/¯´T€"Í4™˜ïı‚¯5'6§¨IPß}‚´á‘ÑáÂ|]¨^­_âk‚_«W†pİL±ÄK °Ì2Ö¬Œ\dÂMŠ“²ĞàœÍc§	üŸ¬Ñ!ÂX|Õp‹ßˆ.…íaY¬A¹i¥D1ÌƒÌì™¨,á¦õë§§7ÓÜcâT©–‚i~YŒwø•WRbEo­ÂtüúÄö¯õŒ Á&-¢5ûü=ñG[ÄßÇß_‹ßŒß_³¢Q°bj*jU×o­ü½ño[8‚ı0zšW+>†¿‡WîZ4–uUõuÕ‚AExYæ4ia3áIN»ı=$‰c°,M&KY‹¿ÄĞ—ˆ|QÀŒ…Æ²¿•âc~ßfN>ÑáCV”û†aû™3Œ,p–g….º­ıëŞèo±^VyìXİ¡úòmg‚4©ä§÷EçêDõ¡í7AaÙå9á%Şê²æ¨Æ²`>Š]Äxò+Ÿ ‚¾sİ­ş`æ ÒÆãÚe¿¬J>7e*²ËÕmú›%ÑnõÑ+×“{KV™±ôºşÕÅÇ“ÃcbÍáÓÃŞbñ{€µfı•ôê‹ÂÿwâO¶‡Y;hûÖA7éãÔ4Î	c²¡ğğdB‘¦¦Ğ¶Ã'VTáÊtÒ„­šß|äšn3:D•7ĞøÆèüÚ¡7nßD$¿AZ?*LğûLµáõû;ú#WduGüş¥Ó§ ?¬@JOŸ«·
Óù{¹
L³áÄh>,ğ‹[è‘µÜïœWT$äspç
‰¯ğÁËï»l|vÍ÷¡o·è¼Å¢Ók}üB~±méŸƒ?‰[Ö§"Zâ?FßñÓøü×¿ÿ‘_âñGğÒ}ø}ğÊÂúü9,î£/gyXYQ£ı=v¨ú‹êª:¡A¶`ÿZÇ;¼ş¥c9×†€Šê—|˜Íj/,ug­&YÂœCQ d%² YÒ#$Är.Á‹¥gˆé L‘Z8û#á°İcJ€hCœÍßÍü»°w‘’kÓU¡™øI\Ö [ZLİnÉÑ^±%'xï«ürXÒÎç¬Ä’vŞwo™Åùõ¹EÅa°Ï{EKö¾
îVÕn®Gl}eîß«Ôáx×ëV¿æ/ÍµÍuÑ‹o´âì5€úmJ 4›?è“Ãé¢ì«#íŞKivËŠ²Î‘`øNNæ§3§IÔrÏcW¦½Ó*Óˆ~ÈâçìÍ^”›ßM´ññ*’5^k—W¿+¹$ŸlûÚQhw~£i-ı5{á¾²o¶…-hy”Û'cşëŞí¶O×,¼	ßõø3²õ€¢ãOH¢^xFá3øœæ`Í±Ê¿°?±CUÇkı9ÒT×k¶¡ IÈ1¬ù©Ix£'Xu"²9U¶şaE6ı\8s.²NWœ®8DOW@ìıtÅşû¹nb'C‚%¾i#İâøÜ%Ú´£lî
‘-M°¹Êœ»€û{|Â‚UÀ«ÏV×6TÕUVGzÀƒÕ$_ƒ×—‚oŞti
‘dSS¼ˆæ¾ÏÔm&n2„”¾Û¿ùÔl£šjÏŸÙAR!*ÉBƒº¸€•°R–ÿ…7 ùØ¦Ê`ì6–Œ»nÚY:U–
pØZmÅõ”A}´ö„¹8­>ü„F#8äên•Ìz¯ÈÎÖÄjoe€j~=óñŸš
†e4Ô¿îõ¶ãÎ‹OÏOÍõ¢şá¼j1LæGÚ5âT#Ò848Î!oÛZû#º›™ÖÔq·µ#À8)¸Ñ$“»Es*YRxÈ:£ï™dœOnœáˆ‹–(èàí™¹Nb›š!šÚ,š7ä†ÅÏ¼ï;ÇŸ¹ç¨Ñ¼¡ÑÛÒ3LÂÿSoİXD:CĞ–, û
í»MpÆ•k£Èç£Ù… ñe9cˆíbn)ˆh•y@ÀŠ‡
˜Ë£x]°×‚5àò€|ÀKRÀÍ|
 wï?x;§äÖ“T™y6ç&,j¸X‹7ìYİ• ª„Œä2ÊgBù˜óè	ûğÖñ.ØMXŒ{ö°ãXÓ‡t“"n¯Âä&]4 ñ·V[M9%¹#Ò>Ã°€Üb"b¹"¥¥°»‚*È¬k=Ÿ·Å¤rš%ô(l‘OyÃÙd¢%ØµÌøø˜*!—I¿.ÌF¡K•ay¹¼>XÔŠjîIà
›Tùï9àj-iûRòEäŠpƒ×/››iØQÌYºÔ©ˆª‚ªÀo7ÓˆC Ÿ	*¨­`	×Iş`ƒ"< ¥ûŒÙlX¸ŒÂ%m‡%oÀœœ56Ã½–˜½Ğ^öñIg ôw~³ÉáÓïÇ>` VÜM?O(,©aó:#VL:Î·Rª¿c.)ìò°|y7;kÔä2ìËÆîgl¥è)ÊBí 4ÿh’ÏöÄfÅceøMùss¸Ükó»¢øšl4ûÆ å–ÿ¼p¹¼¢s5Âçl+€Cşn˜BÉØ|a-·öÄ„ÃTã.8ƒĞvÓ~ínú£›?÷O‰`•ÇjN­-gÇk*şZUÇTÕc_V4TÔ±êZvèDEƒwhØdmˆwÖ‚xW]İ/b˜çH¶zûƒ›4r\®uf
ß€1¾O¶´9õ_$#üDK½ØcâƒÚ(Bµá\ë}§*ÀğF1à{•fĞTÁ×ö“ö“Nhä$Ñ[9‹K
%eG½ĞY²û›Ì8¡jòÁ-¯Û-ø¸­w´“¥sÒ@Ù?lÎM½òºäé:ãä0Eeõ$Bü@h›kh³ëiƒ0ğÒUqP¹\Œ½ˆôAÆ‚ŸÃküláålMU¹Ij<‰ø›n/¿ÃĞ7kıû×½;÷‹õq~gkû#²ªpmß"©ªWQ7¯“×Mâ7ºk {{8ş4~vŒkña–ÿ>>„å…±†a>xÅáZPTù_UÔÕU«3p{N2}f<Z_60’kñ;80·aH0c˜Ò¹†â·Ò©ÁĞTÔTÔUW²üŠºJĞéÖ².=…Çˆ÷Ãÿâ÷¡SƒĞÁÆ;SUó×úêõ,ÿhÅáªİ›”y;,n‡‡Z’ßëb•¨mX5À=Zí,¿VÑJ8C»iÜô~”»Ó!‘*üğ»Ë[@íã¿[Ö~òµ³°D•ıßd!P<?+µo¼ålEf¶ŠU(ÔQÉj!·³Š[R1ÿ'³:ì9;‹Ã¾y%ûnd(D&ì+~«ÿò0;¢yEy¬‘3‚ı¡ÓF"P×bÿÇ ·Ûi¯ÜrPÁz“ÌÎvúßÈıæêKŸñû¡yp8X¾gOKKKaKQ¡¢6ïqÚafa$6ß˜!m¢·Æ¦Ä`1NW«ñ{ÔyÅ3h´ÙÑXÜhäVClÉûƒRØcè¹ ›u–°{%ü,²³¢æ´ÃŸğ³²”~ßK¯ï¥ÏÀOP-{øç^;++9R7–áŸNz‹šúSCSZ¼·¸l»Mq,²š'ıÄîÕĞ+æz(7ÃõşzøWŒÃøŠ)Ú–cfJ*`C.v)-û²¸ìH‘óËb§9y°79ö:¥mÖ7”rç—eÕ[ºˆxSÉ>Ùnh¯*î4VyøCÌ¶Ûì5(ñÈ¶ÓÅ&4†ÑS5,
éuÕT¶×±×P!û2j¥ÿƒGM+E½¹ïşKÛ(è¯ Ûv0ÿæïÚÁük.ó˜ß^Z,m7`‹@½„0¿1¿q;üDÌ¿ï¥×÷Òg8Ô'´?÷ÙóÑŸNz…šú„PÚ?„öí5{õßKKjèusıtØ÷•½ÇÉ\?ËXqie1`YüB?‹KMÂœ¢bwÑ¾}0Ç}ğ†§Ô‰ÃSRFSü)YhÈ,#­ûÿ  ÿÿì}ër×•î«l3›Jˆà-4<.Š’eNt‘öd"»è&Ğ$:hHâ0®²$'NF•ÛT|2ÛqrêÌ9u(Y´¨©W ^a^à¼ÂYkíİWt÷^»R”-¸L‘ ú¶/ëú­oå±gJ9íó?oÛp}ÃF-×V,¡ªY˜ 5‹±(ü‰ïäuÈVW×&×î¡8dÆF²‘|ÎZ·ã6°ÕUë…1œıza÷½òÃ“¥ÒøÔÁE4¶”»·‹ıMXÚ< Š÷Ê-¦gÑHœ>;‹¿¾YÊiOØ¥ñ™ÙµšÓlÒ’š5\RpIS=×1T§j¤§Ê¥Ê«G1Ğ8}–ü0t<LwX à¦~0±šËPõlÍŒÁâ=Sk\sêë¢ÜhcöœôPcMXŞÆ­šoÜÃÌ}˜®l–9­1kzmr­|Ä–éFĞoœ™F¯h†B3ãÓôÎ4½3Cß™¦Ìà´Œ$Œ‹Y8ŠŞ™¢wè<ù¥ù
?03ŒİŒa;¬Õv£Öq±Èªm»…qDpl‚€n\ó„È+ø`âĞ‡ãaÑ´E³á °`_µën»@F÷©M¸ŸâÏ!’0.2LëõÁ càÇÕ0¸›&+H¥ß:YA4#µ“Á“©|¹‘a®zÉ!Áå¼¶2ûX‰(F»Â*ÂŞŞü€¬AÀjòvş"‹+UådŞ›"`V÷?ºÛİ¯¢çà# ùGSôP¾¦‘ÚsË„Š¡ò|ëüâò?«6Ã§O‰S—æÏˆùóøË…‹âÜüyD_/\8ÿÆâ™·.Í//^8/.ÎŸ?}V£Pûë’gAf²UÈL¼.Ùëâ²gôØ¿JãìíÄë·>é]v5¡.®Ö\-Li÷­ë»BvİödLôCŸMr8)NüØjFıàip‚§¥`S'Ã¶)z‘ÍZ†¼Ö«'ªS}²·½Ñ'weÙJZQ
K@v?íîa¡tïƒŞ-I:EßO»ûë·å'Xóİ}Ôİñ‘Ôç¬:Xƒ-Ñ½Ç¸Â[uÇİdìáêc\8]–¸MnºŸw÷{¿D‘§
Ûwd‘øn÷^ïN÷Aï¶,-—4[$½ö¾ªx,>§Âò½îçÊXÉ~KÜaÈob…|w[R$ÌŒ'‹¥ñµ’pÎ –d¬KaF0ÎP
ø$M˜!mk$c’"­8âa±¾ÖˆÊ‡)S¢]m9õ+`0èÄ¨^"3 º\ƒD÷Öc×œ(7*vø®£(ä©ñÄ6aR²`k1í¬%ëªUK›mçb™Ó>éÄq¼OvZdš øËÒ`<>‚yƒh(üÚíİ„Í{÷®zc~(àKŞ¸úÌ÷9—”L=·è2páèÎß‹PUÜ‡vğú;½ß’D,øÒ]|€?îİá\VUÈgøÚ›})¶	Ñ[ìK:ŒŞí"CRpá„ÚC‹¸Ñ$-›ng: —Ä2²è¢ñ°»¨¥YJ OÎß[/ªX‚.Q÷†±é‰èãRDÍÓø¯RÒózÒæ•Ï8­W…=\’\_á]ªJ°™<]1MÏX\;·Æ.í™m0;~N-¨k‰A†ö/pÉKN¹rNW·Ñ*ÂğêÏ”3wˆøåñ	œğ¯aY¶›v‹A”¶D›÷©7_Û¤“ñàÛ'‰säÅ'½_‘á ?Æ{·u‡œ>ƒš–]%,C‰wğšM—ÕÁ+KÁGZTh\^·0ŠævëW\#¼*ıôÛPG
¿¸¼†Ûj`p4n˜{İkØ‰RQŸ>1RAjX„Èeô1Št#&uº©!zÓiVvÈVóílÑ¬¦Ø…™ğÌ;Šj·í7´ƒI«=DÍù›Ş¯Ir‚¢†íÇK£2äe "Pf"’=cóvm³İ¶ŠM&Ã½¡e#ÄqşWãy‘¬Êª;úÅF¼J‡W)«šõİc¤×Î §W3ÖïYÅ=¬Ş:c6	à„¿²\Q>KA>ä‚Œó]Œ|;¸ÿøb}­G‡B~Øô§ö=ƒ¨û„uQ°á£‡
r¨B×O>‰á«¾Ï“¼5’}–2c²_f<‡á#Š4>ú–ÊhF·	’Gq›?¢ĞË'°ÿ÷ÉqCÎOÄ¸IòbO†InÒ‰`p‡*<¼Å”O€¨Â—òãy’ÍN«Y³Ÿµø˜êO(‡ñíŞÃK‘ñ€º£<èŞ—›Ÿw'ätcÈg÷xÈ`Ùª¼P‹'Ÿ¸ „ğaÑ',0Üåó‘eÑ@„¥è˜.b›ißÈ¼15²Û}*úcPßP™âçx¿öb¾j×KNsjÂ@¿O™¤‡$W0=îYï×½á;Ôç™$2Äü˜L¼]2eÂ!ïî]xç.}¼OÖ~é¢sİZµô©s|âd©xö”™c‚âÚ»Æ|Õãp¾L=>¤ Öõ]ŞSüLÕáÁáş‰ÔX‘Å%GÏû „2eÜ@Š>
Rd2#FÂyP‰ªöT>‰êÇ·ŞFÖõ…šÓ¾Xİßáà~…®rÒ¨ıô­¢ğ¢Ø‰?»waÉİGİÇ[ÖÄo/¯'¡ÈÜI©Ù[•¥·Õi%àÕ­°ã’íºÈ}ÏM;Ñ}=CU7ğ—^ /RÏôxñxñxaô½4¡2¬EqæôùÓ—æ±ÿÓÅKÎ]\^ä>;qiñäÙÓâô.ÎŸ_BˆêÂü¥SK†UvVZ‡í—|3Ô5FŞ›í*0P§	ËëmCÈe‚åº'–šVëJÍn'É<‰åß/‡EUV.™ó¸/>§_÷Á…ÆõüL­†­îö1q<œkÁ¯:G“ƒØ4°á×ª9å`z§ATFıÂ Øå”Fu»ì"å%cœO‚FMåKcq“Jí	»vÕ®–¡XG´’ö|k0V±Ö;cB®’ ÇeÑ«¬Ò,´¸,vpIÉš,êñÓì´lñc»î0Êƒ`=¹N¹êHòÿ·ÇDÅ¶›¢n7ê*!ìM»­—`K4K«µœ«jF†ªV³ZÔ¦hgy£ÑjVö†ö|íj£SƒíÒ–µŞ¨U
¶[®‚*´ëvk}S´:u¸1ñ¦³^­é= ŠíZNÍ®Œ‰Ù‰2ÜÛ¬²¨!i3Œİ˜¸Pv-¸ãK6,¦Cd³ûåêaT·®:ëÈG[,ƒç±Ú°Z•âµL<Bã¸µã#†+Ùp¥¬DşJ3]I¦+Åx%03›_äm=¢“ÖÊ
znyF†øˆ‡è}ğ»Õ²aofÖ£³šu$ÕØµp^ˆöÛZÆÊ<.åjİÑß†Aœm­ªóôÅQL¯!2+èr3ìA^ã
­#šÏyü6›P’×Ü†úOtHTôNt¿Àş¥ùCØ-+ê­E1ßnÛnûmEÍ‹*ìÜ‚k—«âÒÅ30E6>ºlè
¡nc&Ş™ölX’.z£ãvZN%8œy½Q·jz³§QÛ¤oİeÊğÌèêÆÑ´Ç@Éğ¬12¹I[¡*¨«@·µxê¨ªÀÂÓMŠ®ˆâÃéE
'µ¯ÿ´ã´q ¸7"ü,U¢F+Ô_¹Ñ’“ºjÕñËVSX®p«úYpMQ³×Ü¢8]³×­º+Úe§°æˆ7ß:ÆÎğUN3ì±¶³^ÿ†˜YFKİh)›,U³¥h¸ÔL—©xKáHÚY!‰•âßRsKjÇˆµ4§xal|ë·Å¦ä—"Œ?¬ ş0BøÌ ~xc£U¶ïå={{M¦%\æç¡ˆ´¬ÜıŠ‚ÈûØjšz~U¿Y£ÄcB’'#ãp/sW“.N(’}™ÕµúCÄ²ğA&‡cXxsşÒüÂòéK+*Z\Ü¨06/-WİO©zŸ&ëAïâz0zOœs*?itZu{Se©Å’k­ÖlqÊY[ë´A²¼¤İÖúõÃİÙšÓ?×…Çkë¡ğø$m®ÙÌ‰ÉnËç9lÙçHG¥Pÿ¢IXb½eUØö·QXmQÉ_n²‡~@î]ªÀšO" Hìtgœbô•%…q¥âĞí™¸¦ãnbœ=ŞÛºõ.ÇÏ|«æPR;¨˜(&kDø‚Q¾‡Î•íœ3%'¦£Q›°vSİ­‚ß‚ïŸà‚˜†~F~6$Ş˜À*>‰ßÚ3)şQÓÆ?”Ïî‡A\\ÁLú|uÿW‚E}„ıZÿDå¨7$]FÁ!à o&M…i8Â¦H²]æub?çù‹²+­Tí÷$`ùpÏ€qH|›	7‹“Â61ş–Qï®VN¬ì5 #.Zµ<&(é~SmQÚÜ+-–›høÜêŞU}Í	…ï°’ë.e¿§ì³îÃ¢¬˜¿KFØ.ZHŠ‰e—`¾·iª‰ƒ…Rş˜‹¿Oeãp³ŒKÒEîJĞğ.ÖYŒI#7Ì‰”ƒ‡‚s	ªàŞ§rñ[t@LQïÁä\¯#|Š–ÔF5³s5f®³á®á©ò Ø8ù_{3Š¢G!Ğ†-‚3,¨	#ÌîpNQXâõb­±î”_/VÀtÊš»¹ÒV´×‹»çúheNXõMŠMñ¹Ÿùá/ïuÅŞ|m«R¬ÙWíš	g4|fp 0K¶+§üQU÷Àmˆ‹/“ûæzë=d£Ã¥ä4h÷Âü./d<uü‘‰¥O=¶Ñ©ˆ¦/jEä©htEYG5öjùš£ËD¡ó>ëG\4HK8d5™Ìä{ü¹4Ylæ+Û´ü1fõ¢©p`HÑ,®À©d®ÀC1“+å—B¬½l]4H°Ëoİ2Æ‹OK¯ŒÉTó†àŒ`–ü–ì¡[ˆĞFf/Åà d·$µÒ?v`àÚ°qêën•„Ô8³W’uÚ×ø^ÎÚL`lšTÁf'pævx,1"*¢@7E™‚Ò¢ÜJ3”'Ã»VÜºÃƒ|³¶2·³CdbĞŠı)™cÂ™õ†Ô,œ&¦x#óÀáŠÏ,Ï¥@j4g"êØëÃÍ¬âÊğÄş@cDÅ8»ˆÎgÂtœì»\öíO‹®ãÖl-ÇçÖÍx¢Wt£;ïşUî|æÄu“'ç›ÆøâÚékğ8fgì†G
Ï$YÛo'ró‚òuìõb³fmÚ­•ÕF½Ó^Ù€S8Íš›èg?%ƒs+º7pÿGKlêsÓ¥ÅvØèËZ4NÈşµêÎ*šf§Ö6¦G×ñ˜ŞQÑÚá ô³Bû¹CûZ–à59kïòxq
MÑA ^åVÜº~cæóe<â˜Êrû#Ø÷•m +Ûmrøæ8§.BorGâ?E#c¥M¥?Ş_ÍF£ÆŒ^ŒÒf±¶=BÖ] èTx")Íw™4ö\¨M’R± AÄŒàX,ù‚åƒ—Ø¥Ù{ô’‰í“à¾ÕÖóf,ü@4õ¸0³‰h™ÛEÜ”};i&pË™Xæöó­Çn›DåM’¶Û"z%£}Tc³FÆÙŞCğ
ë®#m•~F:~|N-Nˆ	[oÁR÷HúÁ­¢§Ô‡Q
×RloøIüI¾Œšï°ÃDêË&ÖÄ~2úrŸ/?æÑ¹k&Ò!IÉ†º#å¹s<Ö İo”G›0 Ca Jº¤4‡fcy˜™ˆ	HVƒyèşUåYîÉ^÷BQU¡ÊãépQõe"¥^r¼ÆŞ°’ûå¬$ÔÄün3‡²ˆóL“bÃ-X·ÁÏØÑ¹“=¹Ïòå¬jŒ—"«\ş´õSºMë§`ë‘gì~†ş°‰à5ÊdÍ‡sô²á3‹Åğ8™=˜bvĞ»L<´|ñõ?,_a‘º)•Š¦*+”«3‰¤ÅS1óÜÏŠ%£‡˜Ïc s%<b¯w‡3–/Ót”’	Iƒì©%„}‡9¾AMwŒM4Ò$'›9Ètl†ØÑZtƒ¶ãaDÓ>Ühà$S¿ƒ™YI±ÔY]¶VeË¸ùE±SèÚëàpŒdd}NçO½C§Áª½¶µåÁ&çÄø˜h—­š¿_Í*~9ºä]Ê:4É…MíCœ1ÂF(Û‰(Û|,7$1ô˜İ¡%gU¸Óıäo”~£>b>ëó‡•ß¦´ÁWH´8ŒÜéB³“êš`¡Ë1áDuÓ [úPİ…ÕŸØe·Ó×‚-ª7ªR–³Òö79&¶Ş×Ê2™t¼óg	uôæ»&a>fbxÃ‚$L5ÇÁ\ÖÇÉhho]ÎõUÌÄšö¦ò€Ï†ÁKË;–øyÇÜPZöœ™eˆ†b‹ŸâHîª4‰hÃ&–L"úS‡+âàì9Ï>lk(­¤<¡ <ŞRËÈÅßòdO5,Ş½cC2ælƒC¨¹Á è«r¥Üx!Ò	#×Ğ
BRBpxW©µ <Àó|Ín¹ÔH>ºÅf`‹Í˜Ö~ µYwÇ·²¼%ƒÖBtC™Ì§œ
,«;`dÅ±Ø¬MÇK¡$†¼¶Šµ`‰º¤7.3E–rÚ²eidÙ:0”+–6eÃéË‰|ÊOÚ¸9–³äÜ§¸Ãš;‰Pq‘p9…K,¸wîR$')Î¼ãéñ¿’Ş÷S"ÆF‡Û‡¼Q2¦V‰>ÜøGg–—á~°ƒõrµe·«0ÖG|áN¬ªÇE÷¯ğÕRáGO,æƒ«h€eØhuN<äKPÓÄëŞûEw»OEwwAÜ#Èxaµ>ûé3«÷„¬Â˜b”å— {Í5†‰Ÿp« ™îÛê›Æ Ùªl5şˆƒ Â=ÆíÛ¼ù\Ia‚•üÜ‚[=ìõtá¾Å Ò¹w[¿™Ìï¾ÙâyŞì|Â]mT6ÃOÛVbaS¨_Ô’}öù*äCòØûåË¬p_[5ô}2lĞ	·’¾lÃ¨à(OM'¬8¦¼L½õH¨ÜØ&ñoşœåV‹¤cLao÷!ß¿'Jãã†¸3DØßê8›/‰+´ˆ8<fAWä.ªvùJRÌ$À÷QxqNùâ–BEnéG©7¢#æ·qp†+øé6†ZVJêc©“6<[=÷L	¢vş”gç‡Ügìàæ°ó6ÎC¨°e€tj÷ß‰š‚bs Ä[Yá§İO5ó©sN,U»V!18¼S¿~!"©ú)V]Ä–Í¥ÇŠ¹s>+Ê¾ÇpÑ»Éàm¬à‘„À)Æ6Ò£àŸÈbTäJÙÂ3Ú³j“¼Ìçù£ß8QMõ$ŠòE%ì·åò“í«z·Ô#< ç¤º]éÜîÚ}AÔ-÷¨»g$ÕSE+¹Jæ$™y$T™G’·ï±œ!‚0<<´§ú=RÚÀ=ü–Ï¦H‘è%$çç¦âÔ{Ôİ÷&°Tœ¾ôÍ!Ïiğ©{Wâjà-°¼¸€ë0¸Í3é•Ü·æRæ!ĞtbÜ‘Ë­ZÑe0DæÙö¥“ñİ~ÃeÇtá¶`ÃŠ=fçŸRïœ]ïˆ%·Sq™8½”z–òËUê©·œt¶P!(#ØW0ÆÌ€…†O¼í´;V­-ÎYm8CvoF'‘´cq;Äá´.#‘ÖÛYD	1H%)MD­A2cĞÃÈ6~µ4[œ-k73Ï§7Ó%
±²d‡ª6®É¹9ƒmfFİVÇÖx:úş»´H…†ö^ãı°¯X#Nlû>g2ŠHˆWƒ&RJ…—GlîûØ-ı•2Ñéšå™Â ¬u”©‡Ìc¢kØŠ6û†Ç¤‡DzÎ"ĞÓÃµur7­\[öUÇ¾kçTãZ½Ö°*¯mUÔo'}Šü÷ÓŸ)ãRYˆ`ÏğÖš9à>
`çAªHL¹c´²jÕ¬zÙ~½è:v‹LÅ/æŠ"œ}˜L‡•…;°a…˜•œI!Lò ÙB,cLu|Ëß¥áÙù*Çs¶p%İÍ&²Ê4¥òn¥€[¦ó³sêí1sF­dè°o¬Œüı›çŒp»‰çÃ l¾¼€œ°jóòø»ïü?Jïqœ‘ñL&e~ù‡C˜U?ÈÄXî•ĞÌà_ßö©9uú!LÍ ™49{-41ø×3›˜¡ùâY¡£7a¸Mb¹×áb‘º¡V¿X€õã¹_¨§Â!šOĞÍ¯aLÄêlY-°ªW¸æ&¾ò›œŞC°6›	ZaJ
½(L 9’OÄù“ÑˆßAÂ;;+åš‘ì®1ö¹Àù“|©GxdÎÆœÉHÏ,/gBê\0HÕî]vÂğ8ïÂ^ÃôºÁ•'ô®-[£d~õàúS9õï ûTåzúğLç:Ú»‘ó$ŞMÌä<Ş»rî3I*'¯‘½*»<¾2¾Ršn^_i­¯Z£“Sc¥ÒØÄäìØxqúØ»üËñ‰¡øe}ƒTiæšzö`®È§Ä10_èst9¢X·	ì,khõş‹kæ>Ÿ´fç!Tb•4‚øzë³úªyU!ßº|ÓtÿF€ìÙòmÛÿ tø/	¼p7ÜÃ“Çü™›µcódV‘,{e­ÑÚèÔ,ñòËŒ…nÊ0€¯à"w¿¨Í£ “xL\µjF$ø2e7‡9øÍIQiœóû¢™Ô<‹„<­`k‘³oŸ8bÅ–İ„³GGVFÆÄHaäØû|Â8#‚h=—:%Nñ]%ZÌHÈ·`=S¨«-?Àâ¨“G¯8Šz¶"v›±Šîÿ¡\{Ÿôj(1@İ‘!Àí«Øˆnv©Å(°¾ÕîÑ	Éz†(wZíF«PµkÍƒíøpMjØŞ!×_¿Òç?Üò$sÑÿ`<ÚK4â¥F>	7ÜÍË³ÆÎÚ©2b÷L–Õ³Æ¹H…€ğºª&÷Z-çl¶ê½¶`«<[É¯·‡†¡’1Iq¡{Q—M%€ğŞíî×ÔDñP­4ï£a,z¿
`**ï>Âf~d>SD” ÿì^¼?U]¶ƒîI²Ï35Ş¾ª•Àâ{ÔK{·÷ .w—À¾ÜKö~­!®Jv)¤+HØüø¥é¿ÓörZ+ø\3 †I«,åœ‰o#fF2™ú ÅŠÕ®®6¬V%˜¤H¥ ó¦"Ä¥MœÑ«V«àVzäOĞm}°ÛY±áÔÕ
tâDJzæøå7¬ë…k…0Í7®Ë+ë¼æ­ãßÿØ±êngCœuêWÄ"lğq¤ÅÉZİ÷'ï^®!–äi‡ë«ÕïyS(,ÑóFåë˜=x§lâo,”JÙM¿¢(*9‚¨³ ªımÙÂí²\}Ùsfwì ³’õ–­Y©(ø€ğ#ŒSäõÌRöñİ†0ºÈĞéëp†ºU£…>_«°G—æœHG
û¾àÀ0¡Ô½=ª€³ÙÄÒàLL…=FŸ,MŸ„§wa½¨oÎY5'kv… è«·À·Ø e­ÆAlï)šµÇ¢ı ü0(Àìxƒ¡†ÎÓ'õÀÁ—ÿDyîÕ¨­Té|aşÉÉë5m^:šc4¨0³îy2gG@ÕÆ† ?‰ to)úß':«I•ÁÔ?ÉÚùJEƒçD;¸\ƒ(g›™wÿBåh÷=‹ñ)z©;]ÂC´{Şã%Lú2{ïÈèª+Y’xbÜÊÔ;¼û]Zœ*Ú,›ac‡ñ_vwÇğ·mÏ(EŞlª”ÅfTâÙ»¥¹(lä_jûö@Û™‡ò7î}Í©ƒ’*6šv}täøªß•r£^·Ën£Ulnbàq6YıÊˆä_çEõóÎP¡@¸I´ßDFUD7ulÿ¦Ç(÷^ÜñõÁ%û«LĞÚ%ÿ™ç—Ñ"KmÍÑS±ëC”(‚¥:|Lfóáˆ_?Y=Tá{®Ş´*iX¬7£G\6¸ó^!­ô¿1%·-àO¬òy@Şş>,(A<?¤&¿ßÁĞÀ#’OU­´N†D;Èay¿ÂÃO(‚Ñ½ÛûWÕbx›ŞÂH3–ş¢mô«ŞÇ(½Aâ}™J#è/ˆb¹}ØÕwUûÄid7¦Ó„”*J½‹ª íâaIQÍÇlã9,5'®k#ŞÙRÚ|Ş;óêWµ©ïîŸÀ#ÉU‹U°be uÀ7Ÿ¢¹¥b’{ª~'+S©Kåí¯¡&Ú®Õr™Çş¸€¤×ˆ§-ÂõéåŠ9²şúĞôvµ…6NÎUTÌO› y8ùÂ?SäúIĞ~*)¸í1p˜‰÷à.*‘ıÈöF´s÷¾¾<xTÉd ¢¨qÅ>…ªoÀyn3.EïCøûa÷1ê\Ó·ŞVúı5wÑU Çå	Ùàß‚œ‰_z×§Ò`O¡éeğHö‹­@/xö°·¿4(Û¬\›òŒõè•bÛ#ú¬õ^~Hi&µ]NWWü÷/~'.¶ì5»e×Ëv›ş¯T
:ı®=íbV`­6&;"ŞUf%^Áß(‚2ûÈ†şŠÚWí÷>BçûÅN:¤¤ƒä›ªy÷™’ÃŞEA¤ë¡·(Ü…*pGÌ·Û¶Ûößéİ#¸Ë­½7%<ŸhG&L£›ˆ˜…à²ÅÀÓ£<îÚR7áŠªIÕƒ‡îÖU_wG~ë–Mı0ËzÇàx®S‡]$kâX’s íe³f5qÆ®Û*G‘šyÔòäc/ {¾f/ÖßvìkfüÈâĞl´\:
Döœ@²ôïçÌšª!^Ò4É“ISU‰˜6õ@ÑdMïùÈ§RÏYÍÅr´ed,ªÊ¶&›ÊÂ£B,3¥šÔRÆ<£ê-3}B5qĞ¿±¹TUğšpµ–å.1™ˆ'Ÿú)"Šú’¨øƒÂô~@¦9¢ÂCCÍÁGÜA·£“H“CmúŒÒQÄ–::´Ñ‚U¿jµ_znR«SŒM§!Ñéş^y¥ÃyDáŞ§^·!‹àH÷K:ø+}ˆö¼¡eØéş;Ñ¢³ÿPµæW4}ú+~.£Ip5•0W×ôn:óŠÕiGÆ„S¹Î åòÂÀ‡[¹®³øüp±ÕŞ¬—·8ª¶Ê¯-^Ö5ü¬5Û-W9pâ‘ãVÓ9 çãëÒ<±ÄKŞf0Qr¨.ñµa»ÕÒV^¼°´Ì:¯H­o·Úsìß#(¿Ğ¬X†é‹YÍ&&©Øã?i7êÌké=å›çÄ?,]8_”`nĞ;£Ü{-¢ve/óÖà@03ğùÀÃEÊ{ W5ã±­ÉåJ¬X®å/EX–EşQÆáàÎœ³Ûmkİn6[öUÚ œ®íÅb¿?¼5Ûj`ãËwZxƒ]–‹[kóŞw¾óôòĞ¤ @šÉ?÷PÉÄE6Àå›ßİ"†¤wêïÔ»ÿWús÷QÚR85pä¦d0QB…‰}I¢î…hgısG#$-0†îÉÏ»IaMœp®m¼é$²ê%½Ä´;½eD5Ú˜0O7ğN‹­Rİ¤èòşU çx@úïî^ş=ñ}æxãÊ”}¸N`i4ÑÀ×hÍ¯ˆ38
ÖBA|ï{ßİªÉàï}oNÑÔƒâl×°èÜØQkNL³ÑQ;¨¾³‰/K‚{ÁtköÈ1xÖúº[}©Å•EH™7°wQÃ’•ã"«V½â¸+ek£\é=~]3xÒpê£#ïÔG±§ú=Ú¿C+ï	u`ÚQ	c	I§Õø ÜéıZ"Óïû1üp(HÌû[,PÚhæ,á}5¼ˆ„äµbÉi\½ÌÑr¤ĞFsNœuQ¬7®Kª¿Ë“Ëóz|t¤\µÜíQ”ˆòãI‰e9M°!^`*Äê½ïjXE¾úñ2BàÊ)÷;®³à(Áğ?¨ŠTğ‚°¡ğşÆª×[Fè¾5!N0ôdãz¶QnØ6wÆ—Ó"r‹ÃÈ*d”àğJqCˆ—m•ç*ô~Öå;Z /#ªÇ"dU®æ†‹UR#¡ó·±¶wYáš–`YÛğ.&^?£ÁÏpEF?Ã%$ğµP“”¾Ø§Ãù¦?e×’ŒØ§—·ù&?}hÔ`ÑÏœKşLöolô³û›Kn£O¿¡LØ.âÄÿ"ñØR¤‰hèç^[Iø—š¶<¢èæc¸õxûc‚<¬G²WHï!PÙFü	}kÛ+ı|BîãEU>w$F»‹Kß2Si|Ût\]Z'ÓæjaB Ìr÷Kšğ[Ø=òAhöoÄ°D¼šéìt9Ãú±Û¥*–¨w™ÖìÑßRÜEbos=9e?s*†ÍöRr´GÃ^#Û\jÙ­f«ü@Ke«C-§é¶_WŞ¹øÙÏÄø7“Él~ÃzDãyÀp”nŞ²½§¢àOçx_zîA³e¯÷œ-Tm;H¹Z¤—ÃÇíC]Àş–DA…õ1ÔÚ*Ç‚ÈåJ0+‰ğE/+çkvË]n9V}=ŞT’¶Ò5C”’`>rPPİÏˆ9b›€|_ÓöWÁ¹G
üû¦íì ¼9ÅqĞ2 ÙÈ‰Ó!ÜõF]¿Ğ£ûÔª[µÍ¶ÕB?yÅi·;6›™¾}„™ãÅ%™e")99\JcnÎ-D@ŒıøfE`âÏ1MB(D~Ş ón‰ÔğHŒİUq£†¶—Í”Éeª÷°BßÍXÆúÂ™°øÈ1YsjÄ¦šàWömÇ•!ßœ¼ß™‡ú->Ÿ)ê{½0`}U=›<úKßK½Kå!Köû¤ú~Ñ÷°Ñ“½'§Xr¬õ;ú*Öˆ‰Ş5×Çƒ‡U—Mt¤Ü¶8ƒ.—À&ÛƒÔxĞŒJŞlaî'e|Z6v¯a‹Y€­Ùßg.Ò.BS£±2Lê)Kê½Ø6¬ÙÓ•ë…â\T5ÕİÎ¼å¬6‹º=’$j&Ò{ÚDÏˆbù>º7n™yãq¹(½Ü]ÅN¡Jg(O½§mÚ‘¡ÌÂßß¤ÍâŞJÚŞl:Úi]ÓŸ‹ıG¢ûÛFL]¾pêBîâyØØn£Ò8¢;:Òr[‹?ğß+ñÄû‹]Ì|ˆIá÷AIÛÄòSÆî:<?;ø³ş„M&«ıó°‰-¬][i£-‹;\Š•¶ó/Ùùô6ÚÁËìrâ¸ö„¥ñ‰)æ×t®\øçº]-eCßæ@¢éBcçN™÷Y÷®Ê5Q„ş†P$°;2z?€°Ëø(ÕCúa½q­fWÖmqwÚ¬…Ì‚»!Q}ÂÛQ¦Ïd>j¶%àw~o5®iºƒiR¡<YmL}‰Ğ ø‘D}9i U˜ˆõÒ!LŸN°Ÿµ6íV;	€w¸X…~EÎÛ«ğ…ìtBù¿úé·æE¤ë‰¨ßÍ˜ekúŸ $+ï‹/ê®ÂÁ*B ¤Á{D´x‡¨¢UaÂ¿÷‰³ÄÍ_\ó°¸šk]Üt«°ãáĞeX„«5»]m4\d¹{—@;Ô\ù$QD/¢{¤ıŞ‡~Øİ+Ğd|-)=:‘ú±¶É¯0ªZõJÍ~«Y¹üáÉ,MSqÚ˜¹¯¼¶å´éû0”ÙG„#æÍë°1T/õÙ÷a!BLkaün›¦`øŞ53ê÷‡ğá(şSsì\p¬CBJ}ß¼öˆúY®”àD ¯32.™•0È´ÊÂÃ¡µ«N\²×Z°®%@……Uw6ptÚM§®‘³HC®¹ÔRÓj]=kJÎŒìÆx$$‘ü2Òb±HI™¨´ÂòP%`3,2w “æ¼}M¼a[n+ü†÷ÕµÈåÚF¨ş³‚P*÷1Ê
Úç?{‘²	Ìlo×”Šô$Lÿz–›5˜ó8(ª1‹IÕf{düÜ¤8éC	xŒ‘ìP‘]É°%sHŠ*òAÆ•†à?İ6¡;'1üK.hL¤=M¾ä=ñÖ¥‹Çß<uébºşÏÔıÆºûŒíƒ!m9Vm¡Qm¿ó´r„ò1¤êúƒ@)]uB™üH¹¼Ù1t*åò*?FbÑ0~lPyıCI~qÜ“0‘åês*áşÅÙ7–Ä’ívšGW.öùŞÒ®—#¢ªİâºãZ®Ûr`}ƒ½fãÍ:”e²èÉŠá}YÂ‹£÷©oê¬)<>D›+`zûfïeõKVŞ¬Á"bë=Dé	3ÿí™ò¨.CÕ¯ÑUóBj&ßî0óHãğœŠL85†S<‹èùš$|ÖUE=ìhß'$ÒÍŞ-1º¼²W _kLœ[ñ,]4¾¯]•‹¥[šU‘ß«²Š™4cÜ ºÔ—cÎQÉ7l^àç½=OrkûÔÈR³„{¤ÌO=zv¡Ÿ:cN^Î|ÑöYÁí=•qiÃ|ò™š @`uå¢!ÙJKOÅ•.¯Œ¯L‚öYi­¯Z£?xuìãc¯'½ËBôxÃ©Ù¸MŠº«så«3ğ” §H<Y*qÈôÎä
êh¦—¯‹—.üÃé…eqn~iùô%qòì[§/^Z<¿Ì¹am€^}µœô¥NêlñZ¯æõÂÚ<ãq¨s°Ğ¦“øf£’*Ëé¡Ğ¶æ"t9dtşó˜tå$<èö7ìŠÓá+ÈW÷˜ê ³Y’¥ŞÔ sÅ÷)ğ*ÂÜF%©^ÓgôİGÌk’±~—c géñy fîn=°ò«<†œ:‹ª_Ñ®È~²Tm\ÃÇíQ$Ğö}ÅWt#©4HTDûşC°cÂ’¨Ï¼ìïTÌuú!b´<EX_vÁ‚ûqµîC´Éˆİßçí0Ö6‡óB~oh…\é_øO÷Èı%º¬=‰¾öR©ëÄ™–Õ¬â~óìÒY$Rå%W—*f{ ¹fÖ±QİŠh·÷	ò}g¸„ÜqÖ÷°iÃ69ß Iñîéà:#İ¿\ÜúÜSa†üBè¿¢ıôX›|F÷ŞGhØûyİ²IU	nš¤ ß‘nQKá6âÔ×šoe!w¾}¡„h7˜±]‚3ïJÍ“Ùãî€fÚXmİ &©l´æe±Œ°ÓŒ¦gijV[Ëm…O8*¢iü3„ €ì–ÏÀ<±l·6œºUKJ GÃÍ¬²ĞîØä ÈÅopC‰ª„a€Ú…ôyÔÙC+æ”¥œcÂaVo2kù•›ÙÌl‰vJâa÷†lb8–ƒ1G^w'Cœ*uÃ*v|…ŠByÕ„F^Kò#$ÕßKß¥Qo˜Ş9¿t“çü2H[6	fÚ¾P¾Ì+=YN¯QcedßÍÖK&5O/¿¬"ÚQñë>9xü‚Ïß!ß¬*Œˆ3…(²I×“9 ZĞÒÒYœEßE¨iñPIö4íş…Mèºd{w¡:™ÑùE0µ
§ùË”Òu¸îñ4%G’~ç*g–jæ*ædj$%´uvÙôõÅ?š[ãEoŸj“ ÑØ#ç½TH‚â\xcñGçN‡k¾N°M¸Aicv1&)*ÜÀ¶I`&ãÛşè?3ãf&ŸqC7~´m›	v\\Mz¸vãKY,´øJ5f¢u‡oÅø…³«f•³§úJæˆ˜%™Çg…6d¹—Å›NÛm´6_D7rEU;Õ&
!d}‚Few
 Q½ÛGÈŠ”RÖ¬Öº½‚R”Û £FlC¯oMí©Ã
wÈŠ—îƒk,n€·ÛßÜ,´bC1ƒ°©ÅJçÓ%õv¿Ûî¶:õ2¶¿aëlZ=Å¦åVŸÒÖVh†DƒF(Ú¨1I“ü‡Å2eNç=ê3O¤ˆÍİ<Càw(€#’Xx;Š-yyöûú>Ås¢4~îä±£b¼PĞ™lyòŸtĞÃ;S¸Êëæ.sÁ£çœºS8ã¸Æ˜R”—á†QìdÓPŠfKKà¹U• qà ?ë‡«kÖ|&ÔB­/ÚDÒR8Êƒß´H…‰TøØIì*S¸\˜"ÿ”p”‰ñM¥ª÷ÂI(ÚW±›0Å5¬Š¦OMğ’±ix2ŠV½×\ß5ËU¬ãï]Ø+rå7	Ö0;ÀèÍB0&˜˜ècç/üÙ¦`wáŞiİ¾F«¸}ÔèÉü®W¼&YH¯r¶mG–áÀ%jÊ8ÊÂiÕtJ4A.—Jı¤¤fö¯œméËˆï#pãée¡Ä6ZL?•-`©ŠéÏÄ{¶çµ·÷QE½[¢wÃow/+|	#6^;º‹`Î`è&=`cfİ%Õ$D	m¨&¡“—ß†îç™›™qšxîJĞëã/‚ÁKbjèõ¡–8 ×­UyjmHS®IÙ’“'-:#‹nÀ.j~<LyÜšÑã*x= 'Ç§S'ğòk™â¸úUMHNÍSxú†xô=6dÚ&¼Êæærl®6¬VE­Öb¹Q±Ç„¿ÊxBspÒ{4ùÔ9lTâÑ°>Î³4)¾˜ó	ãâØ•Å
éTXxò•”§µa\nLOOİ¾8LkÂµùã-b|0»-‰{qrû°ĞÔâ*Åjë—P¾	€ÁµuñáX02d¿ª.\§ĞÑcá6-¶›5Ç•½y/¿û~±8$¢öÔ!¥ÏŒ>Jù ñmÜQ–×UWºÛ*lºB8ôÄİ7è ¡™ÜªSüÙœ¸ë+g%·)ĞÙ™Ë$aqfãOúLQ*™Ñæ,re—·–&üúÖéÄòÖ(F½+š­9ßm]³—ÍNTÎ İ93Å«•Ìî){Rn5jÿ·®À ×Á©l"‹jÛ–?qË¨?À^—‹‹À0OœSÔØ´WV_/v¼bÅrVànÁå´êîë {6Vº]Y¹â~şìgb$»Jû¸ş¢ù8ğk¼êğÙx…îğˆFS7é!“¹Fçÿ%—-‡›†³ ùU‹8$¾P­²>gµq˜/Ùe\›à¨vìfDWö­…TÏ	ú7}ñÊ¤Wd™ôŠ_&]<» Vw…G‚Ğ§~á˜ß1n·÷ÉóAÓ™³5Zy:Ã¨5cjH;£nyJ”(õÎ3‹ÍşcËê†1hmûSÓNuäÌgİ)Û'JW‘V¿–lÄı &U *[N¨«ÄH0œz¡ZĞ7ÀÆ ä›6¼´êuø‡Oµİ×£ ¬ü”î³°'ê±;º‚ğê–\ÓjaÒ*ºŞä±ª&I¯ÏRmM 3°¹9²6MmHp¶&bÁK$Ü˜RŒòaYĞy‘ÖÇ!¯…iiLë¤a[ó´)C¦ GS ‡4Éğm›Z;›Ä„ÑK!;"ŞßàÜÔÈÙ±óx@± #6E{û4ÿ÷_b©>éI©e{·±¶ı¨JIúîvïö>î}"s.ÙAïÌ&èšx‹?‘ÇgCøîÜ¤Uğ˜-«&d/F°‰š5U¥.Îã:øïş ¿€ÉTïà·ìšJœnµ­,„Rvy}Nw¢ûH@™œñÔÀNÀS­Y”oŸ¾´´xáüœØ²šÍ·íVõ}Îj_X^|û´X<¿´<~áôœX:}	N&Î_8uZ€8~*™s–SÿÔh]!$(ì$šVİ®±5NXÑ‘óâk9©3ËÀÏÚk®X‚•¶jµDA ügÁ5ÀL_+ÌÇÔ,;B"[OzY§“.‘ù¬¢%˜Òf~°ÂÚKN¢û¹Å.éğü„æ}f±£„'«­ÒÔïg²tH€,l„¯×¦“\óà£É¸ª©<K	rOÛÈO2(Òh†4­ƒ²À°yPJxrÃ…Eè3xÁ37[ğ-MÀ22´ÒIÿÊD¶ú.J”"•œ­ltV¹Q‡=ï´¥‚°+â5ğÎä¯x²×	¡A&?şú™'lÙn§U×'g™©8ÊÏú—Ö'<ÍĞãÈQ1Éòn½§¼iÌ Ğl&CËš2¿rc¤ñNŒ§4€V‹¾	Ê‚Ò“ÅJ>I[ÄïèÛÁÓ~Ì5õ[pDvÏ†àjqüòxq|â]M[¶XFœÓ!B¾ôp9ãÌÓÖ{Í°‘X¥üŠÌĞöf”ÜÊ«É”é+O&Š!ÂJ]'øœFêùx	Q?_+¨Ê‰ƒªÅ½€bS]Œ[ùŸÀ‚B¤õÍalÚbÿÁ¦
_RdqñåŞ©kàç,ÀbuÅK ›qÑ®aú€Wó¯™¡ú=ªœ4ZD™‡í¼˜êŸaÒií}áÈ¸L&õ¸Ì´<·&ÉÁId.sÇ³ lè½[ï%Ç‡—~çoy^úšƒN8–e¼Ÿ1Ş/+Ã™¹D~äEò
˜&h£6%qù¶c_Ëˆâe{Ua¿Fƒí
;6S¼yz>c¶©+ºµ*ª2™‰‡ŒİxµPšJu…(l®—ÄşsN°\2n”mp>-N8P+­Z/iI)íÜ
ÛÅ CÂ’¤§VK¿ëİîŞ•ÍT$FWar±SöåH®è=p ÉŒíŸ¬fQp%øq:¿Ôgø˜qD‘Œş¢]¯´ÿÉq«£¯ËíW¡ş^ø
|‘úV-òp¿k™˜©"¢w2 ÂÍ1Ê,ß{¢Ï÷NXæ\í’üÀ†Z˜;¼Zí{B³Ñ9h©%—¶Ğì v$i4Yáw|i³^Rõf*ÿôyYyE\ ÁVåb(Ú0_¯`ØFltÍªµíL xõQ«¾×ı‹ì&MvÜ‰Ïî6˜ğ}÷}LŸSB\‘2á~¯$¢˜Æâ	øim÷ºû}ù÷Şí—Ş#íN¹l·Û#¬›àº|ZX¸í'qÛ£y­ÏeŞIlWK ‰I’Ï^®ãÖàÙ`ÒâA¹h#ìŸ|Ÿ¦9Ñˆî=dLÃ"øùÉ’Êa1pƒqš*ıDÿï’„s)õıJ¸ÏcL†¾Jå•WÀİÃÂ	Æt?ëş¶ûgøù»î§İ/»ÿÅPA5Í§×ê¸ï qCØT\)*r-–[6ŒêiùÁèˆÅÛSŞ	Ñ¿9Yk¬Â±Æ½Ş÷JÖ¼;&¶²4)çıx³fa»Î÷™WS7^¬¶ì5¸Ö[—Îª¿°Š’şõnÅğŒo®^ë·ygò‡¡‹Eö.ˆØªS«ŒªkŞQgw”yPôâ-{„M‹'Èw•(ÈcrZõHüˆÈ_HèÁ$´I)•B•ÌOOAßé>¢(öEU´\ÂÄâŸº¿éşoøÿ „åÇ" @sSœ”âòâ ¤¸Ç8òuØGA†Ö­«Îºå6Z¸EeÅPñZf1£	R»÷lw
n(åAàºœ›üÙî¿ÁtıWÄ°JÜ¬‘ll¼'ºwá‹ÊŞhwIF‰í—gı—D­<{@õû#|†d"ÙQ]ù”ùğ¸÷	¶OE£­8b*hT°ºÑqGıÜPh˜¤ñ:F9ÿƒ2[ïeHŠ¯N„²J ˆD³S…Dfî†ËÏPöÇ‚j³2ÌDS!Ì3ÌESS`MÌ¯pæ„A£À
BƒÁ.Rc8vê‹©œ³Iº
« Ìk7%_$Q?ë~æçd†şÍQ]¸ºÖUøõrãĞWVÇOgÉWßSƒ>¾Ì¢Qù>ı¼ûå0‡‚bâvv9ÈüìêJŠŸrÚ`=oŠy0€Îq4„*Õš‰ºú™Òü„·Trü8àwß²×b*€Õ©ƒÔ/ÄQKYøL™§ÄÿáßJ«ÑÄêŠV¡­ïœ•F×ê/¼°[ÊØ	™ ¥@a„f$p7«r10¥ŒÒÜPıªo{õòM4T|èİm}1%'x¨ÍW¡¬J°ô«¦Ù²û†2X´¡µ/¤¡$OÂyo%Ü¨.O WĞ=¾Ùö¨DÔ™{!–ÇL\^aØ« •Øé’“'DH&È"¼§ÌD?ê÷:«†I²Œæ.ñ¢|˜ŞêpUi³—™¢ödÂ6™uÛâ¬SÏö@úÂÊ/E &ĞÜ?Á³éÆ3ƒá”3KÈhÊÁÍş÷âüò›s	™‚7éQó6Ô#Aú÷gÏŸ^šKÜ•ª2ü•wê¯ó°—¼\‡<õÂ›ó—æ–O_J;¿ù9û„•Ïƒ‚¢ç xúÎìÔ	©°Zk€ÕzM’LĞÏxÍàê~Å¬Wrù/?õÏ‡!û5_HİÄ‡QBU‡qŠ^ÿÄñzqÅØ'ÏË$îğ‚#g‡ã-'Œ œ¹P«Y–8Ó™+Î5*VO~"õ´[èûËcå¡1	s"¸ÑÈ8upC­Úk[[^)ĞœûÆj9D¾TŠÉ¾î¸Ù§	ïrµ®gÍıjœñ­çl¿•¦×nXıº^tÓ?2mdkƒ/¾:&à&ú 2êÛ%únÂW½aœ5ÊQpù;¥	øoêİ´}«âÏQ5‚YçìøÕê»šR½€Ø êœÇR©Ü01ûƒ€Púï
ºÊ›V=^‘][—ê>K7–ûéP`N§óéÒeÅT)w¨dÄè…µ5íb~ñ˜‰÷—Tõ”&úR¦ÉÀ!£pIò96;©G8?A`h£Õî%õ¸–æÙ'ˆ	»‰W—šŸçÄ¬Ñ_—–½.e=•Š¢ûWIR¨’ë[¦ñtP¤7TUµ¶1ï¼şÃ.u¢Ş¦zù½ÀwßŞNgF>a¥
˜1ƒÚu›í¹ãÇt5dïHOºHî¾6²š©~%í[ñÇ÷DZ¸I“L³t‚ûJ1i¬VhÊßŠL§tÇÀ3ûm’Eƒ¼Ä&Š²ÉWÔpítûHÄvh=¤Õ¦ÄbÅ¾ôPo’»ŠŞ0ŸöXŒ.œ;uŒXîQkèûŞØüô~ïV¤ÅoğH»â,N[â¥'çÒF>áí4R­©H·]¬¿ëóÜbWš0¤Ë…'Zº ß&ÓUŞ¯˜Il³Laªb¯XŒ$ÃÓP]LË!-?Ëæ€¡‹˜ş{ò0?÷÷qtRíh‡AXFlÃw©¿6|„Ô¯¥ÒÔäTrt£ûop¶Çx¶]9pí=¤ õnã)5‚7ÆüîÓOé[;~Æ{7p±ÒecDÏ¨˜8N¾®¿5{‰Œ>kŒ:ÌGI¢ë1è¶ÕRÉs4UÚÁÙØôO'Ún«Q_ÿ{lşs\YùÖ™ãFQõŞöUÔMæ»0½;rˆS. ÄË.&ê»OÄl÷÷İßRsIš(Z7 kPR?4¡OzwğM1ªæq_¥^ï—¥<Í÷E*íŠ}šBšo\j0ıçß^<µ8ŸÒ	"M¸É	o¦ûÈIïûd‚ò’Ï9ë²8ÏÔEö”Ç½ğ’¿…^òdà%¿úíö’-ì§æ'«O3<å3{¾T®mù-$ìÉŒ®Ãv–£¢ç…¿,ûõÏ_‘”Odš¹ÚÉvm‘Ät:‘DF­û%Y•·z¿¢,™$ß{’ónä¦?`z9Ä±Z^„;µÖ=‚æ•u}ÛM»¾ÕNŒN1äàäïo7XÉ[&+ÙUkÆµj0òŒêcVÆ…×<%›£'tWW9Ï€7¦O%Øùñ›âöå ;·ˆ@)‘“—:)úÙ4÷tdEÓ@4=¤˜º;Æò(‰ëëËªU¯Ôl©(S×~Åi[«5»b¥­tj}=íË5G6`sSú¤íhXõ^¸ØO@™êÌ"ŸìCyF¬/³ø`øÑSWìëb|DêÄO°5õ¬€Å2®Ôhñæ „K<0:±~#N–ÕLZ6F·µáD‰ËÈ‰Â
Q=¡©î¦éjÃt¤!Úø\ÑStìŞ×ó9gÈDr]o°ùßì¬æKPÃ±ÕÎê‹5½¾®÷‹µçz'9sÊït—¶ĞÑö·•ˆXhÔÛšılRÓ!QóÍpµ”¯æ¼ï‹è{ı6clø2_å‘]¦dµëk„‹îVpMÛC:_óƒY„ğUŒÀ«Ìá¯)nÿDô~‰9@ŒÚc
F!Xoi´¾nP8?œ9ÊÈ\—c,a¿qaî·Úà˜½ÓŸïœ²ÛWÜFó7íl[2Ñ
ˆ3ë{#ÅhÃÈJ/£á§¶8å»ğ)ÈéÕñg%úßLN&¦ùA¼€N¸[¯íëÖËlo¨;i¨KpÊ‰8¥·<-vy|e|e¶y}¥µ¾jN¿:Vš›˜š/N{7µV ÙÁ+éÛ¶f‡-ºÿ‰ËV”æ(Ëà,´ÄœNAÃªô@”>GOÏ±ú£cÑdğ¶Lİ÷nË&^«Í4w5ÅİNşİ—VfÊ×—L¾\ñ„çlk<£E?1‡ÌÏo{D"²äÅc+Iëåqàkß¿«ŞÇvr;~ú7Fz%ºÜï4€½z7·…nU*"-ıb¥¾Ò'a¥NâSÑæK0Á‹Ïf…ÿ”Ë‡°d!4ac¤”'½#aªğçy[ËåÆÆüSØ#‹ÒeWoÍ©0ÙË~»äy¯qXr(îÅÚÆÚŸš“ì»°öï(È!ÙªO)Â¹4é~6;Á´zvNÂ½á–™ÛÒ¸‘ÈN_Eì(ö­KgŸ·ƒì40«¨-g=ÅÂë®“K ØãT,şŸ+İï~Öı=°ößù¢ûo ñ>9ƒTNŸQıøÿ,®§ØSWh¥Wéâ]®J€CáœÈp™¾û5¨ÿJôX&§ÆJ¯ş`ìÕ©gê°LÓ®¾Vº*½«ò”°Şn[j¸q©aû%î}ğ¼íÏf§ë¾£6gşåŸò6'ÈdåšækvË]n9V}½f§FœÔ5Ø1§” EÚª>Qfn•Ğè;oHVÏ?F>ÆIÇ…NÚn™s;	ñ~&­ûW¬÷ ö Ô>öâgöH@x#^Ç {.‚2,†Ñ{
ş|#ZI’vÙUC2X}‰H¯0‰×˜ÀV*´ËYE&Ü2“şB%B³«L²ğ0¡»K{ĞãVÚá#æ§y2ËâIxY°À“Ü^P}/'Âıbƒ„ƒ,Ì`c‘Iñ²`"a°aæ°êMSÑ· Ç¼`5­U§e·MsÌ‘c_~Y”c?OIçIL:3òÊS™éhîôyN:[ğ_Y›t¾<IÍ«Ó ß³ÓIyçP÷H
Ú4ïìÛîæyç”£±nÛ¡ö¢¹Õ)&Qrí/«ÂI}5\Áo¡Øi¶Î­fÔÎ
5ºÎê9Ÿ	•K³«úsÛ×kÉîG4£m·R–¾ ÎbÇWÃÓÌ«”ö“µ¯Ûÿ’È1r†%gwE‘`€øeMĞì{Û²4;>şıPmĞA)ps–ÛÜŒıdzÆŞë¾\­kÍÏçÓ"~6ù|/‰ÊK¿V°`F¶ì•µN°¶^+;østÄæ˜pE¹Ò2@å€òl0yÌ2Œ5sÎîkéé¤3)ÖÑúö}ğ<Íb·pøŠÄœN~“çà	ô£¼›¬FéñŒ49, :ÉÅägàxÍzâduz“×kvZÍší5ÃÉ Z|¦¸ĞîW’óítŸÀoá@S¢¤®NrT|ß®Ïz°èFGzõöŠlWhŸ§¬"bŸm·CÊ“ Ş­¦~hZV¢z'#ÚA+úbi\8 L½ƒô=¤ú€åì¦ul,ñƒøá¾÷£3|VcåJ½qfxİ^YÅ½‘ÒNÛ–³ÌˆuşN¿Ïb‹ÓuûÌÙĞ¢u’0ı·IÆXê©ûŒ40:ÏÔ«Ø|³n·Ö7Óõ@â¶§Ï²ˆFSY 9]~QL…ãsİı¹¬¸YÊ2ûÿ   ÿÿì}‰vÇ™î«”i;†bÀÅ$G²[ÖX²5Z<Itudl’ˆ 4¦P¤i£Årì«ÄrÏÇ‰×ÙÏLÎ¥(Ñ¦(‘:'O ¼B^àænıUuWw×”(:32	¢·Zşıÿ¾Ø2Ëµ³ôa.ÌÚ@L=FFòn—E®³0¬l•ó~ ˆ5$p`öW£=œI`³#C¿ÜƒÑ3ËW\Ä7;aÒq–^íƒÅş¦Áç¯îœù²Y„6ŒĞ0%¤äƒ®‡–#ƒ©É~”¾Õ®ªD|Oƒ–ùğcdü²§Æ§Zqú_l¿t#rMGØÓ€È¥ø ‹nÀù.ã‡Zd‡‘âZË‘Ğ—¹â¢n–¬š§|yÆÂûTÜ¥Zqù¦®µ2~­vÕv-,ZiÅÿØ¯½ÖƒaF¿k1ÊJ•ÕJ­ÒÂ¶³æ+¹r;€Pàºü´§7ŸÉÕ¤
¨6ú´=U¦œlÈ,@:ÿÎSxÏ;Ÿ#Œû²óº³ºÖõ%o«©%­=ÓDÆæÑ¬œ3xZï•ƒfvÑÊ.n,¤¥Ohfñ¹`\R‹VŞÑmx
ó.^ÁÚó‚V¨ÉÛ¾Ëçéø^˜÷êŞŒÜM=h?iÿQz·Fi¶¨u•XÑ‘5ÂS–w%†I9B? Jƒ+ÊhH/[<Jş\
¨ƒ×Ë–&Í²ƒñK{d±¤´ºLYw–‰…äµj¼pfaÏş79áŞŞä©g-—J}‘î ?X¾0ojBöù!I»Å%)N·U=¸yu»šÖ-ì¢×i+ü¨»õ,éÜèx¿ùCHé‘@yàè~‡LpºÁ·y„BÓìwBxA]"}Aøüà¥ìJŒBNÊÃrİ¦&è6š·RBÚ]¥ñŒ›c·+>ù?²½õ¶Ø<DBe”_{uŸ•ıZÃ¯ÓœÂO7›|¬hóŸâxºÙÅfû
ÎoóÊ—mÖi¿7·ÛœWjµ­Î™¹§›M>Væn¶½°Ù¾¡Û,Àªâöæ†CDË•Zƒ1Ÿc¤ÍaóµŸn>ùXi?‰›o 1¹‹3¯äfXµç…Z©Ş.UŸôpÜ!ß¿øVÃ«k}õ~Ê¤‚MÀ³}t&Dulf±˜#Ãdq<WÈöíĞ{W$ÃŠã¤ğ”à‡ôaÒVÆA? ‰‹å$6$ÚŠˆSı¾Tş¬è šƒøİŠf¤îMú16Èor¢Ö»S{s>ôY‚>Œüa•?lc,å^÷C`| ¬ğ?&8?9Òù¶^ñä5Dñ¹¥Í™à»n ‰'ÂıĞSğ%3ò­ÓÇ‰ŞEk»7³ääÂr³R.U«Ëä`¹Ü|Orf@/N”šIË'§¼FXŸ×6¢¬³FzÚhvoìs»Cÿ±£;zÂPÌ«SºY“¢FcVEEHR‚ä`®Y­”½L>K&Õ†3èí ªSTfÂø²_§¢ƒãø’„^È"à9uæĞÈé±ğ›s•*C*gèÎ©[¿Şğ™}cüVàQ«¹şøÊN¸î-ÊíXvÀÑ”F–)xCì1‡2BvØÕôâŠ2³[Ü4#—MÚ:)¶wağTej Ğ/ıÕGTx(¶$§n¥ÚjrOX"Êï%?Uµ%ı‰t[d¬’ë º!ûèİX‚öÅÌÛ1ØŞ K#–Pv‚ZæöS¼ŸrÂŞô‘îdÒbË«æp÷Qş]»ToµkÀW}±×>J~.ú„µM
2VïTº#ò1`õïJÛ¤êª}·MæÆÓ“c}qå pH!ŸW‚ö'î¤·öÊ×„a™ ºI^<ïuÕ”ªûb¥$µZûŒ#O@Y%l˜
ßÓİn™‹Ruw+â"úa“£î“˜ë%á‹·©Øİ~Ï¢®áSflhıŒÉÑ×Úh8gb-ŸZw=Ÿª:üè™ÏåsE:SÚg~Ó£^V•£ëBğ¢Ë¼
‰YëínË§¤b\;>ÇôŸ£=w|º²Ê¸ôâR@(¨í7K‹•y6eg¨Ì“tvxnÚĞfæÔDøÂ]ZéÊz™RDÃ œâöõe>WŞá]®VuŒa-f.ˆƒOısj_y¿
9pà 	_G³Ó^!C).5>à‘êÏ'«aŒŒ mèvo—0ôc®“~¨®yùô,)WüáöŒ¶*W„ıÔXïz"şÖFö,«‹½ŒqmÉlö|Â…å®«î7µªÂ³÷àš2Ô‡âºb fU±hÀeYñ·1®+Ş‰_YüCymEËéÕ%]c®/hÄÖ..aÖñ‚Z¥^J‡Aw	Â€†C±$ÂmW[•aNfKN·J­v“¼D=r¨T¯ÓÅ’6úì>$ræ\‘{‹NnMlèQÉåÑ0“Ş&vò9ù7	_1 ŸşÂd<¨@ ¼Ü0[pâÓx_ ÿTçñ
J•úáJPn °¼ÊF‹t™Ñvo£J)EÃóî°T‘`´R±§¼‹Ş3ŒÇj¥yïXÙO¤,¶£¼´¹3sº¬I°2‰}fúô€;˜â[cp½ß3dEöKE°³†2Hë‚y?Q<Á€¯‘Æ’fÒ¿|!&°¶è¯È3®ËŒ=Æòa˜tcßĞìœ
”;É wäì	Œ)>W‘=[İ8îbOÆx`L Ë=Vdm#S #+§?AÚiMïE/ú^FDsc?Ì#"feOXƒšX‰9]—ê·L×]Ä ­'¤ˆ°¡—­°©xù^Á÷º&ä­Ã§ÈÁ2 m6ÂTÑÃkÆíƒ¤)°£W•¬ãË¾]ÁhËq¾R&£¹|¯<@Î=-zOys×\8|I+zcÍVä´?×KAs=Ù˜¨{Muâºï[·d:^™j3´q>öéaÎ!Ë>À%`KöM=ÇH”ÙÀ½’«4/ízÎ)7·Õ Û…¼a›™Í¼mrHeôŠCanª²|CSÎÔˆb^Î–ÆtsÕ KI™æÙ&{æ5Ï™(‹xï=2t¶øu’‰ıAY<ú$l *{º¬[€GÌöÈ&HÌÛÓm×¡ŠµNoî“¶ø;qro¬¼Åb®Ï-=][?šµuÊ›õul+xçG¸¸ÆËÊ!ÆŠ:EU©²][•Â(~4§²”õ;Y²4M†UøÜìPÖóàIÚSUA‰¤^ˆ{¾î•`;"]¬*Â‰ÒG”s2¶›½[¹¿êœJ†Õ"¤HôØ±,aB0I	G3:ƒ”†ì¯hÿá¯|1ô„˜'kòÿ„†;ÛŒZG$ÃˆjÑnıtV‘âp>T¥uác˜ËYU¾øè¡Àñù¾e¼bİ_™ˆí}LĞŒÊ e˜ñw^hHPî›·õß*¢§ĞEş<ªù€ƒS>ì^‡¸œh]Àª~d®ãS3†9±Ñ…a|bÍ £P½O¯¾Á¾pI7¿ÇG]‡¿~€UıŒî’>â¨–	ã­ĞğÀ‚ª&ê.'¶İÆ èÏ^C°˜k¼Ÿà¼vB¦¦gìÑÀhÕ„ÁS³FN.·üú°íŞÔu)ûL)NqÄSF=y©R§¾XÎoxõÌĞˆ¨.ûõºWnùA®±<”%‚›ÆTÔïÈ;ÿFâLE~ÒkÆ4,’ªşl%¿ñÒM§}5³”LM„ğ B6£mb¸ŠjFËF—ïÁ$ãúÌ›úÄ0ÿ&êr%’T[P‰{kDÏÇ5œœ”“O*¦ ;{±ŒAµ'Bìæ},Ú›(u5eşå¦3°ŒêQ/µµ ÿ¤ÊŠäZ7kıA|Ç,ön´ãOQ€§ ÿ¸m†W±u—ôâwì´a9*Ü|µ>Om-r2ğË^³IO|^Ö¤=İòMr,Ö®ìlÛÚ ·‰#UÁÎ†Ú!ĞF¸éVä«}N5lŠĞjV¶‘ÔÀÇŞd‹¯œ3, sû21L“!`2†Oj*!·ìÉ("8ºpÖ·q–:B%yˆå†p4Ãàpv{:Š`Ê0£‰äÌìr^{Îf—³Ü»BõÆ4Ùÿe*ÔÊÈt…Ë¦?:Öç¬e-1jt²naÇ1ûT˜×ÓäÕY X~™îdoÎ m¥	¿œöëøã±z³E•5ô>¥ğ6Úf"N­²p´Gí´×jÑÅß|LãöIè­ö4f_s0©
à3ƒéâÛ†E°ÓÙæÄë“b„FPğ‚ã)<ÁG6>ß ?Æ¶hu~ˆ›c*1êK,ŸxÀ×Èo¾€y»â]jø0M~
İÔè>Ğll˜^À-K,} Îåtß&¦í°…ûô„X?~ ó}D‡Û˜/-{ÁãZ”ßÁËPÑ‡îVzl»nelD¿+väá¼J2áŠdl¥W¦É2I6Á:pï¾°w„Ç¦î5TæÀ€¢¿rÌ§¥Bóc•gÍø¢î†y³xæíá°ˆçfòVé¬}ÏE0ºˆğçª×t¾³Æ{¶TèKÖ‚/JevõÓ±+…Ø¥à£u°‰¾äª«Œj”‚‹UoÀËá<k4oR»$K*³KvxRKR¬{vÉÜª¬‰FBxQY‡&(©î­‹‘gÏR€E–ü”äs©FÅä¡îÎŠ â'¸#e¸ˆÅLO9H ĞbÅ6l¡Bœ1¹W_"é¤Å½”Ø“qÉ¤gkm_õ—ƒ…ny‡¾v}™ô%sÅ;94™G§4‡­@^$…ËÓüed_Ñûêèš¥ïØºÌÆ{"ÿJzûCBXì6QÓ?ô$ı¥iL÷ÜG¦•;IËz@ôI™§9ã¥iNFê±IÈÛ°QÇœ	Ûwv#ÃºhŠùbáE
1\ê02z³{=çb…{”aë!ZVÍlWnqò$Ã,¿Ë|0¦A(jB:“)ÙÀf¼`?È…" —>ü,‰'²h$ólËª Í7€:fz&ğk1úp=tkÂ¤ÑÍ Ï}~]¤^îÁ_ï°|ÏZ²>şC‡ÙÂ{/;‚3Îr®ÜÜÅÔˆ*MŒdM‹È]‡ƒMŠ¨»şúHŒ¤&ã	ÌŠ„
¢—¬HL«üÏËŠÈK³÷œˆĞµ¶ŒÈ‘R«„„€Æ´È½ù	Ë‹ğ~*ª/ûµF¥JGæUv;)É~šqL‹°@¥a¸É±3yx…Ú›ù:Ñ†ä\ôD¦.çˆhH<à)$úã&Fß˜	”Á¨*5& ©÷K*ş÷eY8ñ^]\ƒ@­×jŠä‰ˆ$â_16ş@<„ë¨IÉBhø< /03ë…s“çb=pk…ÜÚy&!œA7‘ÂŠ}g´R:™°NÁua×1v)¯ò‘§
Š—±4š%6¥#ÜvÍáÔg÷Ä€nˆs`l[A:\Ş[|²84ı/ŒÜ:¢ònuîf#n<¶cøwùí¥X¸<ëîø°7ø>à«+rñ^t_&Ò"nóbÅô´w´É“g¼š€zP{é!¤·;›¡3ø÷è98Ï0Û!q&çä†#È‚ÓTñ´«¥ &–İ…ÍHİMğº¨…Ò“Í“•<—AçŒQKó»`úåGY	Û*îØ0M7]2·â¶íRx`i×}‰Â’6«¼üTN<–r"ğ´¹«¸ ‡7^°0$¶ĞSæ›•^íğB‰ÚÔ\¥R´h^ ÒYaÇ÷mœjZèş†©§œ_aãñ –I‚g ÓIFhÿF.VŸ'H¡ñƒT$ÚY}…(—$«Ä4¥¹\³Æt%íæòxš¾zš¾Rº}J_ZŸºÒÄK&®&®¼ÄUß*œ”FÔEXµã=&ªnÉªI÷\•1&áIèI)C¯ öL“¹5k‰)ŠZ¤G,TÇ²Ú°°%s—`L{¦ ¸9)cÜ÷ğˆ%Ôw‹	‚¦-XØpÓ÷Mtæ¬ƒ%/¦Zr¸µ!A°,éĞIÃgM{¨ğØÜ)ó^Ë‚İ2@:-Z×ä:¡î[©I•j€Ò5 b€QÎ#`³`\úöÍ©üÎ fÊ€‚cÂzˆ!uk×çÈ/b›!—hvu¦tÇ~xñRà•½Xª¶é,²á>øµ†ÑŠ¡«gö]>^|ı°s3^®…s–Ã—QƒJ(oã–Œ5œ,²$êZ7Ñy|È¿˜D8án©>KœYì•?ær&W#æGpøÑ¢5ƒ•pÀ«Ö²¤¥w–Öêâr—ÛÍi¿İÂb‘º_÷øGJÍÀkVŞå_K¸ÚwK£ÑGÂ>Vê³X¢=¢Óíùyº5+P¯ËŒÀ¡[™&¼u†D—rSÑêÚ[.N]â•¸H#ãÅÂäû˜“YøX0Ö<è0ø4FZC,ƒ#±¡%ÇPB=´®ŞCC
^'˜‡ÚèÜ†Ì…%Jwø
µâ¨…R­32œmv¯w?Â}Ø³{İáºŸ!gë*$ÿZj‚²ú3ú³pİŞúğ±Ã•—%M;ÆÛj Zü6ùm:=æçPæÍk^øƒŞÆ³ÁªüñR÷q%ñH9“H Á£ÛYÄ¨ÂL}ŸŠÙ¤ƒ«šœW:kƒk‹è-*pÁÜ¸luØã;º`¢<+…êŸ (¤J[ôI[Ë«¸„Â±Çìv¡)"À®av1Œ–	}ÚR«U*/x³¯U¨,¼W/“¼×2¼°]¢™µHêŞ¸ğçèYâ¸âá8ƒ›¾‡„u"ñ ı•š©TÍ |`5×ªöÕ¯6Lğ?ş¤ª‡Ë°`m®M[>JÕÖ!6²ö¯[rdp¸Ê•*èÄA…ÊAyd2À[t>¢ôë‚c0s!
ëC òêzTŒi¾è°ÂÑãD‡¥]Ã€8]$Ãrºˆ^}a§*åcKƒãOç5¨Ô©ø³õñ\5IcTö…â$ÍœÂÛ;y*æŞğT°¥ô¬wç¤Ã±z£İ:åÍåÊí œ^É•á+g”åDœêÇ´vÓ”å%C3¡¨”İ â°Xï'K €«•D>8‚âï|ôuŸ±PWy¶ê1‹ç¨W÷€ ¢ƒæ©›­4K3Uoö€9MH¥»4àa¾÷åœÌ3²«Ÿk•]k?ù‰ug*u<fòf±×Sj
5±P£8y´V#8¤P–KŸÑ­Òôê·`!ú—¼Y]­s/4¶Êæ8®µ‘Gÿ«E1ûê^!ùĞ¿0²nQ|ÚlTêöÚMâ'v»T±F¸9Í×6¯Å»[ä,ØH¢WY5gÕşÁ‚ƒÕ¥Åå6]/Y Mw1BÈcx§¼f»ÚÂ„Ñúv«Kè¿Ş ïjeSûı&}æÇğz=TS4±DHçX£  †¸tÅM“XŒ<×òÏÂ}ÓûPÛÀD!K.ääÏÏ¼şÖ›U?ü¬åbvôMW aÙT !uFk5¡Ô˜ñKÁlîR@gù†LrOXhÁá šÚ7éŞ˜«”1yg“qp°Ì»¢òa$ otnw¯wß¡µV<š³úŒ5\„o¶Ë 0äğeëëY*utµ:É~EÎìĞ³=)ÉI"­iÊV?á°ßXÖEwño°*,kÛÁ*´ÖmP3^q*UF:Fä1-1¶§°Òngôº/9AiY'Ä‘R.f5GßÖ¤ãì5,À¤ÓIÜeSpÃ.›ı¸ı¯{Õ03UõE†QöQÔÌ%‘P>Iœ,K²“Ó£Æ@&!ÂË(ÒÀ!J¹tSõíˆˆ7,Heˆuò¹gB/Î$°ª4Kº¿ÂNÛØÉ) K >¯¿eB²2£kCÑ‹šÊ\$!?å†Rz*’ÕŞ¯üƒT K_XTÎ®ÑóKèÈ}$(‘8Xç&Kg`ƒî–Êš©ĞZz:¢>‹´áÈøökT¾ˆÂ•^Í–Ék^©Õ¦âÊ…ãM®re›LAÜ–`üñcLGÚ¨Ñ|´%'v•½mŒD1ı:U(º“6ŸmüìâBQÖløãœÔvJàvi¸ >U!é4÷TÉ/“5±1j#àÖº$a÷N_Ù.i0…«ÅSËñÍ$1e›lr8}<ğA­¯"X_y)õ…¡^i|õZáàìb©^Ö¢0˜	¯t¶öúº"]9/¥•_}~§ó)ÖCQÉÃØîRŸcz@çybá€öv÷jŒ€îSêÕ™`Ó^uç¹¥,´vŒs:ª°>âYXì°ø°Î	lvÃ1cÑKè>=8ıê^ÙbªY¥%Øy9kÑLU/üuiÊî´e‰¶‘É¼ƒßšZÏê;™ÊÂ-PğÿŞù¤ó]Ë˜]x1ğë§p§¯/í$wãm×_ò¯:¿ïüîÇşš¿ëücçŸèË~Fÿûi_¢i@Ô‘Iô÷¥ 	¦ú4‚dªZ-£dŞ8Ù/Q[‹t‡(ú8³fly)öô!&v¨ÏwWkë×"İNÛˆW
ŞÇzª‰x™	­ÉÑ4 :}iEœ»Q¥NÔ?øgåñÇw”(YÛ‘XßılØÆ'Û“úJw»»8Nß³sBÔWæ\ñöOÑŠM€$¿ê$„ÄÎ&ùóuş‹¹®ĞÂëîÿµŒğ¾kùDG7p<È(Hœ-
¨ĞSÛˆ:"¡ŸQ‡D¯«á{ÖÖÜ“~5Kå‡ÉOS’¥Uâ{úÏ™¾æ²¥E¬eı:q!€¹cMXbb±mµNÊ* ®¾N$%OåHl	'‚]ã•2¼n&‰³2˜†~àKtìŒ'ù¡O"ß5vñr2_¡é&ú¿pãPP™÷ÈâJ©Ñx›Që]ÖQøê–¦6&Ÿ.Ü;½à_â’Mf®D•†2N¯= +8V| D D>ã|ª¢‚~‘©lÙZ}9”jÈ±_»(ìrÑ„"¯{]1öê ²bNtQõçáàï9ÈvÀÉÀk|y(XÀ¢yûjÔ@iÏ–úaüç‘m³?u¾¸Ç
¤CŞ~#:5‘TÔ¥•‰ÃdúA™#L~É[ª´Ì—‘ÅHƒ£ë]`eÏ«HfQ(b§¨¸¥ÿüÆ0Âfa),ÿ˜˜1mU©g1›Ì£ªğ6¥qM‹Y‡,ª©^o'ˆsÏæçòs…ây['ìjäRRÔ8\ªòäÃ¹©üâÂùT¼.íÅw#İİñ˜Ü,hÚ$WäÂÜ¶“+&’Àš¦lÑÓO¥˜2åèi;ÙıÀ ™ ‡OIÅ*>%è$~­‰_+Í{ÇÊ~]·¤qŸn!jp¢L˜Ù¼§#gÉ}ÄÊõ¯é–éí$âba27©!~7Ğ»t›Ø×…âÒ?ü<o°!€2©B­õ»Ô^[ÿ/W¾#¦v.ç_û6ª`Ù-Sµw3AÒfDRdCíaB»ÇíS:=Y¥ª_în‘FÜÄ6ËÁêü4şø—k¤ßqo€r»Eİ‘• \¦—¾4|ntí÷˜h¤?Smı–J†àÍùè5–Y*Ú)¿<iXM¡WœekÿX$Ol;s”îLí~, kÃFç!+Ü`2DGİ§ïà²€è-½¶Ú™ô6|ûâ™å†—Âô²©Ÿ7ÖË;<®ª-Hì6Õ*,GÖñœ©Jg„•ñ‡1|›ğşŞĞHDÍÒ¹)üËUoc–ÎÆù/ˆdMœ%Šj/¨ï#vïÔ×¬YKØaT¨¢¸RL‚{rKÿ,½
HÙ(á!OÈõ)½à„Æ§òK Ã3¯;_è‹‡-¥àİ¾7Lâ{c»à£8l)ë›Ê¢k“¹O·ÿÃKFöæVÉOæ—
SEØ+ı³+»Å–=y‚´u1ÇÇëùş&Óu÷jdQÿùş`UyÁR`hÁ6É…:½W®Š-æk¢ˆı7Xè´ÚÙ2ˆ6°áÃÔN‹p÷ou?b€†Û!¥òËAãŞî¬gÎ?#º.ğºPíÈĞkpÒfXµÌ‹³8ßÖBëFÉ˜{,”X›67Óïc-»!^—q!¼Ï<?ú•÷éĞpßvßã°²«İ_Ó/Ü¢_Üdoú€µ`G¼P«ö+VYF?ş {Ó0º¼@—9e”‚è§ŞÁ‡ß¶ ¸ºÍåw¬¨à	^œ¬3˜†j‹ml„@¾Ã[ÍùdÒ¯ ò8¿ØŸãÀª8amÀRd~7Œä-8çé–ÁûkÂó¿¬Òï`åáVbè1Öz5Å?d‰ŒØŒ1…6-ğ"]ÚÀTÓ=^}Ù±øÜ/UJuúÿ»7…¿Åg€M‚ru Ó÷'\¨bP¶ƒù5Ü0›œèTğŞw¾Â…¼”ØŸp‚ßg¿¯ä²»ÂU1÷YõâGPÑ_ñÊÎ5
AçìÛûXßr?„«–'^„.àuğ\\ˆÚH fxØ½6Œ"çÚîÍÍùÛİBÄn\mÆİì:A_àµŠHÌC\ä[À
îÜÀî.K;\¤·è²’!ps—3¾o±ÊTş¨Y,oB}Æ3Ë¿2”mTtÁßÔÍ$ãuİ`íN÷ñ{kÈ ¿ÍÇÚiÿë –6¶«yb‡³ô/¼ ë>B­<ày^®.2_ßÅz¿PÙÜWÜæãK¦åºWeÅ2ÌGê
Ï8ãdnº²;¢'f¨HÕù}BIn³Êãët>·xv}#Ë´ëšPô3.,A±}K¹sØ3<F‰ºvI}hç‰Ã†4 Ô²ã'Øûy8¬‚á*7ŒCƒ°ÇÈ±©5ö¦#^~zUGS†º:ôõœ›¡EÚÁ‚kì.If£ûeğXá£›ÒÜáaë÷²WŸÁøà@.Ÿ“ı%=Xü¹|T@8–1“×X"	:‘×4Êº5¨nßÆ‚WT	ÿÂµİ ¯	,…m&ŸÛ–Ú5{MšÛË•°Œ‚oÍ¢øa¶Ô\ğfcev±Ÿv`q'Â¨+yn¥é·ƒ²‡i0Œ©$JõÆóaşÅ¦H‡pÈ¨9mÀôëG‚Òü[ôüWÏË*	}Ê#Ş\©]méñàd¿¨×µéKê»Ë~jx*U 'Î–Z¥3Ø±à€”â5ÏåÏëÏ®ÌáÙgÔüìKê uïpSøéá?)ç×«>ı¡>KO·ãÅ@r*šç¿F€-m¤Ô$ÍV@w‹Ë}á?›@ätöÔqö¦ú³tó¦iÅÔÆâkÔ —uİ\9–š@Z¤úÀ—Ñ‚’ù))3oô6´#Ä9’ŞBrdZ"x›iÓÍÌZ}!©ÊË«NÚ£%ÖšT$ÊOQBùÒûĞ–±!ÿ$$c¼ª¥YÛìJ"mê‚è°Cà3ÔÄş^0 ¢jMo¨ê¤oc&I¬Cšª‡ü iƒ8Û q¦¨µŞ1œ*Fm!¾ågjûS]k²ìN^`'X£,":ÒÔ1? R_ñ“ŠÛZk¸=¿ıyÿĞı=»{‘Ã¿ÊÊ‘·ÉxşÄ¡<µ}B+€
e¼Ek¹õpUK
ÉÄ#äQ§áWpÑ˜åo¹ì5¨®€ù©ùË 0Kzû°ØN0IO£Eß_W¹öO)?Æ ÈÈËñ<üô>@ğ ßsÜVìi<½xÑ†:­ÂœÃÎxÓq´é+£¡ë"õ°‰QÈYÇx*†æºWw0ûú6PÅ`· pôC(Å¾#=BIfÄJ[>=@N3H Ä §m¸Ò²—S«Ô¡p³0Æjıã&„9ÍÃJ½®—ú†2×ä<	9íí7°Œ¹©Ÿã9­ÒŒ ¡Ş~ƒ¼÷yF,4yæ–á¥e)ÓïD	Ëû(A eÊšÉ‚ñxÁ`d„›Ëìïk”Â¬€II¶jÚãEYõ€¡”6^G+¯YfNa3š\¾xjpbÄgùÜÔäùÁÖ$×ŒLÍhtÉ`O8kr‰'O†ÉŒT¦-	ıÅm n¶WT#"ÄßĞ‰Æ8V²Ç÷ ".¡ë÷}õ¤HÂ;ÑùŞ‰¥aVKèJjXNºMÑx2NÌ¤=d+"’'²=‘'}á›…şŒK·¨4f‹O·J­¶A\ã=]Ã”ÏÅdĞX~èefCÌÓıÖ¼ü¼ChºWw)¢Å8În"xÒ>WDìµÚ©OËË;òÆN¨ÓÕZpàõ‹µšğ“Şy.6ïô„¶ k°Ó+ÅJ‰ÄleŞWè„°Ğø¶µ¨‚N8­Åä2·ƒ;¢Ì„ˆ“ÑİÀ®ƒpLm6–)XïêFç²ÔŞq\@‘¹à¤ró§7=hĞ–Á8å(£İêŞÂÜ7£–9¸–€2$–“0)B°:«J¡,"rç'råÅƒ¬ÌÕ‡ùh3^\Ôà˜µr¿|_}z°šSŸCŸËØ1¤·&9
Õm.Í¾—w,Ÿ{6_¢ÿ+Ÿï·a@lÃ¹Ò¬GåƒrQAÅŸ3HúùW•|¾OİsHò²ıâ0t"Ø?0ÑÊO£RÙh”Š‰dfr!NZbN‘Ù6ƒ$‚¤ª	úO×ƒ½
Óá&cyjZQÖjÑŒƒÄzÁ’¢#A9Íxù^oÏW©¤£õê‡M¯Í~Ê¾&+$E°3PÁ‰:pğÑä†‘êütôë(f$¥"TM²Qı2=˜·†4nüebZT½ÈÓºÉPÄh¾7o;æ&+‡g½ÎM&Îãh‰O
ã¹©‘±ó†ô¬{ñ·Aj8hw6QãjBJzXPŞ„ı3=¹…'!ïâTAUQèÍìÙ8·¹•L¼Ù@¡ûcî7pó°âŞ<Cİì~lvá}¤”+P[³0+\‡U^‹õáİ}‘˜c±4MÎOŒeÉÄØy»7‘`·~²ò¯Ôš&Çêsà-»€*8M
¹q—<:İÓ Pê^)p€bvxÏøv.‚4cB°Ë_È_ GÿÂ³£3“Å¹	Cì‹VbgƒN½êO:^É’Ã"D*Ğ¦éZ6Ö îû´O— !/B¤—ËUù$úwü’ş˜NãªEÿ­$vø•ä'¦4Ç/|¿v`%ÓªaAW¥:Ê›e%ğİùšùVW¯õTÿhPJN”ôÿƒ‹TnŸnT+-à=Â”°ÓÃ³Ÿ"•ÄgíÇˆTR¦ÿó’a¶Rm†ş›Pxz´’‘JÄì>SãùŒPr=vôö¨|‰„fòhÀK¢P¡—°¿*áK¢ÉÔã—ökzCê°lïRÑ¿´gñLz¦¦äClı¸Æº»0Ù@,}½]tØ!Ğ;òp4!äp‡sã!ïƒ<-¦èDl/!“pØÅ Ôı V2•RK¡{Ikl!‹Ë—8ë”˜õ^ãy¬»V|œ,( 8ùæQ2ùGßg¡GÖ¹·‘lÒQ7>¸—¸Ş}ŠøòcB|™Ü-ÀxÁÓ-¯A
Ó„>²òYjş}9½ày-ó¸®I`é!±j)‹|T…\yæsÖò)µİ~'8- 8\WÔabw.¾™çe7G¼šÏv¸¥|[DIJ¤Y¤šYöá¨P4©tzFÖU2ß*‹—çµ%-úyê|Çe2Ã•½Gº"$@mi'ÈA»’5•cCUİ°ßX²ªl"–âHÖ•K5ja§A¼Z´¯®œ"	ã³#/iÃO;gQçŒ$¸Î™Zôì;êœI7M3z2šğ&Nı25‡S¿ŒÔæãĞ:#¢0;ì q¨¹`’c"ßM5ÿØì„ÒP…j‚é·!#©ÊgÈs5¤v£›",XFÀbìcGì÷¨hyçYmMÂ~ïëòXßˆÀók!ğ<ï	ù‡ÿnUò"y{‡/±“@÷`û’x¾NjÉd[R´Xå?%ªô,a	èó|¦ó¨ó5[PXîó/ï§‹îcÆºÃ9ÂeÈÚŒïeîìqìFrmD¢¿¬{ğ L¹*qÂ/Û(î˜MûÅR—ÔcQ’µMÉ¥Ei°íIÎ­I©»9š|¼!½WrFsÏÕàÛ‘É×§Ñ7³¯?ÃÏÄ¯Í«j–gÍ]ah¢8MŞğ–TÂo€LTaôá~ÿÂáıƒÉ‚Ø#Ç¥‘@»8òÉt>á˜Q«&`÷ØŠ1E‘€g±’Ù:WÍÅ]L°úò9ıÙZó%ÅHl­~5qw¾ -jĞßu Ş!TCÖ0^³ãh„Å×›9vx!ğk¥á‹Ş2akrFï»XØÈŒ.z¢b3ş’AWÀ7 û‹yO¯—š«…Ro…'bçFı…üâN}ca_Úb=³‰x)ÎOØm%;¬Öºó@´ª?ôçæ@ÿ±ó§ò®ÚÏ,•K¤­0P‘Ün×ßÄ\äÕsğ×ØXƒv§ûŞ×mO—ZUvtş½{ kÌ>5çˆÏ>Q÷·¬ˆòd—|ºÈ}ˆ l#]eïb+éŒ_õ  İiSĞà:ä æ¨H­R?0div¬•–TtBòÁ{{Œ¤r5ÚÂËg¥ é«§š¥Í=¡½ÆÀ¶®ËrL/`üEŠÅ»
;ıõ²ÿŞ»iÒŠ §ØÔ¤[ÙêH‘¸5aóò¾í<èŞêÜè‡h7g˜¿Ğ¢9XoU†KÕJ©	æ¸q×X÷•ë¶±kc8ùtÍ÷!i8¿;+>¼|Zkj¥Z­rôüGSm É-&G=¿æµ‚eòr8ğH¼÷Ú%äà,lªİ_Oš»4Š–·yÕÆ&çrş‘p?ŠvŞCL~m`Iƒ³cxd¹^ªUÊäd)€ô~»Ş"¬ÎÓWU‡™‡l€æ•îzV’ÇlZíÌ°B„ñ8~ùv„øø»™Î·|)lwÖv(VoSq‘
‹×ĞNà:m1p›Rp´¤Ğ*¿V”ÑˆjR	d¹JÌÊr 4N¨oÂK÷eaõi»;Ö•I©¨¶6&ÒX„åRPj h–q®œ+dI1KF³d,KÆ³d"K^Ê’É,™Ê’BC×Û5Ø0>‘&¯÷§´µ_¤KÙ’“
OåŞú¾òdñAJ¬ü$XQÅb$9ø\rùb?}$Õ-„ˆNu(AØ¢+–Êí-oì°zuÖq¶ÿévÙùı™6Æ¨ÕÁXdrºò®GšÕŠº<›_ğñ)`StãIVÀÿX/8kËb¯¤@ó3Ï°döb \Ù«jÖ,£ËÏ?)jØ¨dQOC1%¾şÀu¬¸òRÅ:&ƒyÉ<U¶yúnAƒÎ7À¸‚ÛæŠ .äd6„‚0'’?k‚±àÆµC¿yÈ®A-#ß¤P².6éê>}Ãª!‡Ü§¼~*z7ß™èı”ÎçulG` …@Z±Ø&×€Ó„dN–fÀ…ªHèJÜ«b—?å#u‡Gíb×LÊ—ğmØëï‚gƒ~*t{º_±*G%`"F7#ï.Hè„ìiHÅÖäPnø| â6&¦ŸL‰+¨ày/J_½$Ú¾¿ôƒªËÄX›±¢[d‘Pù]¾¸ÌÁ»X“©—÷
…óÌŸ{JôúoáÂğåÎ¿1MÔFLd§ç÷Y¥•Ö-7V:¸yÏvŸxÂÒ>UZ¨êşˆy²1ué&z3L-ÒFv!z>’/‰Î†şò;ßb9!Ä‹İ£$3AÍ¨î5³µ»Éƒ™ÀÑ&0D‰áÈÄ¦/„ªHq!™gO%ÙK“÷°²‘Ìè æÌ%x¡ù«ğ«PšpL&Z²¤2»d)IdÅ‚ÕÒŒW%¯@¯ÄÓŒCò
‘pR ŞÂ ëÈ”Ú§Œ­¬{cŸ
XÜ*Õ&KéòNxûTTì2°H£ÖäOô%Tºï{ÇrÍéØ5áÙ^$…ËïØºz–Ù|Ç”=àÏùÌ£×>O¡/Ët®R÷ôH%ìeç[†v<ŸËëŸ<ğZí núµáù0"·Ù%gÑúO¤13<Ê«Ò€Õ]hyö[cFüDÏ4šÃ–R–>]´)­‡Óa#Söê¦îWp/^v)¿Q¸Ky´Š\Úú„ülš¬°7Ìµü× y$SØwùyòùù´	 :VøÚo{—j#K	Ş{ä§äg„šãóuH+kc´á55æ«9U¡:S¹`ô>ƒ{Khş¹CXwF^Ó6ó3«!¿jvW›­’
¿d/Ã¹¢
tˆ-õRø=–tø&KæsÆä ;¸Íwı{ï‘¼}ôÔ ¦8èíÈ‚ŞôkU¿”ò§Í%ôò•Ê~c™^ê\.—‹™Æ&ƒèe¦‘À#¹œbÙ}2p‡çíşÌ)Ğ_nphV)ôó©ú­8à«İèç½J •‚x*‚ôA|ØTñóóG%~şPèÓ>bùÓ÷´ãqù‘†Ÿ,Õ½ê49ıò'o±â]rÃ¬I‡¿Dÿ7•Æ»{’òĞ–FEóŒB€æ¹>úXÂóãî˜àJ´5„/-L$Š^ûFï'7 q&£ê&—ê‹¥&¾,l?zåc;!‘qTœ^ˆ’‹Hg¯lp&Ø®ãƒVCÎ„Ûõ0İı.0(0Ü³«ÑˆWz8½a—è¨±D·0İ±qŒCUp[ÁÓ*@Îè|;ñkÄ‘w4H‰b±#X»×Y k,º–@6ĞÕ¸6Ñ^Ô‘19!¥ÿ GTĞšÓ,c0«AG½Tæ+ujÌìÛ•f›î÷wÀ¥Jkn·8ú¬®hİô®DZUéŞàÁ~»=NÜĞ«  ¾(#éÎÅ®aØ¹÷¡„—|Á¿v›ïfÄ*£‹8Ñ%‘Åp!A2Ç¤ÊØ’Fª:„(hñ¡
ÓSä "U‘×úL;x?[!‚ÈB©‚)’°lCw2§ç™Ûbbm	°½mÅA!’Üçğ)ºğ„ƒ.'ÕÒ²O% àÍã¸Y:qiÃ]´VåôuÀ£Ò›˜„¼£´šs¢%+àè=a‡-i½	P¤""."{$j!n7šÿ22Bì+`¿ûsá=Ÿ™ñ¹8¬ù8Ür$p4[ËUÏÙ êÍµ‘-‚ËÏ¿ãBUĞòx’-¸Ñ¿Icîx.§@¡N“¡!…¡,ÌKóÚ¥À#e(³¦B§á=3`Ç=…áâ/Ì3ÅAc!Å˜Êˆ1,Œ7–.ó3¥Lql<[ŸÌ
Ù|ntßyóÉ5v\}ÇÍ´È­çğJ;9#|)Ê,ã®˜mö2ı´îˆqÔp™ÁÈ©Û
RéyT‘Âñ@:àzğq½«Ìä³ÑvLBOĞ>&híïéI4ÇŒOmÖz»¦Ó*`ìs/õ6*NoLæä÷]~>›ÎÀåí¸¾G0î ëÎ$E,è2?Aùh8²<N6Œ0šM6>Bu™â¦g,Òg,&BóÒsÉ{{ÈSæÆ§¼üÌyVGû¸™ŒvØÔbğeDÿè[íVƒ-,áõã·˜ø¨L¥uƒöJÒ¼‹ˆ†.Õ¨ƒJİ‰múCÍI¢¡“@§w¯šî¢D?¢wüXï•ôh“3
·ˆ†4kÓzÒ6FöfîŸS„z³ÚÕÌ€„ı)ÉŒR+z„:Â­z«¥L!›¬×Ù·o±	zË:áRŸä0´ÃQÛîºx2Ï­Ğç½œ%“y*òÆáĞ¿†Êœè‚³~K¾Vx©B_.³íÁdÉÇDU×Ój@›G“ Íã¦À´’œA×ÜÔĞtë+krGe¹R-(i1lå®áã€Ïíğ=bÕ,q^`§Kö`éÃ¹Ÿ¸§Ãíb‚ãü¥Ó¨ò¨Ÿ'yBuynE\å²“)ïd€[ZìM¸-8ë÷@"7m	‘©¢¡³æåñ.I>¸·c:W˜Àµ©)àÕ)UC~Ì³¯85:s¡ĞXÊânjĞÓë-øºç¢‡—jºP¤6H¡È¯Ú•ö"z+økš~¼¡±÷@6ÎG§Œ&…Ób	¼9HHW3Òp°ëŸòæš¹r;€!YaÏ))ì¸F]BHõV©R·ä&ñt­dñ£G)À¢Xt#R5›¼L´›cjóò¾G¼§]·§[¯>‰’uú²ˆRÊ‚ñ`—X{ğáHO´HÊ$§D6£‰›¦pUúJ`D&R41ş]ºF^Dç
¹|ñ<)•a§ŠÏò¹©I*8Æe§„»–èÕ%¥+o¹…¾Š©$jœ;¬-{ûŞãuÍzú“šğ´×!Œ53•Õ^:àtº"ÁÒ7Å$R 7Ë8Ë¥ÍÉÔ2´Âéœ•ıÇês¾œÙŸÍ*´.ë—ë{m²ûg^FtìûèDò.<H]İë¾ß½	Ìiºâ^&¤%K‚ÓÅú!cÚÚFWô@&@x‚x:\ûDÔIòê•FRĞîµF8­Ï,ú½E÷2ã€{¿ÎğÛ<µ~Üâ(Y»Iw÷‡ø¶àGoq§™ççá$l0Ã>³MÈç±Ç‚±¡Í†ä5|ò›ÜÕ£î ºÕt·w”ÓËß¥¿¬
 JöhwYù÷@s! ©£Âf23Í¶zu‰ü³¥™ªGÎ0™ Çj?h‘£mº^÷¬u1¦(¶<9.4Tšì)Cœà×i"zÆÔô<9*â6 –Ì3ÍØïéZ¯£6Æ4ù!ß¿øVÃ««¤HI/î‰›}•Èq×F³¸kj´ÿúõï7ÿßÆ-è»òcV|,Æ@ãó7öùò]ÿ4@u‡Ÿ÷ {¾ü ÿpš†‰¶Ğ¾ĞZÍÀfÔx†Îı0Iê·Âá=rv%S.]×+ñ…ËºÕX[9ø5â"q@Œø´S#©¤Z(ñ0}&Ë«4©ƒÌ¤R€1í%¶ÖÒ iÚğhn<VøºÄfØ‚ã©^&úAÉ‘Á#Vcq_ÿúõ¾ÁMøO:¿C½¯èlßáœQ)Ú¦ØlƒáäU^Çª¦U’9Ø¤"²i®VY7¾·Ö¦éAª@5Á¨÷µ	¨Es2ğÁqe†ËZˆâ±ê:’ÿ ä%ÔÎA.=O¿ù±ñNø%¨ÜÄ/_¡ójÂàşÓÆ‘0S´«òHT+ÍÖğl¥Y&øS…úHt£„+0d•8¹ñx­QµöQì¯Vzî´‹ÁÁ¶NF^­/VûBFN”Í«Ãz&Zïşrå3,Ä£Ór•OCâÅÀLÕ-¬ÄÛâØ=À%›9Q™ı¥ßêŞ²ª‡=‰m˜ß8b9Ş€‡RÊ+Pà¥j1ƒŸ

Òı$ò3\Ó,wVŸ¤=]*ú_º*½(¯"5’‘ºœ£~%(àğF¯A”Ä¯Ó7aK#W¶…(İÚv6ûGÚUkMßÿDMü­¤‰ˆæK†?†zöGWL\äE‘¥É ¶…–Fò#WÅÇê˜1ğf9lDd{!zC ´6/~ß:&}º7Ì÷L°„ï=<í{†^²xäÌrÃsêBwÂkzµŠ‘˜=:8Ñt¦x„º”³äì13L¼kCû Ä?²şìcºç«¦Zf¼é£F>ÃøHËpƒÖÓp½âãì­±9Ö$gÂücyyƒäÕ¼ T†)Ôãô×¯¿şOèØ¤ÂîcNV±M2ªİ/‰½fÏl¶£¼ÿ‡œ\XnVÊMr
U›;ÄÀoz}-4:€ÿJ¥±!Ü+#õ+°lbéPúìJ¡Ëÿ¼WÆæ`½ÒôÉqoÑ«>–±Û+q¢´„ˆÚeŠù±IÑÔ8–ŸÒ‡DùíÕ @Ïlà5›Ôy,ãòze~üt½µ–Á˜<uŠTÇ_¿şõ×Â#UzD‡ª¾_îü©û>0øò`$dşÈt'ºµ\%™×œÚNÑ·ÉxãUôˆğWûJì:ç‚,>pá W]n6Í¦šËjON•xOÈ%DŒ\5ùÙS'wâ¨)f†;^‰ğÔmÔJUtËÄÚ¯F0Qše¬çÙÄt÷Z”¯•Ö „£O\xÓ£ãõH’Î@ù¡8ˆ8ÓÄöbs¶^Yô‚&}ÿS^6äÉJÃÛ`¤xd„9RÃÇ+­á#Ş\©]5#È¸¬†}9ÙÄåÿ6]ôZøFåÕZ%=
Š% Ûùg|Ç‘„äà5®%V4å^%T‡©Å-hº…Ì@Tİş¨wá‹ §s¹y±˜3#K:n¾OYÔ6&*»7`—ºòfèÌ¿íWÛ5D]éP0Ñ½Î±ÃûÏ¼Œ’—MØ™ª©è›N“|n*K½¿–•\ËÓ¤Hßl˜Êî¼q-4êÿS•ß§ÿ-”ßÔÏ“¹p}ÆÕÛ6K…“áä'À×Å®voî½×CôÛûî§ñœ#•RÕŸo{§—›-¯Fu^½4oŠ;ù±Ç!®ÉHa%Ï]ª®woc•æRyZıc,2üãîõ=¤í Qõµ „p+*0°Šj‹Ê>éşóAt)vVAdĞÕÃ Ÿc¾Õ]F€C•+g¢cT8Ê,¨Ü½…Qä«°? M6°0 ÌëÍ¶BFqÑÀÏ|÷;,®]šM$lL”»ôuÏğ¶T–¼­ÃÒùÏ:}B^ÿÂO÷&%s%€e*_læX ã6aêÂëlHd!ĞÙ0ŞÚÊêi¶ù0Ü#hŠÉµ¨ÔfnÂ,yÀ»B-\÷šùNˆÒŞYf9wúxw½ÊÚ`$€³†=]ZôÒ«°­ï¸åÍZö©:ëU}ö¿…:§Kù@ÁæôÜ¼€–[P‚ÂK0XÒëj—Àß&™_xu Ÿ9QYò‚½ Ó>Ã$Uø¸P‚z“0Ô ¥DJ)¹hf	û[Ş.êY…°d-ƒÌÃZeI°¾¯ÁˆzáCæîIéïîM7Á¸¿ìÏz/lÏVü×KõÙªìÁè ÷t6Wöüle(»†Ql°‚D› ¥“µ»N·ëşÈYz§}b–7°x´Ò†ĞF(òƒÂzßÈ6Ô°®XÔ
£Ef‹
×û(­·ø›ŸöZ-:÷ÍÈÒáïï ‘ÖĞsÂ$Ä&N¿æÙc‡°çàôÜÒ¡
xá„`ÂÖ!7Ûı<ÜÒÎØBÓé
P3›(
°	„-¿S¡ÛóM„ÏN+>Ü?Ró!è’KüEıyøûG²*Ş“ÔÙòêe/Ô!PÄÍyŠ$†I*ÜŞŸ:U\~…•B{³ì‰òËıÑÅ^¡B¥05¦¬¬Hêù%[¦xÑqìK…ä—¼¥JË|E!¶üÈ™z»ZML“¬Ò ø>/=m8OŞ>WÌç¥fÂ©ql„ÄÆ1DA}Nø”ĞÂ+Ëß…0¤ßnÉåä‰i7!ŠL2D‘I£1ièeâõí)1İEyòÌK ·Àa2Q3LHÊÎ˜ÄÀğÖ­ê¼R™ì?İ(«^Sª\gˆ’ª:uİŞÓ³*ÊÙåN5~`fA£ÑsÙÆì8!Üq k\å<èJÉL3”¥•ü)»cÅĞaLu5ÕĞ¹²Xx)WWÆœ”âÔUÄm;c´›õÌ^®I÷ÄIº'KóØOšQv§©%Aú›ŠNÎXØ˜²é1 K]IÒã —&Í’û5Ó€¢îLv®(¦ëgŠ¦ÜVÏ\Tù3ce“ûÊè›_OMºªõPåÛètHL‹`'êUAL§ğS
Yƒv‰ô‹ëdzjŞÎ>•_¼t^€û½4¾¸p>œìsÅ  ’Íäš%"a†­æÏ­,^„r.¤úªÒ† !ƒ·×Ÿ›)Lœç-ØØ”!ş@wíÔÈØyŞy¯à¥Nn4Å¦ŠŸ’ÚÁ*lBÄ#Œ™éÍÅPùwH	ªjÓMV®}æ¡¦W ´Şœ^pÒ§¯º|`¨î‹’_1Ù`F=]Õ1¶¦õn:]¶€=ş–a}Û†/^@dÆÜ/óé—d¸ó†.Ğq¨_TBŞÜo@Q¡nfúXpÄ¡{'¡áh,f¤yÍÀ½ØO«í‡fZ3t¯Nf¦ }Åc)}òQİÄm“ñmÎDs‡G$ï§ddéÉU‹ò\ï`@+À>™Ç¼	µÃŠ_£º’?Ø&¼m¨ö*U½¯ÿRxŞt¾LT!æIMæT³f÷ ÌKï0õo8d	¤{·ğ<Åif+B{·8N3§òÚ]Y¹q´+b°:Õy(q|{Ø\Cğ7uärVÇS¾¤DAí PÔíØbëdæJÕ¦®;öâE³<KÍGåü)?ÀIC)ÑFÔ¾ÚşÃŞbà×{s-W}„ç)ó$&Jß„ m ¬XQµé™ªo ‡¸)ËV©F›91a•¤×ç¨lb@³³¤FtP¡°lLÄ©X5C©gxÑƒ˜,Ó\UyÃÎ‘ŠDo@ªÿsÍğ.5O²3RjäêûS8ÙÕU¬8X)5o{TëèˆrL®kEAß‚ !{<¶õ£­]Ğ¨Ô@&…Az—+‡ÀÙ×muîP%ŞCºm&eØÏXq«+±Ì£€‡6[¨’š5ÛFi¶6	ci> 
IliˆÌ]gvù¦:&eßº×Y¸r¥â+ÇBO©ÆhÔà©Ôˆ"œ¼¬ÁÚÑO–ZØ#£ŞC)æ2ºÌO&ÎLR’©¯Ô Æ™¥Y^p`…wØ}×½2MOÿ¯Ë8üòê‡O{ërÚ!"Fİ‰qæNHTz¶ii½„Ùır»9í·[P–È°ÙGü&lâÎ)_‹×5kªwÒ‡–ŸÜí‚îŞ±²_×îişœ7ÌÑc'N’LÎ[òT¹ö]İ'ó•Z£MrT>­B÷C.—û_p÷áQxñ'd?ğ‰}º!bwşE©¡İ
åå’RsSŞ¬ß9êÓ•W}<û!€§êcCœŠ×Ç8Âv^ç	Ú8½O·CìÎ‡ü%ívàÕá†MşCUlxÔK~†İ–;=®ûCé“b0å
9WÜÛRïÓ]Şù5KíFƒ‘^ ¨î^5¼nà_äêÅC-®
nW7LÕ/—ª}èˆã±óåW<váLPªÔéD]8Rj•MVBnËôPë‘D¯c4«ºêO¡ì$\rİ›YÄeb@©¦	QRqf Á\9Zêai,zÎm¬¹Îš¥ÂeŞ½•<ÔtÔsĞ'D_d•ã!pË}„~	MYıbC!t¿†d¦D^—Zµ ¦*ù3Õ¦X|Äª¦¥@c8\œ3Ó¼ÖúÆËÖ-È¾Ê…R0”õšYl#¤Şˆ‡tÛâ°vé*J7ì8 K`r¾"Ø÷3Æ$b¸êÿË´<îa‡snrß¼
E&Lpà§øˆ×ô°¨©ş¶nB—Ä&¼7øeÑ‚\§²ÿlÅ-¼ŒCt4èükdól¥	(¼³V*M~
bËIiêƒ>Joj7°E6)§ö#ñ©¬ ºÛ™‰W,ŞZĞ\©j>´é“SŞt!¾¤‰bË•=Bã?§i¬Àá¹ÍF¥•7Švhò2êKk0X‡ro=êî£~—àY¿êÜZ^é<QjÂì†·RVxÓ0ª¦‚„ìŠğ¼1'š-(]¥k\êšÆ].KÈÉSoıí«‡Ï\8qğô™WO]8tüì«'O{óL®6‹º	i·é0=`zFà>ĞÔéÇÛ-z”Ê]«Â~)ZtÆîæ8Å³~—‹M§ßĞÍš"Aê²‘İ²!¯è¤0¸ôÔè—©Ã¨1¡-_bÈv8Va­“Û`E…PJB‹ê¼C±’fÌ¾•Ü%²½%uÿhN· [ƒCÇ÷Z	ƒç* Ÿˆb—zCÅL¼Œc‡Å0ËÀìX c+…Š¢ŒÉfÂc¾°U´Œ*ëdcL²ù¼ ’});5•-gó¹bŠI–âNåÎ÷Qö2’Q·üá€Ì~-Vi@?åşºRFx=õh˜FSÊ7Q2gnÇä
—{Œº‚Imí„²#Ã½v‚^½Í7ÿ´º¹s•œ=F“¹IMÃÁÂhß¾s<@¤`>Üx†BjP=Dwg•1p 	4z0#k=¤;éŞˆz·èÛCZx¾yçöHÂEàOˆ®•¢ëê›oa÷7ë,HeÑ¸›+}X‘ìu-¢P³EñÆ	ÜJñ	’ª&àK2®®}9eqôWiy*-¸áÊµåáR»åS1Õş¢o»HÊT,P)Ó,>´ò«¬âıM‘˜T!T5‡c¿WbûjöÿñÒ²ßî­ì,ŞYe'/‡Í´…F {ùëgK $Áa„è[ìeº¯aôÑ¶Z™*»Uš!iY£	aü%ª“q<:ôf+mİÛG¯F×B+ğëó/óf÷£ğ|5?h,Tšµı#ü$ƒèÏX°D}I†Æá¨öE2T½ÏÀ&ô†¨>[¸’-Aî°ã¿ØÌ7¸/ça·ˆE÷”ÏpöØÈÙŸ	ÉxêäÑi‚]Ê×¡­kéoiY†³¤ó {‹~z±×	Z½?Ğnâ@©Åhæ·¨ÀöÑK ZÂ°Æé›úbàfÃ¼yz#ˆÎã­V†to!¢´î¦kŠô:áàâ†äÔ ­ò½€IÉ‚)¿ÁBYôç/ğİ¯²¹Ü*’ÒtM‹{ƒÑÆ4µw\è¨Âc
y”âj®ÍÆ¸šAŞL:É*³dëQ¶IE-Úou%™YÂíŒ£3ö;ŒÃ#â,–?âBÑQè™ˆLäzqär;{ùÊ9íãšX8[‡ú_ä!Ãêøœ¾İ7O¨lúï!Ñæ,F°’Û‚`µ“HæÙ¹©—F°…Ã»Á×8x<d^«Ò7„/İF0C¹?ĞòzˆĞ>ŠAÎğH—õêm¾¤oÿ-}ÿo¨6û'ú§ÏíãŠùı‹ã£ŞLêıÿƒ3¸ê…3G½R „GòÒŸvş«ó¯ô…?³¿lLÖÉ/<[.NÓş¢B]e,C?{Ôoù	NÌ’µ¿Ş×ñÍêõıõ~„z>3ƒ¦âŒ‚\ç2 ªNP®#bskHëpUæcbÃOµh_ï~>W+52ªYRAO@Ïâ¾¿Z1¼7#n7á‰$E9#ÒL´^Û¡Œ´wĞÛ´.¹„HJøoŒ®œ>J¤JXÛÊÃ©¾…¢üŞNØ¢îãH±;ƒÔf¬t€ ¨+MÚ(¼«Ô»Ğ¬éB†Yúñ>M…YŸµ«V†•vGÓò˜lŒ¯y¥
ø±“uM­ï“`iü3>Şç¬›NÚ1š¶’¦ÁiEıVEDKëYît ,Ñ]TGè,<À»ßgÀOQˆ©àÕÎïÛ-E$!ÿ}~Ò:·ùWíƒ ç>dğ”ÔŸ VİuÂ¡?&o{Á29î_Ê²N —šE ô,û#ûñl•¾*íÚ;~Ã›¾Ú‡B]º‘×ğmæ>3 21–_›Ìƒ´M&ß ™—&&é'£Å<8ÂŸDóxñå†yw·ÚíİzÑßcÁÅ&*§?ı€ü3),Åõ4wqætÊášû «K˜Uh'A¶´{£ûúÅ»ÜzFÕçŸÇºt79õ%Ÿšƒ2'ÂªŸ§ÆDâB{Ì˜HÜ[dd€‘Z©[×Wõg¼Çªıƒ(ï‹ÓÄn &æ6N[qÃİ‰Ùš£R£8¶® ÏÏö6—9³ÑOmÄXw¶CrOŠ±gåÿrUBÕÁä‹:€T8ªK{ı!>“ ]å•¤TßÜÅğÅ÷hQ:›%¯¾™%G^Í’×NÑŸOgÉßÌ’7ŞÊ’_¼®G|Äú‘»Ñêâå®ZC‡a5#ƒáÜî< ¬4RD°TñvPˆzUO\ª›4™Ê?¶Åö)l}je¤‘o¢Ñx¤+MbéŞdõÍW¶ú’9ØªùÍÆ‚TÊä5#©€ÄqmsÎˆ„ª½ãáR³UõÈéJuÁoS÷ÏËªV8µ>…!™¶ÊH=´§k;
Ö[‰«–½±Âä çÆ,ñ´LlA¬îCª~5Û/“Ùæ×éßõ7Ã8'¿
sHé¯(¥ø2[ã¥ş·8ÓI˜A”£ƒL¢‰$æ¸¸DÜ–OòjœÀTX_T å˜c*$Ğ°İ(Z¹¡É®X¼¢†d€mÙ¢Ëè
bì?o”çş¾tªÑØNé*1v¤%D
H~Tç.¥87¤"¶ß“Io€‡¥%·ì7–ÕnW«B)ï$úEò5Œ¿ü
Óª€UÊåóÃˆ\’ÄîâC@ÂĞI¨Ô½Z©EÕÀ©“GéÌÕ<röXÊ`Xô®æÕÛY²P™_öê³ÊK-Vší™­åª—#°óçqø¦	Ôñ€z(i›¹R½UjªIäª¥úl³\jxä¤úkô* {ØDzİ2j—fœYğjŞ4™-•W„áVHgÚ"órÉ É ¨ÃbCöœìfğ¬V3Ë>(5–é+ÅÑƒØT/°©nÒqkÏàÜ)¯£²áÍe€¾]3-†¢Ëú|iÿü:}‡æ4ùûäøA‘dDøŠ´|¢š|ƒª÷ÀCxe0@×L*õrµÏ¬¼ÜèäX~©X˜ÈãÀ‡¡¢,]-A¹]-Ñ'dÑº²ÈÈ“àE”W›Ag`GXI2JôÕj¨ÃÄc:¯#æÌˆ…Ißi’$Ó¨––éoåÀo6IêV÷ÎF=_õ/éŞ¡l†¬}_Êı2ğŒYÂò0h³ûräÍÒb…!NÓÌ«çßÖaTüšlzÙK•‚€ÎKD@yúôk5åµp¼rädàÏ1ÊGº»°r$ÆS4•Y(z÷šêPT´½g€ Šïd¾”è¥ +VšJpèõ›ƒ%ßæv½ÏåV*ÁµÅ›¤DIJ˜·>Q™ı¥ßêŞ2u?>ü*<Q5!æèÀ¢Bu,¨¹v\p4ÑYµM<mSßƒ	|—yõ{`!÷Ğkê{åÀ&?5uO„İŸÉn¬~o§PÂœ@^VMğŸÿS®%o£hò®œeò—+ßi‹_Õƒ¹ëœâ3†µöÿ  ÿÿ Ü¤ğxœ¬TMkÛ@½ûWLëCê`Y²R¥A1†¶‡B!PÈ±²Òä%«İewÛ5†Ğk{ÏßÈ¹¿ÂşG]ù+ñ‡Ü*ƒdfŞÎ¼÷vÈ;Ôq’{	/Ñ‹‚ ,¬70‹Ia]‚¤·P*…:%—ù¯İ@¾Õ.ÇDîEcAËRP¤^8âUNf™áÌ€P9ôF›ë†~¼nÀÎÓß‹ Ìf³ßóûùÏùù¯=@ÏOJk¥Ø=Úó)»ÛöüBVÄ:;™ÃñÖ´±N¿¬ ¿h4(Rì7V™‰
Ÿ¸L‡××Î7WDß:™NıÍycÇû“›MéN±By‰¤cPaòŒOâÜÍÆĞì†î÷^±BIm‰°—ÏŠPêî!†n'Œ45°DjŠÚÓ„²Ò8ğß¢F`$gt7aµáét‚¨US¡¬ŒWW)­¬9r&ˆqÓ:ËRI·=[ÌlF
ÆÇ1œ|FûA&\I!OÚP¸Q$Å:‹ã†}ÇØ‰»8be*¹tş4Ïeäe¤Õã¥.ã¦·8~ƒµK¾U’3ãTTÓæÙ±rZ(3éñÑñ8f¶ê~Dóÿ`:è¶÷Bá~èlKÏÊìåV:rqCdùÀ©x£+Uá¿+}±Lrˆó,¹³óš‹ÅJ1•šTK(†j—jÎÄaÉëÖ7Ó¿\,‹Àfİµ.ÓÆ   ÿÿ ñë«