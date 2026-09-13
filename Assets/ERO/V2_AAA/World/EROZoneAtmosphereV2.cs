using UnityEngine;

namespace EternalRealmsOnline.V2
{
    public enum EROZoneV2
    {
        Greenhaven, Everwood, Elyndor, Frostfall, Sunscar,
        Sylvaris, Abyssia, Rift, EternalRift, EndlessAbyss
    }

    /// <summary>
    /// Zone atmosphere hook. Final production scenes can connect these values to
    /// Unity Volume/Lighting/Weather assets.
    /// </summary>
    public sealed class EROZoneAtmosphereV2 : MonoBehaviour
    {
        public EROZoneV2 zone = EROZoneV2.Greenhaven;

        void Awake()
        {
            switch (zone)
            {
                case EROZoneV2.Frostfall:
                    RenderSettings.fogDensity = 0.012f;
                    break;
                case EROZoneV2.Abyssia:
                case EROZoneV2.Rift:
                case EROZoneV2.EternalRift:
                case EROZoneV2.EndlessAbyss:
                    RenderSettings.fogDensity = 0.018f;
                    break;
                default:
                    RenderSettings.fogDensity = 0.006f;
                    break;
            }
        }
    }
}
