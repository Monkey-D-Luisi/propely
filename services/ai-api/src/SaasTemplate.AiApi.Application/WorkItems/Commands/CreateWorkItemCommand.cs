// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using SaasTemplate.AiApi.Domain.WorkItems;

namespace SaasTemplate.AiApi.Application.WorkItems.Commands;

/// <summary>
/// Command to create a new WorkItem.
/// </summary>
public sealed record CreateWorkItemCommand(
    Guid OrgId,
    Guid UserId,
    string Title,
    string? Description = null,
    WorkItemPriority? Priority = null,
    WorkItemType? Type = null,
    DateTime? DueDateUtc = null,
    WorkItemEffort? EstimatedEffort = null,
    Guid? CorrelationId = null,
    Guid? CausationId = null) : IRequest<CreateWorkItemResult>;

/// <summary>
/// Result of a successful WorkItem creation.
/// </summary>
public sealed record CreateWorkItemResult(
    Guid Id,
    Guid OrgId,
    string Title,
    string? Description,
    WorkItemStatus Status,
    WorkItemPriority? Priority,
    WorkItemType? Type,
    DateTime? DueDateUtc,
    WorkItemEffort? EstimatedEffort,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
