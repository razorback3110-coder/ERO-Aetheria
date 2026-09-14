# ERO — MASTER HANDOFF FOR CLAUDE CODE

## Project
- Name: Eternal Realms Online (ERO)
- Repository: razorback3110-coder/ERO-Aetheria
- Branch: main
- Engine: Unity 6
- Required Unity version: 6000.0.67f1
- Primary platform: Windows PC
- Genre: original fantasy MMORPG

## ABSOLUTE RULE
ERO must remain the complete MMORPG in development. Never replace it with a disposable demo, temporary scene, parallel prototype, or reduced build. Preserve existing systems. Add and integrate; do not delete functionality merely to make compilation easier.

Before changing anything: inspect existing code, scenes, assets, packages and references. Reuse existing systems. Correct bugs instead of bypassing them. Compile and test after important changes.

## Core vision
Player flow: launcher -> login -> character selection/creation -> Aetheria world -> exploration -> combat -> quests -> loot -> equipment -> progression -> party -> guild -> dungeons/raids/MVP -> PvP/GvG -> endgame.

Target quality: high-end/AAA-inspired fantasy MMORPG presentation: detailed characters, armor, weapons, environments, PBR materials, animation, lighting, VFX, audio, UI, weather and day/night. AAA means production quality and coherence; do not claim third-party low-detail assets are automatically AAA.

## Current classes — preserve all
1. Warrior
2. Mage
3. Archer
4. Assassin
5. Priest
6. Paladin
7. Summoner
8. Bard
9. Alchemist
10. Dragon Knight
11. Gunner
12. Arcanyste

Every class needs distinct gameplay, stats, skills, equipment, animations, VFX, PvE/PvP/GvG builds and endgame relevance.

## Character
- male/female
- face, skin, hair, eyes, colors, accessories
- voice and appearance customization
- equipment appearance
- persistent character data
- level, XP, stats, skills, inventory, equipment, quests, currencies, titles, mounts, pets, guild

## Progression
Initial target: level 1-100.
Stats should support HP, MP, ATK, MATK, DEF, MDEF, speed, crit, accuracy, evasion, penetration, resistances, elemental damage, PvE/PvP modifiers.

## Combat
- targeting
- auto attacks
- skills/cooldowns/resources
- damage and mitigation
- elements
- buffs/debuffs/DoT/CC
- aggro/threat
- death/respawn
- loot
- animation, hit effects, projectiles, trails, particles, camera feedback and SFX

## Skills
Data-driven skill definitions: ID, name, class, level requirement, cost, cooldown, range, animation, VFX, SFX, damage, type, element, effects, FR/EN text.

## Hotbar
Multiple bars, keyboard shortcuts, drag/drop, icons, cooldown visualization, skills, consumables and potions.

## Inventory
Original ERO UI with tabs: Equipment, Consumables, Resources, Quests, Materials, Misc, Special. Include stacking, sorting, search, filters, tooltips, drag/drop, destruction, locking and equipment comparison.

## Equipment
Weapon, head, armor, gloves, legs, boots, accessories, cape and extensible slots. Include rarity, stats, level, enhancement, sets, bonuses, enchantments, sockets, gems and appearance. Support advanced/royal/legendary sets.

## World — Aetheria
- capital and villages
- plains, forests, mountains, snow, desert, swamp
- ruins, corrupted and magical zones
- roads, POIs, teleporters, secrets
- dungeons, raids, PvP zones, boss zones
- world streaming/chunking and optimization

## Art assets
Create/integrate original or legally usable assets for:
- player characters, NPCs, faces, hair, armor, weapons, accessories
- mobs, elites, rares, bosses, MVPs, pets
- cities, villages, houses, castles, taverns, shops, banks, markets, temples, guild halls, portals, ruins
- trees, plants, rocks, grass, water, snow, sand and biome dressing
- props: chests, furniture, lamps, tools, potions, signs, market goods
- animations, materials, textures, VFX, UI and audio

