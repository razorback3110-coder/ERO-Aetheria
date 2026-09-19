using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Authoritative bridge between deterministic loot generation and persistent instance inventory.
    /// A loot grant is identified by the same encounter/drop identity that generated the item,
    /// making retries safe across network reconnects and server restarts when the inventory snapshot is persisted.
    /// </summary>
    public sealed class EROLootInventoryService
    {
        private readonly EROInstanceInventory inventory;

        public EROLootInventoryService(EROInstanceInventory inventory)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public EROInstanceInventory Inventory => inventory;

        public LootGrantResult Grant(
            string encounterId,
            long encounterSeed,
            int dropIndex,
            string itemId,
            int quantity,
            int maxStack,
            int level,
            IReadOnlyDictionary<string, long> stats = null)
        {
            string transactionId = BuildTransactionId(encounterId, encounterSeed, dropIndex, itemId);
            EROItemInstance instance = EROLootGenerator.CreateInstance(
                encounterId,
                encounterSeed,
                dropIndex,
                itemId,
                quantity,
                maxStack,
                level,
                stats);

            bool added = inventory.TryAdd(transactionId, instance);
            if (added)
                return new LootGrantResult(LootGrantStatus.Granted, transactionId, instance);

            if (inventory.Contains(instance.InstanceId))
                return new LootGrantResult(LootGrantStatus.AlreadyGranted, transactionId, instance);

            return new LootGrantResult(LootGrantStatus.InventoryFullOrConflict, transactionId, null);
        }

        public static string BuildTransactionId(string encounterId, long encounterSeed, int dropIndex, string itemId)
        {
            string instanceId = EROLootGenerator.BuildInstanceId(encounterId, encounterSeed, dropIndex, itemId);
            return "loot:" + instanceId;
        }

        public enum LootGrantStatus
        {
            Granted = 1,
            AlreadyGranted = 2,
            InventoryFullOrConflict = 3
        }

        public readonly struct LootGrantResult
        {
            public LootGrantResult(LootGrantStatus status, string transactionId, EROItemInstance instance)
            {
                Status = status;
                TransactionId = transactionId;
                Instance = instance;
            }

            public LootGrantStatus Status { get; }
            public string TransactionId { get; }
            public EROItemInstance Instance { get; }
            public bool Success => Status == LootGrantStatus.Granted || Status == LootGrantStatus.AlreadyGranted;
        }
    }
}
