// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.WorkItems.Interfaces;
using Propely.AiApi.Domain.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.AiApi.Application.WorkItems.Commands;

public sealed class UpdateWorkItemCommandHandler : IRequestHandler<UpdateWorkItemCommand>
{
    private readonly IWorkItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWorkItemCommandHandler> _logger;

    public UpdateWorkItemCommandHandler(
        IWorkItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateWorkItemCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (workItem is null)
        {
            _logger.LogWarning("WorkItem {WorkItemId} not found for update.", request.Id);
            throw new NotFoundException($"WorkItem with ID {request.Id} not found.");
        }

        if (workItem.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} tried to update WorkItem {WorkItemId} owned by {OwnerId}", request.UserId, request.Id, workItem.UserId);
            throw new ForbiddenException($"User {request.UserId} is not authorized to update WorkItem {request.Id}");
        }

        workItem.Update(request.Title, request.Description, request.Status, request.Priority, request.Type, request.DueDateUtc, request.EstimatedEffort);

        await _repository.UpdateAsync(workItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("WorkItem {WorkItemId} updated.", request.Id);
    }
}
