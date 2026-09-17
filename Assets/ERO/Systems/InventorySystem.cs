using System;
using System.Collections.Generic;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    public sealed class InventorySystem : MonoBehaviour
    {
        public readonly List<ItemData> Items = new List<ItemData>();
        public int Capacity = 100;

        public bool Add(ItemData item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id) || item.quantity <= 0 || Capacity <= 0)
                return false;

            var existing = Items.Find(x => x != null && string.Equals(x.id, item.id, StringComparison.Ordinal));
            if (existing != null)
            {
                existing.quantity = Math.Max(1, existing.quantity);
                if (item.quantity > int.MaxValue - existing.quantity) return false;
                existing.quantity += item.quantity;
                return true;
            }

            if (Items.Count >= Capacity) return false;
            Items.Add(item);
            return true;
        }

        public bool Remove(string id, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || count <= 0) return false;

            var item = Items.Find(x => x != null && string.Equals(x.id, id, StringComparison.Ordinal));
            if (item == null || item.quantity < count) return false;

            item.quantity -= count;
            if (item.quantity == 0) Items.Remove(item);
            return true;
        }

        public ItemData Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Items.Find(x => x != null && string.Equals(x.id, id, StringComparison.Ordinal));
        }
    }
}
