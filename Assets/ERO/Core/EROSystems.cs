using UnityEngine;
using ERO.Systems;
namespace ERO.Core {
 public sealed class EROSystems:MonoBehaviour {
  public LocalizationSystem Localization{get;private set;} public CharacterSystem Character{get;private set;} public ProgressionSystem Progression{get;private set;} public InventorySystem Inventory{get;private set;} public CombatSystem Combat{get;private set;} public QuestSystem Quests{get;private set;} public EconomySystem Economy{get;private set;} public SocialSystem Social{get;private set;} public WorldSystem World{get;private set;} public SaveSystem Save{get;private set;} public EROAutoSaveCoordinator AutoSave{get;private set;}
  void Awake(){Localization=Add<LocalizationSystem>(); Character=Add<CharacterSystem>(); Progression=Add<ProgressionSystem>(); Inventory=Add<InventorySystem>(); Combat=Add<CombatSystem>(); Quests=Add<QuestSystem>(); Economy=Add<EconomySystem>(); Social=Add<SocialSystem>(); World=Add<WorldSystem>(); Save=Add<SaveSystem>(); AutoSave=Add<EROAutoSaveCoordinator>();}
  T Add<T>() where T:Component {return gameObject.AddComponent<T>();}
 }
}
