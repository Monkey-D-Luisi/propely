// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Response DTO for a branch within an agency.
/// </summary>
public sealed record BranchResponse(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc);
