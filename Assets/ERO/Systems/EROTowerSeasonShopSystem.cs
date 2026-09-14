using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    public enum EROTowerShopCurrency { TowerCoins, TowerShards }

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

    [Serializable]
    public class EROTowerSeasonWallet
    {
        public string seasonId;
        public int towerCoins;
        public int towerShards;
        public int towerKeys;
        public int gemDust;
        public int runeFragments;
        public int transcendenceEssence;
        public int cosmeticTokens;
    }

    /// <summary>Season-specific exchange and reward routing for the Tower.</summary>
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

        public static EROTowerSeasonWallet CreateWallet(string seasonId) => new EROTowerSeasonWallet { seasonId = seasonId };

        public static bool ApplyReward(EROTowerSeasonWallet wallet, EROTowerReward reward)
        {
            if (wallet == null || reward == null || string.IsNullOrEmpty(wallet.seasonId) || reward.amount <= 0) return false;
            switch (reward.resource)
            {
                case EROTowerResourceType.TowerCoins: wallet.towerCoins += reward.amount; break;
                case EROTowerResourceType.TowerShards: wallet.towerShards += reward.amount; break;
                case EROTowerResourceType.TowerKeys: wallet.towerKeys += reward.amount; break;
                case EROTowerResourceType.GemDust: wallet.gemDust += reward.amount; break;
                case EROTowerResourceType.RuneFragments: wallet.runeFragments += reward.amount; break;
                case EROTowerResourceType.TranscendenceEssence: wallet.transcendenceEssence += reward.amount; break;
                case EROTowerResourceType.CosmeticToken: wallet.cosmeticTokens += reward.amount; break;
                default: return false;
            }
            return true;
        }

        /// <summary>Atomic purchase: currency is spent and the purchased reward is delivered exactly once.</summary>
        public static bool TryPurchase(EROTowerSeasonWallet wallet, EROTowerShopOffer offer, out EROTowerReward reward)
        {
            reward = null;
            if (wallet == null || offer == null || string.IsNullOrEmpty(wallet.seasonId) || offer.purchased >= offer.purchaseLimit || offer.cost <= 0 || offer.amount <= 0)
                return false;
            if (offer.currency == EROTowerShopCurrency.TowerCoins)
            {
                if (wallet.towerCoins < offer.cost) return false;
                wallet.towerCoins -= offer.cost;
            }
            else
            {
                if (wallet.towerShards < offer.cost) return false;
                wallet.towerShards -= offer.cost;
            }
            reward = new EROTowerReward { resource = offer.reward, amount = offer.amount };
            if (!ApplyReward(wallet, reward))
            {
                if (offer.currency == EROTowerShopCurrency.TowerCoins) wallet.towerCoins += offer.cost;
                else wallet.towerShards += offer.cost;
                reward = null;
                return false;
            }
            offer.purchased++;
            return true;
        }

        /// <summary>Backward-compatible purchase method; the reward is still delivered atomically.</summary>
        public static bool TryPurchase(EROTowerSeasonWallet wallet, EROTowerShopOffer offer)
        {
            EROTowerReward reward;
            return TryPurchase(wallet, offer, out reward);
        }

        [Obsolete("Purchases now deliver rewards atomically through TryPurchase to prevent duplicate claims.")]
        public static bool ApplyPurchasedReward(EROTowerSeasonWallet wallet, EROTowerShopOffer offer) => false;

        public static int GetCatchUpMultiplier(int daysSinceSeasonStart)
        {
            if (daysSinceSeasonStart < 7) return 1;
            if (daysSinceSeasonStart < 15) return 2;
            if (daysSinceSeasonStart < 22) return 3;
            return 4;
        }

        public static int GetWeeklyTowerResourceBonus(int completedRuns)
            => completedRuns < 5 ? 0 : Math.Min(100, 10 + (completedRuns / 5) * 5);

        private static EROTowerShopOffer Offer(string id, string name, EROTowerShopCurrency currency, int cost, EROTowerResourceType reward, int amount, int limit, bool premiumRecommended)
            => new EROTowerShopOffer { id = id, displayName = name, currency = currency, cost = cost, reward = reward, amount = amount, purchaseLimit = limit, premiumRecommended = premiumRecommended };
    }
}
