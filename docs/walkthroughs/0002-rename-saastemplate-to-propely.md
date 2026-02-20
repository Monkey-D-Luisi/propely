# Walkthrough: 0002-rename-saastemplate-to-propely

## Task Reference
- Task: `docs/tasks/0002-rename-saastemplate-to-propely.md`
- Walkthrough: `docs/walkthroughs/0002-rename-saastemplate-to-propely.md`
- Branch/PR: `feat/0002-rename-saastemplate-to-propely` / TBD
- Date: `2026-02-20`

## Summary
Comprehensive rename of all SaasTemplate references to Propely across the entire codebase. Covered .NET namespaces (~495 .cs files), solution/project files (2 .sln + 12 .csproj), Docker artifacts (3 compose + 4 Dockerfiles), database names (1 SQL init), scripts (11 .ps1/.sh), CI workflows (3 .yml), Terraform (8 .tf), email templates (13 .cshtml), frontend brand (1 .tsx), appsettings (4 .json), and .http test files (2). Approximately 572 source files with ~2,076 occurrences updated. All builds and 773 tests pass.

## Context
- Background: Codebase was bootstrapped from a SaaS Starter Kit template and retained the `SaasTemplate.*` naming
- Problem statement: All code references need to be updated to `Propely.*` before new services can be scaffolded
- Constraints: Must not break existing builds, tests, or functionality

## Decisions & Trade-offs
- **Decision:** Use git mv for directory/file renames, then bulk sed for content replacement
  - Options considered: Manual file-by-file editing, IDE refactoring, automated script
  - Why this choice: Most efficient and less error-prone for a codebase-wide rename
  - Consequences / risks: None materialized; all tests pass

- **Decision:** Keep documentation references to "SaasTemplate" in task descriptions and walkthroughs
  - Options considered: Replace all .md references, keep task title references
  - Why this choice: Task documents describe the rename action and must reference the old name for traceability. Task document immutability rules apply to completed tasks.

## Implementation Notes
- Key changes:
  - Renamed 16 directories and 18 files via `git mv`
  - PascalCase replacement (`SaasTemplate` -> `Propely`) in all .cs, .csproj, .sln, .cshtml, .http files
  - Lowercase replacement (`saastemplate` -> `propely`) in Docker, SQL, .env, scripts, Terraform, .json files
  - 5 .cs files had lowercase `saastemplate` in connection strings and email addresses that required a second pass
- Edge cases handled:
  - DesignTimeDbContextFactory files had lowercase `saastemplate` in fallback connection strings
  - Email sender files had `no-reply@saastemplate.test` addresses
  - Terraform README had a note referencing the old naming that was updated
- Known limitations:
  - The `actions-runner/` directory (CI runner working copy) retains old naming; it will be updated on next CI run
  - Documentation files (.md) reference "SaasTemplate" in task titles and descriptions (by design)

## Data / Schema / Migrations
- DB changes: `infra/postgres/init/00-init-databases.sql` renamed databases from `saastemplate_aiapi`/`saastemplate_orgsapi` to `propely_aiapi`/`propely_orgsapi`
- Migration strategy: Existing EF Core migrations retain their timestamps; only namespace declarations and model snapshot references updated
- Backward compatibility: Requires `dev-reset` to recreate databases with new names

