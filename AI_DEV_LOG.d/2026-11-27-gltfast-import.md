# 2026-11-27 — glTFast for the wraith GLB

Bro's wraith candidate (register SM_Wraith_GhostSkull) ships as .glb; the
project had no glTF importer. Adds com.unity.cloud.gltfast 6.21.0 so the
editor and CI import it directly (skinned mesh + animation clips), no offline
conversion. Packages-lock regenerates on next editor open; the manifest is
the committed truth. The model itself stays a bro-lane PR at
Assets/_Project/Models/SM_Wraith_GhostSkull.glb.
