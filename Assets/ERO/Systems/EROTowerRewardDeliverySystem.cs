using System;
using ERO.Data;

namespace ERO.Systems
{
    public static class EROTowerRewardDeliverySystem
    {
        /// <summary>Delivers a claimed seasonal reward exactly once to the player character or seasonal wallet.</summary>
        public static bool TryDeliver(CharacterData character, EROTowerSeasonWallet wallet, EROTowerReward reward)
        {
            if (character == null || wallet == null || reward == null || reward.amount <= 0) return false;
            if (reward.resource == EROTowerResourceType.Credits)
            {
                character.credits += reward.amount;
                return true;
            }
            if (IsWalletResource(reward.resource)) return EROTowerSeasonShopSystem.ApplyReward(wallet, reward);
            if (reward.resource == EROTowerResourceType.GemChest || reward.resource == EROTowerResourceType.RuneChest)
            {
                if (string.IsNullOrEmpty(reward.itemId)) return false;
                return AddStack(character, reward.itemId, reward.resource.ToString(), Rarity.Rare, reward.amount);
            }
            return false;
        }

        private static bool IsWalletResource(EROTowerResourceType resource)
            => resource == EROTowerResourceType.TowerCoins || resource == EROTowerResourceType.TowerShards ||
               resource == EROTowerResourceType.TowerKeys || resource == EROTowerResourceType.GemDust ||
               resource == EROTowerResourceType.RuneFragments || resource == EROTowerResourceType.TranscendenceEssence ||
               resource == EROTowerResourceType.CosmeticToken;

        private static bool AddStack(CharacterData character, string id, string name, Rarity rarity, int amount)
        {
            if (character.inventory == null) character.inventory = new System.Collections.Generic.List<ItemData>();
            foreach (var item in character.inventory)
            {
                if (item != null && item.id == id && !item.equipped)
                {
                    item.quantity += amount;
                    return true;
                }
            }
            character.inventory.Add(new ItemData { id = id, name = name, rarity = rarity, level = 1, quantity = amount });
            return true;
        }
    }
}
