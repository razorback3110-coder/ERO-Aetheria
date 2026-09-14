# Eternal Realms Online (ERO) — Master Systems Roadmap V7

> Consolidated implementation contract for the ERO MMORPG. Target Unity: **6000.0.67f1**.

## 1. Product pillars

- Persistent MMORPG progression with a launch cap of **250**, designed to rise beyond 250 in future updates.
- Server-authoritative validation for progression, inventory, economy, loot, rewards, rankings, events and anti-abuse systems.
- Build freedom without advertising a universal “best class”.
- FR/EN localization from the architecture, extensible to additional languages.
- Original/legal assets only; no ripped proprietary assets.
- Weekly and monthly activity loops without deleting permanent character progression.

## 2. Classes and combat

Seven base classes:

1. Paladin — Tank
2. Priest — Heal
3. Invocateur — Magic DPS
4. Mage — Magic DPS
5. Assassin — Physical DPS
6. Archer — Physical DPS
7. Guerrier — Physical DPS

Class role is fixed, while stats, skills, passives, talents, equipment, PET and Forge allow deep customization. Each class targets roughly 10–15 active skills plus equippable/activatable passives. Skills are never lost.

### Invocateur rules

- Core skills include Chain Lightning (AOE + DoT), Frost Prison (AOE + Burst + Control) and Void Lance (single target + Burst).
- Invocateur has direct combat skills and does not rely exclusively on summons.
- Three Pact families: DPS, Tank, Heal.
- Pact defines invocation function, not the player's class role.
- Powerful invocation use is constrained by Command Points.
- Invocations evolve through appearance, AI, skills and synergies.
- PETs remain separate from class invocations.

## 3. Class progression

Grades: Novice I–V, Apprenti I–V, Adepte I–V, Expert I–V, Maître I–V, Grand Maître I–V, Maître Suprême.

Evolution tickets: Green, Blue, Orange, Red. They are obtained through the Guild Shop (guild level 3+) and Towers. Maximum **4 evolution tickets/week/player**. No class-evolution packs.

## 4. Character progression

- Launch cap: 250; future updates may increase it.
- Overflow XP is retained for later cap increases.
- Content unlocks progressively so players always have a next objective.
- Level 159 is a major Unique Set/endgame milestone, not the final level cap.

## 5. Equipment and Forge

Quality ladder: Common → Uncommon → Rare → Epic → Legendary → Mythic → Unique.

- Legendary is reasonably obtainable, including early game.
- Mythic is rarer and equippable from level 40.
- Unique Set is late/endgame.
- Sets can grant bonuses at thresholds such as 2/4/6 pieces.
- Appearance can be separated from stats through transmog/appearance systems.
- Gear Score is distinct from ranked rating.
- Forge supports creation, reinforcement, improvement and controlled stat reforge with server validation.

## 6. Inventory and persistence

Inventory categories include: All, Equipment, Consumables, Materials, Quests, Cards, Costume/Appearance, PET, Miscellaneous and Premium.

Required behaviors: search, sorting, capacity/weight, locking, protected deletion, item details, right-click/double-click, drag-drop, direct equip, enhancement and card management.

Quest/important items are protected from accidental deletion. Inventory is connected to loot, rewards, market, Forge and persistence.

## 7. PET system

Exactly **25 PETs**, each with **5 evolutions (I–V)** = 125 evolution forms.

Roles: Tank, Physical DPS, Magic DPS, Heal, Support, Hybrid.

- 6 PETs are SSR.
- Only SSR PETs obtain humanoid form at evolution V.
- PET Invocation Tickets are earned through content, notably Tower milestones every 10 floors.
- Control modes: Auto / Semi-auto / Manual, with no stat advantage for Manual.
- Target-priority system.
- PET packs may include welcome, small, medium, large, luxury and Founder Pack content.
- Pity/protection should prevent unacceptable streaks of bad luck.

## 8. Five Towers — permanent monthly loop

Exactly **5 Towers**, 100% solo: one player + PET, no party matchmaking.

### Monthly reset contract

- The five Tower progressions reset monthly.
- Every floor has a reward.
- Important milestones have stronger rewards.
- Reward tables change monthly.
- Monthly leaderboards reset.
- Historical records remain in the Hall of Fame.
- Bosses, modifiers and special challenges may rotate.
- Character level, equipment, Forge, PETs, cards, titles, mounts, Codex and other permanent progression are **never reset**.

Tower rewards can include PET Invocation Tickets, class evolution tickets/resources and other controlled progression rewards. There is no normal Tower shop.

## 9. Three Season Pass systems

Three reusable activity-based Season Pass types:

1. **Season Pass — Aventurier**: quests, monsters, dungeons, MVPs, exploration, events, guild activity, PvP, collections and crafting.
2. **Season Pass — Maître des Tours**: Tower floors, records, all five Towers, special challenges, Tower bosses and PET objectives.
3. **Season Pass — Maître de l’Arène**: PvP wins, competitive objectives, rankings, guild PvP and competitive challenges.

