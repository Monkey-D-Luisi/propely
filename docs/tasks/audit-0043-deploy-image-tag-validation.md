# Audit Action: audit-0043-deploy-image-tag-validation

## Metadata
- ID: audit-0043
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P3
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "Add input validation for workflow_dispatch image_tag"
- Dependencies:
  - audit-0036 (deploy.yml expression hardening)
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0043-deploy-image-tag-validation.md`

## Goal
Validate that `inputs.image_tag` matches a 40-character hex SHA format before using it in the deployment pipeline.

## Context
The `workflow_dispatch` `image_tag` input is a free-form string. While expression injection is now prevented (audit-0036), validating the format adds defense-in-depth and provides a clear error message for typos.

## Scope
### In scope
- SHA format validation step in the `prepare` job

### Out of scope
- Validating the image actually exists (handled by GHCR wait step)
- Changing the input type (string is correct for SHAs)

## Requirements
- R1: Reject image_tag values that don't match `^[0-9a-f]{40}$`
- R2: Only validate on `workflow_dispatch` trigger

## Acceptance Criteria
- [x] AC1: Validation step exists before resolve step
- [x] AC2: Only runs on `workflow_dispatch` events
- [x] AC3: Clear error message on validation failure

## Constraints
- C1: Must not affect `workflow_run` or `push` triggers

## Implementation Steps
1. Add validation step with `if: github.event_name == 'workflow_dispatch'`
2. Map `inputs.image_tag` to env var (consistent with audit-0036)
3. Regex check with `grep -qE`

## Testing Plan
- Unit tests: N/A (workflow)
- Integration tests: N/A
- Manual checks: YAML syntax validation

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
