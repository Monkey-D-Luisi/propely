// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Commands.DeleteProperty;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class DeletePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _repository = Substitute.For<IPropertyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeletePropertyCommandHandler _handler;

    public DeletePropertyCommandHandlerTests()
    {
        _handler = new DeletePropertyCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_PropertyExists_SoftDeletesProperty()
    {
        var tenantId = Guid.NewGuid();
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, tenantId, Guid.NewGuid());

        _repository.GetByIdAsync(property.Id, tenantId, Arg.Any<CancellationToken>())
            .Returns(property);

        await _handler.Handle(new DeletePropertyCommand(property.Id, tenantId), CancellationToken.None);

        property.IsDeleted.Should().BeTrue();
        property.DeletedAtUtc.Should().NotBeNull();
        _repository.Received(1).Update(property);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var act = async () => await _handler.Handle(
            new DeletePropertyCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
