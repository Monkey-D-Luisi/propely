# CR-0053: Design System PR Review

- **PR**: [#252](https://github.com/Monkey-D-Luisi/saas-template/pull/252)
- **Branch**: `feat/0065-design-system` → `main`
- **CI Status**: All checks passed (Web Build & Test ✅, Claude Code Review ✅)
- **Reviewers**: Copilot, Gemini Code Assist

## Changed Files (36)

See PR for the full list of 36 changed files spanning design tokens, auth pages, form components, icons, error pages, and walkthrough docs.

## Review Comments Summary

- Inline review comments: 18
- General reviews: 2 (Copilot overview + Gemini overview)
- Issue comments: 2 (non-actionable: ChatGPT Codex limit notice + Gemini summary)

## Comment Resolution Plan

### MUST_FIX

- [x] **Hardcoded "View" string** (Copilot #2795746444, Gemini #2795746746) — `orgs/mine/page.tsx:159`
  - Add `mine.view` key to `en.json` and `es.json`, replace hardcoded "View" with `t('mine.view')`

- [x] **Font URL mismatch** (Copilot #2795746527) — `layout.tsx:12`
  - PR says "Material Symbols Outlined" but loads legacy "Material Icons". Update to Material Symbols Outlined URL and update CSS class from `material-icons` to `material-symbols-outlined` in all usages.

- [x] **Focus ring inconsistency** (Gemini #2795746769, #2795746771, #2795746774) — form-field.tsx:56, form-select.tsx:51, form-textarea.tsx:44
  - Change `focus:ring-primary-500/20` → `focus:ring-primary-100` to match design system docs in CLAUDE.md

### SHOULD_FIX

- [x] **Inline SVGs should use shared icons** (Copilot #2795746474, #2795746482, #2795746501; Gemini #2795746737, #2795746743, #2795746749, #2795746751, #2795746757, #2795746763)
  - `error.tsx:22-24` → `AlertTriangleIcon`
  - `not-found.tsx:18-20` → `ArrowLeftIcon`
  - `global-error.tsx:26-28` → `AlertTriangleIcon`
  - `FeatureFlagTable.tsx:76-78` → `FlagIcon`
  - `MembersManager.tsx:218-221` → `SettingsIcon`
  - `MembersManager.tsx:237-239` → `SearchIcon`

- [x] **AuthLayout max-w-[440px] → max-w-md** (Gemini #2795746752) — auth-layout.tsx:11
  - Design system docs say `max-w-md` for auth pages

- [x] **Double `renderResendFeedback` call** (Copilot #2795746515) — VerifyEmailContent.tsx:198-200
  - Store result in variable to avoid redundant invocation

### SUGGESTION

- [ ] **Button shadow: `shadow-lg shadow-primary-600/25` → `shadow-sm`** (Gemini #2795746765) — button.tsx:8
  - **REJECTED**: The `shadow-lg shadow-primary-600/25` matches the Stitch design mockups which use elevated brand-colored shadows for primary CTA buttons. The `shadow-sm` in CLAUDE.md is a simplification; the actual Stitch designs use the larger shadow. Keeping as-is for pixel-perfect alignment with designs.

## Behavioral Parity Checks

- [x] Redirect parity checked (`next` propagation and sanitization) — N/A, no auth flow logic changed
- [x] Locale source correctness checked — N/A, no locale handling changed
- [x] API/UI contract parity checked — N/A, no API contracts changed
- [x] Test parity checked — all 305 tests pass, test assertions updated for new button labels
