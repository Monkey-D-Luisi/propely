// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.UploadMedia;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class UploadMediaCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository = Substitute.For<IPropertyRepository>();
    private readonly IPropertyMediaRepository _mediaRepository = Substitute.For<IPropertyMediaRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IImageProcessingService _imageProcessingService = Substitute.For<IImageProcessingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UploadMediaCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public UploadMediaCommandHandlerTests()
    {
        _handler = new UploadMediaCommandHandler(
            _propertyRepository, _mediaRepository, _storageService,
            _imageProcessingService, _unitOfWork);
    }

    private static Property CreateTestProperty()
    {
        return Property.Create(
            title: "Test Property",
            propertyType: PropertyType.Apartment,
            operationType: OperationType.Sale,
            tenantId: TenantId,
            agentId: Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_PhotoUpload_ResizesAndUploadWithThumbnail()
    {
        var property = CreateTestProperty();
        _propertyRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(property);
        _mediaRepository.CountByPropertyIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(0);

        var resizedStream = new MemoryStream([1, 2, 3]);
        var thumbnailStream = new MemoryStream([4, 5]);
        _imageProcessingService.ResizeAsync(Arg.Any<Stream>(), PropertyMedia.MaxWidthPixels, Arg.Any<CancellationToken>())
            .Returns((resizedStream, 2048, 1536));
        _imageProcessingService.ResizeAsync(Arg.Any<Stream>(), PropertyMedia.ThumbnailWidthPixels, Arg.Any<CancellationToken>())
            .Returns((thumbnailStream, 400, 300));

        var command = new UploadMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            MediaType = MediaType.Photo,
            FileName = "photo.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 1024,
            FileStream = new MemoryStream([1, 2, 3])
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.MediaId.Should().NotBeEmpty();
        result.StoragePath.Should().NotBeNullOrEmpty();
        result.ThumbnailPath.Should().NotBeNullOrEmpty();
        await _storageService.Received(2).UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), "image/jpeg", Arg.Any<CancellationToken>());
        await _mediaRepository.Received(1).AddAsync(Arg.Any<PropertyMedia>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FloorPlanUpload_UploadsWithoutResize()
    {
        var property = CreateTestProperty();
        _propertyRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(property);
        _mediaRepository.CountByPropertyIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(0);
        _mediaRepository.CountByPropertyIdAndMediaTypeAsync(Arg.Any<Guid>(), TenantId, MediaType.FloorPlan, Arg.Any<CancellationToken>())
            .Returns(0);

        var command = new UploadMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            MediaType = MediaType.FloorPlan,
            FileName = "floorplan.pdf",
            ContentType = "application/pdf",
            SizeBytes = 2048,
            FileStream = new MemoryStream([1, 2, 3])
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ThumbnailPath.Should().BeNull();
        await _storageService.Received(1).UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), "application/pdf", Arg.Any<CancellationToken>());
        await _imageProcessingService.DidNotReceive().ResizeAsync(Arg.Any<Stream>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ThrowsNotFoundException()
    {
        _propertyRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var command = new UploadMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            MediaType = MediaType.Photo,
            FileName = "photo.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 1024,
            FileStream = new MemoryStream([1, 2, 3])
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MaxPhotosExceeded_ThrowsDomainException()
    {
        var property = CreateTestProperty();
        _propertyRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(property);
        _mediaRepository.CountByPropertyIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(PropertyMedia.MaxPhotosPerProperty);

        var command = new UploadMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            MediaType = MediaType.Photo,
            FileName = "photo.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 1024,
            FileStream = new MemoryStream([1, 2, 3])
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Maximum*photos*");
    }

    [Fact]
    public async Task Handle_MaxFloorPlansExceeded_ThrowsDomainException()
    {
        var property = CreateTestProperty();
        _propertyRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(property);
        _mediaRepository.CountByPropertyIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(0);
        _mediaRepository.CountByPropertyIdAndMediaTypeAsync(Arg.Any<Guid>(), TenantId, MediaType.FloorPlan, Arg.Any<CancellationToken>())
            .Returns(PropertyMedia.MaxFloorPlansPerProperty);

        var command = new UploadMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            MediaType = MediaType.FloorPlan,
            FileName = "floorplan.pdf",
            ContentType = "application/pdf",
            SizeBytes = 2048,
            FileStream = new MemoryStream([1, 2, 3])
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Maximum*floor plans*");
    }
}
