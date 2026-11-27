# CI: cache Gradle deps against Maven Central flakes (2026-11-27)

Two Android builds tonight died on Maven Central resolving
kotlin-stdlib-jdk8:2.1.0 (armv7 on #34's first head, arm64 on #33) - an
infra hiccup, not a code problem; both pass on rerun. fast-checks and the
Library import cache already existed, but the Gradle dependency download
was un-cached, so every cold Android build re-rolled the Maven dice.
actions/cache now persists ~/.gradle/caches and ~/.gradle/wrapper per
project lockfile+editor version. Cold-cache builds still work; they just
stop depending on Maven Central being awake.
