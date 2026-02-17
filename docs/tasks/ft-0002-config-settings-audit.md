# ft-0002: Configuration Settings Audit

## Status: DONE

## Goal

Audit and fix configuration settings across both .NET services (ai-api and orgs-api) to ensure all bound configuration sections exist in appsettings files and environment variables are correctly mapped in all contexts (runtime, development, integration tests).

## Background

Integration tests were passing but no OpenAI API calls were being registered, indicating the API key wasn't being loaded. Investigation revealed multiple missing configuration sections and a copy-paste bug in Redis key prefix.

## Scope

### In Scope
- Add missing `OpenAi` and `Redis` sections to ai-api appsettings files
- Add missing `Redis` section to orgs-api appsettings files
- Fix orgs-api `RedisConfiguration.KeyPrefix` copy-paste bug (`"aiapi:"` -> `"orgsapi:"`)
- Add `Environment.GetEnvironmentVariable` fallback for OpenAI API key
- Fix test factories to map prefixed env vars from `.env` to service-expected keys
- Update affected test assertions

### Out of Scope
- Adding new configuration sections beyond what's already bound in DI
- Changing the env var mapping strategy in run scripts

## Acceptance Criteria
- [x] All `Configure<T>(GetSection("..."))` calls have matching sections in appsettings.json
- [x] orgs-api Redis KeyPrefix defaults to `"orgsapi:"` not `"aiapi:"`
- [x] OpenAiService has env var fallback for API key
- [x] Test factories map `AIAPI_OPENAI_API_KEY` -> `OPENAI_API_KEY`
- [x] Both services build with 0 errors
- [x] All tests pass (ai-api: 128, orgs-api: 50)

## Files Changed

### ai-api
- `src/SaasTemplate.AiApi.Api/appsettings.json` — Added `OpenAi` and `Redis` sections
- `src/SaasTemplate.AiApi.Api/appsettings.Development.json` — Added `OpenAi` and `Redis` sections
- `src/SaasTemplate.AiApi.Infrastructure/Services/OpenAiService.cs` — Added `Environment.GetEnvironmentVariable` fallback
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Map `AIAPI_*` env vars to service keys

### orgs-api
- `src/SaasTemplate.OrgsApi.Api/appsettings.json` — Added `Redis` section
- `src/SaasTemplate.OrgsApi.Api/appsettings.Development.json` — Added `Redis` section
- `src/SaasTemplate.OrgsApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs` — Fixed KeyPrefix `"aiapi:"` -> `"orgsapi:"`
- `tests/SaasTemplate.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Map `ORGSAPI_*` env vars to service keys
- `tests/SaasTemplate.OrgsApi.IntegrationTests/Caching/RedisCacheServiceTests.cs` — Updated assertion for corrected KeyPrefix
