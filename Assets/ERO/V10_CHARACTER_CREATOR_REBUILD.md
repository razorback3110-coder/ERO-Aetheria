# ERO V10 — Character Creator Rebuild

This patch addresses the V9 runtime screenshot where the character preview showed only the camera background.

Changes:
- Dedicated isolated preview studio on layer 30.
- Dedicated RenderTexture camera with explicit SolidColor background.
- Key/fill/rim lighting.
- Floor, backdrop and rune pillars.
- Forced renderer layer assignment for the generated elf.
- Explicit preview camera render after character creation/refresh.
- Larger cinematic preview frame.
- No baked reference image is used.

The V10 creator is still based on ERO's procedural character system; it is not an AAA asset replacement.
