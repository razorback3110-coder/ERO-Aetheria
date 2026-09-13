using System;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8SeasonDefinition
    {
        public string id;
        public string name;
        public long startUnix;
        public long endUnix;
        public int battlePassLevels = 100;
        public bool rankedPvP;
        public bool worldEvents;
    }
}
