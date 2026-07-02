# 📘 Fate Continent • Книга Промптов и Инструкций (v18.11.17)

Добро пожаловать в официальный справочник и руководство по генерации ресурсов для проекта **Fate Continent (Континент Судьбы)**! 

Этот файл содержит все необходимые промпты для искусственного интеллекта **Leonardo.ai**, настройки генерации, подробные характеристики войск, а также рекомендации по переносу и интеграции графических асетов в кузницу, казармы и академию.

---

## 🎨 Инструкции по Настройке Генератора Leonardo.ai

Для создания идеальных, профессиональных портретов воинов, которые будут выглядеть как «одно лицо» (крупный план, высокая детализация, аккуратный игровой интерфейс), примените следующие параметры в веб-интерфейсе Leonardo.ai:

### ⚙️ Рекомендуемые Параметры:
1. **Preset / Model:** `3D Animation Style` или `Leonardo Phoenix` (для кинематографичности).
2. **Aspect Ratio (Соотношение сторон):** `1:1 (Square)` — идеально для круглых рамок интерфейса и слотов покупки.
3. **Contrast (Контраст):** `Medium` или `High` (для насыщенных неоновых HDR-свечений).
4. **Prompt Magic / Alchemy v2:** `ON` (High-resolution, RAW mode).
5. **Подход «Одно лицо» (Face Focus):** Добавление ключевых слов `centered portrait close-up`, `headshot focus`, `head and shoulders portrait`, `isolated dark slate grey background`. Это уберет лишние детали окружения и сфокусирует кадр исключительно на лице персонажа.

### 🚫 Негативный Промпт (Negative Prompt):
> `low quality, raw render, multiple heads, deformed face, hands, full body, asymmetrical eyes, text, signatures, blur, noisy background, bright sunlight background, extra limbs`

---

## ⚔️ Каталог Войск: Характеристики и Промпты для Leonardo.ai

Все базовые воины (простые типы) укомплектованы **2 пассивными навыками** и **1 активным (ульт) навыком**, а также автоматическим приростом характеристик за уровни.

### 1. 🛡️ Обычный Гвардеец (Guard - Мечник)
*   **Редкость:** `Обычный (Common)`
*   **Характеристики:** Сила `12`, Ловкость `8`, Интеллект `3`, Живучесть `14`. Auto-level up!
*   **Способности:**
    *   *Пассивный 1:* **Стальная Кожа** (+15 Броня).
    *   *Пассивный 2:* **Закаленный Боец** (+20% Макс. здоровье).
    *   *Активный (Ульт):* **Удар Защитника** (Наносит 250% физического урона одиночной цели и оглушает на 2 сек).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered high-contrast 3D game portrait, headshot of a loyal male guard wearing dark heavy metal steel helmet and armor with glowing neon emerald trim. Masterpiece close-up face, brave eyes, determined look, high fantasy RPG style, isolated dark game-asset background, cinematic octane render, 8k --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Стальная Кожа):* `Centered 3D fantasy game icon, glowing steel metallic plate skin texture with emerald magical runes, close-up, high-detail game asset, isolated black slate background, volumetric lighting`
    *   *Пассивный 2 (Закаленный Боец):* `Centered 3D fantasy game icon, a glowing red beating heart wrapped in heavy steel chains, high detailed vector-like render, neon highlights, isolated dark background`
    *   *Активный 1 (Удар Защитника):* `Centered 3D fantasy game icon, a heavy steel mace or sword crashing down with bright electric green shockwave effect, action splash, epic impact, isolated dark background`

---

### 2. 🏹 Лучник-Ополченец (Sentry - Стрелок)
*   **Редкость:** `Обычный (Common)`
*   **Характеристики:** Сила `8`, Ловкость `13`, Интеллект `4`, Живучесть `9`.
*   **Способности:**
    *   *Пассивный 1:* **Острый Глаз** (+25% Дальность атаки).
    *   *Пассивный 2:* **Легкий Шаг** (+15% Скорость передвижения).
    *   *Активный (Ульт):* **Дождь Стрел** (Шквал огненных стрел по площади наносит 180% урона в секунду).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered high-contrast 3D fantasy game headshot portrait of an agile archer ranger, leather cowl hood with glowing green runes, glowing eyes, focused gaze, holding a glowing wooden bow handle. Epic game asset style, isolated midnight slate grey background, high details --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Острый Глаз):* `Centered 3D game icon, glowing neon green archer's eye with target crosshairs and wind runes, high fantasy RPG style, game asset, isolated dark grey background`
    *   *Пассивный 2 (Легкий Шаг):* `Centered 3D game icon, a glowing winged runic boot steps on light leaves, neon green mist trail, speedy motion feel, isolated dark background`
    *   *Активный 1 (Дождь Стрел):* `Centered 3D game icon, a volley of flaming arrows raining down from heaven, blazing golden fire trail, epic action burst, isolated dark background`

