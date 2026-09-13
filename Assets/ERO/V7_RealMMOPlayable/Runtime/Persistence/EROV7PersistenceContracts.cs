using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.V7
{
    [Serializable]
    public sealed class EROV7CharacterSave
    {
        public string accountId;
        public string characterId;
        public string characterName;
        public int level = 1;
        public int experience;
        public int gold;
        public int hp = 100;
        public int maxHp = 100;
        public string zoneId = "Greenhaven";
        public List<string> itemIds = new();
        public List<string> equipmentIds = new();
    }

    public interface IEROV7Persistence
    {
        void QueueSave(EROV7CharacterSave save);
        bool TryLoad(string accountId, string characterId, out EROV7CharacterSave save);
    }
}
