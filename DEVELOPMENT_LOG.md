# DEVELOPMENT LOG

## [2026-07-13]
- **Версия 18.11.24:** Multilingual Expansion & Disaster Recovery System - Upgraded all Castle Town sections (Barracks, Academy, training options, potion shops, item tooltips and buy logs) to support 9 languages (Russian, English, German, French, Spanish, Portuguese, Japanese, Korean, Simplified Chinese) with the `GetText9` helper function. Established a full disaster recovery guide (`AI_ASSISTANT_RECOVERY_GUIDE.md`) displaying structural layouts, core state patterns, and recovery maps.
- **Грозовые Кряжи (Stormy Crags) Landing Correction:** Fixed a critical bug in `LandingPositionManager.cs` where the player's 3D landing camera focused on Region 11 instead of Region 8 (Ancient Ruins / Грозовые Кряжи). Refactored `GetLandingAnchorPosition` and `GetActualRegionIndexFromLanding` to align physical anchor points and logical region index mappings.
- **Dynamic AI Castle Behavior:** Implemented difficulty-based behavioral strategies (Novice, Easy, Normal, Hard, Nightmare) for neutral, defense/allied (Green), and aggressive (Red) AI commanders. Integrated logic with dynamic army building, research, espionage, and player counter-measures across all regions.

## [2026-07-12]
- **Версия 18.11.23:** Potion Mechanics Rework & Interface Stabilization - Re-engineered all potion types (Vital Health, Giant Strength, Swift Agility, Mind Intelligence, Iron Stamina) to strictly grant a temporary, one-battle combat buff instead of active healing. Potion consumption and equipment slots are restricted purely to the main Player Hero and the rival AI commander. Restored and stabilized the previously corrupted custom castle calibration layout, DrawStatRow, and DrawNewDayOverlay functions in FateCastleManager.cs. Fully localized all custom town subpanels across Russian, English, Korean, and Chinese languages.

## [2026-07-11]
- **Версия 18.11.22:** Memory Optimization, Skip Day Lock & Hover Skills Detail - Optimizes RAM and VRAM memory usage by lazy-caching common GUIStyle fields in FateCastleManager.cs. Implements an interlocking mechanism that disables the "Skip Day" button when the Hero Management character screen is active. Converts active & passive skill cards to unclickable boxes that display interactive hover tooltips following the mouse cursor. Integrates horizontal scrolling for inventory tabs and compresses potion slot labels ("Зел. Жизни") to solve wrapping and clipping.

## [2026-07-10]
- **Версия 18.11.21:** Zenith Skill Detail Sync & Video Reference Update - Resolves the missing `ShowSkillDetailPopup` method compiler errors (CS0103) in `FateCastleManager.cs`. Standardizes active skill descriptions dynamically matching all three major player hero classes (Warrior, Archer, Mage) depending on their character class data loaded from `SaveGameSystem`. Integrates the newly requested YouTube video knowledge reference into the persistent knowledge indexes.

## [2026-07-09]
- **Версия 18.11.20:** Fullscreen Character Panel & Advanced Inventory Grid - Integrates a fullscreen 3-column Zenith Hero Control Panel in FateCastleManager.cs. Implements a local-persistent, secure 36-slot inventory grid supporting stacking items (potions) and gear. Formulates a dynamic 8-slot equipment mannequin with attributes calculations, larger passive/ultimate skill cards, and links potion merchants and forge slot selections directly to the player inventory database.

