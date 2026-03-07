// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Infrastructure.Caching;
using Propely.OrgsApi.Infrastructure.Messaging;
using Propely.OrgsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Propely.OrgsApi.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that uses Testcontainers for Postgres.
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string JwtSecret = "integration-test-secret-key-that-is-at-least-32-bytes-long-for-hmac-sha256";

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

        // OAuth handlers are registered during service configuration, so provide
        // credentials via process env vars before host creation.
        Environment.SetEnvironmentVariable("ORGSAPI_OAuth__Google__ClientId", "test-google-client-id");
        Environment.SetEnvironmentVariable("ORGSAPI_OAuth__Google__ClientSecret", "test-google-client-secret");
        Environment.SetEnvironmentVariable("ORGSAPI_OAuth__GitHub__ClientId", "test-github-client-id");
        Environment.SetEnvironmentVariable("ORGSAPI_OAuth__GitHub__ClientSecret", "test-github-client-secret");
        Environment.SetEnvironmentVariable("ORGSAPI_Auth__FrontendBaseUrl", "http://localhost:3000");
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set testing environment to bypass connection string validation in Program.cs
        builder.UseEnvironment("Testing");

        // Configure test settings including anonymous access and fast outbox polling
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Strip ORGSAPI_ prefix from environment variables (same as Program.cs)
            config.AddEnvironmentVariables("ORGSAPI_");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AllowAnonymous"] = "true",
                ["OutboxDispatcher:PollingIntervalSeconds"] = "1",
                ["OutboxDispatcher:Enabled"] = "true",
                ["Jwt:Secret"] = JwtSecret,
                ["Auth:FrontendBaseUrl"] = "http://localhost:3000",
                ["OAuth:Google:ClientId"] = "test-google-client-id",
                ["OAuth:Google:ClientSecret"] = "test-google-client-secret",
                ["OAuth:GitHub:ClientId"] = "test-github-client-id",
                ["OAuth:GitHub:ClientSecret"] = "test-github-client-secret",
                // Disable infrastructure health checks in CI (no Redis/RabbitMQ available)
                ["Redis:ConnectionString"] = "",
                ["RabbitMQ:Host"] = "",
                // Billing configuration for integration tests
                ["Billing:Mode"] = "free",
                ["Billing:Stripe:WebhookSecret"] = "whsec_test_integration_secret",
                ["Billing:Plans:0:Id"] = "free",
                ["Billing:Plans:0:Name"] = "Free",
                ["Billing:Plans:0:MaxMembers"] = "5",
                ["Billing:Plans:0:MaxOrganizations"] = "1",
                ["Billing:Plans:0:Features:0"] = "5 members",
                ["Billing:Plans:1:Id"] = "pro",
                ["Billing:Plans:1:Name"] = "Pro",
                ["Billing:Plans:1:StripePriceId"] = "price_pro_test",
                ["Billing:Plans:1:MaxMembers"] = "50",
                ["Billing:Plans:1:MaxOrganizations"] = "10",
                ["Billing:Plans:1:Features:0"] = "50 members",
                ["Billing:Plans:1:Features:1"] = "Priority support"
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

            // Replace RabbitMQ publisher with InMemoryMessagePublisher
            services.RemoveAll<RabbitMqPublisher>();
            services.RemoveAll<IMessagePublisher>();
            
            services.AddSingleton<IMessagePublisher, Propely.OrgsApi.IntegrationTests.Infrastructure.InMemoryMessagePublisher>();

            // Replace Redis cache service with a mock
            services.RemoveAll<RedisCacheService>();
            services.RemoveAll<ICacheService>();
            var mockCacheService = Substitute.For<ICacheService>();
            services.AddSingleton(mockCacheService);

            // Replace email service with a mock
            services.RemoveAll<IEmailService>();
            var mockEmailService = Substitute.For<IEmailService>();
            services.AddSingleton(mockEmailService);

            // Replace payment service with a mock (billing enabled for integration tests)
            services.RemoveAll<IPaymentService>();
            var mockPaymentService = Substitute.For<IPaymentService>();
            mockPaymentService.IsEnabled.Returns(true);
            mockPaymentService
                .CreateCheckoutSessionAsync(
                    Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns("https://checkout.stripe.com/test-session");
            mockPaymentService
                .CreateCustomerPortalSessionAsync(
                    Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns("https://billing.stripe.com/test-portal");
            services.AddSingleton(mockPaymentService);

            // Override DevScheme: use JWT Bearer as default for real auth flow in endpoint tests.
            // NOTE: The app's AddAuthenticationAndAuthorization reads Jwt:Secret at service registration
            // time (before ConfigureAppConfiguration runs), so useJwt=false and AddJwtBearer() is called
            // without options. We must configure JWT Bearer options here explicitly.
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtKey,
                    ValidateIssuer = true,
                    ValidIssuer = "orgs-api",
                    ValidateAudience = true,
                    ValidAudience = "propely",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
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
