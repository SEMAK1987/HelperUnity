# 🌌 Fate Continent (Континент Судьбы) — Справка по возможностям проекта и Руководство по интеграции (v18.11.16)

Добро пожаловать в единый центр знаний вашего проекта! Этот документ содержит полное описание всего, что умеет текущая сборка игры, пошаговые инструкции по работе с графикой/моделями и готовые шпаргалки для переноса ресурсов в Unity 6.

---

## 🗺️ КРАТКИЙ СПИСОК ВОЗМОЖНОСТЕЙ & ЧТО УЖЕ СДЕЛАНО

Проект представляет собой полноценную основу для пошаговой стратегии с элементами RPG (**Turn-Based Strategy & RPG**), оптимизированную под **Unity 6** и оформленную в ультрасовременном стиле **Zenith Glassmorphic UI (8K)**.

Below is what we have implemented and stabilized:

### 1. 🏰 Пошаговое управление королевством и Замок (v18.11.14 - v18.11.15)
- **Turn-Based Castle Income Ticking:** Полностью удалено ежесекундное начисление золота "в фоне". Теперь казна пополняется строго за каждый завершенный ход (при нажатии **End Turn**).
- **Castle Level 1 ➔ Level 2 Shape-Shifting:** Внедрена процедурная система строительства. При улучшении замка происходит плавный 3D-морфинг внешнего вида постройки прямо на карте.
- **Гарнизоны и Рекрутинг:** Наем различных родов войск в замке с автоматическим масштабированием вместимости замка (*Garrison Capacity*) в зависимости от его уровня.
- **Шпионаж и Разведка:** Система посылки лазутчиков во вражеские замки с автономной калибровкой цен на основе разницы в силе королевств.
- **Пошаговый ИИ Симулятор:** Противники делают осмысленные ходы в реальном времени, сбалансировано прокачивают свои цитадели и нанимают новые подразделения из гарнизона.

### 2. 🧬 RPG Глубина: Атрибуты Классов, Свободные Очки и Глоссарий (v18.11.15)
- **Character Initialization:** 3 игровых класса с уникальным распределением базовых характеристик:
  - **Воин**: Сила (STR) 15, Ловкость (AGI) 10, Интеллект (INT) 4, Выносливость (STA) 15.
  - **Лучник**: Сила 10, Ловкость 14, Интеллект 6, Выносливость 11.
  - **Маг**: Сила 6, Ловкость 10, Интеллект 10, Выносливость 9.
- **Difficulty Point Pools:** Динамический пул бонусных очков для старта на основе сложности:
  - *Новичок*: +30 очков | *Легко*: +20 очков | *Нормально*: +10 очков | *Сложно*: +5 очков | *Кошмар*: +0 очков.
- **Safety Base Barrier:** Предотвращено уменьшение характеристик ниже базовых значений класса в меню распределения статов.
- **Auto-Allocation Panel:** Автоматическое интеллектуальное распределение очков согласно весам выбранного класса.
- **Skills Glossary Panel:** Красивое стеклянное всплывающее меню с глоссарием пассивных умений и ультимативных способностей (с поддержкой перевода на несколько языков).

### 3. 🎥 Улучшенная Камера, Edge Scrolling & Границы Меша (v18.11.11 - v18.11.13)
- **Ground-Focused Camera Clamping:** Камера теперь математически ограничивает фокусную точку взгляда на земле ($Y = 0$), а не свои физические координаты. Это убрало старые баги, когда камера улетала в пустоту или застревала при зуме.
- **New Input System & Preprocessor Switch:** Авто-переключение на New Input System без ошибок компиляции и вылетов.
- **AutoFitBounds System:** Камера автоматически сканирует размер меша острова `New_Kontinent` и динамически настраивает лимиты передвижения.
- **Edge Scrolling:** Перемещение по карте классическим способом — подведением курсора мыши к краям экрана.
- **Calibrated Camera Heights:** Улучшены дефолтные параметры высоты камеры при высадке (спуск с 15f до комфортных 2.5f с автоматической доводкой).

