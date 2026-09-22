using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>
    /// Reusable AOI delta calculator for server ticks.
    /// Keeps membership buffers between calls so enter/leave replication does not allocate
    /// HashSets every tick. The output lists remain caller-owned and deterministically sorted.
    /// </summary>
    public sealed class EROAOIInterestDeltaBuffer
    {
        private readonly HashSet<ulong> previousIds = new HashSet<ulong>();
        private readonly HashSet<ulong> currentIds = new HashSet<ulong>();

        public void Compute(
            IReadOnlyList<EROAOIEntity> previous,
            IReadOnlyList<EROAOIEntity> current,
            List<ulong> entered,
            List<ulong> left)
        {
            if (entered == null) throw new ArgumentNullException(nameof(entered));
            if (left == null) throw new ArgumentNullException(nameof(left));

            entered.Clear();
            left.Clear();
            previousIds.Clear();
            currentIds.Clear();

            AddIds(previous, previousIds);
            AddIds(current, currentIds);

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

        private static void AddIds(IReadOnlyList<EROAOIEntity> source, HashSet<ulong> destination)
        {
            if (source == null) return;

            for (int i = 0; i < source.Count; i++)
            {
                var entity = source[i];
                if (entity != null) destination.Add(entity.networkId);
            }
        }
    }
}
