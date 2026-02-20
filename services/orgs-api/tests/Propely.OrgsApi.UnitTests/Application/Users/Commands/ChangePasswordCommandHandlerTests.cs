// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Commands.ChangePassword;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ChangePasswordCommandHandler(
            _userRepository, _passwordHasher, _unitOfWork,
            NullLogger<ChangePasswordCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldChangePassword()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash", "John Doe");
        var command = new ChangePasswordCommand(user.Id, "OldPassword123!", "NewPassword456!");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("OldPassword123!", "old_hash").Returns(true);
        _passwordHasher.HashPassword("NewPassword456!").Returns("new_hash");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be("new_hash");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldSaveChanges()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash", "John Doe");
        var command = new ChangePasswordCommand(user.Id, "OldPassword123!", "NewPassword456!");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("OldPassword123!", "old_hash").Returns(true);
        _passwordHasher.HashPassword("NewPassword456!").Returns("new_hash");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "OldPassword123!", "NewPassword456!");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsWrong_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash", "John Doe");
        var command = new ChangePasswordCommand(user.Id, "WrongPassword!", "NewPassword456!");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", "old_hash").Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_PASSWORD");
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsWrong_ShouldNotHashNewPasswordOrSave()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash", "John Doe");
        var command = new ChangePasswordCommand(user.Id, "WrongPassword!", "NewPassword456!");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", "old_hash").Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCallPasswordHasher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "OldPassword123!", "NewPassword456!");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash", "John Doe");
        var command = new ChangePasswordCommand(user.Id, "OldPassword123!", "NewPassword456!");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("new_hash");

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(user.Id, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
