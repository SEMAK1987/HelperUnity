# DEVELOPMENT LOG

## [2026-06-06]
- **v18.11.7**: Selective Dialog Ending Map Dismissal & Hidden Faction Markers
  - Solves critical issue where the map background and its rings stayed displayed after clicking 'End Dialogue' / 'Завершить диалог'.
  - Automatically hides active map markers, glowing rings, and background on non-interactive dialog steps.
  - Fully removes flat white circular faction sprites (`Faction_Marker_Aelyssa`/class markers) from the tactical world map view and during the interactive landing phase to eliminate visual clutter.

## [2026-06-05]
- **v18.11.6**: Synchronized Map & Marker Dismissal
  - Automatically caches `Faction_Marker_Aelyssa` and player class markers on startup to control their visibility in sync with the map.
  - Controls map visibility dynamically to clean up viewports when dialogue ends.
- **v18.11.5**: Single Ring Visibility & Pure Coordinates Preservation
  - Dynamically disables non-chosen landing rings at runtime.
  - Establishes and preserves exact manually fine-tuned layout coordinates as hardcoded default variable parameters in C#.

## [2026-05-14]
- **v18.5.8**: Zenith Multi-Tool Synergy & Settings Fix.
