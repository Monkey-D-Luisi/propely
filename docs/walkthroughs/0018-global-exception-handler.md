# Walkthrough: 0018-global-exception-handler

## Task Reference
- Task: `docs/tasks/0018-global-exception-handler.md`
- Walkthrough: `docs/walkthroughs/0018-global-exception-handler.md`
- Branch/PR: `feat/0018-global-exception-handler`
- Date: `2026-02-06`

## Summary
Implemented a global exception handler middleware for both `orgs-api` and `ai-api` services that catches unhandled exceptions and returns standardized RFC 7807 ProblemDetails responses. Created a domain exception hierarchy (`DomainException`, `NotFoundException`, `ConflictException`, `ForbiddenException`) and removed all per-controller try/catch blocks, letting exceptions propagate to the middleware for centralized handling.

## Context
- Background: Error handling was scattered across controllers with repetitive try/catch blocks. Different endpoints returned inconsistent error formats. Handlers threw standard .NET exceptions (`KeyNotFoundException`, `InvalidOperationException`) rather than domain-specific types.
- Problem statement: Need centralized, consistent error responses following RFC 7807 with proper exception-to-HTTP-status mapping and structured logging.
- Constraints: Must maintain same HTTP status codes for existing error paths. Frontend must not need changes (it already handles HTTP status codes).

## Decisions & Trade-offs
- **Decision: Domain exception hierarchy vs. Result<T> pattern**
  - Options considered: (a) Domain exception hierarchy with middleware, (b) ErrorOr/Result<T> wrapper pattern
  - Why this choice: Existing codebase already uses exception-based flow. Domain exceptions + middleware is simpler to retrofit and aligns with the current architecture. Result<T> would require rewriting every handler signature.
  - Consequences / risks: Exception flow is slightly less explicit than Result<T>, but the middleware makes it predictable.

- **Decision: ForbiddenException base class change**
  - Changed `ForbiddenException : Exception` to `ForbiddenException : DomainException`
  - This is a behavioral breaking change: `catch (DomainException)` blocks and `is DomainException` checks now also match `ForbiddenException`. Since we're removing per-controller try/catch in favor of centralized middleware, this is acceptable.

- **Decision: OperationCanceledException mapping to 499**
  - Maps client disconnections to 499 (Client Closed Request) to distinguish from server errors.

## Implementation Notes

### Domain Layer (both services)
- Created `DomainException` (base, → 400), `NotFoundException` (→ 404), `ConflictException` (→ 409)
- Modified `ForbiddenException` to extend `DomainException` instead of `Exception`

### API Layer (both services)
- Created `ExceptionHandlerMiddleware` in `Api/Middleware/`
- Registered after `UseCorrelationId()` to capture correlation IDs in error logs
- Exception mapping: `NotFoundException → 404`, `ConflictException → 409`, `ForbiddenException → 403`, `UnauthorizedAccessException → 401`, `DomainException → 400`, `OperationCanceledException → 499`, unknown → 500
- Development: includes exception details + stack trace. Production: generic 500 message.
- All exceptions logged with correlation ID, method, and path.

### Handler Refactoring (orgs-api)
- `RegisterUserCommandHandler`: `InvalidOperationException("EMAIL_TAKEN")` → `ConflictException("EMAIL_TAKEN")`
- `CreateInvitationCommandHandler`: `KeyNotFoundException` → `NotFoundException`
- `AcceptInvitationCommandHandler`: `InvalidOperationException` → `DomainException` (INVALID_TOKEN, ALREADY_USED, EXPIRED, USER_EMAIL_MISMATCH)
- `UpdateMemberRoleCommandHandler`: `KeyNotFoundException` → `NotFoundException`, `InvalidOperationException` → `DomainException`
- `GetMembersQueryHandler`: `UnauthorizedAccessException` → `ForbiddenException`

### Handler Refactoring (ai-api)
- `UpdateWorkItemCommandHandler`: `KeyNotFoundException` → `NotFoundException`
- `DeleteWorkItemCommandHandler`: `KeyNotFoundException` → `NotFoundException`

