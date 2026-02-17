// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Commands.RegisterUser;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _emailService = Substitute.For<IEmailService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();
        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtTokenService,
            _emailService,
            _refreshTokenRepository,
            _unitOfWork,
            logger);

        _jwtTokenService.GenerateEmailVerificationToken(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns("verify-token");
        _emailService.SendEmailVerificationEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
        _emailService.SendWelcomeEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUserIdAndToken()
    {
        // Arrange
        var command = CreateCommand("Test@Example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword("Password123!").Returns("hashed_password");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("jwt_token_123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeEmpty();
        result.Token.Should().Be("jwt_token_123");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistUser()
    {
        // Arrange
        var command = CreateCommand("Test@Example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        User? capturedUser = null;

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword("Password123!").Returns("hashed_password");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("jwt_token_123");
        _userRepository.AddAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("test@example.com");
        capturedUser.Name.Should().Be("John Doe");
        capturedUser.PasswordHash.Should().Be("hashed_password");
        capturedUser.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = User.Create("test@example.com", "hash", "Existing");
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("EMAIL_TAKEN");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldNotPersistAnything()
    {
        // Arrange
        var existingUser = User.Create("test@example.com", "hash", "Existing");
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendEmailVerificationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<string>());
        await _emailService.DidNotReceive().SendWelcomeEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokenForCreatedUser()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        User? tokenUser = null;

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Do<User>(u => tokenUser = u), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtTokenService.Received(1).GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>());
        tokenUser.Should().NotBeNull();
        tokenUser!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_ShouldSendEmailVerificationMessage()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");
        _jwtTokenService.GenerateEmailVerificationToken(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns("verification-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendEmailVerificationEmailAsync(
            "test@example.com",
            "http://localhost:3000/en/verify-email?token=verification-token",
            Arg.Any<CancellationToken>(),
            "en");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _userRepository.Received(1).GetByEmailAsync(Arg.Any<string>(), token);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
        await _emailService.Received(1).SendEmailVerificationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            token,
            Arg.Any<string>());
        await _emailService.Received(1).SendWelcomeEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            token,
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldSendWelcomeEmail()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendWelcomeEmailAsync(
            "test@example.com",
            "John Doe",
            "http://localhost:3000/en/login",
            Arg.Any<CancellationToken>(),
            "en");
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrows_ShouldStillReturnSuccessResult()
    {
        // Arrange
        var command = CreateCommand("test@example.com", "Password123!", "John Doe", "http://localhost:3000", "en");
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyList<Guid>>()).Returns("jwt_token");
        _emailService.SendEmailVerificationEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("Email service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — registration succeeds even when email fails
        result.Should().NotBeNull();
        result.UserId.Should().NotBeEmpty();
        result.Token.Should().Be("jwt_token");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static RegisterUserCommand CreateCommand(
        string email,
        string password,
        string? name,
        string frontendBaseUrl,
        string locale) =>
        new(email, password, name, frontendBaseUrl, locale);
}
