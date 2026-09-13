#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace EternalRealmsOnline.V6.Editor
{
    public static class EROV6DedicatedServerBuild
    {
        [MenuItem("ERO/V6/Build Linux Dedicated Server")]
        public static void BuildLinuxServer()
        {
            Directory.CreateDirectory("Builds/Server");
            var scenes = EditorBuildSettings.scenes;
            var opts = new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(scenes, s => s.path),
                locationPathName = "Builds/Server/ERO-WorldServer.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("ERO V6 dedicated server build failed.");
            Debug.Log("ERO V6 dedicated server build complete.");
        }
    }
}
#endif
