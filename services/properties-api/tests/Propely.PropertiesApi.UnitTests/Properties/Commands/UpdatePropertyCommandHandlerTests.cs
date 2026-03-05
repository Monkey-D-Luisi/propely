// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class UpdatePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _repository = Substitute.For<IPropertyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdatePropertyCommandHandler _handler;

    public UpdatePropertyCommandHandlerTests()
    {
        _handler = new UpdatePropertyCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_PropertyExists_UpdatesAndReturnsDto()
    {
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var property = Property.Create("Original", PropertyType.Apartment, OperationType.Sale, tenantId, agentId);

        _repository.GetByIdAsync(property.Id, tenantId, Arg.Any<CancellationToken>())
            .Returns(property);

        var command = new UpdatePropertyCommand
        {
            PropertyId = property.Id,
            TenantId = tenantId,
            UpdatedBy = agentId,
            Title = "Updated Title",
            Financials = new PropertyFinancialsDto { Price = 300000m }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Updated Title");
        result.Financials!.Price.Should().Be(300000m);
        _repository.Received(1).Update(property);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var command = new UpdatePropertyCommand
        {
            PropertyId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
