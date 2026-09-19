using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Installs the dependency-free playable HUD only in the ERO_Playable scene.
    /// Production UI can replace this bootstrap without changing gameplay code.
    /// </summary>
    public static class EROPlayableHUDBootstrap
    {
        private const string RootName = "ERO_Playable_HUD";
        private static bool installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (installed || SceneManager.GetActiveScene().name != "ERO_Playable") return;
            installed = true;
            var root = new GameObject(RootName);
            root.AddComponent<EROPlayableHUD>();
        }
    }
}
