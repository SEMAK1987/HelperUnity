# 📘 FATE CONTINENT: MASTER KNOWLEDGE BASE (v18.5.5)

> **TECHNICAL OVERVIEW:**
> - **Engine:** Unity 6 (6000.3.10f1)
> - **Genre:** Turn-based Strategy / RPG
> - **Style:** Zenith Glassmorphism (8K)
> - **Updates:** Triple Font Bridge (v18.5.5). Korean Hangul Support. Compiler Fix.

---

## 🏗️ 1. CORE ARCHITECTURE
Project uses a centralized management system:
- `GlobalSettingsManager`: Handles sound, music, quality (Ultra 8K), and localization.
- `Translator`: Supports 9 languages (RU, EN, DE, FR, ES, PT, JA, KO, ZH) with specific font handling.
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

## 🈳 6. TRIPLE FONT BRIDGE (v18.5.5)
- **Standard**: LiberationSans SDF.
- **Chinese/Japanese**: SimHei Legacy CJK.
- **Korean**: Malgun Gothic / Noto Sans KR.
- **Logic**: Automatic fallbacks and singleton-safe static wrappers in `Translator.cs`.

## 📘 15. ОБРАЗОВАТЕЛЬНЫЙ ХАБ (v18.5.5 Sync)
- [Unity 6 Physics Mastery](https://www.youtube.com/watch?v=9vuyis_Y-LY)
- [Blender Advanced Rigging](https://www.youtube.com/watch?v=UKZp67dY1_w)
- [Shader Graph Advanced](https://www.youtube.com/watch?v=-hvxjyzcSkI)
- [Geometry Nodes Pro](https://www.youtube.com/watch?v=4YEB_Q8EOD8)
- [AI & ML-Agents](https://www.youtube.com/watch?v=JBszeE_NgmA)

---
*Документация актуализирована (v18.5.5)*
