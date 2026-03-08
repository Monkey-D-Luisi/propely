// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Infrastructure.AI;

public sealed class RedisConversationContextTests
{
    private readonly ICacheService _cache;
    private readonly RedisConversationContext _context;
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const string SessionId = "session-abc-123";

    public RedisConversationContextTests()
    {
        _cache = Substitute.For<ICacheService>();
        var options = Options.Create(new ConversationContextOptions { SessionTtlMinutes = 30, MaxExchanges = 10 });
        _context = new RedisConversationContext(_cache, options);
    }

    [Fact]
    public async Task GetAsync_ShouldDelegateToCacheWithCorrectKey()
    {
        // Arrange
        var expected = new ConversationSession();
        _cache.GetAsync<ConversationSession>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _context.GetAsync(TenantId, AgentId, SessionId);

        // Assert
        result.Should().BeSameAs(expected);
        await _cache.Received(1).GetAsync<ConversationSession>(
            $"conversation:{TenantId}:{AgentId}:{SessionId}",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WhenSessionNotFound_ShouldReturnNull()
    {
        // Arrange
        _cache.GetAsync<ConversationSession>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ConversationSession?)null);

        // Act
        var result = await _context.GetAsync(TenantId, AgentId, SessionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldDelegateToCacheWithCorrectKeyAndTtl()
    {
        // Arrange
        var session = new ConversationSession();

        // Act
        await _context.SaveAsync(TenantId, AgentId, SessionId, session);

        // Assert
        await _cache.Received(1).SetAsync(
            $"conversation:{TenantId}:{AgentId}:{SessionId}",
            session,
            TimeSpan.FromMinutes(30),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_DifferentTenants_ShouldUseDifferentKeys()
    {
        // Arrange
        var otherTenant = Guid.NewGuid();
        var session = new ConversationSession();

        // Act
        await _context.SaveAsync(TenantId, AgentId, SessionId, session);
        await _context.SaveAsync(otherTenant, AgentId, SessionId, session);

        // Assert — two different keys used
        await _cache.Received(1).SetAsync(
            $"conversation:{TenantId}:{AgentId}:{SessionId}",
            Arg.Any<ConversationSession>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        await _cache.Received(1).SetAsync(
            $"conversation:{otherTenant}:{AgentId}:{SessionId}",
            Arg.Any<ConversationSession>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }
}
