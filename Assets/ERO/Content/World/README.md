# ERO Final Content — World

## Procedural Aetheria World Generator

`EROProceduralWorldGenerator.cs` is the deterministic world-layout foundation for ERO. It creates the complete Aetheria zone graph from a fixed world seed and does not depend on third-party copyrighted game maps.

### Current zone graph

| Zone | Levels | Grid |
|---|---:|---:|
| Greenhaven | 1–10 | 0,0 |
| Everwood | 10–20 | 1,0 |
| Elyndor | 20–30 | 0,1 |
| Frostfall | 30–40 | 1,1 |
| Sunscar | 40–50 | 2,0 |
| Lunareth | 50–60 | 2,1 |
| Abyssia | 60–70 | 3,0 |
| The Eternal Rift | 70–100 | 3,1 |

Each zone is 512 m x 512 m and is generated deterministically from seed `740067` using layered noise, roads, settlement anchors, points of interest and zone boundaries.

### AAA asset integration strategy

The generator is intentionally asset-agnostic. Approved assets can be plugged into the generation layer later:

- CC0 / explicitly commercially licensed environment kits
- approved vegetation, rocks, props and architecture
- ERO-original landmarks, bosses, NPCs and signature structures
- ERO materials, VFX, lighting, audio and biome dressing

The procedural system generates **layout and composition**, not copies of third-party game maps. It is the base for deterministic world streaming, biome dressing, POI population, navigation data and server-side zone identity.

### Production roadmap

1. Replace primitive dressing with approved/licensed prefabs.
2. Add biome-specific procedural rules and weighted spawn tables.
3. Add roads, rivers, cliffs, caves, landmarks and dungeon entrances as spline/graph features.
4. Add NavMesh/runtime navigation baking strategy.
5. Add additive scene/chunk streaming around players.
6. Add server-authoritative zone/interest-management integration.
7. Add minimap/world-map data generated from the same zone graph.
8. Add deterministic POI IDs so quests, NPCs, gathering nodes and events persist across servers.
