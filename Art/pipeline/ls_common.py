# ls_common.py — shared helpers for the LIGHT SHADOW Blender hero-prop pipeline.
#
# All pipeline scripts import this module (same folder) and agree on:
#   - repo-root discovery (--root argv flag or the two-levels-up default),
#   - the prop registry (export name, collection name, tri budget, class),
#   - the Unity material names the FBX files carry so the lead can remap
#     them 1:1 onto the existing Mat_Art_* assets (SceneBuilderArt names).
#
# Blender 5.3, headless (blender -b -P <script> -- --root <repo>).

import math
import os
import sys

import bpy
import bmesh
from mathutils import Vector, Matrix

# --------------------------------------------------------------- paths

def repo_root():
    """Repo root: --root argv flag wins, else Art/pipeline/../.."""
    argv = sys.argv
    if "--" in argv:
        rest = argv[argv.index("--") + 1:]
        for i, token in enumerate(rest):
            if token == "--root" and i + 1 < len(rest):
                return os.path.abspath(rest[i + 1])
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.abspath(os.path.join(here, "..", ".."))

ROOT = repo_root()
PIPELINE_DIR = os.path.join(ROOT, "Art", "pipeline")
SOURCES_DIR = os.path.join(ROOT, "Art", "Sources")
PREVIEWS_DIR = os.path.join(ROOT, "Art", "Previews")
BLEND_PATH = os.path.join(SOURCES_DIR, "HeroProps.blend")
HERO_FBX_DIR = os.path.join(ROOT, "Assets", "_Project", "Models", "Hero")
MANIFEST_PATH = os.path.join(PIPELINE_DIR, "manifest.json")

# --------------------------------------------------------- prop registry
# class: "hero" (<=30k tris) or "small" (<=3k tris) — STYLE_GUIDE/master plan
# §7.4 budgets. bake_transform: True for single-mesh props (master plan §7.4
# "Apply Transform on" — clean identity roots in Unity); False for the
# flashlight hierarchy so the Crank child keeps its pivot-ready local origin.

PROPS = [
    {"name": "SM_Story_MemoryBox",  "budget": 30000, "class": "hero",  "bake_transform": True},
    {"name": "SM_Story_Card",       "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_Bookmark",   "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_ExamPaper",  "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_Prism",      "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_Photograph", "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_Programme",  "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Story_Mug",        "budget": 3000,  "class": "small", "bake_transform": True},
    {"name": "SM_Player_Flashlight", "budget": 30000, "class": "hero", "bake_transform": False},
    {"name": "SM_Wraith_HD",        "budget": 30000, "class": "hero",  "bake_transform": True},
    {"name": "SM_Arch_DoorGrille",  "budget": 30000, "class": "hero",  "bake_transform": True},
    {"name": "SM_Arch_Chalkboard",  "budget": 30000, "class": "hero",  "bake_transform": True},
    {"name": "SM_Arch_GateCrank",   "budget": 30000, "class": "hero",  "bake_transform": True},
    {"name": "SM_Arch_IronGate",    "budget": 30000, "class": "hero",  "bake_transform": True},
]

PROP_NAMES = [p["name"] for p in PROPS]
PROP_BY_NAME = {p["name"]: p for p in PROPS}


def ensure_dirs():
    for path in (SOURCES_DIR, PREVIEWS_DIR, HERO_FBX_DIR):
        os.makedirs(path, exist_ok=True)


def collection_of(prop_name):
    """The prop collection by export name, or None."""
    coll = bpy.data.collections.get(prop_name)
    return coll


def collection_objects(prop_name):
    """All objects inside a prop collection (including children objects)."""
    coll = collection_of(prop_name)
    if coll is None:
        return []
    return list(coll.objects)


