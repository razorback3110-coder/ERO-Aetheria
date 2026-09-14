using Unity.Netcode;
using UnityEngine;

namespace ERO.Network
{
    /// <summary>Central server tick source. Keeps gameplay replication on a deterministic cadence.</summary>
    public sealed class ERONetworkTickSystem : NetworkBehaviour
    {
        [SerializeField, Min(5)] private int tickRate = 20;
        private uint serverTick;
        private float accumulator;

        public uint CurrentTick => serverTick;
        public float TickInterval => 1f / Mathf.Max(5, tickRate);

        private void Update()
        {
            if (!IsServer) return;
            accumulator += Time.unscaledDeltaTime;
            float interval = TickInterval;
            while (accumulator >= interval)
            {
                accumulator -= interval;
                serverTick++;
            }
        }

        public bool IsTick(uint tick) => tick == serverTick;
    }
}
