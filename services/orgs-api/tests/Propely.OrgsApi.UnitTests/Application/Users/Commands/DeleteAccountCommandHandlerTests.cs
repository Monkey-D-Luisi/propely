// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Commands.DeleteAccount;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class DeleteAccountCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _invitationRepository = Substitute.For<IInvitationRepository>();
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _paymentService = Substitute.For<IPaymentService>();
        _emailService = Substitute.For<IEmailService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteAccountCommandHandler(
            _userRepository,
            _passwordHasher,
            _membershipRepository,
            _organizationRepository,
            _invitationRepository,
            _subscriptionRepository,
            _paymentService,
            _emailService,
            _unitOfWork,
            NullLogger<DeleteAccountCommandHandler>.Instance);

        // Default: batch queries return empty
        _membershipRepository.GetByOrgIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());
        _organizationRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Organization>());
    }

    [Fact]
    public async Task Handle_WithValidPassword_ShouldSoftDeleteUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");
        SetupValidUser(user, "CorrectPassword!");
        SetupNoMemberships(user.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithValidPassword_ShouldSaveChanges()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");
        SetupValidUser(user, "CorrectPassword!");
        SetupNoMemberships(user.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPassword_ShouldSendConfirmationEmail()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");
        SetupValidUser(user, "CorrectPassword!");
        SetupNoMemberships(user.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendAccountDeletionConfirmationEmailAsync(
            "test@example.com",
            "John Doe",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteAccountCommand(userId, "Password!");
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var command = new DeleteAccountCommand(user.Id, "WrongPassword!");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", "hash").Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_PASSWORD");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldNotSaveOrEmail()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var command = new DeleteAccountCommand(user.Id, "WrongPassword!");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", "hash").Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendAccountDeletionConfirmationEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsSoleOwner_ShouldDeleteOrganization()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var org = Organization.Create("My Org");
        var membership = Membership.Create(user.Id, org.Id, MembershipRole.Owner);
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");

        SetupValidUser(user, "CorrectPassword!");
        _membershipRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _membershipRepository.GetByOrgIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _organizationRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Organization> { org });
        _paymentService.IsEnabled.Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        org.IsDeleted.Should().BeTrue();
        membership.IsDeleted.Should().BeTrue();
        user.IsDeleted.Should().BeTrue();
        await _invitationRepository.Received(1).CancelByOrgAsync(org.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsNonSoleOwner_ShouldOnlyDeleteMembership()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var otherUserId = Guid.NewGuid();
        var org = Organization.Create("Shared Org");
        var userMembership = Membership.Create(user.Id, org.Id, MembershipRole.Member);
        var ownerMembership = Membership.Create(otherUserId, org.Id, MembershipRole.Owner);
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");

        SetupValidUser(user, "CorrectPassword!");
        _membershipRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { userMembership });
        _membershipRepository.GetByOrgIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { userMembership, ownerMembership });
        _paymentService.IsEnabled.Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        userMembership.IsDeleted.Should().BeTrue();
        ownerMembership.IsDeleted.Should().BeFalse();
        user.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AsSoleOwnerWithSubscription_ShouldCancelSubscription()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var org = Organization.Create("My Org");
        var membership = Membership.Create(user.Id, org.Id, MembershipRole.Owner);
        var subscription = Subscription.Create(org.Id, "cus_123", "sub_456", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddMonths(1));
        var command = new DeleteAccountCommand(user.Id, "CorrectPassword!");

        SetupValidUser(user, "CorrectPassword!");
        _membershipRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _membershipRepository.GetByOrgIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { membership });
        _organizationRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Organization> { org });
        _paymentService.IsEnabled.Returns(true);
        _subscriptionRepository.GetByOrgIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(subscription);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _paymentService.Received(1).CancelSubscriptionAsync("sub_456", Arg.Any<CancellationToken>());
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    private void SetupValidUser(User user, string password)
    {
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(password, user.PasswordHash).Returns(true);
    }

    private void SetupNoMemberships(Guid userId)
    {
        _membershipRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());
    }
}
