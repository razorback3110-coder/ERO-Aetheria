# ERO — World Asset Integration Specification

Status: ACTIVE PRODUCTION SPEC

## Objective

Turn the approved CC0/commercial asset catalog into repeatable ERO world production without shipping primitive-looking presentation scenes.

## Legal gate

Every generated/imported asset family must already exist in `Assets/ERO/Legal/ERO_Asset_License_Registry.md`. This document does not grant a license. A missing registry entry blocks production use.

## Automatic integration pipeline

1. Discover only registry-approved source families.
2. Import textured source packages into the project through the local/editor import process.
3. Normalize units, pivots and naming without altering source attribution metadata.
4. Apply ERO material profiles where source maps permit it.
5. Add colliders and navigation data according to gameplay use.
6. Generate LODGroups for repeated/high-count assets.
7. Mark static/instanced content for batching where appropriate.
8. Build biome-specific prefabs and composition rules.
9. Validate that no production presentation prefab uses Unity primitive geometry as its visual mesh.
10. Record the source family and ERO treatment in the scene/content manifest.

## First production biome — Aetheria Frontier

### Village core
- Quaternius Medieval Village MegaKit: houses, inns, market and settlement structures.
- Quaternius Fantasy Props MegaKit: stalls, chests, furniture and environmental dressing.
- Kenney Fantasy Town Kit: supplementary structures only when art-directed to match.

### Wilderness
- Quaternius Stylized Nature MegaKit: trees, plants, rocks, grass and bushes.
- Kenney Road Textures: road/ground support where visually coherent.
- Layer terrain with paths, clearings, landmarks and resource nodes.

### Ruins / dungeon entrance
- Quaternius Ultimate Modular Ruins Pack.
- Quaternius Modular Dungeons Pack.
- Kenney Modular Dungeon Kit for supplementary filler.

## ERO visual treatment

The environment must read as one world despite mixed sources:
- consistent scale;
- consistent fog and lighting response;
- restrained dark-fantasy palette;
- shared material parameters for wood, stone, metal, cloth and foliage;
- controlled saturation;
- decals/wear/terrain variation;
- clustered props rather than isolated assets;
- strong landmarks for navigation.

## Streaming strategy

World content is divided into region/chunk manifests. Each region declares:
- region ID;
- biome;
- bounds;
- spawn tables;
- approved asset families;
- navigation data;
- LOD/streaming tier;
- dynamic event hooks.

Only nearby chunks and required gameplay-critical data should be resident. Far content uses lower LOD or unloaded state. Server simulation and persistence remain authoritative independently of client visual streaming.

## Performance baseline

- Shared materials where visually safe.
- GPU instancing for repeated props/foliage.
- LOD for large/repeated meshes.
- Texture resolution chosen per asset tier, not globally.
- Avoid per-object Update loops for environment dressing.
- Profile before increasing quality budgets.

## Character/equipment boundary

World integration must expose sockets/anchors for player characters, visible equipment, PETs, mounts and interaction points. Gameplay data never depends on a specific third-party mesh.

## Commercial release gate

Before a release candidate is built, enumerate all referenced third-party asset families and compare them with the license registry. Any unregistered or license-uncertain family fails the release gate.
