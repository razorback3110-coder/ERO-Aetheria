using UnityEngine;

namespace EternalRealmsOnline.V2
{
    /// <summary>
    /// Central visual profile for the ERO AAA target. Keeps art-direction settings
    /// in one place so production assets can be swapped in without rebuilding UI/gameplay.
    /// </summary>
    [CreateAssetMenu(menuName = "ERO/AAA Visual Profile V2", fileName = "EROAAAVisualProfileV2")]
    public sealed class EROAAAVisualProfileV2 : ScriptableObject
    {
        [Header("Target")]
        public string targetName = "Aetheria Anime Dark Fantasy AAA";

        [Header("Camera")]
        [Range(45f, 75f)] public float gameplayFov = 58f;
        [Range(0.1f, 2f)] public float cameraSmoothing = 0.65f;
        public float combatCameraShake = 0.12f;

        [Header("Lighting")]
        [Range(0f, 2f)] public float ambientIntensity = 0.45f;
        [Range(0f, 2f)] public float reflectionIntensity = 0.75f;
        public bool useVolumetricAtmosphere = true;

        [Header("World")]
        public bool useDistanceFog = true;
        public bool useDynamicWeather = true;
        public bool useDayNightCycle = true;

        [Header("Characters")]
        public bool requirePBRMaterials = true;
        public bool requireNormalMaps = true;
        public bool requireLODChain = true;
        public bool requireAnimationController = true;

        [Header("VFX")]
        public bool enableRiftVFX = true;
        public bool enableSpellTrails = true;
        public bool enableImpactVFX = true;

        [Header("Quality")]
        public int targetDesktopFps = 60;
        public int targetSteamDeckFps = 40;
        public int maxVisibleCombatants = 80;
    }
}
