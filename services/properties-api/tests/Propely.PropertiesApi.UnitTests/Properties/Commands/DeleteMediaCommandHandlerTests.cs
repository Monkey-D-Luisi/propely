// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.DeleteMedia;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class DeleteMediaCommandHandlerTests
{
    private readonly IPropertyMediaRepository _mediaRepository = Substitute.For<IPropertyMediaRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteMediaCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public DeleteMediaCommandHandlerTests()
    {
        _handler = new DeleteMediaCommandHandler(_mediaRepository, _storageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidMedia_DeletesFromStorageAndRepository()
    {
        var media = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "photo.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);

        _mediaRepository.GetByIdAsync(media.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(media);

        var command = new DeleteMediaCommand(media.Id, PropertyId, TenantId);

        await _handler.Handle(command, CancellationToken.None);

        await _storageService.Received(1).DeleteAsync(media.StoragePath, Arg.Any<CancellationToken>());
        await _storageService.Received(1).DeleteAsync(media.ThumbnailPath!, Arg.Any<CancellationToken>());
        _mediaRepository.Received(1).Delete(media);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FloorPlanMedia_DeletesWithoutThumbnail()
    {
        var media = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.FloorPlan, fileName: "floorplan.pdf",
            contentType: "application/pdf", sizeBytes: 2048, displayOrder: 0);

        _mediaRepository.GetByIdAsync(media.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(media);

        var command = new DeleteMediaCommand(media.Id, PropertyId, TenantId);

        await _handler.Handle(command, CancellationToken.None);

        await _storageService.Received(1).DeleteAsync(media.StoragePath, Arg.Any<CancellationToken>());
        _mediaRepository.Received(1).Delete(media);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MediaNotFound_ThrowsNotFoundException()
    {
        _mediaRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns((PropertyMedia?)null);

        var command = new DeleteMediaCommand(Guid.NewGuid(), PropertyId, TenantId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MediaDoesNotBelongToProperty_ThrowsNotFoundException()
    {
        var otherPropertyId = Guid.NewGuid();
        var media = PropertyMedia.Create(
            propertyId: otherPropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "photo.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);

        _mediaRepository.GetByIdAsync(media.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(media);

        var command = new DeleteMediaCommand(media.Id, PropertyId, TenantId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*does not belong*");
    }
}
