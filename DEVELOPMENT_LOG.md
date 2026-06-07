# Development Log • Fate Continent (Континент Судьбы)

## [v18.11.8] - 2026-06-07 (Interactive Spawn Auto-Sync & Anchor Persistence)
### Added
- **4-Zone Deployment Setup:** Standardized and initialized player landing vectors into four explicit regions: Wastes (Кровавые Пустоши), Peak (Ледяной Пик), Ruins (Древние Руины / Леса), and Sanctuary (Святилище Зенита).
- **Physical Spawn Anchor Auto-Sync:** Integrated automated hierarchy searching inside `LandingPositionManager.cs` to dynamically locate and link `"Wastes_SpawnPoint"`, `"Peak_SpawnPoint"`, `"Ruins_SpawnPoint"`, and `"Crags_SpawnPoint"` GameObjects at scene start, bypassing manual Inspector assignment.
- **Pristine Dialogue Purity:** Implemented structural masking that deactivates the 3D continent map assets and character figures during introductory dialogue panels, activating them upon landing selection.
- **Persistent Progress Saving:** Unified PlayerPrefs destination caching with `DialogueSystem_Manager.cs` and `SaveGameSystem.cs` to preserve selections across game reboots.

## [v18.11.7] - 2026-06-06 (Selective Dialogue Map Dismissal)
### Fixed
- Directed dialogue exit routines to dismiss visual maps, neon rings, and companion elements upon clicking "Конец диалога".
- Hidden non-glowing faction reference circles from the tactical view layout.

## [v18.11.0] - 2026-06-05 (Zenith Map & Dialogue Core Integration)
### Added
- Constructed modular dialogue system UI centering left-right portrait layouts.
- Programmed neon HDR auto-calibration in `FactionMapMarker.cs`.
- Styled user selection buttons with customizable choices.
