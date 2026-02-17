// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Api.Configuration;

/// <summary>
/// Centralized authorization policy names per security baseline.
/// Policies are defined in one location to avoid magic strings in controllers.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy that requires the authenticated user to have the "admin" role claim.
    /// Applied to admin-only endpoints such as audit logs and feature flag toggling.
    /// </summary>
    public const string AdminOnly = "AdminOnly";
}
