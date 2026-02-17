# Task: cr-0008 — PR #147 Code Review (Error Boundary & Custom Error Pages)

## PR Metadata
- **PR:** #147 — `feat(web): error boundary and custom error pages (#0009)`
- **Branch:** `feat/0009-error-boundary-pages` → `main`
- **CI Status:** SUCCESS (all checks pass)

## Changed Files
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/app/[locale]/not-found.tsx`
- `apps/web/src/app/[locale]/error.tsx`
- `apps/web/src/app/global-error.tsx`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0009-error-boundary-pages.md`
- `docs/walkthroughs/0009-error-boundary-pages.md`

## Review Threads

### Source 1: Inline Review Comments (8)
1. Gemini #2773037094 — `global-error.tsx:29` — Extract strings to constants object + log error with useEffect
2. Gemini #2773037099 — `error.tsx:36` — Log error with useEffect for debugging
3. Copilot #2773043587 — `error.tsx:23` — Add focus ring styles to retry button
4. Copilot #2773043610 — `error.tsx:29` — Add focus ring styles to Link
5. Copilot #2773043636 — `not-found.tsx:14` — Add focus ring styles to Link
6. Copilot #2773043652 — `global-error.tsx:21` — Add focus ring styles to button
7. Copilot #2773043667 — `en.json:196` — Unused `errors.global` i18n keys
8. Copilot #2773043677 — `es.json:196` — Unused `errors.global` i18n keys (duplicate of #7)

### Source 2: General Reviews (2)
- Gemini: Positive review, references inline comments
- Copilot: Summary with 6 inline comments (already counted above)

### Source 3: Issue Comments (2)
- Gemini: Summary/changelog, no actionable feedback
- Claude: "No issues found"

## Comment Resolution Plan

### SHOULD_FIX
- [x] **#1 (Gemini)**: Extract hardcoded strings in `global-error.tsx` to a constants object and log error with `useEffect` in development
- [x] **#2 (Gemini)**: Log error with `useEffect` in `error.tsx` for debugging in development
- [x] **#3 (Copilot)**: Add `focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2` to retry button in `error.tsx`
- [x] **#4 (Copilot)**: Add focus ring styles to "Go to home page" Link in `error.tsx`
- [x] **#5 (Copilot)**: Add focus ring styles to "Go to home page" Link in `not-found.tsx`
- [x] **#6 (Copilot)**: Add focus ring styles to "Refresh page" button in `global-error.tsx`

### OUT_OF_SCOPE
- [x] **#7/#8 (Copilot)**: `errors.global` i18n keys — these are intentionally kept for documentation and future use. The `global-error.tsx` uses hardcoded English because the i18n provider is unavailable when the layout errors. The keys document the canonical strings and will be used if locale detection from the URL is added later.

## Acceptance Criteria
- [x] All SHOULD_FIX items resolved
- [x] CI passes (lint + build)
- [x] All PR comments replied to
