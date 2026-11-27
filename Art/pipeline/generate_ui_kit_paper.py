#!/usr/bin/env python3
"""LightShadow UI kit "paper" generator (one-off, standalone).

Reproduces the 14 scratch-approved UI PNGs:
  Assets/_Project/Textures/UI/Kit/T_UI_Kit_{Panel_Inventory,Slot_Empty,Slot_Selected,
  Slot_Locked,Button_Normal,Button_Pressed,HUD_Frame,ItemFrame,PaperGrain_Tile}.png
  Assets/_Project/Textures/UI/Kit/T_UI_Kit_Icon_{master_key,classroom_key,staff_key,medkit,fuse}.png

Sources:
  - Paper001 (ambientCG, CC0 1.0): Paper001_1K-JPG_Color.jpg provides the paper
    grain for the 9 kit backgrounds. Expected input path passed as argv[1].
    Archive: https://ambientcg.com/get?file=Paper001_1K-JPG.zip
    zip sha256 5be094ffad8a6343ed96ec728e6eda3d84ae542cd2b16c32bc2ad67bb57a0013
    member sha256 e8db6f5b91f7882f4711ab5b2dc5c2e91f822d4f80a7d0081991532cf5cd9b9a
  - All layout, bevels, shadows, scratches, vignette, glyphs: original procedural.

Usage: python3 generate_ui_kit_paper.py /path/to/Paper001_1K-JPG_Color.jpg [out_dir]
Fixed seed 7 for deterministic output.
"""
import random, sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops, ImageOps

SRC_PATH = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else 'Assets/_Project/Textures/UI/Kit'
random.seed(7)

SRC = Image.open(SRC_PATH).convert('L')
G = ImageOps.autocontrast(SRC.crop((384, 0, 896, 512)), cutoff=1)  # uniform region, measured stddev 2.8

def grain(w, h):
    x = random.randint(0, 512 - w) if w < 512 else 0
    y = random.randint(0, 512 - h) if h < 512 else 0
    g = G.crop((x, y, x + min(w, 512), y + min(h, 512)))
    return g.resize((w, h)) if g.size != (w, h) else g

def apply_grain(img, lo=0.86, hi=1.22):
    g = grain(*img.size); gp = g.load(); ip = img.load()
    for y in range(img.height):
        for x in range(img.width):
            f = lo + (hi - lo) * (gp[x, y] / 255.0)
            r, gg, b = ip[x, y][:3]
            ip[x, y] = (min(255, int(r * f)), min(255, int(gg * f)), min(255, int(b * f)))
    return img

