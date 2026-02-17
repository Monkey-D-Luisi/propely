# Task: 0006 - Add ES Translations and Language Switcher

## Metadata
- ID: 0006
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0006-es-translations-switcher.md`

## Goal
Create complete Spanish translation files, implement automatic locale detection from browser settings, and add a language switcher component to the AppHeader.

## Context
Task 0005 extracted all strings to EN message files. Now we need to provide the ES translations, detect the user's preferred language automatically, and give them a way to manually switch languages.

## Scope
### In scope
- Create complete `messages/es.json` with all Spanish translations
- Verify next-intl middleware detects Accept-Language header (configured in task 0003)
- Create LanguageSwitcher component in AppHeader
- Persist language preference in a cookie so it survives navigation
- Ensure locale is reflected in the URL (`/es/login`, `/en/login`)

### Out of scope
- Adding more languages beyond EN and ES
- Translating backend API error messages (those remain in English)

## Requirements
- R1: `messages/es.json` contains translations for 100% of keys in `messages/en.json`
- R2: Browser with `Accept-Language: es` is automatically redirected to `/es/` routes
- R3: Language switcher in AppHeader allows manual toggle between EN and ES
- R4: Selected language persists across page navigations (cookie-based)
- R5: SEO: `<html lang="">` attribute reflects current locale
- R6: SEO: alternate hreflang tags for both locales

## Acceptance Criteria
- AC1: All pages render correctly in Spanish when locale is `es`
- AC2: All pages render correctly in English when locale is `en`
- AC3: Clicking the language switcher changes the URL prefix and re-renders in the new language
- AC4: A fresh browser with Spanish system language lands on `/es/` pages
- AC5: `npm run build` succeeds
- AC6: ESLint passes

## Constraints (non-negotiable)
- English-only repo content (code comments, variable names in English; only message values in Spanish).
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Translate all EN strings to ES in messages/es.json
2. Verify locale detection middleware works
3. Create LanguageSwitcher component
4. Add to AppHeader
5. Add locale persistence via cookie
6. Add hreflang tags in layout

## Implementation Steps
1. Copy `messages/en.json` to `messages/es.json` and translate all values to Spanish
2. Verify middleware locale detection by testing with Accept-Language header
3. Create `apps/web/src/components/layout/LanguageSwitcher.tsx`:
   - Show current locale flag/label
   - On click, navigate to same page with different locale prefix
   - Use next-intl's `useRouter` and `usePathname` for locale-aware navigation
4. Add LanguageSwitcher to AppHeader
5. Configure next-intl to persist locale preference in cookie
6. Add `<html lang={locale}>` in root layout (likely already handled by next-intl)
7. Add hreflang `<link>` tags in layout head
8. Test both locales thoroughly

## Files to Create / Modify
- `apps/web/messages/es.json` (create with full translations)
- `apps/web/src/components/layout/LanguageSwitcher.tsx` (create)
- `apps/web/src/components/layout/AppHeader.tsx` (modify - add LanguageSwitcher)
- `apps/web/src/app/[locale]/layout.tsx` (modify - add hreflang tags, html lang)

## Testing Plan
- Unit tests: LanguageSwitcher component test (task 0014)
- Manual verification: Test both locales, test detection, test switcher

## Security & Privacy
- Locale cookie is not sensitive (no HttpOnly needed)
- No PII involved

## Observability
- No changes to observability

## Rollback Plan
Remove es.json, LanguageSwitcher component. Revert AppHeader and layout changes.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
