// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Api.Dtos;

public sealed record CreatePropertyRequest
{
    public string Title { get; init; } = null!;
    public PropertyType PropertyType { get; init; }
    public OperationType OperationType { get; init; }
    public Guid? AgencyId { get; init; }
    public LocalizedTextDto? Description { get; init; }
    public AddressDto? Address { get; init; }
    public PropertyFeaturesDto? Features { get; init; }
    public PropertyFinancialsDto? Financials { get; init; }
    public string? VirtualTourUrl { get; init; }
    public string? VideoUrl { get; init; }
}

public sealed record UpdatePropertyRequest
{
    public string? Title { get; init; }
    public PropertyType? PropertyType { get; init; }
    public OperationType? OperationType { get; init; }
    public LocalizedTextDto? Description { get; init; }
    public AddressDto? Address { get; init; }
    public PropertyFeaturesDto? Features { get; init; }
    public PropertyFinancialsDto? Financials { get; init; }
    public string? VirtualTourUrl { get; init; }
    public string? VideoUrl { get; init; }
}

public sealed record ChangeStatusRequest
{
    public PropertyStatus Status { get; init; }
}
