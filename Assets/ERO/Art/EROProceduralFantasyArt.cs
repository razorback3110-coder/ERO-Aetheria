using UnityEngine;

namespace ERO.Art
{
    /// <summary>
    /// Runtime-safe procedural art foundation for ERO. These meshes are authored by code,
    /// so the playable MMORPG can have a coherent visual language without depending on
    /// placeholder Unity primitives or unlicensed third-party assets.
    /// </summary>
    public static class EROProceduralFantasyArt
    {
        private static Shader LitShader => Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        public static GameObject CreateGround(Transform parent, float size, int seed)
        {
            var go = new GameObject("Aetheria_Ground");
            go.transform.SetParent(parent, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var mc = go.AddComponent<MeshCollider>();

            int n = 12;
            var vertices = new Vector3[(n + 1) * (n + 1)];
            var triangles = new int[n * n * 6];
            var colors = new Color[vertices.Length];
            var rng = new System.Random(seed);
            float half = size * 0.5f;
            for (int z = 0; z <= n; z++)
            for (int x = 0; x <= n; x++)
            {
                int i = z * (n + 1) + x;
                float px = (x / (float)n) * size - half;
                float pz = (z / (float)n) * size - half;
                float wave = Mathf.Sin((px + seed) * 0.055f) * 0.18f + Mathf.Cos((pz - seed) * 0.045f) * 0.14f;
                vertices[i] = new Vector3(px, wave, pz);
                float variation = 0.82f + (float)rng.NextDouble() * 0.18f;
                colors[i] = new Color(0.17f * variation, 0.31f * variation, 0.17f * variation, 1f);
            }
            int t = 0;
            for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
            {
                int a = z * (n + 1) + x;
                int b = a + 1;
                int c = a + n + 1;
                int d = c + 1;
                triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
            }
            var mesh = new Mesh { name = "Aetheria_TerrainPatch" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            mr.sharedMaterial = MakeMaterial(new Color(0.16f, 0.30f, 0.16f));
            return go;
        }

        public static GameObject CreateTree(Transform parent, Vector3 localPosition, float scale, int seed)
        {
            var root = new GameObject("Aetheria_Tree");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            root.transform.localScale = Vector3.one * scale;
            CreateCone(root.transform, "Trunk", 0.42f, 2.2f, new Vector3(0f, 1.1f, 0f), new Color(0.24f, 0.12f, 0.055f), 8);
            CreateCone(root.transform, "CanopyLower", 1.55f, 2.4f, new Vector3(0f, 2.8f, 0f), new Color(0.075f, 0.25f, 0.12f), 7);
            CreateCone(root.transform, "CanopyUpper", 1.15f, 2.2f, new Vector3(0f, 4.15f, 0f), new Color(0.10f, 0.34f, 0.15f), 7);
            if ((seed & 1) == 0)
                CreateOrb(root.transform, "SpiritLight", new Vector3(0.25f, 4.65f, 0.15f), 0.18f, new Color(0.42f, 0.75f, 1f));
            return root;
        }

        public static GameObject CreateCrystal(Transform parent, Vector3 localPosition, float scale, int seed)
        {
            var root = new GameObject("Aetheria_Crystal");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            root.transform.localScale = Vector3.one * scale;
            float rotation = (seed % 360 + 360) % 360;
            root.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            CreateCone(root.transform, "Crystal", 0.45f, 2.3f, Vector3.up * 1.15f, new Color(0.28f, 0.55f, 0.95f), 6);
            CreateCone(root.transform, "CrystalCore", 0.22f, 1.5f, Vector3.up * 1.85f, new Color(0.52f, 0.82f, 1f), 6);
            return root;
        }

        public static GameObject CreateEnemy(Transform parent, Vector3 localPosition)
        {
            var root = new GameObject("Aetheria_Creature");
            root.transform.SetParent(parent, false);
            root.transform.position = localPosition;
            CreateCone(root.transform, "Body", 1.0f, 1.8f, new Vector3(0f, 1.0f, 0f), new Color(0.36f, 0.055f, 0.08f), 8);
            CreateOrb(root.transform, "Core", new Vector3(0f, 1.35f, 0.8f), 0.28f, new Color(0.95f, 0.28f, 0.18f));
            CreateOrb(root.transform, "EyeL", new Vector3(-0.28f, 1.7f, 0.55f), 0.10f, new Color(1f, 0.72f, 0.28f));
            CreateOrb(root.transform, "EyeR", new Vector3(0.28f, 1.7f, 0.55f), 0.10f, new Color(1f, 0.72f, 0.28f));
            return root;
        }

        private static void CreateCone(Transform parent, string name, float radius, float height, Vector3 position, Color color, int sides)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var mesh = new Mesh { name = name + "Mesh" };
            var v = new Vector3[sides * 2];
            var tr = new int[(sides - 1) * 6 + (sides - 2) * 3 * 2];
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                v[i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                v[sides + i] = new Vector3(Mathf.Cos(a) * radius * 0.78f, height, Mathf.Sin(a) * radius * 0.78f);
            }
            int p = 0;
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                tr[p++] = i; tr[p++] = sides + i; tr[p++] = j;
                tr[p++] = j; tr[p++] = sides + i; tr[p++] = sides + j;
            }
            for (int i = 1; i < sides - 1; i++) { tr[p++] = 0; tr[p++] = i + 1; tr[p++] = i; }
            for (int i = 1; i < sides - 1; i++) { tr[p++] = sides; tr[p++] = sides + i; tr[p++] = sides + i + 1; }
            mesh.vertices = v; mesh.triangles = tr; mesh.RecalculateNormals();
            mf.sharedMesh = mesh; mr.sharedMaterial = MakeMaterial(color);
        }

        private static void CreateOrb(Transform parent, string name, Vector3 position, float radius, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * radius;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color, true);
        }

        private static Material MakeMaterial(Color color, bool emission = false)
        {
            var mat = new Material(LitShader) { color = color };
            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2.2f);
            }
            return mat;
        }
    }
}
