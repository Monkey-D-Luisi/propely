// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Domain.Common;
using SaasTemplate.AiApi.Domain.WorkItems.Events;
using SaasTemplate.AiApi.Domain.WorkItems.Exceptions;

namespace SaasTemplate.AiApi.Domain.WorkItems;

/// <summary>
/// Represents a trackable unit of work in the domain.
/// </summary>
public sealed class WorkItem : Entity, ISoftDeletable
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public Guid Id { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public WorkItemStatus Status { get; private set; }
    public WorkItemPriority? Priority { get; private set; }
    public WorkItemType? Type { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public WorkItemEffort? EstimatedEffort { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Required for ORM materialization
    private WorkItem() { }

    private WorkItem(Guid id, Guid orgId, Guid userId, string title, string? description,
        WorkItemPriority? priority, WorkItemType? type, DateTime? dueDateUtc, WorkItemEffort? estimatedEffort)
    {
        Id = id;
        OrgId = orgId;
        UserId = userId;
        Title = title;
        Description = description;
        Status = WorkItemStatus.Pending;
        Priority = priority;
        Type = type;
        DueDateUtc = dueDateUtc;
        EstimatedEffort = estimatedEffort;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        Version = 1;
    }

    /// <summary>
    /// Creates a new WorkItem with the specified title and optional description.
    /// </summary>
    /// <param name="title">The title of the work item (required, 1-200 characters).</param>
    /// <param name="description">The description of the work item (optional, max 2000 characters).</param>
    /// <param name="correlationId">Optional correlation ID for tracing.</param>
    /// <param name="causationId">Optional causation ID for tracing.</param>
    /// <returns>A new WorkItem instance.</returns>
    /// <exception cref="WorkItemValidationException">Thrown when validation fails.</exception>
    public static WorkItem Create(
        Guid orgId,
        Guid userId,
        string title,
        string? description = null,
        WorkItemPriority? priority = null,
        WorkItemType? type = null,
        DateTime? dueDateUtc = null,
        WorkItemEffort? estimatedEffort = null,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("Organization ID must not be empty.", nameof(orgId));

        // Normalize inputs: trim whitespace, convert empty/whitespace description to null
        var normalizedTitle = title?.Trim() ?? string.Empty;
        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        ValidateTitle(normalizedTitle);
        ValidateDescription(normalizedDescription);

        var workItem = new WorkItem(Guid.NewGuid(), orgId, userId, normalizedTitle, normalizedDescription,
            priority, type, dueDateUtc, estimatedEffort);

        workItem.RaiseDomainEvent(new WorkItemCreatedV1(
            workItem.Id,
            workItem.OrgId,
            workItem.UserId,
            workItem.Title,
            workItem.Description,
            workItem.Status,
            workItem.Priority,
            workItem.Type,
            workItem.DueDateUtc,
            workItem.EstimatedEffort,
            workItem.CreatedAtUtc,
            correlationId,
            causationId));

        return workItem;
    }

    /// <summary>
    /// Updates the WorkItem's details and status.
    /// </summary>
    public void Update(
        string title,
        string? description,
        WorkItemStatus status,
        WorkItemPriority? priority = null,
        WorkItemType? type = null,
        DateTime? dueDateUtc = null,
        WorkItemEffort? estimatedEffort = null,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        if (Status == WorkItemStatus.Deleted)
        {
            throw new WorkItemValidationException("Status", "Cannot update a deleted work item.");
        }

        var normalizedTitle = title?.Trim() ?? string.Empty;
        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        ValidateTitle(normalizedTitle);
        ValidateDescription(normalizedDescription);

        Title = normalizedTitle;
        Description = normalizedDescription;
        Status = status;
        Priority = priority;
        Type = type;
        DueDateUtc = dueDateUtc;
        EstimatedEffort = estimatedEffort;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new WorkItemUpdatedV1(
            Id,
            Title,
            Description,
            Status,
            Priority,
            Type,
            DueDateUtc,
            EstimatedEffort,
            UpdatedAtUtc,
            correlationId,
            causationId));
    }

    /// <summary>
    /// Marks the WorkItem as Deleted (Soft Delete).
    /// </summary>
    public void Delete(Guid? correlationId = null, Guid? causationId = null)
    {
        if (Status == WorkItemStatus.Deleted)
        {
            return;
        }

        Status = WorkItemStatus.Deleted;
        IsDeleted = true;
        DeletedAtUtc = UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new WorkItemDeletedV1(
            Id,
            UpdatedAtUtc,
            correlationId,
            causationId));
    }

    public void SoftDelete()
    {
        Delete();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new WorkItemValidationException(
                nameof(Title),
                "Title is required and cannot be empty.");
        }

        if (title.Length > TitleMaxLength)
        {
            throw new WorkItemValidationException(
                nameof(Title),
                $"Title cannot exceed {TitleMaxLength} characters.");
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > DescriptionMaxLength)
        {
            throw new WorkItemValidationException(
                nameof(Description),
                $"Description cannot exceed {DescriptionMaxLength} characters.");
        }
    }
}
