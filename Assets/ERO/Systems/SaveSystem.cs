using System;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Local development persistence with schema validation and recovery backup.</summary>
    public sealed class SaveSystem : MonoBehaviour
    {
        private const string Key = "ERO.Character.v2";
        private const string BackupKey = "ERO.Character.v2.backup";
        private const int SchemaVersion = 2;

        [Serializable]
        private sealed class SaveEnvelope
        {
            public int schemaVersion = SchemaVersion;
            public long savedAtUtcTicks;
            public CharacterData character;
        }

        public void Save(CharacterData character)
        {
            if (character == null) return;
            Normalize(character);
            var json = JsonUtility.ToJson(new SaveEnvelope { schemaVersion = SchemaVersion, savedAtUtcTicks = DateTime.UtcNow.Ticks, character = character });
            if (string.IsNullOrEmpty(json)) return;
            var previous = PlayerPrefs.GetString(Key, string.Empty);
            if (!string.IsNullOrEmpty(previous)) PlayerPrefs.SetString(BackupKey, previous);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public CharacterData Load()
        {
            var character = TryLoad(Key);
            return character ?? TryLoad(BackupKey);
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.DeleteKey(BackupKey);
            PlayerPrefs.Save();
        }

        private static CharacterData TryLoad(string key)
        {
            var json = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(json);
                if (envelope == null || envelope.schemaVersion != SchemaVersion || envelope.character == null) return null;
                Normalize(envelope.character);
                return envelope.character;
            }
            catch (Exception) { return null; }
        }

        private static void Normalize(CharacterData character)
        {
            if (character.appearance == null) character.appearance = new Appearance();
            if (character.stats == null) character.stats = new EROStatAllocation();
            if (character.inventory == null) character.inventory = new System.Collections.Generic.List<ItemData>();
            if (character.level < 1) character.level = 1;
            character.xp = Math.Max(0L, character.xp);
            character.overflowXp = Math.Max(0L, character.overflowXp);
            character.credits = Math.Max(0L, character.credits);
            character.eroCrystals = Math.Max(0L, character.eroCrystals);
            character.unspentStatPoints = Math.Max(0, character.unspentStatPoints);
            for (int i = character.inventory.Count - 1; i >= 0; i--)
            {
                var item = character.inventory[i];
                if (item == null || string.IsNullOrEmpty(item.id) || item.quantity <= 0) character.inventory.RemoveAt(i);
            }
        }
    }
}
