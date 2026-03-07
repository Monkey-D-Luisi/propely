# Walkthrough: cr-0021 — PR #46 Tool Schema Registry Review

## Task Reference
- Task: `docs/tasks/cr-0021-pr46-tool-schema-registry-review.md`
- PR: #46 (`feat/0048-provider-neutral-tool-schema-registry` → `main`)
- Date: 2026-03-07

## Summary
Code review of the ToolSchemaRegistry extraction (Task 8.1 / Epic P8). Addressed 9 reviewer comments from Copilot and Codex, plus agent findings. Key fix: cache converted OpenAI tools at construction time instead of rebuilding per request.

## Changes Made

### R1: Cache converted tools in OpenAiIntentClassifier
- Cached `_providerTools` as a field initialized in the constructor
- Eliminates 20 `ChatTool` object allocations per `ClassifyAsync` call
- Restores performance parity with the original static `ToolDefinitions.All`

### R2: Remove unused import
- Removed `using Propely.AiApi.Infrastructure.AI.Adapters;` from PromptRegressionTests.cs

### R3: Fix test namespace convention
- Moved `OpenAiToolAdapterTests.cs` from `tests/AI/Adapters/` to `tests/Infrastructure/AI/Adapters/`
- Updated namespace from `Propely.AiApi.UnitTests.AI.Adapters` to `Propely.AiApi.UnitTests.Infrastructure.AI.Adapters`

### R4: Update Task 8.3 scope in roadmap
- Clarified that classifier DI injection was completed in 8.1
- Task 8.3 now scoped to keyed DI for multi-provider support only

### R5: Check acceptance criteria boxes
- Marked all 6 acceptance criteria as checked in epic-P8 doc

### R6: Fix scope description to match implementation
- Updated `RequiredParameters` → `ActionType` in ToolSchema description
- Updated `ConvertToProviderFormat(ToolSchema)` → `ConvertAll(IReadOnlyList<ToolSchema>)` in IToolAdapter description

### R7: Fix walkthrough claim
- Corrected ToolSchemaRegistryTests from "Already existed" to "New — 10 tests"

### R8/A1: Consolidate duplicate tests
- Deleted `ToolDefinitionsTests.cs` (was duplicating ToolSchemaRegistryTests assertions)
- All unique assertions were already covered by ToolSchemaRegistryTests

## Commands Run
```bash
dotnet build services/ai-api/Propely.AiApi.sln
dotnet test services/ai-api/Propely.AiApi.sln
```

## Checklist
- [x] All findings addressed
- [x] Tests pass
- [x] No secrets committed
