# Walkthrough: 0023-intent-classifier-action-router

## Task Reference
- Task: `docs/tasks/0023-intent-classifier-action-router.md`
- Walkthrough: `docs/walkthroughs/0023-intent-classifier-action-router.md`
- Branch/PR: `feat/p2-properties-core`
- Date: `2026-03-05`

## Summary
Implemented the AI Action Engine core infrastructure for the ai-api service: intent classification using OpenAI function calling, action routing with placeholder handlers, and a new REST endpoint at `POST /v1/actions/execute`. This provides the foundational pipeline that all subsequent AI action handler tasks (3.2+) will plug into.

## Context
- Background: The ai-api service had basic OpenAI text generation but no structured intent classification or action routing. The platform's core differentiator is NL-driven actions.
- Problem statement: Needed a pipeline to convert free-form user text into classified intents with extracted parameters, then route them to typed action handlers.
- Constraints: Must use OpenAI SDK v2.9.0 function calling API. Must follow Clean Architecture. Must be extensible for future action types. TDD mandatory.

## Decisions & Trade-offs
- **Decision: OpenAI function calling for intent classification (vs. custom prompt + JSON parsing)**
  - Options considered: (1) Prompt engineering with JSON response parsing (like ParseWorkItem), (2) OpenAI function calling with tool definitions
  - Why this choice: Function calling provides structured, reliable intent classification. The model is constrained to only call defined functions, eliminating parsing errors and hallucinated actions. Parameters are extracted directly from the function call arguments.
  - Consequences / risks: Tied to OpenAI SDK for classification. Mitigated by IIntentClassifier interface allowing alternative implementations.

- **Decision: Confidence threshold of 0.5 for action execution**
  - Options considered: 0.3, 0.5, 0.7, 0.8
  - Why this choice: 0.5 provides a reasonable balance. When the model selects a tool call, confidence is 1.0 (the model is decisive about function calling). When no tool is called, confidence is 0.0 (Unknown). The threshold primarily guards against future classification strategies that may produce intermediate confidence values.
  - Consequences / risks: May need tuning as real usage data comes in.

- **Decision: Placeholder action router (vs. MediatR dispatch)**
  - Options considered: (1) Full MediatR dispatch to typed handlers, (2) Placeholder responses for all known types
  - Why this choice: Individual action handlers don't exist yet (Tasks 3.2+). Building the routing plumbing now with placeholder responses allows the pipeline to be tested end-to-end. Each subsequent task just needs to register its handler.
  - Consequences / risks: None. Clean plug-in pattern.

- **Decision: Scoped DI lifetime for IIntentClassifier and IActionRouter**
  - Options considered: Singleton, Scoped, Transient
  - Why this choice: Scoped matches the per-request nature of intent classification. The OpenAiIntentClassifier creates its own ChatClient in the constructor (from IOptions), so Scoped is appropriate for per-request isolation while reusing the options binding.
  - Consequences / risks: None significant.

## Implementation Notes
- Key changes:
  - **Domain layer**: `ActionType` enum (19 action types), `ActionResult`/`ActionResult<T>` records with static factory methods, `ClassifiedIntent` record
  - **Application layer**: `IIntentClassifier` and `IActionRouter` interfaces in `Actions/Interfaces/`, `ExecuteActionCommand`/`ExecuteActionCommandHandler`/`ExecuteActionCommandValidator` in `Actions/Commands/ExecuteAction/`
  - **Infrastructure layer**: `OpenAiIntentClassifier` uses `ChatTool.CreateFunctionTool()` with JSON Schema parameter definitions, `ActionRouter` returns placeholder results, `ToolDefinitions` defines 9 tool schemas with snake_case parameters per OpenAI convention
  - **API layer**: `ActionsController` with `POST /v1/actions/execute`, `ActionDtos` (request/response), `ExecuteActionRequestValidator`
  - **DI**: Added `IIntentClassifier -> OpenAiIntentClassifier` and `IActionRouter -> ActionRouter` as Scoped in Infrastructure DependencyInjection
  - **Rate limiting**: Added 20 req/min limit for `POST:/v1/actions/execute`
