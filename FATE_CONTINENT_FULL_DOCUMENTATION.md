# 📘 FATE CONTINENT: MASTER KNOWLEDGE BASE (v18.11.15)

> **TECHNICAL OVERVIEW:**
> - **Engine:** Unity 6 (6000.3.10f1)
> - **Genre:** Turn-based Strategy / RPG
> - **Style:** Zenith Glassmorphism (8K)
> - **Updates:** RPG Skills & Turn-Based Castle Integration (v18.11.15). Custom Starting Attributes & Allocation Barriers.

---

## 🏗️ 1. CORE ARCHITECTURE
Project uses a centralized management system:
- `GlobalSettingsManager`: Handles sound, music, quality (Ultra 8K), and localization.
- `Translator`: Supports 9 languages (RU, EN, DE, FR, ES, PT, JA, KO, ZH) with specific font handling.
- `DataLoader`: Loads `races_data.json` and `items_data.json` using `JsonUtility`.
- `GameManager`: Persists across scenes (`DontDestroyOnLoad`), manages gold and difficulty.

## 🧬 2. RPG CLASS & STARTING ATTRIBUTE MECHANICS (v18.11.15)
- **Class Baselining**: Supports three primary classes with distinct starting attributes configured on Slot-0 load inside `CharacterSelectionController.cs`:
  - **Warrior**: STR 15, AGI 10, INT 4, STA 15.
  - **Archer**: STR 10, AGI 14, INT 6, STA 11.
  - **Mage**: STR 6, AGI 10, INT 10, STA 9.
- **Difficulty Bonus Skills Pools**: Allocates bonus starter points based on selected level of difficulty on character creation:
  - Novice: +30 points
  - Easy: +20 points
  - Normal: +10 points
  - Hard: +5 points
  - Nightmare: +0 points
- **Clamped Manual Allocation (`FateCastleManager.cs`)**: Prevents reducing stats below their designated class baseline values using safety boundaries:
  - `statValue > minValue`
- **AI Autonomous Allocation**: Integrated `AutoAllocateAllPoints()` system distributes unused points according to weighted class specifications.

## ⚔️ 3. HERO CLASS SKILLS GLOSSARY (v18.11.15)
Displays and maintains unique character class passives and ultimate skills inside the Zenith HUD panel:
- **Warrior**:
  - *Passives*: IronSkin (+15% Armor Protection), Regen (+5 HP per turn), Threat (+10% threat aggro).
  - *Ultimate*: TitanShield (Cooldown 4 turns, blocks 70% of incoming damage).
- **Archer**:
  - *Passives*: Crit Master (+15% Critical Chance), LongShot (+10% ranged damage), Evasion (+10% dodge).
  - *Ultimate*: Death Rain (Cooldown 3 turns, x1.8 AoE field damage).
- **Mage**:
  - *Passives*: ManaFlow (+5 MP per turn), Elemental (+15% elemental magic damage), Resist (+15% spell defense).
  - *Ultimate*: Time Rift (Cooldown 4 turns, slows enemies down for 2 turns).

## 🗺️ 4. WORLD & NAVIGATION
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
