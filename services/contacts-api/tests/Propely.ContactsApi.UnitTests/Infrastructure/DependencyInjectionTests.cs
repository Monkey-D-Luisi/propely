// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Infrastructure;

namespace Propely.ContactsApi.UnitTests.Infrastructure;

public class DependencyInjectionTests
{
    private static IConfiguration BuildTestConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Testing"] = "true"
            })
            .Build();

    private static IHostEnvironment BuildTestEnvironment()
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns("Testing");
        return env;
    }

    [Fact]
    public void AddInfrastructureServices_RegistersRepositories()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildTestConfiguration();
        var environment = BuildTestEnvironment();

        services.AddInfrastructureServices(configuration, environment);

        services.Any(d => d.ServiceType == typeof(IContactRepository)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(IContactReadRepository)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(ILeadRepository)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(ILeadReadRepository)).Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructureServices_InTestEnvironment_DoesNotThrowWhenConnectionStringMissing()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildTestConfiguration();
        var environment = BuildTestEnvironment();

        var act = () => services.AddInfrastructureServices(configuration, environment);

        act.Should().NotThrow();
    }
}
