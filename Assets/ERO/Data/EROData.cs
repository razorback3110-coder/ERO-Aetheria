using System; using UnityEngine;
namespace ERO.Data {
 public enum EROClass{Knight,Assassin,Ranger,Mage,Priest,Monk,Summoner,Paladin}
 public enum ClassRole{PhysicalDPS,MagicDPS,Tank,Heal}
 public enum SummonerPactRole{DPS,Tank,Heal}
 public enum SummonerSkillMode{Debuff,SingleTarget,AOEBurst}
 public enum SummonerPetControl{Auto,SemiAuto,Manual}
 public enum Gender{Male,Female}
 public enum Rarity{Common,Uncommon,Rare,Epic,Legendary,Mythic,Eternal}
 public enum Language{French,English,German,Spanish,Italian,Dutch,Portuguese,Japanese,Korean,ChineseSimplified}
 [Serializable] public class Appearance{public Gender gender; public int face; public int hair; public int hairColor; public int eyeColor; public int skinTone; public float height=1f;}
 [Serializable] public class CharacterData{public string id; public string name; public EROClass classId; public int level=1; public long xp; public Appearance appearance=new Appearance(); public int gold; public int crystals;}
 [Serializable] public class SummonerPactData{public string id; public string name; public SummonerPactRole role; public bool primary; public int maxActiveSummons=1; public SummonerSkillMode specialization=SummonerSkillMode.SingleTarget;}
 [Serializable] public class ItemData{public string id; public string name; public Rarity rarity; public int level; public int quantity=1; public bool equipped;}
 [Serializable] public class QuestData{public string id; public string title; public string description; public int required; public int progress; public int goldReward; public long xpReward; public bool completed;}
 [Serializable] public class ZoneData{public string id; public string name; public int minLevel; public int maxLevel; public string[] pointsOfInterest;}
 [Serializable] public class GuildData{public string id; public string name; public int level=1; public int members; public long experience;}
}
