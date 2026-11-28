# 2026-11-27 - UI kit "paper" (scratch previews -> input-only delivery)

- Scope: 14 PNGs under Assets/_Project/Textures/UI/Kit/ + fresh metas. No PR,
  no merge, no wiring, no existing meta/import edits. Branch feat/ui-kit-paper.
- Peer decisions applied: ice #BFE6EE selected highlight (warm #F2C078 ring
  rejected); grain tile uses Paper001 luminance DIRECTLY with contrast remap
  0.5 + toroidal blend (offset-half, linear weight) - mirror averaging was
  REJECTED for banding; slots and buttons spriteBorder 24, Panel 48, HUD and
  ItemFrame 32, tile and icons 0; maxTextureSize 1024 explicit everywhere;
  tile wrap Repeat; no Unity-viewed claim.
- Origins: 9 files (kit backgrounds incl. grain tile) = Paper001-derivative
  (grain only, CC0 1.0 ambientCG); ItemFrame has dual origin (Paper001 grain,
  original layout); 5 icon glyphs + ItemFrame/bevel/color work = original
  procedural. Paper001 credited only where actually used.
- Icon bands per peer: master pale cyan, classroom silver-blue, staff muted
  teal-green; medkit muted rose cross; fuse horizontal glass tube with
  endcaps and filament, tiny pale amber-white dot, no mechanic implied.
- Repro: Art/pipeline/generate_ui_kit_paper.py (new standalone, seed 7).
- Processed sha256 per file in the manifest rows.
- Caveat: tile repeat evidence is the 4x4 render only; no seamless claim;
  tile quality pending peer review.
