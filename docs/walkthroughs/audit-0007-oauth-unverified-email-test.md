# Walkthrough: audit-0007-oauth-unverified-email-test

## Task Reference
- Task: `docs/tasks/audit-0007-oauth-unverified-email-test.md`
- Walkthrough: `docs/walkthroughs/audit-0007-oauth-unverified-email-test.md`
- Branch/PR: `fix/audit-0006-session-invalidation`
- Date: `2026-02-09`

## Summary
Added an integration test that verifies the OAuth callback endpoint properly rejects login attempts when the OAuth provider returns an unverified email. The test forges a valid `ExternalOAuth` cookie containing claims with `email_verified=false` and verifies the user is redirected with the `email_not_verified` error.

## Context
- Background: Audit epic-002 identified that while the `HasVerifiedEmail` check exists in `AuthController.OAuthCallback`, no integration test exercises this code path.
- Problem statement: The email verification check could silently break during refactoring without detection.
- Constraints: Must forge a valid external cookie without going through a real OAuth provider flow.

## Decisions & Trade-offs
- **Decision: Cookie forging via `TicketDataFormat`**
  - Options considered: (1) Test endpoint for external sign-in, (2) Test authentication handler, (3) Direct cookie forging via `CookieAuthenticationOptions.TicketDataFormat`
  - Why this choice: Direct cookie forging requires no changes to the application code or test infrastructure. It uses the same data protection provider to create a valid cookie that the real OAuth flow would produce.
  - Consequences / risks: Tied to ASP.NET Core cookie auth internals, but this is stable API surface.

## Files Changed
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs` — Added `OAuthCallback_WithUnverifiedEmail_ShouldRedirectWithEmailNotVerifiedError` test and required using statements

## Tests
### Integration
- `OAuthCallback_WithUnverifiedEmail_ShouldRedirectWithEmailNotVerifiedError` — Forges external OAuth cookie with `email_verified=false`, calls callback, verifies redirect with `oauthError=email_not_verified`

## Checklist
- [x] Task scope matches `docs/tasks/audit-0007-oauth-unverified-email-test.md`
- [x] Tests updated and passing
- [x] No secrets committed
