# Code Review: cr-0034-epic-002-audit-pr-review

## Metadata
- PR: #231
- Title: fix(security): Epic 002 audit remediation & audit workflow
- Branch: `fix/epic-002-audit-remediation` → `main`
- CI Status: QUEUED (Detect Changes)
- Reviewers: gemini-code-assist[bot], copilot-pull-request-reviewer[bot]

## Changed Files (25)
- `.agent/rules/audit-action-workflow.md`
- `.agent/rules/audit-epic-workflow.md`
- `.agent/templates/audit-action-template.md`
- `.agent/templates/audit-executive-summary-template.md`
- `AGENTS.md`
- `CLAUDE.md`
- `docs/audits/epic-002-executive-summary.md`
- `docs/tasks/0029-email-verification.md`
- `docs/tasks/0030-auth-rate-limiting.md`
- `docs/tasks/audit-0001-cookie-secure-flag.md`
- `docs/tasks/audit-0002-timing-attack-mitigation.md`
- `docs/tasks/audit-0003-max-password-length.md`
- `docs/tasks/audit-0004-csrf-rejection-tests.md`
- `docs/walkthroughs/audit-0001-cookie-secure-flag.md`
- `docs/walkthroughs/audit-0002-timing-attack-mitigation.md`
- `docs/walkthroughs/audit-0003-max-password-length.md`
- `docs/walkthroughs/audit-0004-csrf-rejection-tests.md`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ChangePasswordRequestValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/RegisterRequestValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ResetPasswordRequestValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommandValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/LoginUserCommandHandlerTests.cs`

## Review Threads (7 inline comments)

### Unresolved

1. **[GEMINI #2782568128] LoginUserCommandHandler.cs:10** — TimingSafetyHash is not a valid bcrypt hash. 'x' characters are invalid in bcrypt base64 alphabet, BCrypt.Verify will throw exception, reintroducing timing difference.
2. **[COPILOT #2782666556] LoginUserCommandHandler.cs:10** — Same issue as above. Suggests replacement with real pre-computed hash.
3. **[COPILOT #2782666600] AuthController.cs:64** — Use `IsEnvironment("Testing")` instead of string comparison for case-insensitive consistency with `OAuthConfiguration.cs`.
4. **[COPILOT #2782666624] LoginUserCommandHandlerTests.cs:93** — Tighten assertion to verify hash argument starts with `$2` instead of `Arg.Any<string>()`.
5. **[GEMINI #2782568145] audit-action-workflow.md:43** — Use `<NNNN>` placeholder instead of `<action-number>` for consistency.
6. **[COPILOT #2782666639] audit-epic-workflow.md:104** — Executive summary filename inconsistency in Step 3 vs actual convention.
7. **[COPILOT #2782666654] audit-action-workflow.md:128** — File naming table lacks directory paths to distinguish tasks from walkthroughs.

## Comment Resolution Plan

### MUST_FIX
- [x] #1 + #2: Replace `TimingSafetyHash` with a real, valid pre-computed bcrypt hash (CRITICAL — current hash will throw exception)

### SHOULD_FIX
- [x] #3: Use `IsEnvironment("Testing")` instead of `EnvironmentName != "Testing"` for case-insensitive consistency
- [x] #4: Tighten unit test assertion to verify hash argument starts with `$2` (bcrypt format)
- [x] #5: Use `<NNNN>` placeholder in branch naming convention (consistency)
- [x] #6: Align executive summary filename in audit-epic-workflow.md Step 3
- [x] #7: Add directory paths to file naming convention table

### SUGGESTION
- (none)

### QUESTION
- (none)

### OUT_OF_SCOPE
- (none)

## Parity Verification Checklist
- [x] Redirect parity checked — N/A, no redirect changes in this PR
- [x] Locale source correctness checked — N/A, no locale changes
- [x] API/UI contract parity checked — N/A, no new API fields/frontend fields
- [x] Test parity checked — CSRF rejection tests added for auth POST endpoints; unit test updated for timing safety
