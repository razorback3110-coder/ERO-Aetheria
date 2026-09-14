using System;
using System.Collections.Generic;

namespace ERO.Systems
{
    public enum EROGuildHouseRoomType { MainHall, Treasury, TrainingHall, CraftingHall, TrophyHall, StrategyRoom, Garden, Barracks }

    [Serializable]
    public sealed class EROGuildHouseRoom
    {
        public string id;
        public EROGuildHouseRoomType type;
        public int level = 1;
        public bool unlocked;
        public int capacity = 10;
        public int bonusBasisPoints;
    }

    [Serializable]
    public sealed class EROGuildHouseState
    {
        public string guildId;
        public string houseId;
        public string displayName = "Guild Hall";
        public int level = 1;
        public long experience;
        public int prestige;
        public bool publicAccess;
        public List<EROGuildHouseRoom> rooms = new List<EROGuildHouseRoom>();
        public List<string> unlockedDecorations = new List<string>();
        public List<string> placedTrophies = new List<string>();
    }

    public static class EROGuildHouseSystem
    {
        public const int MaximumHouseLevel = 30;
        public const int MaximumRooms = 50;

        public static EROGuildHouseState Create(string guildId, string houseId, string name = "Guild Hall")
        {
            var state = new EROGuildHouseState { guildId = guildId, houseId = houseId, displayName = string.IsNullOrEmpty(name) ? "Guild Hall" : name };
            UnlockRoom(state, EROGuildHouseRoomType.MainHall, 1, 50);
            UnlockRoom(state, EROGuildHouseRoomType.Barracks, 1, 30);
            return state;
        }

        public static bool CanEnter(EROGuildHouseState house, string guildId, bool isGuildMember)
        {
            if (house == null || string.IsNullOrEmpty(guildId) || house.guildId != guildId) return false;
            return isGuildMember || house.publicAccess;
        }

        public static bool TryUpgrade(EROGuildHouseState house, long experienceCost)
        {
            if (house == null || experienceCost <= 0 || house.level >= MaximumHouseLevel || house.experience < experienceCost) return false;
            house.experience -= experienceCost;
            house.level++;
            return true;
        }

        public static bool TryAddExperience(EROGuildHouseState house, long amount)
        {
            if (house == null || amount <= 0) return false;
            house.experience = checked(house.experience + amount);
            return true;
        }

        public static bool UnlockRoom(EROGuildHouseState house, EROGuildHouseRoomType type, int level, int capacity)
        {
            if (house == null || house.rooms == null || house.rooms.Count >= MaximumRooms || level > house.level || level < 1) return false;
            foreach (var room in house.rooms) if (room != null && room.type == type) return false;
            house.rooms.Add(new EROGuildHouseRoom { id = "guildhouse_" + type.ToString().ToLowerInvariant(), type = type, level = level, unlocked = true, capacity = Math.Max(1, capacity), bonusBasisPoints = GetDefaultBonus(type) });
            return true;
        }

        public static bool TryAddDecoration(EROGuildHouseState house, string decorationId)
        {
            if (house == null || house.unlockedDecorations == null || string.IsNullOrEmpty(decorationId) || house.unlockedDecorations.Contains(decorationId)) return false;
            house.unlockedDecorations.Add(decorationId);
            return true;
        }

        public static bool TryPlaceTrophy(EROGuildHouseState house, string trophyId)
        {
            if (house == null || house.placedTrophies == null || string.IsNullOrEmpty(trophyId) || house.placedTrophies.Contains(trophyId)) return false;
            house.placedTrophies.Add(trophyId);
            house.prestige++;
            return true;
        }

        private static int GetDefaultBonus(EROGuildHouseRoomType type)
        {
            switch (type)
            {
                case EROGuildHouseRoomType.TrainingHall: return 500;
                case EROGuildHouseRoomType.CraftingHall: return 400;
                case EROGuildHouseRoomType.Treasury: return 300;
                case EROGuildHouseRoomType.TrophyHall: return 250;
                case EROGuildHouseRoomType.StrategyRoom: return 500;
                case EROGuildHouseRoomType.Garden: return 200;
                case EROGuildHouseRoomType.Barracks: return 350;
                default: return 0;
            }
        }
    }
}
