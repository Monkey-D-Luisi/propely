// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Common;
using System.Text.Json.Serialization;

namespace Propely.AiApi.Domain.WorkItems.Events;

/// <summary>
/// Domain event raised when a WorkItem is created.
/// </summary>
public sealed record WorkItemCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(WorkItemCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "WorkItemApi";

    public WorkItemCreatedV1Data Data { get; }

    public WorkItemCreatedV1(
        Guid workItemId,
        Guid orgId,
        Guid userId,
        string title,
        string? description,
        WorkItemStatus status,
        WorkItemPriority? priority,
        WorkItemType? type,
        DateTime? dueDateUtc,
        WorkItemEffort? estimatedEffort,
        DateTime createdAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = createdAtUtc;
        CorrelationId = correlationId;
        CausationId = causationId;
        Data = new WorkItemCreatedV1Data(workItemId, orgId, userId, title, description, status, priority, type, dueDateUtc, estimatedEffort, createdAtUtc);
    }
}

/// <summary>
/// Data payload for WorkItemCreatedV1 event.
/// </summary>
public sealed record WorkItemCreatedV1Data(
    Guid WorkItemId,
    Guid OrgId,
    Guid UserId,
    string Title,
    string? Description,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemStatus Status,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemPriority? Priority,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemType? Type,
    DateTime? DueDateUtc,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] WorkItemEffort? EstimatedEffort,
    DateTime CreatedAtUtc);
