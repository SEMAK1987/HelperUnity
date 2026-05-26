import fs from 'fs';

const filePath = 'src/App.tsx';
let content = fs.readFileSync(filePath, 'utf8');

// We find the Dialogue Body Text marker in the patched App.tsx
const startMarker = '                      {/* Dialogue Body Text */}';
const startIndex = content.indexOf(startMarker);

if (startIndex === -1) {
  console.error("Could not find dialogue body start marker!");
  process.exit(1);
}

// We find the Interactive Choice list marker in the patched App.tsx
const endMarker = '                              {/* Interactive Choice list inside Dialogue Bubble */}';
const endIndex = content.indexOf(endMarker);

if (endIndex === -1) {
  console.error("Could not find dialogue choice list end marker!");
  process.exit(1);
}

console.log("Found body range: from", startIndex, "to", endIndex);

const replacement = `                      {/* Dialogue Body Text */}
                      {/* Active Simulation dual-avatar workspace row */}
                      <div className="flex flex-col lg:flex-row items-stretch gap-8 mt-6 relative z-10">
                        {/* Aelyssa Left Portrait (White hair, neon-purple eyes) */}
                        <div className="flex flex-col items-center shrink-0">
                          <div className="w-24 h-24 md:w-32 md:h-32 rounded-[2rem] bg-black/80 border-2 border-indigo-500/80 shadow-[0_0_25px_rgba(99,102,241,0.25)] relative overflow-hidden transition-all duration-500 transform hover:scale-105">
                            {/* Aelyssa Hand-Drawn SVG Avatar */}
                            <svg className="w-full h-full p-2" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
                              <circle cx="50" cy="50" r="45" fill="#0f172a" />
                              {/* Glowing white haired elf hair silhouette */}
                              <path d="M20 70C20 40 30 15 50 15C70 15 80 40 80 70C75 75 70 80 50 82C30 80 25 75 20 70Z" fill="#e2e8f0" />
                              <path d="M10 50C10 40 25 20 50 20C75 20 90 40 90 50C90 60 85 75 50 85C15 75 10 60 10 50Z" fill="#f1f5f9" />
                              {/* Elf ears */}
                              <path d="M15 35C5 32 5 25 8 20C12 25 18 30 20 35H15Z" fill="#fbcfe8" />
                              <path d="M85 35C95 32 95 25 92 20C88 25 82 30 80 35H85Z" fill="#fbcfe8" />
                              {/* Face collar */}
                              <path d="M30 50C30 35 38 30 50 30C62 30 70 35 70 50C70 65 62 80 50 80C38 80 30 65 30 50Z" fill="#fbcfe8" />
                              {/* Long silver strands sweeping past face */}
                              <path d="M28 20C32 25 35 35 32 50C30 60 25 70 20 75" stroke="#f1f5f9" strokeWidth="2.5" />
                              <path d="M72 20C68 25 65 35 68 50C70 60 75 70 80 75" stroke="#f1f5f9" strokeWidth="2.5" />
                              {/* Deep neon-purple eyes */}
                              <ellipse cx="40" cy="48" rx="3" ry="5" fill="#a855f7" />
                              <ellipse cx="60" cy="48" rx="3" ry="5" fill="#a855f7" />
                              <circle cx="41" cy="46" r="1" fill="#ffffff" />
                              <circle cx="61" cy="46" r="1" fill="#ffffff" />
                              {/* Crystalline collar */}
                              <path d="M35 75C40 78 45 80 50 80C55 80 60 78 65 75" stroke="#818cf8" strokeWidth="3" />
                              <path d="M50 80L50 90" stroke="#f59e0b" strokeWidth="2" />
                            </svg>
                          </div>
                          <div className="mt-2.5 px-3 py-1 rounded-full bg-indigo-500/20 border border-indigo-400/30 shadow-md">
                            <span className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">
                              {simDialogueLang === 'RU' ? 'АЭЛИССА (ГИД)' : 'AELYSSA (GUIDE)'}
                            </span>
                          </div>
                        </div>

                        {/* Dialogue bubble middle column */}
                        <div className="flex-1 bg-black/50 rounded-[2.5rem] border border-white/5 p-6 md:p-8 flex flex-col justify-between relative overflow-hidden backdrop-mirror group hover:border-indigo-500/20 transition-all duration-300">
                          <div className="absolute inset-0 bg-gradient-to-b from-indigo-500/5 to-transparent pointer-events-none" />
                          <div className="relative z-10 flex flex-col h-full">
                            <div className="flex items-center gap-2">
                              <span className="w-2 h-2 rounded-full bg-green-500 animate-ping" />
                              <span className="text-[9px] font-black tracking-widest text-slate-500 uppercase">
                                {simDialogueLang === 'RU' ? 'АКТИВНАЯ РЕЧЬ' : 'ACTIVE SPEECH'}
                              </span>
                            </div>

                            <p className="text-[13px] md:text-sm text-slate-100 font-medium italic mt-2 leading-relaxed">
                              {simDialogueLang === 'RU' ? (
                                simDialogueStep === 0 ? 'Здравствуй, путник! Наш Континент Судьбы погружается во тьму древнего безвременья. Я буду сопровождать тебя в этом опасном походе.'
                                : simDialogueStep === 1 ? 'Меня зовут Аэлисса, хранительница священного Кристалла Зенита. Моя магия защитит тебя от коварства Кровавых Пустошей.'
                                : simDialogueStep === 2 ? 'Отважный боевой дух! Твое оружие уже заряжено энергией Зенита. Двинемся вперед через северные врата замка!'
                                : simDialogueStep === 3 ? 'Помни: каждый выбор здесь имеет значение. Наш отряд готов к бою. Теперь выбери область на Континенте Судьбы для первой боевой зачистки:'
                                : simDialogueStep === 4 ? 'Вы выбрали Кровавые Пустоши! Здесь сильны орды бандитов и адские ветры Зенита. Да пребудет с тобой благословение Кристалла! Мы отправляемся в бой.'
                                : simDialogueStep === 5 ? 'Вы выбрали Ледяной Пик! Вечная мерзлота проверяет волю на прочность, а Ледяные Големы стерегут древние сокровища. Да пребудет с тобой благословение Кристалла!'
                                : 'Вы выбрали Древние Руины! Забытые катакомбы хранят остатки древних кристаллов Зенита, но берегись ловушек и древних теней. Да пребудет с тобой благословение Кристалла!'
                              ) : simDialogueLang === 'EN' ? (
                                simDialogueStep === 0 ? 'Greetings, traveler! Our Fate Continent is sinking into the darkness of ancient timelessness. I will accompany you in this dangerous journey.'
                                : simDialogueStep === 1 ? 'My name is Aelyssa, keeper of the sacred Zenith Crystal. My magic will protect you from the treachery of the Crimson Wastes.'
                                : simDialogueStep === 2 ? 'Courageous battle spirit! Your weapon is infused with Zenith energy. Let us move forward through the northern castle gates!'
                                : simDialogueStep === 3 ? 'Remember: every choice here has consequences. Our squad is ready. Now select a territory on the Fate Continent for the initial tactical sweep:'
                                : simDialogueStep === 4 ? 'You have selected the Crimson Wastes! Bandit hordes and infernal Zenith winds plague this land. May the blessing of the Crystal guide us! Charging into battle.'
                                : simDialogueStep === 5 ? 'You have selected the Ice-Bound Peak! The absolute permafrost tests our resolve, while giant Ice Golems stand guard over absolute wonders. May the Crystal protect us!'
                                : 'You have selected the Ancient Ruins! Forgotten catacombs hold remnants of ancient Zenith energy crystals, but beware deadly traps and immortal shadows. Crystal bless you!'
                              ) : simDialogueLang === 'KR' ? (
                                simDialogueStep === 0 ? '반갑다, 여행자여! 우리의 운명 대륙이 고대 무한의 어둠 속으로 잠기고 있다. 내가 이 위험한 여정에 동행하겠다.'
                                : simDialogueStep === 1 ? '내 이름은 앨리사, 신성한 제니스 크리스탈의 수호자다. 나의 마법이 크림슨 황무지의 배신으로부터 당신을 지켜줄 것이다.'
                                : simDialogueStep === 2 ? '용감한 전향이여! 당신의 무기에 제니스 에너지가 주입되었다. 북쪽 성문을 통해 전진하자!'
                                : simDialogueStep === 3 ? '기억해라: 이곳에서의 모든 선택은 그 결과가 따른다. 우리 부대는 전투 준비가 끝났다. 이제 운명의 대륙에서 첫 전술적 소탕을 전개할 지역을 선택해라:'
                                : simDialogueStep === 4 ? '크림슨 황무지를 선택했다! 도적 떼와 거친 제니스 마력 폭풍이 몰아치는 대지다. 크리스탈의 축복이 당신을 인도하기를! 전장으로 진격한다.'
                                : simDialogueStep === 5 ? '빙설의 봉우리를 선택했다! 혹독한 영구 동토가 의지를 시험하며, 거대한 얼음 골렘들이 고대의 신비를 경비하고 있다. 크리스탈의 보살핌이 있기를!'
                                : '고대 유적지를 선택했다! 잊혀진 지하 묘지에 고대 제니스 마력 결정의 잔재가 숨겨져 있지만, 치명적인 함정과 불멸의 그림자를 경계해라. 크리스탈의 축복을!'
                              ) : (
                                simDialogueStep === 0 ? '你好，旅人！我们的命运大陆正在沉入远古无尽的黑暗之中。我将陪伴你度过这段危险的旅程。'
                                : simDialogueStep === 1 ? '我叫艾莉莎，神圣天顶水晶的守护者。我的魔法将保护你免受绯红荒野的防守犯。'
                                : simDialogueStep === 2 ? '英勇的斗志！你的武已经被注入了天顶能量。让我们从北门穿过城堡前进吧！'
                                : simDialogueStep === 3 ? '记住：这里的每一个选择都有其后果。我们的队伍已准备就绪。现在请选择命运大陆上的一个区域进行首次战术肃清：'
                                : simDialogueStep === 4 ? '你选择了绯红荒野！这里充斥着强盗匪帮和狂暴的天顶狂风。愿水晶祝福我们！即刻出发，开辟战场。'
                                : simDialogueStep === 5 ? '你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！'
                                : '你选择了远古遗迹！忘记的地下墓穴里藏着古老的高天魔力水晶，注意致命的陷阱和不朽的阴影。天顶水晶祝福你！'
                              )}
                            </p>
`;

content = content.slice(0, startIndex) + replacement + content.slice(endIndex);
fs.writeFileSync(filePath, content, 'utf8');
console.log("Successfully fixed dialogue body!");
