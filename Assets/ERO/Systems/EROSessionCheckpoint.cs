using UnityEngine;
using UnityEngine.SceneManagement;

namespace ERO.Systems
{
    /// <summary>
    /// Flushes the active character before scene transitions and focus loss.
    /// This complements the periodic autosave and protects progression during
    /// normal MMORPG-style world/scene travel while the backend persistence layer
    /// is still being introduced.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EROSessionCheckpoint : MonoBehaviour
    {
        private static EROSessionCheckpoint instance;
        private SaveSystem saveSystem;
        private CharacterSystem characterSystem;
        private bool subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;

            var host = new GameObject("ERO_SessionCheckpoint");
            host.hideFlags = HideFlags.HideAndDontSave;
            instance = host.AddComponent<EROSessionCheckpoint>();
            DontDestroyOnLoad(host);
        }

        private void Awake()
        {
            ResolveSystems();
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            Application.focusChanged += HandleFocusChanged;
            subscribed = true;
        }

        private void OnDestroy()
        {
            if (!subscribed) return;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            Application.focusChanged -= HandleFocusChanged;
            subscribed = false;
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            SaveNow();
        }

        private void HandleFocusChanged(bool hasFocus)
        {
            if (!hasFocus) SaveNow();
        }

        private void SaveNow()
        {
            ResolveSystems();
            if (saveSystem == null || characterSystem == null || characterSystem.Active == null) return;
            saveSystem.Save(characterSystem.Active);
        }

        private void ResolveSystems()
        {
            if (saveSystem == null)
                saveSystem = FindFirstObjectByType<SaveSystem>();
            if (characterSystem == null)
                characterSystem = FindFirstObjectByType<CharacterSystem>();
        }
    }
}