### 4. 🌊 Окружение, Океан и Графика (v18.11.12)
- **Ocean Plane Spawner:** Процедурный спавн бесконечного океана с 40x40 UV-тайлингом для PBR текстур высокого разрешения.
- **Quality Sync Shader:** Вода автоматически меняет свою металличность, зеркальность и детализацию в зависимости от графических настроек (Low UI, Med, Ultra 8K).
- **Ocean Occlusion & Dialogue Security:** Океан автоматически скрывается во время диалоговой сцены завязки игры и плавно включается после десантирования игрока на остров, сохраняя чистоту сцены.

### 5. 📖 Нарративная Система и Высадка (v18.11.14)
- **Post-Landing Briefing (Шаги 8-12):** Включен детальный сюжетный брифинг. Камера блокирует управление игрока, фокусируется на его стартовом замке и начинает плавный текстовый диалог с Аэлиссой о планах по развитию континента.
- **Landing Position Synchronizer:** Синхронизация 4 зон высадки:
  - *Кровавые Пустоши* -> `Oasis_SpawnPoint`
  - *Ледяной Пик* -> `Outpost_SpawnPoint`
  - *Древние Руины* -> `Shore_SpawnPoint`
  - *Святилище Зенита* -> `Citadel_SpawnPoint`
- **Clipping Protection:** Локальная позиция Z колец-маркеров жестко установлена на $-2.0\text{f}$, а спутников/героев на $-2.05\text{f}$. Это избавило от мерцания и взаимного наложения объектов на 3D-террейн карты.
- **Dialog Purity Mode:** Моментальное скрытие маркерных кругов, кнопок высадки и фона карты во время активных диалогов и их возврат при завершении беседы.

### 6. 🛡️ Безопасность и Глобальные Настройки (v18.11.16)
- **GPU Anti-Overheat Protection:** Встроенный ограничитель кадров в `SettingsManager.cs` (30 FPS на Low для предотвращения перегрева GPU при частых игровых тестах в Unity, 60 FPS на Med/High, 120 FPS на Ultra). Вес эффектов Bloom и Post-Processing принудительно снижается до 15% на слабых машинах.
- **Resolution Universal Sync:** Полное сохранение и динамическое восстановление системного разрешения экрана и режима во весь экран при переходах между сценами и во время загрузок.
- **Anti-Cheat Save Verifier:** Хэширование файлов сохранений с проверкой целостности методом HMAC-SHA256, проверка процессов на наличие запущенных утилит взлома (CheatEngine и др.) и валидатор времени против Speedhack.

---

## 🎨 ЧАСТЬ 2: ГДЕ ВЗЯТЬ ПРОМПТЫ ДЛЯ ВОЙСК И ПОРТРЕТОВ?

Все необходимые промпты настроены с учетом современных требований и находятся в файле:
👉 **`GUIDE_CASTLES_AND_3D_HEROES.md` (внутри папки вашего проекта)**

