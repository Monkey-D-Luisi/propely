// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Commands.ResendVerification;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class ResendVerificationCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ResendVerificationCommandHandler _handler;

    public ResendVerificationCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _emailService = Substitute.For<IEmailService>();
        _handler = new ResendVerificationCommandHandler(_userRepository, _jwtTokenService, _emailService);

        _emailService.SendEmailVerificationEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WhenUserIsUnverified_ShouldSendVerificationEmail()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash", "User", emailVerified: false);
        var command = new ResendVerificationCommand(user.Id, "http://localhost:3000", "es");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateEmailVerificationToken(user.Id, user.Email)
            .Returns("verification-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Sent.Should().BeTrue();
        result.AlreadyVerified.Should().BeFalse();

        await _emailService.Received(1).SendEmailVerificationEmailAsync(
            "user@example.com",
            "http://localhost:3000/es/verify-email?token=verification-token",
            Arg.Any<CancellationToken>(),
            "es");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyVerified_ShouldReturnAlreadyVerifiedWithoutSending()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash", "User", emailVerified: true);
        var command = new ResendVerificationCommand(user.Id, "http://localhost:3000", "en");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Sent.Should().BeFalse();
        result.AlreadyVerified.Should().BeTrue();
        _jwtTokenService.DidNotReceive().GenerateEmailVerificationToken(Arg.Any<Guid>(), Arg.Any<string>());
        await _emailService.DidNotReceive().SendEmailVerificationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ResendVerificationCommand(userId, "http://localhost:3000", "en");
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("USER_NOT_FOUND");
    }
}
