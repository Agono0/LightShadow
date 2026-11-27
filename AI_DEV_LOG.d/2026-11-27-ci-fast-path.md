### 2026-11-27 — CI fast path: docs-only PRs skip the Unity builds — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: a `changes` detector job in `.github/workflows/build.yml` (PR diff vs
  base; anything under Assets/, ProjectSettings/, Packages/ or .github/ counts as code) and
  per-step guards on the three build jobs so a docs-only PR (audit notes, cue sheets,
  dev-log fragments, READMEs) finishes its required checks in seconds instead of ~35
  minutes of Unity player builds it cannot affect.
- What I changed myself: pending review — lands through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — the guard lives on the steps, not the jobs: a skipped JOB can
  leave a required check pending forever, while a job whose steps all skip still completes
  and reports success under the same check name. Library caching and PR concurrency-cancel
  already existed and are unchanged.
- Test result: pending — this PR touches .github/, so the detector classifies it as code
  and the full builds run on it as usual; YAML validated locally with a parser.
