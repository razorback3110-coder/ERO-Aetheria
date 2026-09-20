using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EternalRealmsOnline.CI
{
    public static class EROBuildAutomation
    {
        private const string RequiredUnityVersion = "6000.0.67f1";
        private const string RequiredUnityRevision = "78a1c2bbeb6a";
        private const string PlayableScene = "Assets/Scenes/ERO/ERO_Playable.unity";
        private const string WindowsBuildPath = "Builds/Windows/ERO.exe";

        public static void ValidateCompile()
        {
            Debug.Log("[ERO CI] Compile validation started.");
            ValidateUnityProjectIdentity();
            ValidateLegalRegistry();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if (EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Unity reports script compilation errors.");

            ValidateScenes();
            Debug.Log("[ERO CI] Unity compile/project validation completed successfully.");
            EditorApplication.Exit(0);
        }

        public static void BuildWindows()
        {
            Debug.Log("[ERO CI] Windows playable build started.");
            ValidateUnityProjectIdentity();
            ValidateLegalRegistry();
            ValidatePlayableScene();

            if (EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Unity reports script compilation errors before the Windows build.");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
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
            EditorApplication.Exit(0);
        }

        private static void ValidateUnityProjectIdentity()
        {
            if (!string.Equals(Application.unityVersion, RequiredUnityVersion, StringComparison.Ordinal))
                throw new InvalidOperationException("ERO requires Unity " + RequiredUnityVersion + "; running " + Application.unityVersion + ".");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string versionFile = Path.Combine(projectRoot, "ProjectSettings/ProjectVersion.txt");
            if (!File.Exists(versionFile))
                throw new FileNotFoundException("Unity project version file is missing.", versionFile);

            string expected = RequiredUnityVersion + " (" + RequiredUnityRevision + ")";
            string actual = string.Empty;
            foreach (string line in File.ReadAllLines(versionFile))
            {
                if (line.StartsWith("m_EditorVersionWithRevision:", StringComparison.Ordinal))
                {
                    actual = line.Substring("m_EditorVersionWithRevision:".Length).Trim();
                    break;
                }
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
                throw new InvalidOperationException("ERO requires Unity editor revision " + expected + "; project declares " + actual + ".");
        }

        private static void ValidateLegalRegistry()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string registry = Path.Combine(projectRoot, "Assets/ERO/Legal/ERO_Asset_License_Registry.md");
            if (!File.Exists(registry))
                throw new FileNotFoundException("ERO legal asset registry is missing.", registry);
        }

        private static void ValidateScenes()
        {
            string[] scenes = AssetDatabase.FindAssets("t:Scene");
            if (scenes == null || scenes.Length == 0)
                throw new InvalidOperationException("ERO project contains no Unity scenes.");

            foreach (string guid in scenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    Debug.Log("[ERO CI] Scene validated: " + path);
            }

            ValidatePlayableScene();
        }

        private static void ValidatePlayableScene()
        {
            if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, PlayableScene)))
                throw new FileNotFoundException("ERO playable scene is missing.", PlayableScene);
        }
    }
}
