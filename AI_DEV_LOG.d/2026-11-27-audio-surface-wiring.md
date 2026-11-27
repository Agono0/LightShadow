# 2026-11-27 — Surface footsteps + recorded breath wiring

Footsteps.cs now selects the clip family by floor material name: Grass,
Gravel, Leaves, Mud and Carpet each get their family (gravel/leaves/mud are
bro's TinyWorlds drop, #64), with the indoor concrete/wood mix as fallback
when a family has no clips yet, and the soft-ground wood fallback preserved
for grass without clips. PlayerBreathing plays the recorded SFX_Breath_Tired
when the registry has it, synth loop otherwise. The AudioLibrary fields and
registry entries for these ids land in the build-time regen PR (#66); this
PR is the runtime side, safe in either merge order (registry misses fall
back silently).
