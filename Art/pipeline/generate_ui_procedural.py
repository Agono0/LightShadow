#!/usr/bin/env python3
"""generate_ui_procedural.py - LightShadow night-UI textures (P2).

Generates four UI textures:
  T_UI_Panel_Night.png    256x256  - licensed pack member, vector re-raster
                                     at target size + two-tone recolor
  T_UI_Button_Night.png   256x128  - 9-slice composition of the same licensed
                                     pack member's frame/fill pieces
  T_UI_Glow_Night.png     256x256  - ORIGINAL PROCEDURAL radial glow
                                     (script-made, no external source)
  T_UI_Backdrop_Night.png 1024x512 - ORIGINAL PROCEDURAL moonlit vertical
                                     gradient (script-made, no external source)

Licensed source for panel/button: Kenney "Fantasy UI Borders", CC0 1.0
  page: https://opengameart.org/content/fantasy-ui-borders
  zip:  https://opengameart.org/sites/default/files/kenney_fantasy-ui-borders.zip
  zip sha256: 59532da3bd61195a425585455b40baa6cbf1eda8227c42adcef31a535a737769
Members used (Double style, design 000):
  - Vector/fantasy-ui-borders.svg (full-resolution vector source; the
    panel-000 member cell is at x 469.5-531.5, y 219.5-281.5 of the
    1080x1080 sheet, matched by template comparison against the PNG member)
  - PNG/Double/Panel/panel-000.png (geometry reference / verification only)
Recolor method: the pack art is flat white silhouette + alpha; each output
pixel keeps the source alpha and takes a flat palette color per region
(frame vs fill). No hue-shift of colored pixels is involved.

Palette (blue-teal night; peer-retinted 2026-11-27: frames cool, warm only
in the glow and a thin low-saturation band at the backdrop's very bottom;
no orange/gold/brown in the frames, no browns/brass anywhere):
  PANEL_FILL  #122E3A  deep blue-teal
  BUTTON_FILL #1A3D4B  lighter blue-teal
  FRAME       #8FC3CE  soft cyan (picked over cool silver #C9D6DB by visual
              comparison: reads luminous on the dark fill, silver read washed)
  GLOW_CORE   #F2C078  warm lantern amber core, fades to transparent
  BACKDROP    #0B1E29 (top) -> #16414E -> #1D4A55 (to 90% height), then a
              THIN low-saturation amber-rose band #AC8378 at the very bottom
              (plain moonlit gradient, no motif)

Glow parameters: radius = half canvas, falloff alpha = (1 - (r/R))**2.2,
peak alpha 0.85. Backdrop: piecewise-linear vertical stops below.

Requires: Pillow, numpy, ImageMagick `convert` (SVG rasterization only).
Usage: python3 Art/pipeline/generate_ui_procedural.py PACK_DIR OUT_DIR
Prints sha256 of every input member used and every output written.
"""
import hashlib
import os
import subprocess
import sys
import tempfile
from collections import deque

import numpy as np
from PIL import Image

# ---- parameters ----------------------------------------------------------
CELL = (469.5, 219.5, 531.5, 281.5)   # panel-000 member cell in the 1080 sheet
SVG_DENSITY = 384                      # 4x the 96dpi base -> 4320px sheet
PANEL_SIZE = (256, 256)
BUTTON_SIZE = (256, 128)
GLOW_SIZE = (256, 256)
BACKDROP_SIZE = (1024, 512)
CORNER_SRC = 62                        # 9-slice corner in the 248px cell
CORNER_DST = 48                        # 9-slice corner in the button (fits 128px height)

PANEL_FILL = (0x12, 0x2E, 0x3A)
BUTTON_FILL = (0x1A, 0x3D, 0x4B)
FRAME = (0x8F, 0xC3, 0xCE)
GLOW_CORE = (0xF2, 0xC0, 0x78)
GLOW_FALLOFF = 2.2
GLOW_PEAK_ALPHA = 0.85
BACKDROP_STOPS = [  # (y fraction, rgb)
    (0.00, (0x0B, 0x1E, 0x29)),
    (0.55, (0x16, 0x41, 0x4E)),
    (0.90, (0x1D, 0x4A, 0x55)),
    (1.00, (0xAC, 0x83, 0x78)),  # thin low-saturation amber-rose band at the very bottom
]
# --------------------------------------------------------------------------


def sha256(path):
    return hashlib.sha256(open(path, "rb").read()).hexdigest()


