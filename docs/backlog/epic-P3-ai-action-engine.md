# Epic P3 -- AI Action Engine

## Overview

Build the core AI orchestration layer that enables users to perform **any system action** through natural language — text or voice. Instead of navigating menus and filling forms, agents speak or type commands like "Create a 3-bedroom apartment in Malaga for 250k" or "Book a viewing for Maria Garcia next Tuesday at 10am," and the system classifies the intent, extracts entities, and executes the action across the appropriate microservice.

**This is the platform's primary differentiator.** The AI is the interface.

**Existing base:** The `ParseWorkItemCommand` pattern in ai-api demonstrates NL text → structured output via `IOpenAiService`. Phase 3 extends this into a full action engine with OpenAI function calling, cross-service SDK execution, voice input, and a frontend command bar.

## Service Ownership

| Capability | Service |
|---|---|
| Intent classification & action routing | `services/ai-api` (Application layer) |
| Action handlers (cross-service execution) | `services/ai-api` (Application + Infrastructure layers) |
| Voice transcription (`gpt-4o-mini-transcription`) | `services/ai-api` (Infrastructure layer) |
| Content generation (copy, extraction) | `services/ai-api` (Application layer) |
| Prompt engineering & tool definitions | `services/ai-api` (Infrastructure layer) |
| NuGet SDK client | `services/ai-api` (Client package) |
| Frontend command bar & voice UI | `apps/web` |

## AI Action Engine Architecture

```
User Input (text or voice audio)
        │
        ▼
┌─────────────────────────────────┐
│  Voice?                          │
│  YES → gpt-4o-mini-transcription │── POST /v1/voice/transcribe
│  NO  → pass through              │
└──────────┬──────────────────────┘
           │ (text)
           ▼
┌─────────────────────────────────┐
│  Intent Classification           │
│  OpenAI function calling with    │
│  tool definitions for each       │── POST /v1/actions/execute
│  action type. Model selects      │
│  tool + extracts parameters.     │
└──────────┬──────────────────────┘
           │ (ActionType + parameters)
           ▼
┌─────────────────────────────────┐
│  Action Router (MediatR)         │
│  Maps ActionType → IRequest<T>   │── Dispatches typed command via DI
│  via ActionCommandFactory        │
└──────────┬──────────────────────┘
           │
           ▼
┌────────────────────────────────────────────────────────┐
│  Action Handlers                                        │
│  (each is a standard MediatR IRequestHandler)           │
│                                                         │
│  CreatePropertyHandler    → IPropertiesApi (SDK)        │
│  UpdatePropertyHandler    → IPropertiesApi (SDK)        │
│  QueryPropertiesHandler   → IPropertiesApi (SDK)        │
│  ChangeStatusHandler      → IPropertiesApi (SDK)        │
│  CreateLeadHandler        → ILeadsApi (SDK)             │
│  CreateContactHandler     → IContactsApi (SDK)          │
│  QualifyLeadHandler       → ILeadsApi (SDK)             │
│  BookViewingHandler       → IAppointmentsApi (SDK)      │
│  GenerateCopyHandler      → IOpenAiService (local)      │
│  ExtractFromTextHandler   → IOpenAiService (local)      │
│  ExtractFromPhotosHandler → IOpenAiService (local)      │
│  ...extensible                                          │
└──────────┬─────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│  ActionResult<T>                 │
│  - Success: bool                 │
│  - Data: T (created entity, etc) │
│  - Message: string (NL confirm)  │── "Created apartment AP-2024-001 in Malaga"
│  - Errors: string[] (if failed)  │
└─────────────────────────────────┘
```

### Design Principles

1. **One handler per action** — Clean separation, each handler is a focused MediatR command handler
2. **Function calling for classification** — OpenAI selects the tool (action) and extracts parameters natively; no custom NER needed
3. **SDK clients for execution** — ai-api calls other services via their NuGet SDK clients (Refit), maintaining tenant isolation and auth
4. **Extensible by registration** — Adding a new action = new tool definition + new command/handler + DI registration
5. **Graceful degradation** — If OpenAI fails, return an error; if confidence is low, ask for confirmation
6. **Stateless actions** — Each action is self-contained. Conversation context (P7.2) is a later enhancement

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 3.1 | Intent Classifier & Action Router | DONE | 0.4 |
| 3.2 | Property Actions via Natural Language | DONE | 3.1, 2.6 |
| 3.3 | AI Content Generation | DONE | 3.1 |
| 3.4 | Contact & Lead Actions via Natural Language | IN_PROGRESS | 3.1, 4.5 |
| 3.5 | Operation Actions via Natural Language | DONE | 3.2 |
| 3.6 | Appointment Actions via Natural Language | BLOCKED | 3.1, 5.8 |
| 3.7 | Voice Input (Speech-to-Text) | DONE | 3.1 |
| 3.8 | AI-API NuGet SDK Client | DONE | 3.1, 3.7 |
| 3.9 | Prompt Engineering & Spanish RE Vocabulary | BLOCKED | 3.2, 3.3, 3.4 |
| 3.10 | Frontend: Command Bar | DONE | 3.8 |
| 3.11 | Frontend: Voice Mode | DONE | 3.10, 3.7 |

---

## Task 3.1 -- Intent Classifier & Action Router

**Status:** DONE
**Dependencies:** 0.4 (NuGet SDK infrastructure)

### Goal

Build the core AI orchestration infrastructure: an endpoint that receives natural language text, classifies the intent via OpenAI function calling, and routes to the correct action handler.

### Scope

**In scope:**
- `POST /v1/actions/execute` endpoint in ai-api — accepts `{ "text": "..." }`, returns `ActionResultDto`
- `IActionClassifier` interface with OpenAI function-calling implementation
- Tool definitions (OpenAI function schemas) for each supported action type
- `ActionRouter` service that maps classified actions to MediatR commands
- `ActionCommandFactory` that creates typed commands from action parameters
- `ActionResultDto` response model: success, message (NL confirmation), data (optional entity), errors
- `ActionType` enum: `CreateProperty`, `UpdateProperty`, `QueryProperties`, `ChangePropertyStatus`, `CreateLead`, `CreateContact`, `QualifyLead`, `ConvertLead`, `BookViewing`, `CancelAppointment`, `RescheduleAppointment`, `QueryAppointments`, `GenerateCopy`, `ExtractFromText`, `ExtractFromPhotos`, `Unknown`
- Fallback: if OpenAI cannot classify, return `ActionType.Unknown` with a helpful message
- Telemetry: action type distribution, classification latency, success/failure rates

**Out of scope:**
- Specific action handler implementations (tasks 3.2–3.6)
- Voice input (task 3.7)
- Frontend (task 3.10)
- Conversation context/memory (P7.2)

### Acceptance Criteria

- [ ] **AC1:** `POST /v1/actions/execute` accepts `{ "text": "..." }` and returns `ActionResultDto`
- [ ] **AC2:** `IActionClassifier.ClassifyAsync(text)` calls OpenAI with function-calling tool definitions and returns `ClassifiedAction { ActionType, Parameters: Dictionary<string, object>, Confidence: double }`
- [ ] **AC3:** `ActionRouter.RouteAsync(ClassifiedAction)` dispatches to the correct MediatR command handler and returns `ActionResult<T>`
- [ ] **AC4:** `ActionCommandFactory.Create(ClassifiedAction)` instantiates the correct `IRequest<T>` from action type and extracted parameters
- [ ] **AC5:** All 16 `ActionType` values have corresponding tool definitions in the OpenAI function schema
- [ ] **AC6:** If OpenAI returns no function call (unrecognized intent), result is `ActionType.Unknown` with message "I didn't understand that. Try: create a property, add a lead, book a viewing..."
- [ ] **AC7:** If OpenAI confidence is below 0.6, result includes `NeedsConfirmation: true` and a human-readable summary of what the system understood
- [ ] **AC8:** Empty or whitespace-only text returns 400 Bad Request
- [ ] **AC9:** Text exceeding 5,000 characters returns 400 Bad Request
- [ ] **AC10:** Endpoint requires authentication and tenant context (JWT + X-Org-Id)
- [ ] **AC11:** OpenAI API errors are caught and returned as 502 with safe error message
- [ ] **AC12:** Request metadata is logged (action type, latency) but NOT the input text (privacy)
- [ ] **AC13:** Telemetry counters: `actions.classified`, `actions.executed`, `actions.failed`, `actions.unknown`
- [ ] **AC14:** Unit tests achieve >= 90% coverage for classifier, router, and factory
- [ ] **AC15:** Integration test confirms end-to-end pipeline with mocked OpenAI response

