// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Application.Properties.Queries.ListPropertyMedia;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Queries;

public class ListPropertyMediaQueryHandlerTests
{
    private readonly IPropertyMediaRepository _mediaRepository = Substitute.For<IPropertyMediaRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly ListPropertyMediaQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public ListPropertyMediaQueryHandlerTests()
    {
        _handler = new ListPropertyMediaQueryHandler(_mediaRepository, _storageService);
    }

    [Fact]
    public async Task Handle_ReturnsMediaWithSignedUrls()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "photo.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);
        var media2 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.FloorPlan, fileName: "floor.pdf",
            contentType: "application/pdf", sizeBytes: 2048, displayOrder: 1);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1, media2 });

        _storageService.GenerateSignedUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => $"https://signed/{callInfo.Arg<string>()}");

        var query = new ListPropertyMediaQuery(PropertyId, TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].FileName.Should().Be("photo.jpg");
        result[0].Url.Should().StartWith("https://signed/");
        result[0].ThumbnailUrl.Should().NotBeNull();
        result[1].FileName.Should().Be("floor.pdf");
        result[1].ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia>());

        var query = new ListPropertyMediaQuery(PropertyId, TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_OrdersByDisplayOrder()
    {
        var media1 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "second.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 1);
        var media2 = PropertyMedia.Create(
            propertyId: PropertyId, tenantId: TenantId,
            mediaType: MediaType.Photo, fileName: "first.jpg",
            contentType: "image/jpeg", sizeBytes: 1024, displayOrder: 0);

        _mediaRepository.GetByPropertyIdAsync(PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PropertyMedia> { media1, media2 });

        _storageService.GenerateSignedUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns("https://signed/url");

        var query = new ListPropertyMediaQuery(PropertyId, TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result[0].FileName.Should().Be("first.jpg");
        result[1].FileName.Should().Be("second.jpg");
    }
}
