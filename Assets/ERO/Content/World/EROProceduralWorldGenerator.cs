using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.World
{
    /// <summary>
    /// Deterministic automatic world generator for Aetheria.
    /// Builds terrain plus roads, rivers, forests, cliffs, settlements, dungeons,
    /// gathering nodes and world-event/boss anchors from one seed. Presentation
    /// prefabs can later replace the generated primitives without changing the rules.
    /// </summary>
    public sealed class EROProceduralWorldGenerator : MonoBehaviour
    {
        [Serializable]
        public sealed class ZoneDefinition
        {
            public string id;
            public string displayName;
            public int minLevel, maxLevel, gridX, gridZ;
            public Color terrainTint;
            public float height, moisture, temperature;
            public int poiCount;
        }

        const string RootName = "ERO_Aetheria_ProceduralWorld";
        const string SceneName = "ERO_Playable";
        const int WorldSeed = 740067;
        const float ZoneSize = 512f;
        const int Resolution = 33;
        const float CellSize = ZoneSize / (Resolution - 1);
        const float WaterLevel = 2.0f;

        static EROProceduralWorldGenerator instance;
        readonly List<GameObject> generatedZones = new List<GameObject>();
        Material terrainMaterial, roadMaterial, waterMaterial, markerMaterial, foliageMaterial, rockMaterial;
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

            for (int i = 0; i < Zones.Length; i++) GenerateZone(world.transform, Zones[i], i);
            GenerateWorldSpine(world.transform);
            PositionMainCamera();
            Debug.Log("[ERO World] Aetheria auto-generated: " + Zones.Length + " zones, seed " + WorldSeed + ".");
        }

        void BuildMaterials()
        {
            terrainMaterial = MakeMaterial("ERO Procedural Terrain", new Color(.20f,.25f,.30f));
            roadMaterial = MakeMaterial("ERO Roads", new Color(.20f,.16f,.12f));
            waterMaterial = MakeMaterial("ERO Water", new Color(.05f,.16f,.28f));
            markerMaterial = MakeMaterial("ERO POI", new Color(.75f,.58f,.18f));
            foliageMaterial = MakeMaterial("ERO Foliage", new Color(.12f,.30f,.12f));
            rockMaterial = MakeMaterial("ERO Rock", new Color(.22f,.22f,.25f));
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

            var rng = new System.Random(WorldSeed + index * 1009);
            CreateSettlement(go.transform, zone, rng);
            CreateRoadNetwork(go.transform, zone, rng);
            CreateRiver(go.transform, zone, index);
            CreateNaturalFeatures(go.transform, zone, rng);
            CreatePOIs(go.transform, zone, index, rng);
            CreateDungeonAndBoss(go.transform, zone, rng);
            CreateBoundary(go.transform, zone);
        }

        Mesh BuildTerrainMesh(ZoneDefinition zone, int index)
        {
            var mesh = new Mesh { name = zone.id + "_Terrain" };
            if (Resolution * Resolution > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            var vertices = new Vector3[Resolution * Resolution];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(Resolution - 1) * (Resolution - 1) * 6];
            int t = 0;
            float seedOffset = WorldSeed * .001f + index * 91.73f;

            for (int z = 0; z < Resolution; z++)
            for (int x = 0; x < Resolution; x++)
            {
                float nx = x / (float)(Resolution - 1), nz = z / (float)(Resolution - 1);
                float gx = zone.gridX + nx, gz = zone.gridZ + nz;
                float n1 = Mathf.PerlinNoise(gx * 2.15f + seedOffset, gz * 2.15f + seedOffset);
                float n2 = Mathf.PerlinNoise(gx * 6.5f + seedOffset, gz * 6.5f + seedOffset);
                float ridge = 1f - Mathf.Abs(n1 * 2f - 1f);
                float h = zone.height * (n1 * .55f + n2 * .16f + ridge * .24f - .15f);
                h += Mathf.Sin((gx + seedOffset) * 3.1f) * Mathf.Cos((gz + seedOffset) * 2.7f) * 2.2f;
                vertices[z * Resolution + x] = new Vector3(x * CellSize, Mathf.Max(-2f, h), z * CellSize);
                uv[z * Resolution + x] = new Vector2(nx, nz);
                if (x < Resolution - 1 && z < Resolution - 1)
                {
                    int a = z * Resolution + x, b = a + 1, c = a + Resolution, d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }
            mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        void CreateSettlement(Transform parent, ZoneDefinition zone, System.Random rng)
        {
            var root = new GameObject("Settlement_" + zone.id);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(ZoneSize*.5f, 2f, ZoneSize*.5f);
            CreateMarker(root.transform, "TownCenter", 7f, 5f, markerMaterial);
            int buildings = 4 + rng.Next(5);
            for (int i = 0; i < buildings; i++)
            {
                float a = i * Mathf.PI * 2f / buildings;
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = "Building_" + i.ToString("00");
                b.transform.SetParent(root.transform, false);
                b.transform.localPosition = new Vector3(Mathf.Cos(a)*26f, 4f, Mathf.Sin(a)*26f);
                float s = 5f + (float)rng.NextDouble()*4f;
                b.transform.localScale = new Vector3(s, 8f, s);
                b.GetComponent<Renderer>().sharedMaterial = rockMaterial;
            }
        }

        void CreateRoadNetwork(Transform parent, ZoneDefinition zone, System.Random rng)
        {
            float y = zone.height * .08f + 2f;
            CreateRoad(parent, new Vector3(ZoneSize*.5f,y,0), new Vector3(ZoneSize*.5f,y,ZoneSize));
            CreateRoad(parent, new Vector3(0,y,ZoneSize*.5f), new Vector3(ZoneSize,y,ZoneSize*.5f));
            if (rng.NextDouble() > .35) CreateRoad(parent, new Vector3(64,y,64), new Vector3(ZoneSize-64,y,ZoneSize-64));
        }

        void CreateRoad(Transform parent, Vector3 a, Vector3 b)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Road"; go.transform.SetParent(parent, false);
            Vector3 d = b-a; go.transform.localPosition = (a+b)*.5f;
            go.transform.localScale = new Vector3(Mathf.Max(5f,Mathf.Abs(d.x)),.45f,Mathf.Max(5f,Mathf.Abs(d.z)));
            go.GetComponent<Renderer>().sharedMaterial = roadMaterial;
        }

        void CreateRiver(Transform parent, ZoneDefinition zone, int index)
        {
            if (zone.moisture < .25f) return;
            var root = new GameObject("River_" + zone.id); root.transform.SetParent(parent, false);
            int points = 13;
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.positionCount = points; line.widthMultiplier = 5f;
            line.material = waterMaterial;
            for (int i = 0; i < points; i++)
            {
                float z = i * ZoneSize/(points-1);
                float x = ZoneSize*.15f + Mathf.Sin(i*.72f + index)*ZoneSize*.25f + Mathf.Sin(i*1.7f)*22f;
                line.SetPosition(i, new Vector3(Mathf.Clamp(x,18f,ZoneSize-18f), WaterLevel, z));
            }
        }

        void CreateNaturalFeatures(Transform parent, ZoneDefinition zone, System.Random rng)
        {
            int trees = Mathf.RoundToInt(18f + zone.moisture * 42f);
            int rocks = 8 + Mathf.RoundToInt((1f-zone.moisture) * 20f);
            for (int i = 0; i < trees; i++) CreateTree(parent, rng, zone);
            for (int i = 0; i < rocks; i++) CreateRock(parent, rng, zone);
            if (zone.temperature < .2f || zone.temperature > .85f) CreateCliffRidges(parent, zone);
        }

        void CreateTree(Transform parent, System.Random rng, ZoneDefinition zone)
        {
            float x=20f+(float)rng.NextDouble()*(ZoneSize-40f), z=20f+(float)rng.NextDouble()*(ZoneSize-40f);
            var trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder); trunk.name="Tree_Trunk"; trunk.transform.SetParent(parent,false);
            trunk.transform.localPosition=new Vector3(x,4f,z); trunk.transform.localScale=new Vector3(.7f,4f,.7f); trunk.GetComponent<Renderer>().sharedMaterial=rockMaterial;
            var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.name="Tree_Canopy"; crown.transform.SetParent(trunk.transform,false);
            crown.transform.localPosition=new Vector3(0,1.15f,0); crown.transform.localScale=new Vector3(4f,3.5f,4f); crown.GetComponent<Renderer>().sharedMaterial=foliageMaterial;
        }

        void CreateRock(Transform parent, System.Random rng, ZoneDefinition zone)
        {
            float x=16f+(float)rng.NextDouble()*(ZoneSize-32f), z=16f+(float)rng.NextDouble()*(ZoneSize-32f);
            var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere); rock.name="Rock"; rock.transform.SetParent(parent,false);
            float s=1.5f+(float)rng.NextDouble()*5f; rock.transform.localPosition=new Vector3(x,2f,z); rock.transform.localScale=new Vector3(s,s*.65f,s*1.2f); rock.GetComponent<Renderer>().sharedMaterial=rockMaterial;
        }

        void CreateCliffRidges(Transform parent, ZoneDefinition zone)
        {
            for (int i=0;i<4;i++)
            {
                var c=GameObject.CreatePrimitive(PrimitiveType.Cube); c.name="Cliff_Ridge_"+i; c.transform.SetParent(parent,false);
                c.transform.localPosition=new Vector3(64f+i*120f, zone.height*.35f, i%2==0?36f:ZoneSize-36f);
                c.transform.localScale=new Vector3(70f,zone.height*.65f,18f); c.GetComponent<Renderer>().sharedMaterial=rockMaterial;
            }
        }

        void CreatePOIs(Transform parent, ZoneDefinition zone, int index, System.Random rng)
        {
            for (int i=0;i<zone.poiCount;i++)
            {
                float x=32f+(float)rng.NextDouble()*(ZoneSize-64f), z=32f+(float)rng.NextDouble()*(ZoneSize-64f);
                float size=1.5f+(float)rng.NextDouble()*3.5f;
                var poi=GameObject.CreatePrimitive(i%5==0?PrimitiveType.Cylinder:PrimitiveType.Cube);
                poi.name="POI_"+zone.id+"_"+i.ToString("00"); poi.transform.SetParent(parent,false);
                poi.transform.localPosition=new Vector3(x,3f+size,z); poi.transform.localScale=new Vector3(size,size*1.8f,size); poi.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            }
        }

        void CreateDungeonAndBoss(Transform parent, ZoneDefinition zone, System.Random rng)
        {
            int dungeonCount = zone.maxLevel >= 70 ? 2 : 1;
            for (int i=0;i<dungeonCount;i++)
            {
                float x=80f+(float)rng.NextDouble()*(ZoneSize-160f), z=80f+(float)rng.NextDouble()*(ZoneSize-160f);
                var dungeon=GameObject.CreatePrimitive(PrimitiveType.Cube); dungeon.name="DungeonEntrance_"+zone.id+"_"+i; dungeon.transform.SetParent(parent,false);
                dungeon.transform.localPosition=new Vector3(x,4f,z); dungeon.transform.localScale=new Vector3(10f,8f,10f); dungeon.GetComponent<Renderer>().sharedMaterial=rockMaterial;
                var boss=GameObject.CreatePrimitive(PrimitiveType.Capsule); boss.name="WorldBossAnchor_"+zone.id+"_"+i; boss.transform.SetParent(dungeon.transform,false);
                boss.transform.localPosition=new Vector3(0,1.8f,0); boss.transform.localScale=new Vector3(2.5f,3.5f,2.5f); boss.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            }
        }

        void CreateMarker(Transform parent, string name, float radius, float height, Material material)
        {
            var marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder); marker.name=name; marker.transform.SetParent(parent,false);
            marker.transform.localPosition=new Vector3(0,height*.5f,0); marker.transform.localScale=new Vector3(radius,height*.5f,radius); marker.GetComponent<Renderer>().sharedMaterial=material;
        }

        void CreateBoundary(Transform parent, ZoneDefinition zone)
        {
            var edge=new GameObject("ZoneBoundary"); edge.transform.SetParent(parent,false); edge.transform.localPosition=new Vector3(ZoneSize*.5f,1f,ZoneSize*.5f);
            var line=edge.AddComponent<LineRenderer>(); line.useWorldSpace=false; line.loop=true; line.widthMultiplier=1.2f; line.positionCount=4; line.material=markerMaterial;
            line.SetPosition(0,new Vector3(0,0,0)); line.SetPosition(1,new Vector3(ZoneSize,0,0)); line.SetPosition(2,new Vector3(ZoneSize,0,ZoneSize)); line.SetPosition(3,new Vector3(0,0,ZoneSize));
        }

        void GenerateWorldSpine(Transform parent)
        {
            var rift=GameObject.CreatePrimitive(PrimitiveType.Cylinder); rift.name="EternalRift_WorldSpine"; rift.transform.SetParent(parent,false);
            rift.transform.position=new Vector3(4f*ZoneSize-70f,8f,ZoneSize); rift.transform.localScale=new Vector3(28f,8f,28f); rift.GetComponent<Renderer>().sharedMaterial=waterMaterial;
        }

        void PositionMainCamera()
        {
            var cam=Camera.main; if(cam==null || cam.transform.position.sqrMagnitude>1f) return;
            cam.transform.position=new Vector3(ZoneSize*.5f,80f,-110f); cam.transform.rotation=Quaternion.Euler(28f,0f,0f);
        }

        public static IReadOnlyList<ZoneDefinition> GetZoneDefinitions(){return Zones;}
        public static float GetZoneSize(){return ZoneSize;}
        public static int GetWorldSeed(){return WorldSeed;}
    }
}
