using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative character progression. XP is monotonic, overflow-safe and
    /// deterministic so the same reward cannot produce divergent levels on client/server.
    /// </summary>
    public sealed class EROCharacterProgression
    {
        public const int CurrentSnapshotVersion = 1;
        public const int MaxLevel = 100;

        private long experience;
        private int level = 1;

        public EROCharacterProgression(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId))
                throw new ArgumentException("ActorId is required.", nameof(actorId));
            ActorId = actorId;
        }

        public string ActorId { get; }
        public int Level => level;
        public long Experience => experience;

        public ProgressionResult GrantExperience(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0 || level >= MaxLevel)
                return new ProgressionResult(level, level, experience, 0);

            long before = experience;
            experience = checked(experience + amount);
            int previousLevel = level;

            while (level < MaxLevel && experience >= ExperienceRequiredForLevel(level + 1))
                level++;

            return new ProgressionResult(previousLevel, level, experience, experience - before);
        }

        public long GetExperienceIntoCurrentLevel()
        {
            long start = ExperienceRequiredForLevel(level);
            return experience < start ? 0 : experience - start;
        }

        public long GetExperienceToNextLevel()
        {
            if (level >= MaxLevel) return 0;
            long next = ExperienceRequiredForLevel(level + 1);
            return next - experience;
        }

        public ProgressionSnapshot CaptureSnapshot()
        {
            return new ProgressionSnapshot(CurrentSnapshotVersion, ActorId, level, experience);
        }

        public void RestoreSnapshot(ProgressionSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != CurrentSnapshotVersion)
                throw new InvalidOperationException("Unsupported progression snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Progression actor mismatch.");
            if (snapshot.Level < 1 || snapshot.Level > MaxLevel)
                throw new InvalidOperationException("Progression snapshot contains an invalid level.");
            if (snapshot.Experience < 0)
                throw new InvalidOperationException("Progression snapshot contains negative experience.");
            if (snapshot.Level < MaxLevel && snapshot.Experience < ExperienceRequiredForLevel(snapshot.Level))
                throw new InvalidOperationException("Progression snapshot is below its level floor.");

            level = snapshot.Level;
            experience = snapshot.Experience;
        }

        public static long ExperienceRequiredForLevel(int targetLevel)
        {
            if (targetLevel < 1 || targetLevel > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(targetLevel));
            if (targetLevel == 1) return 0;

            // Quadratic curve: 100 * (L-1)^2. The result is deterministic and stays
            // comfortably inside Int64 for the current level cap.
            long previous = targetLevel - 1L;
            return checked(100L * previous * previous);
        }
    }

    public readonly struct ProgressionResult
    {
        public ProgressionResult(int previousLevel, int currentLevel, long experience, long grantedExperience)
        {
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            Experience = experience;
            GrantedExperience = grantedExperience;
        }

        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public long Experience { get; }
        public long GrantedExperience { get; }
        public bool LeveledUp => CurrentLevel > PreviousLevel;
    }

    public sealed class ProgressionSnapshot
    {
        public ProgressionSnapshot(int version, string actorId, int level, long experience)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Level = level;
            Experience = experience;
        }

        public int Version { get; }
        public string ActorId { get; }
        public int Level { get; }
        public long Experience { get; }
    }
}
