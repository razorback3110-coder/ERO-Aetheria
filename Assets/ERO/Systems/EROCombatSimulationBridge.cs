using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Connects the deterministic combat command queue to the fixed-step ERO
    /// simulation driver. The bridge owns no gameplay rules: it only drains
    /// commands at the authoritative tick and exposes them to combat systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EROCombatSimulationBridge : MonoBehaviour
    {
        [SerializeField, Min(1)] private int commandCapacity = 256;

        private EROCombatCommandBuffer commandBuffer;
        private readonly List<EROCombatCommand> dueCommands = new List<EROCombatCommand>(256);
        private EROSimulationDriver simulationDriver;

        public int PendingCommandCount => commandBuffer?.Count ?? 0;
        public event Action<EROCombatCommand> OnCommandReady;

        private void Awake()
        {
            commandBuffer = new EROCombatCommandBuffer(commandCapacity);
            simulationDriver = GetComponent<EROSimulationDriver>();
            if (simulationDriver == null)
                simulationDriver = gameObject.AddComponent<EROSimulationDriver>();
        }

        private void OnEnable()
        {
            if (simulationDriver != null)
                simulationDriver.OnSimulationTick += ProcessTick;
        }

        private void OnDisable()
        {
            if (simulationDriver != null)
                simulationDriver.OnSimulationTick -= ProcessTick;
        }

        /// <summary>
        /// Queues client input for authoritative processing. Past ticks are
        /// normalized by EROCombatCommandBuffer to the current simulation tick.
        /// </summary>
        public bool TryQueueCommand(ulong actorId, ulong targetId, int skillId, ulong requestedTick, out EROCombatCommand command)
        {
            ulong currentTick = simulationDriver != null ? simulationDriver.TickId : 0UL;
            return commandBuffer.TryEnqueue(currentTick, requestedTick, actorId, targetId, skillId, out command);
        }

        private void ProcessTick(ulong tickId)
        {
            if (commandBuffer.DrainDue(tickId, dueCommands) == 0)
                return;

            for (int i = 0; i < dueCommands.Count; i++)
                OnCommandReady?.Invoke(dueCommands[i]);
        }
    }
}
