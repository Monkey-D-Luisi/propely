// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.WorkItems;

namespace Propely.AiApi.Application.WorkItems.Dtos;

/// <summary>
/// Data transfer object for WorkItem read operations.
/// </summary>
public sealed record WorkItemDto(
    Guid Id,
    Guid OrgId,
    string Title,
    string? Description,
    WorkItemStatus Status,
    WorkItemPriority? Priority,
    WorkItemType? Type,
    DateTime? DueDateUtc,
    WorkItemEffort? EstimatedEffort,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    /// <summary>
    /// Creates a WorkItemDto from a WorkItem entity.
    /// </summary>
    /// <param name="workItem">The WorkItem entity to map.</param>
    /// <returns>A new WorkItemDto instance.</returns>
    public static WorkItemDto FromEntity(WorkItem workItem) =>
        new(
            workItem.Id,
            workItem.OrgId,
            workItem.Title,
            workItem.Description,
            workItem.Status,
            workItem.Priority,
            workItem.Type,
            workItem.DueDateUtc,
            workItem.EstimatedEffort,
            workItem.CreatedAtUtc,
            workItem.UpdatedAtUtc);
}
