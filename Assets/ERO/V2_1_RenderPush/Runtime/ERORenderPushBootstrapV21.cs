using UnityEngine;

namespace EternalRealmsOnline.V21
{
    /// <summary>
    /// Installs the render push on the first active camera without requiring
    /// a manual scene edit. It intentionally does not replace gameplay/network systems.
    /// </summary>
    public static class ERORenderPushBootstrapV21
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                foreach (var c in Object.FindObjectsByType<Camera>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (c.isActiveAndEnabled) { cam = c; break; }
                }
            }

            if (cam == null) return;
            if (cam.GetComponent<ERORenderPushV21>() == null)
                cam.gameObject.AddComponent<ERORenderPushV21>();
        }
    }
}
