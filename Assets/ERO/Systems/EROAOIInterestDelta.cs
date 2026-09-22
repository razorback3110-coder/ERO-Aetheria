using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Computes deterministic AOI enter/leave deltas from two interest snapshots.
    /// The result is transport-agnostic so a dedicated server can feed it directly into
    /// spawn/despawn replication without coupling gameplay code to a networking package.
    /// </summary>
    public static class EROAOIInterestDelta
    {
        public static void Compute(
            IReadOnlyList<EROAOIEntity> previous,
            IReadOnlyList<EROAOIEntity> current,
            List<ulong> entered,
            List<ulong> left)
        {
            if (entered == null) throw new ArgumentNullException(nameof(entered));
            if (left == null) throw new ArgumentNullException(nameof(left));

            entered.Clear();
            left.Clear();

            var previousIds = new HashSet<ulong>();
            var currentIds = new HashSet<ulong>();

            if (previous != null)
            {
                for (int i = 0; i < previous.Count; i++)
                {
                    var entity = previous[i];
                    if (entity != null) previousIds.Add(entity.networkId);
                }
            }

            if (current != null)
            {
                for (int i = 0; i < current.Count; i++)
                {
                    var entity = current[i];
                    if (entity != null) currentIds.Add(entity.networkId);
                }
            }

            foreach (ulong id in currentIds)
            {
                if (!previousIds.Contains(id)) entered.Add(id);
            }

            foreach (ulong id in previousIds)
            {
                if (!currentIds.Contains(id)) left.Add(id);
            }

            entered.Sort();
            left.Sort();
        }
    }
}
