# DEVELOPMENT LOG

## [2026-08-10]
- **Версия v18.12.11: Alchemist Cat UI & Localization Calibration**
  - Удалена авто-перепривязка выпадающих списков, которая при старте путала ссылки SetQuality и SetLanguage.
  - Добавлен надежный скоринговый алгоритм для автоопределения выпадающих списков языков, качества и разрешения.
  - В FormatText отключен разрыв слов (TextWrappingModes.NoWrap), а characterSpacing сброшен на 0, чтобы слова не переносились на новые строки.
  - В Transtable_Dropdown.cs добавлена проверка-фильтр `if (gameObject.name.ToLower().Contains("lang"))` для предотвращения перезаписи опций выпадающего списка Языков.
  - Методы OnStartPressed(), OnSettingsPressed(), OnExitPressed() и OnBackPressed() в Menu_Game.cs сделаны публичными (public) для легкой настройки событий в Unity Inspector.

## [2026-08-09]
- **Версия v18.12.10: Advanced Dropdown Calibration & Text Wrapping Fixes**
  - Стандартизировано форматирование выпадающих списков (TextWrappingModes.NoWrap).
  - Подавление предупреждений CS0618 в SettingsManager.cs путем миграции на современное свойство textWrappingMode.
  - Оптимизация пивотов и высот элементов для предотвращения обрезки интерфейса.