def components(mask):
    """4-connected component labels for a boolean mask."""
    h, w = mask.shape
    lab = -np.ones((h, w), dtype=np.int32)
    sizes = []
    n = 0
    for y in range(h):
        for x in range(w):
            if mask[y, x] and lab[y, x] < 0:
                q = deque([(y, x)])
                lab[y, x] = n
                s = 0
                while q:
                    cy, cx = q.popleft()
                    s += 1
                    for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                        ny, nx = cy + dy, cx + dx
                        if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and lab[ny, nx] < 0:
                            lab[ny, nx] = n
                            q.append((ny, nx))
                sizes.append(s)
                n += 1
    return lab, sizes


def load_member_alpha(pack_dir):
    """Rasterize the pack SVG, crop the panel-000 cell, return float alpha 0..1."""
    svg = os.path.join(pack_dir, "Vector", "fantasy-ui-borders.svg")
    with tempfile.NamedTemporaryFile(suffix=".png", delete=False) as tmp:
        sheet = tmp.name
    subprocess.run(["convert", "-background", "none", "-density", str(SVG_DENSITY),
                    svg, sheet], check=True)
    scale = SVG_DENSITY / 96
    x0, y0, x1, y1 = [int(round(v * scale)) for v in CELL]
    cell = Image.open(sheet).crop((x0, y0, x1, y1))
    os.unlink(sheet)
    return np.array(cell.split()[-1], dtype=np.float32) / 255.0


def split_frame_fill(alpha):
    """Split a silhouette alpha into fill (largest component) and frame (rest)."""
    mask = alpha > 0.5
    lab, sizes = components(mask)
    fill_id = int(np.argmax(sizes))
    return (lab == fill_id).astype(np.float32), ((lab >= 0) & (lab != fill_id)).astype(np.float32)


def resize_alpha(a, size):
    return np.array(Image.fromarray((a * 255).astype(np.uint8)).resize(size, Image.LANCZOS),
                    dtype=np.float32) / 255.0


def tint(fill_a, frame_a, fill_rgb, frame_rgb, size):
    fa = resize_alpha(fill_a, size)
    fr = resize_alpha(frame_a, size)
    rgb = (fa[..., None] * np.array(fill_rgb, dtype=np.float32)
           + fr[..., None] * np.array(frame_rgb, dtype=np.float32))
    alpha = np.clip(fa + fr, 0, 1)
    out = np.dstack([np.clip(rgb, 0, 255).astype(np.uint8), (alpha * 255).astype(np.uint8)])
    return Image.fromarray(out, "RGBA")


def slice9(src_a, fill_a, size, corner_src, corner_dst):
    """9-slice frame + fill from a square source cell to a target size."""
    W, H = size
    cd = corner_dst
    def part(a, x0, y0, x1, y1, w, h):
        return resize_alpha(a[y0:y1, x0:x1], (w, h))
    cs = corner_src
    S = src_a.shape[0]
    fa = np.zeros((H, W), dtype=np.float32)
    fr = np.zeros((H, W), dtype=np.float32)
    for dst_box, src_box in [
        ((0, 0, cd, cd), (0, 0, cs, cs)),
        ((W - cd, 0, W, cd), (S - cs, 0, S, cs)),
        ((0, H - cd, cd, H), (0, S - cs, cs, S)),
        ((W - cd, H - cd, W, H), (S - cs, S - cs, S, S)),
        ((cd, 0, W - cd, cd), (cs, 0, S - cs, cs)),
        ((cd, H - cd, W - cd, H), (cs, S - cs, S - cs, S)),
        ((0, cd, cd, H - cd), (0, cs, cs, S - cs)),
        ((W - cd, cd, W, H - cd), (S - cs, cs, S, S - cs)),
        ((cd, cd, W - cd, H - cd), (cs, cs, S - cs, S - cs)),
    ]:
        dx0, dy0, dx1, dy1 = dst_box
        sx0, sy0, sx1, sy1 = src_box
        w, h = dx1 - dx0, dy1 - dy0
        fr[dy0:dy1, dx0:dx1] = part(src_a, sx0, sy0, sx1, sy1, w, h)
        fa[dy0:dy1, dx0:dx1] = part(fill_a, sx0, sy0, sx1, sy1, w, h)
    return fa, fr


def make_glow(size):
    W, H = size
    yy, xx = np.mgrid[0:H, 0:W]
    r = np.sqrt((xx - (W - 1) / 2) ** 2 + (yy - (H - 1) / 2) ** 2) / (W / 2)
    a = np.clip(1 - r, 0, 1) ** GLOW_FALLOFF * GLOW_PEAK_ALPHA
    rgb = np.ones((H, W, 3), dtype=np.float32) * np.array(GLOW_CORE, dtype=np.float32)
    out = np.dstack([rgb.astype(np.uint8), (a * 255).astype(np.uint8)])
    return Image.fromarray(out, "RGBA")


