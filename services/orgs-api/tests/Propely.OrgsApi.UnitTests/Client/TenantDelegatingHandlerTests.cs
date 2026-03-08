// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.Shared.Http;

namespace Propely.OrgsApi.UnitTests.Client;

public sealed class TenantDelegatingHandlerTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantDelegatingHandler> _logger;

    public TenantDelegatingHandlerTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _logger = Substitute.For<ILogger<TenantDelegatingHandler>>();
    }

    [Fact]
    public async Task SendAsync_WhenTenantIdInIncomingRequest_ShouldPropagateToOutgoingRequest()
    {
        // Arrange
        var tenantId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = tenantId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var innerHandler = new RecordingHandler();
        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers
            .GetValues(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().ContainSingle()
            .Which.Should().Be(tenantId);
    }

    [Fact]
    public async Task SendAsync_WhenNoTenantIdInIncomingRequest_ShouldNotAddHeader()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var innerHandler = new RecordingHandler();
        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers
            .Contains(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenNoHttpContext_ShouldNotAddHeader()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var innerHandler = new RecordingHandler();
        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers
            .Contains(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenHeaderAlreadyExistsOnOutgoingRequest_ShouldNotOverwrite()
    {
        // Arrange
        var incomingTenantId = Guid.NewGuid().ToString();
        var explicitTenantId = Guid.NewGuid().ToString();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = incomingTenantId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var innerHandler = new RecordingHandler();
        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
        request.Headers.Add(TenantDelegatingHandler.TenantIdHeaderName, explicitTenantId);

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers
            .GetValues(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().ContainSingle()
            .Which.Should().Be(explicitTenantId, "explicit header should not be overwritten");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAsync_WhenTenantIdIsEmptyOrWhitespace_ShouldNotAddHeader(string tenantId)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = tenantId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var innerHandler = new RecordingHandler();
        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers
            .Contains(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().BeFalse();
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrow()
    {
        // Act
        var act = () => new TenantDelegatingHandler(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpContextAccessor");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrow()
    {
        // Act
        var act = () => new TenantDelegatingHandler(_httpContextAccessor, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// A simple delegating handler that records the last request for assertion purposes.
    /// </summary>
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
