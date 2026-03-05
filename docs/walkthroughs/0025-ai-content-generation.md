# Walkthrough: 0025-ai-content-generation

## Task Reference
- Task: `docs/tasks/0025-ai-content-generation.md`
- Walkthrough: `docs/walkthroughs/0025-ai-content-generation.md`
- Branch/PR: `feat/p3-ai-action-engine`
- Date: `2026-03-05`

## Summary
Implemented three AI content action handlers for the AI-API action engine. ExtractFromTextActionHandler extracts structured property data from unstructured text with per-field confidence scores. ExtractFromPhotosActionHandler analyzes property photos via OpenAI vision API. GenerateCopyActionHandler generates marketing descriptions in 5 languages with 4 tone options. All handlers follow the MediatR CQRS pattern and are dispatched by the ActionRouter.

## Context
- Background: Tasks 3.1 and 3.2 built the AI action engine plumbing and property action handlers. The ActionRouter returned placeholder responses for content-related action types (GenerateCopy, ExtractFromText).
- Problem statement: Real estate agents need AI-powered tools to quickly extract property data from unstructured text (emails, notes), analyze property photos, and generate multi-language marketing copy -- all via natural language commands.
- Constraints: Handlers in Application layer cannot reference Infrastructure (Clean Architecture). AI calls must be mocked for unit testing. JSON response parsing must be resilient to missing fields.

## Decisions & Trade-offs
- **Decision: Prompts embedded as constants in handlers, not in Infrastructure**
  - Options considered: Prompts in Infrastructure/AI/Prompts/ only, prompts in Application via interface, prompts embedded in handlers
  - Why this choice: Handlers are in the Application layer and cannot reference Infrastructure (dependency inversion). Prompts are part of the use case logic (what to ask the AI), not infrastructure concerns (how to call the API). Infrastructure prompt files are kept as standalone reference copies.
  - Consequences / risks: Slight duplication between handler constants and Infrastructure prompt files. Acceptable trade-off for Clean Architecture compliance.

- **Decision: Shared ParseExtractionResponse between text and photo handlers**
  - Why: Both ExtractFromText and ExtractFromPhotos produce the same ExtractedPropertyDto structure from JSON. The parsing logic is identical, so ExtractFromPhotos reuses ExtractFromTextActionHandler.ParseExtractionResponse (internal static).
  - Consequences / risks: Coupling between two handlers. Acceptable since they share the same domain model.

- **Decision: ExtractedField<T> as generic record**
  - Options considered: Separate DTO per field type, Dictionary<string, (object, double)>, generic record
  - Why this choice: Generic record provides type safety, immutability, and clean API. Each field has exactly one value and one confidence score.

- **Decision: Default tone is "professional" when not specified**
  - Why: Professional tone is the most common use case for real estate marketing copy and is the safest default.

## Implementation Notes
- Key changes:
  - `IOpenAiService` gains `GenerateWithJsonResponseAsync` (system + user prompts with JSON mode) and `AnalyzeImagesAsync` (vision API with image URLs)
  - `OpenAiService` implements both using ChatClient with `ChatResponseFormat.CreateJsonObjectFormat()` and `ChatMessageContentPart.CreateImagePart()`
  - `ParameterExtractor` gains `GetStringList` for extracting string arrays from parameters (handles JsonElement arrays, List<string>, string[], IEnumerable<object>)
  - `ActionType` enum adds `ExtractFromPhotos`
  - `ToolDefinitions` adds `extract_from_photos` tool, updates `generate_copy` tool with proper parameters
  - `ActionRouter` dispatches ExtractFromText, ExtractFromPhotos, GenerateCopy to MediatR (removed from placeholder list)

- Files created:
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ExtractedFieldDto.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ExtractedPropertyDto.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/GeneratedCopyDto.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromTextActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromPhotosActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/GenerateCopyActionCommand.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ExtractFromTextActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ExtractFromPhotosActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/GenerateCopyActionHandler.cs`
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Prompts/PropertyExtractionPrompt.cs`
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Prompts/PhotoExtractionPrompt.cs`
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Prompts/CopyGenerationPrompt.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/ExtractFromTextActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/ExtractFromPhotosActionHandlerTests.cs`
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/GenerateCopyActionHandlerTests.cs`

- Files modified:
  - `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IOpenAiService.cs` (added 2 new methods)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiService.cs` (implemented new methods)
  - `services/ai-api/src/Propely.AiApi.Application/Actions/Helpers/ParameterExtractor.cs` (added GetStringList)
  - `services/ai-api/src/Propely.AiApi.Domain/Actions/ActionType.cs` (added ExtractFromPhotos)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` (dispatch 3 content action types)
  - `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs` (added extract_from_photos, updated generate_copy)
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/ActionRouterTests.cs` (updated routing tests)
  - `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/ToolDefinitionsTests.cs` (updated counts)

## Testing
- Unit tests: 29 new tests across 3 handler test files + 3 new ActionRouter tests + 2 updated ToolDefinitions tests
- All tests use NSubstitute to mock IOpenAiService, verifying correct prompt structure, parameter validation, and JSON parsing
- Tests cover: valid inputs, missing required params, boundary conditions (max chars/images), empty AI responses, AI exceptions, partial responses
