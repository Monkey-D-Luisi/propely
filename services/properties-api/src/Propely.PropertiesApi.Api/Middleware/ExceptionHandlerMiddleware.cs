// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.PropertiesApi.Api.Services;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Propely.PropertiesApi.Api.Middleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = MapException(exception);

        LogException(exception, statusCode, context);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Response already started, cannot write ProblemDetails for {ExceptionType}",
                exception.GetType().Name);
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.ToString();
        }

        if (statusCode == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment())
        {
            problemDetails.Detail = "An unexpected error occurred. Please try again later.";
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        try
        {
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails, JsonOptions));
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Failed"),
        DomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
        OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Client Closed Request"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    private void LogException(Exception exception, int statusCode, HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is not null && path.StartsWith("/health/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var correlationId = context.RequestServices?.GetService<ICorrelationIdAccessor>()?.CorrelationId
            ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception on {Method} {Path} [CorrelationId: {CorrelationId}]: {ExceptionType} — {Message}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                exception.GetType().Name,
                exception.Message);
        }
    }
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionHandlerMiddleware>();
}
