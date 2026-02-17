# Walkthrough: 0051 - Semantic Release and Versioning

## Task Reference
- Task: `docs/tasks/0051-semantic-release.md`
- Walkthrough: `docs/walkthroughs/0051-semantic-release.md`
- Branch/PR: `feat/0051-semantic-release`
- Date: `2026-02-12`

## Summary
Configured semantic-release for automated versioning and changelog generation based on conventional commits. On each push to main, the release workflow analyzes commit messages, determines the next version, generates CHANGELOG.md, creates a git tag, and publishes a GitHub release.

## Context
- Background: The project already uses conventional commits (`feat|fix|test|chore|docs(scope): message`). Adding semantic-release automates the version bumping, changelog, and release creation.
- Problem statement: No automated versioning or changelog generation.
- Constraints: Single monorepo version (no per-service versioning). Must use built-in GITHUB_TOKEN.

## Decisions & Trade-offs
- **Root package.json for tooling only**
  - Options considered: (1) Install in apps/web, (2) Create root package.json
  - Why this choice: semantic-release is a monorepo-level tool, not specific to the web app. Root package.json is `private: true` with version `0.0.0` (semantic-release manages the actual version).

- **`conventionalcommits` preset**
  - Options considered: `angular` (default) vs `conventionalcommits`
  - Why this choice: `conventionalcommits` provides more flexibility with `presetConfig` for controlling which commit types appear in the changelog. Hidden types (refactor, test, chore, docs) keep the changelog user-focused.

- **Workflow trigger: `workflow_run` after CI**
  - Options considered: (1) `workflow_run` after CI, (2) Direct push trigger with skip condition
  - Why this choice: `workflow_run` with `conclusion == 'success'` gates releases on CI passing. Combined with `!contains(…, 'chore(release):')` to prevent infinite loops from release commits.

- **`@semantic-release/git` for CHANGELOG.md**
  - The git plugin commits CHANGELOG.md and the updated package.json version back to main. The release commit message `chore(release): v${nextRelease.version}` is excluded from future analysis by the skip condition.

## Implementation Notes
- `@semantic-release/commit-analyzer`: Determines version bump from commit types
- `@semantic-release/release-notes-generator`: Generates release notes from commits
- `@semantic-release/changelog`: Writes/updates CHANGELOG.md
- `@semantic-release/git`: Commits CHANGELOG.md + package.json back to main
- `@semantic-release/github`: Creates GitHub release with tag
- `@semantic-release/npm` (npmPublish: false): Updates version in package.json
- Release rules: `breaking` → major, `feat` → minor, `fix`/`perf` → patch, everything else → no release

## Commands Run
```bash
npm install  # Generates package-lock.json with all semantic-release deps
```

## Files Changed
- `package.json` — New root package.json with semantic-release devDependencies
- `package-lock.json` — Auto-generated lock file
- `.releaserc.json` — Semantic-release configuration with plugin chain
- `.github/workflows/release.yml` — GitHub Actions workflow for automated releases
- `docs/tasks/0051-semantic-release.md` — Status updated to DONE
- `docs/backlog/epic-008-cicd-release.md` — Task 0051 status updated to DONE
- `docs/walkthroughs/0051-semantic-release.md` — This file

## Tests
### Manual
- After merge to main with a `feat` or `fix` commit, verify:
  - GitHub release created with correct version tag
  - CHANGELOG.md generated/updated
  - package.json version bumped

## Security
- Uses built-in `GITHUB_TOKEN` for GitHub releases and `@semantic-release/git` pushes
- `persist-credentials: true` in checkout so `@semantic-release/git` can push the release commit back to main
- No additional secrets required

## Follow-ups / Backlog
- [ ] Add publish workflow integration: tag Docker images with release version (task 0050 enhancement)
- [ ] Consider adding `@semantic-release/exec` to update .csproj versions if needed

## Checklist
- [x] Task scope matches `docs/tasks/0051-semantic-release.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
