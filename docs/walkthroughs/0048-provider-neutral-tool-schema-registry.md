# Walkthrough: 0048-provider-neutral-tool-schema-registry

## Task Reference
- Task: `docs/tasks/0048-provider-neutral-tool-schema-registry.md`
- Walkthrough: `docs/walkthroughs/0048-provider-neutral-tool-schema-registry.md`
- Branch/PR: `feat/0048-provider-neutral-tool-schema-registry` / TBD
- Date: `2026-03-07`

## Summary
Extracted the 20 provider-coupled `ChatTool` definitions from `ToolDefinitions.cs` into a provider-neutral `ToolSchemaRegistry` in the Application layer, and created an `OpenAiToolAdapter` in the Infrastructure layer that converts schemas to OpenAI format at runtime. This enables adding future LLM providers (Claude, Gemini) by implementing a single adapter without touching tool definitions.

## Context
- Background: `ToolDefinitions.cs` created 20 AI action tools using `OpenAI.Chat.ChatTool.CreateFunctionTool`, coupling the Application→Infrastructure boundary to a specific vendor SDK.
- Problem statement: Switching LLM providers requires rewriting all 20 tool definitions. The JSON Schema content is already provider-neutral — only the wrapper type is OpenAI-specific.
- Constraints: Must be behavioral equivalent — all existing tests pass without changes.

## Decisions & Trade-offs
1. **In-memory registry vs JSON files**: Kept schemas as C# raw string literals in `ToolSchemaRegistry.cs` rather than external `.json` files. Rationale: simpler, no file I/O, compile-time errors for typos, and only 20 schemas. Can be extracted to files later if volume grows.
2. **Concrete `OpenAiToolAdapter` injection**: Registered `OpenAiToolAdapter` as concrete type (not via `IToolAdapter` interface) in DI because `OpenAiIntentClassifier` is the only consumer today and it already lives in Infrastructure. When a second adapter appears, switch to `IToolAdapter` keyed registration.
3. **Record type for `ToolSchema`**: Used `sealed record` for immutability and value equality. The `ActionType` property on each schema avoids the need for a separate mapping dictionary.

## Implementation Notes
- `ToolSchema` record lives in Application layer (zero vendor dependencies)
- `IToolSchemaRegistry` and `IToolAdapter` interfaces in Application layer
- `ToolSchemaRegistry` implementation in Application layer (pure data, no vendor deps)
- `OpenAiToolAdapter` in Infrastructure layer (depends on `OpenAI.Chat.ChatTool`)
- `OpenAiIntentClassifier` now accepts `IToolSchemaRegistry` + `OpenAiToolAdapter` via constructor DI instead of using static `ToolDefinitions` class
- `ToolDefinitions.cs` deleted — all 20 JSON Schema strings moved verbatim to `ToolSchemaRegistry`

## Data / Schema / Migrations
- DB changes: none
- Migration strategy: N/A
- Backward compatibility: 100% — same behavior, different internal structure

## Commands Run
```bash
dotnet build services/ai-api/Propely.AiApi.sln      # 0 errors, 25 warnings (pre-existing)
dotnet test services/ai-api/tests/Propely.AiApi.UnitTests/  # 585 passed, 0 failed
```

## Files Changed

### Created
| File | Layer | Purpose |
|------|-------|---------|
| `Application/Actions/Tools/ToolSchema.cs` | Application | Provider-neutral tool schema record |
| `Application/Actions/Tools/IToolSchemaRegistry.cs` | Application | Registry interface |
| `Application/Actions/Tools/IToolAdapter.cs` | Application | Adapter interface (schema → vendor tool) |
| `Application/Actions/Tools/ToolSchemaRegistry.cs` | Application | Registry with all 20 schemas |
| `Infrastructure/AI/Adapters/OpenAiToolAdapter.cs` | Infrastructure | Converts ToolSchema → ChatTool |

### Modified
| File | Change |
|------|--------|
| `Infrastructure/AI/OpenAiIntentClassifier.cs` | Constructor accepts registry + adapter via DI; removed static ToolDefinitions reference |
| `Infrastructure/DependencyInjection.cs` | Added registrations for IToolSchemaRegistry, OpenAiToolAdapter |
| `tests/.../ToolDefinitionsTests.cs` | Updated to use ToolSchemaRegistry instead of deleted ToolDefinitions |
| `tests/.../ToolSchemaRegistryTests.cs` | New — 10 tests for registry (count, names, descriptions, JSON validity, ActionType resolution) |
| `tests/.../OpenAiIntentClassifierTests.cs` | Updated constructor calls to include registry and adapter |
| `tests/.../PromptRegressionTests.cs` | Updated 6 references from ToolDefinitions to _registry |

### Deleted
| File | Reason |
|------|--------|
| `Infrastructure/AI/ToolDefinitions.cs` | Replaced by ToolSchemaRegistry + OpenAiToolAdapter |

## Tests
### Unit
- `ToolSchemaRegistryTests` (10 tests): count, unique names, descriptions, valid JSON, ActionType resolution (20 cases), unknown names, GetByName, no Unknown type
- `OpenAiToolAdapterTests` (7 tests): count, ChatTool instances, function name preservation, description preservation, specific tools, empty list
- `ToolDefinitionsTests` (backward-compat, 25 tests): Same assertions as before, now using ToolSchemaRegistry
- `PromptRegressionTests` (updated 6 references): Spanish vocabulary, tool def count, unique names, descriptions
- `OpenAiIntentClassifierTests` (updated constructor calls): Same behavior verified

### Integration
- No integration test changes required — no behavioral change

### Manual
- No manual verification required

## Observability
- Logs added/updated: none (existing logging unchanged)
- Traces/metrics added/updated: none

## Security
- Validation: JSON Schema strings validated at construction (parsed in tests)
- AuthN/AuthZ impact: none
- Sensitive data handling: none

## Follow-ups / Backlog
- [ ] Claude tool adapter (future task 8.3+)
- [ ] Gemini tool adapter (future)
- [ ] Move schemas to external JSON files if count grows significantly
- [ ] Task 8.2: Typed action parameter records
- [ ] Task 8.4: MCP Server endpoint

## Checklist
- [x] Task scope matches `docs/tasks/0048-provider-neutral-tool-schema-registry.md`
- [x] Tests updated and passing (585/585)
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
