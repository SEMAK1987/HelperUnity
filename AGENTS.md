# 🤖 Unity & Blender AI Assistant • Core Knowledge Base (v18.7.7)

## 📌 Project Identity
- **Name:** Fate Continent (Континент Судьбы)
- **Version:** 18.7.7
- **Engine:** Unity 6 (6000.3.10f1)
- **Updates:** Zenith Canvas Lifecycle Mastery - Resolves active UI panels duplicating or failing back-to-menu navigation states on reload through automatic reference recycling and event binding resets in Menu_Game. (v18.7.7). Core RPG Saves & Sound Routing.
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
1. **Отслеживание прогресса:** Текущая версия v18.7.7 (Zenith Canvas Lifecycle Mastery - Resolves active UI panels duplicating or failing back-to-menu navigation states on reload through automatic reference recycling and event binding resets in Menu_Game. v18.7.7).
2. **Чек-листы:** Инструкции по GLB-скачиванию, TMP-восстановлению, настройке триггеров сохранения, продлению треков в Suno / Udio (Extend, Get Whole Song), бесшовным музыкальным петлям, навигации по Pixabay, СС0 фильтрации, поисковым промптам, UIButtonSfxBinder автоматизации и маршрутизации звука через AudioMixer в Unity.
3. **Обработка ошибок:** Исправлены проблемы CS1061, CS0155 (catch exceptions type, New Input System), дублирования разрешений, сброса шрифтов, некорректной локализации, некорректной фильтрации, некорректного применения аудио-эффектов, устаревших аудио-провайдеров и предупреждений "Exposed name does not exist" в микшере, а также добавлена работа звука без привязки к сцене в редакторе.
4. **ОЧИСТКА:** Используйте только `SettingsManager.cs` для аудио. Удалите `AudioHandler` и `AudioManager` скрипты. Не использовать ElevenLabs.

---
*Ядро AI пересинхронизировано (v18.7.7). Авто-апдейтер активен.*
