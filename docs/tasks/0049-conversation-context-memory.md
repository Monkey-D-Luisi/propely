# Task: 0049-conversation-context-memory

## Metadata
- ID: 0049
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-08
- Related docs:
  - Walkthrough: `docs/walkthroughs/0049-conversation-context-memory.md`
  - Epic: `docs/backlog/epic-P7-intelligence-analytics.md` (Task 7.2)

## Goal
Add multi-turn conversation support to the AI action pipeline so users can have contextual follow-up commands within a session (e.g., "Create a villa in Marbella" → "Change the price to 300k").

## Context
The action pipeline was stateless: each `ExecuteActionCommand(Text, TenantId, AgentId)` was processed independently, with no memory of prior interactions. This made follow-up commands impossible — the AI had no way to know what "it" referred to or what the previous action was.

## Scope
### In scope
- Optional `SessionId` parameter on `ExecuteActionCommand` (backward compatible)
- `ConversationSession` model with exchange history and entity memory
- `IConversationContext` interface and Redis-backed implementation
- History passed to `IIntentClassifier.ClassifyAsync` as prior user/assistant message pairs
- Session loaded before classification, saved after routing
- Entity memory tracking (last property/contact/lead/appointment IDs)
- Frontend `sessionId` generation in CommandBar (UUID per session, reset on close)
- Both text and voice endpoints support `sessionId`

### Out of scope
- Entity memory used for pronoun resolution (future enhancement)
- Cross-session memory or persistent user preferences
- Session management UI (list/delete sessions)

## Requirements
- R1: Commands without `SessionId` work identically to before (stateless)
- R2: Commands with `SessionId` load prior exchanges and pass as history to classifier
- R3: After successful routing, the exchange is saved to the session
- R4: Session uses Redis with sliding TTL (default 30 min, configurable)
- R5: Conversation window limited to last N exchanges (default 10, configurable)
- R6: Frontend generates a UUID session ID on CommandBar open, resets on close

## Changes

### New files (ai-api)
| File | Purpose |
|------|---------|
| `Application/Actions/Models/ConversationExchange.cs` | Record: UserText, ActionType, ResultMessage, Success, Timestamp |
| `Application/Actions/Models/EntityMemory.cs` | Record: LastPropertyId?, LastContactId?, LastLeadId?, LastAppointmentId? |
| `Application/Actions/Models/ConversationSession.cs` | Session with exchange list, entity memory, AddExchange with window |
| `Application/Actions/Models/ConversationContextOptions.cs` | Config: MaxExchanges (10), SessionTtlMinutes (30) |
| `Application/Actions/Interfaces/IConversationContext.cs` | GetAsync/SaveAsync interface |
| `Infrastructure/AI/RedisConversationContext.cs` | Redis-backed implementation via ICacheService |

### New test files
| File | What it tests |
|------|---------------|
| `UnitTests/Application/Actions/ConversationSessionTests.cs` | AddExchange, window trim, entity memory |
| `UnitTests/Infrastructure/AI/RedisConversationContextTests.cs` | Cache delegation, tenant isolation |

### Modified files
| File | Change |
|------|--------|
| `Application/Actions/Commands/ExecuteAction/ExecuteActionCommand.cs` | Added `string? SessionId = null` |
| `Application/Actions/Commands/ExecuteAction/ExecuteActionCommandHandler.cs` | Injected IConversationContext; load/save session; pass history to classifier |
| `Application/Actions/Interfaces/IIntentClassifier.cs` | Added history parameter |
| `Infrastructure/AI/OpenAiIntentClassifier.cs` | Accepts history; inserts as user/assistant messages |
| `Api/Dtos/ActionDtos.cs` | Added SessionId to ExecuteActionRequest |
| `Api/Controllers/ActionsController.cs` | Passes SessionId to command |
| `Api/Controllers/VoiceController.cs` | Accepts sessionId from form data |
| `Infrastructure/DependencyInjection.cs` | Registered IConversationContext + options |
| `UnitTests/.../ExecuteActionCommandHandlerTests.cs` | Updated for new constructor + conversation tests |

### Frontend changes
| File | Change |
|------|--------|
| `hooks/use-execute-action.ts` | `execute` accepts optional `sessionId` |
| `hooks/use-execute-voice-action.ts` | `executeVoice` accepts optional `sessionId` |
| `hooks/use-command-bar.ts` | `sessionId` state (UUID on open, null on close) |
| `components/command-bar/CommandBar.tsx` | Passes `sessionId` to execute/executeVoice |
| `components/command-bar/__tests__/CommandBar.test.tsx` | Updated mock + assertion |

## Verification
```bash
dotnet test services/ai-api/Propely.AiApi.sln  # 655 tests pass
cd apps/web && npm test                         # 824 tests pass
```
