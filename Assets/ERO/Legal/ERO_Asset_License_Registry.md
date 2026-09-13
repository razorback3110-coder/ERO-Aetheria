# ERO Asset & License Registry

This registry is the legal gate for third-party content used by Eternal Realms Online.

## Policy

- Only assets with a verified license permitting use in a commercial game may enter `APPROVED`.
- CC0/public-domain assets are preferred.
- Never copy or ship proprietary assets, maps, UI, characters, music, sounds, animations or code from commercial games without an explicit redistribution/commercial license.
- Do not redistribute third-party assets as a standalone asset pack.
- Keep the original license/source URL with the project so the release can be audited.

## APPROVED — environment / props

| Asset | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| Medieval Village MegaKit | https://quaternius.itch.io/medieval-village-megakit | CC0 1.0 | Yes | Not required | Villages, houses, inns, markets, faction settlements |
| Fantasy Props MegaKit | https://quaternius.itch.io/fantasy-props-megakit | CC0 1.0 | Yes | Not required | Furniture, chests, tools, potions, market props |
| Modular Dungeons Pack | https://quaternius.itch.io/lowpoly-modular-dungeon-pack | CC0 1.0 | Yes | Not required | Dungeon walls, rooms, props |
| Ultimate Modular Ruins Pack | https://quaternius.com/packs/ultimatemodularruins.html | CC0 | Yes | Not required | Ruins, abandoned POIs, world dressing |
| Kenney Modular Dungeon Kit | https://kenney.nl/assets/modular-dungeon-kit | CC0 | Yes | Not required | Dungeon filler / modular dressing |
| Kenney CC0 asset library | https://kenney.nl/support | CC0 | Yes | Not required | UI/props/environment where stylistically appropriate |

## APPROVED — rules

Quaternius' current QAL v1.0 explicitly permits commercial games, modification, embedding and distribution of a completed product, while prohibiting redistribution of the assets themselves as standalone assets. Kenney states that assets on its asset pages are CC0 and usable commercially.

## REVIEW

Any new source must be added here before it is referenced by production generation rules. Record the exact pack name, source URL, license/version, acquisition date and any provider-specific restrictions.

## REJECTED

- Ragnarok Online / Ragnarok: The New World original assets, maps, UI, characters, sounds, music, sprites, models or extracted files.
- Lost Ark original assets or maps.
- Throne and Liberty original assets or maps.
- Any ripped/private-server assets whose redistribution rights cannot be proven.

## Release rule

A production build is considered legally ready only when every non-ERO asset referenced by the build is either CC0/public-domain or covered by an explicit commercial redistribution license, and the corresponding registry entry is present.
