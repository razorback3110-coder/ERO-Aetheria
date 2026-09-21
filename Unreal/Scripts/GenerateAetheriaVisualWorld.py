# ERO Aetheria - COMPLETE deterministic UE5 world generator
# Unreal Engine 5.8 / Editor Python
#
# Generates the complete Aetheria launch-world scaffold in one World Partition map.
# It intentionally uses UE built-in meshes and ERO-authored materials only.
# Production assets can replace generated actors later without changing world rules.
#
# WORLD REGIONS:
#   Aetheria Capital
#   Starter Plains
#   Whispering Forest
#   Frostpeak Mountains
#   Sunscar Desert
#   Mire of Corruption
#   Ancient Ruins
#   Arcane Highlands
#   Dungeon/raid gateways
#   MVP/world-boss territories
#   PvP/GvG frontier
#
# This is editor generation, not runtime spawning. Gameplay/server code remains
# separate from visual world authoring.

import math
import unreal

MAP_PATH = "/Game/Maps/Aetheria_CompleteWorld"
ASSET_PATH = "/Game/ERO/Visual/Generated"

def log(message):
    unreal.log("[ERO WORLD] " + str(message))

def load(path):
    asset = unreal.load_asset(path)
    if not asset:
        raise RuntimeError("Missing Unreal asset: " + path)
    return asset

def make_material(name, rgb, roughness=0.8, metallic=0.0):
    tools = unreal.AssetToolsHelpers.get_asset_tools()
    package_path = ASSET_PATH
    existing = unreal.load_asset(f"{package_path}/{name}.{name}")
    if existing:
        return existing

    material = tools.create_asset(
        name, package_path, unreal.Material, unreal.MaterialFactoryNew())
    if not material:
        raise RuntimeError("Unable to create material " + name)

    base = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant3Vector, -400, 0)
    base.constant = unreal.LinearColor(rgb[0], rgb[1], rgb[2], 1.0)
    unreal.MaterialEditingLibrary.connect_material_property(
        base, "", unreal.MaterialProperty.MP_BASE_COLOR)

    rough = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant, -400, 160)
    rough.r = roughness
    unreal.MaterialEditingLibrary.connect_material_property(
        rough, "", unreal.MaterialProperty.MP_ROUGHNESS)

    metal = unreal.MaterialEditingLibrary.create_material_expression(
        material, unreal.MaterialExpressionConstant, -400, 320)
    metal.r = metallic
    unreal.MaterialEditingLibrary.connect_material_property(
        metal, "", unreal.MaterialProperty.MP_METALLIC)

    unreal.MaterialEditingLibrary.recompile_material(material)
    unreal.EditorAssetLibrary.save_loaded_asset(material)
    return material

def spawn_mesh(mesh, location, scale=(1,1,1), material=None,
               rotation=(0,0,0), label="ERO_Generated"):
    subsystem = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    actor = subsystem.spawn_actor_from_class(
        unreal.StaticMeshActor,
        unreal.Vector(*location),
        unreal.Rotator(*rotation),
        False)
    if not actor:
        return None
    actor.set_actor_label(label)
    component = actor.get_editor_property("static_mesh_component")
    component.set_editor_property("static_mesh", mesh)
    component.set_world_scale3d(unreal.Vector(*scale))
    if material:
        component.set_material(0, material)
    return actor

def spawn_actor(actor_class, location=(0,0,0), rotation=(0,0,0), label="ERO_Generated"):
    subsystem = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    actor = subsystem.spawn_actor_from_class(
        actor_class, unreal.Vector(*location), unreal.Rotator(*rotation), False)
    if actor:
        actor.set_actor_label(label)
    return actor

def add_light_stack():
    sun = spawn_actor(unreal.DirectionalLight, (0,0,3000), (-48,-32,0), "ERO_Sun_Lumen")
    if sun:
        c = sun.get_component_by_class(unreal.DirectionalLightComponent)
        if c:
            c.set_editor_property("mobility", unreal.ComponentMobility.MOVABLE)
            c.set_editor_property("intensity", 8.0)
            c.set_editor_property("cast_shadows", True)

    sky = spawn_actor(unreal.SkyLight, (0,0,1800), label="ERO_SkyLight_Lumen")
    if sky:
        c = sky.get_component_by_class(unreal.SkyLightComponent)
        if c:
            c.set_editor_property("mobility", unreal.ComponentMobility.MOVABLE)
            c.set_editor_property("intensity", 1.0)
            c.recapture_sky()

    spawn_actor(unreal.SkyAtmosphere, label="ERO_SkyAtmosphere")

    fog = spawn_actor(unreal.ExponentialHeightFog, label="ERO_AtmosphericFog")
    if fog:
        c = fog.get_component_by_class(unreal.ExponentialHeightFogComponent)
        if c:
            c.set_editor_property("fog_density", 0.006)
            c.set_editor_property("fog_height_falloff", 0.22)

    pp = spawn_actor(unreal.PostProcessVolume, label="ERO_Lumen_PostProcess")
    if pp:
        pp.set_editor_property("unbound", True)

