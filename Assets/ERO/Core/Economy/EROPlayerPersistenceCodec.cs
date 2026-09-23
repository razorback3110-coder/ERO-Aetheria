using System;
using System.IO;
using System.Text;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Canonical boundary between the in-memory player snapshot and durable storage.
    /// The codec owns schema/version framing; the coordinator remains authoritative for
    /// validating and applying the decoded PlayerPersistenceSnapshot.
    /// </summary>
    public interface IEROPlayerPersistenceCodec
    {
        int SchemaVersion { get; }
        byte[] Encode(PlayerPersistenceSnapshot snapshot);
        PlayerPersistenceSnapshot Decode(byte[] payload);
    }

    /// <summary>
    /// Versioned binary framing codec for an already serialized player snapshot.
    /// A concrete snapshot serializer can be supplied without coupling the persistence
    /// store to Unity serialization or a third-party JSON package.
    /// </summary>
    public sealed class EROPlayerPersistenceCodec : IEROPlayerPersistenceCodec
    {
        private const int Magic = 0x45524F50; // "EROP"
        private const int HeaderBytes = 16;

        private readonly Func<PlayerPersistenceSnapshot, byte[]> serializer;
        private readonly Func<byte[], PlayerPersistenceSnapshot> deserializer;

        public EROPlayerPersistenceCodec(
            int schemaVersion,
            Func<PlayerPersistenceSnapshot, byte[]> serializer,
            Func<byte[], PlayerPersistenceSnapshot> deserializer)
        {
            if (schemaVersion <= 0)
                throw new ArgumentOutOfRangeException(nameof(schemaVersion));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            SchemaVersion = schemaVersion;
        }

        public int SchemaVersion { get; }

        public byte[] Encode(PlayerPersistenceSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != EROPlayerPersistenceCoordinator.CurrentSnapshotVersion)
                throw new InvalidOperationException("Snapshot version is not supported by the current coordinator.");

            byte[] body = serializer(snapshot);
            if (body == null || body.Length == 0)
                throw new InvalidOperationException("Serialized player snapshot cannot be empty.");
            if (body.Length > EROPlayerPersistenceStore.MaxPayloadBytes - HeaderBytes)
                throw new InvalidOperationException("Serialized player snapshot exceeds the configured persistence limit.");

            byte[] payload = new byte[HeaderBytes + body.Length];
            using (var stream = new MemoryStream(payload, true))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write(SchemaVersion);
                writer.Write(snapshot.Version);
                writer.Write(body.Length);
                writer.Write(body);
            }
            return payload;
        }

        public PlayerPersistenceSnapshot Decode(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (payload.Length < HeaderBytes)
                throw new InvalidDataException("Player snapshot codec payload is truncated.");

            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (reader.ReadInt32() != Magic)
                    throw new InvalidDataException("Player snapshot codec magic is invalid.");
                if (reader.ReadInt32() != SchemaVersion)
                    throw new InvalidDataException("Unsupported player snapshot schema version.");

                int snapshotVersion = reader.ReadInt32();
                if (snapshotVersion != EROPlayerPersistenceCoordinator.CurrentSnapshotVersion)
                    throw new InvalidDataException("Unsupported player persistence snapshot version.");

                int bodyLength = reader.ReadInt32();
                if (bodyLength <= 0 || bodyLength != payload.Length - HeaderBytes)
                    throw new InvalidDataException("Player snapshot codec body length is invalid.");

                byte[] body = reader.ReadBytes(bodyLength);
                if (body.Length != bodyLength || stream.Position != stream.Length)
                    throw new InvalidDataException("Player snapshot codec body is truncated or has trailing data.");

                PlayerPersistenceSnapshot snapshot = deserializer(body);
                if (snapshot == null)
                    throw new InvalidDataException("Player snapshot deserializer returned null.");
                if (snapshot.Version != snapshotVersion)
                    throw new InvalidDataException("Decoded player snapshot version mismatch.");
                return snapshot;
            }
        }
    }
}
