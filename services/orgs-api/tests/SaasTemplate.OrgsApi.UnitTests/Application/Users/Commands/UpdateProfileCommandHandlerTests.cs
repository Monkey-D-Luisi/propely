// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Commands.UpdateProfile;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Users.Commands;

public sealed class UpdateProfileCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateProfileCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUpdatedProfile()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "Old Name");
        var command = new UpdateProfileCommand(user.Id, "New Name");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WithNullName_ShouldClearName()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "Old Name");
        var command = new UpdateProfileCommand(user.Id, null);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithWhitespaceName_ShouldClearName()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "Old Name");
        var command = new UpdateProfileCommand(user.Id, "   ");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "Old Name");
        var command = new UpdateProfileCommand(user.Id, "New Name");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

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
        var command = new UpdateProfileCommand(userId, "New Name");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateProfileCommand(userId, "New Name");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashed_password", "Old Name");
        var command = new UpdateProfileCommand(user.Id, "New Name");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(user.Id, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
