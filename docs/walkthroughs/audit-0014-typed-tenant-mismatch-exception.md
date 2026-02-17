# Walkthrough: audit-0014-typed-tenant-mismatch-exception

## Task Reference
- Task: `docs/audits/epic-003-executive-summary.md` (action #2)
- Walkthrough: `docs/walkthroughs/audit-0014-typed-tenant-mismatch-exception.md`
- Branch/PR: `fix/audit-0013-epic-003-remediations`
- Date: `2026-02-09`

## Summary
Replaced the generic `InvalidOperationException` thrown by `SetTenantIdOnNewEntities()` with a typed `TenantMismatchException` that extends `ForbiddenException`, mapping to HTTP 403 instead of 500.

## Context
- Background: The cross-tenant injection prevention mechanism in `AppDbContext.SetTenantIdOnNewEntities()` threw a generic `InvalidOperationException`, which the `ExceptionHandlerMiddleware` mapped to HTTP 500.
- Problem statement: A 500 response is misleading — it suggests a server error rather than a security rejection. It also creates noise in monitoring/alerting systems.
- Constraints: Must follow the existing typed exception pattern in the Domain layer.

## Decisions & Trade-offs
- **Decision: Extend ForbiddenException rather than DomainException**
  - Options considered: (1) Extend `ForbiddenException` (→ 403), (2) Extend `DomainException` directly (→ 400), (3) Add explicit mapping in middleware
  - Why this choice: A tenant mismatch is fundamentally an authorization violation (you don't have permission to write data for another tenant), which maps cleanly to 403 Forbidden. By extending `ForbiddenException`, the existing middleware mapping handles it automatically with no middleware changes.

## Files Changed
- `services/ai-api/src/SaasTemplate.AiApi.Domain/Common/Exceptions/TenantMismatchException.cs` — New typed exception extending `ForbiddenException`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` — Replaced `InvalidOperationException` with `TenantMismatchException` in `SetTenantIdOnNewEntities()`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/TenantQueryFilterTests.cs` — Updated test to expect `TenantMismatchException`

## Tests
### Integration
- `SaveChanges_WithMismatchedOrgId_ShouldThrowTenantMismatchException` test (from audit-0013) updated to assert `TenantMismatchException` — confirms the typed exception is thrown with the expected message

## Checklist
- [x] Task scope matches audit action #2 in `docs/audits/epic-003-executive-summary.md`
- [x] Tests updated and passing (163 total: 5 arch + 82 unit + 76 integration)
- [x] No secrets committed
