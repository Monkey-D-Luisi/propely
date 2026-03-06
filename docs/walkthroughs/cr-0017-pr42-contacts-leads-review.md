# Walkthrough: cr-0017 — PR #42 Contacts & Leads Review

## Task Reference
- Task: `docs/tasks/cr-0017-pr42-contacts-leads-review.md`
- PR: [#42](https://github.com/Monkey-D-Luisi/propely/pull/42)
- Date: 2026-03-06

## Summary
Comprehensive code review of Epic P4 (Contacts & Leads), addressing 12 findings across correctness, testing, frontend i18n, dead code, and documentation accuracy.

---

## Changes Made

### 1. Fix PagedResult constructor argument order (Contacts + Leads handlers)
- **Files**: `ListContactsQueryHandler.cs`, `ListLeadsQueryHandler.cs`
- **What**: Both handlers passed `(items, pageNumber, totalPages, totalCount)` to `PagedResult<T>` constructor but the signature is `(items, totalCount, pageNumber, pageSize)`. Fixed to explicitly pass `result.TotalCount`, `result.PageNumber`, `request.PageSize`.

### 2. Fix EF.Functions.ILike on in-memory data (role filter path)
- **File**: `ContactReadRepository.cs`
- **What**: When `filter.Role` is set, results are materialized into memory then `ApplySorting` is called. If `search` is non-null, the existing `ApplySorting` uses `EF.Functions.ILike` which throws on LINQ-to-Objects. Fixed by adding an `inMemory` parameter to `ApplySorting`; in-memory sorting uses `string.Contains(..., OrdinalIgnoreCase)` instead.

### 3. Fix hardcoded es-ES locale in date formatting
- **Files**: `ContactDetail.tsx`, `ContactsTable.tsx`, `PropertyInterestsList.tsx`, `LeadDetailPanel.tsx`, `LeadListView.tsx`
- **What**: All `formatDate` functions hardcoded `'es-ES'`. Changed to accept locale as parameter, and components call `useLocale()` from `next-intl` to pass the active locale.

### 4. Fix convert button visibility (show only for Qualified leads)
- **Files**: `LeadListView.tsx`, `LeadDetailPanel.tsx`
- **What**: Changed condition from `status !== 'Converted' && status !== 'Lost'` to `status === 'Qualified'` to match domain invariant (`Lead.Convert` only allows Qualified leads).

### 5. Remove unused UpdateLeadRequest and UpdateLeadClientRequest DTOs
- **Files**: `LeadRequests.cs`, `UpdateLeadClientRequest.cs`
- **What**: No update endpoint or CQRS command exists in this PR for leads. Removed orphaned DTOs to avoid misleading consumers.

### 6. Rename CircuitBreakerFailureThreshold → CircuitBreakerMinimumThroughput
- **Files**: `ContactsApiClientOptions.cs`, `ServiceCollectionExtensions.cs`
- **What**: The option maps to `MinimumThroughput` (minimum requests before circuit evaluates). Renamed to match actual semantics.

### 7. Add specific error i18n keys
- **Files**: `apps/web/messages/en.json`, `apps/web/messages/es.json`, `apps/web/src/app/[locale]/contacts/page.tsx`, `apps/web/src/app/[locale]/leads/page.tsx`
- **What**: Added `form.createError`, `delete.error`, and `actions.convertError` translations. Updated toast calls to use specific keys.

### 8. Fix walkthrough 0035 inaccuracies
- **File**: `docs/walkthroughs/0035-contacts-api-sdk-client.md`
- **What**: Removed reference to non-existent `UpdateLeadAsync`. Corrected TenantDelegatingHandler description to `IHttpContextAccessor` (not `ITenantContext`).

### 9. Fix task doc 0035 inaccuracies
- **File**: `docs/tasks/0035-contacts-api-sdk-client.md`
- **What**: Updated DI extension signature to match actual `Action<ContactsApiClientOptions>` implementation.

### 10. Add command/query handler unit tests (CI coverage gate)
- **Files**: Multiple new test files in `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Application/`
- **What**: Added tests for: CreateContact, UpdateContact, DeleteContact, GetContactById, ListContacts, CreateLead, ChangeLeadStatus, AssignLead, DeleteLead, GetLeadById, ListLeads handlers. This brings combined coverage above 60%.

---

## Commands Run
```bash
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
cd apps/web && npm run build
cd apps/web && npx vitest run
```

## Process Deviations
- None

## Validation Results
- All backend tests pass (see CI)
- All frontend tests pass (see CI)
- CI coverage above 60% threshold (see CI)
