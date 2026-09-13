using UnityEngine;

namespace EternalRealmsOnline.V4
{
    /// <summary>
    /// Fast vertical-slice presentation bootstrap. Keeps existing ERO gameplay/network
    /// systems intact and adds a small, testable presentation layer.
    /// </summary>
    [DefaultExecutionOrder(-700)]
    public sealed class EROV4VerticalSliceBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Object.FindFirstObjectByType<EROV4VerticalSliceBootstrap>() != null) return;
            var go = new GameObject("ERO_V4_VerticalSlice");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EROV4VerticalSliceBootstrap>();
        }

        void Awake()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
            QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias, 1.1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.009f;
        }
    }
}
