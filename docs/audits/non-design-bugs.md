# Non-Design Bugs Found During Puppeteer Audit

**Date**: 2026-02-12
**Discovered during**: Design system Puppeteer audit (see `design-system-puppeteer-audit.md`)
**Verified with**: `jaenster/puppeteer-mcp-claude` (headless:false, 1440x900)

---

## BUG-1: All `[orgId]` Routes Return 404

**Severity**: CRITICAL
**Type**: Routing / Middleware
**Screenshots**: `screenshots/15-org-members.png`, `screenshots/16-org-settings.png`

### Description

All three dynamic routes under `/orgs/[orgId]/` return the **default Next.js 404** page instead of rendering the expected content.

### Affected Routes

| Route | Expected | Actual |
|-------|----------|--------|
| `/en/orgs/{uuid}/members` | Members management page | Next.js default 404 |
| `/en/orgs/{uuid}/settings` | Org settings page | Next.js default 404 |
| `/en/orgs/{uuid}/billing` | Billing management page | Next.js default 404 |

### Evidence

- The page files exist in the codebase at `app/[locale]/orgs/[orgId]/members/page.tsx`, `settings/page.tsx`, `billing/page.tsx`
- The My Organizations page (`/orgs/mine`) generates correct links to these routes (e.g., `/en/orgs/1751a481-4485-4c72-a797-1bcbda7c8f86/members`)
- Clicking these links results in a 404

### Probable Cause

The `next-intl` middleware or routing configuration is not handling the nested dynamic segment `[orgId]` properly. The `[locale]` segment is handled, but the `[orgId]` segment under `/orgs/` fails to resolve.

### Impact

- Core application functionality (org management) is completely broken
- Members, Settings, and Billing pages are unreachable from the UI
- Org cards on My Organizations page link to dead routes

---

## BUG-2: Accept Invite Route Returns 404

**Severity**: HIGH
**Type**: Routing / Middleware
**Screenshot**: `screenshots/17-accept-invite.png`

### Description

The `/en/accept-invite?token=...` route returns the default Next.js 404 instead of the accept invite page.

### Evidence

- Page file exists at `app/[locale]/accept-invite/page.tsx`
- Navigating to `/en/accept-invite?token=fake-token` returns default Next.js 404
- May share the same root cause as BUG-1 (middleware/routing misconfiguration)

### Impact

- Users cannot accept team invitations via invite links
- Invitation flow is completely broken

---

## BUG-3: Custom 404 Page Never Renders

**Severity**: MEDIUM
**Type**: Next.js Configuration
**Screenshots**: `screenshots/08-404.png`, `screenshots/18-custom-404.png`

### Description

A custom `not-found.tsx` exists in the codebase with a branded 404 page (badge, title, subtitle, "Go home" button). However, navigating to any non-existent URL always shows the **default Next.js 404** ("404 | This page could not be found.") instead of the custom design.

### Evidence

- `app/[locale]/not-found.tsx` exists with full custom design
- Tested with `/en/this-page-does-not-exist` — shows default Next.js 404
- Tested with multiple non-existent routes — same result

### Probable Cause

The `not-found.tsx` file may not be placed at the correct level in the App Router hierarchy, or the `next-intl` routing configuration prevents it from being used. In Next.js App Router, the `not-found.tsx` must be at the correct segment level to catch unmatched routes.

### Impact

- Users see an unstyled, unbranded 404 page instead of the designed experience
- The custom 404 page's "Go home" navigation is not available to lost users

---

## BUG-4: Next.js Development Issue Indicator

**Severity**: LOW
**Type**: Development Environment
**Screenshots**: `screenshots/12-profile.png`

### Description

A red "1 Issue" indicator appears at the bottom-left of the Profile page (and potentially other pages). This is the Next.js development mode error overlay indicator.

### Evidence

- Visible in `screenshots/12-profile.png` as a red badge "N 1 Issue x"
- Only appears in development mode (`next dev`)

### Impact

- No production impact, but indicates an unresolved console error or warning in development
- Should be investigated to identify the underlying issue
