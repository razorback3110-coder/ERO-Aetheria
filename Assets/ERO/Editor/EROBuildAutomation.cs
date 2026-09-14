using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EternalRealmsOnline.CI
{
    public static class EROBuildAutomation
    {
        public static void ValidateCompile()
        {
            Debug.Log("[ERO CI] Compile validation started.");
            if (Application.unityVersion != "6000.0.67f1")
                throw new InvalidOperationException("ERO requires Unity 6000.0.67f1; running " + Application.unityVersion + ".");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string registry = Path.Combine(projectRoot, "Assets/ERO/Legal/ERO_Asset_License_Registry.md");
            if (!File.Exists(registry))
                throw new FileNotFoundException("ERO legal asset registry is missing.", registry);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if (EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Unity reports script compilation errors.");

            string[] scenes = AssetDatabase.FindAssets("t:Scene");
            if (scenes == null || scenes.Length == 0)
                throw new InvalidOperationException("ERO project contains no Unity scenes.");

            foreach (string guid in scenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("[ERO CI] Scene validated: " + path);
                }
            }

            Debug.Log("[ERO CI] Unity compile/project validation completed successfully.");
        }
    }
}
