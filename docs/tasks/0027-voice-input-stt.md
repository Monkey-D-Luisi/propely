# Task: 0027-voice-input-stt

## Metadata
- ID: 0027
- Type: Standard
- Status: IN_PROGRESS
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0027-voice-input-stt.md`
  - Epic: `docs/backlog/epic-P3-ai-action-engine.md` (Task 3.7)

## Goal
Add voice transcription via gpt-4o-mini-transcription (OpenAI Audio API) to the ai-api service, enabling the voice-to-text-to-action pipeline. Two endpoints: transcribe-only and transcribe+execute.

## Context
The AI action engine already supports text-based action execution via `POST /v1/actions/execute`. This task extends the input modality to voice by adding speech-to-text transcription using OpenAI's `gpt-4o-mini-transcription` model. The transcription can be used standalone (transcribe-only) or chained with action execution (voice-to-action).

## Scope
### In scope
- `POST /v1/voice/transcribe` — accepts audio file (multipart), returns transcribed text + language + duration
- `POST /v1/voice/execute` — accepts audio file, transcribes, then executes action (combined endpoint)
- `IVoiceTranscriptionService` interface in Application layer
- `OpenAiVoiceTranscriptionService` in Infrastructure layer using `gpt-4o-mini-transcription` model
- Supported audio formats: WebM/Opus, WAV, MP3, M4A, OGG, FLAC
- Audio size limit: 25 MB
- Response: `{ "text": "...", "language": "es", "durationMs": 3200 }`
- `TranscriptionModelId` property in OpenAiOptions (default "gpt-4o-mini-transcription")
- Rate limiting: 10 req/min for both voice endpoints
- Unit tests for service and controller

### Out of scope
- Voice output / text-to-speech
- Real-time streaming transcription
- Custom vocabulary or fine-tuning
- Audio storage or history

## Requirements
- R1: Audio files accepted via multipart form data
- R2: File size limited to 25 MB with clear validation error
- R3: Supported formats validated by content type and file extension
- R4: Transcription uses OpenAI `gpt-4o-mini-transcription` model (configurable via options)
- R5: Optional BCP-47 language hint passed to transcription API
- R6: `/voice/execute` chains transcription with existing action execution pipeline
- R7: Empty transcription returns graceful failure (no action execution attempted)
- R8: Rate limited to 10 req/min per IP for both voice endpoints
- R9: Both endpoints require JWT authentication and org context

## Acceptance Criteria
- AC1: `POST /v1/voice/transcribe` accepts audio and returns `{ text, language, durationMs }`
- AC2: `POST /v1/voice/execute` transcribes audio and executes the resulting text as an action
- AC3: Files exceeding 25 MB are rejected with 400 Bad Request
- AC4: Unsupported audio formats are rejected with 400 Bad Request
- AC5: Missing audio file returns 400 Bad Request
- AC6: Unauthenticated requests return 401 Unauthorized
- AC7: Requests without org context return 403 Forbidden
- AC8: Empty transcription result returns success=false with "No speech detected" message
- AC9: Rate limits enforced at 10 req/min for voice endpoints
- AC10: All unit tests pass

## Technical Notes
- OpenAI .NET SDK v2.9.0 provides `OpenAI.Audio.AudioClient` with `TranscribeAudioAsync` method
- `AudioTranscriptionOptions` supports `Language` and `ResponseFormat` properties
- `ResponseFormat = Verbose` returns duration and language metadata
- Service registered as Scoped (new AudioClient per scope, reuses options)
- Controller follows same pattern as ActionsController (auth, org context, MediatR)
