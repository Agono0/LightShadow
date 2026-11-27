# ls_props.py — builds every LIGHT SHADOW hero prop procedurally.
#
# Run:  blender -b -P Art/pipeline/ls_props.py -- --root <repo>
# Saves Art/Sources/HeroProps.blend. Idempotent: starts from a factory-empty
# scene every run, so re-running always reproduces the same file.
#
# Conventions (master plan §7.4):
#   - real-world scale in metres (door = 2.05 m tall, card = 86 mm);
#   - origin at the bounding-box base centre of each prop;
#   - flat-shaded hard surface / smooth smoke silhouette;
#   - UV0 smart-projected islands for tiling PBR, UV1 lightmap islands;
#   - Blender material names == Unity Mat_Art_* names for 1:1 remap.

import math
import os
import sys

import bpy
from mathutils import Matrix, Vector

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import ls_common as C  # noqa: E402


# --------------------------------------------------------------- helpers

def bend_grid(obj, axis, amp, mirror_ok=True):
    """Bend a flat grid along its local x span (axis='z' lifts in z as a
    function of x)."""
    xs = [v.co.x for v in obj.data.vertices]
    x0, x1 = min(xs), max(xs)
    span = max(1e-6, x1 - x0)
    for v in obj.data.vertices:
        t = (v.co.x - x0) / span
        if axis == "z":
            v.co.z += amp * math.sin(math.pi * t)
    obj.data.update()


def wave_grid_y(obj, amp, cycles):
    """Wave a flat grid along its local y span (bookmark/blanket style).
    Returns the height function so callers can sit details on the wave."""
    ys = [v.co.y for v in obj.data.vertices]
    y0, y1 = min(ys), max(ys)
    span = max(1e-6, y1 - y0)

    def height(y):
        return amp * math.sin(math.tau * cycles * (y - y0) / span)

    for v in obj.data.vertices:
        v.co.z += height(v.co.y)
    obj.data.update()
    return height


def ragged_rim(obj, amp, waves=3, phase=1.7):
    """Push the top ring of an open cylinder up/down for a cloth rim."""
    top_z = max(v.co.z for v in obj.data.vertices)
    for v in obj.data.vertices:
        if abs(v.co.z - top_z) < 1e-5:
            angle = math.atan2(v.co.y, v.co.x)
            v.co.z += amp * math.sin(waves * angle + phase)
    obj.data.update()


# ------------------------------------------------------------- builders

def build_memory_box():
    coll = C.new_collection("SM_Story_MemoryBox")
    parts = []
    # carcass: bottom slab + four walls, open top
    parts.append(C.add_box(coll, "Bottom", (0.28, 0.18, 0.012), (0, 0, 0.006)))
    parts.append(C.add_box(coll, "WallFront", (0.28, 0.008, 0.088), (0, -0.086, 0.056)))
    parts.append(C.add_box(coll, "WallBack", (0.28, 0.008, 0.088), (0, 0.086, 0.056)))
    parts.append(C.add_box(coll, "WallLeft", (0.008, 0.164, 0.088), (-0.136, 0, 0.056)))
    parts.append(C.add_box(coll, "WallRight", (0.008, 0.164, 0.088), (0.136, 0, 0.056)))
    # sliding-lid groove rails
    parts.append(C.add_box(coll, "RailFront", (0.27, 0.006, 0.008), (0, -0.077, 0.096)))
    parts.append(C.add_box(coll, "RailBack", (0.27, 0.006, 0.008), (0, 0.077, 0.096)))
    # sliding lid, pushed open toward +y (overhangs the back)
    parts.append(C.add_box(coll, "Lid", (0.286, 0.170, 0.008), (0, 0.038, 0.100)))
    # brass corner brackets: two face plates + one top cap per corner
    for sx in (-1, 1):
        for sy in (-1, 1):
            cx, cy = sx * 0.138, sy * 0.084
            parts.append(C.add_box(coll, "CornerX", (0.006, 0.028, 0.062),
                                   (sx * 0.1425, cy - sy * 0.002, 0.062)))
            parts.append(C.add_box(coll, "CornerY", (0.028, 0.006, 0.062),
                                   (cx - sx * 0.002, sy * 0.0925, 0.062)))
            parts.append(C.add_box(coll, "CornerCap", (0.034, 0.034, 0.006),
                                   (cx, cy, 0.098)))
    # wood carcass + lid; brass rails and corner brackets. Each part gets
    # exactly one material slot (polygon material_index starts at 0).
    for i, part in enumerate(parts):
        brass = i in (5, 6) or i >= 8
        C.assign(part, "Mat_Art_MetalLocker" if brass else "Mat_Art_DoorWood")
    mesh = C.join_parts(parts, "SM_Story_MemoryBox")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.04, layer_name="UV1_Lightmap")


