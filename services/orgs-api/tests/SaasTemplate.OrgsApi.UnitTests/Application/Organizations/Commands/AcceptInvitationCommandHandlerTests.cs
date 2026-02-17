// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Commands.AcceptInvitation;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class AcceptInvitationCommandHandlerTests
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _invitationRepository = Substitute.For<IInvitationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AcceptInvitationCommandHandler(
            _invitationRepository, _membershipRepository, _organizationRepository, _notificationRepository, _unitOfWork);
    }

    private static Invitation CreateValidInvitation(Guid orgId, string email, MembershipRole role = MembershipRole.Member)
    {
        return Invitation.Create(orgId, email, role, TimeSpan.FromDays(7));
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldCreateMembershipAndAcceptInvitation()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "user@example.com");
        var command = new AcceptInvitationCommand(invitation.Token, userId, "user@example.com");

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        Membership? capturedMembership = null;
        _membershipRepository
            .AddAsync(Arg.Do<Membership>(m => capturedMembership = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMembership.Should().NotBeNull();
        capturedMembership!.UserId.Should().Be(userId);
        capturedMembership.OrganizationId.Should().Be(orgId);
        capturedMembership.Role.Should().Be(MembershipRole.Member);
        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public async Task Handle_WhenTokenInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var command = new AcceptInvitationCommand("invalid_token", Guid.NewGuid(), "user@example.com");
        _invitationRepository.GetByTokenAsync("invalid_token", Arg.Any<CancellationToken>())
            .Returns((Invitation?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("INVALID_TOKEN");
    }

    [Fact]
    public async Task Handle_WhenAlreadyAccepted_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "user@example.com");
        invitation.Accept(); // Mark as already accepted

        var command = new AcceptInvitationCommand(invitation.Token, Guid.NewGuid(), "user@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("ALREADY_USED");
    }

    [Fact]
    public async Task Handle_WhenExpired_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        // Create an invitation that expired 1 day ago
        var invitation = Invitation.Create(orgId, "user@example.com", MembershipRole.Member, TimeSpan.FromDays(-1));

        var command = new AcceptInvitationCommand(invitation.Token, Guid.NewGuid(), "user@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("EXPIRED");
    }

    [Fact]
    public async Task Handle_WhenEmailMismatch_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "invited@example.com");

        var command = new AcceptInvitationCommand(invitation.Token, Guid.NewGuid(), "different@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("USER_EMAIL_MISMATCH");
    }

    [Fact]
    public async Task Handle_WhenAlreadyMember_ShouldNotCreateDuplicateMembership()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "user@example.com");
        var existingMembership = Membership.Create(userId, orgId, MembershipRole.Member);

        var command = new AcceptInvitationCommand(invitation.Token, userId, "user@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(existingMembership);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { existingMembership });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _membershipRepository.DidNotReceive().AddAsync(Arg.Any<Membership>(), Arg.Any<CancellationToken>());
        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "user@example.com");
        var command = new AcceptInvitationCommand(invitation.Token, userId, "user@example.com");

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invitation = CreateValidInvitation(orgId, "user@example.com");
        var command = new AcceptInvitationCommand(invitation.Token, userId, "user@example.com");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _invitationRepository.Received(1).GetByTokenAsync(invitation.Token, token);
        await _membershipRepository.Received(1).GetAsync(orgId, userId, token);
        await _membershipRepository.Received(1).AddAsync(Arg.Any<Membership>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task Handle_WhenTokenInvalid_ShouldNotPersistAnything()
    {
        // Arrange
        var command = new AcceptInvitationCommand("bad_token", Guid.NewGuid(), "user@example.com");
        _invitationRepository.GetByTokenAsync("bad_token", Arg.Any<CancellationToken>())
            .Returns((Invitation?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        await _membershipRepository.DidNotReceive().AddAsync(Arg.Any<Membership>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
