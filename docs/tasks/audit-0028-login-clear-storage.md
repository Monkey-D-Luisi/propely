# Audit Action: audit-0028-login-clear-storage

## Metadata
- ID: audit-0028
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#2 — Clear all storage in login E2E test"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0028-login-clear-storage.md`

## Goal
Ensure the login E2E test starts from a completely clean browser state by clearing localStorage and sessionStorage in addition to cookies.

## Context
The login test clears cookies before testing the login flow, but does not clear localStorage or sessionStorage. If authentication state is stored in web storage, the test could pass without truly testing a fresh login.

## Scope
### In scope
- Add `localStorage.clear()` and `sessionStorage.clear()` to login test

### Out of scope
- Changing the test's assertion logic

## Requirements
- R1: All browser storage mechanisms cleared before the login test navigates to `/en/login`

## Acceptance Criteria
- [x] AC1: `localStorage.clear()` and `sessionStorage.clear()` called after `clearCookies()` in the login test

## Constraints
- Must not break existing test behavior

## Implementation Steps
1. Add `page.evaluate()` call to clear localStorage and sessionStorage after `clearCookies()`

## Testing Plan
- Manual checks: Verify test file syntax
- E2E: Login test still passes with the additional clearing

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
