using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROElementalModifiers
    {
        public EROElement element;
        public int attackBasisPoints;
        public int resistanceBasisPoints;
    }

    public static class EROBuildPresetSystem
    {
        public const int MaximumPresetCount = 20;

        public static bool ValidatePreset(EROBuildPreset preset, CharacterData character)
        {
            if (preset == null || character == null || string.IsNullOrEmpty(preset.id)) return false;
            if (preset.specialization != EROSpecialization.None &&
                !ContainsSpecialization(EROAdvancedProgressionSystem.GetSpecializations(character.classId), preset.specialization)) return false;
            return preset.stats != null && preset.stats.Total >= 0;
        }

        public static bool TryCreatePreset(List<EROBuildPreset> presets, EROBuildPreset preset, CharacterData character)
        {
            if (presets == null || preset == null || presets.Count >= MaximumPresetCount || !ValidatePreset(preset, character)) return false;
            if (presets.Exists(x => x != null && x.id == preset.id)) return false;
            presets.Add(preset);
            return true;
        }

        public static bool TryRemovePreset(List<EROBuildPreset> presets, string id)
        {
            if (presets == null || string.IsNullOrEmpty(id)) return false;
            int index = presets.FindIndex(x => x != null && x.id == id);
            if (index < 0) return false;
            presets.RemoveAt(index);
            return true;
        }

        public static int GetElementAttackModifier(EROElement attacker, EROElement defender)
        {
            if (attacker == defender) return 10000;
            if (IsStrongAgainst(attacker, defender)) return 11500;
            if (IsWeakAgainst(attacker, defender)) return 8500;
            return 10000;
        }

        public static int GetElementResistanceModifier(int resistanceBasisPoints)
        {
            return Math.Max(0, Math.Min(10000, resistanceBasisPoints));
        }

        private static bool ContainsSpecialization(IReadOnlyList<EROSpecialization> list, EROSpecialization value)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == value) return true;
            return false;
        }

        private static bool IsStrongAgainst(EROElement a, EROElement b)
        {
            return (a == EROElement.Fire && b == EROElement.Ice) ||
                   (a == EROElement.Ice && b == EROElement.Lightning) ||
                   (a == EROElement.Lightning && b == EROElement.Wind) ||
                   (a == EROElement.Wind && b == EROElement.Earth) ||
                   (a == EROElement.Earth && b == EROElement.Fire) ||
                   (a == EROElement.Light && b == EROElement.Shadow) ||
                   (a == EROElement.Shadow && b == EROElement.Light) ||
                   (a == EROElement.Arcane && b == EROElement.Arcane);
        }

        private static bool IsWeakAgainst(EROElement a, EROElement b)
        {
            return IsStrongAgainst(b, a);
        }
    }
}
