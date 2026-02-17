# Walkthrough: ft-0002-config-settings-audit

## Task Reference
- Task: `docs/tasks/ft-0002-config-settings-audit.md`
- Walkthrough: `docs/walkthroughs/ft-0002-config-settings-audit.md`
- Branch/PR: `ft/ft-0002-config-settings-audit`
- Date: `2026-02-06`

## Summary
Audited and fixed configuration settings across both .NET services. Found and resolved missing appsettings sections, a copy-paste bug in Redis key prefix, missing environment variable fallback in OpenAiService, and missing env var mapping in integration test factories.

## Context
- Background: Integration tests pass but no OpenAI API calls were being registered against the actual API, indicating the API key was not being loaded correctly.
- Problem statement: Multiple `Configure<T>(GetSection("..."))` calls were binding to non-existent sections (silently using defaults), and env vars from `.env` were not being mapped to service-expected keys in integration tests.
- Root cause: When .NET's `IOptions<T>` binds to a missing section, it silently uses default values. The `.env` file uses prefixed vars (`AIAPI_*`, `ORGSAPI_*`) but the run scripts map them to non-prefixed names — integration test factories didn't replicate this mapping.

## Issues Found

### 1. Missing appsettings sections (silent binding failures)
- **ai-api `appsettings.json` / `appsettings.Development.json`**: Missing `OpenAi` and `Redis` sections. `Configure<OpenAiOptions>` and `Configure<RedisConfiguration>` bound to empty sections, silently using class defaults.
- **orgs-api `appsettings.json` / `appsettings.Development.json`**: Missing `Redis` section. Same silent binding issue.

### 2. Copy-paste bug: orgs-api Redis KeyPrefix
- **File**: `OrgsApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs`
- **Bug**: `KeyPrefix` defaulted to `"aiapi:"` instead of `"orgsapi:"` — copied from ai-api without updating.
- **Impact**: Both services would use the same Redis key namespace, causing potential cache collisions.

### 3. OpenAiService missing env var fallback
- **File**: `AiApi.Infrastructure/Services/OpenAiService.cs`
- **Bug**: Only checked `configuration["OPENAI_API_KEY"]`. In some environments, the env var might be set but not yet in IConfiguration.
- **Fix**: Added `Environment.GetEnvironmentVariable("OPENAI_API_KEY")` as fallback.

### 4. Test factories missing env var mapping
- **Files**: Both `ApiWebApplicationFactory.cs` files
- **Bug**: `DotNetEnv.Env.TraversePath().Load()` loads `.env` which sets `AIAPI_OPENAI_API_KEY` in process environment, but the service reads `OPENAI_API_KEY`. Run scripts do this mapping, but test factories did not.
- **Fix**: Added in-memory config overrides that read the prefixed env vars and map them to service-expected keys.

## Decisions & Trade-offs

- **In-memory config override vs modifying .env loading**: Chose to add mappings in `ConfigureAppConfiguration` using `AddInMemoryCollection` rather than modifying how `.env` is loaded. This mirrors what the run scripts do and keeps the test factory explicit about which vars are mapped.

- **Env var fallback in OpenAiService**: Added `Environment.GetEnvironmentVariable()` as a null-coalescing fallback rather than restructuring the DI. This is a minimal change that handles edge cases where env vars are set but not in IConfiguration.

## Implementation Notes

### appsettings sections added
```json
"OpenAi": {
  "ModelId": "gpt-5-mini"
},
"Redis": {
  "ConnectionString": "localhost:6379",
  "Enabled": true,
  "DefaultTtlMinutes": 5,
  "KeyPrefix": "aiapi:"  // or "orgsapi:" for orgs-api
}
```

### OpenAiService fallback
```csharp
var apiKey = configuration["OPENAI_API_KEY"]
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
```

### Test factory env var mapping
```csharp
var openAiApiKey = Environment.GetEnvironmentVariable("AIAPI_OPENAI_API_KEY");
if (!string.IsNullOrWhiteSpace(openAiApiKey))
{
    overrides["OPENAI_API_KEY"] = openAiApiKey;
}
```

## Commands Run
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed

### ai-api
- `src/SaasTemplate.AiApi.Api/appsettings.json` — Added `OpenAi` and `Redis` sections
- `src/SaasTemplate.AiApi.Api/appsettings.Development.json` — Added `OpenAi` and `Redis` sections
- `src/SaasTemplate.AiApi.Infrastructure/Services/OpenAiService.cs` — Added env var fallback for API key
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Added `AIAPI_*` env var mapping

### orgs-api
- `src/SaasTemplate.OrgsApi.Api/appsettings.json` — Added `Redis` section
- `src/SaasTemplate.OrgsApi.Api/appsettings.Development.json` — Added `Redis` section
- `src/SaasTemplate.OrgsApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs` — Fixed KeyPrefix default
- `tests/SaasTemplate.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Added `ORGSAPI_*` env var mapping
- `tests/SaasTemplate.OrgsApi.IntegrationTests/Caching/RedisCacheServiceTests.cs` — Fixed assertion for corrected KeyPrefix

## Verification
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 128 tests passed (67 unit + 5 arch + 56 integration)
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 50 tests passed (21 unit + 5 arch + 24 integration)

## Security
- No secrets committed. API keys remain in `.env` (gitignored).
- appsettings sections contain only non-secret defaults (model IDs, connection strings to localhost, TTLs).

## Follow-ups / Backlog
- [ ] Consider a startup validation that logs warnings when expected configuration sections are missing
- [ ] Consider adding an OpenTelemetry metric for OpenAI API call count to verify integration

## Checklist
- [x] Task scope matches `docs/tasks/ft-0002-config-settings-audit.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
