// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Infrastructure.Caching;
using Propely.AppointmentsApi.Infrastructure.Caching.Configuration;
using Propely.AppointmentsApi.Infrastructure.Calendar;
using Propely.AppointmentsApi.Infrastructure.Messaging;
using Propely.AppointmentsApi.Infrastructure.Messaging.Configuration;
using Propely.AppointmentsApi.Infrastructure.Persistence;
using Propely.AppointmentsApi.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Propely.AppointmentsApi.Infrastructure;

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
                "Set APPOINTMENTSAPI_ConnectionStrings__DefaultConnection environment variable or ConnectionStrings:DefaultConnection in appsettings.json.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString ?? "", b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentReadRepository, AppointmentReadRepository>();
        services.AddScoped<ICalendarConnectionRepository, CalendarConnectionRepository>();
        services.AddScoped<ISyncOperationRepository, SyncOperationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

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

        // Calendar Integration
        services.Configure<CalendarConfiguration>(configuration.GetSection(CalendarConfiguration.SectionName));

        // Data Protection for token encryption
        services.AddDataProtection()
            .SetApplicationName("Propely.AppointmentsApi");

        services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
        services.AddScoped<ICalendarTokenExchangeService, CalendarTokenExchangeService>();

        // Calendar sync services (registered as ICalendarSyncService collection)
        services.AddScoped<ICalendarSyncService, GoogleCalendarSyncService>();
        services.AddScoped<ICalendarSyncService, MicrosoftCalendarSyncService>();

        // Calendar sync orchestrator
        services.AddScoped<CalendarSyncOrchestrator>();

        // Background worker (only in non-testing environments)
        if (!isTestEnvironment)
        {
            services.AddHostedService<CalendarSyncWorker>();
        }

        return services;
    }
}
