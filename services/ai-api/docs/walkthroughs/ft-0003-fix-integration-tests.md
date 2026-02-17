# Walkthrough: ft-0003-fix-integration-tests

## Task Reference
- Task: `docs/tasks/ft-0003-fix-integration-tests.md`
- Walkthrough: `docs/walkthroughs/ft-0003-fix-integration-tests.md`
- Branch/PR: `fix/integration-tests` / https://github.com/Monkey-D-Luisi/ai-api-template/pull/14
- Date: 2025-01-20

## Summary
Fixed 18 failing integration tests by correcting SQL naming conventions, updating Testcontainers syntax, mocking external services, and fixing database initialization approach. All 79 tests now pass.

## Context
- **Background:** Integration tests were failing with `ObjectDisposedException` and PostgreSQL "relation does not exist" errors
- **Problem statement:** Multiple issues caused test failures:
  1. OutboxRepository SQL used PascalCase (`"OutboxMessages"`) but EF Core config uses snake_case (`outbox_messages`)
  2. Testcontainers builders used deprecated parameterless constructors
  3. ApiWebApplicationFactory didn't mock RabbitMQ/Redis, causing connection failures
  4. WorkItemProjectorTests called `MigrateAsync()` but project has no migrations
- **Constraints:** Must fix without removing tests or changing functionality

## Decisions & Trade-offs

### Decision 1: Mock External Services in ApiWebApplicationFactory
- **Options considered:**
  1. Start RabbitMQ/Redis containers for API tests
  2. Mock the service interfaces
- **Why this choice:** Mocking is simpler and faster; API controller tests don't need real messaging
- **Consequences:** Tests don't verify actual RabbitMQ/Redis connectivity (covered by dedicated integration tests)

### Decision 2: Use EnsureCreatedAsync Instead of MigrateAsync
- **Options considered:**
  1. Add proper migrations to the project
  2. Use EnsureCreatedAsync consistently
- **Why this choice:** Project design uses EnsureCreatedAsync; adding migrations is out of scope
- **Consequences:** Consistent with existing pattern in PostgresFixture

### Decision 3: Remove All HostedServices in Test Environment
- **Options considered:**
  1. Mock individual hosted services
  2. Remove all hosted services
- **Why this choice:** Hosted services (OutboxDispatcher, WorkItemProjector) require real connections; removing all is simpler
- **Consequences:** Background processing not tested via API integration tests

## Implementation Notes

### Key changes:

1. **OutboxRepository.cs** - Changed SQL from PascalCase to snake_case:
   ```sql
   -- Before
   SELECT * FROM "OutboxMessages" WHERE "ProcessedAtUtc" IS NULL
   
   -- After  
   SELECT * FROM outbox_messages WHERE processed_at_utc IS NULL
   ```

2. **Testcontainers Builders** - Updated to new syntax:
   ```csharp
   // Before (deprecated)
   new PostgreSqlBuilder("postgres:16-alpine")
   
   // After
   new PostgreSqlBuilder()
       .WithImage("postgres:16-alpine")
   ```

3. **ApiWebApplicationFactory** - Added service mocking:
   ```csharp
   services.RemoveAll<IHostedService>();
   services.RemoveAll<IMessagePublisher>();
   services.AddSingleton(Substitute.For<IMessagePublisher>());
   services.RemoveAll<ICacheService>();
   services.AddSingleton(Substitute.For<ICacheService>());
   ```

4. **WorkItemProjectorTests** - Fixed initialization:
   ```csharp
   // Before
   await _dbContext.Database.MigrateAsync();
   
   // After
   await _dbContext.Database.EnsureCreatedAsync();
   ```

### Code Review Improvements:
- Removed unused `using Microsoft.Extensions.DependencyInjection`
- Removed unused `QueueName` constant
- Renamed `*_Migration_*` tests to `*_TableExists_*` for accuracy
- Removed redundant `Assert.True(true)` statement
- Updated comments to reference "schema creation" instead of "migration"

## Commands Run
```bash
# Initial test run to identify failures
dotnet test

# Build verification
dotnet build

# Final test verification
dotnet test --no-build

# Git operations
git checkout -b fix/integration-tests
git add -A
git commit -m "fix: resolve integration test failures"
git push -u origin fix/integration-tests
gh pr create --title "fix: resolve integration test failures" --body "..."

# After code review
git commit -m "refactor: code review improvements for WorkItemProjectorTests"
git push
```

## Files Changed

| File | Change |
|------|--------|
| `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` | Fixed SQL to use snake_case table/column names |
| `tests/SaasTemplate.AiApi.IntegrationTests/SaasTemplate.AiApi.IntegrationTests.csproj` | Added NSubstitute package reference |
| `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` | Updated builder syntax, added service mocking, removed hosted services |
| `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/PostgresFixture.cs` | Updated builder to use `WithImage()` syntax |
| `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/RabbitMqFixture.cs` | Updated builder to use `WithImage()` syntax |
| `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs` | Changed to `EnsureCreatedAsync()`, code review cleanup |

## Tests

### Unit
- No changes to unit tests
- All 47 unit tests pass

### Integration
- Fixed all 32 integration tests
- No new tests added

### Manual
- Verified all tests pass: `dotnet test`
- Verified build succeeds: `dotnet build`

## Observability
- No changes to logging, metrics, or traces

## Security
- No security-related changes
- No secrets committed

## Performance
- No performance impact
- Tests run slightly faster due to mocked services

## Docs Updated
- Created `docs/tasks/ft-0003-fix-integration-tests.md`
- Created `docs/walkthroughs/ft-0003-fix-integration-tests.md`

## Rollback Plan
- Revert PR #14 if issues discovered
- No data migrations to rollback
