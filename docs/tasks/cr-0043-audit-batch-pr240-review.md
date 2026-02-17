# Code Review: cr-0043-audit-batch-pr240-review

## PR Metadata
- **PR:** #240 — fix(security): resolve Epic 004 audit findings (#audit-0009 to #audit-0015)
- **Branch:** `fix/audit-epic-004-batch` → `main`
- **CI Status:** All checks green (Detect Changes: SUCCESS, Orgs API: SUCCESS, Web: SUCCESS, AI API: SKIPPED)

## Changed Files (36)
See PR for full list.

## Review Threads

### Inline Comments (4)

| # | Source | User | File | Classification | Summary |
|---|--------|------|------|----------------|---------|
| 1 | Gemini | gemini-code-assist | MembershipRepository.cs:52 | SHOULD_FIX | Hardcoded table/column names in raw SQL — use EF Core metadata API |
| 2 | Copilot | Copilot | UnitOfWork.cs:35 | SHOULD_FIX | Bare `catch` — use `catch (Exception)` explicitly |
| 3 | Copilot | Copilot | OrgsDeleteEndpointTests.cs:101 | MUST_FIX | Rate limit IP collision — 14 DELETE requests share same IP, can hit 10/min limit |
| 4 | Copilot | Copilot | epic-004-executive-summary.md:15 | MUST_FIX | Status metadata says "In Progress" but all items are "Done" |

### General Reviews (2)
- Gemini: Positive overall review, references inline comment #1
- Copilot: Summary of changes, references inline comments #2-#4

### Issue Comments (2)
- chatgpt-codex-connector: Usage limit notice (not actionable)
- gemini-code-assist: Summary (not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] **Comment #3:** Add unique `X-Real-IP` header to DELETE requests in integration tests to avoid rate limit collisions
- [x] **Comment #4:** Update executive summary metadata status from "In Progress" to "Complete"

### SHOULD_FIX
- [x] **Comment #1:** Use EF Core metadata API for table/column names in `GetByOrgIdForUpdateAsync`
- [x] **Comment #2:** Change bare `catch` to `catch (Exception)` in `ExecuteInTransactionAsync`

## Behavioral Parity Checks
- [x] Redirect parity checked (`next` propagation and sanitization) — N/A, no auth entry point changes
- [x] Locale source correctness checked (explicit locale + fallback) — N/A, no localization changes
- [x] API/UI contract parity checked (fields and payloads) — N/A, no new DTO fields
- [x] Test parity checked (happy + error/validation paths) — Covered by existing unit, component, and integration tests
