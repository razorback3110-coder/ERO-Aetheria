using System.Collections.Generic;
using UnityEngine;

namespace ERO.Presentation
{
    /// <summary>
    /// Runtime visual pass for the ERO vertical slice. It creates a cohesive original
    /// dark-fantasy presentation from procedural meshes/materials so the project has a
    /// real 3D scene even before the final licensed art packs are imported.
    /// No external/proprietary game assets are referenced.
    /// </summary>
    public sealed class EROVirtualArtDirector : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<EROVirtualArtDirector>() != null) return;
            var root = new GameObject("ERO_VisualSlice");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<EROVirtualArtDirector>();
        }

        private readonly List<Material> materials = new List<Material>();
        private Transform world;

        private void Start()
        {
            BuildPresentation();
        }

        private void BuildPresentation()
        {
            world = new GameObject("Aetheria_ArtPass").transform;
            world.SetParent(transform, false);

            CreateLighting();
            CreateGround();
            CreateVillage();
            CreateForest();
            CreateAetherCrystals();
            CreateHero();
            CreateEnemies();
            CreateLandmarks();
        }

        private Material Mat(string name, Color color, float metallic = 0f, float smooth = 0.35f, bool emission = false)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader) { name = "ERO_" + name };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (emission && m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 2.5f);
            }
            materials.Add(m);
            return m;
        }

        private void CreateLighting()
        {
            var sunGo = new GameObject("Aetheria_Sun");
            sunGo.transform.SetParent(world, false);
            sunGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            var moonGo = new GameObject("Aetheria_Fill");
            moonGo.transform.SetParent(world, false);
            var fill = moonGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.18f;
            fill.color = new Color(0.35f, 0.5f, 0.9f);
            fill.transform.rotation = Quaternion.Euler(125f, 35f, 0f);

            RenderSettings.ambientIntensity = 0.7f;
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.035f, 0.055f, 0.09f);
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Aetheria_Ground";
            ground.transform.SetParent(world, false);
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = Mat("Ground", new Color(0.075f, 0.09f, 0.105f), 0f, 0.8f);

            for (int i = 0; i < 14; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock_" + i;
                rock.transform.SetParent(world, false);
                float a = i * 2.37f;
                float r = 18f + (i % 5) * 7f;
                rock.transform.position = new Vector3(Mathf.Cos(a) * r, 0.35f, Mathf.Sin(a) * r);
                rock.transform.localScale = new Vector3(1.5f + i % 3, 0.7f + i % 2, 1.2f + (i % 4) * 0.25f);
                rock.GetComponent<Renderer>().sharedMaterial = Mat("Stone", new Color(0.16f, 0.17f, 0.19f), 0.05f, 0.25f);
            }
        }

        private void CreateVillage()
        {
            var stone = Mat("VillageStone", new Color(0.22f, 0.23f, 0.25f), 0.05f, 0.35f);
            var wood = Mat("DarkWood", new Color(0.16f, 0.09f, 0.055f), 0f, 0.25f);
            var roof = Mat("Roof", new Color(0.055f, 0.065f, 0.09f), 0.15f, 0.55f);
            var gold = Mat("RoyalGold", new Color(0.65f, 0.43f, 0.12f), 0.7f, 0.65f);
            var glow = Mat("AetherGlow", new Color(0.05f, 0.45f, 1f), 0.1f, 0.7f, true);

            for (int b = 0; b < 6; b++)
            {
                float x = -16f + (b % 3) * 16f;
                float z = 8f + (b / 3) * 10f;
                var baseGo = Cube("House_" + b, new Vector3(x, 2.2f, z), new Vector3(8f, 4.4f, 7f), stone);
                Cube("Door_" + b, new Vector3(x, 1.25f, z - 3.56f), new Vector3(1.4f, 2.5f, 0.18f), wood);
                for (int w = -1; w <= 1; w += 2)
                    Cube("Window_" + b + "_" + w, new Vector3(x + w * 2.2f, 2.3f, z - 3.62f), new Vector3(1.3f, 1.15f, 0.12f), glow);
                var roofGo = Cube("Roof_" + b, new Vector3(x, 5f, z), new Vector3(8.8f, 0.45f, 7.8f), roof);
                roofGo.transform.Rotate(0f, 45f, 0f);
                Cube("Trim_" + b, new Vector3(x, 4.1f, z - 3.7f), new Vector3(7.8f, 0.25f, 0.25f), gold);
            }

            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Aetheria_Plaza";
            plaza.transform.SetParent(world, false);
            plaza.transform.position = new Vector3(0f, 0.12f, 0f);
            plaza.transform.localScale = new Vector3(11f, 0.25f, 11f);
            plaza.GetComponent<Renderer>().sharedMaterial = stone;
            var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = "AetherFountain";
            fountain.transform.SetParent(world, false);
            fountain.transform.position = new Vector3(0f, 1.1f, 0f);
            fountain.transform.localScale = new Vector3(3.5f, 1.1f, 3.5f);
            fountain.GetComponent<Renderer>().sharedMaterial = gold;
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "AetherCore";
            core.transform.SetParent(world, false);
            core.transform.position = new Vector3(0f, 3.2f, 0f);
            core.transform.localScale = Vector3.one * 1.8f;
            core.GetComponent<Renderer>().sharedMaterial = glow;
        }

        private void CreateForest()
        {
            var trunk = Mat("TreeTrunk", new Color(0.12f, 0.075f, 0.04f), 0f, 0.2f);
            var leaves = Mat("NightLeaves", new Color(0.045f, 0.16f, 0.12f), 0f, 0.3f);
            for (int i = 0; i < 34; i++)
            {
                float a = i * 1.618f;
                float r = 24f + (i % 6) * 3f;
                float x = Mathf.Cos(a) * r;
                float z = Mathf.Sin(a) * r;
                var t = Cylinder("TreeTrunk_" + i, new Vector3(x, 2.5f, z), new Vector3(0.55f, 2.5f, 0.55f), trunk);
                t.transform.Rotate(0f, (i * 19) % 25, 0f);
                for (int j = 0; j < 3; j++)
                {
                    var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "TreeCrown_" + i + "_" + j;
                    crown.transform.SetParent(world, false);
                    crown.transform.position = new Vector3(x, 4.2f + j * 1.35f, z);
                    crown.transform.localScale = Vector3.one * (3.1f - j * 0.45f);
                    crown.GetComponent<Renderer>().sharedMaterial = leaves;
                }
            }
        }

        private void CreateAetherCrystals()
        {
            var crystal = Mat("AetherCrystal", new Color(0.08f, 0.42f, 1f), 0.35f, 0.8f, true);
            for (int i = 0; i < 12; i++)
            {
                float a = i * 2.11f;
                float r = 10f + (i % 4) * 5f;
                var c = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                c.name = "AetherCrystal_" + i;
                c.transform.SetParent(world, false);
                c.transform.position = new Vector3(Mathf.Cos(a) * r, 1.4f, Mathf.Sin(a) * r);
                c.transform.localScale = new Vector3(0.45f + i % 2 * 0.2f, 1.5f + i % 3 * 0.5f, 0.45f + i % 2 * 0.2f);
                c.transform.Rotate(0f, i * 31f, i % 3 * 11f);
                c.GetComponent<Renderer>().sharedMaterial = crystal;
            }
        }

        private void CreateHero()
        {
            var armor = Mat("HeroArmor", new Color(0.055f, 0.075f, 0.11f), 0.65f, 0.7f);
            var cloth = Mat("HeroCloth", new Color(0.12f, 0.13f, 0.17f), 0f, 0.35f);
            var gold = Mat("HeroGold", new Color(0.72f, 0.52f, 0.16f), 0.75f, 0.7f);
            var aura = Mat("HeroAura", new Color(0.04f, 0.28f, 1f), 0.1f, 0.6f, true);
            var root = new GameObject("ERO_Hero_Visual");
            root.transform.SetParent(world, false);
            root.transform.position = new Vector3(0f, 0f, -2.5f);
            Capsule("HeroBody", new Vector3(0f, 1.25f, 0f), new Vector3(0.8f, 1.15f, 0.55f), armor, root.transform);
            Sphere("HeroHead", new Vector3(0f, 2.75f, 0f), Vector3.one * 0.58f, cloth, root.transform);
            Cube("HeroShoulderL", new Vector3(-0.82f, 2f, 0f), new Vector3(0.35f, 0.35f, 0.8f), gold, root.transform);
            Cube("HeroShoulderR", new Vector3(0.82f, 2f, 0f), new Vector3(0.35f, 0.35f, 0.8f), gold, root.transform);
            Cube("HeroBlade", new Vector3(1.05f, 1.65f, 0f), new Vector3(0.14f, 2.2f, 0.38f), gold, root.transform);
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Hero_AuraRing";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            ring.transform.localScale = new Vector3(2.4f, 0.035f, 2.4f);
            ring.GetComponent<Renderer>().sharedMaterial = aura;
        }

        private void CreateEnemies()
        {
            var hide = Mat("BeastHide", new Color(0.19f, 0.08f, 0.09f), 0.1f, 0.4f);
            var eye = Mat("BeastEyes", new Color(1f, 0.05f, 0.03f), 0.1f, 0.5f, true);
            for (int i = 0; i < 5; i++)
            {
                float x = -12f + i * 6f;
                float z = -12f + (i % 2) * 4f;
                var root = new GameObject("AetherBeast_" + i);
                root.transform.SetParent(world, false);
                root.transform.position = new Vector3(x, 0f, z);
                Capsule("Body", new Vector3(0f, 1.15f, 0f), new Vector3(1.1f, 1.15f, 0.8f), hide, root.transform);
                Sphere("Head", new Vector3(0f, 2.4f, 0.2f), Vector3.one * 0.8f, hide, root.transform);
                Sphere("EyeL", new Vector3(-0.3f, 2.55f, 0.85f), Vector3.one * 0.12f, eye, root.transform);
                Sphere("EyeR", new Vector3(0.3f, 2.55f, 0.85f), Vector3.one * 0.12f, eye, root.transform);
                Cube("ClawL", new Vector3(-1f, 0.75f, 0.55f), new Vector3(0.35f, 0.8f, 0.35f), hide, root.transform);
                Cube("ClawR", new Vector3(1f, 0.75f, 0.55f), new Vector3(0.35f, 0.8f, 0.35f), hide, root.transform);
            }
        }

        private void CreateLandmarks()
        {
            var stone = Mat("LandmarkStone", new Color(0.18f, 0.19f, 0.22f), 0.15f, 0.45f);
            var gold = Mat("LandmarkGold", new Color(0.7f, 0.48f, 0.12f), 0.8f, 0.75f);
            for (int i = 0; i < 4; i++)
            {
                float x = -27f + i * 18f;
                Cube("Obelisk_" + i, new Vector3(x, 4f, 27f), new Vector3(2f, 8f, 2f), stone);
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "ObeliskCap_" + i;
                cap.transform.SetParent(world, false);
                cap.transform.position = new Vector3(x, 8.3f, 27f);
                cap.transform.localScale = new Vector3(2.6f, 0.35f, 2.6f);
                cap.GetComponent<Renderer>().sharedMaterial = gold;
            }
        }

        private GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(parent ?? world, false);
            g.transform.position = parent == null ? pos : parent.TransformPoint(pos);
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }

        private GameObject Sphere(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = name;
            g.transform.SetParent(parent ?? world, false);
            g.transform.position = parent == null ? pos : parent.TransformPoint(pos);
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }

        private GameObject Capsule(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            g.name = name;
            g.transform.SetParent(parent ?? world, false);
            g.transform.position = parent == null ? pos : parent.TransformPoint(pos);
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }

        private GameObject Cylinder(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = name;
            g.transform.SetParent(world, false);
            g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }
    }
}