---

### 3. 🔮 Адепт Ордена (Novice - Маг)
*   **Редкость:** `Обычный (Common)`
*   **Характеристики:** Сила `5`, Ловкость `7`, Интеллект `14`, Живучесть `8`.
*   **Способности:**
    *   *Пассивный 1:* **Эфирный Щит** (+20% Снижение магического урона).
    *   *Пассивный 2:* **Медитация** (+10 Регенерация маны за ход).
    *   *Активный (Ульт):* **Взрыв Ядра** (Концентрированная сфера тайной магии взрывается, нанося 350% урона по цели).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered 3D gaming portrait of a wizard apprentice, young mage wearing enchanted glowing purple hood cloak, starry sparks glowing all around, mysterious purple eyes, highly detailed magic character design, isolated deep dark backdrop, unity render engine visual --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Эфирный Щит):* `Centered 3D game icon, a bright glowing purple energy sphere dome shield protecting a silhouette, magic stardust, wizard style, isolated dark background`
    *   *Пассивный 2 (Медитация):* `Centered 3D game icon, a peaceful monk silhouette in lotus position, glowing violet energy chakras and lotus flower underneath, magic aura, isolated dark background`
    *   *Активный 1 (Взрыв Ядра):* `Centered 3D game icon, a massive explosion of a magical cosmic purple core, solar flares, magical shockwave, isolated dark background`

---

### 4. 🥇 Легендарный Крестоносец (Paladin Crusader)
*   **Редкость:** `Легендарный (Legendary)`
*   **Характеристики:** Сила `22`, Ловкость `12`, Интеллект `10`, Живучесть `20`.
*   **Способности:**
    *   *Пассивный 1:* **Святая Аура** (+30% Потребление маны врагами снижено в зоне ауры).
    *   *Пассивный 2:* **Реванш** (Отражает 25% урона обратно атакующим).
    *   *Активный (Ульт):* **Гнев Небес** (Призывает луч священной энергии, излечивающий союзников на 300 HP и наносящий 400 урона врагам).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered gorgeous 3D game portrait of a Legendary Paladin in glorious glowing gold heavy plate armor with amber halo glowing crown. Majestic face with silver beard and shining eyes, ultra high fantasy, isolated dark grey volumetric atmosphere backdrop, high render quality --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Святая Аура):* `Centered 3D game icon, a glowing warm golden angel wings and radial pulse aura, celestial light rays, sacred holy spell, game asset, isolated dark background`
    *   *Пассивный 2 (Реванш):* `Centered 3D game icon, a golden shield reflecting an energy bolt, spikes of returned energy, fire spark particles, game asset, isolated dark background`
    *   *Активный 1 (Гнев Небес):* `Centered 3D game icon, a column of divine golden light beam striking from sky, majestic holy cross symbol inside, cosmic spark, isolated dark background`

---

### 5. 🦅 Лунный Следопыт (Moon Huntress)
*   **Редкость:** `Легендарный (Legendary)`
*   **Характеристики:** Сила `14`, Ловкость `24`, Интеллект `12`, Живучесть `13`.
*   **Способности:**
    *   *Пассивный 1:* **Лунное Двойничество** (+30% к шансу уклониться от удара).
    *   *Пассивный 2:* **Оперение Ветра** (Придает 20% критического шанса при дальнем выстреле).
    *   *Активный (Ульт):* **Звездный Выстрел** (Мгновенно уничтожает цель с низким уровнем здоровья, нанося 600% сквозного урона).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered legendary 3D portrait headshot of an elite night elf moon huntress, glowing cyan face paint marks, silver-white braided hair, glowing crystal circlet headpiece. Deep neon mystical twilight mood, elegant game avatar, isolated dark slate grey background --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Лунное Двойничество):* `Centered 3D game icon, multiple glowing cyan moon shadows of a dancing silhouette, ethereal mirror images, starry sparkles, isolated dark background`
    *   *Пассивный 2 (Оперение Ветра):* `Centered 3D game icon, a mystical feather glowing with wind vortex wisps and critical strike sparks, teal cyan neon glow, isolated dark background`
    *   *Активный 1 (Звездный Выстрел):* `Centered 3D game icon, an epic glowing arrow shot turning into a sparkling shooting star, constellation patterns, cosmic explosion, isolated dark background`

