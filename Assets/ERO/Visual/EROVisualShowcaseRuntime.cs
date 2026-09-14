using UnityEngine;

namespace ERO.Visual
{
    /// <summary>
    /// Lightweight runtime presentation layer for the playable showcase.
    /// It intentionally uses Unity-authored primitives so no unlicensed external asset is required.
    /// Production meshes/materials can replace the generated children later.
    /// </summary>
    public sealed class EROVisualShowcaseRuntime : MonoBehaviour
    {
        [SerializeField] private bool createAtmosphere = true;
        [SerializeField] private bool createAetherLights = true;
        [SerializeField] private int crystalCount = 12;
        [SerializeField] private float showcaseRadius = 28f;

        private void Awake()
        {
            if (createAtmosphere)
                BuildAtmosphere();
            if (createAetherLights)
                BuildAetherLights();
        }

        private void BuildAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.ambientIntensity = 0.55f;

            GameObject key = new GameObject("ERO_Aetheria_KeyLight");
            key.transform.SetParent(transform, false);
            Light light = key.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            key.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        }

        private void BuildAetherLights()
        {
            int count = Mathf.Clamp(crystalCount, 4, 32);
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                float radius = showcaseRadius * (0.55f + 0.45f * Mathf.Sin(i * 1.73f) * 0.5f + 0.2f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 1.5f, Mathf.Sin(angle) * radius);

                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.name = $"Aether_Crystal_{i:00}";
                crystal.transform.SetParent(transform, false);
                crystal.transform.position = position;
                crystal.transform.localScale = new Vector3(0.35f, 1.6f + (i % 3) * 0.35f, 0.35f);
                crystal.transform.rotation = Quaternion.Euler(0f, i * 37f, 18f);

                Renderer renderer = crystal.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.SetColor("_BaseColor", new Color(0.12f, 0.38f, 1f, 1f));
                    material.SetColor("_EmissionColor", new Color(0.08f, 0.32f, 1f, 1f) * 2.5f);
                    material.EnableKeyword("_EMISSION");
                    renderer.sharedMaterial = material;
                }

                GameObject glow = new GameObject("AetherGlow");
                glow.transform.SetParent(crystal.transform, false);
                Light point = glow.AddComponent<Light>();
                point.type = LightType.Point;
                point.range = 7f;
                point.intensity = 2.2f;
            }
        }
    }
}
