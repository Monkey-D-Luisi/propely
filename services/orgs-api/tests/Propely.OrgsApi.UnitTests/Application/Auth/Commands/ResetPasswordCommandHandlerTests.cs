// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Auth.Commands.ResetPassword;
using Propely.OrgsApi.Application.Auth.Security;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Auth.Commands;

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ResetPasswordCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtTokenService,
            _unitOfWork,
            NullLogger<ResetPasswordCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPasswordAndSave()
    {
        // Arrange
        var user = User.Create("john@example.com", "old-hash", "John");
        var fingerprint = PasswordResetTokenFingerprint.FromPasswordHash(user.PasswordHash);
        var command = new ResetPasswordCommand("reset-token", "NewPassword123!");

        _jwtTokenService.ValidatePasswordResetToken(command.Token)
            .Returns(new PasswordResetTokenPayload(user.Id, user.Email, fingerprint));
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(command.NewPassword).Returns("new-hash");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be("new-hash");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var command = new ResetPasswordCommand("invalid-token", "NewPassword123!");
        _jwtTokenService.ValidatePasswordResetToken(command.Token).Returns((PasswordResetTokenPayload?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("INVALID_OR_EXPIRED_RESET_TOKEN");
    }

    [Fact]
    public async Task Handle_WhenTokenWasAlreadyUsed_ShouldThrowDomainException()
    {
        // Arrange
        var user = User.Create("john@example.com", "current-hash", "John");
        var usedFingerprint = PasswordResetTokenFingerprint.FromPasswordHash("previous-hash");
        var command = new ResetPasswordCommand("used-token", "NewPassword123!");

        _jwtTokenService.ValidatePasswordResetToken(command.Token)
            .Returns(new PasswordResetTokenPayload(user.Id, user.Email, usedFingerprint));
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("RESET_TOKEN_ALREADY_USED");
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
