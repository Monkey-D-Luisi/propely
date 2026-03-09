// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Client.Dtos;
using Refit;

namespace Propely.PropertiesApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the Propely Properties API.
/// Provides access to property data for cross-service communication.
/// </summary>
public interface IPropertiesApiClient
{
    /// <summary>
    /// Creates a new property listing.
    /// </summary>
    /// <param name="request">The property creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created property details.</returns>
    [Post("/api/properties")]
    Task<PropertyResponse> CreateAsync([Body] CreatePropertyRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a property by its unique identifier.
    /// </summary>
    /// <param name="id">The property ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The property details, or null if not found.</returns>
    [Get("/api/properties/{id}")]
    Task<PropertyResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists properties with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="search">Full-text search across title and address fields.</param>
    /// <param name="type">Filter by property type.</param>
    /// <param name="operation">Filter by operation type.</param>
    /// <param name="status">Filter by property status.</param>
    /// <param name="minPrice">Minimum price filter.</param>
    /// <param name="maxPrice">Maximum price filter.</param>
    /// <param name="city">Filter by city (case-insensitive partial match).</param>
    /// <param name="agentId">Filter by assigned agent.</param>
    /// <param name="minBedrooms">Minimum number of bedrooms.</param>
    /// <param name="minBathrooms">Minimum number of bathrooms.</param>
    /// <param name="minArea">Minimum built area in square meters.</param>
    /// <param name="maxArea">Maximum built area in square meters.</param>
    /// <param name="hasPool">Filter properties with pool.</param>
    /// <param name="hasGarden">Filter properties with garden.</param>
    /// <param name="hasGarage">Filter properties with garage.</param>
    /// <param name="hasElevator">Filter properties with elevator.</param>
    /// <param name="hasTerrace">Filter properties with terrace.</param>
    /// <param name="sortBy">Sort field name.</param>
    /// <param name="sortDesc">Sort in descending order.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged list of property items.</returns>
    [Get("/api/properties")]
    Task<PagedResult<PropertyListItemResponse>> ListAsync(
        [Query] string? search = null,
        [Query] string? type = null,
        [Query] string? operation = null,
        [Query] string? status = null,
        [Query] decimal? minPrice = null,
        [Query] decimal? maxPrice = null,
        [Query] string? city = null,
        [Query] Guid? agentId = null,
        [Query] int? minBedrooms = null,
        [Query] int? minBathrooms = null,
        [Query] decimal? minArea = null,
        [Query] decimal? maxArea = null,
        [Query] bool? hasPool = null,
        [Query] bool? hasGarden = null,
        [Query] bool? hasGarage = null,
        [Query] bool? hasElevator = null,
        [Query] bool? hasTerrace = null,
        [Query] string? sortBy = null,
        [Query] bool sortDesc = false,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets property counts grouped by status for the current tenant.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary mapping status names to their counts.</returns>
    [Get("/api/properties/count-by-status")]
    Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken ct = default);
}
