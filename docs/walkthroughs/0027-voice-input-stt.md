# Walkthrough: 0027-voice-input-stt

## Task Reference
- Task: `docs/tasks/0027-voice-input-stt.md`
- Walkthrough: `docs/walkthroughs/0027-voice-input-stt.md`
- Branch/PR: `feat/p3-ai-action-engine`
- Date: `2026-03-05`

## Summary
Added voice input (speech-to-text) capability to the ai-api service using OpenAI's `gpt-4o-mini-transcription` model. Two new endpoints enable agents to interact via voice: a transcribe-only endpoint and a combined voice-to-action endpoint that chains transcription with the existing AI action execution pipeline.

## Context
- Background: The AI action engine already supported text-based action execution. Voice is the natural next input modality for the platform's core differentiator (natural language process automation).
- Problem statement: Agents needed the ability to use voice commands to interact with the system, requiring speech-to-text integration.
- Constraints: Must use OpenAI SDK v2.9.0 (already in the project). Must follow Clean Architecture layering. Must validate audio files thoroughly for security and UX.

## Decisions & Trade-offs
- **Decision: Separate IVoiceTranscriptionService interface vs extending IOpenAiService**
  - Options considered: (a) Add voice methods to IOpenAiService, (b) Create dedicated IVoiceTranscriptionService
  - Why this choice: Interface Segregation Principle — voice transcription is a distinct capability from text generation. Separate interfaces enable independent testing and future provider swaps.
  - Consequences / risks: One more interface, but clean separation of concerns.

- **Decision: TranscriptionResult in Application.Common.Interfaces namespace**
  - Options considered: (a) Actions/Models, (b) Common/Interfaces, (c) Common/Models
  - Why this choice: The record is tightly coupled to the IVoiceTranscriptionService interface. Co-locating it with the interface follows the established pattern (e.g., no separate Models folder for interface return types).
  - Consequences / risks: If more voice-related models emerge, a dedicated namespace may be warranted.

- **Decision: Content type AND extension validation (OR logic)**
  - Options considered: (a) Content type only, (b) Extension only, (c) Both required, (d) Either accepted
  - Why this choice: Browsers/clients may set generic content types (e.g., `application/octet-stream`) even for valid audio files. Accepting either content type OR extension is more resilient.
  - Consequences / risks: Slightly more permissive, but the OpenAI API will reject truly invalid formats.

- **Decision: Scoped service registration**
  - Options considered: Singleton (like OpenAiService), Scoped, Transient
  - Why this choice: Scoped is appropriate since the AudioClient doesn't need singleton lifecycle and options are read once per scope.
  - Consequences / risks: One AudioClient created per request scope, which is fine for this use case.

## Implementation Notes
- Key changes:
  - `IVoiceTranscriptionService` interface with `TranscribeAsync(Stream, string, string?, CancellationToken)` in Application layer
  - `TranscriptionResult` record with Text, Language, DurationMs in Application layer
  - `OpenAiVoiceTranscriptionService` using `AudioClient("gpt-4o-mini-transcription", apiKey)` in Infrastructure layer
  - `VoiceController` with POST `/v1/voice/transcribe` and POST `/v1/voice/execute` in API layer
  - `TranscriptionResultDto` and `VoiceExecuteResultDto` response DTOs
  - `OpenAiOptions.TranscriptionModelId` property (default: "gpt-4o-mini-transcription")
  - Rate limit rules (10 req/min) for both voice endpoints
  - 8 service tests + 20 controller tests = 28 new unit tests
- Edge cases handled:
  - Null/empty audio file
  - File size exceeding 25 MB (including exact boundary)
  - Unsupported content types and extensions
  - Files with valid extension but unknown content type (accepted)
  - Empty transcription result (no speech detected)
  - Missing user authentication
  - Missing org context
  - API key not configured
- Known limitations:
  - AudioClient cannot be easily mocked with NSubstitute (sealed class); service tests focus on configuration validation and null API key handling
  - No streaming transcription support (batch only)
  - No audio storage or transcription history

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A (new endpoints only)

## Commands Run
```bash
dotnet build services/ai-api/Propely.AiApi.sln --verbosity quiet
dotnet test services/ai-api/Propely.AiApi.sln --verbosity quiet
```

## Files Changed

### Created
| File | Purpose |
|------|---------|
| `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/IVoiceTranscriptionService.cs` | Voice transcription service interface |
| `services/ai-api/src/Propely.AiApi.Application/Common/Interfaces/TranscriptionResult.cs` | Transcription result record |
| `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiVoiceTranscriptionService.cs` | OpenAI Audio API implementation |
| `services/ai-api/src/Propely.AiApi.Api/Controllers/VoiceController.cs` | Voice endpoints controller |
| `services/ai-api/src/Propely.AiApi.Api/Dtos/TranscriptionResultDto.cs` | Transcription response DTO |
| `services/ai-api/src/Propely.AiApi.Api/Dtos/VoiceExecuteResultDto.cs` | Voice execute response DTO |
| `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Services/OpenAiVoiceTranscriptionServiceTests.cs` | Service unit tests |
| `services/ai-api/tests/Propely.AiApi.UnitTests/Api/Controllers/VoiceControllerTests.cs` | Controller unit tests |

### Modified
| File | Change |
|------|--------|
| `services/ai-api/src/Propely.AiApi.Infrastructure/Services/OpenAiOptions.cs` | Added `TranscriptionModelId` property |
| `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` | Registered `IVoiceTranscriptionService` as Scoped |
| `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs` | Added rate limit rules for voice endpoints |

## Checklist
- [x] Application interface created (IVoiceTranscriptionService)
- [x] Infrastructure service implemented (OpenAiVoiceTranscriptionService)
- [x] API controller with both endpoints (VoiceController)
- [x] DTOs created (TranscriptionResultDto, VoiceExecuteResultDto)
- [x] OpenAiOptions extended with TranscriptionModelId
- [x] DI registration in Infrastructure
- [x] Rate limiting in API DI
- [x] Unit tests for service
- [x] Unit tests for controller
- [ ] Build passes
- [ ] All tests pass
