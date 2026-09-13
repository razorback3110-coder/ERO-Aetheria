using UnityEngine;
namespace ERO.Core {
 public sealed class EROGameRoot:MonoBehaviour {
  public static EROGameRoot Instance {get; private set;}
  public EROSystems Systems {get; private set;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot(){ if(Instance!=null)return; var go=new GameObject("ERO_GameRoot"); Instance=go.AddComponent<EROGameRoot>(); DontDestroyOnLoad(go); Instance.Systems=go.AddComponent<EROSystems>(); }
  void Awake(){ if(Instance!=null && Instance!=this){Destroy(gameObject);return;} Instance=this; if(Systems==null) Systems=GetComponent<EROSystems>()??gameObject.AddComponent<EROSystems>(); DontDestroyOnLoad(gameObject); }
 }
}
