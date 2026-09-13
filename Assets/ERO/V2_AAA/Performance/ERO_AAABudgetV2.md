# ERO V2 performance targets

Desktop:
- 60 FPS target in normal gameplay.
- scalable 30/60/120 FPS options where hardware permits.
- dynamic resolution option.
- LOD/HLOD for environments.
- GPU instancing for repeated props.
- pooled VFX/projectiles.
- async loading for zones and dungeons.

Steam Deck:
- target 40 FPS profile.
- conservative shadow distance.
- scalable volumetrics.
- VFX density cap.
- texture streaming.
- controller-first UI.

Combat:
- pool temporary VFX.
- cap simultaneous expensive particles.
- avoid per-frame allocations.
- use distance-based animation/VFX quality.

These are engineering targets, not measured benchmark results.
