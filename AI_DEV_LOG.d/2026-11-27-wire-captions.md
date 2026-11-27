# Wire sound captions to audio call sites (2026-11-27)

- Sfx.Play and Sfx.PlayAt raise a caption cue for ids that have a matching cue (power surge, gate slam, steel door bang, shadow hiss, moan, roam, ambush stinger). Non-positional calls use the main camera position.
- ShadowAudio raises "Shadow growl" when the wraith enters Chase or Repelled.
- CaptionService.Raise is a no-op when the service is missing or captions are off. The 0.75 s duplicate window absorbs the hiss plus growl overlap.
- Only five cues exist in captions.json, so no other ids are mapped. Arabic cue text is not native-verified. Not seen in the editor or on a device.
