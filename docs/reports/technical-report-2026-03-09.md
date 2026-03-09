# Propely -- Technical Report

**Date:** 2026-03-09
**Version:** 1.1.0
**Classification:** Internal / Technical

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Architecture Overview](#2-system-architecture-overview)
3. [AI Action Engine (Deep Dive)](#3-ai-action-engine-deep-dive)
   - 3.1 [Architecture & Pipeline Flow](#31-architecture--pipeline-flow)
   - 3.2 [Domain Model](#32-domain-model)
   - 3.3 [Intent Classification System](#33-intent-classification-system)
   - 3.4 [Tool Schema Registry](#34-tool-schema-registry)
   - 3.5 [Provider Abstraction Layer](#35-provider-abstraction-layer)
   - 3.6 [Action Routing & Parameter Binding](#36-action-routing--parameter-binding)
   - 3.7 [Action Handlers (Complete Catalogue)](#37-action-handlers-complete-catalogue)
   - 3.8 [Voice Pipeline (Speech-to-Action)](#38-voice-pipeline-speech-to-action)
   - 3.9 [Multi-turn Conversation Context](#39-multi-turn-conversation-context)
   - 3.10 [Model Context Protocol (MCP) Server](#310-model-context-protocol-mcp-server)
   - 3.11 [Proactive Suggestion Engine](#311-proactive-suggestion-engine)
   - 3.12 [Content Generation & Vision Services](#312-content-generation--vision-services)
   - 3.13 [Work Items Module](#313-work-items-module)
   - 3.14 [Security & Authorization](#314-security--authorization)
   - 3.15 [Observability & Telemetry](#315-observability--telemetry)
   - 3.16 [Configuration & Dependency Injection](#316-configuration--dependency-injection)
4. [Backend Services](#4-backend-services)
   - 4.1 [Orgs API](#41-orgs-api)
   - 4.2 [Properties API](#42-properties-api)
   - 4.3 [Contacts API](#43-contacts-api)
   - 4.4 [Appointments API](#44-appointments-api)
   - 4.5 [Publishing API](#45-publishing-api)
5. [Frontend (Next.js Web Application)](#5-frontend-nextjs-web-application)
6. [Inter-Service Communication](#6-inter-service-communication)
7. [Infrastructure & DevOps](#7-infrastructure--devops)
8. [Testing Strategy](#8-testing-strategy)
9. [Appendix: File Index](#9-appendix-file-index)

---

## 1. Executive Summary

Propely is a multi-tenant, AI-powered real estate management SaaS platform. Its core differentiator is **natural language process automation**: real estate agents interact with the system through text and voice commands to create properties, manage leads, schedule appointments, close operations, and perform any system action -- all via natural language in Spanish or English.

The platform is structured as a monorepo with **7 deployable services** and **shared libraries**, orchestrated by Docker Compose for development and deployed to Google Cloud Run for production:

| Service | Technology | Port | Purpose |
|---------|-----------|------|---------|
| `apps/web` | Next.js 16, React 19, Tailwind v4 | 3000 | SPA frontend |
| `services/ai-api` | .NET 10, OpenAI SDK, MediatR | 5010 | AI orchestration, NL engine, voice, MCP |
| `services/orgs-api` | .NET 10, EF Core, Identity | 5020 | Auth, orgs, billing, permissions |
| `services/properties-api` | .NET 10, EF Core | 5030 | Property domain CRUD |
| `services/contacts-api` | .NET 10, EF Core | 5050 | Contacts and leads |
| `services/appointments-api` | .NET 10, EF Core | 5060 | Scheduling, calendar |
| `services/publishing-api` | .NET 10 (scaffolded) | 5040 | Portal feeds (deprioritized) |

**Key technology choices:**
- .NET 10 with Clean Architecture + CQRS (MediatR) for all backend services
- OpenAI `gpt-4o-mini` for intent classification via function calling
- OpenAI `gpt-4o-mini-transcription` for voice STT
- PostgreSQL 16 (separate database per service)
- RabbitMQ 3 for async messaging with transactional outbox pattern
- Redis 7 for caching and conversation context storage
- Refit NuGet SDK clients for typed inter-service HTTP communication
- Polly resilience policies (retry + circuit breaker) on all SDK clients
- Aspire Dashboard for OpenTelemetry observability

---

## 2. System Architecture Overview

### 2.1 Topology

```
                         ┌─────────────────────┐
                         │    apps/web          │
                         │    (Next.js 16)      │
                         │    Port 3000         │
                         └─────┬───────────────-┘
                               │
              ┌────────────────┼────────────────┬──────────────┐
              │ Cookie auth    │ Bearer+X-Org-Id│              │ Bearer+X-Org-Id
              ▼                ▼                ▼              ▼
        ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
        │ orgs-api │    │  ai-api  │    │properties│    │contacts  │
        │  :5020   │    │  :5010   │    │  -api    │    │  -api    │ ...
        └──────────┘    └────┬─────┘    │  :5030   │    │  :5050   │
                             │          └──────────┘    └──────────┘
                    SDK Clients (Refit)
                     ┌───────┼───────┐
                     ▼       ▼       ▼
               properties  contacts  appointments
               -api client -api client -api client
```

### 2.2 Clean Architecture Pattern (all .NET services)

Every .NET service follows an identical 4-layer architecture:

```
┌──────────────────────────────────────────────────────────┐
│  Api Layer  (Controllers, Middleware, Program.cs, DTOs)   │
├──────────────────────────────────────────────────────────┤
│  Application Layer  (Commands, Queries, Handlers, IFaces)│
├──────────────────────────────────────────────────────────┤
│  Infrastructure  (EF Core, RabbitMQ, Redis, OpenAI SDK)  │
├──────────────────────────────────────────────────────────┤
│  Domain  (Entities, Value Objects, Events, Enums)         │
└──────────────────────────────────────────────────────────┘
        Dependencies flow INWARD only (Domain has zero deps)
```

**Namespace convention:** `Propely.<Service>.<Layer>` (e.g., `Propely.AiApi.Application`).

### 2.3 Shared Infrastructure

| Component | Version | Purpose |
|-----------|---------|---------|
| PostgreSQL | 16 | Persistent storage (1 DB per service) |
| RabbitMQ | 3 | Async event bus (transactional outbox) |
| Redis | 7 | Distributed cache, conversation sessions |
| Aspire Dashboard | 9.0 | OpenTelemetry traces + logs + metrics |
| MailHog | latest | Dev email trap |

---

## 3. AI Action Engine (Deep Dive)

The AI Action Engine is the platform's core differentiator. It lives entirely within `services/ai-api/` and transforms natural language input (text or voice) into concrete system actions across all other services. This section provides exhaustive detail on every component.

### 3.1 Architecture & Pipeline Flow

The engine operates as a 5-stage pipeline:

```
                     ┌──────────────────────────────────────┐
                     │         USER INPUT                    │
                     │  Text: POST /v1/actions/execute       │
                     │  Voice: POST /v1/voice/execute        │
                     └──────────┬───────────────────────────┘
                                │
                     ┌──────────▼───────────────────────────┐
                     │  STAGE 1: VOICE STT (if audio)       │
                     │  OpenAiVoiceTranscriptionService      │
                     │  Model: gpt-4o-mini-transcription     │
                     │  Formats: WebM, WAV, MP3, M4A, etc   │
                     │  Max: 25 MB, dedup: SHA256 + 30s     │
                     └──────────┬───────────────────────────┘
                                │ transcribed text
                     ┌──────────▼───────────────────────────┐
                     │  STAGE 2: INTENT CLASSIFICATION       │
                     │  OpenAiIntentClassifier                │
                     │  Model: gpt-4o-mini (function calling) │
                     │  20 tool definitions (ToolSchemaReg)  │
                     │  Multi-turn history via Redis sessions │
                     │  Spanish ↔ English bilingual support  │
                     │  Output: ClassifiedIntent{ActionType, │
                     │          Parameters, Confidence}       │
                     └──────────┬───────────────────────────┘
                                │ ClassifiedIntent
                     ┌──────────▼───────────────────────────┐
                     │  STAGE 3: PARAMETER BINDING            │
                     │  DefaultParameterBinder                │
                     │  Dict<string,object?> → typed records  │
                     │  JSON serialization with snake_case    │
                     │  Handles: JsonElement, Guid, DateTime  │
                     └──────────┬───────────────────────────┘
                                │ Typed parameters
                     ┌──────────▼───────────────────────────┐
                     │  STAGE 4: ACTION ROUTING               │
                     │  ActionRouter                          │
                     │  Maps ActionType → MediatR command     │
                     │  20 action types → 20 handlers         │
                     │  Each handler calls service SDK clients│
                     └──────────┬───────────────────────────┘
                                │ ActionResult
                     ┌──────────▼───────────────────────────┐
                     │  STAGE 5: RESPONSE & CONTEXT UPDATE   │
                     │  Update conversation session (Redis)   │
                     │  Track entity memory (last property,   │
                     │    contact, lead, appointment IDs)     │
                     │  Return structured ActionResult + NL   │
                     │  confirmation message to user          │
                     └──────────────────────────────────────┘
```

**Entry points:**

| Endpoint | Method | Controller | Description |
|----------|--------|------------|-------------|
| `/v1/actions/execute` | POST | `ActionsController` | Text-based action execution |
| `/v1/voice/transcribe` | POST | `VoiceController` | Audio → text only |
| `/v1/voice/execute` | POST | `VoiceController` | Audio → text → action (full pipeline) |
| `/v1/suggestions` | GET | `SuggestionsController` | Proactive AI suggestions |
| `/mcp` | Streamable HTTP | MCP Server | MCP tool interoperability |
| `/v1/work-items/*` | REST | `WorkItemsController` | AI-parsed work items |

### 3.2 Domain Model

The AI engine's domain is clean and focused, located in `Propely.AiApi.Domain.Actions`:

#### `ActionType` enum (21 values)

```csharp
public enum ActionType
{
    CreateProperty,        // Create new property listing
    UpdateProperty,        // Modify existing property
    QueryProperties,       // Search/filter properties
    ChangePropertyStatus,  // Activate, deactivate, mark sold
    GenerateCopy,          // Generate marketing copy (multilingual)
    ExtractFromText,       // Extract property data from unstructured text
    ExtractFromPhotos,     // Extract property data from images (vision)
    CreateLead,            // Create new lead inquiry
    CreateContact,         // Create new contact
    QualifyLead,           // Mark lead as qualified
    ConvertLead,           // Convert lead → contact
    QueryLeads,            // Search/filter leads
    BookViewing,           // Schedule property viewing
    QueryAppointments,     // Search appointments
    CancelAppointment,     // Cancel appointment
    RescheduleAppointment, // Reschedule appointment
    ReserveProperty,       // Reserve for buyer/renter
    CloseOperation,        // Close sale/rental
    ArchiveProperty,       // Archive listing
    ReactivateProperty,    // Reactivate archived listing
    Unknown                // Unrecognized intent
}
```

#### `ClassifiedIntent` (value object)

```csharp
public sealed record ClassifiedIntent(
    ActionType ActionType,
    Dictionary<string, object?> Parameters,
    double Confidence,
    string? RawFunctionName = null);
```

- `ActionType`: The resolved action from classification
- `Parameters`: Raw parameter dictionary with snake_case keys (e.g., `"property_type" → "apartment"`)
- `Confidence`: 0.0–1.0 (OpenAI function calling returns 1.0 when a tool is selected)
- `RawFunctionName`: The original tool name for debugging (e.g., `"create_property"`)

#### `ActionResult` (generic and non-generic)

```csharp
public sealed record ActionResult
{
    bool Success;
    object? Data;           // Entity data (e.g., created property)
    string Message;          // NL confirmation message for user
    string[] Errors;         // Error messages (if failed)
    ActionType ActionType;   // What action was attempted
    double Confidence;       // Classification confidence
}
```

Factory methods:
- `ActionResult.Ok(data, message, type, confidence)` — success path
- `ActionResult.Fail(errors, type, message)` — failure path

### 3.3 Intent Classification System

**File:** `Propely.AiApi.Infrastructure.AI.OpenAiIntentClassifier`

The intent classifier is the brain of the engine. It uses **OpenAI function calling** (not prompt-based classification) to achieve deterministic, structured intent recognition.

#### How it works

1. A system prompt is constructed with:
   - Role definition ("AI assistant for Propely, a Spanish real estate management platform")
   - Complete **Spanish real estate vocabulary mapping** (property types, operation types, features, area terms, financial terms, energy certificates)
   - Extraction rules (price shorthand like "250k" → 250000, "1M" → 1000000)
   - Bilingual examples (Spanish/English)

2. All 20 tool schemas are converted to OpenAI `ChatTool` format via `OpenAiToolAdapter`

3. Conversation history (if `sessionId` is provided) is prepended as user/assistant message pairs

4. The OpenAI Chat API is called with `tools` parameter — the model either:
   - **Selects a tool** → function name + JSON arguments are extracted → `ClassifiedIntent`
   - **Returns text** → no tool was matched → `ActionType.Unknown`

#### System Prompt (complete content)

The system prompt covers:

**Spanish property type mappings:**
| Spanish | English enum |
|---------|-------------|
| piso/apartamento | apartment |
| ático/atico | penthouse |
| dúplex/duplex | duplex |
| estudio/loft | studio |
| adosado/pareado/chalet/casa | house |
| villa/finca/cortijo/masía | villa |
| local/oficina/nave | commercial |
| solar/terreno/parcela | land |
| garaje | garage |
| trastero | storage |

**Spanish operation type mappings:**
| Spanish | English enum |
|---------|-------------|
| venta/vender/compra/comprar | sale |
| alquiler/alquilar/arrendar | rent |
| traspaso/traspasar | transfer |

**Feature vocabulary:** piscina=pool, jardín=garden, terraza=terrace, ascensor=elevator, etc.

#### Confidence thresholds

```csharp
private const double MinimumConfidenceThreshold = 0.5;
```

The `ExecuteActionCommandHandler` applies this threshold:
- `confidence < 0.5` → returns ambiguity error
- `ActionType == Unknown` → returns clarification prompt
- Otherwise → proceeds to routing

#### Function argument parsing

```csharp
private Dictionary<string, object?> ParseFunctionArguments(BinaryData functionArguments)
```

Handles all JSON value kinds: String, Number (int64/double), Boolean, Null. Returns raw dictionary for downstream binding.

### 3.4 Tool Schema Registry

**File:** `Propely.AiApi.Application.Actions.Tools.ToolSchemaRegistry`

The registry defines all 20 provider-neutral tool schemas. Each schema is a `ToolSchema` record:

```csharp
public sealed record ToolSchema(
    string Name,                   // e.g., "create_property"
    string Description,            // Human-readable description
    string ParametersJsonSchema,   // JSON Schema string
    ActionType ActionType);        // Mapped ActionType enum
```

**Complete catalogue of 20 tool schemas:**

| # | Tool Name | ActionType | Required Params | Optional Params |
|---|-----------|-----------|-----------------|-----------------|
| 1 | `create_property` | CreateProperty | *(none)* | title, property_type, operation_type, bedrooms, bathrooms, price, city, description |
| 2 | `update_property` | UpdateProperty | *(none)* | property_id, title, price, description, field, value |
| 3 | `query_properties` | QueryProperties | *(none)* | property_type, operation_type, city, min_price, max_price, min_bedrooms, status |
| 4 | `change_property_status` | ChangePropertyStatus | *(none)* | property_id, status, reference |
| 5 | `generate_copy` | GenerateCopy | property_data | tone, languages |
| 6 | `extract_from_text` | ExtractFromText | text | *(none)* |
| 7 | `extract_from_photos` | ExtractFromPhotos | image_urls | *(none)* |
| 8 | `create_lead` | CreateLead | name, email, property_id | phone, message, source |
| 9 | `create_contact` | CreateContact | first_name, last_name, email | phone, role, company, notes, source |
| 10 | `qualify_lead` | QualifyLead | lead_id | *(none)* |
| 11 | `convert_lead` | ConvertLead | lead_id | role, notes |
| 12 | `query_leads` | QueryLeads | *(none)* | status, property_id, search |
| 13 | `book_viewing` | BookViewing | property_id, start_time | contact_id, end_time, title, location, notes |
| 14 | `query_appointments` | QueryAppointments | *(none)* | status, type, property_id, from_date, to_date |
| 15 | `cancel_appointment` | CancelAppointment | appointment_id | reason |
| 16 | `reschedule_appointment` | RescheduleAppointment | appointment_id, new_start_time | new_end_time |
| 17 | `reserve_property` | ReserveProperty | *(none)* | property_id, contact_name, reference |
| 18 | `close_operation` | CloseOperation | *(none)* | property_id, reference |
| 19 | `archive_property` | ArchiveProperty | *(none)* | property_id, reference |
| 20 | `reactivate_property` | ReactivateProperty | *(none)* | property_id, reference |

**Schema categories:**
- **Property CRUD** (4): create, update, query, change status
- **Content/AI** (3): generate copy, extract from text, extract from photos
- **Contact/Lead** (5): create lead, create contact, qualify lead, convert lead, query leads
- **Appointments** (4): book viewing, query, cancel, reschedule
- **Operations** (4): reserve, close, archive, reactivate

### 3.5 Provider Abstraction Layer

The engine is designed to be **provider-neutral**, enabling future support for Claude, Gemini, or other LLMs without modifying application logic.

#### Interface: `IToolAdapter`

```csharp
public interface IToolAdapter
{
    IReadOnlyList<object> ConvertAll(IReadOnlyList<ToolSchema> schemas);
}
```

Each LLM provider implements this interface:
- **`OpenAiToolAdapter`** (Infrastructure) — converts `ToolSchema` → `ChatTool` (OpenAI SDK type)
- Future: `ClaudeToolAdapter`, `GeminiToolAdapter`

#### Interface: `IIntentClassifier`

```csharp
public interface IIntentClassifier
{
    Task<ClassifiedIntent> ClassifyAsync(
        string text,
        IReadOnlyList<ConversationExchange>? history = null,
        CancellationToken ct = default);
}
```

Current implementation: `OpenAiIntentClassifier` (uses `gpt-4o-mini` with function calling).

This abstraction means switching from OpenAI to Claude or Gemini requires only a new `IIntentClassifier` implementation -- no changes to the application or domain layers.

### 3.6 Action Routing & Parameter Binding

#### ActionRouter (`Infrastructure.AI.ActionRouter`)

The router is a pure switch statement mapping `ActionType` → MediatR command:

```
ClassifiedIntent → Bind parameters → Create typed command → Send via MediatR → ActionResult
```

Pattern for each action:
```csharp
ActionType.CreateProperty => await _mediator.Send(
    new CreatePropertyActionCommand(
        _binder.Bind<CreatePropertyParameters>(intent.Parameters),
        tenantId, agentId), ct)
```

All 20 action types are routed. Unknown types return a structured error.

#### DefaultParameterBinder (`Infrastructure.AI.DefaultParameterBinder`)

Converts `Dictionary<string, object?>` (snake_case keys from OpenAI) to strongly-typed records.

**Process:**
1. Normalize values: `JsonElement` → native types, `Guid` → string, `DateTime` → ISO 8601
2. Serialize to JSON with `JsonNamingPolicy.SnakeCaseLower`
3. Deserialize to target record type (e.g., `CreatePropertyParameters`)

**Handling edge cases:**
- `JsonElement` arrays → `List<string>`
- `JsonElement` objects → `Dictionary<string, object?>`
- `long` / `double` / `decimal` — preserved without loss
- `NumberHandling.AllowReadingFromString` — handles string numbers from LLM

#### Parameter Records (20 types)

Each action has a typed parameter record implementing `IActionParameters` (marker interface):

```csharp
public sealed record CreatePropertyParameters(
    string? Title,
    string? PropertyType,
    string? OperationType,
    int? Bedrooms,
    int? Bathrooms,
    decimal? Price,
    string? City,
    string? Description) : IActionParameters;
```

All parameters are nullable — the LLM may not extract all fields, and handlers validate required fields.

### 3.7 Action Handlers (Complete Catalogue)

Each handler is a MediatR `IRequestHandler<TCommand, ActionResult>` that calls downstream service SDK clients.

#### Property Handlers

| Handler | Command | SDK Client | Operation |
|---------|---------|-----------|-----------|
| `CreatePropertyActionHandler` | `CreatePropertyActionCommand` | `IPropertiesApiClient.CreateAsync()` | Creates property via Properties API |
| `UpdatePropertyActionHandler` | `UpdatePropertyActionCommand` | `IPropertiesApiClient.UpdateAsync()` | Updates property fields |
| `QueryPropertiesActionHandler` | `QueryPropertiesActionCommand` | `IPropertiesApiClient.SearchAsync()` | Searches properties with filters |
| `ChangePropertyStatusActionHandler` | `ChangePropertyStatusActionCommand` | `IPropertiesApiClient.UpdateStatusAsync()` | Changes property status |

**Example: CreatePropertyActionHandler flow:**
1. Extract typed parameters (`PropertyType`, `OperationType`, `Bedrooms`, etc.)
2. Validate minimum fields (property type required)
3. Normalize enum values to PascalCase (e.g., `"apartment"` → `"Apartment"`)
4. Auto-generate title if not provided (e.g., `"Apartment in Málaga"`)
5. Build `CreatePropertyRequest` DTO
6. Call `IPropertiesApiClient.CreateAsync()`
7. Build human-readable confirmation message (e.g., *"Created an Apartment for Sale in Málaga with 3 bedrooms, priced at 250,000 EUR."*)
8. Return `ActionResult.Ok()` with property data + confirmation

#### Operation Handlers

| Handler | Command | Description |
|---------|---------|-------------|
| `ReservePropertyActionHandler` | `ReservePropertyActionCommand` | Reserves property, validates status |
| `CloseOperationActionHandler` | `CloseOperationActionCommand` | Marks sale/rental as closed |
| `ArchivePropertyActionHandler` | `ArchivePropertyActionCommand` | Archives listing |
| `ReactivatePropertyActionHandler` | `ReactivatePropertyActionCommand` | Reactivates archived listing |

#### Content/AI Generation Handlers

| Handler | Command | LLM Used | Description |
|---------|---------|----------|-------------|
| `GenerateCopyActionHandler` | `GenerateCopyActionCommand` | `IOpenAiService.GenerateWithJsonResponseAsync()` | Multilingual marketing copy (ES, EN, FR, DE, NL) |
| `ExtractFromTextActionHandler` | `ExtractFromTextActionCommand` | `IOpenAiService.GenerateWithJsonResponseAsync()` | Structured extraction from unstructured text |
| `ExtractFromPhotosActionHandler` | `ExtractFromPhotosActionCommand` | `IOpenAiService.AnalyzeImagesAsync()` | Vision-based property data extraction |

#### Contact & Lead Handlers

| Handler | SDK Client | Operation |
|---------|-----------|-----------|
| `CreateLeadActionHandler` | `IContactsApiClient` | Creates lead |
| `CreateContactActionHandler` | `IContactsApiClient` | Creates contact |
| `QualifyLeadActionHandler` | `IContactsApiClient` | Qualifies lead |
| `ConvertLeadActionHandler` | `IContactsApiClient` | Converts lead to contact |
| `QueryLeadsActionHandler` | `IContactsApiClient` | Searches leads |

#### Appointment Handlers

| Handler | SDK Client | Operation |
|---------|-----------|-----------|
| `BookViewingActionHandler` | `IAppointmentsApiClient` | Schedules viewing appointment |
| `QueryAppointmentsActionHandler` | `IAppointmentsApiClient` | Searches appointments |
| `CancelAppointmentActionHandler` | `IAppointmentsApiClient` | Cancels appointment |
| `RescheduleAppointmentActionHandler` | `IAppointmentsApiClient` | Reschedules appointment |

### 3.8 Voice Pipeline (Speech-to-Action)

**File:** `Propely.AiApi.Api.Controllers.VoiceController`

The voice pipeline extends the action engine with audio input, creating a complete voice-to-action flow.

#### Endpoints

1. **`POST /v1/voice/transcribe`** — Audio → text only (returns `TranscriptionResultDto`)
2. **`POST /v1/voice/execute`** — Audio → text → action execution (returns `VoiceExecuteResultDto`)

#### Audio validation

| Constraint | Value |
|-----------|-------|
| Max file size | 25 MB |
| Supported MIME types | audio/webm, audio/wav, audio/mpeg, audio/mp3, audio/mp4, audio/m4a, audio/ogg, audio/flac |
| Supported extensions | .webm, .wav, .mp3, .m4a, .ogg, .flac |

#### Deduplication mechanism

The controller implements server-side deduplication to prevent client bugs from flooding the STT pipeline:

```csharp
private static readonly ConcurrentDictionary<string, DateTimeOffset> RecentAudioHashes = new();
private static readonly TimeSpan DeduplicationWindow = TimeSpan.FromSeconds(30);
```

1. SHA256 hash of audio file content is computed
2. Key: `"{userId}:{sha256hex}"` → stored with timestamp
3. If same hash seen within 30 seconds → HTTP 429 (Too Many Requests)
4. Eviction: entries older than 60 seconds (2x window) are purged

#### Voice execute flow

```
┌──────────┐    ┌──────────────────────┐    ┌──────────────────────┐
│  Audio   │───▸│ Step 1: Transcribe   │───▸│ Step 2: Execute      │
│  Upload  │    │ gpt-4o-mini-transcrip│    │ Same as text pipeline│
│  (POST)  │    │ Returns: text, lang, │    │ MediatR Send()       │
│          │    │ durationMs           │    │ Returns: ActionResult│
└──────────┘    └──────────────────────┘    └──────────────────────┘
                                                      │
                                            ┌─────────▼─────────┐
                                            │ VoiceExecuteResult │
                                            │ transcribedText    │
                                            │ language           │
                                            │ durationMs         │
                                            │ action (result)    │
                                            └───────────────────┘
```

#### Transcription service (`OpenAiVoiceTranscriptionService`)

**File:** `Propely.AiApi.Infrastructure.Services.OpenAiVoiceTranscriptionService`

```csharp
public class OpenAiVoiceTranscriptionService : IVoiceTranscriptionService
```

- Uses OpenAI Audio API (`AudioClient`)
- Model: configurable via `OpenAiOptions.TranscriptionModelId` (default: `gpt-4o-mini-transcription`)
- Supports language hint (BCP-47, e.g., `"es"`, `"en"`)
- Returns `TranscriptionResult(Text, Language, DurationMs)`
- Custom `AiServiceException` wrapping for non-OpenAI exceptions

### 3.9 Multi-turn Conversation Context

The engine supports **multi-turn conversations** via session-scoped context stored in Redis.

#### Data model

```csharp
// A single exchange: user input → action result
public sealed record ConversationExchange(
    string UserText,
    ActionType ActionType,
    string ResultMessage,
    bool Success,
    DateTimeOffset Timestamp);

// Tracks recently referenced entity IDs for pronoun resolution
public sealed record EntityMemory
{
    Guid? LastPropertyId;
    Guid? LastContactId;
    Guid? LastLeadId;
    Guid? LastAppointmentId;
}

// Full conversation session
public sealed class ConversationSession
{
    List<ConversationExchange> Exchanges;
    EntityMemory EntityMemory;
    DateTimeOffset CreatedAt;
    DateTimeOffset LastActivityAt;
}
```

#### Context storage (`RedisConversationContext`)

```
Redis key: "conversation:{tenantId}:{agentId}:{sessionId}"
TTL: configurable via ConversationContextOptions.SessionTtlMinutes (sliding)
Max exchanges: configurable via ConversationContextOptions.MaxExchanges
```

#### How context flows through intent classification

1. Client sends `sessionId` with each request
2. `ExecuteActionCommandHandler` loads `ConversationSession` from Redis
3. History (`List<ConversationExchange>`) is passed to `IIntentClassifier.ClassifyAsync()`
4. `OpenAiIntentClassifier` converts history to user/assistant message pairs in the ChatCompletion request
5. After execution, the new exchange is appended and session is saved back to Redis
6. `EntityMemory` is updated with any entity IDs returned in the action result

**Entity memory purpose:** Enables pronoun resolution. Example:
- User: "Create a 3 bedroom apartment in Málaga" → creates property, stores `LastPropertyId`
- User: "Set the price to 250k" → classifier sees history + entity memory, maps to `update_property(property_id=LastPropertyId, price=250000)`

### 3.10 Model Context Protocol (MCP) Server

**Files:**
- `Propely.AiApi.Api.MCP.PropelyMcpTools` — tool definitions
- `Propely.AiApi.Infrastructure.MCP.McpToolHandler` — execution bridge

The AI API exposes all 20 action tools as an MCP server, enabling external AI agents (Claude, Cursor, Windsurf, etc.) to interact with Propely programmatically.

#### MCP endpoint

```
POST /mcp (Streamable HTTP transport)
Requires authorization
```

Configured in `Program.cs`:
```csharp
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
app.MapMcp("/mcp").RequireAuthorization();
```

#### Architecture

```
External AI Agent  ───MCP Streamable HTTP───▸  PropelyMcpTools  ───▸  McpToolHandler
                                              (20 static methods)      │
                                                                       │ Resolves tool name → ActionType
                                                                       │ Creates ClassifiedIntent(confidence=1.0)
                                                                       │ Routes via IActionRouter
                                                                       ▼
                                                                   ActionResult
```

**Key design decision:** MCP calls bypass intent classification entirely. Since MCP tools are explicitly called (no NL ambiguity), `McpToolHandler` constructs a `ClassifiedIntent` with `Confidence = 1.0` and routes directly to the `ActionRouter`. This reuses the same handler pipeline as the NL flow.

#### Tool definitions

Each of the 20 tools is defined as a static method with `[McpServerTool]` attribute:

```csharp
[McpServerTool(Name = "create_property")]
[Description("Create a new real estate property listing with the provided details.")]
public static async Task<string> CreateProperty(
    McpToolHandler handler, ClaimsPrincipal user,
    [Description("Property title")] string? title = null,
    [Description("Property type")] string? property_type = null,
    ...)
```

The MCP SDK auto-resolves `McpToolHandler` and `ClaimsPrincipal` from DI; they don't appear in the tool's JSON Schema. All parameters are optional with descriptions for LLM consumption.

### 3.11 Proactive Suggestion Engine

**Files:**
- `Propely.AiApi.Application.Suggestions.Rules.SuggestionEngine`
- `Propely.AiApi.Application.Suggestions.Interfaces.ISuggestionRule`
- Individual rules in `.../Suggestions/Rules/`

The suggestion engine generates proactive, contextual recommendations based on live system data.

#### Architecture

```csharp
public interface ISuggestionRule
{
    SuggestionType Type { get; }
    Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct);
}

public sealed class SuggestionEngine
{
    // Iterates all registered rules, catches errors per-rule, aggregates and sorts by priority
    Task<IReadOnlyList<Suggestion>> GenerateAsync(Guid tenantId, CancellationToken ct);
}
```

#### Implemented rules (5)

| Rule | Type | Description |
|------|------|-------------|
| `DraftPropertyRule` | DraftProperty | Detects properties stuck in draft status |
| `StaleLeadsRule` | StaleLead | Identifies leads without recent activity |
| `EmptyCalendarRule` | EmptyCalendar | No appointments scheduled in upcoming period |
| `GroupedViewingRule` | GroupedViewing | Suggests batching nearby viewings together |
| `LowConversionRule` | LowConversion | Low lead-to-contact conversion rate |

Each rule queries the relevant service SDK client, evaluates conditions, and returns prioritized suggestions.

### 3.12 Content Generation & Vision Services

#### OpenAiService (`Infrastructure.Services.OpenAiService`)

Provides 3 core AI capabilities:

```csharp
public class OpenAiService : IOpenAiService
{
    // General text generation
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct);

    // JSON-structured generation (system + user prompt)
    Task<string> GenerateWithJsonResponseAsync(string systemPrompt, string userPrompt, CancellationToken ct);

    // Vision: analyze images with system prompt
    Task<string> AnalyzeImagesAsync(string systemPrompt, string[] imageUrls, CancellationToken ct);
}
```

- Uses `OpenAI.Chat.ChatClient` from the official OpenAI .NET SDK
- JSON mode: `ChatResponseFormat.CreateJsonObjectFormat()`
- Vision: `ChatMessageContentPart.CreateImagePart(new Uri(url))` with arbitrary number of images
- All methods wrap non-AI exceptions in `AiServiceException`

#### Content actions powered by OpenAiService

| Action | Service Method | Input | Output |
|--------|---------------|-------|--------|
| GenerateCopy | `GenerateWithJsonResponseAsync` | Property description + tone | Multilingual marketing text (ES, EN, FR, DE, NL) |
| ExtractFromText | `GenerateWithJsonResponseAsync` | Unstructured text (emails, notes) | Structured property data (JSON) |
| ExtractFromPhotos | `AnalyzeImagesAsync` | Array of image URLs (max 10) | Structured property data from vision analysis |

### 3.13 Work Items Module

The AI API also manages work items (tasks/tickets) with AI-assisted parsing. This is a secondary module following the same Clean Architecture pattern.

#### Domain

```csharp
public class WorkItem  // Aggregate root
{
    Guid Id, TenantId, UserId;
    string Title, Description;
    WorkItemType Type;     // Bug, Feature, Task, Improvement
    WorkItemPriority Priority;  // Low, Medium, High, Critical
    WorkItemStatus Status;      // Open, InProgress, Done, Cancelled
    WorkItemEffort Effort;      // XSmall, Small, Medium, Large, XLarge
}
```

#### Events: `WorkItemCreatedV1`, `WorkItemUpdatedV1`, `WorkItemDeletedV1` — published via RabbitMQ outbox.

#### AI feature: `ParseWorkItemCommand` — uses OpenAI to extract structured work item data from natural language text.

#### API endpoints

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/v1/work-items` | List (paginated, filtered) |
| GET | `/v1/work-items/{id}` | Get by ID |
| POST | `/v1/work-items` | Create |
| PUT | `/v1/work-items/{id}` | Update |
| DELETE | `/v1/work-items/{id}` | Soft delete |
| POST | `/v1/work-items/parse` | AI-parse from text |

### 3.14 Security & Authorization

#### Authentication

- JWT Bearer tokens validated via `JwtBearerEvents` (tokens issued by orgs-api)
- Development mode: `DevAuthenticationHandler` for testing without real tokens
- Token claims: `sub` (user ID), `org_id` (tenant ID), `role`, `branch_id`

#### Authorization policies

| Policy | Requirement |
|--------|------------|
| `RequireAgent` | Authenticated + `org_id` claim present |
| `RequireViewer` | Authenticated + `org_id` claim (read-level access) |

#### Multi-tenancy

- `OrgContextMiddleware` extracts org ID from `X-Org-Id` header → sets in `ClaimsPrincipal`
- `HttpTenantAccessor` provides `ITenantAccessor` for services that need the current tenant
- All queries are scoped by `TenantId` at the EF Core level

#### Security middleware stack

1. Forwarded Headers (reverse proxy support)
2. Correlation ID (request tracing)
3. Global Exception Handler
4. HSTS + HTTPS Redirection (non-dev)
5. Security Headers (CSP, X-Frame-Options, etc.)
6. IP Rate Limiting (AspNetCoreRateLimit)
7. CORS
8. Authentication → OrgContext → Authorization

### 3.15 Observability & Telemetry

- OpenTelemetry traces exported to Aspire Dashboard (OTLP at port 4317/4318)
- `TelemetryConfiguration` registers custom metrics and traces
- `WorkItemMetrics` — custom counter for work item operations
- `CorrelationIdMiddleware` generates unique `X-Correlation-Id` per request for distributed tracing
- Structured logging throughout (`ILogger<T>`) with semantic log templates

### 3.16 Configuration & Dependency Injection

#### OpenAI configuration

```csharp
public class OpenAiOptions
{
    string ApiKey;              // AIAPI_OpenAi__ApiKey
    string ModelId;             // AIAPI_OpenAi__ModelId (default: gpt-4o-mini)
    string TranscriptionModelId; // AIAPI_OpenAi__TranscriptionModelId (default: gpt-4o-mini-transcription)
}
```

#### Conversation context configuration

```csharp
public record ConversationContextOptions
{
    int MaxExchanges;      // Max history entries per session
    int SessionTtlMinutes; // Redis TTL for session data
}
```

#### Service registrations (high-level)

```
// Application layer
builder.Services.AddApplicationServices()
  → MediatR (handlers, validators, behaviors)
  → FluentValidation (validators)
  → ToolSchemaRegistry (singleton)

// Infrastructure layer
builder.Services.AddInfrastructureServices(config, env)
  → EF Core (AppDbContext → PostgreSQL)
  → Redis (ICacheService)
  → RabbitMQ (IMessagePublisher, OutboxDispatcherService)
  → OpenAI (IOpenAiService, IVoiceTranscriptionService, IIntentClassifier)
  → Action engine (IActionRouter, IParameterBinder, IConversationContext)
  → SDK clients (IPropertiesApiClient, IContactsApiClient, IAppointmentsApiClient)
  → Suggestion rules (ISuggestionRule implementations)

// API layer
builder.Services.AddApiServices(config, env)
  → Controllers + JSON config
  → Authentication (JWT Bearer)
  → Authorization policies
  → Swagger/OpenApi
  → Rate limiting
  → CORS
  → Health checks (PostgreSQL, RabbitMQ, Redis, OpenAI)
  → MCP server

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
```

---

## 4. Backend Services

### 4.1 Orgs API (`services/orgs-api`, port 5020)

**Purpose:** Authentication, organization management, billing, and permissions.

**Domain entities:**
- `User` — system user with email, password hash, profile
- `Organization` — tenant container (name, plan, settings)
- `OrganizationMember` — user ↔ org relation with role
- `Invitation` — pending org invitations
- `Agency` — real estate agency within an org
- `Branch` — office branch within an agency
- `Permission` — granular permissions per user per org
- `AuditLog` — system-wide audit trail

**Key capabilities:**
- Cookie-based auth (login, register, forgot/reset password, email verification)
- JWT token issuance (access + refresh tokens)
- CSRF protection
- Multi-tenancy (org isolation)
- RBAC (admin, manager, agent, viewer roles)
- Agency hierarchy (org → agency → branch)
- Billing management (plan selection, payment history)
- Feature flags

### 4.2 Properties API (`services/properties-api`, port 5030)

**Purpose:** Core property domain CRUD.

**Domain entities:**
- `Property` — aggregate root with full real estate data model
- `PropertyType` enum — Apartment, House, Villa, Studio, Penthouse, Duplex, Commercial, Land, Garage, Storage
- `OperationType` enum — Sale, Rent, Transfer
- `PropertyStatus` enum — Draft, Active, Reserved, Sold, Rented, Archived
- `Address` — full address with city, province, postal code, coordinates
- `PropertyFeatures` — bedrooms, bathrooms, built area, plot area, amenities
- `PropertyFinancials` — price, HOA fees, property tax, energy certificate
- `LocalizedText` — multilingual descriptions (ES, EN, FR, DE, NL)

**API endpoints:**
- Full CRUD (GET list/detail, POST create, PUT update, DELETE soft-delete)
- Status transitions (activate, reserve, sell, rent, archive, reactivate)
- Search with filters (type, operation, city, price range, bedrooms, status)

**SDK Client** (`Propely.PropertiesApi.Client`):
- `IPropertiesApiClient` — Refit interface consumed by ai-api

### 4.3 Contacts API (`services/contacts-api`, port 5050)

**Purpose:** Contact and lead management.

**Domain entities:**
- `Contact` — buyer, seller, tenant, landlord, or professional
- `Lead` — potential client inquiry linked to a property
- `LeadStatus` — New, Contacted, Qualified, Converted, Lost
- `ContactRole` — Buyer, Seller, Tenant, Landlord, Professional
- `LeadSource` — Portal, Phone, WalkIn, Website, Referral

**Key capabilities:**
- Lead pipeline management (status transitions)
- Lead-to-contact conversion
- Contact deduplication
- Property interest tracking

### 4.4 Appointments API (`services/appointments-api`, port 5060)

**Purpose:** Scheduling and calendar management.

**Domain entities:**
- `Appointment` — aggregate with start/end time, type, status
- `AppointmentType` — PropertyViewing, OwnerMeeting, Generic
- `AppointmentStatus` — Scheduled, Confirmed, Completed, Cancelled, NoShow

**Key capabilities:**
- Property viewing scheduling
- Status management (confirm, complete, cancel, no-show)
- Rescheduling
- Date range queries
- FullCalendar integration support (from frontend)

### 4.5 Publishing API (`services/publishing-api`, port 5040)

**Status:** Scaffolded but deprioritized. Infrastructure is in place (Clean Architecture layers, EF Core, Docker) but no business logic has been implemented. Reserved for future portal feeds and webhooks (Phase P6).

---

## 5. Frontend (Next.js Web Application)

### 5.1 Technology Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| Next.js | 16.1.6 | App Router, standalone output |
| React | 19.2.4 | UI rendering |
| Tailwind CSS | v4 | Utility-first styling |
| next-intl | 4.8.3 | i18n (EN, ES) |
| @jsonforms/react | 3.7.0 | Schema-driven property forms |
| @fullcalendar/* | 6.1.20 | Calendar views |
| cmdk | 1.1.1 | AI command palette |
| react-hook-form | 7.71.1 | Form management |
| zod | 4.3.6 | Schema validation |
| recharts | 3.8.0 | Dashboard charts |

### 5.2 Key UI Components

**AI Command Bar** — The primary NL interface:
- `CommandBar.tsx` — cmdk-based palette (Ctrl+K)
- `VoiceMicButton.tsx` — voice recording with audio waveform feedback
- `ConfirmationPanel.tsx` — confirmation for destructive actions
- `CommandResult.tsx` — action result display
- `CommandHistory.tsx` — recent command history

**Property wizard** — 6-step form:
1. BasicInfoStep (type, operation, title)
2. LocationStep (address, city, coordinates)
3. FinancialStep (price, taxes, HOA)
4. FeaturesStep (rooms, amenities, area)
5. DescriptionsStep (multilingual descriptions)
6. MediaStep (photos, floor plans)

### 5.3 State Management

- No external state library (Redux, Zustand, etc.)
- React Context for global concerns (Toast, FeatureFlags, CommandBar)
- Custom hooks for data fetching (`useFetch<T>`, `usePaginatedFetch<T>`)
- Domain hooks for each entity (`useProperties`, `useContacts`, `useLeads`, etc.)
- In-memory token store (security: no localStorage for JWT)
- API layer with automatic token refresh on 401

### 5.4 Authentication Flow

1. Login → POST to orgs-api → HttpOnly cookie set
2. CSRF token fetched and validated per request
3. Cross-origin APIs use Bearer token from in-memory store
4. Automatic refresh: 401 → call `/auth/refresh` → retry original request
5. Refresh deduplication (single in-flight refresh)

---

## 6. Inter-Service Communication

### 6.1 NuGet SDK Client Pattern

Each service publishes a `Propely.<Service>.Client` project:

```
Propely.<Service>.Client/
  I<Service>ApiClient.cs        // Refit interface
  Dtos/                         // Request/response DTOs
  <Service>ApiClientOptions.cs  // Config (BaseUrl, Timeout, etc.)
  ServiceCollectionExtensions.cs // DI registration
```

### 6.2 Resilience (Polly)

All SDK clients are configured with:

| Policy | Configuration |
|--------|--------------|
| Retry | 3 attempts, exponential backoff + jitter, on 5xx/408/429 |
| Circuit Breaker | 50% failure rate (min 5 in 30s), 30s break |
| Timeout | 30s per request |

### 6.3 Tenant Context Propagation

`TenantDelegatingHandler` (shared library) automatically adds `X-Org-Id` header to all outgoing SDK client requests, propagating tenant context across service boundaries.

### 6.4 SDK Client Matrix

| SDK | Publisher | Consumers |
|-----|-----------|-----------|
| `Propely.OrgsApi.Client` | orgs-api | All services |
| `Propely.PropertiesApi.Client` | properties-api | ai-api, contacts-api, appointments-api |
| `Propely.ContactsApi.Client` | contacts-api | ai-api, appointments-api |
| `Propely.AppointmentsApi.Client` | appointments-api | ai-api |

---

## 7. Infrastructure & DevOps

### 7.1 Docker Compose

Three compose files:
- **`docker-compose.yml`** — Development (hot-reload, bind mounts, health checks)
- **`docker-compose.ci.yml`** — CI overlay (production Dockerfiles, CI namespacing)
- **`docker-compose.production.yml`** — Production (CPU limits, no exposed infra ports)

### 7.2 CI/CD Pipeline

**Main pipeline (`ci.yml`):**
1. Path-based change detection per service
2. Matrix build for .NET services (restore, vuln check, build, test+coverage)
3. Web build (npm ci, audit, lint, test:coverage, build)
4. E2E tests (full Docker stack, Playwright)
5. Third-party notice + license header checks

**Coverage thresholds:** 60% for most services, 50% for properties-api.

**Deployment:**
- `publish.yml` → Docker images to GHCR
- `deploy.yml` → Google Cloud Run (staging) via Workload Identity Federation
- `release.yml` → Semantic Release (conventional commits)
- `rollback.yml` → Cloud Run rollback

**Runner:** Single self-hosted Windows runner (`AI-AGENTS-MASTER`).

### 7.3 Environment Management

- `.env.example` (committed, template)
- `.env` (gitignored, secrets)
- `.env.docker` (committed, Docker hostname overrides)
- Service prefix convention: `AIAPI_`, `ORGSAPI_`, `PROPERTIESAPI_`, etc.

---

## 8. Testing Strategy

### 8.1 Backend (per service)

| Level | Framework | Location |
|-------|----------|----------|
| Unit | xUnit + Moq + FluentAssertions | `tests/Propely.<Service>.UnitTests/` |
| Integration | xUnit + WebApplicationFactory + Testcontainers | `tests/Propely.<Service>.IntegrationTests/` |

**TDD is mandatory** — Red-Green-Refactor cycle enforced by governance.

### 8.2 Frontend

| Level | Framework | Location |
|-------|----------|----------|
| Unit/Component | Vitest + Testing Library + happy-dom | `src/**/__tests__/*.test.tsx` |
| E2E | Playwright (Chromium) | `e2e/*.spec.ts` |

**Coverage thresholds:** 50% (lines, functions, branches, statements).

**E2E specs (14):** auth, login, register, org creation, invite, navigation, permissions, properties, contacts, dashboard, AI visibility.

---

## 9. Appendix: File Index

### AI Action Engine — Key Files

| File | Purpose |
|------|---------|
| `Application/Actions/Commands/ExecuteAction/ExecuteActionCommand.cs` | MediatR command for text-based action |
| `Application/Actions/Commands/ExecuteAction/ExecuteActionCommandHandler.cs` | **Central orchestrator** — classification → routing → context |
| `Application/Actions/Interfaces/IIntentClassifier.cs` | Provider-agnostic intent classification interface |
| `Application/Actions/Interfaces/IActionRouter.cs` | Action routing interface |
| `Application/Actions/Interfaces/IParameterBinder.cs` | Parameter binding interface |
| `Application/Actions/Interfaces/IConversationContext.cs` | Session storage interface |
| `Application/Actions/Tools/ToolSchema.cs` | Provider-neutral tool schema record |
| `Application/Actions/Tools/ToolSchemaRegistry.cs` | **Registry of all 20 tool schemas** |
| `Application/Actions/Tools/IToolAdapter.cs` | LLM provider adapter interface |
| `Application/Actions/Models/ConversationSession.cs` | Session model (exchanges + entity memory) |
| `Application/Actions/Models/ConversationExchange.cs` | Single exchange record |
| `Application/Actions/Models/EntityMemory.cs` | Entity ID tracking for pronoun resolution |
| `Application/Actions/Parameters/*.cs` | 20 typed parameter records |
| `Application/Actions/Handlers/*.cs` | 20 action handlers |
| `Application/Actions/Commands/*Actions/*.cs` | 20 MediatR commands |
| `Domain/Actions/ActionType.cs` | 21-value enum of all action types |
| `Domain/Actions/ClassifiedIntent.cs` | Intent classification result |
| `Domain/Actions/ActionResult.cs` | Action execution result |
| `Infrastructure/AI/OpenAiIntentClassifier.cs` | **OpenAI function calling classifier** |
| `Infrastructure/AI/ActionRouter.cs` | Routes ActionType → MediatR command |
| `Infrastructure/AI/DefaultParameterBinder.cs` | Dict → typed record binding |
| `Infrastructure/AI/RedisConversationContext.cs` | Redis-backed session storage |
| `Infrastructure/AI/Adapters/OpenAiToolAdapter.cs` | ToolSchema → ChatTool conversion |
| `Infrastructure/Services/OpenAiService.cs` | Text gen, JSON gen, image analysis |
| `Infrastructure/Services/OpenAiVoiceTranscriptionService.cs` | STT via gpt-4o-mini-transcription |
| `Infrastructure/MCP/McpToolHandler.cs` | MCP → ActionRouter bridge |
| `Api/Controllers/ActionsController.cs` | POST /v1/actions/execute |
| `Api/Controllers/VoiceController.cs` | POST /v1/voice/transcribe, /v1/voice/execute |
| `Api/Controllers/SuggestionsController.cs` | GET /v1/suggestions |
| `Api/MCP/PropelyMcpTools.cs` | 20 MCP tool definitions |
| `Api/Program.cs` | Service entry point, middleware pipeline |

---

*End of Technical Report*
