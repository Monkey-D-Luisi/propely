# Task: 0003 - Setup next-intl Infrastructure

## Metadata
- ID: 0003
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0003-next-intl-setup.md`

## Goal
Install and configure next-intl for the Next.js app with locale routing, middleware for locale detection, and the provider in the root layout. Create the message file structure. Do NOT extract strings yet (that is task 0005).

## Context
The app currently has all strings hardcoded in components. We need the i18n infrastructure in place before we can extract strings. next-intl was chosen for its excellent App Router support, server component integration, and TypeScript safety.

## Scope
### In scope
- Install next-intl
- Configure next-intl middleware for locale detection (Accept-Language header)
- Setup locale routing: `/en/...` and `/es/...` with `en` as default
- Create message file structure: `messages/en.json` and `messages/es.json` (with minimal placeholder content)
- Configure next-intl provider in root layout
- Create i18n configuration file (`i18n/config.ts` or `i18n.ts`)
- Create a `useLocale` / locale switcher foundation (just the config, not the UI)
- Ensure existing pages work without changes (messages can be empty initially)

### Out of scope
- Extracting strings from components (task 0005)
- Spanish translations (task 0006)
- Language switcher UI (task 0006)

## Requirements
- R1: App supports `/en` and `/es` URL prefixes
- R2: Default locale is `en`, used when no locale prefix is present
- R3: Middleware detects browser locale from Accept-Language header
- R4: Message files are organized by feature namespace: `common`, `auth`, `orgs`, `errors`
- R5: Server components and client components can both access translations
- R6: Existing pages continue to work (no regression)

## Acceptance Criteria
- AC1: Navigating to `/en/login` and `/es/login` both render the login page
- AC2: Navigating to `/login` (no prefix) redirects to `/en/login`
- AC3: `messages/en.json` exists with namespace structure
- AC4: `messages/es.json` exists with same namespace keys (values can be empty or same as EN)
- AC5: `npm run build` succeeds
- AC6: ESLint passes

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Install next-intl
2. Create i18n config with supported locales and default locale
3. Create middleware.ts for locale negotiation
4. Restructure app directory for locale routing: `app/[locale]/`
5. Setup NextIntlClientProvider in layout
6. Create empty message files with namespace structure
7. Verify all existing pages still work

## Implementation Steps
1. `cd apps/web && npm install next-intl`
2. Create `apps/web/src/i18n/config.ts` - export locales, defaultLocale, type Locale
3. Create `apps/web/src/i18n/request.ts` - getRequestConfig for next-intl
4. Create/update `apps/web/src/middleware.ts` - createMiddleware from next-intl
5. Create `apps/web/messages/en.json` with namespace structure:
   ```json
   {
     "common": { "appName": "SaaS Template", "signIn": "Sign in", "signOut": "Sign out" },
     "auth": {},
     "orgs": {},
     "errors": {}
   }
   ```
6. Create `apps/web/messages/es.json` with same structure (empty values for now)
7. Move pages from `app/` to `app/[locale]/` to enable locale routing
8. Update root layout to use NextIntlClientProvider
9. Update next.config.ts if needed for next-intl plugin
10. Run build to verify

## Files to Create / Modify
- `apps/web/src/i18n/config.ts` (create)
- `apps/web/src/i18n/request.ts` (create)
- `apps/web/src/middleware.ts` (create or modify)
- `apps/web/messages/en.json` (create)
- `apps/web/messages/es.json` (create)
- `apps/web/src/app/[locale]/layout.tsx` (create - locale-aware layout)
- `apps/web/src/app/[locale]/page.tsx` (move from app/)
- `apps/web/src/app/[locale]/login/page.tsx` (move from app/login/)
- `apps/web/src/app/[locale]/register/page.tsx` (move from app/register/)
- `apps/web/src/app/[locale]/orgs/` (move from app/orgs/)
- `apps/web/next.config.ts` (modify if needed)

## Testing Plan
- Unit tests: N/A (infrastructure setup)
- Integration tests: N/A
- Manual verification: Navigate to `/en/login`, `/es/login`, `/login` and verify correct behavior

## Security & Privacy
- No security impact
- Locale detection uses standard Accept-Language header only

## Observability
- No changes to observability

## Rollback Plan
Remove next-intl, revert app directory structure, revert middleware.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
