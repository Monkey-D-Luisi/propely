// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.PropertiesApi.Api.Middleware;
using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.UnitTests.Api.Middleware;

public sealed class ExceptionHandlerMiddlewareTests
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = Substitute.For<ILogger<ExceptionHandlerMiddleware>>();
    private readonly IHostEnvironment _environment = Substitute.For<IHostEnvironment>();

    private ExceptionHandlerMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionHandlerMiddleware(next, _logger, _environment);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(
            response.Body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(CreateHttpContext());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Item not found."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Not Found");
        problem.Detail.Should().Be("Item not found.");
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictException_ShouldReturn409()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new ConflictException("Already exists."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Conflict");
    }

    [Fact]
    public async Task InvokeAsync_WhenForbiddenException_ShouldReturn403()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new ForbiddenException("Not allowed."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Forbidden");
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantMismatchException_ShouldReturn403()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new TenantMismatchException("Wrong tenant."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn422WithErrors()
    {
        _environment.EnvironmentName.Returns("Production");
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        }.AsReadOnly();
        var middleware = CreateMiddleware(_ => throw new ValidationException(errors));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Validation Failed");
        problem.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainException_ShouldReturn400()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new DomainException("Bad input."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Bad Request");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldReturn500()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Something broke."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Internal Server Error");
        problem.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_InDevelopment_ShouldIncludeDetail()
    {
        _environment.EnvironmentName.Returns("Development");
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Something broke."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Detail.Should().Be("Something broke.");
        problem.Extensions.Should().ContainKey("exception");
    }

    [Fact]
    public async Task InvokeAsync_WhenOperationCanceled_ShouldReturn499()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException());
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status499ClientClosedRequest);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccess_ShouldReturn401()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException("No token."));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var problem = await ReadProblemDetails(context.Response);
        problem!.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetContentType()
    {
        _environment.EnvironmentName.Returns("Production");
        var middleware = CreateMiddleware(_ => throw new NotFoundException("x"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/problem+json");
    }
}