## Boss/MVP/AI
Bosses need phases, telegraphed attacks, mechanics, enrage, rewards and respawn. AI needs detection, aggro, pursuit, attacks, abilities, flee, return-to-spawn, patrol and type-specific behavior.

## Quests
Main, side, class, guild, event, daily, weekly and dynamic quests. Journal must track objectives, progression, rewards, locations, markers and FR/EN text.

## Multiplayer content
Party: invites, accept, kick, leader, roles, XP/loot rules, group state.
Guild: creation, name, emblem, ranks, permissions, members, recruitment, XP/level, storage, guild hall, announcements, logs. Test guild: Patetik. Community: FR/EN.
GvG: territories, objectives, sieges, points, ranking, rewards.
PvP: duel, arena, open PvP, battleground, GvG, ranking, seasons and rewards.

## Dungeons / raids / MVP
Support solo/group dungeons, difficulty, matchmaking, boss and rewards; raids with multi-player phases/mechanics; world MVP spawn/timers/location/contribution/loot/ranking/events.

## Crafting / economy / market
Crafting: gathering, materials, recipes, forge, alchemy, upgrades, equipment and consumables.
Normal currency: Gold.
Premium currency: Cristaux ERO.
Premium should focus on cosmetics, mounts, pets and services; avoid destructive pay-to-win.
Market: buy/sell, search, filters, price, history, expiry, fees and server-authoritative transactions.

## Mounts / pets
Ground mounts; flying mounts if world architecture supports them; speed and skins. Pets may be cosmetic and/or combat according to final design, with levels, bonuses and customization.

## UI
Premium original ERO fantasy UI. Screens: launcher, login, character select/create, HUD, inventory, equipment, skills, quests, map, party, guild, market, crafting, shop, settings, social, notifications.

## Localization
Minimum languages: French and English. Do not hardcode player-facing text. Use localization keys and FR/EN data.

## Audio
Menu, city, exploration, combat, boss, dungeon music; ambience; environment; UI; attacks; impacts; skills; monsters; bosses. Verify licenses.

## Day/night/weather
Sun, moon, sky, lighting and transitions. Weather architecture for rain, storm, fog, snow and wind.

## Networking/server
Target architecture:
CLIENT -> NETWORKING -> AUTHORITATIVE GAME SERVER -> PERSISTENCE/DATABASE

Server must control damage, inventory, currency, equipment, XP, rewards, transactions and other critical state. Preserve/build login, sessions, player registry, authoritative tick, AOI, replication, synchronization, disconnect/reconnect and persistence.

## Persistence
Save account, character, level, XP, stats, inventory, equipment, skills, quests, currencies, guild, mounts, pets and progression. Protect against duplication and data loss.

## Discord
ERO/Patetik community integration: announcements, events, guides, builds, videos, recruitment, server status, patch notes and notifications. FR/EN.

## Launcher
Real ERO Launcher, not a separate demo. Features: login, news, patch notes, file verification, download/update, progress, FR/EN, server status, PLAY, repair and logs. It launches the real ERO client.

## Build / CI
Current playable workflow: `.github/workflows/ero-playable.yml`.
Runner labels: `[self-hosted, Windows, X64]`.
Build target: `StandaloneWindows64`.
Expected output: `Builds/Windows/ERO.exe`.
Required Unity: `6000.0.67f1`.
Pipeline goal: commit -> GitHub Actions -> Unity -> compile -> Windows build -> artifact.

## Legal asset policy
Reference: `Assets/ERO/Legal/ERO_Asset_License_Registry.md`.
Prefer CC0/public-domain or explicitly commercial licenses. Record source/license before production integration. Never use ripped/proprietary assets from Ragnarok, Ragnarok: The New World, Lost Ark, Throne and Liberty, private servers, or any source without redistribution rights.

Production asset pipeline:
SOURCE -> LICENSE -> REGISTRY -> IMPORT -> MATERIAL/TEXTURE -> PREFAB -> COLLISION -> LOD -> OPTIMIZATION -> SCENE -> QA

