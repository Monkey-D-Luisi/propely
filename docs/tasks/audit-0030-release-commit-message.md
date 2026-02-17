# Audit Action: audit-0030-release-commit-message

## Metadata
- ID: audit-0030
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#4 — Shorten release commit message"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0030-release-commit-message.md`

## Goal
Keep release commit messages concise by removing inline release notes from the git commit body.

## Context
The `.releaserc.json` git plugin embeds `${nextRelease.notes}` in the commit message body. For releases with many changes, this creates very long commit messages that clutter `git log --oneline` and blame views. The full release notes are already available on the GitHub Release page and in CHANGELOG.md.

## Scope
### In scope
- Remove `${nextRelease.notes}` from the git commit message template

### Out of scope
- Changing how release notes are generated or displayed on GitHub Releases

## Requirements
- R1: Release commit message should be a single concise line

## Acceptance Criteria
- [x] AC1: Git commit message template is `chore(release): v${nextRelease.version}` without release notes body

## Constraints
- Must keep `chore(release):` prefix (used by skip-condition in release workflow)

## Implementation Steps
1. Update `message` field in `.releaserc.json` git plugin configuration

## Testing Plan
- Manual checks: Verify JSON syntax
- CI: Next release will use the shortened message

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
