# Audit Action: audit-0022-validator-tests

## Metadata
- ID: audit-0022
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-006-executive-summary.md`
  - Action plan item: "Add request validator tests"
- Dependencies:
  - audit-0017, audit-0019
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0022-validator-tests.md`

## Goal
Add unit tests for billing request validators and the shared URL validation helper.

## Acceptance Criteria
- [x] AC1: UrlValidation.IsAllowedRelativePath tested for valid paths, empty inputs, protocol-relative, absolute URLs
- [x] AC2: CheckoutRequestValidator tested for valid, empty, overlong, and malicious URLs
- [x] AC3: CustomerPortalRequestValidator tested for valid, empty, overlong, and malicious URLs
- [x] AC4: Bug fix: `.When()` condition no longer skips `.NotEmpty()` rule

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (27 new validator tests)
- [x] No secrets committed
- [x] Walkthrough updated
