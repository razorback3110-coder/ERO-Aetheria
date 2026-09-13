using System;
using UnityEngine;

namespace EternalRealmsOnline.World
{
    /// <summary>AAA-oriented procedural world rules. Data-only profile for deterministic composition.</summary>
    [Serializable]
    public sealed class EROAAAWorldGenerationProfile : MonoBehaviour
    {
        [Serializable]
        public sealed class BiomeRule
        {
            public string id;
            public string displayName;
            [Range(0, 1)] public float vegetationDensity = .6f;
            [Range(0, 1)] public float rockDensity = .35f;
            [Range(0, 1)] public float waterDensity = .2f;
            [Range(0, 1)] public float landmarkDensity = .08f;
            [Range(0, 1)] public float encounterDensity = .5f;
            public int seedOffset;
        }

        [Header("World Quality")]
        [Min(1)] public int worldSeed = 740067;
        [Min(64)] public int chunkSizeMeters = 128;
        [Min(1)] public int activeChunkRadius = 3;
        [Range(0, 1)] public float terrainDetail = .85f;
        [Range(0, 1)] public float naturalFeatureDensity = .8f;
        [Range(0, 1)] public float landmarkDensity = .18f;
        [Range(0, 1)] public float dungeonDensity = .08f;
        [Range(0, 1)] public float gatheringDensity = .65f;
        [Range(0, 1)] public float worldEventDensity = .12f;
        [Range(0, 1)] public float hiddenSecretDensity = .06f;

        [Header("Streaming / Performance")]
        public bool additiveChunkStreaming = true;
        public bool deterministicGeneration = true;
        public bool generateServerSideIdentity = true;
        public bool generateNavigationData = true;
        public bool generateMapData = true;

        [Header("Biomes")]
        public BiomeRule[] biomes =
        {
            new BiomeRule { id="GREEN", displayName="Greenhaven", vegetationDensity=.75f, rockDensity=.25f, waterDensity=.35f, landmarkDensity=.08f, encounterDensity=.45f, seedOffset=11 },
            new BiomeRule { id="FOREST", displayName="Everwood", vegetationDensity=.95f, rockDensity=.30f, waterDensity=.45f, landmarkDensity=.10f, encounterDensity=.60f, seedOffset=23 },
            new BiomeRule { id="HIGHLAND", displayName="Elyndor", vegetationDensity=.55f, rockDensity=.55f, waterDensity=.25f, landmarkDensity=.13f, encounterDensity=.55f, seedOffset=37 },
            new BiomeRule { id="FROST", displayName="Frostfall", vegetationDensity=.25f, rockDensity=.65f, waterDensity=.20f, landmarkDensity=.14f, encounterDensity=.65f, seedOffset=53 },
            new BiomeRule { id="DESERT", displayName="Sunscar", vegetationDensity=.12f, rockDensity=.70f, waterDensity=.08f, landmarkDensity=.16f, encounterDensity=.70f, seedOffset=71 },
            new BiomeRule { id="LUNAR", displayName="Lunareth", vegetationDensity=.50f, rockDensity=.45f, waterDensity=.30f, landmarkDensity=.18f, encounterDensity=.72f, seedOffset=89 },
            new BiomeRule { id="ABYSS", displayName="Abyssia", vegetationDensity=.20f, rockDensity=.80f, waterDensity=.12f, landmarkDensity=.22f, encounterDensity=.85f, seedOffset=107 },
            new BiomeRule { id="RIFT", displayName="Eternal Rift", vegetationDensity=.30f, rockDensity=.75f, waterDensity=.25f, landmarkDensity=.30f, encounterDensity=.95f, seedOffset=131 }
        };

        public int GetChunkSeed(int chunkX, int chunkZ)
        {
            unchecked { return worldSeed ^ (chunkX * 73856093) ^ (chunkZ * 19349663); }
        }
    }
}
