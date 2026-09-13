# Mobile-first fallback
ERO remains architected as a PC/Steam project. If PC rendering/performance becomes too heavy during early testing, the same project can target Android/iOS with a mobile presentation profile.
Mobile profile priorities:
- scalable resolution and render scale
- reduced shadow distance
- baked/limited lighting
- pooled VFX
- reduced character/monster LODs
- touch HUD
- controller support where available
No gameplay rewrite is required if platform-neutral systems are kept separate from presentation.
