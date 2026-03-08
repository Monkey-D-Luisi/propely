// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Actions.Models;

namespace Propely.AiApi.Application.Actions.Interfaces;

/// <summary>
/// Provides session-scoped conversation context storage for multi-turn AI interactions.
/// </summary>
public interface IConversationContext
{
    /// <summary>
    /// Retrieves an existing conversation session.
    /// </summary>
    /// <returns>The session, or null if not found or expired.</returns>
    Task<ConversationSession?> GetAsync(Guid tenantId, Guid agentId, string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Saves a conversation session with a sliding TTL.
    /// </summary>
    Task SaveAsync(Guid tenantId, Guid agentId, string sessionId, ConversationSession session, CancellationToken ct = default);
}
