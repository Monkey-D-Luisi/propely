// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Commands.CreateAgency;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Agencies.Commands;

public sealed class CreateAgencyCommandHandlerTests
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateAgencyCommandHandler _handler;

    public CreateAgencyCommandHandlerTests()
    {
        _agencyRepository = Substitute.For<IAgencyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateAgencyCommandHandler(_agencyRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnAgencyIdNameAndSlug()
    {
        // Arrange
        var command = new CreateAgencyCommand("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AgencyId.Should().NotBeEmpty();
        result.Name.Should().Be("Test Agency");
        result.Slug.Should().Be("test-agency");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAgency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAgencyCommand("Test Agency", "test-agency", userId);
        Agency? capturedAgency = null;

        _agencyRepository
            .AddAsync(Arg.Do<Agency>(a => capturedAgency = a), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _agencyRepository.Received(1).AddAsync(Arg.Any<Agency>(), Arg.Any<CancellationToken>());
        capturedAgency.Should().NotBeNull();
        capturedAgency!.Name.Should().Be("Test Agency");
        capturedAgency.Slug.Value.Should().Be("test-agency");
        capturedAgency.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var command = new CreateAgencyCommand("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateAgencyCommand("Test Agency", "test-agency", Guid.NewGuid());
        _agencyRepository.ExistsBySlugAsync("test-agency", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An agency with this slug already exists.");
        await _agencyRepository.DidNotReceive().AddAsync(Arg.Any<Agency>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var command = new CreateAgencyCommand("Test Agency", "test-agency", Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _agencyRepository.Received(1).ExistsBySlugAsync("test-agency", Arg.Any<Guid?>(), token);
        await _agencyRepository.Received(1).AddAsync(Arg.Any<Agency>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
