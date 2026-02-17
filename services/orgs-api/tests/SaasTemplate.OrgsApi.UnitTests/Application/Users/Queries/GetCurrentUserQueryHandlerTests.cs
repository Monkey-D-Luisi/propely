// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Queries.GetCurrentUser;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Users.Queries;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetCurrentUserQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John Doe");
        var query = new GetCurrentUserQuery(user.Id);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery(userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(userId, token);
    }
}
