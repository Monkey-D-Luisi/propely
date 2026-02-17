# Code Review: cr-0025 - Notifications PR Review

## Metadata
- PR: #177
- Target branch: main
- CI status: All checks passing (Detect Changes, Orgs API Build & Test, Web Build & Test)
- Reviewers: Copilot, Gemini Code Assist

## Changed Files (40)
See PR #177 file list.

## Review Threads

### Source 1: Inline Review Comments (12)
| # | Reviewer | File | Line | Summary |
|---|----------|------|------|---------|
| 1 | Copilot | NotificationBell.test.tsx | 130 | Test strips `Z` from `createdAtUtc`, should keep it |
| 2 | Copilot | NotificationDropdown.tsx | 16 | `timeAgo` appends `Z`, may produce `ZZ` on API timestamps |
| 3 | Copilot | NotificationDropdown.tsx | 25 | `timeAgo` hardcoded English strings bypass i18n |
| 4 | Gemini | es.json | 241 | `errors.insufficientPermissions` nesting missing |
| 5 | Copilot | NotificationDropdown.tsx | 12 | `onClose` prop defined but unused |
| 6 | Gemini | NotificationDropdown.tsx | 33 | `onClose` prop not destructured (same as #5) |
| 7 | Copilot | NotificationDropdown.test.tsx | 16 | Test data strips `Z`, masks parsing issue |
| 8 | Gemini | NotificationDropdown.tsx | 26 | `timeAgo` hardcoded English (same as #3) |
| 9 | Gemini | notifications.ts | 53 | `setInterval` polling race condition |
| 10 | Gemini | notifications.ts | 76 | `markAsRead`/`markAllAsRead` lack error handling |
| 11 | Gemini | NotificationsController.cs | 82 | `GetUserId()` duplicated across controllers |
| 12 | Gemini | GetNotificationsQueryHandler.cs | 25 | Sequential DB queries should use `Task.WhenAll` |

### Source 2: Reviews (2)
- Gemini: General summary with suggestions (COMMENTED)
- Copilot: General summary, also caught es.json `errors` structure (suppressed) (COMMENTED)

### Source 3: Issue Comments (2)
- ChatGPT Codex: Usage limit message (not actionable)
- Gemini: Summary (not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] Fix #4: Restore `errors.insufficientPermissions` nesting in es.json (runtime translation failure)
- [x] Fix #2: Remove `+ 'Z'` concatenation in `timeAgo` — parse `createdAtUtc` as-is
- [x] Fix #3/#8: Replace hardcoded English `timeAgo` strings with i18n via `useTranslations`

### SHOULD_FIX
- [x] Fix #5/#6: Remove unused `onClose` prop from `NotificationDropdown` (and callers/tests)
- [x] Fix #10: Add try/catch with error toast to `markAsRead`/`markAllAsRead`
- [x] Fix #12: Parallelize DB queries in `GetNotificationsQueryHandler` with `Task.WhenAll`
- [x] Fix #9: Add fetch-in-progress guard to polling hook

### SUGGESTION
- [x] Fix #1/#7: Update test data to keep `Z` suffix in `createdAtUtc`

### OUT_OF_SCOPE
- [ ] #11: Extract `GetUserId()` to shared helper — valid refactoring but unrelated to notifications scope. Will address in a dedicated cleanup task.

## Status: RESOLVED
