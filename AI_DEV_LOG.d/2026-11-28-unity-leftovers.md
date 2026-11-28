## 2026-11-28 Unity pass leftovers that need no editor (lightshadow execution)
- Fragment close-up: `InspectViewController.PickVisibleMesh` takes the visible renderer with the biggest mesh bounds instead of the first MeshFilter (a hidden greybox was picked). Two EditMode tests.
- Memory text no longer printed twice: the HUD line is only shown when there is no close-up (it also ran under the CLOSE button).
- Shadow atlas spam: `AddLamp` lamps no longer cast shadows (21 shadow maps overflowed the atlas). The torch-first design keeps darkness from the torch and decorations; the torch and special lights are unchanged. The committed scenes still carry the old shadow flag until BuildAllBatch is run in the editor.
- generate_models.py (Ziad's fork, not in this repo): it must write `g <name>` after every `o <name>`, otherwise Unity's OBJ importer merges everything into one mesh and drops the Grip and Muzzle markers.
- Not done (needs the editor to see the result): glass sprites for the dialogue box and the Contrib inventory/captions/puzzle panels.
