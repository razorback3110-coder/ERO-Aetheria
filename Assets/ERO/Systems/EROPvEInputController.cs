using UnityEngine;

namespace ERO.Systems
{
    /// <summary>Small input adapter that makes the deterministic PvE loop playable without coupling combat to UI.</summary>
    [DisallowMultipleComponent]
    public sealed class EROPvEInputController : MonoBehaviour
    {
        [SerializeField] private EROPvEEncounterSystem encounter;
        [SerializeField] private KeyCode attackKey = KeyCode.Space;
        [SerializeField] private KeyCode resetKey = KeyCode.R;

        public EROPvEEncounterSystem Encounter => encounter;

        private void Awake()
        {
            if (encounter == null)
                encounter = GetComponent<EROPvEEncounterSystem>() ?? GetComponentInParent<EROPvEEncounterSystem>() ?? FindFirstObjectByType<EROPvEEncounterSystem>();
        }

        private void Update()
        {
            if (encounter == null) return;
            if (Input.GetKeyDown(attackKey))
                encounter.TryAttack();
            if (Input.GetKeyDown(resetKey))
                encounter.ResetEncounter();
        }

        public bool Attack()
        {
            return encounter != null && encounter.TryAttack();
        }

        public void ResetEncounter()
        {
            encounter?.ResetEncounter();
        }
    }
}
