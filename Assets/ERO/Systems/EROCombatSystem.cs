using System;
using ERO.Data;

namespace ERO.Systems
{
    public enum ERODamageType { Physical, Magical, True }
    public enum EROCombatResult { Miss, Hit, Critical, Blocked, Immune }
    [Serializable] public struct EROCombatStats
    {
        public long maxHealth, attack, magicAttack, defense, magicDefense;
        public int criticalChanceBasisPoints, blockChanceBasisPoints, accuracyBasisPoints;
        public long GetPower(ERODamageType type) => type == ERODamageType.Magical ? magicAttack : attack;
        public long GetDefense(ERODamageType type) => type == ERODamageType.Magical ? magicDefense : defense;
    }
    [Serializable] public struct EROSkillDefinition
    {
        public string id; public EROClass classId; public ERODamageType damageType; public int requiredLevel; public int powerBasisPoints; public int criticalBonusBasisPoints; public int accuracyBonusBasisPoints; public int cooldownMilliseconds; public int resourceCost; public bool areaOfEffect;
        public bool IsUsable(EROClass casterClass, int level) => classId == casterClass && level >= requiredLevel && powerBasisPoints > 0;
    }
    [Serializable] public struct EROCombatEvent
    {
        public EROCombatResult result; public ERODamageType damageType; public long rawDamage; public long mitigatedDamage; public bool critical; public int cooldownMilliseconds; public int resourceCost;
    }
    public static class EROCombatSystem
    {
        public const int BasisPoints = 10000; private const long MinimumDamage = 1L;
        public static EROCombatStats BuildStats(CharacterData character) => BuildStats(character, null, null, null);
        public static EROCombatStats BuildStats(CharacterData character, ItemData[] equippedItems, EROGemData[] gems, ERORuneData[] runes)
        {
            if (character == null) return default;
            int level = Math.Max(1, Math.Min(250, character.level)); long primary = 40L + level * 7L; long secondary = 25L + level * 4L; EROCombatStats stats;
            switch (character.classId)
            {
                case EROClass.Paladin: stats = new EROCombatStats { maxHealth=220L+level*34L, attack=primary, magicAttack=secondary, defense=34L+level*6L, magicDefense=30L+level*5L, criticalChanceBasisPoints=700, blockChanceBasisPoints=1800, accuracyBasisPoints=9500 }; break;
                case EROClass.Priest: stats = new EROCombatStats { maxHealth=180L+level*27L, attack=secondary, magicAttack=primary, defense=24L+level*4L, magicDefense=36L+level*6L, criticalChanceBasisPoints=900, blockChanceBasisPoints=900, accuracyBasisPoints=9600 }; break;
                case EROClass.Invocateur: stats = new EROCombatStats { maxHealth=175L+level*26L, attack=secondary, magicAttack=primary+level*2L, defense=22L+level*4L, magicDefense=32L+level*5L, criticalChanceBasisPoints=1100, blockChanceBasisPoints=700, accuracyBasisPoints=9600 }; break;
                case EROClass.Mage: stats = new EROCombatStats { maxHealth=155L+level*23L, attack=secondary, magicAttack=primary+level*4L, defense=18L+level*3L, magicDefense=34L+level*5L, criticalChanceBasisPoints=1300, blockChanceBasisPoints=500, accuracyBasisPoints=9650 }; break;
                case EROClass.Assassin: stats = new EROCombatStats { maxHealth=165L+level*25L, attack=primary+level*3L, magicAttack=secondary, defense=21L+level*4L, magicDefense=22L+level*3L, criticalChanceBasisPoints=2200, blockChanceBasisPoints=600, accuracyBasisPoints=9750 }; break;
                case EROClass.Archer: stats = new EROCombatStats { maxHealth=170L+level*25L, attack=primary+level*2L, magicAttack=secondary, defense=20L+level*3L, magicDefense=24L+level*3L, criticalChanceBasisPoints=1700, blockChanceBasisPoints=650, accuracyBasisPoints=9850 }; break;
                case EROClass.Guerrier: stats = new EROCombatStats { maxHealth=205L+level*32L, attack=primary+level*2L, magicAttack=secondary, defense=30L+level*5L, magicDefense=24L+level*4L, criticalChanceBasisPoints=1000, blockChanceBasisPoints=1200, accuracyBasisPoints=9550 }; break;
                default: stats = new EROCombatStats { maxHealth=150L+level*22L, attack=primary, magicAttack=secondary, defense=18L+level*3L, magicDefense=18L+level*3L, criticalChanceBasisPoints=800, blockChanceBasisPoints=500, accuracyBasisPoints=9500 }; break;
            }
            var affinity = EROStatSystem.GetClassAffinity(character.classId); var allocated = character.stats ?? new EROStatAllocation();
            AddAllocatedStats(ref stats, EROPrimaryStat.Strength, allocated.strength, affinity.strength); AddAllocatedStats(ref stats, EROPrimaryStat.Agility, allocated.agility, affinity.agility); AddAllocatedStats(ref stats, EROPrimaryStat.Dexterity, allocated.dexterity, affinity.dexterity); AddAllocatedStats(ref stats, EROPrimaryStat.Intelligence, allocated.intelligence, affinity.intelligence); AddAllocatedStats(ref stats, EROPrimaryStat.Vitality, allocated.vitality, affinity.vitality); AddAllocatedStats(ref stats, EROPrimaryStat.Spirit, allocated.spirit, affinity.spirit); AddAllocatedStats(ref stats, EROPrimaryStat.Luck, allocated.luck, affinity.luck);
            var gear = EROStatSystem.GetEquipmentStatBonuses(equippedItems, gems, runes); AddGearStats(ref stats, gear); return stats;
        }
        private static void AddAllocatedStats(ref EROCombatStats s, EROPrimaryStat stat, int value, long affinity)
        {
            if (value <= 0) return; long e = EROStatSystem.ApplyAffinity(value, (int)Math.Max(EROStatSystem.BasisPoints, affinity));
            switch (stat) { case EROPrimaryStat.Strength: s.attack+=e*3L; break; case EROPrimaryStat.Agility: s.attack+=e; s.accuracyBasisPoints=SafeBp(s.accuracyBasisPoints+(int)Math.Min(500,e*2)); break; case EROPrimaryStat.Dexterity: s.attack+=e*2L; s.accuracyBasisPoints=SafeBp(s.accuracyBasisPoints+(int)Math.Min(600,e*3)); break; case EROPrimaryStat.Intelligence: s.magicAttack+=e*3L; break; case EROPrimaryStat.Vitality: s.maxHealth+=e*18L; s.defense+=e; break; case EROPrimaryStat.Spirit: s.magicAttack+=e*2L; s.magicDefense+=e*2L; break; case EROPrimaryStat.Luck: s.criticalChanceBasisPoints=SafeBp(s.criticalChanceBasisPoints+(int)Math.Min(900,e*3)); break; }
        }
        private static void AddGearStats(ref EROCombatStats s, EROStatModifiers g)
        {
            s.attack += g.strength*3L + g.agility + g.dexterity*2L; s.magicAttack += g.intelligence*3L + g.spirit*2L; s.maxHealth += g.vitality*18L; s.defense += g.vitality; s.magicDefense += g.spirit*2L;
            s.attack = MultiplyBasisPoints(s.attack, BasisPoints + g.offensivePowerBasisPoints); s.magicAttack = MultiplyBasisPoints(s.magicAttack, BasisPoints + g.offensivePowerBasisPoints);
            s.accuracyBasisPoints=SafeBp(s.accuracyBasisPoints+(int)Math.Min(1200L,g.agility*2L+g.dexterity*3L)); s.criticalChanceBasisPoints=SafeBp(s.criticalChanceBasisPoints+(int)Math.Min(1500L,g.luck*3L));
        }
        public static EROCombatEvent ResolveAttack(EROCombatStats a, EROCombatStats d, EROSkillDefinition skill, int accuracyRoll, int criticalRoll, int blockRoll)
        {
            accuracyRoll=ClampRoll(accuracyRoll); criticalRoll=ClampRoll(criticalRoll); blockRoll=ClampRoll(blockRoll);
            if (accuracyRoll>=ClampBasisPoints(a.accuracyBasisPoints+skill.accuracyBonusBasisPoints)) return new EROCombatEvent { result=EROCombatResult.Miss, damageType=skill.damageType, cooldownMilliseconds=Math.Max(0,skill.cooldownMilliseconds), resourceCost=Math.Max(0,skill.resourceCost) };
            long raw=MultiplyBasisPoints(a.GetPower(skill.damageType),Math.Max(0,skill.powerBasisPoints)); bool crit=criticalRoll<ClampBasisPoints(a.criticalChanceBasisPoints+skill.criticalBonusBasisPoints); if(crit) raw=MultiplyBasisPoints(raw,15000);
            if(skill.damageType!=ERODamageType.True && blockRoll<ClampBasisPoints(d.blockChanceBasisPoints)) { long blocked=Math.Max(MinimumDamage,raw/2L); return new EROCombatEvent { result=EROCombatResult.Blocked,damageType=skill.damageType,rawDamage=raw,mitigatedDamage=blocked,critical=crit,cooldownMilliseconds=Math.Max(0,skill.cooldownMilliseconds),resourceCost=Math.Max(0,skill.resourceCost) }; }
            long mitigation=d.GetDefense(skill.damageType); long damage=skill.damageType==ERODamageType.True?Math.Max(MinimumDamage,raw):Math.Max(MinimumDamage,raw-mitigation);
            return new EROCombatEvent { result=crit?EROCombatResult.Critical:EROCombatResult.Hit,damageType=skill.damageType,rawDamage=raw,mitigatedDamage=damage,critical=crit,cooldownMilliseconds=Math.Max(0,skill.cooldownMilliseconds),resourceCost=Math.Max(0,skill.resourceCost) };
        }
        private static int ClampRoll(int v)=>Math.Max(0,Math.Min(BasisPoints-1,v)); private static int ClampBasisPoints(int v)=>Math.Max(0,Math.Min(BasisPoints,v)); private static int SafeBp(int v)=>ClampBasisPoints(v);
        private static long MultiplyBasisPoints(long v,int bp){if(v<=0||bp<=0)return 0;if(v>long.MaxValue/bp)return long.MaxValue;return v*bp/BasisPoints;}
    }
}
