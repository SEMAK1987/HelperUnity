# 🤖 Unity & Blender AI Assistant • Core Knowledge Base (v18.6.6)

## 📌 Project Identity
- **Name:** Fate Continent (Континент Судьбы)
- **Version:** 18.6.6
- **Engine:** Unity 6 (6000.3.10f1)
- **Updates:** Zenith Multi-Tool Synergy (v18.6.6). Stable Character Sync & UI Hotfixes.
- **Design System:** Zenith Glassmorphism (8K Ultra-High Density)

## 📑 Core Documentation References
1. `FATE_CONTINENT_FULL_DOCUMENTATION.md` - Complete technical manual.
2. `PROJECT_MASTER_BLUEPRINT.md` - Hotfixes, translation IDs, 3D prompts.
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

## 🚀 Протокол «Пошаговое Мастерство» (Step-by-Step Mastery)
1. **Отслеживание прогресса:** Текущая версия v18.6.6 (Character & UI Active).
2. **Чек-листы:** Инструкции по GLB-скачиванию и TMP-восстановлению.
3. **Обработка ошибок:** Исправлены проблемы загрузки Meshy и Missing Components (v18.6.6).
4. **ОЧИСТКА:** Используйте только `SettingsManager.cs` для аудио. Удалите `AudioHandler` и `AudioManager` скрипты.

---
*Ядро AI пересинхронизировано (v18.6.5). Авто-апдейтер активен.*
