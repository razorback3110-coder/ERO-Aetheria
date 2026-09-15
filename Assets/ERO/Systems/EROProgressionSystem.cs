using System;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Deterministic server-friendly character progression. Rewards are applied to CharacterData so existing persistence can capture them.</summary>
    [DisallowMultipleComponent]
    public sealed class EROProgressionSystem : MonoBehaviour
    {
        [SerializeField, Min(1)] private int baseXpPerLevel = 100;
        [SerializeField, Min(1)] private int xpGrowthPercent = 115;
        [SerializeField, Min(0)] private int statPointsPerLevel = 5;
        [SerializeField, Min(1)] private int maxLevel = 100;

        private CharacterSystem characterSystem;
        private EROAutoSaveCoordinator autosave;

        public int MaxLevel => maxLevel;

        private void Awake()
        {
            characterSystem = GetComponent<CharacterSystem>() ?? GetComponentInParent<CharacterSystem>() ?? FindFirstObjectByType<CharacterSystem>();
            autosave = GetComponent<EROAutoSaveCoordinator>() ?? GetComponentInParent<EROAutoSaveCoordinator>() ?? FindFirstObjectByType<EROAutoSaveCoordinator>();
        }

        public bool GrantXp(long amount, long creditReward = 0)
        {
            if (amount < 0) return false;
            var character = characterSystem != null ? characterSystem.Active : null;
            if (character == null) return false;

            character.xp = Math.Max(0L, character.xp + amount);
            if (creditReward > 0) character.credits = Math.Max(0L, character.credits + creditReward);

            var leveled = false;
            while (character.level < maxLevel)
            {
                var required = XpRequired(character.level);
                if (character.xp < required) break;
                character.xp -= required;
                character.level++;
                character.unspentStatPoints = Math.Max(0, character.unspentStatPoints + statPointsPerLevel);
                leveled = true;
            }

            if (character.level >= maxLevel)
                character.overflowXp = Math.Max(0L, character.overflowXp + character.xp);

            autosave?.MarkDirty();
            return leveled;
        }

        public long XpRequired(int level)
        {
            if (level < 1) level = 1;
            var value = (double)baseXpPerLevel;
            for (var i = 1; i < level; i++) value *= Math.Max(100, xpGrowthPercent) / 100.0;
            return Math.Max(1L, (long)Math.Ceiling(value));
        }

        public bool SpendStatPoint(EROPrimaryStat stat)
        {
            var character = characterSystem != null ? characterSystem.Active : null;
            if (character == null || character.unspentStatPoints <= 0) return false;
            character.stats.Add(stat, 1);
            character.unspentStatPoints--;
            autosave?.MarkDirty();
            return true;
        }
    }
}
