using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8QuestDefinition
    {
        public string id;
        public string title;
        public string description;
        public int requiredLevel;
        public int experienceReward;
        public int goldReward;
        public readonly List<string> objectiveIds = new();
    }

    [Serializable]
    public sealed class EROV8QuestProgress
    {
        public string questId;
        public readonly Dictionary<string, int> progress = new();
        public bool completed;
    }
}
