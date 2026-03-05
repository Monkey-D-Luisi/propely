// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Dtos;

public sealed record PropertyListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public PropertyType PropertyType { get; init; }
    public OperationType OperationType { get; init; }
    public PropertyStatus Status { get; init; }
    public decimal? Price { get; init; }
    public string? City { get; init; }
    public decimal? BuiltArea { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public Guid AgentId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