---

### 6. 🐉 Великий Хранитель Бездны (Void Archmage)
*   **Редкость:** `Легендарный (Legendary)`
*   **Характеристики:** Сила `9`, Ловкость `15`, Интеллект `26`, Живучесть `12`.
*   **Способности:**
    *   *Пассивный 1:* **Сингулярность** (+40% урона от заклинаний Бездны).
    *   *Пассивный 2:* **Искажение Времени** (+15% к скорости перезарядки ультимативных способностей союзников).
    *   *Активный (Ульт):* **Коллапс Бездны** (Разрывает пространство, нанося колоссальный урон всем противникам и оглушая их на 4 секунды).
*   **✍️ Промпт для Leonardo.ai (Портрет):**
    > `Centered 3D gaming portrait of a legendary elder void archmage, ancient celestial sorcerer, glowing neon violet star constellation floating around, deep cosmic eyes, flowing energy beard, isolated clean dark abstract gaming space background, Unreal Engine 5 aesthetic --v 6.0`
*   **✍️ Промпты для Иконок Навыков (Leonardo Skill Icons):**
    *   *Пассивный 1 (Сингулярность):* `Centered 3D game icon, a glowing miniature black hole pulling in deep purple star dust and space nebulae, celestial magic sphere, isolated dark background`
    *   *Пассивный 2 (Искажение Времени):* `Centered 3D game icon, a magical hourglass cracking, glowing neon violet gears and clock hands floating out, celestial time warp speed, isolated dark background`
    *   *Активный 1 (Коллапс Бездны):* `Centered 3D game icon, a rift in reality tearing open, violent purple cosmic lighting, massive dimensional crack, dark matter explosion, isolated dark background`

---

## 🔮 Книга Промптов для Навыков Войск (Barracks Troop Skills Prompts Book)

Используйте эти профессионально разработанные промпты для генератора **Leonardo.ai**, чтобы создать стильные, высококонтрастные квадратные иконки навыков (3D RPG/Fantasy Skill Icons). Все промпты оптимизированы для моделей `Leonardo Phoenix`, `3D Animation Style` или `Leonardo XL`.

### 1. 🛡️ Боец фракции (Faction Warrior)
*   🔥 **Активный навык: Удар щитом (Shield Slam)**
    > `Centered 3D fantasy game icon, a massive metal tower shield impacting the air with visual shockwave ripples, vibrant emerald green glow, professional clean vector skill art, isolated dark slate grey background, high contrast octane render`
*   ❄️ **Пассивный 1: Железная Воля (Iron Will)**
    > `Centered 3D fantasy game icon, a glowing heavy iron shield overlaid with light blue divine wings, bright celestial glow, clean vector style, game ability skill icon, dark fantasy theme, isolated dark background`

---

### 2. 🏹 Эльфийский Лучник (Elven Archer)
*   🔥 **Активный навык: Стрела Ветра (Wind Arrow)**
    > `Centered 3D fantasy game icon, a high speed projectile arrow enveloped in spiral green wind currents and glowing sparks, fantasy spell projectile, clean vector asset, dark background, dynamic lighting`
*   ❄️ **Пассивный 1: Меткий Взгляд (Eagle Eye)**
    > `Centered 3D fantasy game icon, a sharp glowing emerald eye target reticle lock, neon green digital lines, clean simplistic mobile ui skill vector icon, fantasy game asset, isolated black slate background`

---

### 3. 🔮 Боевой Маг Зенита (Zenith Battle Mage)
*   🔥 **Активный навык: Чародейская Вспышка (Arcane Flash)**
    > `Centered 3D fantasy game icon, a magnificent spiral cosmic explosion of violet nebula starlight, beam of magical projectile energy, spell ability tile icon vector, neon purple glow, isolated dark background`
*   ❄️ **Пассивный 1: Источник Маны (Mana Source)**
    > `Centered 3D fantasy game icon, a crystal flask shaped container filled with glowing liquid purple magic energy, starry sparkles, stylized mobile RPG skill vector icon, isolated dark background`

