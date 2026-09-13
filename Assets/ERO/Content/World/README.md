# ERO Final Content — World

## AAA Procedural Aetheria World

ERO now treats the world as a deterministic **data + generation + streaming** system rather than a single hand-authored test scene. The current generator defines the complete Aetheria progression graph from world seed `740067`; the AAA profile adds biome dressing, natural features, landmarks, dungeons, gathering, events, secrets, navigation and map-data targets.

### Zone graph

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

Each legacy zone layout is 512 m x 512 m and remains deterministic. The AAA profile is chunk-oriented (128 m target chunks, configurable) so the same world can later stream only the chunks needed around each player.

## AAA generation stack

1. **World seed** — stable zone/chunk identity across clients and dedicated servers.
2. **Macro terrain** — elevation, ridges, valleys, plateaus and biome transitions.
3. **Natural features** — rivers, lakes, cliffs, caves, waterfalls and passes.
4. **Road graph** — main roads, secondary paths, bridges and travel hubs.
5. **Settlements** — villages, cities, camps, faction hubs and safe zones.
6. **POI director** — ruins, shrines, resource sites, quest locations, secrets and landmarks.
7. **Combat director** — monster camps, elite territories, patrols, rare encounters and MVP/world-boss anchors.
8. **Dungeon director** — entrances, procedural layouts/variants and encounter seeds.
9. **World-event director** — invasions, rifts, caravans, public events and dynamic objectives.
10. **Dressing** — approved/licensed vegetation, rocks, buildings, props, VFX and audio selected by biome rules.
11. **Navigation** — runtime/baked NavMesh strategy and traversal metadata.
12. **Streaming** — additive chunk loading/unloading around players with deterministic regeneration.
13. **Persistence identity** — stable zone/chunk/POI IDs for quests, NPCs, gathering nodes and events.
14. **Map data** — minimap, world map, teleport points, discovery and fog-of-war generated from the same graph.
15. **Multiplayer authority** — server owns zone identity, spawn selection, encounter state and persistence; clients receive only relevant streamed state.

## AAA quality rules

- Never copy third-party game maps, proprietary assets or distinctive copyrighted presentation.
- Use CC0 or explicitly commercially licensed assets only; preserve source/license/version/hash in the ERO Asset & License Registry.
- Keep iconic cities, signature landmarks, bosses, player characters, class visuals and major set pieces ERO-original.
- Prefer composition and variation over repeated prefab grids.
- Deterministic generation must not depend on frame rate, machine locale or object discovery order.
- Generation must be scalable: editor preview, local client, dedicated server and future large-world streaming must share the same world IDs.

## Performance targets

- Chunk-based world streaming.
- Object pooling for repeated foliage/props/encounters.
- LOD/HLOD-ready asset slots.
- GPU-instancing-ready dressing layer.
- Occlusion/culling-ready scene structure.
- NavMesh generation separated from visual dressing.
- Server does not instantiate client-only presentation assets.

## Current implementation status

`EROProceduralWorldGenerator.cs` is the deterministic layout foundation. `EROAAAWorldGenerationProfile.cs` is the configurable AAA rule profile. The existing generator still uses primitive placeholders for layout visualization; final production dressing should be supplied by approved/licensed prefabs and ERO-original content.

The architecture is intentionally designed so that improving the art pipeline does not require throwing away the world graph.
