using System;

namespace ERO.Systems
{
    public readonly struct EROCombatantState
    {
        public readonly ulong ActorId;
        public readonly int Level;
        public readonly int Attack;
        public readonly int Defense;
        public readonly int CritChancePercent;
        public readonly int CritMultiplierPercent;
        public readonly int MaxHealth;
        public readonly int Health;

        public EROCombatantState(ulong actorId, int level, int attack, int defense, int critChancePercent, int critMultiplierPercent, int maxHealth, int health)
        {
            ActorId = actorId;
            Level = Math.Max(1, level);
            Attack = Math.Max(0, attack);
            Defense = Math.Max(0, defense);
            CritChancePercent = ClampPercent(critChancePercent);
            CritMultiplierPercent = Math.Max(100, critMultiplierPercent);
            MaxHealth = Math.Max(1, maxHealth);
            Health = Math.Max(0, Math.Min(maxHealth, health));
        }

        private static int ClampPercent(int value) => Math.Max(0, Math.Min(100, value));
    }

    public readonly struct EROCombatSkill
    {
        public readonly int SkillId;
        public readonly int Power;
        public readonly int AccuracyPercent;
        public readonly bool CanCrit;
        public readonly ulong CooldownTicks;

        public EROCombatSkill(int skillId, int power, int accuracyPercent = 100, bool canCrit = true, ulong cooldownTicks = 0UL)
        {
            SkillId = skillId;
            Power = Math.Max(0, power);
            AccuracyPercent = Math.Max(0, Math.Min(100, accuracyPercent));
            CanCrit = canCrit;
            CooldownTicks = cooldownTicks;
        }
    }

    public readonly struct EROCombatResult
    {
        public readonly ulong TickId;
        public readonly ulong Sequence;
        public readonly ulong ActorId;
        public readonly ulong TargetId;
        public readonly int SkillId;
        public readonly int Damage;
        public readonly int TargetHealth;
        public readonly bool Hit;
        public readonly bool Critical;
        public readonly bool TargetDefeated;

        public EROCombatResult(ulong tickId, ulong sequence, ulong actorId, ulong targetId, int skillId, int damage, int targetHealth, bool hit, bool critical, bool targetDefeated)
        {
            TickId = tickId;
            Sequence = sequence;
            ActorId = actorId;
            TargetId = targetId;
            SkillId = skillId;
            Damage = damage;
            TargetHealth = targetHealth;
            Hit = hit;
            Critical = critical;
            TargetDefeated = targetDefeated;
        }
    }

    /// <summary>
    /// Pure deterministic combat kernel. No Unity dependency: suitable for a dedicated server
    /// and deterministic tests. The caller owns authoritative combatant state and applies the result.
    /// </summary>
    public static class EROCombatResolution
    {
        public static EROCombatResult Resolve(ulong tickId, ulong sequence, EROCombatantState attacker, EROCombatantState target, EROCombatSkill skill, ulong seed)
        {
            if (attacker.ActorId == 0UL) throw new ArgumentException("Attacker must have a valid id.", nameof(attacker));
            if (target.ActorId == 0UL) throw new ArgumentException("Target must have a valid id.", nameof(target));
            if (skill.SkillId < 0) throw new ArgumentOutOfRangeException(nameof(skill));

            ulong rng = Mix(seed ^ tickId ^ sequence ^ attacker.ActorId ^ (target.ActorId << 1) ^ (uint)skill.SkillId);
            int accuracyRoll = NextPercent(ref rng);
            bool hit = accuracyRoll < skill.AccuracyPercent;
            if (!hit)
                return new EROCombatResult(tickId, sequence, attacker.ActorId, target.ActorId, skill.SkillId, 0, target.Health, false, false, false);

            bool critical = skill.CanCrit && NextPercent(ref rng) < attacker.CritChancePercent;
            long raw = (long)attacker.Attack + skill.Power;
            if (critical)
                raw = raw * attacker.CritMultiplierPercent / 100L;

            int mitigation = Math.Max(0, target.Defense);
            int damage = Math.Max(1, SaturateToInt(raw - mitigation));
            int remaining = Math.Max(0, target.Health - damage);
            return new EROCombatResult(tickId, sequence, attacker.ActorId, target.ActorId, skill.SkillId, damage, remaining, true, critical, remaining == 0);
        }

        private static int NextPercent(ref ulong state)
        {
            state = Mix(state);
            return (int)(state % 100UL);
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

        private static int SaturateToInt(long value)
        {
            if (value <= 0) return 0;
            return value >= int.MaxValue ? int.MaxValue : (int)value;
        }
    }
}
