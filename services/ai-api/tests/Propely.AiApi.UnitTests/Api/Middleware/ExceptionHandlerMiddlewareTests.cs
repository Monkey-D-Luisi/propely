// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.AiApi.Api.Middleware;
using Propely.AiApi.Domain.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Propely.AiApi.UnitTests.Api.Middleware;

public sealed class ExceptionHandlerMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Theory]
    [InlineData(typeof(NotFoundException), 404, "Not Found")]
    [InlineData(typeof(ConflictException), 409, "Conflict")]
    [InlineData(typeof(ForbiddenException), 403, "Forbidden")]
    [InlineData(typeof(DomainException), 400, "Bad Request")]
    [InlineData(typeof(UnauthorizedAccessException), 401, "Unauthorized")]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldReturnCorrectStatusCode(
        Type exceptionType, int expectedStatusCode, string expectedTitle)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error message")!;
        var (middleware, context) = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(expectedStatusCode);
        context.Response.ContentType.Should().Be("application/problem+json");

        var body = await ReadResponseBody(context);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, JsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(expectedStatusCode);
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Detail.Should().Be("Test error message");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnknownExceptionInProduction_ShouldReturnGenericMessage()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware(
            _ => throw new InvalidOperationException("Internal details"),
            isDevelopment: false);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);

        var body = await ReadResponseBody(context);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, JsonOptions);
        problemDetails!.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnknownExceptionInDevelopment_ShouldIncludeExceptionDetails()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware(
            _ => throw new InvalidOperationException("Internal details"),
            isDevelopment: true);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Internal details");
        body.Should().Contain("exception");
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldPassThrough()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware(_ =>
        {
            _.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_ShouldNotThrow()
    {
        // Arrange — write to response to trigger HasStarted, then throw
        var (middleware, context) = CreateMiddleware(async ctx =>
        {
            await ctx.Response.WriteAsync("partial response");
            throw new NotFoundException("Not found");
        });

        // Act — should not throw even though response already started
        var act = () => middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static (ExceptionHandlerMiddleware Middleware, DefaultHttpContext Context) CreateMiddleware(
        RequestDelegate next, bool isDevelopment = true)
    {
        var logger = NullLogger<ExceptionHandlerMiddleware>.Instance;
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(isDevelopment ? "Development" : "Production");

        var middleware = new ExceptionHandlerMiddleware(next, logger, environment);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return (middleware, context);
    }

    private static async Task<string> ReadResponseBody(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
