# Task: 0018 - Global Exception Handler and Standardized Error Responses

## Metadata
- ID: 0018
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0018-global-exception-handler.md`

## Goal
Create a global exception handler middleware that catches all unhandled exceptions and returns standardized RFC 7807 ProblemDetails responses. Remove per-controller try/catch blocks and use domain exceptions that map to HTTP status codes.

## Context
Currently error handling is scattered across controllers with repetitive try/catch blocks. Some errors return inconsistent formats. A global exception handler centralizes error handling and ensures consistent, parseable error responses.

## Scope
### In scope
- Create `ExceptionHandlerMiddleware` for both services
- Define domain exception hierarchy: `DomainException`, `NotFoundException`, `ConflictException`, `ForbiddenException`, `ValidationException`
- Map domain exceptions to HTTP status codes and ProblemDetails responses
- Remove per-controller try/catch blocks (handlers throw domain exceptions)
- Ensure error responses follow RFC 7807 format: `{ type, title, status, detail, instance }`
- Log exceptions with appropriate severity

### Out of scope
- Changing frontend error handling (it already handles HTTP status codes)
- Adding custom error codes (existing error strings like "EMAIL_TAKEN" are fine)

## Requirements
- R1: All unhandled exceptions return ProblemDetails JSON
- R2: `NotFoundException` -> 404, `ConflictException` -> 409, `ForbiddenException` -> 403
- R3: `ValidationException` -> 400 with validation errors in `errors` extension
- R4: Unknown exceptions -> 500 with generic message (no stack trace in production)
- R5: All exceptions are logged with correlation ID
- R6: Development environment includes exception details; production does not

## Acceptance Criteria
- AC1: A NotFoundException in a handler returns 404 ProblemDetails JSON
- AC2: An unhandled exception returns 500 ProblemDetails JSON
- AC3: No try/catch blocks remain in controllers (except for specific non-exception flows)
- AC4: `dotnet build` succeeds for both services
- AC5: `dotnet test` passes for both services

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create exception classes in Domain/Common/Exceptions/
2. Create ExceptionHandlerMiddleware in Api/Middleware/
3. Register middleware in Program.cs
4. Refactor controllers to remove try/catch, let handlers throw domain exceptions
5. Refactor handlers to throw domain exceptions instead of returning error results
6. Test all error paths

## Files to Create / Modify
### Both services
- `Domain/Common/Exceptions/DomainException.cs` (create)
- `Domain/Common/Exceptions/NotFoundException.cs` (create)
- `Domain/Common/Exceptions/ConflictException.cs` (create)
- `Domain/Common/Exceptions/ForbiddenException.cs` (create)
- `Api/Middleware/ExceptionHandlerMiddleware.cs` (create)
- `Api/Program.cs` (modify - register middleware)
- `Api/Controllers/*.cs` (modify - remove try/catch)
- `Application/*/Handlers/*.cs` (modify - throw domain exceptions)

## Testing Plan
- Unit tests: Middleware maps exceptions to correct status codes
- Integration tests: API returns correct ProblemDetails for error scenarios

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
