using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    public readonly struct EROCombatReward
    {
        public readonly ulong TickId;
        public readonly ulong Sequence;
        public readonly ulong RecipientId;
        public readonly ulong DefeatedActorId;
        public readonly int Experience;
        public readonly int LootRoll;

        public EROCombatReward(ulong tickId, ulong sequence, ulong recipientId, ulong defeatedActorId, int experience, int lootRoll)
        {
            TickId = tickId;
            Sequence = sequence;
            RecipientId = recipientId;
            DefeatedActorId = defeatedActorId;
            Experience = Math.Max(0, experience);
            LootRoll = Math.Max(0, Math.Min(999999, lootRoll));
        }
    }

    /// <summary>
    /// Deterministic post-combat reward ledger. It converts a defeated target into
    /// one idempotent XP/loot outcome suitable for authoritative persistence and replication.
    /// </summary>
    public sealed class EROCombatRewardLedger
    {
        private readonly HashSet<RewardKey> appliedSequences;
        private readonly List<EROCombatReward> rewards;
        private readonly IEROCombatRewardStore persistence;
        private readonly object syncRoot = new object();

        public EROCombatRewardLedger(int expectedCapacity = 256, IEROCombatRewardStore persistence = null)
        {
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            appliedSequences = new HashSet<RewardKey>(expectedCapacity);
            rewards = new List<EROCombatReward>(expectedCapacity);
            this.persistence = persistence;
        }

        public int Count
        {
            get
            {
                lock (syncRoot) return rewards.Count;
            }
        }

        public bool TryApply(EROCombatResult result, EROCombatantState defeated, out EROCombatReward reward)
        {
            lock (syncRoot)
            {
                return TryApplyLocked(result, defeated, out reward);
            }
        }

        private bool TryApplyLocked(EROCombatResult result, EROCombatantState defeated, out EROCombatReward reward)
        {
            reward = default;
            if (!result.TargetDefeated || result.TargetId != defeated.ActorId) return false;

            RewardKey key = new RewardKey(result.ActorId, result.Sequence);
            if (appliedSequences.Contains(key)) return false;

            if (persistence != null && persistence.TryLoad(result.ActorId, result.Sequence, out EROCombatRewardSnapshot stored))
            {
                reward = stored.Reward;
                appliedSequences.Add(key);
                rewards.Add(reward);
                return true;
            }

            int levelDelta = Math.Max(0, defeated.Level);
            int experience = checked(Math.Max(1, levelDelta * 25));
            int lootRoll = RollLoot(result.TickId, result.Sequence, result.ActorId, result.TargetId, result.SkillId);
            EROCombatReward generated = new EROCombatReward(result.TickId, result.Sequence, result.ActorId, result.TargetId, experience, lootRoll);
            EROCombatRewardSnapshot snapshot = new EROCombatRewardSnapshot(generated);

            // Prefer an atomic insert when the backing store supports it. A concurrent
            // worker that loses the insert race reloads the already-committed reward,
            // preventing duplicate XP/loot generation in a multi-worker server.
            if (persistence is IEROAtomicCombatRewardStore atomicStore && !atomicStore.TrySaveIfAbsent(snapshot))
            {
                if (!persistence.TryLoad(result.ActorId, result.Sequence, out stored)) return false;
                reward = stored.Reward;
                appliedSequences.Add(key);
                rewards.Add(reward);
                return true;
            }

            if (persistence != null && !(persistence is IEROAtomicCombatRewardStore))
                persistence.Save(snapshot);

            reward = generated;
            appliedSequences.Add(key);
            rewards.Add(reward);
            return true;
        }

        public bool TryGetLatest(out EROCombatReward reward)
        {
            lock (syncRoot)
            {
                if (rewards.Count == 0)
                {
                    reward = default;
                    return false;
                }
                reward = rewards[rewards.Count - 1];
                return true;
            }
        }

        private static int RollLoot(ulong tickId, ulong sequence, ulong actorId, ulong targetId, int skillId)
        {
            ulong value = tickId ^ (sequence * 0x9E3779B97F4A7C15UL) ^ actorId ^ (targetId << 1) ^ (uint)skillId;
            value ^= value >> 30;
            value *= 0xbf58476d1ce4e5b9UL;
            value ^= value >> 27;
            value *= 0x94d049bb133111ebUL;
            value ^= value >> 31;
            return (int)(value % 1000000UL);
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
