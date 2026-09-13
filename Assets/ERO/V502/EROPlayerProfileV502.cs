using System;
using UnityEngine;

namespace EternalRealmsOnline.V502
{
    [Serializable]
    public sealed class EROPlayerProfile
    {
        public string accountId = "OWNER_ACCOUNT";
        public string characterName = "Aetherian";
        public EROBaseClass baseClass = EROBaseClass.Mage;
        public EROBranch branch = EROBranch.Arcaniste;
        public int level = 1;
        public int experience;
        public int statPoints;
        public int strength = 5, agility = 5, vitality = 5, intelligence = 5, dexterity = 5, luck = 5;
        public EROInventoryState inventory = new();
        public EROEntitlements entitlements = new();

        public EROEvolution Evolution => EROClassDatabaseV502.GetEvolution(baseClass);
        public bool BranchUnlocked => level >= EROClassDatabaseV502.BranchChoiceLevel;
        public bool FirstEvolutionUnlocked => level >= EROClassDatabaseV502.FirstEvolutionLevel;
        public bool FinalEvolutionUnlocked => level >= EROClassDatabaseV502.FinalEvolutionLevel;

        public bool ChooseBranch(EROBranch requested)
        {
            if (!BranchUnlocked) return false;
            foreach (var allowed in EROClassDatabaseV502.GetBranches(baseClass))
                if (allowed == requested) { branch = requested; return true; }
            return false;
        }
    }

    public sealed class EROPlayerProfileRuntime : MonoBehaviour
    {
        public EROPlayerProfile Profile { get; private set; } = new();
        public static EROPlayerProfileRuntime Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(string accountId, EROBaseClass baseClass)
        {
            Profile.accountId = accountId;
            Profile.baseClass = baseClass;
            var config = ScriptableObject.CreateInstance<EROAccountConfig>();
            Profile.entitlements = config.Resolve(accountId);
            Destroy(config);
        }
    }
}