# ------------------------------------------------------------- materials
# Blender-side names match the Unity material asset names exactly so Unity's
# ModelImporter "search project-wide" remap matches them 1:1. Colours below
# are preview stand-ins only — Unity reuses the real Mat_Art_* PBR sets
# (no texture baking in this pipeline).
#
# Three names do NOT exist in Unity yet and are called out in the handover:
#   Mat_Story_RedInk (exam-paper strokes), Mat_Story_Glass (prism),
#   Mat_Story_LED (flashlight LED strip).

MATERIALS = {
    # name: (rgb, roughness, metallic, transmission)
    "Mat_Art_DoorWood":    ((0.30, 0.18, 0.10), 0.60, 0.0, 0.0),
    "Mat_Art_MetalLocker": ((0.62, 0.50, 0.28), 0.35, 0.8, 0.0),   # brass
    "Mat_Art_GateMetal":   ((0.32, 0.34, 0.38), 0.45, 0.9, 0.0),   # iron
    "Mat_Art_TorchMetal":  ((0.10, 0.11, 0.13), 0.45, 0.7, 0.0),
    "Mat_Art_PlasterWall": ((0.87, 0.85, 0.80), 0.80, 0.0, 0.0),   # paper
    "Mat_Art_Fabric":      ((0.45, 0.30, 0.28), 0.90, 0.0, 0.0),
    "Mat_Art_Chalkboard":  ((0.045, 0.085, 0.065), 0.35, 0.0, 0.0),
    "Mat_Art_Stone":       ((0.55, 0.54, 0.50), 0.85, 0.0, 0.0),
    "M_ShadowSmoke":       ((0.02, 0.02, 0.03), 0.80, 0.0, 0.0),
    "Mat_Story_RedInk":    ((0.55, 0.08, 0.06), 0.70, 0.0, 0.0),
    "Mat_Story_Glass":     ((0.85, 0.90, 0.95), 0.05, 0.0, 1.0),
    "Mat_Story_LED":       ((0.95, 0.85, 0.60), 0.30, 0.0, 0.0),
}


def build_materials():
    for name, (rgb, rough, metal, transmission) in MATERIALS.items():
        if name in bpy.data.materials:
            continue
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        bsdf = None
        for node in nodes:
            if node.type == "BSDF_PRINCIPLED":
                bsdf = node
                break
        if bsdf is None:
            continue
        bsdf.inputs["Base Color"].default_value = (rgb[0], rgb[1], rgb[2], 1.0)
        bsdf.inputs["Roughness"].default_value = rough
        bsdf.inputs["Metallic"].default_value = metal
        if "Transmission Weight" in bsdf.inputs:
            bsdf.inputs["Transmission Weight"].default_value = transmission
        # LED strip reads as a warm light in previews only; Unity owns the
        # real emission wiring (Mat_Story_LED remap).
        if name == "Mat_Story_LED" and "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (rgb[0], rgb[1], rgb[2], 1.0)
            if "Emission Strength" in bsdf.inputs:
                bsdf.inputs["Emission Strength"].default_value = 4.0


def assign(obj, mat_name):
    obj.data.materials.append(bpy.data.materials[mat_name])


# --------------------------------------------------------------- builders

def reset_scene():
    """Factory-empty scene: every run of ls_props starts from nothing, so the
    whole pipeline is idempotent by construction."""
    bpy.ops.wm.read_factory_settings(use_empty=True)


def new_collection(name):
    coll = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(coll)
    return coll


def _link(coll, obj):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    coll.objects.link(obj)


# Public alias: builders that create objects via raw bpy.ops (grids) link
# them into the prop collection themselves.
link_to = _link


def add_box(coll, name, size, center=(0, 0, 0), rot_z=0.0):
    """Axis-aligned box; size is the full extent per axis, center the box centre.
    (Blender 5.3's size=1.0 cube spans +/-0.5, so the object scale equals the
    full extent, not half of it.)"""
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (size[0], size[1], size[2])
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.location = (center[0], center[1], center[2])
    if rot_z:
        obj.rotation_euler = (0.0, 0.0, rot_z)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    _link(coll, obj)
    return obj


