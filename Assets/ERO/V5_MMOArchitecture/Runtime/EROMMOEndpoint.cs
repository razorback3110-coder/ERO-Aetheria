using System;
using UnityEngine;

namespace EternalRealmsOnline.V5
{
    public enum EROMMOEndpointRole
    {
        Client,
        Gateway,
        Master,
        World
    }

    [Serializable]
    public sealed class EROMMOEndpoint
    {
        public EROMMOEndpointRole role = EROMMOEndpointRole.Client;
        public string host = "127.0.0.1";
        public ushort port = 7777;
        public string worldId = "greenhaven-01";
    }
}
