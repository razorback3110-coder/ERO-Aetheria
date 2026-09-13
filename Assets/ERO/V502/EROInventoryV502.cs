using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V502
{
    public enum EROEquipmentSlot { Head, Face, Back, Weapon, OffHand, Armor, Gloves, Boots, Ring1, Ring2, Necklace, Costume }
    public enum EROItemRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

    [Serializable]
    public sealed class EROItem
    {
        public string id;
        public string name;
        public EROItemRarity rarity;
        public int level;
        public int quantity = 1;
        public bool bound;
        public int attack;
        public int defense;
        public int magicAttack;
        public int magicDefense;
        public int enhancement;
    }

    [Serializable]
    public sealed class EROInventoryState
    {
        public int capacity = 100;
        public List<EROItem> items = new();
        public Dictionary<EROEquipmentSlot, EROItem> equipment = new();

        public bool TryAdd(EROItem item)
        {
            if (item == null) return false;
            if (items.Count >= capacity) return false;
            items.Add(item); return true;
        }
        public bool TryEquip(EROEquipmentSlot slot, EROItem item)
        {
            if (item == null) return false;
            equipment[slot] = item; return true;
        }
    }
}