- Edge cases handled: Missing API key (returns Unknown gracefully), classifier exceptions, router exceptions, low confidence, unknown intent, cancellation token propagation
- Known limitations: ActionRouter returns placeholder messages for all known action types. Real handler dispatch will be added in Tasks 3.2+.

## Data / Schema / Migrations
- DB changes: None (no persistence added in this task)
- Config changes: Uses existing `OpenAi` config section (ApiKey + ModelId) via `OpenAiOptions`

## Test Coverage
- 51 new unit tests across 6 test files:
  - `ExecuteActionCommandHandlerTests` (8 tests): Unknown intent, low confidence, valid routing, classifier exception, router exception, result propagation
  - `OpenAiIntentClassifierTests` (3 tests): Null/empty/whitespace API key returns Unknown
  - `ActionRouterTests` (5 tests): Unknown type failure, known types return success, parameter inclusion, specific action messages
  - `ToolDefinitionsTests` (5 tests): Tool count, function name mapping, unique names
  - `ExecuteActionCommandValidatorTests` (6 tests): Empty text, max length, empty GUIDs, valid input
  - `ActionResultTests` (6 tests): Ok/Fail factory methods for both generic and non-generic, ClassifiedIntent properties
- All 139 total unit tests pass (no regression)
- All 5 architecture tests pass

## Files Changed

### New Files (17)
| File | Purpose |
|------|---------|
| `src/Propely.AiApi.Domain/Actions/ActionType.cs` | Action type enumeration |
| `src/Propely.AiApi.Domain/Actions/ActionResult.cs` | Generic and non-generic action result records |
| `src/Propely.AiApi.Domain/Actions/ClassifiedIntent.cs` | Classified intent record |
| `src/Propely.AiApi.Application/Actions/Interfaces/IIntentClassifier.cs` | Intent classifier interface |
| `src/Propely.AiApi.Application/Actions/Interfaces/IActionRouter.cs` | Action router interface |
| `src/Propely.AiApi.Application/Actions/Commands/ExecuteAction/ExecuteActionCommand.cs` | MediatR command |
| `src/Propely.AiApi.Application/Actions/Commands/ExecuteAction/ExecuteActionCommandHandler.cs` | Command handler |
| `src/Propely.AiApi.Application/Actions/Commands/ExecuteAction/ExecuteActionCommandValidator.cs` | FluentValidation validator |
| `src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs` | Static OpenAI tool definitions |
| `src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs` | Intent classifier implementation |
| `src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` | Action router implementation |
| `src/Propely.AiApi.Api/Controllers/ActionsController.cs` | REST API controller |
| `src/Propely.AiApi.Api/Dtos/ActionDtos.cs` | Request/response DTOs |
| `src/Propely.AiApi.Api/Validators/ExecuteActionRequestValidator.cs` | API request validator |
| `tests/.../Actions/ExecuteActionCommandHandlerTests.cs` | Handler unit tests |
| `tests/.../Actions/OpenAiIntentClassifierTests.cs` | Classifier unit tests |
| `tests/.../Actions/ActionRouterTests.cs` | Router unit tests |
| `tests/.../Actions/ToolDefinitionsTests.cs` | Tool definitions unit tests |
| `tests/.../Actions/ExecuteActionCommandValidatorTests.cs` | Validator unit tests |
| `tests/.../Domain/Actions/ActionResultTests.cs` | Domain record unit tests |

### Modified Files (2)
| File | Change |
|------|--------|
| `src/Propely.AiApi.Infrastructure/DependencyInjection.cs` | Added IIntentClassifier and IActionRouter registrations |
| `src/Propely.AiApi.Api/DependencyInjection.cs` | Added rate limit rule for actions/execute endpoint |
