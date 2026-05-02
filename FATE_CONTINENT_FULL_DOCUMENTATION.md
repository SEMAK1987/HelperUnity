# 📘 FATE CONTINENT: MASTER KNOWLEDGE BASE (v3.0)

> **TECHNICAL OVERVIEW:**
> - **Engine:** Unity 6000.3.10f1
> - **Genre:** Turn-based Strategy / RPG
> - **Style:** Zenith Glassmorphism (8K)
> - **Platform:** PC Standalone (x86_64)

---

## 🏗️ 1. CORE ARCHITECTURE
Project uses a centralized management system:
- `GlobalSettingsManager`: Handles sound, music, quality (Ultra 8K), and localization.
- `Translator`: Supports 8 languages (RU, EN, DE, FR, ES, JA, KO, ZH) with specific font handling for RU/Asia.
- `DataLoader`: Loads `races_data.json` and `items_data.json` using `JsonUtility`.
- `GameManager`: Persists across scenes (`DontDestroyOnLoad`), manages gold and difficulty.

## ⚔️ 2. BATTLE SYSTEM & FORMULAS
- **CombatResolver**: Static class for calculating damage, crits, and dodge.
- **Formulas**: Uses `defCurve = 50f` for smooth damage reduction.
- **SkillDB**: Static cache for skills to avoid `Find()` overhead.
- **AI Logic**: Behavior-based on HP thresholds (<50% triggers healing) and nearest target searching.

## 🗺️ 3. WORLD & NAVIGATION
- **GridSystem**: Grid-based map (default 60x60) with terrain types (Plains, Forest, Mountains, Water).
- **Pathfinding**: A* (A-Star) algorithm for unit movement, considering movement costs for "Heavy" units.
- **CameraController**: Smooth 2.5D camera with WASD/Mouse support and `Mathf.SmoothStep`.

## 🎒 4. INVENTORY & EQUIPMENT
- **Drag & Drop**: UI-based system with `DraggableItemUI` and `EquipmentDropTarget`.
- **HeroDoll**: 6 slots (Helmet, Armor, Weapon, Shield, 2 Accessories).
- **Buffs/Potions**: Real-time tick system for status effects (str, def, luck, spd).

## 🛡️ 5. SECURITY & PROTECTION
- **Anti-Cheat**: 
  - `TimeValidator` for speedhack detection.
  - `ProcessMonitor` with blacklist (CheatEngine, etc.).
  - `IntegrityValidator` using HMAC-SHA256 for save file verification.
- **Value Clamping**: All critical values (Gold, HP) use property wrappers to prevent memory editing.

## 🚀 6. FUTURE SCALING (APPENDIX)
1. **Multiplayer**: Integration via Photon Fusion or Unity Netcode.
2. **Modding**: StreamingAssets-based loading for JSON mods.
3. **Cloud Saves**: PlayFab / Steam Cloud integration.
4. **DLC**: Addressables system for on-demand content loading.

---
*Information ingested from PDF Technical Manual v3.0*
