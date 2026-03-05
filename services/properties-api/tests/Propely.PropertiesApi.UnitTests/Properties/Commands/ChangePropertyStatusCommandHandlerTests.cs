// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.ChangeStatus;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class ChangePropertyStatusCommandHandlerTests
{
    private readonly IPropertyRepository _repository = Substitute.For<IPropertyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangePropertyStatusCommandHandler _handler;

    public ChangePropertyStatusCommandHandlerTests()
    {
        _handler = new ChangePropertyStatusCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidTransition_ChangesStatus()
    {
        var tenantId = Guid.NewGuid();
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, tenantId, Guid.NewGuid());
        property.ClearDomainEvents();

        _repository.GetByIdAsync(property.Id, tenantId, Arg.Any<CancellationToken>())
            .Returns(property);

        await _handler.Handle(
            new ChangePropertyStatusCommand(property.Id, tenantId, PropertyStatus.Active), CancellationToken.None);

        property.Status.Should().Be(PropertyStatus.Active);
        _repository.Received(1).Update(property);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidTransition_ThrowsDomainException()
    {
        var tenantId = Guid.NewGuid();
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, tenantId, Guid.NewGuid());
        // Draft -> Sold is invalid
        _repository.GetByIdAsync(property.Id, tenantId, Arg.Any<CancellationToken>())
            .Returns(property);

        var act = async () => await _handler.Handle(
            new ChangePropertyStatusCommand(property.Id, tenantId, PropertyStatus.Sold), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var act = async () => await _handler.Handle(
            new ChangePropertyStatusCommand(Guid.NewGuid(), Guid.NewGuid(), PropertyStatus.Active), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