### Почему наши промпты — лучшие для работы с Leonardo.ai + Hunyuan 3D?
Обычные генерации нейросетей страдают от трех проблем: оружие парит отдельно от рук бойца, под ногами рисуется круглый «пьедестал», а на заднем плане видны темные тени. Хунвей 3D воспринимает эти тени и подиумы как часть тела героя, превращая их в уродливые наросты на 3D-модели.
**Наши новые промпты содержат:**
1. Требование **«firmly holding [weapon] in hand»** — персонаж намертво сжимает меч/лук/посох в своих руках.
2. Идеальный белый изолированный фон: **«solid flat pure white background (#ffffff)»**.
3. Жёсткий негативный промпт, блокирующий тени на полу, градиенты фона и пластиковые подпорки.

### 📋 Золотые промпты для Leonardo.ai (Скопируйте в буфер!):

**Категория в Leonardo.ai:** Использовать модель `Leonardo Vision XL` или `3D Animation Style`, формат изображения `Соотношение сторон 1:1`.

```text
// 1. Негативный промпт (Вставить в поле Negative Prompt! ОБЯЗАТЕЛЬНО!):
black background, dark background, grey background, floor shadow, ambient shadow, color gradient, vignette, pedestal, circular base, float pedestal, duplicate items, floating weapons, weapons on side, multiple angles, background leaks, splatters, dirt, cropped image, bad anatomy, flat 2D graphic.
```

```text
// 2. Основной Воин / Паладин (Main Warrior):
Symmetrical full-body 3D game model of a majestic royal paladin knight in glowing cyan and gold steel armor, standing symmetrically in A-pose. He is firmly holding a glowing cyan runic broadsword in his right hand. Straight front-facing view, cartoon style, high-detail clay render, unreal engine 5 render, glowing emissive details. Isolated on a solid flat pure white background (#ffffff), no floor shadows, no ambient occlusion, no pedestal base.
```

```text
// 3. Простой Воин / Рекрут замка (Simple Warrior):
Symmetrical full-body 3D game model of a simple town guard recruit warrior, wearing plain steel plate armor and an iron helmet. Standing in a straight front-facing A-pose, firmly holding a clean round steel shield in his left hand and a short iron broadsword in his right hand. Cute clay texture, bright saturated colors, game-ready asset. Isolated on a solid flat pure white background (#ffffff), no shadows on floor, no background gradients, no pedestal.
```

```text
// 4. Основной Королевский Стрелок (Main Archer):
Symmetrical full-body 3D fantasy character model of an elegant elven master archer, wearing emerald green leather armor with cyan glowing runic lines. Standing in a straight front-facing A-pose, firmly holding a beautiful glowing recurve bow in his left hand, and a clean leather quiver with arrows secured on his back. Epic stylized toy design. Isolated on a solid flat pure white background (#ffffff), no floor shadows, no ambient gradients, no vignette, no pedestal.
```

```text
// 5. Простой Лесной Лучник (Simple Archer):
Symmetrical full-body 3D game character model of a forest scout archer holding a simple wooden bow firmly in his right hand, wearing a leather vest and a simple green hood. Tabletop toy miniature style, cute claymation aesthetic, game ready asset, straight frontal view, standing symmetrically. Isolated on a solid flat pure white background (#ffffff), zero floor shadows, no dirty spots, no bases.
```

```text
// 6. Продвинутый Маг / Архимаг (Main Mage):
Symmetrical full-body 3D character figurine of a powerful legendary wizard archmage, wearing dark violet flowing robes with glowing cyan magic runes. He is standing symmetrically and holding a majestic ancient crystal staff with a rotating glowing ruby gem tightly in his right hand. Straight front-facing view, octane render, stylized game asset. Isolated on a solid flat pure white background (#ffffff), no ambient shadows on the wall, no floor shadows, no bases, no background smoke.
```

```text
// 7. Молодой Маг / Ученик (Simple Mage):
Symmetrical full-body 3D model of a cute young magic apprentice boy wearing an oversized wizard hat and a simple blue tunic. He is holding a short magic wand firmly in his right hand, standing in a straight front-facing view. Stylized cute toy aesthetic, soft plastic texture, game world asset. Isolated on a solid flat pure white background (#ffffff), no shadows on the floor, no background vignette, no pedestal.
```

---

## 🛠️ ЧАСТЬ 3: КАКИЕ СКРИПТЫ МЕНЯТЬ И КАК ВСЕ ЭТО ПЕРЕНОСИТЬ?

Ниже описана простая технология, как оживить игру вашими картинками и моделями из Leonardo/Hunyuan 3D. Вам нужно работать в **Unity Editor** и изменить/проверить ссылки в следующих скриптах на сцене.

### Раздел 3.1: Перенос 2D Портретов Героев (Для Диалогов и Интерфейса)
Когда вы сгенерировали красивые картинки в Leonardo, вы можете использовать их как 2D-портреты ваших персонажей:
1. **Импорт:** Сохраните картинки в формате PNG. Перетащите их в Unity в папку `Assets/Sprites/Portraits/` (или создайте её).
2. **Настройка Импорта в Unity (Критично!):**
   - Выделите все загруженные портреты в окне `Project`.
   - В окне **Inspector** измените параметр **Texture Type** на **`Sprite (2D and UI)`**.
   - Нажмите внизу кнопку **Apply** (Применить).
3. **Как назначить их в скрипты на сцене:**
   - Найдите в иерархии сцены объект **`DialogueManager`** (или компонент с классом `DialogueSystem_Manager`).
   - В инспекторе у него есть поля для спрайтов:
     - `Companion Portrait`: Перетащите туда портрет Аэлиссы.
     - `Warrior Portrait`, `Archer Portrait`, `Mage Portrait`: Сюда перетащите портреты классов, которые вы сгенерировали. Система автоматически подставит нужный портрет справа, когда игрок выберет класс!

---

### Раздел 3.2: Перенос 3D Моделей Войск и Замков (Из Hunyuan 3D и Blender)
Если вы прогнали свои чистые картинки через Hunyuan 3D (Hugging Face) и доработали в Blender (сгладили полигоны и сделали Pivot Point у ног по инструкции из `GUIDE_CASTLES_AND_3D_HEROES.md`), у вас есть файлы `.fbx` со встроенными текстурами.

1. **Импорт:** Перетащите `.fbx` модели в Unity в папку `Assets/Project_Models/Heroes/`.
2. **Настройка префабов юнитов/гарнизона:**
   - Найдите префабы солдат вашего гарнизона или замков на сцене.
   - Откройте префаб (двойной клик на него).
   - Удалите или скройте старый 3D-плейсхолдер (например, цветные кубики или сферы, обозначавшие солдат).
   - Перетащите вашу красивую 3D-модель из папки проектов внутрь префаба как дочерний объект.
   - Настройте её масштаб (Scale) и поворот (Rotation), чтобы он ровно стоял на земле.
   - Сохраните изменения префаба (**Save / Override**).

---

### Раздел 3.3: Ключевые Скрипты Unity, которые управляют этими процессами:
Вам не нужно писать код с нуля! Все системы уже запрограммированы и слинкованы, вам достаточно настраивать их параметры в Инспекторе:

#### 1. 🏰 `FateCastleManager.cs` (Скрипт управления замками на сцене)
- **Где найти:** Висит на объекте `CastleManager` или основном контроллере королевства.
- **За что отвечает:** Пошаговое начисление золота, стоимость шпионажа, найм войск и вызов визуала улучшения замка.
- **Что менять в инспекторе:** Настройте начальное золото (`Initial Gold Setting`), вероятности прокачки ИИ-врагов (`Manual Ai Upgrade Probability`), и размеры гарнизона замка.

#### 2. 📁 `SaveGameSystem.cs` (Система сохранений)
- **За что отвечает:** Загрузка и сохранение характеристик вашего персонажа, выбранного класса, текущего дня и баланса золота в 3 независимых слота.
- **Как переносить прогресс:** При запуске новой игры Character Panel считывает базовые атрибуты класса именно отсюда.

#### 3. 🎥 `StrategicCameraController.cs` (Умная камера континента)
- **Где найти:** Находится на объекте `Main Camera` или `Camera Rig`.
- **Что менять:** Включить или выключить Edge Scrolling (скролл мышкой по краям экрана), настроить минимальный и максимальный зум (`minZoom`, `maxZoom`).

#### 4. 🧭 `LandingPositionManager.cs` (Менеджер высадки)
- **Где найти:** Координирует 4 физические точки старта.
- **Что делать:** Перетащите ваши созданные на террейне якоря-пустышки (`Wastes_SpawnPoint`, `Peak_SpawnPoint` и т.д.) в соответствующие слоты этого скрипта, чтобы игрок спавнился строго на нужной суше.

---

### 💡 Лайфхак по быстрой сборке:
Вы можете открыть сцену, запустить симуляцию в Unity Editor, выбрать класс "Маг" с пулом очков (например, на Распределении Статов), нажать «Начать игру», увидеть диалоги, совершить высадку кликом на Неоновое Кольцо, и у вас запустится кампания с вашим замком! Все изменения ресурсов (замены спрайтов портретов в инспекторе и подмена 3D-моделей в префабах) отобразятся во всех этих сценах мгновенно.
