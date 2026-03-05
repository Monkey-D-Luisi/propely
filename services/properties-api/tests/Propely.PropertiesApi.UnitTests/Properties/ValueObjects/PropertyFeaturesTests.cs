// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.UnitTests.Properties.ValueObjects;

public class PropertyFeaturesTests
{
    [Fact]
    public void Create_WithValidData_SetsAllFields()
    {
        var features = PropertyFeatures.Create(
            bedrooms: 3,
            bathrooms: 2,
            builtArea: 120.5m,
            usableArea: 100m,
            plotArea: 200m,
            floor: 5,
            orientation: Orientation.S,
            yearBuilt: 2020,
            energyRating: EnergyRating.B,
            energyConsumption: 45.5m,
            energyEmissions: 12.3m,
            hasPool: true,
            hasGarden: true,
            hasGarage: false,
            hasElevator: true,
            hasTerrace: true,
            airConditioning: true,
            heating: true,
            furnished: false,
            parkingSpaces: 2);

        features.Bedrooms.Should().Be(3);
        features.Bathrooms.Should().Be(2);
        features.BuiltArea.Should().Be(120.5m);
        features.UsableArea.Should().Be(100m);
        features.PlotArea.Should().Be(200m);
        features.Floor.Should().Be(5);
        features.Orientation.Should().Be(Orientation.S);
        features.YearBuilt.Should().Be(2020);
        features.EnergyRating.Should().Be(EnergyRating.B);
        features.EnergyConsumption.Should().Be(45.5m);
        features.EnergyEmissions.Should().Be(12.3m);
        features.HasPool.Should().BeTrue();
        features.HasGarden.Should().BeTrue();
        features.HasGarage.Should().BeFalse();
        features.HasElevator.Should().BeTrue();
        features.HasTerrace.Should().BeTrue();
        features.AirConditioning.Should().BeTrue();
        features.Heating.Should().BeTrue();
        features.Furnished.Should().BeFalse();
        features.ParkingSpaces.Should().Be(2);
    }

    [Fact]
    public void Create_WithNegativeBedrooms_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(bedrooms: -1);
        act.Should().Throw<DomainException>().WithMessage("*Bedrooms*negative*");
    }

    [Fact]
    public void Create_WithNegativeBathrooms_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(bathrooms: -1);
        act.Should().Throw<DomainException>().WithMessage("*Bathrooms*negative*");
    }

    [Fact]
    public void Create_WithNegativeBuiltArea_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(builtArea: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Built area*negative*");
    }

    [Fact]
    public void Create_WithNegativeUsableArea_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(usableArea: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Usable area*negative*");
    }

    [Fact]
    public void Create_WithNegativePlotArea_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(plotArea: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Plot area*negative*");
    }

    [Fact]
    public void Create_WithNegativeParkingSpaces_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(parkingSpaces: -1);
        act.Should().Throw<DomainException>().WithMessage("*Parking spaces*negative*");
    }

    [Fact]
    public void Create_WithNegativeEnergyConsumption_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(energyConsumption: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Energy consumption*negative*");
    }

    [Fact]
    public void Create_WithNegativeEnergyEmissions_ThrowsDomainException()
    {
        var act = () => PropertyFeatures.Create(energyEmissions: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Energy emissions*negative*");
    }

    [Fact]
    public void Create_WithZeroValues_Succeeds()
    {
        var features = PropertyFeatures.Create(bedrooms: 0, bathrooms: 0, builtArea: 0m, parkingSpaces: 0);
        features.Bedrooms.Should().Be(0);
        features.Bathrooms.Should().Be(0);
        features.BuiltArea.Should().Be(0m);
        features.ParkingSpaces.Should().Be(0);
    }

    [Fact]
    public void Create_WithDefaults_BoolsAreFalse()
    {
        var features = PropertyFeatures.Create();
        features.HasPool.Should().BeFalse();
        features.HasGarden.Should().BeFalse();
        features.HasGarage.Should().BeFalse();
        features.HasElevator.Should().BeFalse();
        features.HasTerrace.Should().BeFalse();
        features.AirConditioning.Should().BeFalse();
        features.Heating.Should().BeFalse();
        features.Furnished.Should().BeFalse();
    }
}
