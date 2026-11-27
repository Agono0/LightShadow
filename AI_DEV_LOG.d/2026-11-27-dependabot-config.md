# Dependabot config for GitHub Actions (2026-11-27)

- Adds .github/dependabot.yml only: weekly, minor and patch grouped, majors individual.
- No workflow files change. The earlier combined PR (#68) bumped actions in build.yml and validate.yml at once and its build jobs failed, so it was closed. Dependabot now proposes each bump as its own PR and each runs the required checks.
- Bot PRs are not auto-merged; a maintainer arms them after CI passes.
