// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.Common.Telemetry;
using Propely.AiApi.Application.WorkItems.Interfaces;
using Propely.AiApi.Domain.WorkItems;
using MediatR;

namespace Propely.AiApi.Application.WorkItems.Commands;

/// <summary>
/// Handles the creation of a new WorkItem.
/// Domain events are automatically dispatched to the outbox by the infrastructure layer.
/// </summary>
public sealed class CreateWorkItemCommandHandler : IRequestHandler<CreateWorkItemCommand, CreateWorkItemResult>
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkItemMetrics _metrics;

    public CreateWorkItemCommandHandler(
        IWorkItemRepository workItemRepository,
        IUnitOfWork unitOfWork,
        WorkItemMetrics metrics)
    {
        _workItemRepository = workItemRepository;
        _unitOfWork = unitOfWork;
        _metrics = metrics;
    }

    /// <summary>
    /// Handles the CreateWorkItemCommand.
    /// </summary>
    /// <param name="request">The command containing work item creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result containing the created work item's ID.</returns>
    public async Task<CreateWorkItemResult> Handle(
        CreateWorkItemCommand request,
        CancellationToken cancellationToken)
    {
        // Create the domain entity (this performs domain validation and raises domain events)
        var workItem = WorkItem.Create(
            request.OrgId,
            request.UserId,
            request.Title,
            request.Description,
            request.Priority,
            request.Type,
            request.DueDateUtc,
            request.EstimatedEffort,
            request.CorrelationId,
            request.CausationId);

        // Persist the work item
        await _workItemRepository.AddAsync(workItem, cancellationToken);

        // Save changes - domain events are automatically dispatched to outbox
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Record metrics
        _metrics.RecordWorkItemCreated();

        return new CreateWorkItemResult(
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
}
