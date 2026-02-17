# Walkthrough: CR-0053 Design System PR Review

- **Task**: `docs/tasks/cr-0053-design-system-review.md`
- **PR**: [#252](https://github.com/Monkey-D-Luisi/saas-template/pull/252)

## What Changed

### MUST_FIX items

1. **Internationalized "View" string** — Added `mine.view` to `en.json`/`es.json`, replaced hardcoded "View" in `orgs/mine/page.tsx`.

2. **Fixed font URL** — Updated Google Fonts link from legacy `Material+Icons` to `Material+Symbols+Outlined` in `layout.tsx`. Updated all `material-icons` CSS classes to `material-symbols-outlined` across `orgs/mine/page.tsx`.

3. **Aligned focus ring classes** — Changed `focus:ring-primary-500/20` to `focus:ring-primary-100` in `form-field.tsx`, `form-select.tsx`, and `form-textarea.tsx` to match design system documentation.

### SHOULD_FIX items

4. **Replaced inline SVGs with shared icon components** in:
   - `error.tsx` → `AlertTriangleIcon`
   - `not-found.tsx` → `ArrowLeftIcon`
   - `global-error.tsx` → `AlertTriangleIcon`
   - `FeatureFlagTable.tsx` → `FlagIcon`
   - `MembersManager.tsx` → `SettingsIcon`, `SearchIcon`

5. **AuthLayout max-width** — Changed `max-w-[440px]` to `max-w-md` in `auth-layout.tsx`.

6. **Eliminated double function call** — `renderResendFeedback()` in `VerifyEmailContent.tsx` now stores result in a variable.

### Rejected

- **Button shadow change** — Kept `shadow-lg shadow-primary-600/25` as it matches Stitch mockups.

## Validation

- `npm run build` — passed
- `npm test` — 305/305 passed
