# Walkthrough: 0012-pr-description-template

## Task Reference
- Task: `docs/tasks/0012-pr-description-template.md`
- Walkthrough: `docs/walkthroughs/0012-pr-description-template.md`
- Date: 2026-01-31

## Summary
Added a professional pull request template with required sections, quality gate checkboxes, and a short how-to-test prompt.

## Context
The repository needed a consistent PR description template to ensure reviewers receive standard information across changes.

## Decisions & Trade-offs
### Decision: Use `.github/PULL_REQUEST_TEMPLATE.md`
- Options considered: Lowercase template name vs uppercase
- Why this choice: GitHub recognizes the standard uppercase filename and this matches common team conventions.
- Consequences: None.

## Implementation Notes
- Added a PR template with Summary, Scope/Changes, How to Test, Testing, Risks, Docs/Notes, and Quality Gates sections.

## Assumptions Made
1. The standard GitHub PR template location is acceptable for the repository.

## Commands Run
- None (documentation-only change).

## Files Changed
- `.github/PULL_REQUEST_TEMPLATE.md` — PR description template
- `docs/tasks/0012-pr-description-template.md` — task definition
- `docs/walkthroughs/0012-pr-description-template.md` — walkthrough

## Tests
- Not applicable (documentation-only change).

## Docs Updated
- Added PR template and supporting task/walkthrough documentation.

## Rollback Plan
Delete the PR template and associated task/walkthrough files.

## Follow-ups / Backlog
- None.

## Checklist
- [x] Task scope matches `docs/tasks/0012-pr-description-template.md`
- [x] Walkthrough updated
- [x] No secrets committed
