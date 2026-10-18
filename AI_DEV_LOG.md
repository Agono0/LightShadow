# AI-Assistance Development Log — LIGHT SHADOW

Per Section 8.3 of the project document, every AI-assisted script or document gets an entry here,
so the team's transparency is provable. This log is submitted with the final deliverables.
The game itself contains **no AI features** — this log is about the development method only.

## Entry template (copy for each entry)

```text
### 【Date】 — 【File or task】 — 【Member Name】
- Tool used: (ChatGPT / Claude / Copilot)
- What AI provided: (e.g. first draft of the battery drain loop)
- What I changed myself: (e.g. removed jumping, fixed crouch height, added comments)
- Teach-back check: (one sentence: what I explained to a teammate, and to whom)
- Test result: (e.g. played 10 minutes, no Console errors, battery drains as expected)
```

---

## Entries

### 2026-10-19 — Week 1 scaffold: Section 6.6 folder tree, README.md, this log — Abdelrhman Mohamed
- Tool used: opencode (AI coding assistant)
- What AI provided: first-draft README and this log's structure; the commands that build the
  Section 6.6 folder tree; Git LFS cleanup commands for the initial commit.
- What I changed myself: checked every folder and file name against Section 6.6; kept the
  official Unity `.gitignore`/`.gitattributes` templates instead of the document's copies
  (they are a superset and already LFS-ready); decided the question bank starts with 24 drafts
  marked `NEEDS-FACT-CHECK`.
- Teach-back check: pending — this entry is docs/scaffold only; the teach-back (doc §8.3 rule 1)
  is completed with a teammate before the first gameplay script is merged.
- Test result: `git status` clean after commit; no `Library/`, `Temp/`, or `Logs/` content tracked
  (checked with `git ls-files`).

---

## Deviations from the project document (tracked here on purpose)

1. **Editor version** — doc §6.3 specifies Unity 6 LTS (6000.0.x); the installed editor on this
   machine is **6000.4.0f1**, which is now the project version. All members must match it
   (doc §6.3 golden rule: everyone on the same version).
2. **`.gitignore` / `.gitattributes`** — the doc §7.5 files were replaced by the official Unity
   templates: same exclusions plus LFS patterns for images, models, audio and binaries.
   Verified working (URP.png is committed as an LFS pointer).
3. **Question bank start size** — doc §9 Week 4 asks for 30 questions; this implementation seeds
   **24** questions (6 categories × 4), each marked `factSource: NEEDS-FACT-CHECK`, and grows to
   60+ through the human fact-check of doc §8.3 before the alpha freeze.
4. **Editor tooling folder** — added `Assets/_Project/Scripts/Tools/Editor/` for the menu tools
   (Project Setup, Generate Questions, Build Scenes); the §6.6 tree has no tooling slot.
5. **Art and audio** — deferred to the art pass (doc §9 Week 7); the first playable build is greybox.