---

### 4. 🥇 Паладин Света (Holy Paladin)
*   🔥 **Активный навык: Очищение (Cleansing)**
    > `Centered 3D fantasy game icon, a warm divine light beam descending from heaven, dissolving black shadow magic, fantasy healing spell skill design icon, golden neon light, isolated dark background`
*   ❄️ **Пассивный 1: Аура Света (Holy Aura)**
    > `Centered 3D fantasy game icon, golden mystical sun rays bursting outwards from a glowing star construct, fantasy halo aura, vector mobile ui icon asset, isolated dark background`
*   ❄️ **Пассивный 2: Священный Доспех (Sacred Plate)**
    > `Centered 3D fantasy game icon, a majestic celestial shining golden breastplate armor, surrounded by holy runic symbols, pristine specular shine, professional game asset, isolated slate background`

---

### 5. 🐎 Имперская Конница (Imperial Cavalry)
*   🔥 **Активный навык: Разбег (Charge)**
    > `Centered 3D fantasy game icon, heavy steel lance tip sparking with lightning kinetic force during a powerful thrust motion, vector web emblem design, high action impact, isolated dark background`
*   ❄️ **Пассивный 1: Натиск (Onslaught)**
    > `Centered 3D fantasy game icon, a silhouetted heavy warhorse hoof kicking up dirt with golden energy trail, motion blur, speed skill emblem icon, isolated dark background`
*   ❄️ **Пассивный 2: Закаленный Всадник (Veteran Rider)**
    > `Centered 3D fantasy game icon, twin crossed iron lances wrapped in red banners, royal golden insignia emblem, medieval battle pass skill icon, isolated dark background`

---

### 6. 💣 Осадно-боевой Пушкарь (Garrison Cannoneer)
*   🔥 **Активный навык: Разрушительный Залп (Demolishing Blast)**
    > `Centered 3D fantasy game icon, massive bronze mortar cannon barrel firing a fiery exploding cannonball with thick smoke rings, stylized 3D blast icon, fiery orange embers, isolated dark background`
*   ❄️ **Пассивный 1: Осадный Прицел (Siege Crosshair)**
    > `Centered 3D fantasy game icon, a digital crosshair overlay on a stone castle wall projection with red structural stress points, skill icon, isolated dark background`
*   ❄️ **Пассивный 2: Тяжелый Порох (Heavy Powder)**
    > `Centered 3D fantasy game icon, a wooden barrel filled with black gunpowder with a burning sparkling fuse, game skill icon design, high-contrast, isolated dark background`

---

### 7. 🦌 Кентавр Степей (Steppe Centaur)
*   🔥 **Активный навык: Бросок Копья (Spear Throw)**
    > `Centered 3D fantasy game icon, a razor sharp war spear propelled forward with intense yellow sonic bloom and speed lines, game skill icon, isolated dark background`
*   ❄️ **Пассивный 1: Степной Ветер (Steppe Wind)**
    > `Centered 3D fantasy game icon, a whirlwind spiral dust wind trail over wild grassy plains, speed visual feedback, vector talent icon, isolated dark background`
*   ❄️ **Пассивный 2: Охотничий Инстинкт (Hunter Instinct)**
    > `Centered 3D fantasy game icon, wild beast golden predator claw marks glowing yellow, stylized nature hunter emblem, game asset graphic, isolated dark background`

---

### 8. 💀 Некромант Тьмы (Shadow Necromancer)
*   🔥 **Активный навык: Подъем Скелета (Raise Skeleton)**
    > `Centered 3D fantasy game icon, a skeletal bony hand breaking through dry graveyard soil holding a rusted iron blade, under eerie neon green moonlight, isolated dark background`
*   ❄️ **Пассивный 1: Жатва Душ (Soul Harvest)**
    > `Centered 3D fantasy game icon, glowing neon green skeletal hands snatching wandering spectral ghost soul wisps, necromancy spell ability emblem design, isolated dark background`
*   ❄️ **Пассивный 2: Оскверненная Кровь (Vile Blood)**
    > `Centered 3D fantasy game icon, a splat of dark toxic purple blood causing smoke acid melting on ground, mobile tactical ui icon, isolated dark background`

---

