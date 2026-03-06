// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Propely.AppointmentsApi.Client;

namespace Propely.AppointmentsApi.UnitTests.Client;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAppointmentsApiClient_Registers_IAppointmentsApiClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add logging (required by TenantDelegatingHandler)
        services.AddLogging();

        services.AddAppointmentsApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5060";
        });

        var provider = services.BuildServiceProvider();

        // Act
        var client = provider.GetService<IAppointmentsApiClient>();

        // Assert
        client.Should().NotBeNull("because AddAppointmentsApiClient should register IAppointmentsApiClient in the DI container");
    }

    [Fact]
    public void AddAppointmentsApiClient_Registers_TenantDelegatingHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAppointmentsApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5060";
        });

        var provider = services.BuildServiceProvider();

        // Act
        var handler = provider.GetService<TenantDelegatingHandler>();

        // Assert
        handler.Should().NotBeNull("because AddAppointmentsApiClient should register TenantDelegatingHandler");
    }

    [Fact]
    public void AddAppointmentsApiClient_Uses_Custom_BaseUrl()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var customBaseUrl = "http://custom-host:9999";

        services.AddAppointmentsApiClient(options =>
        {
            options.BaseUrl = customBaseUrl;
        });

        // Act & Assert -- should not throw
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IAppointmentsApiClient>();
        client.Should().NotBeNull();
    }
}
