// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Commands.RemoveBranchFromAgency;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Agencies.Commands;

public sealed class RemoveBranchFromAgencyCommandHandlerTests
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RemoveBranchFromAgencyCommandHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Agency _agency;
    private readonly Organization _organization;

    public RemoveBranchFromAgencyCommandHandlerTests()
    {
        _agencyRepository = Substitute.For<IAgencyRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RemoveBranchFromAgencyCommandHandler(
            _agencyRepository, _organizationRepository, _unitOfWork);

        _agency = Agency.Create("Test Agency", "test-agency", _userId);
        _organization = Organization.Create("Test Branch");

        // Pre-add the branch to the agency for remove tests
        _agency.AddBranch(_organization.Id);
        _organization.AssignToAgency(_agency.Id);

        // Default happy-path mocks
        _agencyRepository.GetByIdAsync(_agency.Id, Arg.Any<CancellationToken>())
            .Returns(_agency);
        _organizationRepository.GetByIdAsync(_organization.Id, Arg.Any<CancellationToken>())
            .Returns(_organization);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveBranchAndSave()
    {
        // Arrange
        var command = new RemoveBranchFromAgencyCommand(_agency.Id, _organization.Id, _userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _agency.BranchIds.Should().NotContain(_organization.Id);
        _organization.AgencyId.Should().BeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAgencyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var command = new RemoveBranchFromAgencyCommand(agencyId, _organization.Id, _userId);
        _agencyRepository.GetByIdAsync(agencyId, Arg.Any<CancellationToken>())
            .Returns((Agency?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Agency not found.");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAgencyOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var command = new RemoveBranchFromAgencyCommand(_agency.Id, _organization.Id, otherUserId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only the agency owner can remove branches.");
    }

    [Fact]
    public async Task Handle_WhenOrganizationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var command = new RemoveBranchFromAgencyCommand(_agency.Id, orgId, _userId);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Organization not found.");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var command = new RemoveBranchFromAgencyCommand(_agency.Id, _organization.Id, _userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _agencyRepository.Received(1).GetByIdAsync(_agency.Id, token);
        await _organizationRepository.Received(1).GetByIdAsync(_organization.Id, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
