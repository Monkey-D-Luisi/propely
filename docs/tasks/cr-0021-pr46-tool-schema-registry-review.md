# Code Review: cr-0021 — PR #46 Tool Schema Registry

## PR Metadata
- **PR:** #46 (`feat/0048-provider-neutral-tool-schema-registry` → `main`)
- **Title:** feat(ai-api): provider-neutral ToolSchemaRegistry (Epic P8, Task 8.1)
- **CI Status:** All checks green except E2E Smoke Tests (pre-existing failure)

## Changed Files
- `docs/backlog/epic-P8-ai-provider-abstraction.md` (new)
- `docs/roadmap.md` (modified)
- `docs/tasks/0048-provider-neutral-tool-schema-registry.md` (new)
- `docs/walkthroughs/0048-provider-neutral-tool-schema-registry.md` (new)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolAdapter.cs` (new)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolSchemaRegistry.cs` (new)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchema.cs` (new)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchemaRegistry.cs` (new)
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Adapters/OpenAiToolAdapter.cs` (new)
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs` (modified)
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs` (deleted)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (modified)
- `services/ai-api/tests/.../OpenAiToolAdapterTests.cs` (new)
- `services/ai-api/tests/.../ToolDefinitionsTests.cs` (modified)
- `services/ai-api/tests/.../ToolSchemaRegistryTests.cs` (new)
- `services/ai-api/tests/.../OpenAiIntentClassifierTests.cs` (modified)
- `services/ai-api/tests/.../PromptRegressionTests.cs` (modified)

---

## Section 1: Agent Review Findings

| # | Severity | Category | Description |
|---|----------|----------|-------------|
| A1 | NIT | Code Quality | `ToolDefinitionsTests` duplicates `ToolSchemaRegistryTests` — same class tested, overlapping assertions |

**Notes:** Architecture is clean — Application layer has zero vendor deps, Infrastructure correctly contains OpenAI types. DI registrations are correct (singleton for registry/adapter, scoped for classifier). All 20 JSON schemas are verbatim copies. No security issues found.

---

## Section 2: Review Comment Threads

| # | Source | Reviewer | Severity | File | Description |
|---|--------|----------|----------|------|-------------|
| R1 | Codex+Copilot | chatgpt-codex-connector, Copilot | SHOULD_FIX | OpenAiIntentClassifier.cs:119 | Cache converted tools — `ConvertAll` called per request instead of once |
| R2 | Copilot | Copilot | MUST_FIX | PromptRegressionTests.cs:7 | Unused `using Propely.AiApi.Infrastructure.AI.Adapters;` import |
| R3 | Copilot | Copilot | SHOULD_FIX | OpenAiToolAdapterTests.cs:9 | Namespace `AI.Adapters` should be `Infrastructure.AI.Adapters` per convention |
| R4 | Copilot | Copilot | SHOULD_FIX | roadmap.md:803-807 | Task 8.3 scope overlaps with 8.1 — classifier refactor already done |
| R5 | Copilot | Copilot | SHOULD_FIX | epic-P8:87-92 | Task 8.1 acceptance criteria unchecked despite DONE status |
| R6 | Copilot | Copilot | SHOULD_FIX | epic-P8:73-76 | Scope description doesn't match implementation (RequiredParameters vs ActionType, ConvertToProviderFormat vs ConvertAll) |
| R7 | Copilot | Copilot | SHOULD_FIX | walkthrough:58 | Claims ToolSchemaRegistryTests "already existed" — it was newly created |
| R8 | Copilot | Copilot | NIT | ToolDefinitionsTests.cs:10-22 | Consolidate with ToolSchemaRegistryTests to avoid duplication |

---

## Resolution Plan

### MUST_FIX
- [x] R2: Remove unused import in PromptRegressionTests.cs

### SHOULD_FIX
- [x] R1: Cache converted tools in OpenAiIntentClassifier constructor
- [x] R3: Move OpenAiToolAdapterTests to Infrastructure.AI.Adapters namespace and directory
- [x] R4: Update Task 8.3 scope in roadmap to clarify remaining work (keyed DI only)
- [x] R5: Check acceptance criteria boxes in epic-P8 doc
- [x] R6: Update scope description to match actual implementation
- [x] R7: Fix walkthrough claim about ToolSchemaRegistryTests

### NIT
- [x] R8/A1: Consolidate ToolDefinitionsTests into ToolSchemaRegistryTests
