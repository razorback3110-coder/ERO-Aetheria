using System;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public struct EROStatModifiers
    {
        public long strength, agility, dexterity, intelligence, vitality, spirit, luck;
        public int offensivePowerBasisPoints;
        public long Get(EROPrimaryStat stat) { switch (stat) { case EROPrimaryStat.Strength: return strength; case EROPrimaryStat.Agility: return agility; case EROPrimaryStat.Dexterity: return dexterity; case EROPrimaryStat.Intelligence: return intelligence; case EROPrimaryStat.Vitality: return vitality; case EROPrimaryStat.Spirit: return spirit; case EROPrimaryStat.Luck: return luck; default: return 0; } }
        public void Add(EROPrimaryStat stat, long amount) { switch (stat) { case EROPrimaryStat.Strength: strength += amount; break; case EROPrimaryStat.Agility: agility += amount; break; case EROPrimaryStat.Dexterity: dexterity += amount; break; case EROPrimaryStat.Intelligence: intelligence += amount; break; case EROPrimaryStat.Vitality: vitality += amount; break; case EROPrimaryStat.Spirit: spirit += amount; break; case EROPrimaryStat.Luck: luck += amount; break; } }
        public int Total => checked((int)(strength + agility + dexterity + intelligence + vitality + spirit + luck));
    }

    public static class EROStatSystem
    {
        public const int StatPointsPerLevel = 5;
        public const int MaxAllocationPerStat = 1000;
        public const int PrimaryAffinityBasisPoints = 11500;
        public const int SecondaryAffinityBasisPoints = 10800;
        public const int BasisPoints = 10000;

        public static int GetTotalEarnedPoints(int level) { level = Math.Max(1, Math.Min(250, level)); return (level - 1) * StatPointsPerLevel; }
        public static int GetSpentPoints(CharacterData c) => c == null || c.stats == null ? 0 : Math.Max(0, c.stats.Total);
        public static int GetAvailablePoints(CharacterData c) { if (c == null) return 0; return Math.Max(0, Math.Min(GetTotalEarnedPoints(c.level) - GetSpentPoints(c), c.unspentStatPoints)); }

        public static bool TryAllocate(CharacterData c, EROPrimaryStat stat, int amount)
        {
            if (c == null || c.stats == null || amount <= 0 || GetAvailablePoints(c) < amount) return false;
            if (c.stats.Get(stat) + amount > MaxAllocationPerStat) return false;
            c.stats.Add(stat, amount); c.unspentStatPoints -= amount; return true;
        }

        public static bool TryRefund(CharacterData c, EROPrimaryStat stat, int amount)
        {
            if (c == null || c.stats == null || amount <= 0 || c.stats.Get(stat) < amount) return false;
            c.stats.Add(stat, -amount); c.unspentStatPoints += amount; return true;
        }

        public static bool TryReset(CharacterData c)
        {
            if (c == null || c.stats == null) return false;
            int spent = c.stats.Total;
            c.stats = new EROStatAllocation();
            c.unspentStatPoints = Math.Max(0, Math.Min(GetTotalEarnedPoints(c.level), c.unspentStatPoints + spent));
            return true;
        }

        public static int GrantLevelUpPoints(CharacterData c, int levelsGained)
        {
            if (c == null || levelsGained <= 0) return 0;
            int points = checked(levelsGained * StatPointsPerLevel); c.unspentStatPoints = checked(c.unspentStatPoints + points); return points;
        }

        public static void ReconcileUnspentPoints(CharacterData c)
        {
            if (c == null) return;
            c.unspentStatPoints = Math.Max(0, GetTotalEarnedPoints(c.level) - GetSpentPoints(c));
        }

        public static EROStatModifiers GetClassAffinity(EROClass classId)
        {
            var r = new EROStatModifiers();
            switch (classId)
            {
                case EROClass.Guerrier: r.strength = PrimaryAffinityBasisPoints; r.vitality = PrimaryAffinityBasisPoints; r.agility = SecondaryAffinityBasisPoints; break;
                case EROClass.Paladin: r.vitality = PrimaryAffinityBasisPoints; r.strength = PrimaryAffinityBasisPoints; r.spirit = SecondaryAffinityBasisPoints; break;
                case EROClass.Priest: r.spirit = PrimaryAffinityBasisPoints; r.intelligence = PrimaryAffinityBasisPoints; r.vitality = SecondaryAffinityBasisPoints; break;
                case EROClass.Invocateur: r.intelligence = PrimaryAffinityBasisPoints; r.spirit = PrimaryAffinityBasisPoints; r.dexterity = SecondaryAffinityBasisPoints; break;
                case EROClass.Mage: r.intelligence = PrimaryAffinityBasisPoints; r.spirit = PrimaryAffinityBasisPoints; r.dexterity = SecondaryAffinityBasisPoints; break;
                case EROClass.Assassin: r.agility = PrimaryAffinityBasisPoints; r.dexterity = PrimaryAffinityBasisPoints; r.luck = SecondaryAffinityBasisPoints; break;
                case EROClass.Archer: r.dexterity = PrimaryAffinityBasisPoints; r.agility = PrimaryAffinityBasisPoints; r.luck = SecondaryAffinityBasisPoints; break;
            }
            return r;
        }

        public static EROStatModifiers GetEquipmentStatBonuses(ItemData[] equippedItems, EROGemData[] gems, ERORuneData[] runes)
        {
            var r = new EROStatModifiers();
            if (equippedItems != null) foreach (var item in equippedItems) if (item != null && item.equipped) AddSockets(item, gems, runes, ref r);
            return r;
        }

        public static bool TryInsertGem(ItemData item, int socketIndex, EROGemData gem)
        {
            if (item == null || gem == null || item.sockets == null || socketIndex < 0 || socketIndex >= item.sockets.Length) return false;
            var s = item.sockets[socketIndex];
            if (s == null || !s.unlocked || s.runeSocket || !string.IsNullOrEmpty(s.gemId)) return false;
            s.gemId = gem.id; return true;
        }

        public static bool TryInsertRune(ItemData item, int socketIndex, ERORuneData rune, EROClass characterClass, int characterLevel)
        {
            if (item == null || rune == null || item.sockets == null || socketIndex < 0 || socketIndex >= item.sockets.Length) return false;
            var s = item.sockets[socketIndex];
            if (s == null || !s.unlocked || !s.runeSocket || !string.IsNullOrEmpty(s.runeId)) return false;
            if (characterLevel < Math.Max(0, rune.requiredLevel) || rune.requiredClass != characterClass) return false;
            s.runeId = rune.id; return true;
        }

        public static string RemoveSocketItem(ItemData item, int socketIndex)
        {
            if (item == null || item.sockets == null || socketIndex < 0 || socketIndex >= item.sockets.Length) return null;
            var s = item.sockets[socketIndex]; if (s == null) return null;
            string id = s.runeSocket ? s.runeId : s.gemId;
            if (s.runeSocket) s.runeId = null; else s.gemId = null;
            return id;
        }

        private static void AddSockets(ItemData item, EROGemData[] gems, ERORuneData[] runes, ref EROStatModifiers r)
        {
            if (item.sockets == null) return;
            foreach (var s in item.sockets)
            {
                if (s == null || !s.unlocked) continue;
                if (!s.runeSocket && !string.IsNullOrEmpty(s.gemId) && gems != null) foreach (var g in gems) if (g != null && g.id == s.gemId) { r.Add(g.stat, g.statValue); break; }
                if (s.runeSocket && !string.IsNullOrEmpty(s.runeId) && runes != null) foreach (var rune in runes) if (rune != null && rune.id == s.runeId) { r.Add(rune.stat, rune.statValue); if (rune.type == ERORuneType.Offensive) r.offensivePowerBasisPoints = SafeBp(r.offensivePowerBasisPoints + rune.powerBasisPoints); break; }
            }
        }

        public static long ApplyAffinity(long value, int affinityBasisPoints) => value <= 0 ? 0 : value * Math.Max(BasisPoints, affinityBasisPoints) / BasisPoints;
        private static int SafeBp(int value) => Math.Max(0, Math.Min(BasisPoints, value));
    }
}
