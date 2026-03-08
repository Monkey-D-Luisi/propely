// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.ExecuteAction;

/// <summary>
/// Handles the execution of an AI action by classifying user intent and routing to the appropriate handler.
/// Supports multi-turn conversation context when a SessionId is provided.
/// </summary>
public sealed class ExecuteActionCommandHandler : IRequestHandler<ExecuteActionCommand, ActionResult>
{
    private readonly IIntentClassifier _intentClassifier;
    private readonly IActionRouter _actionRouter;
    private readonly IConversationContext _conversationContext;
    private readonly ConversationContextOptions _contextOptions;
    private readonly ILogger<ExecuteActionCommandHandler> _logger;

    private const double MinimumConfidenceThreshold = 0.5;

    public ExecuteActionCommandHandler(
        IIntentClassifier intentClassifier,
        IActionRouter actionRouter,
        IConversationContext conversationContext,
        IOptions<ConversationContextOptions> contextOptions,
        ILogger<ExecuteActionCommandHandler> logger)
    {
        _intentClassifier = intentClassifier;
        _actionRouter = actionRouter;
        _conversationContext = conversationContext;
        _contextOptions = contextOptions.Value;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(ExecuteActionCommand request, CancellationToken cancellationToken)
    {
        // Load conversation history if a session is active
        ConversationSession? session = null;
        IReadOnlyList<ConversationExchange>? history = null;

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            session = await _conversationContext.GetAsync(
                request.TenantId, request.AgentId, request.SessionId, cancellationToken);
            history = session?.Exchanges.ToList();
        }

        ClassifiedIntent intent;

        try
        {
            intent = await _intentClassifier.ClassifyAsync(request.Text, history, cancellationToken);
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

        ActionResult result;

        try
        {
            result = await _actionRouter.RouteAsync(intent, request.TenantId, request.AgentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action routing failed for action type {ActionType}", intent.ActionType);

            return ActionResult.Fail(
                ["An error occurred while executing the action. Please try again."],
                intent.ActionType,
                "I understood your request but encountered an error while executing it. Please try again.");
        }

        // Save exchange to conversation session
        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            session ??= new ConversationSession();

            var exchange = new ConversationExchange(
                request.Text,
                result.ActionType,
                result.Message,
                result.Success,
                DateTimeOffset.UtcNow);

            session.AddExchange(exchange, _contextOptions.MaxExchanges);
            session.EntityMemory = UpdateEntityMemory(session.EntityMemory, result);

            await _conversationContext.SaveAsync(
                request.TenantId, request.AgentId, request.SessionId, session, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Updates entity memory based on data returned in the action result.
    /// Attempts to extract entity IDs from the result data dictionary.
    /// </summary>
    private static EntityMemory UpdateEntityMemory(EntityMemory current, ActionResult result)
    {
        if (!result.Success || result.Data is null)
        {
            return current;
        }

        // Data is typically an anonymous object or dictionary with entity IDs
        var data = result.Data as IDictionary<string, object?>;
        if (data is null) return current;

        return current with
        {
            LastPropertyId = TryExtractGuid(data, "propertyId", "id", result.ActionType, ActionType.CreateProperty, ActionType.UpdateProperty, ActionType.QueryProperties) ?? current.LastPropertyId,
            LastContactId = TryExtractGuid(data, "contactId", "id", result.ActionType, ActionType.CreateContact) ?? current.LastContactId,
            LastLeadId = TryExtractGuid(data, "leadId", "id", result.ActionType, ActionType.CreateLead, ActionType.QualifyLead) ?? current.LastLeadId,
            LastAppointmentId = TryExtractGuid(data, "appointmentId", "id", result.ActionType, ActionType.BookViewing) ?? current.LastAppointmentId
        };
    }

    private static Guid? TryExtractGuid(IDictionary<string, object?> data, string primaryKey, string fallbackKey, ActionType currentAction, params ActionType[] relevantActions)
    {
        if (!relevantActions.Contains(currentAction)) return null;

        if (data.TryGetValue(primaryKey, out var val) && val is string s && Guid.TryParse(s, out var guid))
            return guid;

        if (data.TryGetValue(fallbackKey, out val) && val is string s2 && Guid.TryParse(s2, out guid))
            return guid;

        return null;
    }
}
