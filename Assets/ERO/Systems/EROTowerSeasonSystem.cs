using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    public enum EROSeasonRewardTrack { Free, Premium }
    public enum EROTowerResourceType { TowerShards, TowerKeys, GemChest, RuneChest, GemDust, RuneFragments, TranscendenceEssence, TowerCoins, Credits, CosmeticToken }

    [Serializable] public class EROTowerReward { public EROSeasonRewardTrack track; public int level; public EROTowerResourceType resource; public int amount; public string itemId; public bool milestone; }
    [Serializable] public class EROTowerSeasonDefinition
    {
        public string seasonId; public string displayName; public int maximumTowerFloor = 100; public int maximumPassLevel = 100; public long xpPerLevel = 1000; public string themeId;
        public List<EROTowerReward> rewards = new List<EROTowerReward>();
    }
    [Serializable] public class EROTowerSeasonProgress
    {
        public string seasonId; public int passLevel; public long seasonXp; public int highestFloor; public int towerCoins; public int towerShards; public int towerKeys; public bool premiumUnlocked;
        public bool[] claimedFree = Array.Empty<bool>(); public bool[] claimedPremium = Array.Empty<bool>();
    }

    public static class EROTowerSeasonSystem
    {
        public const int DefaultPassLevels = 100;
        public const int DefaultTowerFloors = 100;
        public const int PremiumXpBonusBasisPoints = 1000;

        public static EROTowerSeasonDefinition CreateSeason(string seasonId, string displayName, string themeId)
        {
            var season = new EROTowerSeasonDefinition { seasonId = seasonId, displayName = displayName, themeId = themeId, maximumTowerFloor = DefaultTowerFloors, maximumPassLevel = DefaultPassLevels, xpPerLevel = 1000 };
            BuildDefaultRewardTable(season);
            return season;
        }

        public static void BuildDefaultRewardTable(EROTowerSeasonDefinition season)
        {
            if (season == null) throw new ArgumentNullException(nameof(season));
            season.rewards.Clear();
            for (int level = 1; level <= season.maximumPassLevel; level++)
            {
                season.rewards.Add(new EROTowerReward { track = EROSeasonRewardTrack.Free, level = level, resource = FreeResourceFor(level), amount = FreeAmountFor(level), itemId = ItemIdFor(FreeResourceFor(level), season.seasonId), milestone = IsMilestone(level) });
                season.rewards.Add(new EROTowerReward { track = EROSeasonRewardTrack.Premium, level = level, resource = PremiumResourceFor(level), amount = PremiumAmountFor(level), itemId = ItemIdFor(PremiumResourceFor(level), season.seasonId), milestone = IsMilestone(level) });
            }
        }

        public static bool TryAddSeasonXp(EROTowerSeasonDefinition season, EROTowerSeasonProgress progress, long xp)
        {
            if (season == null || progress == null || xp <= 0 || progress.seasonId != season.seasonId || progress.passLevel >= season.maximumPassLevel) return false;
            if (progress.premiumUnlocked) xp += xp * PremiumXpBonusBasisPoints / 10000L;
            progress.seasonXp += xp;
            while (progress.passLevel < season.maximumPassLevel && progress.seasonXp >= season.xpPerLevel) { progress.seasonXp -= season.xpPerLevel; progress.passLevel++; }
            return true;
        }

        public static bool TrySetHighestFloor(EROTowerSeasonDefinition season, EROTowerSeasonProgress progress, int floor)
        {
            if (season == null || progress == null || progress.seasonId != season.seasonId || floor < 1 || floor > season.maximumTowerFloor || floor <= progress.highestFloor) return false;
            progress.highestFloor = floor; return true;
        }

        public static bool TryUnlockPremium(EROTowerSeasonDefinition season, EROTowerSeasonProgress progress)
        {
            if (season == null || progress == null || progress.seasonId != season.seasonId || progress.premiumUnlocked) return false;
            progress.premiumUnlocked = true; return true;
        }

        public static bool TryClaim(EROTowerSeasonDefinition season, EROTowerSeasonProgress progress, EROSeasonRewardTrack track, int level, out EROTowerReward reward)
        {
            reward = null;
            if (season == null || progress == null || progress.seasonId != season.seasonId || level < 1 || level > progress.passLevel || (track == EROSeasonRewardTrack.Premium && !progress.premiumUnlocked)) return false;
            EnsureClaimArraySize(progress, season.maximumPassLevel);
            bool[] claimed = track == EROSeasonRewardTrack.Free ? progress.claimedFree : progress.claimedPremium;
            if (claimed[level]) return false;
            foreach (var candidate in season.rewards) if (candidate.track == track && candidate.level == level) { claimed[level] = true; reward = candidate; return true; }
            return false;
        }

        public static EROTowerSeasonProgress CreateProgress(string seasonId, bool premiumUnlocked = false)
            => new EROTowerSeasonProgress { seasonId = seasonId, premiumUnlocked = premiumUnlocked, claimedFree = new bool[DefaultPassLevels + 1], claimedPremium = new bool[DefaultPassLevels + 1] };

        public static EROTowerSeasonProgress ResetForNewSeason(string seasonId, bool premiumUnlocked = false) => CreateProgress(seasonId, premiumUnlocked);

        public static void ApplyTowerRewardToSeasonWallet(EROTowerSeasonProgress progress, EROTowerReward reward)
        {
            if (progress == null || reward == null) return;
            switch (reward.resource) { case EROTowerResourceType.TowerCoins: progress.towerCoins += reward.amount; break; case EROTowerResourceType.TowerShards: progress.towerShards += reward.amount; break; case EROTowerResourceType.TowerKeys: progress.towerKeys += reward.amount; break; }
        }

        private static void EnsureClaimArraySize(EROTowerSeasonProgress progress, int maxLevel)
        {
            int size = maxLevel + 1;
            if (progress.claimedFree == null || progress.claimedFree.Length < size) Array.Resize(ref progress.claimedFree, size);
            if (progress.claimedPremium == null || progress.claimedPremium.Length < size) Array.Resize(ref progress.claimedPremium, size);
        }
        private static bool IsMilestone(int level) => level == 10 || level == 25 || level == 50 || level == 75 || level == 100;
        private static EROTowerResourceType FreeResourceFor(int level) { if (level % 25 == 0) return EROTowerResourceType.GemChest; if (level % 20 == 0) return EROTowerResourceType.RuneChest; if (level % 10 == 0) return EROTowerResourceType.TowerKeys; if (level % 5 == 0) return EROTowerResourceType.TowerShards; return EROTowerResourceType.TowerCoins; }
        private static int FreeAmountFor(int level) { if (level % 25 == 0 || level % 20 == 0) return 1; if (level % 10 == 0) return 2; if (level % 5 == 0) return 15; return 25; }
        private static EROTowerResourceType PremiumResourceFor(int level) { if (level % 25 == 0) return EROTowerResourceType.RuneChest; if (level % 20 == 0) return EROTowerResourceType.GemChest; if (level % 10 == 0) return EROTowerResourceType.TranscendenceEssence; if (level % 5 == 0) return EROTowerResourceType.TowerKeys; return level % 2 == 0 ? EROTowerResourceType.TowerShards : EROTowerResourceType.GemDust; }
        private static int PremiumAmountFor(int level) { if (level % 25 == 0 || level % 20 == 0) return 2; if (level % 10 == 0) return 5; if (level % 5 == 0) return 3; return level % 2 == 0 ? 30 : 50; }
        private static string ItemIdFor(EROTowerResourceType resource, string seasonId) => resource == EROTowerResourceType.GemChest ? seasonId + "_gem_chest" : resource == EROTowerResourceType.RuneChest ? seasonId + "_rune_chest" : null;
    }
}
