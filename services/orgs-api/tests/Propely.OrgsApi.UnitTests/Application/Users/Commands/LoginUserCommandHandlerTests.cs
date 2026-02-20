// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Commands.LoginUser;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new LoginUserCommandHandler(
            _userRepository, _membershipRepository, _passwordHasher, _jwtTokenService,
            _refreshTokenRepository, _unitOfWork,
            NullLogger<LoginUserCommandHandler>.Instance);

        _membershipRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<Membership>());
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnUserIdAndToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "John Doe");
        var command = new LoginUserCommand("test@example.com", "Password123!");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("Password123!", "hashed_password").Returns(true);
        _jwtTokenService.GenerateToken(user, Arg.Any<IReadOnlyList<Guid>>()).Returns("jwt_token_123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Token.Should().Be("jwt_token_123");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "Password123!");
        _userRepository.GetByEmailAsync("nonexistent@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "John Doe");
        var command = new LoginUserCommand("test@example.com", "WrongPassword!");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", "hashed_password").Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldStillCallPasswordHasherForTimingSafety()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "Password123!");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert — password hasher IS called with dummy hash to prevent timing-based email enumeration
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _passwordHasher.Received(1).VerifyPassword(
            command.Password,
            Arg.Is<string>(hash => hash.StartsWith("$2")));
        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "John Doe");
        var command = new LoginUserCommand("test@example.com", "Password123!");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _userRepository.Received(1).GetByEmailAsync(Arg.Any<string>(), token);
    }
}
