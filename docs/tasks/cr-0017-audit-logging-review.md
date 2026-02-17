# Code Review: cr-0017 — Audit Logging Infrastructure

## PR Metadata
- **PR:** #157 — feat(audit): add audit logging infrastructure for orgs-api (#0017)
- **Branch:** feat/0017-audit-logging → main
- **CI Status:** All checks passing (Orgs API Build & Test: SUCCESS, others: SKIPPED/SUCCESS)

## Changed Files
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0017-audit-logging.md`
- `docs/walkthroughs/0017-audit-logging.md`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Services/HttpAuditContext.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IAuditContext.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Common/AuditLog.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260206182723_AddAuditLogs.Designer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260206182723_AddAuditLogs.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Persistence/AuditLogTests.cs`

## Review Sources
- Inline review comments: **12** (codex, gemini-code-assist, copilot)
- General reviews: **3** (summaries only, no additional issues)
- Issue comments: **1** (summary only, no actionable feedback)

## Review Threads (deduplicated)

### MUST_FIX

- [ ] **A. Exclude sensitive fields from audit JSON** — `PasswordHash` and `Token` are serialized into audit log `Changes` column by both `SerializeModifiedProperties` and `SerializeAddedProperties`. This leaks secrets to anyone with DB access. Add a denylist of sensitive property names.
  - Sources: codex #1 (P1), gemini #2 (critical), gemini #3 (critical), copilot #9, copilot #12
  - Files: `AppDbContext.cs` (lines 146, 165), `walkthroughs/0017-audit-logging.md` (line 120)

- [ ] **B. Truncate CorrelationId to column max length** — CorrelationId from request headers could exceed 64-char column limit, causing `DbUpdateException` in `base.SaveChangesAsync` which rolls back the business transaction. Breaks R5 best-effort guarantee.
  - Source: gemini #4 (medium/security)
  - File: `AppDbContext.cs` (line 100)

### SHOULD_FIX

- [ ] **C. Capture deleted entity state in Changes** — For `Deleted` entities, `changes` is currently `null`. Serializing original values provides a more complete audit trail.
  - Source: gemini #5 (medium)
  - File: `AppDbContext.cs` (line 122)

- [ ] **D. Per-entity try/catch in CreateAuditEntries** — A single serialization failure for one entity skips audit logging for all remaining entities. Wrap the per-entry body in try/catch.
  - Source: copilot #8
  - File: `AppDbContext.cs` (line 106–127)

- [ ] **E. Audit DB errors in base.SaveChangesAsync** — The try/catch only protects `CreateAuditEntries()` (in-memory). If the audit rows violate DB constraints (e.g., column length), `base.SaveChangesAsync` throws and rolls back business data too.
  - Source: copilot #7
  - Mitigation: Field validation (Issue B truncation) + per-entity catch (Issue D) makes DB-level errors unlikely. Full transaction separation is out of scope for this PR.

### SUGGESTION

- [ ] **F. Stronger test assertion for Updated audit log** — `auditLog.Changes.Should().Contain("IsDeleted")` only checks substring. Deserialize JSON and assert on structure.
  - Source: gemini #6
  - File: `AuditLogTests.cs` (line 81)

- [ ] **G. Explicit .Where() in foreach loops** — SerializeModifiedProperties/SerializeAddedProperties use implicit filtering in foreach. Use explicit `.Where()` for clarity.
  - Sources: copilot #10, copilot #11
  - Files: `AppDbContext.cs` (lines 154, 169)

### OUT_OF_SCOPE

- **E (full fix)**: Separate transaction/DbContext for audit logs. Deferred — field validation + per-entity catch is sufficient for this PR. Tracked as follow-up.

## Comment Resolution Plan

### MUST_FIX
- [ ] A1: Add `SensitiveProperties` denylist to `AppDbContext` and filter in `SerializeModifiedProperties` and `SerializeAddedProperties`
- [ ] A2: Update walkthrough Security section to reflect the fix
- [ ] B1: Truncate `correlationId` to 64 chars in `CreateAuditEntries`

### SHOULD_FIX
- [ ] C1: Add `SerializeDeletedProperties` method and wire into switch expression
- [ ] D1: Wrap per-entry loop body in individual try/catch
- [ ] E1: Add truncation for `entityType` (100) and `entityId` (200) fields as defensive measure

### SUGGESTION
- [ ] F1: Deserialize Changes JSON in test and assert on structure
- [ ] G1: Use `.Where()` in serialize methods for explicit filtering

## Execution Log
<!-- Updated as fixes are applied -->
