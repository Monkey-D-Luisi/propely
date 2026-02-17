# Walkthrough: 0009 - Error Boundary and Custom Error Pages

## Task Reference
- Task: `docs/tasks/0009-error-boundary-pages.md`
- Walkthrough: `docs/walkthroughs/0009-error-boundary-pages.md`
- Branch/PR: `main`
- Date: `2026-02-06`

## Summary
Created custom error pages for the Next.js app: `not-found.tsx` (404), `error.tsx` (runtime errors with retry), and `global-error.tsx` (layout-level errors). All locale-scoped pages use i18n via `useTranslations`. Global error page uses hardcoded English since it can't access the i18n provider.

## Context
- Background: The app had no custom error pages. Unhandled errors showed React's default error screen, and 404s showed the browser default.
- Problem statement: A professional SaaS needs graceful, branded error handling at every level.
- Constraints: Visual appearance consistent with the rest of the app. No stack traces or internal details exposed.

## Decisions & Trade-offs
- **Decision: global-error.tsx uses hardcoded English**
  - Options considered: Detect locale from URL, use hardcoded English, dynamic import of messages
  - Why this choice: `global-error.tsx` replaces the entire page including `<html>` and `<body>`. It cannot use `NextIntlClientProvider` because the layout that provides it has errored. Hardcoded English is acceptable for this rare edge case.

- **Decision: not-found.tsx as a server component**
  - Options considered: Client component with `'use client'`, server component
  - Why this choice: `not-found.tsx` in Next.js App Router can be a server component when placed in `app/[locale]/`. It uses `useTranslations` from next-intl which works in both server and client components. No client-side interactivity needed.

- **Decision: error.tsx with `reset` button**
  - Options considered: Only "go home" link, retry button + go home link
  - Why this choice: Next.js provides a `reset` function that re-renders the error boundary's children. Offering both retry and go-home gives users the best recovery options.

## Implementation Notes
- Key changes:
  - Added `errors.notFound`, `errors.runtime`, and `errors.global` i18n keys to both EN and ES locale files
  - `not-found.tsx`: Server component, shows 404 code, title, description, and "Go to home page" link
  - `error.tsx`: Client component (`'use client'`), shows error title/description, "Try again" button (calls `reset()`), "Go to home page" link, logs error via `useEffect`
  - `global-error.tsx`: Client component, provides own `<html>`/`<body>`, hardcoded English via `STRINGS` constant, "Refresh page" button (calls `reset()`), logs error via `useEffect`
- Edge cases: global-error.tsx must include its own `<html>` and `<body>` tags since the layout has errored
- Known limitations: global-error.tsx is English-only (acceptable for critical error fallback)

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npm run build
cd apps/web && npx eslint "src/app/[locale]/not-found.tsx" "src/app/[locale]/error.tsx" src/app/global-error.tsx
```

## Files Changed
- `apps/web/messages/en.json` — Added `errors.notFound`, `errors.runtime`, `errors.global` keys
- `apps/web/messages/es.json` — Added matching Spanish keys
- `apps/web/src/app/[locale]/not-found.tsx` — Created custom 404 page with i18n
- `apps/web/src/app/[locale]/error.tsx` — Created runtime error page with retry + i18n
- `apps/web/src/app/global-error.tsx` — Created global error fallback with own html/body
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0009 status and progress tracker
- `docs/tasks/0009-error-boundary-pages.md` — Marked DoD checklist complete

## Tests
### Unit
- What was added/updated: No unit tests in this task (deferred to task 0014)
- How to run: `cd apps/web && npm test`

### Manual
- Verified: `npm run build` succeeds with all routes generating correctly
- Verified: ESLint passes with no errors

## Observability
- `error.tsx` logs runtime errors via `useEffect` + `console.error` for debugging
- `global-error.tsx` logs critical errors via `useEffect` + `console.error` for debugging

## Security
- Error pages do NOT expose stack traces or internal details
- Only user-friendly messages are shown

## Follow-ups / Backlog
- [ ] Unit tests for error pages (task 0014)

## Checklist
- [x] Task scope matches `docs/tasks/0009-error-boundary-pages.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
