# 🤖 Unity & Blender AI Assistant • Core Knowledge Base (v18.11.15)

## 📌 Project Identity
- **Name:** Fate Continent (Континент Судьбы)
- **Version:** 18.11.15
- **Engine:** Unity 6 (6000.3.10f1)
- **v18.11.15 Update:** Turn-Based Castle Income & Dynamic Visual Morphing - Replaces the real-time (per-second) passive gold accumulation with a fully turn-based "End Turn" (Пропустить ход) mechanical system, implementing a daily cycle counter with an integrated glassmorphic GUI panel (Day tracker & Cyan controller). Establishes shape-shifting C# procedural builders that physically transform Castle 3D models upon upgrading from Level 1 (single-tower sentry post) to Level 2 (majestic royal citadel featuring defensive outer ramparts, custom side wings, and a rotating gem spire with custom emission colors).
- **v18.11.14 Update:** Post-Landing Narrative & Castle Progression System - Implements scale and offset persistence for the tactical world map preventing reset on play. Adds a multi-phase post-landing narrative briefing starting at DialogStep 8 through 12, focusing the camera on the player castle, locking movement and pausing. Programmatically spawns four majestic 3D castles (emerald neon for player, ruby neon for enemy built using Standard/URP-compatible materials) and establishes the interactive Castle Management logic featuring Zenith Glassmorphism UI, passive gold income tick system, military recruitment, shop equipment and espionage.
- **v18.11.13 Update:** Ground-Focused Camera Clamping - Refactors camera coordinates clamping in StrategicCameraController.cs to mathematically limit the screen's visual center focal point projected on the ground (Y = 0) rather than restricting raw camera coordinates. This solves physical camera locks at high elevations and allows manual limits to perfectly match visual map coordinate dimensions.
- **v18.11.12 Update:** Dynamic Ocean Occlusion & Quality Synchronization - Integrates auto-active ocean plane hide on Start() and show on DispatchLanding() for dialogue sequence protection; fixes pink standard shader issues in Universal Render Pipeline projects by detecting `M_Ocean_Background` automatically or compiling URP-compatible lit fallbacks; and dynamically scales water glossiness and metallic parameters based on system graphics quality levels (Low, Med, Ultra) loaded from `PlayerPrefs`.
- **v18.11.11 Update:** Real-Time Bound Locking, Edge Scrolling & Ocean Planes - Integrates strict real-time coordinate constraints in StrategicCameraController.cs with a dynamic AutoFitBounds() system that automatically calculates bounds according to New_Kontinent's mesh, implements mouse-steerable Edge Scrolling, and instantiates an automatic Ocean background Plane with 40x40 UV tiling ready for seamless 8K high-res textures.
- **v18.11.10 Update:** 4-Zone Spawn Match & Map Sync - Matches indices and names of zones ("Кровавые Пустоши" -> Oasis_SpawnPoint, "Ледяной Пик" -> Outpost_SpawnPoint, "Древние Руины" -> Shore_SpawnPoint, "Святилище Зенита" -> Citadel_SpawnPoint) in LandingPositionManager.cs to match interactive rings and user's customized map catalog. Fixes clipping/clipping depth by placing Ring interactive markers local Z coordinate at -2.0f and companions/heroes at -2.05f to bypass overlapping from any 3D continent textures.
- **v18.11.9 Update:** Input System Auto-Switch & Camera Rig Calibration - Solves New Input System 999+ Exception errors in StrategicCameraController.cs using smart preprocessor compilation. Calibrates landing point cameraOffset defaults from obsolete 15f height to ideal 2.5f height with auto-correction on startup, and sets crisp strategic min/max zoom limits (0.6f / 8.0f) for full continent overview.
- **v18.11.8 Update:** 4-Zone Landing Position Auto-Sync & Anchor Persistence - Binds four distinct landing points and automatically synchronizes physical spawn anchors (`Wastes_SpawnPoint`, `Peak_SpawnPoint`, `Ruins_SpawnPoint`, `Crags_SpawnPoint`), securely caching player selection across sessions with persistent PlayerPrefs. Fully masks 3D environments during dialog panels to secure scene purity.
- **v18.11.7 Update:** Selective Dialogue Map Dismissal & Hidden Faction Markers - Dismisses map background, landing rings, and companion/hero markers upon clicking 'End Dialogue' or ending conversation. Hides non-glowing faction reference circles (`Faction_Marker_Aelyssa` / class markers) inside the tactical view to keep the landing layout extremely pristine.
- **Design System:** Zenith Glassmorphism (8K Ultra-High Density)

## 📑 Core Documentation References
1. `FATE_CONTINENT_FULL_DOCUMENTATION.md` - Complete technical manual.
2. `PROJECT_MASTER_BLUEPRINT.md` - Hotfixes, translation IDs, 3D prompts, RPG Saves System, Game Audio Prompts & AudioMixer Routing.
3. `DEVELOPMENT_LOG.md` - Daily progress history.

