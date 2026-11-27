# ls_icon.py — LIGHT SHADOW app-icon emblem renderer (Blender 5.x, headless EEVEE)
#
# Renders the game's flashlight emblem — a stylized flashlight silhouette firing a
# warm beam cone on a cold dark background — using the project's gold-on-dark
# palette (Docs/STYLE_GUIDE.md §5: gold #E8B54A on cold-cyan darkness), and writes
# square PNGs directly at every required size (no external downscaling, no PIL).
#
# Usage (headless, from the repo root):
#   blender -b -P Art/Pipeline/ls_icon.py
#   blender -b -P Art/Pipeline/ls_icon.py -- --out Art/Previews --sizes 1024,512,192,96
# (Everything after `--` is passed to this script; `--sizes` defaults to the four
# store/OS sizes below. Each size is rendered natively by re-rendering the scene —
# EEVEE redraws at the target resolution, so small icons stay crisp, not blurred.)
#
# Output: Art/Previews/icon_<size>.png for every requested size (1024 master,
# 512 + 192 + 96 store/OS sizes).
#
# ---------------------------------------------------------------------------
# How Unity consumes these files (PlayerSettings) — read before wiring:
#
# These PNGs live OUTSIDE Assets/ (repo-root Art/ tree, master-plan §6.1), so
# Unity cannot reference them directly. To wire them into the player:
#   1. Copy the chosen master into the project under the texture naming rule
#      (AGENTS.md rule 3), e.g. Assets/_Project/Textures/UI/T_AppIcon_1024.png.
#   2. Project Settings > Player > Icon > Default Icon: assign that texture.
#      Unity auto-generates the per-platform sizes (Windows exe/window icons,
#      Android launcher densities) from the icons listed there — you do NOT
#      hand-place every mip.
#   3. Android adaptive icons: Player Settings > Icon > Android > set "Adaptive
#      Icon" foreground/background layers (432x432 px layers). The foreground
#      must be the emblem alone with generous safe-zone padding (masks crop the
#      outer ring of the 108dp canvas); the background layer is a flat #0B0F16
#      square. A foreground variant with a transparent backdrop can be rendered
#      from this script by adding `--transparent` (film_transparent + alpha PNG).
#   4. These assignments happen in-editor (or through a SceneBuilder extension
#      if one is ever added for it). Per AGENTS.md rule 1, this pipeline NEVER
#      edits ProjectSettings/*.asset YAML.
#
# Palette (STYLE_GUIDE §5 + §10 anchors):
#   background  #0B0F16 (cold dark), glow #E8B54A (warm gold), beam core #FFE9B8,
#   body #141A21 (graphite silhouette), rim #CBBF95 (pale grey-gold chalk edge).
#
# Keep-at-96px rules baked into the design: one strong diagonal silhouette, one
# bright lens dot, beam widths >= 8 px at 96 px output, no text, no fine detail.
# ---------------------------------------------------------------------------

import math
import os
import sys

import bpy

# --- palette (sRGB anchors from STYLE_GUIDE §5; converted to linear on use) ---
BG_DARK = (0x0B / 255, 0x0F / 255, 0x16 / 255, 1.0)    # #0B0F16 backdrop
GLOW_GOLD = (0xE8 / 255, 0xB5 / 255, 0x4A / 255, 1.0)  # #E8B54A beam gold
BEAM_CORE = (0xFF / 255, 0xE9 / 255, 0xB8 / 255, 1.0)  # #FFE9B8 hot core
BODY_GRAPHITE = (0x14 / 255, 0x1A / 255, 0x21 / 255, 1.0)  # #141A21 body
HEAD_GRAPHITE = (0x1B / 255, 0x24 / 255, 0x2E / 255, 1.0)  # #1B242E head
RIM_CHALK = (0xCB / 255, 0xBF / 255, 0x95 / 255, 1.0)  # #CBBF95 bezel/grip
LENS_TINT = (0xFF / 255, 0xE9 / 255, 0xB8 / 255, 1.0)  # #FFE9B8 lens light

SIZES_DEFAULT = (1024, 512, 192, 96)
DIAG_DEG = 35.0          # emblem diagonal (flashlight pointing up-right)
ORTHO_SCALE = 3.7        # camera frame half-width = 1.85 (emblem + margins)


