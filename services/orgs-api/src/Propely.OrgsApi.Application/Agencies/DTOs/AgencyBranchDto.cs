// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Agencies.DTOs;

public sealed record AgencyBranchDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc);
