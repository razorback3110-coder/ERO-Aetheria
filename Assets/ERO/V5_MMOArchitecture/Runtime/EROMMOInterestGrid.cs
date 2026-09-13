using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V5
{
    /// <summary>
    /// Lightweight spatial-interest helper. Gameplay authority remains elsewhere.
    /// It can be used to decide which nearby entities should be replicated.
    /// </summary>
    public sealed class EROMMOInterestGrid
    {
        readonly Dictionary<Vector2Int, HashSet<int>> cells = new();
        readonly float cellSize;

        public EROMMOInterestGrid(float cellSize = 32f)
        {
            this.cellSize = Mathf.Max(1f, cellSize);
        }

        public Vector2Int Cell(Vector3 position) =>
            new(Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.z / cellSize));

        public void Add(int entityId, Vector3 position)
        {
            var c = Cell(position);
            if (!cells.TryGetValue(c, out var set))
                cells[c] = set = new HashSet<int>();
            set.Add(entityId);
        }

        public void Remove(int entityId, Vector3 position)
        {
            var c = Cell(position);
            if (!cells.TryGetValue(c, out var set)) return;
            set.Remove(entityId);
            if (set.Count == 0) cells.Remove(c);
        }

        public IEnumerable<int> Nearby(Vector3 position, int radiusCells = 1)
        {
            var c = Cell(position);
            for (int x = -radiusCells; x <= radiusCells; x++)
                for (int z = -radiusCells; z <= radiusCells; z++)
                    if (cells.TryGetValue(new Vector2Int(c.x + x, c.y + z), out var set))
                        foreach (var id in set)
                            yield return id;
        }
    }
}
