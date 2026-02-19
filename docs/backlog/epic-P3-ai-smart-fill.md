# Epic P3 -- AI Smart-Fill & Content Generation

## Overview

Enable AI-powered field extraction and content generation for property listings. Agents can paste unstructured text or upload photos, and the system extracts structured property data with confidence scores. Additionally, generate marketing copy in multiple languages with configurable tone. All AI capabilities are exposed through the `ai-api` service and consumed by the frontend via an SDK client.

## Service Ownership

| Capability | Service |
|---|---|
| AI extraction & generation endpoints | `services/ai-api` |
| NuGet SDK client | `services/ai-api` (client package) |
| Frontend UX | `apps/web` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 3.1 | AI Field Extraction from Text | PENDING | -- |
| 3.2 | AI Field Extraction from Photos | PENDING | 3.1 |
| 3.3 | AI Copy Generation | PENDING | 3.1 |
| 3.4 | AI Prompt Engineering & Tuning | PENDING | 3.1, 3.2, 3.3 |
| 3.5 | AI-API NuGet SDK Client Enhancement | PENDING | 3.1, 3.2, 3.3 |
| 3.6 | Frontend Smart-Fill UX | PENDING | 3.5 |

---

## Task 3.1 -- AI Field Extraction from Text

**Status:** PENDING
**Dependencies:** None

### Scope

**In scope:**
- `POST /api/properties/extract-from-text` endpoint in `ai-api`
- OpenAI `gpt-5-mini` integration for structured extraction
- Return structured JSON matching the property schema with per-field confidence scores (0.0--1.0)
- Input validation (max text length, empty input rejection)
- Tenant-scoped request context
- Structured logging and telemetry

**Out of scope:**
- Photo-based extraction (task 3.2)
- Frontend integration (task 3.6)
- Prompt optimization for Spanish terminology (task 3.4)

### Acceptance Criteria

- [ ] `POST /api/properties/extract-from-text` accepts `{ "text": "..." }` and returns structured property JSON
- [ ] Response includes all property schema fields: `propertyType`, `bedrooms`, `bathrooms`, `squareMeters`, `price`, `currency`, `address`, `city`, `province`, `postalCode`, `features`, `description`
- [ ] Each extracted field has a `confidence` score between 0.0 and 1.0
- [ ] Fields not found in text are returned as `null` with confidence 0.0
- [ ] Input text exceeding 10,000 characters returns 400 Bad Request
- [ ] Empty or whitespace-only text returns 400 Bad Request
- [ ] Endpoint requires authentication and tenant context
- [ ] OpenAI API errors are caught and returned as 502 Bad Gateway with a safe error message
- [ ] Request/response is logged (text content excluded from logs for privacy)
- [ ] Unit tests achieve >= 90% coverage for the command handler
- [ ] Integration test confirms end-to-end extraction with a mocked OpenAI response

### Implementation Steps

1. Define `ExtractedPropertyDto` record in `Application/Properties/Dtos/` with all property fields plus confidence scores
2. Define `ExtractedFieldDto<T>` generic wrapper: `{ Value: T?, Confidence: double }`
3. Create `ExtractFromTextCommand` (MediatR `IRequest<ExtractedPropertyDto>`) in `Application/Properties/Commands/ExtractFromText/`
4. Create `ExtractFromTextCommandValidator` using FluentValidation (max length, non-empty)
5. Create `ExtractFromTextCommandHandler` that calls `IOpenAiService.ExtractPropertyFieldsAsync(text)`
6. Extend `IOpenAiService` interface with `ExtractPropertyFieldsAsync(string text, CancellationToken ct)` method
7. Implement `ExtractPropertyFieldsAsync` in `Infrastructure/Ai/OpenAiService.cs` using structured output (JSON mode) with `gpt-5-mini`
8. Create the system prompt for extraction with JSON schema enforcement
9. Add `PropertyExtractionController` with `POST /api/properties/extract-from-text` in `Api/Controllers/`
10. Register new services in DI
11. Write unit tests for handler, validator, and OpenAI service (mocked HTTP)
12. Write integration test with `WebApplicationFactory` and mocked OpenAI

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Properties/Dtos/ExtractedPropertyDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Dtos/ExtractedFieldDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromText/ExtractFromTextCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromText/ExtractFromTextCommandHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromText/ExtractFromTextCommandValidator.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/PropertyExtractionPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/PropertyExtractionController.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/ExtractFromTextCommandHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/ExtractFromTextCommandValidatorTests.cs`
- `services/ai-api/tests/Propely.AiApi.Api.Tests/Controllers/PropertyExtractionControllerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IOpenAiService.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/OpenAiService.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `ExtractFromTextCommandValidator` rejects empty/oversized input | xUnit + FluentAssertions |
| Unit | `ExtractFromTextCommandHandler` maps OpenAI response to DTO | xUnit, mock `IOpenAiService` |
| Unit | `OpenAiService.ExtractPropertyFieldsAsync` builds correct prompt and parses response | xUnit, mock `HttpClient` |
| Integration | Full endpoint pipeline with mocked OpenAI | `WebApplicationFactory`, fake OpenAI HTTP handler |
| Manual | Paste real listing text and verify extracted fields | Dev environment |

