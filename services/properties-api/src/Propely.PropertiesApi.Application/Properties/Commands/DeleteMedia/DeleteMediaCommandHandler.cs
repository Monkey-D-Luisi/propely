// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Application.Properties.Commands.DeleteMedia;

public sealed class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand>
{
    private readonly IPropertyMediaRepository _mediaRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMediaCommandHandler(
        IPropertyMediaRepository mediaRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByIdAsync(request.MediaId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Media with ID '{request.MediaId}' was not found.");

        if (media.PropertyId != request.PropertyId)
            throw new NotFoundException($"Media with ID '{request.MediaId}' does not belong to property '{request.PropertyId}'.");

        // Delete from storage
        await _storageService.DeleteAsync(media.StoragePath, cancellationToken);
        if (media.ThumbnailPath is not null)
            await _storageService.DeleteAsync(media.ThumbnailPath, cancellationToken);

        _mediaRepository.Delete(media);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
