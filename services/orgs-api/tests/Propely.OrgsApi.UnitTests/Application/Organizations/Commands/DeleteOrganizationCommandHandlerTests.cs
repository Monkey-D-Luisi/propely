// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Commands.DeleteOrganization;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class DeleteOrganizationCommandHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteOrganizationCommandHandler _handler;

    public DeleteOrganizationCommandHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _invitationRepository = Substitute.For<IInvitationRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteOrganizationCommandHandler(
            _membershipRepository, _organizationRepository, _invitationRepository, _notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_AsOwner_ShouldSoftDeleteOrganization()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        org.IsDeleted.Should().BeTrue();
        org.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AsOwner_ShouldCascadeSoftDeleteMemberships()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, ownerId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        ownerMembership.IsDeleted.Should().BeTrue();
        memberMembership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AsNonOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Admin);
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners can delete the organization.");
    }

    [Fact]
    public async Task Handle_AsMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Not authorized.");
    }

    [Fact]
    public async Task Handle_ShouldCancelPendingInvitations()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _invitationRepository.Received(1).CancelByOrgAsync(orgId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotifyOtherMembers()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, ownerId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership, adminMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Notification>>(n =>
                n.Count() == 2 &&
                n.Any(x => x.UserId == memberId) &&
                n.Any(x => x.UserId == adminId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
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
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Admin);
        var command = new DeleteOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

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
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var org = Organization.Create("Test Org");
        var command = new DeleteOrganizationCommand(orgId, userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _membershipRepository.Received(1).GetByOrgIdAsync(orgId, token);
        await _organizationRepository.Received(1).GetByIdAsync(orgId, token);
        await _invitationRepository.Received(1).CancelByOrgAsync(orgId, token);
        await _notificationRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<Notification>>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
