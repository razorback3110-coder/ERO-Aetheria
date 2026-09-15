using System;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Local development persistence with schema validation, integrity checksum and recovery backup.</summary>
    public sealed class SaveSystem : MonoBehaviour
    {
        private const string Key = "ERO.Character.v3";
        private const string BackupKey = "ERO.Character.v3.backup";
        private const string LegacyKey = "ERO.Character.v2";
        private const string LegacyBackupKey = "ERO.Character.v2.backup";
        private const int SchemaVersion = 3;

        [Serializable]
        private sealed class SaveEnvelope
        {
            public int schemaVersion = SchemaVersion;
            public long savedAtUtcTicks;
            public string checksum;
            public CharacterData character;
        }

        public void Save(CharacterData character)
        {
            if (character == null) return;
            Normalize(character);
            var payload = JsonUtility.ToJson(character);
            if (string.IsNullOrEmpty(payload)) return;
            var envelope = new SaveEnvelope
            {
                schemaVersion = SchemaVersion,
                savedAtUtcTicks = DateTime.UtcNow.Ticks,
                checksum = ComputeChecksum(payload),
                character = character
            };
            var json = JsonUtility.ToJson(envelope);
            if (string.IsNullOrEmpty(json)) return;

            // Preserve only the last known-good primary save as the recovery copy.
            var previous = PlayerPrefs.GetString(Key, string.Empty);
            if (!string.IsNullOrEmpty(previous) && IsValidEnvelope(previous))
                PlayerPrefs.SetString(BackupKey, previous);

            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public CharacterData Load()
        {
            var character = TryLoad(Key);
            if (character != null) return character;
            return TryLoad(BackupKey) ?? TryLoadLegacy(LegacyKey) ?? TryLoadLegacy(LegacyBackupKey);
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.DeleteKey(BackupKey);
            PlayerPrefs.DeleteKey(LegacyKey);
            PlayerPrefs.DeleteKey(LegacyBackupKey);
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
                var payload = JsonUtility.ToJson(envelope.character);
                if (!string.Equals(envelope.checksum, ComputeChecksum(payload), StringComparison.Ordinal)) return null;
                Normalize(envelope.character);
                return envelope.character;
            }
            catch (Exception) { return null; }
        }

        private static CharacterData TryLoadLegacy(string key)
        {
            var json = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var envelope = JsonUtility.FromJson<LegacySaveEnvelope>(json);
                if (envelope == null || envelope.schemaVersion != 2 || envelope.character == null) return null;
                Normalize(envelope.character);
                return envelope.character;
            }
            catch (Exception) { return null; }
        }

        private static bool IsValidEnvelope(string json)
        {
            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(json);
                if (envelope == null || envelope.schemaVersion != SchemaVersion || envelope.character == null) return false;
                return string.Equals(envelope.checksum, ComputeChecksum(JsonUtility.ToJson(envelope.character)), StringComparison.Ordinal);
            }
            catch (Exception) { return false; }
        }

        private static string ComputeChecksum(string payload)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < payload.Length; i++)
                {
                    hash ^= payload[i];
                    hash *= 16777619u;
                }
                return hash.ToString("X8");
            }
        }

        [Serializable]
        private sealed class LegacySaveEnvelope
        {
            public int schemaVersion;
            public long savedAtUtcTicks;
            public CharacterData character;
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
