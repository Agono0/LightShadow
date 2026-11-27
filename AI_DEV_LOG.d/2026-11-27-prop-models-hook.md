# Prop model hook: key, crate, medkit (2026-11-27)

New SceneBuilder partial `SceneBuilderModels.TryAddModelVisual` swaps the
primitive stand-ins for the landed CC0 GLBs (SM_Key, SM_Crate, SM_Medkit from
the P1 model stream). The GLB is auto-fitted by bounds (largest dimension to a
target size, centred or standing on its base), model colliders are removed and
the primitive renderers are hidden while their colliders stay, so physics and
interaction are unchanged. If the GLB does not import, the primitives remain.

Unverified: not seen in the editor. Scale, pivot and orientation are fitted
from bounds only; the key's long axis and the medkit's facing may need a
manual rotation after a first look. Scene regenerates through BuildAll.
