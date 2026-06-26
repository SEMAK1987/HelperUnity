# DEVELOPMENT LOG

## [2026-06-26]
- **Версия 18.11.21:** Zenith Skill Detail Sync & Video Reference Update.
  - Решена ошибка компиляции CS0103 из-за отсутствия `ShowSkillDetailPopup` метода в `FateCastleManager.cs`.
  - Реализован метод `ShowSkillDetailPopup` с поддержкой динамических описаний и кастомных иконок для классов Воина, Лучника и Мага на русском и английском языках.
  - База знаний ИИ-ассистента (`knowledge_base.json`, `PROJECT_MASTER_BLUEPRINT.md`) обновлена с добавлением ссылки на новое обучающее видео: `https://www.youtube.com/watch?v=NpfgeQZKmcU`.
  - Синхронизированы мета-версии во всех служебных скриптах, включая `App.tsx`, `server.ts`, `UnityConnector.cs`, `blender_connector.py`, `version.json`, `metadata.json` и `package.json`.

## [2026-06-25]
- **Версия 18.11.20:** Fullscreen Character Panel & Advanced Inventory Grid.
  - Реализован полноэкранный 3-колонный интерфейс управления персонажем Zenith Hero Control Panel в `FateCastleManager.cs`.
  - Интегрирована локально-персистентная сетка инвентаря на 36 слотов с поддержкой складывания расходников (зелий) и экипировки.
  - Разработан манекен снаряжения на 8 слотов с динамическим расчетом бонусов характеристик (+STR, +AGI, +INT, +STA).

## [2026-06-24]
- **Версия 18.11.19:** Dynamic Dialogue Choice Positioning & Clean High-Density Layout.
  - Настроено адаптивное позиционирование кнопок диалога (Y=-20f, sizeDelta Y=44f) ниже панели, чтобы избежать перекрытия с портретами персонажей.

## [2026-06-22]
- **Версия 18.11.18:** Dynamic Army Units, Character Prompts Book Integration & High-Density UI.
  - Реализованы 14 уникальных типов армейских когорт со слотами для кастомных текстур в инспекторе.

## [2026-06-20]
- **Версия 18.11.16:** GPU Anti-Overheat Protection & Resolution Universal Sync.
  - Добавлено принудительное ограничение частоты кадров (Target Frame Rate 30/60/120 FPS) для оптимизации нагрузки на GPU.

## [2026-06-18]
- **Версия 18.11.15:** RPG Skills & Turn-Based Castle.
  - Внедрены стартовые пулы характеристик персонажей, глоссарий навыков и пошаговая генерация золота в замке.

## [2026-06-15]
- **Версия 18.11.14:** Post-Landing Narrative & Castle Progression System.
  - Написаны кат-сцены высадки на континент и заложена основа для постройки 3D неоновых замков.
