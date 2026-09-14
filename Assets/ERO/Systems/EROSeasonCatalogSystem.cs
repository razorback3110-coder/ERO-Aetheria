using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROSeasonCatalogEntry
    {
        public string seasonId;
        public string displayName;
        public string themeId;
        public string towerResourceName;
        public string towerResourceDescription;
        public string cosmeticTheme;
        public EROTowerSeasonDefinition definition;
    }

    /// <summary>Canonical launch catalog for seasonal Tower content. New seasons can be appended without changing the core systems.</summary>
    public static class EROSeasonCatalogSystem
    {
        public static IReadOnlyList<EROSeasonCatalogEntry> CreateLaunchCatalog()
        {
            return new List<EROSeasonCatalogEntry>
            {
                Create("S01", "Emberfall", "emberfall", "Ember Shards", "Seasonal shards recovered from the Emberfall Tower.", "Ashen Flame"),
                Create("S02", "Veil of Shadows", "veil-of-shadows", "Veil Fragments", "Seasonal fragments condensed from shadow breaches.", "Midnight Veil"),
                Create("S03", "Aetheric Ruin", "aetheric-ruin", "Aether Relics", "Relics resonating with the ancient Aetheric Ruin.", "Astral Ruin")
            };
        }

        public static EROSeasonCatalogEntry Create(string seasonId, string displayName, string themeId, string resourceName, string resourceDescription, string cosmeticTheme)
        {
            if (string.IsNullOrEmpty(seasonId)) throw new ArgumentException("Season id is required.", nameof(seasonId));
            return new EROSeasonCatalogEntry
            {
                seasonId = seasonId,
                displayName = displayName,
                themeId = themeId,
                towerResourceName = resourceName,
                towerResourceDescription = resourceDescription,
                cosmeticTheme = cosmeticTheme,
                definition = EROTowerSeasonSystem.CreateSeason(seasonId, displayName, themeId)
            };
        }
    }
}
