// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Domain.Actions;
using Propely.PropertiesApi.Client;
using Propely.PropertiesApi.Client.Dtos;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles the QueryPropertiesActionCommand by extracting filter parameters
/// and calling the Properties API SDK to search for properties.
/// Returns a human-readable summary of the results.
/// </summary>
public sealed class QueryPropertiesActionHandler : IRequestHandler<QueryPropertiesActionCommand, ActionResult>
{
    private readonly IPropertiesApiClient _propertiesClient;
    private readonly ILogger<QueryPropertiesActionHandler> _logger;

    public QueryPropertiesActionHandler(
        IPropertiesApiClient propertiesClient,
        ILogger<QueryPropertiesActionHandler> logger)
    {
        _propertiesClient = propertiesClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(QueryPropertiesActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling QueryProperties action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyType = request.Parameters.PropertyType;
        var operationType = request.Parameters.OperationType;
        var status = request.Parameters.Status;
        var city = request.Parameters.City;
        var minPrice = request.Parameters.MinPrice;
        var maxPrice = request.Parameters.MaxPrice;
        var minBedrooms = request.Parameters.MinBedrooms;

        PagedResult<PropertyListItemResponse> result;
        try
        {
            result = await _propertiesClient.ListAsync(
                type: propertyType,
                operation: operationType,
                status: status,
                minPrice: minPrice,
                maxPrice: maxPrice,
                city: city,
                ct: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query properties from Properties API");

            return ActionResult.Fail(
                ["Failed to retrieve properties. The Properties service may be unavailable."],
                ActionType.QueryProperties,
                "I encountered an error while searching for properties. Please try again later.");
        }

        // Filter by min bedrooms client-side if specified (not supported by API query params)
        var items = result.Items;
        if (minBedrooms.HasValue)
        {
            items = items.Where(p => p.Bedrooms >= minBedrooms.Value).ToList();
        }

        if (items.Count == 0)
        {
            var noResultsMessage = BuildNoResultsMessage(propertyType, city, minPrice, maxPrice);
            return ActionResult.Ok(
                data: new { items = Array.Empty<object>(), totalCount = 0 },
                message: noResultsMessage,
                type: ActionType.QueryProperties);
        }

        var summary = BuildResultsSummary(items, result.TotalCount, propertyType, city);

        var responseData = new
        {
            items = items.Select(p => new
            {
                p.Id,
                p.Title,
                p.PropertyType,
                p.OperationType,
                p.Status,
                p.Price,
                p.City,
                p.Bedrooms,
                p.Bathrooms,
                p.BuiltArea
            }),
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            totalPages = result.TotalPages
        };

        _logger.LogInformation(
            "QueryProperties returned {Count} results (total: {TotalCount}) for tenant {TenantId}",
            items.Count, result.TotalCount, request.TenantId);

        return ActionResult.Ok(
            data: responseData,
            message: summary,
            type: ActionType.QueryProperties);
    }

    private static string BuildNoResultsMessage(string? propertyType, string? city, decimal? minPrice, decimal? maxPrice)
    {
        var parts = new List<string> { "No properties found" };
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(propertyType))
            filters.Add($"type \"{propertyType}\"");
        if (!string.IsNullOrWhiteSpace(city))
            filters.Add($"in {city}");
        if (minPrice.HasValue || maxPrice.HasValue)
        {
            if (minPrice.HasValue && maxPrice.HasValue)
                filters.Add(string.Format(CultureInfo.InvariantCulture, "between {0:N0} and {1:N0} EUR", minPrice.Value, maxPrice.Value));
            else if (minPrice.HasValue)
                filters.Add(string.Format(CultureInfo.InvariantCulture, "from {0:N0} EUR", minPrice.Value));
            else
                filters.Add(string.Format(CultureInfo.InvariantCulture, "up to {0:N0} EUR", maxPrice!.Value));
        }

        if (filters.Count > 0)
            parts.Add("matching " + string.Join(", ", filters));

        return string.Join(" ", parts) + ". Try adjusting your search criteria.";
    }

    private static string BuildResultsSummary(
        IReadOnlyList<PropertyListItemResponse> items,
        int totalCount,
        string? propertyType,
        string? city)
    {
        var summary = $"I found {totalCount} propert{(totalCount == 1 ? "y" : "ies")}";

        if (!string.IsNullOrWhiteSpace(propertyType))
            summary += $" of type \"{propertyType}\"";
        if (!string.IsNullOrWhiteSpace(city))
            summary += $" in {city}";

        summary += ".";

        // Add a brief overview of the first few results
        if (items.Count > 0)
        {
            var topItems = items.Take(3).Select(p =>
            {
                var desc = p.Title;
                if (p.Price.HasValue)
                    desc += string.Format(CultureInfo.InvariantCulture, " ({0:N0} EUR)", p.Price.Value);
                return desc;
            });
            summary += $" Top results: {string.Join("; ", topItems)}.";
        }

        if (totalCount > items.Count)
            summary += $" Showing {items.Count} of {totalCount} total.";

        return summary;
    }
}
