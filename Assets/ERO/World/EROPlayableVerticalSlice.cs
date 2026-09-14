using System.Collections.Generic;
using UnityEngine;
using ERO.Core;
using ERO.Data;

namespace ERO.World
{
    /// <summary>Playable vertical slice and deterministic streaming fallback.</summary>
    public sealed class EROPlayableVerticalSlice : MonoBehaviour
    {
        private const int ChunkSize = 48;
        private const int StreamRadius = 2;
        private const float PlayerSpeed = 7f;
        private const float SprintMultiplier = 1.65f;
        private const float MouseSensitivity = 2.2f;

        private readonly Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
        private CharacterController controller;
        private Transform player;
        private Camera playerCamera;
        private Transform enemy;
        private float pitch;
        private float enemyRespawnAt;
        private int enemyHealth;
        private int enemyMaxHealth;
        private string status = "Welcome to Eternal Realms Online";
        private GUIStyle titleStyle;
        private GUIStyle panelStyle;
        private GUIStyle textStyle;

        private void Start()
        {
            EnsureCharacter();
            CreatePlayer();
            CreateLighting();
            UpdateStreaming();
            CreateEnemy();
            LockCursor(true);
        }

        private void Update()
        {
            if (player == null) return;
            HandleLook();
            HandleMovement();
            UpdateStreaming();
            UpdateEnemy();
            if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(Cursor.lockState != CursorLockMode.Locked);
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) Attack();
        }

        private void EnsureCharacter()
        {
            var root = EROGameRoot.Instance;
            if (root != null && root.Systems != null && root.Systems.Character.Active == null)
                root.Systems.Character.NewCharacter("Aetherian", EROClass.Mage, Gender.Male);
        }

        private void CreatePlayer()
        {
            var go = new GameObject("ERO_Player");
            player = go.transform;
            player.position = new Vector3(0f, 2f, 0f);
            controller = go.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Player_DebugBody";
            body.transform.SetParent(player);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = MakeMaterial(new Color(0.12f, 0.22f, 0.55f));

            var cameraGo = new GameObject("ERO_PlayerCamera");
            cameraGo.transform.SetParent(player);
            cameraGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            playerCamera = cameraGo.AddComponent<Camera>();
            playerCamera.fieldOfView = 70f;
            playerCamera.nearClipPlane = 0.05f;
            playerCamera.farClipPlane = 600f;
        }

