// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Infrastructure.Caching;
using Propely.AiApi.Infrastructure.Messaging;
using Propely.AiApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Propely.AiApi.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that uses Testcontainers for Postgres.
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    private string _connectionString = string.Empty;

    public ApiWebApplicationFactory()
    {
        // Load .env file from solution root (searches upwards)
        DotNetEnv.Env.TraversePath().Load();
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set testing environment to bypass connection string validation in Program.cs
        builder.UseEnvironment("Testing");

        // Configure test settings including anonymous access and fast outbox polling
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Strip AIAPI_ prefix from environment variables (same as Program.cs)
            config.AddEnvironmentVariables("AIAPI_");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AllowAnonymous"] = "true",
                ["OutboxDispatcher:PollingIntervalSeconds"] = "1",
                ["OutboxDispatcher:Enabled"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Add test DbContext with Testcontainers connection
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Remove RabbitMQ Consumer Service (WorkItemProjectorService)
            // We do NOT want to remove OutboxDispatcherService, so we filter by implementation type if possible
            // But Hosted Services are added as IHostedService.
            
            // Find the specific descriptors
            var servicesToRemove = services.Where(s => s.ServiceType == typeof(IHostedService)).ToList();
            foreach (var serviceDesc in servicesToRemove)
            {
                // We want to remove WorkItemProjectorService (RabbitMQ Consumer)
                // We want to KEEP OutboxDispatcherService
                if (serviceDesc.ImplementationType?.Name == "WorkItemProjectorService") 
                {
                    services.Remove(serviceDesc);
                }
                // Note: If OutboxDispatcherService is also an IHostedService, we intentionally keep it.
                // However, unit tests often remove all background services to avoid noise.
                // Here we NEED OutboxDispatcherService to move messages to our InMemoryPublisher.
            }

            // Replace RabbitMQ publisher with InMemoryMessagePublisher
            services.RemoveAll<RabbitMqPublisher>();
            services.RemoveAll<IMessagePublisher>();
            
            services.AddSingleton<IMessagePublisher, Propely.AiApi.IntegrationTests.Infrastructure.InMemoryMessagePublisher>();

            // Replace Redis cache service with a mock
            services.RemoveAll<RedisCacheService>();
            services.RemoveAll<ICacheService>();
            var mockCacheService = Substitute.For<ICacheService>();
            services.AddSingleton(mockCacheService);
        });
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        // Force the host to start and ensure database schema is created
        // Access Server property to trigger host creation
        _ = Server;

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}
