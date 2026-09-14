using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    [Serializable]
    public sealed class EROPlayerSession
    {
        public string sessionId;
        public string characterId;
        public bool authenticated;
        public bool connected;
        public long connectedAtTick;
        public long lastInputTick;
        public int zoneId;
    }

    /// <summary>Transport-agnostic authoritative session registry. Network adapters may bind to it without moving game authority to clients.</summary>
    public sealed class ERONetworkSessionSystem
    {
        private readonly Dictionary<string, EROPlayerSession> sessions = new Dictionary<string, EROPlayerSession>(StringComparer.Ordinal);
        private const int MaxInputLeadTicks = 3;

        public IReadOnlyCollection<EROPlayerSession> Sessions => sessions.Values;

        public bool TryConnect(string sessionId, string characterId, long serverTick, out EROPlayerSession session)
        {
            session = null;
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(characterId) || sessions.ContainsKey(sessionId)) return false;
            session = new EROPlayerSession { sessionId = sessionId, characterId = characterId, authenticated = true, connected = true, connectedAtTick = Math.Max(0, serverTick), lastInputTick = Math.Max(0, serverTick) - 1 };
            sessions.Add(sessionId, session);
            return true;
        }

        public bool TryAcceptInput(string sessionId, long inputTick, long serverTick)
        {
            EROPlayerSession session;
            if (!sessions.TryGetValue(sessionId, out session) || !session.connected || !session.authenticated) return false;
            if (inputTick < session.lastInputTick || inputTick > serverTick + MaxInputLeadTicks) return false;
            session.lastInputTick = inputTick;
            return true;
        }

        public bool TrySetZone(string sessionId, int zoneId)
        {
            EROPlayerSession session;
            if (!sessions.TryGetValue(sessionId, out session) || !session.connected || zoneId < 0) return false;
            session.zoneId = zoneId;
            return true;
        }

        public bool Disconnect(string sessionId)
        {
            EROPlayerSession session;
            if (!sessions.TryGetValue(sessionId, out session)) return false;
            session.connected = false;
            sessions.Remove(sessionId);
            return true;
        }

        public bool IsOwnedBySession(string sessionId, CharacterData character)
        {
            EROPlayerSession session;
            return character != null && sessions.TryGetValue(sessionId, out session) && session.connected && session.authenticated && session.characterId == character.id;
        }
    }
}
