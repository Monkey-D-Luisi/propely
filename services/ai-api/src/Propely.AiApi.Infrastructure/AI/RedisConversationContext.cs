// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Options;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Application.Common.Interfaces;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Redis-backed implementation of conversation context storage.
/// Uses ICacheService for serialization and TTL management.
/// </summary>
public sealed class RedisConversationContext : IConversationContext
{
    private readonly ICacheService _cache;
    private readonly ConversationContextOptions _options;

    private const string KeyPrefix = "conversation";

    public RedisConversationContext(ICacheService cache, IOptions<ConversationContextOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<ConversationSession?> GetAsync(Guid tenantId, Guid agentId, string sessionId, CancellationToken ct = default)
    {
        var key = BuildKey(tenantId, agentId, sessionId);
        return await _cache.GetAsync<ConversationSession>(key, ct);
    }

    public async Task SaveAsync(Guid tenantId, Guid agentId, string sessionId, ConversationSession session, CancellationToken ct = default)
    {
        var key = BuildKey(tenantId, agentId, sessionId);
        var ttl = TimeSpan.FromMinutes(_options.SessionTtlMinutes);
        await _cache.SetAsync(key, session, ttl, ct);
    }

    internal static string BuildKey(Guid tenantId, Guid agentId, string sessionId)
        => $"{KeyPrefix}:{tenantId}:{agentId}:{sessionId}";
}
