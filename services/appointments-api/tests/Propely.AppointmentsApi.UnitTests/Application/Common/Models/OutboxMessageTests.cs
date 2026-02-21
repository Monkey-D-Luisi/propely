// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Application.Common.Models;

namespace Propely.AppointmentsApi.UnitTests.Application.Common.Models;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var message = OutboxMessage.Create(
            id, "TestEvent", """{"key":"value"}""", occurredAt, correlationId, causationId);

        message.Id.Should().Be(id);
        message.EventType.Should().Be("TestEvent");
        message.Payload.Should().Be("""{"key":"value"}""");
        message.OccurredAtUtc.Should().Be(occurredAt);
        message.CorrelationId.Should().Be(correlationId);
        message.CausationId.Should().Be(causationId);
        message.ProcessedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutOptionalParams_ShouldLeaveNullable()
    {
        var message = OutboxMessage.Create(
            Guid.NewGuid(), "TestEvent", "{}", DateTime.UtcNow);

        message.CorrelationId.Should().BeNull();
        message.CausationId.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedAtUtc()
    {
        var message = OutboxMessage.Create(
            Guid.NewGuid(), "TestEvent", "{}", DateTime.UtcNow);
        var processedAt = DateTime.UtcNow;

        message.MarkAsProcessed(processedAt);

        message.ProcessedAtUtc.Should().Be(processedAt);
    }
}
