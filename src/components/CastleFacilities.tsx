import React, { useState } from 'react';
import { ALL_TROOPS_ROSTER } from '../App';

interface CastleFacilitiesProps {
  currentCastleFacility: 'hub' | 'barracks' | 'forge_shop' | 'academy_arena';
  setCurrentCastleFacility: (fac: 'hub' | 'barracks' | 'forge_shop' | 'academy_arena') => void;
  troopProgress: Record<string, { lvl: number; xp: number }>;
  setTroopProgress: React.Dispatch<React.SetStateAction<Record<string, { lvl: number; xp: number }>>>;
  customImages: Record<string, string>;
  setCustomImages: React.Dispatch<React.SetStateAction<Record<string, string>>>;
  isVramSaverActive: boolean;
  setIsVramSaverActive: (active: boolean) => void;
  simCastleLevel: number;
  playerGold: number;
  setPlayerGold: React.Dispatch<React.SetStateAction<number>>;
  showNotification: (msg: string, type: 'success' | 'error' | 'info') => void;
  
  // Academy States
  trainedHeroLvl: number;
  setTrainedHeroLvl: React.Dispatch<React.SetStateAction<number>>;
  trainedHeroXp: number;
  setTrainedHeroXp: React.Dispatch<React.SetStateAction<number>>;
  heroTrainingCooldownDay: number;
  setHeroTrainingCooldownDay: React.Dispatch<React.SetStateAction<number>>;
  
  trainedWarriorsLvl: number;
  setTrainedWarriorsLvl: React.Dispatch<React.SetStateAction<number>>;
  trainedWarriorsXp: number;
  setTrainedWarriorsXp: React.Dispatch<React.SetStateAction<number>>;
  warriorTrainingCooldownDay: number;
  setWarriorTrainingCooldownDay: React.Dispatch<React.SetStateAction<number>>;
  
  // Hero Equipment States
  equippedItems: Record<string, { name: string; bonus: string; icon: string }>;
  setEquippedItems: React.Dispatch<React.SetStateAction<Record<string, { name: string; bonus: string; icon: string }>>>;
}

