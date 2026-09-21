# ERO Asset & License Registry

This registry is the legal gate for third-party content used by Eternal Realms Online.

## Policy

- Only assets with a verified license permitting use in a commercial game may enter `APPROVED`.
- CC0/public-domain assets are preferred.
- Never copy or ship proprietary assets, maps, UI, characters, sounds, music, animations or code from commercial games without an explicit redistribution/commercial license.
- Do not redistribute third-party assets as a standalone asset pack.
- Keep the original license/source URL with the project so the release can be audited.

## ERO-authored runtime content

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `EROProceduralFantasyArt.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Procedural terrain, vegetation, crystals and creature foundation |
| `EROVisualQualityBootstrap.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Runtime lighting, fog, shadows, HDR camera and environment presentation |
| `EROPerformanceBootstrap.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Runtime frame pacing and texture-streaming baseline for playable world streaming |
| `EROWorldChunkStreaming.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic procedural world chunks, distance-based loading/unloading and scalable world dressing |
| `EROCombatDeterminism.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic combat rolls for authoritative server simulation, replays and rollback-safe execution |
| `EROSimulationClock.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Fixed-step authoritative simulation ticks for server networking and deterministic gameplay |
| `EROAOIInterestSystem.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Allocation-aware deterministic interest management for network replication |
| `EROSimulationDriver.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Unity host for fixed-step simulation ticks; future dedicated-server simulation entry point |
| `EROCombatCommandBuffer.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Bounded deterministic server-side combat input queue; protects server memory from command floods |
| `EROCombatSimulationBridge.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Bridges queued combat commands into authoritative fixed-step simulation |
| `EROCombatResolution.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Pure deterministic hit, critical, mitigation, damage and defeat resolution for authoritative combat |
| `EROCombatStateStore.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Authoritative combatant/skill state and application of deterministic combat results |
| `EROCombatRewards.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Idempotent deterministic XP and loot outcomes after authoritative combat defeat |
| `EROEncounterCombatService.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Connects authoritative combat defeats to persistent XP and loot rewards without Unity dependencies |
| `EROCharacterProgression.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Overflow-safe authoritative XP accumulation and level progression for persistent characters |
| `EROClassProgression.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-safe eight-class progression rules with class selection at 18, first evolution at 40, second evolution at 75 and three skills per tier |
| `EROWallet.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative idempotent currency balances with versioned persistence snapshots |
| `EROCurrencyCatalog.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Canonical server-side currency identifiers and bounded balance rules for Gold and ERO Crystals |
| `EROInventory.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative bounded item storage with operation-bound transaction idempotency and persistence snapshots |
| `EROItemInstance.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Persistent identity, stack limits, level and stat payload for unique/equipment loot |
| `EROInstanceInventory.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative storage and persistence of concrete item instances with transaction-bound ownership changes |
| `EROLootGenerator.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic server-side generation of persistent item instances from authoritative encounter seeds |
| `EROLootInventoryService.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Authoritative bridge that grants deterministic loot instances into persistent player inventory with retry-safe transaction identity |
| `EROEquipmentLoadout.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative equipment slots referencing persistent owned item instances and aggregating equipment stats |
| `EROCharacterCombatStats.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative character base-stat persistence and deterministic aggregation of equipped item bonuses for combat |
| `EROCombatStatsResolver.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Converts authoritative base/equipment stats into deterministic attack, defense, health and critical combat inputs |
| `EROAuthoritativeCombatService.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Applies resolved equipment-derived combat inputs to authoritative health state, prevents damage to already defeated actors and persists health snapshots |
| `EROVerticalSliceRuntime.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Temporary visible vertical-slice runtime using procedural primitives only; production art remains subject to the approved asset pipeline |
| `EROPlayablePersistence.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Local playable-slice persistence bridge for safe avatar position snapshots while authoritative server persistence is being integrated |
| `EROPlayableLootInventory.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Dependency-free visible loot, inventory and gear-score presentation for the playable slice |
| `EROMvpWorldEvent.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Playable Aetheria MVP world event, respawn timer and presentation-side boss encounter |
| `EROV8MVPAuthority.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative network MVP level, safe exponential HP/damage scaling, rate limiting and atomic one-hour respawn persistence across dedicated-server shutdown/restart |
| `EROMvpEncounterRules.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic MVP level scaling, 250,000 base HP, one-hour respawn contract and bounded XP/currency rewards for server and client-slice parity |
| `EROHeadlessServerBootstrap.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic dedicated-server bootstrap for `-ero-server`, disabling client presentation and starting the Netcode server |
| `EROBuildAutomation.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | CI-only Unity build/compile validation, exact Unity version/revision gate, playable-scene and legal-registry checks |

These entries contain no third-party asset payloads and are authored for ERO.

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
| Kenney Retro Fantasy Kit | https://kenney.nl/assets/retro-fantasy-kit | CC0 | Yes | Not required | Supplementary fantasy architecture/props |
| Kenney CC0 asset library | https://kenney.nl/support | CC0 | Yes | Not required | UI/props/environment where stylistically appropriate |

## APPROVED — texture references

| Asset | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| Kenney Road Textures | https://kenney.nl/assets/road-textures | CC0 | Yes | Not required | Road/ground texture support where visually appropriate |
| Kenney Retro Textures Fantasy | https://kenney.nl/assets/retro-textures-fantasy | CC0 | Yes | Not required | Optional fantasy texture accents where consistent with ERO style |

## APPROVED — rules

Quaternius public pack pages currently identify the listed packs as CC0 and explicitly permit personal and commercial use. Kenney states that game assets on its asset pages are public-domain CC0 and may be used in commercial projects. The project must still preserve the exact source URL and verify the current terms before each commercial release.

## Integration quality gate

Legal approval alone does not make an asset production-ready. Production assets must also satisfy `Assets/ERO/ArtDirection/ERO_VISUAL_PRODUCTION_STANDARD.md`: textured/material treatment, collision, LOD/optimization and scene integration must be addressed. Unity primitives remain debug/prototype-only and are not acceptable as final visual assets.

## REVIEW

Any new source must be added here before it is referenced by production generation rules. Record the exact pack name, source URL, license/version, verification date and any provider-specific restrictions.

## REJECTED

- Ragnarok Online / Ragnarok: The New World original assets, maps, UI, characters, sounds, music, sprites, models or extracted files.
- Lost Ark original assets or maps.
- Throne and Liberty original assets or maps.
- Any ripped/private-server assets whose redistribution rights cannot be proven.

## Release rule

A production build is considered legally ready only when every non-ERO asset referenced by the build is either CC0/public-domain or covered by an explicit commercial redistribution license, and the corresponding registry entry is present.

| `Resources/ERO/PlayerGraphics_Mage_Boy.prefab` | Existing ERO character graphics asset | Project-owned/approved ERO asset | Yes | Existing asset | Relocated into Resources solely for runtime vertical-slice loading; no external content added |
| `Resources/ERO/ImpGraphics.prefab` | Existing ERO enemy graphics asset | Project-owned/approved ERO asset | Yes | Existing asset | Relocated into Resources solely for runtime vertical-slice loading |
| `Resources/ERO/VandalImpGraphics.prefab` | Existing ERO enemy graphics asset | Project-owned/approved ERO asset | Yes | Existing asset | Relocated into Resources solely for runtime vertical-slice loading |

## ERO-authored Unreal runtime foundation

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| Unreal project/module foundation under `Unreal/` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Unreal Engine 5.8 runtime foundation, server-authoritative MMORPG architecture and migration target |
| `EROGameMode` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Unreal multiplayer game-mode foundation |
| `EROPlayerCharacter` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Replicated playable character foundation with eight-class identity, health/level state, third-person camera and movement input |
| `EROPlayerCharacter` combat extension | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative primary attack, bounded cooldown/range, pawn sweep damage application and replicated health state |
| `EROPlayerCharacter` progression and respawn extension | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Server-authoritative eight-class stat profiles, level/XP progression, level-18 class selection gate and automatic post-defeat respawn loop |
| `DefaultInput.ini` attack mapping | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Primary left-mouse combat input for the Unreal playable slice |
| `EROAetheria.Build.cs` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Unreal module dependencies for networking, Enhanced Input, Gameplay Ability System and Mass |

## ERO Unreal visual-generation tooling

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| Unreal/Scripts/GenerateAetheriaVisualWorld.py | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic UE5 editor generator for a partitioned Aetheria visual scaffold, biome layout, materials, lighting and POIs |

The generator creates only Unreal Engine built-in primitive geometry and ERO-authored material/layout data. It does not embed third-party assets. Production third-party content must still pass the registry gate above.

| `Unreal/Config/AetheriaWorldManifest.json` | ERO-Aetheria source data | ERO-owned/original | Yes | Not required | Canonical machine-readable definition of the complete Aetheria launch-world region layout and content nodes |
| `Unreal/Scripts/GenerateAetheriaVisualWorld.py` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Complete deterministic UE5 World Partition generator for the Aetheria launch-world scaffold |

## ERO Unreal world architecture tooling

| Content | Source | License | Commercial game | Attribution | ERO use |
|---|---|---|---|---|---|
| `Unreal/Scripts/GenerateAetheriaWorld.py` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic UE5.8 generator for the single persistent Aetheria world containing eleven connected cities |
| `Unreal/Scripts/GenerateAetheriaInstanceMaps.py` | ERO-Aetheria source code | ERO-owned/original | Yes | Not required | Deterministic UE5.8 generator for separate dungeon, tower, PvP and GvG map shells |
| `Unreal/Config/AetheriaMVPManifest.json` | ERO-Aetheria source data | ERO-owned/original | Yes | Not required | Canonical definitions for 22 Aetheria MVPs, their levels, spawn radius, loot tables and unique necklace cosmetics |
| `Unreal/Config/AetheriaCityAchievements.json` | ERO-Aetheria source data | ERO-owned/original | Yes | Not required | Canonical city-specific achievement totals and completion milestones |
| `Unreal/Config/AetheriaInstanceManifest.json` | ERO-Aetheria source data | ERO-owned/original | Yes | Not required | Canonical separate-map definitions for dungeons, Towers, PvP 1v1/4v4 and GvG |

The eleven-city world uses only Unreal Engine built-in primitive geometry and ERO-authored materials in its generated scaffold. No third-party asset payload is embedded by these generators.

### ERO-authored class visual pipeline
- `Unreal/Source/EROAetheria/EROCharacterAppearanceTypes.h` — ERO-owned data types for class silhouettes, armor styles and weapon specializations; commercially usable as original project code.
- `Unreal/Source/EROAetheria/EROClassAppearanceDefinition.h` — ERO-owned data asset contract for class body meshes, outfits, weapon assets and evolution appearances.
- `Unreal/Config/AetheriaClassAppearanceManifest.json` — ERO-owned class modeling/specification manifest; contains no third-party asset payload.

## ERO character presentation bootstrap — 2026-09-21

- `Unreal/Source/EROAetheria/EROCharacterVisualTypes.h` — **ERO-owned/original** gameplay presentation data types and weapon specialization enums.
- `Unreal/Source/EROAetheria/EROCharacterVisualDefinition.h` — **ERO-owned/original** data-driven character visual definition contract.
- `Unreal/Config/EROCharacterClassVisualManifest.json` — **ERO-owned/original** eight-class outfit, weapon and specialization manifest.
- `Unreal/Scripts/GenerateEROCharacterPrototypeBlender.py` — **ERO-owned/original** procedural prototype modeling script; generates only original primitive geometry and must not be treated as final AAA character art.

## ERO Unreal PvE combat foundation — 2026-09-21

- `Unreal/Source/EROAetheria/EROCombatRules.h` — **ERO-owned/original** deterministic server-side weapon-family combat modifiers for damage, range and cooldown.
- `Unreal/Source/EROAetheria/EROEnemyActor.h` — **ERO-owned/original** replicated PvE enemy contract with authoritative health, level, rewards and respawn configuration.
- `Unreal/Source/EROAetheria/EROEnemyActor.cpp` — **ERO-owned/original** server-authoritative PvE damage, defeat, XP reward and respawn implementation.

These files contain no third-party asset payload. They provide the first Unreal PvE opponent foundation for the playable combat loop.
