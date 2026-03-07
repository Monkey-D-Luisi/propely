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
/// Handles the ConvertLeadActionCommand by triggering lead conversion
/// via the Contacts API SDK.
/// </summary>
public sealed class ConvertLeadActionHandler : IRequestHandler<ConvertLeadActionCommand, ActionResult>
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly ILogger<ConvertLeadActionHandler> _logger;

    public ConvertLeadActionHandler(
        ILeadsApiClient leadsClient,
        ILogger<ConvertLeadActionHandler> logger)
    {
        _leadsClient = leadsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(ConvertLeadActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ConvertLead action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;
        var leadId = ParameterExtractor.GetGuid(parameters, "lead_id");
        var role = ParameterExtractor.GetString(parameters, "role");
        var notes = ParameterExtractor.GetString(parameters, "notes");

        if (!leadId.HasValue)
        {
            return ActionResult.Fail(
                ["Lead ID is required to convert a lead."],
                ActionType.ConvertLead,
                "I need to know which lead to convert. Could you specify the lead?");
        }

        ConvertLeadClientResponse result;
        try
        {
            result = await _leadsClient.ConvertLeadAsync(
                leadId.Value,
                new ConvertLeadClientRequest
                {
                    Role = role ?? "Buyer",
                    Notes = notes
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert lead {LeadId} via Contacts API", leadId.Value);

            return ActionResult.Fail(
                ["Failed to convert the lead. The lead may not exist or may not be in Qualified status."],
                ActionType.ConvertLead,
                "I encountered an error while converting the lead. Please verify the lead exists and is in Qualified status.");
        }

        var wasNew = result.WasNewContact ? "new" : "existing";
        var confirmationMessage = $"Lead {result.Lead.Name} has been converted to {wasNew} contact {result.Contact.FirstName} {result.Contact.LastName}.";

        _logger.LogInformation(
            "ConvertLead action completed: lead {LeadId} -> contact {ContactId} (wasNew: {WasNew}) for tenant {TenantId}",
            leadId.Value, result.Contact.Id, result.WasNewContact, request.TenantId);

        return ActionResult.Ok(
            data: new
            {
                LeadId = result.Lead.Id,
                ContactId = result.Contact.Id,
                ContactName = $"{result.Contact.FirstName} {result.Contact.LastName}",
                result.WasNewContact,
                result.Lead.Status
            },
            message: confirmationMessage,
            type: ActionType.ConvertLead);
    }
}
