using System;
using UnityEngine;

namespace EternalRealmsOnline.V5
{
    [CreateAssetMenu(menuName = "ERO/V5/MMO World Shard Profile")]
    public sealed class EROMMOWorldShardProfile : ScriptableObject
    {
        [Serializable]
        public sealed class WorldInstance
        {
            public string id;
            public string zone;
            public int maxPlayers = 100;
            public bool privateInstance;
        }

        public WorldInstance[] instances;
    }
}
