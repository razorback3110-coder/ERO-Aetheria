using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.V8
{
    [Serializable]
    public sealed class EROV8Party
    {
        public string partyId;
        public string leaderId;
        public readonly List<string> members = new();
        public const int MaxMembers = 5;

        public bool Add(string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || members.Contains(playerId) || members.Count >= MaxMembers)
                return false;
            members.Add(playerId);
            return true;
        }

        public bool Remove(string playerId)
        {
            return members.Remove(playerId);
        }

        public bool Contains(string playerId) => members.Contains(playerId);
    }

    public interface IEROV8PartyService
    {
        bool Create(string leaderId, out EROV8Party party);
        bool Invite(EROV8Party party, string playerId);
        bool Leave(EROV8Party party, string playerId);
    }
}
