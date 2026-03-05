// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Dtos;

public sealed record PropertyDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public PropertyType PropertyType { get; init; }
    public OperationType OperationType { get; init; }
    public PropertyStatus Status { get; init; }
    public Guid TenantId { get; init; }
    public Guid AgentId { get; init; }
    public Guid? AgencyId { get; init; }
    public LocalizedTextDto? Description { get; init; }
    public AddressDto? Address { get; init; }
    public PropertyFeaturesDto? Features { get; init; }
    public PropertyFinancialsDto? Financials { get; init; }
    public string? VirtualTourUrl { get; init; }
    public string? VideoUrl { get; init; }
    public decimal? PricePerSqm { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
}

public sealed record LocalizedTextDto
{
    public string? Es { get; init; }
    public string? Pt { get; init; }
    public string? En { get; init; }
    public string? Fr { get; init; }
    public string? De { get; init; }
    public string? Nl { get; init; }
}

public sealed record AddressDto
{
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? Province { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? ProvinceCode { get; init; }
    public string? MunicipalityCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}

public sealed record PropertyFeaturesDto
{
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? BuiltArea { get; init; }
    public decimal? UsableArea { get; init; }
    public decimal? PlotArea { get; init; }
    public int? Floor { get; init; }
    public Orientation? Orientation { get; init; }
    public int? YearBuilt { get; init; }
    public EnergyRating? EnergyRating { get; init; }
    public decimal? EnergyConsumption { get; init; }
    public decimal? EnergyEmissions { get; init; }
    public bool HasPool { get; init; }
    public bool HasGarden { get; init; }
    public bool HasGarage { get; init; }
    public bool HasElevator { get; init; }
    public bool HasTerrace { get; init; }
    public bool AirConditioning { get; init; }
    public bool Heating { get; init; }
    public bool Furnished { get; init; }
    public int? ParkingSpaces { get; init; }
}

public sealed record PropertyFinancialsDto
{
    public decimal? Price { get; init; }
    public decimal? CommunityFees { get; init; }
    public decimal? IbiTax { get; init; }
    public string? CatastroReference { get; init; }
}
