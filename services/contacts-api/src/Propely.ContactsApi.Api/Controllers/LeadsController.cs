// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.ContactsApi.Api.Dtos;
using Propely.ContactsApi.Api.Extensions;
using Propely.ContactsApi.Application.Leads.Commands.AssignLead;
using Propely.ContactsApi.Application.Leads.Commands.ChangeLeadStatus;
using Propely.ContactsApi.Application.Leads.Commands.ConvertLead;
using Propely.ContactsApi.Application.Leads.Commands.CreateLead;
using Propely.ContactsApi.Application.Leads.Commands.DeleteLead;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Queries.GetLeadById;
using Propely.ContactsApi.Application.Leads.Queries.ListLeads;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Api.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public sealed class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new CreateLeadCommand
        {
            Name = request.Name,
            Email = request.Email,
            PropertyId = request.PropertyId,
            TenantId = tenantId.Value,
            Phone = request.Phone,
            Message = request.Message,
            Source = request.Source,
            AssignedAgentId = request.AssignedAgentId
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

        var query = new GetLeadByIdQuery(id, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] LeadStatus? status,
        [FromQuery] Guid? propertyId,
        [FromQuery] Guid? assignedAgentId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var query = new ListLeadsQuery
        {
            TenantId = tenantId.Value,
            Status = status,
            PropertyId = propertyId,
            AssignedAgentId = assignedAgentId,
            Search = search,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new DeleteLeadCommand(id, tenantId.Value);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/assign")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignLeadRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new AssignLeadCommand(id, tenantId.Value, request.AgentId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeLeadStatusRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new ChangeLeadStatusCommand(id, tenantId.Value, request.Status);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Convert(Guid id, [FromBody] ConvertLeadRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new ConvertLeadCommand
        {
            LeadId = id,
            TenantId = tenantId.Value,
            Role = request.Role,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
