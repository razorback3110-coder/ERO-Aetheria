using System;
using UnityEngine;

namespace ERO.ArtDirection
{
    /// <summary>
    /// Central visual contract for ERO asset families. Gameplay code should reference
    /// these IDs instead of hard-coding scene-specific art paths.
    /// </summary>
    [CreateAssetMenu(fileName = "ERO_VisualAssetManifest", menuName = "ERO/Art/Visual Asset Manifest")]
    public sealed class ERO_VisualAssetManifest : ScriptableObject
    {
        [Serializable]
        public sealed class AssetFamily
        {
            public string id;
            public string tier;
            [TextArea] public string purpose;
            public GameObject prefab;
            public Material materialOverride;
        }

        [Header("Hero / Gameplay / Dressing")]
        public AssetFamily[] environment;
        public AssetFamily[] characters;
        public AssetFamily[] creatures;
        public AssetFamily[] equipment;
        public AssetFamily[] props;
        public AssetFamily[] vfx;
        public AssetFamily[] ui;

        [Header("ERO Presentation")]
        [Tooltip("Shared visual vocabulary for rarity, class, element and world presentation.")]
        public string visualStandard = "Assets/ERO/ArtDirection/ERO_VISUAL_PRODUCTION_STANDARD.md";

        public AssetFamily Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Find(environment, id) ?? Find(characters, id) ?? Find(creatures, id) ??
                   Find(equipment, id) ?? Find(props, id) ?? Find(vfx, id) ?? Find(ui, id);
        }

        private static AssetFamily Find(AssetFamily[] families, string id)
        {
            if (families == null) return null;
            foreach (var family in families)
                if (family != null && string.Equals(family.id, id, StringComparison.OrdinalIgnoreCase))
                    return family;
            return null;
        }
    }
}
