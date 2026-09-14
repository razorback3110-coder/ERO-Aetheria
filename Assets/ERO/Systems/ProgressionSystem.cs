using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    public sealed class ProgressionSystem : MonoBehaviour
    {
        public const int LaunchLevelCap = 250;
        [SerializeField] private int currentLevelCap = LaunchLevelCap;
        public event System.Action<int> LevelChanged;
        public event System.Action<long> OverflowXpChanged;
        public int CurrentLevelCap => Mathf.Max(1, currentLevelCap);

        public void SetLevelCap(int newCap) => currentLevelCap = Mathf.Max(1, newCap);

        public void AddXP(CharacterData c, long amount)
        {
            if (c == null || amount <= 0) return;
            EROStatSystem.ReconcileUnspentPoints(c);
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
                if (c.xp < required) break;
                c.xp -= required;
                c.level++;
                EROStatSystem.GrantLevelUpPoints(c, 1);
                LevelChanged?.Invoke(c.level);
            }

            if (c.level >= CurrentLevelCap && c.xp > 0)
            {
                c.overflowXp = SaturatingAdd(c.overflowXp, c.xp);
                c.xp = 0;
                OverflowXpChanged?.Invoke(c.overflowXp);
            }
        }

        public long Need(int level)
        {
            level = Mathf.Clamp(level, 1, 100000);
            return 100L + (long)level * level * 25L;
        }

        public void ConvertOverflowAfterCapIncrease(CharacterData c)
        {
            if (c == null || c.overflowXp <= 0 || c.level >= CurrentLevelCap) return;
            long available = c.overflowXp;
            c.overflowXp = 0;
            c.xp = SaturatingAdd(c.xp, available);
            OverflowXpChanged?.Invoke(c.overflowXp);
            ProcessLevels(c);
        }

        public bool CanAwaken(CharacterData c) => c != null && c.level >= CurrentLevelCap;

        public void Awaken(CharacterData c)
        {
            if (CanAwaken(c)) OverflowXpChanged?.Invoke(c.overflowXp);
        }

        private static long SaturatingAdd(long a, long b)
        {
            if (b <= 0) return a;
            if (a > long.MaxValue - b) return long.MaxValue;
            return a + b;
        }
    }
}
