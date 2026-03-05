// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;

public sealed record UpdatePropertyCommand : IRequest<PropertyDto>
{
    public Guid PropertyId { get; init; }
    public Guid TenantId { get; init; }
    public Guid UpdatedBy { get; init; }
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
