using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8Guild
    {
        public string guildId;
        public string name;
        public string leaderId;
        public readonly List<string> members = new();
        public readonly List<string> officers = new();

        public bool AddMember(string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || members.Contains(playerId)) return false;
            members.Add(playerId);
            return true;
        }

        public bool RemoveMember(string playerId) => members.Remove(playerId);
    }

    public interface IEROV8GuildService
    {
        bool CreateGuild(string leaderId, string name, out EROV8Guild guild);
        bool AddMember(EROV8Guild guild, string playerId);
        bool RemoveMember(EROV8Guild guild, string playerId);
    }
}
