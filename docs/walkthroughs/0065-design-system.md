# Walkthrough: 0065 - Design System Definition (Stitch + Tailwind)

## Overview

Established a pixel-perfect design system for the SaaS template using Google Stitch MCP as the design source of truth and Tailwind CSS v4's `@theme` directive for token definition. Every application screen was aligned to match its corresponding Stitch design. The design follows a professional SaaS aesthetic (Stripe/Linear) with an indigo/violet primary palette.

## Design Tool: Google Stitch MCP

All screen designs are maintained in a Stitch project that serves as the single source of truth for visual design.

- **Stitch Project ID**: `16786124142182555397`
- **URL**: https://stitch.withgoogle.com (open the project by ID)

### Stitch Convention

> **IMPORTANT**: Always use `modelId: GEMINI_3_PRO` when generating or editing screens via the Stitch MCP. This ensures consistent, high-quality output. Do NOT use Flash for final designs.

### Screen Inventory

| App Screen | Stitch Screen Title | Status |
|------------|-------------------|--------|
| Login | SaaS Template Login Page | Aligned |
| Register | SaaS Template Registration Page | Aligned |
| Forgot Password | Forgot Password Entry State | Aligned |
| Forgot Password (success) | Forgot Password Success State | Aligned |
| Reset Password | Reset Password Page | Aligned |
| Email Verification (loading) | Email Verification Loading State | Aligned |
| Email Verification (success) | Email Verification Success | Aligned |
| Email Verification (error) | Email Verification Error State | Aligned |
| Landing/Home | SaaS Template Landing Page | Aligned |
| My Organizations | Organization Management Dashboard | Aligned |
| Org Members | Organization Members Management | Aligned |
| Org Settings | Organization Settings and Team Management | Aligned |
| Org Billing | Organization Billing and Subscriptions | Aligned |
| User Profile | User Profile Settings Dashboard | Aligned |
| Feature Flags | Feature Flags Management Dashboard | Aligned |
| Audit Logs | Audit Logs Admin Page | Aligned |
| Pricing | SaaS Pricing Plans Overview | Aligned |
| Accept Invite | Accept Invitation Page | Aligned |
| Error 404 | 404 Page Not Found | Existing |
| Runtime Error | Runtime Error Page | Existing |

## Design Tokens

### Architecture Decision

Tokens are defined in `apps/web/src/app/globals.css` using Tailwind v4's `@theme` directive. This generates utility classes like `bg-primary-600`, `text-surface`, etc. — no `tailwind.config.ts` file is needed.

**Single source of truth**: changing the `@theme` block in `globals.css` re-themes every component automatically.

### Primary Color

Stitch's chosen primary: `#5048e5` (indigo/violet).

```css
@theme {
  --color-primary-600: #5048e5;   /* ← main brand color, used by Stitch */
  /* Full scale from primary-50 to primary-950 also defined */
}
```

### Semantic Surface Colors

```css
@theme {
  --color-surface:      #f6f6f8;  /* page background (replaces bg-slate-50) */
  --color-surface-dark: #121121;  /* dark mode background */
}
```

### Border Radius Overrides

Stitch uses larger border radii than Tailwind defaults:

```css
@theme {
  --radius:    0.5rem;  /* DEFAULT — used by rounded */
  --radius-lg: 1rem;    /* used by rounded-lg (inputs, buttons) */
  --radius-xl: 1.5rem;  /* used by rounded-xl (cards, containers) */
}
```

### Neutral Scale

Uses Tailwind's built-in `slate` scale (not customized):
- **Page background**: `bg-surface` (semantic) or `bg-slate-50`
- **Card surfaces**: `bg-white`
- **Primary text**: `text-slate-900`
- **Secondary text**: `text-slate-600`
- **Tertiary/subtle text**: `text-slate-500`
- **Card borders**: `border-slate-200`
- **Input borders**: `border-slate-200` (changed from slate-300)

### Typography

- **Font**: Inter (sans-serif), loaded via Google Fonts
- **Page title**: `text-3xl font-bold tracking-tight text-slate-900`
- **Hero title**: `text-5xl md:text-6xl font-extrabold tracking-tight`
- **Section heading**: `text-lg font-semibold text-slate-900`
- **Subtitle**: `text-sm text-slate-500`
- **Body**: default size, `text-slate-600`

### Layout Widths

| Context | Max Width |
|---------|-----------|
| Auth pages | `max-w-md` |
| Accept invite | `max-w-md` |
| Dashboard/org list | `max-w-4xl` |
| Detail pages (members, settings, billing, profile, admin) | `max-w-5xl` |
| Header nav | `max-w-7xl` |
| Pricing page outer | `max-w-7xl` |
| Landing page outer | `max-w-7xl` |

## Component Patterns

### Button (Primary)

```
bg-primary-600 hover:bg-primary-600/90 text-white
rounded-lg shadow-sm px-4 py-2
active:scale-[0.98]
focus:ring-2 focus:ring-primary-600 focus:ring-offset-2
```

