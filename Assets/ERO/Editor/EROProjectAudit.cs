#if UNITY_EDITOR
using UnityEditor; using UnityEngine; using System.Linq;
namespace ERO.Editor { public static class EROProjectAudit { [MenuItem("Eternal Realms Online/Audit ERO Project")] static void Audit(){var scripts=AssetDatabase.FindAssets("t:MonoScript",new[]{"Assets/ERO"}).Length;var scenes=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Length;Debug.Log($"ERO audit: {scripts} ERO scripts, {scenes} scenes. Unity {Application.unityVersion}. No automatic migration performed.");} } }
#endif
