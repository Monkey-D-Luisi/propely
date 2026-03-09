// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Client.Dtos;

/// <summary>
/// Request DTO for creating a new property via the Properties API.
/// </summary>
public sealed record CreatePropertyRequest
{
    /// <summary>Title of the property listing.</summary>
    public string Title { get; init; } = null!;

    /// <summary>Property type as string enum (e.g., "Apartment", "House", "Villa").</summary>
    public string PropertyType { get; init; } = null!;

    /// <summary>Operation type as string enum (e.g., "Sale", "Rent", "Transfer").</summary>
    public string OperationType { get; init; } = null!;

    /// <summary>Optional agency ID.</summary>
    public Guid? AgencyId { get; init; }

    /// <summary>Localized description.</summary>
    public CreateLocalizedTextRequest? Description { get; init; }

    /// <summary>Property address.</summary>
    public CreateAddressRequest? Address { get; init; }

    /// <summary>Property features.</summary>
    public CreatePropertyFeaturesRequest? Features { get; init; }

    /// <summary>Property financial information.</summary>
    public CreatePropertyFinancialsRequest? Financials { get; init; }
}

/// <summary>Localized text for property creation.</summary>
public sealed record CreateLocalizedTextRequest
{
    public string? Es { get; init; }
    public string? En { get; init; }
}

/// <summary>Address for property creation.</summary>
public sealed record CreateAddressRequest
{
    public string? City { get; init; }
}

/// <summary>Features for property creation.</summary>
public sealed record CreatePropertyFeaturesRequest
{
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? BuiltArea { get; init; }
}

/// <summary>Financials for property creation.</summary>
public sealed record CreatePropertyFinancialsRequest
{
    public decimal? Price { get; init; }
}