### Security & Privacy

- Input text may contain personal data (names, phones); do NOT log the text body
- Tenant isolation: extraction requests are scoped to authenticated tenant
- Rate limiting: consider throttling to prevent OpenAI cost abuse (defer to infra layer)
- OpenAI API key stored in environment variables, never in source

### TDD Reminder

Write failing tests FIRST for the validator and handler, then implement to make them pass. Commit test files before implementation files when practical.

---

## Task 3.2 -- AI Field Extraction from Photos

**Status:** PENDING
**Dependencies:** 3.1 (reuses `ExtractedPropertyDto`, `ExtractedFieldDto`, prompt infrastructure)

### Scope

**In scope:**
- `POST /api/properties/extract-from-photos` endpoint in `ai-api`
- Accept up to 10 images (multipart/form-data or base64 JSON array)
- Use `gpt-5-mini` vision capability to analyze property photos
- Extract: property type, room count estimates, notable features (pool, garden, terrace, garage), condition, approximate size
- Return structured JSON using the same `ExtractedPropertyDto` schema with confidence scores
- Image size validation (max 20 MB per image)

**Out of scope:**
- Text-based extraction (task 3.1)
- Photo storage/management (separate future epic)
- Floor plan analysis

### Acceptance Criteria

- [ ] `POST /api/properties/extract-from-photos` accepts images and returns structured property JSON
- [ ] Supports both `multipart/form-data` (file upload) and JSON body with `base64Images` array
- [ ] Maximum 10 images per request; exceeding returns 400
- [ ] Each image must be <= 20 MB; exceeding returns 400 with field-level error
- [ ] Only JPEG, PNG, and WebP formats accepted
- [ ] Extracted fields include confidence scores reflecting certainty from visual analysis
- [ ] Multiple photos are analyzed collectively (not independently) to improve accuracy
- [ ] Endpoint requires authentication and tenant context
- [ ] OpenAI vision API errors return 502 with safe error message
- [ ] Unit tests achieve >= 90% coverage for the command handler
- [ ] Integration test confirms pipeline with mocked OpenAI vision response

### Implementation Steps

1. Create `ExtractFromPhotosCommand` with `List<PropertyImage>` property (byte array + content type)
2. Create `PropertyImage` value object in Application layer
3. Create `ExtractFromPhotosCommandValidator` (count, size, format checks)
4. Create `ExtractFromPhotosCommandHandler` calling `IOpenAiService.ExtractPropertyFieldsFromImagesAsync`
5. Extend `IOpenAiService` with `ExtractPropertyFieldsFromImagesAsync(IReadOnlyList<PropertyImage> images, CancellationToken ct)`
6. Implement vision API call in `OpenAiService` with multi-image content parts
7. Create the system prompt for visual property analysis
8. Add endpoint to `PropertyExtractionController` (or new controller if cleaner)
9. Handle multipart form data binding in the controller
10. Write unit tests for handler and validator
11. Write integration test with mocked vision response

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Properties/Models/PropertyImage.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromPhotos/ExtractFromPhotosCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromPhotos/ExtractFromPhotosCommandHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/ExtractFromPhotos/ExtractFromPhotosCommandValidator.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/PropertyPhotoExtractionPrompt.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/ExtractFromPhotosCommandHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/ExtractFromPhotosCommandValidatorTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IOpenAiService.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/OpenAiService.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/PropertyExtractionController.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Validator rejects >10 images, >20 MB, invalid formats | xUnit + FluentAssertions |
| Unit | Handler maps multi-image OpenAI response to DTO | xUnit, mock `IOpenAiService` |
| Unit | OpenAI service correctly builds vision API request with image parts | xUnit, mock `HttpClient` |
| Integration | Full endpoint with multipart upload and mocked vision API | `WebApplicationFactory` |
| Manual | Upload real property photos and verify extracted data | Dev environment |

