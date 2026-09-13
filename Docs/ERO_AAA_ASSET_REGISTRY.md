# ERO — AAA Asset & License Registry

## Purpose

This registry is the legal gate for all third-party content used by Eternal Realms Online (ERO / Aetheria: The Eternal Rift).

**Rule:** an asset is integrated into the shipped game only when its exact source and license permit commercial game distribution and the required conditions are recorded here.

## Approved sources

| Source | License / terms | Commercial game | Modification | Attribution | ERO status |
|---|---|---:|---:|---:|---|
| Quaternius assets covered by QAL v1.0 | Quaternius Asset License v1.0 | YES | YES | NO | APPROVED |
| Kenney game assets on asset pages | CC0 | YES | YES | NO | APPROVED |
| Unity Asset Store non-restricted assets | Unity Asset Store EULA + provider terms | YES, subject to EULA | YES, subject to EULA | Usually no, verify provider | APPROVED PER-ASSET |

## Quaternius — approved use

The current QAL v1.0 permits worldwide, royalty-free, perpetual commercial use, modification and incorporation into a Product. The completed game may be published and sold. The standalone assets themselves may not be resold or redistributed as an asset pack.

Candidate ERO packs to evaluate/import:

- Medieval Village MegaKit — environment foundation for Greenhaven and rural settlements.
- Fantasy Props MegaKit — props, furniture, dungeon/world dressing.
- Modular Character Outfits — Fantasy — modular NPC/player outfit prototyping.
- Additional Quaternius environment, creature and character packs as individually recorded.

Source: https://quaternius.com/license.html

## Kenney — approved use

Kenney states that game assets on its asset pages are CC0 and may be used in commercial projects without attribution. The exact asset page/license file remains the provenance record for each imported pack.

Candidate ERO use:

- UI/icon prototypes
- interaction and inventory iconography
- generic props
- particles and effects where visually appropriate
- placeholder/environment dressing

Source: https://kenney.nl/support

## Unity Asset Store — per-asset gate

Only non-restricted assets whose provider terms permit our intended use are eligible. Each purchased/free asset must have its provider page, license/EULA, version/date and Asset Store identifier recorded before integration.

The asset must be embedded in ERO as part of a substantially original game and must not be redistributed as a standalone asset collection.

Source: https://unity.com/legal/as-terms

## MMORPG/game-content policy

ERO does **not** import proprietary assets from Ragnarok Online, Ragnarok: The New World, Lost Ark, Black Desert, Throne and Liberty, Final Fantasy XIV, World of Warcraft, Guild Wars 2, Diablo, or other commercial games unless the rights holder explicitly grants a redistribution license covering our use.

These games may be used as design references for mechanics and systems, not as sources of extracted models, textures, maps, audio, UI, animations, characters or other copyrighted game files.

## License statuses

- **APPROVED** — license verified and use is permitted.
- **APPROVED PER-ASSET** — source is acceptable, but the individual asset/provider terms must be recorded.
- **REVIEW** — potentially usable; do not ship until the exact terms are verified.
- **REJECTED** — do not import or ship.

## Required record for every imported asset

```text
Asset ID:
Pack / Asset:
Source URL:
Author / Provider:
License:
License version/date:
Commercial use: YES/NO
Modification: YES/NO
Game distribution: YES/NO
Attribution: NONE / REQUIRED
Share-alike / copyleft: YES/NO
Restrictions:
Imported files:
ERO modifications:
Hash/version:
Approval status:
``` 

## AAA art direction

Third-party CC0/licensed assets are raw material, not the final ERO identity. Before final shipping they should be unified through ERO materials, lighting, post-processing, scale conventions, LODs, collision, VFX, animation retargeting and art-direction rules.

The signature ERO content remains original: player identity, major cities, faction architecture, iconic bosses, legendary equipment, signature VFX, UI language, narrative locations and cinematics.