def make_backdrop(size):
    W, H = size
    ys = np.linspace(0, 1, H, dtype=np.float32)
    img = np.zeros((H, 3), dtype=np.float32)
    for (y0, c0), (y1, c1) in zip(BACKDROP_STOPS, BACKDROP_STOPS[1:]):
        m = (ys >= y0) & (ys <= y1)
        t = (ys[m] - y0) / (y1 - y0)
        img[m] = (1 - t)[:, None] * np.array(c0, dtype=np.float32) + t[:, None] * np.array(c1, dtype=np.float32)
    row = np.tile(img[:, None, :], (1, W, 1)).astype(np.uint8)
    return Image.fromarray(row, "RGB")


def main():
    pack_dir, out_dir = sys.argv[1], sys.argv[2]
    os.makedirs(out_dir, exist_ok=True)
    ref = os.path.join(pack_dir, "PNG", "Double", "Panel", "panel-000.png")
    print("input panel-000.png sha256:", sha256(ref))
    print("input fantasy-ui-borders.svg sha256:",
          sha256(os.path.join(pack_dir, "Vector", "fantasy-ui-borders.svg")))

    alpha = load_member_alpha(pack_dir)
    fill_a, frame_a = split_frame_fill(alpha)
    print("cell alpha %dx%d, fill px %d, frame px %d"
          % (alpha.shape[1], alpha.shape[0], int(fill_a.sum()), int(frame_a.sum())))

    panel = tint(fill_a, frame_a, PANEL_FILL, FRAME, PANEL_SIZE)
    p = os.path.join(out_dir, "T_UI_Panel_Night.png")
    panel.save(p)
    print("wrote", p, sha256(p))

    fa, fr = slice9(frame_a + 0, fill_a, BUTTON_SIZE, CORNER_SRC, CORNER_DST)
    button = tint(fa, fr, BUTTON_FILL, FRAME, BUTTON_SIZE)
    p = os.path.join(out_dir, "T_UI_Button_Night.png")
    button.save(p)
    print("wrote", p, sha256(p))

    p = os.path.join(out_dir, "T_UI_Glow_Night.png")
    make_glow(GLOW_SIZE).save(p)
    print("wrote", p, sha256(p), "(script-generated, no external source)")

    p = os.path.join(out_dir, "T_UI_Backdrop_Night.png")
    make_backdrop(BACKDROP_SIZE).save(p)
    print("wrote", p, sha256(p), "(script-generated, no external source)")


if __name__ == "__main__":
    main()


# ---------------------------------------------------------------------------
# Loading screen art (2026-11-27, APPEND-ONLY extension; the four emitters
# above are untouched and their outputs stay byte-identical). ORIGINAL
# in-house generated: no external source, no stock. Same moonlit gradient as
# the night backdrop plus a pale moon disc and its soft glow; no text.
# ---------------------------------------------------------------------------
LOADING_SIZE = (1024, 512)
MOON_CENTER = (0.75, 0.29)   # relative (x, y)
MOON_RADIUS = 46             # px at 1024x512
MOON_GLOW_RADIUS = 190       # px, fades to 0
MOON_COLOR = (0xDC, 0xE8, 0xEA)      # pale moon
MOON_GLOW_COLOR = (0xBF, 0xD9, 0xE0) # cool glow, peak alpha 0.30


def make_loading(size=LOADING_SIZE):
    base = make_backdrop(size).convert("RGBA")
    W, H = size
    cx, cy = MOON_CENTER[0] * W, MOON_CENTER[1] * H
    yy, xx = np.mgrid[0:H, 0:W]
    dist = np.sqrt((xx - cx) ** 2 + (yy - cy) ** 2)
    glow_a = np.clip(1.0 - dist / MOON_GLOW_RADIUS, 0.0, 1.0) ** 2.2 * 0.30
    glow = np.zeros((H, W, 4), dtype=np.uint8)
    glow[..., 0], glow[..., 1], glow[..., 2] = MOON_GLOW_COLOR
    glow[..., 3] = (glow_a * 255).astype(np.uint8)
    base.alpha_composite(Image.fromarray(glow, "RGBA"))
    from PIL import ImageDraw
    disc = Image.new("RGBA", (W * 2, H * 2), (0, 0, 0, 0))
    d = ImageDraw.Draw(disc)
    d.ellipse([cx * 2 - MOON_RADIUS * 2, cy * 2 - MOON_RADIUS * 2,
               cx * 2 + MOON_RADIUS * 2, cy * 2 + MOON_RADIUS * 2],
              fill=MOON_COLOR + (255,))
    base.alpha_composite(disc.resize(size, Image.LANCZOS))
    return base.convert("RGB")


def emit_loading(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    p = os.path.join(out_dir, "T_UI_Loading_Night.png")
    make_loading().save(p)
    print("wrote", p, sha256(p), "(script-generated, no external source)")


if __name__ == "__main__":
    emit_loading(sys.argv[2])
