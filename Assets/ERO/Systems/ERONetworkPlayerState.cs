using System;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Deterministic network-safe snapshot for a player. Transport-agnostic so it can be
    /// replicated by Netcode for GameObjects without coupling gameplay rules to a client.
    /// </summary>
    [Serializable]
    public sealed class ERONetworkPlayerState
    {
        public string characterId;
        public int level;
        public long xp;
        public long overflowXp;
        public long credits;
        public long eroCrystals;
        public int health;
        public int maxHealth;
        public int resource;
        public int maxResource;
        public EROClass classId;
        public int zoneId;
        public long serverTick;

        public void Capture(CharacterData character, EROCombatStats combatStats, EROSkillRuntimeState skillState, int zone, long tick)
        {
            if (character == null) return;
            characterId = character.id;
            level = Math.Max(1, character.level);
            xp = Math.Max(0L, character.xp);
            overflowXp = Math.Max(0L, character.overflowXp);
            credits = Math.Max(0L, character.credits);
            eroCrystals = Math.Max(0L, character.eroCrystals);
            classId = character.classId;
            maxHealth = Math.Max(0, combatStats.maxHealth);
            health = maxHealth;
            if (skillState != null)
            {
                maxResource = (int)Math.Min(int.MaxValue, Math.Max(0L, skillState.maximumResource));
                resource = (int)Math.Min(int.MaxValue, Math.Max(0L, skillState.currentResource));
            }
            zoneId = zone;
            serverTick = tick;
        }

        public bool IsValidFor(CharacterData character)
        {
            return character != null && !string.IsNullOrEmpty(characterId) && character.id == characterId &&
                   level == Math.Max(1, character.level) && classId == character.classId &&
                   xp >= 0L && overflowXp >= 0L && credits >= 0L && eroCrystals >= 0L;
        }
    }
}
