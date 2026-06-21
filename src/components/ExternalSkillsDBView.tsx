import React, { useState } from "react";
import { 
  Search, 
  Copy, 
  Check, 
  Layers, 
  Cpu, 
  BrainCircuit, 
  Code,
  Zap, 
  ShieldAlert, 
  Database,
  ExternalLink,
  Sliders,
  Terminal,
  FileCode
} from "lucide-react";
import { motion, AnimatePresence } from "motion/react";

interface SkillItem {
  id: string;
  title: string;
  category: "membrane" | "mimo" | "ai-eng";
  short_desc: string;
  full_desc: string;
  tags: string[];
  code_snippet: string;
  instructions: string[];
}

const EXTERNAL_SKILLS_DATA: SkillItem[] = [
  // --- MEMBRANE DEVELOPMENT SKILLS ---
  {
    id: "mem-dynamic-auth",
    title: "Dynamic OAuth & IFrame Auth Bridge",
    category: "membrane",
    short_desc: "Cross-origin dynamic auth flow popup wrapper and iframe local-storage handshake wrapper.",
    full_desc: "Implements popup-based authentication flows designed to safely navigate strict browser cookie constraints, CSRF token generation, and pass tokens securely back to the parent iframe utilizing cross-window messenger channels.",
    tags: ["OAuth", "IFrame", "Security", "Full-Stack"],
    code_snippet: `// Client-side authentication popup controller with secure postMessage orchestration
export class MembraneAuthBridge {
  private static AUTH_POPUP_WIDTH = 600;
  private static AUTH_POPUP_HEIGHT = 700;

  public static launchAuthPopup(authUrl: string, expectedOrigin: string): Promise<string> {
    return new Promise((resolve, reject) => {
      const left = window.screenX + (window.outerWidth - this.AUTH_POPUP_WIDTH) / 2;
      const top = window.screenY + (window.outerHeight - this.AUTH_POPUP_HEIGHT) / 2;
      
      const popup = window.open(
        authUrl,
        "Membrane Auth Sync",
        \`width=\${this.AUTH_POPUP_WIDTH},height=\${this.AUTH_POPUP_HEIGHT},top=\${top},left=\${left},status=no,resizable=yes\`
      );

      if (!popup) {
        return reject(new Error("Popup blocked by browser. Please enable popups."));
      }

      const messageListener = (event: MessageEvent) => {
        if (event.origin !== expectedOrigin) return;
        if (event.data && event.data.type === "MEMBRANE_AUTH_TOKEN") {
          cleanup();
          resolve(event.data.token);
        }
      };

      const checkClosedInterval = setInterval(() => {
        if (popup.closed) {
          cleanup();
          reject(new Error("Authentication flow cancelled by user."));
        }
      }, 500);

      const cleanup = () => {
        window.removeEventListener("message", messageListener);
        clearInterval(checkClosedInterval);
      };

      window.addEventListener("message", messageListener);
    });
  }
}`,
    instructions: [
      "Define the expected callback origin properly in the third-party portal setup.",
      "Call MembraneAuthBridge.launchAuthPopup() upon clicking the Login/Connect button.",
      "The auth popup callback server route must issue window.opener.postMessage({ type: 'MEMBRANE_AUTH_TOKEN', token: ... }, targetOrigin) before self-closure."
    ]
  },
  {
    id: "mem-realtime-synergy",
    title: "Server-Authoritative Real-Time Session Manager",
    category: "membrane",
    short_desc: "Automated WebSocket lobby room connector and local optimistic state merge controller.",
    full_desc: "A highly resilient websocket connection loop equipped with customized client-side optimistic synchronization, automated state reconciliation, backoff retry schedules, and message-queue buffers to protect data integrity.",
    tags: ["WebSockets", "Optimistic State", "Synchronization", "Lobby"],
    code_snippet: `// Resilient state-sync connection loop with exponential backoff & client queue buffer
export class ResilientSyncClient {
  private ws: WebSocket | null = null;
  private retryDelay = 1000;
  private maxRetryDelay = 30000;
  private messageQueue: string[] = [];

  constructor(private url: string, private onSync: (data: any) => void) {}

  public connect(): void {
    try {
      this.ws = new WebSocket(this.url);
      
      this.ws.onopen = () => {
        console.log("[MEMBRANE] Connected. Flashing buffered messages: ", this.messageQueue.length);
        this.retryDelay = 1000;
        while (this.messageQueue.length > 0 && this.ws?.readyState === WebSocket.OPEN) {
          const msg = this.messageQueue.shift();
          if (msg) this.ws.send(msg);
        }
      };

      this.ws.onmessage = (event) => {
        const parsed = JSON.parse(event.data);
        this.onSync(parsed);
      };

      this.ws.onclose = () => {
        console.warn("[MEMBRANE] Connection closed. Scheduled reconnect...");
        this.scheduleReconnect();
      };
    } catch (e) {
      this.scheduleReconnect();
    }
  }

  public sendOptimistic(type: string, payload: any): void {
    const raw = JSON.stringify({ type, payload, timestamp: Date.now() });
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(raw);
    } else {
      this.messageQueue.push(raw);
    }
  }

  private scheduleReconnect(): void {
    setTimeout(() => {
      this.retryDelay = Math.min(this.retryDelay * 2, this.maxRetryDelay);
      this.connect();
    }, this.retryDelay);
  }
}`,
    instructions: [
      "Instantiate the client referencing the secure WS gateway.",
      "Apply incoming packets straight into local state arrays, validating against client timestamp offsets.",
      "Enqueue outbox updates safely in memory while network connection behaves unstably."
    ]
  },
  {
    id: "mem-firestore-blueprint",
    title: "Secure Firestore Schemas & Declarative Guards",
    category: "membrane",
    short_desc: "Zero-leak declarative Firestore schema structure and robust transaction helper rules.",
    full_desc: "Formulates a rigid boilerplate blueprint for Firestore collection layouts, automated transaction validation rules, and strict security guards matching User ID scopes.",
    tags: ["Firestore", "Validation", "Security Rules", "NoSQL"],
    code_snippet: `rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Helper check: Is user logged in?
    function isSignedIn() {
      return request.auth != null;
    }
    
    // Helper check: Does user own the target document?
    function isOwner(userId) {
      return isSignedIn() && request.auth.uid == userId;
    }

    match /users/{userId} {
      allow read: if isSignedIn();
      allow write: if isOwner(userId);
      
      match /castles/{castleId} {
        allow read, write: if isOwner(userId);
      }
    }
    
    match /system_stats/{docId} {
      allow read: if true;
      allow write: if isSignedIn() && request.auth.token.admin == true;
    }
  }
}`,
    instructions: [
      "Deploy rules directly using the Firebase CLI or firebase-blueprint config.",
      "Validate query bounds using strict index matching in local queries to prevent security rule abort faults.",
      "Always include auth scopes inside all fetch queries."
    ]
  },

  // --- XIAOMI MIMO DEVELOPMENT SKILLS ---
  {
    id: "mimo-anti-overheat",
    title: "GPU Hardware Overheat Safeguard (Turn-Based Unity)",
    category: "mimo",
    short_desc: "Capping framerate constraints and post-processing weights during intense gaming tests.",
    full_desc: "A crucial hardware performance coordinator that throttles rendering quality to minimum bounds and locks target Frame Rates (down to 30 FPS) if the device experiences extreme thermal load or long idling phases.",
    tags: ["Hardware Safety", "Thermal Throttling", "Framerate Lock", "Unity"],
    code_snippet: `using UnityEngine;
using UnityEngine.Rendering;

public class GPUHardwareOverheatSafeguards : MonoBehaviour
{
    [Header("Thermal Settings")]
    public int lowBatteryFrameRate = 30;
    public int standardFrameRate = 60;
    public int ultraFrameRate = 120;
    
    [Header("Post-Processing Weight Scales")]
    [Range(0f, 1f)] public float idlePostProcessWeight = 0.15f;
    public Volume globalPostProcessVolume;

    private float lastInteractionTime;
    private bool isThrottled = false;

    void Start()
    {
        lastInteractionTime = Time.time;
        ApplyGraphicsThrottling(false);
    }

    void Update()
    {
        // Detect mouse or touchscreen activity
        if (Input.anyKey || Input.touchCount > 0 || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f)
        {
            lastInteractionTime = Time.time;
            if (isThrottled) ApplyGraphicsThrottling(false);
        }
        else if (Time.time - lastInteractionTime > 180f && !isThrottled) // 3 minutes idle
        {
            ApplyGraphicsThrottling(true);
        }
    }

    private void ApplyGraphicsThrottling(bool throttle)
    {
        isThrottled = throttle;
        if (throttle)
        {
            Application.targetFrameRate = lowBatteryFrameRate;
            if (globalPostProcessVolume != null) globalPostProcessVolume.weight = idlePostProcessWeight;
            QualitySettings.vSyncCount = 1; // Limit swap events
            Debug.Log("[SAFEGUARD] GPU Throttling activated: target locked to 30 FPS.");
        }
        else
        {
            Application.targetFrameRate = standardFrameRate;
            if (globalPostProcessVolume != null) globalPostProcessVolume.weight = 1.0f;
            Debug.Log("[SAFEGUARD] Throttling released. Target set back to 60 FPS.");
        }
    }
}`,
    instructions: [
      "Attach this script to an persistent root manager in your strategic scene.",
      "Assign the Global Post-Process Volume block in the inspector slot.",
      "Enforce target frame rate constraints to stop Unity's render thread from utilizing 99% GPU during long debugging runs."
    ]
  },
  {
    id: "mimo-dangling-structure-healing",
    title: "Structure Auto-Alignment & Pipeline Parser",
    category: "mimo",
    short_desc: "Automatically fixes dangling braces, unclosed tags, and misaligned display tables.",
    full_desc: "A smart pipeline correction helper written in Node.js that scans source scripts to catch unbalanced brackets, unclosed markdown tag lines, and resolves table misalignments to keep code pristine.",
    tags: ["Automation", "Structure Healing", "Formatting", "Regex"],
    code_snippet: `// Automatically heals dangling brackets or misplaced layout brackets inside code assets
import fs from "fs-extra";
import path from "path";

export function healDanglingBrackets(filePath: string): { success: boolean; repaired: boolean } {
  let content = fs.readFileSync(filePath, "utf-8");
  let openCurly = (content.match(/\\{/g) || []).length;
  let closeCurly = (content.match(/\\}/g) || []).length;
  
  if (openCurly === closeCurly) {
    return { success: true, repaired: false };
  }

  console.warn(\`[HEALER] Bracket mismatch in \${filePath}. Open: \${openCurly}, Close: \${closeCurly}\`);
  
  if (openCurly > closeCurly) {
    // Append missing close brackets
    content = content.trim() + "\\n" + "}".repeat(openCurly - closeCurly) + "\\n";
    fs.writeFileSync(filePath, content, "utf-8");
    return { success: true, repaired: true };
  }

  // Underbalanced: too many closings
  return { success: false, repaired: false }; // Manual review recommended
}`,
    instructions: [
      "Incorporate the healer inside your git commit hook pipelines.",
      "Execute the parser after doing major programmatic regex string edits to isolate broken closing code lines.",
      "Run the compiler check immediately after repairing."
    ]
  },

  // --- HANDS-ON AI ENGINEERING SKILLS ---
  {
    id: "ai-rag-semantic-search",
    title: "Client-Side RAG Semantic Search Engine",
    category: "ai-eng",
    short_desc: "Client-side cosine similarity text chunk searcher and automated prompt context injection library.",
    full_desc: "A highly robust client-side vector chunking, matching, and semantic search implementation that fetches context relevance vectors to craft perfect RAG instructions for the LLM.",
    tags: ["RAG", "Semantic Search", "Cosine Similarity", "Vector Embeddings"],
    code_snippet: `// Lightweight client-side cosine similarity ranker for local context documents
export interface TextChunk {
  text: string;
  vector: number[];
}

export class ClientSideRAGEngine {
  // Calculates cosine similarity between two vectors
  public static calculateCosineSimilarity(vecA: number[], vecB: number[]): number {
    if (vecA.length !== vecB.length) return 0;
    let dotProduct = 0;
    let normA = 0;
    let normB = 0;
    
    for (let i = 0; i < vecA.length; i++) {
      dotProduct += vecA[i] * vecB[i];
      normA += vecA[i] * vecA[i];
      normB += vecB[i] * vecB[i];
    }
    
    if (normA === 0 || normB === 0) return 0;
    return dotProduct / (Math.sqrt(normA) * Math.sqrt(normB));
  }

  public static rankRelevantContext(queryVector: number[], corpus: TextChunk[], topK = 3): TextChunk[] {
    return corpus
      .map((chunk) => ({
        chunk,
        similarity: this.calculateCosineSimilarity(queryVector, chunk.vector)
      }))
      .sort((a, b) => b.similarity - a.similarity)
      .slice(0, topK)
      .map((item) => item.chunk);
  }
}`,
    instructions: [
      "Convert your static manuals/markdown guides into text chunks containing pre-generated semantic vectors.",
      "Input the query through an embedding service to fetch its corresponding vector.",
      "Compute topK relative context items and prepend them to the LLM system instructions as local knowledge."
    ]
  },
  {
    id: "ai-agent-tool-calling",
    title: "Autonomous Agent Tool Calling Loop",
    category: "ai-eng",
    short_desc: "Asynchronous tool calling executor loop matching strict declaration schemas with automatic function extraction.",
    full_desc: "A full-featured multi-turn agent coordinator that parses function calling schemas out of the LLM state, translates tool arguments, invokes local endpoints, and pipes output safely back to satisfy complex autonomous reasoning loops.",
    tags: ["Agentic AI", "Function Calling", "Async Orchestration", "Gemini SDK"],
    code_snippet: `import { GoogleGenAI, Type, FunctionDeclaration } from "@google/genai";

// 1. Definition of the dynamic coordinate lookup tool
const fetchCastleCoordinatesTool: FunctionDeclaration = {
  name: "fetchCastleCoordinates",
  description: "Retrieve coordinates and offsets of a calibrated castle by its index.",
  parameters: {
    type: Type.OBJECT,
    properties: {
      castleIdx: {
        type: Type.INTEGER,
        description: "Zero-based index of the castle (from 0 to 11)."
      }
    },
    required: ["castleIdx"]
  }
};

// 2. Multi-turn Tool Executor loop
export async function executeAgentWithTools(apiKey: string, prompt: string) {
  const ai = new GoogleGenAI({ apiKey });
  const chat = ai.chats.create({
    model: "gemini-2.0-flash",
    config: {
      tools: [{ functionDeclarations: [fetchCastleCoordinatesTool] }]
    }
  });

  let response = await chat.sendMessage({ message: prompt });
  
  // Checking if the model issued a call request
  while (response.functionCalls && response.functionCalls.length > 0) {
    const call = response.functionCalls[0];
    console.log(\`[AGENT] Executing function call: \${call.name}\`, call.args);
    
    let result = {};
    if (call.name === "fetchCastleCoordinates") {
      const idx = (call.args as any).castleIdx;
      // Fetch local value
      result = { success: true, index: idx, coords: "X:-5.3, Y:-0.4, Z:4.2" };
    }

    // Pipe response back to model to finish reasoning
    response = await chat.sendMessage({
      message: [
        {
          role: "user",
          content: [
            {
              functionResponse: {
                name: call.name,
                response: result
              }
            }
          ]
        }
      ]
    });
  }

  return response.text;
}`,
    instructions: [
      "Incorporate custom functions strictly declaring parameter types inside the definitions payload.",
      "Execute a while() lookup loop checking for functionCalls inside model packets.",
      "Resolve actions locally and send them back tagged as functionResponse objects to allow the LLM to write out final solutions."
    ]
  }
];

