// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Api.Dtos;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Api.Dtos;

public class PropertyRequestsTests
{
    [Fact]
    public void CreatePropertyRequest_CanBeInstantiated_WithProperties()
    {
        var request = new CreatePropertyRequest
        {
            Title = "Test",
            PropertyType = PropertyType.Apartment,
            OperationType = OperationType.Sale,
            AgencyId = Guid.NewGuid(),
            VirtualTourUrl = "https://tour.example.com",
            VideoUrl = "https://video.example.com"
        };

        request.Title.Should().Be("Test");
        request.PropertyType.Should().Be(PropertyType.Apartment);
        request.OperationType.Should().Be(OperationType.Sale);
        request.AgencyId.Should().NotBeNull();
        request.Description.Should().BeNull();
        request.Address.Should().BeNull();
        request.Features.Should().BeNull();
        request.Financials.Should().BeNull();
        request.VirtualTourUrl.Should().Be("https://tour.example.com");
        request.VideoUrl.Should().Be("https://video.example.com");
    }

    [Fact]
    public void UpdatePropertyRequest_CanBeInstantiated_WithAllNullProperties()
    {
        var request = new UpdatePropertyRequest();

        request.Title.Should().BeNull();
        request.PropertyType.Should().BeNull();
        request.OperationType.Should().BeNull();
        request.Description.Should().BeNull();
        request.Address.Should().BeNull();
        request.Features.Should().BeNull();
        request.Financials.Should().BeNull();
        request.VirtualTourUrl.Should().BeNull();
        request.VideoUrl.Should().BeNull();
    }

    [Fact]
    public void ChangeStatusRequest_CanBeInstantiated_WithStatus()
    {
        var request = new ChangeStatusRequest { Status = PropertyStatus.Active };

        request.Status.Should().Be(PropertyStatus.Active);
    }
}
