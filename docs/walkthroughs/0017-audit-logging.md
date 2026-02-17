# Walkthrough: 0017-audit-logging

## Task Reference
- Task: `docs/tasks/0017-audit-logging.md`
- Walkthrough: `docs/walkthroughs/0017-audit-logging.md`
- Branch/PR: `feat/0017-audit-logging`
- Date: `2026-02-06`

## Summary
Implemented automatic audit logging infrastructure for the orgs-api service. Every create, update, and delete operation on domain entities is captured in a dedicated `audit_logs` table with user identity, correlation ID, and JSON-serialized property changes. The interception is done transparently in `AppDbContext.SaveChangesAsync` using EF Core ChangeTracker, so no individual command handlers need modification.

## Context
- Background: A professional SaaS needs an audit trail for compliance, debugging, and security. No mutations were being logged.
- Problem statement: Create/update/delete operations had no record of who performed them, when, or what changed.
- Constraints: Must follow Clean Architecture, must not fail the main operation (R5 — best-effort), must integrate with existing correlation ID infrastructure.

## Decisions & Trade-offs
- **EF Core SaveChangesAsync override vs MediatR pipeline behavior:**
  - Options considered: MediatR behavior that wraps each command, or ChangeTracker interception in SaveChangesAsync.
  - Why this choice: SaveChangesAsync intercepts all mutations automatically — no need to decorate every command handler. ChangeTracker already knows Added/Modified/Deleted states with original and current values.
  - Consequences: Audit entries are created in the same transaction as the main operation. Non-Entity types (OutboxMessage, ProcessedEvent, AuditLog itself) are excluded because they don't extend the `Entity` base class.

- **IAuditContext interface vs injecting IHttpContextAccessor directly:**
  - Options considered: Inject IHttpContextAccessor into AppDbContext, or define an abstraction.
  - Why this choice: `IAuditContext` in the Application layer keeps Infrastructure decoupled from HTTP concerns. The `HttpAuditContext` implementation lives in the Api layer where HTTP context is available.
  - Consequences: Tests can use a simple `TestAuditContext` without mocking HTTP infrastructure.

- **Best-effort audit (R5) — try-catch in SaveChangesAsync:**
  - Audit entry creation is wrapped in try-catch. If serialization or any error occurs, a warning is logged and the main operation proceeds.
  - Consequences: Audit logs are never a blocker for business operations. Missing audit entries are detectable via log warnings.

- **Dual constructor pattern for AppDbContext:**
  - Parameterless constructor (for EF tooling and tests without audit), and a constructor with `IAuditContext` + `ILogger` (for runtime).
  - Consequences: When `_auditContext` is null (parameterless constructor), audit entries still get created with null UserId/CorrelationId. EF tooling and simple tests work without DI.

## Implementation Notes

### Domain Layer

**Created:**
- `Domain/Common/AuditLog.cs` — Standalone entity (does not extend `Entity` base class — no domain events needed). Fields: Id, UserId, Action (Created/Updated/Deleted), EntityType, EntityId, Changes (JSON), CorrelationId, CreatedAtUtc. Factory method `AuditLog.Create(...)`.

### Application Layer

**Created:**
- `Application/Common/Interfaces/IAuditContext.cs` — Interface providing `Guid? UserId` and `string? CorrelationId` for audit context injection.

### Infrastructure Layer

**Created:**
- `Persistence/Configurations/AuditLogConfiguration.cs` — Table: `audit_logs`, snake_case columns, JSONB for `changes`, ValueGeneratedNever for Id, indexes on (entity_type, entity_id), user_id, and created_at_utc.

**Modified:**
- `Persistence/AppDbContext.cs` — Added `DbSet<AuditLog> AuditLogs`. Added second constructor accepting `IAuditContext` + `ILogger<AppDbContext>`. In `SaveChangesAsync`, calls `CreateAuditEntries()` wrapped in try-catch before `base.SaveChangesAsync()`. Added helper methods: `CreateAuditEntries()`, `GetEntityId()`, `SerializeModifiedProperties()`, `SerializeAddedProperties()`. Only audits entities extending the `Entity` base class (excludes OutboxMessage, ProcessedEvent, AuditLog).