def build_card():
    coll = C.new_collection("SM_Story_Card")
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=8, y_subdivisions=6, size=1.0)
    obj = bpy.context.active_object
    obj.name = "Card"
    C.link_to(coll, obj)
    obj.scale = (0.086, 0.054, 1.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bend_grid(obj, "z", 0.005)
    sol = obj.modifiers.new("Thicken", "SOLIDIFY")
    sol.thickness = 0.0004
    sol.offset = 0.0
    C.assign(obj, "Mat_Art_PlasterWall")
    C.set_base_origin(obj)
    C.smart_uv(obj, angle_deg=45, margin=0.01)
    C.smart_uv(obj, margin=0.03, layer_name="UV1_Lightmap")


def build_bookmark():
    coll = C.new_collection("SM_Story_Bookmark")
    parts = []
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=6, y_subdivisions=16, size=1.0)
    strip = bpy.context.active_object
    strip.name = "Strip"
    C.link_to(coll, strip)
    strip.scale = (0.045, 0.16, 1.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    strip_height = wave_grid_y(strip, 0.0035, 1.0)
    sol = strip.modifiers.new("Thicken", "SOLIDIFY")
    sol.thickness = 0.0006
    sol.offset = 0.0
    C.assign(strip, "Mat_Art_Fabric")
    parts.append(strip)
    # pressed flower: five flattened petals + a round centre at the top end,
    # sitting ON the wavy strip surface
    flower_cy = 0.048
    flower_cz = strip_height(flower_cy) + 0.0012
    for k in range(5):
        angle = math.tau * k / 5.0 + 0.4
        petal = C.add_uv_sphere(coll, f"Petal{k}", 0.008,
                                center=(math.cos(angle) * 0.009, flower_cy + math.sin(angle) * 0.009, flower_cz),
                                segments=12, rings=6, scale=(1.0, 1.0, 0.22))
        C.assign(petal, "Mat_Art_PlasterWall")
        parts.append(petal)
    center = C.add_uv_sphere(coll, "FlowerCenter", 0.004,
                             center=(0.0, flower_cy, flower_cz + 0.0006), segments=10, rings=5,
                             scale=(1.0, 1.0, 0.3))
    C.assign(center, "Mat_Art_MetalLocker")
    parts.append(center)
    mesh = C.join_parts(parts, "SM_Story_Bookmark")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_exam_paper():
    coll = C.new_collection("SM_Story_ExamPaper")
    parts = []
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=10, y_subdivisions=14, size=1.0)
    paper = bpy.context.active_object
    paper.name = "Paper"
    C.link_to(coll, paper)
    paper.scale = (0.21, 0.297, 1.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bend_grid(paper, "z", 0.004)
    sol = paper.modifiers.new("Thicken", "SOLIDIFY")
    sol.thickness = 0.0004
    sol.offset = 0.0
    C.assign(paper, "Mat_Art_PlasterWall")
    parts.append(paper)
    # red-pen margin: one long vertical rule + four answer dashes. Each box
    # is centred on the bent paper surface at its own x so it pierces the
    # 0.4 mm sheet and shows on both faces without hanging underneath.
    paper_span = 0.21

    def bend_z(x):
        return 0.004 * math.sin(math.pi * (x + paper_span / 2.0) / paper_span)

    parts.append(C.add_box(coll, "MarginRule", (0.004, 0.26, 0.004), (-0.075, 0.0, bend_z(-0.075))))
    for i, y in enumerate((0.10, 0.04, -0.02, -0.08)):
        parts.append(C.add_box(coll, f"RedDash{i}", (0.05, 0.004, 0.004), (-0.02, y, bend_z(-0.02))))
    C.assign(parts[1], "Mat_Story_RedInk")
    for part in parts[2:]:
        C.assign(part, "Mat_Story_RedInk")
    mesh = C.join_parts(parts, "SM_Story_ExamPaper")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_prism():
    coll = C.new_collection("SM_Story_Prism")
    parts = []
    # felt pouch: open octagonal wrap with a wavy rim + sealed bottom
    pouch = C.add_cylinder(coll, "Pouch", 0.030, 0.055, (0, 0, 0.0275), verts=8, cap=False)
    ragged_rim(pouch, 0.008, waves=3, phase=1.7)
    bottom = C.add_cylinder(coll, "PouchBottom", 0.030, 0.004, (0, 0, 0.004), verts=8)
    C.assign(pouch, "Mat_Art_Fabric")
    C.assign(bottom, "Mat_Art_Fabric")
    parts += [pouch, bottom]
    # glass triangular prism riding out of the pouch
    prism = C.add_cylinder(coll, "Prism", 0.024, 0.085, (0, 0, 0.050), verts=3)
    C.assign(prism, "Mat_Story_Glass")
    parts.append(prism)
    mesh = C.join_parts(parts, "SM_Story_Prism")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_photograph():
    coll = C.new_collection("SM_Story_Photograph")
    parts = []
    # wooden frame border
    parts.append(C.add_box(coll, "FrameTop", (0.152, 0.012, 0.007), (0, 0.045, 0.0035)))
    parts.append(C.add_box(coll, "FrameBottom", (0.152, 0.012, 0.007), (0, -0.045, 0.0035)))
    parts.append(C.add_box(coll, "FrameLeft", (0.012, 0.078, 0.007), (-0.070, 0, 0.0035)))
    parts.append(C.add_box(coll, "FrameRight", (0.012, 0.078, 0.007), (0.070, 0, 0.0035)))
    # backing board
    parts.append(C.add_box(coll, "Backing", (0.126, 0.076, 0.003), (0, 0, 0.0015)))
    # bowed print
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=8, y_subdivisions=8, size=1.0)
    print_mesh = bpy.context.active_object
    print_mesh.name = "Print"
    C.link_to(coll, print_mesh)
    print_mesh.scale = (0.124, 0.074, 1.0)
    print_mesh.location = (0, 0, 0.0055)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bend_grid(print_mesh, "z", 0.004)
    sol = print_mesh.modifiers.new("Thicken", "SOLIDIFY")
    sol.thickness = 0.0004
    sol.offset = 0.0
    C.assign(print_mesh, "Mat_Art_PlasterWall")
    parts.append(print_mesh)
    for part in parts[:5]:
        C.assign(part, "Mat_Art_DoorWood")
    mesh = C.join_parts(parts, "SM_Story_Photograph")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_programme():
    coll = C.new_collection("SM_Story_Programme")
    parts = []
    for side, (sign, rot_y, mat) in enumerate((
            (-1, math.radians(8), "Mat_Art_Fabric"),     # front cover
            (+1, math.radians(-8), "Mat_Art_PlasterWall"))):  # inner pages
        bpy.ops.mesh.primitive_grid_add(x_subdivisions=4, y_subdivisions=4, size=1.0)
        panel = bpy.context.active_object
        panel.name = "PanelL" if sign < 0 else "PanelR"
        C.link_to(coll, panel)
        panel.scale = (0.148, 0.21, 1.0)
        panel.location = (sign * 0.074, 0, 0)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        # hinge at the spine (x=0), open a shallow V
        panel.rotation_euler = (0.0, rot_y, 0.0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        panel.location = (sign * 0.0735, 0, 0.001)
        bpy.ops.object.transform_apply(location=True, rotation=False, scale=False)
        sol = panel.modifiers.new("Thicken", "SOLIDIFY")
        sol.thickness = 0.0015
        sol.offset = 0.0
        C.assign(panel, mat)
        parts.append(panel)
    mesh = C.join_parts(parts, "SM_Story_Programme")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_mug():
    coll = C.new_collection("SM_Story_Mug")
    parts = []
    # body: outer wall (open top) + inner wall + solid bottom + rolled rim
    outer = C.add_cylinder(coll, "MugOuter", 0.041, 0.091, (0, 0, 0.0495), verts=32, cap=False)
    inner = C.add_cylinder(coll, "MugInner", 0.037, 0.088, (0, 0, 0.052), verts=32, cap=False)
    bottom = C.add_cylinder(coll, "MugBottom", 0.038, 0.008, (0, 0, 0.008), verts=32)
    rim = C.add_torus(coll, "MugRim", 0.039, 0.0022, (0, 0, 0.0955),
                      major_segments=32, minor_segments=8)
    handle = C.add_torus(coll, "MugHandle", 0.027, 0.005, (0.047, 0, 0.052),
                         major_segments=20, minor_segments=8, rot=(math.pi / 2, 0, 0))
    for part in (outer, inner, bottom, rim, handle):
        C.assign(part, "Mat_Art_PlasterWall")
    parts += [outer, inner, bottom, rim, handle]
    # taped note on the far side: local +Z normal maps to -X (faces out),
    # local +X (width) maps to horizontal -Y, local +Y (height) maps to +Z.
    # Positive bow bulges the middle outward like real tape-held paper.
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=4, y_subdivisions=4, size=1.0)
    note = bpy.context.active_object
    note.name = "Note"
    C.link_to(coll, note)
    note.scale = (0.040, 0.052, 1.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bend_grid(note, "z", 0.004)
    note.rotation_euler = (math.pi / 2, 0.0, -math.pi / 2)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    note.location = (-0.0365, 0.0, 0.052)
    bpy.ops.object.transform_apply(location=True, rotation=False, scale=False)
    sol = note.modifiers.new("Thicken", "SOLIDIFY")
    sol.thickness = 0.0004
    sol.offset = 0.0
    C.assign(note, "Mat_Art_PlasterWall")
    parts.append(note)
    tape = C.add_box(coll, "Tape", (0.0004, 0.014, 0.010), (-0.0375, 0.0, 0.0795))
    C.assign(tape, "Mat_Art_PlasterWall")
    parts.append(tape)
    mesh = C.join_parts(parts, "SM_Story_Mug")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.01)
    C.smart_uv(mesh, margin=0.03, layer_name="UV1_Lightmap")


