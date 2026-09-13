using UnityEngine;

namespace EternalRealmsOnline.V6
{
    public enum EROV6Role { Client, Gateway, Master, World }

    [CreateAssetMenu(menuName = "ERO/V6/Network Topology")]
    public sealed class EROV6NetworkTopology : ScriptableObject
    {
        public EROV6Role role = EROV6Role.Client;
        public string gatewayHost = "127.0.0.1";
        public ushort gatewayPort = 7777;
        public string worldId = "greenhaven-01";
        public int maxPlayersPerInstance = 100;
        public bool privateInstances = true;
    }
}
