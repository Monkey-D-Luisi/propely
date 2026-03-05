// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;

namespace Propely.PropertiesApi.Application.Properties.Queries.ListPropertyMedia;

public sealed record ListPropertyMediaQuery(Guid PropertyId, Guid TenantId) : IRequest<IReadOnlyList<PropertyMediaDto>>;

public sealed class ListPropertyMediaQueryHandler : IRequestHandler<ListPropertyMediaQuery, IReadOnlyList<PropertyMediaDto>>
{
    private readonly IPropertyMediaRepository _mediaRepository;
    private readonly IStorageService _storageService;

    public ListPropertyMediaQueryHandler(
        IPropertyMediaRepository mediaRepository,
        IStorageService storageService)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
    }

    public async Task<IReadOnlyList<PropertyMediaDto>> Handle(ListPropertyMediaQuery request, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByPropertyIdAsync(request.PropertyId, request.TenantId, cancellationToken);
        var expiry = TimeSpan.FromHours(1);

        var dtos = new List<PropertyMediaDto>();
        foreach (var item in media.OrderBy(m => m.DisplayOrder))
        {
            var url = await _storageService.GenerateSignedUrlAsync(item.StoragePath, expiry, cancellationToken);
            string? thumbnailUrl = null;
            if (item.ThumbnailPath is not null)
                thumbnailUrl = await _storageService.GenerateSignedUrlAsync(item.ThumbnailPath, expiry, cancellationToken);

            dtos.Add(new PropertyMediaDto
            {
                Id = item.Id,
                PropertyId = item.PropertyId,
                MediaType = item.MediaType,
                FileName = item.FileName,
                ContentType = item.ContentType,
                SizeBytes = item.SizeBytes,
                Width = item.Width,
                Height = item.Height,
                DisplayOrder = item.DisplayOrder,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
                UploadedAtUtc = item.UploadedAtUtc
            });
        }

        return dtos;
    }
}
