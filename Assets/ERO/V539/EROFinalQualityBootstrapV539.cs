using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.V539
{
    [DefaultExecutionOrder(-1000)]
    public sealed class EROFinalQualityBootstrapV539 : MonoBehaviour
    {
        static bool booted;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (booted) return;
            booted = true;
            var go = new GameObject("ERO_FinalQuality_V539");
            DontDestroyOnLoad(go);
            go.AddComponent<EROFinalQualityBootstrapV539>();
        }

        void Awake()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyQuality();
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        static void ApplyQuality()
        {
            try
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.billboardsFaceCameraPosition = true;
            }
            catch { }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var camera in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                camera.allowHDR = true;
                camera.allowMSAA = true;
                camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.05f);
                camera.farClipPlane = Mathf.Max(camera.farClipPlane, 500f);
            }
        }
    }
}
