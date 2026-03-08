// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Propely.PropertiesApi.Client;
using Propely.Shared.Http;

namespace Propely.PropertiesApi.UnitTests.Client;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPropertiesApiClient_RegistersIPropertiesApiClient()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddPropertiesApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5030";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IPropertiesApiClient));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddPropertiesApiClient_RegistersTenantDelegatingHandler()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddPropertiesApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5030";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(TenantDelegatingHandler));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddPropertiesApiClient_WithCustomOptions_AppliesConfiguration()
    {
        var services = new ServiceCollection();
        var configuredBaseUrl = "http://custom-host:9999";

        services.AddLogging();
        services.AddPropertiesApiClient(options =>
        {
            options.BaseUrl = configuredBaseUrl;
            options.RetryCount = 5;
            options.Timeout = TimeSpan.FromSeconds(60);
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IPropertiesApiClient));

        descriptor.Should().NotBeNull("IPropertiesApiClient should be registered");
    }
}
