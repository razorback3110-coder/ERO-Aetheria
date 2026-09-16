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
        private readonly HashSet<ulong> appliedSequences;
        private readonly List<EROCombatReward> rewards;

        public EROCombatRewardLedger(int expectedCapacity = 256)
        {
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            appliedSequences = new HashSet<ulong>(expectedCapacity);
            rewards = new List<EROCombatReward>(expectedCapacity);
        }

        public int Count => rewards.Count;

        public bool TryApply(EROCombatResult result, EROCombatantState defeated, out EROCombatReward reward)
        {
            reward = default;
            if (!result.TargetDefeated || result.TargetId != defeated.ActorId) return false;
            if (!appliedSequences.Add(result.Sequence)) return false;

            int levelDelta = Math.Max(0, defeated.Level);
            int experience = checked(Math.Max(1, levelDelta * 25));
            int lootRoll = RollLoot(result.TickId, result.Sequence, result.ActorId, result.TargetId, result.SkillId);
            reward = new EROCombatReward(result.TickId, result.Sequence, result.ActorId, result.TargetId, experience, lootRoll);
            rewards.Add(reward);
            return true;
        }

        public bool TryGetLatest(out EROCombatReward reward)
        {
            if (rewards.Count == 0)
            {
                reward = default;
                return false;
            }
            reward = rewards[rewards.Count - 1];
            return true;
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
    }
}
