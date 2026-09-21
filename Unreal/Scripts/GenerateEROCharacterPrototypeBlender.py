"""
ERO character prototype modeling bootstrap for Blender 4.x.

Purpose:
- Generate eight distinct low-poly class mannequins.
- Give each class a recognizable armor silhouette and weapon prototype.
- Keep all geometry original and procedural.
- Export-ready for later replacement by production-quality licensed character assets.

This is a prototype generator, not the final AAA character art pipeline.
Run from Blender's Scripting workspace.
"""

import bpy
import math
from mathutils import Vector

CLASSES = {
    "Warrior": ("Ironheart Vanguard", "greatsword"),
    "Ranger": ("Wildstrider Scout", "bow"),
    "Mage": ("Astral Weaver", "staff"),
    "Assassin": ("Nightveil Stalker", "dual_blades"),
    "Cleric": ("Dawnkeeper Vestments", "mace"),
    "Paladin": ("Sunwarden Aegis", "sword_shield"),
    "Warlock": ("Voidbinder Regalia", "scythe"),
    "Summoner": ("Ethercaller Mantle", "orb"),
}

def mat(name, rgba):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = (*rgba, 1.0)
    return m

def cube(name, loc, scale, material):
    bpy.ops.mesh.primitive_cube_add(location=loc)
    o = bpy.context.object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(material)
    return o

def uv(name, loc, scale, material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=20, ring_count=12, location=loc)
    o = bpy.context.object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(material)
    return o

def cylinder(name, loc, radius, depth, material):
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=radius, depth=depth, location=loc)
    o = bpy.context.object
    o.name = name
    o.data.materials.append(material)
    return o

def make_weapon(kind, origin, metal, accent):
    x, y, z = origin
    if kind in ("staff", "scythe"):
        shaft = cylinder(f"Weapon_{kind}_Shaft", (x, y, z+1.2), 0.06, 2.4, metal)
        shaft.rotation_euler[1] = math.radians(2)
        if kind == "staff":
            uv("Weapon_staff_Focus", (x, y, z+2.45), (0.22,0.22,0.22), accent)
        else:
            cube("Weapon_scythe_Blade", (x+0.25,y,z+2.1), (0.08,0.35,0.65), accent)
    elif kind == "bow":
        cube("Weapon_bow_Limb", (x,y,z+1.3), (0.05,0.45,0.9), metal)
    elif kind == "dual_blades":
        for side in (-0.22,0.22):
            cube("Weapon_DualBlade", (x+side,y,z+1.1), (0.04,0.04,0.65), metal)
    elif kind == "orb":
        uv("Weapon_Orb", (x,y,z+1.45), (0.35,0.35,0.35), accent)
    elif kind == "shield":
        uv("Weapon_Shield", (x+0.45,y,z+1.0), (0.55,0.18,0.7), metal)
    else:
        cube(f"Weapon_{kind}", (x+0.45,y,z+1.25), (0.08,0.08,0.8), metal)

def make_character(class_id, outfit, weapon_kind, origin):
    ox, oy, oz = origin
    skin = mat(f"{class_id}_Skin", (0.68,0.50,0.38))
    armor = mat(f"{class_id}_Armor", (0.16,0.20,0.28))
    accent = mat(f"{class_id}_Accent", (0.65,0.42,0.12))
    metal = mat(f"{class_id}_Metal", (0.45,0.48,0.52))

    root = bpy.data.objects.new(f"ERO_{class_id}", None)
    bpy.context.collection.objects.link(root)

    torso = cube(f"{class_id}_Torso", (ox,oy,oz+1.15), (0.42,0.25,0.55), armor)
    head = uv(f"{class_id}_Head", (ox,oy,oz+1.95), (0.28,0.25,0.28), skin)
    belt = cube(f"{class_id}_Belt", (ox,oy,oz+0.82), (0.46,0.27,0.10), accent)

    for side in (-1,1):
        cube(f"{class_id}_Arm", (ox+side*0.58,oy,oz+1.2), (0.16,0.16,0.48), armor)
        cube(f"{class_id}_Leg", (ox+side*0.20,oy,oz+0.25), (0.17,0.20,0.55), armor)

    # Distinctive silhouette accents by class.
    if class_id in ("Warrior","Paladin"):
        cube(f"{class_id}_ShoulderL", (ox-0.52,oy,oz+1.55), (0.25,0.25,0.18), metal)
        cube(f"{class_id}_ShoulderR", (ox+0.52,oy,oz+1.55), (0.25,0.25,0.18), metal)
    elif class_id == "Ranger":
        cube(f"{class_id}_Cloak", (ox,oy+0.20,oz+1.35), (0.45,0.05,0.75), accent)
    elif class_id == "Mage":
        cube(f"{class_id}_Mantle", (ox,oy,oz+1.62), (0.62,0.32,0.10), accent)
    elif class_id == "Assassin":
        cube(f"{class_id}_Hood", (ox,oy,oz+2.18), (0.33,0.30,0.18), armor)
    elif class_id == "Cleric":
        cube(f"{class_id}_Stole", (ox,oy-0.28,oz+1.45), (0.08,0.05,0.70), accent)
    elif class_id == "Warlock":
        cube(f"{class_id}_Mantle", (ox,oy+0.18,oz+1.35), (0.55,0.06,0.65), armor)
    elif class_id == "Summoner":
        for angle in range(0,360,90):
            a = math.radians(angle)
            uv(f"{class_id}_PactFocus", (ox+math.cos(a)*0.55, oy+math.sin(a)*0.55, oz+1.55), (0.10,0.10,0.10), accent)

    make_weapon(weapon_kind, (ox+0.45,oy,oz), metal, accent)

    for obj in list(bpy.context.scene.objects):
        if obj.name.startswith(class_id + "_") or obj.name.startswith("Weapon_"):
            if obj.parent is None:
                obj.parent = root

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

clear_scene()

for i, (class_id, (outfit, weapon)) in enumerate(CLASSES.items()):
    col = i % 4
    row = i // 4
    make_character(class_id, outfit, weapon, (col*3.0, row*3.5, 0.0))

bpy.context.scene["ERO_Prototype"] = True
bpy.context.scene["ERO_CharacterCount"] = len(CLASSES)
print("ERO: generated eight original class character prototypes.")
