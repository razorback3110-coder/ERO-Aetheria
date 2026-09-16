using System;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Unity host for the ERO fixed-step simulation clock. Gameplay systems can
    /// subscribe to OnSimulationTick without coupling their simulation cadence
    /// to render frames. The same clock can later be hosted by a dedicated server.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EROSimulationDriver : MonoBehaviour
    {
        [SerializeField, Min(1)] private int ticksPerSecond = EROSimulationClock.DefaultTicksPerSecond;
        [SerializeField, Min(1)] private int maxCatchUpTicks = EROSimulationClock.DefaultMaxCatchUpTicks;

        private EROSimulationClock clock;

        public ulong TickId => clock?.TickId ?? 0UL;
        public int TicksPerSecond => clock?.TicksPerSecond ?? ticksPerSecond;
        public event Action<ulong> OnSimulationTick;

        private void Awake()
        {
            clock = new EROSimulationClock(ticksPerSecond, maxCatchUpTicks);
        }

        private void Update()
        {
            clock.Advance(Time.unscaledDeltaTime, RunTick);
        }

        private void RunTick(ulong tickId)
        {
            OnSimulationTick?.Invoke(tickId);
        }

        public void ResetSimulation(ulong tickId = 0UL)
        {
            if (clock == null)
                clock = new EROSimulationClock(ticksPerSecond, maxCatchUpTicks);
            clock.Reset(tickId);
        }
    }
}
