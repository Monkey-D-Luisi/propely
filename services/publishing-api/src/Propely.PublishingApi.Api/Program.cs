// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PublishingApi.Api;
using Propely.PublishingApi.Api.Configuration;
using Propely.PublishingApi.Api.Middleware;
using Propely.PublishingApi.Application;
using Propely.PublishingApi.Infrastructure;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("PUBLISHINGAPI_");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.ApplyMigrationsAsync();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorrelationId();
app.UseSecurityHeaders();
app.UseHealthCheckEndpoints();
app.UseGlobalExceptionHandler();

if (!app.Environment.IsDevelopment() && app.Environment.EnvironmentName != "Testing")
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseIpRateLimiting();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseOrgContext();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Partial class for WebApplicationFactory integration tests.
/// </summary>
public partial class Program { }
