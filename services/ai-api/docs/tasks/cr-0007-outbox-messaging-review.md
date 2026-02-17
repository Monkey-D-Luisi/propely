# Code Review: cr-0007-outbox-messaging-review

## Metadata
- PR: #10 - feat(messaging): implement Outbox Dispatcher with RabbitMQ publishing (#0007)
- PR Link: https://github.com/Monkey-D-Luisi/ai-api-template/pull/10
- Target Branch: main
- Head Branch: feat/0007-outbox-dispatcher
- CI Status: pending
- Review Date: 2026-01-29

## Changed Files
- `docs/backlog/work-item-management-*.md`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs`
- `tests/SaasTemplate.AiApi.UnitTests/Messaging/OutboxDispatcherServiceTests.cs`

## Review Sources
- Review Comments: 4
- Reviews: 2 (gemini-code-assist, Copilot)
- Issue Comments: 1

---

## Comment Resolution Plan

### MUST_FIX
- [x] [Comment #2](https://github.com/Monkey-D-Luisi/ai-api-template/pull/10#discussion_r2740788835): **P1 - Guard against concurrent dispatchers claiming same rows**
  - Author: chatgpt-codex-connector[bot]
  - File: `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` (lines 28-32)
  - Issue: When multiple API instances run the dispatcher, `GetUnprocessedMessagesAsync` can return the same rows to different instances, causing duplicate event publishing.
  - **FIXED**: Implemented `FOR UPDATE SKIP LOCKED` pattern using raw SQL.

### SHOULD_FIX
- [x] [Comment #3](https://github.com/Monkey-D-Luisi/ai-api-template/pull/10#discussion_r2740806411): **Separate error handling for publish vs database operations**
  - Author: Copilot
  - File: `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs` (lines 102-130)
  - Issue: If `SaveChangesAsync` fails after `PublishAsync` succeeds, the message remains unprocessed, causing duplicate publishing on next poll. Current error handling doesn't distinguish between publish and database failures.
  - **FIXED**: Added separate try-catch blocks for publish and database operations with distinct error logging.

- [x] [Comment #1](https://github.com/Monkey-D-Luisi/ai-api-template/pull/10#discussion_r2740781453): **Assert against parsed JSON, not raw string**
  - Author: gemini-code-assist[bot]
  - File: `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs` (line 116)
  - Issue: Asserting against raw JSON string can make the test brittle due to formatting changes.
  - **FIXED**: Replaced string assertion with `JsonDocument.Parse` and property assertions.

### SUGGESTION
- [ ] [Comment #4](https://github.com/Monkey-D-Luisi/ai-api-template/pull/10#discussion_r2740806451): **SemaphoreSlim disposal best practice**
  - Author: Copilot
  - File: `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs` (line 17)
  - Issue: `SemaphoreSlim _connectionLock` is properly disposed in `DisposeAsync`, but as a best practice concerns around construction exceptions are raised.
  - Action: Acknowledge - the current implementation is correct for singleton lifecycle. Respond explaining rationale.

### QUESTION
*(none)*

### OUT_OF_SCOPE
*(none)*

---

## Implementation Notes

- **2026-01-29**: Addressed all MUST_FIX and SHOULD_FIX items
  - `OutboxRepository.cs`: Changed from LINQ to raw SQL with `FOR UPDATE SKIP LOCKED`
  - `OutboxDispatcherService.cs`: Split single try-catch into two for better failure diagnostics
  - `OutboxDispatcherTests.cs`: Now uses `JsonDocument.Parse` for robust assertions
- All 45 unit tests pass

## Commits
*(to be filled after commits)*
