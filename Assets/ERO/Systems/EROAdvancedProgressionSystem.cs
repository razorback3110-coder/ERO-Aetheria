using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    public enum EROSpecialization { None, Berserker, Gladiator, Juggernaut, Holy, BattlePriest, Oracle, SummonerDPS, SummonerTank, SummonerSupport, Fire, Frost, Arcane, Shadow, Venom, AssassinCrit, Sniper, Ranger, ElementalArcher }
    public enum EROEquipmentSetBonusTier { TwoPieces = 2, FourPieces = 4, SixPieces = 6 }

    [Serializable]
    public sealed class EROTalentNode
    {
        public string id;
        public string specializationId;
        public int requiredLevel;
        public int maxRank = 1;
        public int currentRank;
        public string prerequisiteId;
    }

    [Serializable]
    public sealed class EROElementProfile
    {
        public EROElement element;
        public int attackBasisPoints;
        public int resistanceBasisPoints;
    }

    [Serializable]
    public sealed class EROEquipmentSetDefinition
    {
        public string id;
        public string name;
        public int requiredPieces = 2;
        public int bonusBasisPoints;
        public string effectId;
    }

    [Serializable]
    public sealed class EROTranscendenceState
    {
        public int level;
        public int maxLevel = 20;
        public bool protectedFromDestruction;
    }

    [Serializable]
    public sealed class EROBuildPreset
    {
        public string id;
        public string name;
        public EROBuildPurpose purpose;
        public EROSpecialization specialization;
        public EROStatAllocation stats = new EROStatAllocation();
        public string[] equipmentIds = Array.Empty<string>();
        public string[] gemIds = Array.Empty<string>();
        public string[] runeIds = Array.Empty<string>();
        public string[] talentIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class EROCodexEntry
    {
        public string id;
        public string category;
        public string displayName;
        public bool discovered;
        public int completion;
        public int maxCompletion = 1;
    }

    public static class EROAdvancedProgressionSystem
    {
        public const int MaxTranscendenceLevel = 20;
        public const int SetTwoPiece = 2;
        public const int SetFourPiece = 4;
        public const int SetSixPiece = 6;

        public static IReadOnlyList<EROSpecialization> GetSpecializations(EROClass classId)
        {
            switch (classId)
            {
                case EROClass.Guerrier: return new[] { EROSpecialization.Berserker, EROSpecialization.Gladiator, EROSpecialization.Juggernaut };
                case EROClass.Paladin: return new[] { EROSpecialization.Holy, EROSpecialization.BattlePriest, EROSpecialization.Juggernaut };
                case EROClass.Priest: return new[] { EROSpecialization.Holy, EROSpecialization.BattlePriest, EROSpecialization.Oracle };
                case EROClass.Invocateur: return new[] { EROSpecialization.SummonerDPS, EROSpecialization.SummonerTank, EROSpecialization.SummonerSupport };
                case EROClass.Mage: return new[] { EROSpecialization.Fire, EROSpecialization.Frost, EROSpecialization.Arcane };
                case EROClass.Assassin: return new[] { EROSpecialization.Shadow, EROSpecialization.Venom, EROSpecialization.AssassinCrit };
                case EROClass.Archer: return new[] { EROSpecialization.Sniper, EROSpecialization.Ranger, EROSpecialization.ElementalArcher };
                default: return Array.Empty<EROSpecialization>();
            }
        }

        public static bool CanUnlockTalent(EROTalentNode node, int characterLevel, IReadOnlyCollection<string> unlockedTalentIds)
        {
            if (node == null || characterLevel < node.requiredLevel || node.currentRank >= node.maxRank) return false;
            return string.IsNullOrEmpty(node.prerequisiteId) || (unlockedTalentIds != null && unlockedTalentIds.Contains(node.prerequisiteId));
        }

        public static bool TryUpgradeTranscendence(EROTranscendenceState state, Rarity rarity, int requiredLevel = 1)
        {
            if (state == null || state.level >= Math.Min(state.maxLevel, MaxTranscendenceLevel) || requiredLevel < 1) return false;
            if (rarity < Rarity.Legendary) return false;
            state.level++;
            return true;
        }

        public static int CountSetPieces(IReadOnlyList<ItemData> equipment, string setId)
        {
            if (equipment == null || string.IsNullOrEmpty(setId)) return 0;
            int count = 0;
            foreach (var item in equipment) if (item != null && item.equipped && item.setId == setId) count++;
            return count;
        }

        public static int GetSetBonusBasisPoints(int pieces, EROEquipmentSetDefinition definition)
            => definition != null && pieces >= definition.requiredPieces ? definition.bonusBasisPoints : 0;

        public static bool DiscoverCodex(EROCodexEntry entry, int amount = 1)
        {
            if (entry == null || amount <= 0) return false;
            entry.discovered = true;
            entry.completion = Math.Min(entry.maxCompletion, entry.completion + amount);
            return true;
        }

        public static int CompletionPercent(EROCodexEntry entry)
            => entry == null || entry.maxCompletion <= 0 ? 0 : Math.Min(100, entry.completion * 100 / entry.maxCompletion);
    }
}
