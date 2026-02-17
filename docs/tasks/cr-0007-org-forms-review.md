# Task: cr-0007 — PR #146 Code Review (Org Forms Refactor)

## PR Metadata
- **PR:** #146 — `feat(web): refactor org forms with react-hook-form (#0008)`
- **Branch:** `feat/0008-refactor-org-forms` → `main`
- **CI Status:** SUCCESS (all checks pass)

## Changed Files
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/app/[locale]/orgs/mine/page.tsx`
- `apps/web/src/components/orgs/InviteForm.tsx`
- `apps/web/src/lib/schemas.ts`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0008-refactor-org-forms.md`
- `docs/walkthroughs/0008-refactor-org-forms.md`

## Review Threads

### Source 1: Inline Review Comments (4)
1. Codex #2772955292 — `schemas.ts:107` — Whitespace-only org names pass validation
2. Gemini #2772957390 — `mine/page.tsx:42` — Capture error object in catch block
3. Gemini #2772957395 — `mine/page.tsx:94` — Simplify FormError with optional chaining
4. Copilot #2772982853 — `mine/page.tsx:94` — Move FormError inside form for consistency

### Source 2: General Reviews (3)
- Codex: Boilerplate intro, no additional issues
- Gemini: Positive review, references inline comments
- Copilot: Summary with 1 inline comment (already counted above)

### Source 3: Issue Comments (2)
- Gemini: Summary/changelog, no actionable feedback
- Claude: "No issues found"

## Comment Resolution Plan

### MUST_FIX
- [x] **#1 (Codex)**: Add `.trim()` to Zod schema so whitespace-only org names are rejected. Remove manual `.trim()` in submit handler since schema handles it.

### SHOULD_FIX
- [x] **#3/#4 (Gemini/Copilot)**: Move FormError inside `<form>`, simplify with optional chaining (`methods.formState.errors.root?.message`)

### SUGGESTION
- [x] **#2 (Gemini)**: Capture error in catch — declined. The error is unused; capturing it would trigger an unused-variable lint warning. The current pattern (`catch { ... }`) is valid TypeScript and intentional.

## Acceptance Criteria
- [x] All MUST_FIX items resolved
- [x] All SHOULD_FIX items resolved
- [x] CI passes (lint + build)
- [x] All PR comments replied to
