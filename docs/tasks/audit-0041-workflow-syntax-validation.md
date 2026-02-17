# Audit Action: audit-0041-workflow-syntax-validation

## Metadata
- ID: audit-0041
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "Add workflow syntax validation to CI"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0041-workflow-syntax-validation.md`

## Goal
Add actionlint to CI to catch GitHub Actions workflow syntax errors before merge.

## Context
No automated validation exists for `.github/workflows/` or `.github/actions/` files. Syntax errors in workflows are only detected at runtime, which can break CI/CD pipelines.

## Scope
### In scope
- Add actionlint job to `infra-ci.yml`
- Extend path triggers to include `.github/workflows/**` and `.github/actions/**`

### Out of scope
- Custom actionlint rules or config
- Linting non-workflow YAML files

## Requirements
- R1: actionlint runs on PRs modifying workflow or action files
- R2: Workflow fails on actionlint errors

## Acceptance Criteria
- [x] AC1: actionlint job exists in `infra-ci.yml`
- [x] AC2: Path triggers include `.github/workflows/**` and `.github/actions/**`
- [x] AC3: `fail-on-error: true` is set

## Constraints
- C1: Uses `raven-actions/actionlint@v2` third-party action (well-maintained, widely used)

## Implementation Steps
1. Add `.github/workflows/**` and `.github/actions/**` to path triggers
2. Add `actionlint` job using `raven-actions/actionlint@v2`

## Testing Plan
- Unit tests: N/A (CI workflow)
- Integration tests: N/A
- Manual checks: YAML syntax validation

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
