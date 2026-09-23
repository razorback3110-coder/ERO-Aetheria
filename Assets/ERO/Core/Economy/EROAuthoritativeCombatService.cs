using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Applies resolved authoritative combat inputs to persistent combatant state.
    /// This is intentionally Unity-free so the same rules can run on a dedicated server.
    /// </summary>
    public sealed class EROAuthoritativeCombatService
    {
        private const int SnapshotVersion = 1;
        private readonly Dictionary<string, long> _healthByActor = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, CombatHitResult> _resolvedAttackCommands = new Dictionary<string, CombatHitResult>(StringComparer.Ordinal);

        public CombatHitResult ApplyAttack(
            string attackerId,
            EROCharacterCombatStats attackerStats,
            string defenderId,
            EROCharacterCombatStats defenderStats,
            ulong seed)
        {
            return ApplyAttackInternal(attackerId, attackerStats, defenderId, defenderStats, seed);
        }

        /// <summary>
        /// Applies one client attack command exactly once for the lifetime of this authoritative service.
        /// Replaying the same command id returns the original authoritative result without applying damage again.
        /// The command journal can be persisted with CaptureAttackCommandJournal/RestoreAttackCommandJournal.
        /// </summary>
        public CombatHitResult ApplyAttackCommand(
            string attackCommandId,
            string attackerId,
            EROCharacterCombatStats attackerStats,
            string defenderId,
            EROCharacterCombatStats defenderStats,
            ulong seed)
        {
            ValidateCommandId(attackCommandId);
            if (_resolvedAttackCommands.TryGetValue(attackCommandId, out CombatHitResult previousResult))
                return previousResult;

            CombatHitResult result = ApplyAttackInternal(attackerId, attackerStats, defenderId, defenderStats, seed);
            _resolvedAttackCommands.Add(attackCommandId, result);
            return result;
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

        public CombatHealthSnapshot CaptureSnapshot()
        {
            var entries = new List<CombatHealthEntry>(_healthByActor.Count);
            foreach (KeyValuePair<string, long> pair in _healthByActor)
            {
                entries.Add(new CombatHealthEntry(pair.Key, pair.Value));
            }
            entries.Sort((left, right) => string.CompareOrdinal(left.ActorId, right.ActorId));
            return new CombatHealthSnapshot(SnapshotVersion, entries);
        }

        public void RestoreSnapshot(CombatHealthSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != SnapshotVersion)
                throw new InvalidOperationException($"Unsupported combat health snapshot version: {snapshot.Version}.");

            var restored = new Dictionary<string, long>(StringComparer.Ordinal);
            string previousActorId = null;
            foreach (CombatHealthEntry entry in snapshot.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.ActorId))
                    throw new InvalidOperationException("Combat health snapshot contains an invalid actor id.");
                if (entry.Health < 0)
                    throw new InvalidOperationException("Combat health snapshot contains negative health.");
                if (previousActorId != null && string.CompareOrdinal(previousActorId, entry.ActorId) >= 0)
                    throw new InvalidOperationException("Combat health snapshot entries must be unique and ordinally sorted.");
                if (!restored.TryAdd(entry.ActorId, entry.Health))
                    throw new InvalidOperationException($"Duplicate combat health actor '{entry.ActorId}'.");
                previousActorId = entry.ActorId;
            }

            _healthByActor.Clear();
            foreach (KeyValuePair<string, long> pair in restored)
            {
                _healthByActor.Add(pair.Key, pair.Value);
            }
        }

        public CombatAttackCommandJournalSnapshot CaptureAttackCommandJournal()
        {
            var entries = new List<CombatAttackCommandEntry>(_resolvedAttackCommands.Count);
            foreach (KeyValuePair<string, CombatHitResult> pair in _resolvedAttackCommands)
            {
                entries.Add(new CombatAttackCommandEntry(pair.Key, pair.Value));
            }
            entries.Sort((left, right) => string.CompareOrdinal(left.CommandId, right.CommandId));
            return new CombatAttackCommandJournalSnapshot(SnapshotVersion, entries);
        }

        public void RestoreAttackCommandJournal(CombatAttackCommandJournalSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != SnapshotVersion)
                throw new InvalidOperationException($"Unsupported combat attack journal version: {snapshot.Version}.");

            var restored = new Dictionary<string, CombatHitResult>(StringComparer.Ordinal);
            string previousCommandId = null;
            foreach (CombatAttackCommandEntry entry in snapshot.Entries)
            {
                ValidateCommandId(entry.CommandId);
                if (previousCommandId != null && string.CompareOrdinal(previousCommandId, entry.CommandId) >= 0)
                    throw new InvalidOperationException("Combat attack journal entries must be unique and ordinally sorted.");
                if (!restored.TryAdd(entry.CommandId, entry.Result))
                    throw new InvalidOperationException($"Duplicate combat attack command '{entry.CommandId}'.");
                previousCommandId = entry.CommandId;
            }

            _resolvedAttackCommands.Clear();
            foreach (KeyValuePair<string, CombatHitResult> pair in restored)
            {
                _resolvedAttackCommands.Add(pair.Key, pair.Value);
            }
        }

        private CombatHitResult ApplyAttackInternal(
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

            // An attacker must have authoritative live state before damage can be applied.
            // This prevents a defeated client from continuing to issue valid-looking attacks.
            EnsureHealthInitialized(attackerId, attacker.MaxHealth);
            if (_healthByActor[attackerId] <= 0)
                return CombatHitResult.AttackerDefeated(attackerId, defenderId);

            EnsureHealthInitialized(defenderId, defender.MaxHealth);

            long currentHealth = _healthByActor[defenderId];
            if (currentHealth <= 0)
                return CombatHitResult.AlreadyDefeated(attackerId, defenderId);

            long damage = EROCombatStatsResolver.CalculateDamage(attacker, defender, seed);
            long newHealth = currentHealth <= damage ? 0 : currentHealth - damage;
            _healthByActor[defenderId] = newHealth;

            bool critical = EROCombatStatsResolver.RollCritical(seed, attacker.CritChanceBasisPoints);
            return new CombatHitResult(attackerId, defenderId, damage, newHealth, critical, newHealth == 0, false, false);
        }

        private static void ValidateCommandId(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("Attack command id is required.", nameof(commandId));
            if (commandId.Length > 128)
                throw new ArgumentException("Attack command id is too long.", nameof(commandId));
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
        private CombatHitResult(string attackerId, string defenderId, long damage, long remainingHealth, bool critical, bool defeated, bool alreadyDefeated, bool attackerDefeated)
        {
            AttackerId = attackerId ?? throw new ArgumentNullException(nameof(attackerId));
            DefenderId = defenderId ?? throw new ArgumentNullException(nameof(defenderId));
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (remainingHealth < 0) throw new ArgumentOutOfRangeException(nameof(remainingHealth));

            Damage = damage;
            RemainingHealth = remainingHealth;
            Critical = critical;
            Defeated = defeated;
            AlreadyDefeated = alreadyDefeated;
            AttackerDefeated = attackerDefeated;
        }

        public CombatHitResult(string attackerId, string defenderId, long damage, long remainingHealth, bool critical, bool defeated)
            : this(attackerId, defenderId, damage, remainingHealth, critical, defeated, false, false)
        {
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
        }

        public static CombatHitResult AlreadyDefeated(string attackerId, string defenderId)
        {
            return new CombatHitResult(attackerId, defenderId, 0, 0, false, true, true, false);
        }

        public static CombatHitResult AttackerDefeated(string attackerId, string defenderId)
        {
            return new CombatHitResult(attackerId, defenderId, 0, 0, false, false, false, true);
        }

        public string AttackerId { get; }
        public string DefenderId { get; }
        public long Damage { get; }
        public long RemainingHealth { get; }
        public bool Critical { get; }
        public bool Defeated { get; }
        public bool AlreadyDefeated { get; }
        public bool AttackerDefeated { get; }
    }

    public sealed class CombatHealthSnapshot
    {
        public CombatHealthSnapshot(int version, IReadOnlyList<CombatHealthEntry> entries)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Version = version;
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public int Version { get; }
        public IReadOnlyList<CombatHealthEntry> Entries { get; }
    }

    public readonly struct CombatHealthEntry
    {
        public CombatHealthEntry(string actorId, long health)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor id is required.", nameof(actorId));
            if (health < 0) throw new ArgumentOutOfRangeException(nameof(health));
            ActorId = actorId;
            Health = health;
        }

        public string ActorId { get; }
        public long Health { get; }
    }

    public sealed class CombatAttackCommandJournalSnapshot
    {
        public CombatAttackCommandJournalSnapshot(int version, IReadOnlyList<CombatAttackCommandEntry> entries)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Version = version;
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public int Version { get; }
        public IReadOnlyList<CombatAttackCommandEntry> Entries { get; }
    }

    public readonly struct CombatAttackCommandEntry
    {
        public CombatAttackCommandEntry(string commandId, CombatHitResult result)
        {
            ValidateCommandId(commandId);
            CommandId = commandId;
            Result = result;
        }

        public string CommandId { get; }
        public CombatHitResult Result { get; }

        private static void ValidateCommandId(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId)) throw new ArgumentException("Attack command id is required.", nameof(commandId));
            if (commandId.Length > 128) throw new ArgumentException("Attack command id is too long.", nameof(commandId));
        }
    }
}