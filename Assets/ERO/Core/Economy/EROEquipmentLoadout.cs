using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative equipment loadout referencing item instances already owned by the actor.
    /// The loadout never creates or destroys ownership; inventory remains the source of truth.
    /// </summary>
    public sealed class EROEquipmentLoadout
    {
        public const int CurrentSnapshotVersion = 1;
        private readonly object sync = new object();
        private readonly Dictionary<string, string> equippedBySlot = new Dictionary<string, string>(StringComparer.Ordinal);

        public EROEquipmentLoadout(string actorId, EROInstanceInventory inventory)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            if (!string.Equals(actorId, inventory.ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Equipment actor must match inventory actor.");
            ActorId = actorId;
        }

        public string ActorId { get; }
        public EROInstanceInventory Inventory { get; }

        public bool TryEquip(string slotId, string instanceId)
        {
            ValidateId(slotId, nameof(slotId));
            ValidateId(instanceId, nameof(instanceId));
            if (!Inventory.TryGet(instanceId, out _)) return false;

            lock (sync)
            {
                equippedBySlot[slotId] = instanceId;
                return true;
            }
        }

        public bool TryUnequip(string slotId, out string instanceId)
        {
            ValidateId(slotId, nameof(slotId));
            lock (sync)
            {
                if (!equippedBySlot.TryGetValue(slotId, out instanceId))
                {
                    instanceId = null;
                    return false;
                }

                equippedBySlot.Remove(slotId);
                return true;
            }
        }

        public bool TryGetEquipped(string slotId, out EROItemInstance instance)
        {
            ValidateId(slotId, nameof(slotId));
            lock (sync)
            {
                if (!equippedBySlot.TryGetValue(slotId, out var instanceId))
                {
                    instance = null;
                    return false;
                }

                if (!Inventory.TryGet(instanceId, out instance))
                    throw new InvalidOperationException("Equipment references an item that is not owned by the inventory.");
                return true;
            }
        }

        public IReadOnlyDictionary<string, long> CalculateBonusStats()
        {
            var totals = new Dictionary<string, long>(StringComparer.Ordinal);
            lock (sync)
            {
                foreach (var pair in equippedBySlot)
                {
                    if (!Inventory.TryGet(pair.Value, out var item))
                        throw new InvalidOperationException("Equipment references an item that is not owned by the inventory.");

                    foreach (var stat in item.Stats)
                    {
                        totals.TryGetValue(stat.Key, out var current);
                        totals[stat.Key] = checked(current + stat.Value);
                    }
                }
            }
            return totals;
        }

        public EquipmentLoadoutSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return new EquipmentLoadoutSnapshot(
                    CurrentSnapshotVersion,
                    ActorId,
                    new Dictionary<string, string>(equippedBySlot, StringComparer.Ordinal));
            }
        }

        public void RestoreSnapshot(EquipmentLoadoutSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion) throw new InvalidOperationException("Unsupported equipment snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal)) throw new InvalidOperationException("Equipment actor mismatch.");

            lock (sync)
            {
                equippedBySlot.Clear();
                foreach (var pair in snapshot.EquippedBySlot)
                {
                    ValidateId(pair.Key, "slotId");
                    ValidateId(pair.Value, "instanceId");
                    if (!Inventory.Contains(pair.Value))
                        throw new InvalidOperationException("Equipment snapshot references an item not owned by the inventory.");
                    equippedBySlot.Add(pair.Key, pair.Value);
                }
            }
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 64) throw new ArgumentException(name + " is too long.", name);
        }
    }

    public sealed class EquipmentLoadoutSnapshot
    {
        public EquipmentLoadoutSnapshot(int version, string actorId, IReadOnlyDictionary<string, string> equippedBySlot)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            EquippedBySlot = equippedBySlot ?? throw new ArgumentNullException(nameof(equippedBySlot));
        }

        public int Version { get; }
        public string ActorId { get; }
        public IReadOnlyDictionary<string, string> EquippedBySlot { get; }
    }
}
