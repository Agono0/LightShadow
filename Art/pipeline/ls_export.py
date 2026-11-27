# ls_export.py — exports every hero prop collection as a Unity-ready FBX.
#
# Run:  blender -b -P Art/pipeline/ls_export.py -- --root <repo>
# Reads Art/Sources/HeroProps.blend, writes Assets/_Project/Models/Hero/
# <PropName>.fbx per prop, verifies tri budgets and prints a manifest
# (also written to Art/pipeline/manifest.json). Idempotent: overwrites.
#
# Export settings (master plan §7.4): Forward -Z, Up Y, Apply Scalings FBX
# All, Apply Transform on for the single-mesh props. The flashlight FBX
# keeps Apply Transform OFF so its Crank child object retains its local
# axle pivot (pivot-ready for the crank minigame) — everything else bakes.

import json
import os
import sys

import bpy
from mathutils import Vector

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import ls_common as C  # noqa: E402


def prop_mesh_objects(prop_name):
    return [obj for obj in C.collection_objects(prop_name) if obj.type == "MESH"]


def prop_root(prop_name):
    """Root node: the empty for hierarchies, else the single mesh."""
    for obj in C.collection_objects(prop_name):
        if obj.type == "EMPTY":
            return obj
    meshes = prop_mesh_objects(prop_name)
    return meshes[0] if meshes else None


def prop_bounds(objs):
    lo = Vector((1e9, 1e9, 1e9))
    hi = Vector((-1e9, -1e9, -1e9))
    for obj in objs:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            lo = Vector(map(min, lo, world))
            hi = Vector(map(max, hi, world))
    return lo, hi


def export_prop(prop):
    name = prop["name"]
    objs = C.collection_objects(name)
    meshes = prop_mesh_objects(name)
    if not meshes:
        raise RuntimeError(f"{name}: no mesh objects found")

    # deselect everything, select the prop, active = root node
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objs:
        obj.select_set(True)
    root = prop_root(name)
    bpy.context.view_layer.objects.active = root

    filepath = os.path.join(C.HERO_FBX_DIR, name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        bake_space_transform=prop["bake_transform"],
        object_types={"MESH"} if len(objs) == 1 else {"MESH", "EMPTY"},
        use_mesh_modifiers=True,
        # Blender 5.3 enum: OFF / FACE / EDGE / SMOOTH_GROUP. FACE writes
        # per-face smoothing flags; the normals layer itself always ships,
        # so flat-shaded boxes stay flat and the smooth wraith stays smooth.
        mesh_smooth_type="FACE",
        bake_anim=False,
        add_leaf_bones=False,
        path_mode="RELATIVE",
        axis_forward="-Z",
        axis_up="Y",
        use_custom_props=False,
    )

    tris = sum(C.tri_count(mesh) for mesh in meshes)
    lo, hi = prop_bounds(meshes)
    dims = (hi.x - lo.x, hi.y - lo.y, hi.z - lo.z)
    materials = sorted({slot.name for mesh in meshes for slot in mesh.material_slots})
    uv_layers = sorted({layer.name for mesh in meshes for layer in mesh.data.uv_layers})
    size = os.path.getsize(filepath)
    return {
        "name": name,
        "tris": tris,
        "budget": prop["budget"],
        "class": prop["class"],
        "size_m": [round(d, 3) for d in dims],
        "materials": materials,
        "uv_layers": uv_layers,
        "fbx_bytes": size,
        "bake_transform": prop["bake_transform"],
    }


def main():
    if not os.path.exists(C.BLEND_PATH):
        print("[ls_export] MISSING " + C.BLEND_PATH + " — run ls_props.py first")
        sys.exit(1)
    bpy.ops.wm.open_mainfile(filepath=C.BLEND_PATH)
    C.ensure_dirs()

    entries = []
    failures = []
    for prop in C.PROPS:
        try:
            entry = export_prop(prop)
            over = entry["tris"] > entry["budget"]
            mark = " OVER BUDGET" if over else ""
            print(f"[ls_export] {entry['name']}: {entry['tris']} tris "
                  f"(budget {entry['budget']}{mark}), "
                  f"{entry['size_m'][0]:.3f}x{entry['size_m'][1]:.3f}x{entry['size_m'][2]:.3f} m, "
                  f"{entry['fbx_bytes']} bytes{mark}")
            if over:
                failures.append(entry["name"])
            entries.append(entry)
        except Exception as error:  # noqa: BLE001 - report and fail the batch
            print(f"[ls_export] FAILED {prop['name']}: {error}")
            failures.append(prop["name"])

    manifest = {"blender": bpy.app.version_string, "props": entries}
    with open(C.MANIFEST_PATH, "w", encoding="utf-8") as handle:
        json.dump(manifest, handle, indent=2)
    print("[ls_export] manifest written", C.MANIFEST_PATH)

    if failures:
        print("[ls_export] FAILURES: " + ", ".join(failures))
        sys.exit(1)
    print("[ls_export] RESULT: PASS")


if __name__ == "__main__":
    main()
