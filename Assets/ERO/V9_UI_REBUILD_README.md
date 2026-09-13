# ERO V9 UI Rebuild

This patch replaces the V536 presentation layer rather than stacking another canvas.

Changes:
- Original ERO character creation layout built as native Unity UI.
- No character-creation reference image is used as a background.
- Dedicated center RenderTexture character preview.
- Left class selection and right customization.
- Bottom character-name/create strip.
- Cleaner in-game HUD with player card, quest tracker, target, map, action bar and menu.
- F8 reopens character creation for testing.
- First launch after an older profile shows the new creator once, then persistence resumes.

The 3D world remains the existing V536 procedural test world. Final production art/animation remains a separate asset gate.
