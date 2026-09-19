using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Self-contained visual vertical slice for ERO_Playable.
    /// It intentionally uses procedural primitives only as a temporary playable test surface;
    /// production art will replace these runtime-generated meshes through the legal asset pipeline.
    /// </summary>
    public sealed class EROVerticalSliceRuntime : MonoBehaviour
    {
        private const string RootName = "ERO_VerticalSlice_Runtime";
        private static bool created;
        private Transform player;
        private GameObject playerVisual;
        private int attackSequence;
        private Camera gameplayCamera;
        private readonly DemoEnemy[] enemies = new DemoEnemy[3];
        private int xp;
        private int gold;
        private string status = "Explore Aetheria — WASD to move, Space/click to attack.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable" || created) return;
            created = true;
            var root = new GameObject(RootName);
            root.AddComponent<EROVerticalSliceRuntime>();
        }

        private void Start()
        {
            BuildLighting();
            BuildWorld();
            BuildPlayer();
            BuildEnemies();
            BuildCamera();
        }

        private void Update()
        {
            if (player == null) return;

            var move = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude > 1f) move.Normalize();
            player.position += move * (6f * Time.deltaTime);
            player.position = new Vector3(Mathf.Clamp(player.position.x, -28f, 28f), 1f, Mathf.Clamp(player.position.z, -28f, 28f));

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                AttackNearest();

            for (var i = 0; i < enemies.Length; i++)
                enemies[i]?.Tick(Time.deltaTime);
        }

        private void BuildLighting()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.025f, 0.04f, 0.08f);
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.ambientIntensity = 0.8f;

            var sun = new GameObject("ERO_Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        }

        private void BuildWorld()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Aetheria_Ground";
            ground.transform.localScale = Vector3.one * 7f;
            ground.GetComponent<Renderer>().material = Mat(new Color(0.07f, 0.14f, 0.12f));

            for (var i = 0; i < 18; i++)
            {
                var angle = i * 20f * Mathf.Deg2Rad;
                var radius = 18f + (i % 4) * 2f;
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.name = "Aether_Crystal_" + i;
                crystal.transform.position = new Vector3(Mathf.Cos(angle) * radius, 1.2f, Mathf.Sin(angle) * radius);
                crystal.transform.localScale = new Vector3(0.45f, 2.2f + (i % 3) * 0.6f, 0.45f);
                crystal.transform.rotation = Quaternion.Euler(0f, i * 17f, 12f);
                crystal.GetComponent<Renderer>().material = Mat(new Color(0.15f, 0.65f, 1f), 2f);
            }

            for (var i = 0; i < 12; i++)
            {
                var x = -24f + (i % 6) * 9.5f;
                var z = -24f + (i / 6) * 48f;
                CreatePillar(new Vector3(x, 2f, z), 2.5f + (i % 3));
            }

            CreatePortal(new Vector3(0f, 2.5f, 22f));
            CreateRuin(new Vector3(-12f, 1.5f, 10f));
            CreateRuin(new Vector3(13f, 1.5f, -9f));
        }

        private void BuildPlayer()
        {
            playerVisual = Resources.Load<GameObject>("ERO/PlayerGraphics_Mage_Boy");
            if (playerVisual != null)
            {
                var instance = Instantiate(playerVisual);
                instance.name = "ERO_Player_Visual";
                instance.transform.position = new Vector3(0f, 0f, -12f);
                instance.transform.localScale = Vector3.one * 0.01f;
                player = instance.transform;
                return;
            }

            player = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            player.name = "ERO_Player_Demo";
            player.position = new Vector3(0f, 1f, -12f);
            player.localScale = new Vector3(0.9f, 1.2f, 0.9f);
            player.GetComponent<Renderer>().material = Mat(new Color(0.22f, 0.55f, 1f), 1.2f);
        }

        private void BuildEnemies()
        {
            enemies[0] = CreateEnemy("Aether Wolf", new Vector3(-8f, 1f, 2f), new Color(0.75f, 0.25f, 0.28f), 60);
            enemies[1] = CreateEnemy("Crystal Warden", new Vector3(8f, 1f, 8f), new Color(0.65f, 0.22f, 0.85f), 90);
            enemies[2] = CreateEnemy("Void Brute", new Vector3(0f, 1.2f, 15f), new Color(0.9f, 0.45f, 0.12f), 120);
        }

        private DemoEnemy CreateEnemy(string label, Vector3 position, Color color, int health)
        {
            var resourceName = label == "Void Brute" ? "ERO/VandalImpGraphics" : "ERO/ImpGraphics";
            var prefab = Resources.Load<GameObject>(resourceName);
            var go = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Enemy_" + label.Replace(" ", "_");
            go.transform.position = position;
            go.transform.localScale = prefab != null ? Vector3.one * 0.01f : Vector3.one * 1.8f;

            if (prefab == null)
                go.GetComponent<Renderer>().material = Mat(color, 0.5f);

            var enemy = go.AddComponent<DemoEnemy>();
            enemy.Initialize(label, health);
            return enemy;
        }

        private void BuildCamera()
        {
            gameplayCamera = new GameObject("ERO_Gameplay_Camera").AddComponent<Camera>();
            gameplayCamera.transform.position = new Vector3(0f, 16f, -22f);
            gameplayCamera.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            gameplayCamera.fieldOfView = 55f;
            gameplayCamera.backgroundColor = new Color(0.015f, 0.025f, 0.06f);
            gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void LateUpdate()
        {
            if (gameplayCamera == null || player == null) return;
            var target = player.position + new Vector3(0f, 1.5f, 0f);
            var desired = player.position + new Vector3(0f, 13f, -16f);
            gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, desired, 6f * Time.deltaTime);
            gameplayCamera.transform.LookAt(target);
        }

        private void AttackNearest()
        {
            DemoEnemy nearest = null;
            var best = 5.5f;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDefeated) continue;
                var distance = Vector3.Distance(player.position, enemy.transform.position);
                if (distance < best) { best = distance; nearest = enemy; }
            }

            if (nearest == null)
            {
                status = "No target in range.";
                return;
            }

            attackSequence++;
            var damage = 24 + ((attackSequence % 5 == 0) ? 12 : 0);
            nearest.TakeDamage(damage);
            status = $"Hit {nearest.Label} for {damage} damage.";
            if (nearest.IsDefeated)
            {
                xp += nearest.XpReward;
                gold += nearest.GoldReward;
                status = $"Defeated {nearest.Label}! +{nearest.XpReward} XP  +{nearest.GoldReward} gold. Loot secured.";
            }
        }

        private void CreatePillar(Vector3 position, float height)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Aether_Pillar";
            pillar.transform.position = position;
            pillar.transform.localScale = new Vector3(2.2f, height, 2.2f);
            pillar.GetComponent<Renderer>().material = Mat(new Color(0.16f, 0.18f, 0.24f));
        }

        private void CreatePortal(Vector3 position)
        {
            var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            portal.name = "Aether_Portal";
            portal.transform.position = position;
            portal.transform.localScale = new Vector3(3f, 0.3f, 3f);
            portal.GetComponent<Renderer>().material = Mat(new Color(0.25f, 0.75f, 1f), 3f);
        }

        private void CreateRuin(Vector3 position)
        {
            for (var i = 0; i < 4; i++)
                CreatePillar(position + new Vector3((i % 2) * 5f, 0f, (i / 2) * 5f), 2f + i * 0.5f);
        }

        private static Material Mat(Color color, float emission = 0f)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            return material;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(18f, 18f, 420f, 122f), "ETERNAL REALMS ONLINE  •  AETHERIA");
            GUI.Label(new Rect(34f, 48f, 380f, 24f), "VERTICAL SLICE — combat / XP / loot loop");
            GUI.Label(new Rect(34f, 74f, 380f, 24f), $"XP {xp}     Gold {gold}     Enemies remaining: {AliveCount()}");
            GUI.Label(new Rect(34f, 100f, 380f, 24f), status);
            GUI.Label(new Rect(34f, 124f, 380f, 24f), "WASD: move   SPACE / Left Click: attack");
        }

        private int AliveCount()
        {
            var count = 0;
            foreach (var enemy in enemies) if (enemy != null && !enemy.IsDefeated) count++;
            return count;
        }

        private sealed class DemoEnemy : MonoBehaviour
        {
            public string Label { get; private set; }
            public bool IsDefeated { get; private set; }
            public int XpReward { get; private set; }
            public int GoldReward { get; private set; }
            private int health;
            private int maxHealth;
            private Vector3 home;
            private float pulse;

            public void Initialize(string label, int maxHealthValue)
            {
                Label = label;
                maxHealth = maxHealthValue;
                health = maxHealthValue;
                XpReward = maxHealthValue / 2;
                GoldReward = maxHealthValue / 3;
                home = transform.position;
            }

            public void TakeDamage(int damage)
            {
                if (IsDefeated) return;
                health = Mathf.Max(0, health - damage);
                transform.localScale *= 1.03f;
                if (health == 0)
                {
                    IsDefeated = true;
                    gameObject.SetActive(false);
                }
            }

            public void Tick(float delta)
            {
                if (IsDefeated) return;
                pulse += delta * 2f;
                transform.position = home + Vector3.up * (Mathf.Sin(pulse) * 0.15f);
            }
        }
    }
}