def build_flashlight():
    """Hero crank flashlight. Hierarchy (crank pivot preserved on export):
        SM_Player_Flashlight (empty root, origin base-centre)
          Body      — tube + grip + head + bezel + lens (origin base-centre)
          LEDStrip  — separate mesh so the lead can add an emissive material
          Crank     — separate mesh, origin AT THE AXLE so Unity can rotate it
    """
    coll = C.new_collection("SM_Player_Flashlight")
    body_parts = []
    # tail cap + grip (knurled) + tube + head + bezel + lens, standing up +Z
    body_parts.append(C.add_cylinder(coll, "TailCap", 0.027, 0.010, (0, 0, 0.005), verts=24))
    body_parts.append(C.add_cylinder(coll, "Grip", 0.030, 0.095, (0, 0, 0.0575), verts=24))
    for k, z in enumerate((0.062, 0.078, 0.094)):
        body_parts.append(C.add_torus(coll, f"Knurl{k}", 0.0295, 0.0022, (0, 0, z),
                                      major_segments=24, minor_segments=6))
    body_parts.append(C.add_cylinder(coll, "Tube", 0.026, 0.115, (0, 0, 0.1625), verts=24))
    body_parts.append(C.add_cylinder(coll, "Head", 0.037, 0.052, (0, 0, 0.246), verts=24))
    body_parts.append(C.add_torus(coll, "Bezel", 0.034, 0.004, (0, 0, 0.270),
                                  major_segments=28, minor_segments=8))
    lens = C.add_cylinder(coll, "Lens", 0.031, 0.005, (0, 0, 0.268), verts=24)
    body_parts.append(lens)
    for part in body_parts:
        C.assign(part, "Mat_Art_TorchMetal")
    C.assign(lens, "Mat_Story_Glass")
    body = C.join_parts(body_parts, "Body")
    C.set_base_origin(body)

    # LED strip: thin emissive bar on the +x side of the tube
    led = C.add_box(coll, "LEDStrip", (0.0025, 0.008, 0.105), (0.0265, 0, 0.1525))
    C.assign(led, "Mat_Story_LED")
    bpy.context.view_layer.objects.active = led
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # hand crank: built at world position, then re-origined onto the axle
    axle = Vector((-0.030, 0.0, 0.135))
    crank_parts = []
    hub = C.add_cylinder(coll, "CrankHub", 0.0095, 0.024, tuple(axle), verts=16,
                         rot=(0.0, math.pi / 2, 0.0))
    C.assign(hub, "Mat_Art_TorchMetal")
    crank_parts.append(hub)
    arm = C.add_box(coll, "CrankArm", (0.013, 0.017, 0.058),
                    (axle.x, axle.y, axle.z - 0.042))
    C.assign(arm, "Mat_Art_GateMetal")
    crank_parts.append(arm)
    grip = C.add_cylinder(coll, "CrankGrip", 0.0115, 0.048,
                          (axle.x, axle.y, axle.z - 0.082), verts=16)
    C.assign(grip, "Mat_Art_DoorWood")
    crank_parts.append(grip)
    crank = C.join_parts(crank_parts, "Crank")
    # shift vertices onto the axle-local frame, park the object at the axle:
    # the object transform (and hence the pivot) survives the FBX export.
    crank.data.transform(Matrix.Translation(-axle))
    crank.location = axle

    # root empty
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "SM_Player_Flashlight"
    root.empty_display_size = 0.05
    for obj in (body, led, crank):
        obj.parent = root
        obj.matrix_parent_inverse = root.matrix_world.inverted()

    for obj in (body, led, crank):
        C.smart_uv(obj, margin=0.01)
        C.smart_uv(obj, margin=0.03, layer_name="UV1_Lightmap")


