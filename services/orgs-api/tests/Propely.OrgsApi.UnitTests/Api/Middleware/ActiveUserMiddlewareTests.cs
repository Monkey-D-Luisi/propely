// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Propely.OrgsApi.Api.Middleware;
using Propely.OrgsApi.Application.Common.Auth;
using Propely.OrgsApi.Application.Users.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Api.Middleware;

public sealed class ActiveUserMiddlewareTests
{
    private readonly IUserRepository _userRepository;
    private readonly ActiveUserMiddleware _middleware;
    private bool _nextCalled;

    public ActiveUserMiddlewareTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _nextCalled = false;
        _middleware = new ActiveUserMiddleware(
            _ =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<ActiveUserMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateAuthenticatedContext(Guid userId, int passwordVersion)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(AuthClaimTypes.PasswordVersion, passwordVersion.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateAuthenticatedContextWithoutPwdVer(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_WithMatchingPasswordVersion_ShouldPassThrough()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(userId, 2);
        _userRepository.IsActiveWithPasswordVersionAsync(userId, 2, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMismatchedPasswordVersion_ShouldReturn401()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(userId, 1);
        _userRepository.IsActiveWithPasswordVersionAsync(userId, 1, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_WithoutPwdVerClaim_ShouldDefaultToVersion0()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateAuthenticatedContextWithoutPwdVer(userId);
        _userRepository.IsActiveWithPasswordVersionAsync(userId, 0, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.Received(1).IsActiveWithPasswordVersionAsync(userId, 0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthenticated_ShouldPassThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.DidNotReceive().IsActiveWithPasswordVersionAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
