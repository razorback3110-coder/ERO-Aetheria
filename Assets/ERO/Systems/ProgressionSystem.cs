using UnityEngine; using ERO.Data;
namespace ERO.Systems { public sealed class ProgressionSystem:MonoBehaviour { public event System.Action<int> LevelChanged; public void AddXP(CharacterData c,long amount){if(c==null)return;c.xp+=amount;while(c.level<100&&c.xp>=Need(c.level)){c.xp-=Need(c.level);c.level++;LevelChanged?.Invoke(c.level);}} public long Need(int level){return 100L+level*level*25L;} public bool CanAwaken(CharacterData c){return c!=null&&c.level>=100;} public void Awaken(CharacterData c){if(CanAwaken(c)) c.level=100;} }
}
