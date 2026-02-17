// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Commands.RemoveMember;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Notifications;
using SaasTemplate.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class RemoveMemberCommandHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RemoveMemberCommandHandler _handler;

    public RemoveMemberCommandHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task>>()());
        _handler = new RemoveMemberCommandHandler(
            _membershipRepository, _organizationRepository, _notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_OwnerRemovesMember_ShouldSoftDeleteMembership()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new RemoveMemberCommand(orgId, memberId, ownerId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        memberMembership.IsDeleted.Should().BeTrue();
        ownerMembership.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AdminRemovesMember_ShouldSoftDeleteMembership()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new RemoveMemberCommand(orgId, memberId, adminId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        memberMembership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AdminRemovesAdmin_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var otherAdminId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var otherAdminMembership = Membership.Create(otherAdminId, orgId, MembershipRole.Admin);
        var command = new RemoveMemberCommand(orgId, otherAdminId, adminId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, otherAdminMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins can only remove members and viewers.");
    }

    [Fact]
    public async Task Handle_AdminRemovesOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new RemoveMemberCommand(orgId, ownerId, adminId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, ownerMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_MemberRemovesMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var otherMemberMembership = Membership.Create(otherMemberId, orgId, MembershipRole.Member);
        var command = new RemoveMemberCommand(orgId, otherMemberId, memberId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { memberMembership, otherMemberMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can remove members.");
    }

    [Fact]
    public async Task Handle_OwnerRemovesLastOtherOwner_WhenTwoOwners_ShouldSucceed()
    {
        // Arrange — 2 owners: removing one leaves the other as sole owner (allowed)
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var otherOwnerMembership = Membership.Create(otherOwnerId, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new RemoveMemberCommand(orgId, otherOwnerId, ownerId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, otherOwnerMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        otherOwnerMembership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RemoveSelf_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RemoveMemberCommand(orgId, userId, userId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("CANNOT_REMOVE_SELF");
    }

    [Fact]
    public async Task Handle_ShouldNotifyRemovedMember()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new RemoveMemberCommand(orgId, memberId, ownerId);

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
        var command = new RemoveMemberCommand(orgId, memberId, ownerId);

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
        var command = new RemoveMemberCommand(orgId, otherMemberId, memberId);

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
        var command = new RemoveMemberCommand(orgId, memberId, ownerId);
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
        var command = new RemoveMemberCommand(orgId, memberId, ownerId);

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
    public async Task Handle_RequestingMemberNotFound_ShouldThrowForbiddenException()
    {
        // Arrange — requesting user is not a member of the org
        var orgId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetMembership = Membership.Create(targetUserId, orgId, MembershipRole.Member);
        var command = new RemoveMemberCommand(orgId, targetUserId, requestingUserId);

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
        var command = new RemoveMemberCommand(orgId, nonExistentId, ownerId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Target member not found.");
    }

    [Fact]
    public async Task Handle_AdminRemovesViewer_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var viewerMembership = Membership.Create(viewerId, orgId, MembershipRole.Viewer);
        var org = Organization.Create("Test Org");
        var command = new RemoveMemberCommand(orgId, viewerId, adminId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { adminMembership, viewerMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        viewerMembership.IsDeleted.Should().BeTrue();
    }
}
