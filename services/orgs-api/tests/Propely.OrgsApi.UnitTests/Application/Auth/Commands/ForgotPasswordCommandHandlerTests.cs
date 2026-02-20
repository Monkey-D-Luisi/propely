// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Auth.Commands.ForgotPassword;
using Propely.OrgsApi.Application.Auth.Security;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Auth.Commands;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _emailService = Substitute.For<IEmailService>();

        _handler = new ForgotPasswordCommandHandler(
            _userRepository,
            _jwtTokenService,
            _emailService,
            NullLogger<ForgotPasswordCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldSendPasswordResetEmail()
    {
        // Arrange
        var user = User.Create("john@example.com", "hash", "John Doe");
        var command = new ForgotPasswordCommand("john@example.com", "http://localhost:3000", "es");
        var expectedFingerprint = PasswordResetTokenFingerprint.FromPasswordHash(user.PasswordHash);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenService.GeneratePasswordResetToken(user.Id, user.Email, expectedFingerprint)
            .Returns("reset-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendPasswordResetEmailAsync(
            user.Email,
            "http://localhost:3000/es/reset-password?token=reset-token",
            Arg.Any<CancellationToken>(),
            "es");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnSilentlyWithoutSendingEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand("missing@example.com", "http://localhost:3000");
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _jwtTokenService.DidNotReceive().GeneratePasswordResetToken(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>());
        await _emailService.DidNotReceive().SendPasswordResetEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<string>());
    }
}
