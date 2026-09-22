using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Owns one observer's AOI snapshots and computes enter/leave replication deltas
    /// without allocating membership sets on every server tick.
    /// </summary>
    public sealed class EROAOIReplicationState
    {
        private List<EROAOIEntity> previous = new List<EROAOIEntity>();
        private List<EROAOIEntity> current = new List<EROAOIEntity>();
        private readonly EROAOIInterestDeltaBuffer deltaBuffer = new EROAOIInterestDeltaBuffer();

        /// <summary>
        /// Rebuilds this observer's interest snapshot and writes deterministic enter/leave ids.
        /// The supplied output lists remain caller-owned and are cleared before use.
        /// </summary>
        public void Update(
            Vector3 observerPosition,
            int observerZone,
            IReadOnlyList<EROAOIEntity> entities,
            List<ulong> entered,
            List<ulong> left,
            float radius = EROAOIInterestSystem.DefaultRadius)
        {
            if (entered == null) throw new ArgumentNullException(nameof(entered));
            if (left == null) throw new ArgumentNullException(nameof(left));

            EROAOIInterestSystem.CollectNonAlloc(
                observerPosition,
                observerZone,
                entities,
                current,
                radius);

            deltaBuffer.Compute(previous, current, entered, left);

            List<EROAOIEntity> completed = previous;
            previous = current;
            current = completed;
            current.Clear();
        }

        /// <summary>Clears the observer's known interest so the next update treats all visible entities as entered.</summary>
        public void Reset()
        {
            previous.Clear();
            current.Clear();
        }
    }
}
