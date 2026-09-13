using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.World
{
    /// <summary>
    /// Deterministic world generator foundation for Aetheria.
    /// It creates the complete zone graph procedurally and is deliberately asset-agnostic:
    /// approved Quaternius/Kenney/other licensed prefabs can be assigned later without changing
    /// the generation rules. The generated layout is stable from the world seed.
    /// </summary>
    public sealed class EROProceduralWorldGenerator : MonoBehaviour
    {
        [Serializable]
        public sealed class ZoneDefinition
        {
            public string id;
            public string displayName;
            public int minLevel;
            public int maxLevel;
            public int gridX;
            public int gridZ;
            public Color terrainTint;
            public float height;
            public float moisture;
            public float temperature;
            public int poiCount;
        }

        const string RootName = "ERO_Aetheria_ProceduralWorld";
        const string SceneName = "ERO_Playable";
        const int WorldSeed = 740067;
        const float ZoneSize = 512f;
        const int Resolution = 33;
        const float CellSize = ZoneSize / (Resolution - 1);

        static EROProceduralWorldGenerator instance;
        readonly List<GameObject> generatedZones = new List<GameObject>();
        Material terrainMaterial;
        Material roadMaterial;
        Material waterMaterial;
        Material markerMaterial;
        bool generated;

        static readonly ZoneDefinition[] Zones =
        {
            new ZoneDefinition { id="GREENHAVEN", displayName="Greenhaven", minLevel=1, maxLevel=10, gridX=0, gridZ=0, terrainTint=new Color(.20f,.38f,.19f), height=22f, moisture=.75f, temperature=.65f, poiCount=12 },
            new ZoneDefinition { id="EVERWOOD", displayName="Everwood", minLevel=10, maxLevel=20, gridX=1, gridZ=0, terrainTint=new Color(.10f,.30f,.18f), height=38f, moisture=.85f, temperature=.55f, poiCount=16 },
            new ZoneDefinition { id="ELYNDOR", displayName="Elyndor", minLevel=20, maxLevel=30, gridX=0, gridZ=1, terrainTint=new Color(.32f,.34f,.24f), height=30f, moisture=.55f, temperature=.60f, poiCount=14 },
            new ZoneDefinition { id="FROSTFALL", displayName="Frostfall", minLevel=30, maxLevel=40, gridX=1, gridZ=1, terrainTint=new Color(.55f,.64f,.68f), height=48f, moisture=.62f, temperature=.12f, poiCount=14 },
            new ZoneDefinition { id="SUNSCAR", displayName="Sunscar", minLevel=40, maxLevel=50, gridX=2, gridZ=0, terrainTint=new Color(.55f,.34f,.12f), height=34f, moisture=.18f, temperature=.92f, poiCount=15 },
            new ZoneDefinition { id="LUNARETH", displayName="Lunareth", minLevel=50, maxLevel=60, gridX=2, gridZ=1, terrainTint=new Color(.20f,.27f,.34f), height=55f, moisture=.70f, temperature=.38f, poiCount=18 },
            new ZoneDefinition { id="ABYSSIA", displayName="Abyssia", minLevel=60, maxLevel=70, gridX=3, gridZ=0, terrainTint=new Color(.16f,.08f,.18f), height=62f, moisture=.40f, temperature=.48f, poiCount=18 },
            new ZoneDefinition { id="ETERNAL_RIFT", displayName="The Eternal Rift", minLevel=70, maxLevel=100, gridX=3, gridZ=1, terrainTint=new Color(.12f,.15f,.28f), height=72f, moisture=.35f, temperature=.40f, poiCount=22 }
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject(RootName);
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EROProceduralWorldGenerator>();
        }

        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        void Start()
        {
            if (SceneManager.GetActiveScene().name.Equals(SceneName, StringComparison.OrdinalIgnoreCase))
                StartCoroutine(GenerateWhenReady());
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals(SceneName, StringComparison.OrdinalIgnoreCase))
                StartCoroutine(GenerateWhenReady());
        }

        IEnumerator GenerateWhenReady()
        {
            if (generated) yield break;
            for (int i = 0; i < 120; i++)
            {
                if (Camera.main != null) break;
                yield return null;
            }
            GenerateWorld();
        }

        public void GenerateWorld()
        {
            if (generated) return;
            generated = true;
            BuildMaterials();
            var world = new GameObject(RootName + "_Generated");
            world.transform.SetParent(transform, false);

            for (int i = 0; i < Zones.Length; i++)
                GenerateZone(world.transform, Zones[i], i);

            GenerateWorldSpine(world.transform);
            PositionMainCamera();
            Debug.Log("[ERO World] Aetheria generated: " + Zones.Length + " zones, deterministic seed " + WorldSeed + ".");
        }

        void BuildMaterials()
        {
            terrainMaterial = MakeMaterial("ERO Procedural Terrain", new Color(.20f,.25f,.30f));
            roadMaterial = MakeMaterial("ERO Roads", new Color(.20f,.16f,.12f));
            waterMaterial = MakeMaterial("ERO Water", new Color(.05f,.16f,.28f));
            markerMaterial = MakeMaterial("ERO POI", new Color(.75f,.58f,.18f));
        }

        Material MakeMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        void GenerateZone(Transform parent, ZoneDefinition zone, int index)
        {
            var go = new GameObject(zone.id);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(zone.gridX * ZoneSize, 0f, zone.gridZ * ZoneSize);
            generatedZones.Add(go);

            var meshGo = new GameObject("Terrain");
            meshGo.transform.SetParent(go.transform, false);
            var filter = meshGo.AddComponent<MeshFilter>();
            var renderer = meshGo.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildTerrainMesh(zone, index);
            renderer.sharedMaterial = terrainMaterial;

            var collider = meshGo.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;

            CreateZoneCenter(go.transform, zone);
            CreateRoads(go.transform, zone);
            CreatePOIs(go.transform, zone, index);
            CreateBoundary(go.transform, zone);
        }

        Mesh BuildTerrainMesh(ZoneDefinition zone, int index)
        {
            var mesh = new Mesh { name = zone.id + "_Terrain" };
            var vertices = new Vector3[Resolution * Resolution];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(Resolution - 1) * (Resolution - 1) * 6];
            int t = 0;
            float seedOffset = WorldSeed * .001f + index * 91.73f;

            for (int z = 0; z < Resolution; z++)
            for (int x = 0; x < Resolution; x++)
            {
                float nx = x / (float)(Resolution - 1);
                float nz = z / (float)(Resolution - 1);
                float globalX = zone.gridX + nx;
                float globalZ = zone.gridZ + nz;
                float n1 = Mathf.PerlinNoise(globalX * 2.15f + seedOffset, globalZ * 2.15f + seedOffset);
                float n2 = Mathf.PerlinNoise(globalX * 6.5f + seedOffset, globalZ * 6.5f + seedOffset);
                float ridge = 1f - Mathf.Abs(n1 * 2f - 1f);
                float h = zone.height * (n1 * .55f + n2 * .16f + ridge * .24f - .15f);
                h += Mathf.Sin((globalX + seedOffset) * 3.1f) * Mathf.Cos((globalZ + seedOffset) * 2.7f) * 2.2f;
                vertices[z * Resolution + x] = new Vector3(x * CellSize, Mathf.Max(-2f, h), z * CellSize);
                uv[z * Resolution + x] = new Vector2(nx, nz);
                if (x < Resolution - 1 && z < Resolution - 1)
                {
                    int a = z * Resolution + x, b = a + 1, c = a + Resolution, d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void CreateZoneCenter(Transform parent, ZoneDefinition zone)
        {
            var center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            center.name = "SettlementAnchor_" + zone.id;
            center.transform.SetParent(parent, false);
            center.transform.localPosition = new Vector3(ZoneSize * .5f, 2f, ZoneSize * .5f);
            center.transform.localScale = new Vector3(9f, .35f, 9f);
            center.GetComponent<Renderer>().sharedMaterial = markerMaterial;
        }

        void CreateRoads(Transform parent, ZoneDefinition zone)
        {
            float y = zone.height * .08f + 1.5f;
            CreateRoad(parent, new Vector3(ZoneSize*.5f, y, 0), new Vector3(ZoneSize*.5f, y, ZoneSize));
            CreateRoad(parent, new Vector3(0, y, ZoneSize*.5f), new Vector3(ZoneSize, y, ZoneSize*.5f));
        }

        void CreateRoad(Transform parent, Vector3 a, Vector3 b)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Road";
            go.transform.SetParent(parent, false);
            Vector3 delta = b - a;
            go.transform.localPosition = (a + b) * .5f;
            go.transform.localScale = new Vector3(Mathf.Max(5f, Mathf.Abs(delta.x)), .45f, Mathf.Max(5f, Mathf.Abs(delta.z)));
            go.GetComponent<Renderer>().sharedMaterial = roadMaterial;
        }

        void CreatePOIs(Transform parent, ZoneDefinition zone, int index)
        {
            var rng = new System.Random(WorldSeed + index * 1009);
            for (int i = 0; i < zone.poiCount; i++)
            {
                float x = 32f + (float)rng.NextDouble() * (ZoneSize - 64f);
                float z = 32f + (float)rng.NextDouble() * (ZoneSize - 64f);
                float size = 1.5f + (float)rng.NextDouble() * 3.5f;
                var poi = GameObject.CreatePrimitive(i % 5 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                poi.name = "POI_" + zone.id + "_" + i.ToString("00");
                poi.transform.SetParent(parent, false);
                poi.transform.localPosition = new Vector3(x, 3f + size, z);
                poi.transform.localScale = new Vector3(size, size * 1.8f, size);
                poi.GetComponent<Renderer>().sharedMaterial = markerMaterial;
            }
        }

        void CreateBoundary(Transform parent, ZoneDefinition zone)
        {
            var edge = new GameObject("ZoneBoundary");
            edge.transform.SetParent(parent, false);
            edge.transform.localPosition = new Vector3(ZoneSize*.5f, 1f, ZoneSize*.5f);
            var line = edge.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = 1.2f;
            line.positionCount = 4;
            line.SetPosition(0, new Vector3(0,0,0));
            line.SetPosition(1, new Vector3(ZoneSize,0,0));
            line.SetPosition(2, new Vector3(ZoneSize,0,ZoneSize));
            line.SetPosition(3, new Vector3(0,0,ZoneSize));
            line.material = markerMaterial;
        }

        void GenerateWorldSpine(Transform parent)
        {
            // Central rift corridor connecting the progression route without copying any third-party map.
            var rift = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rift.name = "EternalRift_WorldSpine";
            rift.transform.SetParent(parent, false);
            rift.transform.position = new Vector3(4f * ZoneSize - 70f, 8f, ZoneSize);
            rift.transform.localScale = new Vector3(28f, 8f, 28f);
            rift.GetComponent<Renderer>().sharedMaterial = waterMaterial;
        }

        void PositionMainCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (cam.transform.position.sqrMagnitude > 1f) return;
            cam.transform.position = new Vector3(ZoneSize*.5f, 80f, -110f);
            cam.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
        }

        public static IReadOnlyList<ZoneDefinition> GetZoneDefinitions() { return Zones; }
        public static float GetZoneSize() { return ZoneSize; }
        public static int GetWorldSeed() { return WorldSeed; }
    }
}
