using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EternalRealmsOnline.CI
{
    public static class EROBuildAutomation
    {
        private const string PlayableScene = "Assets/Scenes/ERO/ERO_Playable.unity";
        private const string WindowsBuildPath = "Builds/Windows/ERO.exe";

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

            bool playableSceneFound = false;
            foreach (string guid in scenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("[ERO CI] Scene validated: " + path);
                    if (string.Equals(path, PlayableScene, StringComparison.OrdinalIgnoreCase))
                        playableSceneFound = true;
                }
            }

            if (!playableSceneFound)
                throw new FileNotFoundException("ERO playable scene is missing.", PlayableScene);

            Debug.Log("[ERO CI] Unity compile/project validation completed successfully.");
        }

        public static void BuildWindows()
        {
            Debug.Log("[ERO CI] Windows playable build started.");
            if (Application.unityVersion != "6000.0.67f1")
                throw new InvalidOperationException("ERO requires Unity 6000.0.67f1; running " + Application.unityVersion + ".");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string registry = Path.Combine(projectRoot, "Assets/ERO/Legal/ERO_Asset_License_Registry.md");
            if (!File.Exists(registry))
                throw new FileNotFoundException("ERO legal asset registry is missing.", registry);

            if (!File.Exists(Path.Combine(projectRoot, PlayableScene)))
                throw new FileNotFoundException("ERO playable scene is missing.", PlayableScene);

            string buildDirectory = Path.Combine(projectRoot, "Builds/Windows");
            Directory.CreateDirectory(buildDirectory);
            string executablePath = Path.Combine(projectRoot, WindowsBuildPath);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { PlayableScene },
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[ERO CI] Building scene: " + PlayableScene);
            Debug.Log("[ERO CI] Output: " + executablePath);
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report == null || report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string result = report == null ? "no build report" : report.summary.result.ToString();
                throw new InvalidOperationException("ERO Windows build failed: " + result);
            }

            if (!File.Exists(executablePath))
                throw new FileNotFoundException("Unity reported a successful build but ERO.exe is missing.", executablePath);

            Debug.Log("[ERO CI] Windows playable build completed: " + executablePath);
        }
    }
}
