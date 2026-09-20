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
