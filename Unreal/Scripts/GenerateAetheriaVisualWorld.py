# ERO Aetheria - deterministic UE5 visual world generator
# Run in Unreal Editor 5.8 with PythonScriptPlugin and EditorScriptingUtilities enabled.

import math
import unreal

MAP_PATH = "/Game/Maps/Aetheria_VisualWorld"
ASSET_PATH = "/Game/ERO/Visual/Generated"

def log(msg):
    unreal.log("[ERO VisualWorld] " + msg)

def make_material(name, color, roughness=0.75, metallic=0.0):
    tools = unreal.AssetToolsHelpers.get_asset_tools()
    existing = unreal.load_asset(f"{ASSET_PATH}/{name}.{name}")
    if existing:
        return existing
    material = tools.create_asset(name, ASSET_PATH, unreal.Material, unreal.MaterialFactoryNew())
    if not material:
        raise RuntimeError(f"Could not create material {name}")

    node = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant3Vector, -300, 0)
    node.constant = unreal.LinearColor(color[0], color[1], color[2], 1.0)
    unreal.MaterialEditingLibrary.connect_material_property(
        node, "", unreal.MaterialProperty.MP_BASE_COLOR)

    rough = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant, -300, 160)
    rough.r = roughness
    unreal.MaterialEditingLibrary.connect_material_property(
        rough, "", unreal.MaterialProperty.MP_ROUGHNESS)

    metal = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant, -300, 320)
    metal.r = metallic
    unreal.MaterialEditingLibrary.connect_material_property(
        metal, "", unreal.MaterialProperty.MP_METALLIC)

    unreal.MaterialEditingLibrary.recompile_material(material)
    unreal.EditorAssetLibrary.save_loaded_asset(material)
    return material

def spawn_mesh(mesh, location, scale=(1,1,1), material=None, rotation=(0,0,0), label=None):
    subsystem = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    actor = subsystem.spawn_actor_from_class(
        unreal.StaticMeshActor,
        unreal.Vector(*location),
        unreal.Rotator(*rotation),
        False)
    if not actor:
        return None
    actor.set_actor_label(label or "ERO_Generated")
    comp = actor.get_editor_property("static_mesh_component")
    comp.set_editor_property("static_mesh", mesh)
    comp.set_world_scale3d(unreal.Vector(*scale))
    if material:
        comp.set_material(0, material)
    return actor

def spawn_actor(actor_class, location=(0,0,0), rotation=(0,0,0), label=None):
    subsystem = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    actor = subsystem.spawn_actor_from_class(
        actor_class, unreal.Vector(*location), unreal.Rotator(*rotation), False)
    if actor and label:
        actor.set_actor_label(label)
    return actor

def setup_lighting():
    sun = spawn_actor(unreal.DirectionalLight, (0,0,1800), (-48,-35,0), "ERO_Sun_Lumen")
    if sun:
        comp = sun.get_component_by_class(unreal.DirectionalLightComponent)
        if comp:
            comp.set_editor_property("mobility", unreal.ComponentMobility.MOVABLE)
            comp.set_editor_property("intensity", 8.0)
            comp.set_editor_property("cast_shadows", True)

    sky = spawn_actor(unreal.SkyLight, (0,0,1000), label="ERO_SkyLight_Lumen")
    if sky:
        comp = sky.get_component_by_class(unreal.SkyLightComponent)
        if comp:
            comp.set_editor_property("mobility", unreal.ComponentMobility.MOVABLE)
            comp.set_editor_property("intensity", 1.0)
            comp.recapture_sky()

    spawn_actor(unreal.SkyAtmosphere, label="ERO_SkyAtmosphere")

    fog = spawn_actor(unreal.ExponentialHeightFog, label="ERO_AtmosphericFog")
    if fog:
        comp = fog.get_component_by_class(unreal.ExponentialHeightFogComponent)
        if comp:
            comp.set_editor_property("fog_density", 0.008)
            comp.set_editor_property("fog_height_falloff", 0.2)

    pp = spawn_actor(unreal.PostProcessVolume, label="ERO_Lumen_PostProcess")
    if pp:
        pp.set_editor_property("unbound", True)

