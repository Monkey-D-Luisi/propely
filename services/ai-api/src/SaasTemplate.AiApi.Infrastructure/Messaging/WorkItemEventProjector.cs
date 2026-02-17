// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Application.WorkItems;
using SaasTemplate.AiApi.Infrastructure.Messaging.Events;
using SaasTemplate.AiApi.Infrastructure.Persistence;
using SaasTemplate.AiApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SaasTemplate.AiApi.Domain.WorkItems;

namespace SaasTemplate.AiApi.Infrastructure.Messaging;

/// <summary>
/// Projects WorkItem domain events to the read model.
/// Handles idempotency tracking and cache invalidation.
/// </summary>
public sealed class WorkItemEventProjector : IEventProjector
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WorkItemEventProjector> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] SupportedEvents = ["WorkItemCreatedV1", "WorkItemUpdatedV1", "WorkItemDeletedV1"];

    public WorkItemEventProjector(
        IServiceScopeFactory scopeFactory,
        ICacheService cacheService,
        ILogger<WorkItemEventProjector> logger)
    {
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> SupportedEventTypes => SupportedEvents;

    /// <inheritdoc />
    public async Task ProjectAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case "WorkItemCreatedV1":
                await ProjectWorkItemCreatedAsync(payload, cancellationToken);
                break;
            case "WorkItemUpdatedV1":
                await ProjectWorkItemUpdatedAsync(payload, cancellationToken);
                break;
            case "WorkItemDeletedV1":
                await ProjectWorkItemDeletedAsync(payload, cancellationToken);
                break;
            default:
                HandleUnknownEvent(eventType);
                break;
        }
    }

    private async Task ProjectWorkItemCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope<WorkItemCreatedPayload>>(payload, JsonOptions);

        if (envelope?.Data is null)
        {
            _logger.LogWarning("Invalid WorkItemCreatedV1 payload - missing envelope or data");
            return; // Skip processing invalid payload
        }

        // Log event metadata for traceability (directly from envelope)
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["EventId"] = envelope.EventId,
            ["EventType"] = envelope.EventType,
            ["CorrelationId"] = envelope.CorrelationId,
            ["EventTimestamp"] = envelope.OccurredAtUtc,
            ["SchemaVersion"] = envelope.SchemaVersion
        });

        _logger.LogDebug(
            "Processing event {EventType} with ID {EventId}, CorrelationId: {CorrelationId}",
            envelope.EventType,
            envelope.EventId,
            envelope.CorrelationId?.ToString() ?? "none");

        await using var dbScope = _scopeFactory.CreateAsyncScope();
        var dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Idempotency check
        var alreadyProcessed = await dbContext.ProcessedEvents
            .AnyAsync(pe => pe.EventId == envelope.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogDebug("Event {EventId} already processed, skipping", envelope.EventId);
            return;
        }

        // Check if read model already exists (handle upsert scenario)
        var existingReadModel = await dbContext.WorkItemsRead
            .FirstOrDefaultAsync(w => w.Id == envelope.Data.WorkItemId, cancellationToken);

        if (existingReadModel is not null)
        {
            _logger.LogDebug("WorkItem {WorkItemId} already in read model", envelope.Data.WorkItemId);
        }
        else
        {
            // Create read model entry
            var workItemRead = WorkItemRead.FromCreatedEvent(
                id: envelope.Data.WorkItemId,
                orgId: envelope.Data.OrgId,
                userId: envelope.Data.UserId,
                title: envelope.Data.Title,
                description: envelope.Data.Description,
                status: envelope.Data.Status,
                priority: envelope.Data.Priority,
                type: envelope.Data.Type,
                dueDateUtc: envelope.Data.DueDateUtc,
                estimatedEffort: envelope.Data.EstimatedEffort,
                createdAtUtc: envelope.Data.CreatedAtUtc);

            dbContext.WorkItemsRead.Add(workItemRead);
        }

        // Record that we processed this event
        var processedEvent = ProcessedEvent.Create(envelope.EventId, "WorkItemCreatedV1");
        dbContext.ProcessedEvents.Add(processedEvent);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate cache to ensure fresh data on next read
        var cacheKey = WorkItemCacheKeys.GetById(envelope.Data.WorkItemId);
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation(
            "Projected WorkItemCreatedV1 for WorkItem {WorkItemId}",
            envelope.Data.WorkItemId);
    }

    private void HandleUnknownEvent(string eventType)
    {
        _logger.LogWarning("Unknown event type: {EventType}", eventType);
        // Unknown events are logged but not processed - message will still be ACK'd by consumer
    }
    private async Task ProjectWorkItemUpdatedAsync(string payload, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope<WorkItemUpdatedPayload>>(payload, JsonOptions);
        if (envelope?.Data is null) return;

        await using var dbScope = _scopeFactory.CreateAsyncScope();
        var dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Idempotency check
        if (await dbContext.ProcessedEvents.AnyAsync(pe => pe.EventId == envelope.EventId, cancellationToken)) return;

        var readModel = await dbContext.WorkItemsRead
            .FirstOrDefaultAsync(w => w.Id == envelope.Data.WorkItemId, cancellationToken);

        if (readModel is not null)
        {
            readModel.Title = envelope.Data.Title;
            readModel.Description = envelope.Data.Description;
            readModel.Status = envelope.Data.Status;
            readModel.Priority = envelope.Data.Priority;
            readModel.Type = envelope.Data.Type;
            readModel.DueDateUtc = envelope.Data.DueDateUtc;
            readModel.EstimatedEffort = envelope.Data.EstimatedEffort;
            readModel.LastProjectedAtUtc = DateTime.UtcNow;
            // Note: CreatedAtUtc unchanged.
        }
        else
        {
            _logger.LogWarning("WorkItem {WorkItemId} not found in read model during update projection", envelope.Data.WorkItemId);
        }

        dbContext.ProcessedEvents.Add(ProcessedEvent.Create(envelope.EventId, "WorkItemUpdatedV1"));
        await dbContext.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(WorkItemCacheKeys.GetById(envelope.Data.WorkItemId), cancellationToken);
    }

    private async Task ProjectWorkItemDeletedAsync(string payload, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope<WorkItemDeletedPayload>>(payload, JsonOptions);
        if (envelope?.Data is null) return;

        await using var dbScope = _scopeFactory.CreateAsyncScope();
        var dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await dbContext.ProcessedEvents.AnyAsync(pe => pe.EventId == envelope.EventId, cancellationToken)) return;

        var readModel = await dbContext.WorkItemsRead
            .FirstOrDefaultAsync(w => w.Id == envelope.Data.WorkItemId, cancellationToken);

        if (readModel is not null)
        {
            readModel.Status = nameof(WorkItemStatus.Deleted); // Soft delete using Status string.
            readModel.LastProjectedAtUtc = DateTime.UtcNow;
        }
        else
        {
            _logger.LogWarning("WorkItem {WorkItemId} not found in read model during delete projection", envelope.Data.WorkItemId);
        }

        dbContext.ProcessedEvents.Add(ProcessedEvent.Create(envelope.EventId, "WorkItemDeletedV1"));
        await dbContext.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(WorkItemCacheKeys.GetById(envelope.Data.WorkItemId), cancellationToken);
    }
}

/// <summary>
/// DTO for the data portion of WorkItemCreatedV1 event.
/// </summary>
internal sealed record WorkItemCreatedPayload
{
    public Guid WorkItemId { get; init; }
    public Guid OrgId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Priority { get; init; }
    public string? Type { get; init; }
    public DateTime? DueDateUtc { get; init; }
    public string? EstimatedEffort { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

internal sealed record WorkItemUpdatedPayload
{
    public Guid WorkItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Priority { get; init; }
    public string? Type { get; init; }
    public DateTime? DueDateUtc { get; init; }
    public string? EstimatedEffort { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}

internal sealed record WorkItemDeletedPayload
{
    public Guid WorkItemId { get; init; }
    public DateTime DeletedAtUtc { get; init; }
}