Key change from previous: hover uses opacity-based (`/90`) instead of a darker shade (`primary-700`).

### Button (Secondary/Outlined)

```
border border-slate-200 bg-white text-slate-700
hover:bg-slate-50
focus:ring-2 focus:ring-primary-600/20
rounded-lg
```

### Auth Submit Button

Taller, more prominent for auth forms:

```
w-full py-3 shadow-lg shadow-primary-600/25
bg-primary-600 hover:bg-primary-600/90 text-white rounded-lg
```

### Input Fields

```
border border-slate-200 rounded-lg px-3 py-2
focus:border-transparent focus:ring-2 focus:ring-primary-600
```

Key change: focus ring uses `primary-600` (not `primary-100`), border becomes transparent on focus.

### Focus Rings

| Element | Pattern |
|---------|---------|
| Primary buttons | `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2` |
| Secondary buttons | `focus:ring-2 focus:ring-primary-600/20 focus:ring-offset-2` |
| Input fields | `focus:ring-2 focus:ring-primary-600 focus:border-transparent` |
| Expand/toggle buttons | `focus:ring-2 focus:ring-primary-600` |

### Cards (Dashboard)

```
overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm
```

Inner padding: `p-6 md:p-8`

Section header inside card:
```
mb-8 border-b border-slate-100 pb-6
  h2: text-lg font-semibold text-slate-900
  p: mt-1 text-sm text-slate-500
```

Footer/actions inside card:
```
flex items-center justify-end border-t border-slate-100 pt-4
```

### Cards (Auth)

```
rounded-2xl bg-white p-8 shadow-xl
```

No border in light mode. Uses `shadow-xl` instead of `shadow-sm`.

### Data Tables

Container:
```
overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm
```

Header:
```
bg-slate-50/50
th: px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500
```

Body rows:
```
group hover:bg-slate-50 transition-colors
td: px-6 py-5
```

Action buttons in rows:
```
opacity-0 group-hover:opacity-100 transition-opacity
```

### Role Badges

```
inline-flex items-center rounded-full px-2.5 py-1
text-xs font-semibold uppercase tracking-wider
```

| Role | Style |
|------|-------|
| Owner | `bg-primary-600/10 text-primary-600` |
| Admin | `bg-slate-100 text-slate-600` |
| Member | `bg-slate-100 text-slate-600` |
| Viewer | `bg-emerald-100 text-emerald-600` |

### Pagination

```
mt-12 pt-8
buttons: rounded-lg border border-slate-200 text-slate-500
text: text-sm text-slate-500
```

### Navigation Bar (AppHeader)

```
sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-slate-200
max-w-7xl mx-auto
```

Logo: `w-8 h-8 bg-primary-600 rounded-lg` with `layers` material icon.
Brand: `font-bold text-lg tracking-tight`.

### Landing Page Hero

```
Pill badge: rounded-full bg-indigo-50 border border-indigo-100
Title: text-5xl md:text-6xl font-extrabold tracking-tight
  Gradient span: bg-gradient-to-r from-primary-600 to-purple-600 bg-clip-text text-transparent
Subtitle: text-xl text-slate-600 leading-relaxed max-w-2xl
CTA primary: px-8 py-3.5 bg-primary-600 shadow-lg shadow-primary-600/25 hover:-translate-y-0.5
CTA secondary: px-8 py-3.5 border border-slate-300 bg-white
```

### Pricing Cards

Non-recommended:
```
flex flex-col p-8 rounded-xl border border-slate-200 bg-white shadow-sm
hover:shadow-md hover:-translate-y-1 transition-all duration-300
```

Recommended:
```
flex flex-col p-8 rounded-xl border-2 border-primary-600 bg-white shadow-xl z-10 relative
hover:-translate-y-1 transition-transform duration-300
Badge: absolute -top-4 left-1/2 -translate-x-1/2 rounded-full bg-primary-600 text-white uppercase tracking-wide
```

Feature list items use `material-symbols-outlined check_circle` in `text-emerald-500`.

### Accept Invite

Uses auth card pattern (centered, rounded-2xl, shadow-xl). Status icons in h-16 w-16 circles:
- Processing: `bg-primary-100 text-primary-600` with animated spinner
- Success: `bg-emerald-100 text-emerald-600` checkmark
- Error: `bg-red-100 text-red-600` cancel icon

### Auth Footer Links

Reusable `<AuthFooterLinks />` component below auth cards:
```
mt-8 text-center text-xs text-slate-400
Links: Privacy Policy · Terms of Service · Support
```

## Files Modified

### Core Design Tokens
- `apps/web/src/app/globals.css` — `@theme` block with primary-600 `#5048e5`, surface colors, border-radius overrides

