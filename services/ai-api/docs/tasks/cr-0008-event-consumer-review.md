# Code Review: cr-0008-event-consumer-review

## Metadata
- PR: #11 - feat(messaging): implement Event Consumer and Read Model Projection (#0008)
- PR Link: https://github.com/Monkey-D-Luisi/ai-api-template/pull/11
- Target Branch: main
- CI Status: passing
- Review Date: 2026-01-29

## Changed Files
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/ProcessedEvent.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/ProcessedEventConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/*`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/ProjectorConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
- `src/SaasTemplate.AiApi.Api/Program.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`

## Review Sources
- Review Comments: 2 (gemini-code-assist, Copilot)
- Reviews: 2 (gemini-code-assist, Copilot)
- Issue Comments: 0

## Comment Resolution Plan

### MUST_FIX
_(none identified)_

### SHOULD_FIX
- [x] Lines 210-219: Unknown event types behavior
  - File: `WorkItemProjectorService.cs`
  - Issue: Unknown event types are logged as warning but still ACK'd
  - Proposed change: Consider if unknown events should be ACK'd or NACK'd - current behavior is reasonable (ACK to avoid infinite retry on events we don't handle)
  - **Decision**: Keep current behavior. Documentation comment explains rationale.

### SUGGESTION
- [x] DLQ documentation: Add XML doc comment explaining DLQ behavior
  - File: `WorkItemProjectorService.cs`
  - Action: Added summary comment for `OnMessageReceivedAsync`

### QUESTION
_(none)_

### OUT_OF_SCOPE
_(none)_

## Implementation Notes
The Copilot review on lines 210-219 asks about handling unknown event types. Current implementation:
1. Unknown events are logged with `LogWarning`
2. They are still ACK'd (happens in caller after `ProcessMessageAsync` returns normally)

This is the **correct behavior** because:
- ACKing unknown events prevents infinite retries
- The queue receives all events (`#` routing key)
- Future event types will be logged until we add handlers
- Only processing errors should go to DLQ (via NACK)

## Commits
- `0e67910`: docs(messaging): add XML documentation for message handling strategy (#cr-0008)
