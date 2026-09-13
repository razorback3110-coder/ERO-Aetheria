using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V6
{
    /// <summary>
    /// Client-side presentation helper for nearby replicated entities.
    /// Server authority must still decide what is actually replicated.
    /// </summary>
    public sealed class EROV6InterestManager
    {
        readonly Dictionary<Vector2Int, HashSet<int>> cells = new();
        readonly float cellSize;

        public EROV6InterestManager(float cellSize = 32f)
        {
            this.cellSize = Mathf.Max(4f, cellSize);
        }

        Vector2Int Cell(Vector3 p) =>
            new(Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.z / cellSize));

        public void Upsert(int id, Vector3 p)
        {
            var c = Cell(p);
            if (!cells.TryGetValue(c, out var set))
                cells[c] = set = new HashSet<int>();
            set.Add(id);
        }

        public IEnumerable<int> Query(Vector3 p, int radius = 1)
        {
            var c = Cell(p);
            for (int x=-radius; x<=radius; x++)
                for (int z=-radius; z<=radius; z++)
                    if (cells.TryGetValue(new Vector2Int(c.x+x,c.y+z), out var set))
                        foreach (var id in set) yield return id;
        }
    }
}
