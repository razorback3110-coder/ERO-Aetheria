using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>Deterministic server clock. Gameplay systems should consume this tick instead of client time.</summary>
    public sealed class ERONetworkTickSystem
    {
        public const int DefaultTicksPerSecond = 20;
        private readonly int ticksPerSecond;
        private double accumulator;
        private long tick;

        public long CurrentTick => tick;
        public int TicksPerSecond => ticksPerSecond;
        public double FixedDeltaSeconds => 1d / ticksPerSecond;

        public ERONetworkTickSystem(int ticksPerSecond = DefaultTicksPerSecond)
        {
            this.ticksPerSecond = Math.Max(1, ticksPerSecond);
        }

        /// <summary>Advances the authoritative clock and returns the number of simulation ticks to process.</summary>
        public int Advance(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0d) return 0;
            accumulator = Math.Min(accumulator + elapsedSeconds, FixedDeltaSeconds * 5d);
            int steps = 0;
            while (accumulator >= FixedDeltaSeconds)
            {
                accumulator -= FixedDeltaSeconds;
                tick++;
                steps++;
            }
            return steps;
        }
    }

    [Serializable]
    public sealed class EROAOISnapshot
    {
        public long serverTick;
        public string sessionId;
        public int zoneId;
        public List<ulong> entityIds = new List<ulong>();
    }

    /// <summary>Builds deterministic AOI replication sets for each authenticated session.</summary>
    public static class ERONetworkAOIReplicationSystem
    {
        public static EROAOISnapshot BuildSnapshot(EROPlayerSession session, long serverTick, UnityEngine.Vector3 observerPosition, IReadOnlyList<EROAOIEntity> entities, float radius = EROAOIInterestSystem.DefaultRadius)
        {
            var snapshot = new EROAOISnapshot { serverTick = serverTick, sessionId = session != null ? session.sessionId : string.Empty, zoneId = session != null ? session.zoneId : -1 };
            if (session == null || !session.connected || !session.authenticated || entities == null) return snapshot;
            var visible = EROAOIInterestSystem.Collect(observerPosition, session.zoneId, entities, radius);
            for (int i = 0; i < visible.Count; i++) snapshot.entityIds.Add(visible[i].networkId);
            return snapshot;
        }
    }
}