### Security & Privacy

- Images may contain metadata (EXIF) with GPS coordinates; strip EXIF before sending to OpenAI
- Do NOT log image content or base64 data
- Enforce file size limits at both controller and validator levels to prevent DoS
- Tenant isolation enforced through authentication

### TDD Reminder

Write validator tests first (file size, count, format). Then handler tests with mocked service. Then implement.

---

## Task 3.3 -- AI Copy Generation

**Status:** PENDING
**Dependencies:** 3.1 (reuses property schema, OpenAI service infrastructure)

### Scope

**In scope:**
- `POST /api/properties/generate-copy` endpoint in `ai-api`
- Accept property data (structured JSON) and generate marketing descriptions
- Support multiple output languages: `es`, `en`, `fr`, `de`, `nl`
- Support tone options: `professional`, `luxury`, `casual`, `concise`
- Return generated copy with word count and language metadata
- Use `gpt-5-mini` for generation

**Out of scope:**
- SEO metadata generation (future enhancement)
- Auto-translation of existing descriptions (separate feature)
- Image caption generation

### Acceptance Criteria

- [ ] `POST /api/properties/generate-copy` accepts property data and generation options
- [ ] Request body: `{ "property": {...}, "languages": ["es", "en"], "tone": "professional", "maxWords": 300 }`
- [ ] Response includes generated copy for each requested language
- [ ] Response format: `{ "copies": [{ "language": "es", "title": "...", "description": "...", "highlights": [...], "wordCount": N }] }`
- [ ] At least languages `es` and `en` are supported; unsupported languages return 400
- [ ] Tone parameter is optional, defaults to `professional`
- [ ] `maxWords` is optional, defaults to 250, max 500
- [ ] Property data must include at minimum `propertyType`; otherwise 400
- [ ] Generated copy does not include fabricated details not present in input data
- [ ] Endpoint requires authentication and tenant context
- [ ] Unit tests >= 90% coverage for handler
- [ ] Integration test with mocked OpenAI response

### Implementation Steps

1. Define `GenerateCopyRequest` record: property data, languages, tone, maxWords
2. Define `GeneratedCopyDto` record: language, title, description, highlights, wordCount
3. Define `GenerateCopyResponse` record: list of `GeneratedCopyDto`
4. Create `GenerateCopyCommand` (MediatR request)
5. Create `GenerateCopyCommandValidator` (valid languages, valid tone, maxWords range, property has type)
6. Create `GenerateCopyCommandHandler` calling `IOpenAiService.GeneratePropertyCopyAsync`
7. Extend `IOpenAiService` with `GeneratePropertyCopyAsync` method
8. Implement in `OpenAiService` with language-specific system prompts
9. Create copy generation prompt templates per language
10. Add endpoint to `PropertyExtractionController` or new `PropertyCopyController`
11. Write unit and integration tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Properties/Dtos/GenerateCopyRequest.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Dtos/GeneratedCopyDto.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/GenerateCopy/GenerateCopyCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/GenerateCopy/GenerateCopyCommandHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Commands/GenerateCopy/GenerateCopyCommandValidator.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/CopyGenerationPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/PropertyCopyController.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/GenerateCopyCommandHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/Commands/GenerateCopyCommandValidatorTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IOpenAiService.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/OpenAiService.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Validator rejects missing property type, invalid language, invalid tone, out-of-range maxWords | xUnit + FluentAssertions |
| Unit | Handler correctly delegates to OpenAI service per language | xUnit, mock `IOpenAiService` |
| Unit | OpenAI service builds correct prompt with property context and tone | xUnit, mock `HttpClient` |
| Integration | Full endpoint with mocked OpenAI returning multi-language copy | `WebApplicationFactory` |
| Manual | Generate copy for a real property and review quality | Dev environment |

### Security & Privacy

- Generated copy is derived from user-provided property data; no PII concerns beyond standard tenant scoping
- Do NOT log full property data in production (may contain owner information)
- Rate limiting recommended to prevent cost abuse

### TDD Reminder

Write validator tests for all edge cases first. Then handler tests with mocked service returning expected DTOs. Implement handler last.

---

## Task 3.4 -- AI Prompt Engineering & Tuning

**Status:** PENDING
**Dependencies:** 3.1, 3.2, 3.3 (all extraction/generation prompts must exist first)

### Scope

