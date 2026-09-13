using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7LootAuthority : NetworkBehaviour
    {
        public NetworkVariable<bool> Claimed = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [ServerRpc]
        public void ClaimServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer || Claimed.Value) return;

            Claimed.Value = true;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                rpcParams.Receive.SenderClientId, out var player))
            {
                var state = player.GetComponent<EROV7PlayerState>();
                if (state != null)
                {
                    state.AddExperienceServer(25);
                    state.Gold.Value += 10;
                }
            }

            NetworkObject.Despawn(true);
        }
    }
}
