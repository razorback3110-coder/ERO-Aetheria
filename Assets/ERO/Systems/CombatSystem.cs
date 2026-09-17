using System;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Unity-facing combat facade. The authoritative rules live in EROCombatStateStore/
    /// EROCombatResolution so the same simulation can run on a dedicated server.
    /// </summary>
    public sealed class CombatSystem : MonoBehaviour
    {
        private EROCombatStateStore stateStore;

        public event Action<EROCombatResult> CombatResolved;

        public int CalculateDamage(CharacterData c, int power, int defense, bool critical = false)
        {
            if (c == null) return 0;
            int baseDamage = Mathf.Max(1, power - defense);
            return critical ? Mathf.RoundToInt(baseDamage * 1.75f) : baseDamage;
        }

        public void InitializeAuthoritativeState(int actorCapacity = 256, int skillCapacity = 64)
        {
            stateStore = new EROCombatStateStore(actorCapacity, skillCapacity);
        }

        public EROCombatStateStore AuthoritativeState
        {
            get
            {
                if (stateStore == null) InitializeAuthoritativeState();
                return stateStore;
            }
        }

        public void RegisterActor(EROCombatantState state) => AuthoritativeState.RegisterActor(state);

        public void RegisterSkill(EROCombatSkill skill) => AuthoritativeState.RegisterSkill(skill);

        /// <summary>
        /// Returns the number of simulation ticks before an actor can use a skill again.
        /// The caller supplies the authoritative server tick; no wall-clock time is used.
        /// </summary>
        public ulong GetSkillCooldownRemainingTicks(ulong actorId, int skillId, ulong currentTick)
        {
            if (!AuthoritativeState.TryGetSkillReadyTick(actorId, skillId, out ulong readyTick))
                return 0UL;
            return currentTick >= readyTick ? 0UL : readyTick - currentTick;
        }

        /// <summary>
        /// Resolves one already-validated combat command against authoritative state.
        /// Returns false for invalid/dead/cooldown-locked commands; successful outcomes
        /// are emitted exactly once for UI, replication, logging and reward systems.
        /// </summary>
        public bool TryResolve(ulong tickId, ulong sequence, ulong actorId, ulong targetId, int skillId, ulong seed, out EROCombatResult result)
        {
            bool accepted = AuthoritativeState.TryResolve(tickId, sequence, actorId, targetId, skillId, seed, out result);
            if (accepted)
                CombatResolved?.Invoke(result);
            return accepted;
        }

        public bool CanAutoInRankedPvP() => false;
        public bool CanAutoInGvG() => false;
    }
}