Unity primitives are debug/prototype only, not final production art.

## Recommended folders
Assets/ERO/Core
Assets/ERO/Character
Assets/ERO/Classes
Assets/ERO/Combat
Assets/ERO/Skills
Assets/ERO/Enemies
Assets/ERO/Boss
Assets/ERO/World
Assets/ERO/Quests
Assets/ERO/Inventory
Assets/ERO/Equipment
Assets/ERO/Crafting
Assets/ERO/Economy
Assets/ERO/Marketplace
Assets/ERO/Party
Assets/ERO/Guild
Assets/ERO/PvP
Assets/ERO/GvG
Assets/ERO/Dungeons
Assets/ERO/Raids
Assets/ERO/MVP
Assets/ERO/Mounts
Assets/ERO/Pets
Assets/ERO/UI
Assets/ERO/Localization
Assets/ERO/Audio
Assets/ERO/VFX
Assets/ERO/Networking
Assets/ERO/Server
Assets/ERO/Launcher
Assets/ERO/Art
Assets/ERO/Legal
Assets/ERO/QA

Do not mass-move/rename existing files without checking Unity references and C# dependencies.

## Data-driven architecture
Use ScriptableObjects/configuration where appropriate for classes, skills, items, equipment, monsters, bosses, quests, recipes, loot tables, regions, buffs/effects, localization and VFX.

## Current base / audit requirement
The existing project already contains substantial ERO engineering around world, classes, progression, stats, combat, targeting, AI, elements/statuses, skills/hotbars, loot, VFX, day/night, inventory, equipment, skill tree, quests, party, guild/GvG, market, crafting, economy, pets, dungeons, raids, MVP, events, map, settings, save systems, audio and networking. Treat code as existing foundation, but verify what is actually connected, compiling and tested.

## Definition of DONE
A feature is done only when code exists, it is integrated, data is connected, Unity compiles, it is testable, it does not break related systems, networking is handled when required, localization is handled when required, and visible features have their necessary UI/assets/VFX/audio.

## Priority backlog
P0: full repository audit; C# compile errors; Unity errors; broken references; packages; scenes; build pipeline; produce and launch Windows build.
P1: integrated player flow, character creation, spawn, movement, camera, combat, mobs, loot, inventory, equipment, skills, quests, progression, save and HUD.
P2: production art pass — characters, armor, weapons, monsters, environments, animations, VFX, UI, audio, lighting, weather, day/night.
P3: MMO — authentication, server connection, replication, AOI, persistence, party, guild, market, economy, PvP, GvG, dungeons, raids, MVP, events.
P4: optimization, LOD, occlusion, streaming, memory, network, security, logs, crash handling, QA, load testing.
P5: final launcher, patcher, differential updates, repair, authentication, news, server status, release packaging and QA.

## Work order
AUDIT -> COMPILE FIXES -> CORE -> PLAYER -> WORLD -> COMBAT -> CLASSES -> SKILLS -> MOBS/BOSS -> LOOT -> INVENTORY -> EQUIPMENT -> QUESTS -> UI -> NETWORK -> SERVER -> PERSISTENCE -> PARTY -> GUILD -> PVP/GVG -> DUNGEONS/RAIDS/MVP -> ECONOMY/MARKET -> CRAFTING -> MOUNTS/PETS -> LOCALIZATION -> AUDIO -> VFX -> AAA ART PASS -> OPTIMIZATION -> LAUNCHER -> CI/CD -> QA -> RELEASE

This is a dependency guide, not permission to delete future systems.

## First mission for Claude Code
Immediately audit the entire repository and produce an `EXISTS / PARTIAL / BROKEN / MISSING / FINAL` matrix for the systems above. Then fix blockers without deleting systems, compile in Unity 6000.0.67f1, stabilize the Windows build, and continue implementing missing integrated systems and production assets.

**Final instruction: build Eternal Realms Online as the complete game. Do not create a disposable demo. Preserve what exists, integrate what is missing, verify every change, and keep the project moving toward the full MMORPG.**
