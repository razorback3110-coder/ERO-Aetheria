using UnityEngine;
using UnityEngine.SceneManagement;

namespace ERO.Art
{
    /// <summary>
    /// Lightweight visual-quality bootstrap for the playable build.
    /// Uses Unity/ERO runtime settings only; no third-party assets are introduced.
    /// </summary>
    public sealed class EROVisualQualityBootstrap : MonoBehaviour
    {
        private const string RootName = "ERO_VisualQuality";
        private static bool installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (installed) return;
            installed = true;
            var root = new GameObject(RootName);
            DontDestroyOnLoad(root);
            root.AddComponent<EROVisualQualityBootstrap>();
        }

        private void Awake()
        {
            ApplyEnvironment();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyEnvironment();
        }

        private static void ApplyEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 280f;
            RenderSettings.fogColor = new Color(0.38f, 0.50f, 0.62f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.42f, 0.56f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.27f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.075f, 0.065f, 0.055f);
            RenderSettings.reflectionIntensity = 0.8f;

            EnsureProceduralSky();
            EnsureSun();
            ConfigureCameras();
            DynamicGI.UpdateEnvironment();
        }

        private static void EnsureProceduralSky()
        {
            if (RenderSettings.skybox != null) return;
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null) return;

            var material = new Material(shader)
            {
                name = "ERO_Runtime_ProceduralSky"
            };
            if (material.HasProperty("_SunSize")) material.SetFloat("_SunSize", 0.035f);
            if (material.HasProperty("_SunSizeConvergence")) material.SetFloat("_SunSizeConvergence", 5f);
            if (material.HasProperty("_AtmosphereThickness")) material.SetFloat("_AtmosphereThickness", 1.05f);
            if (material.HasProperty("_SkyTint")) material.SetColor("_SkyTint", new Color(0.34f, 0.48f, 0.68f));
            if (material.HasProperty("_GroundColor")) material.SetColor("_GroundColor", new Color(0.16f, 0.13f, 0.10f));
            RenderSettings.skybox = material;
        }

        private static void EnsureSun()
        {
            Light sun = null;
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    sun = light;
                    break;
                }
            }

            if (sun == null)
            {
                var go = new GameObject("ERO_Sun");
                go.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.intensity = Mathf.Clamp(sun.intensity <= 0f ? 1.15f : sun.intensity, 0.75f, 1.35f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.72f;
            sun.shadowBias = 0.05f;
        }

        private static void ConfigureCameras()
        {
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                camera.farClipPlane = Mathf.Max(camera.farClipPlane, 300f);
                camera.allowHDR = true;
            }
        }
    }
}
