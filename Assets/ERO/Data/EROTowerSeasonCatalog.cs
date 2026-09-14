using System;
using System.Collections.Generic;
using ERO.Systems;

namespace ERO.Data
{
    [Serializable]
    public class EROTowerSeasonTheme
    {
        public string seasonId;
        public string displayName;
        public string themeId;
        public string towerCurrencyName;
        public string towerShardName;
        public string gemChestId;
        public string runeChestId;
        public string featuredCosmeticId;
        public string description;
    }

    /// <summary>Launch catalog of reusable seasonal identities. Content can be replaced every month without changing systems.</summary>
    public static class EROTowerSeasonCatalog
    {
        public static IReadOnlyList<EROTowerSeasonTheme> Seasons => new List<EROTowerSeasonTheme>
        {
            new EROTowerSeasonTheme
            {
                seasonId = "tower_s01",
                displayName = "Season 1 - Emberfall",
                themeId = "emberfall",
                towerCurrencyName = "Ember Coins",
                towerShardName = "Cinders of the Spire",
                gemChestId = "ember_gem_chest",
                runeChestId = "ember_rune_chest",
                featuredCosmeticId = "tower_emberfall_set",
                description = "A volcanic tower where each ascent feeds the Spire with ancient fire."
            },
            new EROTowerSeasonTheme
            {
                seasonId = "tower_s02",
                displayName = "Season 2 - Veil of Shadows",
                themeId = "shadowveil",
                towerCurrencyName = "Veil Coins",
                towerShardName = "Nightglass Shards",
                gemChestId = "shadow_gem_chest",
                runeChestId = "shadow_rune_chest",
                featuredCosmeticId = "tower_shadowveil_set",
                description = "A forgotten tower consumed by shadows, illusions and cursed relics."
            },
            new EROTowerSeasonTheme
            {
                seasonId = "tower_s03",
                displayName = "Season 3 - Aetheric Ruin",
                themeId = "aetheric_ruin",
                towerCurrencyName = "Aether Coins",
                towerShardName = "Fractured Aether",
                gemChestId = "aether_gem_chest",
                runeChestId = "aether_rune_chest",
                featuredCosmeticId = "tower_aetheric_set",
                description = "An unstable celestial ruin where arcane energy rewrites the Tower."
            }
        };

        public static EROTowerSeasonDefinition CreateSeason(int index)
        {
            var themes = Seasons;
            if (index < 0 || index >= themes.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var theme = themes[index];
            var season = EROTowerSeasonSystem.CreateSeason(theme.seasonId, theme.displayName, theme.themeId);

            foreach (var reward in season.rewards)
            {
                if (reward.resource == EROTowerResourceType.GemChest) reward.itemId = theme.gemChestId;
                else if (reward.resource == EROTowerResourceType.RuneChest) reward.itemId = theme.runeChestId;
                else if (reward.resource == EROTowerResourceType.CosmeticToken) reward.itemId = theme.featuredCosmeticId;
            }
            return season;
        }
    }
}
