# SM_Player / SM_Torch — second pass (Ziad contrib)

Original, procedurally generated static meshes. No rig, no textures, no third-party content.
Generator: `generate_models.py.txt` (Python 3, stdlib only; rename to `.py` and run
`python3 generate_models.py.txt <outdir>`; kept as `.txt` so it is not imported as a project asset).
It rebuilds all four assets byte-for-byte.

**Not tested in Unity.** No import, no material check, no rig/attachment test was run. See "Limits".

## Conventions (unchanged from the first pass)
- Units: 1 unit = 1 m, scale 1. Y up. Character faces +Z. +X = character left.
- Right-handed file space (OBJ convention), CCW winding = front face. Unity's OBJ import mirrors X
  into its left-handed space, so after import the character's left lands on -X and the model still
  faces +Z. Verify on first import; if mirrored, flip Scale X on import.
- One material per file, no textures. Sub-objects are `o` groups; Unity imports them as child meshes.
- OBJ has no pivots: the pivot is the file origin (0,0,0). Pivots, marker names and metre scale are
  the ones the first pass established.
- Every face is a triangle and carries `v/vt/vn`, with smooth (crease-aware) vertex normals and
  per-part planar UVs in 0..1.

| File | Origin | Tris | Verts | Objects | Bounding box (m) |
|---|---|---|---|---|---|
| SM_Player_Body | feet centre on the floor (y = 0) | 5648 | 3244 | 21 | x ±0.243, y 0.000..1.600, z -0.178..0.211 |
| SM_Player_Hand_L | wrist centre | 2668 | 1457 | 9 | x -0.092..0.022, y -0.040..0.046, z -0.185..0.100 |
| SM_Player_Hand_R | wrist centre | 2668 | 1457 | 9 | mirror of L |
| SM_Torch | grip point | 2560 | 1986 | 8 | x,y ±0.0328, z -0.104..0.204 |

Budgets: body ≤ 6000, each hand ≤ 3000, torch ≤ 3000. All inside, with headroom.

## Body
Rounded anatomy instead of box forms: capsule-swept limbs with tapered joints, a lofted skull with
brow / nose / cheekbone / jaw shaping, a hair shell, deltoid caps that blend the arms into the
shoulder line, a jacket with cloth folds through the torso and trouser legs, a flared hem, a standing
collar, and a hood bunched across the shoulders behind the neck.

Objects: Torso, Head, Hair, Ear_L/R, Neck, Hood, Shoulder_L/R, UpperArm_L/R, Forearm_L/R, Hand_L/R,
Thigh_L/R, Shin_L/R, Foot_L/R.

`Hand_L` / `Hand_R` are relaxed hands for when the grip hand asset is not attached. The separate
`SM_Player_Hand_*` assets are the grip hands.

## Hands (wrist origin, four separate fingers + thumb curled around the handle)
- Handle axis: parallel to +Z at x = ∓0.034, y = 0 — the Grip marker position, unchanged.
- Handle contact radius used for the wrap: **0.0212 m** (0.0200 barrel + 1.2 mm rib crest on SM_Torch).
- Fingers wrap in planes perpendicular to the handle, staggered along it (index forward, pinky back),
  starting at the palm's upper edge (θ = 56°) and curling over the top.
- The thumb comes off the palm base, wraps the underside and rises forward to the index finger.
- Cuff/sleeve with cloth folds at the wrist; palm is a rounded slab whose palmar face is tangent to
  the handle.
- Grip marker: 8 mm cube at (∓0.034, 0, 0.050). **Disable/remove its renderer** — use its transform only.

Measured wrap and clearance (left hand; right is the mirror; clearance = surface distance to the
handle contact radius, positive = gap, no interpenetration):

| Part | Wrap | Min clearance (mm) | z span in hand space (m) |
|---|---|---|---|
| Palm | 348.4 (palmar face) | +0.05 | -0.022..0.100 |
| Thumb | 161.9 | +0.55 | -0.008..0.061 |
| Finger_Index | 226.6 | +0.25 | 0.077..0.093 |
| Finger_Middle | 234.0 | +0.24 | 0.058..0.075 |
| Finger_Ring | 224.1 | +0.25 | 0.040..0.056 |
| Finger_Pinky | 208.5 | +0.27 | 0.024..0.036 |

