// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.ReorderMedia;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class ReorderMediaCommandHandlerTests
{
    private readonly IPropertyMediaRepository _mediaRepository = Substitute.For<IPropertyMediaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReorderMediaCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public ReorderMediaCommandHandlerTests()
    {
        _handler = new ReorderMediaCommandHandler(_mediaRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidReorder_UpdatesDisplayOrders()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "a.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);
        var media2 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "b.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 1);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1, media2 });

        var command = new ReorderMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            Items =
            [
                new MediaOrderItem(media1.Id, 1),
                new MediaOrderItem(media2.Id, 0)
            ]
        };

        await _handler.Handle(command, CancellationToken.None);

        media1.DisplayOrder.Should().Be(1);
        media2.DisplayOrder.Should().Be(0);
        _mediaRepository.Received(2).Update(Arg.Any<PropertyMedia>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CountMismatch_ThrowsDomainException()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "a.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1 });

        var command = new ReorderMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            Items = [] // No items but 1 media exists
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*must include all*");
    }

    [Fact]
    public async Task Handle_UnknownMediaId_ThrowsDomainException()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "a.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1 });

        var command = new ReorderMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            Items = [new MediaOrderItem(Guid.NewGuid(), 0)] // Unknown ID
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*must include all*");
    }

    [Fact]
    public async Task Handle_DuplicateDisplayOrders_ThrowsDomainException()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "a.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);
        var media2 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "b.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 1);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1, media2 });

        var command = new ReorderMediaCommand
        {
            PropertyId = PropertyId,
            TenantId = TenantId,
            Items =
            [
                new MediaOrderItem(media1.Id, 0),
                new MediaOrderItem(media2.Id, 0) // Duplicate order
            ]
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*unique*");
    }
}