## 🛠️ Technical Constraints & Rules
- **Shader Rule:** Always use `TextMeshPro/Distance Field`. Enable **Bloom**.
- **Access Rule:** Recommend GoodbyeDPI/WARP for heavy 3D assets.
- **CJK Font Rule:** 
  1. В `Translator.cs` ОБЯЗАТЕЛЬНО должны быть заполнены ТРИ слота: `Default`, `Chinese` и `Korean`.
  2. `SimHei` НЕ содержит корейских букв — используйте `Malgun Gothic` или `Noto Sans KR` для Кореи.
  3. В `LiberationSans SDF` добавьте оба азиатских шрифта в `Fallback Font Assets`.
- **Input Rule:** Use `Input System Package (New)`.
- **Localization:** Automatic sync through `Translator.cs` and `Transtable_Dropdown.cs`.
- **RPG Save Mechanism:** `SaveGameSystem.cs` manages loading and writing player progress using 3 discrete PlayerPrefs-backed slots, formatting descriptions with language-adaptive prefixes (e.g. Ур. / Lvl / 레벨). Trigger-based saving allows seamless saving on world portals or check-points.
- **Audio Rule (CRITICAL):** Используйте только `SettingsManager.cs` для управления звуком и музыкой (включая hover-эффекты и списки воспроизведения). Скрипты `AudioHandler` и `AudioManager` должны быть полностью удалены для чистоты проекта. Музыкальные клипы продлеваются через Suno/Udio и настраиваются на бесшовное зацикливание (`loop = true`). Звуковые файлы берем с Pixabay/Freesound по CC0-лицензии и подключаем напрямую к SettingsManager или через AudioMixer.

## 🚀 Протокол «Пошаговое Мастерство» (Step-by-Step Mastery)
1. **Отслеживание прогресса:** Текущая версия v18.11.13 (Ground-Focused Camera Clamping, v18.11.13).
2. **Инструкции Настройки Компонентов (Verified Inspector Settings):**
   - **FateMapManager (FATE_WORLD_MAP):**
     - *Maps List:* [+] Слот для глобальной карты (Map Name = "Континент Судьбы", Map Background = фоновый спрайт карты).
     - *Rings (Кольца-Маркеры):* [+] Интерактивные точки миров (Кровавые Пустоши, Ледяной Пик, Ржавые Окраины / Древние Руины).
       - *Setup:* Ring Name, Ring Description, Ring Sprite (круглая неоновая текстура), Local Position (X,Y), Associated Dialogue Index (например, 3), Click/Hover Sfx (UI_Click_Metallic / UI_Hover_Soft), Normal/Hover Glow Color (выбирать HDR неоновые оттенки).
       - *Default Glow Material:* `M_Neon_Glow`.
       - *Show Map On Start:* False (карта скрыта до окончания интро диалогов).
   - **DialogueSystem_Manager (DialogueManager):**
     - *Companion Names:* Аэлисса (RU) и Aelyssa (EN).
     - *Portraits (Sprites):* Companion Portrait (левая сторона: `Pomoshnica`), Warrior/Archer/Mage Portraits (правая сторона: `Voin`, `Strelok`, `Mag` - устанавливаются динамически на основе выбранного класса из сохранений SaveGameSystem).
     - *Companion Voice:* `magic chime` (звуковой клип тихой эльфийской речи на фоне реплики).
     - *Enforce Coordinates:* True (принудительное выравнивание координат плашек).
       - *Positions & Offsets:* Настройка сдвигов имен, полей текста и кнопок диалога (Choice Container Y=30f для удержания кнопок в границах видимости).
       - *Dialogue Steps:* Оставьте список пустым, чтобы система загрузила заложенный сценарий завязки игры на 4 языках.
3. **Чек-листы:** Инструкции по GLB-скачиванию, TMP-восстановлению, настройке триггеров сохранения, продлению треков в Suno / Udio (Extend, Get Whole Song), бесшовным музыкальным петлям, навигации по Pixabay, СС0 фильтрации, поисковым промптам, UIButtonSfxBinder автоматизации и маршрутизации звука через AudioMixer в Unity.
4. **Обработка ошибок:** Исправлены проблемы CS1061, CS0155 (catch exceptions type, New Input System), дублирования разрешений, сброса шрифтов, некорректной локализации, некорректной фильтрации, некорректного применения аудио-эффектов, устаревших аудио-провайдеров и предупреждений "Exposed name does not exist" в микшере, а также добавлена работа звука без привязки к сцене в редакторе.
5. **ОЧИСТКА:** Используйте только `SettingsManager.cs` для аудио. Удалите `AudioHandler` и `AudioManager` скрипты. Не использовать ElevenLabs.

---
*Ядро AI пересинхронизировано (v18.11.9). Авто-апдейтер активен.*