### 9. 🦅 Элитный Королевский Грифон (Royal Griffin)
*   🔥 **Активный навык: Удар Когтями (Talon Slash)**
    > `Centered 3D fantasy game icon, four razor sharp metal talon claw marks glowing white cutting through slate iron armor metal plates, sparks, isolated dark background`
*   ❄️ **Пассивный 1: Превосходство Высоты (Altitude Dominance)**
    > `Centered 3D fantasy game icon, a majestic giant eagle silhouette diving from clouds against a bright sun, wings spread, fantasy skill icon vectors, isolated dark background`
*   ❄️ **Пассивный 2: Неуловимый Полет (Evasive Flight)**
    > `Centered 3D fantasy game icon, feather wings flapping leaving faint gold sparkles traces, speed agility passive icon decoration, isolated dark background`
*   ❄️ **Пассивный 3: Гнездовье (The Nest)**
    > `Centered 3D fantasy game icon, a woven wooden high nest holding a golden glowing bird egg on a stellar high mountaintop, starry sky, isolated dark background`

---

### 10. 👑 Рыцарь-Властелин (Dread Overlord)
*   🔥 **Активный навык: Клинок Бездны (Abyss Blade)**
    > `Centered 3D fantasy game icon, a gigantic spiky obsidian greatsword blade wreathed in dark purple flames and dark magic trail arc, isolated dark background`
*   ❄️ **Пассивный 1: Аура Ужаса (Dread Aura)**
    > `Centered 3D fantasy game icon, a terrifying demonic face shadow mask outline with glowing void purple eyes, horror psychological warfare icon, isolated dark background`
*   ❄️ **Пассивный 2: Прилив Скверны (Corruption Surge)**
    > `Centered 3D fantasy game icon, a black bubbling dynamic wave of dark corrupted purple water rising with red highlights, magical corruption, isolated dark background`
*   ❄️ **Пассивный 3: Костяной Щит (Bone Shield)**
    > `Centered 3D fantasy game icon, a ring of three spinning jagged human ribs bones creating a protective purple spectral shield barrier, isolated dark background`

---

### 11. 🐍 Многоголовая Гидра (Swamp Hydra)
*   🔥 **Активный навык: Тройная Атака (Triple Strike)**
    > `Centered 3D fantasy game icon, three giant scary green snake heads lunging simultaneously forward in a dynamic bite action from left to right, isolated dark background`
*   ❄️ **Пассивный 1: Кислотные Укусы (Acidic Bites)**
    > `Centered 3D fantasy game icon, two green reptilian snake fangs dripping luminous fluid green venom droplets, toxic acid, isolated dark background`
*   ❄️ **Пассивный 2: Регенерация Тела (Body Regeneration)**
    > `Centered 3D fantasy game icon, a green lizard scaly tail re-growing with light blue biological cellular cell activity glowing layers, isolated dark background`
*   ❄️ **Пассивный 3: Токсичная Кожа (Toxic Skin)**
    > `Centered 3D fantasy game icon, a close-up of poisonous swamp frog skin texture with neon green toxic pores, high fantasy style, isolated dark background`

---

### 12. 🌌 Легендарный Дракон Пустоты (Void Dragon)
*   🔥 **Активный навык: Дыхание Плазмы (Plasma Breath)**
    > `Centered 3D fantasy game icon, a stream of brilliant cosmic purple stellar flame blast incinerating iron targets, cosmic dragon breath, isolated black background`
*   ❄️ **Пассивный 1: Чешуя Пустоты (Void Scales)**
    > `Centered 3D fantasy game icon, indestructible dark amethyst crystal dragon scales layout glistening with starry points, spell deflection, isolated dark background`
*   ❄️ **Пассивный 2: Межзвездная Ярость (Interstellar Rage)**
    > `Centered 3D fantasy game icon, a raging cosmic violet dragon claw clutching a hot core of glowing supernova star, raw power, isolated dark background`
*   ❄️ **Пассивный 3: Суперсонический полет (Supersonic Flight)**
    > `Centered 3D fantasy game icon, dragon wings outline glowing at warp speed crossing star systems, sonic boom ripples, isolated dark background`

---

### 13. 🐻 Ураганный Медведь Гор (Mountain Bear Guard)
*   🔥 **Активный навык: Растерзание (Mangle)**
    > `Centered 3D fantasy game icon, enormous bear claws slashing vertically downwards leaving three thick blue ice-frost gashes in the midnight air, cold frost, isolated dark background`
