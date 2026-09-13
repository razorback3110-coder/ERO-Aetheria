using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7LocalPersistence : IEROV7Persistence
    {
        private readonly string root = Path.Combine(Application.persistentDataPath, "ERO", "Saves");
        private readonly Dictionary<string, EROV7CharacterSave> cache = new();

        public void QueueSave(EROV7CharacterSave save)
        {
            if (save == null) return;
            Directory.CreateDirectory(root);
            string key = save.accountId + "_" + save.characterId;
            cache[key] = save;
            File.WriteAllText(Path.Combine(root, key + ".json"), JsonUtility.ToJson(save, true));
        }

        public bool TryLoad(string accountId, string characterId, out EROV7CharacterSave save)
        {
            string key = accountId + "_" + characterId;
            if (cache.TryGetValue(key, out save)) return true;

            string path = Path.Combine(root, key + ".json");
            if (!File.Exists(path))
            {
                save = null;
                return false;
            }

            save = JsonUtility.FromJson<EROV7CharacterSave>(File.ReadAllText(path));
            cache[key] = save;
            return save != null;
        }
    }
}
