// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Commands.UpdateOrganization;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class UpdateOrganizationCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateOrganizationCommandHandler _handler;

    public UpdateOrganizationCommandHandlerTests()
    {
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateOrganizationCommandHandler(
            _organizationRepository, _membershipRepository, _unitOfWork);
    }

    private static Organization CreateTestOrg(string name = "Original Name")
    {
        return Organization.Create(name);
    }

    private static Membership CreateTestMembership(Guid userId, Guid orgId, MembershipRole role)
    {
        return Membership.Create(userId, orgId, role);
    }

    [Fact]
    public async Task Handle_AsOwner_ShouldUpdateAndReturnResult()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(org.Id, userId, "New Name", "New Description");

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(org.Id);
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldUpdateAndReturnResult()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Admin);
        var command = new UpdateOrganizationCommand(org.Id, userId, "Admin Updated", null);

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Admin Updated");
        result.Description.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AsMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Agent);
        var command = new UpdateOrganizationCommand(org.Id, userId, "New Name", null);

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_AsViewer_ShouldThrowForbiddenException()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Viewer);
        var command = new UpdateOrganizationCommand(org.Id, userId, "New Name", null);

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name", null);

        _membershipRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, orgId, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(orgId, userId, "New Name", null);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(org.Id, userId, "New Name", null);

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTrimNameAndDescription()
    {
        // Arrange
        var org = CreateTestOrg();
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(org.Id, userId, "  Trimmed Name  ", "  Trimmed Desc  ");

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Trimmed Name");
        result.Description.Should().Be("Trimmed Desc");
    }

    [Fact]
    public async Task Handle_WhenRenamingToDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var org = CreateTestOrg("Original Name");
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(org.Id, userId, "Taken Name", null);

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);
        _organizationRepository.ExistsByNameAsync("Taken Name", org.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenKeepingSameName_ShouldNotCheckUniqueness()
    {
        // Arrange
        var org = CreateTestOrg("Same Name");
        var userId = Guid.NewGuid();
        var membership = CreateTestMembership(userId, org.Id, MembershipRole.Owner);
        var command = new UpdateOrganizationCommand(org.Id, userId, "Same Name", "Updated desc");

        _membershipRepository.GetAsync(org.Id, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _organizationRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Same Name");
        result.Description.Should().Be("Updated desc");
        await _organizationRepository.DidNotReceive()
            .ExistsByNameAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
