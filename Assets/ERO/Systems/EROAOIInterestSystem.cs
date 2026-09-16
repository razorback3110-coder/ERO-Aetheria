using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROAOIEntity
    {
        public ulong networkId;
        public int zoneId;
        public Vector3 position;
        public bool alwaysRelevant;
    }

    /// <summary>Deterministic interest-management helper. Only entities inside the observer's AOI are candidates for replication.</summary>
    public static class EROAOIInterestSystem
    {
        public const float DefaultRadius = 96f;

        public static List<EROAOIEntity> Collect(Vector3 observerPosition, int observerZone, IReadOnlyList<EROAOIEntity> entities, float radius = DefaultRadius)
        {
            var result = new List<EROAOIEntity>();
            CollectNonAlloc(observerPosition, observerZone, entities, result, radius);
            return result;
        }

        /// <summary>
        /// Reuses a caller-owned list to avoid per-tick allocations in server/client interest management.
        /// Results remain deterministically ordered by network id.
        /// </summary>
        public static void CollectNonAlloc(
            Vector3 observerPosition,
            int observerZone,
            IReadOnlyList<EROAOIEntity> entities,
            List<EROAOIEntity> result,
            float radius = DefaultRadius)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();
            if (entities == null || radius < 0f) return;

            float radiusSq = radius * radius;
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (entity == null || entity.zoneId != observerZone) continue;
                if (entity.alwaysRelevant || (entity.position - observerPosition).sqrMagnitude <= radiusSq)
                    result.Add(entity);
            }
            result.Sort((a, b) => a.networkId.CompareTo(b.networkId));
        }
    }
}
