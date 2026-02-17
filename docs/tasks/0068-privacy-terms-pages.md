# Task: 0068 - Privacy Policy and Terms of Service Pages

## Metadata
- ID: 0068
- Type: Feature
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #273
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Create functional `/privacy` and `/terms` pages with professionally written, real legal content — eliminating the current 404 errors when clicking footer links.

## Context
The landing page footer links to `/privacy` and `/terms` but no page components exist. A buyer clicking these links sees a 404 — immediate impression of an unfinished product. i18n strings already exist in `en.json`/`es.json` for navigation labels but not for page content.

### Third-Party Services to Cover
- Stripe (payment processing)
- SendGrid (transactional email)
- Google OAuth (authentication)
- GitHub OAuth (authentication)
- OpenAI (AI-powered features)
- PostgreSQL/Redis/RabbitMQ (infrastructure — data storage)

## Scope
### In scope
- Create `apps/web/src/app/[locale]/privacy/page.tsx`
- Create `apps/web/src/app/[locale]/terms/page.tsx`
- Write professional legal content covering all third-party integrations
- GDPR/RGPD rights section (access, rectification, erasure, portability)
- Cookie policy section
- i18n support (EN/ES) with full translated content
- Stitch design for both pages
- Follow existing design system (typography, spacing, layout)

### Out of scope
- Cookie consent banner (separate future task)
- Legal review by an attorney (content is professional boilerplate)

## Requirements
- R1: Pages render without errors at `/privacy` and `/terms` for all locales
- R2: Content is professionally written with real legal substance
- R3: All third-party services are mentioned with data handling descriptions
- R4: GDPR rights section is accurate and complete
- R5: Both EN and ES translations are provided

## Acceptance Criteria
- AC1: `/privacy` renders with complete privacy policy
- AC2: `/terms` renders with complete terms of service
- AC3: Footer links no longer 404
- AC4: Both pages fully translated (EN/ES)
- AC5: Stitch design exists in project
- AC6: Pixel-perfect implementation matching Stitch design
- AC7: Responsive design (mobile/desktop)

## Implementation Steps
1. Generate Stitch design for privacy policy page
2. Generate Stitch design for terms of service page
3. Write privacy policy content (EN) covering all sections
4. Write terms of service content (EN) covering all sections
5. Translate both to ES
6. Add i18n strings to `en.json` and `es.json`
7. Create page components following Stitch designs
8. Verify footer links work
9. Visual verification against Stitch designs

## Files to Create
- `apps/web/src/app/[locale]/privacy/page.tsx`
- `apps/web/src/app/[locale]/terms/page.tsx`
- `.stitch-html/privacy.html`
- `.stitch-html/terms.html`
- `docs/walkthroughs/0068-privacy-terms-pages.md`

## Files to Modify
- `apps/web/src/messages/en.json` (add legal content strings)
- `apps/web/src/messages/es.json` (add legal content strings)

## Definition of Done Checklist
- [x] Privacy page renders at `/privacy`
- [x] Terms page renders at `/terms`
- [x] Professional content covering all third-party services
- [x] GDPR/RGPD section included
- [x] EN/ES translations complete
- [x] Stitch design created
- [x] Walkthrough updated
