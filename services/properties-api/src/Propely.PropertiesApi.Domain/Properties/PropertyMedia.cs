// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common;
using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Domain.Properties;

public sealed class PropertyMedia : Entity
{
    public const int MaxPhotosPerProperty = 30;
    public const int MaxFloorPlansPerProperty = 10;
    public const long MaxPhotoSizeBytes = 10 * 1024 * 1024;
    public const long MaxFloorPlanSizeBytes = 20 * 1024 * 1024;
    public const int MaxWidthPixels = 2048;
    public const int ThumbnailWidthPixels = 400;

    private static readonly HashSet<string> AllowedPhotoTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly HashSet<string> AllowedFloorPlanTypes = ["image/jpeg", "image/png", "application/pdf"];

    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid TenantId { get; private set; }
    public MediaType MediaType { get; private set; }
    public string StoragePath { get; private set; } = null!;
    public string? ThumbnailPath { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }

    private PropertyMedia() { }

    public static PropertyMedia Create(
        Guid propertyId,
        Guid tenantId,
        MediaType mediaType,
        string fileName,
        string contentType,
        long sizeBytes,
        int displayOrder,
        int? width = null,
        int? height = null)
    {
        if (propertyId == Guid.Empty)
            throw new DomainException("Property ID is required.");

        if (tenantId == Guid.Empty)
            throw new DomainException("Tenant ID is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");

        ValidateContentType(mediaType, contentType);
        ValidateFileSize(mediaType, sizeBytes);

        if (displayOrder < 0)
            throw new DomainException("Display order cannot be negative.");

        var id = Guid.NewGuid();
        var storagePath = $"{tenantId}/properties/{propertyId}/{id}_{fileName}";
        var thumbnailPath = mediaType == MediaType.Photo
            ? $"{tenantId}/properties/{propertyId}/thumbnails/{id}_{fileName}"
            : null;

        return new PropertyMedia
        {
            Id = id,
            PropertyId = propertyId,
            TenantId = tenantId,
            MediaType = mediaType,
            StoragePath = storagePath,
            ThumbnailPath = thumbnailPath,
            FileName = fileName.Trim(),
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Width = width,
            Height = height,
            DisplayOrder = displayOrder,
            UploadedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateDisplayOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Display order cannot be negative.");
        DisplayOrder = order;
    }

    private static void ValidateContentType(MediaType mediaType, string contentType)
    {
        var allowed = mediaType == MediaType.Photo ? AllowedPhotoTypes : AllowedFloorPlanTypes;
        if (!allowed.Contains(contentType))
            throw new DomainException($"Content type '{contentType}' is not allowed for {mediaType}.");
    }

    private static void ValidateFileSize(MediaType mediaType, long sizeBytes)
    {
        var maxSize = mediaType == MediaType.Photo ? MaxPhotoSizeBytes : MaxFloorPlanSizeBytes;
        if (sizeBytes <= 0)
            throw new DomainException("File size must be positive.");
        if (sizeBytes > maxSize)
            throw new DomainException($"File exceeds maximum size of {maxSize / (1024 * 1024)} MB for {mediaType}.");
    }
}
