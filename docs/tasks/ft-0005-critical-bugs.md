# ft-0005 — Critical Bug Fixes (404 routing, CSRF/session, custom 404)

## Goal

Fix critical and high-severity bugs discovered during the Puppeteer design audit (`docs/audits/non-design-bugs.md`), plus a user-reported session/CSRF authentication failure.

## Bugs Addressed

| Bug | Severity | Description |
|-----|----------|-------------|
| BUG-1 | CRITICAL | All `[orgId]` routes return 404 |
| BUG-2 | HIGH | Accept invite route returns 404 |
| BUG-3 | MEDIUM | Custom 404 page never renders |
| Session | HIGH | Login/register fails — "session not recognized" |
| BUG-4 | LOW | Dev mode issue indicator (missing useCallback dep) |

## Scope

### In scope
- Investigate and fix routing 404 issues
- Add catch-all `[...rest]` route and root `not-found.tsx`
- Fix CSRF double-submit cookie fragility (cross-origin cookie issues)
- Add `127.0.0.1` to CORS allowed origins
- Fix React hook dependency warning in AppHeader

### Out of scope
- Next.js 16 Turbopack dev-mode middleware-manifest bug (framework issue)
- Footer links to non-existent pages (tracked separately)

## Acceptance Criteria

- [x] `next build` succeeds with zero errors
- [x] `dotnet build` succeeds with zero errors
- [x] Auth + billing integration tests pass
- [x] Production routes return 200 for valid paths
- [x] Nonexistent routes trigger custom 404 via catch-all
- [x] Root `/` redirects to `/en` (307)
- [x] CsrfValidator accepts header-only tokens (cookie optional)

## Files Modified

| File | Change |
|------|--------|
| `apps/web/src/app/[locale]/[...rest]/page.tsx` | NEW — catch-all route calling `notFound()` |
| `apps/web/src/app/not-found.tsx` | NEW — root-level 404 page |
| `apps/web/src/lib/csrf.ts` | Mirror CSRF cookie via `document.cookie` after API call |
| `apps/web/src/components/layout/AppHeader.tsx` | Add `tAuth` to `useCallback` dependency array |
| `services/orgs-api/.../Services/CsrfValidator.cs` | Make cookie optional in CSRF validation |
| `.env` | Add `ORGSAPI_Cors__AllowedOrigins__0` and `__1` |
