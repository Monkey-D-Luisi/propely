// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Response DTO for detailed agency information including branches.
/// </summary>
public sealed record AgencyDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<BranchResponse> Branches);
