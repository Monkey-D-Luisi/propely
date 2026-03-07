// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.AppointmentsApi.Api.Extensions;
using Propely.AppointmentsApi.Application.Appointments.Commands.ConnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Commands.DisconnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetCalendarStatus;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Api.Controllers;

[ApiController]
[Route("api/calendar")]
[Authorize]
public sealed class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(IMediator mediator, ILogger<CalendarController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("status")]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var query = new GetCalendarStatusQuery(userId.Value, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{provider}/connect")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Connect(
        CalendarProvider provider,
        [FromBody] ConnectCalendarApiRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new ConnectCalendarCommand
        {
            AgentId = userId.Value,
            TenantId = tenantId.Value,
            Provider = provider,
            AuthorizationCode = request.AuthorizationCode,
            RedirectUri = request.RedirectUri
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(201, result);
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(501, new { error = ex.Message });
        }
    }

    [HttpDelete("{provider}/disconnect")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Disconnect(CalendarProvider provider, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new DisconnectCalendarCommand(userId.Value, tenantId.Value, provider);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{provider}/webhook")]
    [AllowAnonymous]
    public IActionResult Webhook(CalendarProvider provider)
    {
        _logger.LogInformation(
            "Calendar webhook received for provider {Provider} -- stub implementation",
            provider);

        return Ok();
    }
}

public sealed class ConnectCalendarApiRequest
{
    public string AuthorizationCode { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;
}
