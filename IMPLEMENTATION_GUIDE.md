# Continent of Fate: Implementation Guide v17.16.0

This guide explains how to use and integrate the various modules for "Continent of Fate" project within Unity and Blender.

---

## 1. Dynamic Weather System (Unity)
- **Component:** `WeatherSystem`
- **Setup:** Assign Particle Systems for `Rain` and `Fog` in the inspector (UnitySystems.cs).
- **How to use:** Call `SetWeather("Rain")` from your environment manager.
- **Visuals:** Automatically toggles particle effects and logs debuffs to the console (v17.16.0).

## 2. Cultivation Quest System (Unity)
- **Component:** `QuestManager`
- **Nodes:** Define stages of cultivation using `QuestNode`.
- **Workflow:** Use `CompleteCurrent()` to advance through levels of power and story.

## 3. AI Director - Aggression (Unity)
- **Component:** `AIDirector`
- **System:** Controls bandit behavior and spawn rates.
- **Usage:** Call `SetAggression(0.1f)` for a peaceful start, increasing over time to challenge the player.

## 4. Procedural Weapon System (Blender)
- **Script:** `blender_world_gen.py`
- **Command:** `create_procedural_weapon(name, rarity)`.
- **Logic:** Generates weapons with materials and scales based on rarity (Common to Legendary).

---

## 5. Landscape Architect (Blender + Unity)

### In Blender:
1. Open Blender and navigate to the **Scripting** tab.
2. Load or paste the contents of `blender_world_gen.py`.
3. Press **Run Script**.
4. **What happens:** 
   - 4 continents are generated with displacement-based relief (mountains/valleys).
   - 12 race capitals are placed with grid systems.
   - Placeholder heroes and units are spawned around each capital.
   - A `world_layout.json` file is (ideally) exported to your Unity project's `Assets` folder (refer to the script's export logic if customized).

### In Unity:
1. Ensure your scene has a `WorldSyncManager` component attached to a GameObject.
2. In the inspector for `WorldSyncManager`, set the `Json Path` to your exported `world_layout.json`.
3. Fill the `Race Prototypes` array with `RaceData` scriptable objects (see Module 4).
4. Click the **Sync From Blender** button (if exposed in the Inspector via a custom editor or context menu).
5. **What happens:** Unity reads the coordinates and types from JSON and instantiates the corresponding prefabs at the exact locations defined in Blender.

---

## 2. Castle Evolution (Unity)

1. Attach the `CastleController` script to your Castle prefabs.
2. In the inspector:
   - Assign the visual models for each level (1 to 5) to the `Level Meshes` array.
   - Assign an `Upgrade Effect` (Particle System) if desired.
3. Call `castleController.Upgrade()` from your game logic (e.g., when a player pays for an upgrade).
4. **Mechanics:** The script automatically switches visibility between the assigned meshes and plays the particle effect.

---

## 3. Battle Zone Generator (Unity)

1. Attach the `BattleZoneGenerator` script to an empty GameObject that will act as the battle arena parent.
2. Assign a `Cell Prefab` (a simple quad or hexagon with a highlight script).
3. Call `GenerateZone(terrainType)` when a battle starts.
4. **Integration:** Use the `game_design.json` data for `world_combat_locations` to determine the `width` and `height` based on the continent.

---

## 4. Army & Hero Hierarchy (Unity)

1. Create `RaceData` assets via **Right Click -> Create -> ContinentOfFate -> RaceData**.
2. Define the base stats (HP, ATK, DEF) and assign prefabs for the race's units.
3. Use the `HeroStats` class to manage hero levels and support bonuses.
4. **Logic:** The `GetSupportBonus` method calculates passive buffs for the main hero based on up to 10 support heroes in the squad.

---

## 5. Smart Camera & Boundaries (Unity)

1. Attach the `SmartCameraController` script to your Main Camera.
2. Assign a `Target` (usually the current active Hero or the player's cursor).
3. Set `Min Bounds` and `Max Bounds` in the Inspector to match your continent's size.
4. **Behavior:** The camera will smoothly follow the target but will stop at the boundaries, preventing the player from seeing "outside" the world map.

---

## 6. Interactive Bridge (JSON Sync)

1. The data exchange uses `world_layout.json`.
2. Format:
   ```json
   {
     "objects": [
       { "name": "Castle_Orcs", "type": "Castle", "position": {"x": 10, "y": 0, "z": 5}, "race": "Orcs" }
     ]
   }
   ```
3. Use `WorldSyncManager.SyncFromBlender()` to re-import the layout if you make changes in Blender.

---

## 7. Magic & Alchemy (Unity)

1. Attach the `MagicController` script to your World or Battle manager.
2. Assign Particle Systems for `Heal`, `Mana`, and `Explosion`.
3. Call `PlayEffect(type, position)` when an ability or potion is used.
4. **Scaling:** Refer to `game_design.json` -> `potions_system` for the mathematical scaling of effects ($Base + Level \times Factor$).

---

## Technical Maintenance
- **Version:** `v17.15.0 (Omniversal Architect Elite)`
- **Core Config:** `game_design.json` (Keep this file updated for all balance changes).
- **Frontend:** Access the "Game Studio" tab in the web dashboard for real-time design adjustments and GDD export.
