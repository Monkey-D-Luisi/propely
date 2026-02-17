# Task: 0017 - Audit Logging Infrastructure

## Metadata
- ID: 0017
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0017-audit-logging.md`

## Goal
Create an audit logging system that captures who performed what action, when, and on which entity. Store audit logs in a dedicated table correlated with request correlation IDs.

## Context
A professional SaaS needs an audit trail for compliance, debugging, and security. Currently no mutations are logged. We need to capture create/update/delete operations with the actor, timestamp, and changes.

## Scope
### In scope
- Create `AuditLog` entity (Id, UserId, Action, EntityType, EntityId, Changes, CorrelationId, CreatedAtUtc)
- Create `IAuditLogger` interface in Application layer
- Implement `EfAuditLogger` in Infrastructure that writes to `audit_logs` table
- Intercept mutations via EF Core SaveChanges override or MediatR pipeline behavior
- Capture: user ID, action (Created/Updated/Deleted), entity type, entity ID, changed properties
- Create EF Core migration for `audit_logs` table
- Add correlation ID from middleware to audit entries

### Out of scope
- Audit log viewing UI (task 0025 notifications could include this later)
- Audit log export/download
- Retention policies

## Requirements
- R1: Every create, update, and delete operation creates an audit log entry
- R2: Audit log includes the user who performed the action
- R3: Audit log includes correlation ID for request tracing
- R4: Changes field captures old/new values for updates
- R5: Audit logging must not fail the main operation (fire and forget or best effort)

## Acceptance Criteria
- AC1: Creating an organization generates an audit log entry
- AC2: Updating a member's role generates an audit log with old/new role
- AC3: Audit logs have correct user ID and correlation ID
- AC4: `dotnet build` succeeds
- AC5: `dotnet test` passes
- AC6: Migration applies cleanly

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Audit logging must not impact request latency significantly.

## Implementation Steps
1. Create `AuditLog` entity in Domain
2. Create `IAuditLogger` interface in Application
3. Create `AuditLogConfiguration` in Infrastructure/Persistence
4. Override `SaveChangesAsync` in AppDbContext to capture changes
5. Or: create MediatR pipeline behavior for audit logging
6. Register in DI
7. Create EF migration
8. Test

## Files to Create / Modify
- `Domain/Common/AuditLog.cs` (create)
- `Application/Common/Interfaces/IAuditLogger.cs` (create)
- `Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs` (create)
- `Infrastructure/Persistence/AppDbContext.cs` (modify - SaveChanges override)
- `Infrastructure/DependencyInjection.cs` (modify - register)

## Testing Plan
- Integration tests: Verify audit log entries created after mutations

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
