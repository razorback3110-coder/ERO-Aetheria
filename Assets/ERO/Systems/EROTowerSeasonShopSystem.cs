using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    public enum EROTowerShopCurrency
    {
        TowerCoins,
        TowerShards
    }

    [Serializable]
    public class EROTowerShopOffer
    {
        public string id;
        public string displayName;
        public EROTowerShopCurrency currency;
        public int cost;
        public EROTowerResourceType reward;
        public int amount;
        public int purchaseLimit;
        public int purchased;
        public bool premiumRecommended;
    }

    /// <summary>
    /// Seasonal exchange attached to the active Tower season. The shop creates a
    /// long-term reason to keep climbing after the Season Pass is completed.
    /// </summary>
    public static class EROTowerSeasonShopSystem
    {
        public static List<EROTowerShopOffer> CreateDefaultShop()
        {
            return new List<EROTowerShopOffer>
            {
                Offer("gem-dust", "Gem Dust", EROTowerShopCurrency.TowerCoins, 100, EROTowerResourceType.GemDust, 50, 20, false),
                Offer("rune-fragments", "Rune Fragments", EROTowerShopCurrency.TowerCoins, 125, EROTowerResourceType.RuneFragments, 25, 20, false),
                Offer("tower-key", "Tower Key", EROTowerShopCurrency.TowerCoins, 250, EROTowerResourceType.TowerKeys, 1, 10, false),
                Offer("gem-chest", "Gem Chest", EROTowerShopCurrency.TowerShards, 20, EROTowerResourceType.GemChest, 1, 10, false),
                Offer("rune-chest", "Rune Chest", EROTowerShopCurrency.TowerShards, 25, EROTowerResourceType.RuneChest, 1, 10, false),
                Offer("transcendence", "Transcendence Essence", EROTowerShopCurrency.TowerShards, 35, EROTowerResourceType.TranscendenceEssence, 1, 8, true),
                Offer("cosmetic-token", "Season Cosmetic Token", EROTowerShopCurrency.TowerShards, 50, EROTowerResourceType.CosmeticToken, 1, 5, true)
            };
        }

        public static bool TryPurchase(EROTowerSeasonProgress progress, EROTowerShopOffer offer)
        {
            if (progress == null || offer == null || offer.purchased >= offer.purchaseLimit)
                return false;

            if (offer.currency == EROTowerShopCurrency.TowerCoins)
            {
                if (progress.towerCoins < offer.cost) return false;
                progress.towerCoins -= offer.cost;
            }
            else
            {
                if (progress.towerShards < offer.cost) return false;
                progress.towerShards -= offer.cost;
            }

            offer.purchased++;
            return true;
        }

        /// <summary>Extra catch-up resources for players who start a season late.</summary>
        public static int GetCatchUpMultiplier(int daysSinceSeasonStart)
        {
            if (daysSinceSeasonStart < 7) return 1;
            if (daysSinceSeasonStart < 15) return 2;
            if (daysSinceSeasonStart < 22) return 3;
            return 4;
        }

        /// <summary>Optional weekly activity bonus, encouraging play rather than purchases.</summary>
        public static int GetWeeklyTowerResourceBonus(int completedRuns)
        {
            if (completedRuns < 5) return 0;
            return Math.Min(100, 10 + (completedRuns / 5) * 5);
        }

        private static EROTowerShopOffer Offer(string id, string name, EROTowerShopCurrency currency,
            int cost, EROTowerResourceType reward, int amount, int limit, bool premiumRecommended)
        {
            return new EROTowerShopOffer
            {
                id = id,
                displayName = name,
                currency = currency,
                cost = cost,
                reward = reward,
                amount = amount,
                purchaseLimit = limit,
                premiumRecommended = premiumRecommended
            };
        }
    }
}
