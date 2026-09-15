using System;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Coordinates periodic and lifecycle-safe character persistence.
    /// The authoritative MMO backend can replace SaveSystem without changing gameplay callers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EROAutoSaveCoordinator : MonoBehaviour
    {
        [SerializeField, Min(5f)] private float intervalSeconds = 30f;
        [SerializeField] private bool saveOnPause = true;
        [SerializeField] private bool saveOnQuit = true;

        private SaveSystem saveSystem;
        private CharacterData character;
        private float nextSaveAt;
        private bool dirty;

        public bool IsDirty => dirty;
        public float IntervalSeconds => intervalSeconds;

        public void Initialize(SaveSystem persistence, CharacterData data)
        {
            saveSystem = persistence;
            character = data;
            dirty = false;
            ScheduleNextSave();
        }

        public void MarkDirty()
        {
            if (saveSystem == null || character == null) return;
            dirty = true;
        }

        public void SaveNow()
        {
            if (saveSystem == null || character == null || !dirty) return;
            saveSystem.Save(character);
            dirty = false;
            ScheduleNextSave();
        }

        private void Update()
        {
            if (!dirty || saveSystem == null || character == null) return;
            if (Time.unscaledTime < nextSaveAt) return;
            SaveNow();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && saveOnPause) SaveNow();
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit) SaveNow();
        }

        private void ScheduleNextSave()
        {
            nextSaveAt = Time.unscaledTime + Mathf.Max(5f, intervalSeconds);
        }
    }
}
