using UnityEngine; using UnityEngine.UI; using TMPro;
namespace EternalRealmsOnline.Full {
 public sealed class EROUIBridge:MonoBehaviour {
  public TMP_Text notification; public TMP_Text characterInfo; public TMP_Text zoneInfo; public Slider hpBar; public Slider mpBar;
  void OnEnable(){if(EROFullGame.I!=null){EROFullGame.I.OnStateChanged+=Refresh;EROFullGame.I.OnNotification+=Notify;Refresh();}}
  void OnDisable(){if(EROFullGame.I!=null){EROFullGame.I.OnStateChanged-=Refresh;EROFullGame.I.OnNotification-=Notify;}}
  void Refresh(){var g=EROFullGame.I;if(g==null)return;if(characterInfo)characterInfo.text=$"{g.Character.name}\nLv {g.Character.level} {g.Character.classId}\nGold {g.Character.gold}  Crystals {g.Character.crystals}";if(zoneInfo)zoneInfo.text=g.CurrentZone;if(hpBar){hpBar.maxValue=g.Stats.maxHp;hpBar.value=g.Stats.hp;}if(mpBar){mpBar.maxValue=g.Stats.maxMp;mpBar.value=g.Stats.mp;}}
  void Notify(string s){if(notification)notification.text=s;}
 }
}
