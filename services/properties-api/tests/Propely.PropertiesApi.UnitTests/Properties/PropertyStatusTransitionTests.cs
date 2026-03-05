// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.Events;

namespace Propely.PropertiesApi.UnitTests.Properties;

public class PropertyStatusTransitionTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    private static Property CreateProperty(PropertyStatus initialStatus = PropertyStatus.Draft)
    {
        var property = Property.Create("Test Property", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        if (initialStatus != PropertyStatus.Draft)
        {
            // Walk through valid transitions to reach the desired status
            var path = GetTransitionPath(PropertyStatus.Draft, initialStatus);
            foreach (var status in path)
                property.ChangeStatus(status);
        }

        property.ClearDomainEvents();
        return property;
    }

    private static List<PropertyStatus> GetTransitionPath(PropertyStatus from, PropertyStatus to)
    {
        return (from, to) switch
        {
            (PropertyStatus.Draft, PropertyStatus.Active) => [PropertyStatus.Active],
            (PropertyStatus.Draft, PropertyStatus.Reserved) => [PropertyStatus.Active, PropertyStatus.Reserved],
            (PropertyStatus.Draft, PropertyStatus.Sold) => [PropertyStatus.Active, PropertyStatus.Sold],
            (PropertyStatus.Draft, PropertyStatus.Rented) => [PropertyStatus.Active, PropertyStatus.Rented],
            (PropertyStatus.Draft, PropertyStatus.Archived) => [PropertyStatus.Archived],
            _ => throw new InvalidOperationException($"No test path from {from} to {to}")
        };
    }

    // Valid transitions from Draft
    [Fact]
    public void Draft_To_Active_Succeeds()
    {
        var property = CreateProperty(PropertyStatus.Draft);
        property.ChangeStatus(PropertyStatus.Active);
        property.Status.Should().Be(PropertyStatus.Active);
    }

    [Fact]
    public void Draft_To_Archived_Succeeds()
    {
        var property = CreateProperty(PropertyStatus.Draft);
        property.ChangeStatus(PropertyStatus.Archived);
        property.Status.Should().Be(PropertyStatus.Archived);
    }

    // Valid transitions from Active
    [Theory]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Archived)]
    public void Active_To_ValidStatus_Succeeds(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Active);
        property.ChangeStatus(newStatus);
        property.Status.Should().Be(newStatus);
    }

    // Valid transitions from Reserved
    [Theory]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Archived)]
    public void Reserved_To_ValidStatus_Succeeds(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Reserved);
        property.ChangeStatus(newStatus);
        property.Status.Should().Be(newStatus);
    }

    // Valid transition from Archived
    [Fact]
    public void Archived_To_Draft_Succeeds()
    {
        var property = CreateProperty(PropertyStatus.Archived);
        property.ChangeStatus(PropertyStatus.Draft);
        property.Status.Should().Be(PropertyStatus.Draft);
    }

    // Terminal states - no transitions allowed from Sold/Rented
    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Archived)]
    public void Sold_To_Any_ThrowsDomainException(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Sold);
        var act = () => property.ChangeStatus(newStatus);
        act.Should().Throw<DomainException>().WithMessage("*Cannot transition*Sold*");
    }

    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Archived)]
    public void Rented_To_Any_ThrowsDomainException(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Rented);
        var act = () => property.ChangeStatus(newStatus);
        act.Should().Throw<DomainException>().WithMessage("*Cannot transition*Rented*");
    }

    // Invalid transitions from Draft
    [Theory]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Rented)]
    public void Draft_To_InvalidStatus_ThrowsDomainException(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Draft);
        var act = () => property.ChangeStatus(newStatus);
        act.Should().Throw<DomainException>().WithMessage("*Cannot transition*Draft*");
    }

    // Invalid transitions from Active
    [Fact]
    public void Active_To_Draft_ThrowsDomainException()
    {
        var property = CreateProperty(PropertyStatus.Active);
        var act = () => property.ChangeStatus(PropertyStatus.Draft);
        act.Should().Throw<DomainException>().WithMessage("*Cannot transition*Active*");
    }

    // Invalid transitions from Archived
    [Theory]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Rented)]
    public void Archived_To_InvalidStatus_ThrowsDomainException(PropertyStatus newStatus)
    {
        var property = CreateProperty(PropertyStatus.Archived);
        var act = () => property.ChangeStatus(newStatus);
        act.Should().Throw<DomainException>().WithMessage("*Cannot transition*Archived*");
    }

    // Same status transition
    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Reserved)]
    public void SameStatus_ThrowsDomainException(PropertyStatus status)
    {
        var property = CreateProperty(status);
        var act = () => property.ChangeStatus(status);
        act.Should().Throw<DomainException>().WithMessage("*already*");
    }

    // Raises event on valid transition
    [Fact]
    public void ValidTransition_RaisesPropertyStatusChangedV1()
    {
        var property = CreateProperty(PropertyStatus.Draft);
        property.ChangeStatus(PropertyStatus.Active);

        property.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PropertyStatusChangedV1>();

        var evt = (PropertyStatusChangedV1)property.DomainEvents.First();
        evt.Data.PreviousStatus.Should().Be(PropertyStatus.Draft);
        evt.Data.NewStatus.Should().Be(PropertyStatus.Active);
        evt.Data.PropertyId.Should().Be(property.Id);
        evt.Data.TenantId.Should().Be(TenantId);
    }

    // PublishedAt is set on first activation
    [Fact]
    public void FirstActivation_SetsPublishedAtUtc()
    {
        var property = CreateProperty(PropertyStatus.Draft);
        property.PublishedAtUtc.Should().BeNull();

        property.ChangeStatus(PropertyStatus.Active);

        property.PublishedAtUtc.Should().NotBeNull();
        property.PublishedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SecondActivation_DoesNotUpdatePublishedAtUtc()
    {
        var property = CreateProperty(PropertyStatus.Draft);
        property.ChangeStatus(PropertyStatus.Active);
        var firstPublished = property.PublishedAtUtc;

        property.ChangeStatus(PropertyStatus.Reserved);
        property.ChangeStatus(PropertyStatus.Active);

        property.PublishedAtUtc.Should().Be(firstPublished);
    }
}
