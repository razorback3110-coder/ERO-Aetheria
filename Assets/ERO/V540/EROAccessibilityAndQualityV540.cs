using UnityEngine;
using UnityEngine.Rendering;

namespace EternalRealmsOnline.V540
{
    /// <summary>Runtime quality/accessibility defaults for the ERO presentation layer.</summary>
    public sealed class EROAccessibilityAndQualityV540 : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] static void Apply(){QualitySettings.vSyncCount=1;QualitySettings.anisotropicFiltering=AnisotropicFiltering.Enable;Application.targetFrameRate=120;}
        public static void ApplyQuality(int level){QualitySettings.SetQualityLevel(Mathf.Clamp(level,0,QualitySettings.names.Length-1),true);}
        public static void SetFrameLimit(int fps){Application.targetFrameRate=Mathf.Clamp(fps,30,240);}
    }
}
