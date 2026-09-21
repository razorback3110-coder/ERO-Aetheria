# ERO Asset & License Registry — Unreal Security Addendum

Date: 2026-09-21

This addendum belongs to `Assets/ERO/Legal/ERO_Asset_License_Registry.md` and records ERO-authored runtime code added after the main registry snapshot.

## ERO-authored Unreal security runtime

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Source/EROAetheria/EROEnemyActor.cpp` attack-origin validation | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative PvE damage validation: attacker must be an authoritative ERO player, the instigator controller must match the attacker, and the server validates horizontal/vertical attack distance before applying damage or granting XP |

No third-party asset payload, model, texture, animation, audio, map, UI, or proprietary game content was added by this change.

## Commercial-release rule

The security code above is ERO-authored source code and does not change the approved third-party asset list. Any future third-party content must still satisfy the main registry's commercial-license verification and production-quality gates before integration.
