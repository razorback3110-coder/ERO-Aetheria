using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Dependency-free playable inventory presentation. It observes the vertical-slice enemies,
    /// converts defeated encounters into visible loot entries, and exposes a compact equipment/gear-score panel.
    /// Production inventory services remain authoritative and can replace this presentation layer.
    /// </summary>
    public sealed class EROPlayableLootInventory : MonoBehaviour
    {
        private readonly List<string> items = new List<string>(8);
        private readonly List<GameObject> trackedEnemies = new List<GameObject>(3);
        private readonly HashSet<int> rewardedObjects = new HashSet<int>();
        private bool open;
        private int gearScore = 100;
        private string equipped = "Apprentice Staff";
        private float nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable") return;
            var root = new GameObject("ERO_Playable_LootInventory");
            root.AddComponent<EROPlayableLootInventory>();
        }

        private void Start()
        {
            RefreshEnemyReferences();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) open = !open;
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.25f;
            RefreshEnemyReferences();
            ScanDefeatedEnemies();
        }

        private void RefreshEnemyReferences()
        {
            var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var transform in all)
            {
                if (!transform.name.StartsWith("Enemy_")) continue;
                if (!trackedEnemies.Contains(transform.gameObject)) trackedEnemies.Add(transform.gameObject);
            }
        }

        private void ScanDefeatedEnemies()
        {
            foreach (var enemy in trackedEnemies)
            {
                if (enemy == null || enemy.activeInHierarchy) continue;
                var id = enemy.GetInstanceID();
                if (!rewardedObjects.Add(id)) continue;

                var label = enemy.name.Replace("Enemy_", "").Replace("_", " ");
                AddLoot(label + " Essence");
                if (label.Contains("Warden")) AddLoot("Crystal Guard Plate");
                else if (label.Contains("Brute")) AddLoot("Voidbreaker Core");
                else AddLoot("Aether Wolf Fang");
            }
        }

        private void AddLoot(string item)
        {
            if (items.Count >= 8) return;
            items.Add(item);
            gearScore += 25;
            if (item.Contains("Staff") || item.Contains("Core") || item.Contains("Plate"))
            {
                equipped = item;
                gearScore += 35;
            }
        }

        private void OnGUI()
        {
            const float width = 300f;
            var x = Screen.width - width - 18f;
            var y = Screen.height - 188f;
            GUI.Box(new Rect(x, y, width, 170f), open ? "INVENTORY / EQUIPMENT" : "INVENTORY");
            GUI.Label(new Rect(x + 14f, y + 28f, width - 28f, 22f), $"Gear Score  {gearScore}    •    {items.Count}/8 slots");
            GUI.Label(new Rect(x + 14f, y + 50f, width - 28f, 22f), $"Weapon: {equipped}");

            if (!open)
            {
                GUI.Label(new Rect(x + 14f, y + 78f, width - 28f, 40f), "Press I to open inventory\nDefeated enemies generate loot automatically.");
                return;
            }

            for (var i = 0; i < 8; i++)
            {
                var row = i / 2;
                var column = i % 2;
                var slot = new Rect(x + 14f + column * 135f, y + 76f + row * 22f, 128f, 20f);
                GUI.Box(slot, i < items.Count ? items[i] : "Empty");
            }
        }
    }
}
