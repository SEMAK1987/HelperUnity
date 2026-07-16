# DEVELOPMENT LOG

## [2026-07-16]
- **Версия 18.11.26**: Tactical Spy Report High-Density Grids. Re-engineered `DrawSpyReportPopup` to utilize high-density visual grids of squares for equipment slots, inventory items, and garrison cohorts. Enabled single-scout persistence that properly replaces older scout records with the current scouted castle's info while keeping scouted castles intact.
- **Версия 18.11.25**: Tactical Landing Regions Level Synchronization. Configured start levels for all regional castles across all 4 drop zones (Crimson Wastes, Ice-Bound Peak, Ancient Ruins, Storm Ridges) inside `FateCastleManager.cs` dynamically to align difficulty balance.
- **Версия 18.11.24**: Multilingual Expansion & Disaster Recovery System. Upgraded dynamic panels to support 9 languages via `GetText9` translation helper and established a disaster recovery blueprint.
