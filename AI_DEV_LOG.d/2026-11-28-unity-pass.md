### 2026-11-28 — Unity pass: first editor run, play-through fixes, torch, glass UI — Ag (with Claude)
- Tool used: Claude (Claude Code, driving the Unity 6000.4.0f1 editor on the owner's PC)
- What AI provided: opened the project in the real editor, regenerated scenes, ran the validator and
  EditMode tests, played the game in the editor (real key and mouse input plus an editor-only
  playtest remote) and fixed what it saw. Validator dawn-lamp exemption and fragment count; RP tier
  GUID churn; OBJ marker import (`g` groups, rig fallback, strict import tests, import policy);
  school environment reflections off (the grey-lit "dark" map); torch per the owner's reference and
  brief (wide, warm, round cookie, longer range); haze cone removed; dust mote blend; caption rows
  (Contrib/Abdelraouf, NRE in Awake); inventory detail pane position (Contrib/Abdelraouf); first quiz
  question swallowed by a late Awake; story pass missing from BuildAll (no StoryDirector, no
  chapter-1 VO); VO played before its data loaded; voice-synced typing; question fade-in; generated
  horror glass UI kit. Full list with evidence: UNITY_PASS_REPORT.md.
- What I changed myself: owner direction on the look (dark map, reference torch video, "circular,
  wide, warm" beam, glassmorphism UI, typing and fade requests) and the order of work.
- Teach-back check: Unity's OBJ importer only splits on `g` lines, so marker objects need a group
  line; a controller on an inactive panel gets its Awake inside the first SetActive(true).
- Test result: ValidateBatch RESULT: PASS; EditMode 573/573. Seen in Play mode: dark school with a
  warm torch pool, VO_SN_Act1_01 plays to 7.4 s with typed subtitles, cursor relocks after a close-up,
  the first fragment question shows. Not verified: player builds, Android, doors, Shadow chase,
  save/load, Arabic layouts, ending.