def build_world():
    levels = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
    if not levels.new_level(MAP_PATH, True):
        raise RuntimeError("Unable to create partitioned Aetheria level")

    cube = unreal.load_asset("/Engine/BasicShapes/Cube.Cube")
    sphere = unreal.load_asset("/Engine/BasicShapes/Sphere.Sphere")
    cylinder = unreal.load_asset("/Engine/BasicShapes/Cylinder.Cylinder")
    cone = unreal.load_asset("/Engine/BasicShapes/Cone.Cone")
    plane = unreal.load_asset("/Engine/BasicShapes/Plane.Plane")

    mats = {
        "capital": make_material("MAT_Capital", (0.12,0.16,0.22), 0.62),
        "plains": make_material("MAT_Plains", (0.18,0.34,0.12), 0.86),
        "forest": make_material("MAT_Forest", (0.05,0.20,0.10), 0.88),
        "desert": make_material("MAT_Desert", (0.58,0.34,0.12), 0.91),
        "snow": make_material("MAT_Snow", (0.72,0.82,0.90), 0.93),
        "stone": make_material("MAT_Stone", (0.24,0.27,0.31), 0.78),
        "crystal": make_material("MAT_Crystal", (0.20,0.65,0.95), 0.18, 0.15),
        "gold": make_material("MAT_Gold", (0.75,0.46,0.08), 0.28, 0.72),
        "wood": make_material("MAT_Wood", (0.22,0.10,0.04), 0.86),
    }

    districts = [
        ("Capital",(0,0),mats["capital"]),
        ("Plains",(0,6500),mats["plains"]),
        ("Forest",(-6500,0),mats["forest"]),
        ("Desert",(6500,0),mats["desert"]),
        ("Snow",(0,-6500),mats["snow"]),
    ]

    for name,(cx,cy),mat in districts:
        spawn_mesh(plane,(cx,cy,-30),(65,65,1),mat,label=f"ERO_{name}_Ground")

    spawn_mesh(cube,(0,0,40),(12,12,0.5),mats["stone"],label="ERO_Capital_Plaza")

    for angle in range(0,360,30):
        r=1900
        x=math.cos(math.radians(angle))*r
        y=math.sin(math.radians(angle))*r
        spawn_mesh(cylinder,(x,y,120),(2,2,2.8),mats["stone"],label="ERO_Capital_Pillar")

    for axis in ("x","y"):
        for sign in (-1,1):
            if axis=="x":
                spawn_mesh(cube,(sign*4200,0,5),(25,3,0.08),mats["stone"],label="ERO_Road")
            else:
                spawn_mesh(cube,(0,sign*4200,5),(3,25,0.08),mats["stone"],label="ERO_Road")

    for district_index,(name,(cx,cy),_) in enumerate(districts):
        for i in range(36):
            angle=math.radians((i*137+district_index*53)%360)
            radius=900+((i*211)%2100)
            x=cx+math.cos(angle)*radius
            y=cy+math.sin(angle)*radius
            z=90+((i*17+district_index*29)%90)
            if name in ("Forest","Plains"):
                spawn_mesh(cylinder,(x,y,z),(0.7,0.7,2.5),mats["wood"],label="ERO_TreeTrunk")
                spawn_mesh(sphere,(x,y,z+450),(2.3,2.3,2.0),mats["forest"],label="ERO_TreeCrown")
            elif name=="Desert":
                spawn_mesh(cone,(x,y,z),(1.5,1.5,3.5),mats["desert"],label="ERO_Dune")
            elif name=="Snow":
                spawn_mesh(sphere,(x,y,z+180),(2.4,2.4,1.5),mats["snow"],label="ERO_SnowDrift")
            else:
                spawn_mesh(cube,(x,y,z),(1,1,1),mats["gold"],label="ERO_Capital_Detail")

    for name,(cx,cy),_ in districts:
        for i in range(8):
            a=math.radians(i*45)
            x=cx+math.cos(a)*2400
            y=cy+math.sin(a)*2400
            spawn_mesh(cube,(x,y,260),(0.8,0.8,4),mats["crystal"],(0,(i*17)%360,0),f"ERO_{name}_Crystal")

    gates=[
        ("Dungeon_North",(0,3100)),
        ("Dungeon_South",(0,-3100)),
        ("Raid_West",(-3100,0)),
        ("MVP_East",(3100,0)),
    ]
    for name,(x,y) in gates:
        spawn_mesh(cube,(x,y,250),(5,1,6),mats["stone"],label=f"ERO_{name}_Gate")
        spawn_mesh(sphere,(x,y,900),(2.2,2.2,2.2),mats["crystal"],label=f"ERO_{name}_Core")

    setup_lighting()
    levels.save_current_level()
    log("Generated " + MAP_PATH + " with World Partition.")

build_world()
