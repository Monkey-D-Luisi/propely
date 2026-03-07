// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Configuration;

/// <summary>
/// Centralized authorization policy names per security baseline.
/// Policies are defined in one location to avoid magic strings in controllers.
/// </summary>
public static class AuthorizationPolicies
{
    // ── Role-based policies (hierarchical: higher roles inherit lower) ──

    /// <summary>
    /// Requires the user to have the "owner" or "admin" role.
    /// </summary>
    public const string RequireOwnerOrAdmin = nameof(RequireOwnerOrAdmin);

    /// <summary>
    /// Requires the user to have at least the "agent" role (owner, admin, or agent).
    /// </summary>
    public const string RequireAgent = nameof(RequireAgent);

    /// <summary>
    /// Requires the user to have at least the "viewer" role (any authenticated role).
    /// </summary>
    public const string RequireViewer = nameof(RequireViewer);

    // ── Work item policies (mapped to role-based equivalents) ──

    /// <summary>
    /// Policy requiring agent-level access for creating work items.
    /// </summary>
    public const string CanCreateWorkItem = nameof(CanCreateWorkItem);

    /// <summary>
    /// Policy requiring viewer-level access for reading work items.
    /// </summary>
    public const string CanReadWorkItem = nameof(CanReadWorkItem);

    /// <summary>
    /// Policy requiring agent-level access for updating work items.
    /// </summary>
    public const string CanUpdateWorkItem = nameof(CanUpdateWorkItem);

    /// <summary>
    /// Policy requiring agent-level access for deleting work items.
    /// </summary>
    public const string CanDeleteWorkItem = nameof(CanDeleteWorkItem);
}
