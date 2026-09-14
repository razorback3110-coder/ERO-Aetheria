using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROSkillRuntimeState
    {
        public long currentResource;
        public long maximumResource;
        public Dictionary<string, long> cooldownUntilMilliseconds = new Dictionary<string, long>();

        public void Reset(long maximum)
        {
            maximumResource = Math.Max(0L, maximum);
            currentResource = maximumResource;
            if (cooldownUntilMilliseconds == null) cooldownUntilMilliseconds = new Dictionary<string, long>();
            else cooldownUntilMilliseconds.Clear();
        }
    }

    [Serializable]
    public struct EROSkillCastResult
    {
        public bool success;
        public string reason;
        public EROAbilityDefinition ability;
        public EROCombatEvent combat;
        public long remainingResource;
        public long cooldownReadyAtMilliseconds;
    }

    /// <summary>
    /// Server-authoritative skill execution layer. It validates class/level/specialization,
    /// resource cost and cooldown before delegating damage resolution to EROCombatSystem.
    /// The caller owns authoritative time and random rolls.
    /// </summary>
    public static class EROSkillRuntimeSystem
    {
        public const long BaseResource = 100L;
        public const long ResourcePerLevel = 4L;
        public const long ResourceRegenPerSecond = 8L;

        public static long GetMaximumResource(CharacterData character)
        {
            if (character == null) return 0L;
            int level = Math.Max(1, Math.Min(250, character.level));
            return BaseResource + level * ResourcePerLevel;
        }

        public static void Initialize(CharacterData character, EROSkillRuntimeState state)
        {
            if (state == null) return;
            state.Reset(GetMaximumResource(character));
        }

        public static void Regenerate(EROAbilityDefinition unused, EROSkillRuntimeState state, long elapsedMilliseconds)
        {
            if (state == null || elapsedMilliseconds <= 0 || state.maximumResource <= 0) return;
            long amount = elapsedMilliseconds * ResourceRegenPerSecond / 1000L;
            if (amount <= 0) return;
            state.currentResource = Math.Min(state.maximumResource, state.currentResource + amount);
        }

        public static EROSkillCastResult TryCast(CharacterData caster, EROSkillRuntimeState state,
            EROSpecialization specialization, string abilityId, EROCombatStats targetStats,
            long nowMilliseconds, int accuracyRoll, int criticalRoll, int blockRoll)
        {
            var failure = new EROSkillCastResult { success = false, reason = "Invalid skill request." };
            if (caster == null || state == null || string.IsNullOrEmpty(abilityId)) return failure;
            var ability = EROAbilitySystem.FindAbility(abilityId);
            if (!EROAbilitySystem.CanUse(ability, caster.classId, specialization, Math.Max(1, caster.level)))
            {
                failure.reason = "Skill unavailable for this character.";
                return failure;
            }
            if (ability.passive)
            {
                failure.reason = "Passive abilities cannot be cast.";
                return failure;
            }
            if (state.cooldownUntilMilliseconds != null && state.cooldownUntilMilliseconds.TryGetValue(ability.id, out long readyAt) && nowMilliseconds < readyAt)
            {
                failure.reason = "Skill is on cooldown.";
                failure.cooldownReadyAtMilliseconds = readyAt;
                return failure;
            }
            if (state.currentResource < Math.Max(0, ability.resourceCost))
            {
                failure.reason = "Not enough resource.";
                return failure;
            }

            var skill = new EROSkillDefinition
            {
                id = ability.id,
                classId = ability.classId,
                damageType = ability.damageType,
                requiredLevel = ability.requiredLevel,
                powerBasisPoints = ability.powerBasisPoints,
                criticalBonusBasisPoints = ability.criticalBonusBasisPoints,
                accuracyBonusBasisPoints = ability.accuracyBonusBasisPoints,
                cooldownMilliseconds = ability.cooldownMilliseconds,
                resourceCost = ability.resourceCost,
                areaOfEffect = ability.areaOfEffect
            };
            var attackerStats = EROCombatSystem.BuildStats(caster);
            var combat = EROCombatSystem.ResolveAttack(attackerStats, targetStats, skill, accuracyRoll, criticalRoll, blockRoll);

            state.currentResource -= Math.Max(0, ability.resourceCost);
            if (state.cooldownUntilMilliseconds == null) state.cooldownUntilMilliseconds = new Dictionary<string, long>();
            long cooldown = Math.Max(0L, ability.cooldownMilliseconds);
            state.cooldownUntilMilliseconds[ability.id] = nowMilliseconds + cooldown;

            return new EROSkillCastResult
            {
                success = true,
                reason = combat.result == EROCombatResult.Miss ? "Miss." : "Cast successful.",
                ability = ability,
                combat = combat,
                remainingResource = state.currentResource,
                cooldownReadyAtMilliseconds = nowMilliseconds + cooldown
            };
        }
    }
}
