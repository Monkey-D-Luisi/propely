// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Commands.OAuthLogin;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Organizations;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class OAuthLoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUserExternalLoginRepository _userExternalLoginRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OAuthLoginCommandHandler _handler;

    public OAuthLoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userExternalLoginRepository = Substitute.For<IUserExternalLoginRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new OAuthLoginCommandHandler(
            _userRepository,
            _userExternalLoginRepository,
            _membershipRepository,
            _passwordHasher,
            _jwtTokenService,
            _refreshTokenRepository,
            _unitOfWork);

        _membershipRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());
    }

    [Fact]
    public async Task Handle_WhenExternalIdentityAlreadyLinked_ShouldReturnTokenForLinkedUser()
    {
        // Arrange
        var user = User.Create("linked@example.com", "hash", "Linked User", emailVerified: true);
        var externalLogin = UserExternalLogin.Create(user.Id, "google", "google-id-123");
        var command = new OAuthLoginCommand(user.Email, user.Name, "google", "google-id-123");

        _userExternalLoginRepository
            .GetByProviderAndExternalIdAsync("google", "google-id-123", Arg.Any<CancellationToken>())
            .Returns(externalLogin);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenService.GenerateToken(user, Arg.Any<IReadOnlyList<Guid>>()).Returns("linked-user-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(user.Id);
        result.Token.Should().Be("linked-user-token");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldCreateUserAndExternalLogin()
    {
        // Arrange
        var command = new OAuthLoginCommand("new@example.com", "New User", "github", "github-id-123");
        User? createdUser = null;
        UserExternalLogin? createdLogin = null;

        _userExternalLoginRepository
            .GetByProviderAndExternalIdAsync("github", "github-id-123", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userRepository.GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("generated-oauth-password-hash");
        _userRepository.AddAsync(Arg.Do<User>(user => createdUser = user), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _userExternalLoginRepository.AddAsync(
                Arg.Do<UserExternalLogin>(login => createdLogin = login),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("new-user-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("new@example.com");
        createdUser.Name.Should().Be("New User");
        createdUser.EmailVerified.Should().BeTrue();

        createdLogin.Should().NotBeNull();
        createdLogin!.UserId.Should().Be(createdUser.Id);
        createdLogin.Provider.Should().Be("github");
        createdLogin.ExternalId.Should().Be("github-id-123");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Token.Should().Be("new-user-token");
    }

    [Fact]
    public async Task Handle_WhenExistingVerifiedUser_ShouldLinkProviderWithoutChangingVerification()
    {
        // Arrange
        var user = User.Create("existing@example.com", "hash", "Existing User", emailVerified: true);
        var command = new OAuthLoginCommand(user.Email, user.Name, "google", "new-google-id");

        _userExternalLoginRepository
            .GetByProviderAndExternalIdAsync("google", "new-google-id", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _userExternalLoginRepository.GetByUserIdAndProviderAsync(user.Id, "google", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userExternalLoginRepository.AddAsync(Arg.Any<UserExternalLogin>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _jwtTokenService.GenerateToken(user, Arg.Any<IReadOnlyList<Guid>>()).Returns("existing-user-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.EmailVerified.Should().BeTrue();
        await _userExternalLoginRepository.Received(1).AddAsync(Arg.Any<UserExternalLogin>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Token.Should().Be("existing-user-token");
    }

    [Fact]
    public async Task Handle_WhenProviderAlreadyLinkedWithDifferentExternalId_ShouldThrowConflict()
    {
        // Arrange
        var user = User.Create("conflict@example.com", "hash", "Conflict User", emailVerified: true);
        var existingProviderLink = UserExternalLogin.Create(user.Id, "google", "google-id-old");
        var command = new OAuthLoginCommand(user.Email, user.Name, "google", "google-id-new");

        _userExternalLoginRepository
            .GetByProviderAndExternalIdAsync("google", "google-id-new", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _userExternalLoginRepository.GetByUserIdAndProviderAsync(user.Id, "google", Arg.Any<CancellationToken>())
            .Returns(existingProviderLink);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("PROVIDER_ALREADY_LINKED");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingUserNotVerified_ShouldMarkEmailAsVerified()
    {
        // Arrange
        var user = User.Create("verify@example.com", "hash", "Needs Verify", emailVerified: false);
        var command = new OAuthLoginCommand(user.Email, user.Name, "github", "github-id-999");

        _userExternalLoginRepository
            .GetByProviderAndExternalIdAsync("github", "github-id-999", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _userExternalLoginRepository.GetByUserIdAndProviderAsync(user.Id, "github", Arg.Any<CancellationToken>())
            .Returns((UserExternalLogin?)null);
        _userExternalLoginRepository.AddAsync(Arg.Any<UserExternalLogin>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _jwtTokenService.GenerateToken(user, Arg.Any<IReadOnlyList<Guid>>()).Returns("verified-after-oauth-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.EmailVerified.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Token.Should().Be("verified-after-oauth-token");
    }
}
