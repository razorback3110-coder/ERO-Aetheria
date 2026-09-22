# ERO Aetheria - canonical single-world generator for UE5.8
# Generates ONE persistent Aetheria world containing 11 connected cities/regions.
# PvP/GvG, Towers and instanced Dungeons are intentionally separate maps.

import math
import unreal

MAP_PATH = "/Game/Maps/Aetheria_CompleteWorld"
ROOT = "/Game/ERO/Visual/Generated"

CITIES = [
    ("AetheriaCapital","Aetheria Capital",0,0,4200,(0.12,0.16,0.22)),
    ("ValoriaPlains","Valoria Plains",0,7200,7200,(0.18,0.34,0.12)),
    ("Elderglen","Elderglen",-7200,0,7200,(0.04,0.20,0.08)),
    ("Sunscar","Sunscar",7200,0,7200,(0.58,0.34,0.12)),
    ("Frostheim","Frostheim",0,-7200,7200,(0.72,0.82,0.90)),
    ("Mirehaven","Mirehaven",-7200,-7200,7200,(0.06,0.16,0.10)),
    ("Arkenfall","Arkenfall",7200,7200,8500,(0.25,0.25,0.27)),
    ("Astralis","Astralis",-14400,0,8500,(0.16,0.08,0.30)),
    ("Duskmoor","Duskmoor",14400,0,8500,(0.12,0.10,0.12)),
    ("Abyssia","Abyssia",0,-14400,9000,(0.08,0.04,0.10)),
    ("Drakoria","Drakoria",0,14400,9000,(0.30,0.08,0.04)),
]

def asset(path):
    obj = unreal.load_asset(path)
    if not obj:
        raise RuntimeError("Missing asset: " + path)
    return obj

def material(name, rgb):
    path = ROOT + "/" + name
    existing = unreal.load_asset(path + "." + name)
    if existing:
        return existing
    tools = unreal.AssetToolsHelpers.get_asset_tools()
    m = tools.create_asset(name, ROOT, unreal.Material, unreal.MaterialFactoryNew())
    node = unreal.MaterialEditingLibrary.create_material_expression(m, unreal.MaterialExpressionConstant3Vector, -400, 0)
    node.constant = unreal.LinearColor(rgb[0], rgb[1], rgb[2], 1)
    unreal.MaterialEditingLibrary.connect_material_property(node, "", unreal.MaterialProperty.MP_BASE_COLOR)
    unreal.MaterialEditingLibrary.recompile_material(m)
    unreal.EditorAssetLibrary.save_loaded_asset(m)
    return m

def spawn(mesh, location, scale, mat, label, rotation=(0,0,0)):
    actors = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    a = actors.spawn_actor_from_class(unreal.StaticMeshActor, unreal.Vector(*location), unreal.Rotator(*rotation), False)
    if not a: return
    a.set_actor_label(label)
    c = a.get_editor_property("static_mesh_component")
    c.set_editor_property("static_mesh", mesh)
    c.set_world_scale3d(unreal.Vector(*scale))
    if mat: c.set_material(0, mat)
    return a

def city(cube, cone, sphere, name, cx, cy, radius, mat):
    # City plaza and a recognizable civic core.
    spawn(cube,(cx,cy,80),(12,12,0.8),mat,"ERO_CITY_"+name+"_PLAZA")
    spawn(cube,(cx,cy,400),(5,5,6),mat,"ERO_CITY_"+name+"_HALL")
    spawn(cone,(cx,cy,1000),(6,6,3),mat,"ERO_CITY_"+name+"_LANDMARK")
    # Deterministic ring of buildings and four gates.
    for i in range(24):
        a=math.radians(i*15)
        r=1100 + (i%4)*180
        x=cx+math.cos(a)*r; y=cy+math.sin(a)*r
        spawn(cube,(x,y,220),(1.4,1.4,2.2),mat,f"ERO_CITY_{name}_BUILDING_{i}")
        if i%6==0:
            spawn(cone,(x,y,600),(1.8,1.8,1.6),mat,f"ERO_CITY_{name}_TOWER_{i}")
    for i,(dx,dy) in enumerate(((0,radius),(radius,0),(0,-radius),(-radius,0))):
        spawn(cube,(cx+dx,cy+dy,280),(2.2,5.0,5.0),mat,f"ERO_CITY_{name}_GATE_{i}")

