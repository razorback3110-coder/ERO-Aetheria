using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Authoritative progression rules shared by gameplay/UI adapters.
    /// Launch cap is 250; the cap is configurable so future expansions can raise it.
    /// XP earned above the current cap is retained as overflow XP.
    /// </summary>
    public sealed class ProgressionSystem : MonoBehaviour
    {
        public const int LaunchLevelCap = 250;

        [SerializeField] private int currentLevelCap = LaunchLevelCap;

        public event System.Action<int> LevelChanged;
        public event System.Action<long> OverflowXpChanged;

        public int CurrentLevelCap => Mathf.Max(1, currentLevelCap);

        public void SetLevelCap(int newCap)
        {
            currentLevelCap = Mathf.Max(1, newCap);
        }

        public void AddXP(CharacterData c, long amount)
        {
            if (c == null || amount <= 0)
                return;

            // Keep XP arithmetic bounded and deterministic. Overflow is persisted separately.
            if (c.level >= CurrentLevelCap)
            {
                c.overflowXp = SaturatingAdd(c.overflowXp, amount);
                OverflowXpChanged?.Invoke(c.overflowXp);
                return;
            }

            c.xp = SaturatingAdd(c.xp, amount);
            ProcessLevels(c);
        }

        private void ProcessLevels(CharacterData c)
        {
            while (c.level < CurrentLevelCap)
            {
                long required = Need(c.level);
                if (c.xp < required)
                    break;

                c.xp -= required;
                c.level++;
                LevelChanged?.Invoke(c.level);
            }

            // XP remaining at the cap is not discarded. It becomes persistent overflow XP.
            if (c.level >= CurrentLevelCap && c.xp > 0)
            {
                c.overflowXp = SaturatingAdd(c.overflowXp, c.xp);
                c.xp = 0;
                OverflowXpChanged?.Invoke(c.overflowXp);
            }
        }

        /// <summary>XP required to progress from the supplied level to the next level.</summary>
        public long Need(int level)
        {
            level = Mathf.Clamp(level, 1, 100000);
            // Preserve the prototype curve while making it safe for long-running MMO progression.
            return 100L + (long)level * level * 25L;
        }

        /// <summary>
        /// Converts stored overflow XP into normal XP after a future server cap increase.
        /// </summary>
        public void ConvertOverflowAfterCapIncrease(CharacterData c)
        {
            if (c == null || c.overflowXp <= 0 || c.level >= CurrentLevelCap)
                return;

            long available = c.overflowXp;
            c.overflowXp = 0;
            c.xp = SaturatingAdd(c.xp, available);
            OverflowXpChanged?.Invoke(c.overflowXp);
            ProcessLevels(c);
        }

        public bool CanAwaken(CharacterData c) => c != null && c.level >= CurrentLevelCap;

        // Legacy API retained as a no-op milestone hook; awakening no longer forces level 100.
        public void Awaken(CharacterData c)
        {
            if (CanAwaken(c))
                OverflowXpChanged?.Invoke(c.overflowXp);
        }

        private static long SaturatingAdd(long a, long b)
        {
            if (b <= 0)
                return a;
            if (a > long.MaxValue - b)
                return long.MaxValue;
            return a + b;
        }
    }
}
