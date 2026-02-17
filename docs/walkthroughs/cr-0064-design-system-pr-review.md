# Walkthrough: CR-0064 — Design System PR #264 Review

- Task: `docs/tasks/cr-0064-design-system-pr-review.md`
- PR: #264

## What Changed

### 1. auth-layout.tsx — MUST_FIX (CI lint + i18n)
- Added `'use client'` directive and `useTranslations` hook
- Replaced `<a>` tags with `<Link />` from `@/i18n/navigation` (fixes 3 ESLint `@next/next/no-html-link-for-pages` errors)
- Removed hardcoded English defaults, now reads from `auth.footer.*` translation keys
- Removed props interface (no longer needed — component is self-contained with i18n)

### 2. page.tsx (landing) — MUST_FIX + SHOULD_FIX (token + i18n)
- Changed pill badge `border-indigo-100 bg-indigo-50` → `border-primary-100 bg-primary-50` (design token compliance)
- Replaced all hardcoded English strings with `t('common.landing.*')` translation keys

### 3. messages/en.json + es.json — SHOULD_FIX (i18n keys)
- Added `common.landing.*` keys for hero badge, title prefix, CTA, and 3 feature cards
- Added `auth.footer.*` keys for privacy, terms, support labels

### 4. MembersTable.tsx — SHOULD_FIX (a11y)
- Added `focus-within:opacity-100` to action controls div so they're accessible via keyboard navigation and on touch devices

## Commands Run

```bash
cd apps/web && npm run build   # Verify compilation
cd apps/web && npm run lint    # Verify lint passes
```

## Validation Results

- Build: PASS
- Lint: PASS (0 errors)
