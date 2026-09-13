using UnityEngine; using ERO.Data;
namespace ERO.Systems { public sealed class EconomySystem:MonoBehaviour { public bool SpendGold(CharacterData c,int amount){if(c==null||amount<0||c.gold<amount)return false;c.gold-=amount;return true;} public bool SpendCrystals(CharacterData c,int amount){if(c==null||amount<0||c.crystals<amount)return false;c.crystals-=amount;return true;} public void GrantGold(CharacterData c,int amount){if(c!=null)c.gold=Mathf.Max(0,c.gold+amount);} public void GrantCrystals(CharacterData c,int amount){if(c!=null)c.crystals=Mathf.Max(0,c.crystals+amount);} }
}