def wraith_radius(t):
    """Silhouette radius profile, t: 0 = ragged hem, 1 = hood tip."""
    keys = [
        (0.00, 0.55), (0.08, 0.46), (0.20, 0.40), (0.35, 0.36),
        (0.45, 0.42), (0.52, 0.50), (0.58, 0.42), (0.66, 0.30),
        (0.74, 0.30), (0.82, 0.33), (0.92, 0.22), (1.00, 0.045),
    ]
    for (t0, r0), (t1, r1) in zip(keys, keys[1:]):
        if t <= t1:
            f = 0.0 if t1 == t0 else (t - t0) / (t1 - t0)
            f = f * f * (3.0 - 2.0 * f)  # smoothstep between keys
            return r0 + (r1 - r0) * f
    return keys[-1][1]


def build_wraith():
    """Tall human-ish smoke silhouette, subdivided + deterministically
    displaced (flowing skirt, arm bulges, forward hunch). Unity's smoke
    shader adds the runtime wobble."""
    coll = C.new_collection("SM_Wraith_HD")
    rings, segments = 72, 96
    height = 2.35

    def gauss(x, width):
        return math.exp(-(x / width) ** 2)

    verts = []
    for ri in range(rings):
        t = ri / (rings - 1)
        base_z = t * height
        r_base = wraith_radius(t)
        noise_amp = 1.0 + 2.2 * max(0.0, (0.22 - t) / 0.22)
        for si in range(segments):
            angle = math.tau * si / segments
            noise = (1.0 + noise_amp * (
                0.10 * math.sin(3.0 * angle + 1.3 + t * 9.0)
                + 0.06 * math.sin(5.0 * angle - 2.1 + t * 17.0)
                + 0.05 * math.sin(8.0 * angle + 0.7 + t * 31.0)))
            # arm/shoulder bulges at both sides of the forward (-y) axis
            arm = 0.085 * gauss(_wrap_angle(angle - (-math.pi / 2 - 0.6)), 0.55) * gauss(t - 0.50, 0.10)
            arm += 0.085 * gauss(_wrap_angle(angle - (-math.pi / 2 + 0.6)), 0.55) * gauss(t - 0.50, 0.10)
            radius = max(0.008, r_base * noise + arm)
            x = radius * math.cos(angle)
            y = radius * math.sin(angle)
            z = base_z
            # skirt vertical rag + hem tendril drop
            if t < 0.15:
                z += 0.08 * math.sin(6.0 * angle + t * 40.0) * (0.15 - t) / 0.15
            if ri == 0:
                z -= (0.16 + 0.14 * math.sin(5.0 * angle + 1.2)) * (0.55 + 0.45 * math.sin(3.0 * angle + 2.0))
            # forward hunch toward -y in the upper half
            hunch = 0.24 * _smoothstep((t - 0.45) / 0.55)
            y -= hunch
            verts.append((x, y, z))

    faces = []
    for ri in range(rings - 1):
        for si in range(segments):
            a = ri * segments + si
            b = ri * segments + (si + 1) % segments
            c = (ri + 1) * segments + (si + 1) % segments
            d = (ri + 1) * segments + si
            faces.append((a, b, c, d))
    # cap the hood tip with a fan
    tip_index = len(verts)
    verts.append((0.0, -0.10, height + 0.03))
    for si in range(segments):
        a = (rings - 1) * segments + si
        b = (rings - 1) * segments + (si + 1) % segments
        faces.append((a, b, tip_index))

    mesh = bpy.data.meshes.new("SM_Wraith_HD")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("SM_Wraith_HD", mesh)
    coll.objects.link(obj)
    C.smooth(obj)
    C.assign(obj, "M_ShadowSmoke")
    C.set_base_origin(obj)
    C.smart_uv(obj, angle_deg=80, margin=0.005)
    C.smart_uv(obj, margin=0.02, layer_name="UV1_Lightmap")


