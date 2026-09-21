# ERO Unreal Economy Addendum — 2026-09-21

This addendum supplements `Assets/ERO/Legal/ERO_Asset_License_Registry.md` for the Unreal runtime economy slice.

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.h` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Replicated server-authoritative player Gold balance |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.cpp` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Overflow-safe server-side Gold awards and replication |
| `AEROGameMode` economy wiring | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Assigns the authoritative economy PlayerState and configures starter PvE Gold rewards |
| `AEROEnemyActor::GoldReward` and reward path | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative Gold reward on validated PvE defeat |

No third-party asset payloads were added. No proprietary game content was copied. This economy slice uses only ERO-authored C++ and existing ERO runtime definitions.
