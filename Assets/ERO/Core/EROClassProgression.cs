using System;

namespace EternalRealmsOnline.Core
{
    public enum EROClassId
    {
        Vanguard = 0,
        Guardian = 1,
        Ranger = 2,
        Assassin = 3,
        Arcanist = 4,
        Cleric = 5,
        Warlock = 6,
        Artificer = 7
    }

    public enum EROEvolutionTier
    {
        Base = 0,
        First = 1,
        Second = 2
    }

    public readonly struct EROClassSkillSet
    {
        public readonly string Skill1;
        public readonly string Skill2;
        public readonly string Skill3;

        public EROClassSkillSet(string skill1, string skill2, string skill3)
        {
            Skill1 = skill1;
            Skill2 = skill2;
            Skill3 = skill3;
        }
    }

    /// <summary>
    /// Server-safe class progression rules. No Unity or asset dependency.
    /// Class selection unlocks at level 18, first evolution at 40 and second at 75.
    /// </summary>
    public static class EROClassProgression
    {
        public const int ClassSelectionLevel = 18;
        public const int FirstEvolutionLevel = 40;
        public const int SecondEvolutionLevel = 75;

        public static bool CanSelectClass(int level) => level >= ClassSelectionLevel;
        public static bool CanFirstEvolve(int level) => level >= FirstEvolutionLevel;
        public static bool CanSecondEvolve(int level) => level >= SecondEvolutionLevel;

        public static EROEvolutionTier GetTier(int level)
        {
            if (level >= SecondEvolutionLevel) return EROEvolutionTier.Second;
            if (level >= FirstEvolutionLevel) return EROEvolutionTier.First;
            return EROEvolutionTier.Base;
        }

        public static bool TryGetSkillSet(EROClassId classId, EROEvolutionTier tier, out EROClassSkillSet skills)
        {
            skills = classId switch
            {
                EROClassId.Vanguard => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Aether Breaker", "Iron Rush", "War Cry"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Aether Rend", "Meteor Charge", "Warlord's Resolve"),
                    _ => new EROClassSkillSet("Heavy Strike", "Guard Stance", "Taunt")
                },
                EROClassId.Guardian => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Bulwark", "Shield Pulse", "Aegis Link"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Citadel", "Radiant Bastion", "Guardian's Oath"),
                    _ => new EROClassSkillSet("Shield Bash", "Brace", "Challenge")
                },
                EROClassId.Ranger => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Piercing Volley", "Hawkeye", "Windstep"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Starfall Volley", "Predator's Mark", "Gale Retreat"),
                    _ => new EROClassSkillSet("Quick Shot", "Focused Aim", "Backstep")
                },
                EROClassId.Assassin => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Shadow Lunge", "Venom Edge", "Vanish"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Night Requiem", "Deathmark", "Umbral Step"),
                    _ => new EROClassSkillSet("Twin Slash", "Bleed", "Smoke Veil")
                },
                EROClassId.Arcanist => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Aether Lance", "Prism Nova", "Arcane Ward"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Astral Cataclysm", "Prismatic Rupture", "Aether Barrier"),
                    _ => new EROClassSkillSet("Arc Bolt", "Frost Sigil", "Mana Ward")
                },
                EROClassId.Cleric => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Radiant Wave", "Sanctuary", "Purifying Light"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Divine Chorus", "Eternal Sanctuary", "Judgment Light"),
                    _ => new EROClassSkillSet("Smite", "Mend", "Blessing")
                },
                EROClassId.Warlock => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Soul Bolt", "Dread Pact", "Plague Field"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Abyssal Storm", "Soul Harvest", "Doom Covenant"),
                    _ => new EROClassSkillSet("Dark Bolt", "Hex", "Life Drain")
                },
                EROClassId.Artificer => tier switch
                {
                    EROEvolutionTier.First => new EROClassSkillSet("Arc Turret", "Overclock", "Magnetic Trap"),
                    EROEvolutionTier.Second => new EROClassSkillSet("Aether Arsenal", "Singularity Mine", "Overdrive Core"),
                    _ => new EROClassSkillSet("Spark Shot", "Deploy Turret", "Repair Field")
                },
                _ => default
            };

            return classId >= EROClassId.Vanguard && classId <= EROClassId.Artificer;
        }

        public static bool IsValidEvolution(int level, EROEvolutionTier tier)
        {
            return tier switch
            {
                EROEvolutionTier.Base => true,
                EROEvolutionTier.First => CanFirstEvolve(level),
                EROEvolutionTier.Second => CanSecondEvolve(level),
                _ => false
            };
        }
    }
}
