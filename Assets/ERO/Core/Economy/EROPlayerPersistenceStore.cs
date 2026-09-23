using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Crash-safe durable storage primitive for authoritative player snapshots.
    /// The coordinator owns snapshot validation; this store owns atomic file replacement,
    /// bounded storage, integrity verification and last-known-good recovery.
    /// </summary>
    public sealed class EROPlayerPersistenceStore
    {
        public const int CurrentFormatVersion = 1;
        public const int MaxPayloadBytes = 8 * 1024 * 1024;

        private const string FileExtension = ".ero.save";
        private const string BackupExtension = ".bak";
        private const string Magic = "ERO_PLAYER_SAVE";

        private readonly string rootDirectory;

        public EROPlayerPersistenceStore(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Root directory is required.", nameof(rootDirectory));
            this.rootDirectory = Path.GetFullPath(rootDirectory);
            Directory.CreateDirectory(this.rootDirectory);
        }

        public string GetPath(string actorId)
        {
            ValidateActorId(actorId);
            return Path.Combine(rootDirectory, actorId + FileExtension);
        }

        public string GetBackupPath(string actorId)
        {
            return GetPath(actorId) + BackupExtension;
        }

        /// <summary>
        /// Writes a fully serialized PlayerPersistenceSnapshot atomically.
        /// When replacing an existing save, the previous verified file is retained as a
        /// last-known-good backup so a truncated or externally damaged primary can recover.
        /// </summary>
        public void Save(string actorId, byte[] payload)
        {
            ValidateActorId(actorId);
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (payload.Length == 0) throw new InvalidOperationException("Persistence payload cannot be empty.");
            if (payload.Length > MaxPayloadBytes) throw new InvalidOperationException("Persistence payload exceeds the configured limit.");

            string path = GetPath(actorId);
            string backupPath = GetBackupPath(actorId);
            string tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            byte[] envelope = BuildEnvelope(actorId, payload);

            try
            {
                using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(envelope, 0, envelope.Length);
                    stream.Flush(true);
                }

                if (File.Exists(path))
                    File.Replace(tempPath, path, backupPath, true);
                else
                    File.Move(tempPath, path);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Loads and integrity-checks a serialized PlayerPersistenceSnapshot.
        /// If the primary save is damaged, the last-known-good backup is attempted before
        /// the corruption is surfaced to the caller. No partially parsed data is returned.
        /// </summary>
        public byte[] Load(string actorId)
        {
            ValidateActorId(actorId);
            string path = GetPath(actorId);
            if (!File.Exists(path))
                return TryLoadBackup(actorId, null);

            byte[] envelope = File.ReadAllBytes(path);
            try
            {
                return ParseEnvelope(actorId, envelope);
            }
            catch (InvalidDataException primaryError)
            {
                return TryLoadBackup(actorId, primaryError);
            }
        }

        public bool Exists(string actorId)
        {
            return File.Exists(GetPath(actorId)) || File.Exists(GetBackupPath(actorId));
        }

        public bool Delete(string actorId)
        {
            string path = GetPath(actorId);
            string backupPath = GetBackupPath(actorId);
            bool deleted = false;
            if (File.Exists(path))
            {
                File.Delete(path);
                deleted = true;
            }
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                deleted = true;
            }
            return deleted;
        }

        private byte[] TryLoadBackup(string actorId, InvalidDataException primaryError)
        {
            string backupPath = GetBackupPath(actorId);
            if (!File.Exists(backupPath))
            {
                if (primaryError != null) throw primaryError;
                return null;
            }

            try
            {
                return ParseEnvelope(actorId, File.ReadAllBytes(backupPath));
            }
            catch (InvalidDataException backupError)
            {
                if (primaryError != null)
                    throw new InvalidDataException("Both primary and backup player saves are corrupt.", backupError);
                throw;
            }
        }

        private static byte[] BuildEnvelope(string actorId, byte[] payload)
        {
            string checksum = ComputeSha256(payload);
            string header = Magic + "\n" + CurrentFormatVersion + "\n" + actorId + "\n" + payload.Length + "\n" + checksum + "\n\n";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] envelope = new byte[headerBytes.Length + payload.Length];
            Buffer.BlockCopy(headerBytes, 0, envelope, 0, headerBytes.Length);
            Buffer.BlockCopy(payload, 0, envelope, headerBytes.Length, payload.Length);
            return envelope;
        }

        private static byte[] ParseEnvelope(string actorId, byte[] envelope)
        {
            if (envelope == null || envelope.Length == 0)
                throw new InvalidDataException("Player save is empty.");

            int headerEnd = FindHeaderEnd(envelope);
            if (headerEnd <= 0 || headerEnd > 4096)
                throw new InvalidDataException("Player save header is invalid.");

            string header = Encoding.UTF8.GetString(envelope, 0, headerEnd - 1);
            string[] lines = header.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length != 5)
                throw new InvalidDataException("Player save header is malformed.");

            if (!string.Equals(lines[0], Magic, StringComparison.Ordinal))
                throw new InvalidDataException("Player save magic is invalid.");
            if (!int.TryParse(lines[1], out int version) || version != CurrentFormatVersion)
                throw new InvalidDataException("Unsupported player save format version.");
            if (!string.Equals(lines[2], actorId, StringComparison.Ordinal))
                throw new InvalidDataException("Player save actor mismatch.");
            if (!int.TryParse(lines[3], out int payloadLength) || payloadLength <= 0 || payloadLength > MaxPayloadBytes)
                throw new InvalidDataException("Player save payload length is invalid.");
            if (lines[4].Length != 64)
                throw new InvalidDataException("Player save checksum is invalid.");

            int actualLength = envelope.Length - headerEnd;
            if (actualLength != payloadLength)
                throw new InvalidDataException("Player save payload length mismatch.");

            byte[] payload = new byte[payloadLength];
            Buffer.BlockCopy(envelope, headerEnd, payload, 0, payloadLength);
            string actualChecksum = ComputeSha256(payload);
            if (!string.Equals(lines[4], actualChecksum, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Player save integrity check failed.");
            return payload;
        }

        private static int FindHeaderEnd(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == (byte)'\n' && bytes[i + 1] == (byte)'\n')
                    return i + 2;
            }
            return -1;
        }

        private static string ComputeSha256(byte[] payload)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(payload);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash)
                    builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }

        private static void ValidateActorId(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId))
                throw new ArgumentException("ActorId is required.", nameof(actorId));
            if (actorId.Length > 128)
                throw new ArgumentException("ActorId is too long.", nameof(actorId));
            if (actorId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                actorId.IndexOf('/') >= 0 || actorId.IndexOf('\\') >= 0 ||
                string.Equals(actorId, ".", StringComparison.Ordinal) ||
                string.Equals(actorId, "..", StringComparison.Ordinal))
                throw new ArgumentException("ActorId contains invalid path characters.", nameof(actorId));
        }
    }
}
