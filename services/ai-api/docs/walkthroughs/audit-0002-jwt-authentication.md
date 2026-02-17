# Walkthrough: audit-0002-jwt-authentication

## Task Reference
- Task: `docs/tasks/audit-0002-jwt-authentication.md`
- Walkthrough: `docs/walkthroughs/audit-0002-jwt-authentication.md`
- Branch/PR: `audit-0002-jwt-authentication`
- Date: `2026-01-30`

## Summary
Implements JWT bearer authentication and policy-based authorization per security baseline. Adds development-mode anonymous access to maintain smooth local development experience while securing production endpoints.

## Context
- Background: Security audit F-01 identified all endpoints are anonymous
- Problem statement: No authentication/authorization middleware configured
- Constraints: Must not break integration tests, must allow dev-mode anonymous access

## Decisions & Trade-offs
- **Decision:** Use conditional anonymous access via configuration flag
  - Options considered: (1) Separate dev/prod pipelines, (2) Configuration flag, (3) Test-only bypass
  - Why this choice: Configuration flag is explicit, documented, and aligns with security baseline example
  - Consequences / risks: See security warning below

> ⚠️ **CRITICAL SECURITY WARNING**: The `AllowAnonymous` flag bypasses ALL authentication and authorization. The application includes a fail-fast guard that throws an exception if this flag is enabled in any environment other than Development or Testing. This guard exists to prevent accidental security bypass in production. **NEVER** attempt to circumvent this guard.

- **Decision:** Define policies centrally in AuthorizationPolicies class
  - Options considered: (1) Inline in Program.cs, (2) Separate static class
  - Why this choice: Easier to maintain and reference policy names consistently
  - Consequences / risks: None significant

- **Decision:** Fail closed with default authentication policy
  - Why this choice: Aligns with security baseline "deny by default" principle
  - Consequences: Any new endpoint without explicit [AllowAnonymous] requires authentication

## Implementation Notes
- Key changes:
  - Added Security configuration section with AllowAnonymous and JWT settings
  - Production guard prevents AllowAnonymous in non-dev environments
  - JWT Authority/Audience validation fails fast if not configured
  - Created AuthorizationPolicies constants class
  - Configured JWT bearer authentication with validation parameters
  - Added conditional authorization (bypassed when AllowAnonymous=true in Development)
  - Applied [Authorize] policies to controller endpoints
- Edge cases handled: Integration tests run in "Testing" environment with AllowAnonymous
- Known limitations: No actual identity provider configured (placeholder values)

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Full

## Commands Run
```bash
dotnet build
dotnet test
```

## Files Changed
- `src/SaasTemplate.AiApi.Api/Program.cs` — Added JWT auth services and middleware with conditional dev/prod modes
- `src/SaasTemplate.AiApi.Api/appsettings.json` — Added Security and JWT configuration sections
- `src/SaasTemplate.AiApi.Api/appsettings.Development.json` — Added AllowAnonymous=true for development
- `src/SaasTemplate.AiApi.Api/appsettings.Testing.json` — New file with AllowAnonymous=true for integration tests
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs` — Added [Authorize] policies to endpoints
- `src/SaasTemplate.AiApi.Api/Configuration/AuthorizationPolicies.cs` — New file with policy name constants
- `src/SaasTemplate.AiApi.Api/Configuration/DevAuthenticationHandler.cs` — New file with dev-mode auto-authentication
- `src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj` — Added Microsoft.AspNetCore.Authentication.JwtBearer package
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` — Added config override for tests

## Tests
### Unit
- What was added/updated: None
- How to run: `dotnet test`

### Integration
- What was added/updated: Tests continue to work via AllowAnonymous in Testing environment
- How to run: `dotnet test`

### Manual
- What you verified: Authentication enforced in non-dev mode
- Steps: Run API without AllowAnonymous flag, verify 401 response

## Observability
- Logs added/updated: JWT validation failures will be logged by ASP.NET Core
- Traces/metrics added/updated: None
- Dashboards/alerts touched: None

## Security
- Validation: JWT tokens validated for issuer, audience, lifetime
- AuthN/AuthZ impact: All endpoints now require authentication except in dev mode
- Sensitive data handling: No secrets in config (issuer/audience are not secrets)

## Performance
- Hot paths impacted: Every request goes through auth middleware
- Any profiling/bench notes: Standard ASP.NET Core JWT middleware, no custom code

## Docs Updated
- Files updated: This walkthrough
- Anything intentionally left for later: README could document auth requirements

## Rollback Plan
- How to revert safely: Remove auth configuration from Program.cs
- Data rollback considerations: None

## Follow-ups / Backlog
- [ ] Configure actual identity provider (Azure AD, Auth0, etc.)
- [ ] Add user claims enrichment if needed
- [ ] Implement HTTPS/HSTS (audit-0003)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0002-jwt-authentication.md`
- [x] Tests updated and passing (79 tests)
- [x] Docs updated where relevant
- [x] No secrets committed
