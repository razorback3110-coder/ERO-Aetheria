#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace EternalRealmsOnline.V7.Editor
{
    public static class EROV7BuildAndQA
    {
        [MenuItem("ERO/V7/Build Linux Dedicated Server")]
        public static void BuildServer()
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { SceneManager.GetActiveScene().path },
                locationPathName = "Builds/ERO_V7_Server/ERO_Server.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[ERO V7] Dedicated Server build result: " + report.summary.result);
        }

        [MenuItem("ERO/V7/Validate Network Manager")]
        public static void ValidateNetworkManager()
        {
            if (NetworkManager.Singleton == null)
                Debug.LogWarning("[ERO V7] No NetworkManager in the active scene.");
            else
                Debug.Log("[ERO V7] NetworkManager detected: " + NetworkManager.Singleton.name);
        }
    }
}
#endif
