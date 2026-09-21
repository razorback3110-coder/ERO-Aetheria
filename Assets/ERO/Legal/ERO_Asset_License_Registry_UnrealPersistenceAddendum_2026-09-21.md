# ERO Asset & License Registry — Unreal Persistence Addendum

Date: 2026-09-21

## ERO-authored runtime code

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROPlayerEconomySaveGame.h` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Versioned server-side SaveGame schema for persistent Gold and inventory snapshots |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.h` persistence lifecycle | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative load/save lifecycle for the Unreal economy PlayerState |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.cpp` persistence implementation | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Local dedicated-server persistence bridge keyed by the stable Unreal network identity when available |
| `Unreal/Source/EROAetheria/EROPlayerEconomyState.cpp` periodic checkpoint extension | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative 60-second economy/inventory checkpoints plus final shutdown save to reduce progress loss during unexpected process termination |
| `Tools/Validate-EROUnrealGameplayContract.sh` persistence checks | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | CI contract validation for the Unreal economy persistence foundation and checkpoint lifecycle |

No third-party asset payload was introduced by this change. The persistence implementation uses Unreal Engine runtime APIs only. It is an intermediate local persistence adapter; production external database/storage integration must retain the same authoritative schema and legal gate.

## Legal gate

No proprietary game content, ripped assets, external art, audio, animation, UI or code was copied or introduced.

Any future third-party persistence service SDK or asset must be independently reviewed and added to `Assets/ERO/Legal/ERO_Asset_License_Registry.md` before production integration.
