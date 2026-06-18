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
