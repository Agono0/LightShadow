### 2026-11-27 — Dev-log fragments: one file per PR, no more shared-tail conflicts — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: the fragment scheme (each PR logs in `AI_DEV_LOG.d/<date>-<slug>.md`
  instead of appending to the shared `AI_DEV_LOG.md` tail), the pointer section in
  `AI_DEV_LOG.md`, and the AGENTS.md rule 6 rewrite.
- What I changed myself: pending review — lands through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — why: with ~20 PRs in flight, every merge to main made every
  other PR conflict on the shared tail line; fragments keep one file per PR so parallel
  branches never touch the same lines. History stays append-only; fragments are immutable
  once merged.
- Test result: pending — CI fast-checks; docs/process change only, no code touched.
