// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Configuration;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Commands.CreateInvitation;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class CreateInvitationCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IEntitlementService _entitlementService;
    private readonly IConfiguration _configuration;
    private readonly CreateInvitationCommandHandler _handler;

    public CreateInvitationCommandHandlerTests()
    {
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _invitationRepository = Substitute.For<IInvitationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _emailService = Substitute.For<IEmailService>();
        _entitlementService = Substitute.For<IEntitlementService>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Auth:FrontendBaseUrl"].Returns("http://localhost:3000");
        _entitlementService.CanAddMemberAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _handler = new CreateInvitationCommandHandler(
            _organizationRepository, _membershipRepository, _invitationRepository, _unitOfWork, _emailService, _entitlementService, _configuration);
    }

    [Fact]
    public async Task Handle_OwnerInvitesMember_ShouldReturnOkAndInviteUrl()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Ok.Should().BeTrue();
        result.InviteUrl.Should().StartWith("http://localhost:3000/orgs/accept-invite?token=");
    }

    [Fact]
    public async Task Handle_AdminInvitesMember_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", adminId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, adminId, Arg.Any<CancellationToken>())
            .Returns(adminMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Ok.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MemberInvites_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberMembership = Membership.Create(memberId, orgId, MembershipRole.Member);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", memberId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, memberId, Arg.Any<CancellationToken>())
            .Returns(memberMembership);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can invite members.");
    }

    [Fact]
    public async Task Handle_ViewerInvites_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var viewerMembership = Membership.Create(viewerId, orgId, MembershipRole.Viewer);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", viewerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, viewerId, Arg.Any<CancellationToken>())
            .Returns(viewerMembership);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can invite members.");
    }

    [Fact]
    public async Task Handle_NonMemberInvites_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var command = new CreateInvitationCommand(orgId, "invite@example.com", nonMemberId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, nonMemberId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Not authorized.");
    }

    [Fact]
    public async Task Handle_AdminInvitesAsOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", adminId, MembershipRole.Owner);

        _membershipRepository.GetAsync(orgId, adminId, Arg.Any<CancellationToken>())
            .Returns(adminMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins can only invite members and viewers.");
    }

    [Fact]
    public async Task Handle_AdminInvitesAsAdmin_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var adminMembership = Membership.Create(adminId, orgId, MembershipRole.Admin);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", adminId, MembershipRole.Admin);

        _membershipRepository.GetAsync(orgId, adminId, Arg.Any<CancellationToken>())
            .Returns(adminMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Admins can only invite members and viewers.");
    }

    [Fact]
    public async Task Handle_OwnerInvitesAsOwner_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Owner);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Ok.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistInvitation()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);
        Invitation? capturedInvitation = null;

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);
        _invitationRepository
            .AddAsync(Arg.Do<Invitation>(i => capturedInvitation = i), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        capturedInvitation.Should().NotBeNull();
        capturedInvitation!.OrganizationId.Should().Be(orgId);
        capturedInvitation.Email.Should().Be("invite@example.com");
        capturedInvitation.Role.Should().Be(MembershipRole.Member);
        capturedInvitation.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSendInvitationEmail()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Admin);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendOrgInvitationEmailAsync(
            "invite@example.com",
            org.Name,
            Arg.Is<string>(url => url.StartsWith("http://localhost:3000/orgs/accept-invite?token=")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Organization not found.");
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldNotCreateInvitationOrSendEmail()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _invitationRepository.DidNotReceive().AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendOrgInvitationEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

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
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _membershipRepository.Received(1).GetAsync(orgId, ownerId, token);
        await _organizationRepository.Received(1).GetByIdAsync(orgId, token);
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
        await _emailService.Received(1).SendOrgInvitationEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), token);
    }

    [Fact]
    public async Task Handle_WhenMemberLimitReached_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var command = new CreateInvitationCommand(orgId, "invite@example.com", ownerId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, ownerId, Arg.Any<CancellationToken>())
            .Returns(ownerMembership);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(org);
        _entitlementService.CanAddMemberAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Member limit reached for your current plan.");
        await _invitationRepository.DidNotReceive().AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
    }
}
