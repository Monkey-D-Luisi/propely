# Task: 0025-ai-content-generation

## Metadata
- ID: 0025
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0025-ai-content-generation.md`
  - Epic: `docs/backlog/epic-P3-ai-engine.md` (Task 3.3)

## Goal
Build three AI content action handlers (ExtractFromText, ExtractFromPhotos, GenerateCopy) that parse NL intent parameters and call IOpenAiService to produce structured output with confidence scores and multi-language marketing copy.

## Context
Task 3.1 established the AI action engine infrastructure (IIntentClassifier, IActionRouter, ActionType, ClassifiedIntent, ExecuteActionCommand). Task 3.2 added property action handlers dispatched via MediatR. This task extends the action engine with AI-powered content generation capabilities: extracting structured property data from text/photos and generating multi-language marketing copy.

## Scope
### In scope
- `GenerateWithJsonResponseAsync` and `AnalyzeImagesAsync` methods on IOpenAiService interface
- Implementation of both methods in OpenAiService (JSON response mode, vision API)
- `ExtractedField<T>` generic wrapper DTO with Value and Confidence
- `ExtractedPropertyDto` with 16 extractable property fields wrapped in ExtractedField
- `GeneratedCopyDto` with language variants dictionary and tone
- `GetStringList` method on ParameterExtractor for array parameter extraction
- `ExtractFromTextActionHandler` - validates max 10,000 chars, calls GenerateWithJsonResponseAsync, parses JSON into ExtractedPropertyDto
- `ExtractFromPhotosActionHandler` - validates max 10 images, calls AnalyzeImagesAsync, parses JSON into ExtractedPropertyDto
- `GenerateCopyActionHandler` - validates tone and languages, calls GenerateWithJsonResponseAsync, parses JSON into GeneratedCopyDto
- `ExtractFromPhotos` added to ActionType enum
- `extract_from_photos` tool definition added to ToolDefinitions
- Updated `GenerateCopy` tool definition with property_data, tone, languages parameters
- ActionRouter dispatches ExtractFromText, ExtractFromPhotos, GenerateCopy to MediatR
- Prompt templates embedded in handlers + reference copies in Infrastructure/AI/Prompts/
- Unit tests for all 3 handlers (9 + 9 + 11 = 29 new tests)
- Updated ActionRouterTests with 3 new routing tests for content actions
- Updated ToolDefinitionsTests for new tool count and mappings

### Out of scope
- Contact/lead/appointment action handlers (Epic P4+)
- Voice input processing (Task 3.4+)
- Integration with actual OpenAI API (handlers tested with mocked IOpenAiService)
- Frontend UI for content generation

## Requirements
- R1: ExtractFromTextActionHandler validates text parameter (required, max 10,000 chars) and returns ExtractedPropertyDto
- R2: ExtractFromPhotosActionHandler validates image_urls parameter (required, 1-10 URLs) and returns ExtractedPropertyDto
- R3: GenerateCopyActionHandler validates tone (professional/luxury/casual/concise) and returns GeneratedCopyDto
- R4: GenerateCopy defaults to professional tone and all 5 languages when not specified
- R5: All handlers gracefully handle empty AI responses and exceptions
- R6: ExtractedField<T> wraps each extracted value with a confidence score (0.0-1.0)
- R7: ActionRouter dispatches all 3 content action types to MediatR (no more placeholders)
- R8: ToolDefinitions includes extract_from_photos tool with proper schema

## Technical Details
- Pattern: MediatR CQRS commands dispatched by ActionRouter, IOpenAiService for AI calls
- Namespaces: Propely.AiApi.Application.Actions.Commands.Content, Propely.AiApi.Application.Actions.Dtos, Propely.AiApi.Application.Actions.Handlers
- DTOs: ExtractedField<T> (generic), ExtractedPropertyDto (16 fields), GeneratedCopyDto (variants + tone)
- Languages: es, en, fr, de, nl
- Tones: professional, luxury, casual, concise
- JSON parsing: System.Text.Json with JsonDocument for AI responses
