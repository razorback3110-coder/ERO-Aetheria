using System;

namespace ERO.Content.MVP
{
    /// <summary>
    /// Deterministic, server-safe MVP encounter rules shared by the Unity slice and
    /// the future dedicated-server implementation. No Unity or asset dependencies.
    /// </summary>
    public static class EROMvpEncounterRules
    {
        public const long BaseHealth = 250_000L;
        public const int BaseRespawnSeconds = 3_600;
        public const int MaximumMvpLevel = 100;
        public const double HealthMultiplierPerLevel = 1.35d;
        public const int BaseExperienceReward = 25_000;
        public const int BaseCurrencyReward = 5_000;

        public static long GetMaxHealth(int level)
        {
            ValidateLevel(level);
            double health = BaseHealth * Math.Pow(HealthMultiplierPerLevel, level - 1);
            return health >= long.MaxValue ? long.MaxValue : Math.Max(BaseHealth, (long)Math.Round(health));
        }

        public static int GetRespawnSeconds(int level)
        {
            ValidateLevel(level);
            return BaseRespawnSeconds;
        }

        public static int GetExperienceReward(int level)
        {
            ValidateLevel(level);
            double reward = BaseExperienceReward * Math.Pow(1.18d, level - 1);
            return reward >= int.MaxValue ? int.MaxValue : Math.Max(BaseExperienceReward, (int)Math.Round(reward));
        }

        public static int GetCurrencyReward(int level)
        {
            ValidateLevel(level);
            double reward = BaseCurrencyReward * Math.Pow(1.12d, level - 1);
            return reward >= int.MaxValue ? int.MaxValue : Math.Max(BaseCurrencyReward, (int)Math.Round(reward));
        }

        public static bool IsSpawnReady(long nowUnixSeconds, long defeatedAtUnixSeconds, int level)
        {
            ValidateLevel(level);
            if (defeatedAtUnixSeconds <= 0) return true;
            return nowUnixSeconds >= defeatedAtUnixSeconds + GetRespawnSeconds(level);
        }

        public static void ValidateContract()
        {
            if (BaseHealth != 250_000L) throw new InvalidOperationException("ERO MVP base HP contract changed unexpectedly.");
            if (BaseRespawnSeconds != 3_600) throw new InvalidOperationException("ERO MVP respawn contract must remain one hour.");
            if (MaximumMvpLevel < 1) throw new InvalidOperationException("ERO MVP maximum level must be positive.");
            if (HealthMultiplierPerLevel <= 1d) throw new InvalidOperationException("ERO MVP HP scaling must increase by level.");
            if (GetMaxHealth(1) != BaseHealth) throw new InvalidOperationException("ERO MVP level 1 HP contract is invalid.");
            if (GetMaxHealth(2) <= GetMaxHealth(1)) throw new InvalidOperationException("ERO MVP level scaling is invalid.");
            if (GetRespawnSeconds(1) != BaseRespawnSeconds) throw new InvalidOperationException("ERO MVP respawn calculation is invalid.");
            if (!IsSpawnReady(10_000L, 0L, 1)) throw new InvalidOperationException("ERO MVP initial spawn must be ready.");
            if (IsSpawnReady(10_000L, 7_500L, 1)) throw new InvalidOperationException("ERO MVP must remain unavailable during its respawn window.");
            if (!IsSpawnReady(11_101L, 7_500L, 1)) throw new InvalidOperationException("ERO MVP must respawn after one hour.");
        }

        private static void ValidateLevel(int level)
        {
            if (level < 1 || level > MaximumMvpLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, $"MVP level must be between 1 and {MaximumMvpLevel}.");
        }
    }
}