**In scope:**
- Refine extraction prompts for Spanish real estate terminology (piso, adosado, chalet, atico, bajo, duplex, finca, cortijo, etc.)
- Ensure correct mapping of Spanish property types to enum values
- Add few-shot examples to prompts for common Spanish listing formats
- Create prompt regression test suite: set of input/expected-output pairs that verify prompt changes do not degrade quality
- Document prompt templates and tuning decisions
- Support for bilingual listings (mixed Spanish/English text)

**Out of scope:**
- Changing the AI model (stays on `gpt-5-mini`)
- Building a prompt management UI
- A/B testing infrastructure

### Acceptance Criteria

- [ ] Extraction prompts correctly map at least 15 Spanish property type terms to standard enum values
- [ ] Prompt regression suite includes >= 20 test cases covering: Spanish listings, English listings, bilingual listings, partial data
- [ ] Regression tests run as part of `dotnet test` and fail if accuracy drops below 85% on the test set
- [ ] Copy generation prompts produce natural Spanish real estate marketing language (no literal translations)
- [ ] Prompts handle Spanish-specific data: catastro reference, IBI tax, community fees, energy certificate rating
- [ ] All prompt templates are stored as embedded resources or constant classes (not hardcoded in handlers)
- [ ] Each prompt change includes a before/after comparison in the regression test results
- [ ] Documentation of prompt engineering decisions in a walkthrough

### Implementation Steps

1. Audit current extraction and generation prompts for Spanish real estate gaps
2. Research common Spanish listing formats from major portals (Idealista, Fotocasa, Habitaclia)
3. Create a `PromptTestCase` model: input text/images, expected output, acceptable field tolerance
4. Build 20+ test cases covering diverse Spanish listing formats
5. Create `PromptRegressionTests` test class that runs each case through the handler with a mocked but deterministic OpenAI response
6. Refine `PropertyExtractionPrompt` with Spanish terminology mapping and few-shot examples
7. Refine `PropertyPhotoExtractionPrompt` with Spanish architectural style references
8. Refine `CopyGenerationPrompt` with Spanish real estate marketing conventions
9. Add Spanish-specific property fields to extraction: `catastroReference`, `ibiTax`, `communityFees`, `energyCertificate`
10. Run regression suite, iterate until accuracy >= 85%
11. Create walkthrough documenting prompt decisions

### Files to Create/Modify

**Create:**
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/PromptRegression/PromptTestCase.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/PromptRegression/PromptRegressionTests.cs`
- `services/ai-api/tests/Propely.AiApi.Application.Tests/Properties/PromptRegression/TestData/` (directory with JSON test case files)
- `docs/walkthroughs/epic-P3-ai-smart-fill/3.4-prompt-engineering.md`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/PropertyExtractionPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/PropertyPhotoExtractionPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Ai/Prompts/CopyGenerationPrompt.cs`
- `services/ai-api/src/Propely.AiApi.Application/Properties/Dtos/ExtractedPropertyDto.cs` (add Spanish-specific fields)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Regression | 20+ prompt test cases with expected extraction accuracy | xUnit, deterministic mocked responses matching known prompt outputs |
| Unit | Spanish property type term mapping completeness | xUnit, parameterized tests for each term |
| Unit | Prompt templates contain required sections (system instruction, few-shot examples, schema) | String assertions on prompt constants |
| Manual | Run against live OpenAI with real Spanish listings and measure accuracy | Dev environment, manual review |

### Security & Privacy

- Test data must not contain real personal information; use synthetic listings
- Prompt templates are code assets, not user data; no special privacy concerns

### TDD Reminder

Create the regression test framework and all 20 test cases BEFORE modifying any prompts. Each prompt change should be validated by running the full suite.

---

## Task 3.5 -- AI-API NuGet SDK Client Enhancement

**Status:** PENDING
**Dependencies:** 3.1, 3.2, 3.3 (API endpoints must be defined)

### Scope

**In scope:**
- Extend `Propely.AiApi.Client` NuGet package with new Refit interfaces for extraction and copy generation
- `IPropertyExtractionApi` interface with `ExtractFromTextAsync` and `ExtractFromPhotosAsync`
- `IPropertyCopyApi` interface with `GenerateCopyAsync`
- Shared DTO models matching API response schemas
- Retry policies (Polly) for transient failures
- XML documentation on all public types

**Out of scope:**
- Publishing the NuGet package (CI/CD task)
- Authentication token management (handled by existing `DelegatingHandler`)
- Client-side caching