def scratches(img, n=14, alpha=34):
    ov = Image.new('RGBA', img.size, (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    for _ in range(n):
        x = random.randint(0, img.width); y = random.randint(0, img.height)
        l = random.randint(30, 150); dx = random.randint(-l, l); dy = random.randint(-l // 3, l // 3)
        c = (255, 255, 255, alpha) if random.random() < 0.5 else (0, 0, 0, alpha)
        d.line([(x, y), (x + dx, y + dy)], fill=c, width=1)
    return Image.alpha_composite(img.convert('RGBA'), ov)

def vignette(img, strength=0.32):
    w, h = img.size; m = Image.new('L', (w, h), 0); d = ImageDraw.Draw(m)
    d.ellipse([-w * 0.25, -h * 0.25, w * 1.25, h * 1.25], fill=255)
    m = m.filter(ImageFilter.GaussianBlur(w // 6))
    dark = Image.new('RGBA', (w, h), (0, 0, 0, int(255 * strength)))
    return Image.composite(dark, img.convert('RGBA'), m.point(lambda v: 255 - v))

def bevel(img, border, light='#8DC1CB', la=90, da=110):
    w, h = img.size; ov = Image.new('RGBA', (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    lc = tuple(int(light[i:i + 2], 16) for i in (1, 3, 5)) + (la,); dk = (0, 0, 0, da)
    for i in range(border):
        f = 1 - (i / border)
        d.line([(i, i), (w - 1 - i, i)], fill=lc[:3] + (int(lc[3] * f),))
        d.line([(i, i), (i, h - 1 - i)], fill=lc[:3] + (int(lc[3] * f * 0.6),))
        d.line([(i, h - 1 - i), (w - 1 - i, h - 1 - i)], fill=dk[:3] + (int(dk[3] * f),))
        d.line([(w - 1 - i, i), (w - 1 - i, h - 1 - i)], fill=dk[:3] + (int(dk[3] * f * 0.8),))
    return Image.alpha_composite(img.convert('RGBA'), ov)

def inner_shadow(img, border, a=70):
    w, h = img.size; ov = Image.new('RGBA', (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    for i in range(border):
        f = (1 - (i / border)) * a
        d.rectangle([i, i, w - 1 - i, h - 1 - i], outline=(0, 0, 0, int(f)))
    return Image.alpha_composite(img.convert('RGBA'), ov)

def base(w, h, c):
    return Image.new('RGB', (w, h), c)

def save(img, name):
    import os
    os.makedirs(OUT, exist_ok=True)
    img.convert('RGB').save(os.path.join(OUT, name))

# --- 9 kit backgrounds (Paper001-derivative) ---
p = apply_grain(base(1024, 1024, '#122E3A')); p = scratches(p); p = vignette(p); p = bevel(p, 48); p = inner_shadow(p, 10, 50)
save(p, 'T_UI_Kit_Panel_Inventory.png')
s = apply_grain(base(256, 256, '#16242C')); s = inner_shadow(s, 40, 110); s = bevel(s, 24, la=60)
save(s, 'T_UI_Kit_Slot_Empty.png')
sel = apply_grain(base(256, 256, '#16242C')); sel = inner_shadow(sel, 40, 100)
ov = Image.new('RGBA', (256, 256), (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
d.rounded_rectangle([6, 6, 249, 249], radius=10, outline=(191, 230, 238, 255), width=5)
d.rounded_rectangle([12, 12, 243, 243], radius=8, outline=(191, 230, 238, 90), width=2)
sel = Image.alpha_composite(sel.convert('RGBA'), ov.filter(ImageFilter.GaussianBlur(1)))
save(sel, 'T_UI_Kit_Slot_Selected.png')
lk = apply_grain(base(256, 256, '#101B21'), lo=0.88, hi=1.12); lk = inner_shadow(lk, 40, 120)
save(lk, 'T_UI_Kit_Slot_Locked.png')
b = apply_grain(base(512, 128, '#1A3D4B')); b = bevel(b, 24); b = scratches(b, 8, 24)
save(b, 'T_UI_Kit_Button_Normal.png')
bp = apply_grain(base(512, 128, '#122E3A')); bp = inner_shadow(bp, 28, 120); bp = scratches(bp, 8, 24)
save(bp, 'T_UI_Kit_Button_Pressed.png')
hf = apply_grain(base(1024, 256, '#122E3A')); hf = bevel(hf, 32)
d = ImageDraw.Draw(hf); d.rectangle([40, 40, 983, 215], outline=(141, 193, 203, 160), width=3)
save(hf, 'T_UI_Kit_HUD_Frame.png')
fr = apply_grain(base(256, 256, '#142B35')); fr = bevel(fr, 32); fr = inner_shadow(fr, 8, 60)
save(fr, 'T_UI_Kit_ItemFrame.png')
# grain tile: Paper001 luminance direct, contrast remap 0.5, toroidal blend (NO mirroring)
t = G.crop((0, 0, 512, 512)).resize((256, 256), Image.LANCZOS)
px = list(t.getdata()); mean = sum(px) // len(px)
t = Image.eval(t, lambda v: max(0, min(255, int((v - mean) * 0.5 + 128))))
A = t.load(); rolled = ImageChops.offset(t, 128, 128); B = rolled.load()
out = Image.new('L', (256, 256)); O = out.load()
for i in range(256):
    for j in range(256):
        w = min(i, 255 - i, j, 255 - j) / 128.0
        w = 1.0 if w > 1.0 else w
        O[i, j] = int(A[i, j] * w + B[i, j] * (1 - w))
save(out, 'T_UI_Kit_PaperGrain_Tile.png')

# --- 5 icons + ItemFrame glyphs (original procedural, no Paper001) ---
BG = '#16242C'; GLYPH = '#DCE8EA'
def canvas():
    im = Image.new('RGB', (256, 256), BG); dr = ImageDraw.Draw(im)
    dr.rounded_rectangle([4, 4, 251, 251], radius=12, outline=(141, 193, 203, 120), width=3)
    return im, dr
def band(im, dr, hex_):
    c = tuple(int(hex_[i:i + 2], 16) for i in (1, 3, 5))
    dr.rounded_rectangle([52, 206, 204, 238], radius=8, fill=c)
def key(bow):
    im, dr = canvas(); cy = 92
    if bow == 'diamond':
        dr.polygon([(128, cy - 42), (170, cy), (128, cy + 42), (86, cy)], outline=GLYPH, width=10)
        dr.ellipse([118, cy - 10, 138, cy + 10], outline=GLYPH, width=6)
    elif bow == 'rect':
        dr.rounded_rectangle([88, cy - 36, 168, cy + 36], radius=10, outline=GLYPH, width=10)
        dr.ellipse([118, cy - 10, 138, cy + 10], outline=GLYPH, width=6)
    else:
        dr.ellipse([88, cy - 40, 168, cy + 40], outline=GLYPH, width=10)
        dr.ellipse([118, cy - 10, 138, cy + 10], outline=GLYPH, width=6)
    dr.line([(128, cy + 42), (128, 196)], fill=GLYPH, width=12)
    dr.line([(128, 168), (156, 168)], fill=GLYPH, width=10)
    dr.line([(128, 190), (150, 190)], fill=GLYPH, width=10)
    return im, dr
im, dr = key('diamond'); band(im, dr, '#8FD8E8'); save(im, 'T_UI_Kit_Icon_master_key.png')
im, dr = key('rect');    band(im, dr, '#6FB4C2'); save(im, 'T_UI_Kit_Icon_classroom_key.png')
im, dr = key('round');   band(im, dr, '#4E9C86'); save(im, 'T_UI_Kit_Icon_staff_key.png')
im, dr = canvas()
dr.rounded_rectangle([56, 84, 200, 192], radius=14, outline=GLYPH, width=10)
dr.line([(128, 84), (128, 64)], fill=GLYPH, width=10); dr.line([(104, 64), (152, 64)], fill=GLYPH, width=10)
dr.rectangle([116, 104, 140, 172], fill='#A25D56'); dr.rectangle([96, 126, 160, 150], fill='#A25D56')
save(im, 'T_UI_Kit_Icon_medkit.png')
im, dr = canvas()
dr.rounded_rectangle([48, 96, 208, 160], radius=20, outline=GLYPH, width=8)
dr.rectangle([30, 90, 56, 166], fill='#8FA7AC'); dr.rectangle([200, 90, 226, 166], fill='#8FA7AC')
dr.line([(66, 128), (100, 128)], fill=GLYPH, width=4)
dr.line([(100, 128), (112, 119), (124, 137), (136, 119), (148, 137), (156, 128), (190, 128)], fill='#E8F2F4', width=4)
dr.ellipse([122, 122, 134, 134], fill='#F5E9CF')
save(im, 'T_UI_Kit_Icon_fuse.png')
print('generated 14 PNGs into', OUT)
