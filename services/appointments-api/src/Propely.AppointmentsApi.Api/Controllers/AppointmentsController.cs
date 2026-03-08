// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.AppointmentsApi.Api.Dtos;
using Propely.AppointmentsApi.Api.Extensions;
using Propely.AppointmentsApi.Application.Appointments.Commands.CancelAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.CompleteAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.ConfirmAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.CreateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.DeleteAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.MarkNoShowAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetAppointmentById;
using Propely.AppointmentsApi.Application.Appointments.Queries.ListAppointments;
using Propely.AppointmentsApi.Application.Appointments.Queries.CountAppointmentsByStatus;
using Propely.AppointmentsApi.Application.Appointments.Queries.CountUpcomingAppointments;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IMediator mediator, ILogger<AppointmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentApiRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new CreateAppointmentCommand
        {
            Title = request.Title,
            Type = request.Type,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = request.EndTimeUtc,
            AgentId = userId.Value,
            TenantId = tenantId.Value,
            Description = request.Description,
            Location = request.Location,
            IsAllDay = request.IsAllDay ?? false,
            PropertyId = request.PropertyId,
            ContactId = request.ContactId,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(201, result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var query = new GetAppointmentByIdQuery(id, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("count-by-status")]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> CountByStatus(CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var query = new CountAppointmentsByStatusQuery(tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("count-upcoming")]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> CountUpcoming([FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var query = new CountUpcomingAppointmentsQuery(tenantId.Value, days);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new { count = result });
    }

    [HttpGet]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] AppointmentType? type,
        [FromQuery] Guid? agentId,
        [FromQuery] Guid? propertyId,
        [FromQuery] Guid? contactId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var query = new ListAppointmentsQuery
        {
            TenantId = tenantId.Value,
            Search = search,
            Status = status,
            Type = type,
            AgentId = agentId,
            PropertyId = propertyId,
            ContactId = contactId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            SortBy = sortBy,
            SortDescending = sortDescending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentApiRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new UpdateAppointmentCommand
        {
            AppointmentId = id,
            TenantId = tenantId.Value,
            Title = request.Title,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = request.EndTimeUtc,
            Description = request.Description,
            Location = request.Location,
            IsAllDay = request.IsAllDay ?? false,
            PropertyId = request.PropertyId,
            ContactId = request.ContactId,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new DeleteAppointmentCommand(id, tenantId.Value);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/confirm")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new ConfirmAppointmentCommand(id, tenantId.Value);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteAppointmentApiRequest? request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new CompleteAppointmentCommand
        {
            AppointmentId = id,
            TenantId = tenantId.Value,
            Notes = request?.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentApiRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new CancelAppointmentCommand
        {
            AppointmentId = id,
            TenantId = tenantId.Value,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/no-show")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new MarkNoShowAppointmentCommand(id, tenantId.Value);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