def _wrap_angle(a):
    while a > math.pi:
        a -= math.tau
    while a < -math.pi:
        a += math.tau
    return a


def _smoothstep(x):
    x = max(0.0, min(1.0, x))
    return x * x * (3.0 - 2.0 * x)


def build_door():
    coll = C.new_collection("SM_Arch_DoorGrille")
    parts = []
    # frame construction: full-height stiles + rails, iron grille window above
    parts.append(C.add_box(coll, "StileL", (0.09, 0.045, 2.05), (-0.43, 0, 1.025)))
    parts.append(C.add_box(coll, "StileR", (0.09, 0.045, 2.05), (0.43, 0, 1.025)))
    parts.append(C.add_box(coll, "RailBottom", (0.77, 0.045, 0.14), (0, 0, 0.07)))
    parts.append(C.add_box(coll, "RailLock", (0.77, 0.045, 0.10), (0, 0, 1.25)))
    parts.append(C.add_box(coll, "RailTop", (0.77, 0.045, 0.15), (0, 0, 1.975)))
    # recessed field panels (thin, centred in the 45 mm door)
    parts.append(C.add_box(coll, "PanelL", (0.345, 0.035, 0.98), (-0.195, 0, 0.67)))
    parts.append(C.add_box(coll, "PanelR", (0.345, 0.035, 0.98), (0.195, 0, 0.67)))
    # iron grille: frame + vertical bars + one crossbar (opening 0.71 x 0.54)
    parts.append(C.add_box(coll, "GrilleFrameT", (0.77, 0.045, 0.03), (0, 0, 1.855)))
    parts.append(C.add_box(coll, "GrilleFrameB", (0.77, 0.045, 0.03), (0, 0, 1.345)))
    parts.append(C.add_box(coll, "GrilleFrameL", (0.03, 0.045, 0.54), (-0.37, 0, 1.60)))
    parts.append(C.add_box(coll, "GrilleFrameR", (0.03, 0.045, 0.54), (0.37, 0, 1.60)))
    for i, x in enumerate([-0.30, -0.20, -0.10, 0.0, 0.10, 0.20, 0.30]):
        parts.append(C.add_box(coll, f"GrilleBar{i}", (0.012, 0.012, 0.51), (x, 0, 1.60)))
    parts.append(C.add_box(coll, "GrilleCross", (0.71, 0.012, 0.012), (0, 0, 1.60)))
    wood_count = 7  # stiles, rails, panels
    for i, part in enumerate(parts):
        C.assign(part, "Mat_Art_DoorWood" if i < wood_count else "Mat_Art_GateMetal")
    mesh = C.join_parts(parts, "SM_Arch_DoorGrille")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.02)
    C.smart_uv(mesh, margin=0.05, layer_name="UV1_Lightmap")


