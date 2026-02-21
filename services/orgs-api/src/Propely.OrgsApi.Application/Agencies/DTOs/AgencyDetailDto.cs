// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Agencies.DTOs;

public sealed record AgencyDetailDto(
    Guid Id,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<AgencyBranchDto> Branches);
