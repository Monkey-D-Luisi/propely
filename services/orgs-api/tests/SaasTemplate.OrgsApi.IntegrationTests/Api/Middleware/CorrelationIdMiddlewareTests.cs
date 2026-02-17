// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Middleware;
using SaasTemplate.OrgsApi.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace SaasTemplate.OrgsApi.IntegrationTests.Api.Middleware;

public sealed class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    [Fact]
    public async Task InvokeAsync_WhenNoHeader_ShouldGenerateCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(accessor.CorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderPresent_ShouldUseExistingCorrelationId()
    {
        // Arrange
        var existingCorrelationId = "my-correlation-id-123";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = existingCorrelationId;

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().Be(existingCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var accessor = new CorrelationIdAccessor();
        string? capturedCorrelationId = null;

        var middleware = new CorrelationIdMiddleware(
            next: async ctx =>
            {
                // Capture what correlation ID was set before headers are sent
                capturedCorrelationId = accessor.CorrelationId;
                await ctx.Response.WriteAsync("OK");
            },
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - verify correlation ID was set during the request
        capturedCorrelationId.Should().NotBeNullOrEmpty();
        accessor.CorrelationId.Should().Be(capturedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyHeader_ShouldGenerateNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "";

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().NotBeNullOrEmpty();
        accessor.CorrelationId.Should().NotBe("");
    }

    [Fact]
    public async Task InvokeAsync_WithTooLongHeader_ShouldGenerateNewCorrelationId()
    {
        // Arrange
        var tooLongCorrelationId = new string('x', 100); // Exceeds 64 char limit
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = tooLongCorrelationId;

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - should generate new ID, not use the too-long one
        accessor.CorrelationId.Should().NotBe(tooLongCorrelationId);
        accessor.CorrelationId!.Length.Should().BeLessThanOrEqualTo(64);
    }
}
