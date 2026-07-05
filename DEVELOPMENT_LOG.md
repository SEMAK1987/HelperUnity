# DEVELOPMENT LOG

## [2026-07-04]
### v18.11.24 - Multilingual Expansion & Disaster Recovery System
- **Multilingual Support for Castle Town:** Expanded all castle subpanels, academy training controls, potion shop inventories, and dynamic tooltips to support 9 languages (Russian, English, German, French, Spanish, Portuguese, Japanese, Korean, Simplified Chinese) using `GetText9()`.
- **System Disaster Recovery Guide:** Generated a comprehensive disaster recovery manual (`AI_ASSISTANT_RECOVERY_GUIDE.md`) including mockups, core state patterns, and reconstruction guides.
- **Synchronized Connectors:** Synced version labels across `server.ts`, `UnityConnector.cs`, `blender_connector.py`, `package.json`, `metadata.json`, and `knowledge_base.json` to v18.11.24.

## [2026-07-02]
### v18.11.23 - Potion Mechanics Rework & Interface Stabilization
- **Potion Buffs Rework:** Changed potion mechanics so that potions now grant temporary, single-battle attribute buffs instead of instant healing.
- **Castle Calibration & Overlays:** Restored custom calibration layout, `DrawStatRow`, and `DrawNewDayOverlay` within `FateCastleManager.cs`.

## [2026-06-28]
### v18.11.22 - Memory Optimization & Hover Tooltips
- **RAM/VRAM Cache:** Lazy-cached GUIStyle allocations to stop reallocation in IMGUI `OnGUI` loop.
- **Skip Day Interlocking:** Interlocked and locked the "Skip Day" button when the Hero Management character panel is active.
- **Tooltip Hover Details:** Converted skills cards to unclickable rectangles displaying dynamic mouse-hover tooltips.

## [2026-06-25]
### v18.11.21 - Zenith Skill Detail Sync
- **ShowSkillDetailPopup Integration:** Implemented missing skill detail popup logic dynamically matching character classes.

## [2026-06-20]
### v18.11.20 - Fullscreen Character Panel & Advanced Inventory Grid
- **Hero Panel Expansion:** Created a fullscreen 3-column Zenith Hero Control Panel inside the castle interface.
- **Persistent Grid:** Implemented 36-slot local-persistent inventory grid with potion stacking and 8 equipment mannequin slots.

## [2026-05-14]
### v18.5.8 - Zenith Multi-Tool Synergy & Settings Fix
- Initial stable release.
