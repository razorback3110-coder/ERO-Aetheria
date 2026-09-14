# ERO Approved Asset Catalog

This is the implementation catalog for legal, textured, commercial-use asset sources. It complements the legal registry; the registry remains the legal gate.

## Environment / world

| Source | Intended ERO use | License status |
|---|---|---|
| Quaternius Medieval Village MegaKit | Aetheria villages, inns, markets, houses, faction settlements | CC0 / commercial use verified |
| Quaternius Fantasy Props MegaKit | Furniture, chests, tools, potions, stalls, weapons and dressing | CC0 / commercial use verified |
| Quaternius Stylized Nature MegaKit | Trees, plants, flowers, rocks, grass and bushes | CC0 / commercial use verified |
| Quaternius Modular Dungeons Pack | Modular dungeon rooms and corridors | CC0 / commercial use verified in legal registry |
| Quaternius Ultimate Modular Ruins Pack | Ruins, abandoned POIs, world dressing | CC0 / commercial use verified in legal registry |
| Kenney Fantasy Town Kit | Supplementary medieval town pieces | CC0 / commercial use verified |
| Kenney Retro Fantasy Kit | Supplementary fantasy architecture/props where style fits | CC0 / commercial use verified |
| Kenney CC0 asset library | Supporting props/UI/environment assets where visually appropriate | CC0 / commercial use verified |

## Integration rule

Asset packs are source material, not the ERO identity. Before a pack enters a production scene:

1. verify its entry in `Assets/ERO/Legal/ERO_Asset_License_Registry.md`;
2. import the textured source rather than replacing it with Unity primitives;
3. normalize scale, pivot and naming;
4. assign ERO-approved materials and lighting treatment;
5. generate/verify LODs and colliders;
6. group repeated assets for instancing/streaming;
7. record any source-specific shader/material dependency;
8. never redistribute the third-party pack as a standalone asset pack.

## Visual mixing policy

Mixing CC0 packs is allowed, but the scene must be art-directed as one ERO world. Recoloring, material normalization, decals, lighting, foliage composition, terrain treatment and architectural kitbashing should be used to create a unified Aetheria look.

Do not intentionally reproduce the visual identity, map layout, characters, UI, monsters, equipment or other protected expression of Ragnarok, Lost Ark, Throne and Liberty or another commercial game.

## Source verification snapshot

Verified 2026-09-13 against the providers' current public pages:
- Quaternius Medieval Village MegaKit states CC0 and free commercial use.
- Quaternius Fantasy Props MegaKit states CC0 and free commercial use.
- Quaternius Stylized Nature MegaKit states CC0 and free commercial use.
- Kenney states its game assets are public-domain CC0 and usable commercially; individual asset pages also identify CC0.

Official source pages are recorded in the legal registry and should be rechecked before each commercial release if a provider changes licensing terms.