### Implementation Steps

1. Create `ActionType` enum in `Domain/Actions/`
2. Create `ClassifiedAction` record in `Application/Actions/Models/` — `ActionType`, `Parameters`, `Confidence`, `RawFunctionName`
3. Create `ActionResult<T>` record in `Application/Actions/Models/` — `Success`, `Message`, `Data`, `Errors`, `NeedsConfirmation`
4. Create `ActionResultDto` response DTO in `Application/Actions/Dtos/`
5. Create `IActionClassifier` interface in `Application/Actions/Interfaces/` — `ClassifyAsync(string text, CancellationToken ct)`
6. Create `ActionToolDefinitions` static class in `Infrastructure/Actions/` — builds OpenAI function/tool schemas for all action types
7. Create `OpenAiActionClassifier` in `Infrastructure/Actions/` — implements `IActionClassifier` using `ChatClient` with tools option
8. Create `IActionRouter` interface in `Application/Actions/Interfaces/` — `RouteAsync(ClassifiedAction, CancellationToken ct)`
9. Create `ActionRouter` in `Application/Actions/Services/` — maps `ActionType` to MediatR commands via `ActionCommandFactory`
10. Create `ActionCommandFactory` in `Application/Actions/Services/` — creates typed `IRequest<T>` from `ClassifiedAction`
11. Create `ActionsController` with `POST /v1/actions/execute` in `Api/Controllers/`
12. Create `ExecuteActionRequest` validator in `Api/Validators/`
13. Register all services in DI (`Infrastructure/DependencyInjection.cs`, `Api/DependencyInjection.cs`)
14. Add telemetry counters
15. Write unit tests for classifier (mocked OpenAI), router, factory
16. Write integration test with `WebApplicationFactory`

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Domain/Actions/ActionType.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Models/ClassifiedAction.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Models/ActionResult.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ActionResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ExecuteActionResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Interfaces/IActionClassifier.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Interfaces/IActionRouter.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionRouter.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/OpenAiActionClassifier.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/ActionsController.cs`
- `services/ai-api/src/Propely.AiApi.Api/Dtos/ExecuteActionRequest.cs`
- `services/ai-api/src/Propely.AiApi.Api/Validators/ExecuteActionRequestValidator.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/ActionRouterTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/ActionCommandFactoryTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/OpenAiActionClassifierTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Api/ActionsControllerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register `IActionClassifier`)
- `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs` (register controller routes, rate limiting for actions endpoint)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `OpenAiActionClassifier` maps function call response to `ClassifiedAction` | xUnit, mock `ChatClient` response |
| Unit | `OpenAiActionClassifier` handles no function call → `ActionType.Unknown` | xUnit, mock empty response |
| Unit | `OpenAiActionClassifier` handles OpenAI error → `AiServiceException` | xUnit, mock exception |
| Unit | `ActionCommandFactory.Create()` returns correct `IRequest<T>` for each `ActionType` | xUnit, parameterized for all 16 types |
| Unit | `ActionCommandFactory.Create()` maps parameters correctly from `Dictionary<string, object>` | xUnit |
| Unit | `ActionRouter.RouteAsync()` dispatches to MediatR and returns result | xUnit, mock `IMediator` |
| Unit | `ActionRouter.RouteAsync()` handles `ActionType.Unknown` without dispatching | xUnit |
| Unit | `ExecuteActionRequestValidator` rejects empty/oversized text | xUnit + FluentAssertions |
| Integration | `POST /v1/actions/execute` with valid text returns 200 + `ActionResultDto` | `WebApplicationFactory`, mock OpenAI |
| Integration | `POST /v1/actions/execute` without auth returns 401 | `WebApplicationFactory` |
| Integration | `POST /v1/actions/execute` with empty text returns 400 | `WebApplicationFactory` |

### Security & Privacy

