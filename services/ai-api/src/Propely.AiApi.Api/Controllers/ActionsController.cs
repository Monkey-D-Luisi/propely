// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.AiApi.Api.Configuration;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.Application.Actions.Commands.ExecuteAction;

namespace Propely.AiApi.Api.Controllers;

/// <summary>
/// API controller for AI action execution.
/// Receives natural language text and routes it through intent classification and action handling.
/// </summary>
[ApiController]
[Route("v1/actions")]
[Authorize(Policy = AuthorizationPolicies.RequireAgent)]
public sealed class ActionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<ExecuteActionRequest> _validator;

    public ActionsController(
        IMediator mediator,
        IValidator<ExecuteActionRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    /// <summary>
    /// Executes an AI action from natural language text input.
    /// The text is classified into an intent, routed to the appropriate handler, and executed.
    /// </summary>
    /// <param name="request">The action request containing natural language text.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the action execution.</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(ActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Execute(
        [FromBody] ExecuteActionRequest request,
        CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }

        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (!User.TryGetOrgId(out var orgId))
        {
            return Forbid();
        }

        var command = new ExecuteActionCommand(request.Text, orgId, userId, request.SessionId);
        var result = await _mediator.Send(command, ct);

        var dto = new ActionResultDto(
            Success: result.Success,
            ActionType: result.ActionType.ToString(),
            Data: result.Data,
            Message: result.Message,
            Errors: result.Errors,
            Confidence: result.Confidence);

        return Ok(dto);
    }
}
