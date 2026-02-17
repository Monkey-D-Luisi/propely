# Walkthrough: 0030-auth-rate-limiting

## Task Reference
- Task: `docs/tasks/0030-auth-rate-limiting.md`
- Walkthrough: `docs/walkthroughs/0030-auth-rate-limiting.md`
- Branch/PR: `feat/auth-rate-limiting-0030` / `TBD`
- Date: `2026-02-09`

## Summary
Implemented auth-specific rate limiting in `orgs-api` for login/register/forgot-password/resend-verification, moved policy to configuration (env-overridable), and added frontend `429` handling with `Retry-After` parsing and localized messages.

## Context
- Background: orgs-api currently uses AspNetCoreRateLimit with partial endpoint-specific rules in code.
- Problem statement: login/register/forgot-password need strict limits and standardized `429` behavior with `Retry-After`.
- Constraints (time, scope, dependencies): no new dependencies; keep within task 0030 scope.

## Decisions & Trade-offs
- **Decision:** Move rate-limit rules from hardcoded DI options into configuration (`appsettings` + env override support).
  - Options considered: keep hardcoded options vs bind from configuration.
  - Why this choice: satisfies requirement for environment-variable overridability and keeps policy centralized.
  - Consequences / risks: integration tests require request-IP isolation to avoid cross-test throttling.
- **Decision:** Keep `QuotaExceededResponse` as compact JSON (`{"error":"RATE_LIMIT_EXCEEDED"}`) instead of ProblemDetails.
  - Why this choice: aligns with existing API error shape consumed by frontend.
  - Consequences / risks: less metadata than RFC 7807; can be expanded later if required.
- **Decision:** Escape JSON braces in `QuotaExceededResponse.Content`.
  - Why this choice: `AspNetCoreRateLimit` formats this value internally; unescaped braces trigger `500` on throttled responses.

## Implementation Notes
- Key changes:
- Bound `IpRateLimitOptions` from `IpRateLimiting` configuration in API DI.
- Added endpoint-specific rules to `appsettings.json`:
  - `post:/auth/login` => `5/1m`
  - `post:/auth/forgot-password` => `3/1m`
  - `post:/auth/register` => `10/1m`
  - `post:/auth/resend-verification` => `1/5m`
  - global fallback `*` => `100/1m`
- Added `.env.example` overrides for all auth-specific rules.
- Extended frontend `ApiError` with `retryAfterSeconds` and parsed `Retry-After` header (seconds or HTTP date).
- Updated auth forms to show localized rate-limit messages with dynamic seconds:
  - `LoginForm`, `RegisterForm`, `ForgotPasswordForm`.
- Updated frontend i18n dictionaries (`en`, `es`) with `auth.errors.rateLimited`.
- Added/updated tests:
  - frontend API + auth form tests for `429` flows
  - orgs-api integration tests for auth endpoint limits and `Retry-After`
  - test isolation via per-request/per-helper `X-Real-IP` where needed.
- Edge cases handled:
- Invalid `Retry-After` header values fall back to `undefined` and UI default `60` seconds.
- Non-rate-limit auth requests in tests use unique IPs to prevent shared-window collisions.
- Verified non-auth endpoint behavior remains governed by global limit (not strict auth thresholds).
- Known limitations:
- Manual browser verification from task Testing Plan is still pending; task should not be marked `DONE` until this is executed.

## Data / Schema / Migrations
- DB changes (if any): None.
- Migration strategy: N/A.
- Backward compatibility: Global default rate limit remains as fallback.

## Commands Run
```bash
npm test -- --run src/lib/__tests__/api.test.ts src/components/auth/__tests__/LoginForm.test.tsx src/components/auth/__tests__/RegisterForm.test.tsx src/components/auth/__tests__/ForgotPasswordForm.test.tsx
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/SaasTemplate.OrgsApi.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointTests"

dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

cd apps/web && npm run build
cd apps/web && npm test -- --run
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs` - rate-limit options now bound from configuration.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` - added auth-specific rules and `429` response config.
- `.env.example` - added env override examples for `IpRateLimiting.GeneralRules`.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs` - updated thresholds, added login/register/global limit tests, added Retry-After assertions, and improved IP isolation.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/OrgsEndpointTests.cs` - add `X-Real-IP` for register helper.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/FeatureFlagEndpointTests.cs` - add `X-Real-IP` for register helper.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/NotificationEndpointTests.cs` - add `X-Real-IP` for register helper.
- `apps/web/src/lib/api.ts` - parse/store `Retry-After` into `ApiError.retryAfterSeconds`.
- `apps/web/src/lib/__tests__/api.test.ts` - coverage for `Retry-After` parsing.
- `apps/web/src/components/auth/LoginForm.tsx` - show localized `429` message with retry seconds.
- `apps/web/src/components/auth/RegisterForm.tsx` - show localized `429` message with retry seconds.
- `apps/web/src/components/auth/ForgotPasswordForm.tsx` - use shared localized `429` message with retry seconds.
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx` - validate `429` UI behavior.
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx` - validate `429` UI behavior.
- `apps/web/src/components/auth/__tests__/ForgotPasswordForm.test.tsx` - updated `429` expectation.
- `apps/web/messages/en.json` - added `auth.errors.rateLimited`.
- `apps/web/messages/es.json` - added `auth.errors.rateLimited`.
- `docs/backlog/epic-002-auth-security.md` - task `0030` moved to `IN_PROGRESS`.
- `docs/walkthroughs/0030-auth-rate-limiting.md` - this walkthrough.

## Tests
### Unit
- What was added/updated:
- Frontend unit tests for `ApiError.retryAfterSeconds` and auth forms `429` rendering.
- How to run:
- `cd apps/web && npm test -- --run`

### Integration
- What was added/updated:
- Auth endpoint rate-limit tests updated/added:
  - forgot-password 3/min + `Retry-After`
  - login 5/min + `Retry-After`
  - register 10/min + `Retry-After`
  - non-auth endpoint still under global policy.
- How to run:
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`

### Manual
- What you verified:
- Pending (required before marking task `DONE`).
- Steps:
  1. Attempt rapid login attempts in browser from same IP/session; confirm UI `429` message with retry seconds.
  2. Attempt rapid forgot-password and register calls; confirm throttling and user-facing message behavior.
  3. Verify normal non-auth navigation/API usage is not affected by strict auth limits.
  4. Verify after waiting for window expiry, auth requests succeed again.

## Observability
- Logs added/updated:
- None.
- Traces/metrics added/updated:
- None.

## Security
- Validation:
- Auth endpoints now enforce stricter brute-force protection with per-endpoint thresholds.
- `Retry-After` is propagated to client for safer retry UX.
- AuthN/AuthZ impact:
- No auth model changes; only request throttling policy and client error handling.
- Sensitive data handling (secrets, PII):
- No secrets committed. Only `.env.example` placeholders/override examples were updated.

## Follow-ups / Backlog
- [ ] Validate production-side rate-limit tuning after real traffic baseline is available.

## Checklist
- [x] Task scope matches `docs/tasks/0030-auth-rate-limiting.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
