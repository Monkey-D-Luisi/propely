// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Dtos;

/// <summary>
/// Request DTO for creating a new work item.
/// </summary>
public sealed record CreateWorkItemRequest(
    string Title,
    string? Description,
    string? Priority = null,
    string? Type = null,
    DateTime? DueDateUtc = null,
    string? EstimatedEffort = null);
