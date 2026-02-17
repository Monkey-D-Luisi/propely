# Audit Executive Summary: Epic 004 — Org & Member Management Completion

## Audit Metadata
- **Epic:** `docs/backlog/epic-004-org-management.md`
- **Date:** 2026-02-10
- **Auditor:** Agent
- **Status:** Complete
- **Tasks audited:** 3 (0036, 0037, 0038) + 3 code reviews (cr-0040, cr-0041, cr-0042)
- **Services affected:** orgs-api (backend), web (frontend)
- **Commits analyzed:** 9

> **Status values:**
> - `In Progress` — There are still `Not started` items in the Prioritized Action Plan.
> - `Complete` — All action plan items are `Done`. The agent will skip this audit when running `next audit action`.

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | 95/100 | Excellent — Clean Architecture fully compliant, proper CQRS |
| Security (Backend) | 70/100 | Needs attention — race condition on owner count, info leakage |
| Security (Frontend) | 88/100 | Good — React escaping, proper error handling, no XSS vectors |
| Code Quality | 93/100 | Excellent — consistent patterns, good naming, minor duplication |
| Test Coverage | 82/100 | Good — strong handler tests, missing DeleteOrgSection component tests |
| Documentation | 97/100 | Excellent — all tasks and reviews fully documented |
| **Overall** | **87/100** | **Good — address security findings before production** |

---

## Security Findings

### CRITICAL

#### F1. Race Condition: Concurrent Owner Operations Can Orphan an Organization
- **Severity:** CRITICAL
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommandHandler.cs:38-45`, `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/RemoveMember/RemoveMemberCommandHandler.cs:60-68`
- **Problem:** Both the leave and remove handlers use a check-then-act pattern on owner count without pessimistic locking. The owner count is read from an in-memory snapshot (`allMembers`), then the guard (`ownerCount <= 1`) is evaluated, then the membership is soft-deleted. Two concurrent requests modify **different rows** (each user's own membership, or different target memberships), so EF Core's optimistic concurrency on individual rows cannot detect the conflict. No `ConcurrencyToken` or `RowVersion` column exists on the `Membership` entity.
- **Impact:** If an org has exactly 2 owners and both simultaneously call `DELETE /orgs/{orgId}/members/me`, both pass the `ownerCount <= 1` check (seeing count=2), both soft-delete their membership, and the org ends up with zero owners — permanently unmanageable. The same applies to an owner removing another owner while that owner simultaneously leaves.
- **Recommendation:** Add a database-level guard. Options include: (a) a serializable transaction wrapping the read-check-write, (b) a `SELECT ... FOR UPDATE` on the membership rows for the org, (c) a database trigger or CHECK constraint that prevents the last owner from being deleted, or (d) a unique partial index / advisory lock pattern. Option (c) provides the strongest guarantee.

### HIGH

#### F2. Information Leakage: 404 vs 403 Reveals Org Membership
- **Severity:** HIGH
- **Files:** `LeaveOrganizationCommandHandler.cs:34-35`, `RemoveMemberCommandHandler.cs:41-42`, `DeleteOrganizationCommandHandler.cs:37-38`, `ExceptionHandlerMiddleware.cs:65,95-103`
- **Problem:** All three handlers throw `NotFoundException("Member not found.")` when the requesting user is not a member of the org, but throw `ForbiddenException` when they are a member but lack permission. The middleware maps these to HTTP 404 and 403 respectively. An authenticated attacker can probe arbitrary org IDs to determine whether they are a member by observing the status code difference.
- **Impact:** Org membership enumeration. An attacker can determine which orgs a victim belongs to by attempting operations with different org IDs and observing 404 vs 403 responses.
- **Recommendation:** Return 403 Forbidden consistently for all unauthorized access when the requester lacks permission, regardless of whether the cause is "not a member" or "insufficient role." Alternatively, always return 404 to hide the distinction (but this may confuse legitimate users).

#### F3. No Rate Limiting on Destructive DELETE Endpoints
- **Severity:** HIGH
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs:167-198`
- **Problem:** The three DELETE endpoints (leave, remove member, delete org) have no rate limiting middleware. An attacker with a valid JWT could spam these endpoints. While each call requires authentication and authorization, rapid repeated calls could cause database load and notification spam.
- **Impact:** Resource exhaustion. An org owner could rapidly mass-remove members or an authenticated user could probe many org IDs quickly (amplifying F2).
- **Recommendation:** Add rate limiting middleware targeting destructive endpoints. .NET 10's built-in `RateLimiter` middleware with a fixed-window or sliding-window policy per-user, per-endpoint would be appropriate. Consider a low limit (e.g., 10 DELETE requests per minute per user).

