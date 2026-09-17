using System.Collections.Generic;
using UnityEngine;
using ERO.Art;

namespace ERO.World
{
    /// <summary>Reusable deterministic chunk streamer. Only rebuilds the active window when the player crosses a chunk boundary.</summary>
    public sealed class EROWorldChunkStreamer : MonoBehaviour
    {
        [SerializeField, Min(8)] private int chunkSize = 48;
        [SerializeField, Range(1, 8)] private int streamRadius = 2;
        [SerializeField] private Transform target;

        private readonly Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int streamedCenter = new Vector2Int(int.MinValue, int.MinValue);

        public int LoadedChunkCount => chunks.Count;
        public Vector2Int CurrentChunk => streamedCenter;

        private void Update()
        {
            if (target == null) return;
            UpdateStreaming(WorldToChunk(target.position));
        }

        public void SetTarget(Transform value)
        {
            target = value;
            streamedCenter = new Vector2Int(int.MinValue, int.MinValue);
        }

        public void Rebuild()
        {
            streamedCenter = new Vector2Int(int.MinValue, int.MinValue);
            if (target != null) UpdateStreaming(WorldToChunk(target.position));
        }

        private void UpdateStreaming(Vector2Int center)
        {
            if (center == streamedCenter) return;
            streamedCenter = center;

            var needed = new HashSet<Vector2Int>();
            for (int z = -streamRadius; z <= streamRadius; z++)
            {
                for (int x = -streamRadius; x <= streamRadius; x++)
                {
                    var coord = new Vector2Int(center.x + x, center.y + z);
                    needed.Add(coord);
                    if (!chunks.ContainsKey(coord)) chunks.Add(coord, GenerateChunk(coord));
                }
            }

            var remove = new List<Vector2Int>();
            foreach (var pair in chunks)
            {
                if (!needed.Contains(pair.Key)) remove.Add(pair.Key);
            }

            foreach (var coord in remove)
            {
                if (chunks[coord] != null) Destroy(chunks[coord]);
                chunks.Remove(coord);
            }
        }

        private GameObject GenerateChunk(Vector2Int coord)
        {
            var root = new GameObject($"Aetheria_Chunk_{coord.x}_{coord.y}");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(coord.x * chunkSize, 0f, coord.y * chunkSize);
            int seed = coord.x * 73856093 ^ coord.y * 19349663;
            EROProceduralFantasyArt.CreateGround(root.transform, chunkSize, seed);
            return root;
        }

        private Vector2Int WorldToChunk(Vector3 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.z / chunkSize));
        }

        private void OnDestroy()
        {
            foreach (var pair in chunks)
            {
                if (pair.Value != null) Destroy(pair.Value);
            }
            chunks.Clear();
        }
    }
}
