// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.ContactActions;

/// <summary>
/// Handles the CreateLeadActionCommand by extracting lead parameters
/// and creating a lead via the Contacts API SDK.
/// </summary>
public sealed class CreateLeadActionHandler : IRequestHandler<CreateLeadActionCommand, ActionResult>
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly ILogger<CreateLeadActionHandler> _logger;

    public CreateLeadActionHandler(
        ILeadsApiClient leadsClient,
        ILogger<CreateLeadActionHandler> logger)
    {
        _leadsClient = leadsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(CreateLeadActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CreateLead action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var name = ParameterExtractor.GetString(parameters, "name");
        var email = ParameterExtractor.GetString(parameters, "email");
        var phone = ParameterExtractor.GetString(parameters, "phone");
        var message = ParameterExtractor.GetString(parameters, "message");
        var source = ParameterExtractor.GetString(parameters, "source");
        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");

        if (string.IsNullOrWhiteSpace(name))
        {
            return ActionResult.Fail(
                ["Lead name is required."],
                ActionType.CreateLead,
                "I need the lead's name to create a lead. Could you provide the name?");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ActionResult.Fail(
                ["Lead email is required."],
                ActionType.CreateLead,
                "I need the lead's email address to create a lead.");
        }

        if (!propertyId.HasValue)
        {
            return ActionResult.Fail(
                ["A property ID is required for a lead."],
                ActionType.CreateLead,
                "I need to know which property this lead is for. Could you specify the property?");
        }

        LeadResponse result;
        try
        {
            result = await _leadsClient.CreateLeadAsync(new CreateLeadClientRequest
            {
                Name = name,
                Email = email,
                PropertyId = propertyId.Value,
                Phone = phone,
                Message = message,
                Source = source ?? "NaturalLanguage",
                AssignedAgentId = request.AgentId
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create lead via Contacts API");

            return ActionResult.Fail(
                ["Failed to create lead. The Contacts service may be unavailable."],
                ActionType.CreateLead,
                "I encountered an error while creating the lead. Please try again later.");
        }

        var confirmationMessage = $"Created lead for {name} ({email}) for property {propertyId.Value:N}.";

        _logger.LogInformation(
            "CreateLead action completed: lead {LeadId} for tenant {TenantId}",
            result.Id, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.Name, result.Email, result.PropertyId, result.Status },
            message: confirmationMessage,
            type: ActionType.CreateLead);
    }
}
