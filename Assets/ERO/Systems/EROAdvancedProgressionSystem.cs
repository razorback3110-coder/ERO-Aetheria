using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    public enum EROElement { Fire, Ice, Lightning, Earth, Wind, Light, Shadow, Arcane }
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
                case EROClass.Priest: return new[] { EROSpecialization.Holy, EROSpecialization.Oracle, EROSpecialization.BattlePriest };
                case EROClass.Invocateur: return new[] { EROSpecialization.SummonerDPS, EROSpecialization.SummonerTank, EROSpecialization.SummonerSupport };
                case EROClass.Mage: return new[] { EROSpecialization.Fire, EROSpecialization.Frost, EROSpecialization.Arcane };
                case EROClass.Assassin: return new[] { EROSpecialization.Shadow, EROSpecialization.Venom, EROSpecialization.AssassinCrit };
                case EROClass.Archer: return new[] { EROSpecialization.Sniper, EROSpecialization.Ranger, EROSpecialization.ElementalArcher };
                default: return Array.Empty<EROSpecialization>();
            }
        }

        public static bool CanUnlockTalent(EROTalentNode node, int level, Func<string, bool> prerequisiteMet)
        {
            if (node == null || level < node.requiredLevel || node.currentRank >= node.maxRank) return false;
            return string.IsNullOrEmpty(node.prerequisiteId) || (prerequisiteMet != null && prerequisiteMet(node.prerequisiteId));
        }

        public static bool TryUpgradeTranscendence(ERORarityState item, int materialCount, bool protectionAvailable)
        {
            if (item == null || materialCount <= 0 || item.transcendence.level >= MaxTranscendenceLevel) return false;
            int next = item.transcendence.level + 1;
            int cost = 1 + next / 4;
            if (materialCount < cost) return false;
            item.transcendence.level = next;
            if (protectionAvailable) item.transcendence.protectedFromDestruction = true;
            return true;
        }

        public static int CountSetPieces(string setId, ItemData[] equipped)
        {
            if (string.IsNullOrEmpty(setId) || equipped == null) return 0;
            int count = 0;
            foreach (var item in equipped)
                if (item != null && item.equipped && item.setId == setId) count++;
            return count;
        }

        public static int GetSetBonusBasisPoints(string setId, ItemData[] equipped, EROEquipmentSetDefinition definition)
        {
            if (definition == null || CountSetPieces(setId, equipped) < definition.requiredPieces) return 0;
            return Math.Max(0, definition.bonusBasisPoints);
        }

        public static bool DiscoverCodex(EROCodexEntry entry)
        {
            if (entry == null || entry.discovered) return false;
            entry.discovered = true;
            entry.completion = Math.Max(1, entry.completion);
            return true;
        }

        public static int CompletionPercent(EROCodexEntry entry)
        {
            if (entry == null || entry.maxCompletion <= 0) return 0;
            return Math.Max(0, Math.Min(100, entry.completion * 100 / entry.maxCompletion));
        }
    }

    [Serializable]
    public sealed class ERORarityState
    {
        public Rarity rarity;
        public EROTranscendenceState transcendence = new EROTranscendenceState();
    }
}
