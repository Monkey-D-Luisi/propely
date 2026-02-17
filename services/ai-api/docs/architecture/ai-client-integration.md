# AI Client Integration

> **Status: Design Spec (partially implemented).** This document was written as a future design plan. The actual implementation uses `IOpenAiService` (not `IAiClient`) with a simpler interface: `GenerateTextAsync(prompt, cancellationToken)`. The multi-provider strategy (`FakeAiClient`, `NoOpAiClient`, `AiClientConfiguration`) described here was not implemented. See `src/SaasTemplate.AiApi.Application/Common/Interfaces/IOpenAiService.cs` and `src/SaasTemplate.AiApi.Infrastructure/Services/OpenAiService.cs` for the actual implementation.

## Overview

This document defines the architecture for integrating AI services (such as OpenAI, Anthropic, etc.) into the application. The integration is designed with decoupling, testability, and graceful degradation as core principles.

**Priority:** Lower than core vertical slice. Implement after Tasks 0002-0010 are complete.

## Design Principles

### 1. Decoupled Interface
The AI client is abstracted behind an interface defined in the Application layer. Infrastructure provides the implementation.

### 2. No Hard Dependency
The core system must function without the AI client. AI features are additive, not essential.

### 3. Graceful Degradation
If the AI service is unavailable (down, misconfigured, or API key missing), the system continues operating with degraded functionality.

### 4. Testable
AI interactions can be easily mocked for unit tests and faked for local development.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Application Layer                                            │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ IAiClient (interface)                                    │ │
│ │ - EvaluateWorkItemDescriptionAsync(...)                  │ │
│ │ - SuggestWorkItemTitleAsync(...)                         │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ implements
┌─────────────────────────────┼───────────────────────────────┐
│ Infrastructure Layer        │                                │
│ ┌───────────────────────────┴─────────────────────────────┐ │
│ │ OpenAiClient / AnthropicClient (real implementation)     │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ FakeAiClient (for local development)                     │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ NoOpAiClient (when AI is disabled)                       │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Interface Definition

```csharp
// src/SaasTemplate.AiApi.Application/Common/Interfaces/IAiClient.cs

namespace SaasTemplate.AiApi.Application.Common.Interfaces;

public interface IAiClient
{
    /// <summary>
    /// Evaluates a work item description for quality, clarity, and completeness.
    /// </summary>
    /// <returns>Evaluation result or null if AI is unavailable</returns>
    Task<DescriptionEvaluation?> EvaluateWorkItemDescriptionAsync(
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests improvements or alternative titles based on description.
    /// </summary>
    /// <returns>Suggestions or empty list if AI is unavailable</returns>
    Task<IReadOnlyList<string>> SuggestWorkItemTitlesAsync(
        string description,
        int maxSuggestions = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the AI client is available and configured.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

public record DescriptionEvaluation(
    decimal ClarityScore,      // 0.0 to 1.0
    decimal CompletenessScore, // 0.0 to 1.0
    IReadOnlyList<string> Suggestions);
```

## Implementation Strategy

### Real Implementation (Production)

```csharp
// src/SaasTemplate.AiApi.Infrastructure/AiClients/OpenAiClient.cs

public class OpenAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiClient> _logger;

    public async Task<DescriptionEvaluation?> EvaluateWorkItemDescriptionAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken))
        {
            _logger.LogWarning("AI client unavailable, skipping evaluation");
            return null;
        }

        try
        {
            // Call OpenAI API
            var response = await _httpClient.PostAsync(...);
            // Parse and return result
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI evaluation failed");
            return null; // Graceful degradation
        }
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!string.IsNullOrEmpty(_options.ApiKey));
    }
}
```

### Fake Implementation (Development)

```csharp
// src/SaasTemplate.AiApi.Infrastructure/AiClients/FakeAiClient.cs

public class FakeAiClient : IAiClient
{
    public Task<DescriptionEvaluation?> EvaluateWorkItemDescriptionAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        // Return deterministic fake data for development
        return Task.FromResult<DescriptionEvaluation?>(new DescriptionEvaluation(
            ClarityScore: 0.85m,
            CompletenessScore: 0.75m,
            Suggestions: new[] { "Consider adding acceptance criteria" }));
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
```

### No-Op Implementation (Disabled)

