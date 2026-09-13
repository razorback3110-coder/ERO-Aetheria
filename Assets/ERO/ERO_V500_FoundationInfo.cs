using UnityEngine;

namespace EternalRealmsOnline
{
    /// <summary>
    /// Marker/configuration component for the ERO V500 foundation fork.
    /// The Boss Room technical foundation remains intact; ERO gameplay is added in later layers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ERO_V500_FoundationInfo : MonoBehaviour
    {
        [SerializeField] private string projectName = "Eternal Realms Online";
        [SerializeField] private string foundation = "Unity Boss Room technical foundation";
        [SerializeField] private string eroVersion = "V500";

        public string ProjectName => projectName;
        public string Foundation => foundation;
        public string EROVersion => eroVersion;
    }
}
