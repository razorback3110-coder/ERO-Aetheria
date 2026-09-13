using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V8
{
    public sealed class EROV8MVPAuthority : NetworkBehaviour
    {
        public NetworkVariable<int> HP = new(100000, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> Defeated = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [ServerRpc(InvokePermission = RpcInvokePermission.Everyone)]
        public void ApplyDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
        {
            if (!IsServer || Defeated.Value || damage <= 0 || damage > 1000000) return;

            HP.Value = Mathf.Max(0, HP.Value - damage);
            if (HP.Value == 0)
                Defeated.Value = true;
        }
    }
}
