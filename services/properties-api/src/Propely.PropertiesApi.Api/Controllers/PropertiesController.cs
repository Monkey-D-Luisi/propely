// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.PropertiesApi.Api.Dtos;
using Propely.PropertiesApi.Api.Extensions;
using Propely.PropertiesApi.Application.Properties.Commands.ChangeStatus;
using Propely.PropertiesApi.Application.Properties.Commands.CreateProperty;
using Propely.PropertiesApi.Application.Properties.Commands.DeleteProperty;
using Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;
using Propely.PropertiesApi.Application.Properties.Queries.GetPropertyById;
using Propely.PropertiesApi.Application.Properties.Queries.CountPropertiesByStatus;
using Propely.PropertiesApi.Application.Properties.Queries.ListProperties;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Api.Controllers;

[ApiController]
[Route("api/properties")]
[Authorize]
public sealed class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> Create([FromBody] CreatePropertyRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new CreatePropertyCommand
        {
            Title = request.Title,
            PropertyType = request.PropertyType,
            OperationType = request.OperationType,
            TenantId = tenantId.Value,
            AgentId = userId.Value,
            AgencyId = request.AgencyId,
            Description = request.Description,
            Address = request.Address,
            Features = request.Features,
            Financials = request.Financials,
            VirtualTourUrl = request.VirtualTourUrl,
            VideoUrl = request.VideoUrl
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

        var query = new GetPropertyByIdQuery(id, tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] PropertyType? type,
        [FromQuery] OperationType? operation,
        [FromQuery] PropertyStatus? status,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? city,
        [FromQuery] Guid? agentId,
        [FromQuery] int? minBedrooms,
        [FromQuery] int? minBathrooms,
        [FromQuery] decimal? minArea,
        [FromQuery] decimal? maxArea,
        [FromQuery] bool? hasPool,
        [FromQuery] bool? hasGarden,
        [FromQuery] bool? hasGarage,
        [FromQuery] bool? hasElevator,
        [FromQuery] bool? hasTerrace,
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

        var query = new ListPropertiesQuery
        {
            TenantId = tenantId.Value,
            Search = search,
            Type = type,
            Operation = operation,
            Status = status,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            City = city,
            AgentId = agentId,
            MinBedrooms = minBedrooms,
            MinBathrooms = minBathrooms,
            MinArea = minArea,
            MaxArea = maxArea,
            HasPool = hasPool,
            HasGarden = hasGarden,
            HasGarage = hasGarage,
            HasElevator = hasElevator,
            HasTerrace = hasTerrace,
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var tenantId = this.GetTenantId();
        if (userId is null || tenantId is null) return Unauthorized();

        var command = new UpdatePropertyCommand
        {
            PropertyId = id,
            TenantId = tenantId.Value,
            UpdatedBy = userId.Value,
            Title = request.Title,
            PropertyType = request.PropertyType,
            OperationType = request.OperationType,
            Description = request.Description,
            Address = request.Address,
            Features = request.Features,
            Financials = request.Financials,
            VirtualTourUrl = request.VirtualTourUrl,
            VideoUrl = request.VideoUrl
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

        var command = new DeletePropertyCommand(id, tenantId.Value);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("count-by-status")]
    [Authorize(Policy = "RequireViewer")]
    public async Task<IActionResult> CountByStatus(CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var query = new CountPropertiesByStatusQuery(tenantId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "RequireAgent")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var tenantId = this.GetTenantId();
        if (tenantId is null) return Unauthorized();

        var command = new ChangePropertyStatusCommand(id, tenantId.Value, request.Status);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}
