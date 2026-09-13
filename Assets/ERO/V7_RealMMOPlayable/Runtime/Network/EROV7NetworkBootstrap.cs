using UnityEngine;
using Unity.Netcode;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private bool autoStartServerInBatchMode = true;
        [SerializeField] private bool autoStartHostInEditor = false;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (NetworkManager.Singleton == null) return;

            if (Application.isBatchMode && autoStartServerInBatchMode)
            {
                if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
                    NetworkManager.Singleton.StartServer();
                return;
            }

#if UNITY_EDITOR
            if (autoStartHostInEditor && !NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.StartHost();
#endif
        }
    }
}