### Controller Refactoring
- Removed all try/catch blocks from `AuthController`, `OrgsController`, and `WorkItemsController`
- Kept validation checks (FluentValidation) and auth guards (GetUserId null checks) as those are not exception-based

### Edge cases handled
- `OperationCanceledException` returns 499 instead of 500
- Production environment strips exception details from 500 responses
- Outer try/catch in middleware ensures no exception escapes unhandled

### Known limitations
- Error response format changed from `{ error: "..." }` to ProblemDetails `{ type, title, status, detail, instance }`. Frontend may need updates if it parses the `error` field specifically.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
```

## Files Changed

### Both services — Domain exceptions
- `Domain/Common/Exceptions/DomainException.cs` — New base domain exception
- `Domain/Common/Exceptions/NotFoundException.cs` — New 404 exception
- `Domain/Common/Exceptions/ConflictException.cs` — New 409 exception
- `Domain/Common/Exceptions/ForbiddenException.cs` — Modified to extend DomainException

### Both services — Middleware
- `Api/Middleware/ExceptionHandlerMiddleware.cs` — New global exception handler middleware
- `Api/Program.cs` — Registered `UseGlobalExceptionHandler()` after correlation ID middleware

### orgs-api — Handler refactoring
- `Application/Users/Commands/RegisterUser/RegisterUserCommandHandler.cs` — ConflictException for EMAIL_TAKEN
- `Application/Organizations/Commands/CreateInvitation/CreateInvitationCommandHandler.cs` — NotFoundException
- `Application/Organizations/Commands/AcceptInvitation/AcceptInvitationCommandHandler.cs` — DomainException for validation codes
- `Application/Organizations/Commands/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs` — NotFoundException + DomainException
- `Application/Organizations/Queries/GetMembers/GetMembersQueryHandler.cs` — ForbiddenException

### orgs-api — Controller refactoring
- `Api/Controllers/AuthController.cs` — Removed try/catch from Register and Login
- `Api/Controllers/OrgsController.cs` — Removed try/catch from GetMembers, CreateInvitation, AcceptInvitation, UpdateMemberRole

### ai-api — Handler refactoring
- `Application/WorkItems/Commands/UpdateWorkItemCommandHandler.cs` — NotFoundException
- `Application/WorkItems/Commands/DeleteWorkItemCommandHandler.cs` — NotFoundException

### ai-api — Controller refactoring
- `Api/Controllers/WorkItemsController.cs` — Removed try/catch from UpdateWorkItem and DeleteWorkItem

### Tests
- `tests/*/UnitTests/Api/Middleware/ExceptionHandlerMiddlewareTests.cs` — New unit tests for middleware (both services)
- `tests/*/UnitTests/Application/WorkItems/*Tests.cs` — Updated to assert NotFoundException instead of KeyNotFoundException
- `tests/*/UnitTests/*.csproj` — Added Api project reference for middleware testing

## Tests
### Unit
- **orgs-api:** 31 tests (10 new for middleware: 5 exception type mappings + 5 behavior tests)
- **ai-api:** 75 tests (8 new for middleware + 2 updated for NotFoundException)
- How to run: `dotnet test services/*/SaasTemplate.*.sln`

### Integration
- **orgs-api:** 28 tests — all passing (unchanged, verify end-to-end error paths via middleware)
- **ai-api:** 56 tests — all passing

## Verification
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 64 tests passed (31 unit + 5 architecture + 28 integration)
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 136 tests passed (75 unit + 5 architecture + 56 integration)

## Security
- No new AuthN/AuthZ impact. Middleware preserves existing auth behavior.
- 500 error details stripped in production to prevent information leakage.
- All exceptions logged with correlation ID for traceability.

## Follow-ups / Backlog
- [ ] Frontend: update error parsing to use ProblemDetails `detail` field instead of `error` field
- [ ] Add FluentValidation pipeline behavior (MediatR) for automatic validation instead of manual checks in controllers
- [ ] Consider adding structured error codes as ProblemDetails extension

## Checklist
- [x] Task scope matches `docs/tasks/0018-global-exception-handler.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
