# ERO Unreal Engine migration plan

## Decision

ERO's target runtime is moving from Unity to Unreal Engine 5.8. The Unity implementation is retained temporarily only as a migration/reference source; new gameplay runtime work should target Unreal.

Epic documents the following UE systems as suitable foundations for ERO:
- World Partition for distance-based world streaming.
- Gameplay Ability System for RPG attributes/abilities, including replicated abilities.
- Mass Entity/Mass Gameplay for data-oriented large populations and LOD/replication.
- Dedicated-server architecture for authoritative multiplayer.

## Migration mapping

| ERO requirement | Unreal target |
|---|---|
| 8 classes / evolutions 18/40/75 | Data Assets + Gameplay Ability System |
| PvE/PvP/MVP/GvG | server-authoritative Gameplay Abilities + replicated gameplay state |
| 250,000 HP MVP / level scaling | authoritative replicated attributes |
| Procedural world | World Partition + PCG |
| Streaming | World Partition runtime cells |
| Inventory/equipment | replicated item state + backend persistence |
| Quests | data-driven quest subsystem |
| Guilds/GvG | server services + replicated guild state |
| Dedicated server | Unreal Server target |
| Steam | Unreal Online/Steam integration after the first multiplayer slice |
| Cristaux ERO | backend-authoritative wallet; never client-authoritative |

## Migration rules

1. Do not port Unity implementation line-for-line when Unreal has a native system that solves the same problem.
2. Preserve gameplay rules and data contracts, not engine-specific implementation.
3. Do not import ripped/proprietary game assets.
4. Do not treat a marketplace asset as approved until its commercial license is verified and recorded.
5. Keep server authority over damage, rewards, inventory, currency, progression and MVP state.

## First playable vertical slice

The first Unreal slice should be:

login/entry -> character -> movement -> target selection -> authoritative attack -> enemy defeat -> XP/loot -> inventory/equipment -> quest -> MVP encounter -> save/load -> dedicated-server client connection.

After that, expand to guilds/GvG, raids, towers, world events and Steam packaging.


## UE5 visual world generation

UE5 is now the visual authoring/runtime target. The intended stack is World Partition + PCG + Nanite + Lumen + Niagara, with editor automation for deterministic world generation.

AEROEnvironmentActor no longer creates lights or fog during gameplay. The renderer is configured for Lumen/virtual shadows, while editor-generated lighting is stored in the level asset. This separates gameplay/server code from editor lighting and removes the recurring Light/lighting rebuild dependency.

Unreal/Scripts/GenerateAetheriaVisualWorld.py creates a partitioned Aetheria visual map with five biome districts (Capital, Plains, Forest, Desert, Snow), roads, landmarks, crystals, vegetation clusters and dungeon/raid/MVP gateways. It uses only Unreal built-in primitives and ERO-authored materials as a generation scaffold.

The same rules will later drive production zones: capital, plains, forest, mountain/snow, desert, swamp/corrupted, ruins, dungeons, raids, MVP regions and PvP/GvG areas. Approved commercial/CC0 assets can replace generated geometry without changing gameplay rules.


## Complete Aetheria world generation

The launch-world generator now targets one complete World Partition map rather than isolated demo zones:

- Aetheria Capital
- Starter Plains
- Whispering Forest
- Frostpeak Mountains
- Sunscar Desert
- Mire of Corruption
- Ancient Ruins
- Arcane Highlands
- PvP/GvG Frontier
- connected world roads and waypoints
- dungeon gateways
- raid gateways
- MVP/world-boss territories
- capital buildings, gates and landmark
- deterministic biome dressing

The canonical generation script is `Unreal/Scripts/GenerateAetheriaVisualWorld.py`; its machine-readable world contract is `Unreal/Config/AetheriaWorldManifest.json`.

This generated world is the full visual launch scaffold. It is intentionally made from UE built-in primitives and ERO-authored materials so it can be generated without introducing unverified third-party IP. Production Nanite/PCG assets can subsequently replace individual generated actors while preserving the world coordinates and gameplay content nodes.


## Corrected Aetheria world architecture

The launch world is a **single persistent Aetheria World Partition map**: `/Game/Maps/Aetheria_CompleteWorld`.

It contains eleven connected cities/regions:
1. Aetheria Capital
2. Valoria Plains
3. Elderglen
4. Sunscar
5. Frostheim
6. Mirehaven
7. Arkenfall
8. Astralis
9. Duskmoor
10. Abyssia
11. Drakoria

City travel is same-world waypoint travel. It does not use `ServerTravel` between cities.

### MVP system

There are exactly 22 launch MVP definitions, two associated with each city. MVPs remain on the main Aetheria map in predefined boss territories. Each MVP has a 100 m movement/spawn radius, a predefined level, an independent respawn/reset contract, a unique loot table and a unique lootable necklace cosmetic with its own appearance and stat preset.

The authoritative machine-readable contract is `Unreal/Config/AetheriaMVPManifest.json`.

### City achievement system

Each city has its own achievement collection and a different total. Completion thresholds are driven by `Unreal/Config/AetheriaCityAchievements.json` and support milestone rewards at 400/600/800/1200 where the city's total permits the milestone. Rewards are city-specific and completion is persistent progression.

### Separate instance maps

Only instanced activities receive separate maps:
- Dungeons
- five monthly-reset Tower boss scenes
- PvP Arena 1v1
- PvP Arena 4v4
- GvG War of Realms

GvG travel is gated by the active GvG event. PvP arenas are separate matchmaking destinations rather than persistent regions of Aetheria.

Canonical generators:
- `Unreal/Scripts/GenerateAetheriaWorld.py` — one 11-city world
- `Unreal/Scripts/GenerateAetheriaInstanceMaps.py` — separate activity-map shells

The previous eleven-city-map shell approach is superseded and must not be used for the city layout.


## Character / class visual foundation

The Unreal migration now has a data-driven presentation layer for the eight current playable classes. Each class has a distinct visual profile/outfit identity, a class-specific weapon pool and three specialization-oriented weapon choices. Weapon selection is replicated and validated server-side through `AEROPlayerCharacter::ServerSelectWeapon`; class identity remains fixed while weapon choice changes combat style.

Current visual identities:
- Warrior — Ironheart Vanguard
- Ranger — Wildstrider Scout
- Mage — Astral Weaver
- Assassin — Nightveil Stalker
- Cleric — Dawnkeeper Vestments
- Paladin — Sunwarden Aegis
- Warlock — Voidbinder Regalia
- Summoner — Ethercaller Mantle

The canonical manifest is `Unreal/Config/EROCharacterClassVisualManifest.json`. A Blender bootstrap at `Unreal/Scripts/GenerateEROCharacterPrototypeBlender.py` can generate original low-poly class prototypes as an art-blockout. These prototypes are not final commercial-quality assets; production meshes, rigging, animations, materials and VFX remain a separate import/validation phase.
