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
/// Handles the QueryLeadsActionCommand by extracting filter parameters
/// and calling the Contacts API SDK to search for leads.
/// </summary>
public sealed class QueryLeadsActionHandler : IRequestHandler<QueryLeadsActionCommand, ActionResult>
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly ILogger<QueryLeadsActionHandler> _logger;

    public QueryLeadsActionHandler(
        ILeadsApiClient leadsClient,
        ILogger<QueryLeadsActionHandler> logger)
    {
        _leadsClient = leadsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(QueryLeadsActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling QueryLeads action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var status = ParameterExtractor.GetString(parameters, "status");
        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var search = ParameterExtractor.GetString(parameters, "search");

        LeadListResponse result;
        try
        {
            result = await _leadsClient.GetLeadsAsync(
                search: search,
                status: status,
                propertyId: propertyId,
                assignedAgentId: null,
                sortBy: "createdAt",
                sortDesc: true,
                page: 1,
                pageSize: 20,
                ct: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query leads from Contacts API");

            return ActionResult.Fail(
                ["Failed to retrieve leads. The Contacts service may be unavailable."],
                ActionType.QueryLeads,
                "I encountered an error while searching for leads. Please try again later.");
        }

        if (result.Items.Count == 0)
        {
            var noResultsMessage = BuildNoResultsMessage(status, propertyId);
            return ActionResult.Ok(
                data: new { items = Array.Empty<object>(), totalCount = 0 },
                message: noResultsMessage,
                type: ActionType.QueryLeads);
        }

        var summary = BuildResultsSummary(result.Items, result.TotalCount, status);

        var responseData = new
        {
            items = result.Items.Select(l => new
            {
                l.Id,
                l.Name,
                l.Email,
                l.Status,
                l.PropertyId,
                l.AssignedAgentId,
                l.CreatedAtUtc
            }),
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            totalPages = result.TotalPages
        };

        _logger.LogInformation(
            "QueryLeads returned {Count} results (total: {TotalCount}) for tenant {TenantId}",
            result.Items.Count, result.TotalCount, request.TenantId);

        return ActionResult.Ok(
            data: responseData,
            message: summary,
            type: ActionType.QueryLeads);
    }

    private static string BuildNoResultsMessage(string? status, Guid? propertyId)
    {
        var parts = new List<string> { "No leads found" };
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(status))
            filters.Add($"with status \"{status}\"");
        if (propertyId.HasValue)
            filters.Add($"for property {propertyId.Value:N}");

        if (filters.Count > 0)
            parts.Add("matching " + string.Join(", ", filters));

        return string.Join(" ", parts) + ". Try adjusting your search criteria.";
    }

    private static string BuildResultsSummary(
        IReadOnlyList<LeadResponse> items,
        int totalCount,
        string? status)
    {
        var summary = $"I found {totalCount} lead{(totalCount == 1 ? "" : "s")}";

        if (!string.IsNullOrWhiteSpace(status))
            summary += $" with status \"{status}\"";

        summary += ".";

        if (items.Count > 0)
        {
            var topItems = items.Take(3).Select(l => $"{l.Name} ({l.Status})");
            summary += $" Top results: {string.Join("; ", topItems)}.";
        }

        if (totalCount > items.Count)
            summary += $" Showing {items.Count} of {totalCount} total.";

        return summary;
    }
}
