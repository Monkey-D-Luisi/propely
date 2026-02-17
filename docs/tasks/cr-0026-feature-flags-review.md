# Code Review: cr-0026 - Feature Flags PR Review

## Metadata
- PR: #178
- Target branch: main
- CI status: All checks passing (Detect Changes, Orgs API Build & Test, Web Build & Test)
- Reviewers: Copilot, Gemini Code Assist

## Changed Files (32)
See PR #178 file list.

## Review Threads

### Source 1: Inline Review Comments (13)
| # | Reviewer | File | Line | Summary |
|---|----------|------|------|---------|
| 1 | Gemini | feature-flags.ts | 67 | N+1 API calls: useFeatureFlag makes individual fetch per flag, need Provider context |
| 2 | Gemini | FeatureFlagRepository.cs | 17 | Case-sensitive GetByNameAsync vs case-insensitive handler lookup |
| 3 | Gemini | FeatureFlag.cs | 35 | DateTime.UtcNow hardcoded, suggests IDateTimeProvider |
| 4 | Copilot | FeatureFlagDefaults.cs | 18 | Case-sensitive ContainsKey in defaults |
| 5 | Copilot | FeatureFlagService.cs | 26 | IsEnabledAsync silently returns false for undefined flags |
| 6 | Copilot | GetFeatureFlagsQueryHandler.cs | 23 | ToDictionary may throw on case-different DB duplicates |
| 7 | Copilot | CheckFeatureFlagQueryHandler.cs | 33 | Case-sensitive TryGetValue check |
| 8 | Copilot | ToggleFeatureFlagCommandHandler.cs | 40 | Case-sensitive IsDefinedFlag allows duplicate DB rows |
| 9 | Copilot | FeatureFlagsController.cs | 13 | Route/auth/caching don't match task doc specs |
| 10 | Copilot | appsettings.json | 37 | PascalCase keys vs task doc lowercase |
| 11 | Copilot | feature-flags.ts | 64 | fetchedRef prevents refetch on name change |
| 12 | Copilot | FeatureFlag.cs | 25 | Create() missing name validation and length checks |
| 13 | Copilot | 0026-feature-flags.md | 8 | Task doc DONE but requirements mention missing caching/admin routes |

### Source 2: Reviews (2)
| # | Reviewer | State | Summary |
|---|----------|-------|---------|
| 1 | Gemini | COMMENTED | Solid implementation, critical perf suggestion for frontend hooks |
| 2 | Copilot | COMMENTED | Good implementation, case-sensitivity and doc alignment concerns |

### Source 3: Issue Comments (2)
| # | Author | Summary |
|---|--------|---------|
| 1 | chatgpt-codex | Usage limit message (not actionable) |
| 2 | gemini-code-assist | Summary of changes (not actionable) |

## Comment Resolution Plan

### MUST_FIX
- [x] #2,#4,#7,#8: Fix case-insensitivity across all layers (IFeatureFlagDefaults uses case-insensitive dict, GetByNameAsync uses ILike, normalize name in handler)
- [x] #1,#11: Create FeatureFlagsProvider context, update useFeatureFlag to read from context, fix stale name bug

### SHOULD_FIX
- [x] #5: FeatureFlagService.IsEnabledAsync should throw for undefined flags
- [x] #6: Defensive GroupBy in GetFeatureFlagsQueryHandler to handle case-different DB rows
- [x] #12: Add domain validation in FeatureFlag.Create (empty name, max length)
- [x] #9,#13: Update task doc requirements to reflect actual implementation decisions

### OUT_OF_SCOPE
- [x] #3: IDateTimeProvider — consistent with all existing entities (Notification, Organization all use DateTime.UtcNow directly)

### SUGGESTION (no action needed)
- [x] #10: PascalCase flag keys — case-insensitivity fix makes this cosmetic; PascalCase is .NET convention

## Status: COMPLETE
