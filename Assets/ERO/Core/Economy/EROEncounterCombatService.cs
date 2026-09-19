using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Connects an authoritative combat hit to the persistent defeat reward pipeline.
    /// The service contains no Unity dependency and is suitable for dedicated-server simulation.
    /// </summary>
    public sealed class EROEncounterCombatService
    {
        private readonly EROAuthoritativeCombatService combat;
        private readonly EROCombatRewards rewards;

        public EROEncounterCombatService(
            EROAuthoritativeCombatService combat,
            EROCombatRewards rewards)
        {
            this.combat = combat ?? throw new ArgumentNullException(nameof(combat));
            this.rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
        }

        public EncounterAttackResult ResolveAttack(
            string attackerId,
            EROCharacterCombatStats attackerStats,
            string defenderId,
            EROCharacterCombatStats defenderStats,
            ulong combatSeed,
            string encounterId,
            long encounterSeed,
            long experience,
            int dropIndex,
            string itemId,
            int quantity,
            int maxStack,
            int itemLevel,
            IReadOnlyDictionary<string, long> itemStats = null)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
                throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            if (string.IsNullOrWhiteSpace(defenderId))
                throw new ArgumentException("Defender id is required.", nameof(defenderId));
            if (string.Equals(attackerId, defenderId, StringComparison.Ordinal))
                throw new InvalidOperationException("An actor cannot attack itself.");

            CombatHitResult hit = combat.ApplyAttack(
                attackerId,
                attackerStats,
                defenderId,
                defenderStats,
                combatSeed);

            if (!hit.Defeated || hit.AlreadyDefeated)
                return EncounterAttackResult.WithoutRewards(hit);

            CombatRewardResult reward = rewards.GrantDefeatRewards(
                encounterId,
                encounterSeed,
                defenderId,
                experience,
                dropIndex,
                itemId,
                quantity,
                maxStack,
                itemLevel,
                itemStats);

            return EncounterAttackResult.WithRewards(hit, reward);
        }
    }

    public readonly struct EncounterAttackResult
    {
        private EncounterAttackResult(CombatHitResult hit, bool rewardsGranted, CombatRewardResult reward)
        {
            Hit = hit;
            RewardsGranted = rewardsGranted;
            Reward = reward;
        }

        public CombatHitResult Hit { get; }
        public bool RewardsGranted { get; }
        public CombatRewardResult Reward { get; }

        public static EncounterAttackResult WithoutRewards(CombatHitResult hit)
        {
            return new EncounterAttackResult(hit, false, default(CombatRewardResult));
        }

        public static EncounterAttackResult WithRewards(CombatHitResult hit, CombatRewardResult reward)
        {
            return new EncounterAttackResult(hit, true, reward);
        }
    }
}
