# Task: 0030 - Auth-Specific Rate Limiting (Login Brute-Force)

## Metadata
- ID: 0030
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #182
- Epic: `docs/backlog/epic-002-auth-security.md`
- Old Issue: Codex #87

## Goal
Add endpoint-specific rate limiting for authentication endpoints to protect against brute-force login attacks, separate from the existing global rate limit.

## Context
The orgs-api already has global rate limiting via `AspNetCoreRateLimit` (100 requests/min per IP, configured in `Program.cs`). However, auth endpoints like login and forgot-password need stricter per-endpoint limits. A global 100/min limit is too generous for login brute-force protection. Industry standard is ~5-10 login attempts per minute per IP.

### Current Rate Limiting Setup
- NuGet package: `AspNetCoreRateLimit` already installed
- Configuration: Global 100 requests/min per IP
- Middleware: `app.UseIpRateLimiting()` in Program.cs
- Config section: `IpRateLimiting` in appsettings or env vars

## Scope
### In scope
- Configure endpoint-specific rate limits for `/auth/login` (5 attempts/min per IP)
- Configure endpoint-specific rate limits for `/auth/forgot-password` (3 attempts/min per IP)
- Configure endpoint-specific rate limits for `/auth/register` (10 attempts/min per IP)
- Configure endpoint-specific rate limits for `/auth/resend-verification` (1 attempt/5 min per IP)
- Return proper `429 Too Many Requests` with `Retry-After` header
- Add i18n error message for rate limit exceeded

### Out of scope
- Per-user/per-email rate limiting (would require distributed state)
- CAPTCHA after N failed attempts
- Account lockout after N failed attempts

## Requirements
- R1: `/auth/login` is limited to 5 requests/minute per IP
- R2: `/auth/forgot-password` is limited to 3 requests/minute per IP
- R3: `/auth/register` is limited to 10 requests/minute per IP
- R4: Rate-limited responses return 429 with `Retry-After` header
- R5: Global rate limit (100/min) still applies to all other endpoints
- R6: Rate limit configuration is environment-variable overridable

## Acceptance Criteria
- AC1: 6th login attempt within 1 minute returns 429
- AC2: 4th forgot-password attempt within 1 minute returns 429
- AC3: 429 response includes `Retry-After` header
- AC4: Non-auth endpoints still allow 100 requests/min
- AC5: Rate limits can be adjusted via environment variables
- AC6: `dotnet build` and `dotnet test` pass

## Constraints (non-negotiable)
- Use existing AspNetCoreRateLimit package (no new dependencies)
- Clean Architecture layers respected
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Update rate limiting configuration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs` or appsettings)
   - AspNetCoreRateLimit supports `GeneralRules` with endpoint-specific overrides
   - Add rules to `IpRateLimitOptions.GeneralRules`:
     ```json
     {
       "Endpoint": "post:/auth/login",
       "Period": "1m",
       "Limit": 5
     },
     {
       "Endpoint": "post:/auth/forgot-password",
       "Period": "1m",
       "Limit": 3
     },
     {
       "Endpoint": "post:/auth/register",
       "Period": "1m",
       "Limit": 10
     },
     {
       "Endpoint": "post:/auth/resend-verification",
       "Period": "5m",
       "Limit": 1
     }
     ```

2. **Verify AspNetCoreRateLimit configuration** in `Program.cs`
   - Ensure `services.AddMemoryCache()` is called (required by AspNetCoreRateLimit)
   - Ensure `services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"))` reads from config
   - Ensure `services.AddInMemoryRateLimiting()` or distributed rate limiting is configured
   - Ensure `app.UseIpRateLimiting()` is in the middleware pipeline

3. **Add configuration to appsettings.json** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json`)
   - Add/update `IpRateLimiting.GeneralRules` section with endpoint-specific rules
   - Keep existing global rule as fallback

4. **Add env var overrides** to `.env`
   - `ORGSAPI_IpRateLimiting__GeneralRules__0__Endpoint=post:/auth/login`
   - `ORGSAPI_IpRateLimiting__GeneralRules__0__Period=1m`
   - `ORGSAPI_IpRateLimiting__GeneralRules__0__Limit=5`
   - (Similar for other endpoints)

5. **Update error handling** for 429 responses
   - AspNetCoreRateLimit returns 429 by default with `Retry-After` header
   - Customize the response body to match ProblemDetails format (RFC 7807)
   - Configure `IpRateLimitOptions.HttpStatusCode = 429`
   - Configure `IpRateLimitOptions.QuotaExceededResponse` with ProblemDetails body

6. **Frontend: handle 429 responses** (`apps/web/src/lib/api.ts`)
   - Detect 429 status in `apiFetch`
   - Parse `Retry-After` header
   - Show user-friendly message: "Too many attempts. Please try again in X seconds."

7. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `errors.rateLimited`: "Too many attempts. Please try again in {seconds} seconds."

### Testing

8. **Integration tests**
   - Send 6 rapid login requests -> verify 6th gets 429
   - Verify `Retry-After` header present in 429 response
   - Verify non-auth endpoints still work at higher rates
   - Wait for rate limit window to expire -> verify access restored

## Files to Create / Modify

### Create
- `docs/walkthroughs/0030-auth-rate-limiting.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` (add endpoint-specific rules)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs` (verify rate limit config)
- `apps/web/src/lib/api.ts` (handle 429 responses)
- `apps/web/messages/en.json` (add rate limit error string)
- `apps/web/messages/es.json` (add rate limit error string)
- `.env` (add rate limit env var examples)

## Testing Plan
- Integration tests: Verify per-endpoint rate limits work correctly
- Integration tests: Verify 429 response format and headers
- Manual test: Rapid login attempts in browser, verify UI shows error

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
