using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>Bridges CharacterSystem and SaveSystem for local persistence; backend MMO persistence can replace SaveSystem later.</summary>
    [DisallowMultipleComponent]
    public sealed class EROAutoSaveCoordinator : MonoBehaviour
    {
        [SerializeField, Min(5f)] private float intervalSeconds = 30f;
        [SerializeField, Min(0.25f)] private float dirtyCheckIntervalSeconds = 1f;
        [SerializeField] private bool saveOnPause = true;
        [SerializeField] private bool saveOnQuit = true;

        private SaveSystem saveSystem;
        private CharacterSystem characterSystem;
        private CharacterData character;
        private float nextSaveAt;
        private float nextDirtyCheckAt;
        private bool dirty;
        private string lastSnapshot;

        public bool IsDirty => dirty;
        public float IntervalSeconds => intervalSeconds;
        public float DirtyCheckIntervalSeconds => dirtyCheckIntervalSeconds;

        private void Awake()
        {
            saveSystem = GetComponent<SaveSystem>() ?? GetComponentInParent<SaveSystem>() ?? FindFirstObjectByType<SaveSystem>();
            characterSystem = GetComponent<CharacterSystem>() ?? GetComponentInParent<CharacterSystem>() ?? FindFirstObjectByType<CharacterSystem>();
            if (saveSystem == null || characterSystem == null) return;

            var restored = saveSystem.Load();
            if (restored != null) characterSystem.TryRestore(restored);
            SyncCharacterReference();
            ScheduleNextSave();
            ScheduleNextDirtyCheck();
        }

        public void Initialize(SaveSystem persistence, CharacterData data)
        {
            saveSystem = persistence;
            character = data;
            lastSnapshot = Snapshot(character);
            dirty = false;
            ScheduleNextSave();
            ScheduleNextDirtyCheck();
        }

        public void MarkDirty()
        {
            SyncCharacterReference();
            if (saveSystem == null || character == null) return;
            dirty = true;
        }

        public void SaveNow()
        {
            SyncCharacterReference();
            if (saveSystem == null || character == null || !dirty) return;
            saveSystem.Save(character);
            lastSnapshot = Snapshot(character);
            dirty = false;
            ScheduleNextSave();
            ScheduleNextDirtyCheck();
        }

        private void Update()
        {
            SyncCharacterReference();
            if (saveSystem == null || character == null) return;

            if (Time.unscaledTime >= nextDirtyCheckAt)
            {
                DetectChanges();
                ScheduleNextDirtyCheck();
            }

            if (dirty && Time.unscaledTime >= nextSaveAt) SaveNow();
        }

        private void DetectChanges()
        {
            var snapshot = Snapshot(character);
            if (!string.Equals(snapshot, lastSnapshot, System.StringComparison.Ordinal))
            {
                dirty = true;
                lastSnapshot = snapshot;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && saveOnPause) SaveNow();
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit) SaveNow();
        }

        private void SyncCharacterReference()
        {
            if (characterSystem != null) character = characterSystem.Active;
            else if (character == null) characterSystem = FindFirstObjectByType<CharacterSystem>();
        }

        private static string Snapshot(CharacterData data)
        {
            return data == null ? string.Empty : JsonUtility.ToJson(data);
        }

        private void ScheduleNextSave()
        {
            nextSaveAt = Time.unscaledTime + Mathf.Max(5f, intervalSeconds);
        }

        private void ScheduleNextDirtyCheck()
        {
            nextDirtyCheckAt = Time.unscaledTime + Mathf.Max(0.25f, dirtyCheckIntervalSeconds);
        }
    }
}
