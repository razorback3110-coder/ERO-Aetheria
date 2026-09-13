using System;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8WorldBossWindow
    {
        public string bossId;
        public string zoneId;
        public long startUnix;
        public long endUnix;
        public bool active;
    }

    public static class EROV8WorldBossRules
    {
        public static bool CanJoin(EROV8WorldBossWindow window, int playerLevel)
        {
            return window != null && window.active && playerLevel >= 60;
        }
    }
}
