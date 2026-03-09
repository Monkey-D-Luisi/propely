// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Infrastructure.Caching;
using Propely.AiApi.Infrastructure.Messaging;
using Propely.AiApi.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Propely.AiApi.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory that replaces the real IVoiceTranscriptionService with a mock.
/// Mirrors ApiWebApplicationFactory setup but adds mock transcription service.
/// </summary>
public sealed class VoiceApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    private string _connectionString = string.Empty;

    /// <summary>
    /// The NSubstitute mock for IVoiceTranscriptionService.
    /// Configure this in each test to control transcription output.
    /// </summary>
    public IVoiceTranscriptionService MockTranscriptionService { get; } = Substitute.For<IVoiceTranscriptionService>();

    public VoiceApiWebApplicationFactory()
    {
        DotNetEnv.Env.TraversePath().Load();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddEnvironmentVariables("AIAPI_");
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AllowAnonymous"] = "true",
                ["OutboxDispatcher:PollingIntervalSeconds"] = "1",
                ["OutboxDispatcher:Enabled"] = "true",
                ["Redis:ConnectionString"] = "",
                ["RabbitMQ:Host"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with Testcontainers Postgres
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Remove RabbitMQ consumer
            var servicesToRemove = services
                .Where(s => s.ServiceType == typeof(IHostedService))
                .Where(s => s.ImplementationType?.Name == "WorkItemProjectorService")
                .ToList();
            foreach (var svc in servicesToRemove) services.Remove(svc);

            // Replace RabbitMQ publisher
            services.RemoveAll<RabbitMqPublisher>();
            services.RemoveAll<IMessagePublisher>();
            services.AddSingleton<IMessagePublisher, Infrastructure.InMemoryMessagePublisher>();

            // Replace Redis cache
            services.RemoveAll<RedisCacheService>();
            services.RemoveAll<ICacheService>();
            services.AddSingleton(Substitute.For<ICacheService>());

            // Replace voice transcription with mock
            services.RemoveAll<IVoiceTranscriptionService>();
            services.AddSingleton(MockTranscriptionService);
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
        _ = Server;

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}
