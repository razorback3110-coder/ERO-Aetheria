using System;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>Small deterministic PvE encounter loop for the playable slice. Combat rewards flow through EROProgressionSystem so persistence captures them.</summary>
    [DisallowMultipleComponent]
    public sealed class EROPvEEncounterSystem : MonoBehaviour
    {
        [SerializeField, Min(1)] private int enemyMaxHealth = 100;
        [SerializeField, Min(1)] private int playerDamage = 25;
        [SerializeField, Min(1)] private long xpReward = 40;
        [SerializeField, Min(0)] private long creditReward = 10;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;

        private EROProgressionSystem progression;
        private EROAutoSaveCoordinator autosave;
        private int enemyHealth;
        private float respawnAt = -1f;

        public int EnemyHealth => enemyHealth;
        public int EnemyMaxHealth => enemyMaxHealth;
        public bool EnemyAlive => enemyHealth > 0;

        private void Awake()
        {
            progression = GetComponent<EROProgressionSystem>() ?? GetComponentInParent<EROProgressionSystem>() ?? FindFirstObjectByType<EROProgressionSystem>();
            autosave = GetComponent<EROAutoSaveCoordinator>() ?? GetComponentInParent<EROAutoSaveCoordinator>() ?? FindFirstObjectByType<EROAutoSaveCoordinator>();
            enemyHealth = Mathf.Max(1, enemyMaxHealth);
        }

        private void Update()
        {
            if (respawnAt >= 0f && Time.unscaledTime >= respawnAt)
            {
                enemyHealth = Mathf.Max(1, enemyMaxHealth);
                respawnAt = -1f;
            }
        }

        /// <summary>Applies one server-friendly player hit. Returns true only when the hit is accepted.</summary>
        public bool TryAttack()
        {
            if (!EnemyAlive) return false;

            enemyHealth = Mathf.Max(0, enemyHealth - Mathf.Max(1, playerDamage));
            if (enemyHealth == 0)
            {
                progression?.GrantXp(Math.Max(0L, xpReward), Math.Max(0L, creditReward));
                autosave?.MarkDirty();
                respawnAt = Time.unscaledTime + Mathf.Max(0.1f, respawnDelay);
            }
            return true;
        }

        public void ResetEncounter()
        {
            enemyHealth = Mathf.Max(1, enemyMaxHealth);
            respawnAt = -1f;
        }
    }
}
