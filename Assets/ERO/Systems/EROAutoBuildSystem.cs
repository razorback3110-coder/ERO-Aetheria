using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    public enum EROBuildMode { Recommended, SemiAutomatic, Automatic }
    public enum EROBuildPurpose { PvE, PvP, MVP, GvG, Farming }

    [Serializable]
    public sealed class EROBuildProfile
    {
        public string id;
        public string name;
        public EROBuildPurpose purpose;
        public EROBuildMode mode = EROBuildMode.Recommended;
        public bool allowStatChanges = true;
        public bool allowGemChanges = true;
        public bool allowRuneChanges = true;
        public bool protectRareResources = true;
    }

    [Serializable]
    public struct EROBuildRecommendation
    {
        public EROPrimaryStat stat;
        public int statPoints;
        public string gemId;
        public string runeId;
        public int score;
    }

    /// <summary>Deterministic build advisor. UI/server adapters decide whether recommendations are applied.</summary>
    public static class EROAutoBuildSystem
    {
        private const int RareResourceRarity = (int)Rarity.Mythic;

        public static EROBuildRecommendation Recommend(CharacterData character, EROBuildPurpose purpose)
        {
            if (character == null) return default;
            EROPrimaryStat primary = PrimaryStat(character.classId);
            EROPrimaryStat secondary = SecondaryStat(character.classId);
            if (purpose == EROBuildPurpose.PvP || purpose == EROBuildPurpose.GvG)
                secondary = DefensiveSecondary(character.classId, secondary);
            if (purpose == EROBuildPurpose.Farming)
                secondary = EROPrimaryStat.Luck;

            return new EROBuildRecommendation
            {
                stat = primary,
                statPoints = EROStatSystem.GetAvailablePoints(character),
                score = 100
            };
        }

        public static bool TryApplyStats(CharacterData character, EROBuildPurpose purpose)
        {
            if (character == null) return false;
            var recommendation = Recommend(character, purpose);
            if (recommendation.statPoints <= 0) return false;
            return EROStatSystem.TryAllocate(character, recommendation.stat, recommendation.statPoints);
        }

        public static List<EROBuildRecommendation> RecommendEquipment(
            CharacterData character, ItemData[] equippedItems, EROGemData[] gems, ERORuneData[] runes, EROBuildPurpose purpose)
        {
            var results = new List<EROBuildRecommendation>();
            if (character == null) return results;
            var primary = PrimaryStat(character.classId);
            if (gems != null)
                foreach (var gem in gems)
                    if (gem != null && gem.stat == primary)
                        results.Add(new EROBuildRecommendation { stat = gem.stat, gemId = gem.id, score = GemScore(gem, purpose) });
            if (runes != null)
                foreach (var rune in runes)
                    if (rune != null && IsRuneUsable(rune, character, purpose))
                        results.Add(new EROBuildRecommendation { stat = rune.stat, runeId = rune.id, score = RuneScore(rune, purpose) });
            results.Sort((a, b) => b.score.CompareTo(a.score));
            return results;
        }

        public static bool IsProtectedFromAutomaticUse(Rarity rarity) =>
            (int)rarity >= RareResourceRarity;

        private static bool IsRuneUsable(ERORuneData rune, CharacterData c, EROBuildPurpose purpose)
        {
            if (rune.requiredLevel > c.level) return false;
            if (rune.requiredClass != c.classId && rune.type == ERORuneType.Class) return false;
            if (purpose == EROBuildPurpose.PvE && rune.type == ERORuneType.Defensive) return false;
            return true;
        }

        private static int GemScore(EROGemData gem, EROBuildPurpose purpose)
        {
            int score = gem.statValue * 10 + (int)gem.rarity * 25 + gem.level * 5;
            if (purpose == EROBuildPurpose.Farming && gem.stat == EROPrimaryStat.Luck) score += 250;
            return score;
        }

        private static int RuneScore(ERORuneData rune, EROBuildPurpose purpose)
        {
            int score = rune.statValue * 10 + rune.powerBasisPoints / 100 + (int)rune.rarity * 30;
            if (purpose == EROBuildPurpose.PvP && rune.type == ERORuneType.Defensive) score += 150;
            if (purpose == EROBuildPurpose.MVP && rune.type == ERORuneType.Offensive) score += 200;
            return score;
        }

        private static EROPrimaryStat PrimaryStat(EROClass c)
        {
            switch (c)
            {
                case EROClass.Paladin: return EROPrimaryStat.Vitality;
                case EROClass.Priest: return EROPrimaryStat.Spirit;
                case EROClass.Invocateur: return EROPrimaryStat.Intelligence;
                case EROClass.Mage: return EROPrimaryStat.Intelligence;
                case EROClass.Assassin: return EROPrimaryStat.Agility;
                case EROClass.Archer: return EROPrimaryStat.Dexterity;
                case EROClass.Guerrier: return EROPrimaryStat.Strength;
                default: return EROPrimaryStat.Vitality;
            }
        }

        private static EROPrimaryStat SecondaryStat(EROClass c)
        {
            switch (c)
            {
                case EROClass.Paladin: return EROPrimaryStat.Strength;
                case EROClass.Priest: return EROPrimaryStat.Intelligence;
                case EROClass.Invocateur: return EROPrimaryStat.Spirit;
                case EROClass.Mage: return EROPrimaryStat.Spirit;
                case EROClass.Assassin: return EROPrimaryStat.Dexterity;
                case EROClass.Archer: return EROPrimaryStat.Agility;
                case EROClass.Guerrier: return EROPrimaryStat.Vitality;
                default: return EROPrimaryStat.Vitality;
            }
        }

        private static EROPrimaryStat DefensiveSecondary(EROClass c, EROPrimaryStat fallback)
        {
            if (c == EROClass.Paladin || c == EROClass.Guerrier) return EROPrimaryStat.Vitality;
            if (c == EROClass.Priest || c == EROClass.Mage || c == EROClass.Invocateur) return EROPrimaryStat.Spirit;
            return fallback;
        }
    }
}
