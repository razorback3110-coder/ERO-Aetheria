using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Authoritative bridge between seasonal Tower rewards and permanent player data.</summary>
    public static class EROTowerRewardDeliverySystem
    {
        public static bool TryDeliver(CharacterData character, EROTowerSeasonWallet wallet, EROTowerReward reward)
        {
            if (character == null || wallet == null || reward == null || reward.amount <= 0 || string.IsNullOrEmpty(wallet.seasonId)) return false;
            if (reward.resource == EROTowerResourceType.Credits)
            {
                character.credits = checked(character.credits + reward.amount);
                return true;
            }
            if (IsWalletResource(reward.resource)) return EROTowerSeasonShopSystem.ApplyReward(wallet, reward);
            if (reward.resource == EROTowerResourceType.GemChest || reward.resource == EROTowerResourceType.RuneChest)
            {
                if (string.IsNullOrEmpty(reward.itemId)) return false;
                return AddStack(character, reward.itemId, reward.resource == EROTowerResourceType.GemChest ? "Tower Gem Chest" : "Tower Rune Chest", Rarity.Rare, reward.amount);
            }
            return false;
        }

        public static bool TryDeliverClaim(CharacterData character, EROTowerSeasonWallet wallet,
            EROTowerSeasonDefinition season, EROTowerSeasonProgress progress,
            EROSeasonRewardTrack track, int level, out EROTowerReward reward)
        {
            reward = null;
            if (!EROTowerSeasonSystem.TryClaim(season, progress, track, level, out reward)) return false;
            if (TryDeliver(character, wallet, reward)) return true;

            bool[] claimed = track == EROSeasonRewardTrack.Free ? progress.claimedFree : progress.claimedPremium;
            if (claimed != null && level >= 0 && level < claimed.Length) claimed[level] = false;
            reward = null;
            return false;
        }

        private static bool IsWalletResource(EROTowerResourceType resource)
            => resource == EROTowerResourceType.TowerCoins || resource == EROTowerResourceType.TowerShards ||
               resource == EROTowerResourceType.TowerKeys || resource == EROTowerResourceType.GemDust ||
               resource == EROTowerResourceType.RuneFragments || resource == EROTowerResourceType.TranscendenceEssence ||
               resource == EROTowerResourceType.CosmeticToken;

        private static bool AddStack(CharacterData character, string id, string name, Rarity rarity, int amount)
        {
            if (character.inventory == null) character.inventory = new List<ItemData>();
            foreach (var item in character.inventory)
            {
                if (item != null && item.id == id && !item.equipped)
                {
                    item.quantity = checked(item.quantity + amount);
                    return true;
                }
            }
            character.inventory.Add(new ItemData { id = id, name = name, rarity = rarity, level = 1, quantity = amount, equipped = false });
            return true;
        }
    }
}
