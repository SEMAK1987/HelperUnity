# DEVELOPMENT LOG

## [v18.12.16] - 2026-08-21
- **Alchemist Cat Calendar Auto-Open & RectOffset Exception Prevention**:
  - Replaced `new RectOffset()` field initializers in `MonthLayoutConfig` (`Calendar_Manager.cs`) with serializable integers (`padLeft`, `padRight`, `padTop`, `padBottom`) to fix 36 `UnityException: set_left can only be called from the main thread` errors.
  - Enhanced `DialogueSystem_Manager.OpenCalendarUI()` to support direct inspector slots (`calendarPanel`, `calendarManager`), search inactive hierarchy objects via `FindAnyObjectByType<Calendar_Manager>(FindObjectsInactive.Include)` and search `"Calendar_Panel"` by name.
  - Added auto-generation check in `Calendar_Manager.OpenCalendar()` to ensure the 12 seasonal months generate immediately if opened while initially inactive at scene start.

## [v18.12.15] - 2026-08-21
- **Alchemist Cat Recipe Scroll & Missed Flask Badges**:
  - Added `missedFlaskSprite` slot to `Calendar_Manager.cs` to render cracked potion flasks on missed days.
  - Built `Recipe_Scroll_Panel` integration in `DialogueSystem_Manager.cs` to display the big parchment recipe scroll after closing the calendar.

## [v18.12.14] - 2026-08-21
- **Alchemist Cat Extended Dialogue & Daily/Monthly/Quarterly/Annual Calendar Reward System**:
  - Extended narrative dialogue in `DialogueSystem_Manager.cs`.
  - Added daily, monthly, quarterly, and annual streak reward progression logic.
