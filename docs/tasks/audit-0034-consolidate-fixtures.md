# Audit Action: audit-0034-consolidate-fixtures

## Metadata
- ID: audit-0034
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P3
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#8 — Consolidate duplicate fixtures"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0034-consolidate-fixtures.md`

## Goal
Remove the duplicated `registerUser()` call between `registeredPage` and `authenticatedPage` fixtures while keeping both fixture names for semantic clarity.

## Context
Both `registeredPage` and `authenticatedPage` called `registerUser()` identically and returned the same structure. The only difference was the fixture name, which serves semantic purposes (login tests use `registeredPage` to signal "about to clear cookies", while org/invite tests use `authenticatedPage` to signal "ready to use the app").

## Scope
### In scope
- Make `authenticatedPage` delegate to `registeredPage`

### Out of scope
- Removing either fixture name (both serve semantic purposes)

## Requirements
- R1: No duplicated `registerUser()` calls
- R2: Both fixture names remain available

## Acceptance Criteria
- [x] AC1: `authenticatedPage` delegates to `registeredPage`
- [x] AC2: All E2E tests still pass

## Constraints
- Must not change test behavior

## Implementation Steps
1. Change `authenticatedPage` to depend on `registeredPage` instead of `page` + `testUser`

## Testing Plan
- Unit tests: N/A
- E2E: All existing tests pass unchanged

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
