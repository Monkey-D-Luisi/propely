// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Infrastructure.Caching;
using Propely.AppointmentsApi.Infrastructure.Messaging;
using Propely.AppointmentsApi.Infrastructure.Persistence;

namespace Propely.AppointmentsApi.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for integration testing with in-memory database.
/// Uses DevScheme authentication to auto-authenticate requests with default user/org claims.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AllowAnonymous"] = "true",
                ["OutboxDispatcher:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString("N"));
            });

            // Replace RabbitMQ publisher with mock
            services.RemoveAll<RabbitMqPublisher>();
            services.RemoveAll<IMessagePublisher>();
            var mockPublisher = Substitute.For<IMessagePublisher>();
            services.AddSingleton(mockPublisher);

            // Replace Redis cache service with mock
            services.RemoveAll<RedisCacheService>();
            services.RemoveAll<ICacheService>();
            var mockCacheService = Substitute.For<ICacheService>();
            services.AddSingleton(mockCacheService);
        });
    }
}