## Commands Run
```bash
# Directory renames (16 directories via git mv)
git mv services/ai-api/src/SaasTemplate.AiApi.Api services/ai-api/src/Propely.AiApi.Api
git mv services/ai-api/src/SaasTemplate.AiApi.Application services/ai-api/src/Propely.AiApi.Application
git mv services/ai-api/src/SaasTemplate.AiApi.Domain services/ai-api/src/Propely.AiApi.Domain
git mv services/ai-api/src/SaasTemplate.AiApi.Infrastructure services/ai-api/src/Propely.AiApi.Infrastructure
git mv services/ai-api/tests/SaasTemplate.AiApi.ArchitectureTests services/ai-api/tests/Propely.AiApi.ArchitectureTests
git mv services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests services/ai-api/tests/Propely.AiApi.IntegrationTests
git mv services/ai-api/tests/SaasTemplate.AiApi.UnitTests services/ai-api/tests/Propely.AiApi.UnitTests
# (same pattern for orgs-api)

# File renames (18 files: .sln, .csproj, .http)
git mv services/ai-api/SaasTemplate.AiApi.sln services/ai-api/Propely.AiApi.sln
git mv services/orgs-api/SaasTemplate.OrgsApi.sln services/orgs-api/Propely.OrgsApi.sln
# (12 .csproj + 2 .http files renamed similarly)

# Content replacement
find services -type f -name "*.cs" ! -path "*/bin/*" ! -path "*/obj/*" | xargs sed -i 's/SaasTemplate/Propely/g'
find services -type f -name "*.csproj" ! -path "*/bin/*" ! -path "*/obj/*" | xargs sed -i 's/SaasTemplate/Propely/g'
sed -i 's/SaasTemplate/Propely/g; s/saastemplate/propely/g' docker-compose.yml docker-compose.production.yml docker-compose.ci.yml
sed -i 's/SaasTemplate/Propely/g; s/saastemplate/propely/g' infra/postgres/init/00-init-databases.sql
# (similar for .env, scripts, CI workflows, Terraform, .cshtml, .tsx, .json, .http)

# Quality checks
dotnet build services/ai-api/Propely.AiApi.sln     # 0 errors, 23 warnings
dotnet build services/orgs-api/Propely.OrgsApi.sln  # 0 errors, 25 warnings
dotnet test services/ai-api/Propely.AiApi.sln       # 169 tests passed
dotnet test services/orgs-api/Propely.OrgsApi.sln   # 604 tests passed
cd apps/web && npm run build                        # success
```

## Files Changed

### Renamed (directories + files)
- `services/ai-api/SaasTemplate.AiApi.sln` -> `Propely.AiApi.sln`
- `services/orgs-api/SaasTemplate.OrgsApi.sln` -> `Propely.OrgsApi.sln`
- 8 ai-api project directories (src/4 + tests/3) renamed from `SaasTemplate.AiApi.*` to `Propely.AiApi.*`
- 8 orgs-api project directories (src/4 + tests/3) renamed from `SaasTemplate.OrgsApi.*` to `Propely.OrgsApi.*`
- 12 .csproj files renamed within directories
- 2 .http files renamed within directories

### Content-replaced
- ~495 .cs files: namespace/using declarations
- 12 .csproj files: project references
- 2 .sln files: project paths
- 3 docker-compose files: service/container names
- 4 Dockerfiles: project/assembly paths
- 1 SQL init file: database/user names
- 3 .env files: connection strings, container names
- 11 scripts (.ps1/.sh): solution/project references
- 3 CI workflows (.yml): solution paths
- 8 Terraform files (.tf): resource naming
- 13 Razor templates (.cshtml): product name
- 4 appsettings.json: database references
- 2 .http files: project references
- 1 .tsx file: brand references
- 1 Terraform README: naming note

## Tests
### Unit
- What was added/updated: No new tests; existing tests verified after rename
- How to run: `dotnet test services/ai-api/Propely.AiApi.sln && dotnet test services/orgs-api/Propely.OrgsApi.sln`
- Results: 88 ai-api unit + 439 orgs-api unit = 527 unit tests passed

### Integration
- What was added/updated: No new tests; existing tests verified after rename
- How to run: Same as above
- Results: 76 ai-api integration + 160 orgs-api integration = 236 integration tests passed

### Architecture
- Results: 5 ai-api + 5 orgs-api = 10 architecture tests passed

### Manual
- What you verified: Zero SaasTemplate references in source files, all builds pass, all 773 tests pass
- Steps: `grep -ri "saastemplate" --include="*.cs" --include="*.csproj" ...` returns 0 hits

## Observability
- Logs added/updated: N/A
- Traces/metrics added/updated: N/A

## Security
- Validation: N/A
- AuthN/AuthZ impact: None
- Sensitive data handling: Verified .env.example contains only placeholder values; no secrets exposed

## Follow-ups / Backlog
- [ ] Run `dev-reset` + `dev-up` to verify Docker containers start with new names (manual step)
- [ ] Verify CI pipeline passes on PR (automated)

## Checklist
- [x] Task scope matches `docs/tasks/0002-rename-saastemplate-to-propely.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
