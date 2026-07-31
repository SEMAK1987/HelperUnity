# DEVELOPMENT LOG

## [2026-07-31]
### Версия 18.12.07: Core Knowledge Base & YouTube References Integration
- **Интеграция видео-уроков:** Добавлены три новые ссылки на высококачественные руководства по Mixamo и Unity в базу знаний (`knowledge_base.json`, `PROJECT_MASTER_BLUEPRINT.md`).
- **Синхронизация метаданных:** Полностью обновлены версии проекта во всех системных файлах, включая `package.json`, `version.json`, `metadata.json`, `server.ts`, `src/App.tsx`, `UnityConnector.cs` и `blender_connector.py`.
- **Обновление документации:** Версия обновлена в `AGENTS.md` и `AI_ASSISTANT_RECOVERY_GUIDE.md` для сохранения полной консистентности.

## [2026-07-22]
### Версия 18.12.06: Battle Grid Unification & Automatic Zero Baked Colors System
- **Очистка сетки боя:** Реализована настройка `unifyGridMaterials` и автоматическое извлечение нейтрально-серого материала в `TacticalBattleGrid.cs` для удаления запеченных цветов.
- **Интерактивные зоны:** Внедрен метод `ToggleDeploymentZones` для скрытия стартовых зон при начале боя с сохранением золотой подсветки.

## [2026-05-14]
### Версия 18.5.8: Zenith Multi-Tool Synergy & Settings Fix
- **Запуск проекта:** Первая стабильная интеграция Unity и Blender коннекторов.
