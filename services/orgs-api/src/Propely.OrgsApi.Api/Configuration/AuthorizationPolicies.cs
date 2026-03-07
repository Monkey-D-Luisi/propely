// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Configuration;

/// <summary>
/// Centralized authorization policy names per security baseline.
/// Policies are defined in one location to avoid magic strings in controllers.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy that requires the authenticated user to have the "sys_admin" claim.
    /// Applied to admin-only endpoints such as audit logs and feature flag toggling.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    // ── Role-based policies (hierarchical: higher roles inherit lower) ──

    /// <summary>
    /// Requires the user to have the "owner" or "admin" role.
    /// Applied to org management (create, update, delete org) and member management.
    /// </summary>
    public const string RequireOwnerOrAdmin = nameof(RequireOwnerOrAdmin);

    /// <summary>
    /// Requires the user to have at least the "agent" role (owner, admin, or agent).
    /// </summary>
    public const string RequireAgent = nameof(RequireAgent);

    /// <summary>
    /// Requires the user to have at least the "viewer" role (any authenticated role).
    /// Applied to read endpoints (get org, list members).
    /// </summary>
    public const string RequireViewer = nameof(RequireViewer);
}