### Acceptance Criteria

- [ ] `IPropertyExtractionApi` has `ExtractFromTextAsync(ExtractFromTextRequest)` returning `ExtractedPropertyResponse`
- [ ] `IPropertyExtractionApi` has `ExtractFromPhotosAsync(StreamPart[])` returning `ExtractedPropertyResponse`
- [ ] `IPropertyCopyApi` has `GenerateCopyAsync(GenerateCopyRequest)` returning `GenerateCopyResponse`
- [ ] All DTOs have XML doc comments
- [ ] Refit interfaces use correct HTTP methods and routes matching the API
- [ ] `AddAiApiClient(this IServiceCollection, Uri baseUrl)` extension method registers all new interfaces
- [ ] Retry policy: 3 retries with exponential backoff for 5xx and 408 responses
- [ ] Timeout policy: 60 seconds (AI calls can be slow)
- [ ] Package builds without warnings
- [ ] Unit tests verify Refit interface registration and Polly policy configuration

### Implementation Steps

1. Add `ExtractFromTextRequest`, `ExtractFromPhotosRequest`, `ExtractedPropertyResponse` DTOs to client project
2. Add `GenerateCopyRequest`, `GenerateCopyResponse`, `GeneratedCopyItem` DTOs to client project
3. Create `IPropertyExtractionApi` Refit interface with `[Post("/api/properties/extract-from-text")]` and `[Post("/api/properties/extract-from-photos")]`
4. Create `IPropertyCopyApi` Refit interface with `[Post("/api/properties/generate-copy")]`
5. Update `ServiceCollectionExtensions.AddAiApiClient` to register new interfaces with `AddRefitClient`
6. Configure Polly retry and timeout policies for the new clients
7. Add XML documentation to all public types and members
8. Write unit tests for DI registration
9. Write unit tests verifying Polly policies trigger on expected status codes

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Client/Properties/IPropertyExtractionApi.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/IPropertyCopyApi.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/ExtractFromTextRequest.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/ExtractedPropertyResponse.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/ExtractedFieldResponse.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/GenerateCopyRequest.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/GenerateCopyResponse.cs`
- `services/ai-api/src/Propely.AiApi.Client/Properties/Models/GeneratedCopyItem.cs`
- `services/ai-api/tests/Propely.AiApi.Client.Tests/Properties/PropertyExtractionApiTests.cs`
- `services/ai-api/tests/Propely.AiApi.Client.Tests/Properties/PropertyCopyApiTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Client/ServiceCollectionExtensions.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Refit interfaces are registered in DI container | xUnit, resolve from `ServiceProvider` |
| Unit | Polly retry policy retries on 500, 502, 503, 408 | xUnit, mock `HttpMessageHandler` returning error codes |
| Unit | Polly timeout policy triggers at 60s | xUnit, delayed mock handler |
| Unit | DTO serialization/deserialization round-trip | xUnit, `System.Text.Json` |
| Integration | Client calls to running ai-api instance | Manual, dev environment |

### Security & Privacy

- Client does not handle authentication tokens directly; relies on existing `DelegatingHandler`
- No secrets stored in client package
- Base URL configured at registration time, not hardcoded

### TDD Reminder

Write DI registration tests and Polly policy tests first. Then implement the interfaces and extension methods.

---

## Task 3.6 -- Frontend Smart-Fill UX

**Status:** PENDING
**Dependencies:** 3.5 (SDK client must be available for API calls)

### Scope

**In scope:**
- "Paste text" flow: textarea input, call extract-from-text API, show preview of extracted fields, user accepts/modifies, then saves
- "Upload photos" flow: dropzone for images, call extract-from-photos API, show extracted data, merge with existing fields, review
- Confidence indicators: color-coded badges per field (green >= 0.8, yellow >= 0.5, red < 0.5)
- Diff view: when merging extracted data with existing fields, show what will change
- Copy generation: button to generate marketing copy, language selector, tone picker, preview and edit before saving
- Stitch design for all new screens/components
- Responsive design (mobile-friendly)

**Out of scope:**
- Drag-and-drop photo reordering
- Batch processing of multiple properties
- Real-time streaming of AI responses (use loading state instead)

### Acceptance Criteria

