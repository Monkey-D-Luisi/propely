# Task: cr-0010 — PR #149 Code Review (Tests for Schemas, Utils, API Client)

## PR Metadata
- **PR:** #149 — `test(web): add tests for schemas, utilities, and API client (#0012)`
- **Branch:** `feat/0012-tests-schemas-utils` → `main`
- **CI Status:** SUCCESS (all checks pass)

## Changed Files
- `apps/web/src/lib/__tests__/schemas.test.ts`
- `apps/web/src/lib/__tests__/api.test.ts`
- `apps/web/src/lib/__tests__/csrf.test.ts`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0012-tests-schemas-utils.md`
- `docs/walkthroughs/0012-tests-schemas-utils.md`

## Review Threads

### Source 1: Inline Review Comments (12)
1. Gemini #2773266386 — `api.test.ts:147` — Use `await promise.catch(e => e)` instead of try/catch
2. Gemini #2773266400 — `api.test.ts:160` — Same pattern suggestion
3. Gemini #2773266408 — `api.test.ts:173` — Same pattern suggestion
4. Gemini #2773266416 — `api.test.ts:186` — Same pattern suggestion
5. Gemini #2773266422 — `csrf.test.ts:27` — Unused `originalDocument` variable
6. Copilot #2773281750 — `walkthrough:6` — Branch/PR field says "main" instead of PR #149
7. Copilot #2773281783 — `csrf.test.ts:27` — Unused `originalDocument` (duplicate of #5)
8. Copilot #2773281801 — `schemas.test.ts:355` — Testing PASSWORD_MIN_LENGTH constant value is redundant
9. Copilot #2773281820 — `walkthrough:32` — Test count says 49, Copilot claims 50
10. Copilot #2773281841 — `walkthrough:10` — Total says 73, Copilot claims 74
11. Copilot #2773281853 — `walkthrough:60` — Total says 78, Copilot claims 79
12. Copilot #2773281864 — `schemas.test.ts:54` — OrgSchema assertion with explicit `role: undefined` is misleading

### Source 2: General Reviews (2)
- Gemini: Positive review, references inline comments
- Copilot: Summary with inline comments

### Source 3: Issue Comments (1)
- Gemini: Summary/changelog, no actionable feedback

## Comment Resolution Plan

### SHOULD_FIX
- [x] **#5/#7 (Gemini/Copilot)**: Remove unused `originalDocument` variable from csrf.test.ts
- [x] **#6 (Copilot)**: Update walkthrough Branch/PR from "main" to "PR #149 (`feat/0012-tests-schemas-utils`)"
- [x] **#12 (Copilot)**: Simplify OrgSchema test assertion — remove explicit `role: undefined`, use `toEqual(validOrg)` since Zod strips absent optional fields and `toEqual` treats missing keys as equivalent to `undefined`

### SUGGESTION
- [x] **#1-4 (Gemini)**: Use `await promise.catch(e => e)` pattern — declined. The try/catch pattern is more explicit, allows multiple assertions on the caught error, and is the more common convention in Vitest test suites. Both patterns are valid; this is a style preference.
- [x] **#8 (Copilot)**: Testing PASSWORD_MIN_LENGTH constant — declined. This acts as a canary test: if someone changes the constant value, this test will fail and force a deliberate review of all downstream effects.

### QUESTION
- [x] **#9/#10/#11 (Copilot)**: Test counts are wrong — Vitest verbose output confirms 49 schema tests, 73 new total, 78 overall. Copilot miscounted.

## Acceptance Criteria
- [x] All SHOULD_FIX items resolved
- [x] CI passes (lint + build)
- [x] All PR comments replied to
