using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using EternalRealmsOnline.V508;

namespace EternalRealmsOnline.V522
{
    /// <summary>Runtime CJK fallback for Character Select. Uses OS fonts so the project does not depend on a missing TMP atlas.</summary>
    public sealed class EROCJKFontRuntimeV522 : MonoBehaviour
    {
        static EROCJKFontRuntimeV522 instance;
        TMP_FontAsset jp, kr, sc;
        readonly List<TMP_Text> texts = new List<TMP_Text>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject("ERO_V522_CJK_FontRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EROCJKFontRuntimeV522>();
        }

        void Awake()
        {
            var loc = EROLocalizationV508.Instance;
            if (loc != null) loc.LanguageChanged += OnLanguageChanged;
        }

        void Start()
        {
            var loc = EROLocalizationV508.Instance;
            if (loc != null) OnLanguageChanged(loc.CurrentLanguage);
        }

        void OnDestroy()
        {
            var loc = EROLocalizationV508.Instance;
            if (loc != null) loc.LanguageChanged -= OnLanguageChanged;
        }

        void OnLanguageChanged(EROLanguage language)
        {
            if (language == EROLanguage.Japanese) Apply(Build(ref jp, new[] { "Yu Gothic UI", "Meiryo", "Noto Sans CJK JP" }));
            else if (language == EROLanguage.Korean) Apply(Build(ref kr, new[] { "Malgun Gothic", "Noto Sans CJK KR" }));
            else if (language == EROLanguage.ChineseSimplified) Apply(Build(ref sc, new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC" }));
        }

        TMP_FontAsset Build(ref TMP_FontAsset cache, string[] candidates)
        {
            if (cache != null) return cache;
            Font font = null;
            foreach (var name in candidates)
            {
                font = Font.CreateDynamicFontFromOSFont(name, 64);
                if (font != null) break;
            }
            if (font == null) return null;
            cache = TMP_FontAsset.CreateFontAsset(font, 64, 8, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            return cache;
        }

        void Apply(TMP_FontAsset asset)
        {
            if (asset == null) return;
            texts.Clear();
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)) texts.Add(t);
            foreach (var t in texts) if (t != null) t.font = asset;
        }
    }
}
