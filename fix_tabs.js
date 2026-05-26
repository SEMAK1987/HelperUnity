import fs from 'fs';

const filePath = 'src/App.tsx';
let content = fs.readFileSync(filePath, 'utf8');

// Identify start marker
const startString = "designSubTab === 'Quests & NPC' ? (\n                  <motion.div \n                    initial={{ opacity: 0, y: 20 }}\n                    animate={{ opacity: 1, y: 0 }}\n                    className=\"space-y-12\"\n                  >\n                    {/* DIAMOND DESIGN DIALOGUE SYSTEM WORKSPACE v18.9.0 */}";

// Since line endings could be \r\n, let's locate the first occurrences with a flexible regex or simple searches.
const startIndex = content.indexOf(") : designSubTab === 'Quests & NPC' ? (\n                  <motion.div pointer-events-none") === -1 
  ? content.indexOf(") : designSubTab === 'Quests & NPC' ? (") 
  : content.indexOf(") : designSubTab === 'Quests & NPC' ? (\n                  <motion.div pointer-events-none");

if (startIndex === -1) {
  console.error("Could not find start index!");
  process.exit(1);
}

// Find ending of the corrupted duplicate block: up to "</span>\n                          </div>" near the Chinese dialog step 5
const endMarker = "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！'                            v18.9.0\r\n                            </span>\r\n                          </div>";
const endMarkerLF = "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！'                            v18.9.0\n                            </span>\n                          </div>";

let endIndex = content.indexOf(endMarker);
let markerLength = endMarker.length;

if (endIndex === -1) {
  endIndex = content.indexOf(endMarkerLF);
  markerLength = endMarkerLF.length;
}

if (endIndex === -1) {
  // Try matching just the v18.9.0 line with any line ending
  const vLine = "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！'                            v18.9.0";
  const vIndex = content.indexOf(vLine);
  if (vIndex !== -1) {
    const sectionAfter = content.slice(vIndex, vIndex + 500);
    const divCloseIndex = sectionAfter.indexOf("</div>");
    if (divCloseIndex !== -1) {
      endIndex = vIndex;
      markerLength = divCloseIndex + 6;
    }
  }
}

if (endIndex === -1) {
  console.error("Could not find end index!");
  process.exit(1);
}

console.log("Found range:", startIndex, "to", endIndex + markerLength);

