// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Extensions;
using SaasTemplate.OrgsApi.Application.Notifications.Commands.MarkAllNotificationsRead;
using SaasTemplate.OrgsApi.Application.Notifications.Commands.MarkNotificationRead;
using SaasTemplate.OrgsApi.Application.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaasTemplate.OrgsApi.Api.Controllers;

[ApiController]
[Route("notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetNotificationsQuery(userId.Value, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new
        {
            result.Notifications.Items,
            result.Notifications.PageNumber,
            result.Notifications.TotalPages,
            result.Notifications.TotalCount,
            result.Notifications.HasPreviousPage,
            result.Notifications.HasNextPage,
            result.UnreadCount
        });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new MarkNotificationReadCommand(id, userId.Value);
        await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true });
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new MarkAllNotificationsReadCommand(userId.Value);
        await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true });
    }
}
