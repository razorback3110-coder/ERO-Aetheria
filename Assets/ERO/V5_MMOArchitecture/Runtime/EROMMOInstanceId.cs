using System;
using UnityEngine;

namespace EternalRealmsOnline.V5
{
    [Serializable]
    public readonly struct EROMMOInstanceId : IEquatable<EROMMOInstanceId>
    {
        public readonly string world;
        public readonly int instance;

        public EROMMOInstanceId(string world, int instance)
        {
            this.world = world ?? string.Empty;
            this.instance = instance;
        }

        public bool Equals(EROMMOInstanceId other) =>
            instance == other.instance &&
            string.Equals(world, other.world, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is EROMMOInstanceId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(world, instance);

        public override string ToString() => $"{world}:{instance}";
    }
}
