# Walkthrough: CR-0069 — Privacy & Terms Pages PR Review

## Task Reference
- Task: `docs/tasks/cr-0069-privacy-terms-review.md`
- PR: #284 — feat(legal): add privacy policy and terms of service pages

## Changes Made

### 1. Moved hardcoded dates to i18n keys
- Added `privacy.lastUpdatedDate` and `terms.lastUpdatedDate` keys to both `en.json` and `es.json`
- Updated `privacy/page.tsx` and `terms/page.tsx` to read dates from i18n instead of hardcoded strings
- This allows content managers to update dates alongside content without code changes

### 2. Separated GDPR rights into title/description keys
- Replaced single `gdprAccess`, `gdprRectification`, etc. keys with `*Title` + `*Desc` pairs
- Updated `privacy/page.tsx` to render using separate keys instead of `split(' — ')`
- Eliminates fragile runtime string parsing that depended on em-dash delimiter

### 3. Fixed Spanish contactEmail
- Changed `soporte@ejemplo.com` to `support@example.com` (matching English)
- Email addresses are functional identifiers, not translatable content
- `ejemplo.com` is not an RFC 2606 reserved domain, making it a real-world collision risk

## Commands Run
- `cd apps/web && npm run build` — verify both routes still build
- `cd apps/web && npm test` — verify all tests pass

## Process Deviations
None.

## Checklist
- [x] Task scope matches cr-0069 task file
- [x] Tests passing
- [x] No secrets committed
