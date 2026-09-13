using System;
using UnityEngine;

namespace EternalRealmsOnline.V502
{
    [Serializable]
    public sealed class EROEntitlements
    {
        public bool founder;
        public bool founderPack;
        public bool premiumContentUnlocked;
        public int crystalBalance;
        public int goldBalance;
    }

    [CreateAssetMenu(menuName = "Eternal Realms Online/Account Configuration", fileName = "EROAccountConfig")]
    public sealed class EROAccountConfig : ScriptableObject
    {
        public string founderAccountId = "OWNER_ACCOUNT";
        public bool founderModeEnabled = true;
        public bool paymentsEnabled = false;

        public bool HasFounderEntitlement(string accountId)
        {
            return founderModeEnabled && !string.IsNullOrEmpty(accountId) &&
                   string.Equals(accountId, founderAccountId, StringComparison.Ordinal);
        }

        public EROEntitlements Resolve(string accountId)
        {
            bool founder = HasFounderEntitlement(accountId);
            return new EROEntitlements
            {
                founder = founder,
                founderPack = founder,
                premiumContentUnlocked = founder,
                crystalBalance = founder ? 10000 : 0,
                goldBalance = founder ? 1000000 : 0
            };
        }
    }
}
