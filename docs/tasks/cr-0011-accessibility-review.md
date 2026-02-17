# Task: cr-0011 — PR #150 Code Review (Accessibility Audit and Improvements)

## PR Metadata
- **PR:** #150 — `feat(web): accessibility audit and improvements (#0011)`
- **Branch:** `feat/0011-accessibility` → `main`
- **CI Status:** SUCCESS (all checks pass)

## Changed Files
- `.agent/rules/autonomous-workflow.md`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/app/[locale]/error.tsx`
- `apps/web/src/app/[locale]/layout.tsx`
- `apps/web/src/app/[locale]/not-found.tsx`
- `apps/web/src/app/global-error.tsx`
- `apps/web/src/components/orgs/LeaveOrgButton.tsx`
- `apps/web/src/components/orgs/MembersManager.tsx`
- `apps/web/src/components/orgs/MembersTable.tsx`
- `apps/web/src/components/ui/toast.tsx`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0011-accessibility.md`
- `docs/walkthroughs/0011-accessibility.md`

## Review Threads

### Source 1: Inline Review Comments (3)
1. Gemini #2773448199 — `toast.tsx:99` — "Close" button text hardcoded; should use `close` i18n key; `aria-label` redundant
2. Copilot #2773465550 — `toast.tsx:98` — Same as #1: hardcoded "Close", use `useTranslations`
3. Copilot #2773465569 — `autonomous-workflow.md:113` — Co-author 4.5→4.6 unrelated to a11y PR

### Source 2: General Reviews (2)
- Gemini: Positive review, references toast i18n suggestion
- Copilot: Positive review, references 2 inline comments

### Source 3: Issue Comments (2)
- Gemini: Summary/changelog, no actionable feedback
- Claude: "No issues found"

## Comment Resolution Plan

### SHOULD_FIX
- [x] **#1/#2 (Gemini/Copilot)**: Use `useTranslations('common')` in `ToastProvider` to i18n the "Close" button text with `tCommon('close')`. Remove redundant `aria-label="Close"`. Updated test utility (`test/utils.tsx`) to wrap with `NextIntlClientProvider`.

### SUGGESTION
- [x] **#3 (Copilot)**: Co-author version update is already in a separate commit (`chore(agent)`) and reflects the actual model version. Declined — the change is accurate and already isolated.

## Acceptance Criteria
- [x] All SHOULD_FIX items resolved
- [x] CI passes (lint + build)
- [x] All PR comments replied to
