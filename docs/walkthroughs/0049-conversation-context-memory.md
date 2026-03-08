# Walkthrough: 0049-conversation-context-memory

## Overview
This walkthrough explains how multi-turn conversation context was added to the AI action pipeline. The feature allows users to have contextual follow-up commands within a session, where each subsequent command has access to the history of prior exchanges.

## Architecture

```
Client (CommandBar)
  │  sessionId = crypto.randomUUID()
  │
  ├─ POST /v1/actions/execute  { text, sessionId }
  │  └─ ExecuteActionCommand(Text, TenantId, AgentId, SessionId)
  │
  └─ POST /v1/voice/execute  FormData { audio, sessionId }
     └─ ExecuteActionCommand(transcribedText, TenantId, AgentId, SessionId)

Handler Pipeline:
  1. Load session from Redis (if sessionId present)
  2. Pass history snapshot to IIntentClassifier.ClassifyAsync
  3. Route classified intent via IActionRouter
  4. Save new exchange + updated entity memory to Redis
```

## Data Model

### ConversationExchange
A single turn in the conversation: what the user said, what action was classified, and what the result was.

```csharp
public sealed record ConversationExchange(
    string UserText,
    ActionType ActionType,
    string ResultMessage,
    bool Success,
    DateTimeOffset Timestamp);
```

### EntityMemory
Tracks the last-referenced entity IDs from action results, enabling future pronoun resolution.

```csharp
public sealed record EntityMemory
{
    public Guid? LastPropertyId { get; init; }
    public Guid? LastContactId { get; init; }
    public Guid? LastLeadId { get; init; }
    public Guid? LastAppointmentId { get; init; }
}
```

### ConversationSession
The composite session object stored in Redis. Manages exchange list with configurable window (default 10).

```csharp
public sealed class ConversationSession
{
    public List<ConversationExchange> Exchanges { get; init; }
    public EntityMemory EntityMemory { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastActivityAt { get; set; }

    public void AddExchange(ConversationExchange exchange, int maxExchanges)
    {
        Exchanges.Add(exchange);
        LastActivityAt = exchange.Timestamp;
        // Trim to window
        if (Exchanges.Count > maxExchanges)
            Exchanges.RemoveRange(0, Exchanges.Count - maxExchanges);
    }
}
```

## Redis Storage

Key format: `conversation:{tenantId}:{agentId}:{sessionId}`

TTL: 30 minutes (default), configurable via `ConversationContext:SessionTtlMinutes`.

The `RedisConversationContext` delegates to the existing `ICacheService` for JSON serialization/deserialization:

```csharp
public async Task<ConversationSession?> GetAsync(...)
{
    var key = BuildKey(tenantId, agentId, sessionId);
    return await _cache.GetAsync<ConversationSession>(key, ct);
}

public async Task SaveAsync(...)
{
    var key = BuildKey(tenantId, agentId, sessionId);
    await _cache.SetAsync(key, session, TimeSpan.FromMinutes(_options.SessionTtlMinutes), ct);
}
```

## History in Intent Classification

The handler passes a **snapshot copy** of `session.Exchanges` (via `.ToList()`) to the classifier. This is a deliberate design decision — the original list is mutated after classification (new exchange added), and a snapshot ensures the classifier receives an immutable view.

In `OpenAiIntentClassifier`, history is injected between the system prompt and current user message:

```csharp
messages.Add(new SystemChatMessage(SystemPrompt));

if (history is { Count: > 0 })
{
    foreach (var exchange in history)
    {
        messages.Add(new UserChatMessage(exchange.UserText));
        messages.Add(new AssistantChatMessage(exchange.ResultMessage));
    }
}

messages.Add(new UserChatMessage(text));
```

This gives the LLM full conversational context when classifying the current intent.

## Frontend SessionId Management

The `useCommandBar` hook manages session lifecycle:
- **Open**: Generate `crypto.randomUUID()` if no session exists
- **Close**: Reset sessionId to `null`
- Each subsequent command within an open session shares the same sessionId

Both `useExecuteAction` and `useExecuteVoiceAction` hooks accept optional `sessionId` and include it in the request body/form data.

## Backward Compatibility

All changes are backward compatible:
- `SessionId` is optional (`string? SessionId = null`)
- No session = stateless mode (identical to before)
- Existing API consumers that don't send `sessionId` are unaffected
- History is `null` when no session, and the classifier handles it gracefully

## Testing

- **Handler tests**: 10 tests covering stateless, session load/save, history passing, entity memory
- **ConversationSession tests**: AddExchange, window trimming, timestamp updates, entity memory immutability
- **RedisConversationContext tests**: Cache delegation, tenant isolation
- **CommandBar tests**: Updated to verify sessionId propagation
