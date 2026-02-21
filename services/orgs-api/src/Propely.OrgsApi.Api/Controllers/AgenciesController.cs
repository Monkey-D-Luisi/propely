// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Extensions;
using Propely.OrgsApi.Application.Agencies.Commands.AddBranchToAgency;
using Propely.OrgsApi.Application.Agencies.Commands.CreateAgency;
using Propely.OrgsApi.Application.Agencies.Commands.RemoveBranchFromAgency;
using Propely.OrgsApi.Application.Agencies.Queries.GetAgencyById;
using Propely.OrgsApi.Application.Agencies.Queries.ListAgenciesForUser;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("api/agencies")]
[Authorize]
public sealed class AgenciesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateAgencyRequest> _createAgencyValidator;
    private readonly IValidator<AddBranchRequest> _addBranchValidator;

    public AgenciesController(
        IMediator mediator,
        IValidator<CreateAgencyRequest> createAgencyValidator,
        IValidator<AddBranchRequest> addBranchValidator)
    {
        _mediator = mediator;
        _createAgencyValidator = createAgencyValidator;
        _addBranchValidator = addBranchValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgency([FromBody] CreateAgencyRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _createAgencyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new CreateAgencyCommand(request.Name, request.Slug, userId.Value);
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new { id = result.AgencyId, name = result.Name, slug = result.Slug });
    }

    [HttpGet]
    public async Task<IActionResult> ListAgencies(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new ListAgenciesForUserQuery(userId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAgency(Guid id, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetAgencyByIdQuery(id, userId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/branches")]
    public async Task<IActionResult> AddBranch(Guid id, [FromBody] AddBranchRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _addBranchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new AddBranchToAgencyCommand(id, request.OrganizationId, userId.Value);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}/branches/{branchId:guid}")]
    public async Task<IActionResult> RemoveBranch(Guid id, Guid branchId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new RemoveBranchFromAgencyCommand(id, branchId, userId.Value);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}
