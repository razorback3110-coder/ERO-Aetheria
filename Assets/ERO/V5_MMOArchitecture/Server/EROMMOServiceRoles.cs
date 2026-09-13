using UnityEngine;

namespace EternalRealmsOnline.V5
{
    /// <summary>
    /// Service-role registry. It describes the intended deployment topology without
    /// coupling ERO gameplay to a specific cloud provider.
    /// </summary>
    public sealed class EROMMOServiceRoles : MonoBehaviour
    {
        public bool gatewayEnabled = true;
        public bool masterEnabled = true;
        public bool worldEnabled = true;

        public string gatewayService = "ero-gateway";
        public string masterService = "ero-master";
        public string worldService = "ero-world";

        public bool Validate()
        {
            return gatewayEnabled && masterEnabled && worldEnabled;
        }
    }
}
