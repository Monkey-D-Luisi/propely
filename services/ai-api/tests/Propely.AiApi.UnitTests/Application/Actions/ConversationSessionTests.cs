// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ConversationSessionTests
{
    [Fact]
    public void AddExchange_ShouldAppendToExchanges()
    {
        // Arrange
        var session = new ConversationSession();
        var exchange = new ConversationExchange("create a villa", ActionType.CreateProperty, "Villa created", true, DateTimeOffset.UtcNow);

        // Act
        session.AddExchange(exchange, 10);

        // Assert
        session.Exchanges.Should().HaveCount(1);
        session.Exchanges[0].UserText.Should().Be("create a villa");
    }

    [Fact]
    public void AddExchange_WhenExceedingMaxWindow_ShouldTrimOldestEntries()
    {
        // Arrange
        var session = new ConversationSession();
        for (int i = 0; i < 12; i++)
        {
            session.AddExchange(
                new ConversationExchange($"message-{i}", ActionType.CreateProperty, $"response-{i}", true, DateTimeOffset.UtcNow.AddMinutes(i)),
                10);
        }

        // Assert
        session.Exchanges.Should().HaveCount(10);
        session.Exchanges[0].UserText.Should().Be("message-2");
        session.Exchanges[^1].UserText.Should().Be("message-11");
    }

    [Fact]
    public void AddExchange_ShouldUpdateLastActivityAt()
    {
        // Arrange
        var session = new ConversationSession();
        var before = session.LastActivityAt;
        var exchangeTime = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        session.AddExchange(
            new ConversationExchange("test", ActionType.CreateProperty, "ok", true, exchangeTime),
            10);

        // Assert
        session.LastActivityAt.Should().Be(exchangeTime);
    }

    [Fact]
    public void EntityMemory_DefaultShouldBeEmpty()
    {
        // Arrange
        var session = new ConversationSession();

        // Assert
        session.EntityMemory.LastPropertyId.Should().BeNull();
        session.EntityMemory.LastContactId.Should().BeNull();
        session.EntityMemory.LastLeadId.Should().BeNull();
        session.EntityMemory.LastAppointmentId.Should().BeNull();
    }

    [Fact]
    public void EntityMemory_ShouldSupportWithExpression()
    {
        // Arrange
        var memory = EntityMemory.Empty;
        var propertyId = Guid.NewGuid();

        // Act
        var updated = memory with { LastPropertyId = propertyId };

        // Assert
        updated.LastPropertyId.Should().Be(propertyId);
        updated.LastContactId.Should().BeNull();
        memory.LastPropertyId.Should().BeNull(); // original unchanged
    }
}