Each pass has Free and Premium tracks. Season Passes are not required to reach the level cap, unlock classes or remain competitive. The creator account entitlement `FOUNDER_CREATOR` may activate Premium tracks for testing.

## 10. Economy

Premium currency: **Cristaux ERO**. Exchange rule: **1 Cristal ERO = 1,000 Credits**, one-way only.

Credits sources include quests, dungeons, MVPs, Towers, events, guild activity and player trading/sales. Activity currencies remain separated:

- Guild Tokens → Guild Shop
- Arena Tokens → Arena Shop
- Dungeon Stones → Dungeon Shop
- MVP Tokens → MVP Shop
- Event Tokens → Event Shop
- Forge materials → Forge
- Towers → direct rewards
- Archangel Board → Credits

Player market includes taxes, anti-inflation, anti-bot, transaction history, large-trade confirmation and server validation.

### Archangel Board

Mythic equipment market with up to 10 simultaneous offers and controlled supply injection. Offers refresh approximately over time and prices scale with item value/level. The board uses Credits and is accessible to free players through normal Credit earning.

## 11. Guilds

Guilds include progression, Guild Tokens, Guild Shop, class evolution tickets, ranks/permissions, guild activities and future territory warfare.

No class evolution packs.

## 12. World and exploration

- Day/night cycle.
- Weather systems.
- Time-based events.
- Rare spawns.
- Evolving regions.
- Regional/faction reputation.
- Hidden caves, passages, chests, NPCs, quests, bosses, events and puzzles.
- Mounts: terrestrial, terrain-specific, aquatic and permitted flying mounts.
- Titles and achievements.
- Choice-driven quests with warnings before essential-content consequences.
- Advanced status effects and elemental reactions.
- Hidden achievements.
- Mentorship.
- Variable merchant inventories and traveling/mysterious merchants.
- Bounties and hunting contracts with anti-abuse controls.
- Boss AI with phases, target priorities, positioning/behavior reactions and controlled variability; no cheating.
- Codex integration across discovery systems.

## 13. New world-life systems V7

### Global events

Invasions, ancient kingdoms, elemental disasters, special MVPs, World Bosses, treasure events, faction events and community events. Rewards can include quests, titles, cosmetics, Codex entries and event currencies.

### Evolving MVPs

MVPs can have controlled variants such as normal, reinforced, corrupted and legendary states. Variants change mechanics, phases and behavior without artificial/impossible stat inflation.

### Expeditions

Structured exploration activities for ruins, islands, temples, abandoned villages, hidden routes, rare NPCs, puzzles and mini-bosses.

### Variable dungeons

Dungeon runs can use controlled environmental/mechanical variants: darkness, fog, cold, electrified enemies, alternate paths, extra bosses and special events.

### Card collection

Cards cover monsters, MVPs, bosses, regions, dungeons, events and rare discoveries. Collection progress feeds the Codex and can grant collection rewards without making every card mandatory for competitive power.

### Secret achievements

Hidden conditions can involve weather, time, NPCs, no-hit boss fights, elemental reactions, merchants or secret areas. Rewards focus on titles, cosmetics, emotes, Codex and prestige.

## 14. Hall of Fame

Permanent historical archive for major firsts and records: first level milestones, first Tower clears, first World Boss kills, first Unique items, guild records and historical activity records. Monthly leaderboards can reset without deleting historical records.

## 15. Housing

Personal domains with house, storage, Forge/atelier, equipment mannequins, PET displays, MVP/Tower trophies, decorations and gardens. Housing is social/collection focused and not a mandatory power system.

## 16. PET Sanctuary

A dedicated gallery for the 25 PETs and 125 evolution forms, showing ownership, evolution history, appearances and collection progress. Collection rewards remain controlled and separate from combat balance.

## 17. Weekly challenges

Rotating challenges can cover dungeons, MVPs, exploration, PvP, Forge, collection and secrets. They can feed the appropriate Season Pass but should not make every activity mandatory.

## 18. Class Mastery

Each class has mastery paths independent of character level. Examples:

- Paladin: Forteresse, Protection, Jugement, Vengeance.
- Invocateur: Pacte, Invocation, Contrôle, Maîtrise.
- Assassin: Ombre, Critique, Poison, Exécution.

Mastery rewards practice and class-specific objectives and can unlock controlled choices, prestige, cosmetics, Codex knowledge and balanced optimization.

## 19. Secret ERO events

Some world events are discovered through clues instead of direct announcements. Example chain: unusual NPC dialogue → environmental change → conditional door → secret boss/event. First discoverers can receive prestige rewards and permanent Codex/achievement records.

## 20. Cities and social layer

Planned core MMO services:

