# LIGHT SHADOW — Performance Ledger

All before/after numbers live here. Measurements on this machine (Windows, Unity 6000.4.0f1
batch tooling) plus per-tier budgets. Device reality is documented honestly (decision D8):
120 FPS is the uncapped target; Android mid-range is managed through quality tiers.

## Baseline (2026-10-30, pre-pass)

| Metric | Value | How measured |
| --- | --- | --- |
| Scene object counts | 4 scenes; 02_School YAML 1.5 MB | filesystem |
| Materials | 23 flat-colour URP/Lit, 0 textures | `find Assets/_Project/Materials -name "*.mat"` |
| Quality levels | 2 (Mobile, PC) | ProjectSettings/QualitySettings.asset |
| Batch validation | PASS (baseline) | `ValidateBatch` |
| Windows build | 125.3 MB (23 Oct build) | Builds/Windows |
| Android APK | 45.7 MB (23 Oct build) | Builds/Android |

## Targets / budgets

| Tier | Platform | Target | Notes |
| --- | --- | --- | --- |
| Ultra | Windows high-end | 120 FPS (8.3 ms) uncapped | vSync off, full post stack, 4× MSAA off for perf… tuned during pass |
| High | Windows mid | 120 FPS | reduced shadows/post |
| Medium | Android flagship/120Hz | 120 Hz render | ASTC, no realtime shadows, render scale 0.8 |
| Low | Android mid-range | stable 60 FPS | render scale 0.7, minimal post, no SSAO |

## Pass results (fill during P2)

| Metric | Before | After | Delta |
| --- | --- | --- | --- |
| — | — | — | — |
