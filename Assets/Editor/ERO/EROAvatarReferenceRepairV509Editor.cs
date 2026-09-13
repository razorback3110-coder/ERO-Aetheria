#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Avatar = Unity.BossRoom.Gameplay.Configuration.Avatar;

namespace EternalRealmsOnline.V509.Editor
{
    public static class EROAvatarReferenceRepairV509Editor
    {
        const string AvatarFolder = "Assets/GameData/Avatars";
        const string ResourceFolder = "Assets/Resources/EROCharacters";

        [MenuItem("Eternal Realms Online/Repair Avatar Character-Select References")]
        public static void Repair()
        {
            string[] guids = AssetDatabase.FindAssets("t:Avatar", new[] { AvatarFolder });
            int repaired = 0;
            int missing = 0;

            foreach (string guid in guids)
            {
                string avatarPath = AssetDatabase.GUIDToAssetPath(guid);
                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
                if (avatar == null) continue;

                string prefabName = GetPrefabName(avatar.name);
                if (prefabName == null) continue;

                string prefabPath = ResourceFolder + "/" + prefabName + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"ERO V513: Cannot repair {avatar.name}: missing prefab at {prefabPath}");
                    missing++;
                    continue;
                }

                bool needsRepair = false;
                try { needsRepair = avatar.GraphicsCharacterSelect != prefab; }
                catch (MissingReferenceException) { needsRepair = true; }

                if (needsRepair)
                {
                    avatar.GraphicsCharacterSelect = prefab;
                    EditorUtility.SetDirty(avatar);
                    repaired++;
                }
            }

            if (repaired > 0)
                AssetDatabase.SaveAssets();

            Debug.Log($"ERO V513 Avatar repair complete: {repaired} repaired, {missing} missing.");
        }

        static string GetPrefabName(string avatarName)
        {
            switch (avatarName)
            {
                case "TankBoy": return "PlayerGraphics_Tank_Boy_CharacterSelect";
                case "TankGirl": return "PlayerGraphics_Tank_Girl_CharacterSelect";
                case "RogueBoy": return "PlayerGraphics_Rogue_Boy_CharacterSelect";
                case "RogueGirl": return "PlayerGraphics_Rogue_Girl_CharacterSelect";
                case "ArcherBoy": return "PlayerGraphics_Archer_Boy_CharacterSelect";
                case "ArcherGirl": return "PlayerGraphics_Archer_Girl_CharacterSelect";
                case "MageBoy": return "PlayerGraphics_Mage_Boy_CharacterSelect";
                case "MageGirl": return "PlayerGraphics_Mage_Girl_CharacterSelect";
                default: return null;
            }
        }
    }
}
#endif
