// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.ExecuteAction;

/// <summary>
/// Handles the execution of an AI action by classifying user intent and routing to the appropriate handler.
/// </summary>
public sealed class ExecuteActionCommandHandler : IRequestHandler<ExecuteActionCommand, ActionResult>
{
    private readonly IIntentClassifier _intentClassifier;
    private readonly IActionRouter _actionRouter;
    private readonly ILogger<ExecuteActionCommandHandler> _logger;

    private const double MinimumConfidenceThreshold = 0.5;

    public ExecuteActionCommandHandler(
        IIntentClassifier intentClassifier,
        IActionRouter actionRouter,
        ILogger<ExecuteActionCommandHandler> logger)
    {
        _intentClassifier = intentClassifier;
        _actionRouter = actionRouter;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(ExecuteActionCommand request, CancellationToken cancellationToken)
    {
        ClassifiedIntent intent;

        try
        {
            intent = await _intentClassifier.ClassifyAsync(request.Text, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Intent classification failed for text: {TextPreview}",
                request.Text.Length > 100 ? request.Text[..100] + "..." : request.Text);

            return ActionResult.Fail(
                ["An error occurred while processing your request. Please try again."],
                ActionType.Unknown,
                "I encountered an error while trying to understand your request.");
        }

        if (intent.ActionType == ActionType.Unknown)
        {
            _logger.LogInformation("Unknown intent for text: {TextPreview}",
                request.Text.Length > 100 ? request.Text[..100] + "..." : request.Text);

            return ActionResult.Fail(
                ["Could not determine the intended action."],
                ActionType.Unknown,
                "I didn't understand that. Please try rephrasing your request. For example: \"Create a 3 bedroom apartment in Malaga for 250,000 EUR\" or \"Show me all villas under 500k\".");
        }

        if (intent.Confidence < MinimumConfidenceThreshold)
        {
            _logger.LogInformation(
                "Low confidence ({Confidence:F2}) for action {ActionType} from text: {TextPreview}",
                intent.Confidence, intent.ActionType,
                request.Text.Length > 100 ? request.Text[..100] + "..." : request.Text);

            return ActionResult.Fail(
                ["The request was ambiguous."],
                intent.ActionType,
                "I'm not sure I understood correctly. Please be more specific about what you'd like to do.");
        }

        try
        {
            var result = await _actionRouter.RouteAsync(intent, request.TenantId, request.AgentId, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action routing failed for action type {ActionType}", intent.ActionType);

            return ActionResult.Fail(
                ["An error occurred while executing the action. Please try again."],
                intent.ActionType,
                "I understood your request but encountered an error while executing it. Please try again.");
        }
    }
}
