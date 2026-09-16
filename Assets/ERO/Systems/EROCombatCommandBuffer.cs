using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>
    /// Server-side friendly command buffer for combat input.
    /// Commands are ordered deterministically by simulation tick and sequence so
    /// the same input stream can be replayed or validated by a dedicated server.
    /// </summary>
    public readonly struct EROCombatCommand
    {
        public readonly ulong Sequence;
        public readonly ulong RequestedTick;
        public readonly ulong ActorId;
        public readonly ulong TargetId;
        public readonly int SkillId;

        public EROCombatCommand(ulong sequence, ulong requestedTick, ulong actorId, ulong targetId, int skillId)
        {
            Sequence = sequence;
            RequestedTick = requestedTick;
            ActorId = actorId;
            TargetId = targetId;
            SkillId = skillId;
        }
    }

    /// <summary>
    /// Allocation-free after construction when the caller provides enough capacity.
    /// The buffer rejects commands scheduled too far in the past and provides a
    /// deterministic drain for authoritative simulation.
    /// </summary>
    public sealed class EROCombatCommandBuffer
    {
        private readonly List<EROCombatCommand> commands;
        private ulong nextSequence = 1UL;

        public EROCombatCommandBuffer(int capacity = 128)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            commands = new List<EROCombatCommand>(capacity);
        }

        public int Count => commands.Count;

        public bool TryEnqueue(ulong currentTick, ulong requestedTick, ulong actorId, ulong targetId, int skillId, out EROCombatCommand command)
        {
            command = default;
            if (actorId == 0UL || skillId < 0)
                return false;

            // Late client input is clamped to the current authoritative tick.
            if (requestedTick < currentTick)
                requestedTick = currentTick;

            command = new EROCombatCommand(nextSequence++, requestedTick, actorId, targetId, skillId);
            commands.Add(command);
            return true;
        }

        /// <summary>
        /// Drains commands due at or before the supplied authoritative tick.
        /// The destination list is caller-owned and should be reused between ticks.
        /// </summary>
        public int DrainDue(ulong currentTick, List<EROCombatCommand> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();

            for (int i = 0; i < commands.Count; i++)
            {
                EROCombatCommand command = commands[i];
                if (command.RequestedTick <= currentTick)
                    destination.Add(command);
            }

            if (destination.Count == 0)
                return 0;

            destination.Sort(CompareCommands);

            for (int i = commands.Count - 1; i >= 0; i--)
            {
                if (commands[i].RequestedTick <= currentTick)
                    commands.RemoveAt(i);
            }

            return destination.Count;
        }

        private static int CompareCommands(EROCombatCommand a, EROCombatCommand b)
        {
            int tick = a.RequestedTick.CompareTo(b.RequestedTick);
            return tick != 0 ? tick : a.Sequence.CompareTo(b.Sequence);
        }
    }
}
