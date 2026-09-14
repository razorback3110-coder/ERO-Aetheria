using System;
using UnityEngine;

namespace ERO.Data
{
    // Locked launch class set. Legacy names are intentionally removed from the public model.
    public enum EROClass
    {
        Paladin,
        Priest,
        Invocateur,
        Mage,
        Assassin,
        Archer,
        Guerrier
    }

    public enum ClassRole { PhysicalDPS, MagicDPS, Tank, Heal }
    public enum SummonerPactRole { DPS, Tank, Heal }
    public enum SummonerSkillMode { Debuff, SingleTarget, AOEBurst }
    public enum SummonerPetControl { Auto, SemiAuto, Manual }
    public enum Gender { Male, Female }

    // Final ERO rarity ladder. Unique is the endgame tier; there is no Eternal tier.
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Unique
    }

    public enum Language
    {
        French, English, German, Spanish, Italian, Dutch,
        Portuguese, Japanese, Korean, ChineseSimplified
    }

    [Serializable]
    public class Appearance
    {
        public Gender gender;
        public int face;
        public int hair;
        public int hairColor;
        public int eyeColor;
        public int skinTone;
        public float height = 1f;
    }

    [Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public EROClass classId;
        public int level = 1;
        public long xp;
        public long overflowXp;
        public Appearance appearance = new Appearance();

        // Authoritative economy values are represented as long to prevent early overflow.
        public long credits;
        public long eroCrystals;

        // Activity currencies. Server-side validation remains mandatory in production.
        public long guildTokens;
        public long arenaTokens;
        public long dungeonStones;
        public long mvpTokens;
        public long eventTokens;
    }

    [Serializable]
    public class SummonerPactData
    {
        public string id;
        public string name;
        public SummonerPactRole role;
        public bool primary;
        public int maxActiveSummons = 1;
        public SummonerSkillMode specialization = SummonerSkillMode.SingleTarget;
    }

    [Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public Rarity rarity;
        public int level;
        public int quantity = 1;
        public bool equipped;
    }

    [Serializable]
    public class QuestData
    {
        public string id;
        public string title;
        public string description;
        public int required;
        public int progress;
        public long creditsReward;
        public long xpReward;
        public bool completed;
    }

    [Serializable]
    public class ZoneData
    {
        public string id;
        public string name;
        public int minLevel;
        public int maxLevel;
        public string[] pointsOfInterest;
    }

    [Serializable]
    public class GuildData
    {
        public string id;
        public string name;
        public int level = 1;
        public int members;
        public long experience;
    }

    public static class EROEconomyRules
    {
        public const long CreditsPerEroCrystal = 1000L;
        public const int MaxClassEvolutionTicketsPerWeek = 4;
    }
}
