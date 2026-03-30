# All Mobs Skills

This file contains the skill definitions for the Unity AI project.

## Basic Skills
- **Patrol**: Move between a set of waypoints.
- **Chase**: Follow the player when within detection range.
- **Attack**: Perform a melee or ranged attack when close to the target.
- **Idle**: Wait or perform ambient animations.

## Advanced Skills
- **Special Ability**: Trigger a unique skill based on mob type (e.g., Fireball, Heal).
- **Flee**: Move away from the target when health is low.
- **Group Coordination**: Communicate with nearby mobs to surround the player.

## Unity Implementation
- Uses `NavMeshAgent` for movement.
- Logic handled via `BaseAIController.cs`.
