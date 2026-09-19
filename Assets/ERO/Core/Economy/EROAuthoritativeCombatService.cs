using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Applies the resolved authoritative combat inputs to persistent combatant state.
    /// This is intentionally Unity-free so the same rules can run on a dedicated server.
    /// </summary>
    public sealed class EROAuthoritativeCombatService
    {
        private readonly Dictionary<string, long> _healthByActor = new Dictionary<string, long>(StringComparer.Ordinal);

        public CombatHitResult ApplyAttack(
            string attackerId,
            EROCharacterCombatStats attackerStats,
            string defenderId,
            EROCharacterCombatStats defenderStats,
            ulong seed)
        {
            if (string.IsNullOrWhiteSpace(attackerId)) throw new ArgumentException("Attacker id is required.", nameof(attackerId));
            if (string.IsNullOrWhiteSpace(defenderId)) throw new ArgumentException("Defender id is required.", nameof(defenderId));
            if (attackerStats == null) throw new ArgumentNullException(nameof(attackerStats));
            if (defenderStats == null) throw new ArgumentNullException(nameof(defenderStats));
            if (string.Equals(attackerId, defenderId, StringComparison.Ordinal))
                throw new InvalidOperationException("An actor cannot attack itself.");

            CombatInputs attacker = EROCombatStatsResolver.Resolve(attackerStats);
            CombatInputs defender = EROCombatStatsResolver.Resolve(defenderStats);
            EnsureHealthInitialized(defenderId, defender.MaxHealth);

            long damage = EROCombatStatsResolver.CalculateDamage(attacker, defender, seed);
            long currentHealth = _healthByActor[defenderId];
            long newHealth = currentHealth <= damage ? 0 : currentHealth - damage;
            _healthByActor[defenderId] = newHealth;

            bool critical = EROCombatStatsResolver.RollCritical(seed, attacker.CritChanceBasisPoints);
            return new CombatHitResult(attackerId, defenderId, damage, newHealth, critical, newHealth == 0);
        }

        public long GetCurrentHealth(string actorId, EROCharacterCombatStats stats)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor id is required.", nameof(actorId));
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            CombatInputs inputs = EROCombatStatsResolver.Resolve(stats);
            EnsureHealthInitialized(actorId, inputs.MaxHealth);
            return _healthByActor[actorId];
        }

        public void RestoreHealth(string actorId, EROCharacterCombatStats stats)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor id is required.", nameof(actorId));
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            CombatInputs inputs = EROCombatStatsResolver.Resolve(stats);
            _healthByActor[actorId] = inputs.MaxHealth;
        }

        private void EnsureHealthInitialized(string actorId, long maxHealth)
        {
            if (!_healthByActor.TryGetValue(actorId, out long health))
            {
                _healthByActor[actorId] = maxHealth;
                return;
            }

            if (health > maxHealth) _healthByActor[actorId] = maxHealth;
            else if (health < 0) _healthByActor[actorId] = 0;
        }
    }

    public readonly struct CombatHitResult
    {
        public CombatHitResult(string attackerId, string defenderId, long damage, long remainingHealth, bool critical, bool defeated)
        {
            AttackerId = attackerId ?? throw new ArgumentNullException(nameof(attackerId));
            DefenderId = defenderId ?? throw new ArgumentNullException(nameof(defenderId));
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (remainingHealth < 0) throw new ArgumentOutOfRangeException(nameof(remainingHealth));

            Damage = damage;
            RemainingHealth = remainingHealth;
            Critical = critical;
            Defeated = defeated;
        }

        public string AttackerId { get; }
        public string DefenderId { get; }
        public long Damage { get; }
        public long RemainingHealth { get; }
        public bool Critical { get; }
        public bool Defeated { get; }
    }
}
