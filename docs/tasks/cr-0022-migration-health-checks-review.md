# Task: cr-0022 - Migration Health Checks Review

## Metadata
- ID: cr-0022
- Type: CodeReview
- Status: DOING
- Owner: Agent
- Created: 2026-02-07
- PR: #173 (`feat/0022-migration-health-checks`)
- CI Status: AI API passed, Orgs API passed, Web skipped, **CodeQL FAILED** (permissions)

## Changed Files
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0022-migration-health-checks.md`
- `docs/walkthroughs/0022-migration-health-checks.md`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Configuration/DatabaseMigrationConfiguration.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Program.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/DatabaseMigrationTests.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/DatabaseMigrationConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Persistence/DatabaseMigrationTests.cs`

## Review Threads

### Unresolved

1. **Copilot** (inline, `ai-api/DatabaseMigrationConfiguration.cs:45`) — `GetConnectionString()` can be null; add guard
2. **Copilot** (inline, `orgs-api/DatabaseMigrationTests.cs:20`) — Tests share container, flaky fresh-db assumption
3. **Copilot** (inline, `ai-api/DatabaseMigrationTests.cs:20`) — Same as #2
4. **Copilot** (inline, `orgs-api/DatabaseMigrationConfiguration.cs:45`) — Same as #1
5. **Gemini** (inline, `0022-migration-health-checks.md:84`) — Docker Compose should use /health/ready
6. **Gemini** (inline, `ai-api/DatabaseMigrationConfiguration.cs:30`) — Logger should be DatabaseMigrationConfiguration
7. **Gemini** (inline, `orgs-api/DatabaseMigrationConfiguration.cs:30`) — Same as #6

### General Reviews (no additional items)
8. **Copilot** (review body) — Summary, references #1-#4
9. **Gemini** (review body) — Summary, references #5-#7

### Issue Comments (no actionable items)
10. **Gemini** (issue comment) — PR summary

### CI Failures
- **CodeQL** — Both C# and JS/TS jobs fail with "Resource not accessible by integration" — missing `actions: read` permission

## Comment Resolution Plan

### MUST_FIX
- [x] CI: Add `actions: read` to `.github/workflows/codeql.yml` permissions

### SHOULD_FIX
- [x] #1/#4: Add null guard for connection string in both DatabaseMigrationConfiguration.cs
- [x] #6/#7: Change logger category from `ILogger<AppDbContext>` to `ILogger<DatabaseMigrationConfiguration>`

### NO_ACTION
- [x] #2/#3: xUnit creates new class instance per test; each gets its own Testcontainer lifecycle. Tests ARE isolated.
- [x] #5: `/health/live` is correct for Docker Compose healthchecks. `/health/ready` would cause unnecessary restarts on transient dependency failures.
- [x] #8-#10: Review/issue comment summaries, no additional action needed
