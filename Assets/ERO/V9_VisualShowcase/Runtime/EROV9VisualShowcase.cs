using UnityEngine;

namespace ERO.V9
{
    /// <summary>
    /// Lightweight visual dressing pass for the playable slice.
    /// Creates an original, license-safe presentation layer at runtime using Unity primitives.
    /// Production meshes/materials can replace these anchors without changing gameplay code.
    /// </summary>
    public sealed class EROV9VisualShowcase : MonoBehaviour
    {
        [SerializeField] private int treeCount = 36;
        [SerializeField] private int crystalCount = 14;
        [SerializeField] private int ruinCount = 8;
        [SerializeField] private float worldRadius = 42f;
        [SerializeField] private int seed = 90210;

        private Transform root;

        private void Start()
        {
            BuildPresentation();
        }

        public void BuildPresentation()
        {
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("ERO_V9_VisualPresentation").transform;
            root.SetParent(transform, false);

            Random.InitState(seed);
            CreateGround();
            CreatePlaza();
            ScatterTrees();
            ScatterCrystals();
            ScatterRuins();
            CreateLighting();
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Aetheria_Ground";
            ground.transform.SetParent(root, false);
            ground.transform.localScale = new Vector3(worldRadius * 0.055f, 0.12f, worldRadius * 0.055f);
            ground.GetComponent<Renderer>().material = MakeMaterial(new Color(0.055f, 0.07f, 0.09f));
        }

        private void CreatePlaza()
        {
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Aetheria_Portal_Plaza";
            plaza.transform.SetParent(root, false);
            plaza.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            plaza.transform.localScale = new Vector3(0.24f, 0.035f, 0.24f);
            plaza.GetComponent<Renderer>().material = MakeMaterial(new Color(0.11f, 0.13f, 0.16f));

            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                CreatePillar(new Vector3(Mathf.Cos(a) * 7f, 1.6f, Mathf.Sin(a) * 7f));
            }
        }

        private void CreatePillar(Vector3 position)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Aetheria_Obelisk";
            pillar.transform.SetParent(root, false);
            pillar.transform.localPosition = position;
            pillar.transform.localScale = new Vector3(0.8f, 3.2f, 0.8f);
            pillar.GetComponent<Renderer>().material = MakeMaterial(new Color(0.16f, 0.17f, 0.2f));
        }

        private void ScatterTrees()
        {
            for (int i = 0; i < treeCount; i++)
            {
                Vector2 p = Random.insideUnitCircle * worldRadius;
                if (p.magnitude < 10f) continue;
                CreateTree(new Vector3(p.x, 0f, p.y));
            }
        }

        private void CreateTree(Vector3 position)
        {
            var tree = new GameObject("Aetheria_Tree").transform;
            tree.SetParent(root, false);
            tree.localPosition = position;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree, false);
            trunk.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            trunk.transform.localScale = new Vector3(0.25f, 1.4f, 0.25f);
            trunk.GetComponent<Renderer>().material = MakeMaterial(new Color(0.16f, 0.1f, 0.07f));

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.transform.SetParent(tree, false);
            crown.transform.localPosition = new Vector3(0f, 3.1f, 0f);
            crown.transform.localScale = new Vector3(2.2f, 2.8f, 2.2f);
            crown.GetComponent<Renderer>().material = MakeMaterial(new Color(0.04f, 0.14f, 0.13f));
        }

        private void ScatterCrystals()
        {
            for (int i = 0; i < crystalCount; i++)
            {
                Vector2 p = Random.insideUnitCircle * (worldRadius * 0.85f);
                CreateCrystal(new Vector3(p.x, 0.7f, p.y));
            }
        }

        private void CreateCrystal(Vector3 position)
        {
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            crystal.name = "Aetheria_Aether_Crystal";
            crystal.transform.SetParent(root, false);
            crystal.transform.localPosition = position;
            crystal.transform.localRotation = Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
            crystal.transform.localScale = new Vector3(0.35f, Random.Range(1.2f, 2.4f), 0.35f);
            var material = MakeMaterial(new Color(0.08f, 0.45f, 0.55f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.02f, 0.35f, 0.55f) * 2f);
            crystal.GetComponent<Renderer>().material = material;
        }

        private void ScatterRuins()
        {
            for (int i = 0; i < ruinCount; i++)
            {
                Vector2 p = Random.insideUnitCircle * (worldRadius * 0.8f);
                if (p.magnitude < 14f) continue;
                var ruin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ruin.name = "Aetheria_Ruin_Block";
                ruin.transform.SetParent(root, false);
                ruin.transform.localPosition = new Vector3(p.x, Random.Range(0.5f, 1.5f), p.y);
                ruin.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
                ruin.transform.localScale = new Vector3(Random.Range(1f, 2.5f), Random.Range(1f, 3f), Random.Range(0.8f, 2f));
                ruin.GetComponent<Renderer>().material = MakeMaterial(new Color(0.19f, 0.19f, 0.21f));
            }
        }

        private void CreateLighting()
        {
            var lightObject = new GameObject("Aetheria_MoonLight");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.7f;
            light.color = new Color(0.55f, 0.68f, 0.95f);
            light.shadows = LightShadows.Soft;
        }

        private static Material MakeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "ERO_V9_RuntimeMaterial" };
            material.color = color;
            return material;
        }
    }
}
