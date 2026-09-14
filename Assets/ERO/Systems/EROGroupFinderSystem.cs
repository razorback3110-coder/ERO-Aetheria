using System;
using System.Collections.Generic;
using ERO.Data;

namespace ERO.Systems
{
    public enum EROGroupActivity
    {
        OpenWorld, Questing, PublicEvent, DynamicEvent, Dungeon, Raid, MonthlyTower, MVP, PvP, Arena, GvG, GuildMission, WorldBoss, Farming, Crafting
    }

    [Serializable]
    public sealed class EROGroupFinderRequest
    {
        public string requestId;
        public string leaderId;
        public string leaderName;
        public EROGroupActivity activity;
        public int minLevel = 1;
        public int maxLevel = 250;
        public int desiredSize = 5;
        public bool crossServer;
        public bool autoAccept;
        public bool voicePreferred;
        public Language language = Language.English;
        public ClassRole[] preferredRoles = Array.Empty<ClassRole>();
        public string zoneId;
        public string contentId;
        public long createdUnixTime;
    }

    [Serializable]
    public sealed class EROGroupMemberSearchProfile
    {
        public string playerId;
        public string playerName;
        public EROClass classId;
        public ClassRole role;
        public int level = 1;
        public Language language = Language.English;
        public bool available = true;
        public bool crossServerEnabled = true;
        public string zoneId;
    }

    [Serializable]
    public sealed class EROGroupFinderMatch
    {
        public string requestId;
        public string playerId;
        public int score;
        public bool compatible;
        public string reason;
    }

    public static class EROGroupFinderSystem
    {
        public const int MaximumGroupSize = 10;
        public const int MaximumSearchResults = 50;
        public const int DefaultSearchRadiusLevels = 20;
        public const int DefaultOpenWorldGroupSize = 5;

        public static bool ValidateRequest(EROGroupFinderRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.requestId) || string.IsNullOrEmpty(request.leaderId)) return false;
            if (request.minLevel < 1 || request.maxLevel < request.minLevel || request.maxLevel > 250) return false;
            if (request.desiredSize < 2 || request.desiredSize > MaximumGroupSize) return false;
            return true;
        }

        public static EROGroupFinderRequest CreateOpenWorldRequest(string playerId, string playerName, int level, string zoneId, Language language)
        {
            return new EROGroupFinderRequest
            {
                requestId = Guid.NewGuid().ToString("N"), leaderId = playerId, leaderName = playerName,
                activity = EROGroupActivity.OpenWorld, minLevel = Math.Max(1, level - DefaultSearchRadiusLevels),
                maxLevel = Math.Min(250, level + DefaultSearchRadiusLevels), desiredSize = DefaultOpenWorldGroupSize,
                language = language, zoneId = zoneId, createdUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public static bool IsCompatible(EROGroupFinderRequest request, EROGroupMemberSearchProfile player)
        {
            if (request == null || player == null || !player.available || player.level < request.minLevel || player.level > request.maxLevel) return false;
            if (request.crossServer && !player.crossServerEnabled) return false;
            if (!string.IsNullOrEmpty(request.zoneId) && !string.IsNullOrEmpty(player.zoneId) && request.activity == EROGroupActivity.OpenWorld && request.zoneId != player.zoneId) return false;
            if (request.language != player.language) return false;
            if (request.preferredRoles != null && request.preferredRoles.Length > 0)
            {
                bool roleFound = false;
                foreach (var role in request.preferredRoles) if (role == player.role) { roleFound = true; break; }
                if (!roleFound) return false;
            }
            return true;
        }

        public static IReadOnlyList<EROGroupFinderMatch> Search(EROGroupFinderRequest request, IEnumerable<EROGroupMemberSearchProfile> players)
        {
            var results = new List<EROGroupFinderMatch>();
            if (!ValidateRequest(request) || players == null) return results;
            foreach (var player in players)
            {
                if (player == null || player.playerId == request.leaderId) continue;
                bool compatible = IsCompatible(request, player);
                if (!compatible) continue;
                int score = Score(request, player);
                results.Add(new EROGroupFinderMatch { requestId = request.requestId, playerId = player.playerId, score = score, compatible = true, reason = BuildReason(request, player) });
            }
            results.Sort((a, b) => b.score.CompareTo(a.score));
            if (results.Count > MaximumSearchResults) results.RemoveRange(MaximumSearchResults, results.Count - MaximumSearchResults);
            return results;
        }

        public static IReadOnlyList<EROGroupActivity> GetAllActivities() => new[]
        {
            EROGroupActivity.OpenWorld, EROGroupActivity.Questing, EROGroupActivity.PublicEvent, EROGroupActivity.DynamicEvent,
            EROGroupActivity.Dungeon, EROGroupActivity.Raid, EROGroupActivity.MonthlyTower, EROGroupActivity.MVP,
            EROGroupActivity.PvP, EROGroupActivity.Arena, EROGroupActivity.GvG, EROGroupActivity.GuildMission,
            EROGroupActivity.WorldBoss, EROGroupActivity.Farming, EROGroupActivity.Crafting
        };

        private static int Score(EROGroupFinderRequest request, EROGroupMemberSearchProfile player)
        {
            int score = 100;
            int midpoint = (request.minLevel + request.maxLevel) / 2;
            score -= Math.Min(30, Math.Abs(player.level - midpoint));
            if (request.language == player.language) score += 20;
            if (request.activity == EROGroupActivity.OpenWorld && request.zoneId == player.zoneId) score += 35;
            if (request.crossServer && player.crossServerEnabled) score += 5;
            if (request.preferredRoles != null && Array.IndexOf(request.preferredRoles, player.role) >= 0) score += 25;
            return Math.Max(0, score);
        }

        private static string BuildReason(EROGroupFinderRequest request, EROGroupMemberSearchProfile player)
        {
            if (request.activity == EROGroupActivity.OpenWorld && request.zoneId == player.zoneId) return "Same open-world zone";
            if (request.preferredRoles != null && Array.IndexOf(request.preferredRoles, player.role) >= 0) return "Preferred role";
            return "Level and language compatible";
        }
    }
}
