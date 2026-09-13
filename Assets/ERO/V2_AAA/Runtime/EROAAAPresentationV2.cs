using UnityEngine;

namespace EternalRealmsOnline.V2
{
    /// <summary>
    /// Applies the V2 presentation profile at runtime. It is intentionally additive:
    /// gameplay/network systems remain separate from presentation.
    /// </summary>
    public sealed class EROAAAPresentationV2 : MonoBehaviour
    {
        public EROAAAVisualProfileV2 profile;

        void Awake()
        {
            if (profile == null) return;

            RenderSettings.ambientIntensity = profile.ambientIntensity;
            RenderSettings.reflectionIntensity = profile.reflectionIntensity;

            Camera cam = Camera.main;
            if (cam != null)
                cam.fieldOfView = profile.gameplayFov;
        }
    }
}