- Action input text may contain PII (names, phone numbers, addresses) — **never log input text**
- Log only: action type, latency, tenant ID, user ID, success/failure
- Tool definitions do not contain sensitive data (they're schema definitions)
- Rate limit: 30 req/min per user for action execution (more generous than work-item parse)

### TDD Reminder

Write `ActionCommandFactory` tests first (all 16 action types → correct command type). Write `ActionRouter` tests with mocked mediator. Write `OpenAiActionClassifier` tests with mocked ChatClient responses. Then implement each component to pass.

---

## Task 3.2 -- Property Actions via Natural Language

**Status:** DONE
**Dependencies:** 3.1 (action engine core), 2.6 (PropertiesApi SDK client)

### Goal

Implement action handlers that create, update, query, and manage properties through natural language commands routed by the action engine.

### Scope

**In scope:**
- `CreatePropertyActionHandler` — extracts property fields from NL, calls `IPropertiesApi.CreateAsync()`
- `UpdatePropertyActionHandler` — updates specific fields ("change the price to 300k", "add 2 parking spaces")
- `QueryPropertiesActionHandler` — searches/filters ("show me apartments in Malaga under 200k", "list my active properties")
- `ChangePropertyStatusActionHandler` — "activate this property", "reserve AP-2024-001", "mark as sold"
- Entity resolution: match property references by code (AP-2024-001), address fragment, or description
- Confidence-scored field extraction: if a field has low confidence, include it in `NeedsConfirmation` response
- Spanish real estate terminology mapping (piso → Apartment, chalet → Villa, atico → Penthouse, etc.)

**Out of scope:**
- Properties-API domain logic (Phase 2)
- Media operations via NL (future enhancement)
- Frontend (task 3.10)

### Acceptance Criteria

- [ ] **AC1:** "Create a 3-bedroom apartment in Malaga for 250k" → creates property via SDK with `{ type: Apartment, bedrooms: 3, city: "Malaga", price: 250000, operation: Sale }`
- [ ] **AC2:** "Pon en venta un piso de 2 habitaciones en la calle Mayor 12 de Madrid por 180.000 euros" → creates property with Spanish input
- [ ] **AC3:** "Change the price of AP-2024-001 to 280,000" → resolves property by reference code, updates price
- [ ] **AC4:** "Show me all active apartments under 300k" → queries properties with filters: status=Active, type=Apartment, maxPrice=300000
- [ ] **AC5:** "Activate the Malaga apartment" → resolves property by city+type, changes status to Active
- [ ] **AC6:** "Reserve AP-2024-001" → changes property status to Reserved
- [ ] **AC7:** Entity resolution: when multiple properties match a vague reference, return `NeedsConfirmation` with a list of matches
- [ ] **AC8:** Spanish property type mapping: piso→Apartment, chalet→Villa, atico→Penthouse, adosado→Townhouse (parameterized test)
- [ ] **AC9:** Missing required fields for creation trigger `NeedsConfirmation` asking for them
- [ ] **AC10:** All handlers propagate tenant context via SDK client
- [ ] **AC11:** All handlers return NL confirmation message with the property reference code
- [ ] **AC12:** Unit tests for each handler with mocked `IPropertiesApi`
- [ ] **AC13:** Entity resolution tests with single match, multiple matches, and no match scenarios

### Implementation Steps

1. Create `CreatePropertyActionCommand` in `Application/Actions/Commands/Properties/` — MediatR command with property fields
2. Create `CreatePropertyActionHandler` — maps extracted parameters to `CreatePropertyRequest`, calls `IPropertiesApi.CreateAsync()`
3. Create `UpdatePropertyActionCommand` and handler — resolves property, applies field updates
4. Create `QueryPropertiesActionCommand` and handler — maps filter parameters to `ListPropertiesRequest`, calls `IPropertiesApi.ListAsync()`
5. Create `ChangePropertyStatusActionCommand` and handler — resolves property, calls status change endpoint
6. Create `PropertyEntityResolver` service — matches vague references to properties (by code, city+type, address fragment) via `IPropertiesApi`
7. Create `SpanishPropertyTypeMapper` utility — bidirectional mapping of Spanish terms to `PropertyType` enum
8. Update `ActionToolDefinitions` to include detailed parameter schemas for property actions
9. Update `ActionCommandFactory` to create property action commands
10. Write unit tests for all handlers and the entity resolver
11. Write integration test for create property via NL

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/CreatePropertyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/CreatePropertyActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/UpdatePropertyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/UpdatePropertyActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/QueryPropertiesActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/QueryPropertiesActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/ChangePropertyStatusActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Properties/ChangePropertyStatusActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/PropertyEntityResolver.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Mappers/SpanishPropertyTypeMapper.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Properties/CreatePropertyActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Properties/UpdatePropertyActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Properties/QueryPropertiesActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Properties/ChangePropertyStatusActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Services/PropertyEntityResolverTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Mappers/SpanishPropertyTypeMapperTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Api/Actions/PropertyActionsTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (add property tool schemas)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs` (add property command creation)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register PropertiesApi SDK client)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `CreatePropertyActionHandler` calls `IPropertiesApi.CreateAsync()` with correct fields | xUnit, mock SDK |
| Unit | `CreatePropertyActionHandler` maps Spanish property types correctly | xUnit, parameterized |
| Unit | `UpdatePropertyActionHandler` resolves property and applies updates | xUnit, mock SDK |
| Unit | `QueryPropertiesActionHandler` maps filter parameters to query | xUnit, mock SDK |
| Unit | `ChangePropertyStatusActionHandler` resolves property and changes status | xUnit, mock SDK |
| Unit | `PropertyEntityResolver` returns single match for exact code | xUnit |
| Unit | `PropertyEntityResolver` returns multiple candidates for vague reference | xUnit |
| Unit | `PropertyEntityResolver` returns empty for no match | xUnit |
| Unit | `SpanishPropertyTypeMapper` maps all Spanish terms bidirectionally | xUnit, parameterized (15+ terms) |
| Integration | NL "create apartment in Malaga" → property created via SDK | `WebApplicationFactory` |

### Security & Privacy

- Property data flows through SDK clients which enforce tenant isolation via `TenantDelegatingHandler`
- Property addresses are business data, not PII — can be included in action results
- Agent can only create/modify properties in their own branch (enforced by properties-api)

### TDD Reminder

Write `SpanishPropertyTypeMapper` tests first (all Spanish terms). Write `PropertyEntityResolver` tests with mocked SDK responses. Write handler tests with mocked SDK. Implement to pass.

---

## Task 3.3 -- AI Content Generation

**Status:** DONE
**Dependencies:** 3.1 (action engine core)

### Goal

Expose AI-powered content extraction and generation as action handlers: text-to-property extraction, photo-based extraction, and marketing copy generation.

### Scope

**In scope:**
- `ExtractFromTextActionHandler` — paste unstructured listing text → structured property fields with per-field confidence scores
- `ExtractFromPhotosActionHandler` — upload photos → extract property type, features, room count via OpenAI vision API (max 10 images)
- `GenerateCopyActionHandler` — generate marketing descriptions in 5 languages (es, en, fr, de, nl) with tone options (professional, luxury, casual, concise)
- `ExtractedPropertyDto` with `ExtractedField<T>` wrapper (value + confidence)
- `GeneratedCopyDto` with language variants + tone used
- Prompt templates for each capability

**Out of scope:**
- Frontend UX (tasks 3.10, 3.11)
- Other action handlers (tasks 3.2, 3.4, 3.5, 3.6)

### Acceptance Criteria

- [ ] **AC1:** `ExtractFromTextActionHandler` parses listing text into `ExtractedPropertyDto` with per-field confidence scores
- [ ] **AC2:** Extracted fields include: propertyType, operationType, bedrooms, bathrooms, builtArea, usableArea, plotArea, price, currency, address (street, city, province, postalCode, country), features list, description
- [ ] **AC3:** Fields not found in text are returned as `null` with confidence 0.0
- [ ] **AC4:** Input text max 10,000 characters; exceeding returns error in result
- [ ] **AC5:** `ExtractFromPhotosActionHandler` accepts up to 10 image URLs/base64, sends to OpenAI vision, returns extracted features
- [ ] **AC6:** Photo extraction returns: estimated propertyType, features (pool, garden, terrace, etc.), estimated room count, property condition (new, good, renovated, needs-work)
- [ ] **AC7:** `GenerateCopyActionHandler` generates marketing text given property data + tone + target languages
- [ ] **AC8:** Generated copy has variants per requested language (default: es + en)
- [ ] **AC9:** Tone options: `professional`, `luxury`, `casual`, `concise` (default: professional)
- [ ] **AC10:** Copy length: 150-300 words per language variant
- [ ] **AC11:** Copy respects Spanish real estate conventions (m², certificado energetico, orientacion, etc.)
- [ ] **AC12:** Unit tests for all three handlers with mocked `IOpenAiService`
- [ ] **AC13:** Prompt templates are externalized as static classes (not inline strings in handlers)

### Implementation Steps

1. Create `ExtractedPropertyDto` and `ExtractedField<T>` in `Application/Actions/Dtos/`
2. Create `GeneratedCopyDto` in `Application/Actions/Dtos/` — `Dictionary<string, string>` (lang → text) + tone
3. Create `ExtractFromTextActionCommand` and handler in `Application/Actions/Commands/Content/`
4. Create `ExtractFromPhotosActionCommand` and handler in `Application/Actions/Commands/Content/`
5. Create `GenerateCopyActionCommand` and handler in `Application/Actions/Commands/Content/`
6. Create prompt templates in `Infrastructure/Actions/Prompts/`: `PropertyExtractionPrompt.cs`, `PhotoExtractionPrompt.cs`, `CopyGenerationPrompt.cs`
7. Extend `IOpenAiService` with `GenerateWithJsonResponseAsync(systemPrompt, userPrompt)` for structured JSON output mode
8. Extend `IOpenAiService` with `AnalyzeImagesAsync(systemPrompt, imageUrls)` for vision API
9. Update `OpenAiService` implementations
10. Update `ActionToolDefinitions` and `ActionCommandFactory` for content actions
11. Write unit tests for all handlers and prompt templates

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ExtractedPropertyDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/ExtractedFieldDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/GeneratedCopyDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromTextActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromTextActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromPhotosActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/ExtractFromPhotosActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/GenerateCopyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Content/GenerateCopyActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Prompts/PropertyExtractionPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Prompts/PhotoExtractionPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Prompts/CopyGenerationPrompt.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/ExtractFromTextActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/ExtractFromPhotosActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Content/GenerateCopyActionHandlerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IOpenAiService.cs` (add `GenerateWithJsonResponseAsync`, `AnalyzeImagesAsync`)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiService.cs` (implement new methods)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (content tool schemas)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs` (content commands)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `ExtractFromTextActionHandler` maps OpenAI JSON → `ExtractedPropertyDto` | xUnit, mock `IOpenAiService` |
| Unit | `ExtractFromTextActionHandler` handles malformed JSON gracefully | xUnit |
| Unit | `ExtractFromPhotosActionHandler` sends images to vision API, maps result | xUnit, mock |
| Unit | `ExtractFromPhotosActionHandler` rejects > 10 images | xUnit |
| Unit | `GenerateCopyActionHandler` generates copy in requested languages | xUnit, mock |
| Unit | `GenerateCopyActionHandler` applies tone correctly | xUnit, parameterized (4 tones) |
| Unit | Prompt templates produce valid, non-empty prompts for all scenarios | xUnit |
| Integration | End-to-end extract-from-text with mocked OpenAI | `WebApplicationFactory` |

### Security & Privacy

- Listing text may contain addresses (business data, not PII) — don't log content
- Photos may reveal property locations — same treatment as property data
- Generated copy is stored in memory only (not persisted unless saved as property description)

### TDD Reminder

Write `ExtractedPropertyDto` mapping tests first. Write handler tests with known OpenAI mock responses. Write prompt template tests. Implement to pass.

---

## Task 3.4 -- Contact & Lead Actions via Natural Language

**Status:** IN_PROGRESS
**Dependencies:** 3.1 (action engine core), 4.5 (ContactsApi SDK client)

### Goal

Implement action handlers for contact and lead management through natural language commands.

### Scope

**In scope:**
- `CreateLeadActionHandler` — "new lead from Maria Garcia for the apartment on Calle Mayor" → creates lead via `ILeadsApi`, resolves property
- `CreateContactActionHandler` — "add contact Juan Lopez, buyer, phone 650123456" → creates contact via `IContactsApi`
- `QualifyLeadActionHandler` — "qualify Maria's lead" → changes lead status via SDK
- `ConvertLeadActionHandler` — "convert Maria's lead to a contact" → triggers conversion flow via SDK
- `QueryLeadsActionHandler` — "show me new leads from this week", "leads for AP-2024-001"
- Automatic property matching: when a lead mentions a property (by address, code, or description), resolve it
- Contact entity resolution: match contacts by name, email, or phone fragments

**Out of scope:**
- Contacts domain logic (Phase 4)
- Frontend lead management (P4.7)

### Acceptance Criteria

- [ ] **AC1:** "Crea un lead de Maria Garcia para el piso de la calle Mayor" → creates lead with name "Maria Garcia", resolves property by address
- [ ] **AC2:** "Add contact Juan Lopez, buyer, 650123456" → creates contact with role Buyer, phone 650123456
- [ ] **AC3:** "Qualify the lead from Maria Garcia" → resolves lead by contact name, changes status to Qualified
- [ ] **AC4:** "Convert Maria's lead" → triggers lead conversion via SDK
- [ ] **AC5:** "Show me new leads this week" → queries leads with status=New, date >= start of week
- [ ] **AC6:** "Leads for AP-2024-001" → queries leads filtered by property reference
- [ ] **AC7:** Property resolution uses `PropertyEntityResolver` (from 3.2) to match property references
- [ ] **AC8:** Contact resolution matches by partial name (case-insensitive, accent-insensitive)
- [ ] **AC9:** Ambiguous contact references return `NeedsConfirmation` with candidates
- [ ] **AC10:** All handlers propagate tenant context via SDK clients
- [ ] **AC11:** Unit tests for each handler with mocked SDK clients

### Implementation Steps

1. Create `CreateLeadActionCommand` and handler in `Application/Actions/Commands/Contacts/`
2. Create `CreateContactActionCommand` and handler
3. Create `QualifyLeadActionCommand` and handler
4. Create `ConvertLeadActionCommand` and handler
5. Create `QueryLeadsActionCommand` and handler
6. Create `ContactEntityResolver` service — matches contacts by name/email/phone via `IContactsApi`
7. Update `ActionToolDefinitions` with contact/lead tool schemas
8. Update `ActionCommandFactory` for contact/lead commands
9. Write unit tests for all handlers and entity resolver

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/CreateLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/CreateLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/CreateContactActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/CreateContactActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/QualifyLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/QualifyLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/ConvertLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/ConvertLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/QueryLeadsActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Contacts/QueryLeadsActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ContactEntityResolver.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Contacts/CreateLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Contacts/CreateContactActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Contacts/QualifyLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Contacts/ConvertLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Contacts/QueryLeadsActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Services/ContactEntityResolverTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (contact/lead tool schemas)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs` (contact/lead commands)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register ContactsApi SDK client)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `CreateLeadActionHandler` creates lead with resolved property | xUnit, mock `ILeadsApi` + `PropertyEntityResolver` |
| Unit | `CreateContactActionHandler` creates contact with parsed role and phone | xUnit, mock `IContactsApi` |
| Unit | `QualifyLeadActionHandler` resolves lead by contact name and qualifies | xUnit, mock SDK |
| Unit | `ConvertLeadActionHandler` triggers conversion via SDK | xUnit, mock SDK |
| Unit | `QueryLeadsActionHandler` maps date filters ("this week", "today") to date range | xUnit |
| Unit | `ContactEntityResolver` matches by partial name (accent-insensitive) | xUnit, parameterized |
| Unit | `ContactEntityResolver` returns multiple candidates for ambiguous names | xUnit |

### Security & Privacy

- Contact names, emails, and phones are PII — **never log** these from NL input
- Lead data flows through SDK clients with tenant isolation
- Contact resolution queries are scoped to the user's branch

### TDD Reminder

Write `ContactEntityResolver` tests first (name matching, accent handling, ambiguity). Write handler tests with mocked SDKs. Implement to pass.

---

## Task 3.5 -- Operation Actions via Natural Language

**Status:** DONE
**Dependencies:** 3.2 (property actions — uses `PropertyEntityResolver` and property SDK)

### Goal

Implement action handlers for property lifecycle operations: reserve, sell, rent, archive, and reactivate.

### Scope

**In scope:**
- `ReservePropertyActionHandler` — "reserve the apartment on Calle Mayor for Maria Garcia"
- `CloseOperationActionHandler` — "close the sale of AP-2024-001" → marks Sold (if Sale) or Rented (if Rent)
- `ArchivePropertyActionHandler` — "archive property AP-2024-015"
- `ReactivatePropertyActionHandler` — "relist the apartment on Calle Mayor"
- Entity resolution for both properties and contacts (link buyer/tenant to the operation)
- Operation type inference: if the property's operation is Sale → mark Sold; if Rent → mark Rented

**Out of scope:**
- Financial tracking (price changes on close, commissions)
- Legal document generation

### Acceptance Criteria

- [ ] **AC1:** "Reserve AP-2024-001 for Maria Garcia" → resolves property + contact, changes status to Reserved
- [ ] **AC2:** "Close the sale of AP-2024-001" → marks property as Sold (inferred from operation type)
- [ ] **AC3:** "Close AP-2024-002" (which is a rental) → marks property as Rented
- [ ] **AC4:** "Archive property AP-2024-015" → changes status to Archived
- [ ] **AC5:** "Relist the Malaga apartment" → resolves property, changes status from Archived/Rented to Active
- [ ] **AC6:** Invalid transitions (e.g., "sell a draft property") return error with explanation
- [ ] **AC7:** If property reference is ambiguous, return `NeedsConfirmation` with candidates
- [ ] **AC8:** NL confirmation includes property reference, new status, and linked contact (if any)
- [ ] **AC9:** Unit tests for each handler with mocked SDK

### Implementation Steps

1. Create `ReservePropertyActionCommand` and handler in `Application/Actions/Commands/Operations/`
2. Create `CloseOperationActionCommand` and handler — infers Sold/Rented from operation type
3. Create `ArchivePropertyActionCommand` and handler
4. Create `ReactivatePropertyActionCommand` and handler
5. Update `ActionToolDefinitions` with operation tool schemas
6. Update `ActionCommandFactory` for operation commands
7. Write unit tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReservePropertyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReservePropertyActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/CloseOperationActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/CloseOperationActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ArchivePropertyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ArchivePropertyActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReactivatePropertyActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Operations/ReactivatePropertyActionHandler.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Operations/ReservePropertyActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Operations/CloseOperationActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Operations/ArchivePropertyActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Operations/ReactivatePropertyActionHandlerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (operation tool schemas)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs` (operation commands)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `ReservePropertyActionHandler` resolves property + contact, reserves via SDK | xUnit, mock SDK |
| Unit | `CloseOperationActionHandler` infers Sold for Sale operations | xUnit |
| Unit | `CloseOperationActionHandler` infers Rented for Rent operations | xUnit |
| Unit | `ArchivePropertyActionHandler` archives via SDK | xUnit, mock SDK |
| Unit | `ReactivatePropertyActionHandler` reactivates from Archived or Rented | xUnit |
| Unit | Invalid transition returns descriptive error | xUnit |

### Security & Privacy

- Same as task 3.2 — operations flow through SDK clients with tenant isolation

### TDD Reminder

Write operation inference tests first (Sale→Sold, Rent→Rented). Write handler tests with mocked SDK. Implement to pass.

---

## Task 3.6 -- Appointment Actions via Natural Language

**Status:** BLOCKED
**Dependencies:** 3.1 (action engine core), 5.8 (AppointmentsApi SDK client)

### Goal

Implement action handlers for scheduling, querying, cancelling, and rescheduling appointments through natural language.

### Scope

**In scope:**
- `BookViewingActionHandler` — "book a viewing for the Malaga villa with Maria Garcia next Tuesday at 10am"
- `QueryAppointmentsActionHandler` — "what do I have this week?", "appointments for AP-2024-001"
- `CancelAppointmentActionHandler` — "cancel Tuesday's viewing"
- `RescheduleAppointmentActionHandler` — "move the viewing to Wednesday at 3pm"
- Relative date parsing: "next Tuesday", "this Friday", "tomorrow afternoon", "mañana a las 10"
- Default viewing duration: 30 minutes (configurable)
- Entity resolution: link viewing to property + contact

**Out of scope:**
- Calendar sync (P5.3–5.5)
- Availability checking (future enhancement)

### Acceptance Criteria

- [ ] **AC1:** "Book a viewing for the Malaga villa with Maria Garcia next Tuesday at 10am" → creates appointment with: type=PropertyViewing, property resolved, contact resolved, start=next Tuesday 10:00 UTC+1, end=10:30
- [ ] **AC2:** "Agenda una visita para el piso de la calle Mayor el viernes a las 16:00" → handles Spanish input with relative dates
- [ ] **AC3:** "What do I have this week?" → returns list of agent's appointments for current week
- [ ] **AC4:** "Cancel Tuesday's viewing" → resolves appointment by day + type, cancels
- [ ] **AC5:** "Move the Malaga viewing to Wednesday at 3pm" → resolves appointment, reschedules
- [ ] **AC6:** Relative date parsing handles: "tomorrow", "next [day]", "this [day]", "[day] at [time]", "mañana", "el viernes", "la semana que viene"
- [ ] **AC7:** Default viewing duration is 30 minutes (configurable via `ActionEngineOptions`)
- [ ] **AC8:** If multiple appointments match a reference, return `NeedsConfirmation` with list
- [ ] **AC9:** Past dates return error ("Cannot book a viewing in the past")
- [ ] **AC10:** All handlers propagate tenant context via SDK clients
- [ ] **AC11:** Unit tests for each handler and date parser

### Implementation Steps

1. Create `BookViewingActionCommand` and handler in `Application/Actions/Commands/Appointments/`
2. Create `QueryAppointmentsActionCommand` and handler
3. Create `CancelAppointmentActionCommand` and handler
4. Create `RescheduleAppointmentActionCommand` and handler
5. Create `RelativeDateParser` utility — parses relative dates in English and Spanish to `DateTimeOffset`
6. Create `AppointmentEntityResolver` — matches appointments by date + property + type
7. Update `ActionToolDefinitions` with appointment tool schemas
8. Update `ActionCommandFactory` for appointment commands
9. Write unit tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/BookViewingActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/BookViewingActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/QueryAppointmentsActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/QueryAppointmentsActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/CancelAppointmentActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/CancelAppointmentActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/RescheduleAppointmentActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/Appointments/RescheduleAppointmentActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/RelativeDateParser.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/AppointmentEntityResolver.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Appointments/BookViewingActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Appointments/QueryAppointmentsActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Appointments/CancelAppointmentActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Commands/Appointments/RescheduleAppointmentActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Services/RelativeDateParserTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Services/AppointmentEntityResolverTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (appointment tool schemas)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/ActionCommandFactory.cs` (appointment commands)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register AppointmentsApi SDK client)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `RelativeDateParser` parses "next Tuesday" relative to current date | xUnit, fixed clock |
| Unit | `RelativeDateParser` parses "mañana a las 10" | xUnit, fixed clock |
| Unit | `RelativeDateParser` parses "el viernes a las 16:00" | xUnit, fixed clock |
| Unit | `RelativeDateParser` rejects past dates | xUnit |
| Unit | `BookViewingActionHandler` creates appointment with resolved property + contact | xUnit, mock SDK |
| Unit | `BookViewingActionHandler` applies 30-minute default duration | xUnit |
| Unit | `QueryAppointmentsActionHandler` maps "this week" to date range | xUnit |
| Unit | `CancelAppointmentActionHandler` resolves and cancels | xUnit, mock SDK |
| Unit | `RescheduleAppointmentActionHandler` resolves and updates time | xUnit, mock SDK |
| Unit | `AppointmentEntityResolver` disambiguates by date + property | xUnit |

### Security & Privacy

- Appointment details include contact names (PII) — don't log from NL input
- Tenant isolation via SDK clients

### TDD Reminder

Write `RelativeDateParser` tests first (both English and Spanish, with fixed clock). Write entity resolver tests. Write handler tests. Implement to pass.

---

## Task 3.7 -- Voice Input (Speech-to-Text)

**Status:** DONE
**Dependencies:** 3.1 (action engine core for pipeline integration)

### Goal

Add voice transcription via `gpt-4o-mini-transcription` (OpenAI Audio API), enabling the voice→text→action pipeline.

### Scope

**In scope:**
- `POST /v1/voice/transcribe` — accepts audio file (multipart), returns transcribed text + detected language
- `POST /v1/voice/execute` — accepts audio file, transcribes, then executes action (combined endpoint for single-step voice→action)
- `IVoiceTranscriptionService` interface in Application layer
- `OpenAiVoiceTranscriptionService` in Infrastructure layer using `gpt-4o-mini-transcription` model via OpenAI Audio API
- Supported audio formats: WebM/Opus (browser MediaRecorder default), WAV, MP3, M4A
- Audio size limit: 25 MB (OpenAI limit)
- Supported languages: Spanish (primary), English, French, German, Dutch (language hint optional)
- Response: `{ "text": "...", "language": "es", "duration_ms": 3200 }`

**Out of scope:**
- Text-to-Speech / voice response (future enhancement)
- Real-time streaming transcription (batch only)
- Frontend voice UI (task 3.11)

### Acceptance Criteria

- [ ] **AC1:** `POST /v1/voice/transcribe` accepts `multipart/form-data` with `audio` file and optional `language` hint
- [ ] **AC2:** Returns `{ "text": "...", "language": "es", "durationMs": 3200 }` on success
- [ ] **AC3:** Uses `gpt-4o-mini-transcription` model via OpenAI Audio Transcription API
- [ ] **AC4:** Audio files > 25 MB return 400 with descriptive error
- [ ] **AC5:** Unsupported audio format returns 400 (accept: webm, wav, mp3, m4a, ogg, flac)
- [ ] **AC6:** Empty/silent audio returns 200 with empty text (let the caller decide)
- [ ] **AC7:** `POST /v1/voice/execute` transcribes audio then chains into `POST /v1/actions/execute` internally
- [ ] **AC8:** `/v1/voice/execute` returns `ActionResultDto` with an additional `transcribedText` field
- [ ] **AC9:** OpenAI Audio API errors are caught → 502 with safe error message
- [ ] **AC10:** Rate limit: 10 req/min per user for voice endpoints (audio processing is expensive)
- [ ] **AC11:** Telemetry: `voice.transcribed`, `voice.duration_ms`, `voice.language`, `voice.executed`
- [ ] **AC12:** Unit tests for transcription service with mocked Audio API
- [ ] **AC13:** Integration test for transcribe endpoint with mocked OpenAI

### Implementation Steps

1. Create `IVoiceTranscriptionService` interface in `Application/Common/Interfaces/` — `TranscribeAsync(Stream audio, string? languageHint, CancellationToken ct)` returns `TranscriptionResult { Text, Language, DurationMs }`
2. Create `TranscriptionResult` record in `Application/Actions/Models/`
3. Create `VoiceExecuteResultDto` in `Application/Actions/Dtos/` — extends `ActionResultDto` with `TranscribedText`
4. Create `OpenAiVoiceTranscriptionService` in `Infrastructure/Services/` — uses OpenAI `AudioClient` with model `gpt-4o-mini-transcription`
5. Create `VoiceController` in `Api/Controllers/` with `POST /v1/voice/transcribe` and `POST /v1/voice/execute`
6. Create `TranscribeRequestValidator` — checks file size, content type
7. Register `IVoiceTranscriptionService` in DI (Singleton, configured with OpenAI API key)
8. Add rate limiting rules for voice endpoints (10/min)
9. Write unit tests for transcription service
10. Write integration tests for both endpoints

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IVoiceTranscriptionService.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Models/TranscriptionResult.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Dtos/VoiceExecuteResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiVoiceTranscriptionService.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/VoiceController.cs`
- `services/ai-api/src/Propely.AiApi.Api/Validators/TranscribeRequestValidator.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Services/OpenAiVoiceTranscriptionServiceTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Api/VoiceControllerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register `IVoiceTranscriptionService`)
- `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs` (rate limiting for voice endpoints)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiOptions.cs` (add `TranscriptionModelId` property, default `gpt-4o-mini-transcription`)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `OpenAiVoiceTranscriptionService` sends audio to API and maps response | xUnit, mock `AudioClient` |
| Unit | `OpenAiVoiceTranscriptionService` handles API error → exception | xUnit |
| Unit | `TranscribeRequestValidator` rejects > 25 MB | xUnit |
| Unit | `TranscribeRequestValidator` rejects unsupported formats | xUnit |
| Unit | `TranscribeRequestValidator` accepts webm, wav, mp3, m4a, ogg, flac | xUnit, parameterized |
| Integration | `POST /v1/voice/transcribe` with valid audio returns transcription | `WebApplicationFactory` |
| Integration | `POST /v1/voice/transcribe` with oversized file returns 400 | `WebApplicationFactory` |
| Integration | `POST /v1/voice/execute` transcribes + executes action | `WebApplicationFactory` |
| Integration | Voice endpoints without auth return 401 | `WebApplicationFactory` |

### Security & Privacy

- Audio files may contain voices (biometric data) — **never** persist audio after transcription
- Audio is streamed to OpenAI API and discarded from memory after response
- Transcribed text follows same privacy rules as text input (don't log content)
- Rate limiting critical to prevent abuse (audio processing is resource-intensive)

### TDD Reminder

Write validator tests first (file size, format). Write transcription service tests with mocked AudioClient. Write controller integration tests. Implement to pass.

---

## Task 3.8 -- AI-API NuGet SDK Client

**Status:** DONE
**Dependencies:** 3.1 (actions endpoint), 3.7 (voice endpoint)

### Goal

Create the `Propely.AiApi.Client` NuGet SDK package for other services and the frontend to consume AI action engine capabilities.

### Scope

**In scope:**
- `Propely.AiApi.Client` project following the established SDK client pattern (Refit + Polly + `TenantDelegatingHandler`)
- `IActionApi` — `ExecuteActionAsync(ExecuteActionRequest)` returns `ActionResultDto`
- `IVoiceApi` — `TranscribeAsync(Stream audio, string? languageHint)`, `ExecuteVoiceActionAsync(Stream audio)`
- `IContentApi` — `ExtractFromTextAsync(string text)`, `ExtractFromPhotosAsync(string[] imageUrls)`, `GenerateCopyAsync(GenerateCopyRequest)`
- Typed request/response DTOs for all endpoints
- `AddAiApiClient(IServiceCollection, Action<AiApiClientOptions>)` extension method
- Polly policies: 60-second timeout for AI calls (longer than standard 30s), no retry (AI calls are not idempotent)

**Out of scope:**
- Frontend consumption (uses `aiApiFetch` directly, not this SDK)
- Action handler implementations (separate tasks)

### Acceptance Criteria

- [ ] **AC1:** `Propely.AiApi.Client` project compiles and follows the established SDK pattern
- [ ] **AC2:** `IActionApi.ExecuteActionAsync()` sends POST to `/v1/actions/execute`
- [ ] **AC3:** `IVoiceApi.TranscribeAsync()` sends multipart POST to `/v1/voice/transcribe`
- [ ] **AC4:** `IVoiceApi.ExecuteVoiceActionAsync()` sends multipart POST to `/v1/voice/execute`
- [ ] **AC5:** `IContentApi.ExtractFromTextAsync()` → POST `/v1/actions/execute` with `ExtractFromText` intent
- [ ] **AC6:** `IContentApi.GenerateCopyAsync()` → POST `/v1/actions/execute` with `GenerateCopy` intent
- [ ] **AC7:** All interfaces use `TenantDelegatingHandler` for tenant propagation
- [ ] **AC8:** Polly timeout is 60 seconds for all AI endpoints
- [ ] **AC9:** No automatic retry (AI operations are not idempotent)
- [ ] **AC10:** `AddAiApiClient()` registers all interfaces, HTTP client, and handlers
- [ ] **AC11:** DI registration test passes
- [ ] **AC12:** Polly policy test confirms 60s timeout and no retry

### Implementation Steps

1. Create `Propely.AiApi.Client` project in `services/ai-api/src/`
2. Create request DTOs: `ExecuteActionRequest`, `GenerateCopyRequest`
3. Create response DTOs: `ActionResultDto`, `TranscriptionResultDto`, `VoiceExecuteResultDto`, `ExtractedPropertyDto`
4. Create Refit interface `IActionApi` with `[Post("/v1/actions/execute")]`
5. Create Refit interface `IVoiceApi` with multipart endpoints
6. Create Refit interface `IContentApi` with convenience methods
7. Create `IAiApiClient` aggregate interface (combines all three)
8. Create `AiApiClientOptions` — `BaseUrl`, `TimeoutSeconds` (default 60)
9. Create `ServiceCollectionExtensions.AddAiApiClient()` with Polly policies
10. Add project to solution
11. Write DI registration tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Client/Propely.AiApi.Client.csproj`
- `services/ai-api/src/Propely.AiApi.Client/IAiApiClient.cs`
- `services/ai-api/src/Propely.AiApi.Client/IActionApi.cs`
- `services/ai-api/src/Propely.AiApi.Client/IVoiceApi.cs`
- `services/ai-api/src/Propely.AiApi.Client/IContentApi.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/ExecuteActionRequest.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/ActionResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/TranscriptionResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/VoiceExecuteResultDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/ExtractedPropertyDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/ExtractedFieldDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/GenerateCopyRequest.cs`
- `services/ai-api/src/Propely.AiApi.Client/Dtos/GeneratedCopyDto.cs`
- `services/ai-api/src/Propely.AiApi.Client/Configuration/AiApiClientOptions.cs`
- `services/ai-api/src/Propely.AiApi.Client/Configuration/ServiceCollectionExtensions.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs`

**Modify:**
- `services/ai-api/Propely.AiApi.sln` (add Client project)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `AddAiApiClient()` registers all interfaces in DI | xUnit, `ServiceCollection` assertions |
| Unit | Polly timeout policy is 60 seconds | xUnit, inspect HTTP client handler chain |
| Unit | No retry policy configured | xUnit, inspect handler chain |
| Unit | `TenantDelegatingHandler` is registered in pipeline | xUnit |

### TDD Reminder

Write DI registration tests first. Verify all interfaces resolve from the service provider. Implement extension method to pass.

---

## Task 3.9 -- Prompt Engineering & Spanish RE Vocabulary

**Status:** BLOCKED
**Dependencies:** 3.2, 3.3, 3.4 (all action handlers that use prompts)

### Goal

Systematic prompt optimization for the Spanish real estate domain. Ensure accurate entity extraction, correct terminology mapping, and high accuracy on realistic inputs.

### Scope

**In scope:**
- Spanish real estate vocabulary mapping (comprehensive, bidirectional)
- Property type terms: piso, apartamento, atico, bajo, duplex, loft, estudio, adosado, pareado, chalet, villa, finca, cortijo, masía, local comercial, oficina, nave industrial, solar, garaje, trastero, edificio
- Operation terms: venta, alquiler, alquiler vacacional, traspaso, alquiler con opcion a compra
- Feature terms: piscina, jardín, garaje, trastero, aire acondicionado, calefacción, ascensor, terraza, balcón, amueblado, luminoso, exterior, interior, reformado, a estrenar
- Financial terms: comunidad (de propietarios), IBI, catastro
- Energy certificate terms: certificado energético clase A-G, consumo, emisiones
- Area terms: m² construidos, m² útiles, m² de parcela
- Few-shot examples in the system prompt covering:
  - Idealista/Fotocasa-style listing text (informal Spanish)
  - WhatsApp-style abbreviated messages
  - Formal property descriptions
  - Mixed Spanish-English input
- Prompt regression test suite: 25+ input/expected-output pairs
- Accuracy threshold: >= 85% field extraction accuracy across test suite

**Out of scope:**
- Portuguese market terminology (later phase)
- Custom models / fine-tuning

### Acceptance Criteria

- [ ] **AC1:** `SpanishRealEstateVocabulary` static class maps all 20+ property type terms to `PropertyType` enum
- [ ] **AC2:** Vocabulary handles accented and unaccented variants (atico ↔ ático)
- [ ] **AC3:** Vocabulary handles plural forms (pisos → Apartment, chalets → Villa)
- [ ] **AC4:** Few-shot examples cover 4+ listing styles (Idealista, WhatsApp, formal, mixed)
- [ ] **AC5:** System prompts include vocabulary context and examples
- [ ] **AC6:** All tool definitions include Spanish term aliases in parameter descriptions
- [ ] **AC7:** Prompt regression test suite has 25+ test cases
- [ ] **AC8:** Each test case specifies: input text, expected action type, expected key parameters
- [ ] **AC9:** Test suite runs as part of `dotnet test` in the unit test project
- [ ] **AC10:** Overall accuracy >= 85% across all test cases (measured by field match)
- [ ] **AC11:** Failing test cases are documented with analysis for future improvement

### Implementation Steps

1. Create `SpanishRealEstateVocabulary` static class in `Infrastructure/Actions/Vocabulary/`
2. Create `VocabularyTests` comprehensive parameterized tests
3. Update all prompt templates to include vocabulary context and few-shot examples
4. Update `ActionToolDefinitions` parameter descriptions with Spanish aliases
5. Create `PromptRegressionTests` test class with 25+ test cases
6. Run regression suite, analyze failures, iterate on prompts
7. Document accuracy metrics and known limitations

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Vocabulary/SpanishRealEstateVocabulary.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Vocabulary/OperationTypeVocabulary.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Vocabulary/FeatureVocabulary.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/Vocabulary/SpanishRealEstateVocabularyTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/Vocabulary/OperationTypeVocabularyTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/Vocabulary/FeatureVocabularyTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Actions/PromptRegressionTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Actions/TestData/regression-cases.json`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ActionToolDefinitions.cs` (add Spanish aliases)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Prompts/PropertyExtractionPrompt.cs` (add vocabulary + examples)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/Prompts/CopyGenerationPrompt.cs` (Spanish RE conventions)
- `services/ai-api/src/Propely.AiApi.Application/Actions/Mappers/SpanishPropertyTypeMapper.cs` (use vocabulary class)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `SpanishRealEstateVocabulary` maps all 20+ property types | xUnit, parameterized |
| Unit | Vocabulary handles accents (atico/ático) | xUnit, parameterized |
| Unit | Vocabulary handles plurals | xUnit, parameterized |
| Unit | Operation type vocabulary maps all terms | xUnit, parameterized |
| Unit | Feature vocabulary maps all terms | xUnit, parameterized |
| Regression | 25+ NL inputs produce correct action type + key parameters | xUnit `[Theory]` with JSON data |
| Regression | Idealista-style listing extracts >= 85% fields correctly | xUnit |
| Regression | WhatsApp-style short message extracts key fields | xUnit |
| Regression | Mixed Spanish-English input handled | xUnit |

### Security & Privacy

- Testing data must not contain real property listings or real names
- Use synthetic, realistic test data

### TDD Reminder

Write vocabulary mapping tests first (all terms, accents, plurals). Write regression test framework. Add test cases. Iterate on prompts until accuracy threshold is met.

---

## Task 3.10 -- Frontend: Command Bar

**Status:** DONE
**Dependencies:** 3.8 (AI SDK client endpoints available)

### Goal

Build a global command bar accessible via `Ctrl+K` / `Cmd+K` that allows users to type natural language commands and see results.

### Scope

**In scope:**
- `cmdk`-based command palette overlay
- Free-form text input at the top
- Submit via Enter key or submit button
- Loading state while action executes
- Result display: success confirmation with entity link, or error with retry option
- `NeedsConfirmation` state: show extracted parameters, ask user to confirm or edit
- Recent commands list (last 10, session-scoped in state, not persisted)
- Keyboard shortcut: `Ctrl+K` / `Cmd+K` to open, `Escape` to close
- Accessible: focus trap, screen reader labels, keyboard navigation
- Responsive: full-screen on mobile, centered modal on desktop
- i18n: en + es
- Global placement: rendered in root layout, available from any page

**Out of scope:**
- Voice mode (task 3.11)
- Conversation history/context (P7.2)
- Autocomplete suggestions from AI (future enhancement)

### Acceptance Criteria

- [ ] **AC1:** `Ctrl+K` / `Cmd+K` opens command bar overlay from any page
- [ ] **AC2:** `Escape` closes command bar
- [ ] **AC3:** Typing text and pressing Enter sends `POST /v1/actions/execute` via `aiApiFetch`
- [ ] **AC4:** Loading spinner shown while waiting for response
- [ ] **AC5:** Success result shows NL confirmation message and a link to the created/modified entity
- [ ] **AC6:** Error result shows error message with a "Try again" option
- [ ] **AC7:** `NeedsConfirmation` result shows extracted parameters and "Confirm" / "Cancel" buttons
- [ ] **AC8:** On confirm, re-sends the action with the confirmed parameters
- [ ] **AC9:** Recent commands shown below input (last 10, most recent first)
- [ ] **AC10:** Clicking a recent command repopulates the input
- [ ] **AC11:** Command bar has focus trap (Tab cycles within the overlay)
- [ ] **AC12:** Command bar has `role="dialog"` and `aria-label`
- [ ] **AC13:** Mobile: full-screen overlay. Desktop: centered modal (max-w-2xl)
- [ ] **AC14:** Stitch design exists for command bar (desktop + mobile)
- [ ] **AC15:** All components tested with Vitest + RTL
- [ ] **AC16:** i18n keys for en + es

### Implementation Steps

1. Install `cmdk` package
2. Create Stitch design for command bar (desktop + mobile variants)
3. Download Stitch HTML to `.stitch-html/command-bar.html`
4. Create `useCommandBar` hook — manages open/close state, keyboard shortcut, recent commands
5. Create `useExecuteAction` hook — wraps `aiApiFetch('/v1/actions/execute', ...)`
6. Create `CommandBar` component (main overlay using `cmdk`)
7. Create `CommandInput` component (text input with submit)
8. Create `CommandResult` component (success, error, needs-confirmation states)
9. Create `CommandHistory` component (recent commands list)
10. Create `ConfirmationPanel` component (shows extracted params for confirmation)
11. Add `CommandBar` to root layout (rendered globally)
12. Add keyboard shortcut listener in layout
13. Add i18n keys
14. Write component tests

### Files to Create/Modify

**Create:**
- `.stitch-html/command-bar.html`
- `apps/web/src/hooks/use-command-bar.ts`
- `apps/web/src/hooks/use-execute-action.ts`
- `apps/web/src/components/command-bar/CommandBar.tsx`
- `apps/web/src/components/command-bar/CommandInput.tsx`
- `apps/web/src/components/command-bar/CommandResult.tsx`
- `apps/web/src/components/command-bar/CommandHistory.tsx`
- `apps/web/src/components/command-bar/ConfirmationPanel.tsx`
- `apps/web/src/components/command-bar/__tests__/CommandBar.test.tsx`
- `apps/web/src/components/command-bar/__tests__/CommandInput.test.tsx`
- `apps/web/src/components/command-bar/__tests__/CommandResult.test.tsx`
- `apps/web/src/components/command-bar/__tests__/CommandHistory.test.tsx`
- `apps/web/src/components/command-bar/__tests__/ConfirmationPanel.test.tsx`

**Modify:**
- `apps/web/src/app/[locale]/layout.tsx` (add `<CommandBar />` to layout)
- `apps/web/src/messages/en.json` (add `commandBar.*` keys)
- `apps/web/src/messages/es.json` (add `commandBar.*` keys)
- `apps/web/package.json` (add `cmdk` dependency)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `CommandBar` opens on `Ctrl+K` keydown | Vitest + RTL, `fireEvent.keyDown` |
| Unit | `CommandBar` closes on Escape | Vitest + RTL |
| Unit | `CommandInput` submits text on Enter | Vitest + RTL |
| Unit | `CommandResult` renders success with entity link | Vitest + RTL |
| Unit | `CommandResult` renders error with retry button | Vitest + RTL |
| Unit | `ConfirmationPanel` shows extracted params and confirm/cancel | Vitest + RTL |
| Unit | `CommandHistory` renders recent commands | Vitest + RTL |
| Unit | `CommandHistory` click repopulates input | Vitest + RTL |
| Unit | `useCommandBar` manages recent commands (max 10) | Vitest |
| Unit | `useExecuteAction` calls `aiApiFetch` with correct payload | Vitest, mock fetch |
| Manual | Open command bar, type command, verify result | Dev environment |
| Manual | Visual comparison against Stitch design | Dev environment |

### Security & Privacy

- Command bar input may contain PII (contact names, phone numbers) — client-side only, not persisted
- Recent commands stored in React state (cleared on page refresh)
- All API calls use existing `aiApiFetch` with JWT auth and tenant context

### TDD Reminder

Write hook tests first (`useCommandBar` state management, `useExecuteAction` API call). Write component render tests for each state. Implement components to pass.

---

## Task 3.11 -- Frontend: Voice Mode

**Status:** DONE
**Dependencies:** 3.10 (command bar must exist), 3.7 (voice transcription endpoint)

### Goal

Add a microphone button to the command bar that records audio, sends it for transcription via `gpt-4o-mini-transcription`, and feeds the text into the command pipeline.

### Scope

**In scope:**
- Microphone button in command bar (icon: microphone)
- Two modes: push-to-talk (hold button) and toggle (click to start/stop)
- Browser `MediaRecorder` API for audio capture
- Audio format: WebM/Opus (browser default, widely supported)
- Recording state indicator: pulsing red ring around mic button
- Waveform visualization during recording (using `AnalyserNode` from Web Audio API)
- After recording stops: upload audio via `POST /v1/voice/execute` (combined endpoint)
- Transcribed text appears in command bar input field
- Action result displayed as usual
- Error handling: microphone permission denied → show permission guide, no audio detected → retry prompt, transcription failed → error message
- Mobile: large tap target (48x48 min), optional haptic feedback via Vibration API
- i18n: en + es

**Out of scope:**
- Real-time streaming transcription (entire recording sent at once)
- Text-to-Speech for results (future)
- Wake word detection ("Hey Propely")

### Acceptance Criteria

- [ ] **AC1:** Microphone button visible in command bar (right side of input field)
- [ ] **AC2:** Click mic button → request microphone permission, start recording
- [ ] **AC3:** If permission denied → show "Microphone access required" message with link to browser settings
- [ ] **AC4:** While recording: mic button shows pulsing red ring, waveform visualization renders
- [ ] **AC5:** Click mic button again (or release if push-to-talk) → stop recording
- [ ] **AC6:** After stop: show loading state, upload audio to `/v1/voice/execute`
- [ ] **AC7:** Transcribed text appears in command bar input field
- [ ] **AC8:** Action result displayed in `CommandResult` component
- [ ] **AC9:** If no audio detected (< 0.5s or silence) → show "No audio detected" message
- [ ] **AC10:** If transcription fails → show error with "Try again" option
- [ ] **AC11:** Push-to-talk mode: hold mic button to record, release to send (configurable, default: toggle)
- [ ] **AC12:** Mobile: mic button is at least 48x48 CSS pixels
- [ ] **AC13:** Optional haptic feedback on recording start/stop (Vibration API, 50ms pulse)
- [ ] **AC14:** Stitch design for voice recording state (desktop + mobile)
- [ ] **AC15:** All components tested with Vitest + RTL (MediaRecorder mocked)
- [ ] **AC16:** i18n keys for en + es

### Implementation Steps

1. Create Stitch design for voice recording state (pulsing mic, waveform)
2. Download Stitch HTML to `.stitch-html/command-bar-voice.html`
3. Create `useVoiceInput` hook — manages MediaRecorder lifecycle, audio chunks, permission state
4. Create `useAudioWaveform` hook — connects MediaRecorder stream to Web Audio API AnalyserNode
5. Create `VoiceMicButton` component — mic icon, pulsing animation, recording states
6. Create `AudioWaveform` component — canvas-based waveform visualization
7. Integrate `VoiceMicButton` into `CommandBar` component
8. Connect `useVoiceInput` output to `useExecuteAction` (voice execute endpoint)
9. Add i18n keys for voice states
10. Write component tests with mocked MediaRecorder

### Files to Create/Modify

**Create:**
- `.stitch-html/command-bar-voice.html`
- `apps/web/src/hooks/use-voice-input.ts`
- `apps/web/src/hooks/use-audio-waveform.ts`
- `apps/web/src/components/command-bar/VoiceMicButton.tsx`
- `apps/web/src/components/command-bar/AudioWaveform.tsx`
- `apps/web/src/components/command-bar/__tests__/VoiceMicButton.test.tsx`
- `apps/web/src/components/command-bar/__tests__/AudioWaveform.test.tsx`
- `apps/web/src/components/command-bar/__tests__/use-voice-input.test.ts`

**Modify:**
- `apps/web/src/components/command-bar/CommandBar.tsx` (add VoiceMicButton)
- `apps/web/src/hooks/use-execute-action.ts` (add `executeVoiceAction` for multipart upload)
- `apps/web/src/messages/en.json` (add `commandBar.voice.*` keys)
- `apps/web/src/messages/es.json` (add `commandBar.voice.*` keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `VoiceMicButton` renders mic icon in idle state | Vitest + RTL |
| Unit | `VoiceMicButton` shows pulsing animation when recording | Vitest + RTL |
| Unit | `VoiceMicButton` click requests mic permission | Vitest + RTL, mock `navigator.mediaDevices` |
| Unit | `VoiceMicButton` shows permission denied message | Vitest + RTL |
| Unit | `useVoiceInput` starts MediaRecorder on startRecording() | Vitest, mock MediaRecorder |
| Unit | `useVoiceInput` collects audio chunks and creates Blob | Vitest |
| Unit | `useVoiceInput` rejects recordings < 0.5s | Vitest |
| Unit | `useVoiceInput` stops and returns audio Blob | Vitest |
| Unit | `AudioWaveform` renders canvas element | Vitest + RTL |
| Unit | Voice execute uploads audio as multipart and returns result | Vitest, mock fetch |
| Manual | Record voice, verify transcription and action execution | Dev environment |
| Manual | Test on mobile: tap target size, haptic feedback | Mobile browser |
| Manual | Visual comparison against Stitch design | Dev environment |

### Security & Privacy

- Audio is captured locally and uploaded directly to the server — **not stored** in the browser
- Audio Blob is released from memory immediately after upload
- Microphone permission is requested only on user interaction (not on page load)
- No audio data is persisted client-side or server-side after transcription

### TDD Reminder

Write `useVoiceInput` hook tests first (MediaRecorder mocking, permission handling, audio chunk assembly). Write component tests for recording states. Implement to pass.
