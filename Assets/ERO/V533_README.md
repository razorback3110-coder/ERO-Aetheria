# ERO V533 - Full Playable + Character Creation

V533 replaces the stacked V531/V532 runtime presentation with one master ERO layer.

## Main fixes
- Character Creation is restored directly in `ERO_Playable`.
- First launch opens Character Creation automatically when `ERO_CHARACTER_CREATED` is not set.
- `C` opens Character Creation at any time.
- Character class, gender, name and appearance choices are persisted.
- The same character model is used after creation in the playable world.
- V531 and V532 automatic runtime presentations are disabled to prevent duplicate HUDs.
- Build order starts with Startup -> MainMenu -> CharSelect -> ERO_Playable.

## Playable systems exposed in V533
- Third-person movement and camera
- 8 classes
- character creation / edit
- XP and levels 1-100
- HP / MP / combat / skills / auto combat
- enemies / targeting / loot
- zone travel and level gates for all 10 Aetheria zones
- inventory, equipment, quests, social/party, guild, pets, mounts, housing
- marketplace / trade / Cristaux ERO
- crafting
- PvP / Ranked / GvG / Raids / MVP / World Boss entry points
- achievements / collections / events / seasons
- settings / localization list
- local profile save

## Controls
WASD = move
Right mouse = camera
Left mouse / E = attack
1-8 = skills
C = character creator
I = inventory
Q = quests
M = nearest target
TAB = map
F1-F8 = class test switch
Shift = sprint

## Asset direction
The current project assets are retained so the Unity test remains self-contained. For production-quality character replacement, the project can use CC0 humanoid assets such as Quaternius Universal Base Characters and Modular Character Outfits Fantasy, both Humanoid/retarget friendly and compatible with Unity URP.


V534 FIX: repaired ERO_Playable scene component serialization and versioned character profile keys so Character Creation appears on first launch of this build.
