// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.WorkItems;

namespace Propely.AiApi.Api.Dtos;

/// <summary>
/// Request to update a work item.
/// </summary>
/// <param name="Title">The new title.</param>
/// <param name="Description">The new description.</param>
/// <param name="Status">The new status.</param>
/// <param name="Priority">The new priority (Low, Medium, High, Critical).</param>
/// <param name="Type">The new type (Task, Bug, Feature, Improvement).</param>
/// <param name="DueDateUtc">The new due date in UTC.</param>
/// <param name="EstimatedEffort">The new estimated effort (XS, S, M, L, XL).</param>
public sealed record UpdateWorkItemRequest(
    string Title,
    string? Description,
    WorkItemStatus Status,
    string? Priority = null,
    string? Type = null,
    DateTime? DueDateUtc = null,
    string? EstimatedEffort = null);
