using UnityEngine;
using Unity.Netcode;

namespace EternalRealmsOnline.V8
{
    /// <summary>
    /// Dedicated-server bootstrap for ERO builds launched with -ero-server.
    /// Keeps server startup deterministic and avoids client-only presentation work.
    /// </summary>
    public sealed class EROHeadlessServerBootstrap : MonoBehaviour
    {
        private static bool created;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (created || !HasServerArgument()) return;
            created = true;
            var go = new GameObject("ERO_HeadlessServerBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<EROHeadlessServerBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].enabled = false;

            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (var i = 0; i < cameras.Length; i++)
                cameras[i].enabled = false;

            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[ERO Server] NetworkManager.Singleton was not found; dedicated server cannot start.");
                return;
            }

            if (!networkManager.IsServer && !networkManager.IsClient && !networkManager.IsListening)
            {
                if (!networkManager.StartServer())
                    Debug.LogError("[ERO Server] NetworkManager failed to start the dedicated server.");
                else
                    Debug.Log("[ERO Server] Dedicated server started.");
            }
        }

        private static bool HasServerArgument()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-ero-server", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