def srgb_to_linear(c):
    """One sRGB channel (0..1) to linear so the authored hexes read true under
    the Standard view transform."""
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def lin(rgba):
    return (srgb_to_linear(rgba[0]), srgb_to_linear(rgba[1]),
            srgb_to_linear(rgba[2]), rgba[3])


def new_material(name):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    return mat


def principled_inputs(mat):
    return mat.node_tree.nodes.get("Principled BSDF").inputs


def kill_specular(mat):
    """Emissive planes must not mirror the studio lights as a grey sheen."""
    inputs = principled_inputs(mat)
    for name in ("Specular IOR Level", "Specular"):
        if name in inputs:
            inputs[name].default_value = 0.0
            break
    inputs["Roughness"].default_value = 1.0


def set_blended(mat):
    """Sorted transparency in EEVEE Next (4.2+); harmless on older engines."""
    try:
        mat.surface_render_method = "BLENDED"
    except AttributeError:
        pass


def make_solid(name, color, metallic=0.0, roughness=0.5,
               emission_strength=0.0, emission_color=None):
    mat = new_material(name)
    inputs = principled_inputs(mat)
    # Emissive surfaces take a black base so scene lights never wash them out.
    inputs["Base Color"].default_value = ((0.0, 0.0, 0.0, 1.0)
                                          if emission_strength > 0.0
                                          else lin(color))
    inputs["Metallic"].default_value = metallic
    inputs["Roughness"].default_value = roughness
    if emission_strength > 0.0:
        inputs["Emission Color"].default_value = lin(
            emission_color if emission_color is not None else color)
        inputs["Emission Strength"].default_value = emission_strength
        kill_specular(mat)
    return mat


def make_beam(name, color, strength, alpha_near, alpha_far, span):
    """Emission cone material: alpha ramps from alpha_near at the lens end
    (local X = -span/2) to alpha_far at the far end (local X = +span/2)."""
    mat = new_material(name)
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    inputs = principled_inputs(mat)

    inputs["Base Color"].default_value = (0.0, 0.0, 0.0, 1.0)
    inputs["Emission Color"].default_value = lin(color)
    inputs["Emission Strength"].default_value = strength
    inputs["Roughness"].default_value = 1.0

    tex_coord = nodes.new("ShaderNodeTexCoord")
    sep_xyz = nodes.new("ShaderNodeSeparateXYZ")
    map_range = nodes.new("ShaderNodeMapRange")
    map_range.inputs["From Min"].default_value = -span / 2.0
    map_range.inputs["From Max"].default_value = span / 2.0
    map_range.inputs["To Min"].default_value = alpha_near
    map_range.inputs["To Max"].default_value = alpha_far
    map_range.clamp = True
    links.new(tex_coord.outputs["Object"], sep_xyz.inputs["Vector"])
    links.new(sep_xyz.outputs["X"], map_range.inputs["Value"])
    links.new(map_range.outputs["Result"], inputs["Alpha"])
    kill_specular(mat)
    set_blended(mat)
    return mat


