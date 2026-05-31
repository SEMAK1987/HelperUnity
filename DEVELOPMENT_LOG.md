# DEVELOPMENT LOG

## [2026-05-30]
- Версия 18.10.1: Zenith Map Master System + Dialogue UI Uplift.
  - Исправлены ошибки компиляции `fillShareWithParent`, добавлена прямая привязка якорей `RectTransform`.
  - Внедрены совместимые методы-псевдонимы `ToggleWorldMap` во `FateMapManager` и `PlaySound` в `SettingsManager`.
  - Оптимизирован запуск вводного диалога с Аэлиссой в `FateGameplayIntro_Manager` без секундных задержек пустого экрана.
  - Устранено занижение кнопок вариантов диалога — они приподняты (Y: 30f) для идеального отображения.
