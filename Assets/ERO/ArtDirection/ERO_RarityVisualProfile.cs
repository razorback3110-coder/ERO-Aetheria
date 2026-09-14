using UnityEngine;

namespace ERO.ArtDirection
{
    /// <summary>Shared presentation rules for ERO item rarity. Gameplay rarity remains server/data driven.</summary>
    [CreateAssetMenu(fileName = "ERO_RarityVisualProfile", menuName = "ERO/Art/Rarity Visual Profile")]
    public sealed class ERO_RarityVisualProfile : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string rarity;
            public Color accent;
            [Range(0f, 1f)] public float emissiveStrength;
            [Range(0f, 1f)] public float borderIntensity;
        }

        public Entry[] entries =
        {
            new Entry { rarity = "Common", accent = new Color(0.72f, 0.72f, 0.76f), emissiveStrength = 0f, borderIntensity = 0.20f },
            new Entry { rarity = "Uncommon", accent = new Color(0.35f, 0.82f, 0.48f), emissiveStrength = 0.02f, borderIntensity = 0.28f },
            new Entry { rarity = "Rare", accent = new Color(0.30f, 0.58f, 1.00f), emissiveStrength = 0.05f, borderIntensity = 0.38f },
            new Entry { rarity = "Epic", accent = new Color(0.68f, 0.36f, 1.00f), emissiveStrength = 0.10f, borderIntensity = 0.50f },
            new Entry { rarity = "Legendary", accent = new Color(1.00f, 0.60f, 0.16f), emissiveStrength = 0.16f, borderIntensity = 0.62f },
            new Entry { rarity = "Mythic", accent = new Color(1.00f, 0.28f, 0.72f), emissiveStrength = 0.25f, borderIntensity = 0.78f },
            new Entry { rarity = "Unique", accent = new Color(0.28f, 0.92f, 1.00f), emissiveStrength = 0.34f, borderIntensity = 0.92f }
        };
    }
}
