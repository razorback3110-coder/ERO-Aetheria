using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Converts an authoritative defeat into idempotent XP and loot rewards.
    /// Reward identity is derived from the encounter, so retries after reconnects cannot duplicate rewards.
    /// </summary>
    public sealed class EROCombatRewards
    {
        private const int SnapshotVersion = 1;
        private readonly HashSet<string> claimedRewards = new HashSet<string>(StringComparer.Ordinal);
        private readonly EROCharacterProgression progression;
        private readonly EROLootInventoryService loot;

        public EROCombatRewards(EROCharacterProgression progression, EROLootInventoryService loot)
        {
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
            this.loot = loot ?? throw new ArgumentNullException(nameof(loot));
        }

        public CombatRewardResult GrantDefeatRewards(
            string encounterId,
            long encounterSeed,
            string defeatedActorId,
            long experience,
            int dropIndex,
            string itemId,
            int quantity,
            int maxStack,
            int itemLevel,
            IReadOnlyDictionary<string, long> itemStats = null)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            if (string.IsNullOrWhiteSpace(defeatedActorId)) throw new ArgumentException("Defeated actor id is required.", nameof(defeatedActorId));
            if (!string.Equals(defeatedActorId, defeatedActorId, StringComparison.Ordinal) && string.IsNullOrWhiteSpace(defeatedActorId))
                throw new InvalidOperationException("Invalid defeated actor identity.");
            if (experience < 0) throw new ArgumentOutOfRangeException(nameof(experience));

            string rewardId = BuildRewardId(encounterId, encounterSeed, defeatedActorId);
            if (!claimedRewards.Add(rewardId))
                return CombatRewardResult.AlreadyGranted(rewardId, progression.Level, progression.Experience);

            ProgressionResult progressionResult = progression.GrantExperience(experience);
            LootGrantResult lootResult = loot.Grant(
                encounterId,
                encounterSeed,
                dropIndex,
                itemId,
                quantity,
                maxStack,
                itemLevel,
                itemStats);

            if (!lootResult.Success)
            {
                // The reward transaction remains claimed: the encounter cannot be replayed to
                // mint XP again. A later recovery job can re-issue the item using the deterministic
                // loot transaction id without replaying progression.
                return new CombatRewardResult(
                    CombatRewardStatus.LootPending,
                    rewardId,
                    progressionResult,
                    lootResult);
            }

            return new CombatRewardResult(
                CombatRewardStatus.Granted,
                rewardId,
                progressionResult,
                lootResult);
        }

        public RewardSnapshot CaptureSnapshot()
        {
            var ids = new List<string>(claimedRewards);
            ids.Sort(StringComparer.Ordinal);
            return new RewardSnapshot(SnapshotVersion, ids);
        }

        public void RestoreSnapshot(RewardSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != SnapshotVersion)
                throw new InvalidOperationException("Unsupported reward snapshot version.");

            var restored = new HashSet<string>(StringComparer.Ordinal);
            string previous = null;
            foreach (string id in snapshot.ClaimedRewardIds)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException("Reward snapshot contains an invalid reward id.");
                if (previous != null && string.CompareOrdinal(previous, id) >= 0)
                    throw new InvalidOperationException("Reward snapshot ids must be unique and ordinally sorted.");
                if (!restored.Add(id))
                    throw new InvalidOperationException("Reward snapshot contains a duplicate reward id.");
                previous = id;
            }

            claimedRewards.Clear();
            foreach (string id in restored) claimedRewards.Add(id);
        }

        public static string BuildRewardId(string encounterId, long encounterSeed, string defeatedActorId)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            if (string.IsNullOrWhiteSpace(defeatedActorId)) throw new ArgumentException("Defeated actor id is required.", nameof(defeatedActorId));
            return "reward:" + encounterId + ":" + encounterSeed.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + defeatedActorId;
        }
    }

    public enum CombatRewardStatus
    {
        Granted = 1,
        AlreadyGranted = 2,
        LootPending = 3
    }

    public readonly struct CombatRewardResult
    {
        public CombatRewardResult(CombatRewardStatus status, string rewardId, ProgressionResult progression, LootGrantResult loot)
        {
            Status = status;
            RewardId = rewardId ?? throw new ArgumentNullException(nameof(rewardId));
            Progression = progression;
            Loot = loot;
        }

        public CombatRewardStatus Status { get; }
        public string RewardId { get; }
        public ProgressionResult Progression { get; }
        public LootGrantResult Loot { get; }
        public bool Success => Status == CombatRewardStatus.Granted || Status == CombatRewardStatus.AlreadyGranted;

        public static CombatRewardResult AlreadyGranted(string rewardId, int level, long experience)
        {
            return new CombatRewardResult(
                CombatRewardStatus.AlreadyGranted,
                rewardId,
                new ProgressionResult(level, level, experience, 0),
                default(LootGrantResult));
        }
    }

    public sealed class RewardSnapshot
    {
        public RewardSnapshot(int version, IReadOnlyList<string> claimedRewardIds)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Version = version;
            ClaimedRewardIds = claimedRewardIds ?? throw new ArgumentNullException(nameof(claimedRewardIds));
        }

        public int Version { get; }
        public IReadOnlyList<string> ClaimedRewardIds { get; }
    }
}