### Shared UI Components
- `apps/web/src/components/ui/button.tsx` — hover `/90`, active:scale, focus ring primary-600
- `apps/web/src/components/ui/form/form-field.tsx` — focus border-transparent + ring-primary-600
- `apps/web/src/components/ui/pagination.tsx` — rounded-lg, border-slate-200, mt-12 pt-8 spacing
- `apps/web/src/components/auth/auth-layout.tsx` — bg-surface, no card border, AuthFooterLinks
- `apps/web/src/components/auth/OAuthButtons.tsx` — gap-4, focus ring update
- `apps/web/src/components/orgs/RoleBadge.tsx` — uppercase tracking-wider, no border

### Auth Screens
- `apps/web/src/components/auth/LoginForm.tsx` — py-3 submit, AuthFooterLinks
- `apps/web/src/components/auth/RegisterForm.tsx` — py-3 submit, AuthFooterLinks
- `apps/web/src/components/auth/ForgotPasswordForm.tsx` — py-3 submit, MailIcon, AuthFooterLinks
- `apps/web/src/components/auth/ResetPasswordForm.tsx` — py-3 submit, AuthFooterLinks
- `apps/web/src/components/auth/VerifyEmailContent.tsx` — h-16 w-16 icons, AuthFooterLinks

### Dashboard Screens
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` — primary-600/10, hover primary-600
- `apps/web/src/app/[locale]/profile/page.tsx` — back link, page title h1
- `apps/web/src/components/profile/ProfileForm.tsx` — card pattern, section header, grid layout
- `apps/web/src/components/profile/ChangePasswordForm.tsx` — card pattern, section header
- `apps/web/src/components/orgs/OrgSettingsForm.tsx` — rounded-xl, section headers, button alignment
- `apps/web/src/components/orgs/DeleteOrgSection.tsx` — rounded-xl, focus ring update
- `apps/web/src/components/orgs/MembersManager.tsx` — text-3xl heading, rounded-xl
- `apps/web/src/components/orgs/MembersTable.tsx` — table pattern (bg-slate-50/50, px-6, group hover)
- `apps/web/src/components/orgs/InviteForm.tsx` — rounded-xl, padding update
- `apps/web/src/components/billing/BillingManagement.tsx` — rounded-xl
- `apps/web/src/components/billing/PlanCard.tsx` — p-8, hover lift, material icon, absolute badge
- `apps/web/src/components/billing/PaymentHistory.tsx` — table pattern
- `apps/web/src/components/billing/PricingContent.tsx` — max-w-7xl, py-20, larger heading

### Admin Screens
- `apps/web/src/app/[locale]/admin/feature-flags/page.tsx` — text-3xl tracking-tight
- `apps/web/src/components/admin/FeatureFlagTable.tsx` — table pattern, toggle primary-600
- `apps/web/src/app/[locale]/admin/audit-logs/page.tsx` — text-3xl, border-slate-200
- `apps/web/src/components/admin/AuditLogTable.tsx` — table pattern (bg-slate-50/50, px-6 py-5)
- `apps/web/src/components/admin/AuditLogFilters.tsx` — card wrapper, focus ring, border-slate-200

### Public Screens
- `apps/web/src/app/[locale]/page.tsx` — full landing page rewrite (hero, features grid)
- `apps/web/src/app/[locale]/pricing/page.tsx` — container for PricingContent
- `apps/web/src/components/orgs/AcceptInvite.tsx` — card pattern, status icons, AuthFooterLinks

### Layout & Navigation
- `apps/web/src/app/[locale]/layout.tsx` — bg-surface
- `apps/web/src/components/layout/AppHeader.tsx` — sticky, backdrop-blur, logo icon, max-w-7xl

### i18n
- `apps/web/messages/en.json` — added `profile.pageTitle`, `profile.pageDescription`
- `apps/web/messages/es.json` — added `profile.pageTitle`, `profile.pageDescription`

## How to Re-Theme

To change the brand color:

1. Update the `@theme` block in `globals.css` with a new primary-600 value
2. All `primary-*` utility classes update automatically
3. Update the Stitch project to generate new designs with the new color
4. No component code changes needed (everything uses `primary-*` semantic classes)

## How to Add a New Screen

1. **Design first**: Use `generate_screen_from_text` with `modelId: GEMINI_3_PRO` to create the design in Stitch
2. **Download HTML**: Fetch the Stitch HTML to `.stitch-html/` for reference
3. **Implement**: Build the React component following the patterns above
4. **Verify**: Compare implementation against Stitch design

## Visual Verification

To verify screens match Stitch designs:

1. Start the dev server: `./scripts/dev-up.sh` or `.\scripts\dev-up.ps1`
2. Open http://localhost:3000 in a browser
3. Compare each screen against its Stitch design (screenshots available in the Stitch project)
4. Optionally use a Puppeteer MCP for automated screenshot comparison

## Testing

- `npm run build` — passes (Next.js 16 + Turbopack, all 16 routes compile)
- `npm test` — 326 tests across 32 test files, all pass
- Manual visual review of all screens against Stitch designs
