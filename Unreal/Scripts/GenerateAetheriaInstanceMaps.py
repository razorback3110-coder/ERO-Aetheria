# ERO Aetheria - create separate instance map shells
import unreal

MAPS = [
("Dungeon_Forest","/Game/Maps/Instances/Dungeon_Forest"),
("Dungeon_Desert","/Game/Maps/Instances/Dungeon_Desert"),
("Dungeon_Frost","/Game/Maps/Instances/Dungeon_Frost"),
("Dungeon_Abyss","/Game/Maps/Instances/Dungeon_Abyss"),
("Tower_01","/Game/Maps/Instances/Tower_01"),
("Tower_02","/Game/Maps/Instances/Tower_02"),
("Tower_03","/Game/Maps/Instances/Tower_03"),
("Tower_04","/Game/Maps/Instances/Tower_04"),
("Tower_05","/Game/Maps/Instances/Tower_05"),
("Arena_1v1","/Game/Maps/Instances/Arena_1v1"),
("Arena_4v4","/Game/Maps/Instances/Arena_4v4"),
("GvG_WarOfRealms","/Game/Maps/Instances/GvG_WarOfRealms"),
]

levels=unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
for name,path in MAPS:
    if unreal.EditorAssetLibrary.does_asset_exist(path):
        unreal.log("[ERO INSTANCES] Exists: "+name)
        continue
    if not levels.new_level(path,True):
        raise RuntimeError("Unable to create "+path)
    levels.save_current_level()
    unreal.log("[ERO INSTANCES] Created: "+name)
unreal.log("[ERO INSTANCES] Dungeon/Tower/PvP/GvG shells ready.")
