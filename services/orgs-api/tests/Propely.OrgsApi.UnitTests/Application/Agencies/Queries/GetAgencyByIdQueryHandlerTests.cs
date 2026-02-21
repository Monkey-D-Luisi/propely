// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Queries.GetAgencyById;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Agencies.Queries;

public sealed class GetAgencyByIdQueryHandlerTests
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly GetAgencyByIdQueryHandler _handler;

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Agency _agency;

    public GetAgencyByIdQueryHandlerTests()
    {
        _agencyRepository = Substitute.For<IAgencyRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new GetAgencyByIdQueryHandler(
            _agencyRepository, _organizationRepository, _membershipRepository);

        _agency = Agency.Create("Test Agency", "test-agency", _ownerId);

        _agencyRepository.GetByIdAsync(_agency.Id, Arg.Any<CancellationToken>())
            .Returns(_agency);
    }

    [Fact]
    public async Task Handle_WhenUserIsOwner_ShouldReturnAgencyDetail()
    {
        // Arrange
        var query = new GetAgencyByIdQuery(_agency.Id, _ownerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_agency.Id);
        result.Name.Should().Be("Test Agency");
        result.Slug.Should().Be("test-agency");
        result.CreatedByUserId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Handle_WhenUserIsOwner_ShouldNotCheckMemberships()
    {
        // Arrange
        var query = new GetAgencyByIdQuery(_agency.Id, _ownerId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.DidNotReceive()
            .GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsBranchMember_ShouldReturnAgencyDetail()
    {
        // Arrange
        var branchUserId = Guid.NewGuid();
        var org = Organization.Create("Branch Org");
        _agency.AddBranch(org.Id);

        var membership = Membership.Create(branchUserId, org.Id, MembershipRole.Agent);
        _membershipRepository.GetByUserIdAsync(branchUserId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        var query = new GetAgencyByIdQuery(_agency.Id, branchUserId);

        _organizationRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Organization> { org });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Branches.Should().HaveCount(1);
        result.Branches[0].Id.Should().Be(org.Id);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoAccess_ShouldThrowForbiddenException()
    {
        // Arrange
        var randomUserId = Guid.NewGuid();
        var query = new GetAgencyByIdQuery(_agency.Id, randomUserId);

        _membershipRepository.GetByUserIdAsync(randomUserId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You do not have access to this agency.");
    }

    [Fact]
    public async Task Handle_WhenAgencyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var query = new GetAgencyByIdQuery(agencyId, _ownerId);
        _agencyRepository.GetByIdAsync(agencyId, Arg.Any<CancellationToken>())
            .Returns((Agency?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Agency not found.");
    }

    [Fact]
    public async Task Handle_WithBranches_ShouldLoadOrganizationDetails()
    {
        // Arrange
        var org1 = Organization.Create("Branch One");
        var org2 = Organization.Create("Branch Two");
        _agency.AddBranch(org1.Id);
        _agency.AddBranch(org2.Id);

        _organizationRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Organization> { org1, org2 });

        var query = new GetAgencyByIdQuery(_agency.Id, _ownerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Branches.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoBranches_ShouldReturnEmptyBranchesList()
    {
        // Arrange
        var query = new GetAgencyByIdQuery(_agency.Id, _ownerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Branches.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var query = new GetAgencyByIdQuery(_agency.Id, _ownerId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _agencyRepository.Received(1).GetByIdAsync(_agency.Id, token);
    }
}
