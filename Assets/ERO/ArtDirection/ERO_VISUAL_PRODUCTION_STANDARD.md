# ERO Visual Production Standard — Textured MMORPG Target

Status: LOCKED FOUNDATION

## Goal

ERO must not ship or present as a primitive prototype. The visual target is a polished stylized dark-fantasy MMORPG with authored material treatment, readable silhouettes, layered texture detail, atmospheric lighting, and strong character presentation.

The existing Character Creation reference remains the visual master reference. This document defines how production assets must be authored and integrated around it.

## Visual pillars

1. **Textured, not flat** — environment and character surfaces use albedo plus appropriate normal/detail/roughness information where the source asset provides it.
2. **Stylized dark fantasy** — deep blue/black foundations, silver/steel, restrained gold accents, magical cyan/violet highlights, warm emissive fire/light sources.
3. **Material hierarchy** — stone, wood, metal, cloth, leather, skin, foliage, magical surfaces and UI must be visually distinguishable at gameplay distance.
4. **Readable silhouettes** — class identity and equipment must remain recognizable before texture detail is even considered.
5. **Controlled wear** — dirt, edge wear, moss, scratches and roughness variation are used to avoid sterile primitive-looking surfaces.
6. **Cinematic lighting** — baked/static lighting where useful, realtime key/fill/rim lighting where needed, fog/atmosphere and restrained bloom for magical highlights.
7. **Consistency** — third-party assets are allowed only when they can be recolored/recomposed into the ERO visual language without copying another game's identity.

## Production asset tiers

### Tier A — Hero assets
Characters, class weapons, major armor sets, MVPs, bosses, important NPCs, mounts and iconic buildings.
- Highest silhouette quality.
- Full material treatment.
- Dedicated LODs.
- Custom ERO color/material variants.

### Tier B — Gameplay assets
Common monsters, normal NPCs, buildings, props, vegetation, dungeon pieces and interactables.
- Textured and optimized.
- Shared material atlases encouraged.
- LODs for repeated/high-count objects.

### Tier C — Background dressing
Small rocks, clutter, distant props, foliage clusters and repeated filler.
- Aggressively optimized.
- Can use shared textures/materials.
- Must still receive correct lighting and color treatment.

## Approved asset integration

The production source catalog is intentionally based on legally verified CC0/commercially usable packs. Current approved high-value sources include:
- Quaternius Medieval Village MegaKit — modular village architecture.
- Quaternius Fantasy Props MegaKit — furniture, tools, weapons, potions, chests and market dressing.
- Quaternius Stylized Nature MegaKit — trees, plants, rocks, grass and bushes.
- Quaternius Modular Dungeons Pack / Ultimate Modular Ruins Pack — dungeon and ruin structures.
- Kenney CC0 libraries, including Fantasy Town Kit and Retro Fantasy Kit, where their visual language fits the scene.

Every source must also exist in `Assets/ERO/Legal/ERO_Asset_License_Registry.md` before production use.

## No primitive placeholders in presentation scenes

Primitive Unity cubes/spheres/capsules are permitted only for:
- collision/debug visualization;
- temporary gameplay prototyping;
- automated tests.

They are not acceptable as final environment, character, weapon, armor, UI or promotional presentation assets.

## ERO material recipe

When source assets support the data, prefer:
- Base Color / Albedo
- Normal Map
- Masked Roughness/Smoothness
- Metallic where physically appropriate
- AO where available
- Emission for magic, portals, runes, crystals and selected UI/world effects

Do not invent missing maps in a way that damages the source asset. Procedural detail may supplement a legal source asset but must remain visually coherent.

## World presentation

Scenes should combine:
- modular architecture;
- textured terrain;
- 3–5 vegetation/rock variants per biome minimum;
- prop clusters rather than isolated primitives;
- decals or material variation for roads, wear and points of interest;
- atmospheric depth;
- controlled color grading;
- dynamic day/night and weather systems.

## Characters

Character production must support the seven locked classes and a shared body/customization architecture. Armor, weapons and cosmetics must be data-driven and visually independent from gameplay stats whenever practical.

The character creation reference in this folder is a composition reference only; no proprietary character or game asset is to be copied from another title.

## Performance requirements

Textured does not mean unoptimized:
- use shared materials and atlases for repeated assets;
- use LODGroups for 3D assets where appropriate;
- use GPU instancing for repeated environment pieces;
- stream world content by region/chunk;
- keep hero assets high quality while reducing background complexity;
- profile before raising texture resolution globally.

## Definition of done for a visual scene

A scene is visually production-ready only when it contains legal textured assets, coherent materials, lighting, atmospheric depth, collision/navigation where required, LOD/optimization treatment, and a documented source/license for every third-party asset family.