*   ❄️ **Пассивный 1: Морозная Стойкость (Frost Resilience)**
    > `Centered 3D fantasy game icon, an armored polar bear footprint seal glowing with cold runic frost blue energy on snow surface, isolated dark background`
*   ❄️ **Пассивный 2: Снежный Гнев (Snow Fury)**
    > `Centered 3D fantasy game icon, a raging polar bear face silhouette glowing red inside a frosted blue glacier shard outline, isolated dark background`
*   ❄️ **Пассивный 3: Ледяной Доспех (Glacier Plates)**
    > `Centered 3D fantasy game icon, a thick slab of clear polar blue glacier ice plate covering ancient steel chest piece armor, isolated dark background`

---

### 14. 🐛 Гигантская Змея Пустошей (Wasteland Serpent)
*   🔥 **Активный навык: Поглощение (Devour)**
    > `Centered 3D fantasy game icon, a massive vertical desert serpent mouth filled with rows of needle teeth rising directly from a sand whirlpool, isolated dark background`
*   ❄️ **Пассивный 1: Песчаная Скрытность (Sand Stealth)**
    > `Centered 3D fantasy game icon, a golden sandy whirlpool vortex sucking down debris under bright intense desert sun, sandstorm, isolated dark background`
*   ❄️ **Пассивный 2: Твердость Чешуи (Carapace Hardness)**
    > `Centered 3D fantasy game icon, a detailed layer of diamond hard golden crystalline snake skin scales pattern, shiny hot desert sunlight glint, isolated dark background`
*   ❄️ **Пассивный 3: Дюны Внимания (Dune Presence)**
    > `Centered 3D fantasy game icon, a dune mirage of giant golden snake eyes outline shimmering over hot heatwave desert sand, isolated dark background`

---

## 🛠️ Что Именно Было Изменено в Скриптах Проекта

Внесенные изменения полностью соответствуют протоколам высокой стандартизованности и оптимизации (v18.11.17):

1.  **Создан новый компонент `src/components/CastleFacilities.tsx`:**
    *   Реализует **три полностью независимых графических окна** вместо старых плоских вкладок: **Казармы (Barracks)**, **Кузница (Forge)**, **Академия & Арена (Academy & Arena)**.
    *   Окна переключаются через стильное интерактивное ядро Hub с красивым неоновым свечением и HUD-кнопками возврата.
    *   Интегрирована полноценная система карточек и профилей юнитов с интерактивным выбором.
    *   Реализована плашка наглядных параметров Leonardo.ai для каждого выбранного воина, чтобы вы могли прямо в игре видеть готовый промпт, копировать его одной кнопкой и вставлять в Леонардо.
    *   Выведена кнопка загрузки собственных изображений («Загрузить свой арт из Leonardo»), которая мгновенно сохраняет загруженный файл в локальный кеш игры (`customImages` state).
    *   Графически представлены опыт персонажа (EXP Ring), шкала уровней, а также красивый список активных и пассивных навыков с неоновыми рамками.

2.  **Интегрирован новый компонент в `src/App.tsx`:**
    *   Все 700+ строк устаревшего дублирующего кода панели вкладок Шага 10 убраны в безопасный оптимизированный модульный вызов `<CastleFacilities ... />`.
    *   Все внутренние игровые состояния, такие как золото, опыт воинов, уровни, Cooldown-дни, экипировка полководца (equippedItems) полностью синхронизированы через пропсы, гарантируя целостность игрового прогресса.
    *   Проведен автоматический рефакторинг Indent-форматирования с помощью Prettier (`npx prettier`), устранивший любые сбои компиляции.
    *   Внедрено GPU Anti-Overheat ограничение частоты кадров в SettingsManager и доскональное сохранение разрешения.

---

## 📂 Как Переносить Изображения в Игру

1.  Генерируйте изображения по промптам выше в **Leonardo.ai**.
2.  В меню Казарм кликните на нужного воина в списке слева.
3.  Нажмите кнопку **«📁 Загрузить Свой Иллюстративный Арт»** прямо под портретом героя в правой детализированной панели.
4.  Выберите сохраненный `.png` или `.jpeg` файл с вашего компьютера.
5.  Интерфейс мгновенно обновится: вместо стандартной векторной заглушки появится сгенерированное вами лицо воина в высоком качестве, а неоновые рамки и индикаторы опыта аккуратно наложатся поверх загруженного арта!
