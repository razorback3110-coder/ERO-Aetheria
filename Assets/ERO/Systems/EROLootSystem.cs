using System;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Deterministic, server-friendly PvE loot generator. Item definitions are data-only and require no third-party assets.</summary>
    public static class EROLootSystem
    {
        public readonly struct LootResult
        {
            public readonly string ItemId;
            public readonly string ItemName;
            public readonly Rarity Rarity;
            public readonly int Quantity;

            public LootResult(string itemId, string itemName, Rarity rarity, int quantity)
            {
                ItemId = itemId;
                ItemName = itemName;
                Rarity = rarity;
                Quantity = quantity;
            }
        }

        public static bool TryGenerate(string encounterId, int playerLevel, long seed, out LootResult result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(encounterId) || playerLevel < 1) return false;

            var value = StableHash(encounterId, playerLevel, seed);
            var roll = (int)(value % 1000L);
            var rarity = roll < 5 ? Rarity.Unique
                : roll < 25 ? Rarity.Mythic
                : roll < 100 ? Rarity.Legendary
                : roll < 260 ? Rarity.Epic
                : roll < 550 ? Rarity.Rare
                : roll < 800 ? Rarity.Uncommon
                : Rarity.Common;

            var quantity = rarity == Rarity.Common || rarity == Rarity.Uncommon ? 1 + (int)((value / 1000L) % 2L) : 1;
            var itemId = encounterId + "-loot-" + Math.Abs(value).ToString("X");
            result = new LootResult(itemId, "Aetheria Loot " + rarity, rarity, quantity);
            return true;
        }

        public static bool TryAddToInventory(CharacterData character, LootResult loot)
        {
            if (character == null || string.IsNullOrEmpty(loot.ItemId) || loot.Quantity <= 0) return false;
            character.inventory ??= new System.Collections.Generic.List<ItemData>();
            for (var i = 0; i < character.inventory.Count; i++)
            {
                var existing = character.inventory[i];
                if (existing != null && existing.id == loot.ItemId)
                {
                    existing.quantity = Math.Max(1, existing.quantity) + loot.Quantity;
                    return true;
                }
            }

            character.inventory.Add(new ItemData
            {
                id = loot.ItemId,
                name = loot.ItemName,
                rarity = loot.Rarity,
                level = character.level,
                quantity = loot.Quantity,
                equipped = false
            });
            return true;
        }

        private static long StableHash(string encounterId, int level, long seed)
        {
            unchecked
            {
                long hash = 1469598103934665603L;
                for (var i = 0; i < encounterId.Length; i++) hash = (hash ^ encounterId[i]) * 1099511628211L;
                hash = (hash ^ level) * 1099511628211L;
                hash = (hash ^ seed) * 1099511628211L;
                hash ^= hash >> 33;
                hash *= -49064778989728563L;
                hash ^= hash >> 33;
                return hash & long.MaxValue;
            }
        }
    }
}
