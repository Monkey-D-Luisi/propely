# Walkthrough: cr-0042 — Remove Member PR #239 Review

## Task Reference
- Task: `docs/tasks/cr-0042-remove-member-pr239-review.md`
- PR: #239
- Branch: `feat/0038-remove-member`
- Date: `2026-02-10`

## Summary
Addressed 5 review comments on PR #239. Fixed a critical bug where error body parsing checked `.error` field but the middleware returns ProblemDetails with `.detail` field. Also cleaned up dialog state management, removed a duplicate test, and added focus trapping for accessibility consistency.

## Changes Made
1. **MUST_FIX**: Fixed error body parsing in `handleRemoveConfirm` and `handleRoleChange` — now checks `detail` field for DomainException codes
2. **SHOULD_FIX**: Moved `setMemberToRemove(null)` to `finally` block to reduce duplication
3. **SHOULD_FIX**: Removed redundant `Handle_AdminCannotRemoveOnlyOwner` test (duplicated `Handle_AdminRemovesOwner`)
4. **SHOULD_FIX**: Replaced global keydown listener with overlay-level focus trap in RemoveConfirmDialog (matching LeaveOrgButton/DeleteOrgSection pattern)

## Commands Run
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln   # 216 unit + 5 arch + 121 integration = 342 passed
cd apps/web && npm test                                    # 279 passed
cd apps/web && npm run build                               # success
```

## Validation Results
All checks pass.
