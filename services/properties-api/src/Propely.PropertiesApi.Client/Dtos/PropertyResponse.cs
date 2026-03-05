// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Client.Dtos;

/// <summary>
/// Full property details returned by the Properties API.
/// </summary>
public sealed record PropertyResponse
{
    /// <summary>The unique property identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The property title.</summary>
    public string Title { get; init; } = null!;

    /// <summary>The property type (e.g., Apartment, House, Villa).</summary>
    public string PropertyType { get; init; } = null!;

    /// <summary>The operation type (e.g., Sale, Rent).</summary>
    public string OperationType { get; init; } = null!;

    /// <summary>The current property status (e.g., Draft, Active, Sold).</summary>
    public string Status { get; init; } = null!;

    /// <summary>The tenant (organization) ID.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The assigned agent's user ID.</summary>
    public Guid AgentId { get; init; }

    /// <summary>The optional agency ID.</summary>
    public Guid? AgencyId { get; init; }

    /// <summary>Localized property description.</summary>
    public LocalizedTextResponse? Description { get; init; }

    /// <summary>Property address details.</summary>
    public AddressResponse? Address { get; init; }

    /// <summary>Property features and characteristics.</summary>
    public PropertyFeaturesResponse? Features { get; init; }

    /// <summary>Property financial information.</summary>
    public PropertyFinancialsResponse? Financials { get; init; }

    /// <summary>URL to a virtual tour.</summary>
    public string? VirtualTourUrl { get; init; }

    /// <summary>URL to a video.</summary>
    public string? VideoUrl { get; init; }

    /// <summary>Computed price per square meter.</summary>
    public decimal? PricePerSqm { get; init; }

    /// <summary>Date the property was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Date the property was last updated (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Date the property was published (UTC).</summary>
    public DateTime? PublishedAtUtc { get; init; }
}

/// <summary>
/// Localized text with support for multiple languages.
/// </summary>
public sealed record LocalizedTextResponse
{
    /// <summary>Spanish text.</summary>
    public string? Es { get; init; }

    /// <summary>Portuguese text.</summary>
    public string? Pt { get; init; }

    /// <summary>English text.</summary>
    public string? En { get; init; }

    /// <summary>French text.</summary>
    public string? Fr { get; init; }

    /// <summary>German text.</summary>
    public string? De { get; init; }

    /// <summary>Dutch text.</summary>
    public string? Nl { get; init; }
}

/// <summary>
/// Property address information.
/// </summary>
public sealed record AddressResponse
{
    /// <summary>Street name and number.</summary>
    public string? Street { get; init; }

    /// <summary>City name.</summary>
    public string? City { get; init; }

    /// <summary>Province or state.</summary>
    public string? Province { get; init; }

    /// <summary>Postal or ZIP code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Country name.</summary>
    public string? Country { get; init; }

    /// <summary>Province code (e.g., INE code).</summary>
    public string? ProvinceCode { get; init; }

    /// <summary>Municipality code (e.g., INE code).</summary>
    public string? MunicipalityCode { get; init; }

    /// <summary>GPS latitude.</summary>
    public double? Latitude { get; init; }

    /// <summary>GPS longitude.</summary>
    public double? Longitude { get; init; }
}

/// <summary>
/// Property features and physical characteristics.
/// </summary>
public sealed record PropertyFeaturesResponse
{
    /// <summary>Number of bedrooms.</summary>
    public int? Bedrooms { get; init; }

    /// <summary>Number of bathrooms.</summary>
    public int? Bathrooms { get; init; }

    /// <summary>Built area in square meters.</summary>
    public decimal? BuiltArea { get; init; }

    /// <summary>Usable area in square meters.</summary>
    public decimal? UsableArea { get; init; }

    /// <summary>Plot area in square meters.</summary>
    public decimal? PlotArea { get; init; }

    /// <summary>Floor number.</summary>
    public int? Floor { get; init; }

    /// <summary>Property orientation (e.g., N, NE, S).</summary>
    public string? Orientation { get; init; }

    /// <summary>Year the property was built.</summary>
    public int? YearBuilt { get; init; }

    /// <summary>Energy efficiency rating (e.g., A through G).</summary>
    public string? EnergyRating { get; init; }

    /// <summary>Energy consumption in kWh/m²/year.</summary>
    public decimal? EnergyConsumption { get; init; }

    /// <summary>Energy emissions in kgCO₂/m²/year.</summary>
    public decimal? EnergyEmissions { get; init; }

    /// <summary>Whether the property has a pool.</summary>
    public bool HasPool { get; init; }

    /// <summary>Whether the property has a garden.</summary>
    public bool HasGarden { get; init; }

    /// <summary>Whether the property has a garage.</summary>
    public bool HasGarage { get; init; }

    /// <summary>Whether the property has an elevator.</summary>
    public bool HasElevator { get; init; }

    /// <summary>Whether the property has a terrace.</summary>
    public bool HasTerrace { get; init; }

    /// <summary>Whether the property has air conditioning.</summary>
    public bool AirConditioning { get; init; }

    /// <summary>Whether the property has heating.</summary>
    public bool Heating { get; init; }

    /// <summary>Whether the property comes furnished.</summary>
    public bool Furnished { get; init; }

    /// <summary>Number of parking spaces.</summary>
    public int? ParkingSpaces { get; init; }
}

/// <summary>
/// Property financial information.
/// </summary>
public sealed record PropertyFinancialsResponse
{
    /// <summary>Property price in euros.</summary>
    public decimal? Price { get; init; }

    /// <summary>Monthly community fees in euros.</summary>
    public decimal? CommunityFees { get; init; }

    /// <summary>Annual IBI tax in euros.</summary>
    public decimal? IbiTax { get; init; }

    /// <summary>Catastro cadastral reference number.</summary>
    public string? CatastroReference { get; init; }
}
