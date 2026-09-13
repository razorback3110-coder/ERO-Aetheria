using UnityEngine;

namespace EternalRealmsOnline.V539
{
    [DefaultExecutionOrder(100)]
    public sealed class EROWorldPresentationV539 : MonoBehaviour
    {
        [SerializeField] Color fogColor = new Color(0.025f, 0.035f, 0.075f, 1f);
        [SerializeField] float fogDensity = 0.006f;
        [SerializeField] float sunIntensity = 1.1f;
        Light sun;

        void Awake()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.20f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.10f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.02f, 0.03f);
            sun = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0
                ? FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0] : null;
            if (sun == null)
            {
                var go = new GameObject("ERO_Sun_V539");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
                go.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            }
            sun.type = LightType.Directional;
            sun.intensity = sunIntensity;
            sun.shadows = LightShadows.Soft;
        }
    }
}
