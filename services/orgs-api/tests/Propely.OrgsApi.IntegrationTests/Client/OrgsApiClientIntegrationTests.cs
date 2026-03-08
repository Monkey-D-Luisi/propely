// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Propely.OrgsApi.Client;
using Propely.OrgsApi.Client.Agencies;
using Propely.OrgsApi.Client.Permissions;
using Propely.Shared.Http;

namespace Propely.OrgsApi.IntegrationTests.Client;

/// <summary>
/// Integration tests for the Orgs API SDK client, verifying that
/// the full DI pipeline (registration, handler chain, header propagation) works end-to-end.
/// </summary>
public sealed class OrgsApiClientIntegrationTests
{
    [Fact]
    public void AddOrgsApiClient_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddOrgsApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5020";
        });

        var provider = services.BuildServiceProvider();

        // Act & Assert
        var client = provider.GetService<IOrgsApiClient>();
        client.Should().NotBeNull("IOrgsApiClient should be registered in DI");

        var handler = provider.GetService<TenantDelegatingHandler>();
        handler.Should().NotBeNull("TenantDelegatingHandler should be registered in DI");

        var permissionsApi = provider.GetService<IPermissionsApi>();
        permissionsApi.Should().NotBeNull("IPermissionsApi should be registered in DI");

        var agenciesApi = provider.GetService<IAgenciesApi>();
        agenciesApi.Should().NotBeNull("IAgenciesApi should be registered in DI");

        var permissionGuard = provider.GetService<IPermissionGuard>();
        permissionGuard.Should().NotBeNull("IPermissionGuard should be registered in DI");
    }

    [Fact]
    public async Task TenantDelegatingHandler_PropagatesTenantId_ThroughDIPipeline()
    {
        // Arrange
        var tenantId = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();

        // Simulate an incoming HTTP request context with the tenant header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantDelegatingHandler.TenantIdHeaderName] = tenantId;
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

        services.AddOrgsApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5020";
        });

        var provider = services.BuildServiceProvider();

        // Act - Resolve the handler from DI and verify it propagates the header
        var handler = provider.GetRequiredService<TenantDelegatingHandler>();
        handler.InnerHandler = new HeaderCapturingHandler();
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5020/orgs/mine");
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var capturedHandler = (HeaderCapturingHandler)handler.InnerHandler;
        capturedHandler.CapturedHeaders.Should().ContainKey(TenantDelegatingHandler.TenantIdHeaderName);
        capturedHandler.CapturedHeaders[TenantDelegatingHandler.TenantIdHeaderName]
            .Should().Be(tenantId);
    }

    [Fact]
    public async Task TenantDelegatingHandler_WhenNoTenantId_DoesNotAddHeader()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Simulate an incoming HTTP request context WITHOUT the tenant header
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

        services.AddOrgsApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5020";
        });

        var provider = services.BuildServiceProvider();

        // Act
        var handler = provider.GetRequiredService<TenantDelegatingHandler>();
        handler.InnerHandler = new HeaderCapturingHandler();
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5020/orgs/mine");
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var capturedHandler = (HeaderCapturingHandler)handler.InnerHandler;
        capturedHandler.CapturedHeaders.Should().NotContainKey(TenantDelegatingHandler.TenantIdHeaderName);
    }

    [Fact]
    public void OrgsApiClientOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new OrgsApiClientOptions();

        // Assert
        options.BaseUrl.Should().Be("http://localhost:5020");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.RetryCount.Should().Be(3);
        options.CircuitBreakerFailureThreshold.Should().Be(5);
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// A handler that captures outgoing request headers for assertion.
    /// </summary>
    private sealed class HeaderCapturingHandler : HttpMessageHandler
    {
        public Dictionary<string, string> CapturedHeaders { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            foreach (var header in request.Headers)
            {
                CapturedHeaders[header.Key] = string.Join(",", header.Value);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
