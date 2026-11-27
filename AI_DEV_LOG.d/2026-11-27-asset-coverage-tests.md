# Asset coverage tests (2026-11-27)

EditMode tests: EN and AR VO clip sets match, every ending line has a clip in
both languages, and every literal id passed to Sfx.Play/PlayAt/Pick resolves to
a shipped SFX file (catches typos that otherwise fail silently). The SFX test
only becomes meaningful once the wiring PR (#98) is on main; before that it
finds no calls and passes.
