using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.World
{
    /// <summary>
    /// Lightweight deterministic world streaming foundation for ERO_Playable.
    /// Generates ERO-authored procedural dressing in square chunks around the player and
    /// unloads distant chunks. It is intentionally independent from third-party assets.
    /// </summary>
    public sealed class EROWorldChunkStreaming : MonoBehaviour
    {
        private const string RootName = "ERO_World_Streaming";
        private const int ChunkSize = 24;
        private const int LoadRadius = 2;
        private const int UnloadRadius = 3;
        private const int Seed = 7319;

        private readonly Dictionary<Vector2Int, GameObject> loaded = new Dictionary<Vector2Int, GameObject>();
        private Transform player;
        private Vector2Int currentCenter = new Vector2Int(int.MinValue, int.MinValue);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable") return;
            if (GameObject.Find(RootName) != null) return;
            var root = new GameObject(RootName);
            root.AddComponent<EROWorldChunkStreaming>();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable") return;
            if (player == null) player = FindPlayer();
            if (player == null) return;

            var center = ToChunk(player.position);
            if (center == currentCenter) return;
            currentCenter = center;
            StreamAround(center);
        }

        private static Transform FindPlayer()
        {
            var go = GameObject.Find("ERO_Player_Visual");
            if (go != null) return go.transform;
            go = GameObject.Find("ERO_Player_Demo");
            return go != null ? go.transform : null;
        }

        private static Vector2Int ToChunk(Vector3 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.z / ChunkSize));
        }

        private void StreamAround(Vector2Int center)
        {
            for (var x = -LoadRadius; x <= LoadRadius; x++)
            {
                for (var z = -LoadRadius; z <= LoadRadius; z++)
                {
                    var coord = new Vector2Int(center.x + x, center.y + z);
                    if (!loaded.ContainsKey(coord)) LoadChunk(coord);
                }
            }

            var remove = new List<Vector2Int>();
            foreach (var pair in loaded)
            {
                var distance = Mathf.Max(Mathf.Abs(pair.Key.x - center.x), Mathf.Abs(pair.Key.y - center.y));
                if (distance > UnloadRadius) remove.Add(pair.Key);
            }

            for (var i = 0; i < remove.Count; i++)
            {
                Destroy(loaded[remove[i]]);
                loaded.Remove(remove[i]);
            }
        }

        private void LoadChunk(Vector2Int coord)
        {
            var root = new GameObject($"ERO_Chunk_{coord.x}_{coord.y}");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(coord.x * ChunkSize, 0f, coord.y * ChunkSize);

            var random = new System.Random(Seed ^ (coord.x * 73856093) ^ (coord.y * 19349663));
            var count = 6 + random.Next(0, 5);
            for (var i = 0; i < count; i++)
            {
                var x = (float)random.NextDouble() * (ChunkSize - 2f) - (ChunkSize * 0.5f - 1f);
                var z = (float)random.NextDouble() * (ChunkSize - 2f) - (ChunkSize * 0.5f - 1f);
                var primitive = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
                primitive.name = "ERO_Procedural_Dressing";
                primitive.transform.SetParent(root.transform, false);
                primitive.transform.localPosition = new Vector3(x, i % 3 == 0 ? 0.65f : 0.35f, z);
                var scale = 0.5f + (float)random.NextDouble() * 1.4f;
                primitive.transform.localScale = i % 3 == 0
                    ? new Vector3(scale * 0.55f, scale * 1.8f, scale * 0.55f)
                    : new Vector3(scale, scale * 0.6f, scale);
                primitive.GetComponent<Renderer>().material = CreateMaterial(i % 3 == 0);
            }

            loaded.Add(coord, root);
        }

        private static Material CreateMaterial(bool vegetation)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            material.color = vegetation ? new Color(0.12f, 0.28f, 0.16f) : new Color(0.22f, 0.24f, 0.28f);
            return material;
        }

        private void OnDestroy()
        {
            foreach (var pair in loaded)
            {
                if (pair.Value != null) Destroy(pair.Value);
            }
            loaded.Clear();
        }
    }
}
