using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative inventory for concrete item instances.
    /// Unique/equipment loot is stored by immutable InstanceId, preventing
    /// duplicate ownership and allowing per-item level/stat payloads to persist.
    /// </summary>
    public sealed class EROInstanceInventory
    {
        public const int CurrentSnapshotVersion = 1;
        private readonly object sync = new object();
        private readonly Dictionary<string, EROItemInstance> instances = new Dictionary<string, EROItemInstance>(StringComparer.Ordinal);
        private readonly Dictionary<string, InstanceInventoryTransaction> appliedTransactions = new Dictionary<string, InstanceInventoryTransaction>(StringComparer.Ordinal);

        public EROInstanceInventory(string actorId, int capacity)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            if (capacity <= 0 || capacity > 10000) throw new ArgumentOutOfRangeException(nameof(capacity));
            ActorId = actorId;
            Capacity = capacity;
        }

        public string ActorId { get; }
        public int Capacity { get; }

        public bool Contains(string instanceId)
        {
            ValidateId(instanceId, nameof(instanceId));
            lock (sync) return instances.ContainsKey(instanceId);
        }

        public bool TryGet(string instanceId, out EROItemInstance instance)
        {
            ValidateId(instanceId, nameof(instanceId));
            lock (sync) return instances.TryGetValue(instanceId, out instance);
        }

        public bool TryAdd(string transactionId, EROItemInstance instance)
        {
            ValidateId(transactionId, nameof(transactionId));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            lock (sync)
            {
                var transaction = new InstanceInventoryTransaction(instance.InstanceId, instance.ItemId, instance.Quantity, InstanceInventoryOperation.Add);
                if (appliedTransactions.TryGetValue(transactionId, out var previous))
                {
                    if (!previous.Matches(transaction))
                        throw new InvalidOperationException("TransactionId was already used for a different item-instance operation.");
                    return false;
                }

                if (instances.ContainsKey(instance.InstanceId) || instances.Count >= Capacity)
                    return false;

                instances.Add(instance.InstanceId, instance);
                appliedTransactions.Add(transactionId, transaction);
                return true;
            }
        }

        public bool TryRemove(string transactionId, string instanceId, out EROItemInstance removed)
        {
            ValidateId(transactionId, nameof(transactionId));
            ValidateId(instanceId, nameof(instanceId));

            lock (sync)
            {
                if (appliedTransactions.TryGetValue(transactionId, out var previous))
                {
                    if (!previous.Matches(new InstanceInventoryTransaction(instanceId, previous.ItemId, previous.Quantity, InstanceInventoryOperation.Remove)))
                        throw new InvalidOperationException("TransactionId was already used for a different item-instance operation.");
                    removed = null;
                    return false;
                }

                if (!instances.TryGetValue(instanceId, out removed))
                    return false;

                instances.Remove(instanceId);
                appliedTransactions.Add(transactionId, new InstanceInventoryTransaction(
                    removed.InstanceId, removed.ItemId, removed.Quantity, InstanceInventoryOperation.Remove));
                return true;
            }
        }

        public InstanceInventorySnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                var itemSnapshots = new Dictionary<string, ItemInstanceSnapshot>(StringComparer.Ordinal);
                foreach (var pair in instances)
                    itemSnapshots.Add(pair.Key, pair.Value.CaptureSnapshot());

                return new InstanceInventorySnapshot(
                    CurrentSnapshotVersion,
                    ActorId,
                    Capacity,
                    itemSnapshots,
                    new Dictionary<string, InstanceInventoryTransaction>(appliedTransactions, StringComparer.Ordinal));
            }
        }

        public void RestoreSnapshot(InstanceInventorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion) throw new InvalidOperationException("Unsupported instance inventory snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal)) throw new InvalidOperationException("Inventory actor mismatch.");
            if (snapshot.Capacity != Capacity) throw new InvalidOperationException("Inventory capacity mismatch.");

            lock (sync)
            {
                instances.Clear();
                foreach (var pair in snapshot.Instances)
                {
                    if (!string.Equals(pair.Key, pair.Value.InstanceId, StringComparison.Ordinal))
                        throw new InvalidOperationException("Snapshot instance key does not match InstanceId.");
                    if (instances.Count >= Capacity) throw new InvalidOperationException("Inventory exceeds capacity.");
                    instances.Add(pair.Key, EROItemInstance.Restore(pair.Value));
                }

                appliedTransactions.Clear();
                foreach (var pair in snapshot.Transactions)
                {
                    ValidateId(pair.Key, "transactionId");
                    if (!pair.Value.IsValid) throw new InvalidOperationException("Snapshot contains an invalid transaction.");
                    appliedTransactions.Add(pair.Key, pair.Value);
                }
            }
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }

        public enum InstanceInventoryOperation
        {
            Add = 1,
            Remove = 2
        }

        public readonly struct InstanceInventoryTransaction
        {
            public InstanceInventoryTransaction(string instanceId, string itemId, int quantity, InstanceInventoryOperation operation)
            {
                InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
                ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
                Quantity = quantity;
                Operation = operation;
            }

            public string InstanceId { get; }
            public string ItemId { get; }
            public int Quantity { get; }
            public InstanceInventoryOperation Operation { get; }
            public bool IsValid => !string.IsNullOrEmpty(InstanceId) && !string.IsNullOrEmpty(ItemId) && Quantity > 0 &&
                                   (Operation == InstanceInventoryOperation.Add || Operation == InstanceInventoryOperation.Remove);

            public bool Matches(InstanceInventoryTransaction other)
            {
                return string.Equals(InstanceId, other.InstanceId, StringComparison.Ordinal) &&
                       string.Equals(ItemId, other.ItemId, StringComparison.Ordinal) &&
                       Quantity == other.Quantity && Operation == other.Operation;
            }
        }
    }

    public sealed class InstanceInventorySnapshot
    {
        public InstanceInventorySnapshot(
            int version,
            string actorId,
            int capacity,
            IReadOnlyDictionary<string, ItemInstanceSnapshot> instances,
            IReadOnlyDictionary<string, EROInstanceInventory.InstanceInventoryTransaction> transactions)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Capacity = capacity;
            Instances = instances ?? throw new ArgumentNullException(nameof(instances));
            Transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public int Version { get; }
        public string ActorId { get; }
        public int Capacity { get; }
        public IReadOnlyDictionary<string, ItemInstanceSnapshot> Instances { get; }
        public IReadOnlyDictionary<string, EROInstanceInventory.InstanceInventoryTransaction> Transactions { get; }
    }
}
