### 2026-11-27 — Audit A5: hide Resolution/Fullscreen settings rows on mobile — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `SettingsPanelController` now treats the Resolution and Fullscreen rows as
  desktop-only: on `Application.isMobilePlatform` they are hidden via the existing
  `ApplyPageVisibility` path (page toggling can never resurrect them) and the surviving page A
  rows reflow with the builder's own layout formula (y = -120 - i * 64, prev/next at +3) so no
  dead gap remains. Runs in Awake on both settings panels (pause menu + main menu), which share
  the one `BuildSettingsPanel` builder.
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains why hiding goes through ApplyPageVisibility
  (it is the single place row visibility is decided) and why reflow reuses the builder's
  constants instead of a layout group (the panel is fixed-position by design).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Desktop is
  untouched (HiddenOnMobile is false, no reflow runs). Closes AUDIT_2.md finding A5.