def make_zone_ground(plane, zone, mats):
    name, cx, cy, radius, mat_key = zone
    spawn_mesh(
        plane, (cx, cy, -60), (radius/50.0, radius/50.0, 1),
        mats[mat_key], label=f"ERO_ZONE_{name}_GROUND")

    # Raised perimeter markers make the zones readable from above.
    for i in range(16):
        a = math.radians(i * 22.5)
        x = cx + math.cos(a) * radius
        y = cy + math.sin(a) * radius
        spawn_mesh(
            mats["_cube_mesh"], (x,y,80), (2.5,2.5,1.5),
            mats["stone"], (0,(i*22.5),0),
            f"ERO_ZONE_{name}_BOUNDARY")

def make_road(cube, mats, a, b, width=180.0, z=8.0, label="ERO_Road"):
    ax, ay = a
    bx, by = b
    dx, dy = bx-ax, by-ay
    length = math.sqrt(dx*dx + dy*dy)
    angle = math.degrees(math.atan2(dy, dx))
    spawn_mesh(
        cube,
        ((ax+bx)/2, (ay+by)/2, z),
        (length/100.0, width/100.0, 0.08),
        mats["road"], (0,0,angle), label)

def make_building(cube, cone, mats, x, y, scale, material_key, label):
    sx, sy, sz = scale
    spawn_mesh(cube, (x,y,sz*50), (sx,sy,sz), mats[material_key], label=label)
    spawn_mesh(
        cone, (x,y,sz*100+80), (sx*1.15,sy*1.15,0.8),
        mats["roof"], (0,45,0), label=label+"_ROOF")

def make_tree(cylinder, sphere, mats, x, y, size=1.0, label="ERO_Tree"):
    spawn_mesh(cylinder, (x,y,120*size), (0.45*size,0.45*size,2.0*size),
               mats["wood"], label=label+"_TRUNK")
    spawn_mesh(sphere, (x,y,430*size), (1.6*size,1.6*size,1.35*size),
               mats["foliage"], label=label+"_CROWN")

def make_crystal(cube, sphere, mats, x, y, size=1.0, label="ERO_Crystal"):
    spawn_mesh(cube, (x,y,220*size), (0.55*size,0.55*size,2.6*size),
               mats["crystal"], (0,17,15), label=label)
    spawn_mesh(sphere, (x,y,520*size), (0.75*size,0.75*size,0.75*size),
               mats["crystal"], label=label+"_CORE")

def make_gate(cube, sphere, mats, x, y, label, color_key="crystal"):
    spawn_mesh(cube, (x-320,y,260), (1.2,2.0,5.0), mats["stone"], label=label+"_LEFT")
    spawn_mesh(cube, (x+320,y,260), (1.2,2.0,5.0), mats["stone"], label=label+"_RIGHT")
    spawn_mesh(cube, (x,y,650), (5.2,1.0,1.1), mats["stone"], label=label+"_TOP")
    spawn_mesh(sphere, (x,y,900), (1.5,1.5,1.5), mats[color_key], label=label+"_CORE")

