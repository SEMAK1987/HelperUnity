# Continent of Fate: Implementation Guide v17.18.1

This master guide provides step-by-step instructions for integrating the complex modules of the "Continent of Fate" project using Unity and Blender.

---

## 🛠️ Active Module Details

### 1. Global Event Director (Unity)
- **File:** `UnitySystems.cs`
- **Function:** Handles world-wide events like **Eclipse**, **Ether Tide**, and **Blood Moon**.
- **Usage:** Call `GetComponent<GlobalEventDirector>().TriggerEvent(WorldEvent.Eclipse);` to shift the world state and lighting.

### 2. Terrain Deformation (Unity)
- **File:** `UnitySystems.cs`
- **Actions:**
  - `CreateCrater(pos)`: Creates visual and physical depressions on the 10x10 battle grid.
  - `SpawnWall(pos)`: Spawns barriers that block unit pathfinding.

### 3. Cultivation Auras (Unity)
- **File:** `UnitySystems.cs`
- **Effect:** Changes character aura color based on their rank (1-10) using `UpdateAura()`. Rank 10 results in legendary golden/white effects.

### 4. AI Personalities (Unity)
- **File:** `UnitySystems.cs`
- **Races:** Orcs (Aggressive), Elves (Magical), Humans (Tactical), Dwarves (Defensive).
- **Execution:** The AI Director uses `ExecuteTurn()` to determine behavior based on the race's specific profile.

### 5. Procedural Relic System (Blender)
- **File:** `blender_world_gen.py`
- **Command:** `create_procedural_relic("Ancient Orb", "Orb")`
- **Result:** Generates a glowing, emissive artifact for hiding in Dungeons.

---

## 🏗️ Legacy Core Modules (v17.16.0)

### 6. Dynamic Weather System (Unity)
- **Component:** `WeatherSystem`
- **Setup:** Assign Particle Systems for `Rain` and `Fog` in the inspector (UnitySystems.cs).
- **Usage:** `GetComponent<WeatherSystem>().SetWeather("Rain");`

### 7. Cultivation Quest System (Unity)
- **Component:** `QuestManager`
- **Nodes:** Define stages of cultivation using `QuestNode`. Advance with `CompleteCurrent()`.

### 8. Procedural Weapon System (Blender)
- **Command:** `create_procedural_weapon("Sword", "Legendary")`.

---

## 🎨 Creative Manifestation: Blender to Unity Workflow

### Step 1: Continent & Infrastructure (Blender)
1. **Script:** Use `blender_world_gen.py`.
2. **Continents:** 4 distinct zones are created.
3. **Castles:** 12 race capitals spawned. Levels (1-5) determine the visual complexity (number of towers and scale).
4. **Roads & Cells:**
   - Roads are Bezier curves connecting capitals.
   - Cells are generated as a sub-grid of the continent planes for turn-based movement.
5. **Heroes:**
   - **Main Hero:** Larger scale (Warrior/Mage/Archer styles).
   - **Secondary Heroes:** Smaller scale, same classes.
   - **Races:** Empire, Bandits, and Player-specific markers are applied via materials and naming conventions (`Hero_Empire_Mage_Main`).

### Step 2: Export & Sync
1. **Export:** Run the script in Blender; it generates `world_layout.json`.
2. **Unity Setup:** Ensure `WorldSyncManager` is in your scene.
3. **Logic:**
   - Unity reads `world_layout.json`.
   - It instantiates prefabs (Warrior/Mage/Archer) based on the `type` and `class` fields.
   - Roads are instantiated as line renderers or procedural mesh paths.

### Step 3: Turn-Based Combat & Skills
1. **On-Map State:** Heroes are "small" markers on the cells.
2. **Transition:** When Hero (Player) clicks on a Cell containing a Bandit/Empire hero, the `BattleZoneGenerator` is triggered.
3. **Battle Arena:**
   - A 10x10 sub-grid is spawned.
   - Models are scaled up to "Combat Mode" size.
   - **Animation:** Use simple `MoveTowards` and `Triggers` to show one unit attacking another.
4. **Skills (3+1+1 Pattern):**
   - **3 Common Skills:** Strike, Block, Heal (shared by all).
   - **1 Ultimate:** Each class has its own (e.g., Mage -> Fireball).
   - **1 Passive:** Each race has its own (e.g., Orcs -> Rage).

### Step 4: Step-by-Step Implementation Guide
1. **In Blender:**
   - Press `Run Script`.
   - Verify 4 continents with roads are visible.
   - Check `world_layout.json` exists in the folder.
2. **In Unity:**
   - Add `UnitySystems.cs` to a Global GameObject.
   - Configure `QuestManager` and `AIDirector`.
   - Use the `SmartCameraController` to follow your Hero across the roads.

---

## 🎯 Future Roadmap (v17.18+)

1. **Multiplayer Sync Hub:**
   - Real-time synchronization of world events between different player instances.
2. **Dynamic Economy Module:**
   - Resource pricing based on continent scarcity and trade routes.
3. **Advanced Shader Magic:**
   - Volumetric clouds and realistic water shaders for the oceanic boundaries.

---

## 📂 Project Structure Note
- **Blender Scripts:** `blender_world_gen.py`, `blender_connector.py`
- **Unity Scripts:** `UnitySystems.cs`, `UnityWorldSync.cs`, `UnityRaceData.cs`, `UnityConnector.cs`
- **Data Repository:** `game_design.json`, `knowledge_base.json`
- **Documentation:** `IMPLEMENTATION_GUIDE.md` (This file), `PROJECT_MASTER_BLUEPRINT.md`
