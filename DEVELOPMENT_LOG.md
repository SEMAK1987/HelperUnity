# DEVELOPMENT LOG

## [v18.11.23] - 2026-07-02
### Added
- Re-engineered Potion Shop item rendering with level-based color rarity names.
- Added detailed characteristics and temporary combat buffs description for each potion item.
- Added explicit required Castle level under each potion item name.
- Repositioned price button cleanly at the far-right of each horizontal shop row.
- Expanded Barracks troop passive skills column width from 250 to 380 (descriptions to 370) to prevent wrapping and squishing.
- Enhanced XP cheat code reward to give +100 XP (previously +50 XP).
- Lifted Class Active/Passive skills layout upwards by reducing vertical spacer between the equipment mannequin and skill titles.

## [v18.11.22] - 2026-07-01
### Optimized
- Replaced dynamic GUIStyle allocations inside the OnGUI rendering loop in `FateCastleManager.cs` with lazy-cached class fields to optimize memory.
- Added interlocking system to disable the "Skip Day" button when the Hero Management character panel is active.
- Configured active & passive skill cards as unclickable hover boxes displaying interactive tooltips following the mouse cursor.
- Integrated horizontal scrolling for inventory tabs and compressed potion labels to solve layout clipping.

## [v18.11.21] - 2026-06-30
### Fixed
- Fixed missing `ShowSkillDetailPopup` method compiler errors (CS0103) in `FateCastleManager.cs`.
- Standardized active skill descriptions dynamically matching all three major player classes (Warrior, Archer, Mage) using class data from `SaveGameSystem`.

## [v18.11.20] - 2026-06-29
### Added
- Integrated a fullscreen 3-column Zenith Hero Control Panel in `FateCastleManager.cs` to solve the small parameters view.
- Implemented a local-persistent, secure 36-slot inventory grid supporting stacking items and gear.
- Added dynamic 8-slot equipment mannequin with attributes calculations (+STR, +AGI, +INT, +STA).

## [v18.11.19] - 2026-06-28
### Improved
- Solved overlapping of dialogue choice buttons with portraits and dialogue text by lowering layout positions (anchoredPosition Y=-20f, sizeDelta=-120f, 44f).

## [v18.11.18] - 2026-06-27
### Added
- Formulated 14 diverse troop cohort definitions (such as Боец Фракции, Паладин Света, Кентавр Степей, Легендарный Дракон Пустоты, etc.) with unique traits and skill quantities limits.
