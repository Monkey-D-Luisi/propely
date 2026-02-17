# Walkthrough: 0057-developer-cookbook

## Task Reference
- Task: `docs/tasks/0057-developer-cookbook.md`
- Walkthrough: `docs/walkthroughs/0057-developer-cookbook.md`
- Branch/PR: `feat/0057-developer-cookbook`
- Date: `2026-02-14`

## Summary
Created a developer cookbook at `docs/cookbook.md` with 8 step-by-step recipes for common extension scenarios. Each recipe references actual codebase file paths and follows project conventions. Recipes cover both backend (.NET Clean Architecture + CQRS) and frontend (Next.js + next-intl) patterns.

## Context
- Background: Developers buying this SaaS template need clear instructions for extending it. Without recipes, they would need to reverse-engineer patterns from the existing WorkItem, Organization, and FeatureFlag implementations.
- Problem statement: No cookbook or extension guide existed. Developers had to study existing code to understand the entity → configuration → repository → command → controller → frontend page flow.
- Constraints: Documentation-only task. All file paths must reference actual codebase locations. Code snippets must follow project conventions (sealed classes, private setters, static factories, snake_case EF Core, CQRS split, etc.).

## Decisions & Trade-offs
- **Decision:** Used a fictional "Invoice" entity as the running example across recipes
  - Why: Avoids confusion with existing entities (WorkItem, Organization). Invoice is a realistic SaaS concept that readers can relate to.
  - Consequences: Examples are cohesive across recipes — Recipe 1 creates the entity, Recipe 2 adds commands, Recipe 3 exposes the API, Recipe 4 builds the frontend.

- **Decision:** Included a Quick Reference folder structure section at the end
  - Why: Developers extending the template need a single reference for where files go. Consolidating the naming conventions into one section avoids repetition across recipes.

- **Decision:** Kept each recipe self-contained but referenced other recipes for prerequisites
  - Why: Developers may jump directly to the recipe they need (e.g., Recipe 7 for feature flags) without reading the full document.

## Implementation Notes
- Key changes: Created `docs/cookbook.md` with 8 recipes + architecture overview + folder reference
- Recipes written:
  1. Add a New Domain Entity (entity, EF config, migration, repository, DI)
  2. Add a New CQRS Command (command, handler, query pattern)
  3. Add a New API Endpoint (controller, DTOs, validator, HTTP conventions)
  4. Add a New Frontend Page (route, page component, hook, i18n, tests)
  5. Scaffold a New Module (scaffold-module.ps1/sh walkthrough)
  6. Add a New Locale (routing config, message files, email templates)
  7. Configure a Feature Flag (defaults, FeatureGate component, toggle hook)
  8. Add a New Email Template (model, Razor template, localization, IEmailService)
- All code snippets verified against actual codebase patterns (WorkItem entity, WorkItemsController, feature-flags.tsx, InvitationEmailModel, etc.)

## Data / Schema / Migrations
- No DB changes
- N/A

## Commands Run
```bash
bash scripts/verify-license-headers.sh   # All files pass
```

## Files Changed
- `docs/cookbook.md` — New file: developer cookbook with 8 extension recipes
- `docs/backlog/epic-010-docs-onboarding.md` — Task 0057 status: PENDING → DONE
- `docs/tasks/0057-developer-cookbook.md` — Status: PENDING → DONE, DoD boxes checked
- `docs/roadmap-v1.md` — Task 0057 status: PENDING → DONE

## Tests
### Unit
- N/A (documentation-only task)

### Integration
- N/A

### Manual
- Verified all file paths referenced in recipes exist in the codebase
- Verified code snippets follow actual project conventions (entity pattern, CQRS pattern, controller pattern, hook pattern)
- Verified scaffold-module.ps1 script exists and its output matches the recipe description
- Verified i18n routing configuration matches the recipe
- Verified feature flag pattern matches actual FeatureGate component and hooks
- Verified email template pattern matches actual BaseEmailModel hierarchy and Infrastructure template structure
- Cross-links verified: getting-started.md, configuration.md

## Observability
- N/A (no runtime changes)

## Security
- No code changes
- No secrets in documentation — example code uses placeholder values only

## Follow-ups / Backlog
- [ ] Add Recipe 9: Add a new background worker (OutboxDispatcher/Projector pattern)
- [ ] Add Recipe 10: Add a new infrastructure integration (Redis/RabbitMQ pattern)
- [ ] Add diagrams showing the layer-by-layer data flow for each recipe

## Checklist
- [x] Task scope matches `docs/tasks/0057-developer-cookbook.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
