using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8DungeonDefinition
    {
        public string id;
        public string displayName;
        public int recommendedLevel;
        public int maxPlayers = 5;
        public int bossCount = 1;
        public int timeLimitSeconds;
        public readonly List<string> encounterIds = new();
    }

    public static class EROV8DungeonCatalog
    {
        public static readonly EROV8DungeonDefinition RiftTrial = new()
        {
            id = "rift_trial",
            displayName = "Rift Trial",
            recommendedLevel = 70,
            maxPlayers = 5,
            bossCount = 1,
            timeLimitSeconds = 1800
        };

        public static readonly EROV8DungeonDefinition EternalRift = new()
        {
            id = "eternal_rift",
            displayName = "Eternal Rift",
            recommendedLevel = 80,
            maxPlayers = 5,
            bossCount = 2,
            timeLimitSeconds = 2400
        };
    }
}
