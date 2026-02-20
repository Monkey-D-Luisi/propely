// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Configuration;

/// <summary>
/// Centralized authorization policy names per security baseline.
/// Policies are defined in one location to avoid magic strings in controllers.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring workitems:write scope for creating work items.
    /// </summary>
    public const string CanCreateWorkItem = nameof(CanCreateWorkItem);

    /// <summary>
    /// Policy requiring workitems:read scope for reading work items.
    /// </summary>
    public const string CanReadWorkItem = nameof(CanReadWorkItem);

    /// <summary>
    /// Policy requiring workitems:write scope for updating work items.
    /// </summary>
    public const string CanUpdateWorkItem = nameof(CanUpdateWorkItem);

    /// <summary>
    /// Policy requiring workitems:write scope for deleting work items.
    /// </summary>
    public const string CanDeleteWorkItem = nameof(CanDeleteWorkItem);
}
