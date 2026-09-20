using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Lightweight vertical-slice MVP event. Uses an already approved ERO enemy prefab
    /// and persists the world-boss progression/cooldown locally for the playable build.
    /// The authoritative server should own this state when dedicated-server persistence is wired in.
    /// </summary>
    public sealed class EROMvpWorldEvent : MonoBehaviour
    {
        [Serializable]
        private sealed class MvpState
        {
            public int level = 1;
            public int defeats;
            public long respawnUtcTicks;
        }

        private static bool created;
        private GameObject boss;
        private Transform player;
        private const int BaseHealth = 250000;
        private const float HealthGrowthPerLevel = 1.6f;
        private const int MaxMvpLevel = 50;
        private const float RespawnDelaySeconds = 3600f;
        private int mvpLevel = 1;
        private int maxHealth = BaseHealth;
        private int health = BaseHealth;
        private DateTime respawnUtc = DateTime.MinValue;
        private bool active;
        private bool encounterStarted;
        private int defeats;
        private string status = "Defeat the Aetheria creatures to awaken the MVP.";

        private string SavePath => Path.Combine(Application.persistentDataPath, "ero_mvp_world_state.json");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (created || SceneManager.GetActiveScene().name != "ERO_Playable") return;
            created = true;
            new GameObject("ERO_MVP_WorldEvent").AddComponent<EROMvpWorldEvent>();
        }

        private void Start()
        {
            LoadState();
            InvokeRepeating(nameof(EvaluateEncounter), 1f, 1f);
        }

        private void Update()
        {
            if (player == null)
            {
                var playerObject = GameObject.Find("ERO_Player_Visual");
                if (playerObject != null) player = playerObject.transform;
            }

            if (!active || boss == null || player == null) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                TryAttackBoss();
        }

        private void EvaluateEncounter()
        {
            if (active || DateTime.UtcNow < respawnUtc) return;

            // The previous implementation looked for inactive objects through
            // FindGameObjectsWithTag("Untagged"), but Unity does not return inactive
            // objects from that API. That made the MVP unable to awaken after the
            // vertical-slice enemies were defeated. Track the encounter once the
            // expected enemy objects exist, then require zero active enemies.
            var enemyObjects = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var totalEnemiesSeen = 0;
            var activeEnemies = 0;
            for (var i = 0; i < enemyObjects.Length; i++)
            {
                var enemy = enemyObjects[i];
                if (enemy == null || !enemy.gameObject.scene.IsValid()) continue;
                if (!enemy.name.StartsWith("Enemy_", StringComparison.Ordinal)) continue;
                totalEnemiesSeen++;
                if (enemy.gameObject.activeInHierarchy) activeEnemies++;
            }

            if (totalEnemiesSeen > 0) encounterStarted = true;
            if (!encounterStarted || activeEnemies > 0) return;

            SpawnBoss();
        }

        private void SpawnBoss()
        {
            mvpLevel = Mathf.Clamp(mvpLevel, 1, MaxMvpLevel);
            maxHealth = CalculateMaxHealth(mvpLevel);
            health = maxHealth;
            var prefab = Resources.Load<GameObject>("ERO/VandalImpGraphics");
            boss = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boss.name = "ERO_MVP_Aetheria_Warden";
            boss.transform.position = new Vector3(0f, 1.4f, 18f);
            boss.transform.localScale = prefab != null ? Vector3.one * (0.014f + (mvpLevel - 1) * 0.00012f) : Vector3.one * 2.8f;
            active = true;
            status = $"⚔ MVP AWAKENED — Aetheria Warden Lv.{mvpLevel}!";
        }

        private void TryAttackBoss()
        {
            var distance = Vector3.Distance(player.position, boss.transform.position);
            if (distance > 7f)
            {
                status = "MVP trop loin — approche-toi.";
                return;
            }

            var critical = UnityEngine.Random.Range(0, 5) == 0;
            var damage = CalculateDamage(critical);
            health = Mathf.Max(0, health - damage);
            SpawnImpact(critical);
            status = critical ? $"CRITICAL! MVP -{damage:N0} HP" : $"MVP -{damage:N0} HP";

            if (health != 0) return;

            active = false;
            defeats++;
            Destroy(boss);
            boss = null;
            mvpLevel = Mathf.Min(mvpLevel + 1, MaxMvpLevel);
            respawnUtc = DateTime.UtcNow.AddSeconds(RespawnDelaySeconds);
            SaveState();
            status = $"MVP vaincu ! Prochain niveau: {mvpLevel} • Réapparition dans 1 heure. Victoires: {defeats}";
        }

        private void SpawnImpact(bool critical)
        {
            if (boss == null) return;
            var fx = new GameObject(critical ? "ERO_MVP_Critical" : "ERO_MVP_Impact");
            fx.transform.position = boss.transform.position + Vector3.up;
            var ps = fx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = critical ? 8f : 5f;
            main.startSize = critical ? 0.25f : 0.16f;
            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, critical ? 28u : 18u) });
            Destroy(fx, 0.6f);
        }

        private static int CalculateMaxHealth(int level)
        {
            var scaled = BaseHealth * Mathf.Pow(HealthGrowthPerLevel, level - 1);
            return Mathf.Clamp(Mathf.RoundToInt(scaled), BaseHealth, int.MaxValue);
        }

        private int CalculateDamage(bool critical)
        {
            var baseDamage = 40f * Mathf.Pow(1.45f, Mathf.Clamp(mvpLevel - 1, 0, MaxMvpLevel - 1));
            var damage = Mathf.RoundToInt(critical ? baseDamage * 1.5f : baseDamage);
            return Mathf.Clamp(damage, 1, 1000000);
        }

        private static string FormatRespawnTime(double seconds)
        {
            var remaining = Math.Max(0, (int)Math.Ceiling(seconds));
            var hours = remaining / 3600;
            var minutes = (remaining % 3600) / 60;
            var secs = remaining % 60;
            return hours > 0 ? $"{hours}h {minutes:00}m {secs:00}s" : $"{minutes}m {secs:00}s";
        }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                var json = File.ReadAllText(SavePath);
                var state = JsonUtility.FromJson<MvpState>(json);
                if (state == null) return;
                mvpLevel = Mathf.Clamp(state.level, 1, MaxMvpLevel);
                defeats = Mathf.Max(0, state.defeats);
                respawnUtc = state.respawnUtcTicks > 0 ? new DateTime(state.respawnUtcTicks, DateTimeKind.Utc) : DateTime.MinValue;
                status = DateTime.UtcNow < respawnUtc
                    ? $"MVP Lv.{mvpLevel} — monde boss en cooldown."
                    : "Defeat the Aetheria creatures to awaken the MVP.";
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ERO MVP state load failed: {exception.Message}");
            }
        }

        private void SaveState()
        {
            try
            {
                var state = new MvpState
                {
                    level = mvpLevel,
                    defeats = defeats,
                    respawnUtcTicks = respawnUtc == DateTime.MinValue ? 0 : respawnUtc.Ticks
                };
                var json = JsonUtility.ToJson(state, true);
                var tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tempPath, SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ERO MVP state save failed: {exception.Message}");
            }
        }

        private void OnGUI()
        {
            if (!active)
            {
                if (DateTime.UtcNow < respawnUtc)
                {
                    var remaining = (respawnUtc - DateTime.UtcNow).TotalSeconds;
                    GUI.Label(new Rect(Screen.width - 390f, 116f, 370f, 24f), $"MVP Lv.{mvpLevel} respawn: {FormatRespawnTime(remaining)}");
                }
                return;
            }

            GUI.Box(new Rect(Screen.width * 0.5f - 220f, 18f, 440f, 92f), "AETHERIA WORLD BOSS • MVP");
            GUI.Label(new Rect(Screen.width * 0.5f - 205f, 46f, 410f, 22f), $"Aetheria Warden • Lv.{mvpLevel}   {health}/{maxHealth:N0} HP");
            GUI.Label(new Rect(Screen.width * 0.5f - 205f, 70f, 410f, 22f), status);
        }
    }
}
