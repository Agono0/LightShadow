# External Agent Brief — LightShadow

You are an AI agent contributing to **LightShadow**, a Unity 6000.4 first-person
horror game (Android + Windows) set in a Egyptian night school. You coordinate
with two existing workstreams **through this repository only**: our side
(agent: scene/gameplay code) and bro's side (assets: audio, models, textures).
Nobody can message you; leave and read signals in the repo.

**The single most important rule: you OPEN pull requests, nothing more.
Only the user's own side can merge — not you, not bro's side, not anyone
else. There is no path, no exception and no emergency that lets you merge,
push to `main`, or force-push anything.**

## 1. Read first, in this order

1. `AGENTS.md` — repo conventions (binding workflow rules).
2. `Docs/MASTER_PLAN.md` — the plan everything hangs on.
3. `Docs/AUDIT_2.md` (and `AUDIT.md`) — known problems; your audit work starts here.
4. `AI_DEV_LOG.d/` — one fragment per PR, newest first: what is already done.
5. `Docs/ASSET_MANIFEST.md` + `Docs/ASSET_LICENSES.md` — the asset register and
   its rules; `Docs/WORK_CLAIMS.md` — who owns what right now.
6. `Docs/ENDING_VO_SPEC.md` — the current ending plan (in flight).

## 2. How you contribute (hard rules)

- **PRs only, big and real.** Every PR must carry at least 5,000 lines of valuable code change (more is better; owner rule 2026-11-27). Bundle related work. Never pad, churn or duplicate to reach the number; state the count and its contents in the PR body. You NEVER merge (only
  the user's side can), never force-push, never commit to `main`. Our side
  reviews and queues merges.
- Every PR carries an `AI_DEV_LOG.d/<YYYY-MM-DD>-<slug>.md` fragment: what,
  why, what is unverified.
- Never hand-edit `.unity` / `.prefab` YAML. Scene changes go through the
  `SceneBuilder*` partials (C#); **scenes are regenerated in CI at build
  time**, so builder-code changes are enough.
- New `.cs` files need a hand-authored `.cs.meta` with a fresh GUID you have
  verified unique repo-wide (`grep -r <guid> Assets --include='*.meta'`).
- If you touch `Docs/ASSET_MANIFEST.md`: `python3 Art/pipeline/validate_asset_manifest.py`
  must print `RESULT: PASS` before you push.
- Required checks must go green: fast-checks, test-editmode, build-android
  (arm64-v8a, armeabi-v7a), build-windows. A red PR does not merge.
- Only assets with CC0/equivalent provenance, each with a register row
  (source_url, license, author, hash where available). No exceptions.

## 3. Lanes — what is taken right now (do not touch)

**Our side (in-flight chain + redesign, changes daily):**
- `Assets/_Project/Scripts/**` — all of it, especially `SceneBuilder*`,
  `GameConfig`, `GameEvents`, `Localization`, `SaveSystem`, player/enemy/UI code.
- `ProjectSettings/`, `Packages/`, `.github/`.
- `Docs/ENDING_VO_SPEC.md`, `Docs/EXTERNAL_AGENT_BRIEF.md`, `Docs/WORK_CLAIMS.md`.
- Redesign streams already underway (do not duplicate): two-hit health +
  checkpoint respawn, doors/keys + puzzle-granted keys, dynamic enemy AI,
  torch/lighting/dust, UI/loading retheme, stamina/breathing/footsteps,
  throwables, pushable props, unified ending cinematic + VO spec.

**Bro's side (assets):** `Audio/`, `Models/`, `Materials/`, `Textures/`,
`Prefabs/`, `Art/`, register rows in `Docs/ASSET_MANIFEST.md` /
`ASSET_LICENSES.md`, his `AI_DEV_LOG.d` fragments.

**Free for you (examples, claim first):**
- New audit passes (areas `AUDIT_2.md` marks open, or fresh ground: performance,
  accessibility, save-system robustness) — as reports in a PR, not code sweeps.
- New EditMode tests in `Assets/_Project/Tests/Editor/` for logic that lacks them.
- Self-contained new features NOT on the redesign list above, agreed via your
  claim entry before you build.

## 4. Claiming work (required before you start)

Append one row to `Docs/WORK_CLAIMS.md` in your PR (or comment on an Issue/PR):
date, agent name, scope, files/areas touched, status (claimed / in-review /
done). First claim wins; check the file and recent `AI_DEV_LOG.d` fragments
before claiming. Unclaimed overlapping work may be closed unmerged.

## 5. How we reply

PR comments. Our side reviews, asks in comments, and queues the merge when
green and lane-clean. Watch your PR threads.

## 6. Excluded (never)

Deploys, releases or tags; spending money or creating accounts; changes to
`ProjectSettings/`, `Packages/`, `.github/`; merges; force-pushes; assets
without provenance; hand-edited scenes/prefabs; work that collides with a
claimed lane.
