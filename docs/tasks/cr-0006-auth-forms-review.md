# Task: cr-0006 — PR #145 Code Review (Auth Forms Refactor)

## PR Metadata
- **PR:** #145 — `feat(web): refactor auth forms with react-hook-form (#0007)`
- **Branch:** `feat/0007-refactor-auth-forms` → `main`
- **CI Status:** FAILURE (2 ESLint errors: `react-hooks/immutability`, 1 warning: missing dep)

## Changed Files
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/components/auth/LoginForm.tsx`
- `apps/web/src/components/auth/RegisterForm.tsx`
- `apps/web/src/lib/schemas.ts`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0007-refactor-auth-forms.md`
- `docs/walkthroughs/0007-refactor-auth-forms.md`

## Review Threads

### Source 1: Inline Review Comments (8)
1. Gemini #2772719845 — `LoginForm.tsx:31` — Remove `passwordMinLength: ''` after type split
2. Gemini #2772719849 — `RegisterForm.tsx:15` — Import `PASSWORD_MIN_LENGTH` from schemas
3. Gemini #2772719858 — `schemas.ts:68` — Split `AuthValidationMessages` into Base + Extended
4. Gemini #2772719861 — `schemas.ts:77` — Export `PASSWORD_MIN_LENGTH` from schemas.ts
5. Codex #2772722218 — `LoginForm.tsx:156` — Add `noValidate` to form for Zod errors to surface
6. Copilot #2772728767 — `schemas.ts:61` — Split validation message types (duplicate of #3)
7. Copilot #2772728785 — `RegisterForm.tsx:58` — Add `t` to useEffect dependency array
8. Copilot #2772728792 — `schemas.ts:76` — Centralize PASSWORD_MIN_LENGTH (duplicate of #4)

### Source 2: General Reviews (3)
- Gemini: Positive review, no additional issues
- Codex: Boilerplate intro, no additional issues
- Copilot: Summary with 3 inline comments (already counted above)

### Source 3: Issue Comments (2)
- Gemini: Summary/changelog, no actionable feedback
- Claude: "No issues found"

### CI Failures (not from reviewers)
- `LoginForm.tsx:110` — `react-hooks/immutability` error on `window.location.href = nextParam`
- `RegisterForm.tsx:91` — `react-hooks/immutability` error on `window.location.href = '/'`
- `RegisterForm.tsx:58` — warning: useEffect missing `t` dependency

## Comment Resolution Plan

### MUST_FIX
- [x] **CI-1**: Fix `react-hooks/immutability` in LoginForm — use `window.location.assign()` instead of assignment
- [x] **CI-2**: Fix `react-hooks/immutability` in RegisterForm — same approach
- [x] **#5 (Codex)**: Add `noValidate` to `<form>` in LoginForm and RegisterForm so Zod errors surface instead of browser native validation

### SHOULD_FIX
- [x] **#3/#6 (Gemini/Copilot)**: Split `AuthValidationMessages` into `LoginValidationMessages` and `RegisterValidationMessages`
- [x] **#4/#8 (Gemini/Copilot)**: Export `PASSWORD_MIN_LENGTH` from schemas.ts, use in schema
- [x] **#1 (Gemini)**: Remove `passwordMinLength: ''` from LoginForm (consequence of type split)
- [x] **#2 (Gemini)**: Import `PASSWORD_MIN_LENGTH` from schemas in RegisterForm
- [x] **#7 (Copilot)**: Add `t` to useEffect dependency array in RegisterForm (also fixes CI warning)

### OUT_OF_SCOPE
- CI warnings in AppHeader.tsx, AcceptInvite.tsx, MembersManager.tsx — pre-existing, not from this PR
- `layout.tsx:17` unused `locale` — pre-existing from previous PR

## Acceptance Criteria
- [x] All MUST_FIX items resolved
- [x] All SHOULD_FIX items resolved
- [x] CI passes (lint errors fixed)
- [x] Build passes
- [x] All PR comments replied to
