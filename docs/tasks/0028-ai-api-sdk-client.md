# Task: 0028-ai-api-sdk-client

## Metadata
- ID: 0028
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0028-ai-api-sdk-client.md`
  - Pattern reference: `docs/tasks/0004-nuget-sdk-client-infrastructure.md`

## Goal
Create the `Propely.AiApi.Client` NuGet SDK package following the established SDK client pattern (Refit + Polly + TenantDelegatingHandler). This client enables other services to invoke AI API endpoints for action execution, voice transcription, and content generation in a type-safe manner.

## Context
The AI API (`ai-api`) is the AI orchestration layer of the Propely platform. Other services and the frontend need to call AI API endpoints for natural language action execution, voice transcription, and content generation (text extraction, photo extraction, copy generation). Following the established SDK client pattern from `Propely.PropertiesApi.Client`, this task creates the corresponding client for the AI API.

Key difference from other SDK clients: AI calls are **not idempotent** (they invoke LLM inference), so the Polly resilience policy uses a **60-second timeout only** with **NO retry** and **NO circuit breaker**. This prevents duplicate AI operations and respects the longer latency of AI inference calls.

## Scope
### In scope
- Create `Propely.AiApi.Client` project inside `services/ai-api/src/`
- Refit-based typed HTTP client interfaces for actions, voice, and content endpoints
- `TenantDelegatingHandler` for X-Tenant-Id header propagation
- Polly timeout-only policy (60 seconds, no retry, no circuit breaker)
- `IServiceCollection.AddAiApiClient(Action<AiApiClientOptions> configure)` extension method
- `AiApiClientOptions` configuration class (BaseUrl, Timeout)
- Client DTOs mirroring the AI API response shapes
- Unit tests for DI registration
- Task and walkthrough documentation

### Out of scope
- Modifying the AI API itself
- Authentication token propagation (handled separately)
- Registering the client in consumer services (separate tasks)

## Requirements
- R1: The SDK client must use Refit for declarative HTTP client generation
- R2: The `TenantDelegatingHandler` must read tenant ID from `IHttpContextAccessor` and add `X-Tenant-Id` header
- R3: Polly policy must use 60-second timeout ONLY -- NO retry, NO circuit breaker (AI calls are not idempotent)
- R4: The DI extension must register `IActionApi`, `IVoiceApi`, and `IContentApi` in a single call
- R5: Voice endpoints must use `[Multipart]` attribute for audio file upload
- R6: All DTOs must mirror the AI API response shapes exactly

## Refit Interfaces

### IActionApi
- `POST /v1/actions/execute` -- Execute an AI action from natural language text

### IVoiceApi
- `POST /v1/voice/transcribe` -- Transcribe audio to text (multipart)
- `POST /v1/voice/execute` -- Transcribe and execute as action (multipart)

### IContentApi
- `POST /v1/actions/execute` -- Extract data from text (convenience wrapper)
- `POST /v1/actions/execute` -- Generate marketing copy (convenience wrapper)

## DTOs
- `ExecuteActionRequest` -- Input text for action execution
- `ActionResultDto` -- Action execution result with success, type, data, message, errors, confidence
- `TranscriptionResultDto` -- Transcription result with text, language, duration
- `VoiceExecuteResultDto` -- Combined transcription + action result
- `ExtractedPropertyDto` -- Structured property data from AI extraction
- `ExtractedFieldDto<T>` -- Generic field with confidence score
- `GenerateCopyRequest` -- Copy generation parameters
- `GeneratedCopyDto` -- Generated copy in multiple languages

## Verification
- `dotnet build services/ai-api/Propely.AiApi.sln` -- Must build without errors
- `dotnet test services/ai-api/Propely.AiApi.sln` -- All tests must pass
- DI registration tests verify all three interfaces and TenantDelegatingHandler are registered
