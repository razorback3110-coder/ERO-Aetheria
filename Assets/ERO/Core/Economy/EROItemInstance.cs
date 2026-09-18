using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative identity for a concrete item instance.
    /// Stackable inventory entries may use ItemId alone; equipment and unique loot
    /// use this immutable instance identity so persistence and trading never rely on display names.
    /// </summary>
    public sealed class EROItemInstance
    {
        public EROItemInstance(string instanceId, string itemId, int quantity, int maxStack, int level, IReadOnlyDictionary<string, long> stats = null)
        {
            ValidateId(instanceId, nameof(instanceId));
            ValidateId(itemId, nameof(itemId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (maxStack <= 0 || maxStack > 1000000) throw new ArgumentOutOfRangeException(nameof(maxStack));
            if (quantity > maxStack) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (level < 0 || level > 10000) throw new ArgumentOutOfRangeException(nameof(level));

            InstanceId = instanceId;
            ItemId = itemId;
            Quantity = quantity;
            MaxStack = maxStack;
            Level = level;
            Stats = CopyStats(stats);
        }

        public string InstanceId { get; }
        public string ItemId { get; }
        public int Quantity { get; }
        public int MaxStack { get; }
        public int Level { get; }
        public IReadOnlyDictionary<string, long> Stats { get; }

        public EROItemInstance WithQuantity(int quantity)
        {
            return new EROItemInstance(InstanceId, ItemId, quantity, MaxStack, Level, Stats);
        }

        public ItemInstanceSnapshot CaptureSnapshot()
        {
            return new ItemInstanceSnapshot(1, InstanceId, ItemId, Quantity, MaxStack, Level, Stats);
        }

        public static EROItemInstance Restore(ItemInstanceSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != 1) throw new InvalidOperationException("Unsupported item instance snapshot version.");
            return new EROItemInstance(snapshot.InstanceId, snapshot.ItemId, snapshot.Quantity, snapshot.MaxStack, snapshot.Level, snapshot.Stats);
        }

        private static Dictionary<string, long> CopyStats(IReadOnlyDictionary<string, long> stats)
        {
            var copy = new Dictionary<string, long>(StringComparer.Ordinal);
            if (stats == null) return copy;
            foreach (var pair in stats)
            {
                ValidateId(pair.Key, "statId");
                copy.Add(pair.Key, pair.Value);
            }
            return copy;
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }
    }

    public sealed class ItemInstanceSnapshot
    {
        public ItemInstanceSnapshot(int version, string instanceId, string itemId, int quantity, int maxStack, int level, IReadOnlyDictionary<string, long> stats)
        {
            Version = version;
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
            Quantity = quantity;
            MaxStack = maxStack;
            Level = level;
            Stats = stats ?? new Dictionary<string, long>(StringComparer.Ordinal);
        }

        public int Version { get; }
        public string InstanceId { get; }
        public string ItemId { get; }
        public int Quantity { get; }
        public int MaxStack { get; }
        public int Level { get; }
        public IReadOnlyDictionary<string, long> Stats { get; }
    }
}
