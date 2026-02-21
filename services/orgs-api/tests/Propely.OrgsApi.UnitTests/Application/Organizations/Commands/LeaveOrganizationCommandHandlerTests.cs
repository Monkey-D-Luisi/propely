// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Commands.LeaveOrganization;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class LeaveOrganizationCommandHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LeaveOrganizationCommandHandler _handler;

    public LeaveOrganizationCommandHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task>>()());
        _handler = new LeaveOrganizationCommandHandler(_membershipRepository, _organizationRepository, _notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidMember_ShouldSoftDeleteMembership()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        membership.IsDeleted.Should().BeTrue();
        membership.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Not authorized.");
    }

    [Fact]
    public async Task Handle_WhenLastOwner_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("CANNOT_REMOVE_LAST_OWNER");
    }

    [Fact]
    public async Task Handle_WhenMultipleOwners_ShouldAllowLeave()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var otherOwner = Membership.Create(otherOwnerId, orgId, MembershipRole.Owner);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership, otherOwner });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        membership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldNotSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLastOwner_ShouldNotSoftDelete()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        membership.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldNotifyAdminsAndOwners()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership, adminMembership, ownerMembership });
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(Organization.Create("Test Org"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Notification>>(n => n.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);
        var command = new LeaveOrganizationCommand(orgId, userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _membershipRepository.Received(1).GetByOrgIdForUpdateAsync(orgId, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
        await _unitOfWork.Received(1).ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), token);
    }

    [Fact]
    public async Task Handle_ShouldExecuteInsideTransaction()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);
        var command = new LeaveOrganizationCommand(orgId, userId);

        _membershipRepository.GetByOrgIdForUpdateAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
    }
}
