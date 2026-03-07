// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.AppointmentActions;

/// <summary>
/// Handles the QueryAppointmentsActionCommand by querying appointments
/// via the Appointments API SDK.
/// </summary>
public sealed class QueryAppointmentsActionHandler : IRequestHandler<QueryAppointmentsActionCommand, ActionResult>
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly ILogger<QueryAppointmentsActionHandler> _logger;

    public QueryAppointmentsActionHandler(
        IAppointmentsApiClient appointmentsClient,
        ILogger<QueryAppointmentsActionHandler> logger)
    {
        _appointmentsClient = appointmentsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(QueryAppointmentsActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling QueryAppointments action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var status = ParameterExtractor.GetString(parameters, "status");
        var type = ParameterExtractor.GetString(parameters, "type");
        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var fromDate = ParameterExtractor.GetString(parameters, "from_date");
        var toDate = ParameterExtractor.GetString(parameters, "to_date");

        DateTime? fromUtc = null;
        DateTime? toUtc = null;
        if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out var parsedFrom))
            fromUtc = parsedFrom;
        if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out var parsedTo))
            toUtc = parsedTo;

        AppointmentListResponse result;
        try
        {
            result = await _appointmentsClient.GetAppointmentsAsync(
                page: 1,
                pageSize: 20,
                status: status,
                type: type,
                agentId: null,
                propertyId: propertyId,
                contactId: null,
                fromUtc: fromUtc,
                toUtc: toUtc,
                sortBy: "startTimeUtc",
                sortDescending: false,
                ct: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query appointments from Appointments API");

            return ActionResult.Fail(
                ["Failed to retrieve appointments. The Appointments service may be unavailable."],
                ActionType.QueryAppointments,
                "I encountered an error while searching for appointments. Please try again later.");
        }

        if (result.Items.Count == 0)
        {
            return ActionResult.Ok(
                data: new { items = Array.Empty<object>(), totalCount = 0 },
                message: "No appointments found matching your criteria.",
                type: ActionType.QueryAppointments);
        }

        var summary = $"I found {result.TotalCount} appointment{(result.TotalCount == 1 ? "" : "s")}.";

        if (result.Items.Count > 0)
        {
            var topItems = result.Items.Take(3).Select(a =>
                $"{a.Title} ({a.StartTimeUtc:yyyy-MM-dd HH:mm}, {a.Status})");
            summary += $" Top results: {string.Join("; ", topItems)}.";
        }

        if (result.TotalCount > result.Items.Count)
            summary += $" Showing {result.Items.Count} of {result.TotalCount} total.";

        var responseData = new
        {
            items = result.Items.Select(a => new
            {
                a.Id, a.Title, a.Type, a.Status,
                a.StartTimeUtc, a.EndTimeUtc,
                a.PropertyId, a.ContactId
            }),
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            totalPages = result.TotalPages
        };

        _logger.LogInformation(
            "QueryAppointments returned {Count} results for tenant {TenantId}",
            result.Items.Count, request.TenantId);

        return ActionResult.Ok(
            data: responseData,
            message: summary,
            type: ActionType.QueryAppointments);
    }
}
