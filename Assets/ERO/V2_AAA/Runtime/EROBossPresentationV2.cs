using UnityEngine;

namespace EternalRealmsOnline.V2
{
    /// <summary>
    /// Boss presentation controller. Production boss animation/VFX can bind to these states.
    /// </summary>
    public sealed class EROBossPresentationV2 : MonoBehaviour
    {
        public string bossId = "EternalKing";
        public float phase;
        public float enragedIntensity;

        public void SetPhase(float normalizedPhase)
        {
            phase = Mathf.Clamp01(normalizedPhase);
        }

        public void EnterEnrage()
        {
            enragedIntensity = 1f;
        }

        void Update()
        {
            enragedIntensity = Mathf.MoveTowards(enragedIntensity, 0f, Time.deltaTime * 0.08f);
        }
    }
}