def make_zone_dressing(zone_index, zone, meshes, mats):
    name, cx, cy, radius, mat_key = zone
    cylinder, sphere, cone, cube = meshes

    # Deterministic pseudo-random placement: same world every generation.
    for i in range(34):
        angle = math.radians((i * 137 + zone_index * 71) % 360)
        distance = 900 + ((i * 263 + zone_index * 191) % max(1000, int(radius*0.62)))
        x = cx + math.cos(angle) * distance
        y = cy + math.sin(angle) * distance

        if name in ("WhisperingForest", "StarterPlains"):
            make_tree(cylinder, sphere, mats, x, y, 0.8 + ((i % 4)*0.12),
                      f"ERO_{name}_TREE_{i}")
        elif name == "FrostpeakMountains":
            spawn_mesh(cone, (x,y,250), (2.5,2.5,5.0), mats["snow_rock"],
                       (0,(i*23)%360,0), f"ERO_{name}_PEAK_{i}")
        elif name == "SunscarDesert":
            spawn_mesh(cone, (x,y,150), (2.8,2.8,2.0), mats["sand"],
                       (0,(i*17)%360,0), f"ERO_{name}_DUNE_{i}")
        elif name == "MireOfCorruption":
            spawn_mesh(cylinder, (x,y,100), (1.2,1.2,1.0), mats["corrupt"],
                       label=f"ERO_{name}_MIRE_{i}")
            make_crystal(cube, sphere, mats, x+80, y-120, 0.8,
                         f"ERO_{name}_CRYSTAL_{i}")
        elif name == "AncientRuins":
            spawn_mesh(cube, (x,y,250), (1.2,1.2,4.5), mats["ruin"],
                       (0,(i*31)%360,0), f"ERO_{name}_COLUMN_{i}")
        elif name == "ArcaneHighlands":
            make_crystal(cube, sphere, mats, x, y, 1.2,
                         f"ERO_{name}_CRYSTAL_{i}")
        elif name == "PvPGvGFrontier":
            make_building(cube, cone, mats, x, y, (1.8,1.8,3.0),
                          "fort", f"ERO_{name}_FORT_{i}")
        else:
            # Capital receives denser civic details.
            make_building(cube, cone, mats, x, y, (1.4,1.4,2.5),
                          "building", f"ERO_{name}_BUILDING_{i}")

def make_capital(cube, cylinder, sphere, cone, mats):
    spawn_mesh(cube, (0,0,60), (16,16,0.6), mats["stone"], label="ERO_Capital_Plaza")

    # Main palace/central landmark.
    make_building(cube, cone, mats, 0, 0, (5.5,5.5,6.5),
                  "palace", "ERO_Capital_Palace")

    for i in range(24):
        a = math.radians(i*15)
        x = math.cos(a)*2600
        y = math.sin(a)*2600
        make_building(cube, cone, mats, x, y,
                      (1.1,1.1,2.1), "building",
                      f"ERO_Capital_Building_{i}")

    # Four monumental entrance gates.
    for label, (x,y) in {
        "N":(0,3600), "S":(0,-3600), "E":(3600,0), "W":(-3600,0)
    }.items():
        make_gate(cube, sphere, mats, x, y, f"ERO_Capital_Gate_{label}", "gold")

def make_dungeons(cube, sphere, mats):
    content = [
        ("Dungeon_Forest",( -7200, 1200)),
        ("Dungeon_Desert",( 7200, 1200)),
        ("Dungeon_Ruins",( -1200, 7200)),
        ("Dungeon_Snow",( 1200,-7200)),
        ("Raid_Abyss",( 0, 9300)),
        ("Raid_Dragon",( 9300, 0)),
        ("MVP_ElderTitan",( -9300, 0)),
        ("MVP_CorruptedKing",( 0,-9300)),
    ]
    for label,(x,y) in content:
        make_gate(cube,sphere,mats,x,y,"ERO_"+label,"crystal")

def make_pvp_frontier(cube, mats):
    x, y = 9300, 9300
    spawn_mesh(cube,(x,y,30),(30,30,0.25),mats["fort"],label="ERO_PvPGvG_Arena")
    for i in range(12):
        a=math.radians(i*30)
        px=x+math.cos(a)*2400
        py=y+math.sin(a)*2400
        make_building(cube, cube, mats, px, py, (1.4,1.4,4.0),
                      "fort", f"ERO_PvPGvG_Tower_{i}")

