// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.UploadMedia;

public sealed class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, UploadMediaResult>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyMediaRepository _mediaRepository;
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadMediaCommandHandler(
        IPropertyRepository propertyRepository,
        IPropertyMediaRepository mediaRepository,
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadMediaResult> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Property with ID '{request.PropertyId}' was not found.");

        if (request.MediaType == MediaType.Photo)
        {
            var count = await _mediaRepository.CountByPropertyIdAsync(property.Id, request.TenantId, cancellationToken);
            if (count >= PropertyMedia.MaxPhotosPerProperty)
                throw new DomainException($"Maximum of {PropertyMedia.MaxPhotosPerProperty} photos per property exceeded.");
        }

        var currentCount = await _mediaRepository.CountByPropertyIdAsync(property.Id, request.TenantId, cancellationToken);

        var media = PropertyMedia.Create(
            propertyId: property.Id,
            tenantId: request.TenantId,
            mediaType: request.MediaType,
            fileName: request.FileName,
            contentType: request.ContentType,
            sizeBytes: request.SizeBytes,
            displayOrder: currentCount);

        // Upload main file (resize photos)
        if (request.MediaType == MediaType.Photo && request.ContentType != "application/pdf")
        {
            var (resized, width, height) = await _imageProcessingService.ResizeAsync(
                request.FileStream, PropertyMedia.MaxWidthPixels, cancellationToken);
            await using (resized)
            {
                await _storageService.UploadAsync(media.StoragePath, resized, request.ContentType, cancellationToken);
            }

            // Generate thumbnail
            request.FileStream.Position = 0;
            var (thumbnail, _, _) = await _imageProcessingService.ResizeAsync(
                request.FileStream, PropertyMedia.ThumbnailWidthPixels, cancellationToken);
            await using (thumbnail)
            {
                if (media.ThumbnailPath is not null)
                    await _storageService.UploadAsync(media.ThumbnailPath, thumbnail, request.ContentType, cancellationToken);
            }
        }
        else
        {
            await _storageService.UploadAsync(media.StoragePath, request.FileStream, request.ContentType, cancellationToken);
        }

        await _mediaRepository.AddAsync(media, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadMediaResult(media.Id, media.StoragePath, media.ThumbnailPath);
    }
}
