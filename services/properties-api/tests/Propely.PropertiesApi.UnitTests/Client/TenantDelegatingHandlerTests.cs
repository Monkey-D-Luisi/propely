// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.PropertiesApi.Client;

namespace Propely.PropertiesApi.UnitTests.Client;

public class TenantDelegatingHandlerTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ILogger<TenantDelegatingHandler> _logger = Substitute.For<ILogger<TenantDelegatingHandler>>();

    [Fact]
    public async Task SendAsync_WithTenantIdInIncomingRequest_PropagatesHeader()
    {
        var tenantId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = tenantId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = new FakeInnerHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        await client.SendAsync(request);

        var innerHandler = (FakeInnerHandler)handler.InnerHandler;
        innerHandler.LastRequest!.Headers.GetValues(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().ContainSingle().Which.Should().Be(tenantId);
    }

    [Fact]
    public async Task SendAsync_WithoutTenantIdInIncomingRequest_DoesNotAddHeader()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = new FakeInnerHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        await client.SendAsync(request);

        var innerHandler = (FakeInnerHandler)handler.InnerHandler;
        innerHandler.LastRequest!.Headers.Contains(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenHeaderAlreadyPresent_DoesNotOverride()
    {
        var existingTenantId = Guid.NewGuid().ToString();
        var incomingTenantId = Guid.NewGuid().ToString();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = incomingTenantId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = new FakeInnerHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
        request.Headers.TryAddWithoutValidation(TenantDelegatingHandler.TenantIdHeaderName, existingTenantId);

        await client.SendAsync(request);

        var innerHandler = (FakeInnerHandler)handler.InnerHandler;
        innerHandler.LastRequest!.Headers.GetValues(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().ContainSingle().Which.Should().Be(existingTenantId);
    }

    [Fact]
    public async Task SendAsync_WithNullHttpContext_DoesNotAddHeader()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var handler = new TenantDelegatingHandler(_httpContextAccessor, _logger)
        {
            InnerHandler = new FakeInnerHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        await client.SendAsync(request);

        var innerHandler = (FakeInnerHandler)handler.InnerHandler;
        innerHandler.LastRequest!.Headers.Contains(TenantDelegatingHandler.TenantIdHeaderName)
            .Should().BeFalse();
    }

    [Fact]
    public void Constructor_NullHttpContextAccessor_ThrowsArgumentNullException()
    {
        var act = () => new TenantDelegatingHandler(null!, _logger);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new TenantDelegatingHandler(_httpContextAccessor, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class FakeInnerHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
