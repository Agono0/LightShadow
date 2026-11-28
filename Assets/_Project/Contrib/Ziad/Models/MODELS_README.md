# SM_Player / SM_Torch placeholder models (Ziad contrib)

Original, procedurally generated low-poly static meshes. No rig, no textures, no third-party content.
Generator: `generate_models.py.txt` (Python 3, stdlib only; rename to .py and run `python3 generate_models.py.txt <outdir>`; kept as .txt so it is not treated as a project asset).

NOT tested in Unity. Import, scale, normals, material look and fit with torch/hands are unverified there.

## Conventions
- Units: 1 unit = 1 m, scale 1. Y up. Character faces +Z.
- File is written right-handed (OBJ convention): +X = the character's LEFT, CCW winding = front face.
  Unity's OBJ import mirrors X to its left-handed space, so after import the character's left should land on -X (Unity left) and the model should face +Z. Check this on first import; if mirrored, flip Scale X on import or re-export.
- Each file has one material (no textures). Sub-objects are `o` groups; Unity imports them as child meshes.
- OBJ has no pivots: the pivot is the file origin (0,0,0).

| File | Origin | Height / size (m) | Tris | Material (Kd) |
|---|---|---|---|---|
| SM_Player_Body | feet centre on floor (y=0) | 1.60 tall, 0.57 wide, z -0.105..0.20 | 400 | MAT_Player_Body 0.14 0.13 0.13 |
| SM_Player_Hand_L | wrist centre | z -0.28..0.13 (forearm back along -Z, hand forward +Z) | 168 | MAT_Player_Hand 0.17 0.15 0.14 |
| SM_Player_Hand_R | wrist centre | mirror layout of L | 168 | MAT_Player_Hand |
| SM_Torch | grip point | 0.30 long along +Z (z -0.10..0.20), 0.08 dia head | 196 | MAT_Torch 0.10 0.10 0.11 |

## Objects
- Body: Head, Neck, Torso, UpperArm_L/R, Forearm_L/R, Hand_L/R, Thigh_L/R, Shin_L/R, Foot_L/R. Arms hang straight down.
- Hands: Forearm, Palm, Fingers, Thumb, Grip. Palm faces the body midline (L palm faces -X, R palm faces +X). Grip marker at (-/+0.034, 0, 0.05) (L is -X, R is +X), a 8 mm cube.
- Torch: Handle, Body, Reflector, Lens, Switch, Grip (cube at origin), Muzzle (cube at z=0.200, lens front). Torch beam direction = +Z.
- Grip and Muzzle are 8 mm cubes: use their position, then disable/remove the renderer.
- Fingers are a simplified curled block pose, not a fitted grip around the torch handle.
