# CR-0027: OAuth Social Login and Docker Full-Stack Default Review

## PR Metadata
- PR: [#217](https://github.com/Monkey-D-Luisi/saas-template/pull/217)
- Title: `feat(auth): implement oauth social login and docker full-stack default`
- Branch: `feat/oauth-social-login-0027` -> `main`
- State: OPEN
- CI snapshot during review:
  - Detect Changes: SUCCESS
  - AI API - Build & Test: SKIPPED
  - Orgs API - Build & Test: SUCCESS
  - Web - Build & Test: FAILURE

## Review Comment Counts (Mandatory Verification)
- Inline review comments (source 1): `9`
- General reviews (source 2): `2`
- Issue comments (source 3): `2`

## Changed Files (47)
- `.agent.md`
- `.agent/rules/autonomous-workflow.md`
- `.agent/rules/coding-standards.md`
- `.agent/rules/pr-workflow.md`
- `.env.example`
- `.github/copilot-instructions.md`
- `AGENTS.md`
- `CLAUDE.md`
- `GEMINI.md`
- `QUICKSTART.md`
- `README.md`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/app/[locale]/register/page.tsx`
- `apps/web/src/components/auth/LoginForm.tsx`
- `apps/web/src/components/auth/OAuthButtons.tsx`
- `apps/web/src/components/auth/RegisterForm.tsx`
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx`
- `apps/web/src/components/auth/__tests__/OAuthButtons.test.tsx`
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`
- `docs/backlog/epic-002-auth-security.md`
- `docs/tasks/0027-oauth-social-login.md`
- `docs/walkthroughs/0027-oauth-social-login.md`
- `scripts/dev-up.ps1`
- `scripts/dev-up.sh`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConstants.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/SaasTemplate.OrgsApi.Api.csproj`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.Development.json`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/OAuthLogin/OAuthLoginCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/OAuthLogin/OAuthLoginCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Interfaces/IUserExternalLoginRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/UserExternalLogin.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260207210317_AddUserExternalLogins.Designer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260207210317_AddUserExternalLogins.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserExternalLoginConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/UserExternalLoginRepository.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/OAuthLoginCommandHandlerTests.cs`

## Review Threads

### Source 1: Inline Comments (9)
1. `2778219997` - gemini-code-assist - `OAuthConfiguration.cs:19`
   - Claim: external OAuth cookie secure policy should be hardened.
   - Classification: MUST_FIX.
2. `2778220002` - gemini-code-assist - `apps/web/messages/es.json:72`
   - Claim: missing accents in Spanish strings.
   - Classification: MUST_FIX.
3. `2778222196` - Copilot - `apps/web/messages/es.json:72`
   - Claim: missing accents in `oauthError`.
   - Classification: MUST_FIX.
4. `2778222203` - Copilot - `apps/web/messages/es.json:72`
   - Claim: missing accents in `providerAlreadyLinked`.
   - Classification: MUST_FIX.
5. `2778222208` - Copilot - `AuthController.cs:168`
   - Claim: callback provider is not bound to authenticated external ticket principal.
   - Classification: MUST_FIX.
6. `2778222212` - Copilot - `AuthController.cs:209`
   - Claim: `catch (Exception)` captures cancellations and misreports as OAuth failure.
   - Classification: MUST_FIX.
7. `2778222216` - Copilot - `docs/tasks/0027-oauth-social-login.md:186`
   - Claim: task immutability/process concern.
   - Classification: SHOULD_FIX (process/documentation).
8. `2778222219` - Copilot - `AuthController.cs:363`
   - Claim: redundant local assignment (`decoded`).
   - Classification: SUGGESTION.
9. `2778222229` - Copilot - `RegisterForm.tsx:27`
   - Claim: redundant local assignment (`decoded`).
   - Classification: SUGGESTION.

### Source 2: General Reviews (2)
1. `3768415213` - gemini-code-assist - COMMENTED
   - Summary: positive overall; flagged cookie secure policy and Spanish typos.
   - Classification: MUST_FIX.
2. `3768421088` - copilot-pull-request-reviewer - COMMENTED
   - Summary: positive overall; flagged provider binding, cancellation behavior, and minor cleanups.
   - Classification: MUST_FIX + SUGGESTION.

### Source 3: Issue Comments (2)
1. `IC_kwDOP0hK4s7malqU` - chatgpt-codex-connector
   - Summary: usage limit notification (non-actionable).
   - Classification: OUT_OF_SCOPE.
2. `IC_kwDOP0hK4s7mal6b` - gemini-code-assist
   - Summary: auto-generated PR summary (non-actionable).
   - Classification: OUT_OF_SCOPE.

## Comment Resolution Plan

### MUST_FIX
- [x] Bind callback provider to the external auth ticket/principal and reject mismatch.
- [x] Harden external OAuth cookie secure policy for production-like environments.
- [x] Correct Spanish OAuth error message accents.
- [x] Handle request cancellation separately from true OAuth errors.

### SHOULD_FIX
- [x] Capture process issue regarding task-doc immutability handling in documentation workflow.

### SUGGESTION
- [x] Remove redundant `decoded` initializations in touched auth components/controllers.

### OUT_OF_SCOPE
- [x] Ignore non-actionable issue comments.

## Status
- COMPLETE