def make_halo():
    """Radial gold halo behind the lens: alpha 0.9 at centre -> 0 at the rim."""
    mat = new_material("Icon_Halo")
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    inputs = principled_inputs(mat)

    inputs["Base Color"].default_value = (0.0, 0.0, 0.0, 1.0)
    inputs["Emission Color"].default_value = lin(GLOW_GOLD)
    inputs["Emission Strength"].default_value = 2.2

    tex = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (0.42, 0.42, 0.42)
    grad = nodes.new("ShaderNodeTexGradient")
    grad.gradient_type = "RADIAL"
    ramp = nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.0
    ramp.color_ramp.elements[0].color = (0.0, 0.0, 0.0, 0.75)
    e_mid = ramp.color_ramp.elements.new(0.5)
    e_mid.color = (0.0, 0.0, 0.0, 0.28)
    ramp.color_ramp.elements[-1].position = 1.0
    ramp.color_ramp.elements[-1].color = (0.0, 0.0, 0.0, 0.0)
    links.new(tex.outputs["Object"], mapping.inputs["Vector"])
    links.new(mapping.outputs["Vector"], grad.inputs["Vector"])
    links.new(grad.outputs["Fac"], ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"], inputs["Alpha"])
    kill_specular(mat)
    set_blended(mat)
    return mat


def emblem_materials():
    return {
        "body": make_solid("Icon_Body", BODY_GRAPHITE, metallic=0.55,
                           roughness=0.42),
        "head": make_solid("Icon_Head", HEAD_GRAPHITE, metallic=0.55,
                           roughness=0.38),
        "groove": make_solid("Icon_Groove", (0x0F / 255, 0x14 / 255,
                                             0x1A / 255, 1.0),
                             metallic=0.4, roughness=0.6),
        "rim": make_solid("Icon_Rim", RIM_CHALK, metallic=0.8, roughness=0.3),
        "lens": make_solid("Icon_Lens", LENS_TINT, roughness=0.2,
                           emission_strength=6.0, emission_color=LENS_TINT),
        "beam_outer": make_beam("Icon_BeamOuter", GLOW_GOLD, strength=1.8,
                                alpha_near=0.32, alpha_far=0.0, span=1.45),
        "beam_mid": make_beam("Icon_BeamMid", GLOW_GOLD, strength=2.6,
                              alpha_near=0.50, alpha_far=0.0, span=1.20),
        "beam_inner": make_beam("Icon_BeamInner", GLOW_GOLD, strength=3.6,
                                alpha_near=0.68, alpha_far=0.0, span=0.90),
        "beam_core": make_beam("Icon_BeamCore", BEAM_CORE, strength=5.0,
                               alpha_near=0.85, alpha_far=0.0, span=0.55),
    }


class EmblemBuilder:
    """Builds the emblem flat along +X (tail cap at -X, beam toward +X) with the
    whole span centred on the origin, then parents every piece to a pivot empty
    rotated DIAG_DEG about Z: beam to the upper-right, body to the lower-left."""

    def __init__(self, pivot):
        self.pivot = pivot
        self.x_shift = -0.675  # span tail -0.70 .. beam end +2.05 -> centred
        self.mats = emblem_materials()

    def _adopt(self, obj, x=0.0):
        obj.location.x += self.x_shift
        obj.parent = self.pivot
        obj.visible_shadow = False
        return obj

    def cylinder(self, name, radius, depth, x, mat):
        bpy.ops.mesh.primitive_cylinder_add(vertices=64, radius=radius,
                                            depth=depth,
                                            location=(x, 0.0, 0.0),
                                            rotation=(0.0, math.radians(90.0), 0.0))
        obj = bpy.context.active_object
        obj.name = name
        obj.data.materials.append(mat)
        return self._adopt(obj, x)

    def cone(self, name, radius_far, radius_near, depth, x, mat):
        bpy.ops.mesh.primitive_cone_add(vertices=64, radius1=radius_far,
                                        radius2=radius_near, depth=depth,
                                        location=(x, 0.0, 0.0),
                                        rotation=(0.0, math.radians(90.0), 0.0))
        obj = bpy.context.active_object
        obj.name = name
        obj.data.materials.append(mat)
        return self._adopt(obj, x)

    def build(self):
        # Flashlight silhouette: tail cap, body, recessed grip grooves, tapered
        # head, brass bezel, glowing lens.
        self.cylinder("Icon_TailCap", 0.150, 0.10, -0.630, self.mats["body"])
        self.cylinder("Icon_Body", 0.155, 0.85, -0.150, self.mats["body"])
        for i, gx in enumerate((-0.42, -0.22, -0.02)):
            self.cylinder(f"Icon_Grip{i}", 0.163, 0.030, gx,
                          self.mats["groove"])
        self.cone("Icon_Head", 0.165, 0.230, 0.28, 0.415, self.mats["head"])
        self.cylinder("Icon_Bezel", 0.250, 0.07, 0.570, self.mats["rim"])
        self.cylinder("Icon_Lens", 0.175, 0.05, 0.588, self.mats["lens"])

        # Beam: four nested cones, narrow at the lens, widening forward.
        self.cone("Icon_BeamOuter", 0.85, 0.045, 1.45, 0.615 + 1.45 / 2.0,
                  self.mats["beam_outer"])
        self.cone("Icon_BeamMid", 0.62, 0.045, 1.20, 0.615 + 1.20 / 2.0,
                  self.mats["beam_mid"])
        self.cone("Icon_BeamInner", 0.40, 0.045, 0.90, 0.615 + 0.90 / 2.0,
                  self.mats["beam_inner"])
        self.cone("Icon_BeamCore", 0.20, 0.045, 0.55, 0.615 + 0.55 / 2.0,
                  self.mats["beam_core"])


def build_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene

    pivot = bpy.data.objects.new("Icon_EmblemPivot", None)
    pivot.empty_display_type = "PLAIN_AXES"
    pivot.rotation_euler = (0.0, 0.0, math.radians(DIAG_DEG))
    scene.collection.objects.link(pivot)

    emblem = bpy.data.collections.new("Icon_Emblem")
    scene.collection.children.link(emblem)
    EmblemBuilder(pivot).build()

    # Background: large emissive plane (literal colour, unaffected by lights).
    import bmesh  # noqa: PLC0415 (cheap quad; ops plane_add would need unlinking)
    mesh = bpy.data.meshes.new("Icon_BackdropMesh")
    bm = bmesh.new()
    bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=6.0)
    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(make_solid("Icon_BackdropMat", BG_DARK,
                                     emission_strength=1.0,
                                     emission_color=BG_DARK))
    bg = bpy.data.objects.new("Icon_Backdrop", mesh)
    bg.location = (0.0, 0.0, -2.0)
    bg.visible_shadow = False
    emblem.objects.link(bg)

    # Soft gold halo behind the lens (slightly toward the beam side).
    halo_mesh = bpy.data.meshes.new("Icon_HaloMesh")
    bm = bmesh.new()
    bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=4.5)
    bm.to_mesh(halo_mesh)
    bm.free()
    halo_mesh.materials.append(make_halo())
    halo = bpy.data.objects.new("Icon_Halo", halo_mesh)
    halo.location = (0.10, 0.10, -1.9)
    halo.visible_shadow = False
    emblem.objects.link(halo)

    # Lights: warm key (upper-left) + cool chalk rim (lower-right, behind).
    key_data = bpy.data.lights.new("Icon_Key", type="AREA")
    key_data.energy = 900.0
    key_data.size = 5.0
    key_data.color = (1.0, 0.94, 0.82)
    key = bpy.data.objects.new("Icon_Key", key_data)
    key.location = (-2.6, 2.6, 4.0)
    key.rotation_euler = (math.radians(-38.0), math.radians(28.0), 0.0)
    scene.collection.objects.link(key)

    rim_data = bpy.data.lights.new("Icon_Rim", type="AREA")
    rim_data.energy = 500.0
    rim_data.size = 4.0
    rim_data.color = (0.62, 0.72, 0.86)
    rim = bpy.data.objects.new("Icon_Rim", rim_data)
    rim.location = (3.2, -2.6, 3.0)
    rim.rotation_euler = (math.radians(-40.0), math.radians(-42.0), 0.0)
    scene.collection.objects.link(rim)

    # Camera: orthographic, square, looking straight down -Z.
    cam_data = bpy.data.cameras.new("Icon_Camera")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = ORTHO_SCALE
    cam_data.clip_start = 0.1
    cam_data.clip_end = 50.0
    cam = bpy.data.objects.new("Icon_Camera", cam_data)
    cam.location = (0.0, 0.0, 8.0)
    scene.collection.objects.link(cam)
    scene.camera = cam

    configure_render(scene)
    return scene