- [ ] "Smart Fill from Text" button opens a modal with a textarea
- [ ] Pasting text and clicking "Extract" calls the API and shows a preview panel
- [ ] Preview panel displays each extracted field with its confidence badge
- [ ] User can accept individual fields, modify values, or dismiss
- [ ] "Accept All" and "Accept High Confidence" (>= 0.8) bulk actions available
- [ ] "Smart Fill from Photos" button opens an image upload dropzone
- [ ] Supports drag-and-drop and file picker; max 10 images; shows thumbnails
- [ ] After extraction, shows diff view comparing extracted vs. current values
- [ ] "Generate Copy" button opens a panel with language checkboxes and tone dropdown
- [ ] Generated copy appears in an editable textarea per language
- [ ] User can regenerate, edit, or save generated copy
- [ ] All states have loading indicators (skeleton/spinner during API calls)
- [ ] Error states display user-friendly messages
- [ ] Stitch design exists for: Smart Fill modal, confidence preview, diff view, copy generation panel
- [ ] All components have unit tests (Vitest + React Testing Library)
- [ ] Responsive layout works on screens >= 375px wide

### Implementation Steps

1. Create Stitch designs for Smart Fill modal, confidence preview, diff view, copy generation panel
2. Download Stitch HTML to `.stitch-html/` for reference
3. Create `useExtractFromText` hook calling AI API endpoint
4. Create `useExtractFromPhotos` hook calling AI API endpoint
5. Create `useGenerateCopy` hook calling AI API endpoint
6. Create `ConfidenceBadge` component (green/yellow/red based on score)
7. Create `ExtractedFieldPreview` component showing field name, value, confidence, accept/reject controls
8. Create `SmartFillTextModal` component with textarea and extraction preview
9. Create `SmartFillPhotosModal` component with dropzone and extraction preview
10. Create `FieldDiffView` component comparing extracted vs. current values
11. Create `CopyGenerationPanel` component with language/tone selectors and editable output
12. Integrate Smart Fill buttons into the property form page
13. Write component tests with mocked API responses
14. Verify against Stitch designs pixel-for-pixel

### Files to Create/Modify

**Create:**
- `apps/web/src/components/smart-fill/SmartFillTextModal.tsx`
- `apps/web/src/components/smart-fill/SmartFillPhotosModal.tsx`
- `apps/web/src/components/smart-fill/ConfidenceBadge.tsx`
- `apps/web/src/components/smart-fill/ExtractedFieldPreview.tsx`
- `apps/web/src/components/smart-fill/FieldDiffView.tsx`
- `apps/web/src/components/smart-fill/CopyGenerationPanel.tsx`
- `apps/web/src/hooks/useExtractFromText.ts`
- `apps/web/src/hooks/useExtractFromPhotos.ts`
- `apps/web/src/hooks/useGenerateCopy.ts`
- `apps/web/src/components/smart-fill/__tests__/SmartFillTextModal.test.tsx`
- `apps/web/src/components/smart-fill/__tests__/SmartFillPhotosModal.test.tsx`
- `apps/web/src/components/smart-fill/__tests__/ConfidenceBadge.test.tsx`
- `apps/web/src/components/smart-fill/__tests__/CopyGenerationPanel.test.tsx`
- `.stitch-html/smart-fill-text-modal.html`
- `.stitch-html/smart-fill-photos-modal.html`
- `.stitch-html/copy-generation-panel.html`

**Modify:**
- `apps/web/src/app/[locale]/(dashboard)/properties/[id]/edit/page.tsx` (add Smart Fill buttons)
- `apps/web/src/messages/en.json` (add Smart Fill translation keys)
- `apps/web/src/messages/es.json` (add Smart Fill translation keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `ConfidenceBadge` renders correct color for each threshold | Vitest + RTL |
| Unit | `SmartFillTextModal` calls API on submit, displays preview | Vitest + RTL, mock fetch |
| Unit | `SmartFillPhotosModal` validates file count/size/type before upload | Vitest + RTL |
| Unit | `FieldDiffView` highlights changed fields correctly | Vitest + RTL |
| Unit | `CopyGenerationPanel` sends correct language/tone params | Vitest + RTL, mock fetch |
| Unit | Hooks handle loading, success, and error states | Vitest, mock fetch |
| Manual | Full flow: paste text, review, accept, save | Dev environment |
| Manual | Full flow: upload photos, review, merge, save | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- File uploads are processed client-side only for preview; actual extraction happens server-side
- No property data is stored in browser localStorage or sessionStorage
- Image thumbnails are created using `URL.createObjectURL` and revoked after use

### TDD Reminder

Write component tests with mock data first. Define expected renders for each confidence level, diff scenario, and error state. Then implement components to pass the tests.
