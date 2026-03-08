// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Propely.AiApi.Client;
using Propely.AiApi.Client.Configuration;
using Propely.Shared.Http;

namespace Propely.AiApi.UnitTests.Client;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAiApiClient_RegistersIActionApi()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddAiApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5010";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IActionApi));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddAiApiClient_RegistersIVoiceApi()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddAiApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5010";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IVoiceApi));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddAiApiClient_RegistersIContentApi()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddAiApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5010";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IContentApi));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddAiApiClient_RegistersTenantDelegatingHandler()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddAiApiClient(options =>
        {
            options.BaseUrl = "http://localhost:5010";
        });

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(TenantDelegatingHandler));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddAiApiClient_WithCustomOptions_AppliesConfiguration()
    {
        var services = new ServiceCollection();
        var configuredBaseUrl = "http://custom-host:9999";

        services.AddLogging();
        services.AddAiApiClient(options =>
        {
            options.BaseUrl = configuredBaseUrl;
            options.Timeout = TimeSpan.FromSeconds(120);
        });

        var actionDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IActionApi));
        var voiceDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IVoiceApi));
        var contentDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IContentApi));

        actionDescriptor.Should().NotBeNull("IActionApi should be registered");
        voiceDescriptor.Should().NotBeNull("IVoiceApi should be registered");
        contentDescriptor.Should().NotBeNull("IContentApi should be registered");
    }

    [Fact]
    public void AiApiClientOptions_HasCorrectDefaults()
    {
        var options = new AiApiClientOptions();

        options.BaseUrl.Should().Be("http://localhost:5010");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
    }
}
