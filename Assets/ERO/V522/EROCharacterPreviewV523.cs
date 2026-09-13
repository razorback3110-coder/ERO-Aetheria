using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EternalRealmsOnline.V523
{
    /// <summary>Presentation-only styling for ERO's own Character Select models.</summary>
    public static class EROCharacterPreviewV523
    {
        static readonly Color Skin = new Color(0.78f, 0.56f, 0.45f, 1f);
        static readonly Color Dark = new Color(0.055f, 0.06f, 0.085f, 1f);
        static readonly Color Metal = new Color(0.58f, 0.64f, 0.72f, 1f);
        static readonly Color[] Hair =
        {
            new Color(0.12f,0.07f,0.045f,1f),
            new Color(0.42f,0.20f,0.07f,1f),
            new Color(0.08f,0.16f,0.23f,1f)
        };

        public static void Prepare(GameObject root, int classIndex, bool male, int hairIndex)
        {
            if (root == null) return;
            classIndex = Mathf.Clamp(classIndex, 0, 7);
            hairIndex = Mathf.Clamp(hairIndex, 0, 2);
            var accent = Accent(classIndex);

            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.Destroy(collider);

            var cache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var source = renderer.sharedMaterials;
                if (source == null || source.Length == 0)
                {
                    renderer.sharedMaterial = GetMaterial(cache, "Cloth", accent, classIndex, hairIndex);
                    continue;
                }

                var result = new Material[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    string n = source[i] == null ? "Cloth" : source[i].name;
                    result[i] = GetMaterial(cache, n, accent, classIndex, hairIndex);
                }
                renderer.sharedMaterials = result;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            // Keep the model visually grounded and consistently framed.
            var bounds = CalculateBounds(root);
            if (bounds.size.y > 0.01f)
            {
                float targetHeight = 2.35f;
                float scale = targetHeight / bounds.size.y;
                root.transform.localScale *= scale;
                bounds = CalculateBounds(root);
            }
            root.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        }

        static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        static Material GetMaterial(Dictionary<string, Material> cache, string sourceName, Color accent, int classIndex, int hairIndex)
        {
            string key = sourceName + "|" + classIndex + "|" + hairIndex;
            if (cache.TryGetValue(key, out var existing)) return existing;
            string lower = sourceName.ToLowerInvariant();
            Color color = lower.Contains("skin") ? Skin :
                          lower.Contains("hair") ? Hair[hairIndex] :
                          lower.Contains("metal") ? Metal :
                          lower.Contains("dark") ? Dark : accent;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            var mat = new Material(shader) { name = "ERO_V523_" + sourceName };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", lower.Contains("metal") ? 0.55f : 0.05f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", lower.Contains("metal") ? 0.7f : 0.35f);
            cache[key] = mat;
            return mat;
        }

        public static Color Accent(int cls)
        {
            switch (cls)
            {
                case 0: return new Color(.30f,.55f,1f,1f);
                case 1: return new Color(.72f,.32f,1f,1f);
                case 2: return new Color(.25f,.78f,.42f,1f);
                case 3: return new Color(.55f,.32f,1f,1f);
                case 4: return new Color(1f,.72f,.22f,1f);
                case 5: return new Color(1f,.38f,.25f,1f);
                case 6: return new Color(.78f,.25f,.90f,1f);
                default: return new Color(1f,.62f,.25f,1f);
            }
        }
    }
}