```csharp
// src/SaasTemplate.AiApi.Infrastructure/AiClients/NoOpAiClient.cs

public class NoOpAiClient : IAiClient
{
    public Task<DescriptionEvaluation?> EvaluateWorkItemDescriptionAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DescriptionEvaluation?>(null);
    }

    public Task<IReadOnlyList<string>> SuggestWorkItemTitlesAsync(
        string description,
        int maxSuggestions = 3,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
```

## Configuration

### Environment Variables

```bash
# In monorepo root .env (AIAPI_ prefix stripped by .NET at startup)

# AI Client Configuration
AIAPI_OpenAi__ApiKey=CHANGEME          # OpenAI API key
AIAPI_AiClient__Provider=OpenAI        # OpenAI | Fake | Disabled
AIAPI_AiClient__Model=gpt-4           # Model to use
AIAPI_AiClient__TimeoutSeconds=30      # Request timeout
```

### appsettings.json

```json
{
  "AiClient": {
    "Provider": "Disabled",
    "ApiKey": "",
    "Model": "gpt-4",
    "TimeoutSeconds": 30
  }
}
```

### Registration

```csharp
// src/SaasTemplate.AiApi.Api/Configuration/AiClientConfiguration.cs

public static class AiClientConfiguration
{
    public static IServiceCollection AddAiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["AiClient:Provider"] ?? "Disabled";

        services.AddSingleton<IAiClient>(sp => provider switch
        {
            "OpenAI" => new OpenAiClient(...),
            "Anthropic" => new AnthropicClient(...),
            "Fake" => new FakeAiClient(),
            _ => new NoOpAiClient()
        });

        return services;
    }
}
```

## Usage in Application Layer

```csharp
// Example: Enriching work item creation with AI suggestions

public class CreateWorkItemCommandHandler
{
    private readonly IAiClient _aiClient;
    private readonly IWorkItemRepository _repository;

    public async Task<CreateWorkItemResult> Handle(
        CreateWorkItemCommand command,
        CancellationToken cancellationToken)
    {
        // Create work item (core functionality)
        var workItem = new WorkItem(command.Title, command.Description);
        await _repository.AddAsync(workItem, cancellationToken);

        // Optional AI enhancement (non-blocking)
        var evaluation = await _aiClient.EvaluateWorkItemDescriptionAsync(
            command.Description ?? "",
            cancellationToken);

        return new CreateWorkItemResult(
            workItem.Id,
            workItem.Title,
            workItem.Status,
            workItem.CreatedAtUtc,
            AiEvaluation: evaluation); // May be null
    }
}
```

## Graceful Degradation Scenarios

| Scenario | Behavior |
|----------|----------|
| API key not configured | AI methods return null/empty |
| AI service timeout | Log warning, return null/empty |
| AI service error (500) | Log error, return null/empty |
| Invalid response | Log error, return null/empty |
| Rate limited | Log warning, return null/empty |

The core system ALWAYS continues working. AI features are additive.

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task Handle_ShouldCreateWorkItem_WhenAiUnavailable()
{
    // Arrange
    var aiClient = new NoOpAiClient();
    var handler = new CreateWorkItemCommandHandler(aiClient, _mockRepository);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.AiEvaluation.Should().BeNull();
    // Work item still created successfully
}
```

### Integration Tests

Use `FakeAiClient` to get deterministic results without external dependencies.

## Future Considerations

1. **Caching**: Cache AI responses to reduce API calls for similar inputs
2. **Batching**: Batch multiple AI requests for efficiency
3. **Fallback Chain**: Try multiple providers in order
4. **Circuit Breaker**: Temporarily disable after repeated failures
5. **Usage Tracking**: Monitor AI API usage for cost control

## Files to Create (Future Task)

```
src/
├── SaasTemplate.AiApi.Application/
│   └── Common/
│       └── Interfaces/
│           └── IAiClient.cs
└── SaasTemplate.AiApi.Infrastructure/
    └── AiClients/
        ├── OpenAiClient.cs
        ├── AnthropicClient.cs
        ├── FakeAiClient.cs
        ├── NoOpAiClient.cs
        └── Configuration/
            └── AiClientOptions.cs
```

## Related Documents

- [Security Baseline](../standards/security-baseline.md) — API key handling
- [Vertical Slice](vertical-slice.md) — Core architecture
- [Coding Standards](../standards/naming-conventions.md) — Implementation patterns
