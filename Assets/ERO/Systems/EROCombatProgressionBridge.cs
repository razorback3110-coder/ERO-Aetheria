using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>Server-side bridge from committed combat rewards into character progression.</summary>
    public sealed class EROCombatProgressionBridge : IDisposable
    {
        private readonly Dictionary<ulong, EROCharacterProgression> progressions;
        private readonly EROCombatStateStore combat;
        private readonly IEROCharacterProgressionStore persistence;
        private bool disposed;

        public EROCombatProgressionBridge(EROCombatStateStore combat, int expectedCapacity = 256, IEROCharacterProgressionStore persistence = null)
        {
            this.combat = combat ?? throw new ArgumentNullException(nameof(combat));
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            progressions = new Dictionary<ulong, EROCharacterProgression>(expectedCapacity);
            this.persistence = persistence;
            combat.CombatRewardGranted += OnCombatRewardGranted;
        }

        public int CharacterCount => progressions.Count;
        public event Action<ulong, EROProgressionResult> ProgressionChanged;

        public void RegisterCharacter(ulong actorId, int startingLevel = 1, long startingExperience = 0, int maxLevel = EROCharacterProgression.DefaultMaxLevel)
        {
            if (actorId == 0UL) throw new ArgumentOutOfRangeException(nameof(actorId));
            if (persistence != null && persistence.TryLoad(actorId, out EROCharacterProgressionSnapshot snapshot))
            {
                progressions[actorId] = new EROCharacterProgression(snapshot.Level, snapshot.TotalExperience, snapshot.MaxLevel);
                return;
            }

            progressions[actorId] = new EROCharacterProgression(startingLevel, startingExperience, maxLevel);
            Persist(actorId, progressions[actorId]);
        }

        public bool RemoveCharacter(ulong actorId)
        {
            bool removed = progressions.Remove(actorId);
            if (removed) persistence?.Delete(actorId);
            return removed;
        }

        public bool TryGetProgression(ulong actorId, out EROCharacterProgression progression)
            => progressions.TryGetValue(actorId, out progression);

        public bool SaveCharacter(ulong actorId)
        {
            if (persistence == null || !progressions.TryGetValue(actorId, out EROCharacterProgression progression)) return false;
            Persist(actorId, progression);
            return true;
        }

        private void OnCombatRewardGranted(EROCombatReward reward)
        {
            if (disposed || reward.RecipientId == 0UL || reward.Experience <= 0) return;
            if (!progressions.TryGetValue(reward.RecipientId, out EROCharacterProgression progression)) return;

            EROProgressionResult result = progression.GrantExperience(reward.Experience);
            Persist(reward.RecipientId, progression);
            ProgressionChanged?.Invoke(reward.RecipientId, result);
        }

        private void Persist(ulong actorId, EROCharacterProgression progression)
        {
            persistence?.Save(new EROCharacterProgressionSnapshot(actorId, progression));
        }

        public void Dispose()
        {
            if (disposed) return;
            combat.CombatRewardGranted -= OnCombatRewardGranted;
            disposed = true;
            progressions.Clear();
        }
    }
}
