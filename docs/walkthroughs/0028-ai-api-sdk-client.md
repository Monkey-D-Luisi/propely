# Walkthrough: 0028-ai-api-sdk-client

## Overview
This walkthrough documents the implementation of the `Propely.AiApi.Client` NuGet SDK package, which provides a typed HTTP client for the AI API service. The client follows the same pattern established by `Propely.PropertiesApi.Client` but with a critical difference: AI calls use timeout-only resilience (no retry, no circuit breaker) because they are not idempotent.

## File Structure

```
services/ai-api/src/Propely.AiApi.Client/
  Propely.AiApi.Client.csproj          # Project file (net10.0, Refit + Polly)
  TenantDelegatingHandler.cs            # X-Tenant-Id header propagation
  IActionApi.cs                         # Refit interface for action execution
  IVoiceApi.cs                          # Refit interface for voice endpoints
  IContentApi.cs                        # Refit interface for content generation
  IAiApiClient.cs                       # Aggregate interface combining all three
  Configuration/
    AiApiClientOptions.cs               # Options: BaseUrl, Timeout (60s default)
    ServiceCollectionExtensions.cs      # DI registration with Polly timeout
  Dtos/
    ExecuteActionRequest.cs             # Action execution input
    ActionResultDto.cs                  # Action execution result
    TranscriptionResultDto.cs           # Voice transcription result
    VoiceExecuteResultDto.cs            # Combined voice + action result
    ExtractedPropertyDto.cs             # AI-extracted property data
    ExtractedFieldDto.cs                # Generic field with confidence
    GenerateCopyRequest.cs              # Copy generation parameters
    GeneratedCopyDto.cs                 # Generated copy variants

services/ai-api/tests/Propely.AiApi.UnitTests/Client/
  ServiceCollectionExtensionsTests.cs   # DI registration tests
```

## Key Design Decisions

### 1. No Retry / No Circuit Breaker
Unlike `PropertiesApi.Client` which uses retry + circuit breaker, the AI API client uses timeout-only resilience. This is because:
- AI inference calls are **not idempotent** -- retrying could cause duplicate property creation, duplicate extractions, etc.
- AI calls have **longer latency** (10-30 seconds is normal) -- the timeout is set to 60 seconds
- Circuit breaker would be counter-productive since AI service temporary slowdowns are expected under load

### 2. Three Separate Refit Interfaces
Instead of one monolithic `IAiApiClient`, the client defines three focused interfaces:
- `IActionApi` -- General action execution
- `IVoiceApi` -- Multipart audio endpoints
- `IContentApi` -- Content generation convenience wrappers

This follows the Interface Segregation Principle. Consumers can depend on only what they need. The `IAiApiClient` aggregate interface is provided for convenience but its use is discouraged.

### 3. Multipart for Voice Endpoints
Voice endpoints use `[Multipart]` with `StreamPart` for audio file upload, matching the server-side `[FromForm] IFormFile` parameter binding.

### 4. Content API as Convenience Layer
`IContentApi` wraps the same `/v1/actions/execute` endpoint as `IActionApi`. It exists to provide semantic clarity at the call site -- consumers call `ExtractFromTextAsync` or `GenerateCopyAsync` instead of the generic `ExecuteActionAsync`.

## DI Registration Pattern
The `AddAiApiClient` extension method registers all three Refit interfaces with:
1. Custom `BaseAddress` from options
2. `TenantDelegatingHandler` for tenant ID propagation
3. Polly timeout handler (60s, no retry, no circuit breaker)

```csharp
services.AddAiApiClient(options =>
{
    options.BaseUrl = configuration["AiApi:BaseUrl"] ?? "http://localhost:5010";
    options.Timeout = TimeSpan.FromSeconds(90); // optional override
});
```

## Test Coverage
- `AddAiApiClient_RegistersIActionApi` -- Verifies IActionApi is registered
- `AddAiApiClient_RegistersIVoiceApi` -- Verifies IVoiceApi is registered
- `AddAiApiClient_RegistersIContentApi` -- Verifies IContentApi is registered
- `AddAiApiClient_RegistersTenantDelegatingHandler` -- Verifies handler is transient
- `AddAiApiClient_WithCustomOptions_AppliesConfiguration` -- Verifies all interfaces with custom config
- `AiApiClientOptions_HasCorrectDefaults` -- Verifies default values (port 5010, 60s timeout)
