using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROAbilityDefinition
    {
        public string id;
        public string name;
        public EROClass classId;
        public EROSpecialization specialization;
        public int requiredLevel;
        public ERODamageType damageType;
        public EROElement element;
        public int powerBasisPoints;
        public int criticalBonusBasisPoints;
        public int accuracyBonusBasisPoints;
        public int cooldownMilliseconds;
        public int resourceCost;
        public bool areaOfEffect;
        public bool passive;
    }

    [Serializable]
    public sealed class EROTalentDefinition
    {
        public string id;
        public string name;
        public EROClass classId;
        public EROSpecialization specialization;
        public int requiredLevel;
        public int maxRank = 5;
        public string prerequisiteId;
        public EROPrimaryStat affectedStat;
        public int valuePerRank;
        public bool percentage;
    }

    public static class EROAbilitySystem
    {
        private static readonly EROAbilityDefinition[] Abilities =
        {
            new EROAbilityDefinition { id="paladin_guardian_strike", name="Guardian Strike", classId=EROClass.Paladin, requiredLevel=1, damageType=ERODamageType.Physical, element=EROElement.Light, powerBasisPoints=12000, cooldownMilliseconds=0, resourceCost=10 },
            new EROAbilityDefinition { id="paladin_holy_aegis", name="Holy Aegis", classId=EROClass.Paladin, specialization=EROSpecialization.Holy, requiredLevel=30, damageType=ERODamageType.Magical, element=EROElement.Light, powerBasisPoints=14500, cooldownMilliseconds=12000, resourceCost=25, areaOfEffect=true },
            new EROAbilityDefinition { id="priest_smite", name="Radiant Smite", classId=EROClass.Priest, requiredLevel=1, damageType=ERODamageType.Magical, element=EROElement.Light, powerBasisPoints=12500, cooldownMilliseconds=0, resourceCost=10 },
            new EROAbilityDefinition { id="priest_oracle_burst", name="Oracle Burst", classId=EROClass.Priest, specialization=EROSpecialization.Oracle, requiredLevel=30, damageType=ERODamageType.Magical, element=EROElement.Light, powerBasisPoints=15500, cooldownMilliseconds=10000, resourceCost=25, areaOfEffect=true },
            new EROAbilityDefinition { id="summoner_arcane_beast", name="Arcane Beast", classId=EROClass.Invocateur, requiredLevel=1, damageType=ERODamageType.Magical, element=EROElement.Arcane, powerBasisPoints=12000, cooldownMilliseconds=0, resourceCost=12 },
            new EROAbilityDefinition { id="summoner_shadow_pact", name="Shadow Pact", classId=EROClass.Invocateur, specialization=EROSpecialization.SummonerDPS, requiredLevel=30, damageType=ERODamageType.Magical, element=EROElement.Shadow, powerBasisPoints=16000, cooldownMilliseconds=10000, resourceCost=30 },
            new EROAbilityDefinition { id="mage_arcane_bolt", name="Arcane Bolt", classId=EROClass.Mage, requiredLevel=1, damageType=ERODamageType.Magical, element=EROElement.Arcane, powerBasisPoints=13000, cooldownMilliseconds=0, resourceCost=10 },
            new EROAbilityDefinition { id="mage_flame_storm", name="Flame Storm", classId=EROClass.Mage, specialization=EROSpecialization.Fire, requiredLevel=30, damageType=ERODamageType.Magical, element=EROElement.Fire, powerBasisPoints=17500, cooldownMilliseconds=9000, resourceCost=30, areaOfEffect=true },
            new EROAbilityDefinition { id="mage_frost_lance", name="Frost Lance", classId=EROClass.Mage, specialization=EROSpecialization.Frost, requiredLevel=30, damageType=ERODamageType.Magical, element=EROElement.Ice, powerBasisPoints=16000, cooldownMilliseconds=7000, resourceCost=25 },
            new EROAbilityDefinition { id="assassin_shadow_cut", name="Shadow Cut", classId=EROClass.Assassin, requiredLevel=1, damageType=ERODamageType.Physical, element=EROElement.Shadow, powerBasisPoints=13500, criticalBonusBasisPoints=800, cooldownMilliseconds=0, resourceCost=10 },
            new EROAbilityDefinition { id="assassin_venom_burst", name="Venom Burst", classId=EROClass.Assassin, specialization=EROSpecialization.Venom, requiredLevel=30, damageType=ERODamageType.Physical, element=EROElement.Earth, powerBasisPoints=16500, cooldownMilliseconds=8000, resourceCost=25, areaOfEffect=true },
            new EROAbilityDefinition { id="archer_sniper_shot", name="Sniper Shot", classId=EROClass.Archer, specialization=EROSpecialization.Sniper, requiredLevel=30, damageType=ERODamageType.Physical, element=EROElement.Wind, powerBasisPoints=18000, criticalBonusBasisPoints=1000, cooldownMilliseconds=9000, resourceCost=20 },
            new EROAbilityDefinition { id="archer_ranger_volley", name="Ranger Volley", classId=EROClass.Archer, specialization=EROSpecialization.Ranger, requiredLevel=30, damageType=ERODamageType.Physical, element=EROElement.Wind, powerBasisPoints=14500, cooldownMilliseconds=6000, resourceCost=20, areaOfEffect=true },
            new EROAbilityDefinition { id="warrior_heavy_strike", name="Heavy Strike", classId=EROClass.Guerrier, requiredLevel=1, damageType=ERODamageType.Physical, element=EROElement.Earth, powerBasisPoints=12500, cooldownMilliseconds=0, resourceCost=10 },
            new EROAbilityDefinition { id="warrior_berserker_rage", name="Berserker Rage", classId=EROClass.Guerrier, specialization=EROSpecialization.Berserker, requiredLevel=30, damageType=ERODamageType.Physical, element=EROElement.Fire, powerBasisPoints=17000, criticalBonusBasisPoints=900, cooldownMilliseconds=10000, resourceCost=25 },
        };

        private static readonly EROTalentDefinition[] Talents = BuildTalents();

        public static IReadOnlyList<EROAbilityDefinition> GetAbilities(EROClass classId, EROSpecialization specialization, int level)
        {
            var result = new List<EROAbilityDefinition>();
            foreach (var a in Abilities)
                if (a.classId == classId && level >= a.requiredLevel && (a.specialization == EROSpecialization.None || a.specialization == specialization)) result.Add(a);
            return result;
        }

        public static EROAbilityDefinition FindAbility(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var a in Abilities) if (a.id == id) return a;
            return null;
        }

        public static IReadOnlyList<EROTalentDefinition> GetTalents(EROClass classId, EROSpecialization specialization)
        {
            var result = new List<EROTalentDefinition>();
            foreach (var t in Talents) if (t.classId == classId && t.specialization == specialization) result.Add(t);
            return result;
        }

        public static bool CanUse(EROAbilityDefinition ability, EROClass classId, EROSpecialization specialization, int level)
        {
            if (ability == null || ability.classId != classId || level < ability.requiredLevel) return false;
            return ability.specialization == EROSpecialization.None || ability.specialization == specialization;
        }

        private static EROTalentDefinition[] BuildTalents()
        {
            var result = new List<EROTalentDefinition>();
            AddSpecializationTalents(result, EROClass.Guerrier, EROSpecialization.Berserker, EROPrimaryStat.Strength, 2);
            AddSpecializationTalents(result, EROClass.Guerrier, EROSpecialization.Gladiator, EROPrimaryStat.Dexterity, 2);
            AddSpecializationTalents(result, EROClass.Guerrier, EROSpecialization.Juggernaut, EROPrimaryStat.Vitality, 3);
            AddSpecializationTalents(result, EROClass.Paladin, EROSpecialization.Holy, EROPrimaryStat.Spirit, 3);
            AddSpecializationTalents(result, EROClass.Paladin, EROSpecialization.BattlePriest, EROPrimaryStat.Strength, 2);
            AddSpecializationTalents(result, EROClass.Paladin, EROSpecialization.Juggernaut, EROPrimaryStat.Vitality, 3);
            AddSpecializationTalents(result, EROClass.Priest, EROSpecialization.Holy, EROPrimaryStat.Spirit, 3);
            AddSpecializationTalents(result, EROClass.Priest, EROSpecialization.BattlePriest, EROPrimaryStat.Intelligence, 2);
            AddSpecializationTalents(result, EROClass.Priest, EROSpecialization.Oracle, EROPrimaryStat.Intelligence, 3);
            AddSpecializationTalents(result, EROClass.Invocateur, EROSpecialization.SummonerDPS, EROPrimaryStat.Intelligence, 3);
            AddSpecializationTalents(result, EROClass.Invocateur, EROSpecialization.SummonerTank, EROPrimaryStat.Vitality, 3);
            AddSpecializationTalents(result, EROClass.Invocateur, EROSpecialization.SummonerSupport, EROPrimaryStat.Spirit, 3);
            AddSpecializationTalents(result, EROClass.Mage, EROSpecialization.Fire, EROPrimaryStat.Intelligence, 3);
            AddSpecializationTalents(result, EROClass.Mage, EROSpecialization.Frost, EROPrimaryStat.Spirit, 3);
            AddSpecializationTalents(result, EROClass.Mage, EROSpecialization.Arcane, EROPrimaryStat.Intelligence, 3);
            AddSpecializationTalents(result, EROClass.Assassin, EROSpecialization.Shadow, EROPrimaryStat.Agility, 3);
            AddSpecializationTalents(result, EROClass.Assassin, EROSpecialization.Venom, EROPrimaryStat.Dexterity, 3);
            AddSpecializationTalents(result, EROClass.Assassin, EROSpecialization.AssassinCrit, EROPrimaryStat.Luck, 4);
            AddSpecializationTalents(result, EROClass.Archer, EROSpecialization.Sniper, EROPrimaryStat.Dexterity, 3);
            AddSpecializationTalents(result, EROClass.Archer, EROSpecialization.Ranger, EROPrimaryStat.Agility, 3);
            AddSpecializationTalents(result, EROClass.Archer, EROSpecialization.ElementalArcher, EROPrimaryStat.Intelligence, 2);
            return result.ToArray();
        }

        private static void AddSpecializationTalents(List<EROTalentDefinition> list, EROClass c, EROSpecialization s, EROPrimaryStat stat, int value)
        {
            string prefix = c.ToString().ToLowerInvariant() + "_" + s.ToString().ToLowerInvariant();
            list.Add(new EROTalentDefinition { id=prefix+"_mastery", name=s+" Mastery", classId=c, specialization=s, requiredLevel=30, maxRank=5, affectedStat=stat, valuePerRank=value });
            list.Add(new EROTalentDefinition { id=prefix+"_advanced", name=s+" Advanced", classId=c, specialization=s, requiredLevel=60, maxRank=5, prerequisiteId=prefix+"_mastery", affectedStat=stat, valuePerRank=value+1 });
            list.Add(new EROTalentDefinition { id=prefix+"_master", name=s+" Master", classId=c, specialization=s, requiredLevel=100, maxRank=5, prerequisiteId=prefix+"_advanced", affectedStat=stat, valuePerRank=value+2 });
        }
    }
}
