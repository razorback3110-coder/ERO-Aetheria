# ERO Asset & License Registry

This registry is the legal gate for third-party content used by Eternal Realms Online.

## Policy

- Only assets with a verified license permitting use in a commercial game may enter `APPROVED`.
- CC0/public-domain assets are preferred.
- Never copy or ship proprietary assets, maps, UI, characters, music, sounds, animations or code from commercial games without an explicit redistribution/commercial license.
- Do not redistribute third-party assets as a standalone asset pack.
- Keep the original license/source URL with the project so the release can be audited.

## APPROVED — environment / props / nature

| Asset | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| Medieval Village MegaKit | https://quaternius.com/packs/medievalvillagemegakit.html | CC0 1.0 | Yes | Not required | Villages, houses, inns, markets, faction settlements |
| Fantasy Props MegaKit | https://quaternius.com/packs/fantasypropsmegakit.html | CC0 1.0 | Yes | Not required | Furniture, chests, tools, potions, market props, dressing |
| Stylized Nature MegaKit | https://quaternius.com/packs/stylizednaturemegakit.html | CC0 1.0 | Yes | Not required | Trees, plants, flowers, rocks, grass, bushes, biome dressing |
| Modular Dungeons Pack | https://quaternius.itch.io/lowpoly-modular-dungeon-pack | CC0 1.0 | Yes | Not required | Dungeon walls, rooms, props |
| Ultimate Modular Ruins Pack | https://quaternius.com/packs/ultimatemodularruins.html | CC0 | Yes | Not required | Ruins, abandoned POIs, world dressing |
| Kenney Modular Dungeon Kit | https://kenney.nl/assets/modular-dungeon-kit | CC0 | Yes | Not required | Dungeon filler / modular dressing |
| Kenney Fantasy Town Kit | https://kenney.nl/assets/fantasy-town-kit | CC0 | Yes | Not required | Supplementary town architecture and props |
| Kenney Retro Fantasy Kit | https://kenney.nl/assets/retro-fantasy-kit | CC0 | Yes | Not required | Supplementary fantasy architecture/props where art-directed |
| Kenney CC0 asset library | https://kenney.nl/support | CC0 | Yes | Not required | UI/props/environment where stylistically appropriate |

## APPROVED — texture references

| Asset | Source | License | Commercial game | ERO use |
|---|---|---|---|---|
| Kenney Road Textures | https://kenney.nl/assets/road-textures | CC0 | Yes | Road/ground texture support where visually appropriate |
| Kenney Retro Textures Fantasy | https://kenney.nl/assets/retro-textures-fantasy | CC0 | Yes | Optional fantasy texture accents where consistent with ERO style |

## APPROVED — rules

Quaternius public pack pages currently identify the listed packs as CC0 and explicitly permit personal and commercial use. Kenney states that game assets on its asset pages are public-domain CC0 and may be used in commercial projects. The project must still preserve the exact source URL and verify the current terms before each commercial release.

## Integration quality gate

Legal approval alone does not make an asset production-ready. Production assets must also satisfy `Assets/ERO/ArtDirection/ERO_VISUAL_PRODUCTION_STANDARD.md`: textured/material treatment, coherent ERO art direction, collision, LOD/optimization and scene integration must be addressed. Unity primitives remain debug/prototype-only and are not acceptable as final visual assets.

## REVIEW

Any new source must be added here before it is referenced by production generation rules. Record the exact pack name, source URL, license/version, verification date and any provider-specific restrictions.

## REJECTED

- Ragnarok Online / Ragnarok: The New World original assets, maps, UI, characters, sounds, music, sprites, models or extracted files.
- Lost Ark original assets or maps.
- Throne and Liberty original assets or maps.
- Any ripped/private-server assets whose redistribution rights cannot be proven.

## Release rule

A production build is considered legally ready only when every non-ERO asset referenced by the build is either CC0/public-domain or covered by an explicit commercial redistribution license, and the corresponding registry entry is present.
