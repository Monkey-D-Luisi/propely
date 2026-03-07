// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.ContactsApi.Api.Dtos;
using Propely.ContactsApi.Api.Extensions;
using Propely.ContactsApi.Application.Contacts.Commands.CreateContact;
using Propely.ContactsApi.Application.Contacts.Commands.DeleteContact;
using Propely.ContactsApi.Application.Contacts.Commands.UpdateContact;
using Propely.ContactsApi.Application.Contacts.Queries.GetContactById;
using Propely.ContactsApi.Application.Contacts.Queries.ListContacts;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Api.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public sealed class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new CreateContactCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            TenantId = tenantId.Value,
            Roles = request.Roles,
            Phone = request.Phone,
            SecondaryPhone = request.SecondaryPhone,
            Company = request.Company,
            Notes = request.Notes,
            PreferredLanguage = request.PreferredLanguage,
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

        var query = new GetContactByIdQuery(id, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] ContactRole? role,
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

        var query = new ListContactsQuery
        {
            TenantId = tenantId.Value,
            Search = search,
            Role = role,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new UpdateContactCommand
        {
            ContactId = id,
            TenantId = tenantId.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Roles = request.Roles,
            Phone = request.Phone,
            SecondaryPhone = request.SecondaryPhone,
            Company = request.Company,
            Notes = request.Notes,
            PreferredLanguage = request.PreferredLanguage,
            Source = request.Source,
            AssignedAgentId = request.AssignedAgentId
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

        var command = new DeleteContactCommand(id, tenantId.Value);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
