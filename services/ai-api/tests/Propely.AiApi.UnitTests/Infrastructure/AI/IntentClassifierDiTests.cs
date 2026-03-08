// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Infrastructure;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Infrastructure.AI;

public sealed class IntentClassifierDiTests
{
    [Fact]
    public void Resolving_IIntentClassifier_WithOpenAiProvider_ReturnsOpenAiIntentClassifier()
    {
        // Arrange
        var services = CreateServiceCollection("OpenAI");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var classifier = scope.ServiceProvider.GetService<IIntentClassifier>();

        // Assert
        classifier.Should().NotBeNull();
        classifier.Should().BeOfType<OpenAiIntentClassifier>();
    }

    [Fact]
    public void Resolving_IIntentClassifier_WithDefaultConfig_ReturnsOpenAiIntentClassifier()
    {
        // Arrange — no AiProvider configured, should default to OpenAI
        var services = CreateServiceCollection(null);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var classifier = scope.ServiceProvider.GetService<IIntentClassifier>();

        // Assert
        classifier.Should().NotBeNull();
        classifier.Should().BeOfType<OpenAiIntentClassifier>();
    }

    [Fact]
    public void Resolving_KeyedService_WithOpenAiKey_ReturnsOpenAiIntentClassifier()
    {
        // Arrange
        var services = CreateServiceCollection("OpenAI");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var classifier = scope.ServiceProvider.GetKeyedService<IIntentClassifier>("OpenAI");

        // Assert
        classifier.Should().NotBeNull();
        classifier.Should().BeOfType<OpenAiIntentClassifier>();
    }

    [Fact]
    public void Resolving_IIntentClassifier_WithUnknownProvider_ThrowsOnResolve()
    {
        // Arrange
        var services = CreateServiceCollection("UnknownProvider");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var act = () => scope.ServiceProvider.GetRequiredService<IIntentClassifier>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static IServiceCollection CreateServiceCollection(string? aiProvider)
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Testing"] = "true",
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
            ["Redis:ConnectionString"] = "",
            ["RabbitMQ:Host"] = ""
        };

        if (aiProvider != null)
        {
            configValues["AiProvider"] = aiProvider;
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Minimal environment stub
        var env = new TestHostEnvironment { EnvironmentName = "Testing" };
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(env);

        services.AddInfrastructureServices(config, env);

        return services;
    }

    private sealed class TestHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = ".";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