def build_chalkboard():
    coll = C.new_collection("SM_Arch_Chalkboard")
    parts = []
    parts.append(C.add_box(coll, "BackPanel", (2.40, 0.024, 1.20), (0, 0.012, 0.60)))
    parts.append(C.add_box(coll, "ChalkSurface", (2.30, 0.012, 1.10), (0, -0.008, 0.605)))
    parts.append(C.add_box(coll, "FrameTop", (2.40, 0.06, 0.05), (0, 0, 1.175)))
    parts.append(C.add_box(coll, "FrameBottom", (2.40, 0.06, 0.05), (0, 0, 0.025)))
    parts.append(C.add_box(coll, "FrameLeft", (0.05, 0.06, 1.10), (-1.175, 0, 0.60)))
    parts.append(C.add_box(coll, "FrameRight", (0.05, 0.06, 1.10), (1.175, 0, 0.60)))
    # chalk tray + lip + two chalk sticks + felt eraser
    parts.append(C.add_box(coll, "Tray", (2.40, 0.075, 0.02), (0, -0.025, 0.01)))
    parts.append(C.add_box(coll, "TrayLip", (2.40, 0.012, 0.025), (0, -0.0625, 0.0285)))
    parts.append(C.add_cylinder(coll, "Chalk1", 0.005, 0.030, (-0.50, -0.030, 0.035),
                                verts=10, rot=(math.pi / 2, 0, 0)))
    parts.append(C.add_cylinder(coll, "Chalk2", 0.005, 0.026, (0.52, -0.034, 0.033),
                                verts=10, rot=(math.pi / 2, 0.15, 0)))
    parts.append(C.add_box(coll, "Eraser", (0.12, 0.05, 0.025), (0.90, -0.030, 0.0375)))
    for part in parts:
        if part.name == "Eraser":
            C.assign(part, "Mat_Art_Fabric")
        elif part.name == "ChalkSurface":
            C.assign(part, "Mat_Art_Chalkboard")
        elif part.name.startswith("Chalk"):
            C.assign(part, "Mat_Art_PlasterWall")
        else:
            C.assign(part, "Mat_Art_DoorWood")
    mesh = C.join_parts(parts, "SM_Arch_Chalkboard")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.02)
    C.smart_uv(mesh, margin=0.05, layer_name="UV1_Lightmap")


