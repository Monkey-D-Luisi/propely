# Task: cr-0042 — Remove Member PR #239 Review

## Metadata
- Type: Code Review
- PR: #239
- Branch: `feat/0038-remove-member`
- Target: `main`

## Changed Files
16 files (see Copilot review summary for full list)

## Review Threads

### Source 1: Inline Review Comments (5)
1. **Gemini** (RemoveMemberCommandHandler.cs:45) — Performance: replace GetByOrgIdAsync with targeted queries
2. **Gemini** (MembersManager.tsx:153) — Move setMemberToRemove(null) to finally block
3. **Copilot** (RemoveMemberCommandHandlerTests.cs:165) — Duplicate test Handle_AdminCannotRemoveOnlyOwner
4. **Copilot** (MembersManager.tsx:142) — error.body.error vs error.body.detail (ProblemDetails mismatch)
5. **Copilot** (MembersManager.tsx:322) — Focus trap missing in RemoveConfirmDialog

### Source 2: General Reviews (2)
- Gemini: General overview, no additional issues
- Copilot: PR summary + file overview, no additional issues

### Source 3: Issue Comments (2)
- Codex: Usage limit notification (not actionable)
- Gemini: Summary preview (not actionable)

## Behavioral Parity Checks
- [x] Redirect parity checked — N/A, no auth redirects in this PR
- [x] Locale source correctness checked — N/A, only i18n strings added
- [x] API/UI contract parity checked — endpoint returns `{ ok: true }`, frontend doesn't parse response body on success. **BUG FOUND**: error body parsing checks `.error` but middleware returns ProblemDetails with `.detail`
- [x] Test parity checked — backend has happy + error paths, frontend has visibility + interaction tests

## Comment Resolution Plan

### MUST_FIX
- [x] Comment 4: Fix error body parsing in handleRemoveConfirm — check `detail` field instead of `error` field for DomainException codes (CANNOT_REMOVE_LAST_OWNER, CANNOT_REMOVE_SELF). Also fixed same bug in handleRoleChange (pre-existing, same pattern).

### SHOULD_FIX
- [x] Comment 2: Move setMemberToRemove(null) to finally block to reduce duplication
- [x] Comment 3: Remove redundant Handle_AdminCannotRemoveOnlyOwner test (duplicates Handle_AdminRemovesOwner)
- [x] Comment 5: Add focus trapping to RemoveConfirmDialog (consistent with LeaveOrgButton/DeleteOrgSection pattern)

### OUT_OF_SCOPE
- [x] Comment 1: Replace GetByOrgIdAsync with targeted queries — premature optimization, consistent with existing handlers (LeaveOrg, DeleteOrg, UpdateMemberRole). All use the same in-memory pattern for audit logging via EF Core ChangeTracker.
