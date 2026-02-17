# Epic 003: Multi-tenancy & Data Isolation

## Overview

Implement proper multi-tenant data isolation by adding TenantId (OrgId) to core entities, configuring EF Core global query filters for automatic tenant scoping, and writing cross-tenant leakage tests. Also add organization name uniqueness constraint.

## Success Criteria

- All tenant-scoped entities have a TenantId (OrgId) column
- EF Core global query filters automatically scope queries to the current tenant
- Cross-tenant data access is impossible without bypassing filters
- Integration tests prove no data leaks between tenants
- Organization names are unique (case-insensitive)
- Migrations are backward-compatible (additive only)

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Tenant identification | OrgId from JWT claims | Already available in auth context, no extra lookup |
| Query filtering | EF Core global query filters | Same pattern used for soft delete, consistent approach |
| Uniqueness | DB unique index + application validation | Defense in depth |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0032 - Add TenantId (OrgId) to core entities
- **Status:** DONE
- **GitHub Issue:** #184
- **Dependencies:** None
- **File:** `docs/tasks/0032-tenant-id-entities.md`
- **Scope:** Add TenantId column to WorkItem and other tenant-scoped entities in both services. Create EF migration. Backfill existing data. Add TenantId to domain entity constructors.
- **Old Issue:** #16

### Task 0033 - EF Core global query filters for tenant isolation
- **Status:** DONE
- **GitHub Issue:** #185
- **Dependencies:** 0032
- **File:** `docs/tasks/0033-tenant-query-filters.md`
- **Scope:** Configure HasQueryFilter for TenantId on all scoped entities (alongside existing soft delete filter). Create ITenantAccessor to resolve current tenant from HTTP context. Apply in both DbContexts.
- **Old Issue:** #17

### Task 0034 - Cross-tenant data leakage tests
- **Status:** DONE
- **GitHub Issue:** #186
- **Dependencies:** 0033
- **File:** `docs/tasks/0034-tenant-leakage-tests.md`
- **Scope:** Write integration tests that create data under tenant A and verify tenant B cannot access it. Test all CRUD endpoints. Test query filters cannot be bypassed via direct queries.
- **Old Issue:** #18

### Task 0035 - Org name uniqueness constraint
- **Status:** DONE
- **GitHub Issue:** #187
- **Dependencies:** None
- **File:** `docs/tasks/0035-org-name-uniqueness.md`
- **Scope:** Add unique index on Organization.Name (case-insensitive). Add application-level validation in CreateOrganizationCommand. Return proper error message. Add i18n strings.
- **Old Issue:** Codex #103

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Multi-tenancy | 0032-0035 | 4 | 0 |
| **Total** | **4** | **4** | **0** |

## Dependency Graph

```
0032 (TenantId) ──► 0033 (Query filters) ──► 0034 (Leakage tests)
0035 (Org uniqueness) ── standalone
```
