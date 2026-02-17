# Walkthrough: audit-0009-race-condition-owner-count

## Task Reference
- Task: `docs/tasks/audit-0009-race-condition-owner-count.md`
- Walkthrough: `docs/walkthroughs/audit-0009-race-condition-owner-count.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Added pessimistic locking via `SELECT ... FOR UPDATE` and explicit transaction wrapping to the Leave Organization and Remove Member command handlers. This prevents concurrent requests from reading stale owner counts and bypassing the last-owner guard.

## Context
- Background: The `LeaveOrganizationCommandHandler` and `RemoveMemberCommandHandler` both use a check-then-act pattern: read all members, count owners, validate the guard (`ownerCount <= 1`), then soft-delete. Without locking, concurrent requests each read the same stale snapshot.
- Problem statement: If an org has exactly 2 owners and both simultaneously leave (or one leaves while another is removed), both pass the guard (seeing count=2) and both soft-delete, leaving the org with zero owners — permanently unmanageable.
- Constraints: Must use PostgreSQL-compatible locking. EF Core's global query filters wrap raw SQL in subqueries, which is incompatible with `FOR UPDATE`.

## Decisions & Trade-offs
- **Decision: `SELECT ... FOR UPDATE` + explicit transaction vs alternatives**
  - Options considered: (1) Pessimistic row locking via `FOR UPDATE`, (2) Database trigger/CHECK constraint, (3) Advisory locks, (4) Serializable transaction isolation
  - Why this choice: `FOR UPDATE` is the simplest approach that integrates cleanly with the existing EF Core + PostgreSQL stack. It locks the membership rows for the org during the transaction, serializing concurrent operations. Database triggers (option 2) would provide the strongest guarantee but add hidden behavior outside the application layer. Advisory locks (option 3) are heavier to manage. Serializable isolation (option 4) has broader locking scope than needed.
  - Consequences: Slight increase in lock contention on org membership rows during leave/remove operations. Acceptable for the expected low frequency of these operations.

- **Decision: `IgnoreQueryFilters()` on raw SQL**
  - Why: EF Core's global query filter (`HasQueryFilter(m => !m.IsDeleted)`) causes the raw SQL to be wrapped in a subquery (`SELECT * FROM (raw_sql) AS t WHERE is_deleted = FALSE`). PostgreSQL does not allow `FOR UPDATE` inside a subquery. Using `IgnoreQueryFilters()` prevents the wrapping, and we add the `is_deleted = FALSE` filter directly in the raw SQL.

- **Decision: Self-removal check outside transaction in RemoveMember**
  - Why: The `request.TargetUserId == request.RequestingUserId` check requires no database access, so there's no benefit to holding a transaction open for it. Fail-fast before acquiring any locks.

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IUnitOfWork.cs` — Added `ExecuteInTransactionAsync` method signature
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/UnitOfWork.cs` — Implemented `ExecuteInTransactionAsync` with `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IMembershipRepository.cs` — Added `GetByOrgIdForUpdateAsync` method signature
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs` — Implemented `GetByOrgIdForUpdateAsync` using `FromSqlRaw` with `FOR UPDATE` and `IgnoreQueryFilters()`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommandHandler.cs` — Wrapped handler body in `ExecuteInTransactionAsync`, changed to `GetByOrgIdForUpdateAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/RemoveMember/RemoveMemberCommandHandler.cs` — Same: wrapped in transaction, changed to locked read
- `services/orgs-api/tests/.../LeaveOrganizationCommandHandlerTests.cs` — Updated mocks for transaction + locked read, added `Handle_ShouldExecuteInsideTransaction` test
- `services/orgs-api/tests/.../RemoveMemberCommandHandlerTests.cs` — Same updates as LeaveOrganization tests

## Tests
### Unit
- All 218 existing unit tests pass
- Added `Handle_ShouldExecuteInsideTransaction` to both `LeaveOrganizationCommandHandlerTests` and `RemoveMemberCommandHandlerTests`
- Updated `Handle_ShouldPassCancellationTokenToAllDependencies` in both test classes to verify `ExecuteInTransactionAsync` receives the token
- All `GetByOrgIdAsync` mocks updated to `GetByOrgIdForUpdateAsync`

## Checklist
- [x] Task scope matches `docs/tasks/audit-0009-race-condition-owner-count.md`
- [x] Tests updated and passing
- [x] No secrets committed
