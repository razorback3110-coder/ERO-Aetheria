# ERO Unreal Inventory Addendum — 2026-09-21

This addendum covers the server-authoritative Unreal inventory foundation added to ERO-Aetheria.

## Original ERO code

| File | Purpose | License basis |
|---|---|---|
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.h` | Replicated inventory stack data and economy state declaration | Original ERO project code; no third-party asset dependency |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.cpp` | Server-authoritative gold and item grants | Original ERO project code; no third-party asset dependency |
| `Unreal/Source/EROAetheria/EROEnemyActor.h` | PvE item reward configuration | Original ERO project code; no third-party asset dependency |
| `Unreal/Source/EROAetheria/EROEnemyActor.cpp` | Server-authoritative PvE item reward grant | Original ERO project code; no third-party asset dependency |

## Runtime item identifier

`Aetherium_Shard` is an ERO gameplay identifier only. It is not a reference to an external game's asset, trademark, model, texture, audio file, or other proprietary content.

No external asset was imported by this change.

## Commercial-use gate

Any future visual, audio, animation, model, texture, font, or other third-party content associated with inventory items must be independently verified for commercial-game use and recorded in `Assets/ERO/Legal/ERO_Asset_License_Registry.md` before integration.
