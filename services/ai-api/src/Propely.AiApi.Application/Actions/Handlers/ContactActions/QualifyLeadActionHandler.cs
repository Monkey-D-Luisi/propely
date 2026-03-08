// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.ContactActions;

/// <summary>
/// Handles the QualifyLeadActionCommand by changing a lead's status to Qualified
/// via the Contacts API SDK.
/// </summary>
public sealed class QualifyLeadActionHandler : IRequestHandler<QualifyLeadActionCommand, ActionResult>
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly ILogger<QualifyLeadActionHandler> _logger;

    public QualifyLeadActionHandler(
        ILeadsApiClient leadsClient,
        ILogger<QualifyLeadActionHandler> logger)
    {
        _leadsClient = leadsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(QualifyLeadActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling QualifyLead action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var leadId = request.Parameters.LeadId;

        if (!leadId.HasValue)
        {
            return ActionResult.Fail(
                ["Lead ID is required to qualify a lead."],
                ActionType.QualifyLead,
                "I need to know which lead to qualify. Could you specify the lead?");
        }

        LeadResponse result;
        try
        {
            result = await _leadsClient.ChangeLeadStatusAsync(
                leadId.Value,
                new ChangeStatusClientRequest { Status = "Qualified" },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to qualify lead {LeadId} via Contacts API", leadId.Value);

            return ActionResult.Fail(
                ["Failed to qualify the lead. The lead may not exist or the status transition may be invalid."],
                ActionType.QualifyLead,
                "I encountered an error while qualifying the lead. Please verify the lead exists and is in Contacted status.");
        }

        var confirmationMessage = $"Lead {result.Name} has been qualified.";

        _logger.LogInformation(
            "QualifyLead action completed: lead {LeadId} for tenant {TenantId}",
            leadId.Value, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.Name, result.Email, result.Status },
            message: confirmationMessage,
            type: ActionType.QualifyLead);
    }
}
