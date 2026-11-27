# 2026-11-27 — Dependabot for CI actions + safe version bumps

User brief: "add bump bot to fix ci action". Adds .github/dependabot.yml:
github-actions ecosystem, weekly (Sunday 06:00 Africa/Cairo), minor/patch
updates grouped into a single PR, major updates as individual PRs, cap 3 open
bot PRs. Bot PRs ride the normal required checks; auto-merge stays off.

Version check against the live GitHub releases API, bumped here:
checkout v4->v7.0.1, cache v4->v6.1.0, upload-artifact v4->v7.0.2,
download-artifact v4->v8.0.2, free-disk-space @main->v2.0.0 (pinned),
unity-builder v4->v6.0.0 (v6 is a thin wrapper around the game-ci CLI;
inputs unchanged per its release notes; this PR's own CI validates it).
Left as-is: unity-test-runner @v4 (v4.3.2 is the latest and the v4 tag
floats to it) and softprops/action-gh-release @v3 (v3.0.4 is latest).

Honest note: the recent test-editmode flake was the runner failing to
resolve the game-ci CLI from the GitHub API (HTTP 403 rate limit), an infra
hiccup, not an outdated action; no version bump removes that class, the
one-rerun policy remains the fix.
