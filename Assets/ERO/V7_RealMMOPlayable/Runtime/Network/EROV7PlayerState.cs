using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7PlayerState : NetworkBehaviour
    {
        public NetworkVariable<int> Level = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Experience = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> HP = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> MaxHP = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Gold = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Level.Value = Mathf.Max(1, Level.Value);
                MaxHP.Value = Mathf.Max(1, MaxHP.Value);
                HP.Value = Mathf.Clamp(HP.Value, 0, MaxHP.Value);
            }
        }

        [ServerRpc]
        public void SubmitMoveIntentServerRpc(Vector3 requestedPosition, ServerRpcParams rpcParams = default)
        {
            // V7 keeps movement server-authoritative: production movement should use
            // input/velocity validation rather than trusting arbitrary client positions.
            if (Vector3.Distance(transform.position, requestedPosition) > 8f)
                return;

            transform.position = requestedPosition;
        }

        public void AddExperienceServer(int amount)
        {
            if (!IsServer || amount <= 0) return;
            Experience.Value += amount;
            while (Experience.Value >= RequiredXP(Level.Value))
            {
                Experience.Value -= RequiredXP(Level.Value);
                Level.Value++;
                MaxHP.Value += 10;
                HP.Value = MaxHP.Value;
            }
        }

        private static int RequiredXP(int level) => 100 + (level - 1) * 50;
    }
}
