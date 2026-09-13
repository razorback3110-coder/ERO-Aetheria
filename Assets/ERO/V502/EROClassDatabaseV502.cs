using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V502
{
    public enum EROBaseClass { Knight, Assassin, Ranger, Mage, Priest, Monk, Summoner, Paladin }
    public enum EROEvolution { Guardian, Shadow, Sniper, Archmage, HighPriest, Champion, Demonologist, Crusader }
    public enum EROBranch
    {
        Vanguard, Runeblade, Aegis,
        Nightstalker, Venomancer, Phantom,
        Hawkeye, Beastwarden, Windrunner,
        Arcaniste, Cryomancien, Tempestaire,
        HighPriest, Oracle, Luminary,
        Champion, MysticFist, DragonDisciple,
        Demonologist, Soulcaller, Beastbinder,
        Crusader, HolyKnight, Dawnwarden
    }

    [Serializable]
    public sealed class EROBranchDefinition
    {
        public EROBranch branch;
        public string displayName;
        public string fantasy;
        public string[] specializationSkills = new string[3];
        public string[] branchSkills = new string[6];

        public EROBranchDefinition(EROBranch branch, string name, string fantasy, string[] specs, string[] skills)
        {
            this.branch = branch; displayName = name; this.fantasy = fantasy;
            specializationSkills = specs; branchSkills = skills;
        }
    }

    public static class EROClassDatabaseV502
    {
        public const int BranchChoiceLevel = 18;
        public const int FirstEvolutionLevel = 40;
        public const int FinalEvolutionLevel = 75;

        static readonly Dictionary<EROBaseClass, EROEvolution> evolutions = new()
        {
            { EROBaseClass.Knight, EROEvolution.Guardian }, { EROBaseClass.Assassin, EROEvolution.Shadow },
            { EROBaseClass.Ranger, EROEvolution.Sniper }, { EROBaseClass.Mage, EROEvolution.Archmage },
            { EROBaseClass.Priest, EROEvolution.HighPriest }, { EROBaseClass.Monk, EROEvolution.Champion },
            { EROBaseClass.Summoner, EROEvolution.Demonologist }, { EROBaseClass.Paladin, EROEvolution.Crusader }
        };

        static readonly Dictionary<EROBaseClass, EROBranch[]> branches = new()
        {
            { EROBaseClass.Knight, new[]{ EROBranch.Vanguard, EROBranch.Runeblade, EROBranch.Aegis } },
            { EROBaseClass.Assassin, new[]{ EROBranch.Nightstalker, EROBranch.Venomancer, EROBranch.Phantom } },
            { EROBaseClass.Ranger, new[]{ EROBranch.Hawkeye, EROBranch.Beastwarden, EROBranch.Windrunner } },
            { EROBaseClass.Mage, new[]{ EROBranch.Arcaniste, EROBranch.Cryomancien, EROBranch.Tempestaire } },
            { EROBaseClass.Priest, new[]{ EROBranch.HighPriest, EROBranch.Oracle, EROBranch.Luminary } },
            { EROBaseClass.Monk, new[]{ EROBranch.Champion, EROBranch.MysticFist, EROBranch.DragonDisciple } },
            { EROBaseClass.Summoner, new[]{ EROBranch.Demonologist, EROBranch.Soulcaller, EROBranch.Beastbinder } },
            { EROBaseClass.Paladin, new[]{ EROBranch.Crusader, EROBranch.HolyKnight, EROBranch.Dawnwarden } }
        };

        public static EROEvolution GetEvolution(EROBaseClass c) => evolutions[c];
        public static IReadOnlyList<EROBranch> GetBranches(EROBaseClass c) => branches[c];
        public static bool CanChooseBranch(int level) => level >= BranchChoiceLevel;
        public static bool CanFirstEvolve(int level) => level >= FirstEvolutionLevel;
        public static bool CanFinalEvolve(int level) => level >= FinalEvolutionLevel;

        public static EROBranchDefinition GetDefinition(EROBranch b)
        {
            string n = b.ToString();
            switch (b)
            {
                case EROBranch.Vanguard: return D(b,n,"frontline / aggro", new[]{"Taunting Core","Iron Will","Bulwark"}, new[]{"Shield Rush","Fortify","War Cry","Guard Break","Last Stand","Titan Wall"});
                case EROBranch.Runeblade: return D(b,n,"magic-infused melee", new[]{"Rune Edge","Arcane Guard","Mana Cut"}, new[]{"Flame Rune","Frost Rune","Storm Rune","Rune Break","Arcane Slash","Grand Sigil"});
                case EROBranch.Aegis: return D(b,n,"defensive support", new[]{"Aegis Pulse","Barrier","Guardian Light"}, new[]{"Holy Guard","Reflect","Mass Shield","Sanctuary","Aegis Crash","Immortal Bastion"});
                case EROBranch.Nightstalker: return D(b,n,"stealth assassin", new[]{"Shadowstep","Ambush","Execution Mark"}, new[]{"Backstab","Vanish","Nightfall","Bleedout","Shadow Dance","Death Sentence"});
                case EROBranch.Venomancer: return D(b,n,"poison damage", new[]{"Venom Edge","Toxic Mark","Corrode"}, new[]{"Poison Rain","Plague Knife","Toxin Burst","Contagion","Venom Storm","Fatal Dose"});
                case EROBranch.Phantom: return D(b,n,"evasion / clones", new[]{"Mirror Step","Phantom Blade","Afterimage"}, new[]{"Decoy","Blink","Twin Strike","False Target","Spectral Barrage","Phantom Execution"});
                case EROBranch.Hawkeye: return D(b,n,"precision ranged", new[]{"Deadeye","Piercing Shot","Hunter's Mark"}, new[]{"Snipe","Arrow Rain","Piercing Volley","Eagle Eye","Kill Zone","Skyfall"});
                case EROBranch.Beastwarden: return D(b,n,"pet ranger", new[]{"Bond","Pack Call","Wild Instinct"}, new[]{"Beast Rush","Howl","Primal Shot","Pack Frenzy","Nature's Claim","Alpha Hunt"});
                case EROBranch.Windrunner: return D(b,n,"mobility / rapid fire", new[]{"Gale Step","Rapid Draw","Wind Sense"}, new[]{"Wind Shot","Gust Trap","Hailstorm","Cyclone Volley","Tempest Dash","Sky Hunter"});
                case EROBranch.HighPriest: return D(b,n,"classic high priest", new[]{"Greater Heal","Holy Shield","Sacred Chant"}, new[]{"Heal","Blessing","Resurrection","Holy Light","Mass Heal","Miracle"});
                case EROBranch.Arcaniste: return D(b,n,"arcane burst", new[]{"Arcane Lance","Mana Surge","Astral Focus"}, new[]{"Arcane Bolt","Meteor","Rift Pulse","Arcane Prison","Astral Storm","Eternal Nova"});
                case EROBranch.Cryomancien: return D(b,n,"frost control", new[]{"Ice Shard","Frozen Core","Winter Veil"}, new[]{"Frost Nova","Ice Lance","Glacial Wall","Deep Freeze","Blizzard","Absolute Zero"});
                case EROBranch.Tempestaire: return D(b,n,"lightning / AoE", new[]{"Spark","Overcharge","Storm Sight"}, new[]{"Chain Lightning","Thunder Orb","Static Field","Storm Call","Tempest","Heaven's Wrath"});
                case EROBranch.Oracle: return D(b,n,"divination / support", new[]{"Foresight","Prophecy","Blessed Thread"}, new[]{"Radiant Heal","Future Sight","Purify","Holy Word","Destiny Link","Oracle's Grace"});
                case EROBranch.Luminary: return D(b,n,"holy damage / support", new[]{"Light Spear","Radiant Aura","Dawn Prayer"}, new[]{"Holy Bolt","Radiance","Sunburst","Lightfall","Seraphic Field","Dawn Ascension"});
                case EROBranch.Champion: return D(b,n,"martial DPS", new[]{"Fighting Spirit","Iron Fist","Combo Mastery"}, new[]{"Tiger Palm","Meteor Fist","Rising Dragon","Counter","Limit Break","Champion's Roar"});
                case EROBranch.MysticFist: return D(b,n,"spiritual melee", new[]{"Mystic Flow","Spirit Mark","Chi Burst"}, new[]{"Spirit Palm","Chi Wave","Soul Crush","Meditation","Astral Fist","Mystic Cataclysm"});
                case EROBranch.DragonDisciple: return D(b,n,"dragon martial arts", new[]{"Dragon Breath","Scale Guard","Draconic Chi"}, new[]{"Dragon Claw","Dragon Roar","Sky Breaker","Drake Step","Dragon Fury","Ancient Dragon"});
                case EROBranch.Demonologist: return D(b,n,"demonic summoning", new[]{"Demon Pact","Infernal Mark","Soul Contract"}, new[]{"Imp Swarm","Hellfire","Demon Gate","Soul Harvest","Infernal Legion","Doom Herald"});
                case EROBranch.Soulcaller: return D(b,n,"spirit summoning", new[]{"Soul Link","Spirit Whisper","Grave Pact"}, new[]{"Wisp Call","Soul Bolt","Spirit Chain","Possession","Soulstorm","Requiem"});
                case EROBranch.Beastbinder: return D(b,n,"beast army", new[]{"Beast Pact","Alpha Bond","Wild Call"}, new[]{"Summon Wolf","Summon Bear","Bestial Roar","Pack Hunt","Primal Legion","Ancient Beast"});
                case EROBranch.Crusader: return D(b,n,"holy knight", new[]{"Holy Oath","Shield of Dawn","Judgment"}, new[]{"Smite","Holy Charge","Judgment Strike","Sacred Guard","Divine Shield","Crusader's Verdict"});
                case EROBranch.HolyKnight: return D(b,n,"holy tank", new[]{"Radiant Shield","Oathkeeper","Sacred Wall"}, new[]{"Holy Taunt","Blessed Armor","Atonement","Sanctuary","Divine Bulwark","Heaven's Bastion"});
                case EROBranch.Dawnwarden: return D(b,n,"support / frontline", new[]{"Dawn Guard","Aurora","Last Prayer"}, new[]{"Dawn Strike","Morning Star","Radiant Banner","Hope","Dawn Barrier","Eternal Dawn"});
                default: throw new ArgumentOutOfRangeException(nameof(b));
            }
        }
        static EROBranchDefinition D(EROBranch b,string n,string f,string[] s,string[] sk) => new(b,n,f,s,sk);
    }
}