def build_gate_crank():
    coll = C.new_collection("SM_Arch_GateCrank")
    parts = []
    # wall bracket plate + bolts (axis of the wheel points +x out of the wall)
    parts.append(C.add_box(coll, "WallPlate", (0.025, 0.16, 0.26), (0.0125, 0, 0.18)))
    for i, (y, z) in enumerate(((-0.055, 0.075), (0.055, 0.075), (-0.055, 0.285), (0.055, 0.285))):
        parts.append(C.add_cylinder(coll, f"Bolt{i}", 0.007, 0.014, (0.032, y, z),
                                    verts=10, rot=(0.0, math.pi / 2, 0.0)))
    parts.append(C.add_cylinder(coll, "Axle", 0.018, 0.062, (0.055, 0, 0.18),
                                verts=16, rot=(0.0, math.pi / 2, 0.0)))
    # wheel: rim + six spokes + hub
    wheel = C.add_torus(coll, "WheelRim", 0.16, 0.014, (0.10, 0, 0.18),
                        major_segments=32, minor_segments=8, rot=(0.0, math.pi / 2, 0.0))
    parts.append(wheel)
    parts.append(C.add_cylinder(coll, "Hub", 0.032, 0.030, (0.10, 0, 0.18),
                                verts=16, rot=(0.0, math.pi / 2, 0.0)))
    for k in range(6):
        angle = math.tau * k / 6.0
        spoke = C.add_box(coll, f"Spoke{k}", (0.012, 0.024, 0.155),
                          (0.10, math.cos(angle) * 0.078, 0.18 + math.sin(angle) * 0.078))
        # rotate the spoke about the wheel axis (x), then bake it in
        spoke.rotation_euler = (angle, 0.0, 0.0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(spoke)
    # crank handle: stub arm + wooden grip
    parts.append(C.add_cylinder(coll, "HandleStub", 0.010, 0.028, (0.128, 0, 0.18),
                                verts=12, rot=(0.0, math.pi / 2, 0.0)))
    grip = C.add_cylinder(coll, "HandleGrip", 0.013, 0.055, (0.162, 0, 0.18),
                          verts=12, rot=(0.0, math.pi / 2, 0.0))
    parts.append(grip)
    for part in parts:
        if part is grip:
            C.assign(part, "Mat_Art_DoorWood")
        elif part.name == "WallPlate":
            C.assign(part, "Mat_Art_Stone")
        else:
            C.assign(part, "Mat_Art_GateMetal")
    mesh = C.join_parts(parts, "SM_Arch_GateCrank")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.015)
    C.smart_uv(mesh, margin=0.04, layer_name="UV1_Lightmap")


