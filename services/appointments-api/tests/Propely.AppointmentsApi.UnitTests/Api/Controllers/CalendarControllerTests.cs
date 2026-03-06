// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AppointmentsApi.Api.Controllers;
using Propely.AppointmentsApi.Application.Appointments.Commands.ConnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Commands.DisconnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetCalendarStatus;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Api.Controllers;

public class CalendarControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<CalendarController> _logger = Substitute.For<ILogger<CalendarController>>();
    private readonly CalendarController _controller;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public CalendarControllerTests()
    {
        _controller = new CalendarController(_mediator, _logger);
        SetAuthenticatedUser(UserId, TenantId);
    }

    private void SetAuthenticatedUser(Guid? userId, Guid? tenantId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim("sub", userId.Value.ToString()));
        if (tenantId.HasValue)
            claims.Add(new Claim("org_id", tenantId.Value.ToString()));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }

    private static CalendarConnectionDto MakeConnectionDto(CalendarProvider provider = CalendarProvider.Google) => new()
    {
        Id = Guid.NewGuid(),
        AgentId = UserId,
        Provider = provider,
        SyncState = CalendarSyncState.Active,
        ExternalCalendarId = "primary",
        CreatedAtUtc = DateTime.UtcNow
    };

    // --- GetStatus ---

    [Fact]
    public async Task GetStatus_WithValidClaims_ReturnsOk()
    {
        var connections = new List<CalendarConnectionDto> { MakeConnectionDto() };
        _mediator.Send(Arg.Any<GetCalendarStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(connections);

        var result = (OkObjectResult)await _controller.GetStatus(CancellationToken.None);

        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(connections);
    }

    [Fact]
    public async Task GetStatus_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.GetStatus(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetStatus_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.GetStatus(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Connect ---

    [Fact]
    public async Task Connect_WithValidClaims_Returns201()
    {
        var dto = MakeConnectionDto();
        _mediator.Send(Arg.Any<ConnectCalendarCommand>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        var request = new ConnectCalendarApiRequest
        {
            AuthorizationCode = "auth-code-123",
            RedirectUri = "https://app.propely.com/callback"
        };

        var result = (ObjectResult)await _controller.Connect(
            CalendarProvider.Google, request, CancellationToken.None);

        result.StatusCode.Should().Be(201);
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Connect_SendsCorrectCommand()
    {
        var dto = MakeConnectionDto();
        _mediator.Send(Arg.Any<ConnectCalendarCommand>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        var request = new ConnectCalendarApiRequest
        {
            AuthorizationCode = "my-auth-code",
            RedirectUri = "https://app.propely.com/callback"
        };

        await _controller.Connect(CalendarProvider.Google, request, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<ConnectCalendarCommand>(c =>
                c.AgentId == UserId &&
                c.TenantId == TenantId &&
                c.Provider == CalendarProvider.Google &&
                c.AuthorizationCode == "my-auth-code" &&
                c.RedirectUri == "https://app.propely.com/callback"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Connect_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.Connect(
            CalendarProvider.Google,
            new ConnectCalendarApiRequest { AuthorizationCode = "code", RedirectUri = "uri" },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Connect_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Connect(
            CalendarProvider.Google,
            new ConnectCalendarApiRequest { AuthorizationCode = "code", RedirectUri = "uri" },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Disconnect ---

    [Fact]
    public async Task Disconnect_WithValidClaims_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DisconnectCalendarCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        var result = await _controller.Disconnect(CalendarProvider.Google, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Disconnect_SendsCorrectCommand()
    {
        _mediator.Send(Arg.Any<DisconnectCalendarCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        await _controller.Disconnect(CalendarProvider.Microsoft, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<DisconnectCalendarCommand>(c =>
                c.AgentId == UserId &&
                c.TenantId == TenantId &&
                c.Provider == CalendarProvider.Microsoft),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Disconnect_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.Disconnect(CalendarProvider.Google, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Disconnect_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Disconnect(CalendarProvider.Google, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Webhook ---

    [Fact]
    public void Webhook_ReturnsOk()
    {
        var result = _controller.Webhook(CalendarProvider.Google);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void Webhook_MicrosoftProvider_ReturnsOk()
    {
        var result = _controller.Webhook(CalendarProvider.Microsoft);

        result.Should().BeOfType<OkResult>();
    }
}
