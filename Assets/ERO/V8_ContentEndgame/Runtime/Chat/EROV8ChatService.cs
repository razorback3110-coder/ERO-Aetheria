using System;

namespace EternalRealmsOnline.V8
{
    public enum EROV8ChatChannel
    {
        Global,
        Zone,
        Party,
        Guild,
        Whisper,
        System
    }

    [Serializable]
    public struct EROV8ChatMessage
    {
        public string senderId;
        public string receiverId;
        public string text;
        public EROV8ChatChannel channel;
        public long unixTime;
    }

    public static class EROV8ChatRules
    {
        public const int MaxMessageLength = 300;

        public static bool Validate(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Length <= MaxMessageLength;
        }
    }
}
