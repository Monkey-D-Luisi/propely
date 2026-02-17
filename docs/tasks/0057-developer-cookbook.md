# Task: 0057 - Developer Cookbook / Extension Recipes

## Metadata
- ID: 0057
- Type: Documentation
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #209
- Epic: `docs/backlog/epic-010-docs-onboarding.md`
- Old Issue: #43
- Milestone: v1.0

## Goal
Write a developer cookbook with step-by-step recipes for common extension scenarios: adding entities, endpoints, pages, modules, and configuring features.

## Context
Developers using this template will need to extend it for their own SaaS product. Without clear recipes, they'll need to reverse-engineer patterns from existing code. A cookbook with practical recipes accelerates this process.

### Existing Patterns to Document
- Clean Architecture with CQRS (MediatR)
- Entity + Configuration + Repository + Command/Query pattern
- scaffold-module.ps1 script for new modules
- Frontend: hooks + components + pages pattern
- i18n: adding new locale or new strings
- Feature flags: adding new flags

## Scope
### In scope
- Recipe: Add a new domain entity (full stack)
- Recipe: Add a new CQRS command/query
- Recipe: Add a new API endpoint
- Recipe: Add a new frontend page
- Recipe: Add a new module (using scaffold script)
- Recipe: Add a new locale
- Recipe: Configure a feature flag
- Recipe: Add a new email template

### Out of scope
- Architecture deep-dive / theory
- Contributing guide
- API reference documentation

## Requirements
- R1: Each recipe is self-contained with step-by-step instructions
- R2: Recipes reference actual file paths in the codebase
- R3: Code snippets are accurate and follow project conventions
- R4: Recipes cover both backend and frontend where applicable

## Acceptance Criteria
- AC1: Document exists at `docs/cookbook.md`
- AC2: At least 8 recipes covering common extension scenarios
- AC3: Each recipe has numbered steps with code snippets
- AC4: File paths reference actual codebase locations

## Implementation Steps

1. **Write intro**: Explain the cookbook purpose and project architecture overview
2. **Recipe 1**: Add a new domain entity (entity, config, migration, repository, command, controller, frontend)
3. **Recipe 2**: Add a new CQRS command (command, handler, validator, registration)
4. **Recipe 3**: Add a new API endpoint (controller method, DTO, routing)
5. **Recipe 4**: Add a new frontend page (route, page component, hooks, i18n)
6. **Recipe 5**: Add a new module (scaffold-module.ps1 walkthrough)
7. **Recipe 6**: Add a new locale (message files, next-intl config)
8. **Recipe 7**: Configure a feature flag (backend config, frontend gate)
9. **Recipe 8**: Add a new email template (model, Razor template, service method)

## Files to Create

- `docs/cookbook.md`
- `docs/walkthroughs/0057-developer-cookbook.md`

## Definition of Done Checklist
- [x] At least 8 recipes written
- [x] Recipes use real file paths
- [x] Code snippets follow project conventions
- [x] Walkthrough updated
