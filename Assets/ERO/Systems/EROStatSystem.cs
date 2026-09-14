using System;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public struct EROStatModifiers
    {
        public long strength;
        public long agility;
        public long dexterity;
        public long intelligence;
        public long vitality;
        public long spirit;
        public long luck;

        public long Get(EROPrimaryStat stat)
        {
            switch (stat)
            {
                case EROPrimaryStat.Strength: return strength;
                case EROPrimaryStat.Agility: return agility;
                case EROPrimaryStat.Dexterity: return dexterity;
                case EROPrimaryStat.Intelligence: return intelligence;
                case EROPrimaryStat.Vitality: return vitality;
                case EROPrimaryStat.Spirit: return spirit;
                case EROPrimaryStat.Luck: return luck;
                default: return 0;
            }
        }

        public void Add(EROPrimaryStat stat, long amount)
        {
            switch (stat)
            {
                case EROPrimaryStat.Strength: strength += amount; break;
                case EROPrimaryStat.Agility: agility += amount; break;
                case EROPrimaryStat.Dexterity: dexterity += amount; break;
                case EROPrimaryStat.Intelligence: intelligence += amount; break;
                case EROPrimaryStat.Vitality: vitality += amount; break;
                case EROPrimaryStat.Spirit: spirit += amount; break;
                case EROPrimaryStat.Luck: luck += amount; break;
            }
        }

        public int Total => checked((int)(strength + agility + dexterity + intelligence + vitality + spirit + luck));
    }

    public static class EROStatSystem
    {
        public const int StatPointsPerLevel = 5;
        public const int MaxAllocationPerStat = 1000;
        public const int PrimaryAffinityBasisPoints = 11500;
        public const int SecondaryAffinityBasisPoints = 10800;
        public const int BasisPoints = 10000;

        public static int GetTotalEarnedPoints(int level)
        {
            level = Math.Max(1, Math.Min(250, level));
            return (level - 1) * StatPointsPerLevel;
        }

        public static int GetSpentPoints(CharacterData character)
        {
            return character == null || character.stats == null ? 0 : Math.Max(0, character.stats.Total);
        }

        public static int GetAvailablePoints(CharacterData character)
        {
            if (character == null) return 0;
            int earned = GetTotalEarnedPoints(character.level);
            int spent = GetSpentPoints(character);
            return Math.Max(0, Math.Min(earned - spent, character.unspentStatPoints));
        }

        public static bool TryAllocate(CharacterData character, EROPrimaryStat stat, int amount)
        {
            if (character == null || character.stats == null || amount <= 0) return false;
            if (GetAvailablePoints(character) < amount) return false;
            if (character.stats.Get(stat) + amount > MaxAllocationPerStat) return false;
            character.stats.Add(stat, amount);
            character.unspentStatPoints -= amount;
            return true;
        }

        public static bool TryRefund(CharacterData character, EROPrimaryStat stat, int amount)
        {
            if (character == null || character.stats == null || amount <= 0) return false;
            if (character.stats.Get(stat) < amount) return false;
            character.stats.Add(stat, -amount);
            character.unspentStatPoints += amount;
            return true;
        }

        public static int GrantLevelUpPoints(CharacterData character, int levelsGained)
        {
            if (character == null || levelsGained <= 0) return 0;
            int points = checked(levelsGained * StatPointsPerLevel);
            character.unspentStatPoints = checked(character.unspentStatPoints + points);
            return points;
        }

        public static void ReconcileUnspentPoints(CharacterData character)
        {
            if (character == null) return;
            int earned = GetTotalEarnedPoints(character.level);
            int spent = GetSpentPoints(character);
            character.unspentStatPoints = Math.Max(0, earned - spent);
        }

        public static EROStatModifiers GetClassAffinity(EROClass classId)
        {
            var result = new EROStatModifiers();
            switch (classId)
            {
                case EROClass.Guerrier:
                    result.strength = PrimaryAffinityBasisPoints; result.vitality = PrimaryAffinityBasisPoints; result.agility = SecondaryAffinityBasisPoints; break;
                case EROClass.Paladin:
                    result.vitality = PrimaryAffinityBasisPoints; result.strength = PrimaryAffinityBasisPoints; result.spirit = SecondaryAffinityBasisPoints; break;
                case EROClass.Priest:
                    result.spirit = PrimaryAffinityBasisPoints; result.intelligence = PrimaryAffinityBasisPoints; result.vitality = SecondaryAffinityBasisPoints; break;
                case EROClass.Invocateur:
                    result.intelligence = PrimaryAffinityBasisPoints; result.spirit = PrimaryAffinityBasisPoints; result.dexterity = SecondaryAffinityBasisPoints; break;
                case EROClass.Mage:
                    result.intelligence = PrimaryAffinityBasisPoints; result.spirit = PrimaryAffinityBasisPoints; result.dexterity = SecondaryAffinityBasisPoints; break;
                case EROClass.Assassin:
                    result.agility = PrimaryAffinityBasisPoints; result.dexterity = PrimaryAffinityBasisPoints; result.luck = SecondaryAffinityBasisPoints; break;
                case EROClass.Archer:
                    result.dexterity = PrimaryAffinityBasisPoints; result.agility = PrimaryAffinityBasisPoints; result.luck = SecondaryAffinityBasisPoints; break;
            }
            return result;
        }

        public static EROStatModifiers GetEquipmentStatBonuses(ItemData[] equippedItems, EROGemData[] gems, ERORuneData[] runes)
        {
            var result = new EROStatModifiers();
            if (equippedItems != null)
                foreach (var item in equippedItems)
                    if (item != null && item.equipped) AddSockets(item, gems, runes, ref result);
            return result;
        }

        private static void AddSockets(ItemData item, EROGemData[] gems, ERORuneData[] runes, ref EROStatModifiers result)
        {
            if (item.sockets == null) return;
            foreach (var socket in item.sockets)
            {
                if (socket == null || !socket.unlocked) continue;
                if (!socket.runeSocket && !string.IsNullOrEmpty(socket.gemId) && gems != null)
                    foreach (var gem in gems)
                        if (gem != null && gem.id == socket.gemId) { result.Add(gem.stat, gem.statValue); break; }
                if (socket.runeSocket && !string.IsNullOrEmpty(socket.runeId) && runes != null)
                    foreach (var rune in runes)
                        if (rune != null && rune.id == socket.runeId) { result.Add(rune.stat, rune.statValue); break; }
            }
        }

        public static long ApplyAffinity(long value, int affinityBasisPoints)
        {
            if (value <= 0) return 0;
            return value * Math.Max(BasisPoints, affinityBasisPoints) / BasisPoints;
        }
    }
}
