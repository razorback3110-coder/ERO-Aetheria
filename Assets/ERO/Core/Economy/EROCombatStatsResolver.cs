using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Converts the authoritative character/equipment stat aggregate into the
    /// small set of combat inputs consumed by the deterministic combat layer.
    /// No Unity dependency: dedicated servers and clients use identical rules.
    /// </summary>
    public static class EROCombatStatsResolver
    {
        public const string AttackStat = "Attack";
        public const string DefenseStat = "Defense";
        public const string MaxHealthStat = "MaxHealth";
        public const string CritChanceBasisPointsStat = "CritChanceBasisPoints";
        public const string CritMultiplierBasisPointsStat = "CritMultiplierBasisPoints";

        public const long DefaultCritMultiplierBasisPoints = 17500;
        public const long BasisPointsPerWhole = 10000;

        public static CombatInputs Resolve(EROCharacterCombatStats stats)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            IReadOnlyDictionary<string, long> finalStats = stats.CalculateFinalStats();
            long attack = ReadNonNegative(finalStats, AttackStat);
            long defense = ReadNonNegative(finalStats, DefenseStat);
            long maxHealth = ReadNonNegative(finalStats, MaxHealthStat);
            long critChance = Clamp(ReadNonNegative(finalStats, CritChanceBasisPointsStat), 0, BasisPointsPerWhole);
            long critMultiplier = ReadNonNegative(finalStats, CritMultiplierBasisPointsStat);
            if (critMultiplier == 0) critMultiplier = DefaultCritMultiplierBasisPoints;

            return new CombatInputs(attack, defense, maxHealth, critChance, critMultiplier);
        }

        public static bool RollCritical(ulong seed, long critChanceBasisPoints)
        {
            if (critChanceBasisPoints <= 0) return false;
            if (critChanceBasisPoints >= BasisPointsPerWhole) return true;

            ulong mixed = Mix(seed);
            return (mixed % (ulong)BasisPointsPerWhole) < (ulong)critChanceBasisPoints;
        }

        public static long CalculateDamage(CombatInputs attacker, CombatInputs defender, ulong seed)
        {
            long raw = Math.Max(1L, attacker.Attack - defender.Defense);
            if (!RollCritical(seed, attacker.CritChanceBasisPoints)) return raw;

            checked
            {
                return Math.Max(1L, raw * attacker.CritMultiplierBasisPoints / BasisPointsPerWhole);
            }
        }

        private static long ReadNonNegative(IReadOnlyDictionary<string, long> stats, string id)
        {
            if (!stats.TryGetValue(id, out long value)) return 0;
            if (value < 0) throw new InvalidOperationException($"Combat stat '{id}' cannot be negative.");
            return value;
        }

        private static long Clamp(long value, long min, long max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static ulong Mix(ulong value)
        {
            value ^= value >> 30;
            value *= 0xbf58476d1ce4e5b9UL;
            value ^= value >> 27;
            value *= 0x94d049bb133111ebUL;
            value ^= value >> 31;
            return value;
        }
    }

    public readonly struct CombatInputs
    {
        public CombatInputs(long attack, long defense, long maxHealth, long critChanceBasisPoints, long critMultiplierBasisPoints)
        {
            if (attack < 0 || defense < 0 || maxHealth < 0 || critChanceBasisPoints < 0 || critMultiplierBasisPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(attack), "Combat inputs must be non-negative and have a positive critical multiplier.");

            Attack = attack;
            Defense = defense;
            MaxHealth = maxHealth;
            CritChanceBasisPoints = critChanceBasisPoints;
            CritMultiplierBasisPoints = critMultiplierBasisPoints;
        }

        public long Attack { get; }
        public long Defense { get; }
        public long MaxHealth { get; }
        public long CritChanceBasisPoints { get; }
        public long CritMultiplierBasisPoints { get; }
    }
}
