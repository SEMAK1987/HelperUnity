# DEVELOPMENT LOG

## [2026-06-03]
- **v18.11.1**: Ultimate Independent Map & Ring Scaling and Positioning Sync. Separated ring scaling completely from parent Map scale compensation inside both `FateMapManager.cs` and `FactionMapMarker.cs`. Recalculates `baseScale` and `targetScale` dynamically every frame at runtime to facilitate real-time inspector updates.

## [2026-06-02]
- **v18.11.0**: Zenith Map & Dialogue Blueprint Sync. Integrates precise Inspector guidelines into the project core database. Synchronizes FateMapManager (glowing landmarks, customized hover/click soundscapes, and auto-HDR neon calibration mapped to bloom material M_Neon_Glow) and DialogueSystem_Manager (dual local companion voices, case-insensitive class portraits, and anchored coords alignments).
- **v18.10.0**: Zenith Map Master System. Interactive Continents & Auto-HDR Neon Calibration. Custom interactive map markers with auto-calibrating HDR neon colors, Bloom-glowing ring feedback, click-sound bindings, and multi-option dial branching.
- **v18.9.0**: Zenith Dialogue Master System. Modular DialogueSystem_Manager.cs to orchestrate custom dialog systems using a dual-avatar layout (Aelyssa on left, class-specific player hero on right) and multi-option pointer branching options. Syncs with Translator.cs.
- **v18.8.0**: Zenith Audio Autonomy & Standalone Routing. Isolated UIButtonPauseHover audio execution by targeting stable active Scene hosts (such as GamePause_Manager or SettingsManager singletons). Excludes pause scenes from UIButtonSfxBinder automatic scan.
- **v18.7.9**: Zenith Self-Healing UI. Automatically heals and reassigns misconfigured TextMeshProUGUI references at scene load.

## [2026-05-14]
- **v18.5.8**: Zenith Multi-Tool Synergy & Settings Fix.
