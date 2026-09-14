using System;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Bridges an authoritative network snapshot to the existing persistence model without trusting client state.</summary>
    public static class ERONetworkPersistenceBridge
    {
        public static bool ApplyAuthoritativeState(CharacterData character, ERONetworkPlayerState snapshot)
        {
            if (character == null || snapshot == null || !snapshot.IsValidFor(character)) return false;
            character.level = Math.Max(1, Math.Min(250, snapshot.level));
            character.xp = Math.Max(0L, snapshot.xp);
            character.overflowXp = Math.Max(0L, snapshot.overflowXp);
            character.credits = Math.Max(0L, snapshot.credits);
            character.eroCrystals = Math.Max(0L, snapshot.eroCrystals);
            return true;
        }

        public static ERONetworkPlayerState CreateSnapshot(CharacterData character, EROCombatStats stats, EROSkillRuntimeState skillState, int zoneId, long serverTick)
        {
            if (character == null) return null;
            var snapshot = new ERONetworkPlayerState();
            snapshot.Capture(character, stats, skillState, Math.Max(0, zoneId), Math.Max(0L, serverTick));
            return snapshot;
        }
    }
}
