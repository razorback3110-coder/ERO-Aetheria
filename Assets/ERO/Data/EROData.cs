using System;
using UnityEngine;

namespace ERO.Data
{
    public enum EROClass { Paladin, Priest, Invocateur, Mage, Assassin, Archer, Guerrier }
    public enum ClassRole { PhysicalDPS, MagicDPS, Tank, Heal }
    public enum SummonerPactRole { DPS, Tank, Heal }
    public enum SummonerSkillMode { Debuff, SingleTarget, AOEBurst }
    public enum SummonerPetControl { Auto, SemiAuto, Manual }
    public enum Gender { Male, Female }
    public enum EROPrimaryStat { Strength, Agility, Dexterity, Intelligence, Vitality, Spirit, Luck }
    public enum EROGemColor { Red, Blue, Green, Yellow, Purple, White, Prismatic }
    public enum ERORuneType { Offensive, Defensive, Utility, Class }
    public enum ERORuneTrigger { Passive, OnHit, OnCritical, OnBlock, OnKill, OnLowHealth }

    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic, Unique }
    public enum Language { French, English, German, Spanish, Italian, Dutch, Portuguese, Japanese, Korean, ChineseSimplified }

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
    public class EROStatAllocation
    {
        public int strength;
        public int agility;
        public int dexterity;
        public int intelligence;
        public int vitality;
        public int spirit;
        public int luck;

        public int Get(EROPrimaryStat stat)
        {
            switch (stat)
            {
                case EROPrimaryStat.Strength: return strength;
                case EROPrimaryStat.Agility: return agility;
                case EROPrimaryStat.Dexterity: return dexterity;
                case EROPrimaryStat.Intelligence: return intelligence;
                case EROPrimaryStat.Vitality: return vitality;
                case EROPrimaryStat.Spirit: return spirit;
                case EROPrimaryStat.Luck: return luck;
                default: return 0;
            }
        }

        public void Add(EROPrimaryStat stat, int amount)
        {
            switch (stat)
            {
                case EROPrimaryStat.Strength: strength += amount; break;
                case EROPrimaryStat.Agility: agility += amount; break;
                case EROPrimaryStat.Dexterity: dexterity += amount; break;
                case EROPrimaryStat.Intelligence: intelligence += amount; break;
                case EROPrimaryStat.Vitality: vitality += amount; break;
                case EROPrimaryStat.Spirit: spirit += amount; break;
                case EROPrimaryStat.Luck: luck += amount; break;
            }
        }

        public int Total => strength + agility + dexterity + intelligence + vitality + spirit + luck;
    }

    [Serializable]
    public class EROGemData
    {
        public string id;
        public string name;
        public Rarity rarity;
        public EROGemColor color;
        public EROPrimaryStat stat;
        public int statValue;
        public int level = 1;
    }

    [Serializable]
    public class ERORuneData
    {
        public string id;
        public string name;
        public Rarity rarity;
        public ERORuneType type;
        public ERORuneTrigger trigger = ERORuneTrigger.Passive;
        public EROPrimaryStat stat;
        public int statValue;
        public int powerBasisPoints;
        public EROClass requiredClass;
        public int requiredLevel;
        public string setId;
    }

    [Serializable]
    public class EROEquipmentSocket
    {
        public bool unlocked;
        public bool runeSocket;
        public string gemId;
        public string runeId;
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
        public EROStatAllocation stats = new EROStatAllocation();
        public int unspentStatPoints;

        public long credits;
        public long eroCrystals;
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
        public EROEquipmentSocket[] sockets = Array.Empty<EROEquipmentSocket>();
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
