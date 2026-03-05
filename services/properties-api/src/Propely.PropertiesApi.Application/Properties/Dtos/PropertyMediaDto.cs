// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Dtos;

public sealed record PropertyMediaDto
{
    public Guid Id { get; init; }
    public Guid PropertyId { get; init; }
    public MediaType MediaType { get; init; }
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long SizeBytes { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public int DisplayOrder { get; init; }
    public string Url { get; init; } = null!;
    public string? ThumbnailUrl { get; init; }
    public DateTime UploadedAtUtc { get; init; }
}
