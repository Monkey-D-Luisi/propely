// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.UploadMedia;

public sealed record UploadMediaCommand : IRequest<UploadMediaResult>
{
    public Guid PropertyId { get; init; }
    public Guid TenantId { get; init; }
    public MediaType MediaType { get; init; }
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long SizeBytes { get; init; }
    public Stream FileStream { get; init; } = null!;
}

public sealed record UploadMediaResult(Guid MediaId, string StoragePath, string? ThumbnailPath);
