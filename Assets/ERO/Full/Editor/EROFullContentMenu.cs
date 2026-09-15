#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace EternalRealmsOnline.Full.Editor
{
    public static class EROFullContentMenu
    {
        [MenuItem("ERO/Production/Validate Full Content")]
        public static void Validate()
        {
            Debug.Log($"ERO Full Content: {EROWorldContent.Zones.Length} zones, launch classes: {System.Enum.GetNames(typeof(ERO.Data.EROClass)).Length}. Runtime systems installed.");
        }

        [MenuItem("ERO/Production/Create Content Folders")]
        public static void Folders()
        {
            foreach (var p in new[]
            {
                "Assets/ERO/Full/Runtime",
                "Assets/ERO/Full/Data",
                "Assets/ERO/Full/Editor",
                "Assets/ERO/World",
                "Assets/ERO/UI",
                "Assets/ERO/Audio",
                "Assets/ERO/VFX"
            })
            {
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            }

            AssetDatabase.Refresh();
            Debug.Log("ERO content folders created.");
        }
    }
}
#endif
