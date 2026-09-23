using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Coordinates a single authoritative player-state snapshot across progression,
    /// inventory, wallet and combat rewards. The snapshot is immutable and restores
    /// all components together with rollback if any component rejects the payload.
    /// </summary>
    public sealed class EROPlayerPersistenceCoordinator
    {
        public const int CurrentSnapshotVersion = 1;

        private readonly EROCharacterProgression progression;
        private readonly EROInstanceInventory inventory;
        private readonly EROAuthoritativeWallet wallet;
        private readonly EROCombatRewards rewards;

        public EROPlayerPersistenceCoordinator(
            EROCharacterProgression progression,
            EROInstanceInventory inventory,
            EROAuthoritativeWallet wallet,
            EROCombatRewards rewards)
        {
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));

            if (!string.Equals(progression.ActorId, inventory.ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Progression and inventory actor ids must match.");
        }

        public string ActorId => progression.ActorId;

        public PlayerPersistenceSnapshot CaptureSnapshot()
        {
            return new PlayerPersistenceSnapshot(
                CurrentSnapshotVersion,
                ActorId,
                progression.CaptureSnapshot(),
                inventory.CaptureSnapshot(),
                wallet.CaptureSnapshot(),
                wallet.CaptureTransactionJournal(),
                rewards.CaptureSnapshot());
        }

        public void RestoreSnapshot(PlayerPersistenceSnapshot snapshot)
        {
            ValidateSnapshot(snapshot);

            PlayerPersistenceSnapshot previous = CaptureSnapshot();
            try
            {
                progression.RestoreSnapshot(snapshot.Progression);
                inventory.RestoreSnapshot(snapshot.Inventory);
                wallet.RestoreSnapshot(snapshot.WalletBalances);
                wallet.RestoreTransactionJournal(snapshot.WalletTransactions);
                rewards.RestoreSnapshot(snapshot.Rewards);
            }
            catch
            {
                RestoreWithoutRollback(previous);
                throw;
            }
        }

        /// <summary>
        /// Captures, codecs and durably stores one complete authoritative player state.
        /// The store performs its own atomic write and integrity envelope.
        /// </summary>
        public void Save(EROPlayerPersistenceStore store, IEROPlayerPersistenceCodec codec)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (codec == null) throw new ArgumentNullException(nameof(codec));
            store.Save(ActorId, codec.Encode(CaptureSnapshot()));
        }

        /// <summary>
        /// Loads one durable state, decodes it, validates it, then applies it atomically
        /// with rollback if any component rejects the snapshot.
        /// </summary>
        public bool TryLoad(EROPlayerPersistenceStore store, IEROPlayerPersistenceCodec codec)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (codec == null) throw new ArgumentNullException(nameof(codec));

            byte[] payload = store.Load(ActorId);
            if (payload == null)
                return false;

            PlayerPersistenceSnapshot snapshot = codec.Decode(payload);
            RestoreSnapshot(snapshot);
            return true;
        }

        private void ValidateSnapshot(PlayerPersistenceSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion)
                throw new InvalidOperationException("Unsupported player persistence snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Player persistence actor mismatch.");
            if (!string.Equals(snapshot.Progression.ActorId, ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Progression snapshot actor mismatch.");
            if (!string.Equals(snapshot.Inventory.ActorId, ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Inventory snapshot actor mismatch.");
            if (snapshot.WalletBalances == null) throw new InvalidOperationException("Wallet balance snapshot is required.");
            if (snapshot.WalletTransactions == null) throw new InvalidOperationException("Wallet transaction snapshot is required.");
            if (snapshot.Rewards == null) throw new InvalidOperationException("Reward snapshot is required.");
        }

        private void RestoreWithoutRollback(PlayerPersistenceSnapshot snapshot)
        {
            progression.RestoreSnapshot(snapshot.Progression);
            inventory.RestoreSnapshot(snapshot.Inventory);
            wallet.RestoreSnapshot(snapshot.WalletBalances);
            wallet.RestoreTransactionJournal(snapshot.WalletTransactions);
            rewards.RestoreSnapshot(snapshot.Rewards);
        }
    }

    public sealed class PlayerPersistenceSnapshot
    {
        public PlayerPersistenceSnapshot(
            int version,
            string actorId,
            ProgressionSnapshot progression,
            InstanceInventorySnapshot inventory,
            IReadOnlyList<EROWalletBalanceEntry> walletBalances,
            IReadOnlyList<EROWalletTransactionEntry> walletTransactions,
            RewardSnapshot rewards)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Progression = progression ?? throw new ArgumentNullException(nameof(progression));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            WalletBalances = walletBalances ?? throw new ArgumentNullException(nameof(walletBalances));
            WalletTransactions = walletTransactions ?? throw new ArgumentNullException(nameof(walletTransactions));
            Rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
        }

        public int Version { get; }
        public string ActorId { get; }
        public ProgressionSnapshot Progression { get; }
        public InstanceInventorySnapshot Inventory { get; }
        public IReadOnlyList<EROWalletBalanceEntry> WalletBalances { get; }
        public IReadOnlyList<EROWalletTransactionEntry> WalletTransactions { get; }
        public RewardSnapshot Rewards { get; }
    }
}
