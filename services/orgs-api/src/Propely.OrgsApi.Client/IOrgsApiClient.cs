// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Client.Dtos;
using Refit;

namespace Propely.OrgsApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the Propely Orgs API.
/// Provides methods for user validation, organization queries, and membership checks.
/// </summary>
public interface IOrgsApiClient
{
    /// <summary>
    /// Gets the current authenticated user's profile.
    /// The API returns an envelope: <c>{ "user": { ... } }</c>.
    /// </summary>
    [Get("/auth/me")]
    Task<MeResponse> GetCurrentUserAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an organization by its ID.
    /// </summary>
    [Get("/orgs/{orgId}")]
    Task<OrgResponse> GetOrganizationAsync(Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Lists organizations for the current authenticated user.
    /// </summary>
    [Get("/orgs/mine")]
    Task<PagedResult<OrgResponse>> GetMyOrganizationsAsync(
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets members of an organization.
    /// </summary>
    [Get("/orgs/{orgId}/members")]
    Task<PagedResult<MemberResponse>> GetMembersAsync(
        Guid orgId,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        [Query] string? search = null,
        CancellationToken ct = default);
}
