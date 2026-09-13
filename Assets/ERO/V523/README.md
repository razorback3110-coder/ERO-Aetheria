# ERO V523
Unity reference: 6000.0.67f1

Based on V522. This build fixes Character Select model resolution and presentation.
- Loads ERO's 16 class/gender OBJ models from Resources before any Boss Room fallback.
- Rebuilds preview materials from Skin/Cloth/Accent/Dark/Hair/Metal slots to prevent the white character issue.
- Keeps Male/Female and Hair 1/2/3.
- Redesigned Character Select presentation.
- Adds a non-invasive ERO visual skin to MainMenu without replacing Boss Room/network controls.
- Preserves V522 CJK font runtime.

Use a clean import when moving versions: do not copy Library/Temp/obj.
