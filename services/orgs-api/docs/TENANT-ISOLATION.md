# Tenant Isolation in orgs-api

## Overview

The orgs-api intentionally does **not** use the `OrgContextMiddleware` + `ITenantAccessor` + global query filter pattern that other services (properties-api, contacts-api, appointments-api, publishing-api) use. This is by design, not a missing feature.

## Why orgs-api is different

orgs-api is the **authentication and organization management** service. It is responsible for:

- User authentication (login, token refresh, OAuth)
- Organization CRUD (create, update, delete orgs)
- Membership management (invite, remove, change roles)
- Permission evaluation (role defaults + overrides)
- Billing and subscription management (planned)

Because orgs-api **manages organizations themselves**, it must operate **across organizational boundaries**. A single authenticated user may belong to multiple organizations, and operations like "list my organizations" or "evaluate permissions for org X" require cross-org access by nature.

## How tenant scoping works in orgs-api

Instead of a global query filter that silently scopes all queries to a single tenant, orgs-api uses **explicit query-level scoping**:

- `GetOrgsForUser(userId)` -- returns only orgs where the user has an active membership
- `GetMembership(orgId, userId)` -- verifies the user is a member of the specific org before returning data
- `HasPermissionAsync(userId, orgId, permission)` -- checks permission within a specific org context

This approach ensures that:
1. Users can only see organizations they belong to
2. Organization-specific operations always verify membership first
3. Cross-org operations (like listing all user's orgs) work correctly

## How other services handle tenant isolation

All other backend services use a three-layer pattern:

1. **`OrgContextMiddleware`** -- extracts the `X-Org-Id` header (set by the frontend or SDK clients) and stores it in `ITenantAccessor`
2. **`ITenantAccessor`** -- provides the current org ID to the rest of the request pipeline
3. **EF Core global query filters** -- automatically append `.Where(e => e.OrgId == currentOrgId)` to all queries, preventing cross-tenant data leakage

This pattern is appropriate for those services because they always operate within the context of a single organization.

## Summary

| Aspect | orgs-api | Other services |
|--------|----------|---------------|
| Tenant scoping | Explicit (query-level) | Implicit (global query filters) |
| Cross-org access | Yes (by design) | No (by design) |
| OrgContextMiddleware | Not used | Required |
| ITenantAccessor | Not used | Required |
| X-Org-Id header | Not required | Required |
