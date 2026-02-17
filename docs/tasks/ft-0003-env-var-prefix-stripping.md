# ft-0003: Eliminate env var translation layer using AddEnvironmentVariables(prefix)

## Status: DONE

## Goal

Replace the fragile manual env var mapping (in run scripts, test factories, and application code fallbacks) with .NET's built-in `AddEnvironmentVariables(prefix)` which strips the prefix and maps `__` to `:` for config hierarchy automatically.

## Background

After ft-0002 fixed missing config sections and added env var mapping in test factories, the architecture had three separate places replicating the same AIAPI_/ORGSAPI_ prefix-to-config-key translation: run scripts, test factories, and application code fallbacks. This was fragile and had already caused a bug. .NET's `AddEnvironmentVariables("AIAPI_")` eliminates all translation layers in one idiomatic pattern.

## Scope

### In Scope
- Rename `.env` and `.env.example` vars to use `__` convention (e.g., `AIAPI_RabbitMQ__Host`)
- Add `AddEnvironmentVariables("AIAPI_")` / `AddEnvironmentVariables("ORGSAPI_")` to both `Program.cs`
- Remove mapping blocks from all 4 run scripts
- Refactor `OpenAiService` to use `IOptions<OpenAiOptions>` for API key (add `ApiKey` property)
- Remove `DATABASE_CONNECTION_STRING` fallbacks from DI and HealthChecks
- Update `DesignTimeDbContextFactory` env var names
- Simplify test factories (replace manual mapping with `AddEnvironmentVariables`)
- Update `OpenAiServiceTests` config key path
- Update CLAUDE.md to reflect new convention

### Out of Scope
- Changing shared infra vars (`POSTGRES_*`, `RABBITMQ_*`, `REDIS_*`)
- Changing `NEXT_PUBLIC_*` vars

## Acceptance Criteria
- [x] `.env` uses `__` as section separator for all `AIAPI_*` and `ORGSAPI_*` vars
- [x] Both `Program.cs` call `AddEnvironmentVariables(prefix)`
- [x] Run scripts have no env var mapping logic (just load `.env` + run dotnet)
- [x] `OpenAiService` reads API key from `IOptions<OpenAiOptions>` (not `IConfiguration`)
- [x] No `Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")` fallbacks remain
- [x] `DesignTimeDbContextFactory` uses full prefixed env var names
- [x] Test factories use `AddEnvironmentVariables(prefix)` instead of manual mapping
- [x] Both services build with 0 errors
- [x] All tests pass (ai-api: 121, orgs-api: 37)

## Files Changed

### Root
- `.env` — Renamed all `AIAPI_*` / `ORGSAPI_*` vars to `__` convention
- `.env.example` — Same rename
- `CLAUDE.md` — Updated Environment Variables section

### ai-api
- `src/SaasTemplate.AiApi.Api/Program.cs` — Added `AddEnvironmentVariables("AIAPI_")`
- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs` — Removed `DATABASE_CONNECTION_STRING` fallback
- `src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs` — Removed `DATABASE_CONNECTION_STRING` fallback
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/DesignTimeDbContextFactory.cs` — Updated env var to `AIAPI_ConnectionStrings__DefaultConnection`
- `src/SaasTemplate.AiApi.Infrastructure/Services/OpenAiOptions.cs` — Added `ApiKey` property
- `src/SaasTemplate.AiApi.Infrastructure/Services/OpenAiService.cs` — Refactored to use `IOptions<OpenAiOptions>` for API key
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Simplified with `AddEnvironmentVariables("AIAPI_")`
- `tests/SaasTemplate.AiApi.IntegrationTests/Services/OpenAiServiceTests.cs` — Updated config key to `OpenAi:ApiKey`

### orgs-api
- `src/SaasTemplate.OrgsApi.Api/Program.cs` — Added `AddEnvironmentVariables("ORGSAPI_")`
- `src/SaasTemplate.OrgsApi.Api/Configuration/HealthChecksConfiguration.cs` — Removed `DATABASE_CONNECTION_STRING` fallback
- `src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` — Removed `DATABASE_CONNECTION_STRING` fallback
- `src/SaasTemplate.OrgsApi.Infrastructure/Persistence/DesignTimeDbContextFactory.cs` — Updated env var to `ORGSAPI_ConnectionStrings__DefaultConnection`
- `tests/SaasTemplate.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Simplified with `AddEnvironmentVariables("ORGSAPI_")`

### Scripts
- `scripts/run-ai-api.ps1` — Removed env var mapping block
- `scripts/run-ai-api.sh` — Removed env var mapping block
- `scripts/run-orgs-api.ps1` — Removed env var mapping block
- `scripts/run-orgs-api.sh` — Removed env var mapping block
