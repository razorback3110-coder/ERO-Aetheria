using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative aggregation of persistent character base stats and equipped item bonuses.
    /// This class is deliberately engine-independent so dedicated servers can use the same rules as the client.
    /// </summary>
    public sealed class EROCharacterCombatStats
    {
        public const int CurrentSnapshotVersion = 1;

        private readonly object sync = new object();
        private readonly Dictionary<string, long> baseStats = new Dictionary<string, long>(StringComparer.Ordinal);

        public EROCharacterCombatStats(string actorId, EROEquipmentLoadout equipment)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            if (!string.Equals(actorId, equipment.ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Combat stats actor must match equipment actor.");
            ActorId = actorId;
        }

        public string ActorId { get; }
        public EROEquipmentLoadout Equipment { get; }

        public void SetBaseStat(string statId, long value)
        {
            ValidateStatId(statId);
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Base stats cannot be negative.");
            lock (sync) baseStats[statId] = value;
        }

        public bool TryGetBaseStat(string statId, out long value)
        {
            ValidateStatId(statId);
            lock (sync) return baseStats.TryGetValue(statId, out value);
        }

        public IReadOnlyDictionary<string, long> CalculateFinalStats()
        {
            var result = new Dictionary<string, long>(StringComparer.Ordinal);
            lock (sync)
            {
                foreach (var pair in baseStats) result[pair.Key] = pair.Value;
            }

            foreach (var pair in Equipment.CalculateBonusStats())
            {
                result.TryGetValue(pair.Key, out var current);
                result[pair.Key] = checked(current + pair.Value);
            }

            return result;
        }

        public long GetFinalStat(string statId)
        {
            ValidateStatId(statId);
            var finalStats = CalculateFinalStats();
            return finalStats.TryGetValue(statId, out var value) ? value : 0L;
        }

        public CombatStatSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return new CombatStatSnapshot(
                    CurrentSnapshotVersion,
                    ActorId,
                    new Dictionary<string, long>(baseStats, StringComparer.Ordinal));
            }
        }

        public void RestoreSnapshot(CombatStatSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion) throw new InvalidOperationException("Unsupported combat stat snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal)) throw new InvalidOperationException("Combat stats actor mismatch.");

            lock (sync)
            {
                baseStats.Clear();
                foreach (var pair in snapshot.BaseStats)
                {
                    ValidateStatId(pair.Key);
                    if (pair.Value < 0) throw new InvalidOperationException("Combat stat snapshot contains a negative base stat.");
                    baseStats.Add(pair.Key, pair.Value);
                }
            }
        }

        private static void ValidateStatId(string statId)
        {
            if (string.IsNullOrWhiteSpace(statId)) throw new ArgumentException("StatId is required.", nameof(statId));
            if (statId.Length > 64) throw new ArgumentException("StatId is too long.", nameof(statId));
        }
    }

    public sealed class CombatStatSnapshot
    {
        public CombatStatSnapshot(int version, string actorId, IReadOnlyDictionary<string, long> baseStats)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            BaseStats = baseStats ?? throw new ArgumentNullException(nameof(baseStats));
        }

        public int Version { get; }
        public string ActorId { get; }
        public IReadOnlyDictionary<string, long> BaseStats { get; }
    }
}
