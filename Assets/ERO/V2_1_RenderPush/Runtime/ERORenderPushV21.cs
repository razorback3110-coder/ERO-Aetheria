using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EternalRealmsOnline.V21
{
    /// <summary>
    /// Runtime visual pass for ERO. Designed as an overlay on V2/V543.
    /// It upgrades the actual camera/render output: HDR, shadows, fog,
    /// exposure, contrast, bloom, vignette and filmic tonemapping.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class ERORenderPushV21 : MonoBehaviour
    {
        [Header("Camera")]
        [Range(45f, 75f)] public float fieldOfView = 60f;
        public bool enableHDR = true;
        public bool enablePost = true;

        [Header("Atmosphere")]
        public Color fogColor = new Color(0.035f, 0.045f, 0.075f, 1f);
        [Range(0f, 0.08f)] public float fogDensity = 0.012f;
        [Range(0f, 1f)] public float ambientIntensity = 0.45f;

        [Header("Post")]
        [Range(0f, 2f)] public float bloomIntensity = 0.35f;
        [Range(0f, 1f)] public float vignetteIntensity = 0.16f;
        [Range(-2f, 2f)] public float postExposure = 0.15f;
        [Range(0f, 2f)] public float contrast = 18f;

        private Volume _volume;

        private void Awake()
        {
            ApplyCamera();
            ApplyLighting();
            ApplyAtmosphere();
            ApplyPost();
        }

        private void OnEnable()
        {
            ApplyCamera();
            ApplyLighting();
            ApplyAtmosphere();
            ApplyPost();
        }

        private void ApplyCamera()
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            cam.fieldOfView = fieldOfView;
            cam.allowHDR = enableHDR;
            cam.allowMSAA = true;

            var urp = cam.GetUniversalAdditionalCameraData();
            urp.renderPostProcessing = enablePost;
            urp.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            urp.antialiasingQuality = AntialiasingQuality.High;
        }

        private void ApplyLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = 0.75f;

            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 120f);
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;

            Light main = null;
            foreach (Light l in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional && l.enabled)
                {
                    main = l;
                    break;
                }
            }

            if (main != null)
            {
                main.shadows = LightShadows.Soft;
                main.shadowStrength = Mathf.Clamp01(Mathf.Max(main.shadowStrength, 0.75f));
                main.shadowBias = 0.03f;
                main.shadowNormalBias = 0.4f;
                main.intensity = Mathf.Max(main.intensity, 1.0f);
            }
        }

        private void ApplyAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        private void ApplyPost()
        {
            if (!enablePost) return;

            if (_volume == null)
            {
                var go = new GameObject("ERO_V21_GlobalVolume");
                go.transform.SetParent(transform, false);
                _volume = go.AddComponent<Volume>();
                _volume.isGlobal = true;
                _volume.priority = 1000;
                _volume.weight = 1f;
                _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            VolumeProfile p = _volume.profile;

            var tonemap = GetOrAdd<Tonemapping>(p);
            tonemap.mode.overrideState = true;
            tonemap.mode.value = TonemappingMode.ACES;

            var bloom = GetOrAdd<Bloom>(p);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.05f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomIntensity;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.72f;

            var vignette = GetOrAdd<Vignette>(p);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.72f;

            var color = GetOrAdd<ColorAdjustments>(p);
            color.postExposure.overrideState = true;
            color.postExposure.value = postExposure;
            color.contrast.overrideState = true;
            color.contrast.value = contrast;
            color.saturation.overrideState = true;
            color.saturation.value = 4f;

            var whiteBalance = GetOrAdd<WhiteBalance>(p);
            whiteBalance.temperature.overrideState = true;
            whiteBalance.temperature.value = -2f;
            whiteBalance.tint.overrideState = true;
            whiteBalance.tint.value = 3f;
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
                component = profile.Add<T>();
            component.active = true;
            return component;
        }
    }
}
