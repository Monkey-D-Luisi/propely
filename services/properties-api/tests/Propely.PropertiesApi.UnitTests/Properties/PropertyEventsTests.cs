// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.Events;

namespace Propely.PropertiesApi.UnitTests.Properties;

public class PropertyEventsTests
{
    [Fact]
    public void PropertyUpdatedV1_SetsAllProperties()
    {
        var propertyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var evt = new PropertyUpdatedV1(propertyId, tenantId, agentId, occurredAt);

        evt.EventId.Should().NotBeEmpty();
        evt.EventType.Should().Be(nameof(PropertyUpdatedV1));
        evt.SchemaVersion.Should().Be(1);
        evt.OccurredAtUtc.Should().Be(occurredAt);
        evt.Producer.Should().Be("PropertiesApi");
        evt.Data.PropertyId.Should().Be(propertyId);
        evt.Data.TenantId.Should().Be(tenantId);
        evt.Data.AgentId.Should().Be(agentId);
    }

    [Fact]
    public void PropertyUpdatedV1_SupportsCorrelationAndCausation()
    {
        var evt = new PropertyUpdatedV1(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow)
        {
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        evt.CorrelationId.Should().NotBeNull();
        evt.CausationId.Should().NotBeNull();
    }

    [Fact]
    public void PropertyStatusChangedV1_SetsAllProperties()
    {
        var propertyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var evt = new PropertyStatusChangedV1(propertyId, tenantId, agentId,
            PropertyStatus.Draft, PropertyStatus.Active, occurredAt);

        evt.EventId.Should().NotBeEmpty();
        evt.EventType.Should().Be(nameof(PropertyStatusChangedV1));
        evt.SchemaVersion.Should().Be(1);
        evt.OccurredAtUtc.Should().Be(occurredAt);
        evt.Producer.Should().Be("PropertiesApi");
        evt.Data.PropertyId.Should().Be(propertyId);
        evt.Data.TenantId.Should().Be(tenantId);
        evt.Data.AgentId.Should().Be(agentId);
        evt.Data.PreviousStatus.Should().Be(PropertyStatus.Draft);
        evt.Data.NewStatus.Should().Be(PropertyStatus.Active);
    }

    [Fact]
    public void PropertyStatusChangedV1_SupportsCorrelationAndCausation()
    {
        var evt = new PropertyStatusChangedV1(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            PropertyStatus.Draft, PropertyStatus.Active, DateTime.UtcNow)
        {
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        evt.CorrelationId.Should().NotBeNull();
        evt.CausationId.Should().NotBeNull();
    }

    [Fact]
    public void PropertyUpdatedV1Data_RecordEquality()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var data1 = new PropertyUpdatedV1Data(id, tenantId, agentId);
        var data2 = new PropertyUpdatedV1Data(id, tenantId, agentId);

        data1.Should().Be(data2);
    }

    [Fact]
    public void PropertyStatusChangedV1Data_RecordEquality()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var data1 = new PropertyStatusChangedV1Data(id, tenantId, agentId, PropertyStatus.Draft, PropertyStatus.Active);
        var data2 = new PropertyStatusChangedV1Data(id, tenantId, agentId, PropertyStatus.Draft, PropertyStatus.Active);

        data1.Should().Be(data2);
    }
}
