using Unity.Netcode;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7NetworkStatusHUD : MonoBehaviour
    {
        private void OnGUI()
        {
            if (NetworkManager.Singleton == null) return;

            GUILayout.BeginArea(new Rect(18, 18, 360, 90), GUI.skin.box);
            GUILayout.Label("ERO • NETWORK");
            GUILayout.Label(
                NetworkManager.Singleton.IsServer ? "SERVER" :
                NetworkManager.Singleton.IsHost ? "HOST" :
                NetworkManager.Singleton.IsClient ? "CLIENT" : "OFFLINE"
            );
            GUILayout.Label("Connected: " + NetworkManager.Singleton.ConnectedClientsIds.Count);
            GUILayout.EndArea();
        }
    }
}
