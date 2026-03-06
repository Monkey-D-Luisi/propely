# Code Review: pr-42 — Contacts & Leads Epic (P4)

## Metadata
- **PR**: [#42 — feat(P4): Contacts & Leads — domain, API, SDK, frontend](https://github.com/Monkey-D-Luisi/propely/pull/42)
- **Branch**: `feat/p4-contacts-leads` → `main`
- **CI Status**: FAILING — `Contacts API - Build & Test` (coverage 49.5% < 60% threshold)
- **Review Date**: 2026-03-06
- **Reviewer**: Agent (cr-0017)

## Changed Files Summary
141 files changed across:
- `services/contacts-api/` — domain, CQRS, EF Core, SDK client, tests
- `apps/web/` — contacts/leads pages, components, hooks, i18n
- `docs/tasks/0031–0037`, `docs/walkthroughs/0031–0037`

---

## Section 1: Agent Review Findings

### Finding 1 — MUST_FIX — PagedResult constructor argument order (contacts)
- **File**: `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/ListContacts/ListContactsQueryHandler.cs:35-39`
- **Category**: Code Quality / Correctness
- **Description**: `PagedResult<T>` constructor is `(items, totalCount, pageNumber, pageSize)` but the handler passes `(items, pageNumber, totalPages, totalCount)`. This corrupts pagination metadata: `TotalCount` receives the page number, `PageNumber` receives total pages.
- **Fix**: Change to `new PagedResult<ContactListItemDto>(mappedItems, result.TotalCount, result.PageNumber, request.PageSize)`.

### Finding 2 — MUST_FIX — PagedResult constructor argument order (leads)
- **File**: `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/ListLeads/ListLeadsQueryHandler.cs:37-41`
- **Category**: Code Quality / Correctness
- **Description**: Same bug as Finding 1.
- **Fix**: Change to `new PagedResult<LeadListItemDto>(mappedItems, result.TotalCount, result.PageNumber, request.PageSize)`.

### Finding 3 — MUST_FIX — EF.Functions.ILike on in-memory LINQ
- **File**: `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactReadRepository.cs:71`
- **Category**: Correctness / Runtime Error
- **Description**: When `filter.Role` is set, results are materialized and `ApplySorting` is called with `AsQueryable()`. If `search` is non-null, `ApplySorting` uses `EF.Functions.ILike`, which is provider-only and throws on LINQ-to-Objects.
- **Fix**: When in the role-filter path, sort in-memory without `EF.Functions.ILike`. Use `string.Contains(…, StringComparison.OrdinalIgnoreCase)` for in-memory search-relevance sorting.

### Finding 4 — MUST_FIX — CI coverage 49.5% below 60% threshold
- **Category**: Testing
- **Description**: `Contacts API - Build & Test` fails because combined test coverage is 49.5%. Many command/query handlers in Application layer have 0% coverage. No controller tests exist.
- **Fix**: Add unit tests for command/query handlers to bring Application coverage up and combined coverage above 60%.

### Finding 5 — SHOULD_FIX — Hardcoded 'es-ES' locale in date formatting
- **Files**: `ContactDetail.tsx:19`, `ContactsTable.tsx:19`, `PropertyInterestsList.tsx:15`, `LeadDetailPanel.tsx:19`, `LeadListView.tsx:21`
- **Category**: Frontend / i18n
- **Description**: All `formatDate` functions hardcode `'es-ES'` locale, breaking internationalization for non-Spanish users.
- **Fix**: Use `useLocale()` from `next-intl` and pass locale to `Intl.DateTimeFormat`.

### Finding 6 — SHOULD_FIX — Convert button shown for all non-Converted/non-Lost leads
- **Files**: `LeadListView.tsx:81`, `LeadDetailPanel.tsx:136`
- **Category**: Frontend / UX
- **Description**: Condition `status !== 'Converted' && status !== 'Lost'` shows Convert for New/Contacted leads too, but `Lead.Convert` domain method throws if status is not Qualified. Users are offered an action that always fails.
- **Fix**: Change condition to `status === 'Qualified'`.

### Finding 7 — SHOULD_FIX — Unused UpdateLeadRequest DTO in API
- **File**: `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/LeadRequests.cs:19-26`
- **Category**: Code Quality / Dead Code
- **Description**: `UpdateLeadRequest` has no corresponding endpoint or CQRS command in this PR.
- **Fix**: Remove `UpdateLeadRequest` from `LeadRequests.cs`. Add to future backlog when update endpoint is implemented.

### Finding 8 — SHOULD_FIX — Unused UpdateLeadClientRequest in SDK
- **File**: `services/contacts-api/src/Propely.ContactsApi.Client/Models/UpdateLeadClientRequest.cs`
- **Category**: Code Quality / Dead Code
- **Description**: `UpdateLeadClientRequest` has no corresponding method in `ILeadsApiClient`.
- **Fix**: Remove the file. Add back when SDK exposes UpdateLeadAsync.

### Finding 9 — SHOULD_FIX — Misleading option name CircuitBreakerFailureThreshold
- **Files**: `ContactsApiClientOptions.cs:29`, `ServiceCollectionExtensions.cs:83`
- **Category**: Code Quality / Naming
- **Description**: The option is named `CircuitBreakerFailureThreshold` but maps to `MinimumThroughput` (min requests needed before circuit can evaluate failure ratio). The XML comment is correct but the property name is misleading.
- **Fix**: Rename to `CircuitBreakerMinimumThroughput`.

### Finding 10 — SUGGESTION — Generic error messages for create/delete/convert
- **Files**: `apps/web/src/app/[locale]/contacts/page.tsx:65,80`; leads page convert handler
- **Category**: Frontend / UX
- **Description**: `t('loadError')` is used for all error toasts including create and delete failures.
- **Fix**: Add `form.createError`, `delete.error`, and `actions.convertError` i18n keys with specific messages.

### Finding 11 — SHOULD_FIX — Walkthrough 0035 inaccurate (UpdateLeadAsync, TenantDelegatingHandler)
- **File**: `docs/walkthroughs/0035-contacts-api-sdk-client.md:34,36`
- **Category**: Documentation
- **Description**: Claims `ILeadsApiClient` includes `UpdateLeadAsync` (it doesn't) and that `TenantDelegatingHandler` reads from `ITenantContext` (it reads from `IHttpContextAccessor`).
- **Fix**: Update walkthrough to reflect actual implementation.

### Finding 12 — SHOULD_FIX — Task doc 0035 inaccurate (AddContactsApiClient signature)
- **File**: `docs/tasks/0035-contacts-api-sdk-client.md`
- **Category**: Documentation
- **Description**: Requirements spec `AddContactsApiClient(this IServiceCollection, Uri baseUrl)` but implementation uses `Action<ContactsApiClientOptions>`.
- **Fix**: Update task doc to reflect final implemented API.

---

## Section 2: Review Comment Threads

### Source 1: Inline Review Comments (12 total)

| # | Reviewer | File | Issue | Classification |
|---|----------|------|-------|----------------|
| 1 | chatgpt-codex | ListContactsQueryHandler.cs:39 | PagedResult arg order | MUST_FIX → see Finding 1 |
| 2 | chatgpt-codex | ListLeadsQueryHandler.cs:41 | PagedResult arg order | MUST_FIX → see Finding 2 |
| 3 | chatgpt-codex | ContactReadRepository.cs:71 | EF.Functions.ILike on in-memory | MUST_FIX → see Finding 3 |
| 4 | chatgpt-codex | LeadListView.tsx:81 | Convert for non-Qualified | SHOULD_FIX → see Finding 6 |
| 5 | gemini-code-assist | ContactDetail.tsx:19 | Hardcoded es-ES locale | SHOULD_FIX → see Finding 5 |
| 6 | gemini-code-assist | contacts/page.tsx:66 | Generic error messages | SUGGESTION → see Finding 10 |
| 7 | Copilot | UpdateLeadClientRequest.cs:25 | Unused SDK model | SHOULD_FIX → see Finding 8 |
| 8 | Copilot | 0035 walkthrough:38 | UpdateLeadAsync/TenantContext inaccuracy | SHOULD_FIX → see Finding 11 |
| 9 | Copilot | 0035 task doc:44 | Signature mismatch in task doc | SHOULD_FIX → see Finding 12 |
| 10 | Copilot | ContactReadRepository.cs:67 | In-memory materialization perf | SHOULD_FIX → see Finding 3 |
| 11 | Copilot | ServiceCollectionExtensions.cs:85 | CircuitBreakerFailureThreshold semantics | SHOULD_FIX → see Finding 9 |
| 12 | Copilot | LeadRequests.cs:26 | Unused UpdateLeadRequest | SHOULD_FIX → see Finding 7 |

### Source 2: General Reviews (3 total)
- chatgpt-codex: Automated review, findings captured above.
- gemini-code-assist: High severity = locale; Medium = error messages. Both captured.
- Copilot: 6 inline comments, all captured above.

### Source 3: Issue Comments (1 total)
- gemini-code-assist: PR summary comment, no additional issues beyond inline.

---

## Resolution Plan

### MUST_FIX (blocking merge)
- [x] Finding 1: Fix PagedResult arg order in ListContactsQueryHandler
- [x] Finding 2: Fix PagedResult arg order in ListLeadsQueryHandler
- [x] Finding 3: Fix EF.Functions.ILike called on in-memory data (role filter path)
- [x] Finding 4: Add tests to reach 60% coverage threshold (CI gate)

### SHOULD_FIX
- [x] Finding 5: Fix hardcoded es-ES locale in 5 frontend components
- [x] Finding 6: Fix convert button condition in LeadListView + LeadDetailPanel
- [x] Finding 7: Remove unused UpdateLeadRequest from LeadRequests.cs
- [x] Finding 8: Remove unused UpdateLeadClientRequest.cs from SDK
- [x] Finding 9: Rename CircuitBreakerFailureThreshold → CircuitBreakerMinimumThroughput
- [x] Finding 11: Update walkthrough 0035 for accuracy
- [x] Finding 12: Update task doc 0035 for accuracy

### SUGGESTION
- [x] Finding 10: Add specific error i18n keys (createError, deleteError, convertError)
