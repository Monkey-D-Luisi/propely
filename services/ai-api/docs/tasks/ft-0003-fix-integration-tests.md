# Task: ft-0003-fix-integration-tests

## Metadata
- ID: ft-0003
- Type: FastTrack
- Status: DONE
- Owner: Agent
- Created: 2025-01-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/ft-0003-fix-integration-tests.md`
  - PR: https://github.com/Monkey-D-Luisi/ai-api-template/pull/14

## Goal
Fix all 18 failing integration tests to restore green CI status.

## Context
The integration test suite had 18 failing tests due to:
1. SQL raw queries using PascalCase names while EF Core configuration uses snake_case
2. Testcontainers builders using deprecated constructor syntax
3. ApiWebApplicationFactory not mocking external dependencies (RabbitMQ, Redis)
4. WorkItemProjectorTests using `MigrateAsync()` when the test fixtures use `EnsureCreatedAsync()`

## Scope
### In scope
- Fix OutboxRepository SQL to use correct table/column names
- Update Testcontainers to use `WithImage()` syntax
- Mock external services in API integration tests
- Fix database initialization in WorkItemProjectorTests

### Out of scope
- Adding new tests
- Refactoring existing test patterns
- Updating package versions

## Acceptance Criteria
- [x] All 79 tests pass (47 unit + 32 integration)
- [x] No breaking changes to production code
- [x] Build passes without errors

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/SaasTemplate.AiApi.IntegrationTests.csproj`
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/PostgresFixture.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/RabbitMqFixture.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`

## Definition of Done
- [x] Goal achieved (all tests pass)
- [x] No secrets committed
- [x] Walkthrough updated
- [x] PR created and pushed
