// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Commands.UpdateMemberRole;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Notifications;
using SaasTemplate.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class UpdateMemberRoleCommandHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateMemberRoleCommandHandler _handler;

    public UpdateMemberRoleCommandHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task>>()());
        _handler = new UpdateMemberRoleCommandHandler(
            _membershipRepository, _organizationRepository, _notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_OwnerUpdatesMemberRole_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, ownerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        memberMembership.Role.Should().Be(MembershipRole.Admin);
    }

    [Fact]
    public async Task Handle_AdminUpdatesMemberToViewer_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, adminId, MembershipRole.Viewer);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        memberMembership.Role.Should().Be(MembershipRole.Viewer);
    }

    [Fact]
    public async Task Handle_CannotChangeOwnRole_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new UpdateMemberRoleCommand(orgId, userId, userId, MembershipRole.Admin);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("CANNOT_CHANGE_OWN_ROLE");
    }

    [Fact]
    public async Task Handle_RequestingMemberNotFound_ShouldThrowForbiddenException()
    {
        // Arrange — requesting user is not a member of the org
        var orgId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetMembership = Membership.Create(targetUserId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, targetUserId, requestingUserId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { targetMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Not authorized.");
    }

    [Fact]
    public async Task Handle_TargetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new UpdateMemberRoleCommand(orgId, nonExistentId, ownerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Member not found.");
    }

    [Fact]
    public async Task Handle_AdminChangesOwnerRole_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new UpdateMemberRoleCommand(orgId, ownerId, adminId, MembershipRole.Member);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, ownerMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins can only change roles of members and viewers.");
    }

    [Fact]
    public async Task Handle_AdminChangesAdminRole_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var otherAdminId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var otherAdminMembership = Membership.Create(otherAdminId, orgId, MembershipRole.Admin);
        var command = new UpdateMemberRoleCommand(orgId, otherAdminId, adminId, MembershipRole.Member);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, otherAdminMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins can only change roles of members and viewers.");
    }

    [Fact]
    public async Task Handle_AdminPromotesToOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, memberId, adminId, MembershipRole.Owner);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, memberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins cannot promote to owner or admin.");
    }

    [Fact]
    public async Task Handle_AdminPromotesToAdmin_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, memberId, adminId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, memberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins cannot promote to owner or admin.");
    }

    [Fact]
    public async Task Handle_MemberChangesRole_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var otherMemberMembership = Membership.Create(otherMemberId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, otherMemberId, memberId, MembershipRole.Viewer);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { memberMembership, otherMemberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can change member roles.");
    }

    [Fact]
    public async Task Handle_ViewerChangesRole_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var viewerMembership = Membership.Create(viewerId, orgId, MembershipRole.Viewer);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, memberId, viewerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { viewerMembership, memberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can change member roles.");
    }

    [Fact]
    public async Task Handle_WhenMultipleOwners_ShouldAllowDowngrade()
    {
        // Arrange — 2 owners: one demotes the other
        var orgId = Guid.NewGuid();
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();
        var owner1 = Membership.Create(owner1Id, orgId, MembershipRole.Owner);
        var owner2 = Membership.Create(owner2Id, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, owner2Id, owner1Id, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { owner1, owner2 });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        owner2.Role.Should().Be(MembershipRole.Admin);
    }

    [Fact]
    public async Task Handle_ShouldNotifyTargetUserOnRoleChange()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, ownerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.UserId == memberId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, ownerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenForbidden_ShouldNotSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var otherMemberMembership = Membership.Create(otherMemberId, orgId, MembershipRole.Member);
        var command = new UpdateMemberRoleCommand(orgId, otherMemberId, memberId, MembershipRole.Viewer);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { memberMembership, otherMemberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, ownerId, MembershipRole.Admin);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _membershipRepository.Received(1).GetByOrgIdForUpdateAsync(orgId, token);
        await _organizationRepository.Received(1).GetByIdAsync(orgId, token);
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
        await _unitOfWork.Received(1).ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), token);
    }

    [Fact]
    public async Task Handle_ShouldExecuteInsideTransaction()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, memberId, ownerId, MembershipRole.Admin);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AdminUpdatesViewerToMember_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var viewerMembership = Membership.Create(viewerId, orgId, MembershipRole.Viewer);
        var org = Organization.Create("Test Org");
        var command = new UpdateMemberRoleCommand(orgId, viewerId, adminId, MembershipRole.Member);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, viewerMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        viewerMembership.Role.Should().Be(MembershipRole.Member);
    }
}
