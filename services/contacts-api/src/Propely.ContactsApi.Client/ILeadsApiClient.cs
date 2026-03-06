// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Client.Models;
using Refit;

namespace Propely.ContactsApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the Propely Leads API.
/// Provides access to leads data for cross-service communication.
/// </summary>
public interface ILeadsApiClient
{
    /// <summary>
    /// Lists leads with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="search">Full-text search across name and email fields.</param>
    /// <param name="status">Filter by lead status.</param>
    /// <param name="propertyId">Filter by property ID.</param>
    /// <param name="assignedAgentId">Filter by assigned agent ID.</param>
    /// <param name="sortBy">Sort field name.</param>
    /// <param name="sortDesc">Sort in descending order.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged list of leads.</returns>
    [Get("/api/leads")]
    Task<LeadListResponse> GetLeadsAsync(
        [Query] string? search = null,
        [Query] string? status = null,
        [Query] Guid? propertyId = null,
        [Query] Guid? assignedAgentId = null,
        [Query] string? sortBy = null,
        [Query] bool sortDesc = false,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a lead by its unique identifier.
    /// </summary>
    /// <param name="id">The lead ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The lead details.</returns>
    [Get("/api/leads/{id}")]
    Task<LeadResponse> GetLeadByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new lead.
    /// </summary>
    /// <param name="request">The lead creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created lead.</returns>
    [Post("/api/leads")]
    Task<LeadResponse> CreateLeadAsync([Body] CreateLeadClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a lead.
    /// </summary>
    /// <param name="id">The lead ID.</param>
    /// <param name="ct">Cancellation token.</param>
    [Delete("/api/leads/{id}")]
    Task DeleteLeadAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Assigns an agent to a lead.
    /// </summary>
    /// <param name="id">The lead ID.</param>
    /// <param name="request">The assign agent request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated lead.</returns>
    [Put("/api/leads/{id}/assign")]
    Task<LeadResponse> AssignLeadAsync(Guid id, [Body] AssignLeadClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Changes the status of a lead.
    /// </summary>
    /// <param name="id">The lead ID.</param>
    /// <param name="request">The status change request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated lead.</returns>
    [Put("/api/leads/{id}/status")]
    Task<LeadResponse> ChangeLeadStatusAsync(Guid id, [Body] ChangeStatusClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Converts a qualified lead into a contact.
    /// </summary>
    /// <param name="id">The lead ID.</param>
    /// <param name="request">The conversion request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The conversion result with both lead and contact data.</returns>
    [Post("/api/leads/{id}/convert")]
    Task<ConvertLeadClientResponse> ConvertLeadAsync(Guid id, [Body] ConvertLeadClientRequest request, CancellationToken ct = default);
}
