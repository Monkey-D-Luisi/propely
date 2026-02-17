// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api;
using SaasTemplate.OrgsApi.Api.Configuration;
using SaasTemplate.OrgsApi.Api.Middleware;
using SaasTemplate.OrgsApi.Application;
using SaasTemplate.OrgsApi.Infrastructure;
using AspNetCoreRateLimit; // For UseIpRateLimiting

var builder = WebApplication.CreateBuilder(args);

// Strip ORGSAPI_ prefix from environment variables and map to IConfiguration
// e.g. ORGSAPI_RabbitMQ__Host → configuration["RabbitMQ:Host"]
builder.Configuration.AddEnvironmentVariables("ORGSAPI_");

// Add Application Services
builder.Services.AddApplicationServices();

// Add Infrastructure Services
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Add Api Services (Auth, Telemetry, etc.)
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Apply pending EF Core migrations (with advisory lock for concurrency safety)
await app.ApplyMigrationsAsync();

// Middleware pipeline
// Configure forwarded headers for reverse proxy support (must be first)
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
// Cloud Run (and similar PaaS) terminates TLS at the load balancer.
// Clear known-proxy restrictions so X-Forwarded-Proto is trusted from any source.
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Swagger UI for API exploration (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Correlation ID for request tracing (early in pipeline)
app.UseCorrelationId();

// Health checks before exception handler so they return proper JSON responses
// (the health check middleware has its own error handling for dependency failures)
app.UseHealthCheckEndpoints();

// Global exception handler (after correlation ID so errors include it)
app.UseGlobalExceptionHandler();

// HTTPS redirection and HSTS for non-development/testing environments
if (!app.Environment.IsDevelopment() && app.Environment.EnvironmentName != "Testing")
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSecurityHeaders();
app.UseIpRateLimiting();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseActiveUserValidation();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Partial class for WebApplicationFactory integration tests.
/// </summary>
public partial class Program { }
