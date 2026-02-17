# Walkthrough: 0068 - Privacy Policy and Terms of Service Pages

## Summary

Created functional `/privacy` and `/terms` pages with professionally written legal content, fully translated in EN and ES. Both pages follow the existing design system and are accessible from the landing page and auth footer links that previously resulted in 404 errors.

## Decisions

- **Content in i18n**: All legal content is stored in the `legal` namespace in `en.json`/`es.json` rather than inline or in markdown files. This keeps the translation workflow consistent with the rest of the app.
- **Semicolon-delimited lists**: List items (GDPR rights, acceptable use items) are stored as semicolon-delimited strings in i18n and split at render time. This avoids deeply nested translation keys while keeping content translatable.
- **Shared layout pattern**: Both pages share a common layout structure (nav bar with logo + "Back to home", content card, footer) but are implemented as standalone pages rather than sharing a layout component, matching the existing landing page pattern.
- **Design system adherence**: Used `bg-surface` background, `rounded-xl` card with `border-slate-200` and `shadow-sm`, `max-w-4xl` content width, and the standard typography scale per CLAUDE.md design system rules.

## Files Changed

### New files
| File | Purpose |
|------|---------|
| `apps/web/src/app/[locale]/privacy/page.tsx` | Privacy policy page component |
| `apps/web/src/app/[locale]/terms/page.tsx` | Terms of service page component |

### Modified files
| File | Change |
|------|--------|
| `apps/web/messages/en.json` | Added `legal` namespace with privacy + terms content |
| `apps/web/messages/es.json` | Added `legal` namespace with Spanish translations |

## Content Coverage

### Privacy Policy sections
1. Introduction
2. Information We Collect (Account, Usage, Payment, AI Service data)
3. How We Use Your Information (7 bullet points)
4. Third-Party Services (Stripe, SendGrid, Google OAuth, GitHub OAuth, OpenAI)
5. Data Storage and Security (PostgreSQL, Redis, RabbitMQ, TLS)
6. Your Rights Under GDPR (6 rights: access, rectification, erasure, portability, restriction, objection)
7. Cookies (essential only, no tracking)
8. Data Retention (30-day deletion, 90-day anonymization)
9. Children's Privacy (16+ only)
10. Changes to This Policy
11. Contact Us

### Terms of Service sections
1. Agreement to Terms
2. Description of Service
3. Account Registration and Security
4. Acceptable Use Policy (8 prohibited activities)
5. Subscription and Billing (Stripe integration)
6. Intellectual Property
7. User Content
8. Third-Party Services
9. Disclaimer of Warranties (uppercase legal boilerplate)
10. Limitation of Liability (uppercase legal boilerplate)
11. Indemnification
12. Termination
13. Governing Law
14. Changes to Terms
15. Contact Us

## Stitch Designs

Stitch designs were generated in project `16786124142182555397` for both pages.

## Quality Checks

- [x] Next.js build: passed (both routes registered)
- [x] Frontend tests: 431 passed (46 test files)
- [x] TypeScript: no errors

## Checklist

- [x] Task scope matches docs/tasks/0068-privacy-terms-pages.md
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
