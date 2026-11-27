# 2026-11-27 question batch 1 (package 5)

- Agent: AI agent working for Yusuf, `Docs/YUSUF_WORK_PLAN.md` package 5.
- Added 12 new `QuestionData` assets (2 per category: `Q_Sci_023-024`, `Q_Mat_021-022`, `Q_His_023-024`, `Q_Geo_019-020`, `Q_Lan_023-024`, `Q_Hea_017-018`) with `.meta` files and unique guids.
- All use the `Q_Geo_001.asset` field pattern, full Arabic fields and `factSource: NEEDS-FACT-CHECK`. Nothing is reviewed. Facts and Arabic need a human check.
- Existing questions, question code and `QuestionDatabase.asset` were not edited. The new assets are not linked in `QuestionDatabase.asset`, so they do not appear in the game or in the bank-integrity count until `QuestionGenerator` (which rebuilds the list from the folder) or a manual link adds them.

- Review follow-up (instinct-side, pre-merge): all 12 answers and explanations fact-checked and correct; the two most check-worthy claims sourced (Nile longest in Africa; ADA two-minute brushing guidance). Q_His_024 explanation tightened to name all three scripts on the stone. The Arabic wording in this batch is AI-written and still needs a native-speaker review before the questions are linked into QuestionDatabase.