### MEDIUM

#### F4. DomainException Messages Exposed in API Response Detail Field
- **Severity:** MEDIUM
- **Files:** `ExceptionHandlerMiddleware.cs:65`
- **Problem:** The middleware places `exception.Message` directly into the ProblemDetails `Detail` field for all exception types, including `DomainException`. This means internal domain error codes like `CANNOT_REMOVE_SELF`, `CANNOT_REMOVE_LAST_OWNER` are visible to API consumers.
- **Recommendation:** This is acceptable for UX purposes (the frontend uses these codes to display specific error messages). However, document this as an intentional design choice and ensure no future `DomainException` messages contain sensitive internal details. Consider a whitelist of allowed domain codes that can be exposed.

### LOW

#### F5. Missing Idempotency on DELETE Operations
- **Severity:** LOW
- **Files:** All three command handlers
- **Problem:** Calling the same DELETE operation twice yields different results (200 then 404). Client retry logic or network retries could produce confusing error responses.
- **Recommendation:** Consider returning 200/204 on repeated soft-delete of already-deleted resources, or document that these operations are not idempotent.

---

## Architecture Compliance

### Adherence Score: 95/100

All three command handlers (`LeaveOrganizationCommandHandler`, `DeleteOrganizationCommandHandler`, `RemoveMemberCommandHandler`) demonstrate textbook Clean Architecture + CQRS implementation.

### Positive Observations
- **Domain purity:** Domain layer has zero framework dependencies. `Membership`, `Organization`, `Notification` are pure C# classes. `ISoftDeletable` interface is domain-only.
- **Inward dependency flow:** Application layer imports only `SaasTemplate.OrgsApi.Application.*` and `SaasTemplate.OrgsApi.Domain.*`. No Infrastructure or Api imports in any handler.
- **Interface segregation:** `IMembershipRepository`, `IOrganizationRepository`, `IInvitationRepository`, `INotificationRepository` each have focused contracts defined in the Application layer.
- **CQRS naming:** All commands follow `<Verb><Noun>Command` / `<Verb><Noun>CommandHandler` convention with `IRequestHandler<T>` from MediatR.
- **Unit of Work:** Single `SaveChangesAsync` call at the end of each handler ensures atomicity.
- **Controller thin-ness:** All three controller actions follow the same 5-line pattern: extract userId, null-check, create command, send via MediatR, return OK.

### Violations / Concerns
- **DialogOverlay component duplication:** The same modal overlay pattern (backdrop click, Escape key, Tab focus trapping, auto-focus) is implemented three times: `LeaveOrgButton.tsx`, `DeleteOrgSection.tsx`, `MembersManager.tsx:RemoveConfirmDialog`. This is a DRY concern, not an architecture violation. Noted in walkthrough `0038-remove-member.md` as a follow-up item.

---

## Code Quality

### Backend
**Strengths:**
- Consistent handler structure across all three features: validate → authorize → guard → mutate → notify → save.
- Proper exception hierarchy: `NotFoundException`, `ForbiddenException`, `DomainException` with clear semantic meaning.
- Defensive last-owner guard in `RemoveMemberCommandHandler:60-68` even though it's unreachable with current role hierarchy (documented in walkthrough).
- Self-removal check (`RemoveMemberCommandHandler:34-37`) placed before any DB queries for fail-fast behavior.
- Notification patterns correctly use `AddAsync` (singular) for single-recipient and `AddRangeAsync` for multi-recipient scenarios.

