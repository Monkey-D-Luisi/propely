// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.Events;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.UnitTests.Properties;

public class PropertyTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsFieldsAndRaisesEvent()
    {
        var property = Property.Create(
            "Luxury Apartment in Malaga",
            PropertyType.Apartment,
            OperationType.Sale,
            TenantId,
            AgentId);

        property.Id.Should().NotBeEmpty();
        property.Title.Should().Be("Luxury Apartment in Malaga");
        property.PropertyType.Should().Be(PropertyType.Apartment);
        property.OperationType.Should().Be(OperationType.Sale);
        property.Status.Should().Be(PropertyStatus.Draft);
        property.TenantId.Should().Be(TenantId);
        property.AgentId.Should().Be(AgentId);
        property.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        property.IsDeleted.Should().BeFalse();
        property.PublishedAtUtc.Should().BeNull();

        property.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PropertyCreatedV1>();

        var evt = (PropertyCreatedV1)property.DomainEvents.First();
        evt.Data.PropertyId.Should().Be(property.Id);
        evt.Data.TenantId.Should().Be(TenantId);
        evt.Data.AgentId.Should().Be(AgentId);
        evt.Data.Title.Should().Be("Luxury Apartment in Malaga");
        evt.Data.PropertyType.Should().Be(PropertyType.Apartment);
        evt.Data.OperationType.Should().Be(OperationType.Sale);
    }

    [Fact]
    public void Create_WithOptionalFields_SetsAllOptionalFields()
    {
        var description = LocalizedText.Create(es: "Descripci\u00F3n", en: "Description");
        var address = Address.Create(city: "Malaga", country: "ES");
        var features = PropertyFeatures.Create(bedrooms: 3, bathrooms: 2, builtArea: 120);
        var financials = PropertyFinancials.Create(price: 250000m);
        var agencyId = Guid.NewGuid();

        var property = Property.Create(
            "Test",
            PropertyType.Villa,
            OperationType.Rent,
            TenantId,
            AgentId,
            agencyId: agencyId,
            description: description,
            address: address,
            features: features,
            financials: financials,
            virtualTourUrl: "https://tour.example.com",
            videoUrl: "https://video.example.com");

        property.AgencyId.Should().Be(agencyId);
        property.Description.Should().Be(description);
        property.Address.Should().Be(address);
        property.Features.Should().Be(features);
        property.Financials.Should().Be(financials);
        property.VirtualTourUrl.Should().Be("https://tour.example.com");
        property.VideoUrl.Should().Be("https://video.example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ThrowsDomainException(string? title)
    {
        var act = () => Property.Create(title!, PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        act.Should().Throw<DomainException>().WithMessage("*title*required*");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ThrowsDomainException()
    {
        var longTitle = new string('A', 201);
        var act = () => Property.Create(longTitle, PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        act.Should().Throw<DomainException>().WithMessage("*title*200*");
    }

    [Fact]
    public void Create_TrimsTitle()
    {
        var property = Property.Create("  Apartment  ", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        property.Title.Should().Be("Apartment");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsDomainException()
    {
        var act = () => Property.Create("Test", PropertyType.Apartment, OperationType.Sale, Guid.Empty, AgentId);
        act.Should().Throw<DomainException>().WithMessage("*Tenant*required*");
    }

    [Fact]
    public void Create_WithEmptyAgentId_ThrowsDomainException()
    {
        var act = () => Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Agent*required*");
    }

    [Fact]
    public void Update_ChangesTitle_RaisesEvent()
    {
        var property = Property.Create("Original", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        property.ClearDomainEvents();

        property.Update(title: "Updated Title");

        property.Title.Should().Be("Updated Title");
        property.UpdatedAtUtc.Should().NotBeNull();
        property.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PropertyUpdatedV1>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyTitle_ThrowsDomainException(string title)
    {
        var property = Property.Create("Original", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        var act = () => property.Update(title: title);
        act.Should().Throw<DomainException>().WithMessage("*title*required*");
    }

    [Fact]
    public void Update_WithNullTitle_DoesNotChangeTitle()
    {
        var property = Property.Create("Original", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        property.ClearDomainEvents();

        property.Update(title: null, propertyType: PropertyType.Villa);

        property.Title.Should().Be("Original");
        property.PropertyType.Should().Be(PropertyType.Villa);
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedAndRaisesEvent()
    {
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        property.ClearDomainEvents();

        property.SoftDelete();

        property.IsDeleted.Should().BeTrue();
        property.DeletedAtUtc.Should().NotBeNull();
        property.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PropertyDeletedV1>();
    }

    [Fact]
    public void AssignAgent_ChangesAgentId()
    {
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        var newAgentId = Guid.NewGuid();

        property.AssignAgent(newAgentId);

        property.AgentId.Should().Be(newAgentId);
        property.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void AssignAgent_WithEmptyId_ThrowsDomainException()
    {
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        var act = () => property.AssignAgent(Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Agent*required*");
    }

    [Fact]
    public void PricePerSqm_WithValidValues_ReturnsCorrectCalculation()
    {
        var financials = PropertyFinancials.Create(price: 300000m);
        var features = PropertyFeatures.Create(builtArea: 120m);
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId,
            features: features, financials: financials);

        property.PricePerSqm.Should().Be(2500m);
    }

    [Fact]
    public void PricePerSqm_WithZeroPrice_ReturnsNull()
    {
        var financials = PropertyFinancials.Create(price: 0m);
        var features = PropertyFeatures.Create(builtArea: 120m);
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId,
            features: features, financials: financials);

        property.PricePerSqm.Should().BeNull();
    }

    [Fact]
    public void PricePerSqm_WithZeroArea_ReturnsNull()
    {
        var financials = PropertyFinancials.Create(price: 300000m);
        var features = PropertyFeatures.Create(builtArea: 0m);
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId,
            features: features, financials: financials);

        property.PricePerSqm.Should().BeNull();
    }

    [Fact]
    public void PricePerSqm_WithNoFinancials_ReturnsNull()
    {
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId);
        property.PricePerSqm.Should().BeNull();
    }

    [Fact]
    public void PricePerSqm_WithNoFeatures_ReturnsNull()
    {
        var financials = PropertyFinancials.Create(price: 300000m);
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, TenantId, AgentId,
            financials: financials);

        property.PricePerSqm.Should().BeNull();
    }
}
