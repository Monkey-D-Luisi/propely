// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Models;

/// <summary>
/// A conversation session comprising a history of exchanges and entity memory.
/// Stored in Redis with a sliding TTL.
/// </summary>
public sealed class ConversationSession
{
    public List<ConversationExchange> Exchanges { get; init; } = [];
    public EntityMemory EntityMemory { get; set; } = EntityMemory.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Adds an exchange and trims the history to the max window size.
    /// </summary>
    public void AddExchange(ConversationExchange exchange, int maxExchanges)
    {
        Exchanges.Add(exchange);
        LastActivityAt = exchange.Timestamp;

        if (Exchanges.Count > maxExchanges)
        {
            Exchanges.RemoveRange(0, Exchanges.Count - maxExchanges);
        }
    }
}
