using System;
using ERO.Data;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>Deterministic single-player PvE encounter loop. The state transitions are suitable for later authoritative server driving.</summary>
    [DisallowMultipleComponent]
    public sealed class EROPvEEncounterSystem : MonoBehaviour
    {
        [SerializeField, Min(1)] private int enemyMaxHealth = 100;
        [SerializeField, Min(1)] private int playerMaxHealth = 250;
        [SerializeField, Min(1)] private int playerDamage = 25;
        [SerializeField, Min(1)] private int enemyDamage = 12;
        [SerializeField, Min(0.1f)] private float playerAttackCooldown = 0.5f;
        [SerializeField, Min(0.1f)] private float enemyAttackInterval = 1.5f;
        [SerializeField, Min(1)] private long xpReward = 40;
        [SerializeField, Min(0)] private long creditReward = 10;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;
        [SerializeField] private string encounterId = "starter-encounter";

        private EROProgressionSystem progression;
        private EROAutoSaveCoordinator autosave;
        private CharacterSystem characterSystem;
        private int enemyHealth;
        private int playerHealth;
        private float respawnAt = -1f;
        private float nextPlayerAttackAt;
        private float nextEnemyAttackAt;
        private int defeatedCount;
        private EROLootSystem.LootResult lastLoot;
        private bool hasLastLoot;

        public int EnemyHealth => enemyHealth;
        public int EnemyMaxHealth => enemyMaxHealth;
        public bool EnemyAlive => enemyHealth > 0;
        public int PlayerHealth => playerHealth;
        public int PlayerMaxHealth => playerMaxHealth;
        public bool PlayerAlive => playerHealth > 0;
        public bool EncounterActive => EnemyAlive && PlayerAlive;
        public int DefeatedCount => defeatedCount;
        public bool HasLastLoot => hasLastLoot;
        public EROLootSystem.LootResult LastLoot => lastLoot;

        private void Awake()
        {
            progression = GetComponent<EROProgressionSystem>() ?? GetComponentInParent<EROProgressionSystem>() ?? FindFirstObjectByType<EROProgressionSystem>();
            autosave = GetComponent<EROAutoSaveCoordinator>() ?? GetComponentInParent<EROAutoSaveCoordinator>() ?? FindFirstObjectByType<EROAutoSaveCoordinator>();
            characterSystem = GetComponent<CharacterSystem>() ?? GetComponentInParent<CharacterSystem>() ?? FindFirstObjectByType<CharacterSystem>();
            ResetEncounter();
        }

        private void Update()
        {
            if (!PlayerAlive && respawnAt < 0f)
                respawnAt = Time.unscaledTime + Mathf.Max(0.1f, respawnDelay);

            if (respawnAt >= 0f && Time.unscaledTime >= respawnAt)
            {
                ResetEncounter();
                return;
            }

            if (EncounterActive && Time.unscaledTime >= nextEnemyAttackAt)
            {
                TakePlayerDamage(enemyDamage);
                nextEnemyAttackAt = Time.unscaledTime + Mathf.Max(0.1f, enemyAttackInterval);
            }
        }

        /// <summary>Applies one player hit. Returns true only when the attack is accepted.</summary>
        public bool TryAttack()
        {
            if (!EncounterActive || Time.unscaledTime < nextPlayerAttackAt)
                return false;

            nextPlayerAttackAt = Time.unscaledTime + Mathf.Max(0.1f, playerAttackCooldown);
            enemyHealth = Mathf.Max(0, enemyHealth - Mathf.Max(1, playerDamage));
            if (enemyHealth == 0)
                ResolveVictory();
            return true;
        }

        public bool TakePlayerDamage(int amount)
        {
            if (!PlayerAlive || amount <= 0) return false;
            playerHealth = Mathf.Max(0, playerHealth - amount);
            autosave?.MarkDirty();
            if (playerHealth == 0)
                respawnAt = Time.unscaledTime + Mathf.Max(0.1f, respawnDelay);
            return true;
        }

        private void ResolveVictory()
        {
            defeatedCount++;
            progression?.GrantXp(Math.Max(0L, xpReward), Math.Max(0L, creditReward));
            hasLastLoot = false;

            var character = characterSystem != null ? characterSystem.Active : null;
            var level = character != null ? Math.Max(1, character.level) : 1;
            var seed = defeatedCount;
            if (EROLootSystem.TryGenerate(encounterId, level, seed, out var loot))
            {
                lastLoot = loot;
                hasLastLoot = EROLootSystem.TryAddToInventory(character, loot);
            }

            autosave?.MarkDirty();
            respawnAt = Time.unscaledTime + Mathf.Max(0.1f, respawnDelay);
        }

        public void ResetEncounter()
        {
            enemyHealth = Mathf.Max(1, enemyMaxHealth);
            playerHealth = Mathf.Max(1, playerMaxHealth);
            respawnAt = -1f;
            nextPlayerAttackAt = 0f;
            nextEnemyAttackAt = Time.unscaledTime + Mathf.Max(0.1f, enemyAttackInterval);
        }
    }
}
