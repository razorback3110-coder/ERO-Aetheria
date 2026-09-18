using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>
    /// Authoritative, allocation-conscious combat state store. It applies the
    /// pure EROCombatResolution result to server-owned actor state and exposes
    /// immutable outcomes for XP/loot/replication systems.
    /// </summary>
    public sealed class EROCombatStateStore
    {
        private readonly Dictionary<ulong, EROCombatantState> combatants;
        private readonly Dictionary<int, EROCombatSkill> skills;
        private readonly Dictionary<ulong, Dictionary<int, ulong>> skillReadyTicks;
        private readonly Dictionary<ulong, ulong> lastProcessedSequences;
        private readonly Dictionary<ulong, ulong> respawnReadyTicks;
        private readonly EROCombatRewardLedger rewardLedger;
        private readonly ulong respawnDelayTicks;

        public EROCombatStateStore(int actorCapacity = 256, int skillCapacity = 64, ulong respawnDelayTicks = 100UL)
        {
            if (actorCapacity < 1) throw new ArgumentOutOfRangeException(nameof(actorCapacity));
            if (skillCapacity < 1) throw new ArgumentOutOfRangeException(nameof(skillCapacity));
            this.respawnDelayTicks = respawnDelayTicks;
            combatants = new Dictionary<ulong, EROCombatantState>(actorCapacity);
            skills = new Dictionary<int, EROCombatSkill>(skillCapacity);
            skillReadyTicks = new Dictionary<ulong, Dictionary<int, ulong>>(actorCapacity);
            lastProcessedSequences = new Dictionary<ulong, ulong>(actorCapacity);
            respawnReadyTicks = new Dictionary<ulong, ulong>(actorCapacity);
            rewardLedger = new EROCombatRewardLedger(actorCapacity);
        }

        public int ActorCount => combatants.Count;
        public int SkillCount => skills.Count;
        public EROCombatRewardLedger RewardLedger => rewardLedger;

        /// <summary>Raised after an accepted combat command has mutated authoritative state.</summary>
        public event Action<EROCombatResult> CombatResolved;

        /// <summary>Raised once when an accepted hit reduces a target from alive to defeated.</summary>
        public event Action<ulong, ulong> CombatantDefeated;

        /// <summary>Raised once when a deterministic reward is committed for a defeat.</summary>
        public event Action<EROCombatReward> CombatRewardGranted;

        public void RegisterActor(EROCombatantState state)
        {
            if (state.ActorId == 0UL) throw new ArgumentException("Actor must have a valid id.", nameof(state));
            combatants[state.ActorId] = state;
            lastProcessedSequences.Remove(state.ActorId);
            skillReadyTicks.Remove(state.ActorId);
            respawnReadyTicks.Remove(state.ActorId);
        }

        public bool RemoveActor(ulong actorId)
        {
            skillReadyTicks.Remove(actorId);
            lastProcessedSequences.Remove(actorId);
            respawnReadyTicks.Remove(actorId);
            return combatants.Remove(actorId);
        }

        public bool TryGetActor(ulong actorId, out EROCombatantState state) => combatants.TryGetValue(actorId, out state);

        public void RegisterSkill(EROCombatSkill skill)
        {
            if (skill.SkillId < 0) throw new ArgumentOutOfRangeException(nameof(skill));
            skills[skill.SkillId] = skill;
        }

        public bool TryGetSkillReadyTick(ulong actorId, int skillId, out ulong readyTick)
        {
            readyTick = 0UL;
            return skillReadyTicks.TryGetValue(actorId, out Dictionary<int, ulong> actorCooldowns)
                && actorCooldowns.TryGetValue(skillId, out readyTick);
        }

        /// <summary>Returns the authoritative tick at which a defeated actor may respawn.</summary>
        public bool TryGetRespawnReadyTick(ulong actorId, out ulong readyTick)
            => respawnReadyTicks.TryGetValue(actorId, out readyTick);

        /// <summary>
        /// Restores a defeated actor when the server-owned respawn-ready tick is reached.
        /// The ready tick is created by the authoritative store on defeat; callers cannot
        /// shorten the delay by supplying a client-controlled timestamp.
        /// </summary>
        public bool TryRespawnActor(ulong actorId, ulong currentTick)
        {
            if (!combatants.TryGetValue(actorId, out EROCombatantState actor)) return false;
            if (actor.Health > 0) return false;
            if (!respawnReadyTicks.TryGetValue(actorId, out ulong readyTick)) return false;
            if (currentTick < readyTick) return false;

            combatants[actorId] = new EROCombatantState(
                actor.ActorId,
                actor.Level,
                actor.Attack,
                actor.Defense,
                actor.CritChancePercent,
                actor.CritMultiplierPercent,
                actor.MaxHealth,
                actor.MaxHealth);
            skillReadyTicks.Remove(actorId);
            respawnReadyTicks.Remove(actorId);
            return true;
        }

        /// <summary>
        /// Compatibility overload. The supplied ready tick is validated against the
        /// server-owned schedule and can never override it.
        /// </summary>
        public bool TryRespawnActor(ulong actorId, ulong currentTick, ulong expectedReadyTick)
        {
            return TryGetRespawnReadyTick(actorId, out ulong readyTick)
                && readyTick == expectedReadyTick
                && TryRespawnActor(actorId, currentTick);
        }

        public bool TryResolve(ulong tickId, ulong sequence, ulong actorId, ulong targetId, int skillId, ulong seed, out EROCombatResult result)
        {
            result = default;
            if (!combatants.TryGetValue(actorId, out EROCombatantState attacker)) return false;
            if (!combatants.TryGetValue(targetId, out EROCombatantState target)) return false;
            if (!skills.TryGetValue(skillId, out EROCombatSkill skill)) return false;
            if (attacker.Health <= 0 || target.Health <= 0) return false;
            if (lastProcessedSequences.TryGetValue(actorId, out ulong lastSequence) && sequence <= lastSequence) return false;

            // Consume a valid command sequence even when the skill is rejected by
            // cooldown. This prevents replaying the same command after the cooldown
            // expires and keeps the authoritative command stream monotonic.
            lastProcessedSequences[actorId] = sequence;
            if (TryGetSkillReadyTick(actorId, skillId, out ulong readyTick) && tickId < readyTick) return false;

            result = EROCombatResolution.Resolve(tickId, sequence, attacker, target, skill, seed);

            if (skill.CooldownTicks > 0UL)
            {
                ulong nextReadyTick = ulong.MaxValue - tickId < skill.CooldownTicks
                    ? ulong.MaxValue
                    : tickId + skill.CooldownTicks;
                if (!skillReadyTicks.TryGetValue(actorId, out Dictionary<int, ulong> actorCooldowns))
                {
                    actorCooldowns = new Dictionary<int, ulong>();
                    skillReadyTicks.Add(actorId, actorCooldowns);
                }
                actorCooldowns[skillId] = nextReadyTick;
            }

            if (!result.Hit && result.Damage == 0)
            {
                CombatResolved?.Invoke(result);
                return true;
            }

            combatants[targetId] = new EROCombatantState(
                target.ActorId,
                target.Level,
                target.Attack,
                target.Defense,
                target.CritChancePercent,
                target.CritMultiplierPercent,
                target.MaxHealth,
                result.TargetHealth);

            CombatResolved?.Invoke(result);
            if (target.Health > 0 && result.TargetHealth <= 0)
            {
                ulong respawnReadyTick = ulong.MaxValue - tickId < respawnDelayTicks
                    ? ulong.MaxValue
                    : tickId + respawnDelayTicks;
                respawnReadyTicks[targetId] = respawnReadyTick;
                CombatantDefeated?.Invoke(targetId, actorId);
                if (rewardLedger.TryApply(result, target, out EROCombatReward reward))
                    CombatRewardGranted?.Invoke(reward);
            }
            return true;
        }
    }
}
