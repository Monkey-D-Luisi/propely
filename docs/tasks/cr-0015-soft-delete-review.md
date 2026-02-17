# CR-0015: Soft Delete Pattern Code Review

## PR Metadata

- **PR**: [#154 - feat(orgs,ai): add soft delete pattern (#0016)](https://github.com/Monkey-D-Luisi/saas-template/pull/154)
- **Branch**: `feat/0016-soft-delete` -> `main`
- **CI Status**: All checks passing (Detect Changes, AI API Build & Test, Orgs API Build & Test)

## Changed Files (23)

- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0016-soft-delete.md`
- `docs/walkthroughs/0016-soft-delete.md`
- `services/ai-api/src/SaasTemplate.AiApi.Domain/Common/ISoftDeletable.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/20260206140027_AddSoftDelete.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/20260206140027_AddSoftDelete.Designer.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Common/ISoftDeletable.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Organizations/Invitation.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Organizations/Membership.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Organizations/Organization.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260206135247_AddSoftDelete.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260206135247_AddSoftDelete.Designer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/InvitationConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/MembershipConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Domain/Organizations/InvitationSoftDeleteTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Domain/Organizations/MembershipSoftDeleteTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Domain/Organizations/OrganizationSoftDeleteTests.cs`

## Review Threads

### Source 1: Inline Review Comments (5)

| # | Reviewer | File | Summary |
|---|----------|------|---------|
| 1 | chatgpt-codex-connector | AI API Migration `AddSoftDelete.cs` | Backfill `is_deleted=true` for existing rows where `status='Deleted'` |
| 2 | gemini-code-assist | `Organization.cs:30-31` | `SoftDelete()` should also update `UpdatedAtUtc` |
| 3 | gemini-code-assist | `WorkItem.cs:127-128` | Use chained assignment for `DeletedAtUtc`/`UpdatedAtUtc` |
| 4 | Copilot | `MembershipConfiguration.cs:45-47` | Unique index on `(user_id, org_id)` blocks re-creating membership after soft delete |
| 5 | Copilot | `WorkItemConfiguration.cs:62-66` | Read model `work_items_read` has no soft-delete filter |

### Source 2: General Reviews (3)

- chatgpt-codex-connector: Codex review header (no additional body issues beyond inline comment #1)
- gemini-code-assist: Positive summary, two suggestions delivered as inline comments #2 and #3
- Copilot: PR overview, two issues delivered as inline comments #4 and #5

### Source 3: Issue Comments (1)

- gemini-code-assist: Summary/changelog (no actionable feedback)

## Comment Resolution Plan

### MUST_FIX

- [x] **#2** `Organization.SoftDelete()` must update `UpdatedAtUtc` for consistency with `WorkItem.Delete()`
- [x] **#4** Add `HasFilter("is_deleted = FALSE")` to memberships unique index `(user_id, org_id)` + new migration

### SHOULD_FIX

- [x] **#1** Backfill AI API migration: set `is_deleted=true`, `deleted_at_utc=updated_at_utc` for rows where `status='Deleted'`

### SUGGESTION

- [x] **#3** Use chained assignment `UpdatedAtUtc = DeletedAtUtc = DateTime.UtcNow` in `WorkItem.Delete()`

### OUT_OF_SCOPE

- [ ] **#5** Read model `work_items_read` has no soft-delete filter — pre-existing behavior not introduced by this PR. The read model never excluded `Status=Deleted` items. A simple `HasQueryFilter(x => x.Status != "Deleted")` on `WorkItemReadConfiguration` could address this without schema changes, but it's a separate concern from the write-model soft-delete pattern. Tracked for follow-up.
