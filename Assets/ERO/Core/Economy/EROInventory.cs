using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative bounded inventory for persistent MMO items.
    /// Item mutations are keyed by transaction id so retries cannot duplicate grants.
    /// </summary>
    public sealed class EROInventory
    {
        public const int CurrentSnapshotVersion = 1;
        private readonly object sync = new object();
        private readonly Dictionary<string, int> items = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> appliedTransactions = new HashSet<string>(StringComparer.Ordinal);

        public EROInventory(string actorId, int capacity)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            if (capacity <= 0 || capacity > 10000) throw new ArgumentOutOfRangeException(nameof(capacity));
            ActorId = actorId;
            Capacity = capacity;
        }

        public string ActorId { get; }
        public int Capacity { get; }

        public int GetQuantity(string itemId)
        {
            ValidateItem(itemId);
            lock (sync) return items.TryGetValue(itemId, out var quantity) ? quantity : 0;
        }

        public bool TryAdd(string transactionId, string itemId, int quantity, int maxStack, out int newQuantity)
        {
            ValidateTransaction(transactionId);
            ValidateItem(itemId);
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (maxStack <= 0) throw new ArgumentOutOfRangeException(nameof(maxStack));

            lock (sync)
            {
                if (appliedTransactions.Contains(transactionId))
                {
                    newQuantity = items.TryGetValue(itemId, out var existing) ? existing : 0;
                    return false;
                }

                var current = items.TryGetValue(itemId, out var value) ? value : 0;
                int next;
                try { next = checked(current + quantity); }
                catch (OverflowException) { newQuantity = current; return false; }
                if (next > maxStack) { newQuantity = current; return false; }
                if (!items.ContainsKey(itemId) && items.Count >= Capacity) { newQuantity = current; return false; }

                items[itemId] = next;
                appliedTransactions.Add(transactionId);
                newQuantity = next;
                return true;
            }
        }

        public bool TryRemove(string transactionId, string itemId, int quantity, out int newQuantity)
        {
            ValidateTransaction(transactionId);
            ValidateItem(itemId);
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            lock (sync)
            {
                if (appliedTransactions.Contains(transactionId))
                {
                    newQuantity = items.TryGetValue(itemId, out var existing) ? existing : 0;
                    return false;
                }

                var current = items.TryGetValue(itemId, out var value) ? value : 0;
                if (current < quantity) { newQuantity = current; return false; }
                var next = current - quantity;
                if (next == 0) items.Remove(itemId); else items[itemId] = next;
                appliedTransactions.Add(transactionId);
                newQuantity = next;
                return true;
            }
        }

        public InventorySnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return new InventorySnapshot(
                    CurrentSnapshotVersion,
                    ActorId,
                    Capacity,
                    new Dictionary<string, int>(items, StringComparer.Ordinal),
                    new HashSet<string>(appliedTransactions, StringComparer.Ordinal));
            }
        }

        public void RestoreSnapshot(InventorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion) throw new InvalidOperationException("Unsupported inventory snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal)) throw new InvalidOperationException("Inventory actor mismatch.");
            if (snapshot.Capacity != Capacity) throw new InvalidOperationException("Inventory capacity mismatch.");

            lock (sync)
            {
                items.Clear();
                foreach (var pair in snapshot.Items)
                {
                    ValidateItem(pair.Key);
                    if (pair.Value <= 0) throw new InvalidOperationException("Inventory quantities must be positive.");
                    items.Add(pair.Key, pair.Value);
                }
                if (items.Count > Capacity) throw new InvalidOperationException("Inventory exceeds capacity.");

                appliedTransactions.Clear();
                foreach (var transactionId in snapshot.AppliedTransactions)
                {
                    ValidateTransaction(transactionId);
                    if (!appliedTransactions.Add(transactionId)) throw new InvalidOperationException("Inventory snapshot contains duplicate transaction ids.");
                }
            }
        }

        private static void ValidateTransaction(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("TransactionId is required.", nameof(transactionId));
            if (transactionId.Length > 128) throw new ArgumentException("TransactionId is too long.", nameof(transactionId));
        }

        private static void ValidateItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("ItemId is required.", nameof(itemId));
            if (itemId.Length > 128) throw new ArgumentException("ItemId is too long.", nameof(itemId));
        }
    }

    public sealed class InventorySnapshot
    {
        public InventorySnapshot(int version, string actorId, int capacity, IReadOnlyDictionary<string, int> items, IReadOnlyCollection<string> appliedTransactions)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Capacity = capacity;
            Items = items ?? throw new ArgumentNullException(nameof(items));
            AppliedTransactions = appliedTransactions ?? throw new ArgumentNullException(nameof(appliedTransactions));
        }

        public int Version { get; }
        public string ActorId { get; }
        public int Capacity { get; }
        public IReadOnlyDictionary<string, int> Items { get; }
        public IReadOnlyCollection<string> AppliedTransactions { get; }
    }
}
