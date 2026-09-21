# ERO Asset & License Registry — Unreal Gold Fix Addendum — 2026-09-21

This addendum supplements `Assets/ERO/Legal/ERO_Asset_License_Registry.md` for the player-gold implementation hardening.

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROPlayerCharacterEconomy.cpp` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Overflow-safe server-authoritative player Gold award implementation required by the Unreal character/economy contract |
| `Tools/Validate-EROUnrealGameplayContract.sh` gold implementation check | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | CI regression gate ensuring the declared player Gold API has a concrete implementation |

No third-party asset payloads were added. No proprietary game content was copied or referenced.

Any future third-party visual, audio, animation, model, UI, font, or SDK dependency must be independently verified for commercial use and recorded in the main registry before production integration.
