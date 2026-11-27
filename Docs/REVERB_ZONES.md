# LIGHT SHADOW — Reverb Zone Spec (audit + gaps)

Status: SPEC / AUDIT ONLY. No scene, prefab, mixer or script changes are made by
this document or its PR. Zone generation lives in the lead's
`Scripts/Tools/Editor/SceneBuilderScenes.cs` (`AttachReverbZones`); any change
to it is the lead's work. Scope source: premium-pass brief §8.3 (via
`Docs/DECISIONS.md` D5, `Docs/MASTER_PLAN.md` §8.3) and the mixer spec
`Docs/AGENT_REPORT_AUDIO_MIXER.md`.

References verified against `origin/main` 75eb286 on 2026-11-27:
`TASKS.md` marks "Reverb zones + occlusion" done for the school scene;
`SceneBuilderScenes.cs` contains the `ReverbRoom[]` table and falloff math;
`02_School.unity` contains 8 baked `AudioReverbZone` components;
`03_Ending.unity` contains none.

## 1. What exists today (02_School, builder-generated)

| Room (ReverbRoom.name) | Preset (code) | Enum value | Brief §8.3 tier |
| --- | --- | --- | --- |
| Library | `Room` | 3 | "library dead" |
| PhysicsLab | `Room` | 3 | (small/dry) |
| Classroom | `Room` | 3 | (small/dry) |
| Garden | `Room` | 3 | (small/dry) |
| StaffRoom | `Room` | 3 | (small/dry) |
| Corridor | `StoneCorridor` | 14 | "corridor medium" |
| Entrance | `StoneCorridor` | 14 | "corridor medium" |
| MainHall | `Concerthall` | 8 | "assembly hall long" |

Falloff is structural, computed in the builder: `minDistance` = half of the
smaller floor extent (fully-wet radius fits inside the room), `maxDistance` =
the full floor diagonal. Zone colliders are triggers on the Default layer,
deliberately outside the Level-layer NavMesh bake mask and all project queries.
The table is idempotent (`ReverbZone` child guard) and baked into
`02_School.unity` (8 components, presets 3 x5, 14 x2, 8 x1 — matches the table).

Occlusion is a separate system already landed: `AudioOcclusionProbe` on the
player camera, interval linecasts, lazy low-pass 2400 Hz + -6 dB on blocked 3D
sources (TASKS.md). Not part of this doc.

## 2. Gaps and deviations (for lead decision — nothing here is claimed broken)

1. **03_Ending has no reverb zones.** The dawn/ending scene
   (`03_Ending.unity`) contains zero `AudioReverbZone` components. If the dawn
   sequence plays memory/music cues with any 3D sources, they run dry.
   Proposal: one outdoor-ish zone over the ending area. The brief gives no
   tier for dawn; candidates are `Plain` (open, nearly dry) or `City`
   (street-like early reflections). Needs a lead pick + builder table entry.
2. **"Library dead" tier is implemented as `Room`.** The brief's tier word is
   "dead". Unity's `Room` preset still adds a short tail; the drier options are
   `PaddedCell` (near-anechoic) or `CarpetedHallway`. Current choice may be
   intentional (a library with shelves is not anechoic); flagging the wording
   gap only.
3. **TASKS.md names a preset that does not exist.** TASKS.md says
   "Corridor/Entrance = MediumRoom"; `AudioReverbPreset` has no `MediumRoom`
   member — the code comment already notes this and uses `StoneCorridor`.
   Suggest correcting the TASKS.md wording when it is next touched.
4. **Garden uses an indoor preset.** The Garden room is presumably open-air;
   `Room` adds an indoor tail. Candidates: `Plain` or `Forest`. Low priority.
5. **No per-source reverb level policy is documented.** Unity sends reverb per
   `AudioSource.reverbZoneMix` (default 1). Voice (VO) and UI sources typically
   want 0 (dry); footsteps/ambience want 1. No code sets `reverbZoneMix`
   anywhere today (checked `Assets/_Project/Scripts` on main). Proposal: the
   audio wiring pass sets `reverbZoneMix = 0` on VO/UI sources when clips land.

## 3. Interaction with the mixer work (informational)

Reverb zones are scene-side and independent of the `M_Main.mixer` snapshots in
`Docs/AGENT_REPORT_AUDIO_MIXER.md` (Explore / Stalk / Chase / Hide /
MemoryFlash / Dawn own ducking and the Master low-pass). No conflict: snapshots
never touch reverb, zones never touch gains. The one overlap to keep in mind is
perceived wetness during `Snapshot_Hide` (low-pass on Master also dulls reverb
returns, which usually reads as *more* enclosed — desirable).

## 4. Verification checklist (human, editor — cannot be run here)

- [ ] Walk each room with a looping footstep/ambience source; confirm the
      preset reads as intended (library dry-ish, corridor medium, hall long).
- [ ] Cross two adjacent zone boundaries (Corridor <-> Library); confirm the
      blend is smooth and `minDistance`/`maxDistance` falloff feels structural.
- [ ] Confirm zone triggers never interact with interaction rays, `EnemySight`
      or the occlusion linecasts (they are on the Default layer by design).
- [ ] If gap 1 is accepted: audition the dawn scene with the chosen preset.
