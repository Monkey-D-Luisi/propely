// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Infrastructure.Persistence.Entities;

namespace Propely.ContactsApi.UnitTests.Infrastructure.Persistence.Entities;

public sealed class ProcessedEventTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var eventId = Guid.NewGuid();
        var eventType = "TestEventType";

        var result = ProcessedEvent.Create(eventId, eventType);

        result.EventId.Should().Be(eventId);
        result.EventType.Should().Be(eventType);
        result.ProcessedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
