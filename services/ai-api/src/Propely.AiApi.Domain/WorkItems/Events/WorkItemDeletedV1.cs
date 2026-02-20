// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Common;

namespace Propely.AiApi.Domain.WorkItems.Events;

/// <summary>
/// Domain event raised when a WorkItem is deleted (soft delete).
/// </summary>
public sealed record WorkItemDeletedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(WorkItemDeletedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "WorkItemApi";

    public WorkItemDeletedV1Data Data { get; }

    public WorkItemDeletedV1(
        Guid workItemId,
        DateTime deletedAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = deletedAtUtc;
        CorrelationId = correlationId;
        CausationId = causationId;
        Data = new WorkItemDeletedV1Data(workItemId, deletedAtUtc);
    }
}

/// <summary>
/// Data payload for WorkItemDeletedV1 event.
/// </summary>
public sealed record WorkItemDeletedV1Data(
    Guid WorkItemId,
    DateTime DeletedAtUtc);
