using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Versioned, integrity-checked persistence envelope for character snapshots.
    /// The envelope is storage-format metadata only; the authoritative server remains
    /// the source of truth for live MMO state.
    /// </summary>
    [Serializable]
    public sealed class EROCharacterPersistenceEnvelope
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public string characterId;
        public string payloadJson;
        public string payloadSha256;

        public static EROCharacterPersistenceEnvelope Create(string characterId, string payloadJson)
        {
            if (string.IsNullOrWhiteSpace(characterId)) throw new ArgumentException("Character id is required.", nameof(characterId));
            if (payloadJson == null) throw new ArgumentNullException(nameof(payloadJson));

            return new EROCharacterPersistenceEnvelope
            {
                schemaVersion = CurrentSchemaVersion,
                characterId = characterId,
                payloadJson = payloadJson,
                payloadSha256 = ComputeSha256(payloadJson)
            };
        }

        public bool TryGetPayload(string expectedCharacterId, out string payload)
        {
            payload = null;
            if (schemaVersion != CurrentSchemaVersion) return false;
            if (string.IsNullOrWhiteSpace(expectedCharacterId) || !string.Equals(characterId, expectedCharacterId, StringComparison.Ordinal)) return false;
            if (string.IsNullOrEmpty(payloadJson) || string.IsNullOrEmpty(payloadSha256)) return false;
            if (!string.Equals(payloadSha256, ComputeSha256(payloadJson), StringComparison.OrdinalIgnoreCase)) return false;

            payload = payloadJson;
            return true;
        }

        public static bool TryDeserialize(string json, string expectedCharacterId, out string payload)
        {
            payload = null;
            if (string.IsNullOrWhiteSpace(json)) return false;

            try
            {
                EROCharacterPersistenceEnvelope envelope = JsonUtility.FromJson<EROCharacterPersistenceEnvelope>(json);
                return envelope != null && envelope.TryGetPayload(expectedCharacterId, out payload);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static string ComputeSha256(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) builder.Append(hash[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
