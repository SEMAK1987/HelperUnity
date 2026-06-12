# Development Log - Fate Continent (Континент Судьбы)

## [v18.11.10] - 2026-06-12
- **4-Zone Spawn Match & Map Sync:** Checked, synchronized, and locked the indices and names of zones to physical spawn anchors inside `LandingPositionManager.cs`.
- **Render Depth Fix:** Shifted Ring interactive markers local Z render coordinate from `-1.0f` to `-2.0f` and companion/hero markers to `-2.05f`. This solves the issue of the selected "Кровавые Пустоши" (Crimson Wastes) interactive circle getting covered/clipped by the 3D map territory mesh texture at that point.
- **Master Knowledge Alignment:** Fully updated `knowledge_base.json`, `metadata.json`, `PROJECT_MASTER_BLUEPRINT.md`, `DEVELOPMENT_LOG.md`, `AGENTS.md`, `package.json` with the new version and features specification.

## [v18.11.9] - 2026-06-11
- **Camera Calibration:** Fixed camera offset landing positions inside `StrategicCameraController.cs` using preprocessor directives and set ideal strategic min/max limits (0.6f / 8.0f) with height set to 2.5f default.
