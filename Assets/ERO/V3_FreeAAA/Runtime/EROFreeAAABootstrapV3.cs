using UnityEngine;

namespace EternalRealmsOnline.V3
{
    [DefaultExecutionOrder(-850)]
    public static class EROFreeAAABootstrapV3
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Object.FindFirstObjectByType<EROFreeAAAPresentationV3>() != null) return;
            var go = new GameObject("ERO_V3_FreeAAA_Presentation");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EROFreeAAAPresentationV3>();
        }
    }

    public sealed class EROFreeAAAPresentationV3 : MonoBehaviour
    {
        void Awake()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.009f;
            RenderSettings.reflectionIntensity = Mathf.Max(RenderSettings.reflectionIntensity, 0.65f);
        }
    }
}