export const CastleFacilities: React.FC<CastleFacilitiesProps> = ({
  currentCastleFacility,
  setCurrentCastleFacility,
  troopProgress,
  setTroopProgress,
  customImages,
  setCustomImages,
  isVramSaverActive,
  setIsVramSaverActive,
  simCastleLevel,
  playerGold,
  setPlayerGold,
  showNotification,
  
  trainedHeroLvl,
  setTrainedHeroLvl,
  trainedHeroXp,
  setTrainedHeroXp,
  heroTrainingCooldownDay,
  setHeroTrainingCooldownDay,
  
  trainedWarriorsLvl,
  setTrainedWarriorsLvl,
  trainedWarriorsXp,
  setTrainedWarriorsXp,
  warriorTrainingCooldownDay,
  setWarriorTrainingCooldownDay,
  
  equippedItems,
  setEquippedItems,
}) => {
  const [selectedTroopId, setSelectedTroopId] = useState<string>('guard');
  const [dragActiveId, setDragActiveId] = useState<string | null>(null);

  const selectedTroop = ALL_TROOPS_ROSTER.find(t => t.id === selectedTroopId) || ALL_TROOPS_ROSTER[0];
  const currentProgress = troopProgress[selectedTroop.id] || { lvl: 1, xp: 0 };

  const currentLvl = currentProgress.lvl;
  const currentXp = currentProgress.xp;

  // Formula-based attributes dynamically scaled by current unit level
  const computedHp = selectedTroop.baseHp + (currentLvl - 1) * selectedTroop.hpPerLvl;
  const computedDmg = selectedTroop.baseDmg + (currentLvl - 1) * selectedTroop.dmgPerLvl;
  const computedArm = selectedTroop.baseArm + (currentLvl - 1) * selectedTroop.armPerLvl;
  const computedAttr = selectedTroop.baseAttr + (currentLvl - 1) * selectedTroop.attrPerLvl;

  // Handle Level Upgrade Math (Required XP = Lvl * 100)
  const xpNeeded = currentLvl * 100;

  const playClickSfx = (freq: number = 380, duration: number = 0.1) => {
    try {
      const ctx = new (window.AudioContext || (window as any).webkitAudioContext)();
      const osc = ctx.createOscillator();
      const g = ctx.createGain();
      osc.frequency.setValueAtTime(freq, ctx.currentTime);
      g.gain.setValueAtTime(0.04, ctx.currentTime);
      osc.connect(g);
      g.connect(ctx.destination);
      osc.start();
      osc.stop(ctx.currentTime + duration);
    } catch(e){}
  };

  const handleLvlUp = () => {
    playClickSfx(520, 0.2);
    setTroopProgress(prev => {
      const existing = prev[selectedTroop.id] || { lvl: 1, xp: 0 };
      return {
        ...prev,
        [selectedTroop.id]: {
          lvl: existing.lvl + 1,
          xp: existing.xp,
        }
      };
    });
    showNotification(`⭐ ${selectedTroop.name} получил новый уровень! Характеристики повышены автоматически!`, "success");
  };

  const handleAddXp = (amount: number) => {
    playClickSfx(420, 0.15);
    setTroopProgress(prev => {
      const existing = prev[selectedTroop.id] || { lvl: 1, xp: 0 };
      let newXp = existing.xp + amount;
      let newLvl = existing.lvl;
      
      while (newXp >= newLvl * 100) {
        newXp -= newLvl * 100;
        newLvl += 1;
        setTimeout(() => {
          showNotification(`🏆 УРОВЕНЬ ПОВЫШЕН! ${selectedTroop.name} достиг Ур. ${newLvl}!`, "success");
        }, 10);
      }
      
      return {
        ...prev,
        [selectedTroop.id]: {
          lvl: newLvl,
          xp: newXp
        }
      };
    });
    showNotification(`🧪 Получено +${amount} XP для ${selectedTroop.name}!`, "info");
  };

  const copyToClipboard = (text: string) => {
    playClickSfx(650, 0.1);
    navigator.clipboard.writeText(text);
    showNotification("📋 Промпт для Leonardo.ai успешно скопирован в буфер обмена!", "success");
  };

  // Image Upload base64 Conversion
  const handleFile = (file: File, id: string) => {
    if (!file.type.startsWith('image/')) {
      showNotification("❌ Пожалуйста, выберите корректное изображение (PNG/JPG)", "error");
      return;
    }
    const reader = new FileReader();
    reader.onloadend = () => {
      setCustomImages(prev => ({
        ...prev,
        [`troop_${id}`]: reader.result as string
      }));
      showNotification(`🎨 Картинка для ${selectedTroop.name} успешно интегрирована!`, "success");
    };
    reader.readAsDataURL(file);
  };

  return (
    <div className="flex-1 flex flex-col justify-between space-y-4" id="Castle_Facilities_Panel">
      
      {/* Vram Protection Optimizer status */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 p-3 bg-slate-950/40 backdrop-blur border border-cyan-500/15 rounded-2xl text-left">
        <div className="flex items-center gap-2">
          <span className="p-1 px-2 bg-emerald-500/20 text-emerald-400 font-mono text-[8px] rounded font-bold uppercase tracking-wider animate-pulse">
            {isVramSaverActive ? '⚡ RAM/VRAM СБЕРЕЖЕНИЕ: АКТИВНО' : '🛡️ РЕСУРСОСБЕРЕЖЕНИЕ'}
          </span>
          <span className="text-[10px] text-slate-300 font-bold">
            Ограничение тиков рендера для предотвращения крашей GPU и снижения нагрева
          </span>
        </div>
        <label className="flex items-center gap-1.5 text-[9px] font-bold text-cyan-400 cursor-pointer select-none">
          <input 
            type="checkbox" 
            checked={isVramSaverActive} 
            onChange={(e) => {
              setIsVramSaverActive(e.target.checked);
              showNotification(e.target.checked ? "📉 Лимит фреймрейта и термозащита включены!" : "📈 Полное ускорение рендера активировано.", "info");
            }}
            className="w-3.5 h-3.5 rounded bg-slate-900 border-white/20 accent-cyan-500 cursor-pointer"
          />
          Защита от перегруза видеокарты
        </label>
      </div>

      {/* HUB COURT / COURTYARD SELECTOR */}
      {currentCastleFacility === 'hub' && (
        <div className="space-y-4">
          <div className="text-center p-3 bg-indigo-950/20 rounded-2xl border border-indigo-500/10 mb-2">
            <span className="text-[10.5px] font-bold text-indigo-300 block mb-1">
              🏰 ВНУТРЕННИЙ ДВОР ВЕЛИКОЙ ЦИТАДЕЛИ (Замок Ур. {simCastleLevel})
            </span>
            <p className="text-[10px] text-slate-400 leading-relaxed max-w-xl mx-auto">
              Перед вами раскинулись три могучих оплота Fate Continent. Выберите строение для управления войсками, улучшения доспехов Света или муштры героев на Арене Судьбы.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4" id="Castle_Three_Main_Drawn_Fortresses">
            
            {/* 1. BARRACKS DRAWN CASTLE */}
            <div className="bg-slate-950/80 p-5 rounded-3xl border border-emerald-500/20 flex flex-col justify-between space-y-4 hover:border-emerald-400/40 transition-all duration-300 shadow-lg shadow-emerald-950/20 relative group overflow-hidden">
              <div className="absolute top-2 right-2 px-2 py-0.5 bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-mono text-[7px] rounded-full uppercase">
                АКТИВНО
              </div>
              <div>
                <div className="h-28 flex items-center justify-center bg-slate-900/60 rounded-2xl mb-3 border border-white/5 relative group-hover:bg-slate-900/90 transition-all">
                  {/* Drawing structure for barracks (Drawn neon castle) */}
                  <svg className="w-20 h-20 text-emerald-400 filter drop-shadow-[0_0_8px_rgba(52,211,153,0.3)]" viewBox="0 0 100 100" fill="none" stroke="currentColor" strokeWidth="2.5">
                    <path d="M20 80V40L35 25L50 40M50 40L65 25L80 40V80H20Z" fill="rgba(16,185,129,0.1)"/>
                    <path d="M30 40H70V80H30V40Z" />
                    <path d="M45 80V60H55V80" strokeWidth="3" />
                    <line x1="20" y1="55" x2="30" y2="55" />
                    <line x1="70" y1="55" x2="80" y2="55" />
                    <polygon points="35,25 35,15 42,18" fill="currentColor"/>
                    <polygon points="65,25 65,15 72,18" fill="currentColor"/>
                  </svg>
                </div>
                <h4 className="text-xs font-black text-emerald-400 uppercase tracking-wider text-center flex items-center justify-center gap-1">
                  🛡️ Замок (Казармы)
                </h4>
                <p className="text-[9.5px] text-slate-400 text-center leading-relaxed mt-2 p-1">
                  Расквартирование элитных войск Альянса. Здесь содержатся все 10 классов солдат Судьбы, детально настраиваются статы и генерируются промпты Leonardo.
                </p>
                <div className="mt-3 py-1 bg-emerald-950/20 border border-emerald-500/10 text-center rounded-xl">
                  <span className="text-[7.5px] font-mono text-emerald-300">🔊 ШУМ ЛАТ: АКТИВИРОВАН</span>
                </div>
              </div>
              <button
                onClick={() => {
                  playClickSfx(380);
                  setCurrentCastleFacility('barracks');
                }}
                className="w-full py-2 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-black text-[9px] uppercase tracking-wider rounded-xl transition-all shadow-[0_4px_15px_rgba(16,185,129,0.25)] hover:scale-[1.02]"
              >
                🚪 Войти во внутренний зал Войск ➔
              </button>
            </div>

            {/* 2. FORGE SHOP DRAWN CASTLE */}
            <div className="bg-slate-950/80 p-5 rounded-3xl border border-amber-500/20 flex flex-col justify-between space-y-4 hover:border-amber-400/40 transition-all duration-300 shadow-lg shadow-amber-950/20 relative group overflow-hidden">
              <div className="absolute top-2 right-2 px-2 py-0.5 bg-amber-500/10 border border-amber-500/30 text-amber-400 font-mono text-[7px] rounded-full uppercase">
                ГОРЯЧЕЕ
              </div>
              <div>
                <div className="h-28 flex items-center justify-center bg-slate-900/60 rounded-2xl mb-3 border border-white/5 relative group-hover:bg-slate-900/90 transition-all">
                  {/* Drawing structure for forge (Drawn neon forge castle) */}
                  <svg className="w-20 h-20 text-amber-400 filter drop-shadow-[0_0_8px_rgba(245,158,11,0.35)]" viewBox="0 0 100 100" fill="none" stroke="currentColor" strokeWidth="2.5">
                    <rect x="25" y="55" width="50" height="30" rx="4" fill="rgba(245,158,11,0.1)"/>
                    <path d="M40 30H60V55H40V30Z" />
                    <path d="M45 55L35 85H65L55 55" />
                    <circle cx="50" cy="42" r="6" fill="#f59e0b" className="animate-pulse" />
                    <line x1="15" y1="85" x2="85" y2="85" strokeWidth="3" />
                  </svg>
                </div>
                <h4 className="text-xs font-black text-amber-400 uppercase tracking-wider text-center flex items-center justify-center gap-1">
                  🧪 Замок (Кузница & Лавка)
                </h4>
                <p className="text-[9.5px] text-slate-400 text-center leading-relaxed mt-2 p-1">
                  Жгучий огонь плавит доспехи Света и кует Легендарное Оружие. Полный доступ к торговле королевским обвесом и микстурами Алхимической палаты.
                </p>
                <div className="mt-3 py-1 bg-amber-950/20 border border-amber-500/10 text-center rounded-xl">
                  <span className="text-[7.5px] font-mono text-amber-300">🔥 ТЕМПЕРАТУРА: 1200°C • ГОРЯЧО</span>
                </div>
              </div>
              <button
                onClick={() => {
                  playClickSfx(440);
                  setCurrentCastleFacility('forge_shop');
                }}
                className="w-full py-2 bg-gradient-to-r from-amber-600 to-yellow-600 hover:from-amber-500 hover:to-yellow-500 text-white font-black text-[9px] uppercase tracking-wider rounded-xl transition-all shadow-[0_4px_15px_rgba(245,158,11,0.25)] hover:scale-[1.02]"
              >
                🚪 Войти в мастерские Стали ➔
              </button>
            </div>

            {/* 3. ACADEMY ARENA DRAWN CASTLE */}
            <div className="bg-slate-950/80 p-5 rounded-3xl border border-cyan-500/20 flex flex-col justify-between space-y-4 hover:border-cyan-400/40 transition-all duration-300 shadow-lg shadow-cyan-950/20 relative group overflow-hidden">
              <div className="absolute top-2 right-2 px-2 py-0.5 bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 font-mono text-[7px] rounded-full uppercase">
                ЭПИЧНО
              </div>
              <div>
                <div className="h-28 flex items-center justify-center bg-slate-900/60 rounded-2xl mb-3 border border-white/5 relative group-hover:bg-slate-900/90 transition-all">
                  {/* Drawing structure for academy crystal spire */}
                  <svg className="w-20 h-20 text-cyan-400 filter drop-shadow-[0_0_8px_rgba(6,182,212,0.35)]" viewBox="0 0 100 100" fill="none" stroke="currentColor" strokeWidth="2.5">
                    <path d="M50 10L30 50H70L50 10Z" fill="rgba(6,182,212,0.1)"/>
                    <rect x="35" y="50" width="30" height="35" />
                    <line x1="50" y1="10" x2="50" y2="85" strokeDasharray="3 3"/>
                    <ellipse cx="50" cy="35" rx="8" ry="8" strokeWidth="1.5" />
                    <circle cx="50" cy="35" r="2" fill="currentColor"/>
                  </svg>
                </div>
                <h4 className="text-xs font-black text-cyan-400 uppercase tracking-wider text-center flex items-center justify-center gap-1">
                  🎯 Замок (Академия & Арена)
                </h4>
                <p className="text-[9.5px] text-slate-400 text-center leading-relaxed mt-2 p-1">
                  Обучающий плац для гладиаторов Судьбы. Повышайте ранги союзным воинам на манерах боевых арен и сверяйте тактические хроники противостояний ИИ.
                </p>
                <div className="mt-3 py-1 bg-cyan-950/20 border border-cyan-500/10 text-center rounded-xl">
                  <span className="text-[7.5px] font-mono text-cyan-300">🏆 АВТОНОМНЫЙ ПЛАЦ: ДОСТУПЕН</span>
                </div>
              </div>
              <button
                onClick={() => {
                  playClickSfx(500);
                  setCurrentCastleFacility('academy_arena');
                }}
                className="w-full py-2 bg-gradient-to-r from-cyan-600 to-indigo-600 hover:from-cyan-500 hover:to-indigo-500 text-white font-black text-[9px] uppercase tracking-wider rounded-xl transition-all shadow-[0_4px_15px_rgba(6,182,212,0.25)] hover:scale-[1.02]"
              >
                🚪 Посетить манеж гладиаторов ➔
              </button>
            </div>

          </div>
        </div>
      )}


      {/* 🛡️ SUB-WINDOW: BARRACKS INTERNAL CHAMBERS */}
      {currentCastleFacility === 'barracks' && (
        <div className="space-y-4 animate-fadeIn">
          
          {/* HEADER BACK NAVIGATION BUTTON */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 border-b border-white/5 pb-3">
            <button
              onClick={() => {
                playClickSfx(300, 0.1);
                setCurrentCastleFacility('hub');
              }}
              className="px-3 py-1.5 bg-slate-950/90 text-emerald-400 border border-emerald-500/30 rounded-xl text-[9px] font-bold uppercase tracking-wider hover:bg-emerald-500/10 hover:border-emerald-500 transition-all flex items-center gap-1.5"
            >
              ◀ Вернуться во внутренний двор Цитадели
            </button>
            <span className="text-[10px] font-mono text-slate-400">
              FateCastleManager.cs • Гарнизон Казарм
            </span>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-12 gap-5" id="Barracks_Roster_Layout">
            
            {/* LEFT SIDE: List of 10 soldiers (3 cols) */}
            <div className="lg:col-span-4 flex flex-col space-y-2 max-h-[580px] overflow-y-auto pr-1 text-left scrollbar-thin">
              <span className="text-[8px] font-bold tracking-widest text-emerald-500 uppercase px-1">
                👥 Реестр Дивизий (Найм и Прокачка):
              </span>
              {ALL_TROOPS_ROSTER.map(troop => {
                const prog = troopProgress[troop.id] || { lvl: 1, xp: 0 };
                const isSelected = selectedTroopId === troop.id;
                const userImg = customImages[`troop_${troop.id}`];

                return (
                  <button
                    key={troop.id}
                    onClick={() => {
                      playClickSfx(400 + troop.cost / 2, 0.1);
                      setSelectedTroopId(troop.id);
                    }}
                    className={`w-full p-2.5 rounded-xl border flex items-center justify-between text-left transition-all ${
                      isSelected 
                        ? 'bg-gradient-to-r from-emerald-950/80 to-slate-900 border-emerald-500 text-white shadow-[0_0_12px_rgba(16,185,129,0.15)] scale-[1.01]' 
                        : 'bg-slate-950/70 border-white/5 text-slate-400 hover:text-white hover:bg-slate-900/60 hover:border-white/10'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      {/* Round indicator with image or icon */}
                      <div className="w-9 h-9 rounded-lg bg-slate-900 border border-white/10 flex items-center justify-center overflow-hidden relative">
                        {userImg ? (
                          <img src={userImg} className="w-full h-full object-cover" alt="" referrerPolicy="no-referrer" />
                        ) : (
                          <span className="text-base">{troop.icon}</span>
                        )}
                      </div>
                      <div>
                        <div className="text-[10px] font-black tracking-tight">{troop.name}</div>
                        <div className="text-[7.5px] text-slate-500 uppercase tracking-wider">
                          {troop.rarity} • {troop.classType}
                        </div>
                      </div>
                    </div>
                    
                    <div className="text-right">
                      <div className="text-[9.5px] font-mono text-emerald-400 font-bold">Ур. {prog.lvl}</div>
                      <div className="text-[7.5px] text-slate-500 font-mono">Cost: {troop.cost} 🪙</div>
                    </div>
                  </button>
                );
              })}
            </div>


            {/* RIGHT SIDE: Detail Sheet & Leonardo Prompts / Image custom uploads (8 cols) */}
            <div className="lg:col-span-8 bg-slate-950/90 border border-white/5 p-5 rounded-2xl flex flex-col justify-between space-y-4 text-left">
              
              {/* Unit Hero Showcase */}
              <div>
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 pb-2 border-b border-white/5 mb-3">
                  <div>
                    <h3 className="text-sm font-black text-white flex items-center gap-1.5">
                      <span className="text-xl">{selectedTroop.icon}</span> {selectedTroop.name}
                    </h3>
                    <span className="text-[8px] uppercase tracking-widest text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded font-bold">
                      {selectedTroop.rarity} Класс • {selectedTroop.classType} • Одно Лицо
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-[9.5px] font-mono text-slate-400">Текущий Ранг Симулятора:</span>
                    <div className="text-lg font-black font-mono text-amber-400">Уровень {currentLvl}</div>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-12 gap-4">
                  
                  {/* Left subpart: Photo Frame & Image drag-and-drop box (5/12 cols) */}
                  <div className="md:col-span-5 flex flex-col items-center space-y-3">
                    <div className="text-[8px] font-bold text-slate-400 uppercase tracking-wider text-center">
                      🖼️ Фото воина (Drag & Drop или Клик):
                    </div>
                    
                    {/* Upload Drag box */}
                    <div
                      onDragOver={(e) => {
                        e.preventDefault();
                        setDragActiveId(selectedTroop.id);
                      }}
                      onDragLeave={() => setDragActiveId(null)}
                      onDrop={(e) => {
                        e.preventDefault();
                        setDragActiveId(null);
                        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
                          handleFile(e.dataTransfer.files[0], selectedTroop.id);
                        }
                      }}
                      onClick={() => {
                        const ipt = document.getElementById(`file-ipt-${selectedTroop.id}`);
                        if (ipt) ipt.click();
                      }}
                      className={`w-40 h-40 rounded-2xl bg-slate-900 border-2 border-dashed flex flex-col items-center justify-center p-2 relative overflow-hidden transition-all group cursor-pointer ${
                        dragActiveId === selectedTroop.id 
                          ? 'border-emerald-400 bg-emerald-500/10' 
                          : 'border-white/10 hover:border-emerald-500/50 hover:bg-slate-900/80'
                      }`}
                    >
                      <input 
                        type="file" 
                        id={`file-ipt-${selectedTroop.id}`} 
                        className="hidden" 
                        accept="image/*"
                        onChange={(e) => {
                          if (e.target.files && e.target.files[0]) {
                            handleFile(e.target.files[0], selectedTroop.id);
                          }
                        }}
                      />
                      
                      {customImages[`troop_${selectedTroop.id}`] ? (
                        <div className="w-full h-full relative">
                          <img 
                            src={customImages[`troop_${selectedTroop.id}`]} 
                            className="w-full h-full object-cover rounded-xl" 
                            alt="" 
                            referrerPolicy="no-referrer" 
                          />
                          <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center text-white p-2 text-center text-[8.5px]">
                            <span>🔄 Перетащите файл для замены</span>
                          </div>
                        </div>
                      ) : (
                        <div className="text-center p-2 space-y-1">
                          <span className="text-3xl filter saturate-50">{selectedTroop.icon}</span>
                          <span className="text-[8px] text-slate-500 block">Перенесите фото сюда</span>
                          <span className="text-[7px] text-emerald-400 font-bold uppercase bg-emerald-500/10 px-1.5 py-0.5 rounded inline-block">Загрузить JPG</span>
                        </div>
                      )}
                    </div>
                    
                    {/* Add Troops recruitment mock indicator */}
                    <div className="w-full bg-slate-900 p-2.5 rounded-xl border border-white/5 text-center">
                      <span className="text-[9.5px] text-slate-400 block mb-1">
                        Стоимость найма: <strong className="text-yellow-400">{selectedTroop.cost} 🪙</strong>
                      </span>
                      <button
                        onClick={() => {
                          if (playerGold >= selectedTroop.cost) {
                            setPlayerGold(prev => prev - selectedTroop.cost);
                            playClickSfx(750, 0.25);
                            showNotification(`⚔️ Дивизия [${selectedTroop.name}] нанята в гарнизон Цитадели!`, "success");
                          } else {
                            showNotification(`❌ Недостаточно золота для найма! Требуется: ${selectedTroop.cost} золота.`, "error");
                          }
                        }}
                        className="w-full py-1 bg-emerald-600/20 hover:bg-emerald-600 text-[8px] hover:text-white text-emerald-400 font-bold uppercase tracking-wider rounded-lg border border-emerald-500/30 transition-all"
                      >
                        ⚔️ Нанять Отряд (Покупка)
                      </button>
                    </div>

                  </div>

                  {/* Right subpart: Dynamic Stats Scaling & Level sliders (7/12 cols) */}
                  <div className="md:col-span-7 flex flex-col justify-between space-y-3">
                    
                    {/* STATS TABLE */}
                    <div className="bg-slate-900 border border-white/5 p-3 rounded-2xl space-y-2">
                       <div className="text-[8.5px] font-bold text-slate-400 uppercase tracking-widest border-b border-white/5 pb-1 flex justify-between">
                         <span>📈 ХАРАКТЕРИСТИКИ ВОИНА</span>
                         <span className="text-emerald-400 font-mono font-bold">АПТИК: АВТОМАТИЧЕСКИ</span>
                       </div>
                       
                       <div className="grid grid-cols-2 gap-2 text-[9.5px] font-mono">
                         <div className="p-1 px-2 bg-slate-950/40 rounded flex justify-between">
                           <span className="text-slate-500">Здоровье (HP):</span>
                           <span className="text-white font-bold">{computedHp} HP</span>
                         </div>
                         <div className="p-1 px-2 bg-slate-950/40 rounded flex justify-between">
                           <span className="text-slate-500">Урон (DMG):</span>
                           <span className="text-white font-bold">⚔️ {computedDmg}</span>
                         </div>
                         <div className="p-1 px-2 bg-slate-950/40 rounded flex justify-between">
                           <span className="text-slate-500">Защита (ARM):</span>
                           <span className="text-white font-bold">🛡️ {computedArm}</span>
                         </div>
                         <div className="p-1 px-2 bg-slate-950/40 rounded flex justify-between">
                           <span className="text-slate-500">{selectedTroop.mainAttr}:</span>
                           <span className="text-cyan-300 font-bold">💫 {computedAttr}</span>
                         </div>
                       </div>
                       
                       <p className="text-[7.5px] text-slate-500 italic leading-snug">
                         Каждый уровень автоматически повышает здоровье на +{selectedTroop.hpPerLvl} ед, урон на +{selectedTroop.dmgPerLvl}, защиту на +{selectedTroop.armPerLvl} и главный атрибут на +{selectedTroop.attrPerLvl}.
                       </p>
                    </div>

                    {/* EXP & ACTIONS */}
                    <div className="bg-slate-900 border border-white/5 p-3 rounded-2xl space-y-2">
                      <div className="flex justify-between items-center text-[9px] font-mono">
                        <span className="text-slate-400">Шкала Опыта Разведки:</span>
                        <span className="text-amber-400 font-bold">{currentXp} / {xpNeeded} XP</span>
                      </div>
                      
                      {/* Real Progress Bar */}
                      <div className="w-full h-2 bg-slate-950 rounded-full overflow-hidden border border-white/5">
                        <div 
                          className="h-full bg-gradient-to-r from-amber-500 to-yellow-400 transition-all duration-300"
                          style={{ width: `${Math.min(100, (currentXp / xpNeeded) * 100)}%` }}
                        />
                      </div>

                      <div className="grid grid-cols-2 gap-2 pt-1">
                        <button
                          onClick={handleLvlUp}
                          className="py-1.5 bg-cyan-600/20 hover:bg-cyan-600 hover:text-white text-cyan-300 text-[8px] font-bold uppercase tracking-wider rounded-lg border border-cyan-500/30 transition-all"
                        >
                          🏋️‍♂️ Ап уровня (+1 лвл)
                        </button>
                        <button
                          onClick={() => handleAddXp(50)}
                          className="py-1.5 bg-amber-600/20 hover:bg-amber-600 hover:text-white text-amber-300 text-[8px] font-bold uppercase tracking-wider rounded-lg border border-amber-500/30 transition-all"
                        >
                          🧪 Дать Свиток (+50 XP)
                        </button>
                      </div>
                    </div>

                  </div>

                </div>

              </div>

              {/* TWO PASSIVES & ONE ACTIVE SKILLBOOK VIEW */}
              <div className="bg-slate-900/60 p-3.5 rounded-2xl border border-white/5">
                <span className="text-[8px] text-slate-500 font-black tracking-widest block uppercase mb-2">
                  📖 УНИКАЛЬНЫЙ ТАКТИЧЕСКИЙ СБОРНИК НАВЫКОВ (PASSIVES & ACTIVE ULT):
                </span>
                
                <div className="grid grid-cols-1 md:grid-cols-3 gap-2.5">
                  {/* Render passive skills */}
                  {selectedTroop.passives.map((passive, index) => (
                    <div key={index} className="bg-slate-950 p-2 rounded-xl border border-white/5 space-y-1">
                      <div className="flex justify-between items-center text-[8.5px] font-bold">
                        <span className="text-emerald-400">🛡️ {passive.name}</span>
                        <span className="px-1 py-0.2 bg-emerald-500/10 text-emerald-400 font-mono text-[6.5px] rounded">ПАССИВ</span>
                      </div>
                      <p className="text-[8px] text-slate-400 leading-normal">{passive.desc}</p>
                    </div>
                  ))}

                  {/* Render active skills */}
                  {selectedTroop.actives.map((active, index) => (
                    <div key={index} className="bg-slate-950 p-2 rounded-xl border border-amber-500/20 space-y-1">
                      <div className="flex justify-between items-center text-[8.5px] font-bold">
                        <span className="text-amber-400">⚡ {active.name}</span>
                        <span className="px-1 py-0.2 bg-amber-500/10 text-amber-400 font-mono text-[6.5px] rounded">АКТИВНЫЙ УЛЬТ</span>
                      </div>
                      <p className="text-[8px] text-slate-400 leading-normal">{active.desc}</p>
                    </div>
                  ))}
                </div>
              </div>


              {/* COMPREHENSIVE LEONARDO.AI SCREENSHOT & COPYABLE PROMPTER */}
              <div className="bg-slate-950 border border-indigo-500/35 p-3 rounded-2xl space-y-3">
                <div className="flex justify-between items-center border-b border-indigo-500/25 pb-1">
                  <div className="text-[8.5px] font-bold text-cyan-400 uppercase tracking-widest flex items-center gap-1">
                    <span>🎨 COPY PROMPTER ДЛЯ LEONARDO.AI (ЦЕНТРИРОВАННОЕ ЛИЦО)</span>
                    <span className="text-[7px] bg-cyan-500/10 text-cyan-300 px-1 py-0.2 rounded font-mono">1:1 ASPECT</span>
                  </div>
                  <button
                    onClick={() => copyToClipboard(selectedTroop.p)}
                    className="px-2 py-0.5 bg-gradient-to-r from-indigo-600 to-indigo-500 text-white rounded text-[7.5px] font-bold uppercase hover:scale-105 transition-all flex items-center gap-1"
                  >
                    📋 Скопировать Промпт
                  </button>
                </div>

                <div className="p-2 bg-slate-900 rounded-xl space-y-1.5 select-all">
                  <p className="text-[9px] font-mono text-slate-300 leading-relaxed italic">
                    "{selectedTroop.p}"
                  </p>
                </div>

                {/* Screenshot settings representation mimicking Leonardo.ai UI precisely */}
                <div className="p-3 bg-slate-900 rounded-xl border border-white/5 space-y-1.5" id="Leonardo_Settings_Screenshot_Representation">
                  <span className="text-[7.5px] font-mono text-slate-500 block uppercase tracking-wide text-center border-b border-white/5 pb-1">
                    📸 РЕКОМЕНДУЕМЫЕ НАСТРОЙКИ В LEONARDO.AI ПОД ПРОВЕРЕННЫЙ ШАБЛОН:
                  </span>
                  
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-[8px] font-mono text-slate-400">
                    <div className="p-1 px-1.5 bg-slate-950 rounded border border-white/5">
                      <span className="text-slate-500 block">BASE MODEL:</span>
                      <strong className="text-white">Leonardo Anime XL</strong>
                    </div>
                    <div className="p-1 px-1.5 bg-slate-950 rounded border border-white/5">
                      <span className="text-slate-500 block">ASPECT RATIO:</span>
                      <strong className="text-white">1:1 Square (1024x1024)</strong>
                    </div>
                    <div className="p-1 px-1.5 bg-slate-950 rounded border border-white/5">
                      <span className="text-slate-500 block">STYLIZED TYPE:</span>
                      <strong className="text-white">Cute 3D Clay Portrait</strong>
                    </div>
                    <div className="p-1 px-1.5 bg-slate-950 rounded border border-white/5">
                      <span className="text-slate-500 block">FLAT COLOR BG:</span>
                      <strong className="text-white">#ffffff (Pristine White)</strong>
                    </div>
                  </div>
                  
                  <div className="text-[7.5px] text-amber-500 text-center italic">
                    💡 Белый фоновый спрайт позволит с легкостью обрезать края в Blender или Unity, удалив белый цвет через оверлейные текстуры!
                  </div>
                </div>

              </div>

            </div>

          </div>

        </div>
      )}


      {/* 🧪 SUB-WINDOW: FORGE SHOP INTERNAL */}
      {currentCastleFacility === 'forge_shop' && (
        <div className="space-y-4 animate-fadeIn">
          
          {/* HEADER BACK NAVIGATION BUTTON */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 border-b border-white/5 pb-3">
            <button
              onClick={() => {
                playClickSfx(300, 0.1);
                setCurrentCastleFacility('hub');
              }}
              className="px-3 py-1.5 bg-slate-950/90 text-amber-400 border border-amber-500/30 rounded-xl text-[9px] font-bold uppercase tracking-wider hover:bg-amber-500/10 hover:border-amber-500 transition-all flex items-center gap-1.5"
            >
              ◀ Вернуться во внутренний двор Цитадели
            </button>
            <span className="text-[10px] font-mono text-slate-400">
              FateCastleManager.cs • Кузнечные Мастерские
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-left">
            
            {/* ARMORY FORGE */}
            <div className="bg-slate-950 p-4 rounded-2xl border border-white/5 flex flex-col justify-between space-y-4">
              <div>
                <div className="text-[10px] font-black text-amber-400 uppercase tracking-tight flex items-center gap-1.5">
                  <span>🔨 ОРУЖЕЙНЫЙ КУЗНЕЧНЫЙ ГОРН</span>
                  <span className="text-[8px] bg-amber-500/10 text-amber-300 px-1 py-0.5 rounded font-mono">FORGE v18.11.16</span>
                </div>
                <span className="text-[7px] font-mono text-slate-500 block mb-1">GameObject: Armory_Anvil</span>
                <p className="text-[9.5px] text-slate-400 leading-relaxed mb-3">
                  Тут вы можете снарядить главного полководца древними реликвиями. Легендарные мечи, амулеты и кольца Силы влияют на боевые свойства в тактических сражениях.
                </p>

                {/* Equipment Status Bar */}
                <div className="bg-slate-900 border border-white/5 p-3 rounded-xl mb-3 space-y-1">
                  <span className="text-[7.5px] text-slate-500 font-mono block uppercase">Экипировано в Текущем Слоте:</span>
                  <div className="grid grid-cols-3 gap-2 text-[8.5px] font-mono text-slate-300">
                    <div className="p-1 bg-slate-950/80 rounded border border-white/5">
                      🗡️ Меч: <span className="text-white font-bold">{equippedItems.weapon?.name || 'Нет'}</span>
                    </div>
                    <div className="p-1 bg-slate-950/80 rounded border border-white/5">
                      🛡️ Щит: <span className="text-white font-bold">{equippedItems.shield?.name || 'Нет'}</span>
                    </div>
                    <div className="p-1 bg-slate-950/80 rounded border border-white/5">
                      💍 Кольцо: <span className="text-white font-bold">{equippedItems.ring?.name || 'Нет'}</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Forge Store items */}
              <div className="bg-slate-900 border border-white/5 p-3 rounded-xl space-y-2">
                <span className="text-[8px] text-slate-500 font-black block uppercase">Доступная Кузница Экипировки:</span>
                
                <div className="grid grid-cols-1 gap-2">
                  <button
                    onClick={() => {
                      if (playerGold >= 50) {
                        setPlayerGold(prev => prev - 50);
                        setEquippedItems(prev => ({
                          ...prev,
                          weapon: { name: 'Стальной Эгида-Меч', bonus: '+15 Сила', icon: '🗡️' }
                        }));
                        playClickSfx(700, 0.2);
                        showNotification("⚔️ Куплен Стальной Меч (+15 Сила)!", "success");
                      } else {
                        showNotification("❌ Недостаточно золота для покупки Steel Sword (50)!", "error");
                      }
                    }}
                    className="w-full p-2 bg-slate-950/80 hover:bg-slate-950 border border-white/5 rounded-xl flex justify-between items-center text-[9px] font-mono text-slate-300 hover:border-amber-400 transition-all text-left"
                  >
                    <span>🗡️ Стальной Меч (+15 Сила)</span>
                    <strong className="text-yellow-400">50 🪙</strong>
                  </button>

                  <button
                    onClick={() => {
                      if (playerGold >= 120) {
                        setPlayerGold(prev => prev - 120);
                        setEquippedItems(prev => ({
                          ...prev,
                          shield: { name: 'Круглый Щит Зари', bonus: '+35 Броня', icon: '🛡️' }
                        }));
                        playClickSfx(750, 0.2);
                        showNotification("🛡️ Куплен Крейсерский Щит Зари (+35 Броня)!", "success");
                      } else {
                        showNotification("❌ Недостаточно золота для Shield (120)!", "error");
                      }
                    }}
                    className="w-full p-2 bg-slate-950/80 hover:bg-slate-950 border border-white/5 rounded-xl flex justify-between items-center text-[9px] font-mono text-slate-300 hover:border-amber-400 transition-all text-left"
                  >
                    <span>🛡️ Щит Зари (+35 Броня)</span>
                    <strong className="text-yellow-400">120 🪙</strong>
                  </button>

                  <button
                    onClick={() => {
                      if (playerGold >= 200) {
                        setPlayerGold(prev => prev - 200);
                        setEquippedItems(prev => ({
                          ...prev,
                          weapon: { name: 'Скимитар Рока', bonus: '+45 Сила', icon: '🔥' }
                        }));
                        playClickSfx(880, 0.25);
                        showNotification("🔥 Легендарный Скимитар Рока Экипирован!", "success");
                      } else {
                        showNotification("❌ Недостаточно золота для Scimitar (200)!", "error");
                      }
                    }}
                    className="w-full p-2 bg-slate-950/80 hover:bg-slate-950 border border-amber-500/30 rounded-xl flex justify-between items-center text-[9px] font-mono text-slate-300 hover:border-amber-400 hover:shadow-[0_0_10px_rgba(245,158,11,0.1)] transition-all text-left"
                  >
                    <span>🔥 Скимитар Рока (+45 Сила)</span>
                    <strong className="text-yellow-400 font-bold">200 🪙</strong>
                  </button>
                </div>
              </div>

            </div>


            {/* ALCHEMY LAB */}
            <div className="bg-slate-950 p-4 rounded-2xl border border-white/5 flex flex-col justify-between space-y-4">
              <div>
                <div className="text-[10px] font-black text-purple-400 uppercase tracking-tight flex items-center gap-1.5">
                  <span>🧪 АЛХИМИЧЕСКИЙ РЕАКТОР И ЛАВКА</span>
                  <span className="text-[8px] bg-purple-500/10 text-purple-300 px-1 py-0.5 rounded font-mono">CHEMISTRY</span>
                </div>
                <span className="text-[7px] font-mono text-slate-500 block mb-1">GameObject: Alchemy_Lab</span>
                <p className="text-[9.5px] text-slate-400 leading-relaxed mb-3">
                  Синтезируйте редкие жидкие сырья и варите эликсиры ума на основе кристаллического кристалла Зенита для перманентного восполнения шкалы маны героев!
                </p>
              </div>

              <div className="bg-slate-900 border border-white/5 p-3 rounded-xl space-y-2">
                <span className="text-[8px] text-slate-500 font-black block uppercase">Сварить Эликсиры:</span>
                
                <div className="grid grid-cols-1 gap-2">
                  <button
                    onClick={() => {
                      if (playerGold >= 60) {
                        setPlayerGold(prev => prev - 60);
                        playClickSfx(600, 0.3);
                        showNotification("🧪 Сварено Зелье Маны! Мана всех магов восстановлена на +50!", "success");
                      } else {
                        showNotification("❌ Недостаточно золота для Alchemy (60)!", "error");
                      }
                    }}
                    className="w-full p-2 bg-slate-950/80 hover:bg-slate-950 border border-white/5 rounded-xl flex justify-between items-center text-[9px] font-mono text-slate-300 hover:border-purple-400 transition-all text-left"
                  >
                    <span>💧 Концентрат Маны (+50 Мана)</span>
                    <strong className="text-yellow-400">60 🪙</strong>
                  </button>

                  <button
                    onClick={() => {
                      if (playerGold >= 100) {
                        setPlayerGold(prev => prev - 100);
                        playClickSfx(800, 0.45);
                        showNotification("🔮 Эликсир Бессмертия сварен! Опыт всей армии увеличен на +250 XP!", "success");
                      } else {
                        showNotification("❌ Недостаточно золота для Elixir (100)!", "error");
                      }
                    }}
                    className="w-full p-2 bg-slate-950/80 hover:bg-slate-950 border border-purple-500/30 rounded-xl flex justify-between items-center text-[9px] font-mono text-slate-300 hover:border-purple-400 hover:shadow-[0_0_10px_rgba(168,85,247,0.15)] transition-all text-left"
                  >
                    <span>🔮 Эликсир Бессмертия (+250 XP)</span>
                    <strong className="text-yellow-400 font-bold">100 🪙</strong>
                  </button>
                </div>
              </div>

            </div>

          </div>

        </div>
      )}


      {/* 🎯 SUB-WINDOW: ACADEMY INTERNAL CHAMBERS */}
      {currentCastleFacility === 'academy_arena' && (
        <div className="space-y-4 animate-fadeIn">
          
          {/* HEADER BACK NAVIGATION BUTTON */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 border-b border-white/5 pb-3">
            <button
              onClick={() => {
                playClickSfx(300, 0.1);
                setCurrentCastleFacility('hub');
              }}
              className="px-3 py-1.5 bg-slate-950/90 text-cyan-400 border border-cyan-500/30 rounded-xl text-[9px] font-bold uppercase tracking-wider hover:bg-cyan-500/10 hover:border-cyan-500 transition-all flex items-center gap-1.5"
            >
              ◀ Вернуться во внутренний двор Цитадели
            </button>
            <span className="text-[10px] font-mono text-slate-400">
              FateCastleManager.cs • Великая Академия Наук
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            
            {/* HERO COMBAT ACADEMY */}
            <div className="bg-slate-950 p-4 rounded-2xl border border-white/5 flex flex-col justify-between space-y-3 text-left">
              <div>
                <div className="text-[10px] font-black text-amber-400 uppercase tracking-tight flex items-center justify-between">
                  <span>⚔️ АКАДЕМИЯ ГЕРОЕВ</span>
                  {heroTrainingCooldownDay > 0 ? (
                    <span className="px-1.5 py-0.5 bg-rose-500/20 rounded text-[7px] text-rose-300 font-mono">
                      🕒 ПЕРЕЗАРЯДКА: {heroTrainingCooldownDay} дня
                    </span>
                  ) : (
                    <span className="px-1.5 py-0.5 bg-emerald-500/10 rounded text-[7px] text-emerald-400 font-mono">БАТАЛИЯ: ГОТОВ</span>
                  )}
                </div>
                <span className="text-[7px] font-mono text-slate-500 block mb-1">GameObject: Hero_Academy_Field</span>
                <p className="text-[9.5px] text-slate-400 leading-relaxed">
                  Курируйте Локала-Полководца для постоянного повышения его характеристик. Каждая боевая сессия дает опыт и отправляет плац на перезарядку.
                </p>
              </div>

              <div className="pt-2 border-t border-white/5 space-y-3">
                <div className="space-y-1">
                  <div className="flex justify-between text-[9px] font-mono text-slate-400">
                    <span>Уровень Героя Академии:</span>
                    <span className="text-white font-bold font-mono">Ур. {trainedHeroLvl}</span>
                  </div>
                  <div className="flex justify-between text-[9px] font-mono text-slate-400">
                    <span>Накопленный опыт:</span>
                    <span className="text-amber-400 font-bold font-mono">{trainedHeroXp} / 500 XP</span>
                  </div>
                  <div className="w-full h-1.5 bg-slate-900 rounded-full overflow-hidden">
                    <div className="h-full bg-amber-400 transition-all duration-300" style={{ width: `${(trainedHeroXp / 500) * 100}%` }} />
                  </div>
                </div>

                <button
                  onClick={() => {
                    if (heroTrainingCooldownDay > 0) {
                      showNotification(`❌ Тренировочное поле на перезарядке! Подождите еще дней: ${heroTrainingCooldownDay}`, "error");
                      return;
                    }
                    if (playerGold >= 50) {
                      setPlayerGold(prev => prev - 50);
                      setHeroTrainingCooldownDay(2);
                      const newXp = trainedHeroXp + 150;
                      if (newXp >= 500) {
                        setTrainedHeroLvl(prev => prev + 1);
                        setTrainedHeroXp(newXp - 500);
                        playClickSfx(880, 0.3);
                        showNotification("🏆 ПОВЫШЕНИЕ! Герой Академии поднялся на новый уровень!", "success");
                      } else {
                        setTrainedHeroXp(newXp);
                        playClickSfx(480, 0.2);
                        showNotification("🏋️‍♂️ Тренировка завершена! Герой получил +150 опыта.", "success");
                      }
                    } else {
                      showNotification("❌ Недостаточно золота для проведения тренировки (50)!", "error");
                    }
                  }}
                  disabled={heroTrainingCooldownDay > 0}
                  className={`w-full py-2 rounded-lg text-[9px] font-black uppercase tracking-wider transition-all cursor-pointer ${
                    heroTrainingCooldownDay > 0 
                      ? 'bg-slate-900 border border-rose-500/20 text-rose-400/50 cursor-not-allowed'
                      : 'bg-gradient-to-r from-amber-600 to-amber-500 text-white hover:scale-[1.02]'
                  }`}
                >
                  🏋️‍♂️ Начать Курс Боя (50 🪙)
                </button>
              </div>
            </div>


            {/* RECRUIT PARADE PLAZA */}
            <div className="bg-slate-950 p-4 rounded-2xl border border-white/5 flex flex-col justify-between space-y-3 text-left">
              <div>
                <div className="text-[10px] font-black text-emerald-400 uppercase tracking-tight flex items-center justify-between">
                  <span>👥 ПЛАЦ НАВОБРАНЦЕВ</span>
                  {warriorTrainingCooldownDay > 0 ? (
                    <span className="px-1.5 py-0.5 bg-rose-500/20 rounded text-[7px] text-rose-300 font-mono">
                      🕒 ПЕРЕЗАРЯДКА: {warriorTrainingCooldownDay} дня
                    </span>
                  ) : (
                    <span className="px-1.5 py-0.5 bg-emerald-500/10 rounded text-[7px] text-emerald-400 font-mono">СТУПЕНЬ: ГОТОВ</span>
                  )}
                </div>
                <span className="text-[7px] font-mono text-slate-500 block mb-1">GameObject: Troops_Training_Parade</span>
                <p className="text-[9.5px] text-slate-400 leading-relaxed">
                  Муштруйте рядовых бойцов и копейщиков. Повышение ранга улучшает их форму построения в автоматических битвах руин Континента.
                </p>
              </div>

              <div className="pt-2 border-t border-white/5 space-y-3">
                <div className="space-y-1">
                  <div className="flex justify-between text-[9px] font-mono text-slate-400">
                    <span>Ранг Бойцов Гарнизона:</span>
                    <span className="text-white font-bold font-mono">Ранг {trainedWarriorsLvl}</span>
                  </div>
                  <div className="flex justify-between text-[9px] font-mono text-slate-400">
                    <span>Опыт подразделения:</span>
                    <span className="text-emerald-400 font-bold font-mono">{trainedWarriorsXp} / 200 XP</span>
                  </div>
                  <div className="w-full h-1.5 bg-slate-900 rounded-full overflow-hidden">
                    <div className="h-full bg-emerald-400 transition-all duration-300" style={{ width: `${(trainedWarriorsXp / 200) * 100}%` }} />
                  </div>
                </div>

                <button
                  onClick={() => {
                    if (warriorTrainingCooldownDay > 0) {
                      showNotification(`❌ Подразделения восстанавливают силы! Подождите еще дней: ${warriorTrainingCooldownDay}`, "error");
                      return;
                    }
                    if (playerGold >= 30) {
                      setPlayerGold(prev => prev - 30);
                      setWarriorTrainingCooldownDay(1);
                      const newXp = trainedWarriorsXp + 60;
                      if (newXp >= 200) {
                        setTrainedWarriorsLvl(prev => prev + 1);
                        setTrainedWarriorsXp(newXp - 200);
                        playClickSfx(950, 0.35);
                        showNotification("🎖️ ВЫШЕ РАНГ! Гарнизон получил улучшение боевого построения альянса!", "success");
                      } else {
                        setTrainedWarriorsXp(newXp);
                        playClickSfx(580, 0.2);
                        showNotification("⚔️ Марш-бросок закончен! Воинам добавлено +60 опыта муштры.", "success");
                      }
                    } else {
                      showNotification("❌ Недостаточно золота для проработки марша (30)!", "error");
                    }
                  }}
                  disabled={warriorTrainingCooldownDay > 0}
                  className={`w-full py-2 rounded-lg text-[9px] font-black uppercase tracking-wider transition-all cursor-pointer ${
                    warriorTrainingCooldownDay > 0 
                      ? 'bg-slate-900 border border-rose-500/20 text-rose-400/50 cursor-not-allowed'
                      : 'bg-gradient-to-r from-emerald-600 to-teal-600 text-white hover:scale-[1.02]'
                  }`}
                >
                  ⚔️ Объявить Марш-Бросок (30 🪙)
                </button>
              </div>
            </div>

          </div>

        </div>
      )}

    </div>
  );
};
