# 🤖 Unity & Blender AI Assistant • Core Knowledge Base (v18.5.8)

## 📌 Project Identity
- **Name:** Fate Continent (Континент Судьбы)
- **Version:** 18.5.8
- **Engine:** Unity 6 (6000.3.10f1)
- **Updates:** Zenith Multi-Tool Synergy (v18.5.8). Settings & Localization Fix.
- **Design System:** Zenith Glassmorphism (8K Ultra-High Density)

## 📑 Core Documentation References
1. `FATE_CONTINENT_FULL_DOCUMENTATION.md` - Complete technical manual.
2. `PROJECT_MASTER_BLUEPRINT.md` - Hotfixes, translation IDs, and HDR recipes.
3. `DEVELOPMENT_LOG.md` - Daily progress history.

## 🛠️ Technical Constraints & Rules
- **Shader Rule:** Always use `TextMeshPro/Distance Field`. Enable **Bloom**.
- **CJK Font Rule:** 
  1. В `Translator.cs` ОБЯЗАТЕЛЬНО должны быть заполнены ТРИ слота: `Default`, `Chinese` и `Korean`.
  2. `SimHei` НЕ содержит корейских букв — используйте `Malgun Gothic` или `Noto Sans KR` для Кореи.
  3. В `LiberationSans SDF` добавьте оба азиатских шрифта в `Fallback Font Assets`.
- **Input Rule:** Use `Input System Package (New)`.
- **Localization:** Automatic sync through `Translator.cs` and `Transtable_Dropdown.cs`.

## 🚀 Протокол «Пошаговое Мастерство» (Step-by-Step Mastery)
1. **Отслеживание прогресса:** Текущая версия v18.5.8 (Settings & Localization stable).
2. **Чек-листы:** Инструкции по Multi-Atlas и Fallback шрифтам.
3. **Обработка ошибок:** Исправлены ошибки сборки Translator (UIText ref).
4. **ОЧИСТКА:** Удалите `Translator.cs`, `Transtable_Dropdown.cs` и `Transtable_Text.cs` из `Assets/`. Используйте только версию в `Assets/src/`.

---
*Ядро AI пересинхронизировано (v18.5.8). Авто-апдейтер активен.*
