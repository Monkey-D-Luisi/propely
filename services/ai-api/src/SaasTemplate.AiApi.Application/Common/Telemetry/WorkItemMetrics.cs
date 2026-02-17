// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Diagnostics.Metrics;

namespace SaasTemplate.AiApi.Application.Common.Telemetry;

/// <summary>
/// Custom metrics for work item operations.
/// </summary>
public sealed class WorkItemMetrics
{
    /// <summary>
    /// The meter name for work item metrics.
    /// </summary>
    public const string MeterName = "SaasTemplate.AiApi.WorkItems";

    private readonly Counter<long> _workItemsCreated;
    private readonly Histogram<double> _queryDuration;
    private readonly ObservableGauge<int> _outboxPending;

    private int _pendingOutboxMessages;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkItemMetrics"/> class.
    /// </summary>
    public WorkItemMetrics()
    {
        var meter = new Meter(MeterName, "1.0.0");

        _workItemsCreated = meter.CreateCounter<long>(
            name: "workitems_created_total",
            unit: "{workitem}",
            description: "Total number of work items created");

        _queryDuration = meter.CreateHistogram<double>(
            name: "workitems_query_duration_seconds",
            unit: "s",
            description: "Duration of work item queries in seconds");

        _outboxPending = meter.CreateObservableGauge(
            name: "outbox_messages_pending",
            observeValue: () => _pendingOutboxMessages,
            unit: "{message}",
            description: "Number of pending outbox messages");
    }

    /// <summary>
    /// Records that a work item was created.
    /// </summary>
    public void RecordWorkItemCreated()
    {
        _workItemsCreated.Add(1);
    }

    /// <summary>
    /// Records the duration of a work item query.
    /// </summary>
    /// <param name="durationSeconds">The query duration in seconds.</param>
    public void RecordQueryDuration(double durationSeconds)
    {
        _queryDuration.Record(durationSeconds);
    }

    /// <summary>
    /// Sets the current count of pending outbox messages.
    /// Thread-safe update using interlocked operation.
    /// </summary>
    /// <param name="count">The pending message count.</param>
    public void SetPendingOutboxMessages(int count)
    {
        Interlocked.Exchange(ref _pendingOutboxMessages, count);
    }
}
