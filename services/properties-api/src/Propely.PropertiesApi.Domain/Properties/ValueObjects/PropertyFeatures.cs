// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Domain.Properties.ValueObjects;

public sealed class PropertyFeatures
{
    public int? Bedrooms { get; private set; }
    public int? Bathrooms { get; private set; }
    public decimal? BuiltArea { get; private set; }
    public decimal? UsableArea { get; private set; }
    public decimal? PlotArea { get; private set; }
    public int? Floor { get; private set; }
    public Orientation? Orientation { get; private set; }
    public int? YearBuilt { get; private set; }
    public EnergyRating? EnergyRating { get; private set; }
    public decimal? EnergyConsumption { get; private set; }
    public decimal? EnergyEmissions { get; private set; }
    public bool HasPool { get; private set; }
    public bool HasGarden { get; private set; }
    public bool HasGarage { get; private set; }
    public bool HasElevator { get; private set; }
    public bool HasTerrace { get; private set; }
    public bool AirConditioning { get; private set; }
    public bool Heating { get; private set; }
    public bool Furnished { get; private set; }
    public int? ParkingSpaces { get; private set; }

    private PropertyFeatures() { }

    public static PropertyFeatures Create(
        int? bedrooms = null,
        int? bathrooms = null,
        decimal? builtArea = null,
        decimal? usableArea = null,
        decimal? plotArea = null,
        int? floor = null,
        Orientation? orientation = null,
        int? yearBuilt = null,
        EnergyRating? energyRating = null,
        decimal? energyConsumption = null,
        decimal? energyEmissions = null,
        bool hasPool = false,
        bool hasGarden = false,
        bool hasGarage = false,
        bool hasElevator = false,
        bool hasTerrace = false,
        bool airConditioning = false,
        bool heating = false,
        bool furnished = false,
        int? parkingSpaces = null)
    {
        if (bedrooms.HasValue && bedrooms.Value < 0)
            throw new DomainException("Bedrooms cannot be negative.");

        if (bathrooms.HasValue && bathrooms.Value < 0)
            throw new DomainException("Bathrooms cannot be negative.");

        if (builtArea.HasValue && builtArea.Value < 0)
            throw new DomainException("Built area cannot be negative.");

        if (usableArea.HasValue && usableArea.Value < 0)
            throw new DomainException("Usable area cannot be negative.");

        if (plotArea.HasValue && plotArea.Value < 0)
            throw new DomainException("Plot area cannot be negative.");

        if (parkingSpaces.HasValue && parkingSpaces.Value < 0)
            throw new DomainException("Parking spaces cannot be negative.");

        if (energyConsumption.HasValue && energyConsumption.Value < 0)
            throw new DomainException("Energy consumption cannot be negative.");

        if (energyEmissions.HasValue && energyEmissions.Value < 0)
            throw new DomainException("Energy emissions cannot be negative.");

        return new PropertyFeatures
        {
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            BuiltArea = builtArea,
            UsableArea = usableArea,
            PlotArea = plotArea,
            Floor = floor,
            Orientation = orientation,
            YearBuilt = yearBuilt,
            EnergyRating = energyRating,
            EnergyConsumption = energyConsumption,
            EnergyEmissions = energyEmissions,
            HasPool = hasPool,
            HasGarden = hasGarden,
            HasGarage = hasGarage,
            HasElevator = hasElevator,
            HasTerrace = hasTerrace,
            AirConditioning = airConditioning,
            Heating = heating,
            Furnished = furnished,
            ParkingSpaces = parkingSpaces
        };
    }
}
