using UnityEngine; using ERO.Data;
namespace ERO.Systems { public sealed class CombatSystem:MonoBehaviour { public int CalculateDamage(CharacterData c,int power,int defense,bool critical=false){if(c==null)return 0;int baseD=Mathf.Max(1,power-defense);return critical?Mathf.RoundToInt(baseD*1.75f):baseD;} public bool CanAutoInRankedPvP(){return false;} public bool CanAutoInGvG(){return false;} }
}
