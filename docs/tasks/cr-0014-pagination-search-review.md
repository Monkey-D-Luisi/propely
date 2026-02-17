# Task: cr-0014-pagination-search-review

## Metadata
- ID: cr-0014
- Type: CodeReview
- Status: DONE
- Owner: Agent
- Created: 2026-02-06
- PR: #153 (`feat(orgs): add pagination and search to list endpoints (#0015)`)
- Target branch: main
- CI: All passing (Detect Changes ✓, Orgs API ✓, Web ✓, AI API skipped)

## Changed Files
### Backend (orgs-api)
- `Application/Organizations/Queries/GetMyOrgs/GetMyOrgsQuery.cs`
- `Application/Organizations/Queries/GetMyOrgs/GetMyOrgsQueryHandler.cs`
- `Application/Organizations/Queries/GetMembers/GetMembersQuery.cs`
- `Application/Organizations/Queries/GetMembers/GetMembersQueryHandler.cs`
- `Application/Organizations/Interfaces/IMembershipRepository.cs`
- `Infrastructure/Persistence/Repositories/MembershipRepository.cs`
- `Api/Controllers/OrgsController.cs`

### Frontend (apps/web)
- `src/lib/schemas.ts`
- `src/hooks/orgs.ts`
- `src/components/ui/pagination.tsx` (new)
- `src/components/orgs/MembersManager.tsx`
- `src/components/orgs/MembersTable.tsx`
- `src/app/[locale]/orgs/mine/page.tsx`
- `messages/en.json`
- `messages/es.json`

### Test files
- `src/hooks/__tests__/orgs.test.ts`
- `src/lib/__tests__/schemas.test.ts`
- `src/components/orgs/__tests__/MembersManager.test.tsx`
- `src/components/orgs/__tests__/MembersTable.test.tsx`
- `src/components/orgs/__tests__/LeaveOrgButton.test.tsx`

### CI (unrelated)
- `.github/workflows/claude-code-review.yml.disabled`
- `.github/workflows/claude.yml.disabled`

## Review Threads

### Source 1: Inline Review Comments (9 total)

1. **Codex #2774126914** (MembersManager.tsx:50) — P2: `currentMember` becomes null when user paginates to a page where they're not listed, losing role/permissions display
2. **Gemini #2774135750** (OrgsController.cs:84) — SECURITY HIGH: IDOR vulnerability — `GetMembers` endpoint doesn't verify requesting user is a member of the org
3. **Gemini #2774135755** (GetMembersQueryHandler.cs:23) — SECURITY HIGH: No authorization check in the query handler (same issue as #2)
4. **Gemini #2774135757** (hooks/orgs.ts:91) — Medium: Use object destructuring for pagination state in `useMyOrgs`
5. **Gemini #2774135768** (hooks/orgs.ts:134) — Medium: Same destructuring suggestion for `useMembers`
6. **Gemini #2774135770** (MembershipRepository.cs:81) — Medium: `ToLower()` prevents index usage, suggest `EF.Functions.ILike()`
7. **Copilot #2774153971** (IMembershipRepository.cs:15) — Repository returns Application DTOs (`OrgWithRole`, `MemberDto`), violates Clean Architecture layer separation
8. **Copilot #2774153989** (MembershipRepository.cs:82) — `ToLower()` prevents index usage (duplicate of #6)
9. **Copilot #2774154002** (MembershipRepository.cs:98) — Missing integration tests for paginated repository methods

### Source 2: General Reviews (3 total)
- Codex: Summary review — references inline comment #1, no additional actionable items
- Gemini: Summary — references IDOR vulnerability + minor suggestions, no additional items
- Copilot: Detailed PR overview with file-by-file summary, no additional items beyond inlines

### Source 3: Issue Comments (1 total)
- Gemini: Detailed summary/changelog of all changes — no actionable feedback

## Comment Resolution Plan

### MUST_FIX
- [x] **IDOR on GetMembers** (comments #2, #3): Add `RequestingUserId` to `GetMembersQuery`, verify membership in handler, return 403 if not a member. Update controller to pass authenticated userId.

### SHOULD_FIX
- [x] **currentMember pagination regression** (comment #1): Only update `currentMember` when user is found on current page; preserve previous value when not found.
- [x] **ToLower() → EF.Functions.ILike()** (comments #6, #8): Replace `ToLower().Contains()` with `EF.Functions.ILike()` for idiomatic PostgreSQL case-insensitive search.

### SUGGESTION
- [x] **Destructuring in hooks** (comments #4, #5): Apply object destructuring with rest spread for pagination state in both `useMyOrgs` and `useMembers`.

### OUT_OF_SCOPE
- [ ] **Repository returns Application DTOs** (comment #7): Pragmatic trade-off to eliminate N+1 queries. Creating domain-level projection types would add complexity with no runtime benefit. Respond with rationale.
- [ ] **Missing integration tests** (comment #9): Integration tests require database infrastructure. The project has unit + architecture tests. Defer to a future task.
