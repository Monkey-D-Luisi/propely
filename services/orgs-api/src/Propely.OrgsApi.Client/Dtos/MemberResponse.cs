// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Represents a member of an organization.
/// </summary>
public sealed record MemberResponse(
    Guid UserId,
    string Email,
    string? Name,
    string Role);
