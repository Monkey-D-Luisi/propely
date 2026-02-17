# Task: 0019 - Backend Test Coverage Improvement

## Metadata
- ID: 0019
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0019-backend-tests.md`

## Goal
Significantly improve backend test coverage by adding unit tests for all command/query handlers and integration tests for all API endpoints. Target coverage: Domain >90%, Application >80%, Infrastructure >60%.

## Context
The backend has architecture tests and basic integration test infrastructure (WebApplicationFactory + Testcontainers) but minimal actual test coverage. After tasks 0015-0018 added pagination, soft delete, audit logging, and exception handling, we need comprehensive tests.

## Scope
### In scope
- Unit tests for all command handlers (RegisterUser, LoginUser, CreateOrg, CreateInvitation, AcceptInvitation, UpdateMemberRole)
- Unit tests for all query handlers (GetCurrentUser, GetMyOrgs, GetMembers)
- Integration tests for all API endpoints via WebApplicationFactory
- Test the new pagination, search, soft delete, and audit logging features
- Test the exception handler middleware

### Out of scope
- Frontend tests (tasks 0012-0014)
- Performance/load tests
- E2E tests

## Requirements
- R1: Every handler has at least one success test and one failure test
- R2: Integration tests use Testcontainers for real PostgreSQL
- R3: Tests follow AAA pattern (Arrange, Act, Assert)
- R4: Test names describe the scenario: `Should_ReturnPagedMembers_When_PageSizeIsSpecified`

## Acceptance Criteria
- AC1: `dotnet test` passes with all new tests
- AC2: Domain layer coverage > 90%
- AC3: Application layer coverage > 80%
- AC4: Every API endpoint has at least one integration test
- AC5: Architecture tests still pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Add unit tests for each command handler
2. Add unit tests for each query handler
3. Add integration tests for auth endpoints
4. Add integration tests for org endpoints
5. Add integration tests for member endpoints
6. Run coverage report and verify thresholds

## Files to Create / Modify
### orgs-api tests
- `tests/UnitTests/Users/Commands/` (create handler tests)
- `tests/UnitTests/Orgs/Commands/` (create handler tests)
- `tests/UnitTests/Orgs/Queries/` (create handler tests)
- `tests/IntegrationTests/Endpoints/` (create endpoint tests)

## Testing Plan
- This IS the testing task. Run `dotnet test` with coverage collection.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
