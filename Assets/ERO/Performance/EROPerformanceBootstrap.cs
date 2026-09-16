using UnityEngine;

namespace ERO.Performance
{
    /// <summary>
    /// Lightweight runtime performance baseline for the playable ERO vertical slice.
    /// Keeps the prototype responsive while world streaming and combat systems are expanded.
    /// No third-party assets are used.
    /// </summary>
    public static class EROPerformanceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
            QualitySettings.asyncUploadTimeSlice = 4;
            QualitySettings.asyncUploadBufferSize = 16;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsAddAllCameras = true;
        }
    }
}