**Issues:**
- Minor inconsistency in error message specificity: `ForbiddenException("Admins can only remove members and viewers.")` in `RemoveMemberCommandHandler:52` is descriptive, while `DomainException("CANNOT_REMOVE_LAST_OWNER")` in `LeaveOrganizationCommandHandler:43` is code-only. Both approaches work, but a consistent style would be cleaner.

### Frontend
**Strengths:**
- Consistent hook pattern: `useLeaveOrg`, `useDeleteOrg`, `useRemoveMember` all wrap `apiFetch` with proper method and path.
- Error handling uses `isApiError()` type guard consistently across all components.
- Role-based UI visibility in `MembersTable.tsx:66-72` correctly mirrors backend authorization hierarchy.
- Proper state cleanup via `finally` blocks in all async handlers.
- i18n coverage is complete for both EN and ES across all three features.

**Issues:**
- `useLeaveOrg` has a different return signature (`Promise<{ok, reason?}>`) than `useDeleteOrg`/`useRemoveMember` (`Promise<void>`) due to client-side owner validation. Justified by design but breaks hook symmetry.
- Error body type assertion `(error.body as { detail?: string }).detail` is repeated in multiple places. A shared helper like `getDomainErrorCode(error)` would reduce duplication and prevent future `.error`/`.detail` confusion.

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (handlers) | 38 (10+11+17) | Race condition scenarios not testable at unit level |
| Integration (endpoints) | 121 (pre-existing, all pass) | No new integration tests added for epic 004 endpoints |
| Architecture | 5 (pre-existing, all pass) | None |
| Frontend (components) | 43 (11+14+8+10) | **Missing: DeleteOrgSection.test.tsx** |
| Frontend (hooks) | 11 (7+2+2) | None |

### Missing Tests
1. **DeleteOrgSection component tests** — `DeleteOrgSection.tsx` has zero test coverage. No `DeleteOrgSection.test.tsx` file exists. This component handles the dangerous org deletion with name-confirmation dialog and should be tested for: button visibility by role, dialog open/close, name confirmation validation, success/error toasts, redirect after deletion.
2. **Integration tests for new endpoints** — No integration tests were added for `DELETE /orgs/{orgId}/members/me`, `DELETE /orgs/{orgId}`, or `DELETE /orgs/{orgId}/members/{userId}`. The existing 121 integration tests pass but don't cover these new endpoints.
3. **MembersManager remove flow integration** — No test verifies the full remove-member flow (click Remove → dialog appears → confirm → success toast → members list refreshes).
4. **Edge cases** — No test for deleting an org with zero other members (cascade on empty list). No timestamp verification (`DeletedAtUtc`) in soft-delete assertions.

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DOD | Walkthrough | Issue |
|------|----------|-------------|-------|
| 0036 | OK (7/7 checked) | OK | Aligned |
| 0037 | OK (7/7 checked) | OK | Aligned |
| 0038 | OK (7/7 checked) | OK | Aligned |
| cr-0040 | OK (all resolved) | OK | Aligned |
| cr-0041 | OK (all resolved) | OK | Aligned |
| cr-0042 | OK (all resolved) | OK | Aligned |

### Other Documentation Issues
- None. Documentation is comprehensive. All decisions are documented with alternatives considered and consequences noted. Code review comments are systematically triaged with clear rationale for OUT_OF_SCOPE deferrals.

---

## Commit History

