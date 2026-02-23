// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Response DTO for agency list results.
/// </summary>
public sealed record AgencyResponse(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAtUtc,
    int BranchCount);
