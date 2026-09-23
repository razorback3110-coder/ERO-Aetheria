using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Converts an authoritative defeat into idempotent XP, loot and optional Gold rewards.
    /// Loot is committed before XP so an inventory-full retry cannot consume progression.
    /// </summary>
    public sealed class EROCombatRewards
    {
        private const int SnapshotVersion = 2;
        private const int LegacySnapshotVersion = 1;
        private readonly HashSet<string> claimedRewards = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, PendingGoldReward> pendingGoldRewards = new Dictionary<string, PendingGoldReward>(StringComparer.Ordinal);
        private readonly EROCharacterProgression progression;
        private readonly EROLootInventoryService loot;
        private readonly EROAuthoritativeWallet wallet;

        public EROCombatRewards(EROCharacterProgression progression, EROLootInventoryService loot)
            : this(progression, loot, null) { }

        public EROCombatRewards(EROCharacterProgression progression, EROLootInventoryService loot, EROAuthoritativeWallet wallet)
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
            if (gold > 0 && wallet == null) throw new InvalidOperationException("A wallet is required when a combat reward grants Gold.");

            string rewardId = BuildRewardId(encounterId, encounterSeed, defeatedActorId);
            if (claimedRewards.Contains(rewardId))
                return CombatRewardResult.AlreadyGranted(rewardId, progression.Level, progression.Experience);

            // Commit the inventory grant first. If the inventory is full, no XP is consumed and
            // the same deterministic reward can be retried after the player makes space.
            LootGrantResult lootResult = loot.Grant(
                encounterId, encounterSeed, dropIndex, itemId, quantity, maxStack, itemLevel, itemStats);
            if (!lootResult.Success)
            {
                return new CombatRewardResult(
                    CombatRewardStatus.LootPending, rewardId,
                    new ProgressionResult(progression.Level, progression.Level, progression.Experience, 0),
                    lootResult, 0L);
            }

            ProgressionResult progressionResult = progression.GrantExperience(experience);
            claimedRewards.Add(rewardId);

            if (gold > 0 && !wallet.TryApplyTransaction(rewardId + ":gold", defeatedActorId, EROCurrencyCatalog.Gold, gold, true))
            {
                pendingGoldRewards[rewardId] = new PendingGoldReward(defeatedActorId, gold);
                return new CombatRewardResult(CombatRewardStatus.GoldPending, rewardId, progressionResult, lootResult, gold);
            }

            return new CombatRewardResult(CombatRewardStatus.Granted, rewardId, progressionResult, lootResult, gold);
        }

        /// <summary>Finalizes Gold after a transient wallet failure without re-granting XP or loot.</summary>
        public bool TryFinalizeGoldReward(string rewardId, string ownerId, long gold)
        {
            if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("Reward id is required.", nameof(rewardId));
            if (!claimedRewards.Contains(rewardId)) throw new InvalidOperationException("Gold cannot be finalized for an unclaimed reward.");
            if (gold <= 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (wallet == null) throw new InvalidOperationException("A wallet is required to finalize Gold.");

            if (!pendingGoldRewards.TryGetValue(rewardId, out PendingGoldReward pending))
            {
                string transactionId = rewardId + ":gold";
                if (!wallet.HasAppliedTransaction(transactionId)) return false;
                return wallet.TryApplyTransaction(transactionId, ownerId, EROCurrencyCatalog.Gold, gold, true);
            }

            if (!string.Equals(pending.OwnerId, ownerId, StringComparison.Ordinal) || pending.Gold != gold)
                throw new InvalidOperationException("Pending Gold does not match the original combat reward owner or amount.");

            bool applied = wallet.TryApplyTransaction(rewardId + ":gold', pending.OwnerId, EROCurrencyCatalog.Gold, pending.Gold, true);
            if (applied) pendingGoldRewards.Remove(rewardId);
            return applied;
        }

        public RewardSnapshot CaptureSnapshot()
        {
            var ids = new List<string>(claimedRewards);
            ids.Sort(StringComparer.Ordinal);
            var pending = new List<PendingGoldRewardSnapshot>(pendingGoldRewards.Count);
            foreach (KeyValuePair<string, PendingGoldReward> entry in pendingGoldRewards)
                pending.Add(new PendingGoldRewardSnapshot(entry.Key, entry.Value.OwnerId, entry.Value.Gold));
            pending.Sort((left, right) => string.CompareOrdinal(left.RewardId, right.RewardId));
            return new RewardSnapshot(SnapshotVersion, ids, pending);
        }

        public void RestoreSnapshot(RewardSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != SnapshotVersion && snapshot.Version != LegacySnapshotVersion)
                throw new InvalidOperationException("Unsupported reward snapshot version.");

            var restored = new HashSet<string>(StringComparer.Ordinal);
            string previous = null;
            foreach (string id in snapshot.ClaimedRewardIds)
            {
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Reward snapshot contains an invalid reward id.");
                if (previous != null && string.CompareOrdinal(previous, id) >= 0) throw new InvalidOperationException("Reward snapshot ids must be unique and ordinally sorted.");
                if (!restored.Add(id)) throw new InvalidOperationException("Reward snapshot contains a duplicate reward id.");
                previous = id;
            }

            var restoredPending = new Dictionary<string, PendingGoldReward>(StringComparer.Ordinal);
            if (snapshot.Version == SnapshotVersion)
            {
                string previousPending = null;
                foreach (PendingGoldRewardSnapshot entry in snapshot.PendingGoldRewards)
                {
                    if (string.IsNullOrWhiteSpace(entry.RewardId) || string.IsNullOrWhiteSpace(entry.OwnerId) || entry.Gold <= 0)
                        throw new InvalidOperationException("Reward snapshot contains an invalid pending Gold entry.");
                    if (!restored.Contains(entry.RewardId)) throw new InvalidOperationException("Pending Gold entry references an unknown reward.");
                    if (previousPending != null && string.CompareOrdinal(previousPending, entry.RewardId) >= 0)
                        throw new InvalidOperationException("Pending Gold entries must be unique and ordinally sorted.");
                    if (!restoredPending.TryAdd(entry.RewardId, new PendingGoldReward(entry.OwnerId, entry.Gold)))
                        throw new InvalidOperationException("Reward snapshot contains duplicate pending Gold entries.");
                    previousPending = entry.RewardId;
                }
            }

            claimedRewards.Clear();
            foreach (string id in restored) claimedRewards.Add(id);
            pendingGoldRewards.Clear();
            foreach (KeyValuePair<string, PendingGoldReward> entry in restoredPending) pendingGoldRewards.Add(entry.Key, entry.Value);
        }

        public static string BuildRewardId(string encounterId, long encounterSeed, string defeatedActorId)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            if (string.IsNullOrWhiteSpace(defeatedActorId)) throw new ArgumentException("Defeated actor id is required.", nameof(defeatedActorId));
            return "reward:" + encounterId + ":" + encounterSeed.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + defeatedActorId;
        }

        private readonly struct PendingGoldReward
        {
            public PendingGoldReward(string ownerId, long gold)
            {
                if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner id is required.", nameof(ownerId));
                if (gold <= 0) throw new ArgumentOutOfRangeException(nameof(gold));
                OwnerId = ownerId;
                Gold = gold;
            }
            public string OwnerId { get; }
            public long Gold { get; }
        }
    }

    public enum CombatRewardStatus { Granted = 1, AlreadyGranted = 2, LootPending = 3, GoldPending = 4 }

    public readonly struct CombatRewardResult
    {
        public CombatRewardResult(CombatRewardStatus status, string rewardId, ProgressionResult progression, LootGrantResult loot, long gold)
        {
            Status = status; RewardId = rewardId ?? throw new ArgumentNullException(nameof(rewardId)); Progression = progression; Loot = loot; Gold = gold;
        }
        public CombatRewardStatus Status { get; }
        public string RewardId { get; }
        public ProgressionResult Progression { get; }
        public LootGrantResult Loot { get; }
        public long Gold { get; }
        public bool Success => Status == CombatRewardStatus.Granted || Status == CombatRewardStatus.AlreadyGranted;
        public static CombatRewardResult AlreadyGranted(string rewardId, int level, long experience)
        {
            return new CombatRewardResult(CombatRewardStatus.AlreadyGranted, rewardId, new ProgressionResult(level, level, experience, 0), default(LootGrantResult), 0L);
        }
    }

    public sealed class RewardSnapshot
    {
        public RewardSnapshot(int version, IReadOnlyList<string> claimedRewardIds)
            : this(version, claimedRewardIds, Array.Empty<PendingGoldRewardSnapshot>()) { }
        public RewardSnapshot(int version, IReadOnlyList<string> claimedRewardIds, IReadOnlyList<PendingGoldRewardSnapshot> pendingGoldRewards)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Version = version; ClaimedRewardIds = claimedRewardIds ?? throw new ArgumentNullException(nameof(claimedRewardIds)); PendingGoldRewards = pendingGoldRewards ?? throw new ArgumentNullException(nameof(pendingGoldRewards));
        }
        public int Version { get; }
        public IReadOnlyList<string> ClaimedRewardIds { get; }
        public IReadOnlyList<PendingGoldRewardSnapshot> PendingGoldRewards { get; }
    }

    public readonly struct PendingGoldRewardSnapshot
    {
        public PendingGoldRewardSnapshot(string rewardId, string ownerId, long gold)
        {
            if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("Reward id is required.", nameof(rewardId));
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner id is required.", nameof(ownerId));
            if (gold <= 0) throw new ArgumentOutOfRangeException(nameof(gold));
            RewardId = rewardId; OwnerId = ownerId; Gold = gold;
        }
        public string RewardId { get; }
        public string OwnerId { get; }
        public long Gold { get; }
    }
}