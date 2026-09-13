using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V540
{
    [Serializable] public sealed class EROSeasonState { public int season=1; public int level=1; public int xp; public int premiumTrack; public bool claimedFree; public bool claimedPremium; }
    [Serializable] public sealed class EROAchievementV540 { public string id; public string title; public string description; public int progress; public int target; public int rewardGold; public bool claimed; }
    [Serializable] public sealed class EROCollectionEntry { public string id; public string category; public string title; public bool discovered; public int count; }
    [Serializable] public sealed class ERODungeonProgress { public string id; public int difficulty=1; public int clears; public float bestTime; }
    [Serializable] public sealed class EROGuildProgress { public string guildId; public int level=1; public int xp; public int research; public int storageSlots=100; public int territory; }
    [Serializable] public sealed class EROBattlePassReward { public int level; public string rewardId; public int quantity=1; public bool premium; }

    /// <summary>Offline-first progression/content state. The server implementation can replace persistence without changing UI contracts.</summary>
    public sealed class EROProgressionAndContentV540 : MonoBehaviour
    {
        public static EROProgressionAndContentV540 Instance { get; private set; }
        public EROSeasonState Season { get; private set; } = new();
        public readonly List<EROAchievementV540> Achievements = new();
        public readonly List<EROCollectionEntry> Collections = new();
        public readonly List<ERODungeonProgress> Dungeons = new();
        public readonly List<EROGuildProgress> Guilds = new();
        public readonly List<EROBattlePassReward> BattlePass = new();
        public event Action<string> OnReward;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Instance!=null)return;var g=new GameObject("ERO_Progression_V540");DontDestroyOnLoad(g);Instance=g.AddComponent<EROProgressionAndContentV540>();}
        void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;Seed();}
        void Seed(){if(Achievements.Count>0)return;
            Achievements.Add(new EROAchievementV540{id="first_blood",title="First Blood",description="Defeat your first enemy",target=1,rewardGold=100});
            Achievements.Add(new EROAchievementV540{id="rift_walker",title="Rift Walker",description="Discover five Rift points",target=5,rewardGold=500});
            Achievements.Add(new EROAchievementV540{id="level_50",title="Aether Veteran",description="Reach level 50",target=50,rewardGold=2500});
            Achievements.Add(new EROAchievementV540{id="level_100",title="Awakened",description="Reach level 100",target=100,rewardGold=10000});
            foreach(var s in new[]{"Monsters","Equipment","Zones","Bosses","Lore","Mounts","Pets"})Collections.Add(new EROCollectionEntry{id=s.ToLowerInvariant(),category=s,title=s});
            foreach(var x in new[]{"Ancient Ruins","Frost Temple","Sunscar Tomb","Abyssal Fortress","Eternal Rift Raid"})Dungeons.Add(new ERODungeonProgress{id=x});
            for(int i=1;i<=100;i++)BattlePass.Add(new EROBattlePassReward{level=i,rewardId=i%5==0?"cosmetic_chest":"gold",quantity=i%5==0?1:100*i,premium=i%10==0});
        }
        public void AddSeasonXp(int amount){Season.xp=Mathf.Max(0,Season.xp+amount);while(Season.level<100&&Season.xp>=Season.level*500){Season.xp-=Season.level*500;Season.level++;OnReward?.Invoke("Season Level "+Season.level);} }
        public void Progress(string achievementId,int amount){var a=Achievements.Find(x=>x.id==achievementId);if(a==null||a.claimed)return;a.progress=Mathf.Clamp(a.progress+amount,0,a.target);}
        public bool ClaimAchievement(string achievementId){var a=Achievements.Find(x=>x.id==achievementId);if(a==null||a.claimed||a.progress<a.target)return false;a.claimed=true;OnReward?.Invoke(a.title+" reward: "+a.rewardGold+" Gold");return true;}
        public void Discover(string id){var c=Collections.Find(x=>x.id==id);if(c!=null)c.discovered=true;}
    }
}
