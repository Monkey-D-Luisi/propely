// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Extensions;
using Propely.OrgsApi.Application.Organizations.Commands.AcceptInvitation;
using Propely.OrgsApi.Application.Organizations.Commands.CreateInvitation;
using Propely.OrgsApi.Application.Organizations.Commands.CreateOrganization;
using Propely.OrgsApi.Application.Organizations.Commands.DeleteOrganization;
using Propely.OrgsApi.Application.Organizations.Commands.LeaveOrganization;
using Propely.OrgsApi.Application.Organizations.Commands.RemoveMember;
using Propely.OrgsApi.Application.Organizations.Commands.UpdateOrganization;
using Propely.OrgsApi.Application.Organizations.Commands.UpdateMemberRole;
using Propely.OrgsApi.Application.Organizations.Queries.GetMembers;
using Propely.OrgsApi.Application.Organizations.Queries.GetMyOrgs;
using Propely.OrgsApi.Application.Organizations.Queries.GetOrganization;
using Propely.OrgsApi.Domain.Organizations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("orgs")]
[Authorize]
public sealed class OrgsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateOrgRequest> _createOrgValidator;
    private readonly IValidator<InviteRequest> _inviteValidator;
    private readonly IValidator<UpdateRoleRequest> _updateRoleValidator;
    private readonly IValidator<UpdateOrgRequest> _updateOrgValidator;

    public OrgsController(
        IMediator mediator,
        IValidator<CreateOrgRequest> createOrgValidator,
        IValidator<InviteRequest> inviteValidator,
        IValidator<UpdateRoleRequest> updateRoleValidator,
        IValidator<UpdateOrgRequest> updateOrgValidator)
    {
        _mediator = mediator;
        _createOrgValidator = createOrgValidator;
        _inviteValidator = inviteValidator;
        _updateRoleValidator = updateRoleValidator;
        _updateOrgValidator = updateOrgValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrg([FromBody] CreateOrgRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _createOrgValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new CreateOrganizationCommand(request.Name, userId.Value);
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new { id = result.OrgId, name = result.Name });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyOrgs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetMyOrgsQuery(userId.Value, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{orgId:guid}")]
    public async Task<IActionResult> GetOrg(Guid orgId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetOrganizationQuery(orgId, userId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{orgId:guid}/members")]
    public async Task<IActionResult> GetMembers(
        Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetMembersQuery(orgId, userId.Value, page, pageSize, search);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{orgId:guid}/invitations")]
    public async Task<IActionResult> CreateInvitation(Guid orgId, [FromBody] InviteRequest request, CancellationToken cancellationToken)
    {
        var requestingUserId = this.GetUserId();
        if (requestingUserId is null) return Unauthorized();

        var validationResult = await _inviteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        if (!Enum.TryParse<MembershipRole>(request.Role, true, out var role))
        {
            return BadRequest(new { error = "Invalid role." });
        }

        var command = new CreateInvitationCommand(orgId, request.Email, requestingUserId.Value, role);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { result.Ok });
    }

    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInviteRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var command = new AcceptInvitationCommand(request.Token, userId.Value, email);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = true });
    }

    [HttpPut("{orgId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> UpdateMemberRole(Guid orgId, Guid userId, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var requestingUserId = this.GetUserId();
        if (requestingUserId is null) return Unauthorized();

        var validationResult = await _updateRoleValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        if (!Enum.TryParse<MembershipRole>(request.Role, true, out var role))
        {
            return BadRequest(new { error = "Invalid role." });
        }

        var command = new UpdateMemberRoleCommand(orgId, userId, requestingUserId.Value, role);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = true });
    }

    [HttpDelete("{orgId:guid}/members/me")]
    public async Task<IActionResult> LeaveOrganization(Guid orgId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new LeaveOrganizationCommand(orgId, userId.Value);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = true });
    }

    [HttpDelete("{orgId:guid}/members/{targetUserId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid orgId, Guid targetUserId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new RemoveMemberCommand(orgId, targetUserId, userId.Value);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = true });
    }

    [HttpDelete("{orgId:guid}")]
    public async Task<IActionResult> DeleteOrganization(Guid orgId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new DeleteOrganizationCommand(orgId, userId.Value);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = true });
    }

    [HttpPatch("{orgId:guid}")]
    public async Task<IActionResult> UpdateOrg(Guid orgId, [FromBody] UpdateOrgRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _updateOrgValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new UpdateOrganizationCommand(orgId, userId.Value, request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { result.Id, result.Name, result.Description });
    }
}
