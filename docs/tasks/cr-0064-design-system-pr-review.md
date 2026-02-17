# CR-0064: Design System PR #264 Review

- PR: #264
- Target branch: main
- CI status: FAILURE (Web lint — 3 errors in auth-layout.tsx using `<a>` for internal routes)
- GitHub Issue: #250

## Changed Files (46)

All under `apps/web/` — CSS-only Tailwind class changes across 26 source files, plus audit docs and walkthrough updates.

## Review Threads

### Source 1: Inline Review Comments (7)

| ID | Reviewer | File | Line | Summary |
|----|----------|------|------|---------|
| 2801121283 | Copilot | page.tsx | 14 | Pill badge uses `indigo-*` instead of `primary-*` |
| 2801121310 | Copilot | page.tsx | 82 | Hardcoded English strings on `[locale]` page |
| 2801121321 | Copilot | auth-layout.tsx | 65 | AuthFooterLinks hardcoded English defaults |
| 2801121778 | Gemini | auth-layout.tsx | 70 | Use `<Link />` instead of `<a>` for internal routes |
| 2801121916 | Codex | MembersTable.tsx | 84 | Action controls hidden on hover — touch/keyboard inaccessible |
| 2801121917 | Codex | auth-layout.tsx | 68 | Footer links to non-existent `/privacy`, `/terms`, `/support` → 404 |
| 2801121918 | Codex | page.tsx | 16 | Hardcoded English copy on locale homepage |

### Source 2: General Reviews (3)

| ID | Reviewer | State | Actionable |
|----|----------|-------|-----------|
| 3793743430 | Copilot | COMMENTED | No (summary only) |
| 3793744341 | Gemini | COMMENTED | No (positive review, suggestions via inline comments) |
| 3793744530 | Codex | COMMENTED | No (header only, suggestions via inline comments) |

### Source 3: Issue Comments (1)

| Author | Actionable |
|--------|-----------|
| gemini-code-assist | No (PR summary) |

## Comment Resolution Plan

### MUST_FIX

- [x] **CI lint failure**: `auth-layout.tsx` lines 68-70 — change `<a>` to `<Link />` from `@/i18n/navigation` (Gemini #2801121778, CI)
- [x] **Design token violation**: `page.tsx` line 14 — change `indigo-100`/`indigo-50` to `primary-100`/`primary-50` (Copilot #2801121283)

### SHOULD_FIX

- [x] **Landing page i18n**: `page.tsx` — move all hardcoded English strings to translation files (Copilot #2801121310, Codex #2801121918)
- [x] **AuthFooterLinks i18n**: `auth-layout.tsx` — use `useTranslations` instead of hardcoded English defaults (Copilot #2801121321)
- [x] **Members table a11y**: `MembersTable.tsx` line 84 — add `focus-within:opacity-100` so actions are accessible via keyboard/touch (Codex #2801121916)

### OUT_OF_SCOPE

- [ ] **Create `/privacy`, `/terms`, `/support` pages** (Codex #2801121917): These are standard placeholder footer links. Creating full legal/support pages is out of scope for the design system task. The 404 page handles gracefully. Deferred to a future task.

## Parity Verification

- [x] Redirect parity: N/A — no auth flow changes
- [x] Locale source correctness: Fixed — landing page now uses i18n keys
- [x] API/UI contract parity: N/A — no API changes
- [x] Test parity: N/A — CSS-only + i18n string additions, no behavior changes
