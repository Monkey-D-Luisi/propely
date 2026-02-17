# Task: 0071 - Product Rebranding

## Metadata
- ID: 0071
- Type: Chore
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #276
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Rename the product from the generic "SaaS Template" to a definitive product name, centralizing the brand in a single configuration point so future rebranding is trivial.

## Context
"SaaS Template" sounds generic and fails to differentiate in a competitive market against ShipFast, SaaS Pegasus, and Next.js SaaS Boilerplates. The pre-sale audit recommends a distinctive name and centralized brand configuration. The new name must be decided with the product owner before implementation.

### Current Brand References (non-exhaustive)
- `apps/web/package.json` — name field
- `apps/web/src/app/[locale]/page.tsx` — landing page title/hero
- `docker-compose.yml` — service labels/names
- Email templates — product name in subject/body
- `README.md` — header, description
- `LICENSE` / `EULA` — product name
- Terraform descriptions
- CI workflow names
- HTML meta tags (title, og:site_name)
- i18n strings (`en.json`, `es.json`)

## Scope
### In scope
- Create centralized brand config (e.g., `apps/web/src/config/brand.ts` for frontend, shared constant for backend)
- Rename all user-visible references to the new product name
- Update landing page hero, meta tags, email templates, README, LICENSE, EULA
- Update Docker Compose service labels, CI workflow display names
- Update Terraform resource descriptions
- Update i18n strings that contain the product name

### Out of scope
- Renaming .NET namespaces or project files (`SaasTemplate.*` stays for code stability)
- Renaming the GitHub repository
- Changing database names
- Creating a logo (separate design task)

## Requirements
- R1: Product name centralized in a single source of truth per stack
- R2: All user-visible references updated
- R3: .NET internal namespaces explicitly NOT renamed (document this decision)
- R4: Running `grep -ri "saas template"` should return zero user-facing hits (code namespaces excluded)

## Acceptance Criteria
- AC1: Brand config file(s) created with name, tagline, URLs
- AC2: Landing page shows new product name
- AC3: Email templates use new product name
- AC4: README header uses new product name
- AC5: Meta tags updated
- AC6: `grep -ri "saas template"` only returns code namespace hits, not user-facing text
- AC7: Documentation explains the intentional namespace retention

## Implementation Steps
1. Ask product owner for definitive product name and tagline
2. Create `apps/web/src/config/brand.ts` with centralized brand constants
3. Update landing page to use brand config
4. Update email templates (Razor views) to use brand constant
5. Update `README.md` header and description
6. Update `LICENSE` and `EULA` product name references
7. Update `package.json` name field
8. Update Docker Compose service labels
9. Update CI workflow display names
10. Update Terraform resource descriptions
11. Update i18n strings containing product name
12. Update HTML meta tags (title, og:*)
13. Full-repo grep to verify no user-facing "SaaS Template" remains
14. Document namespace retention decision

## Files to Create
- `apps/web/src/config/brand.ts`
- `docs/walkthroughs/0071-product-rebranding.md`

## Files to Modify
- `apps/web/package.json`
- `apps/web/src/app/[locale]/page.tsx`
- `apps/web/src/app/layout.tsx` (or equivalent for meta tags)
- `apps/web/src/messages/en.json`
- `apps/web/src/messages/es.json`
- `docker-compose.yml`
- `README.md`
- `LICENSE`
- Email template files
- Terraform description fields
- CI workflow files

## Definition of Done Checklist
- [x] Brand config centralized
- [x] All user-facing references updated
- [x] Landing page shows new name
- [x] Email templates updated
- [x] README updated
- [x] Meta tags updated
- [x] Namespace retention documented
- [x] Walkthrough updated
