# ERO Asset & License Registry — Unreal Character Persistence Addendum — 2026-09-21

This addendum supplements `Assets/ERO/Legal/ERO_Asset_License_Registry.md`.

## ERO-authored runtime code

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROPlayerCharacter.h` persistence restore extension | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-safe restoration of level, XP, class, weapon identity and weapon family from the player's persistent save |
| `Unreal/Source/EROAetheria/EROPlayerEconomySaveGame.h` character snapshot fields | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Version-2 persistent character progression snapshot alongside Gold and inventory |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.h/.cpp` character persistence bridge | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-only loading, delayed pawn restoration, checkpoint persistence and backward-compatible schema migration |
| `Tools/Validate-EROUnrealGameplayContract.sh` persistence contract checks | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | CI gate for persistent character progression fields and restore/save lifecycle |

No third-party models, textures, animations, audio, UI, maps or extracted game content were added by this change. All identifiers and persistence structures are ERO-authored data/code.

## Persistence compatibility

Save schema advances from version 1 to version 2. Version-1 saves remain readable for Gold/inventory and are not treated as character-progression snapshots. New saves write version 2 with character level, experience, class, weapon ID and weapon family.

## Legal gate

This change introduces no new third-party asset dependency. Any future production visual/equipment asset must still be independently verified under the main registry before integration.
