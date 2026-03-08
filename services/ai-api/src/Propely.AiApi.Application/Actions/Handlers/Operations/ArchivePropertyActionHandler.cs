// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers.Operations;

/// <summary>
/// Handles the ArchivePropertyActionCommand by extracting the property identifier
/// and returning structured data confirming the archive operation.
/// Sets the target status to "Archived".
/// </summary>
public sealed class ArchivePropertyActionHandler : IRequestHandler<ArchivePropertyActionCommand, ActionResult>
{
    private readonly ILogger<ArchivePropertyActionHandler> _logger;

    public ArchivePropertyActionHandler(ILogger<ArchivePropertyActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(ArchivePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ArchiveProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyId = request.Parameters.PropertyId;
        var reference = request.Parameters.Reference;

        // Validate property identifier
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.ArchiveProperty,
                "I need to know which property to archive. Could you provide the property ID or reference?"));
        }

        var archiveData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["targetStatus"] = "Archived",
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var message = $"I'll archive property {identifier}. The listing will be removed from active listings.";

        _logger.LogInformation(
            "ArchiveProperty action prepared: property {PropertyId} in tenant {TenantId}",
            identifier, request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: archiveData,
            message: message,
            type: ActionType.ArchiveProperty));
    }
}