def road(cube, mat, a, b, width=260):
    ax,ay=a; bx,by=b; dx=bx-ax; dy=by-ay
    length=max(100,math.sqrt(dx*dx+dy*dy))
    ang=math.degrees(math.atan2(dy,dx))
    spawn(cube,((ax+bx)/2,(ay+by)/2,10),(length/100,width/100,0.08),mat,"ERO_WORLD_ROAD")

def add_lighting():
    """Create a complete UE5 preview lighting stack so the generated world is visible."""
    actors = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)

    sun = actors.spawn_actor_from_class(
        unreal.DirectionalLight,
        unreal.Vector(0, 0, 8000),
        unreal.Rotator(-35, -35, 0),
    )
    if sun:
        sun.set_actor_label("ERO_Sun_Directional")
        comp = sun.get_editor_property("directional_light_component")
        comp.set_editor_property("intensity", 8.0)
        comp.set_editor_property("cast_shadows", True)
        comp.set_editor_property("atmosphere_sun_light", True)
        comp.set_editor_property("affects_world", True)

    sky = actors.spawn_actor_from_class(unreal.SkyLight, unreal.Vector(0, 0, 5000))
    if sky:
        sky.set_actor_label("ERO_SkyLight")
        comp = sky.get_editor_property("sky_light_component")
        comp.set_editor_property("intensity", 1.0)
        comp.set_editor_property("real_time_capture", True)

    atmosphere = actors.spawn_actor_from_class(unreal.SkyAtmosphere, unreal.Vector(0, 0, 0))
    if atmosphere:
        atmosphere.set_actor_label("ERO_SkyAtmosphere")

    fog = actors.spawn_actor_from_class(unreal.ExponentialHeightFog, unreal.Vector(0, 0, 0))
    if fog:
        fog.set_actor_label("ERO_HeightFog")
        comp = fog.get_editor_property("component")
        comp.set_editor_property("fog_density", 0.003)
        comp.set_editor_property("fog_height_falloff", 0.2)

    pp = actors.spawn_actor_from_class(unreal.PostProcessVolume, unreal.Vector(0, 0, 0))
    if pp:
        pp.set_actor_label("ERO_PostProcess")
        pp.set_editor_property("unbound", True)
        settings = pp.get_editor_property("settings")
        settings.set_editor_property("auto_exposure_method", unreal.AutoExposureMethod.AEM_HISTOGRAM)
        settings.set_editor_property("auto_exposure_min_brightness", 0.5)
        settings.set_editor_property("auto_exposure_max_brightness", 2.0)
        pp.set_editor_property("settings", settings)


def build():
    levels=unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
    if not levels.new_level(MAP_PATH,True):
        raise RuntimeError("Unable to create " + MAP_PATH)
    cube=asset("/Engine/BasicShapes/Cube.Cube")
    cone=asset("/Engine/BasicShapes/Cone.Cone")
    sphere=asset("/Engine/BasicShapes/Sphere.Sphere")
    mats={cid:material("MAT_"+cid,rgb) for cid,_,_,_,_,rgb in CITIES}
    roadmat=material("MAT_WorldRoad",(0.10,0.10,0.11))
    add_lighting()

    for cid,name,cx,cy,radius,rgb in CITIES:
        spawn(cube,(cx,cy,-40),(radius/100,radius/100,0.5),mats[cid],"ERO_REGION_"+cid+"_GROUND")
        city(cube,cone,sphere,cid,cx,cy,radius,mats[cid])

    for i in range(1,len(CITIES)):
        road(cube,roadmat,(CITIES[0][2],CITIES[0][3]),(CITIES[i][2],CITIES[i][3]))

    # Waypoints are part of the same world; region travel never changes map.
    for cid,name,cx,cy,_,_ in CITIES:
        spawn(sphere,(cx,cy,120),(2,2,2),mats[cid],"ERO_WAYPOINT_"+cid)

    director_class=unreal.load_class(None,"/Script/EROAetheria.EROWorldMapDirector")
    if director_class:
        a=unreal.get_editor_subsystem(unreal.EditorActorSubsystem).spawn_actor_from_class(
            director_class,unreal.Vector(0,0,150))
        if a: a.set_actor_label("ERO_WorldMapDirector")

    levels.save_current_level()
    unreal.log("[ERO WORLD] Single Aetheria world generated: 11 connected cities. No PvP/GvG region.")

build()
