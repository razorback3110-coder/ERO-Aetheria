using System;
using UnityEngine;

namespace EternalRealmsOnline.V508
{
    public enum EROContentSystem { Account, Character, Classes, Skills, Equipment, Inventory, Crafting, Loot, Economy, Crystals, Quests, World, Dungeons, Raids, Towers, Rift, MVP, WorldBoss, PvE, PvP, GvG, Guild, Social, Pets, Mounts, Housing, Collections, Events, Seasons, Achievements, Mail, Trading, Leaderboards, Security, Localization }

    [Serializable]
    public sealed class EROSystemStateV508
    {
        public bool founderModeEnabled = true;
        public bool paymentsEnabled = false;
        public string founderAccountId = "OWNER_ACCOUNT";
        public int founderCrystals = 10000;
        public int founderGold = 1000000;
        public int maxCharacterLevel = 75;
        public int baseClassCount = 8;
        public int evolutionLevel = 40;
        public int finalEvolutionLevel = 75;
        public string premiumCurrencyName = "Cristaux ERO";
        public string[] supportedLanguages = { "fr", "en", "de", "es", "it", "nl", "pt", "ja", "ko", "zh-CN" };
    }

    public sealed class EROWorldSystemsV508 : MonoBehaviour
    {
        public static EROWorldSystemsV508 Instance { get; private set; }
        public EROSystemStateV508 State { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("ERO_V508_WorldSystems");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<EROWorldSystemsV508>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            State = new EROSystemStateV508();
        }

        public bool HasFounderEntitlement(string accountId)
        {
            return State.founderModeEnabled && string.Equals(accountId, State.founderAccountId, StringComparison.Ordinal);
        }

        public bool IsContentUnlocked(string accountId, EROContentSystem system)
        {
            if (HasFounderEntitlement(accountId)) return true;
            return system != EROContentSystem.Crystals || State.paymentsEnabled;
        }
    }
}
