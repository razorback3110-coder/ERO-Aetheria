using System;

namespace EternalRealmsOnline.Core
{
    public readonly struct EROCombatStats
    {
        public readonly int Attack;
        public readonly int Defense;
        public readonly int MaxHealth;
        public readonly int Health;
        public readonly int CriticalChanceBps;
        public readonly int CriticalMultiplierBps;

        public EROCombatStats(
            int attack,
            int defense,
            int maxHealth,
            int health,
            int criticalChanceBps = 0,
            int criticalMultiplierBps = 15000)
        {
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack));
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (health < 0 || health > maxHealth) throw new ArgumentOutOfRangeException(nameof(health));
            if (criticalChanceBps < 0 || criticalChanceBps > 10000) throw new ArgumentOutOfRangeException(nameof(criticalChanceBps));
            if (criticalMultiplierBps < 10000) throw new ArgumentOutOfRangeException(nameof(criticalMultiplierBps));

            Attack = attack;
            Defense = defense;
            MaxHealth = maxHealth;
            Health = health;
            CriticalChanceBps = criticalChanceBps;
            CriticalMultiplierBps = criticalMultiplierBps;
        }

        public EROCombatStats WithHealth(int health)
        {
            return new EROCombatStats(Attack, Defense, MaxHealth, health, CriticalChanceBps, CriticalMultiplierBps);
        }
    }

    public readonly struct EROCombatResult
    {
        public readonly int RawDamage;
        public readonly int FinalDamage;
        public readonly int RemainingHealth;
        public readonly bool Critical;
        public readonly bool Defeated;

        public EROCombatResult(int rawDamage, int finalDamage, int remainingHealth, bool critical, bool defeated)
        {
            RawDamage = rawDamage;
            FinalDamage = finalDamage;
            RemainingHealth = remainingHealth;
            Critical = critical;
            Defeated = defeated;
        }
    }

    /// <summary>
    /// Deterministic, server-safe combat resolution. No Unity or asset dependency.
    /// Randomness is supplied by the authoritative server so clients never decide damage.
    /// </summary>
    public static class EROCombatResolver
    {
        public static EROCombatResult ResolveBasicAttack(
            in EROCombatStats attacker,
            in EROCombatStats defender,
            int randomBasis)
        {
            if (defender.Health <= 0)
                throw new InvalidOperationException("A defeated target cannot receive damage.");

            int rawDamage = Math.Max(1, attacker.Attack - defender.Defense);
            bool critical = attacker.CriticalChanceBps > 0 && Mod(randomBasis, 10000) < attacker.CriticalChanceBps;
            if (critical)
                rawDamage = MultiplyBps(rawDamage, attacker.CriticalMultiplierBps);

            int finalDamage = Math.Min(rawDamage, defender.Health);
            int remainingHealth = defender.Health - finalDamage;
            return new EROCombatResult(rawDamage, finalDamage, remainingHealth, critical, remainingHealth == 0);
        }

        public static int ComputeSkillDamage(int power, int skillMultiplierBps, int defense)
        {
            if (power < 0) throw new ArgumentOutOfRangeException(nameof(power));
            if (skillMultiplierBps < 0) throw new ArgumentOutOfRangeException(nameof(skillMultiplierBps));
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense));

            int scaledPower = MultiplyBps(power, skillMultiplierBps);
            return Math.Max(1, scaledPower - defense);
        }

        private static int MultiplyBps(int value, int basisPoints)
        {
            long scaled = (long)value * basisPoints;
            long rounded = (scaled + 9999L) / 10000L;
            return rounded >= int.MaxValue ? int.MaxValue : (int)Math.Max(1L, rounded);
        }

        private static int Mod(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
