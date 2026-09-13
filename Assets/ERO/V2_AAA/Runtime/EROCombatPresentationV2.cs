using UnityEngine;

namespace EternalRealmsOnline.V2
{
    /// <summary>
    /// Lightweight presentation hooks for combat: hit-stop, camera impulse,
    /// spell flash and Rift pulse. Gameplay damage remains owned by the combat system.
    /// </summary>
    public sealed class EROCombatPresentationV2 : MonoBehaviour
    {
        public float hitStop = 0.035f;
        public float cameraImpulse = 0.08f;
        public float riftPulse = 0.0f;

        public void PlayLightImpact()
        {
            riftPulse = Mathf.Max(riftPulse, 0.35f);
        }

        public void PlayHeavyImpact()
        {
            riftPulse = Mathf.Max(riftPulse, 1.0f);
        }

        void Update()
        {
            riftPulse = Mathf.MoveTowards(riftPulse, 0f, Time.unscaledDeltaTime * 1.8f);
        }
    }
}
