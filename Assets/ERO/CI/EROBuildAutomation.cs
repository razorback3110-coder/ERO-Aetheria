#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EternalRealmsOnline.CI
{
    public static class EROBuildAutomation
    {
        private const string DefaultScene = "Assets/Scenes/ERO/ERO_Playable.unity";
        private const string VerticalSliceScript = "Assets/ERO/World/EROPlayableVerticalSlice.cs";

        [MenuItem("ERO/CI/Validate Compile")]
        public static void ValidateCompile()
        {
            ValidateProject();
            Debug.Log("[ERO CI] Unity editor loaded and EROBuildAutomation compiled successfully.");
            EditorApplication.Exit(0);
        }

        [MenuItem("ERO/CI/Build Windows")]
        public static void BuildWindows()
        {
            ValidateProject();
            var report = BuildPipeline.BuildPlayer(CreateOptions(BuildTarget.StandaloneWindows64, "Builds/Windows/ERO.exe", StandaloneBuildSubtarget.Player));
            EnsureSucceeded(report, "Windows");
            EditorApplication.Exit(0);
        }

        [MenuItem("ERO/CI/Build Linux Dedicated Server")]
        public static void BuildLinuxDedicatedServer()
        {
            ValidateProject();
            var report = BuildPipeline.BuildPlayer(CreateOptions(BuildTarget.StandaloneLinux64, "Builds/LinuxServer/ERO-WorldServer.x86_64", StandaloneBuildSubtarget.Server));
            EnsureSucceeded(report, "Linux Dedicated Server");
            EditorApplication.Exit(0);
        }

        private static BuildPlayerOptions CreateOptions(BuildTarget target, string output, StandaloneBuildSubtarget subtarget)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            return new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = output,
                target = target,
                subtarget = (int)subtarget,
                options = BuildOptions.None
            };
        }

        private static string[] GetScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes != null && scenes.Length > 0)
            {
                var enabled = Array.FindAll(scenes, s => s != null && s.enabled && !string.IsNullOrWhiteSpace(s.path));
                if (enabled.Length > 0) return Array.ConvertAll(enabled, s => s.path);
            }
            if (File.Exists(DefaultScene)) return new[] { DefaultScene };
            throw new BuildFailedException("No enabled build scenes found and ERO_Playable.unity is missing.");
        }

        private static void ValidateProject()
        {
            const string expectedVersion = "6000.0.67f1";
            var versionFile = "ProjectSettings/ProjectVersion.txt";
            if (!File.Exists(versionFile)) throw new BuildFailedException("Missing ProjectSettings/ProjectVersion.txt");
            if (!File.ReadAllText(versionFile).Contains(expectedVersion)) throw new BuildFailedException("ERO requires Unity " + expectedVersion + ".");
            if (!Directory.Exists("Assets") || !Directory.Exists("Packages") || !Directory.Exists("ProjectSettings")) throw new BuildFailedException("ERO Unity project folders are incomplete.");
            if (!File.Exists(DefaultScene)) throw new BuildFailedException("Missing required playable scene: " + DefaultScene);
            if (!File.Exists(VerticalSliceScript)) throw new BuildFailedException("Missing playable vertical slice script: " + VerticalSliceScript);
            var sceneText = File.ReadAllText(DefaultScene);
            if (!sceneText.Contains("7d2f4f1a2e3b4c5d9e8f7a6b5c4d3e2f")) throw new BuildFailedException("Playable scene is not wired to EROPlayableVerticalSlice.");
        }

        private static void EnsureSucceeded(BuildReport report, string target)
        {
            if (report == null || report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("ERO " + target + " build failed. Check the Unity build log.");
            Debug.Log($"[ERO CI] {target} build succeeded: {report.summary.totalSize / (1024f * 1024f):F1} MB");
        }
    }
}
#endif
