using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>Versioned durable record for an authoritative combat reward.</summary>
    public readonly struct EROCombatRewardSnapshot
    {
        public const int CurrentVersion = 1;
        public readonly int Version;
        public readonly ulong ActorId;
        public readonly ulong Sequence;
        public readonly EROCombatReward Reward;

        public EROCombatRewardSnapshot(EROCombatReward reward)
        {
            if (reward.RecipientId == 0UL) throw new ArgumentOutOfRangeException(nameof(reward));
            Version = CurrentVersion;
            ActorId = reward.RecipientId;
            Sequence = reward.Sequence;
            Reward = reward;
        }
    }

    /// <summary>Persistence boundary for replay-safe combat rewards.</summary>
    public interface IEROCombatRewardStore
    {
        bool TryLoad(ulong actorId, ulong sequence, out EROCombatRewardSnapshot snapshot);
        void Save(EROCombatRewardSnapshot snapshot);
    }

    /// <summary>
    /// Optional stronger persistence contract for stores that can atomically insert a reward
    /// only when the (ActorId, Sequence) key is still unused. This prevents duplicate rewards
    /// when multiple authoritative workers process the same combat completion concurrently.
    /// </summary>
    public interface IEROAtomicCombatRewardStore : IEROCombatRewardStore
    {
        bool TrySaveIfAbsent(EROCombatRewardSnapshot snapshot);
    }

    /// <summary>In-process store for local and dedicated-server bootstrap/testing.</summary>
    public sealed class EROInMemoryCombatRewardStore : IEROAtomicCombatRewardStore
    {
        private readonly Dictionary<RewardKey, EROCombatRewardSnapshot> snapshots;

        public EROInMemoryCombatRewardStore(int expectedCapacity = 256)
        {
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            snapshots = new Dictionary<RewardKey, EROCombatRewardSnapshot>(expectedCapacity);
        }

        public int Count => snapshots.Count;

        public bool TryLoad(ulong actorId, ulong sequence, out EROCombatRewardSnapshot snapshot)
            => snapshots.TryGetValue(new RewardKey(actorId, sequence), out snapshot);

        public bool TrySaveIfAbsent(EROCombatRewardSnapshot snapshot)
        {
            Validate(snapshot);
            RewardKey key = new RewardKey(snapshot.ActorId, snapshot.Sequence);
            if (snapshots.ContainsKey(key)) return false;
            snapshots.Add(key, snapshot);
            return true;
        }

        public void Save(EROCombatRewardSnapshot snapshot)
        {
            Validate(snapshot);
            snapshots[new RewardKey(snapshot.ActorId, snapshot.Sequence)] = snapshot;
        }

        private static void Validate(EROCombatRewardSnapshot snapshot)
        {
            if (snapshot.Version != EROCombatRewardSnapshot.CurrentVersion)
                throw new InvalidOperationException("Unsupported ERO combat reward snapshot version.");
            if (snapshot.ActorId == 0UL || snapshot.Sequence != snapshot.Reward.Sequence || snapshot.ActorId != snapshot.Reward.RecipientId)
                throw new ArgumentOutOfRangeException(nameof(snapshot));
        }

        private readonly struct RewardKey : IEquatable<RewardKey>
        {
            private readonly ulong actorId;
            private readonly ulong sequence;

            public RewardKey(ulong actorId, ulong sequence)
            {
                this.actorId = actorId;
                this.sequence = sequence;
            }

            public bool Equals(RewardKey other) => actorId == other.actorId && sequence == other.sequence;
            public override bool Equals(object obj) => obj is RewardKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(actorId, sequence);
        }
    }
}
