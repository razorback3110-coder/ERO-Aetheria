using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Local persistence bridge for the playable slice. It persists the avatar transform
    /// without coupling the prototype to the future authoritative server persistence layer.
    /// </summary>
    public sealed class EROPlayablePersistence : MonoBehaviour
    {
        private const string RootName = "ERO_Playable_Persistence";
        private const string FileName = "ero-playable-position.json";
        private const float SaveInterval = 5f;
        private float nextSave;
        private Transform player;
        private bool restored;

        [Serializable]
        private sealed class PositionData
        {
            public float x;
            public float y = 1f;
            public float z = -12f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable") return;
            if (GameObject.Find(RootName) != null) return;
            var root = new GameObject(RootName);
            root.AddComponent<EROPlayablePersistence>();
            DontDestroyOnLoad(root);
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "ERO_Playable") return;
            if (player == null) player = FindPlayer();
            if (player == null) return;

            if (!restored)
            {
                Restore();
                restored = true;
            }

            if (Time.unscaledTime >= nextSave)
            {
                Save();
                nextSave = Time.unscaledTime + SaveInterval;
            }
        }

        private Transform FindPlayer()
        {
            var named = GameObject.Find("ERO_Player_Visual");
            if (named != null) return named.transform;
            named = GameObject.Find("ERO_Player_Demo");
            return named != null ? named.transform : null;
        }

        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private void Restore()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                var data = JsonUtility.FromJson<PositionData>(File.ReadAllText(FilePath));
                if (data == null) return;
                player.position = new Vector3(
                    Mathf.Clamp(data.x, -28f, 28f),
                    1f,
                    Mathf.Clamp(data.z, -28f, 28f));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ERO] Playable position restore failed: {ex.Message}");
            }
        }

        private void Save()
        {
            if (player == null) return;
            try
            {
                var data = new PositionData
                {
                    x = player.position.x,
                    y = 1f,
                    z = player.position.z
                };
                var json = JsonUtility.ToJson(data, true);
                var tempPath = FilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tempPath, FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ERO] Playable position save failed: {ex.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
