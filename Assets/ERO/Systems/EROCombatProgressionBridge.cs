using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>
    /// Server-side bridge that converts committed combat rewards into persistent
    /// character progression. Combat remains responsible for producing the reward;
    /// this service owns the progression state and exposes the resulting level-up.
    /// </summary>
    public sealed class EROCombatProgressionBridge : IDisposable
    {
        private readonly Dictionary<ulong, EROCharacterProgression> progressions;
        private readonly EROCombatStateStore combat;
        private bool disposed;

        public EROCombatProgressionBridge(EROCombatStateStore combat, int expectedCapacity = 256)
        {
            this.combat = combat ?? throw new ArgumentNullException(nameof(combat));
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            progressions = new Dictionary<ulong, EROCharacterProgression>(expectedCapacity);
            combat.CombatRewardGranted += OnCombatRewardGranted;
        }

        public int CharacterCount => progressions.Count;

        public event Action<ulong, EROProgressionResult> ProgressionChanged;

        public void RegisterCharacter(ulong actorId, int startingLevel = 1, long startingExperience = 0, int maxLevel = EROCharacterProgression.DefaultMaxLevel)
        {
            if (actorId == 0UL) throw new ArgumentOutOfRangeException(nameof(actorId));
            progressions[actorId] = new EROCharacterProgression(startingLevel, startingExperience, maxLevel);
        }

        public bool RemoveCharacter(ulong actorId) => progressions.Remove(actorId);

        public bool TryGetProgression(ulong actorId, out EROCharacterProgression progression)
            => progressions.TryGetValue(actorId, out progression);

        private void OnCombatRewardGranted(EROCombatReward reward)
        {
            if (disposed || reward.RecipientId == 0UL || reward.Experience <= 0) return;
            if (!progressions.TryGetValue(reward.RecipientId, out EROCharacterProgression progression)) return;

            EROProgressionResult result = progression.GrantExperience(reward.Experience);
            ProgressionChanged?.Invoke(reward.RecipientId, result);
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
