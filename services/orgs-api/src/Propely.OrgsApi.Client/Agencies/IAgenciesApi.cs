// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Client.Dtos;
using Refit;

namespace Propely.OrgsApi.Client.Agencies;

/// <summary>
/// Refit client interface for the orgs-api agency management endpoints.
/// </summary>
public interface IAgenciesApi
{
    /// <summary>
    /// Create a new agency.
    /// </summary>
    [Post("/api/agencies")]
    Task<CreateAgencyResponse> CreateAgencyAsync(
        [Body] Dtos.CreateAgencyRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// List agencies the authenticated user belongs to.
    /// </summary>
    [Get("/api/agencies")]
    Task<List<AgencyResponse>> ListAgenciesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get agency detail by ID including branches.
    /// </summary>
    [Get("/api/agencies/{id}")]
    Task<AgencyDetailResponse> GetAgencyByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Add a branch (existing organization) to an agency.
    /// </summary>
    [Post("/api/agencies/{id}/branches")]
    Task AddBranchAsync(
        Guid id,
        [Body] Dtos.AddBranchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Remove a branch from an agency.
    /// </summary>
    [Delete("/api/agencies/{id}/branches/{branchId}")]
    Task RemoveBranchAsync(Guid id, Guid branchId, CancellationToken ct = default);
}
