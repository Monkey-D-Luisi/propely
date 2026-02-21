// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Agencies.DTOs;

public sealed record AgencyDto(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAtUtc,
    int BranchCount);
