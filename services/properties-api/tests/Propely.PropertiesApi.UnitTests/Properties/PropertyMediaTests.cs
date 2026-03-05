// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties;

public class PropertyMediaTests
{
    private readonly Guid _propertyId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_ValidPhoto_SetsAllFields()
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "photo.jpg", "image/jpeg", 1024 * 1024, 0);

        media.Id.Should().NotBe(Guid.Empty);
        media.PropertyId.Should().Be(_propertyId);
        media.TenantId.Should().Be(_tenantId);
        media.MediaType.Should().Be(MediaType.Photo);
        media.FileName.Should().Be("photo.jpg");
        media.ContentType.Should().Be("image/jpeg");
        media.SizeBytes.Should().Be(1024 * 1024);
        media.DisplayOrder.Should().Be(0);
        media.StoragePath.Should().Contain(_tenantId.ToString());
        media.ThumbnailPath.Should().NotBeNull();
        media.ThumbnailPath.Should().Contain("thumbnails");
    }

    [Fact]
    public void Create_FloorPlan_HasNoThumbnail()
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.FloorPlan,
            "floor.pdf", "application/pdf", 5 * 1024 * 1024, 0);

        media.ThumbnailPath.Should().BeNull();
    }

    [Fact]
    public void Create_InvalidContentType_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "file.exe", "application/octet-stream", 1024, 0);

        act.Should().Throw<DomainException>().WithMessage("*not allowed*");
    }

    [Fact]
    public void Create_OversizedPhoto_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "huge.jpg", "image/jpeg", 11 * 1024 * 1024, 0);

        act.Should().Throw<DomainException>().WithMessage("*exceeds*");
    }

    [Fact]
    public void Create_OversizedFloorPlan_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.FloorPlan,
            "huge.pdf", "application/pdf", 21 * 1024 * 1024, 0);

        act.Should().Throw<DomainException>().WithMessage("*exceeds*");
    }

    [Fact]
    public void Create_EmptyPropertyId_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            Guid.Empty, _tenantId, MediaType.Photo,
            "photo.jpg", "image/jpeg", 1024, 0);

        act.Should().Throw<DomainException>().WithMessage("*Property ID*");
    }

    [Fact]
    public void Create_EmptyFileName_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "", "image/jpeg", 1024, 0);

        act.Should().Throw<DomainException>().WithMessage("*File name*");
    }

    [Fact]
    public void Create_NegativeDisplayOrder_ThrowsDomainException()
    {
        var act = () => PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "photo.jpg", "image/jpeg", 1024, -1);

        act.Should().Throw<DomainException>().WithMessage("*Display order*negative*");
    }

    [Fact]
    public void UpdateDisplayOrder_ValidOrder_Updates()
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "photo.jpg", "image/jpeg", 1024, 0);

        media.UpdateDisplayOrder(5);
        media.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void UpdateDisplayOrder_NegativeOrder_ThrowsDomainException()
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "photo.jpg", "image/jpeg", 1024, 0);

        var act = () => media.UpdateDisplayOrder(-1);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Create_AllowedPhotoTypes_Succeeds(string contentType)
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.Photo,
            "photo.jpg", contentType, 1024, 0);

        media.ContentType.Should().Be(contentType);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("application/pdf")]
    public void Create_AllowedFloorPlanTypes_Succeeds(string contentType)
    {
        var media = PropertyMedia.Create(
            _propertyId, _tenantId, MediaType.FloorPlan,
            "plan.pdf", contentType, 1024, 0);

        media.ContentType.Should().Be(contentType);
    }
}
