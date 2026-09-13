using System;
using UnityEngine;
using Avatar = Unity.BossRoom.Gameplay.Configuration.Avatar;

namespace Unity.BossRoom.Gameplay.GameState
{
    /// <summary>
    /// ERO compatibility bridge for Boss Room character selection.
    /// It first uses the serialized Avatar reference and then resolves the exact
    /// character-select prefab from Resources using the Avatar asset name.
    /// </summary>
    internal static class EROAvatarReferenceRepairV509
    {
        public static GameObject GetCharacterSelectPrefab(Avatar avatar)
        {
            if (avatar == null) return null;

            GameObject prefab = null;
            try
            {
                prefab = avatar.GraphicsCharacterSelect;
            }
            catch (MissingReferenceException)
            {
                // Unity can keep a managed wrapper while the serialized object is missing.
            }

            if (prefab != null)
                return prefab;

            string resourceName = GetResourceName(avatar.name);
            if (string.IsNullOrEmpty(resourceName))
            {
                Debug.LogError($"ERO V511: Unknown Avatar '{avatar.name}'. No character-select prefab mapping exists.");
                return null;
            }

            prefab = Resources.Load<GameObject>(resourceName);
            if (prefab == null)
            {
                Debug.LogError($"ERO V511: Missing Resources prefab '{resourceName}' for Avatar '{avatar.name}'.");
            }

            return prefab;
        }

        static string GetResourceName(string avatarName)
        {
            if (string.IsNullOrEmpty(avatarName)) return null;
            switch (avatarName)
            {
                case "TankBoy": return "EROCharacters/PlayerGraphics_Tank_Boy_CharacterSelect";
                case "TankGirl": return "EROCharacters/PlayerGraphics_Tank_Girl_CharacterSelect";
                case "RogueBoy": return "EROCharacters/PlayerGraphics_Rogue_Boy_CharacterSelect";
                case "RogueGirl": return "EROCharacters/PlayerGraphics_Rogue_Girl_CharacterSelect";
                case "ArcherBoy": return "EROCharacters/PlayerGraphics_Archer_Boy_CharacterSelect";
                case "ArcherGirl": return "EROCharacters/PlayerGraphics_Archer_Girl_CharacterSelect";
                case "MageBoy": return "EROCharacters/PlayerGraphics_Mage_Boy_CharacterSelect";
                case "MageGirl": return "EROCharacters/PlayerGraphics_Mage_Girl_CharacterSelect";
                default: return null;
            }
        }
    }
}