        private void CreateLighting()
        {
            var lightGo = new GameObject("ERO_Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.025f, 0.04f, 0.08f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0045f;
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            player.Rotate(0f, Input.GetAxis("Mouse X") * MouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * MouseSensitivity, -70f, 70f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 direction = (player.right * x + player.forward * z).normalized;
            float speed = Input.GetKey(KeyCode.LeftShift) ? PlayerSpeed * SprintMultiplier : PlayerSpeed;
            Vector3 velocity = direction * speed;
            velocity.y = controller.isGrounded ? -1f : Physics.gravity.y;
            controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateStreaming()
        {
            Vector2Int center = WorldToChunk(player.position);
            var needed = new HashSet<Vector2Int>();
            for (int z = -StreamRadius; z <= StreamRadius; z++)
            for (int x = -StreamRadius; x <= StreamRadius; x++)
            {
                var coord = new Vector2Int(center.x + x, center.y + z);
                needed.Add(coord);
                if (!chunks.ContainsKey(coord)) chunks.Add(coord, GenerateChunk(coord));
            }

            var remove = new List<Vector2Int>();
            foreach (var pair in chunks) if (!needed.Contains(pair.Key)) remove.Add(pair.Key);
            foreach (var coord in remove)
            {
                if (chunks[coord] != null) Destroy(chunks[coord]);
                chunks.Remove(coord);
            }
        }

        private GameObject GenerateChunk(Vector2Int coord)
        {
            var root = new GameObject("WorldChunk_" + coord.x + "_" + coord.y);
            root.transform.position = new Vector3(coord.x * ChunkSize, 0f, coord.y * ChunkSize);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Terrain_DebugFallback";
            ground.transform.SetParent(root.transform);
            ground.transform.localPosition = new Vector3(ChunkSize * 0.5f, -0.75f, ChunkSize * 0.5f);
            ground.transform.localScale = new Vector3(ChunkSize, 1.5f, ChunkSize);
            ground.GetComponent<Renderer>().material = MakeMaterial(new Color(0.06f, 0.16f, 0.10f));

            uint seed = unchecked((uint)(coord.x * 73856093 ^ coord.y * 19349663));
            var random = new System.Random((int)seed);
            for (int i = 0; i < 10; i++)
            {
                float x = (float)random.NextDouble() * (ChunkSize - 4f) + 2f;
                float z = (float)random.NextDouble() * (ChunkSize - 4f) + 2f;
                float scale = 0.8f + (float)random.NextDouble() * 1.7f;
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.name = "Nature_DebugFallback";
                tree.transform.SetParent(root.transform);
                tree.transform.localPosition = new Vector3(x, 1.2f * scale, z);
                tree.transform.localScale = new Vector3(0.45f * scale, 1.2f * scale, 0.45f * scale);
                tree.GetComponent<Renderer>().material = MakeMaterial(new Color(0.05f, 0.22f, 0.10f));
            }
            return root;
        }

        private void CreateEnemy()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "TrainingEnemy_DebugFallback";
            go.transform.position = new Vector3(0f, 1f, 12f);
            enemy = go.transform;
            enemy.GetComponent<Renderer>().material = MakeMaterial(new Color(0.55f, 0.08f, 0.12f));
            enemyMaxHealth = 100;
            enemyHealth = enemyMaxHealth;
        }

        private void UpdateEnemy()
        {
            if (enemy == null || player == null) return;
            if (enemyHealth <= 0 && Time.time >= enemyRespawnAt)
            {
                enemyHealth = enemyMaxHealth;
                enemy.gameObject.SetActive(true);
                enemy.position = player.position + player.forward * 10f + Vector3.up;
            }
            if (enemyHealth > 0)
            {
                Vector3 flat = player.position - enemy.position;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.01f) enemy.rotation = Quaternion.LookRotation(flat.normalized);
            }
        }

        private void Attack()
        {
            if (Cursor.lockState != CursorLockMode.Locked || enemy == null || enemyHealth <= 0) return;
            Vector3 toEnemy = enemy.position - playerCamera.transform.position;
            if (toEnemy.magnitude > 16f || Vector3.Angle(playerCamera.transform.forward, toEnemy) > 24f) return;
            var root = EROGameRoot.Instance;
            var character = root.Systems.Character.Active;
            var stats = EROCombatSystem.BuildStats(character);
            var skill = new EROSkillDefinition
            {
                id = "basic_attack",
                classId = character.classId,
                damageType = character.classId == EROClass.Mage || character.classId == EROClass.Priest || character.classId == EROClass.Invocateur ? ERODamageType.Magical : ERODamageType.Physical,
                requiredLevel = 1,
                powerBasisPoints = 10000,
                criticalBonusBasisPoints = 0,
                accuracyBonusBasisPoints = 0,
                cooldownMilliseconds = 500,
                resourceCost = 0,
                areaOfEffect = false
            };
            var combatEvent = EROCombatSystem.ResolveAttack(stats, stats, skill, 0, Random.Range(0, 10000), 10000);
            if (combatEvent.result == EROCombatResult.Miss) { status = "Attack missed"; return; }
            int damage = Mathf.Max(1, (int)Mathf.Min(int.MaxValue, combatEvent.mitigatedDamage));
            enemyHealth = Mathf.Max(0, enemyHealth - damage);
            status = (combatEvent.critical ? "Critical! " : "Hit for ") + damage + " damage";
            if (enemyHealth == 0)
            {
                enemy.gameObject.SetActive(false);
                enemyRespawnAt = Time.time + 3f;
                root.Systems.Progression.AddXP(character, 150);
                character.credits += 25;
                AddLoot(character, "loot_ember_shard", "Ember Shard");
                status = "Enemy defeated • +150 XP • +25 Credits";
            }
        }

        private static void AddLoot(CharacterData character, string id, string name)
        {
            if (character.inventory == null) character.inventory = new List<ItemData>();
            foreach (var item in character.inventory)
            {
                if (item != null && item.id == id && !item.equipped) { item.quantity++; return; }
            }
            character.inventory.Add(new ItemData { id = id, name = name, rarity = Rarity.Uncommon, level = 1, quantity = 1 });
        }

        private void OnGUI()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            panelStyle ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14 };
            textStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14 };
            var root = EROGameRoot.Instance;
            var character = root != null && root.Systems != null ? root.Systems.Character.Active : null;
            if (character == null) return;
            GUI.Box(new Rect(18, 18, 350, 170), GUIContent.none, panelStyle);
            GUI.Label(new Rect(32, 28, 320, 30), "ETERNAL REALMS ONLINE", titleStyle);
            GUI.Label(new Rect(32, 64, 320, 24), character.name + " • " + character.classId, textStyle);
            GUI.Label(new Rect(32, 88, 320, 24), "Level " + character.level + "   XP " + character.xp + "   Credits " + character.credits, textStyle);
            GUI.Label(new Rect(32, 112, 320, 24), "Inventory: " + (character.inventory == null ? 0 : character.inventory.Count) + " stacks", textStyle);
            GUI.Label(new Rect(32, 136, 320, 24), "WASD Move • Shift Sprint • Mouse Look • LMB/Space Attack", textStyle);
            GUI.Label(new Rect(32, 160, 320, 24), status, textStyle);
            if (enemy != null && enemyHealth > 0)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 120f, 34, 240, 42), GUIContent.none, panelStyle);
                GUI.Label(new Rect(Screen.width * 0.5f - 105f, 42, 210, 24), "Training Enemy  " + enemyHealth + "/" + enemyMaxHealth, textStyle);
            }
        }

        private static Vector2Int WorldToChunk(Vector3 position)
            => new Vector2Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.z / ChunkSize));

        private static Material MakeMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (material.shader == null || material.shader.name == "Hidden/InternalErrorShader") material.shader = Shader.Find("Standard");
            material.color = color;
            return material;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
