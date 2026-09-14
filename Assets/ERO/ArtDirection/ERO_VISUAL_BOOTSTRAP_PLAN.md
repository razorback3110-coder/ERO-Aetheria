# ERO Visual Bootstrap — Production Acceleration

Status: ACTIVE

## Objective
Replace the prototype look quickly without waiting for every custom asset to be authored.

## Fast-track asset strategy

1. Use the approved CC0/commercially usable Quaternius and Kenney sources as production foundations.
2. Import textured source assets into `Assets/ERO/Art/External/` rather than rebuilding equivalent geometry.
3. Normalize scale, pivots, materials, lighting and LODs through one ERO import pipeline.
4. Kitbash modular villages, ruins and dungeons into original ERO layouts.
5. Use ERO material variants, decals, foliage composition, VFX and lighting to unify different sources.
6. Keep Tier A assets (player characters, class equipment, bosses/MVPs, mounts, SSR PETs and iconic buildings) reserved for custom/hero treatment.

## Required production folders

- `Assets/ERO/Art/External/Environment`
- `Assets/ERO/Art/External/Characters`
- `Assets/ERO/Art/External/Creatures`
- `Assets/ERO/Art/External/Props`
- `Assets/ERO/Art/External/VFX`
- `Assets/ERO/Art/Materials`
- `Assets/ERO/Art/Textures`
- `Assets/ERO/Art/Prefabs`
- `Assets/ERO/Art/Scenes`
- `Assets/ERO/Art/UI`

## First visual slice

The first playable presentation slice should contain:
- an Aetheria village assembled from textured modular buildings;
- roads, vegetation, rocks, props and lighting;
- one combat clearing;
- one dungeon entrance and a small dungeon section;
- a player presentation area;
- class/PET presentation placeholders only where a legal finished model is unavailable;
- ERO HUD/inventory/equipment styling;
- day/night and atmospheric treatment.

## Hard rule

Unity primitives may remain for collision/debug/testing only. They must not be used as the visible final asset for a production presentation scene.

## Legal gate

Every external source must be listed in `ERO_Asset_License_Registry.md` and `ERO_APPROVED_ASSET_CATALOG.md` before inclusion in production content. Never import ripped/proprietary game assets.
