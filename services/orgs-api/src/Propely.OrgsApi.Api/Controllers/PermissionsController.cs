// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Extensions;
using Propely.OrgsApi.Application.Permissions.Commands.RemovePermissionOverride;
using Propely.OrgsApi.Application.Permissions.Commands.SetPermissionOverride;
using Propely.OrgsApi.Application.Permissions.Queries.GetUserPermissions;
using Propely.OrgsApi.Domain.Permissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("api/organizations/{orgId:guid}/permissions")]
[Authorize]
public sealed class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get effective permissions for a user in an organization.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserPermissions(
        Guid orgId, Guid userId, CancellationToken cancellationToken)
    {
        var requestingUserId = this.GetUserId();
        if (requestingUserId is null) return Unauthorized();

        try
        {
            var query = new GetUserPermissionsQuery(orgId, userId, requestingUserId.Value);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Set a permission override for a user. Only admins and owners can do this.
    /// </summary>
    [HttpPut("{userId:guid}/{permission}")]
    public async Task<IActionResult> SetPermissionOverride(
        Guid orgId, Guid userId, string permission,
        [FromBody] SetPermissionOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var requestingUserId = this.GetUserId();
        if (requestingUserId is null) return Unauthorized();

        if (!Enum.TryParse<Permission>(permission, ignoreCase: true, out var parsedPermission))
            return BadRequest(new { error = $"Invalid permission: {permission}" });

        try
        {
            var command = new SetPermissionOverrideCommand(
                orgId, userId, parsedPermission, request.Granted, requestingUserId.Value);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a permission override, reverting to role default.
    /// Only admins and owners can do this.
    /// </summary>
    [HttpDelete("{userId:guid}/{permission}")]
    public async Task<IActionResult> RemovePermissionOverride(
        Guid orgId, Guid userId, string permission,
        CancellationToken cancellationToken)
    {
        var requestingUserId = this.GetUserId();
        if (requestingUserId is null) return Unauthorized();

        if (!Enum.TryParse<Permission>(permission, ignoreCase: true, out var parsedPermission))
            return BadRequest(new { error = $"Invalid permission: {permission}" });

        try
        {
            var command = new RemovePermissionOverrideCommand(
                orgId, userId, parsedPermission, requestingUserId.Value);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
