# ERO Aetheria - generate the 11-map world shell set in UE5.8
import unreal

MAPS = [
("Aetheria_CompleteWorld","Aetheria Capital"),
("Aetheria_StarterPlains","Starter Plains"),
("Aetheria_WhisperingForest","Whispering Forest"),
("Aetheria_SunscarDesert","Sunscar Desert"),
("Aetheria_FrostpeakMountains","Frostpeak Mountains"),
("Aetheria_MireOfCorruption","Mire of Corruption"),
("Aetheria_AncientRuins","Ancient Ruins"),
("Aetheria_ArcaneHighlands","Arcane Highlands"),
("Aetheria_PvPGvGFrontier","PvP / GvG Frontier"),
("Aetheria_AbyssalDepths","Abyssal Depths"),
("Aetheria_DragonSanctum","Dragon Sanctum"),
]

def generate():
    levels = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
    for asset_name, display_name in MAPS:
        path = "/Game/Maps/" + asset_name
        if unreal.EditorAssetLibrary.does_asset_exist(path):
            unreal.log("[ERO MAPS] Exists: " + display_name)
            continue
        if not levels.new_level(path, True):
            raise RuntimeError("Unable to create " + path)
        levels.save_current_level()
        unreal.log("[ERO MAPS] Created: " + display_name + " -> " + path)

    unreal.log("[ERO MAPS] 11-map shell set ready. Populate each map from the world manifest.")

generate()
