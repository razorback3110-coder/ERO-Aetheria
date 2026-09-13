using System;
using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7ZoneRouter : MonoBehaviour
    {
        [Serializable]
        public struct Zone
        {
            public string id;
            public int minLevel;
            public int maxLevel;
        }

        [SerializeField] private Zone[] zones =
        {
            new Zone { id="Greenhaven", minLevel=1, maxLevel=10 },
            new Zone { id="Everwood", minLevel=10, maxLevel=20 },
            new Zone { id="Elyndor", minLevel=20, maxLevel=30 },
            new Zone { id="Frostfall", minLevel=30, maxLevel=40 },
            new Zone { id="Sunscar", minLevel=40, maxLevel=50 },
            new Zone { id="Sylvaris", minLevel=50, maxLevel=60 },
            new Zone { id="Abyssia", minLevel=60, maxLevel=70 },
            new Zone { id="Rift", minLevel=70, maxLevel=80 },
            new Zone { id="Eternal Rift", minLevel=80, maxLevel=90 },
            new Zone { id="Endless Abyss", minLevel=90, maxLevel=100 }
        };

        public bool TryGetZone(int level, out string zoneId)
        {
            foreach (var zone in zones)
            {
                if (level >= zone.minLevel && level <= zone.maxLevel)
                {
                    zoneId = zone.id;
                    return true;
                }
            }
            zoneId = "Greenhaven";
            return false;
        }
    }
}
