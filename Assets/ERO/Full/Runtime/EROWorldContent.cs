using System;
using System.Collections.Generic;
using UnityEngine;
using ERO.Data;
namespace EternalRealmsOnline.Full {
 [Serializable] public class EROZoneDefinition { public string id,name,region; public int minLevel,maxLevel; public Vector3 spawn; public string[] pointsOfInterest; }
 public static class EROWorldContent {
  public static readonly EROZoneDefinition[] Zones={
   Z("greenhaven","Greenhaven","Elyndor",1,10,"Village Square","Elder House","Rift Shrine"),
   Z("everwood","Everwood","Elyndor",10,20,"Spirit Grove","Moonwell","Hunter Camp"),
   Z("elyndor","Kingdom of Elyndor","Elyndor",20,30,"Elyndria","Royal Castle","Grand Cathedral"),
   Z("frostfall","Frostfall","Elyndor",30,40,"Frosthold","Frozen Lake","Ice Cavern"),
   Z("sunscar","Sunscar Desert","Neutral",40,50,"Solara","Dune Sea","Sun Temple"),
   Z("sylvaris","Sylvaris","Sylvaris",50,60,"Lunareth","World Tree","Spirit Court"),
   Z("abyssia","Abyssia","Abyssia",60,70,"Vhar’Zul","Blackreach","Ashen Plains","Blood Marsh","Obsidian Fortress"),
   Z("rift","The Rift","Rift",70,80,"Rift Gate","Fractured Fields"),
   Z("eternal_rift","Eternal Rift","Rift",80,90,"Eternal Gate","Rift Nexus"),
   Z("endless_abyss","Endless Abyss","Abyss",90,100,"Abyss Tower","Eternal Throne")};
  static EROZoneDefinition Z(string id,string n,string r,int min,int max,params string[] poi)=>new EROZoneDefinition{id=id,name=n,region=r,minLevel=min,maxLevel=max,pointsOfInterest=poi};
  public static bool TryGet(string id,out EROZoneDefinition zone){foreach(var z in Zones)if(z.id==id){zone=z;return true;}zone=null;return false;}
 }
 public static class EROClassContent {
  public static readonly Dictionary<EROClass,string> Roles=new(){
   [EROClass.Paladin]="Tank / Holy DPS",
   [EROClass.Priest]="Healer / Support",
   [EROClass.Invocateur]="Summoner / Control",
   [EROClass.Mage]="Magic DPS / Control",
   [EROClass.Assassin]="Melee DPS / Burst",
   [EROClass.Archer]="Ranged DPS / Mobility",
   [EROClass.Guerrier]="Melee DPS / Frontline"};
  public static readonly Dictionary<EROClass,string> Weapons=new(){
   [EROClass.Paladin]="Holy Sword + Shield",
   [EROClass.Priest]="Holy Staff",
   [EROClass.Invocateur]="Grimoire",
   [EROClass.Mage]="Staff + Grimoire",
   [EROClass.Assassin]="Twin Blades",
   [EROClass.Archer]="Bow",
   [EROClass.Guerrier]="Sword + Greatsword"};
 }
}