### Api Layer

**Created:**
- `Services/HttpAuditContext.cs` — Implements `IAuditContext`. Reads `ClaimTypes.NameIdentifier` (fallback to `"sub"`) from `IHttpContextAccessor` for UserId. Reads `ICorrelationIdAccessor` for CorrelationId.

**Modified:**
- `DependencyInjection.cs` — Added `services.AddHttpContextAccessor()` and `services.AddScoped<IAuditContext, HttpAuditContext>()`.

### Migration
- `20260206182723_AddAuditLogs` — Creates `audit_logs` table with columns: id (uuid PK), user_id (uuid nullable), action (varchar 50), entity_type (varchar 100), entity_id (varchar 200), changes (jsonb nullable), correlation_id (varchar 64 nullable), created_at_utc (timestamp with time zone). Three indexes: idx_audit_logs_entity, idx_audit_logs_user_id, idx_audit_logs_created_at.

## Data / Schema / Migrations
- DB changes: New `audit_logs` table with 3 indexes.
- Migration strategy: Additive only — no existing tables modified.
- Backward compatibility: Fully backward compatible. No existing behavior changes.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet ef migrations add AddAuditLogs --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed

### orgs-api — New Files
- `Domain/Common/AuditLog.cs` — AuditLog entity
- `Application/Common/Interfaces/IAuditContext.cs` — Audit context interface
- `Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs` — EF configuration
- `Api/Services/HttpAuditContext.cs` — HTTP-based audit context implementation
- `Infrastructure/Migrations/20260206182723_AddAuditLogs.cs` — Migration
- `Infrastructure/Migrations/20260206182723_AddAuditLogs.Designer.cs` — Migration designer

### orgs-api — Modified Files
- `Infrastructure/Persistence/AppDbContext.cs` — Added DbSet, audit interception in SaveChangesAsync
- `Api/DependencyInjection.cs` — Registered HttpAuditContext + HttpContextAccessor
- `Infrastructure/Migrations/AppDbContextModelSnapshot.cs` — Updated snapshot

### Tests
- `IntegrationTests/Persistence/AuditLogTests.cs` — New: 4 integration tests

### Documentation
- `docs/tasks/0017-audit-logging.md` — Status updated to DONE
- `docs/backlog/epic-001-professional-saas-refinement.md` — Task 0017 status DONE, progress tracker updated

## Tests
### Integration (4 new tests)
- `SaveChangesAsync_WhenEntityCreated_ShouldCreateAuditLogWithCreatedAction` — Verifies Organization creation generates AuditLog with Action=Created, correct UserId, CorrelationId, and non-empty Changes JSON.
- `SaveChangesAsync_WhenEntityModified_ShouldCreateAuditLogWithUpdatedAction` — Verifies soft-delete generates AuditLog with Action=Updated and Changes containing "IsDeleted".
- `SaveChangesAsync_WhenNoAuditContext_ShouldStillCreateAuditLogWithNullUser` — Verifies parameterless constructor still creates audit entries with null UserId/CorrelationId.
- `SaveChangesAsync_ShouldNotCreateAuditLogForAuditLogEntity` — Verifies AuditLog entities are not recursively audited (only Entity subclasses are audited).

### How to run
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Verification
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 54 tests passed (21 unit + 5 architecture + 28 integration)

## Security
- No new AuthN/AuthZ impact. Audit context reads from existing JWT claims.
- Audit logs capture UserId from authenticated claims — no PII beyond what's already in the JWT.
- Sensitive properties (`PasswordHash`, `Token`) are excluded from Changes JSON via a denylist in `AppDbContext.SensitiveProperties`.
- `CorrelationId` is truncated to 64 characters to prevent column-length violations from rolling back business transactions.

## Follow-ups / Backlog
- [ ] Audit log viewing API endpoint (admin-only)
- [ ] Audit log retention policies (auto-delete entries older than N days)
- [ ] Audit log export to external systems (e.g., SIEM)
- [ ] Extend to ai-api service

## Checklist
- [x] Task scope matches `docs/tasks/0017-audit-logging.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
