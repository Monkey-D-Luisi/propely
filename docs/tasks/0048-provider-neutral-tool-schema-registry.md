# Task: 0048-provider-neutral-tool-schema-registry

## Metadata
- ID: 0048
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-07
- Related docs:
  - Walkthrough: `docs/walkthroughs/0048-provider-neutral-tool-schema-registry.md`
  - Epic: `docs/backlog/epic-P8-ai-provider-abstraction.md` (Task 8.1)

## Goal
Extract all 20 tool definitions from the OpenAI-coupled `ToolDefinitions.cs` into a provider-neutral `ToolSchemaRegistry` with `IToolAdapter` pattern, enabling multi-LLM support without changing action behavior.

## Context
The current `ToolDefinitions.cs` creates all 20 AI action tool definitions using `OpenAI.Chat.ChatTool.CreateFunctionTool` — a direct dependency on the OpenAI SDK. If the project needs to support Claude, Gemini, or any other LLM provider for intent classification, all 20 definitions must be rewritten. The JSON Schema content (parameter names, types, descriptions) is provider-neutral by nature — only the wrapper type (`ChatTool`) is OpenAI-specific.

## Scope
### In scope
- Create `ToolSchema` record type (Name, Description, ParametersJsonSchema) in Application layer
- Create `IToolSchemaRegistry` interface and `ToolSchemaRegistry` implementation with all 20 schemas
- Create `IToolAdapter` interface in Application layer for provider-specific conversion
- Create `OpenAiToolAdapter` in Infrastructure layer (converts `ToolSchema` → `ChatTool`)
- Maintain `ActionType` resolution via `ToolSchemaRegistry.ResolveActionType(string)`
- Update `OpenAiIntentClassifier` to use injected registry + adapter
- Delete `ToolDefinitions.cs`
- Include epic file creation and roadmap update as part of this task

### Out of scope
- Claude or Gemini adapters (future tasks)
- Changing JSON Schema content (parameter names, types, descriptions stay identical)
- Moving schemas to external JSON files
- Changes to action handlers or ActionRouter

## Requirements
- R1: All 20 tool schemas must be provider-neutral records in Application layer
- R2: No OpenAI SDK types (`ChatTool`, `BinaryData`) in Application layer
- R3: `OpenAiToolAdapter` output must be functionally identical to current `ToolDefinitions.All`
- R4: `ActionType` resolution must work identically for all 20 function names
- R5: Behavioral equivalence — all existing tests must pass without modification

## Acceptance Criteria
- AC1: `ToolSchemaRegistry` exposes all 20 tool schemas with correct names, descriptions, and JSON Schemas
- AC2: `ToolSchemaRegistry.ResolveActionType` returns correct `ActionType` for all 20 function names
- AC3: `OpenAiToolAdapter` converts all 20 schemas into valid `ChatTool` objects
- AC4: `ToolDefinitions.cs` is deleted — no references to `ChatTool.CreateFunctionTool` outside the adapter
- AC5: `OpenAiIntentClassifier` gets tools via DI-injected registry + adapter, not static class
- AC6: All existing unit and integration tests pass without modification

## Constraints (non-negotiable)
- Clean Architecture layers respected (no OpenAI types in Application).
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests before implementation.

## Proposed Approach (high-level)
1. Define provider-neutral types in Application layer (`ToolSchema`, `IToolSchemaRegistry`, `IToolAdapter`)
2. Implement `ToolSchemaRegistry` with all 20 schemas (copy JSON Schema strings verbatim from `ToolDefinitions.cs`)
3. Implement `OpenAiToolAdapter` in Infrastructure layer
4. Refactor `OpenAiIntentClassifier` to use DI-injected services
5. Delete `ToolDefinitions.cs`
6. Verify with comprehensive TDD

## Implementation Steps
1. Create `ToolSchema` record in `Propely.AiApi.Application.Actions.Tools`
2. Create `IToolSchemaRegistry` interface in same namespace
3. Write unit tests for `ToolSchemaRegistry` (all 20 schemas present, names correct, ActionType resolution)
4. Implement `ToolSchemaRegistry` — make tests pass
5. Create `IToolAdapter` interface in Application layer
6. Write unit tests for `OpenAiToolAdapter` (convert all schemas, verify function names and parameter schemas)
7. Implement `OpenAiToolAdapter` in Infrastructure layer — make tests pass
8. Refactor `OpenAiIntentClassifier` to inject `IToolSchemaRegistry` and `OpenAiToolAdapter`
9. Update `DependencyInjection.cs` to register new services
10. Delete `ToolDefinitions.cs`
11. Run full build + test suite
12. Update walkthrough

## Files to Create
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchema.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolSchemaRegistry.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchemaRegistry.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolAdapter.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Adapters/OpenAiToolAdapter.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Actions/Tools/ToolSchemaRegistryTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/AI/Adapters/OpenAiToolAdapterTests.cs`

## Files to Modify
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs`

## Files to Delete
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs`

## Testing Plan
- Unit tests: `ToolSchemaRegistryTests` — all 20 schemas present, ActionType resolution, no duplicates
- Unit tests: `OpenAiToolAdapterTests` — conversion output matches original ToolDefinitions
- Unit tests: `OpenAiIntentClassifier` — verify adapter injection works
- Integration tests: existing pipeline tests pass unchanged (behavioral equivalence)
- Manual verification: none required

## Security & Privacy
- No new attack surface — same tool definitions, same behavior
- JSON Schema strings validated at construction time
- No new auth or tenant isolation changes

## Observability
- Logs: existing logging in `OpenAiIntentClassifier` unchanged
- Metrics: none added
- Traces: none added

## Rollback Plan
Revert the commit — all changes are contained within ai-api service. No database migrations, no external dependencies changed.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (585/585)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
