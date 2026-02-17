# Audit Action: audit-0018-webhook-logging

## Metadata
- ID: audit-0018
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-006-executive-summary.md`
  - Action plan item: "Add webhook signature failure logging"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0018-webhook-logging.md`

## Goal
Log failed Stripe webhook signature verifications at Warning level to enable attack detection.

## Context
When `EventUtility.ConstructEvent()` throws `StripeException`, the controller returned `BadRequest()` with no logging. Failed signature verifications could indicate attack attempts that should be observable.

## Scope
### In scope
- Log `StripeException.Message` at Warning level

### Out of scope
- Webhook IP whitelisting
- Rate limiting on webhook endpoint

## Requirements
- R1: Log warning on signature verification failure with exception message

## Acceptance Criteria
- [x] AC1: Failed webhook signature logs warning with exception message

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
