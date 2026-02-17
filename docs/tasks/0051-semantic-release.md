# Task: 0051 - Semantic Release and Versioning

## Metadata
- ID: 0051
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #203
- Epic: `docs/backlog/epic-008-cicd-release.md`
- Old Issue: #40
- Milestone: v1.0

## Goal
Configure semantic-release for automated versioning and changelog generation based on conventional commits.

## Context
The project already uses conventional commits (`feat|fix|test|chore|docs(scope): message`). Semantic-release can automatically determine the next version number, generate changelogs, and create GitHub releases based on commit messages.

## Scope
### In scope
- Install and configure semantic-release
- Configure for monorepo (single version for all services)
- Auto-generate CHANGELOG.md from conventional commits
- Create GitHub releases with tags
- Update version in `package.json` and optionally `.csproj` files
- GitHub Actions workflow for release (on main branch)

### Out of scope
- Per-service versioning (use single monorepo version)
- NPM publishing
- NuGet publishing

## Requirements
- R1: Merges to main trigger version analysis
- R2: `feat` commits trigger minor version bump
- R3: `fix` commits trigger patch version bump
- R4: `BREAKING CHANGE` triggers major version bump
- R5: CHANGELOG.md generated automatically
- R6: GitHub release created with tag

## Acceptance Criteria
- AC1: A feat commit on main creates a new minor release
- AC2: CHANGELOG.md is generated/updated
- AC3: GitHub release is created with release notes
- AC4: Git tag is created (e.g., v1.2.0)
- AC5: Version is updated in package.json

## Constraints (non-negotiable)
- Use conventional commit format (already in use)
- Single version for entire monorepo
- Update walkthrough

## Implementation Steps

1. **Install semantic-release** (root-level npm dev dependency)
   - `npm install --save-dev semantic-release @semantic-release/changelog @semantic-release/git`

2. **Create config** (`.releaserc.json` or `release.config.js` in root)
   - Plugins: commit-analyzer, release-notes-generator, changelog, git, github

3. **Create release workflow** (`.github/workflows/release.yml`)
   - Trigger: push to main (after CI and publish pass)
   - Run semantic-release

4. **Verify conventional commits** are parsed correctly

## Files to Create / Modify

### Create
- `.releaserc.json`
- `.github/workflows/release.yml`
- `docs/walkthroughs/0051-semantic-release.md`

### Modify
- `package.json` (root - add semantic-release devDependency and version)

## Testing Plan
- Manual: Verify release on test branch
- CI: Workflow runs and creates release

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Release workflow succeeds
- [x] CHANGELOG.md generated
- [x] GitHub release created *(v1.0.0, v1.0.1)*
- [x] Walkthrough updated
