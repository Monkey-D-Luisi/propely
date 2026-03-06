# Walkthrough: FT-0001 — Stitch Design Audit & Migration

## Summary

Complete audit and migration of all Propely screen designs from the wrong Stitch project (`16786124142182555397`) to the correct one (`11823747923864658899`). Generated 43 detailed screen designs covering every Next.js page, including a Design System Reference Sheet for consistency.

## Problem

After the P2 epic project restructuring, all Stitch designs were in a project that didn't belong to this development. The user couldn't access the designs, and no HTML references existed in the repository for the correct project.

## Approach

1. **Inventory** — Scanned all 36 `page.tsx` files in `apps/web/src/app/[locale]/` to build a complete screen inventory
2. **Design System First** — Created a Design System Reference Sheet as the first screen, establishing exact Propely tokens (primary-600=#4f46e5, bg-surface=#f6f6f8, Inter font, rounded-xl cards, etc.)
3. **Batch Generation** — Generated 39+ screens in logical groups (auth, marketing, org, properties, contacts, work items, agencies, admin, errors) with extremely detailed prompts referencing the design system
4. **HTML Download** — Downloaded all 43 HTML files to `.stitch-html/` for pixel-perfect implementation reference
5. **Cleanup** — Removed old screenshots from wrong project, updated CLAUDE.md and agent memory

## Decisions

| Decision | Rationale |
|----------|-----------|
| Single Stitch project | User explicitly stated project `16786124142182555397` doesn't belong to this development |
| Design System Reference Sheet | Ensures consistency across all screens by establishing tokens first |
| Detailed prompts with exact CSS classes | Maximizes design fidelity and reduces implementation guesswork |
| Download all HTML to repo | Enables offline reference and pixel-perfect implementation |

## Files Changed

| File | Change |
|------|--------|
| `CLAUDE.md` | Updated Stitch project references (removed old project, single `11823747923864658899`) |
| `.stitch-html/*.html` (43 files) | HTML design references for all screens |

## Screen-to-Page Mapping

| HTML File | Next.js Page |
|-----------|-------------|
| `login.html` | `(public)/(auth)/login` |
| `register.html` | `(public)/(auth)/register` |
| `forgot-password.html` | `(public)/(auth)/forgot-password` |
| `forgot-password-success.html` | `(public)/(auth)/forgot-password` (success state) |
| `reset-password.html` | `(public)/(auth)/reset-password` |
| `verify-email-loading.html` | `(public)/(auth)/verify-email` (loading) |
| `verify-email-success.html` | `(public)/(auth)/verify-email` (success) |
| `verify-email-error.html` | `(public)/(auth)/verify-email` (error) |
| `landing.html` | `(public)/(marketing)` |
| `pricing.html` | `(public)/(marketing)/pricing` |
| `privacy.html` | `(public)/(marketing)/privacy` |
| `terms.html` | `(public)/(marketing)/terms` |
| `orgs-mine.html` | `(protected)/orgs/mine` |
| `org-settings.html` | `(protected)/orgs/[orgId]/settings` |
| `org-members.html` | `(protected)/orgs/[orgId]/members` |
| `org-billing.html` | `(protected)/orgs/[orgId]/billing` |
| `permissions.html` | `(protected)/orgs/[orgId]/permissions` |
| `permission-detail.html` | `(protected)/orgs/[orgId]/permissions/[userId]` |
| `accept-invite-loading.html` | `(protected)/orgs/accept-invite` (loading) |
| `accept-invite-success.html` | `(protected)/orgs/accept-invite` (success) |
| `accept-invite-error.html` | `(protected)/orgs/accept-invite` (error) |
| `properties-list.html` | `(protected)/properties` |
| `property-detail.html` | `(protected)/properties/[id]` |
| `property-new.html` | `(protected)/properties/new` |
| `property-edit.html` | `(protected)/properties/[id]/edit` |
| `contacts.html` | `(protected)/contacts` |
| `contact-detail.html` | `(protected)/contacts/[id]` |
| `leads.html` | `(protected)/leads` |
| `appointments.html` | `(protected)/appointments` |
| `work-items.html` | `(protected)/work-items` |
| `work-item-detail.html` | `(protected)/work-items/[id]` |
| `work-item-new.html` | `(protected)/work-items/new` |
| `work-item-edit.html` | `(protected)/work-items/[id]/edit` |
| `agency-new.html` | `(protected)/agencies/new` |
| `agency-detail.html` | `(protected)/agencies/[id]` |
| `agency-settings.html` | `(protected)/agencies/[id]/settings` |
| `profile.html` | `(protected)/profile` |
| `feature-flags.html` | `(protected)/admin/feature-flags` |
| `audit-logs.html` | `(protected)/admin/audit-logs` |
| `admin-version.html` | `(protected)/admin/version` |
| `not-found.html` | `not-found` |
| `error.html` | `error` |
| `design-system.html` | (reference, no page) |

## Follow-ups

- Visual verification with Puppeteer comparing each app page against its Stitch design
- Pixel-perfect implementation adjustments based on new designs
