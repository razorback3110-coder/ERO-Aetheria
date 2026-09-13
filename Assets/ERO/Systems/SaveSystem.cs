using UnityEngine; using ERO.Data; using ERO.Core;
namespace ERO.Systems { public sealed class SaveSystem:MonoBehaviour { const string Key="ERO.Character"; public void Save(CharacterData c){if(c==null)return;PlayerPrefs.SetString(Key,JsonUtility.ToJson(c));PlayerPrefs.Save();} public CharacterData Load(){var s=PlayerPrefs.GetString(Key,"");return string.IsNullOrEmpty(s)?null:JsonUtility.FromJson<CharacterData>(s);} public void Delete(){PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();} }
}