### Pattern Compliance
- Conventional commits: **Yes** — 100% compliance. All 9 commits use valid `feat|fix|docs(scope): message (#reference)` format.
- Branch naming: **Yes** — All 3 branches follow `feat/<task-number>-<description>` convention.
- Code review cycles: **Observed** — Every PR (#237, #238, #239) has a corresponding `fix(code-review)` commit.

### Observations
- Merge strategy is **Rebase and Merge** (linear history on main, commit tree hashes match between branch and main).
- No force pushes, amend operations, or unusual patterns detected in reflog.
- All commits dated 2026-02-10 (single-day execution of entire epic).
- Clean sequential workflow: `main → feat/0036 → main → feat/0037 → main → feat/0038`.

---

## What's Done Well

1. **Consistent handler structure** — All three handlers follow the exact same validate → authorize → guard → mutate → notify → save pattern. Reading one handler teaches you how all three work. (`LeaveOrganizationCommandHandler.cs`, `DeleteOrganizationCommandHandler.cs`, `RemoveMemberCommandHandler.cs`)
2. **Defensive last-owner guard** — `RemoveMemberCommandHandler:60-68` includes a last-owner check that is currently unreachable (only owners can remove owners, meaning count >= 2), but protects against future authorization changes. This is explicitly documented in the walkthrough.
3. **Critical bug caught in code review** — cr-0042 identified that error body parsing checked `.error` instead of `.detail` (ProblemDetails field), which would have caused all domain error code handling to silently fall through to generic error toasts. Also caught and fixed a pre-existing instance of the same bug in `handleRoleChange`.
4. **Thorough backend test coverage** — 38 unit tests across 3 handlers with comprehensive role-based permission matrix, negative paths, notification verification, and cancellation token propagation.
5. **Proper separation of Leave vs Remove** — Self-removal is prevented in RemoveMember with `CANNOT_REMOVE_SELF`, directing users to the dedicated Leave action with its own UX and messaging. Clean domain boundary.
6. **Complete i18n** — Both EN and ES translations added for all user-facing strings across all three features, including specific error messages for each domain exception.
7. **Systematic code review process** — All 3 PRs reviewed, all actionable comments addressed, clear MUST_FIX/SHOULD_FIX/OUT_OF_SCOPE categorization with documented rationale.

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | CRITICAL | Race condition on owner count | Concurrent leave/remove operations can bypass the last-owner guard because the owner count check-then-act is not atomic. Two owners leaving simultaneously would both pass the `ownerCount <= 1` check and orphan the org. Add database-level locking or a serializable transaction. | `LeaveOrganizationCommandHandler.cs:38-45`, `RemoveMemberCommandHandler.cs:60-68` | None | Done |
| 2 | P1 | HIGH | 404 vs 403 info leakage | Non-members receive 404 while members with insufficient role receive 403, allowing authenticated users to enumerate org membership. Normalize error responses to return 403 for all unauthorized access regardless of membership status. | `LeaveOrganizationCommandHandler.cs:34-35`, `RemoveMemberCommandHandler.cs:41-42`, `DeleteOrganizationCommandHandler.cs:37-38` | None | Done |
| 3 | P1 | HIGH | Add rate limiting to DELETE endpoints | No rate limiting on the three destructive DELETE endpoints. Add per-user, per-endpoint rate limiting using .NET's built-in `RateLimiter` middleware. | `OrgsController.cs:167-198` | None | Done |
| 4 | P1 | MEDIUM | Add DeleteOrgSection component tests | `DeleteOrgSection.tsx` has zero test coverage despite handling the dangerous org deletion flow with name-confirmation dialog. Add tests for: button visibility, dialog interaction, name confirmation, success/error toasts, redirect. | `apps/web/src/components/orgs/DeleteOrgSection.tsx` | None | Done |
| 5 | P2 | LOW | Extract shared DialogOverlay component | Same modal overlay pattern (backdrop click, Escape, Tab focus trap, auto-focus) duplicated in LeaveOrgButton, DeleteOrgSection, and MembersManager. Extract to `@/components/ui/dialog-overlay.tsx`. | `LeaveOrgButton.tsx`, `DeleteOrgSection.tsx`, `MembersManager.tsx` | None | Done |
| 6 | P2 | LOW | Add integration tests for new endpoints | No integration tests were added for the three new DELETE endpoints. Add tests covering auth, authorization, happy path, and error responses. | `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/` | None | Done |
| 7 | P3 | LOW | Extract domain error code helper | `(error.body as { detail?: string }).detail` type assertion is repeated across MembersManager.tsx. Extract a `getDomainErrorCode(error): string | undefined` helper to `lib/api.ts` to prevent future `.error`/`.detail` confusion. | `apps/web/src/lib/api.ts`, `apps/web/src/components/orgs/MembersManager.tsx` | None | Done |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build && npm test
```
