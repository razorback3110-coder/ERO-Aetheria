# ERO Asset & License Registry — Unreal Runtime Addendum

This addendum is governed by `Assets/ERO/Legal/ERO_Asset_License_Registry.md` and exists to preserve an auditable record for the latest Unreal runtime-only additions.

## ERO-authored runtime content

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROEnemyActor.h` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Replicated PvE enemy state, health, defeat and respawn contract |
| `Unreal/Source/EROAetheria/EROEnemyActor.cpp` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative PvE damage, defeat, XP reward and respawn behavior |
| `Unreal/Source/EROAetheria/EROEncounterDirectorTypes.h` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Data-driven starter encounter spawn definitions and validation |
| `Unreal/Source/EROAetheria/EROGameMode.h` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Configurable server-owned starter encounter collection |
| `Unreal/Source/EROAetheria/EROGameMode.cpp` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Runtime spawning from validated encounter definitions instead of hard-coded spawn loops |

## Asset policy

These changes contain no third-party mesh, texture, animation, audio, map, UI or proprietary game content. They are source-code/data definitions authored for ERO. Any future production asset integration remains subject to the parent registry's commercial-license verification and art-direction quality gate.
