// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.AuditLogs.Interfaces;
using Propely.OrgsApi.Application.Billing;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Infrastructure.Billing;
using Propely.OrgsApi.Infrastructure.Caching;
using Propely.OrgsApi.Infrastructure.Caching.Configuration;
using Propely.OrgsApi.Infrastructure.Email;
using Propely.OrgsApi.Infrastructure.Messaging;
using Propely.OrgsApi.Infrastructure.Messaging.Configuration;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.Infrastructure.Persistence.Repositories;
using Propely.OrgsApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Propely.OrgsApi.Infrastructure;

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
                "Set ORGSAPI_ConnectionStrings__DefaultConnection environment variable or ConnectionStrings:DefaultConnection in appsettings.json.");
        }

        services.AddDbContextPool<AppDbContext>(options =>
            // If connection string is missing in test env, we pass empty string but Testcontainers implies it won't be used directly there
            // or the WebApplicationFactory replaces it.
            options.UseNpgsql(connectionString ?? "", b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserExternalLoginRepository, UserExternalLoginRepository>();
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPermissionOverrideRepository, PermissionOverrideRepository>();

        // Feature Flags
        services.Configure<Dictionary<string, bool>>(configuration.GetSection("FeatureFlags"));
        services.AddSingleton<IFeatureFlagDefaults, FeatureFlagDefaults>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        // Auth services
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Email
        services.AddSingleton<IEmailTemplateRenderer, RazorEmailTemplateRenderer>();
        services.AddSingleton<IEmailService, EmailService>();

        var emailProvider = configuration.GetValue<string>("Email:Provider") ?? "smtp";
        if (string.Equals(emailProvider, "sendgrid", StringComparison.OrdinalIgnoreCase))
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "SendGrid API key not configured. " +
                    "Set ORGSAPI_SendGrid__ApiKey environment variable or SendGrid:ApiKey in appsettings.json.");
            }

            apiKey = apiKey.Trim();
            services.AddSingleton<SendGrid.ISendGridClient>(new SendGrid.SendGridClient(apiKey));
            services.AddSingleton<IEmailSender, SendGridEmailSender>();
        }
        else
        {
            var smtpHost = configuration["Smtp:Host"];
            if (!isTestEnvironment && string.IsNullOrWhiteSpace(smtpHost))
            {
                throw new InvalidOperationException(
                    "SMTP host not configured. " +
                    "Set ORGSAPI_Smtp__Host environment variable or Smtp:Host in appsettings.json.");
            }

            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }

        // Billing
        services.Configure<BillingConfiguration>(configuration.GetSection(BillingConfiguration.SectionName));
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddSingleton<IPlanProvider, PlanProvider>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddSingleton<IWebhookEventMapper, WebhookEventMapper>();

        var billingMode = configuration.GetValue<string>("Billing:Mode") ?? "free";
        if (string.Equals(billingMode, "stripe", StringComparison.OrdinalIgnoreCase))
        {
            var stripeSecretKey = configuration["Billing:Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(stripeSecretKey))
            {
                throw new InvalidOperationException(
                    "Stripe secret key not configured. " +
                    "Set ORGSAPI_Billing__Stripe__SecretKey environment variable or Billing:Stripe:SecretKey in appsettings.json.");
            }

            var stripeWebhookSecret = configuration["Billing:Stripe:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(stripeWebhookSecret))
            {
                throw new InvalidOperationException(
                    "Stripe webhook secret not configured. " +
                    "Set ORGSAPI_Billing__Stripe__WebhookSecret environment variable or Billing:Stripe:WebhookSecret in appsettings.json.");
            }

            services.AddScoped<IPaymentService, StripePaymentService>();
        }
        else
        {
            services.AddScoped<IPaymentService, NoOpPaymentService>();
        }

        // RabbitMQ
        services.Configure<RabbitMqConfiguration>(configuration.GetSection(RabbitMqConfiguration.SectionName));
        services.Configure<OutboxDispatcherConfiguration>(configuration.GetSection(OutboxDispatcherConfiguration.SectionName));
        services.Configure<ProjectorConfiguration>(configuration.GetSection(ProjectorConfiguration.SectionName));

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
