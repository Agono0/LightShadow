# ls_preview.py — renders an 8-frame turntable per hero prop.
#
# Run:  blender -b -P Art/pipeline/ls_preview.py -- --root <repo>
# Opens Art/Sources/HeroProps.blend, frames each prop collection with a
# three-point light rig and renders 8 PNGs (45-degree steps) to
# Art/Previews/<Prop>_f<N>.png. Idempotent: overwrites every frame.
#
# Render engine: EEVEE when a GPU context is available in background mode,
# Cycles (CPU, low samples) as the automatic fallback.

import math
import os
import sys

import bpy
from mathutils import Vector

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import ls_common as C  # noqa: E402

FRAMES = 8
RESOLUTION = 512
SAMPLES_EEVEE = 64
SAMPLES_CYCLES = 24


def pick_engine():
    """EEVEE first (fast GPU), Cycles fallback for headless-less machines."""
    for identifier, samples in (("BLENDER_EEVEE_NEXT", SAMPLES_EEVEE),
                                ("BLENDER_EEVEE", SAMPLES_EEVEE),
                                ("CYCLES", SAMPLES_CYCLES)):
        try:
            bpy.context.scene.render.engine = identifier
            if "CYCLES" in identifier:
                bpy.context.scene.cycles.samples = samples
                bpy.context.scene.cycles.use_denoising = True
            else:
                bpy.context.scene.eevee.taa_render_samples = samples
            print("[ls_preview] engine:", identifier)
            return identifier
        except TypeError:
            continue
    raise RuntimeError("no usable render engine found")


def setup_stage():
    """World + light rig shared by every turntable (not part of any prop
    collection, so exports are unaffected)."""
    world = bpy.data.worlds.new("PreviewWorld")
    bpy.context.scene.world = world
    world.use_nodes = True
    background = world.node_tree.nodes.get("Background")
    if background:
        background.inputs[0].default_value = (0.045, 0.047, 0.055, 1.0)
        background.inputs[1].default_value = 1.0

    stage = bpy.data.collections.new("PreviewStage")
    bpy.context.scene.collection.children.link(stage)

    def light(name, kind, location, energy, size=None, rot=(0, 0, 0)):
        data = bpy.data.lights.new(name, kind)
        data.energy = energy
        if kind == "AREA" and size is not None:
            data.size = size
        elif kind == "SPOT":
            data.spot_size = math.radians(55.0)
            data.shadow_soft_size = 0.4
        obj = bpy.data.objects.new(name, data)
        obj.location = location
        obj.rotation_euler = rot
        stage.objects.link(obj)
        return obj

    sun = light("KeySun", "SUN", (4, -4, 6), 2.2)
    sun.rotation_euler = (math.radians(35), 0.0, math.radians(45))
    fill = light("FillArea", "AREA", (-3, -2, 2), 220, size=3.0)
    rim = light("RimSpot", "SPOT", (0.5, 4.5, 3.5), 480)

    camera_data = bpy.data.cameras.new("TurntableCam")
    camera_data.lens = 60.0
    camera = bpy.data.objects.new("TurntableCam", camera_data)
    stage.objects.link(camera)
    bpy.context.scene.camera = camera
    return camera, (fill, rim)


def look_at(camera, target):
    direction = target - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_prop(prop_name, camera, lights, engine):
    # solo the prop collection
    for prop in C.PROPS:
        coll = C.collection_of(prop["name"])
        if coll:
            coll.hide_render = prop["name"] != prop_name

    objs = [obj for obj in C.collection_objects(prop_name) if obj.type == "MESH"]
    lo = Vector((1e9, 1e9, 1e9))
    hi = Vector((-1e9, -1e9, -1e9))
    for obj in objs:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            lo = Vector(map(min, lo, world))
            hi = Vector(map(max, hi, world))
    center = (lo + hi) * 0.5
    max_dim = max(hi.x - lo.x, hi.y - lo.y, hi.z - lo.z, 0.05)
    radius = max_dim * 1.9
    elevation = math.radians(16.0)

    # Scale the (non-directional) rig with the prop so a 5 cm card and a
    # 2.4 m chalkboard get the same exposure and relative light spread.
    fill, rim = lights
    fill.location = center + Vector((-1.6, -1.0, 1.2)) * max_dim
    fill.data.energy = 220.0 * max_dim * max_dim
    rim.location = center + Vector((0.4, 2.6, 1.6)) * max_dim
    rim.data.energy = 480.0 * max_dim * max_dim

    scene = bpy.context.scene
    scene.render.resolution_x = RESOLUTION
    scene.render.resolution_y = RESOLUTION
    scene.render.image_settings.file_format = "PNG"

    for frame in range(FRAMES):
        azimuth = math.tau * frame / FRAMES
        camera.location = center + Vector((
            radius * math.cos(azimuth) * math.cos(elevation),
            radius * math.sin(azimuth) * math.cos(elevation),
            radius * math.sin(elevation),
        ))
        look_at(camera, center)
        scene.render.filepath = os.path.join(C.PREVIEWS_DIR, f"{prop_name}_f{frame}.png")
        bpy.ops.render.render(write_still=True)
        print(f"[ls_preview] {prop_name} frame {frame + 1}/{FRAMES}")


def main():
    if not os.path.exists(C.BLEND_PATH):
        print("[ls_preview] MISSING " + C.BLEND_PATH + " — run ls_props.py first")
        sys.exit(1)
    bpy.ops.wm.open_mainfile(filepath=C.BLEND_PATH)
    C.ensure_dirs()
    engine = pick_engine()
    camera, lights = setup_stage()
    for prop in C.PROPS:
        render_prop(prop["name"], camera, lights, engine)
    print("[ls_preview] RESULT: PASS —", len(C.PROPS) * FRAMES, "frames in", C.PREVIEWS_DIR)


if __name__ == "__main__":
    main()
