using System;
using System.IO;
using UnityEngine;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Small, deterministic local profile store for the playable slice.
    /// It is deliberately isolated from the future authoritative MMO persistence service.
    /// </summary>
    public static class EROPersistentProfile
    {
        private const string FileName = "ero-profile.json";

        [Serializable]
        public sealed class Data
        {
            public int level = 1;
            public int xp;
            public int gold;
            public float positionX;
            public float positionY = 1f;
            public float positionZ = -12f;
        }

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static Data Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new Data();
                var json = File.ReadAllText(FilePath);
                var data = JsonUtility.FromJson<Data>(json);
                return data ?? new Data();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ERO] Profile load failed; using defaults. {ex.Message}");
                return new Data();
            }
        }

        public static bool Save(Data data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                var tempPath = FilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tempPath, FilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ERO] Profile save failed. {ex.Message}");
                return false;
            }
        }
    }
}
