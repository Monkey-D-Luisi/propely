// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Common;
using System.Text.Json.Serialization;

namespace Propely.AiApi.Domain.WorkItems.Events;

/// <summary>
/// Domain event raised when a WorkItem is updated.
/// </summary>
public sealed record WorkItemUpdatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(WorkItemUpdatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "WorkItemApi";

    public WorkItemUpdatedV1Data Data { get; }

    public WorkItemUpdatedV1(
        Guid workItemId,
        string title,
        string? description,
        WorkItemStatus status,
        WorkItemPriority? priority,
        WorkItemType? type,
        DateTime? dueDateUtc,
        WorkItemEffort? estimatedEffort,
        DateTime updatedAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = updatedAtUtc;
        CorrelationId = correlationId;
        CausationId = causationId;
        Data = new WorkItemUpdatedV1Data(workItemId, title, description, status, priority, type, dueDateUtc, estimatedEffort, updatedAtUtc);
    }
}

/// <summary>
/// Data payload for WorkItemUpdatedV1 event.
/// </summary>
public sealed record WorkItemUpdatedV1Data(
    Guid WorkItemId,
    string Title,
    string? Description,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemStatus Status,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemPriority? Priority,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemType? Type,
    DateTime? DueDateUtc,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemEffort? EstimatedEffort,
    DateTime UpdatedAtUtc);
