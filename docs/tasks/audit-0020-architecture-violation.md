# Audit Action: audit-0020-architecture-violation

## Metadata
- ID: audit-0020
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-006-executive-summary.md`
  - Action plan item: "Fix API→Infrastructure architecture violation"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0020-architecture-violation.md`

## Goal
Move `BillingConfiguration` POCO from Infrastructure to Application layer so API layer no longer directly references Infrastructure.

## Context
`BillingController.cs` imported `SaasTemplate.OrgsApi.Infrastructure.Billing` to access `BillingConfiguration`, violating Clean Architecture (API should not reference Infrastructure directly). The config classes are pure POCOs with no framework dependencies and belong in the Application layer.

## Acceptance Criteria
- [x] AC1: `BillingConfiguration` resides in `Application.Billing` namespace
- [x] AC2: No `using Infrastructure.Billing` for config types in API layer
- [x] AC3: All Infrastructure services updated to use Application.Billing import
- [x] AC4: Architecture tests pass
- [x] AC5: All existing tests pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes (0 errors)
- [x] Tests pass (445 total: 5 arch + 305 unit + 135 integration)
- [x] No secrets committed
- [x] Walkthrough updated
