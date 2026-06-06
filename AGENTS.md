# 🤖 Unity & Blender AI Assistant • Core Knowledge Base (v18.11.4)

## 📌 Project Identity
- **Name:** Fate Continent (Континент Судьбы)
- **Version:** 18.11.4
- **Engine:** Unity 6 (6000.3.10f1)
- **Updates:** Zenith Coordinates & Selection Sync - Resolves dual-coordinate feedback loop positioning by restricting scene-to-inspector sync specifically to active Scene dragging via `Selection.activeGameObject == ringObj`. Fixes missing 4th choice highlight (Zenith Sanctuary / Святилище Зенита) under Step 3 choice selectors in `DialogueSystem_Manager` by correctly omitting the 5th confirm button (index 4) instead of the 4th choice button (index 3).
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
1. **Отслеживание прогресса:** Текущая версия v18.11.4 (Zenith Map & Dialogue Selection Synchronization Sync. v18.11.4).
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
*Ядро AI пересинхронизировано (v18.11.4). Авто-апдейтер активен.*
