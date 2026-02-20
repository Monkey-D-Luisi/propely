// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Infrastructure.Persistence.Entities;

/// <summary>
/// Read model for WorkItem optimized for queries.
/// Populated by consuming domain events.
/// </summary>
public sealed class WorkItemRead
{
    /// <summary>
    /// Primary key - matches the WorkItem.Id from domain.
    /// </summary>
    public Guid Id { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>
    /// Work item title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Work item description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Work item priority as string (e.g. "Low", "Medium", "High", "Critical").
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Work item type as string (e.g. "Task", "Bug", "Feature", "Improvement").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Optional due date in UTC.
    /// </summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>
    /// Estimated effort as string (e.g. "XS", "S", "M", "L", "XL").
    /// </summary>
    public string? EstimatedEffort { get; set; }

    /// <summary>
    /// Current status as string.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the work item was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// UTC timestamp when this read model was last updated.
    /// </summary>
    public DateTime LastProjectedAtUtc { get; set; }

    private WorkItemRead() { }

    /// <summary>
    /// Creates a new read model entry from a WorkItemCreatedV1 event.
    /// </summary>
    public static WorkItemRead FromCreatedEvent(
        Guid id,
        Guid orgId,
        Guid userId,
        string title,
        string? description,
        string status,
        string? priority,
        string? type,
        DateTime? dueDateUtc,
        string? estimatedEffort,
        DateTime createdAtUtc)
    {
        return new WorkItemRead
        {
            Id = id,
            OrgId = orgId,
            UserId = userId,
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            Type = type,
            DueDateUtc = dueDateUtc,
            EstimatedEffort = estimatedEffort,
            CreatedAtUtc = createdAtUtc,
            LastProjectedAtUtc = DateTime.UtcNow
        };
    }
}
