// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Api.Configuration;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.Application.WorkItems.Commands;
using Propely.AiApi.Application.WorkItems.Commands.ParseWorkItem;
using Propely.AiApi.Application.WorkItems.Queries;
using Propely.AiApi.Application.WorkItems.Queries.ListWorkItems;
using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Application.WorkItems.Dtos;
using Propely.AiApi.Domain.WorkItems;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Propely.AiApi.Api.Controllers;

/// <summary>
/// API controller for Work Item operations.
/// </summary>
[ApiController]
[Route("v1/work-items")]
[Authorize]
public sealed class WorkItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateWorkItemRequest> _createValidator;
    private readonly IValidator<UpdateWorkItemRequest> _updateValidator;
    private readonly IValidator<ParseWorkItemRequest> _parseValidator;

    /// <summary>
    /// Initializes the controller with required dependencies.
    /// </summary>
    public WorkItemsController(
        IMediator mediator,
        IValidator<CreateWorkItemRequest> createValidator,
        IValidator<UpdateWorkItemRequest> updateValidator,
        IValidator<ParseWorkItemRequest> parseValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _parseValidator = parseValidator;
    }

    /// <summary>
    /// Creates a new work item.
    /// </summary>
    /// <param name="request">The work item creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work item.</returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanCreateWorkItem)]
    [ProducesResponseType(typeof(WorkItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWorkItem(
        [FromBody] CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }
        
        // Extract UserId from Claims (provided by JwtBearer or DevHandler)
        if (!User.TryGetUserId(out var userId))
        {
            // Should not happen if [Authorize] is engaged and token is valid/contains sub
             return Unauthorized();
        }

        // Extract OrgId from Claims (tenant context)
        if (!User.TryGetOrgId(out var orgId))
        {
            // Authenticated user is missing required tenant context -> treat as authorization failure
            return Forbid();
        }

        var priority = Enum.TryParse<WorkItemPriority>(request.Priority, out var p) ? p : (WorkItemPriority?)null;
        var type = Enum.TryParse<WorkItemType>(request.Type, out var t) ? t : (WorkItemType?)null;
        var effort = Enum.TryParse<WorkItemEffort>(request.EstimatedEffort, out var e) ? e : (WorkItemEffort?)null;

        var command = new CreateWorkItemCommand(orgId, userId, request.Title, request.Description,
            Priority: priority, Type: type, DueDateUtc: request.DueDateUtc, EstimatedEffort: effort);
        var result = await _mediator.Send(command, cancellationToken);

        var response = WorkItemResponse.Created(
            result.Id,
            result.OrgId,
            result.Title,
            result.Description,
            result.Status,
            result.Priority,
            result.Type,
            result.DueDateUtc,
            result.EstimatedEffort,
            result.CreatedAtUtc,
            result.UpdatedAtUtc);

        return CreatedAtAction(
            nameof(GetWorkItem),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Gets a work item by ID.
    /// </summary>
    /// <param name="id">The work item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work item if found.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CanReadWorkItem)]
    [ProducesResponseType(typeof(WorkItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkItem(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetWorkItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(WorkItemResponse.FromDto(result));
    }

    /// <summary>
    /// Lists work items with pagination and filtering.
    /// </summary>
    /// <param name="query">The list query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of work items.</returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanReadWorkItem)]
    [ProducesResponseType(typeof(PagedResult<WorkItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListWorkItems(
        [FromQuery] ListWorkItemsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    /// <summary>
    /// Updates a work item.
    /// </summary>
    /// <param name="id">The work item ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CanUpdateWorkItem)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkItem(
        Guid id,
        [FromBody] UpdateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }

        // Extract UserId
        if (!User.TryGetUserId(out var userId)) return Unauthorized();

        var priority = Enum.TryParse<WorkItemPriority>(request.Priority, out var p) ? p : (WorkItemPriority?)null;
        var type = Enum.TryParse<WorkItemType>(request.Type, out var t) ? t : (WorkItemType?)null;
        var effort = Enum.TryParse<WorkItemEffort>(request.EstimatedEffort, out var e) ? e : (WorkItemEffort?)null;

        var command = new UpdateWorkItemCommand(id, userId, request.Title, request.Description, request.Status,
            Priority: priority, Type: type, DueDateUtc: request.DueDateUtc, EstimatedEffort: effort);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a work item (Soft Delete).
    /// </summary>
    /// <param name="id">The work item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No Content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CanDeleteWorkItem)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkItem(Guid id, CancellationToken cancellationToken)
    {
        // Extract UserId
        if (!User.TryGetUserId(out var userId)) return Unauthorized();

        var command = new DeleteWorkItemCommand(id, userId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Parses natural language text into structured work item fields using AI.
    /// </summary>
    /// <param name="request">The parse request containing free-form text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed work item fields.</returns>
    [HttpPost("parse")]
    [Authorize]
    [ProducesResponseType(typeof(ParseWorkItemResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ParseWorkItem(
        [FromBody] ParseWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _parseValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }

        var command = new ParseWorkItemCommand(request.Text);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
