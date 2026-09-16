using System;

namespace ERO.Systems
{
    public readonly struct EROProgressionResult
    {
        public readonly int PreviousLevel;
        public readonly int NewLevel;
        public readonly long TotalExperience;
        public readonly long ExperienceIntoLevel;
        public readonly long ExperienceToNextLevel;
        public readonly bool LeveledUp;

        public EROProgressionResult(int previousLevel, int newLevel, long totalExperience, long experienceIntoLevel, long experienceToNextLevel, bool leveledUp)
        {
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            TotalExperience = totalExperience;
            ExperienceIntoLevel = experienceIntoLevel;
            ExperienceToNextLevel = experienceToNextLevel;
            LeveledUp = leveledUp;
        }
    }

    /// <summary>
    /// Authoritative, overflow-safe character XP and level progression.
    /// The state is intentionally plain data so it can be serialized by the persistence layer.
    /// </summary>
    public sealed class EROCharacterProgression
    {
        public const int DefaultMaxLevel = 100;
        private readonly int maxLevel;
        private int level;
        private long totalExperience;

        public EROCharacterProgression(int startingLevel = 1, long startingExperience = 0, int maxLevel = DefaultMaxLevel)
        {
            if (maxLevel < 1) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            if (startingLevel < 1 || startingLevel > maxLevel) throw new ArgumentOutOfRangeException(nameof(startingLevel));
            if (startingExperience < 0) throw new ArgumentOutOfRangeException(nameof(startingExperience));
            this.maxLevel = maxLevel;
            level = startingLevel;
            totalExperience = startingExperience;
            RecalculateLevel();
        }

        public int Level => level;
        public int MaxLevel => maxLevel;
        public long TotalExperience => totalExperience;
        public long ExperienceIntoCurrentLevel => level >= maxLevel ? 0 : totalExperience - ExperienceAtLevel(level);
        public long ExperienceToNextLevel => level >= maxLevel ? 0 : ExperienceAtLevel(level + 1) - totalExperience;

        public EROProgressionResult GrantExperience(int amount)
        {
            if (amount <= 0)
                return Snapshot(level, false);

            int previousLevel = level;
            totalExperience = checked(totalExperience + amount);
            RecalculateLevel();
            return Snapshot(previousLevel, level > previousLevel);
        }

        public static long ExperienceAtLevel(int targetLevel)
        {
            if (targetLevel < 1) throw new ArgumentOutOfRangeException(nameof(targetLevel));
            long n = targetLevel - 1L;
            return checked(n * n * 100L + n * 900L);
        }

        private void RecalculateLevel()
        {
            while (level < maxLevel && totalExperience >= ExperienceAtLevel(level + 1))
                level++;
        }

        private EROProgressionResult Snapshot(int previousLevel, bool leveledUp)
        {
            return new EROProgressionResult(
                previousLevel,
                level,
                totalExperience,
                ExperienceIntoCurrentLevel,
                ExperienceToNextLevel,
                leveledUp);
        }
    }
}
