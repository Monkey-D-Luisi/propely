# Task: 0009 - Error Boundary and Custom Error Pages

## Metadata
- ID: 0009
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0009-error-boundary-pages.md`

## Goal
Create a global error boundary component, custom `not-found.tsx` and `error.tsx` pages at the app level, providing a polished error experience with retry capabilities and i18n support.

## Context
Currently if a component throws an error, the entire app crashes with React's default error screen. There are no custom 404 or error pages. A professional SaaS needs graceful error handling at every level.

## Scope
### In scope
- Create `ErrorBoundary` client component for runtime errors
- Create `app/[locale]/not-found.tsx` - custom 404 page
- Create `app/[locale]/error.tsx` - custom error page with retry button
- Create `app/[locale]/global-error.tsx` - root error boundary (catches layout errors)
- All error pages use i18n strings
- Link to go back / go home on error pages
- Clean, professional design consistent with rest of app

### Out of scope
- API error handling in components (already exists)
- Server-side error logging (separate concern)

## Requirements
- R1: Unhandled runtime errors show a friendly error page with retry button
- R2: 404 routes show a custom not-found page with link to home
- R3: Layout-level errors are caught by global-error.tsx
- R4: All error pages have i18n strings
- R5: Error pages include the SaaS Template branding/header (when possible)

## Acceptance Criteria
- AC1: Navigating to `/en/nonexistent-page` shows custom 404 page
- AC2: A component that throws renders the error page with retry button
- AC3: Clicking retry re-renders the errored component
- AC4: Error pages display correctly in both EN and ES
- AC5: `npm run build` succeeds
- AC6: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Create error.tsx and not-found.tsx in app/[locale]/
2. Create global-error.tsx in app/
3. Design clean error pages with illustration or icon
4. Add retry functionality in error.tsx
5. i18n all strings

## Implementation Steps
1. Add error-related strings to `messages/en.json` under `errors` namespace
2. Create `app/[locale]/not-found.tsx`:
   - Centered layout with 404 message
   - "Go back home" link
   - Uses `useTranslations('errors')`
3. Create `app/[locale]/error.tsx`:
   - Receives `error` and `reset` props from Next.js
   - Shows friendly error message
   - "Try again" button calls `reset()`
   - "Go to home" link as fallback
   - `'use client'` directive required
4. Create `app/global-error.tsx`:
   - Minimal error page (can't use layout)
   - Must include its own `<html>` and `<body>` tags
5. Run build and test

## Files to Create / Modify
- `apps/web/src/app/[locale]/not-found.tsx` (create)
- `apps/web/src/app/[locale]/error.tsx` (create)
- `apps/web/src/app/global-error.tsx` (create)
- `apps/web/messages/en.json` (modify - add error strings)
- `apps/web/messages/es.json` (modify - add error strings)

## Testing Plan
- Unit tests: Will be covered in task 0014
- Manual verification: Navigate to nonexistent URLs, trigger component errors

## Security & Privacy
- Error pages must NOT leak stack traces or internal details
- Only show user-friendly messages

## Observability
- Consider logging errors to console in development only

## Rollback Plan
Remove created files. Next.js falls back to default error handling.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
