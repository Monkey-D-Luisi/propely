# Task: 0012-pr-description-template

## Metadata
- ID: 0012
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-31
- Related docs:
  - Walkthrough: `docs/walkthroughs/0012-pr-description-template.md`

## Goal
Provide a professional pull request description template for consistent PRs.

## Context
The repository does not yet include a standard PR template, which leads to inconsistent PR descriptions and missing quality gate information.

## Scope
### In scope
- Add a PR template file under `.github/` with required sections.

### Out of scope
- CI changes
- Code changes beyond documentation

## Requirements
- R1: Template includes Summary, Scope/Changes, Testing, Risks, Docs/Notes sections.
- R2: Content is professional and English-only.
- R3: Optional checkboxes for quality gates and a short “How to Test” section.
- R4: Walkthrough created and updated.

## Acceptance Criteria
- AC1: `.github/PULL_REQUEST_TEMPLATE.md` exists and contains the required sections.
- AC2: Quality gate checkboxes are present.
- AC3: Walkthrough file exists and documents the change.

## Constraints (non-negotiable)
- English-only repository content.
- No secrets in repository.

## Proposed Approach (high-level)
1. Create the PR template file with professional sections and checklists.
2. Add matching walkthrough documentation.

## Implementation Steps
1. Add `.github/PULL_REQUEST_TEMPLATE.md`.
2. Create `docs/walkthroughs/0012-pr-description-template.md`.

## Files to Create / Modify
- `.github/PULL_REQUEST_TEMPLATE.md`
- `docs/tasks/0012-pr-description-template.md`
- `docs/walkthroughs/0012-pr-description-template.md`

## Testing Plan
- Not applicable (documentation-only change).

## Security & Privacy
- No secrets are introduced.

## Observability
- Not applicable.

## Rollback Plan
Delete the PR template and associated task/walkthrough files.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Walkthrough updated
- [x] No secrets committed
