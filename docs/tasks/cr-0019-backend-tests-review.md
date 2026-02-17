# Code Review: cr-0019 - Backend Tests Review

## Metadata
- PR: #159 (`test(orgs-api): add comprehensive unit and integration tests (#0019)`)
- Branch: `feat/0019-backend-tests` → `main`
- Reviewers: Copilot (15 comments), Gemini Code Assist (2 comments)
- Total inline review comments: 17

## Changed Files
- `services/orgs-api/tests/.../Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/.../Api/OrgsEndpointTests.cs`
- `services/orgs-api/tests/.../Fixtures/ApiWebApplicationFactory.cs`
- `services/orgs-api/tests/.../Users/Commands/LoginUserCommandHandlerTests.cs`
- `services/orgs-api/tests/.../Users/Commands/RegisterUserCommandHandlerTests.cs`
- `services/orgs-api/tests/.../Users/Queries/GetCurrentUserQueryHandlerTests.cs`
- `services/orgs-api/tests/.../Organizations/Commands/*.cs` (4 files)
- `services/orgs-api/tests/.../Organizations/Queries/*.cs` (2 files)
- `services/orgs-api/src/.../Repositories/MembershipRepository.cs`
- `docs/walkthroughs/0019-backend-tests.md`
- `docs/tasks/0019-backend-tests.md`
- `docs/backlog/epic-001-professional-saas-refinement.md`

## Comment Resolution Plan

### SHOULD_FIX

- [x] **UpdateMemberRole assertion loop missing "found" flag** (Copilot #2777278812, Gemini #2777283521)
  - OrgsEndpointTests.cs:274 — foreach loop won't fail if member is missing from response
  - Fix: Add `found` flag + final assertion, or use FluentAssertions LINQ

- [x] **GetMyOrgs assertion loop same pattern** (Copilot #2777278820)
  - OrgsEndpointTests.cs:110 — foreach implicitly filters, should use explicit approach
  - Fix: Refactor to use FluentAssertions `.Should().ContainSingle()`

- [x] **JWT secret hardcoded/duplicated** (Copilot #2777278816, Gemini #2777283523)
  - ApiWebApplicationFactory.cs:104 — same literal string on line 58 and line 104
  - Fix: Extract to const field, reference from both places

- [x] **LoginUser test name mismatch** (Copilot #2777278817)
  - LoginUserCommandHandlerTests.cs:82 — name says "ShouldNotCallPasswordHasherOrTokenService" but only asserts token service
  - Fix: Add `_passwordHasher.DidNotReceive()` assertion

- [x] **HttpRequestMessage not disposed** (Copilot #2777278832, #2777278839, #2777278850, #2777278860, #2777278868, #2777278878, #2777278887)
  - 6 instances in AuthEndpointTests.cs, 1 in OrgsEndpointTests.cs
  - Fix: Add `using` keyword to all `new HttpRequestMessage(...)` declarations

- [x] **Unused variable assignments** (Copilot #2777278896, #2777278905, #2777278913)
  - AuthEndpointTests.cs:197 — `userId` not used
  - OrgsEndpointTests.cs:138 — `userId` not used
  - OrgsEndpointTests.cs:239 — `ownerUserId` not used
  - Fix: Use discard `_` for unused tuple elements

### SUGGESTION (addressed above)

- [x] **Foreach loop filtering style** (Copilot #2777278824)
  - OrgsEndpointTests.cs:274 — addressed by the SHOULD_FIX for the same location
