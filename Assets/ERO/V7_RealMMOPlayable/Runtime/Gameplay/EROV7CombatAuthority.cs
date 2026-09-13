using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7CombatAuthority : NetworkBehaviour
    {
        [SerializeField] private float attackRange = 3.0f;
        [SerializeField] private int baseDamage = 20;

        [ServerRpc]
        public void RequestAttackServerRpc(ulong targetNetworkObjectId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var target))
                return;

            if (target == NetworkObject)
                return;

            if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
                return;

            var targetState = target.GetComponent<EROV7CombatTarget>();
            if (targetState != null)
                targetState.ApplyServerDamage(baseDamage);
        }
    }

    public sealed class EROV7CombatTarget : NetworkBehaviour
    {
        public NetworkVariable<int> HP = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public void ApplyServerDamage(int damage)
        {
            if (!IsServer || damage <= 0) return;
            HP.Value = Mathf.Max(0, HP.Value - damage);
        }
    }
}
