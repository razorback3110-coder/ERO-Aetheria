using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    /// <summary>Versioned persistence DTO for authoritative character progression.</summary>
    public readonly struct EROCharacterProgressionSnapshot
    {
        public const int CurrentVersion = 1;
        public readonly int Version;
        public readonly ulong ActorId;
        public readonly int Level;
        public readonly long TotalExperience;
        public readonly int MaxLevel;

        public EROCharacterProgressionSnapshot(ulong actorId, EROCharacterProgression progression)
        {
            if (actorId == 0UL) throw new ArgumentOutOfRangeException(nameof(actorId));
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            Version = CurrentVersion;
            ActorId = actorId;
            Level = progression.Level;
            TotalExperience = progression.TotalExperience;
            MaxLevel = progression.MaxLevel;
        }
    }

    /// <summary>Persistence boundary for database, file, or service adapters.</summary>
    public interface IEROCharacterProgressionStore
    {
        bool TryLoad(ulong actorId, out EROCharacterProgressionSnapshot snapshot);
        void Save(EROCharacterProgressionSnapshot snapshot);
        void Delete(ulong actorId);
    }

    /// <summary>In-process implementation for tests and local/dedicated-server bootstrap.</summary>
    public sealed class EROInMemoryCharacterProgressionStore : IEROCharacterProgressionStore
    {
        private readonly Dictionary<ulong, EROCharacterProgressionSnapshot> snapshots;

        public EROInMemoryCharacterProgressionStore(int expectedCapacity = 256)
        {
            if (expectedCapacity < 1) throw new ArgumentOutOfRangeException(nameof(expectedCapacity));
            snapshots = new Dictionary<ulong, EROCharacterProgressionSnapshot>(expectedCapacity);
        }

        public int Count => snapshots.Count;

        public bool TryLoad(ulong actorId, out EROCharacterProgressionSnapshot snapshot)
            => snapshots.TryGetValue(actorId, out snapshot);

        public void Save(EROCharacterProgressionSnapshot snapshot)
        {
            Validate(snapshot);
            snapshots[snapshot.ActorId] = snapshot;
        }

        public void Delete(ulong actorId) => snapshots.Remove(actorId);

        private static void Validate(EROCharacterProgressionSnapshot snapshot)
        {
            if (snapshot.Version != EROCharacterProgressionSnapshot.CurrentVersion)
                throw new InvalidOperationException("Unsupported ERO progression snapshot version.");
            if (snapshot.ActorId == 0UL) throw new ArgumentOutOfRangeException(nameof(snapshot));
            if (snapshot.MaxLevel < 1 || snapshot.Level < 1 || snapshot.Level > snapshot.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(snapshot));
            if (snapshot.TotalExperience < 0) throw new ArgumentOutOfRangeException(nameof(snapshot));
        }
    }
}
