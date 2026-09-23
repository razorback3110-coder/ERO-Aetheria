using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Converts an authoritative defeat into idempotent XP, loot and optional Gold rewards.
    /// Reward identity is derived from the encounter, so retries after reconnects cannot duplicate rewards.
    /// </summary>
    public sealed class EROCombatRewards
    {
        private const int SnapshotVersion = 1;
        private readonly HashSet<string> claimedRewards = new HashSet<string>(StringComparer.Ordinal);
        private readonly EROCharacterProgression progression;
        private readonly EROLootInventoryService loot;
        private readonly EROAuthoritativeWallet wallet;

        public EROCombatRewards(EROCharacterProgression progression, EROLootInventoryService loot)
            : this(progression, loot, null)
        {
        }

        public EROCombatRewards(
            EROCharacterProgression progression,
            EROLootInventoryService loot,
            EROAuthoritativeWallet wallet)
        {
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
            this.loot = loot ?? throw new ArgumentNullException(nameof(loot));
            this.wallet = wallet;
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
            IReadOnlyDictionary<string, long> itemStats = null,
            long gold = 0L)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            if (string.IsNullOrWhiteSpace(defeatedActorId)) throw new ArgumentException("Defeated actor id is required.", nameof(defeatedActorId));
            if (experience < 0) throw new ArgumentOutOfRangeException(nameof(experience));
            if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (gold > 0 && wallet == null)
                throw new InvalidOperationException("A wallet is required when a combat reward grants Gold.");

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
                // The encounter remains claimed so a retry cannot mint XP or Gold again.
                // The deterministic loot transaction id can be recovered independently.
                return new CombatRewardResult(
                    CombatRewardStatus.LootPending,
                    rewardId,
                    progressionResult,
                    lootResult,
                    0L);
            }

            if (gold > 0 && !wallet.TryApplyTransaction(
                rewardId + ":gold",
                defeatedActorId,
                EROCurrencyCatalog.Gold,
                gold,
                true))
            {
                // The reward remains claimed. The deterministic wallet transaction can be retried
                // independently through TryFinalizeGoldReward without minting XP or loot again.
                return new CombatRewardResult(
                    CombatRewardStatus.GoldPending,
                    rewardId,
                    progressionResult,
                    lootResult,
                    gold);
            }

            return new CombatRewardResult(
                CombatRewardStatus.Granted,
                rewardId,
                progressionResult,
                lootResult,
                gold);
        }

        /// <summary>
        /// Finalizes Gold for an already-claimed combat reward after a transient wallet failure.
        /// The transaction id is derived from the original reward id, so retries are idempotent.
        /// </summary>
        public bool TryFinalizeGoldReward(string rewardId, string ownerId, long gold)
        {
            if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("Reward id is required.", nameof(rewardId));
            if (!claimedRewards.Contains(rewardId))
                throw new InvalidOperationException("Gold cannot be finalized for an unclaimed reward.");
            if (gold <= 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (wallet == null) throw new InvalidOperationException("A wallet is required to finalize Gold.");

            return wallet.TryApplyTransaction(
                rewardId + ":gold",
                ownerId,
                EROCurrencyCatalog.Gold,
                gold,
                true);
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
        LootPending = 3,
        GoldPending = 4
    }

    public readonly struct CombatRewardResult
    {
        public CombatRewardResult(
            CombatRewardStatus status,
            string rewardId,
            ProgressionResult progression,
            LootGrantResult loot,
            long gold)
        {
            Status = status;
            RewardId = rewardId ?? throw new ArgumentNullException(nameof(rewardId));
            Progression = progression;
            Loot = loot;
            Gold = gold;
        }

        public CombatRewardStatus Status { get; }
        public string RewardId { get; }
        public ProgressionResult Progression { get; }
        public LootGrantResult Loot { get; }
        public long Gold { get; }
        public bool Success => Status == CombatRewardStatus.Granted || Status == CombatRewardStatus.AlreadyGranted;

        public static CombatRewardResult AlreadyGranted(string rewardId, int level, long experience)
        {
            return new CombatRewardResult(
                CombatRewardStatus.AlreadyGranted,
                rewardId,
                new ProgressionResult(level, level, experience, 0),
                default(LootGrantResult),
                0L);
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
