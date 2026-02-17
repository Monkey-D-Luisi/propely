# Audit Action: audit-0017-url-bypass-fix

## Metadata
- ID: audit-0017
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-006-executive-summary.md`
  - Action plan item: "Fix protocol-relative URL bypass in validators"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0017-url-bypass-fix.md`

## Goal
Fix the protocol-relative URL bypass (`//attacker.com`) in path validators and extract the duplicated validation method to a shared helper.

## Context
Both `CheckoutRequestValidator` and `CustomerPortalRequestValidator` contained duplicate `IsAllowedRelativePath` methods that accepted protocol-relative URLs like `//attacker.com`. While the `_frontendBaseUrl` prepend mitigates exploitation, this is a defense-in-depth gap.

## Scope
### In scope
- Add `path.StartsWith("//")` rejection
- Extract duplicate method to shared `UrlValidation` helper
- Remove private methods from both validators

### Out of scope
- Other URL validation rules (max length handled in audit-0019)

## Requirements
- R1: URLs starting with `//` must be rejected
- R2: Single shared validation method in `UrlValidation.cs`

## Acceptance Criteria
- [x] AC1: `//attacker.com` is rejected by both validators
- [x] AC2: Valid relative paths like `/en/pricing` continue to pass
- [x] AC3: No duplicate `IsAllowedRelativePath` methods remain

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