def configure_render(scene):
    engine_items = [item.identifier for item in
                    bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items]
    for candidate in ("BLENDER_EEVEE_NEXT", "BLENDER_EEVEE"):
        if candidate in engine_items:
            scene.render.engine = candidate
            break
    else:
        raise RuntimeError("EEVEE not available in this Blender build")

    scene.eevee.taa_render_samples = 64
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.look = "None"
    scene.render.film_transparent = False
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGB"
    scene.render.image_settings.color_depth = "8"
    scene.render.image_settings.compression = 15


def render_all(scene, out_dir, sizes):
    os.makedirs(out_dir, exist_ok=True)
    for size in sizes:
        scene.render.resolution_x = size
        scene.render.resolution_y = size
        scene.render.resolution_percentage = 100
        path = os.path.join(out_dir, f"icon_{size}.png")
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        print(f"[ls_icon] wrote {path}")


def main():
    argv = sys.argv
    user_args = argv[argv.index("--") + 1:] if "--" in argv else []
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                           os.pardir, "Previews")
    sizes = list(SIZES_DEFAULT)
    for i, arg in enumerate(user_args):
        if arg == "--out" and i + 1 < len(user_args):
            out_dir = user_args[i + 1]
        elif arg == "--sizes" and i + 1 < len(user_args):
            sizes = [int(s) for s in user_args[i + 1].split(",") if s.strip()]

    out_dir = os.path.abspath(out_dir)
    scene = build_scene()
    render_all(scene, out_dir, sizes)


main()
