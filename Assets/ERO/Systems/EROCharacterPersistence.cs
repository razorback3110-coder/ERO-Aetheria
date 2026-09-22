using System;
using System.IO;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Local persistence adapter for ERO character snapshots. The file format is
    /// versioned and integrity-checked; the authoritative server remains the source
    /// of truth for live MMO state.
    /// </summary>
    public sealed class EROCharacterPersistence : IEROCharacterPersistenceStore
    {
        private const string FilePrefix = "ero-character-";
        private const string FileSuffix = ".json";

        public string GetPath(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) throw new ArgumentException("Character id is required.", nameof(characterId));
            return Path.Combine(Application.persistentDataPath, FilePrefix + SanitizeId(characterId) + FileSuffix);
        }

        public bool Save(CharacterData character)
        {
            if (character == null || string.IsNullOrWhiteSpace(character.id)) return false;

            string path = GetPath(character.id);
            string tempPath = path + ".tmp";
            string payloadJson = JsonUtility.ToJson(character, false);
            string envelopeJson = JsonUtility.ToJson(EROCharacterPersistenceEnvelope.Create(character.id, payloadJson), false);

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(tempPath, envelopeJson);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);
                return true;
            }
            catch (IOException)
            {
                TryDelete(tempPath);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                TryDelete(tempPath);
                return false;
            }
        }

        public bool TryLoad(string characterId, out CharacterData character)
        {
            character = null;
            if (string.IsNullOrWhiteSpace(characterId)) return false;

            string path = GetPath(characterId);
            if (!File.Exists(path)) return false;

            try
            {
                string envelopeJson = File.ReadAllText(path);
                if (!EROCharacterPersistenceEnvelope.TryDeserialize(envelopeJson, characterId, out string payloadJson)) return false;

                character = JsonUtility.FromJson<CharacterData>(payloadJson);
                return character != null && string.Equals(character.id, characterId, StringComparison.Ordinal);
            }
            catch (IOException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool Delete(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) return false;
            string path = GetPath(characterId);
            if (!File.Exists(path)) return false;

            try
            {
                File.Delete(path);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static string SanitizeId(string value)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string sanitized = value.Trim();
            for (int i = 0; i < invalid.Length; i++) sanitized = sanitized.Replace(invalid[i].ToString(), string.Empty);
            return sanitized;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
                // Best-effort cleanup only; the primary save result remains authoritative.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup only; the primary save result remains authoritative.
            }
        }
    }
}