def build_complete_world():
    levels = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)

    if not levels.new_level(MAP_PATH, True):
        raise RuntimeError("Unable to create " + MAP_PATH)

    cube = load("/Engine/BasicShapes/Cube.Cube")
    sphere = load("/Engine/BasicShapes/Sphere.Sphere")
    cylinder = load("/Engine/BasicShapes/Cylinder.Cylinder")
    cone = load("/Engine/BasicShapes/Cone.Cone")
    plane = load("/Engine/BasicShapes/Plane.Plane")

    mats = {
        "capital": make_material("MAT_Capital",(0.12,0.16,0.22),0.60),
        "plains": make_material("MAT_Plains",(0.18,0.34,0.12),0.86),
        "forest": make_material("MAT_Forest",(0.04,0.20,0.08),0.88),
        "snow": make_material("MAT_Snow",(0.72,0.82,0.90),0.93),
        "desert": make_material("MAT_Desert",(0.58,0.34,0.12),0.91),
        "swamp": make_material("MAT_Swamp",(0.06,0.16,0.10),0.92),
        "ruins": make_material("MAT_Ruins",(0.25,0.25,0.27),0.84),
        "arcane": make_material("MAT_Arcane",(0.16,0.08,0.30),0.35,0.25),
        "frontier": make_material("MAT_Frontier",(0.16,0.14,0.12),0.82),
        "stone": make_material("MAT_Stone",(0.24,0.27,0.31),0.78),
        "road": make_material("MAT_Road",(0.12,0.12,0.13),0.90),
        "roof": make_material("MAT_Roof",(0.08,0.06,0.07),0.72),
        "building": make_material("MAT_Building",(0.34,0.36,0.40),0.76),
        "palace": make_material("MAT_Palace",(0.55,0.42,0.20),0.35,0.55),
        "fort": make_material("MAT_Fort",(0.22,0.18,0.16),0.82),
        "wood": make_material("MAT_Wood",(0.22,0.10,0.04),0.88),
        "foliage": make_material("MAT_Foliage",(0.04,0.24,0.08),0.90),
        "crystal": make_material("MAT_Crystal",(0.18,0.65,0.95),0.18,0.18),
        "gold": make_material("MAT_Gold",(0.75,0.46,0.08),0.28,0.72),
        "snow_rock": make_material("MAT_SnowRock",(0.48,0.56,0.62),0.90),
        "sand": make_material("MAT_Sand",(0.62,0.38,0.14),0.95),
        "corrupt": make_material("MAT_Corrupt",(0.12,0.04,0.18),0.50,0.15),
        "ruin": make_material("MAT_Ruin",(0.30,0.29,0.26),0.92),
    }
    mats["_cube_mesh"] = cube

    zones = [
        ("AetheriaCapital",0,0,4200,"capital"),
        ("StarterPlains",0,6200,4200,"plains"),
        ("WhisperingForest",-6200,0,4200,"forest"),
        ("FrostpeakMountains",0,-6200,4200,"snow"),
        ("SunscarDesert",6200,0,4200,"desert"),
        ("MireOfCorruption",-6200,-6200,4200,"swamp"),
        ("AncientRuins",6200,6200,4200,"ruins"),
        ("ArcaneHighlands",-12000,0,4200,"arcane"),
        ("PvPGvGFrontier",12000,0,4200,"frontier"),
    ]

    for zone in zones:
        make_zone_ground(plane, zone, mats)

    # Major world roads create a connected launch-world navigation skeleton.
    connections = [
        ((0,0),(0,6200)), ((0,0),(-6200,0)),
        ((0,0),(0,-6200)), ((0,0),(6200,0)),
        ((-6200,0),(-6200,-6200)),
        ((6200,0),(6200,6200)),
        ((-6200,-6200),(-12000,0)),
        ((6200,6200),(12000,0)),
        ((0,6200),(6200,6200)),
        ((0,-6200),(-6200,-6200)),
    ]
    for i,(a,b) in enumerate(connections):
        make_road(cube,mats,a,b,220,10,f"ERO_WorldRoad_{i}")

    make_capital(cube,cylinder,sphere,cone,mats)
    meshes=(cylinder,sphere,cone,cube)
    for idx,zone in enumerate(zones):
        make_zone_dressing(idx,zone,meshes,mats)

    make_dungeons(cube,sphere,mats)
    make_pvp_frontier(cube,mats)

    # World center teleporter/waypoint network.
    for i,(x,y) in enumerate([
        (0,0),(0,6200),(-6200,0),(0,-6200),(6200,0),
        (-6200,-6200),(6200,6200),(-12000,0),(12000,0)
    ]):
        make_crystal(cube,sphere,mats,x,y,1.5,f"ERO_Waypoint_{i}")

    add_light_stack()

    # Place the runtime World Director in the generated launch map.
    # It owns server-authoritative world-event state; content actors can subscribe later.
    director_class = unreal.load_class(None, "/Script/EROAetheria.EROWorldDirector")
    if director_class:
        actor_subsystem = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
        director = actor_subsystem.spawn_actor_from_class(director_class, unreal.Vector(0, 0, 120))
        if director:
            director.set_actor_label("ERO_WorldDirector")

    levels.save_current_level()
    log("COMPLETE AETHERIA WORLD GENERATED: " + MAP_PATH)
    log("Zones: 9 major regions + capital + dungeons + raids + MVP + PvP/GvG frontier.")
    log("Next production step: replace generated primitives with licensed Nanite/PCG assets.")

build_complete_world()
