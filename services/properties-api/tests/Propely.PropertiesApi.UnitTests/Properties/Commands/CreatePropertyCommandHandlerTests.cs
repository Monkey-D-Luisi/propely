// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.CreateProperty;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class CreatePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _repository = Substitute.For<IPropertyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePropertyCommandHandler _handler;

    public CreatePropertyCommandHandlerTests()
    {
        _handler = new CreatePropertyCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPropertyAndReturnsDto()
    {
        var command = new CreatePropertyCommand
        {
            Title = "Test Property",
            PropertyType = PropertyType.Apartment,
            OperationType = OperationType.Sale,
            TenantId = Guid.NewGuid(),
            AgentId = Guid.NewGuid()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Property");
        result.PropertyType.Should().Be(PropertyType.Apartment);
        result.OperationType.Should().Be(OperationType.Sale);
        result.Status.Should().Be(PropertyStatus.Draft);
        result.TenantId.Should().Be(command.TenantId);
        result.AgentId.Should().Be(command.AgentId);

        await _repository.Received(1).AddAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAllFields_MapsAllFieldsCorrectly()
    {
        var command = new CreatePropertyCommand
        {
            Title = "Luxury Villa",
            PropertyType = PropertyType.Villa,
            OperationType = OperationType.Sale,
            TenantId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            AgencyId = Guid.NewGuid(),
            Description = new() { Es = "Descripci\u00f3n", En = "Description" },
            Address = new() { City = "Madrid", Country = "ES" },
            Features = new() { Bedrooms = 4, Bathrooms = 3, HasPool = true },
            Financials = new() { Price = 500000m },
            VirtualTourUrl = "https://example.com/tour",
            VideoUrl = "https://example.com/video"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Description!.Es.Should().Be("Descripci\u00f3n");
        result.Description!.En.Should().Be("Description");
        result.Address!.City.Should().Be("Madrid");
        result.Features!.Bedrooms.Should().Be(4);
        result.Features!.HasPool.Should().Be(true);
        result.Financials!.Price.Should().Be(500000m);
        result.VirtualTourUrl.Should().Be("https://example.com/tour");
    }
}