def build_iron_gate():
    """Iron gate section sized to the Kenney GraveyardKit fence line
    (FenceTargetHeight 1.7 m after swap normalisation; modelled 1.8 m tall)."""
    coll = C.new_collection("SM_Arch_IronGate")
    parts = []
    parts.append(C.add_box(coll, "StileL", (0.05, 0.05, 1.80), (-0.475, 0, 0.90)))
    parts.append(C.add_box(coll, "StileR", (0.05, 0.05, 1.80), (0.475, 0, 0.90)))
    parts.append(C.add_box(coll, "RailTop", (1.00, 0.04, 0.05), (0, 0, 1.755)))
    parts.append(C.add_box(coll, "RailMid", (1.00, 0.03, 0.04), (0, 0, 1.10)))
    parts.append(C.add_box(coll, "RailBottom", (1.00, 0.04, 0.05), (0, 0, 0.045)))
    bar_count = 11
    for i in range(bar_count):
        x = -0.40 + i * 0.08
        parts.append(C.add_box(coll, f"Bar{i}", (0.014, 0.014, 1.69), (x, 0, 0.895)))
        parts.append(C.add_cone(coll, f"Spear{i}", 0.016, 0.0, 0.05, (x, 0, 1.765), verts=8))
    # two decorative scroll rings near the mid rail
    for i, x in enumerate((-0.16, 0.16)):
        parts.append(C.add_torus(coll, f"Scroll{i}", 0.032, 0.006, (x, 0, 1.30),
                                 major_segments=16, minor_segments=6))
    for part in parts:
        C.assign(part, "Mat_Art_GateMetal")
    mesh = C.join_parts(parts, "SM_Arch_IronGate")
    C.set_base_origin(mesh)
    C.smart_uv(mesh, margin=0.02)
    C.smart_uv(mesh, margin=0.05, layer_name="UV1_Lightmap")


# ------------------------------------------------------------------ main

BUILDERS = {
    "SM_Story_MemoryBox": build_memory_box,
    "SM_Story_Card": build_card,
    "SM_Story_Bookmark": build_bookmark,
    "SM_Story_ExamPaper": build_exam_paper,
    "SM_Story_Prism": build_prism,
    "SM_Story_Photograph": build_photograph,
    "SM_Story_Programme": build_programme,
    "SM_Story_Mug": build_mug,
    "SM_Player_Flashlight": build_flashlight,
    "SM_Wraith_HD": build_wraith,
    "SM_Arch_DoorGrille": build_door,
    "SM_Arch_Chalkboard": build_chalkboard,
    "SM_Arch_GateCrank": build_gate_crank,
    "SM_Arch_IronGate": build_iron_gate,
}


def main():
    C.reset_scene()
    C.ensure_dirs()
    C.build_materials()
    for prop in C.PROPS:
        builder = BUILDERS[prop["name"]]
        builder()
        print("[ls_props] built", prop["name"])
    bpy.ops.wm.save_as_mainfile(filepath=C.BLEND_PATH)
    print("[ls_props] saved", C.BLEND_PATH)


if __name__ == "__main__":
    main()
