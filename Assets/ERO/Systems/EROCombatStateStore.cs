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
        private readonly Dictionary<ulong, ulong> lastAcceptedSequences;

        public EROCombatStateStore(int actorCapacity = 256, int skillCapacity = 64)
        {
            if (actorCapacity < 1) throw new ArgumentOutOfRangeException(nameof(actorCapacity));
            if (skillCapacity < 1) throw new ArgumentOutOfRangeException(nameof(skillCapacity));
            combatants = new Dictionary<ulong, EROCombatantState>(actorCapacity);
            skills = new Dictionary<int, EROCombatSkill>(skillCapacity);
            skillReadyTicks = new Dictionary<ulong, Dictionary<int, ulong>>(actorCapacity);
            lastAcceptedSequences = new Dictionary<ulong, ulong>(actorCapacity);
        }

        public int ActorCount => combatants.Count;
        public int SkillCount => skills.Count;

        public void RegisterActor(EROCombatantState state)
        {
            if (state.ActorId == 0UL) throw new ArgumentException("Actor must have a valid id.", nameof(state));
            combatants[state.ActorId] = state;
            lastAcceptedSequences.Remove(state.ActorId);
            skillReadyTicks.Remove(state.ActorId);
        }

        public bool RemoveActor(ulong actorId)
        {
            skillReadyTicks.Remove(actorId);
            lastAcceptedSequences.Remove(actorId);
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

        public bool TryResolve(ulong tickId, ulong sequence, ulong actorId, ulong targetId, int skillId, ulong seed, out EROCombatResult result)
        {
            result = default;
            if (!combatants.TryGetValue(actorId, out EROCombatantState attacker)) return false;
            if (!combatants.TryGetValue(targetId, out EROCombatantState target)) return false;
            if (!skills.TryGetValue(skillId, out EROCombatSkill skill)) return false;
            if (attacker.Health <= 0 || target.Health <= 0) return false;
            if (lastAcceptedSequences.TryGetValue(actorId, out ulong lastSequence) && sequence <= lastSequence) return false;
            if (TryGetSkillReadyTick(actorId, skillId, out ulong readyTick) && tickId < readyTick) return false;

            result = EROCombatResolution.Resolve(tickId, sequence, attacker, target, skill, seed);
            lastAcceptedSequences[actorId] = sequence;

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

            if (!result.Hit && result.Damage == 0) return true;

            combatants[targetId] = new EROCombatantState(
                target.ActorId,
                target.Level,
                target.Attack,
                target.Defense,
                target.CritChancePercent,
                target.CritMultiplierPercent,
                target.MaxHealth,
                result.TargetHealth);
            return true;
        }
    }
}
