using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Deterministic, server-side loot instance generator.
    /// The caller supplies an authoritative encounter seed and drop sequence;
    /// no client-side randomness or proprietary game content is involved.
    /// </summary>
    public static class EROLootGenerator
    {
        public static EROItemInstance CreateInstance(
            string encounterId,
            long encounterSeed,
            int dropIndex,
            string itemId,
            int quantity,
            int maxStack,
            int level,
            IReadOnlyDictionary<string, long> stats = null)
        {
            ValidateId(encounterId, nameof(encounterId));
            ValidateId(itemId, nameof(itemId));
            if (encounterSeed < 0) throw new ArgumentOutOfRangeException(nameof(encounterSeed));
            if (dropIndex < 0) throw new ArgumentOutOfRangeException(nameof(dropIndex));

            string instanceId = BuildInstanceId(encounterId, encounterSeed, dropIndex, itemId);
            return new EROItemInstance(instanceId, itemId, quantity, maxStack, level, stats);
        }

        public static string BuildInstanceId(string encounterId, long encounterSeed, int dropIndex, string itemId)
        {
            ValidateId(encounterId, nameof(encounterId));
            ValidateId(itemId, nameof(itemId));
            if (encounterSeed < 0) throw new ArgumentOutOfRangeException(nameof(encounterSeed));
            if (dropIndex < 0) throw new ArgumentOutOfRangeException(nameof(dropIndex));

            string canonical = encounterId + "|" + encounterSeed.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                               "|" + dropIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + itemId;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                var builder = new StringBuilder(64);
                foreach (byte value in digest) builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }
    }
}
