// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Infrastructure.Caching;
using Propely.PropertiesApi.Infrastructure.Caching.Configuration;
using Propely.PropertiesApi.Infrastructure.Messaging;
using Propely.PropertiesApi.Infrastructure.Messaging.Configuration;
using Propely.PropertiesApi.Infrastructure.Persistence;
using Propely.PropertiesApi.Infrastructure.Persistence.Repositories;
using Propely.PropertiesApi.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Propely.PropertiesApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        // EF Core with Npgsql
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var isTestEnvironment = environment.EnvironmentName == "Testing" || configuration["Testing"] == "true";
        if (!isTestEnvironment && string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not configured. " +
                "Set PROPERTIESAPI_ConnectionStrings__DefaultConnection environment variable or ConnectionStrings:DefaultConnection in appsettings.json.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString ?? "", b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyReadRepository, PropertyReadRepository>();
        services.AddScoped<IPropertyMediaRepository, PropertyMediaRepository>();

        // Storage & Image Processing
        services.AddSingleton<IStorageService, GcpStorageService>();
        services.AddSingleton<IImageProcessingService, ImageProcessingService>();

        // RabbitMQ
        services.Configure<RabbitMqConfiguration>(configuration.GetSection(RabbitMqConfiguration.SectionName));
        services.Configure<OutboxDispatcherConfiguration>(configuration.GetSection(OutboxDispatcherConfiguration.SectionName));

        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqPublisher>());
        services.AddHostedService<OutboxDispatcherService>();

        // Redis Caching
        services.Configure<RedisConfiguration>(configuration.GetSection(RedisConfiguration.SectionName));
        services.AddSingleton<RedisCacheService>();
        services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RedisCacheService>());
        services.AddSingleton<ICacheSettings>(sp => sp.GetRequiredService<IOptions<RedisConfiguration>>().Value);

        return services;
    }
}
