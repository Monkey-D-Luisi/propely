// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Infrastructure.Caching;
using Propely.PropertiesApi.Infrastructure.Messaging;
using Propely.PropertiesApi.Infrastructure.Persistence;
using Propely.PropertiesApi.Infrastructure.Storage;

namespace Propely.PropertiesApi.IntegrationTests.Fixtures;

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
                ["OutboxDispatcher:Enabled"] = "false",
                ["Redis:ConnectionString"] = "",
                ["RabbitMQ:Host"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext and Npgsql provider to switch to InMemory
            var descriptorsToRemove = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                  || d.ServiceType == typeof(DbContextOptions)
                  || (d.ServiceType.IsGenericType &&
                      d.ServiceType.GetGenericTypeDefinition().FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                  || d.ServiceType.FullName?.StartsWith("Npgsql") == true
                  || d.ImplementationType?.FullName?.StartsWith("Npgsql") == true)
                .ToList();
            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            // Re-register AppDbContext with InMemory provider
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase("PropertiesTestDb");
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

            // Replace storage service with mock
            services.RemoveAll<GcpStorageService>();
            services.RemoveAll<IStorageService>();
            var mockStorageService = Substitute.For<IStorageService>();
            services.AddSingleton(mockStorageService);

            // Replace image processing service with mock
            services.RemoveAll<ImageProcessingService>();
            services.RemoveAll<IImageProcessingService>();
            var mockImageService = Substitute.For<IImageProcessingService>();
            services.AddSingleton(mockImageService);
        });
    }
}
