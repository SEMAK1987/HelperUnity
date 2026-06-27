# DEVELOPMENT LOG • Fate Continent (Континент Судьбы)

## [v18.11.22] - 2026-06-26
### Added
- **Horizontal ScrollView for Inventory Tabs**: Wraps the pagination bar (unlocked/locked tabs 1-28) in a horizontal scroll panel (`GUILayout.BeginScrollView`), resolving horizontal squishing and clipping on smaller screens.
- **Dynamic Skill Hover Tooltip**: Adds an interactive hover detection system in `DrawStatsAllocationPanel` that displays complete descriptions, icons, and skill types (Passive/Ultimate) following the mouse cursor.
- **Memory & VRAM Optimization**: Implements class-level style caching (`GUIStyle` fields) for all primary rendering structures in `FateCastleManager.cs`, replacing dynamic `new GUIStyle` allocations inside `OnGUI` frame loops to eliminate GC allocations and system lag.

### Changed
- **Unclickable Skill Cards**: Replaces clickable skill button triggers with unclickable `GUILayout.Box` texture displays to prevent accidental modal locks during skill inspections.
- **Shortened Consumable Naming**: Compresses long potion strings inside inventory buttons (e.g., `Зелье Жизни` -> `Зел. Жизни`) and scales down slot grid font size to 8px to guarantee that the letter 'З' is fully visible and not cut off by text wrapping.
- **Skip Turn Button Interlocking**: Disables and hides the top-right "Skip Day" button when the Hero Management character screen (`showStatsPanel`) is active to avoid skipping days concurrently with character editing.

## [v18.11.21] - 2026-06-25
### Changed
- **Zenith Skill Detail Sync & Video Reference Update**: Resolves the missing `ShowSkillDetailPopup` method compiler errors (CS0103) in `FateCastleManager.cs`.
- **Dynamic Character Skills Mapping**: Matches Warrior, Archer, and Mage character class data dynamically with skill definitions loaded from `SaveGameSystem`.
- **AI Knowledge Index**: Registers external video reference link `https://www.youtube.com/watch?v=NpfgeQZKmcU` in `knowledge_base.json`.
