using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.V540
{
    public enum EROFeatureCategory { Character, World, Combat, Social, Economy, Collection, Competitive, Content, System }

    [Serializable] public sealed class EROFeatureDefinition
    {
        public string id; public string title; public EROFeatureCategory category; public string description; public string shortcut; public bool enabled = true;
    }

    [Serializable] public sealed class EROActivityDefinition
    {
        public string id; public string name; public string type; public int recommendedLevel; public int maxPlayers; public string scene; public string loadingTitle;
    }

    /// <summary>Central feature contract for the complete ERO client. UI and gameplay systems can bind to this hub without depending on Boss Room presentation.</summary>
    [DefaultExecutionOrder(-9000)]
    public sealed class EROFeatureHubV540 : MonoBehaviour
    {
        public static EROFeatureHubV540 Instance { get; private set; }
        public readonly List<EROFeatureDefinition> Features = new();
        public readonly List<EROActivityDefinition> Activities = new();
        public string CurrentFeature { get; private set; } = "HUD";
        public string CurrentActivity { get; private set; } = "Open World";
        public bool InQueue { get; private set; }
        public event Action<string> FeatureOpened;
        public event Action<string> ActivityStarted;
        public event Action<string> Notification;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("ERO_FeatureHub_V540");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<EROFeatureHubV540>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildFeatureCatalog();
            BuildActivityCatalog();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        void OnSceneLoaded(Scene s, LoadSceneMode m) { InQueue = false; }

        void BuildFeatureCatalog()
        {
            if (Features.Count > 0) return;
            Add("character", "Character", EROFeatureCategory.Character, "Character profile, appearance, class evolution and Awakening", "C");
            Add("inventory", "Inventory", EROFeatureCategory.Economy, "Items, consumables, sorting, filtering and equipment", "I");
            Add("equipment", "Equipment", EROFeatureCategory.Character, "Gear slots, set bonuses, refinement and transmog", "P");
            Add("skills", "Skills", EROFeatureCategory.Combat, "Class skills, passive skills, evolution and Awakening", "K");
            Add("quests", "Quests", EROFeatureCategory.Content, "Main, side, daily, weekly and faction quests", "Q");
            Add("map", "Aetheria Map", EROFeatureCategory.World, "World map, zones, waypoints, dungeons and activities", "M");
            Add("party", "Party", EROFeatureCategory.Social, "Party, roles, loot rules and ready check", "G");
            Add("guild", "Guild", EROFeatureCategory.Social, "Guild roster, ranks, research, storage and GvG", "H");
            Add("social", "Social", EROFeatureCategory.Social, "Friends, block list, whispers and presence", "O");
            Add("chat", "Chat", EROFeatureCategory.Social, "World, party, guild, whisper and system channels", "Enter");
            Add("pets", "Pets", EROFeatureCategory.Collection, "Companions, growth, skills and appearances", "J");
            Add("mounts", "Mounts", EROFeatureCategory.Collection, "Mount collection, summon and progression", "V");
            Add("crafting", "Crafting", EROFeatureCategory.Economy, "Gathering, recipes, crafting levels and materials", "N");
            Add("market", "Marketplace", EROFeatureCategory.Economy, "Listings, search, buy/sell and taxes", "");
            Add("trade", "Trade", EROFeatureCategory.Economy, "Secure player-to-player trading", "");
            Add("mail", "Mail", EROFeatureCategory.Economy, "Player mail, attachments and notifications", "");
            Add("housing", "Housing", EROFeatureCategory.Collection, "Personal estate, furniture and decoration", "");
            Add("codex", "Codex", EROFeatureCategory.Collection, "Monsters, items, zones, lore and collections", "");
            Add("achievements", "Achievements", EROFeatureCategory.Collection, "Milestones, titles and rewards", "");
            Add("rankings", "Rankings", EROFeatureCategory.Competitive, "PvE, PvP, GvG, collection and seasonal rankings", "");
            Add("events", "Events", EROFeatureCategory.Content, "Limited-time events, activities and rewards", "");
            Add("season", "Season", EROFeatureCategory.Content, "Season progression, missions and rewards", "");
            Add("pvp", "PvP", EROFeatureCategory.Competitive, "Arena, ranked, duels and battlegrounds", "");
            Add("gvg", "GvG", EROFeatureCategory.Competitive, "Guild wars, objectives and territory", "");
            Add("dungeon", "Dungeon", EROFeatureCategory.Content, "Matchmaking, boss encounters and loot", "");
            Add("raid", "Raid", EROFeatureCategory.Content, "Multi-boss raid encounters and difficulty", "");
            Add("mvp", "MVP", EROFeatureCategory.Content, "MVP hunt, contribution and rewards", "");
            Add("worldboss", "World Boss", EROFeatureCategory.Content, "Open-world boss with contribution rewards", "");
            Add("crystals", "Cristaux ERO", EROFeatureCategory.Economy, "Premium currency, shop and cosmetic catalogue", "");
            Add("settings", "Settings", EROFeatureCategory.System, "Graphics, audio, controls, accessibility and language", "Esc");
        }
        void Add(string id,string title,EROFeatureCategory cat,string desc,string shortcut)=>Features.Add(new EROFeatureDefinition{id=id,title=title,category=cat,description=desc,shortcut=shortcut});

        void BuildActivityCatalog()
        {
            if (Activities.Count > 0) return;
            AddActivity("greenhaven","Greenhaven","Open World",1,50,"ERO_Playable","ENTERING GREENHAVEN");
            AddActivity("everwood","Everwood","Open World",10,50,"ERO_Playable","ENTERING EVERWOOD");
            AddActivity("elyndor","Elyndor","Open World",20,50,"ERO_Playable","ENTERING ELYNDOR");
            AddActivity("frostfall","Frostfall","Open World",30,50,"ERO_Playable","ENTERING FROSTFALL");
            AddActivity("sunscar","Sunscar Desert","Open World",40,50,"ERO_Playable","ENTERING SUNSCAR");
            AddActivity("sylvaris","Sylvaris","Open World",50,50,"ERO_Playable","ENTERING SYLVARIS");
            AddActivity("abyssia","Abyssia","Open World",60,50,"ERO_Playable","ENTERING ABYSSIA");
            AddActivity("rift","The Rift","Open World",70,50,"ERO_Playable","ENTERING THE RIFT");
            AddActivity("eternal_rift","Eternal Rift","Open World",80,50,"ERO_Playable","ENTERING THE ETERNAL RIFT");
            AddActivity("endless_abyss","Endless Abyss","Open World",90,50,"ERO_Playable","ENTERING ENDLESS ABYSS");
            AddActivity("dungeon_ancient_ruins","Ancient Ruins","Dungeon",20,5,"ERO_Playable","MATCHING: ANCIENT RUINS");
            AddActivity("dungeon_frost_temple","Frost Temple","Dungeon",35,5,"ERO_Playable","MATCHING: FROST TEMPLE");
            AddActivity("dungeon_sunscar_tomb","Sunscar Tomb","Dungeon",45,5,"ERO_Playable","MATCHING: SUNSCAR TOMB");
            AddActivity("raid_abyssal_fortress","Abyssal Fortress","Raid",65,10,"ERO_Playable","ENTERING ABYSSAL FORTRESS");
            AddActivity("raid_eternal_rift","Eternal Rift Raid","Raid",80,10,"ERO_Playable","ENTERING ETERNAL RIFT RAID");
            AddActivity("mvp_ancient_dragon","Ancient Dragon","MVP",70,20,"ERO_Playable","MVP ENCOUNTER");
            AddActivity("worldboss_rift_guardian","Rift Guardian","World Boss",80,100,"ERO_Playable","WORLD BOSS INCOMING");
            AddActivity("arena","Arena","PvP",20,2,"ERO_Playable","ENTERING ARENA");
            AddActivity("ranked","Ranked Arena","Ranked",40,2,"ERO_Playable","ENTERING RANKED");
            AddActivity("gvg","Guild War","GvG",40,100,"ERO_Playable","GUILD WAR");
        }
        void AddActivity(string id,string name,string type,int lvl,int players,string scene,string loading)=>Activities.Add(new EROActivityDefinition{id=id,name=name,type=type,recommendedLevel=lvl,maxPlayers=players,scene=scene,loadingTitle=loading});

        public void Open(string featureId)
        {
            var f=Features.Find(x=>x.id.Equals(featureId,StringComparison.OrdinalIgnoreCase));
            if(f==null){NotifyUser("Feature unavailable: "+featureId);return;}
            CurrentFeature=f.id; FeatureOpened?.Invoke(f.id);
        }
        public bool StartActivity(string activityId)
        {
            var a=Activities.Find(x=>x.id.Equals(activityId,StringComparison.OrdinalIgnoreCase));
            if(a==null){NotifyUser("Activity unavailable");return false;}
            InQueue=true; CurrentActivity=a.id; ActivityStarted?.Invoke(a.id); NotifyUser(a.loadingTitle); return true;
        }
        public void CancelQueue(){InQueue=false;NotifyUser("Queue cancelled");}
        public void NotifyUser(string text){Notification?.Invoke(text);}
        public bool HasFeature(string id)=>Features.Exists(x=>x.id.Equals(id,StringComparison.OrdinalIgnoreCase)&&x.enabled);
        public EROFeatureDefinition GetFeature(string id)=>Features.Find(x=>x.id.Equals(id,StringComparison.OrdinalIgnoreCase));
        public EROActivityDefinition GetActivity(string id)=>Activities.Find(x=>x.id.Equals(id,StringComparison.OrdinalIgnoreCase));
    }
}
