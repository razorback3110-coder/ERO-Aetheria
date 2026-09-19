using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Lightweight vertical-slice MVP event. Uses an already approved ERO enemy prefab
    /// and remains presentation-side until the authoritative encounter service is wired in.
    /// </summary>
    public sealed class EROMvpWorldEvent : MonoBehaviour
    {
        private static bool created;
        private GameObject boss;
        private Transform player;
        private const int MaxHealth = 250000;
        private const float RespawnDelaySeconds = 3600f;
        private int health = MaxHealth;
        private float respawnAt;
        private bool active;
        private int defeats;
        private string status = "Defeat the Aetheria creatures to awaken the MVP.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (created || SceneManager.GetActiveScene().name != "ERO_Playable") return;
            created = true;
            new GameObject("ERO_MVP_WorldEvent").AddComponent<EROMvpWorldEvent>();
        }

        private void Start()
        {
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
            if (active || Time.time < respawnAt) return;
            var remaining = GameObject.FindGameObjectsWithTag("Untagged");
            var defeatedCreatures = 0;
            for (var i = 0; i < remaining.Length; i++)
            {
                if (remaining[i] == null || !remaining[i].name.StartsWith("Enemy_")) continue;
                if (!remaining[i].activeSelf) defeatedCreatures++;
            }

            if (defeatedCreatures < 3) return;
            SpawnBoss();
        }

        private void SpawnBoss()
        {
            var prefab = Resources.Load<GameObject>("ERO/VandalImpGraphics");
            boss = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boss.name = "ERO_MVP_Aetheria_Warden";
            boss.transform.position = new Vector3(0f, 1.4f, 18f);
            boss.transform.localScale = prefab != null ? Vector3.one * 0.014f : Vector3.one * 2.8f;
            health = MaxHealth;
            active = true;
            status = "⚔ MVP AWAKENED — Aetheria Warden!";
        }

        private void TryAttackBoss()
        {
            var distance = Vector3.Distance(player.position, boss.transform.position);
            if (distance > 7f)
            {
                status = "MVP trop loin — approche-toi.";
                return;
            }

            var critical = Random.Range(0, 5) == 0;
            var damage = critical ? 60 : 40;
            health = Mathf.Max(0, health - damage);
            SpawnImpact(critical);
            status = critical ? $"CRITICAL! MVP -{damage} HP" : $"MVP -{damage} HP";

            if (health != 0) return;

            active = false;
            defeats++;
            Destroy(boss);
            boss = null;
            respawnAt = Time.time + RespawnDelaySeconds;
            status = $"MVP vaincu ! Réapparition dans 1 heure. Victoires: {defeats}";
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

        private static string FormatRespawnTime(float seconds)
        {
            var remaining = Mathf.Max(0, Mathf.CeilToInt(seconds));
            var hours = remaining / 3600;
            var minutes = (remaining % 3600) / 60;
            var secs = remaining % 60;
            return hours > 0 ? $"{hours}h {minutes:00}m {secs:00}s" : $"{minutes}m {secs:00}s";
        }

        private void OnGUI()
        {
            if (!active)
            {
                if (Time.time < respawnAt)
                    GUI.Label(new Rect(Screen.width - 360f, 116f, 340f, 24f), $"MVP respawn: {FormatRespawnTime(respawnAt - Time.time)}");
                return;
            }

            GUI.Box(new Rect(Screen.width * 0.5f - 220f, 18f, 440f, 92f), "AETHERIA WORLD BOSS • MVP");
            GUI.Label(new Rect(Screen.width * 0.5f - 205f, 46f, 410f, 22f), $"Aetheria Warden   {health}/{MaxHealth:N0} HP");
            GUI.Label(new Rect(Screen.width * 0.5f - 205f, 70f, 410f, 22f), status);
        }
    }
}
