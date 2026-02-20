// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Represents an organization with the requesting user's role.
/// </summary>
public sealed record OrgResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Role);
