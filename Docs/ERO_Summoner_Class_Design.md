# ERO — Summoner Class Design

## Identity
- Class: `Summoner`
- Automatic role: `Magic DPS`
- The class itself can attack and cast its own skills. Summons are an extension of the build, not a replacement for the player.
- PETs remain separate from class summons and can run in Auto, Semi-Auto, or Manual mode.

## Pact rule
Pacts are strictly grouped by role to prevent the Summoner from being simultaneously the best damage dealer, tank and healer.

### Pact families
- DPS Pacts: elemental/arcane damage themes such as Flame, Storm, Shadow, Frost, Necrosis and Astral.
- Tank Pacts: Guardian/Titan/Ancient-style summons focused on threat, mitigation and protection.
- Heal Pacts: Nature/Light/Source-style summons focused on direct healing, healing-over-time and support.

A build equips one **Primary Pact** and up to two **Secondary Pacts**. The Primary Pact gets the strongest progression and signature skill. Secondary Pacts provide limited complementary utility. A Summoner cannot activate unlimited Pacts or unlimited major summons at once.

## Paired skill architecture
ERO classes use skills in pairs. For Summoner, each pair has a clear purpose and prevents role overlap.

### Pair 1 — Summoner Core (player-controlled DPS)
Choose one of two active skills:
- `Arcane Bolt`: reliable single-target magic damage with good resource efficiency.
- `Rift Nova`: short-range area burst around the Summoner.

This guarantees that the Summoner remains an active combatant.

### Pair 2 — Pact Command
Choose one command skill:
- `Pact Assault`: commands the active summon to immediately use its offensive signature; strongest with DPS Pacts.
- `Pact Guard`: commands a Tank/Heal summon to prioritize protection or support.

Only the command compatible with the equipped Pact is enabled; incompatible commands do not change the Pact's role.

### Pair 3 — Specialization
Choose one specialization skill for the active summon:
- `Focused Rite`: single-target / boss-oriented execution.
- `Cataclysmic Rite`: AOE / burst-oriented execution.

A third specialization path is available through the summon data as a **Debuff** variant, but each summon can equip only one specialization at a time. This keeps the original three-way choice: Debuff, Single Target, or AOE/Burst.

### Pair 4 — Control / Utility
Choose one:
- `Recall`: instantly recalls active summons, cancels dangerous positioning and refunds a controlled portion of summon resource.
- `Overbind`: temporarily empowers the active summon at an explicit resource/cooldown cost.

### Ultimate pair
The Summoner chooses one ultimate according to its Primary Pact:
- `Grand Convergence`: the active summons combine their attacks for a large coordinated burst.
- `Sovereign Pact`: greatly empowers the Primary Pact's signature behavior for a short window.

## Balance rules
1. The Summoner's automatic class role remains Magic DPS.
2. A Tank Pact provides protection/aggro but does not turn the Summoner into the game's best Tank.
3. A Heal Pact provides sustain but does not turn the Summoner into the game's best dedicated healer.
4. DPS Pacts specialize in damage; their specialization remains either Debuff, Single Target, or AOE/Burst.
5. Major summon count is capped by build data and cannot be multiplied without an explicit balance rule.
6. Summons consume a dedicated resource and use cooldowns, so the Summoner cannot maintain unlimited burst.
7. The player's own skills remain relevant at all times.
8. PET behavior is independent and selectable: Auto, Semi-Auto, or Manual.
9. Gear, stats, talents, PET and Pact choices can create different builds, but cannot bypass the Pact role restrictions.

## Example builds
- Boss: Astral DPS Primary + Shadow DPS Secondary + Frost DPS Secondary; Focused Rite; player mono-target skills.
- Farming: Flame DPS Primary + Storm DPS Secondary + Guardian Tank Secondary; Cataclysmic Rite; AOE player skill.
- Safe solo: Guardian Tank Primary + Astral DPS Secondary + Nature Heal Secondary; balanced damage with protection and sustain.
- Supportive DPS: Astral DPS Primary + Nature Heal Secondary + Shadow DPS Secondary; player remains Magic DPS while summons provide limited support/debuff utility.

## Design goal
The Summoner should feel like a battlefield commander who personally fights alongside controlled entities. It must be versatile without being omnipotent: Pact role limits define **what** the summon contributes; paired skills define **how** it contributes; the player's build defines the final playstyle.
