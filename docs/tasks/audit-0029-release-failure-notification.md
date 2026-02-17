# Audit Action: audit-0029-release-failure-notification

## Metadata
- ID: audit-0029
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#3 — Add release failure notification"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0029-release-failure-notification.md`

## Goal
Notify developers when the semantic-release workflow fails, so release failures don't go unnoticed.

## Context
The release workflow runs after CI passes but has no failure notification mechanism. If semantic-release fails due to token issues, network errors, or plugin misconfiguration, no one is alerted.

## Scope
### In scope
- Add a failure notification step to the release workflow

### Out of scope
- Slack/email notifications (no external integrations configured)

## Requirements
- R1: A notification is created when the semantic-release step fails

## Acceptance Criteria
- [x] AC1: A commit comment is posted when `semantic-release` fails, with a link to the failed run

## Constraints
- Use `actions/github-script` (already available, no new dependencies)
- Must only run on failure (`if: failure()`)

## Implementation Steps
1. Add `actions/github-script@v7` step with `if: failure()` after the semantic-release step

## Testing Plan
- Manual checks: Verify YAML syntax
- CI: Workflow validated on next push to main

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
