// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PublishingApi.Domain.Common;

namespace Propely.PublishingApi.UnitTests.Domain.Common;

public sealed class EntityTests
{
    private sealed class TestEntity : Entity
    {
        public void Raise(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
    }

    [Fact]
    public void DomainEvents_Initially_ShouldBeEmpty()
    {
        var entity = new TestEntity();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToCollection()
    {
        var entity = new TestEntity();
        var domainEvent = Substitute.For<IDomainEvent>();

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void RaiseDomainEvent_MultipleTimes_ShouldAddAllEvents()
    {
        var entity = new TestEntity();
        var event1 = Substitute.For<IDomainEvent>();
        var event2 = Substitute.For<IDomainEvent>();

        entity.Raise(event1);
        entity.Raise(event2);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().ContainInOrder(event1, event2);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var entity = new TestEntity();
        entity.Raise(Substitute.For<IDomainEvent>());
        entity.Raise(Substitute.For<IDomainEvent>());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyCollection()
    {
        var entity = new TestEntity();

        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }
}