Ring positions carry a chord-sag correction (`r = (r_handle + r_finger) / cos(step/2)`), so the
flat-sided tubes touch the handle at the chord midpoints and never cut inside it. Finger centreline
lengths are 0.088–0.096 m; a 202–234° wrap at that radius is what a 40 mm-diameter handle gives.

## Torch (grip origin, beam +Z, 0.30 m: z -0.104..0.204)
Ribbed grip (18 rings, 1.2 mm ribs, 0.0200 barrel), tapered body, head ring with a flared bezel,
recessed reflector cone, inset lens, and a switch block. Front rim at z = 0.200.
- Grip marker: 8 mm cube at the origin. Muzzle marker: 8 mm cube at (0, 0, 0.200). **Disable both renderers.**

## Grip-fit decisions and documented conflicts
1. **"0.04 m radius handle" vs the delivered torch.** A 0.04 m *radius* handle (80 mm diameter) needs
   a π·0.046 = 0.145 m finger centreline wrap at contact radius, well past a 0.09 m finger, and it is
   2.5× the 32 mm handle the first pass shipped. It is read here as 0.04 m **diameter**, i.e. a
   0.0200 m barrel, which this pass widened slightly from 0.0160 to 0.0200 (rib crest 0.0212).
   Nothing else about the torch scale changed: same origin, same +Z beam, same 0.30 m length, same
   Muzzle at 0.200. If you really want an 80 mm-diameter handle, the fingers cannot reach around it
   and the torch would need a new scale — say so and I will rebuild both sides around it.
2. **Torch tail vs sleeve.** The torch's tail sits 0.104 m behind the grip origin and the hand's wrist
   is 0.050 m behind the grip, so the tail and the cuff/forearm occupy the same volume: measured
   minimum clearance **-18.0 mm** (20 hand vertices inside the torch envelope), all of it between the
   torch tail and `Forearm`/`Cuff` where they coexist in z. The fingers, palm and thumb do not touch
   the torch anywhere (see the table above). Fix on your side by seating the torch ~20 mm forward on
   the hand bone, or tell me to shorten the tail (the grip origin would then sit nearer the tail).

## Validation performed on the shipped files
Parsed back from the written OBJ (not from the generator's state):
- Indices: every face references valid v/vt/vn; all faces are triangles; no duplicate faces.
- Topology: position-welded edge test on all four files — 0 open or non-manifold edges, 0 edges shared
  by two faces in the same direction, 0 degenerate (zero-area) faces.
- Normals: all unit length (|n| within 1e-4 of 1); no face opposes its corner vertex normals.
- UVs: all inside 0..1 (min 0.002, max 0.998).
- Pivots: body height exactly 1.600 m with min y = 0.000 and x centred; torch Grip marker centre at
  (0, 0, 0), Muzzle at (0, 0, 0.200); hand Grip markers at (∓0.034, 0, 0.050).
- Grip fit and interference: numbers above, computed from the written meshes.

## Limits (honest)
- **No Unity import was run.** Nothing here proves how Unity's OBJ importer handles the groups,
  normals, UVs or the mirrored X. Untested in-engine.
- UVs are per-part planar projections chosen on the thinnest bounding-box axis. They are continuous
  and inside 0..1, but front and back faces of a part share UV space, so they suit a solid-colour or
  triplanar material, not a unique painted texture.
- The meshes are separate closed parts that overlap at the joints (as in the first pass), not a single
  welded body. They read as one form under smooth shading but are not watertight as a whole.
- The face is stylized: brow, nose, cheekbones, jaw and hair are modelled, but there are no eyes,
  mouth interior or ears beyond a simple shell. Close-up facial fidelity is not there yet.
- The style is a clean smooth-shaded blockout of a horror protagonist (hooded jacket, gaunt head),
  not a sculpted high-poly asset. The triangle budgets are met with room to spare.
- `SM_Player_Body` still contains relaxed `Hand_L`/`Hand_R`; do not double them with the grip hands.
