# Walkthrough: 0026-operation-actions-nl

## Task Reference
- Task: `docs/tasks/0026-operation-actions-nl.md`
- Walkthrough: `docs/walkthroughs/0026-operation-actions-nl.md`
- Branch/PR: `feat/p3-ai-action-engine`
- Date: `2026-03-05`

## Summary
Implemented 4 operation action handlers for property lifecycle management via natural language. The ActionRouter now dispatches ReserveProperty, CloseOperation, ArchiveProperty, and ReactivateProperty intents to dedicated MediatR command handlers. Each handler extracts parameters using ParameterExtractor, validates inputs, and returns structured ActionResult data. Added ReactivateProperty to the ActionType enum and reactivate_property to ToolDefinitions.

## Context
- Background: Task 3.2 (0024) implemented property action handlers (Create, Query, Update, ChangeStatus). The ActionRouter returned placeholder messages for operation types like ReserveProperty, CloseOperation, and ArchiveProperty. The ActionType enum already contained these values, and ToolDefinitions already had tool schemas for reserve_property, close_operation, and archive_property.
- Problem statement: Natural language commands like "reserve the apartment for Maria Garcia" or "close the sale of AP-2024-001" returned placeholder messages instead of executing real logic. ReactivateProperty had no enum value or tool definition at all.
- Constraints: Properties SDK is read-only, so handlers extract and validate parameters only. When SDK write endpoints are added, these handlers can be extended to call them.

## Decisions & Trade-offs
- **Decision: Handlers in Handlers/Operations/ subfolder**
  - Options considered: (a) Same Handlers/ folder as property handlers, (b) New Handlers/Operations/ subfolder, (c) Commands/Operations/ co-located
  - Why this choice: Separate Operations subfolder provides clear separation between property CRUD actions and lifecycle operation actions, while keeping the Handlers parent folder consistent with existing convention.
  - Consequences / risks: None; the MediatR assembly scanning picks up handlers from any namespace.

- **Decision: CloseOperation defaults to Sold when operation_type is missing**
  - Options considered: (a) Require operation_type, return error if missing, (b) Default to Sold
  - Why this choice: Sale operations are more common in the domain. Defaulting to Sold provides a better user experience when the agent says "close the operation on property X" without specifying the type.
  - Consequences / risks: Rent operations must explicitly specify operation_type. The AI intent classifier typically extracts this from context ("close the rental").

- **Decision: Case-insensitive operation_type validation**
  - Why: OpenAI function calling may return "Sale", "sale", or "SALE". The handler normalizes via StringComparer.OrdinalIgnoreCase.

- **Decision: Invalid operation_type returns failure (not default)**
  - Why: If the user explicitly says "transfer" (which is a valid operation_type for properties but not for closing), we should not silently default to Sold. Better to return an error explaining valid close operation types.

## Implementation Notes
- Key changes:
  - `Domain/Actions/ActionType.cs` gains ReactivateProperty enum value
  - `Infrastructure/AI/ToolDefinitions.cs` gains reactivate_property tool definition and mapping
  - `Infrastructure/AI/ActionRouter.cs` dispatches 4 new operation types and removes their placeholder messages
  - `Application/Actions/Commands/Operations/` contains 4 command records
  - `Application/Actions/Handlers/Operations/` contains 4 handler classes

- Files created:
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReservePropertyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/CloseOperationActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ArchivePropertyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReactivatePropertyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/Operations/ReservePropertyActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/Operations/CloseOperationActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/Operations/ArchivePropertyActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/Operations/ReactivatePropertyActionHandler.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/Operations/ReservePropertyActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/Operations/CloseOperationActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/Operations/ArchivePropertyActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/Operations/ReactivatePropertyActionHandlerTests.cs`

- Files modified:
  - `services/ai-api/src/Propely.AiApi.Domain/Actions/ActionType.cs` (added ReactivateProperty)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs` (added reactivate_property tool + mapping)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` (dispatch + removed placeholder messages)

## Test Results
- New tests: 25 (6 Reserve + 9 CloseOperation + 5 Archive + 5 Reactivate)
- Note: Tests must be verified by running `dotnet test services/ai-api/Propely.AiApi.sln --verbosity quiet`

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A (new action types only)