- Living cities with schedules, shops, taverns, inns, banks, auction/market services and social areas.
- Friends, groups, group finder, block list, private messages, emotes, player interactions and gifts.
- Personal bank and guild bank with permissions and transaction history.
- In-game mail for messages and supported item/currency transfers.

## 21. Professions and secondary activities

Professions:

- Blacksmith
- Alchemist
- Cook
- Equipment crafter
- Tailor
- Jeweler

Secondary activities may include fishing, cooking, hunting, gathering, mining, botany, treasure hunting, creature collection and mount races. These should provide alternative progression and economy loops without mandatory power gating.

## 22. Guild territories and warfare

Future guild endgame includes territory ownership, fortifications, sieges, attacks, defenses, territory objectives and rewards. Anti-monopoly rules must prevent a single guild from permanently controlling essential content.

## 23. Dynamic regions

Player actions can change the state of a region: city defense failures, monster occupation, new NPCs, new quests, new bosses and later reconquest. Essential progression must remain recoverable.

## 24. World Bosses

World Bosses are large-scale encounters distinct from standard MVPs. They can involve many players/guilds, multi-phase objectives, contribution scoring and participation rewards. Contribution systems must avoid permanent monopolization.

## 25. Dynamic world map

Map UI can expose current events, bosses, invasions, rare merchants, dungeons, bounties, guild territories, resources, discoveries, weather and regional states. Some information can remain hidden to preserve discovery.

## 26. Evolving story

ERO's main story is chapter-based and can react to selected player/world outcomes. Consequences should enrich the world without permanently locking essential content.

## 27. Character presentation

Advanced character creator, equipment display, transmog/appearance collection, mounts, emotes and social presentation. Appearance systems should be data-driven so additional cosmetics can be added without changing core progression.

## 28. Technical architecture

Target architecture:

`Gateway → World Coordinator → Zones/Instances → PvP → Chat/Social → Persistence`

Target logical capacity from the broader design: approximately 1,000 concurrent players/server architecture, subject to profiling and production validation.

Critical server-authoritative domains:

- Inventory
- Economy
- Loot
- Progression
- Rewards
- Rankings
- Events
- Bounties
- Guild ownership/permissions
- Anti-abuse

Unity target: **6000.0.67f1**.

## 29. Security, moderation and GM tools

Required future service layer:

- anti-bot and anti-exploit checks
- suspicious transaction monitoring
- reward validation
- moderation tools
- player reports
- mute/ban/suspension controls
- GM diagnostics
- audit logs
- rollback/recovery procedures
- server-side creator/admin entitlement for `FOUNDER_CREATOR`

Creator entitlement is never transferable or duplicable and exists for testing premium/seasonal features without payment. Normal players remain subject to normal commercial rules.

## 30. Discord/community integration

Community tooling should support announcements, events, guides, videos, builds, class information and guild utilities. The game architecture remains independent from Discord so core gameplay cannot depend on an external community platform.

## 31. Content cadence

**Weekly:** rotating challenges, selected activities and community objectives.

**Monthly:** five Tower resets, new floor reward tables, new monthly Tower leaderboards, rotating Tower modifiers/boss challenges and refreshed seasonal activity goals.

**Long term:** level-cap expansions, new zones, dungeons, MVPs, PETs, equipment, class grades, Towers II–V depth, PvP/endgame and story chapters.

## 32. Non-negotiable design rules

- No ripped/proprietary game assets.
- No class-evolution packs.
- No mandatory Season Pass for power progression.
- No manual-PET stat advantage.
- No fundamental role swapping that invalidates class identity.
- No artificial boss cheating or impossible fights.
- No monthly reset of permanent character progression.
- No permanent guild monopoly over essential content.
- No hidden player-facing “best class” ranking.
- Every major economy/reward/progression operation must be server validated.

## 33. Implementation order

1. Stabilize current Unity project and CI.
2. Establish data-driven domain models and server contracts.
3. Build character/classes/stats/skills/passives/talents.
4. Build inventory/equipment/Forge/loot/persistence.
5. Build PETs and invocation architecture.
6. Build world/zones/navigation/NPCs/quests/reputation/Codex.
7. Build dungeons/MVPs/Towers and monthly reset framework.
8. Build economy/market/guilds/rankings/PvP.
9. Build UI: character, inventory, gear, skills, PET, map, Codex, guild, market, Season Pass.
10. Add social, professions, housing and secondary activities.
11. Add dynamic events, secret content, World Bosses and evolving regions/story.
12. Add moderation/GM/security tooling and production observability.
13. Validate performance, persistence, exploit resistance and recovery before public testing.

## 34. Definition of “complete”

ERO is not considered feature-complete until the permanent progression loop, monthly Tower loop, three Season Pass systems, seven classes, PET system, equipment/Forge, inventory, economy, guilds, PvP, PvE, world simulation, exploration, social systems, crafting, housing, Codex, events, secrets, rankings, moderation and server-authoritative persistence are represented in implementation and validated by automated/manual tests.
