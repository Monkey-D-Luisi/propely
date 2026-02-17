// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Commands.VerifyEmail;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class VerifyEmailCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new VerifyEmailCommandHandler(_userRepository, _jwtTokenService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldVerifyUserEmail()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash", "User", emailVerified: false);
        var command = new VerifyEmailCommand("valid-token");
        _jwtTokenService.ValidateEmailVerificationToken("valid-token")
            .Returns(new EmailVerificationTokenPayload(user.Id, user.Email));
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.EmailVerified.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        user.EmailVerifiedAtUtc.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldThrowDomainException()
    {
        // Arrange
        var command = new VerifyEmailCommand("invalid-token");
        _jwtTokenService.ValidateEmailVerificationToken("invalid-token").Returns((EmailVerificationTokenPayload?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("INVALID_OR_EXPIRED_VERIFICATION_TOKEN");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyEmailCommand("valid-token");
        _jwtTokenService.ValidateEmailVerificationToken("valid-token")
            .Returns(new EmailVerificationTokenPayload(userId, "user@example.com"));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenTokenEmailDoesNotMatchUser_ShouldThrowDomainException()
    {
        // Arrange
        var user = User.Create("real@example.com", "hash", "User");
        var command = new VerifyEmailCommand("valid-token");
        _jwtTokenService.ValidateEmailVerificationToken("valid-token")
            .Returns(new EmailVerificationTokenPayload(user.Id, "different@example.com"));
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("INVALID_OR_EXPIRED_VERIFICATION_TOKEN");
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_ShouldRemainIdempotent()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash", "User", emailVerified: true);
        var command = new VerifyEmailCommand("valid-token");
        _jwtTokenService.ValidateEmailVerificationToken("valid-token")
            .Returns(new EmailVerificationTokenPayload(user.Id, user.Email));
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.EmailVerified.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
