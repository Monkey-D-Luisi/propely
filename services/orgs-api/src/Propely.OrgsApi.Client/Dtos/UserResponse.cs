// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Represents a user profile returned by the orgs-api.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string? Name,
    bool EmailVerified,
    bool IsSystemAdmin = false);
