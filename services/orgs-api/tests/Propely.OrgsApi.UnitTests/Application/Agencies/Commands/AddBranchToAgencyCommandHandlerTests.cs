// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Commands.AddBranchToAgency;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Agencies.Commands;

public sealed class AddBranchToAgencyCommandHandlerTests
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AddBranchToAgencyCommandHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Agency _agency;
    private readonly Organization _organization;

    public AddBranchToAgencyCommandHandlerTests()
    {
        _agencyRepository = Substitute.For<IAgencyRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AddBranchToAgencyCommandHandler(
            _agencyRepository, _organizationRepository, _membershipRepository, _unitOfWork);

        _agency = Agency.Create("Test Agency", "test-agency", _userId);
        _organization = Organization.Create("Test Branch");

        // Default happy-path mocks
        _agencyRepository.GetByIdAsync(_agency.Id, Arg.Any<CancellationToken>())
            .Returns(_agency);
        _organizationRepository.GetByIdAsync(_organization.Id, Arg.Any<CancellationToken>())
            .Returns(_organization);
        _membershipRepository.GetAsync(_organization.Id, _userId, Arg.Any<CancellationToken>())
            .Returns(Membership.Create(_userId, _organization.Id, MembershipRole.Owner));
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddBranchAndSave()
    {
        // Arrange
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _agency.BranchIds.Should().Contain(_organization.Id);
        _organization.AgencyId.Should().Be(_agency.Id);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAgencyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var command = new AddBranchToAgencyCommand(agencyId, _organization.Id, _userId);
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
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, otherUserId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only the agency owner can add branches.");
    }

    [Fact]
    public async Task Handle_WhenOrganizationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var command = new AddBranchToAgencyCommand(_agency.Id, orgId, _userId);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Organization not found.");
    }

    [Fact]
    public async Task Handle_WhenOrgAlreadyAssignedToAgency_ShouldThrowDomainException()
    {
        // Arrange
        _organization.AssignToAgency(Guid.NewGuid()); // already assigned to another agency
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("This organization is already assigned to an agency.");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOrgMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);
        _membershipRepository.GetAsync(_organization.Id, _userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You must be an owner or admin of the organization to add it as a branch.");
    }

    [Fact]
    public async Task Handle_WhenUserIsOrgViewer_ShouldThrowForbiddenException()
    {
        // Arrange
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);
        _membershipRepository.GetAsync(_organization.Id, _userId, Arg.Any<CancellationToken>())
            .Returns(Membership.Create(_userId, _organization.Id, MembershipRole.Viewer));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You must be an owner or admin of the organization to add it as a branch.");
    }

    [Fact]
    public async Task Handle_WhenUserIsOrgAdmin_ShouldSucceed()
    {
        // Arrange
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);
        _membershipRepository.GetAsync(_organization.Id, _userId, Arg.Any<CancellationToken>())
            .Returns(Membership.Create(_userId, _organization.Id, MembershipRole.Admin));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _agency.BranchIds.Should().Contain(_organization.Id);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var command = new AddBranchToAgencyCommand(_agency.Id, _organization.Id, _userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _agencyRepository.Received(1).GetByIdAsync(_agency.Id, token);
        await _organizationRepository.Received(1).GetByIdAsync(_organization.Id, token);
        await _membershipRepository.Received(1).GetAsync(_organization.Id, _userId, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
