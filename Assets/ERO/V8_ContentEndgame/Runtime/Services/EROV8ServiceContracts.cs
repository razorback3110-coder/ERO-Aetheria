using System;

namespace EternalRealmsOnline.V8
{
    public interface IEROV8MailService
    {
        bool Send(string senderId, string receiverId, string subject, string body);
    }

    public interface IEROV8TradeService
    {
        bool ValidateTrade(string playerA, string playerB);
    }

    public interface IEROV8RankingService
    {
        void SubmitScore(string playerId, string category, long score);
    }

    public interface IEROV8EventService
    {
        bool IsActive(string eventId);
    }

    [Serializable]
    public sealed class EROV8PlayerSocialSnapshot
    {
        public string playerId;
        public string guildId;
        public string partyId;
        public string zoneId;
        public int level;
    }
}
