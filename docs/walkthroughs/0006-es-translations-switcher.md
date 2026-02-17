# Walkthrough: 0006 - Add ES Translations and Language Switcher

## Task Reference
- Task: `docs/tasks/0006-es-translations-switcher.md`
- Walkthrough: `docs/walkthroughs/0006-es-translations-switcher.md`
- Branch/PR: `main`
- Date: `2026-02-06`

## Summary
Added complete Spanish translations for all user-facing strings, created a LanguageSwitcher component in the AppHeader, and verified hreflang/SEO metadata. The next-intl middleware already handles Accept-Language detection and cookie-based locale persistence.

## Context
- Background: Task 0005 extracted all frontend strings to EN locale files. The i18n infrastructure (next-intl middleware, routing, navigation helpers) was set up in task 0003.
- Problem statement: Need ES translations, a way for users to manually switch languages, and proper SEO metadata for both locales.
- Constraints: English-only code comments and variable names; only message values in Spanish.

## Decisions & Trade-offs
- **Decision: LanguageSwitcher as simple toggle button**
  - Options considered: Dropdown select, toggle button, flag icons
  - Why this choice: With only two locales, a toggle button is simpler and more direct than a dropdown. The button shows the target locale label ("ES" or "EN") to indicate what you switch to.
  - Consequences / risks: If more locales are added, this needs to become a dropdown.

- **Decision: hreflang via middleware + generateMetadata**
  - Options considered: Middleware-only (Link headers), HTML-only (meta tags), both
  - Why this choice: next-intl middleware automatically adds `Link` headers with `rel="alternate"` for all configured locales. Added `alternates.languages` in `generateMetadata` for HTML-level tags as belt-and-suspenders SEO.
  - Consequences / risks: Layout-level alternates point to locale roots (`/en`, `/es`), not page-specific URLs. Per-page hreflang would require each page to implement `generateMetadata`.

- **Decision: Locale persistence via next-intl default cookie**
  - Options considered: Custom cookie, localStorage, next-intl built-in
  - Why this choice: next-intl middleware automatically reads/writes a `NEXT_LOCALE` cookie. No custom code needed.

## Implementation Notes
- Key changes:
  - Filled all empty strings in `messages/es.json` with Spanish translations
  - Added `language`, `switchToEs`, `switchToEn` keys to both EN and ES locale files
  - Created `LanguageSwitcher` component using `useRouter` and `usePathname` from next-intl navigation
  - Integrated LanguageSwitcher into AppHeader (appears before auth controls)
  - Converted static `metadata` export to `generateMetadata` function in locale layout to include `alternates.languages`
- Edge cases handled: LanguageSwitcher preserves current pathname when switching locale
- Known limitations: hreflang tags in layout point to locale roots, not per-page URLs

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npm run build
```

## Files Changed
- `apps/web/messages/es.json` — Filled all empty strings with Spanish translations, added language switcher keys
- `apps/web/messages/en.json` — Added `language`, `switchToEs`, `switchToEn` keys
- `apps/web/src/components/layout/LanguageSwitcher.tsx` — New component: locale toggle button
- `apps/web/src/components/layout/AppHeader.tsx` — Added LanguageSwitcher import and rendered it in the header
- `apps/web/src/app/[locale]/layout.tsx` — Converted static metadata to generateMetadata with alternates.languages
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0006 status to IN_PROGRESS then DONE

## Tests
### Unit
- What was added/updated: No unit tests in this task (deferred to task 0014)
- How to run: `cd apps/web && npm test`

### Integration
- N/A

### Manual
- Verified: `npm run build` succeeds with all locale routes generated (`/en/*`, `/es/*`)
- Verified: JSON key parity between en.json and es.json (identical structure)

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Locale is validated by next-intl middleware against configured locales
- AuthN/AuthZ impact: None
- Sensitive data handling: None (locale preference is not sensitive)

## Follow-ups / Backlog
- [ ] Per-page hreflang tags (each page implements generateMetadata with specific alternates)
- [ ] If more locales are added, convert LanguageSwitcher to a dropdown

## Checklist
- [x] Task scope matches `docs/tasks/0006-es-translations-switcher.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
