# Task: 0002-rename-saastemplate-to-propely

## Metadata
- ID: 0002
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/0002-rename-saastemplate-to-propely.md`
  - Epic: `docs/backlog/epic-P0-foundation.md` (Task 0.2)

## Goal
Perform a comprehensive rename of all SaasTemplate references throughout the entire codebase to Propely. This includes .NET namespaces, solution files, project files, Docker artifacts, database names, scripts, environment variables, and frontend brand configuration.

## Context
The codebase was bootstrapped from a SaaS Starter Kit template and still carries the `SaasTemplate.*` naming throughout. All documentation was already updated in Task 0.1, but the code itself still uses the old naming. This rename is required before any new services can be scaffolded (Task 0.3) to avoid namespace confusion.

## Scope
### In scope
- All .NET namespace renames: `SaasTemplate.*` to `Propely.*`
- Solution file renames: `SaasTemplate.AiApi.sln` to `Propely.AiApi.sln`, `SaasTemplate.OrgsApi.sln` to `Propely.OrgsApi.sln`
- All `.csproj` file renames and internal references
- All `using` statements and `namespace` declarations in `.cs` files
- Docker Compose service names and image references (`saastemplate-*` to `propely-*`)
- Database names in `infra/postgres/init/00-init-databases.sql`: `saastemplate_*` to `propely_*`
- `.env` and `.env.example` updates
- All scripts: run scripts, dev scripts, seed scripts
- CI workflow references in `.github/workflows/`
- Frontend brand references (`apps/web/`)
- EF Core migration snapshots and designer files (namespace references)
- Terraform infrastructure files
- Razor email templates
- `.http` test files

### Out of scope
- Adding new services (Task 0.3)
- Changing architecture or functionality
- Changing port allocations

## Requirements
- R1: Zero references to "SaasTemplate" or "saastemplate" (case-insensitive) in any source file after completion
- R2: All existing builds (`dotnet build`) must pass
- R3: All existing tests (`dotnet test`) must pass
- R4: Frontend build (`npm run build`) must pass
- R5: Docker Compose configuration must validate

## Acceptance Criteria
- AC1: `grep -ri "SaasTemplate"` returns zero results in source files (excluding .git, node_modules, bin, obj)
- AC2: `dotnet build services/ai-api/Propely.AiApi.sln` succeeds
- AC3: `dotnet build services/orgs-api/Propely.OrgsApi.sln` succeeds
- AC4: `dotnet test services/ai-api/Propely.AiApi.sln` passes all tests
- AC5: `dotnet test services/orgs-api/Propely.OrgsApi.sln` passes all tests
- AC6: `cd apps/web && npm run build` succeeds
- AC7: All run scripts reference correct solution/project paths
- AC8: Database init script creates `propely_*` databases
- AC9: Docker Compose validates (`docker compose config`)
- AC10: `grep -ri "saastemplate"` (case-insensitive) returns zero results in source files

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Rename directories (git mv) for both ai-api and orgs-api project folders
2. Rename individual files (.sln, .csproj, .http) within renamed directories
3. Bulk find-and-replace `SaasTemplate` with `Propely` in all file contents
4. Bulk find-and-replace `saastemplate` with `propely` (lowercase) in Docker, scripts, SQL, env files
5. Build and test to verify

## Implementation Steps
1. Inventory all occurrences (completed: ~572 files, ~2,076 occurrences)
2. Rename .sln files
3. Rename project directories (src/ and tests/ for both services)
4. Rename .csproj and .http files within directories
5. Update .sln file contents (project references)
6. Update .csproj file contents (project references)
7. Update all .cs files (namespaces, usings)
8. Update EF Core migration artifacts
9. Update Docker Compose files
10. Update database init SQL
11. Update .env and .env.example
12. Update scripts (run, dev, seed)
13. Update CI workflows
14. Update Terraform files
15. Update Razor email templates
16. Update frontend brand references
17. Update .http test files
18. Update appsettings.json files
19. Full verification: build, test, grep

## Files to Create / Modify
- **Rename:** 16 directories, 18 files (see inventory)
- **Modify:** ~572 source files with content replacements

## Testing Plan
- Unit tests: `dotnet test` both solutions
- Integration tests: `dotnet test` both solutions
- Manual verification: `grep -ri "saastemplate"` returns zero hits, `dotnet build` succeeds for both solutions

## Security & Privacy
- Ensure `.env.example` does not contain real secrets
- Verify no credentials are accidentally committed during the rename process

## Observability
- N/A (rename-only task)

## Rollback Plan
Git revert the entire commit. Since this is a rename-only operation, reverting is straightforward.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
