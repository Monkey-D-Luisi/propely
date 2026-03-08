// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.AiApi.Application.WorkItems.Interfaces;
using Propely.AiApi.Infrastructure.AI;
using Propely.AiApi.Infrastructure.AI.Adapters;
using Propely.AiApi.Infrastructure.Caching;
using Propely.AiApi.Infrastructure.Caching.Configuration;
using Propely.AiApi.Infrastructure.Messaging;
using Propely.AiApi.Infrastructure.Messaging.Configuration;
using Propely.AiApi.Infrastructure.Persistence;
using Propely.AiApi.Infrastructure.Persistence.Repositories;
using Propely.AiApi.Infrastructure.Services;
using Propely.PropertiesApi.Client;
using Propely.ContactsApi.Client;
using Propely.AppointmentsApi.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Propely.AiApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        // EF Core with Npgsql
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Validate connection string in non-test environments
        var isTestEnvironment = environment.EnvironmentName == "Testing" || configuration["Testing"] == "true";
        if (!isTestEnvironment && string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not configured. " +
                "Set AIAPI_ConnectionStrings__DefaultConnection environment variable or ConnectionStrings:DefaultConnection in appsettings.json.");
        }

        services.AddDbContext<AppDbContext>(options =>
            // If connection string is missing in test env, we pass empty string but Testcontainers implies it won't be used directly there
            // or the WebApplicationFactory replaces it.
            options.UseNpgsql(connectionString ?? "", b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        services.AddScoped<IWorkItemReadRepository, WorkItemReadRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // RabbitMQ
        services.Configure<RabbitMqConfiguration>(configuration.GetSection(RabbitMqConfiguration.SectionName));
        services.Configure<OutboxDispatcherConfiguration>(configuration.GetSection(OutboxDispatcherConfiguration.SectionName));
        services.Configure<ProjectorConfiguration>(configuration.GetSection(ProjectorConfiguration.SectionName));

        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqPublisher>());
        services.AddHostedService<OutboxDispatcherService>();

        // Event Projector
        services.AddSingleton<IEventProjector, WorkItemEventProjector>();
        services.AddHostedService<WorkItemProjectorService>();

        // Redis Caching
        services.Configure<RedisConfiguration>(configuration.GetSection(RedisConfiguration.SectionName));
        services.AddSingleton<RedisCacheService>();
        services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RedisCacheService>());
        services.AddSingleton<ICacheSettings>(sp => sp.GetRequiredService<IOptions<RedisConfiguration>>().Value);

        // OpenAI Service
        // We register this here as part of Infrastructure
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton<IOpenAiService, OpenAiService>();

        // Voice Transcription Service (OpenAI Audio API)
        services.AddScoped<IVoiceTranscriptionService, OpenAiVoiceTranscriptionService>();

        // AI Action Engine (Tool Schema Registry, Adapter, Intent Classifier, Action Router)
        services.AddSingleton<IToolSchemaRegistry, ToolSchemaRegistry>();
        services.AddSingleton<OpenAiToolAdapter>();
        services.AddScoped<IIntentClassifier, OpenAiIntentClassifier>();
        services.AddScoped<IActionRouter, ActionRouter>();

        // Conversation Context (Redis-backed session storage for multi-turn conversations)
        services.Configure<ConversationContextOptions>(configuration.GetSection(ConversationContextOptions.SectionName));
        services.AddScoped<IConversationContext, RedisConversationContext>();

        // Proactive Suggestion Engine (rule-based business suggestions)
        services.AddScoped<ISuggestionRule, StaleLeadsRule>();
        services.AddScoped<ISuggestionRule, DraftPropertyRule>();
        services.AddScoped<ISuggestionRule, EmptyCalendarRule>();
        services.AddScoped<ISuggestionRule, LowConversionRule>();
        services.AddScoped<ISuggestionRule, GroupedViewingRule>();
        services.AddScoped<SuggestionEngine>();

        // Properties API SDK Client (cross-service communication)
        services.AddPropertiesApiClient(options =>
        {
            options.BaseUrl = configuration["PropertiesApi:BaseUrl"] ?? "http://localhost:5030";
        });

        // Contacts API SDK Client (cross-service communication)
        services.AddContactsApiClient(options =>
        {
            options.BaseUrl = configuration["ContactsApi:BaseUrl"] ?? "http://localhost:5050";
        });

        // Appointments API SDK Client (cross-service communication)
        services.AddAppointmentsApiClient(options =>
        {
            options.BaseUrl = configuration["AppointmentsApi:BaseUrl"] ?? "http://localhost:5060";
        });

        return services;
    }
}
