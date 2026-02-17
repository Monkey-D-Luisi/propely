# Audit Action: audit-0035-duplicate-registration-test

## Metadata
- ID: audit-0035
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P3
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#9 — Add duplicate-registration E2E test"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0035-duplicate-registration-test.md`

## Goal
Add an E2E test that verifies the registration form shows an appropriate error when a user attempts to register with an email that already exists.

## Context
The existing register spec tests the happy path and client-side validation (empty fields, password mismatch), but does not test the server-side duplicate email check (409 Conflict). This is a common error path that should be covered.

## Scope
### In scope
- Add duplicate-registration test to `register.spec.ts`

### Out of scope
- Testing other 409 scenarios
- Testing rate limiting on registration

## Requirements
- R1: Test registers a user, then attempts to register again with the same email
- R2: Test verifies the "An account with this email already exists." error message appears

## Acceptance Criteria
- [x] AC1: New test case added to `register.spec.ts`
- [x] AC2: Test uses `registeredPage` fixture to get a pre-registered user
- [x] AC3: Test asserts the exact error message from `en.json`

## Constraints
- Must use the existing `registeredPage` fixture (no duplicate registration logic)

## Implementation Steps
1. Add test case using `registeredPage` fixture
2. Navigate back to `/en/register` and attempt re-registration with same email
3. Assert "An account with this email already exists." error appears

## Testing Plan
- E2E: New test passes against full stack

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
