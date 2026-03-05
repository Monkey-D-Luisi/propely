# Walkthrough: 0024-property-actions-nl

## Task Reference
- Task: `docs/tasks/0024-property-actions-nl.md`
- Walkthrough: `docs/walkthroughs/0024-property-actions-nl.md`
- Branch/PR: `feat/p3-ai-action-engine`
- Date: `2026-03-05`

## Summary
Implemented property action handlers for the AI-API natural language action engine. The ActionRouter now dispatches property-related intents (CreateProperty, QueryProperties, UpdateProperty, ChangePropertyStatus) to dedicated MediatR command handlers. QueryPropertiesActionHandler calls the Properties-API SDK for live property queries; the other three handlers extract and validate parameters, returning structured data (ready for when write endpoints are added to the SDK).

## Context
- Background: Task 3.1 built the AI action engine plumbing (IIntentClassifier, IActionRouter, ActionType, ClassifiedIntent, ExecuteActionCommand) with placeholder responses. Properties-API SDK client existed with read-only access (ListAsync, GetByIdAsync, CountByStatusAsync).
- Problem statement: Natural language commands like "search apartments in Malaga" or "create a 3-bedroom villa" needed to actually execute against real services rather than returning placeholder messages.
- Constraints: Properties SDK is read-only, so Create/Update/ChangeStatus handlers can only extract and validate parameters. Must handle JsonElement values from OpenAI function calling. Must be locale-independent.

## Decisions & Trade-offs
- **Decision: Handlers in Application layer, not Infrastructure**
  - Options considered: Handlers in Infrastructure (closer to SDK), handlers in Application (CQRS pattern)
  - Why this choice: MediatR handlers belong in the Application layer per Clean Architecture. The Application layer references the PropertiesApi.Client (interface dependency, not implementation dependency) for QueryPropertiesActionHandler.
  - Consequences / risks: Application layer now has a project reference to PropertiesApi.Client, but the client is an interface-only contract (Refit generates the implementation at runtime).

- **Decision: ParameterExtractor as static helper**
  - Options considered: Instance-based service, extension methods on Dictionary, static helper
  - Why this choice: Parameter extraction is stateless and purely functional. A static helper is the simplest pattern with no DI overhead. Handles both native .NET types and JsonElement from JSON deserialization.
  - Consequences / risks: None; easily testable and reusable.

- **Decision: InvariantCulture for all number formatting**
  - Why: The machine runs with Spanish locale where 1.500 is used instead of 1,500. Since AI responses should be consistent regardless of server locale, all N0 formatting uses CultureInfo.InvariantCulture.

- **Decision: Simplified Swagger configuration to unblock build**
  - Why: Pre-existing Swashbuckle 10.x / Microsoft.OpenApi 2.x incompatibility prevented the Api project from building. The OpenApiSecurityScheme types were removed/restructured in OpenApi v2. Simplified to `services.AddSwaggerGen()` to unblock; JWT auth in Swagger can be re-added when the API stabilizes.

## Implementation Notes
- Key changes:
  - `Propely.AiApi.Application.csproj` gains ProjectReference to PropertiesApi.Client
  - `Propely.AiApi.Infrastructure.csproj` gains ProjectReference to PropertiesApi.Client
  - `Infrastructure/DependencyInjection.cs` registers PropertiesApi SDK via `AddPropertiesApiClient`
  - `Infrastructure/AI/ActionRouter.cs` now requires IMediator, dispatches 4 property action types via switch expression
  - `Application/Actions/Helpers/ParameterExtractor.cs` provides GetString, GetInt, GetDecimal, GetGuid, GetEnum methods
  - `Application/Actions/Commands/PropertyActions/` contains 4 command records
  - `Application/Actions/Handlers/` contains 4 handler classes
  - `Api/DependencyInjection.cs` simplified Swagger setup to fix pre-existing OpenApi v2 build error

- Files created:
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Helpers/ParameterExtractor.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/PropertyActions/CreatePropertyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/PropertyActions/QueryPropertiesActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/PropertyActions/UpdatePropertyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/PropertyActions/ChangePropertyStatusActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/CreatePropertyActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/QueryPropertiesActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/UpdatePropertyActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ChangePropertyStatusActionHandler.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Helpers/ParameterExtractorTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/CreatePropertyActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/QueryPropertiesActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/UpdatePropertyActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ChangePropertyStatusActionHandlerTests.cs`

- Files modified:
  - `services/ai-api/src/Propely.AiApi.Application/Propely.AiApi.Application.csproj` (added PropertiesApi.Client reference)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/Propely.AiApi.Infrastructure.csproj` (added PropertiesApi.Client reference)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (SDK DI registration)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` (MediatR dispatch)
  - `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs` (Swagger fix)
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/ActionRouterTests.cs` (updated for MediatR)

## Test Results
- 194 total unit tests, all passing
- New tests: 47 (24 ParameterExtractor + 5 Create + 5 Query + 5 Update + 8 ChangeStatus)
- Updated tests: 9 (ActionRouterTests rewritten for MediatR dispatch)