export function ExternalSkillsDBView() {
  const [activeSubTab, setActiveSubTab] = useState<"all" | "membrane" | "mimo" | "ai-eng">("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const handleCopy = (id: string, text: string) => {
    navigator.clipboard.writeText(text);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const filteredSkills = EXTERNAL_SKILLS_DATA.filter((skill) => {
    const matchesCategory = activeSubTab === "all" || skill.category === activeSubTab;
    const matchesSearch = 
      skill.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      skill.short_desc.toLowerCase().includes(searchQuery.toLowerCase()) ||
      skill.full_desc.toLowerCase().includes(searchQuery.toLowerCase()) ||
      skill.tags.some(tag => tag.toLowerCase().includes(searchQuery.toLowerCase()));
    return matchesCategory && matchesSearch;
  });

  return (
    <motion.div
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0 }}
      className="h-full flex flex-col p-6 space-y-6 overflow-hidden"
    >
      {/* Dynamic Header */}
      <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-black text-white uppercase tracking-tighter italic flex items-center gap-3">
            <BrainCircuit className="w-8 h-8 text-blue-500 animate-pulse" />
            Интегрированная База Знаний ИИ Помощника
          </h2>
          <p className="text-xs text-slate-500 uppercase tracking-widest font-bold ml-11">
            Библиотека продвинутых концепций • Membranedev & MiMo & Hands-On AI Eng
          </p>
        </div>

        {/* Categories Tab Selector */}
        <div className="flex bg-white/5 p-1 rounded-2xl border border-white/5 self-end md:self-auto">
          {[
            { id: "all", label: "Все Скиллы", icon: <Layers className="w-3.5 h-3.5" /> },
            { id: "membrane", label: "Membrane Apps", icon: <Database className="w-3.5 h-3.5" /> },
            { id: "mimo", label: "Xiaomi MiMo", icon: <Sliders className="w-3.5 h-3.5" /> },
            { id: "ai-eng", label: "AI Engineering", icon: <Cpu className="w-3.5 h-3.5" /> }
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveSubTab(tab.id as any)}
              className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase transition-all flex items-center gap-1.5 ${
                activeSubTab === tab.id
                  ? "bg-blue-600 text-white shadow-xl shadow-blue-600/30"
                  : "text-slate-400 hover:text-white"
              }`}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Search Input Box */}
      <div className="relative">
        <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-500" />
        <input
          type="text"
          placeholder="Поиск по Базе Знаний ИИ (напр. RAG, API, Overheat, WebSocket)..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="w-full bg-black/40 border border-white/10 rounded-2xl py-4 pl-12 pr-4 text-xs text-white uppercase tracking-wider focus:outline-none focus:border-blue-500 transition-all placeholder:text-slate-600 font-bold"
        />
      </div>

      {/* Main Grid View */}
      <div className="flex-1 overflow-y-auto bg-black/40 border border-white/5 rounded-[2.5rem] p-6 backdrop-blur-md custom-scrollbar relative">
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 relative z-10">
          {filteredSkills.map((skill) => (
            <motion.div
              key={skill.id}
              layoutId={skill.id}
              className="p-6 rounded-3xl bg-neutral-900/60 border border-white/5 hover:border-blue-500/30 transition-all flex flex-col space-y-4 hover:shadow-[0_0_30px_rgba(37,99,235,0.05)] text-slate-300"
            >
              {/* Card Title & Label */}
              <div className="flex items-start justify-between gap-4">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className={`px-2 py-0.5 rounded text-[8px] font-black uppercase ${
                      skill.category === "membrane" ? "bg-emerald-600/20 text-emerald-400" :
                      skill.category === "mimo" ? "bg-amber-600/20 text-amber-400" :
                      "bg-blue-600/20 text-blue-400"
                    }`}>
                      {skill.category.toUpperCase()}
                    </span>
                    <span className="text-[9px] text-slate-500 font-mono">ID: {skill.id}</span>
                  </div>
                  <h3 className="text-md font-bold text-white uppercase tracking-tight">{skill.title}</h3>
                </div>
                
                {/* Micro Actions Bar */}
                <div className="flex gap-2">
                  <button
                    onClick={() => handleCopy(skill.id, skill.code_snippet)}
                    className="p-2 rounded-lg bg-white/5 hover:bg-white/10 text-slate-400 hover:text-white transition-all border border-white/10"
                    title="Скопировать Код"
                  >
                    {copiedId === skill.id ? (
                      <Check className="w-4 h-4 text-emerald-400" />
                    ) : (
                      <Copy className="w-4 h-4" />
                    )}
                  </button>
                </div>
              </div>

              {/* Description Block */}
              <p className="text-xs text-slate-400 font-medium leading-relaxed italic">{skill.short_desc}</p>
              <p className="text-[11px] text-slate-500 leading-relaxed">{skill.full_desc}</p>

              {/* Tags Cloud */}
              <div className="flex flex-wrap gap-1.5">
                {skill.tags.map((tag) => (
                  <span key={tag} className="px-2 py-0.5 bg-white/5 rounded-full text-[9px] font-bold text-slate-400 uppercase tracking-tight">
                    #{tag}
                  </span>
                ))}
              </div>

              {/* Interactive Code Area */}
              <div className="relative rounded-2xl overflow-hidden bg-black/90 border border-white/5 flex flex-col">
                <div className="bg-[#111] px-4 py-2 border-b border-white/5 flex justify-between items-center">
                  <span className="text-[9px] font-mono text-slate-400 flex items-center gap-1.5">
                    <FileCode className="w-3.5 h-3.5 text-blue-500" /> template_source.ts
                  </span>
                  <span className="text-[8px] font-bold text-slate-600 uppercase tracking-widest">
                    Cortex Sandbox Active
                  </span>
                </div>
                <pre className="p-4 overflow-x-auto text-[10px] font-mono text-slate-300 max-h-48 leading-relaxed scrollbar-thin scrollbar-thumb-white/5">
                  <code>{skill.code_snippet}</code>
                </pre>
              </div>

              {/* Step-by-step Guides */}
              <div className="space-y-2 pt-2 border-t border-white/5">
                <span className="text-[9px] font-black uppercase text-slate-400 tracking-wider flex items-center gap-1.5">
                  <Terminal className="w-3.5 h-3.5 text-slate-400" /> Пошаговый Рецепт Настройки:
                </span>
                <ul className="space-y-1.5">
                  {skill.instructions.map((step, idx) => (
                    <li key={idx} className="text-[10px] text-slate-400 flex items-start gap-2">
                      <span className="w-4 h-4 rounded bg-white/5 flex items-center justify-center text-[8px] font-mono text-blue-400 mt-0.5 border border-white/10 shrink-0">
                        {idx + 1}
                      </span>
                      <span className="flex-1 leading-relaxed">{step}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Empty State warning */}
        {filteredSkills.length === 0 && (
          <div className="absolute inset-0 flex flex-col items-center justify-center space-y-4 p-8 text-center bg-black/30 backdrop-blur-sm">
            <ShieldAlert className="w-12 h-12 text-blue-500 animate-pulse" />
            <div className="max-w-md">
              <p className="text-xs text-white font-black uppercase tracking-widest mb-1">Ничего не найдено</p>
              <p className="text-[10px] text-slate-500 uppercase leading-relaxed">
                Попробуйте изменить поисковый запрос или переключить категорию фильтров
              </p>
            </div>
          </div>
        )}

        {/* Ambient Blur Accents */}
        <div className="absolute top-0 right-0 w-80 h-80 bg-blue-600/5 rounded-full blur-[120px] pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-80 h-80 bg-purple-600/5 rounded-full blur-[120px] pointer-events-none" />
      </div>
    </motion.div>
  );
}
