# Walkthrough: cr-0005 -- PR #15 SDK Client Review

## Task Reference
- Task: `docs/tasks/cr-0005-pr15-sdk-client-review.md`
- PR: https://github.com/Monkey-D-Luisi/propely/pull/15
- Branch: `feat/0004-nuget-sdk-client-infrastructure`
- Date: `2026-02-20`

## Summary
Addressed PR #15 review feedback from Codex, Gemini Code Assist, and GitHub Copilot. Fixed 7 issues across 2 MUST_FIX (API contract mismatch, missing DI registration) and 5 SHOULD_FIX (header validation safety, duplicate timeout, documentation accuracy). Documented 1 false positive (cross-tenant DoS claim) and 1 out-of-scope item (RetryDelay configurability).

## Changes Made

### MUST_FIX
1. **`IOrgsApiClient.cs`**: Changed `GetCurrentUserAsync` return type from `Task<UserResponse>` to `Task<MeResponse>` to match the `/auth/me` envelope response (`{ user: ... }`).
2. **`Dtos/MeResponse.cs`**: Created new envelope DTO `MeResponse(UserResponse User)`.
3. **`ServiceCollectionExtensions.cs`**: Added `services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>()` so consumers don't need to register it separately.

### SHOULD_FIX
4. **`TenantDelegatingHandler.cs`**: Changed `request.Headers.Add(...)` to `request.Headers.TryAddWithoutValidation(...)` to prevent `ArgumentException` from malformed header values.
5. **`ServiceCollectionExtensions.cs`**: Changed `client.Timeout = options.Timeout` to `client.Timeout = Timeout.InfiniteTimeSpan` so Polly manages timeout exclusively (no duplicate).
6. **`OrgsApiClientOptions.cs`**: Fixed XML doc on `CircuitBreakerFailureThreshold` from "consecutive failures" to "minimum requests in sampling window before evaluating failure ratio".
7. **`docs/architecture/sdk-client-pattern.md`**: Fixed circuit breaker table description, added `ShouldRetry` method to code example, added `TryAddSingleton` and `Timeout.InfiniteTimeSpan` to DI extension example, noted auto-registration of `IHttpContextAccessor` in consumer section.

### FALSE_POSITIVE
- Gemini's cross-tenant DoS via shared circuit breaker: The circuit breaker is per-downstream-service-instance, which is the correct and standard scoping. Per-tenant circuit breakers would be over-engineering.

### OUT_OF_SCOPE
- Gemini's `RetryDelay` property suggestion: YAGNI for initial implementation.

## Commands Run
```bash
dotnet build services/orgs-api/Propely.OrgsApi.sln    # 0 errors
dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests/         # 447 passed
dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/ --filter OrgsApiClientIntegration  # 4 passed
dotnet test services/orgs-api/tests/Propely.OrgsApi.ArchitectureTests/ # 5 passed
```

## Validation Results
- Build: 0 errors, 25 warnings (all pre-existing)
- Unit tests: 447 passed, 0 failed
- Integration tests: 4 passed, 0 failed
- Architecture tests: 5 passed, 0 failed
