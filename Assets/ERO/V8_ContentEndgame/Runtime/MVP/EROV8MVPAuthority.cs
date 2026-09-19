using System;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V8
{
    /// <summary>
    /// Server-authoritative world-boss state for the networked endgame path.
    /// Damage is bounded and rate-limited per sender; level, HP and respawn state are server-owned.
    /// World-boss progression survives dedicated-server restarts through an atomic local snapshot.
    /// </summary>
    public sealed class EROV8MVPAuthority : NetworkBehaviour
    {
        [Serializable]
        private sealed class PersistentState
        {
            public int level = 1;
            public int defeats;
            public long respawnUtcTicks;
        }

        private const int BaseHealth = 250000;
        private const float HealthGrowth = 1.6f;
        private const int MaxLevel = 50;
        private const double RespawnSeconds = 3600d;
        private const double DamageIntervalSeconds = 0.15d;
        private const int MaxDamagePerHit = 1000000;

        public NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> HP = new(BaseHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> Defeated = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<double> RespawnAtServerTime = new(0d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly Dictionary<ulong, double> lastDamageByClient = new();
        private int defeats;
        private DateTime respawnUtc = DateTime.MinValue;

        private string SavePath => Path.Combine(Application.persistentDataPath, "ero_mvp_authority_state.json");

        public int MaxHealth => CalculateMaxHealth(Level.Value);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            LoadState();
            Level.Value = Mathf.Clamp(Level.Value, 1, MaxLevel);
            var nowUtc = DateTime.UtcNow;
            var remaining = respawnUtc == DateTime.MinValue ? 0d : Math.Max(0d, (respawnUtc - nowUtc).TotalSeconds);

            if (remaining > 0d)
            {
                HP.Value = 0;
                Defeated.Value = true;
                RespawnAtServerTime.Value = NetworkManager.ServerTime.Time + remaining;
            }
            else
            {
                HP.Value = MaxHealth;
                Defeated.Value = false;
                RespawnAtServerTime.Value = 0d;
                respawnUtc = DateTime.MinValue;
            }
        }

        private void Update()
        {
            if (!IsServer || !Defeated.Value) return;
            if (NetworkManager == null) return;

            var now = NetworkManager.ServerTime.Time;
            if (now < RespawnAtServerTime.Value) return;

            HP.Value = MaxHealth;
            Defeated.Value = false;
            RespawnAtServerTime.Value = 0d;
            respawnUtc = DateTime.MinValue;
            SaveState();
        }

        [ServerRpc(InvokePermission = RpcInvokePermission.Everyone)]
        public void ApplyDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
        {
            if (!IsServer || Defeated.Value || damage <= 0 || damage > MaxDamagePerHit) return;
            if (NetworkManager == null) return;

            var senderId = rpcParams.Receive.SenderClientId;
            var now = NetworkManager.ServerTime.Time;
            if (lastDamageByClient.TryGetValue(senderId, out var lastDamage) && now - lastDamage < DamageIntervalSeconds)
                return;

            var damageCap = Mathf.Clamp(Mathf.RoundToInt(40f * Mathf.Pow(1.45f, Level.Value - 1) * 25f), 1, MaxDamagePerHit);
            if (damage > damageCap) return;

            lastDamageByClient[senderId] = now;
            HP.Value = Mathf.Max(0, HP.Value - damage);
            if (HP.Value != 0) return;

            Defeated.Value = true;
            Level.Value = Mathf.Min(Level.Value + 1, MaxLevel);
            defeats++;
            respawnUtc = DateTime.UtcNow.AddSeconds(RespawnSeconds);
            RespawnAtServerTime.Value = now + RespawnSeconds;
            SaveState();
        }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                var json = File.ReadAllText(SavePath);
                var state = JsonUtility.FromJson<PersistentState>(json);
                if (state == null) return;

                Level.Value = Mathf.Clamp(state.level, 1, MaxLevel);
                defeats = Mathf.Max(0, state.defeats);
                respawnUtc = state.respawnUtcTicks > 0
                    ? new DateTime(state.respawnUtcTicks, DateTimeKind.Utc)
                    : DateTime.MinValue;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ERO network MVP state load failed: {exception.Message}");
            }
        }

        private void SaveState()
        {
            try
            {
                var state = new PersistentState
                {
                    level = Mathf.Clamp(Level.Value, 1, MaxLevel),
                    defeats = Mathf.Max(0, defeats),
                    respawnUtcTicks = respawnUtc == DateTime.MinValue ? 0 : respawnUtc.Ticks
                };
                var json = JsonUtility.ToJson(state, true);
                var tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tempPath, SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ERO network MVP state save failed: {exception.Message}");
            }
        }

        private static int CalculateMaxHealth(int level)
        {
            var scaled = BaseHealth * Mathf.Pow(HealthGrowth, Mathf.Clamp(level - 1, 0, MaxLevel - 1));
            return Mathf.Clamp(Mathf.RoundToInt(scaled), BaseHealth, int.MaxValue);
        }
    }
}
