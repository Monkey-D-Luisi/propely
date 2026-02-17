// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Domain.WorkItems;
using MediatR;

namespace SaasTemplate.AiApi.Application.WorkItems.Commands;

public record UpdateWorkItemCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description,
    WorkItemStatus Status,
    WorkItemPriority? Priority = null,
    WorkItemType? Type = null,
    DateTime? DueDateUtc = null,
    WorkItemEffort? EstimatedEffort = null) : IRequest;