// Let's build the perfect replacement block
const replacement = `) : designSubTab === 'Combat & Environment' ? (
                  <motion.div 
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="space-y-12"
                  >
                     <div className="grid grid-cols-1 xl:grid-cols-2 gap-8">
                        <div className="space-y-6">
                           <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4 flex items-center gap-2">
                              <MapIcon className="w-3 h-3" /> Континенты и Ландшафт
                           </h3>
                           <div className="grid grid-cols-1 gap-4">
                              {Object.entries(gameDesign?.world_combat_locations?.continents || {}).map(([key, data]: any) => (
                                <div key={key} className="p-8 rounded-[3rem] bg-black/40 border border-white/5 space-y-6 hover:bg-black/60 transition-all flex flex-col">
                                   <div className="flex items-center justify-between">
                                      <h4 className={\`text-xl font-black uppercase italic tracking-tighter \${
                                         key === 'plains_of_winds' ? 'text-green-400' :
                                         key === 'mountain_range' ? 'text-slate-400' :
                                         key === 'ancient_woods' ? 'text-emerald-500' : 'text-amber-400'
                                      }\`}>{data.name}</h4>
                                      <div className="flex gap-2 text-slate-500 opacity-50">
                                         {key === 'plains_of_winds' ? <Wind className="w-5 h-5" /> : 
                                          key === 'mountain_range' ? <Mountain className="w-5 h-5" /> :
                                          key === 'ancient_woods' ? <Flame className="w-5 h-5" /> : <Shield className="w-5 h-5" />}
                                      </div>
                                   </div>
                                   
                                   <div className="grid grid-cols-2 gap-4">
                                      <div className="space-y-2">
                                         <div className="text-[8px] text-slate-600 uppercase font-black tracking-widest">Состав Клеток:</div>
                                         <div className="space-y-1">
                                            {Object.entries(data.cells).map(([ctype, perc]: any) => (
                                              <div key={ctype} className="flex justify-between items-center text-[10px] text-slate-400">
                                                 <span className="italic">{ctype}</span>
                                                 <span className="font-mono">{perc}%</span>
                                              </div>
                                            ))}
                                         </div>
                                      </div>
                                      <div className="space-y-2">
                                         <div className="text-[8px] text-slate-600 uppercase font-black tracking-widest">Эффекты:</div>
                                         <div className="p-3 bg-white/5 rounded-xl space-y-1">
                                            <div className="text-[9px] text-green-400 font-bold tracking-tighter">{data.effects.bonus}</div>
                                            <div className="text-[9px] text-red-400 font-bold tracking-tighter">{data.effects.debuff}</div>
                                         </div>
                                      </div>
                                   </div>

                                   <div className="p-4 bg-white/5 rounded-2xl border border-white/5 mt-auto">
                                      <div className="text-[8px] text-slate-500 uppercase font-black mb-1">Тактика:</div>
                                      <p className="text-[11px] text-slate-300 italic leading-relaxed">{data.tactics}</p>
                                   </div>
                                </div>
                              ))}
                           </div>
                        </div>

                        <div className="space-y-8">
                           <div className="space-y-6">
                              <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] px-4 flex items-center gap-2">
                                 <Zap className="w-3 h-3 text-yellow-500" /> Динамические События
                              </h3>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                 <div className="p-8 rounded-[2.5rem] bg-indigo-600/5 border border-indigo-500/20 space-y-6">
                                    <div className="flex items-center gap-4">
                                       <div className="p-3 bg-blue-500 rounded-xl">
                                          <Droplets className="w-5 h-5 text-white" />
                                       </div>
                                       <div>
                                          <div className="text-[10px] font-black text-blue-400 uppercase tracking-widest">Weather: Rain</div>
                                          <div className="text-[9px] text-slate-500 font-bold uppercase">Chance: {gameDesign?.dynamic_events?.weather?.rain?.chance * 100}%</div>
                                       </div>
                                    </div>
                                    <p className="text-[10px] text-slate-400 italic leading-relaxed">{gameDesign?.dynamic_events?.weather?.rain?.effects}</p>
                                 </div>

                                 <div className="p-8 rounded-[2.5rem] bg-slate-600/5 border border-slate-500/20 space-y-6">
                                    <div className="flex items-center gap-4">
                                       <div className="p-3 bg-slate-500 rounded-xl">
                                          <CloudOff className="w-5 h-5 text-white" />
                                       </div>
                                       <div>
                                          <div className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Weather: Fog</div>
                                          <div className="text-[9px] text-slate-500 font-bold uppercase">Chance: {gameDesign?.dynamic_events?.weather?.fog?.chance * 100}%</div>
                                       </div>
                                    </div>
                                    <p className="text-[10px] text-slate-400 italic leading-relaxed">{gameDesign?.dynamic_events?.weather?.fog?.effects}</p>
                                 </div>
                              </div>
                           </div>

                           <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                              <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6 shadow-2xl">
                                 <Sun className="w-5 h-5 text-orange-400" />
                                 <div className="space-y-2">
                                    <h4 className="text-xs font-black text-white uppercase italic tracking-widest">Дневной Цикл</h4>
                                    <p className="text-[10px] text-slate-500 italic leading-relaxed">{gameDesign?.dynamic_events?.time_cycle?.day?.effects}</p>
                                 </div>
                              </div>
                              <div className="p-8 rounded-[2.5rem] bg-black/40 border border-white/10 space-y-6 shadow-2xl">
                                 <Moon className="w-5 h-5 text-indigo-400" />
                                 <div className="space-y-2">
                                    <h4 className="text-xs font-black text-white uppercase italic tracking-widest">Ночной Цикл</h4>
                                    <p className="text-[10px] text-slate-500 italic leading-relaxed">{gameDesign?.dynamic_events?.time_cycle?.night?.effects}</p>
                                 </div>
                              </div>
                           </div>

                           <div className="p-10 rounded-[3rem] bg-red-600/5 border border-red-500/20 space-y-6">
                              <h4 className="text-[10px] font-black text-red-500 uppercase tracking-[0.4em]">Случайные Угрозы</h4>
                              <div className="grid grid-cols-1 gap-4">
                                 {Object.entries(gameDesign?.dynamic_events?.random_encounters || {}).map(([key, data]: any) => (
                                   <div key={key} className="flex items-center justify-between p-4 bg-white/5 rounded-2xl border border-white/5 group hover:bg-white/10 transition-all cursor-default text-slate-100">
                                      <div className="flex flex-col gap-1">
                                         <span className="text-[11px] font-black text-white uppercase italic tracking-tighter">{key.replace('_', ' ')}</span>
                                         {data.chance && <span className="text-[9px] text-slate-600 font-bold uppercase">Шанс: {data.chance * 100}%</span>}
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
                        <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">Боевые Объекты и Клетки</h4>
                        <div className="grid grid-cols-2 md:grid-cols-5 gap-6">
                           {Object.entries(gameDesign?.world_combat_locations?.cell_types || {}).map(([key, data]: any) => (
                             <div key={key} className="flex flex-col items-center gap-4 p-6 rounded-3xl bg-white/5 border border-white/5 group hover:border-white/20 transition-all">
                                <div className={\`p-4 rounded-2xl \${
                                   key === 'passable' ? 'bg-green-600/10 text-green-400' :
                                   key === 'hard' ? 'bg-orange-600/10 text-orange-400' :
                                   key === 'impassable' ? 'bg-slate-600/10 text-slate-400' :
                                   key === 'hidden' ? 'bg-blue-600/10 text-blue-400' : 'bg-red-600/10 text-red-400'
                                }\`}>
                                   {key === 'passable' ? <Layout className="w-6 h-6" /> :
                                    key === 'hard' ? <Activity className="w-6 h-6" /> :
                                    key === 'impassable' ? <Box className="w-6 h-6" /> :
                                    key === 'hidden' ? <Eye className="w-6 h-6" /> : <Skull className="w-6 h-6" />}
                                </div>
                                <div className="text-center space-y-1">
                                   <div className="text-[11px] font-black text-white uppercase tracking-tighter">{data.name}</div>
                                   <div className="text-[8px] text-slate-500 font-bold uppercase">{data.penalty || data.effect || data.bonus}</div>
                                 </div>
                              </div>
                            ))}
                         </div>
                      </div>
                   </motion.div>
                ) : designSubTab === 'Potions & Alchemy' ? (
                  <motion.div 
                    initial={{ opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="space-y-12"
                  >
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                       <div className="lg:col-span-2 space-y-8">
                          <div className="flex items-center justify-between px-4">
                             <h3 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em] flex items-center gap-2">
                                <FlaskConical className="w-3 h-3 text-emerald-400" /> Алхимия и Зелья
                             </h3>
                             <div className="flex items-center gap-2">
                                <div className="px-2 py-1 bg-white/5 rounded border border-white/10 text-[8px] text-slate-500 font-black">LVL MULTIPLIER: {gameDesign?.potions_system?.scaling?.level_multiplier_formula}</div>
                             </div>
                          </div>
                          
                          <div className="space-y-4">
                              <div className="text-[9px] text-slate-500 uppercase font-black px-2">Правила использования</div>
                              <div className="grid grid-cols-1 gap-3">
                                 {[
                                   { t: "Cooldown", v: gameDesign?.potions_system?.rules?.cooldown, i: <Clock className="w-3 h-3 text-orange-400" /> },
                                   { t: "Visual", v: gameDesign?.potions_system?.rules?.visuals?.animation, i: <Eye className="w-3 h-3 text-blue-400" /> },
                                   { t: "Particles", v: "Зелёные (HP) / Синие (MP)", i: <Sparkles className="w-3 h-3 text-emerald-400" /> },
                                   { t: "UI Result", v: "Плавное заполнение + Floating Text", i: <Type className="w-3 h-3 text-purple-400" /> }
                                 ].map((rule, ri) => (
                                   <div key={ri} className="flex items-center gap-4 p-4 bg-white/5 rounded-2xl border border-white/5">
                                      <div className="p-2 bg-white/5 rounded-lg">{rule.i}</div>
                                      <div className="space-y-0.5">
                                         <div className="text-[9px] text-slate-500 font-black uppercase tracking-widest">{rule.t}</div>
                                         <div className="text-[10px] text-white font-bold leading-tight">{rule.v}</div>
                                      </div>
                                   </div>
                                 ))}
                              </div>
                              <div className="p-6 bg-emerald-600/10 rounded-[2.5rem] border border-emerald-500/20 text-[10px] text-emerald-300 italic leading-relaxed">
                                 "Система автоматически масштабирует полезность зелий от 1 до 9999 уровня, предотвращая инфляцию статов и сохраняя актуальность расходных материалов на всех этапах игры."
                              </div>
                           </div>
                       </div>

                       <div className="space-y-8">
                          <div className="p-8 rounded-[3rem] bg-indigo-600/5 border border-indigo-500/20 space-y-6">
                             <h4 className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">Артефакты Алхимии</h4>
                             <div className="space-y-3">
                                {gameDesign?.potions_system?.equipment_bonuses?.items?.map((item: any, i: number) => (
                                  <div key={i} className="flex items-center justify-between p-4 bg-black/40 rounded-2xl border border-white/5 group hover:border-indigo-500/30 transition-all">
                                     <div className="flex flex-col gap-0.5">
                                        <span className="text-[10px] font-black text-white uppercase italic tracking-tighter">{item.name}</span>
                                        <span className="text-[9px] text-slate-500 leading-none">{item.effect}</span>
                                     </div>
                                     <Star className="w-3 h-3 text-indigo-400 opacity-20 group-hover:opacity-100 transition-opacity" />
                                  </div>
                                ))}
                             </div>
                             <div className="p-3 bg-red-600/5 rounded-xl border border-red-500/20 text-[9px] text-red-300 italic text-center leading-snug">
                                "Максимальный суммарный бонус от экипировки: +{gameDesign?.potions_system?.equipment_bonuses?.max_capped_bonus * 100}%"
                             </div>
                          </div>

                          <div className="p-8 rounded-[3rem] bg-black/40 border border-white/5 space-y-6">
                             <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Эталонные Значения (scaling)</h4>
                             <div className="space-y-4">
                                {gameDesign?.potions_system?.scaling?.examples?.map((ex: any, i: number) => (
                                  <div key={i} className="flex items-center justify-between text-[10px]">
                                     <div className="flex items-center gap-3">
                                        <div className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
                                        <span className="text-slate-500 font-black">Level {ex.level}</span>
                                     </div>
                                     <span className="text-emerald-400 font-mono font-bold">x{ex.multiplier.toFixed(1)} effect</span>
                                  </div>
                                ))}
                             </div>
                          </div>
                       </div>
                    </div>

                    <div className="p-10 rounded-[4rem] bg-black/40 border border-white/10 space-y-8">
                       <div className="flex items-center justify-between">
                          <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">Реализация (C# MonoBehaviour)</h4>
                          <div className="flex items-center gap-4 text-[9px] text-slate-600 font-black">
                             <div className="flex items-center gap-1.5"><div className="w-2 h-2 rounded-full bg-green-500" /> SCALABLE</div>
                             <div className="flex items-center gap-1.5"><div className="w-2 h-2 rounded-full bg-blue-500" /> CLASS-AWARE</div>
                          </div>
                       </div>
                       <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                          <div className="space-y-3">
                             <div className="text-[9px] text-slate-500 uppercase font-black px-2 mb-1">Potion.cs</div>
                             <div className="p-6 bg-slate-950 rounded-3xl border border-white/5 text-[10px] text-blue-300 font-mono leading-relaxed overflow-x-auto whitespace-pre">
                   {\`public float CalculateRestoreAmount(CharacterStats target) {
    float levelMultiplier = 1f + Mathf.Log10(target.level);
    float baseRestoreAmount = 0;

    switch (potionType) {
        case SmallHealth:
            baseRestoreAmount = (base + level * factor); break;
        case MediumHealth:
            baseRestoreAmount = (target.maxHP * percentage); break;
        case LargeHealth:
            baseRestoreAmount = (base + level * factor) + (target.maxHP * percentage); break;
        // MP logic identical...
    }

    float classBonus = GetClassBonus(target);
    float equipBonus = target.GetEquipBonus();
    
    return baseRestoreAmount * levelMultiplier * classBonus * equipBonus;
}\`}
                             </div>
                          </div>
                       </div>
                    </div>
                  </motion.div>
                ) : designSubTab === 'Quests & NPC' ? (
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

                      <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-6 relative z-10">
                        <div>
                          <div className="flex items-center gap-3">
                            <span className="px-3 py-1 rounded-full bg-indigo-500/20 border border-indigo-500/30 text-[9px] font-black uppercase text-indigo-400 tracking-widest animate-pulse">
                              ZENITH EXCLUSIVE UI (НЕ ПЛАГИАТ)
                            </span>
                            <span className="px-2 py-0.5 rounded-full bg-amber-500/20 border border-amber-400/30 text-[9px] font-bold text-amber-400">
                              v18.9.0
                            </span>
                          </div>`;

content = content.slice(0, startIndex) + replacement + content.slice(endIndex + markerLength);

fs.writeFileSync(filePath, content, 'utf8');
console.log("Successfully patched App.tsx!");
