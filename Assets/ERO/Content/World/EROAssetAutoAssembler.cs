using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.World
{
    /// <summary>
    /// Automatically dresses the deterministic Aetheria layout with approved prefabs.
    /// Drop licensed prefabs under Resources/ERO/ApprovedAssets and the world generator
    /// will use them without hand-placing every object. Missing categories safely fall back
    /// to the procedural placeholders.
    /// </summary>
    public sealed class EROAssetAutoAssembler : MonoBehaviour
    {
        const string RootName = "ERO_LicensedAssetAssembler";
        const string SceneName = "ERO_Playable";
        const string ResourcePath = "ERO/ApprovedAssets";
        static EROAssetAutoAssembler instance;
        bool assembled;

        static readonly string[][] Categories =
        {
            new[] { "tree", "pine", "forest", "bush", "plant", "vegetation" },
            new[] { "rock", "stone", "cliff", "boulder" },
            new[] { "house", "village", "inn", "market", "building", "blacksmith" },
            new[] { "dungeon", "crypt", "ruin", "wall", "pillar", "torch" },
            new[] { "chest", "crate", "barrel", "cart", "prop", "furniture" }
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject(RootName);
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EROAssetAutoAssembler>();
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
                Invoke(nameof(Assemble), 0.25f);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals(SceneName, StringComparison.OrdinalIgnoreCase))
            {
                assembled = false;
                Invoke(nameof(Assemble), 0.25f);
            }
        }

        void Assemble()
        {
            if (assembled) return;
            assembled = true;

            GameObject[] prefabs = Resources.LoadAll<GameObject>(ResourcePath);
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.Log("[ERO Assets] No approved prefabs installed yet; keeping procedural fallback dressing.");
                return;
            }

            int placed = 0;
            for (int category = 0; category < Categories.Length; category++)
            {
                GameObject prefab = FindPrefab(prefabs, Categories[category]);
                if (prefab == null) continue;
                placed += DressCategory(category, prefab);
            }

            Debug.Log("[ERO Assets] Automatic licensed dressing complete. Prefab instances: " + placed + ".");
        }

        static GameObject FindPrefab(GameObject[] prefabs, string[] keywords)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                string name = prefabs[i].name.ToLowerInvariant();
                for (int k = 0; k < keywords.Length; k++)
                    if (name.Contains(keywords[k])) return prefabs[i];
            }
            return null;
        }

        static int DressCategory(int category, GameObject prefab)
        {
            GameObject[] pois = GameObject.FindGameObjectsWithTag("Untagged");
            int placed = 0;
            for (int i = 0; i < pois.Length; i++)
            {
                if (pois[i] == null || !pois[i].name.StartsWith("POI_", StringComparison.Ordinal)) continue;
                if ((StableHash(pois[i].name) + category * 17) % 5 != category) continue;

                GameObject instance = Instantiate(prefab, pois[i].transform.position, Quaternion.identity);
                instance.name = "ERO_Licensed_" + prefab.name + "_" + placed.ToString("000");
                instance.transform.localScale *= 1f + ((StableHash(instance.name) % 25) / 100f);
                placed++;
            }
            return placed;
        }

        static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return Mathf.Abs(hash);
            }
        }
    }
}
