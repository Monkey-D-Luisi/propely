// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.PropertiesApi.Api.Extensions;
using Propely.PropertiesApi.Application.Properties.Commands.DeleteMedia;
using Propely.PropertiesApi.Application.Properties.Commands.ReorderMedia;
using Propely.PropertiesApi.Application.Properties.Commands.UploadMedia;
using Propely.PropertiesApi.Application.Properties.Queries.ListPropertyMedia;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:guid}/media")]
[Authorize]
public sealed class PropertyMediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyMediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid propertyId, IFormFile file, [FromQuery] MediaType mediaType = MediaType.Photo, CancellationToken cancellationToken = default)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "File is required." });

        await using var stream = file.OpenReadStream();
        var command = new UploadMediaCommand
        {
            PropertyId = propertyId,
            TenantId = tenantId.Value,
            MediaType = mediaType,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            FileStream = stream
        };

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid propertyId, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var query = new ListPropertyMediaQuery(propertyId, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{mediaId:guid}")]
    public async Task<IActionResult> Delete(Guid propertyId, Guid mediaId, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new DeleteMediaCommand(mediaId, propertyId, tenantId.Value);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("reorder")]
    public async Task<IActionResult> Reorder(Guid propertyId, [FromBody] ReorderRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new ReorderMediaCommand
        {
            PropertyId = propertyId,
            TenantId = tenantId.Value,
            Items = request.Items.Select(i => new MediaOrderItem(i.MediaId, i.DisplayOrder)).ToList()
        };

        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}

public sealed record ReorderRequest
{
    public IReadOnlyList<ReorderItemRequest> Items { get; init; } = [];
}

public sealed record ReorderItemRequest
{
    public Guid MediaId { get; init; }
    public int DisplayOrder { get; init; }
}
