# Task: 0024-property-actions-nl

## Metadata
- ID: 0024
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0024-property-actions-nl.md`
  - Epic: `docs/backlog/epic-P3-ai-engine.md` (Task 3.2)

## Goal
Connect the Intent Classifier and Action Router (Task 3.1) to the Properties-API SDK client, enabling property actions (create, query, update, change status) to be executed via natural language commands through the AI-API service.

## Context
Task 3.1 established the AI action engine infrastructure: IIntentClassifier, IActionRouter, ActionType enum, ActionResult, ClassifiedIntent, and ExecuteActionCommand. The ActionRouter returned placeholder results for all action types. This task implements real property action handlers that the ActionRouter dispatches to via MediatR, with QueryProperties calling the Properties-API SDK for live data.

## Scope
### In scope
- PropertiesApi.Client project reference in both Application and Infrastructure layers
- PropertiesApi SDK registration in DI (Infrastructure/DependencyInjection.cs)
- ParameterExtractor static helper for safe typed parameter extraction from Dictionary<string, object?>
- Four MediatR command records: CreatePropertyActionCommand, QueryPropertiesActionCommand, UpdatePropertyActionCommand, ChangePropertyStatusActionCommand
- Four MediatR handlers: CreatePropertyActionHandler, QueryPropertiesActionHandler, UpdatePropertyActionHandler, ChangePropertyStatusActionHandler
- ActionRouter updated to dispatch property action types to MediatR instead of returning placeholders
- Unit tests: ParameterExtractorTests (24 tests), CreatePropertyActionHandlerTests (5 tests), QueryPropertiesActionHandlerTests (5 tests), UpdatePropertyActionHandlerTests (5 tests), ChangePropertyStatusActionHandlerTests (8 tests), ActionRouterTests updated (9 tests)
- Fix for pre-existing Swashbuckle 10.x / OpenApi v2 build error in Api DependencyInjection.cs

### Out of scope
- Write endpoints on the PropertiesApi SDK (currently read-only)
- Contact/lead/appointment action handlers (Task 3.3+)
- Voice input processing (Task 3.4+)

## Requirements
- R1: ActionRouter dispatches CreateProperty, QueryProperties, UpdateProperty, ChangePropertyStatus to MediatR handlers
- R2: Non-property action types continue to return placeholder messages
- R3: QueryPropertiesActionHandler calls IPropertiesApiClient.ListAsync with extracted filters
- R4: Create/Update/ChangeStatus handlers extract and validate parameters, returning structured data
- R5: ParameterExtractor handles native .NET types, JsonElement values, and null gracefully
- R6: All handlers validate required parameters and return appropriate error messages
- R7: ChangePropertyStatusActionHandler validates against a set of known valid statuses
- R8: All number formatting uses InvariantCulture for locale-independent output

## Technical Details
- Pattern: MediatR CQRS commands dispatched by ActionRouter
- Namespace: Propely.AiApi.Application.Actions.Commands.PropertyActions, Propely.AiApi.Application.Actions.Handlers, Propely.AiApi.Application.Actions.Helpers
- SDK: Propely.PropertiesApi.Client (Refit) registered via AddPropertiesApiClient in DI
- Tests: 194 total unit tests, all passing
