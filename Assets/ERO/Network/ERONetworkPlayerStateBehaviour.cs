using Unity.Netcode;
using UnityEngine;

namespace ERO.Network
{
    /// <summary>Replicates validated RPG state. Only the server can authoritatively mutate values.</summary>
    public sealed class ERONetworkPlayerStateBehaviour : NetworkBehaviour
    {
        [SerializeField] private int minimumLevel = 1;
        [SerializeField] private int maximumLevel = 250;
        [SerializeField] private long maximumResource = 1000000L;
        [SerializeField] private long maximumCurrency = 9223372036854770000L;

        public NetworkVariable<int> Level = new NetworkVariable<int>(1);
        public NetworkVariable<long> Experience = new NetworkVariable<long>(0L);
        public NetworkVariable<long> OverflowExperience = new NetworkVariable<long>(0L);
        public NetworkVariable<long> CurrentHealth = new NetworkVariable<long>(1L);
        public NetworkVariable<long> MaximumHealth = new NetworkVariable<long>(1L);
        public NetworkVariable<long> CurrentResource = new NetworkVariable<long>(0L);
        public NetworkVariable<long> MaximumResource = new NetworkVariable<long>(0L);
        public NetworkVariable<long> Credits = new NetworkVariable<long>(0L);
        public NetworkVariable<long> EroCrystals = new NetworkVariable<long>(0L);
        public NetworkVariable<int> ZoneId = new NetworkVariable<int>(0);
        public NetworkVariable<uint> ServerTick = new NetworkVariable<uint>(0U);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            Level.Value = Mathf.Clamp(Level.Value, minimumLevel, maximumLevel);
            Experience.Value = Clamp(Experience.Value, 0L, long.MaxValue);
            OverflowExperience.Value = Clamp(OverflowExperience.Value, 0L, long.MaxValue);
            MaximumHealth.Value = Clamp(MaximumHealth.Value, 0L, long.MaxValue);
            CurrentHealth.Value = Clamp(CurrentHealth.Value, 0L, MaximumHealth.Value);
            MaximumResource.Value = Clamp(MaximumResource.Value, 0L, maximumResource);
            CurrentResource.Value = Clamp(CurrentResource.Value, 0L, MaximumResource.Value);
            Credits.Value = Clamp(Credits.Value, 0L, maximumCurrency);
            EroCrystals.Value = Clamp(EroCrystals.Value, 0L, maximumCurrency);
            ZoneId.Value = Mathf.Max(0, ZoneId.Value);
        }

        public void ApplyAuthoritativeState(int level, long experience, long overflowExperience,
            long currentHealth, long maximumHealth, long currentResource, long maximumResourceValue,
            long credits, long eroCrystals, int zoneId, uint serverTick)
        {
            if (!IsServer) return;
            Level.Value = Mathf.Clamp(level, minimumLevel, maximumLevel);
            Experience.Value = Clamp(experience, 0L, long.MaxValue);
            OverflowExperience.Value = Clamp(overflowExperience, 0L, long.MaxValue);
            MaximumHealth.Value = Clamp(maximumHealth, 0L, long.MaxValue);
            CurrentHealth.Value = Clamp(currentHealth, 0L, MaximumHealth.Value);
            MaximumResource.Value = Clamp(maximumResourceValue, 0L, maximumResource);
            CurrentResource.Value = Clamp(currentResource, 0L, MaximumResource.Value);
            Credits.Value = Clamp(credits, 0L, maximumCurrency);
            EroCrystals.Value = Clamp(eroCrystals, 0L, maximumCurrency);
            ZoneId.Value = Mathf.Max(0, zoneId);
            ServerTick.Value = serverTick;
        }

        private static long Clamp(long value, long minimum, long maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}
