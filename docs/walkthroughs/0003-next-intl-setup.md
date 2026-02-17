# Walkthrough: 0003-next-intl-setup

## Task Reference
- Task: `docs/tasks/0003-next-intl-setup.md`
- Walkthrough: `docs/walkthroughs/0003-next-intl-setup.md`
- Branch/PR: `feat/0003-next-intl-setup`
- Date: 2026-02-05

## Summary
Installed and configured next-intl v4.8.2 for the Next.js app with locale-based routing (`/en/...`, `/es/...`), proxy middleware for locale detection, message file structure, and the NextIntlClientProvider in the locale layout. All existing pages work under the new locale routing.

## Context
- Background: All strings in the app are hardcoded in English. The i18n infrastructure needs to be in place before string extraction (task 0005).
- Problem statement: Need locale routing, middleware, message files, and provider setup.
- Constraints: Infrastructure only, no string extraction yet.

## Decisions & Trade-offs
- **Decision:** Used `proxy.ts` instead of `middleware.ts`
  - Options considered: middleware.ts (pre-Next.js 16), proxy.ts (Next.js 16+)
  - Why this choice: Next.js 16 renamed middleware to proxy. next-intl docs confirm `proxy.ts` for Next.js 16+.
  - Consequences / risks: None.

- **Decision:** Used i18n-aware `Link` from `@/i18n/navigation` throughout
  - Options considered: `next/link` Link with type assertions, i18n-aware Link from next-intl
  - Why this choice: next-intl's `createNavigation` provides a `Link` that automatically prefixes the current locale to hrefs. This avoids typed routes errors and ensures locale-aware navigation. All components using `<a>` or `next/link` for internal routes were updated.
  - Consequences / risks: Components now depend on `@/i18n/navigation`. This is the intended pattern for next-intl apps.

- **Decision:** Root layout is a pass-through, `[locale]/layout.tsx` has html/body
  - Options considered: html/body in root layout, pass-through root layout
  - Why this choice: Standard next-intl pattern. The locale layout needs access to the locale param for `<html lang={locale}>` and `setRequestLocale()`.

## Implementation Notes
- Key changes: next-intl config, proxy, locale routing, message files, locale-aware layout
- Edge cases handled: `<a>` tags and `next/link` replaced with i18n-aware Link throughout codebase
- Known limitations: Message files have minimal placeholder content only (full extraction in task 0005)

## Commands Run
```bash
npm install next-intl           # v4.8.2
npm run lint                    # 0 warnings
npm run build                   # success, routes generated for /en and /es
```

## Files Changed
- `apps/web/next.config.ts` (modify) — Added createNextIntlPlugin wrapper
- `apps/web/src/i18n/routing.ts` (create) — Defines locales ['en', 'es'] with 'en' default
- `apps/web/src/i18n/request.ts` (create) — getRequestConfig loading messages from JSON
- `apps/web/src/i18n/navigation.ts` (create) — createNavigation exports (Link, redirect, usePathname, useRouter, getPathname)
- `apps/web/src/proxy.ts` (create) — next-intl middleware for locale detection/routing
- `apps/web/messages/en.json` (create) — EN messages with common/auth/orgs/errors namespaces
- `apps/web/messages/es.json` (create) — ES messages with same structure
- `apps/web/src/app/layout.tsx` (modify) — Minimal pass-through root layout
- `apps/web/src/app/[locale]/layout.tsx` (create) — Full locale layout with html/body, NextIntlClientProvider, AppHeader
- `apps/web/src/app/[locale]/page.tsx` (move) — Home page
- `apps/web/src/app/[locale]/login/page.tsx` (move) — Login page
- `apps/web/src/app/[locale]/register/page.tsx` (move) — Register page
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` (move+modify) — My orgs page, updated Link import
- `apps/web/src/app/[locale]/orgs/accept-invite/page.tsx` (move) — Accept invite page
- `apps/web/src/app/[locale]/orgs/[orgId]/members/page.tsx` (move) — Members page
- `apps/web/src/components/auth/LoginForm.tsx` (modify) — `<a>` → i18n-aware `<Link>`
- `apps/web/src/components/auth/RegisterForm.tsx` (modify) — `<a>` → i18n-aware `<Link>`
- `apps/web/src/components/orgs/AcceptInvite.tsx` (modify) — `<a>` → i18n-aware `<Link>`
- `apps/web/src/components/layout/AppHeader.tsx` (modify) — `next/link` → i18n-aware `<Link>`
- `apps/web/package.json` (modify) — Added next-intl dependency

## Tests
### Manual
- `npm run build` — Routes generated: /en, /es, /en/login, /es/login, /en/register, /es/register, etc.
- `npm run lint` — 0 warnings
- Proxy middleware detected and active

## Follow-ups / Backlog
- Task 0005: Extract all frontend strings to EN locale messages
- Task 0006: Add ES translations and language switcher UI

## Checklist
- [x] Task scope matches `docs/tasks/0003-next-intl-setup.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