def add_cylinder(coll, name, radius, depth, center=(0, 0, 0), verts=24,
                 rot=(0.0, 0.0, 0.0), cap=True):
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius, depth=depth, vertices=verts, location=(0, 0, 0),
        end_fill_type="NGON" if cap else "NOTHING")
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.location = center
    obj.rotation_euler = rot
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    _link(coll, obj)
    return obj


def add_cone(coll, name, radius1, radius2, depth, center=(0, 0, 0), verts=12):
    bpy.ops.mesh.primitive_cone_add(
        radius1=radius1, radius2=radius2, depth=depth, vertices=verts, location=(0, 0, 0),
        end_fill_type="TRIFAN")
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.location = center
    _link(coll, obj)
    return obj


def add_torus(coll, name, major_radius, minor_radius, center=(0, 0, 0),
              major_segments=32, minor_segments=12, rot=(0.0, 0.0, 0.0)):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius, minor_radius=minor_radius,
        major_segments=major_segments, minor_segments=minor_segments, location=(0, 0, 0))
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.location = center
    obj.rotation_euler = rot
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    _link(coll, obj)
    return obj


def add_uv_sphere(coll, name, radius, center=(0, 0, 0), segments=16, rings=8,
                  scale=(1.0, 1.0, 1.0)):
    bpy.ops.mesh.primitive_uv_sphere_add(
        radius=radius, segments=segments, ring_count=rings, location=(0, 0, 0))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.location = center
    _link(coll, obj)
    return obj


def join_parts(parts, name):
    """Join part objects into one mesh named `name`, apply all transforms.
    Keeps the active object's transform."""
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.shade_flat()
    return obj


def set_base_origin(obj):
    """Move vertices so the bounding-box bottom centre sits at the object
    origin, and the object at the world origin (master plan §7.4 rule 2)."""
    bpy.context.view_layer.objects.active = obj
    bb = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    center = Vector((
        (min(v.x for v in bb) + max(v.x for v in bb)) * 0.5,
        (min(v.y for v in bb) + max(v.y for v in bb)) * 0.5,
        min(v.z for v in bb),
    ))
    obj.location = Vector((0.0, 0.0, 0.0)) - center
    bpy.ops.object.transform_apply(location=True, rotation=False, scale=False)
    obj.location = (0.0, 0.0, 0.0)


def smart_uv(obj, angle_deg=66.0, margin=0.02, layer_name="UVMap"):
    """Smart-project UVs on the given layer. UV0 = tiling-friendly islands;
    UV1 (lightmap) is a second pass with a bigger non-overlap margin, so the
    FBX ships lightmap UVs even though Unity 6 removed the importer flag."""
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    uv_layers = obj.data.uv_layers
    if layer_name not in uv_layers:
        uv_layers.new(name=layer_name)
    uv_layers.active = uv_layers[layer_name]
    bpy.ops.uv.smart_project(
        angle_limit=math.radians(angle_deg),
        island_margin=margin,
        correct_aspect=True,
        scale_to_bounds=False,
    )
    bpy.ops.object.mode_set(mode="OBJECT")


def tri_count(obj):
    """Triangle count as Unity will see it (FBX quads are triangulated on
    import), after modifier evaluation."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh()
    tris = 0
    for poly in mesh.polygons:
        tris += len(poly.vertices) - 2
    evaluated.to_mesh_clear()
    return tris


def dims(obj):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh()
    xs = [v.co.x for v in mesh.vertices]
    ys = [v.co.y for v in mesh.vertices]
    zs = [v.co.z for v in mesh.vertices]
    evaluated.to_mesh_clear()
    if not xs:
        return (0.0, 0.0, 0.0)
    return (max(xs) - min(xs), max(ys) - min(ys), max(zs) - min(zs))


def smooth(obj):
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.shade_smooth()
