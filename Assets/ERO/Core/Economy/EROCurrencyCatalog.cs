using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Canonical server-side currency definitions. Currency ids are stable persistence keys;
    /// gameplay systems must use these ids instead of hard-coded display names.
    /// </summary>
    public static class EROCurrencyCatalog
    {
        public const string Gold = "gold";
        public const string EROCrystals = "ero_crystals";

        public const long MaxGold = 9_000_000_000L;
        public const long MaxEROcrystals = 9_000_000_000L;

        private static readonly IReadOnlyDictionary<string, long> MaxBalances =
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                { Gold, MaxGold },
                { EROCrystals, MaxEROcrystals }
            };

        public static bool IsKnown(string currencyId)
        {
            return !string.IsNullOrWhiteSpace(currencyId) && MaxBalances.ContainsKey(currencyId);
        }

        public static long GetMaxBalance(string currencyId)
        {
            if (!IsKnown(currencyId))
                throw new ArgumentException("Unknown ERO currency.", nameof(currencyId));

            return MaxBalances[currencyId];
        }

        public static bool IsValidBalance(string currencyId, long balance)
        {
            return IsKnown(currencyId) && balance >= 0 && balance <= MaxBalances[currencyId];
        }
    }
}
