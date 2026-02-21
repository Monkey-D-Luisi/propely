// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.PublishingApi.Api.Middleware;
using Propely.PublishingApi.Api.Services;

namespace Propely.PublishingApi.UnitTests.Api.Middleware;

public sealed class CorrelationIdMiddlewareTests
{
    private readonly ILogger<CorrelationIdMiddleware> _logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();

    [Fact]
    public async Task InvokeAsync_WhenNoHeader_ShouldGenerateNewCorrelationId()
    {
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, accessor);

        accessor.CorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(accessor.CorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidHeader_ShouldUseProvidedValue()
    {
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "my-correlation-id";

        await middleware.InvokeAsync(context, accessor);

        accessor.CorrelationId.Should().Be("my-correlation-id");
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderTooLong_ShouldGenerateNewId()
    {
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = new string('a', 65);

        await middleware.InvokeAsync(context, accessor);

        accessor.CorrelationId.Should().NotBe(new string('a', 65));
        accessor.CorrelationId!.Length.Should().BeLessThanOrEqualTo(64);
    }

    [Fact]
    public async Task InvokeAsync_WhenEmptyHeader_ShouldGenerateNewId()
    {
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "";

        await middleware.InvokeAsync(context, accessor);

        accessor.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        var accessor = new CorrelationIdAccessor();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, _logger);

        await middleware.InvokeAsync(new DefaultHttpContext(), accessor);

        nextCalled.Should().BeTrue();
    }
}
