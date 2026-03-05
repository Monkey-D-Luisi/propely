// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;

namespace Propely.PropertiesApi.Application.Properties.Commands.ReorderMedia;

public sealed class ReorderMediaCommandHandler : IRequestHandler<ReorderMediaCommand>
{
    private readonly IPropertyMediaRepository _mediaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderMediaCommandHandler(
        IPropertyMediaRepository mediaRepository,
        IUnitOfWork unitOfWork)
    {
        _mediaRepository = mediaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReorderMediaCommand request, CancellationToken cancellationToken)
    {
        var existingMedia = await _mediaRepository.GetByPropertyIdAsync(request.PropertyId, request.TenantId, cancellationToken);
        var mediaById = existingMedia.ToDictionary(m => m.Id);

        foreach (var item in request.Items)
        {
            if (mediaById.TryGetValue(item.MediaId, out var media))
            {
                media.UpdateDisplayOrder(item.DisplayOrder);
                _mediaRepository.Update(media);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
