using System;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Deterministic combat-roll helper for authoritative servers, replays and rollback-safe simulation.
    /// The caller supplies a stable seed; no UnityEngine dependency is required.
    /// </summary>
    public static class EROCombatDeterminism
    {
        public static EROCombatEvent ResolveAttack(
            EROCombatStats attacker,
            EROCombatStats defender,
            EROSkillDefinition skill,
            ulong seed)
        {
            uint state = Mix((uint)seed) ^ Mix((uint)(seed >> 32));
            int accuracy = NextBasisPoints(ref state);
            int critical = NextBasisPoints(ref state);
            int block = NextBasisPoints(ref state);
            return EROCombatSystem.ResolveAttack(attacker, defender, skill, accuracy, critical, block);
        }

        private static int NextBasisPoints(ref uint state)
        {
            state = 1664525u * state + 1013904223u;
            return (int)(state % (uint)EROCombatSystem.BasisPoints);
        }

        private static uint Mix(uint value)
        {
            value ^= value >> 16;
            value *= 0x7feb352du;
            value ^= value >> 15;
            value *= 0x846ca68bu;
            value ^= value >> 16;
            return value;
        }
    }
}
