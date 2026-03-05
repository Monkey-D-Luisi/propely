# Task: 0026-operation-actions-nl

## Metadata
- ID: 0026
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0026-operation-actions-nl.md`
  - Epic: `docs/backlog/epic-P3-ai-engine.md` (Task 3.5)

## Goal
Implement operation action handlers for property lifecycle management via natural language: reserve, close operation (sell/rent), archive, and reactivate. These handlers extract parameters from classified NL intents and execute property status transitions through the AI action engine.

## Context
Task 3.2 (0024) established property action handlers (create, query, update, change status) dispatched by the ActionRouter via MediatR. The ActionRouter returned placeholder messages for operation-related action types (ReserveProperty, CloseOperation, ArchiveProperty). This task replaces those placeholders with real handlers and adds a new ReactivateProperty action type. The operation handlers follow the same pattern as property action handlers: extract parameters via ParameterExtractor, validate inputs, and return structured ActionResult data.

## Scope
### In scope
- ReservePropertyActionHandler -- "reserve the apartment on Calle Mayor for Maria Garcia"
- CloseOperationActionHandler -- "close the sale of AP-2024-001" (infers Sold for Sale, Rented for Rent)
- ArchivePropertyActionHandler -- "archive property AP-2024-015"
- ReactivatePropertyActionHandler -- "relist the apartment on Calle Mayor"
- ReactivateProperty added to ActionType enum
- reactivate_property tool definition added to ToolDefinitions
- ActionRouter updated to dispatch all 4 operation action types to MediatR
- Unit tests for all 4 handlers

### Out of scope
- Actual SDK calls to Properties-API write endpoints (not yet available)
- Contact/lead/appointment action handlers (Tasks 3.6+)
- Voice input processing (Task 3.4)

## Requirements
- R1: ActionRouter dispatches ReserveProperty, CloseOperation, ArchiveProperty, ReactivateProperty to MediatR handlers
- R2: Placeholder messages removed for the 4 implemented operation types
- R3: ReservePropertyActionHandler extracts property_id/reference and optional contact_name
- R4: CloseOperationActionHandler infers target status from operation_type (Sale -> Sold, Rent -> Rented, default Sold)
- R5: ArchivePropertyActionHandler extracts property identifier and returns archive confirmation
- R6: ReactivatePropertyActionHandler extracts property identifier and returns reactivation to Active status
- R7: All handlers validate required property identifier (ID or reference) and return appropriate error messages
- R8: CloseOperationActionHandler validates operation_type against known values (sale, rent) case-insensitively
- R9: ReactivateProperty enum value added to ActionType
- R10: reactivate_property tool definition added to ToolDefinitions with function name mapping

## Technical Details
- Pattern: MediatR CQRS commands dispatched by ActionRouter
- Namespace: Propely.AiApi.Application.Actions.Commands.Operations, Propely.AiApi.Application.Actions.Handlers.Operations
- Tests: 4 test classes with comprehensive coverage (reserve: 6 tests, close: 9 tests, archive: 5 tests, reactivate: 5 tests)
