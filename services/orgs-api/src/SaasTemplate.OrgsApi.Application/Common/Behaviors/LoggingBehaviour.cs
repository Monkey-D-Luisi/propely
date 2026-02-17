// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.OrgsApi.Application.Common.Behaviors;

public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string[] SensitiveKeywords =
    [
        "password",
        "token",
        "secret",
        "apikey",
        "authorization",
        "credential",
        "email",
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const long SlowRequestThresholdMs = 500;

    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sanitizedPayload = SanitizeRequest(request);

        _logger.LogInformation("Handling {RequestName} with payload {Payload}", requestName, sanitizedPayload);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request: {RequestName} completed in {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                requestName,
                elapsedMs,
                SlowRequestThresholdMs);
        }
        else
        {
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                requestName,
                elapsedMs);
        }

        return response;
    }

    private string SanitizeRequest(TRequest request)
    {
        try
        {
            var properties = typeof(TRequest).GetProperties();
            var sanitized = new Dictionary<string, object?>();

            foreach (var property in properties)
            {
                if (IsSensitive(property.Name))
                {
                    sanitized[property.Name] = "***REDACTED***";
                }
                else
                {
                    sanitized[property.Name] = property.GetValue(request);
                }
            }

            return JsonSerializer.Serialize(sanitized, SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to serialize request of type {RequestType}", typeof(TRequest).Name);
            return "[Could not serialize request]";
        }
    }

    private static bool IsSensitive(string propertyName)
    {
        foreach (var keyword in SensitiveKeywords)
        {
            if (propertyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
